-- ============================================================
-- Phase 29 Stage 29.3 - Archive and Retention Policy Script
-- ============================================================
-- PURPOSE:
--   1) Show archive/purge candidates using explicit retention windows.
--   2) Optionally apply batched retention cleanup for operational tables.
--
-- SAFETY:
--   - Dry-run by default (@ApplyChanges = 0).
--   - Skips tables/columns that do not exist in the current schema.
--   - Uses batched deletes to reduce lock pressure.
--
-- USAGE:
--   - Review dry-run output first.
--   - Set @ApplyChanges = 1 only after validation in non-prod/prod windows.
-- ============================================================

USE [TabsanEduSphere];
GO

SET NOCOUNT ON;

DECLARE @ApplyChanges bit = 0;
DECLARE @AsOfUtc datetime2 = SYSUTCDATETIME();
DECLARE @BatchSize int = 2000;

DECLARE @Policies TABLE
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL,
    RetentionDays int NOT NULL,
    RequireSoftDelete bit NOT NULL,
    ExtraPredicate nvarchar(500) NULL
);

-- Final-Touches Phase 29 Stage 29.3 - retention policy matrix
INSERT INTO @Policies (TableName, DateColumn, RetentionDays, RequireSoftDelete, ExtraPredicate)
VALUES
    (N'audit_logs', N'OccurredAt', 365, 0, NULL),
    (N'user_sessions', N'CreatedAt', 90, 0, N'(ExpiresAt < @AsOfUtc OR RevokedAt IS NOT NULL)'),
    (N'notification_recipients', N'CreatedAt', 180, 0, N'IsRead = 1'),
    (N'password_history', N'CreatedAt', 365, 0, NULL),
    (N'outbound_email_logs', N'CreatedAt', 365, 0, NULL),
    (N'support_ticket_messages', N'CreatedAt', 730, 1, NULL),
    (N'support_tickets', N'CreatedAt', 730, 1, NULL),
    (N'graduation_application_approvals', N'CreatedAt', 1095, 1, NULL),
    (N'graduation_applications', N'CreatedAt', 1095, 1, NULL),
    (N'results', N'CreatedAt', 1825, 1, NULL);

DECLARE @Summary TABLE
(
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL,
    CutoffUtc datetime2 NOT NULL,
    CandidateRows bigint NOT NULL,
    ActionTaken nvarchar(100) NOT NULL,
    Notes nvarchar(300) NULL
);

DECLARE
    @Id int = 1,
    @MaxId int,
    @TableName sysname,
    @DateColumn sysname,
    @RetentionDays int,
    @RequireSoftDelete bit,
    @ExtraPredicate nvarchar(500),
    @CutoffUtc datetime2,
    @HasTable bit,
    @HasDateColumn bit,
    @HasIsDeleted bit,
    @WhereClause nvarchar(1200),
    @CountSql nvarchar(max),
    @DeleteSql nvarchar(max),
    @CandidateRows bigint,
    @RowsDeleted int,
    @DeletedTotal bigint,
    @Note nvarchar(300);

SELECT @MaxId = MAX(Id) FROM @Policies;

