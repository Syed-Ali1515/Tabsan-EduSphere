SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_ID(N'Tabsan-EduSphere') IS NULL
BEGIN
    RAISERROR('Database [Tabsan-EduSphere] does not exist. Run 01-Schema-Current.sql first.', 16, 1);
    RETURN;
END;
GO

USE [Tabsan-EduSphere];
GO

IF DB_NAME() <> N'Tabsan-EduSphere'
BEGIN
    RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting archive policy script.', 16, 1);
    RETURN;
END;
GO

/*
  Phase 29.3 - Archive and Retention Policy Script
  Safe defaults:
    - @ApplyCleanup = 0 (dry-run only)
    - @BatchSize limits write pressure when cleanup is enabled
*/
DECLARE @ApplyCleanup bit = 0;
DECLARE @BatchSize int = 2000;
DECLARE @Now datetime2 = SYSUTCDATETIME();
DECLARE @TotalDeleted int;
DECLARE @DeletedInBatch int;

PRINT 'Phase 29.3 archive policy started. ApplyCleanup=' + CAST(@ApplyCleanup AS nvarchar(1));

DECLARE @Policy TABLE
(
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL,
    Predicate nvarchar(400) NOT NULL,
    RetentionDays int NOT NULL
);

INSERT INTO @Policy (TableName, DateColumn, Predicate, RetentionDays)
VALUES
    (N'user_sessions', N'ExpiresAt', N'ExpiresAt IS NOT NULL', 30),
    (N'notification_recipients', N'CreatedAt', N'1=1', 365),
    (N'outbound_email_logs', N'CreatedAt', N'1=1', 180),
    (N'audit_logs', N'OccurredAt', N'1=1', 730),
    (N'support_ticket_messages', N'CreatedAt', N'1=1', 1095);

CREATE TABLE #RetentionAudit
(
    TableName sysname NOT NULL,
    RetentionDays int NOT NULL,
    CandidateCount bigint NOT NULL,
    DeletedCount bigint NOT NULL DEFAULT 0
);

DECLARE
    @TableName sysname,
    @DateColumn sysname,
    @Predicate nvarchar(400),
    @RetentionDays int,
    @Cutoff datetime2,
    @Sql nvarchar(max),
    @CandidateCount bigint;

DECLARE PolicyCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT TableName, DateColumn, Predicate, RetentionDays
FROM @Policy;

OPEN PolicyCursor;
FETCH NEXT FROM PolicyCursor INTO @TableName, @DateColumn, @Predicate, @RetentionDays;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(@TableName) IS NULL OR COL_LENGTH(@TableName, @DateColumn) IS NULL
    BEGIN
        PRINT 'Skipping missing table/column policy: ' + @TableName + '.' + @DateColumn;
        FETCH NEXT FROM PolicyCursor INTO @TableName, @DateColumn, @Predicate, @RetentionDays;
        CONTINUE;
    END;

    SET @Cutoff = DATEADD(DAY, -@RetentionDays, @Now);
    SET @Sql = N'
        SELECT @CandidateCountOut = COUNT_BIG(1)
        FROM ' + QUOTENAME(@TableName) + N'
        WHERE ' + QUOTENAME(@DateColumn) + N' < @CutoffIn
          AND ' + @Predicate + N';';

    EXEC sp_executesql
        @Sql,
        N'@CutoffIn datetime2, @CandidateCountOut bigint OUTPUT',
        @CutoffIn = @Cutoff,
        @CandidateCountOut = @CandidateCount OUTPUT;

    INSERT INTO #RetentionAudit (TableName, RetentionDays, CandidateCount, DeletedCount)
    VALUES (@TableName, @RetentionDays, @CandidateCount, 0);

    IF @ApplyCleanup = 1
    BEGIN
        SET @TotalDeleted = 0;

        WHILE 1 = 1
        BEGIN
            SET @Sql = N'
                DELETE TOP (' + CAST(@BatchSize AS nvarchar(20)) + N')
                FROM ' + QUOTENAME(@TableName) + N'
                WHERE ' + QUOTENAME(@DateColumn) + N' < @CutoffIn
                  AND ' + @Predicate + N';
                SELECT @RowsOut = @@ROWCOUNT;';

            EXEC sp_executesql
                @Sql,
                N'@CutoffIn datetime2, @RowsOut int OUTPUT',
                @CutoffIn = @Cutoff,
                @RowsOut = @DeletedInBatch OUTPUT;

            SET @TotalDeleted += ISNULL(@DeletedInBatch, 0);

            IF ISNULL(@DeletedInBatch, 0) = 0
                BREAK;
        END;

        UPDATE #RetentionAudit
        SET DeletedCount = @TotalDeleted
        WHERE TableName = @TableName;
    END;

    FETCH NEXT FROM PolicyCursor INTO @TableName, @DateColumn, @Predicate, @RetentionDays;
END;

CLOSE PolicyCursor;
DEALLOCATE PolicyCursor;

SELECT
    TableName,
    RetentionDays,
    CandidateCount,
    DeletedCount,
    CASE WHEN @ApplyCleanup = 1 THEN N'Applied' ELSE N'DryRun' END AS RunMode
FROM #RetentionAudit
ORDER BY CandidateCount DESC;

PRINT 'Phase 29.3 archive policy completed.';