WHILE @Id <= @MaxId
BEGIN
    SELECT
        @TableName = TableName,
        @DateColumn = DateColumn,
        @RetentionDays = RetentionDays,
        @RequireSoftDelete = RequireSoftDelete,
        @ExtraPredicate = ExtraPredicate
    FROM @Policies
    WHERE Id = @Id;

    SET @CutoffUtc = DATEADD(DAY, -@RetentionDays, @AsOfUtc);
    SET @HasTable = CASE WHEN OBJECT_ID(N'dbo.' + QUOTENAME(@TableName), N'U') IS NOT NULL THEN 1 ELSE 0 END;

    IF @HasTable = 0
    BEGIN
        INSERT INTO @Summary (TableName, DateColumn, CutoffUtc, CandidateRows, ActionTaken, Notes)
        VALUES (@TableName, @DateColumn, @CutoffUtc, 0, N'skipped', N'table not found');
        SET @Id += 1;
        CONTINUE;
    END;

    SET @HasDateColumn = CASE WHEN COL_LENGTH(N'dbo.' + @TableName, @DateColumn) IS NOT NULL THEN 1 ELSE 0 END;
    IF @HasDateColumn = 0
    BEGIN
        INSERT INTO @Summary (TableName, DateColumn, CutoffUtc, CandidateRows, ActionTaken, Notes)
        VALUES (@TableName, @DateColumn, @CutoffUtc, 0, N'skipped', N'date column not found');
        SET @Id += 1;
        CONTINUE;
    END;

    SET @HasIsDeleted = CASE WHEN COL_LENGTH(N'dbo.' + @TableName, N'IsDeleted') IS NOT NULL THEN 1 ELSE 0 END;

    SET @WhereClause = QUOTENAME(@DateColumn) + N' < @CutoffUtc';

    IF @RequireSoftDelete = 1
    BEGIN
        IF @HasIsDeleted = 1
            SET @WhereClause += N' AND [IsDeleted] = 1';
        ELSE
        BEGIN
            INSERT INTO @Summary (TableName, DateColumn, CutoffUtc, CandidateRows, ActionTaken, Notes)
            VALUES (@TableName, @DateColumn, @CutoffUtc, 0, N'skipped', N'requires IsDeleted flag but column not found');
            SET @Id += 1;
            CONTINUE;
        END;
    END;

    IF NULLIF(@ExtraPredicate, N'') IS NOT NULL
        SET @WhereClause += N' AND (' + @ExtraPredicate + N')';

    SET @CountSql = N'SELECT @CandidateRows = COUNT_BIG(1) FROM dbo.' + QUOTENAME(@TableName) + N' WHERE ' + @WhereClause + N';';
    SET @CandidateRows = 0;

    EXEC sp_executesql
        @CountSql,
        N'@CutoffUtc datetime2, @AsOfUtc datetime2, @CandidateRows bigint OUTPUT',
        @CutoffUtc = @CutoffUtc,
        @AsOfUtc = @AsOfUtc,
        @CandidateRows = @CandidateRows OUTPUT;

    IF @ApplyChanges = 0
    BEGIN
        INSERT INTO @Summary (TableName, DateColumn, CutoffUtc, CandidateRows, ActionTaken, Notes)
        VALUES (@TableName, @DateColumn, @CutoffUtc, @CandidateRows, N'dry-run', NULL);
        SET @Id += 1;
        CONTINUE;
    END;

    SET @DeletedTotal = 0;
    SET @RowsDeleted = 1;

    WHILE @RowsDeleted > 0
    BEGIN
        SET @DeleteSql =
            N'DELETE TOP (@BatchSize) FROM dbo.' + QUOTENAME(@TableName) +
            N' WHERE ' + @WhereClause + N'; SET @RowsDeleted = @@ROWCOUNT;';

        EXEC sp_executesql
            @DeleteSql,
            N'@BatchSize int, @CutoffUtc datetime2, @AsOfUtc datetime2, @RowsDeleted int OUTPUT',
            @BatchSize = @BatchSize,
            @CutoffUtc = @CutoffUtc,
            @AsOfUtc = @AsOfUtc,
            @RowsDeleted = @RowsDeleted OUTPUT;

        SET @DeletedTotal += @RowsDeleted;
    END;

    SET @Note = CONCAT(N'batch size ', @BatchSize);
    INSERT INTO @Summary (TableName, DateColumn, CutoffUtc, CandidateRows, ActionTaken, Notes)
    VALUES (@TableName, @DateColumn, @CutoffUtc, @DeletedTotal, N'cleanup-applied', @Note);

    SET @Id += 1;
END;

SELECT
    TableName,
    DateColumn,
    CutoffUtc,
    CandidateRows,
    ActionTaken,
    Notes
FROM @Summary
ORDER BY TableName;
