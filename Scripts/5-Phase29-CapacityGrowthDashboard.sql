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
    RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting capacity dashboard script.', 16, 1);
    RETURN;
END;
GO

/*
  Phase 29.3 - Capacity and Growth Dashboard
  Outputs:
    1) Table size and row-count snapshot
    2) Recent row growth windows for selected high-volume tables
    3) Index usage summary for top write-heavy indexes
*/
DECLARE @Now datetime2 = SYSUTCDATETIME();

;WITH TableSpace AS
(
    SELECT
        s.name AS SchemaName,
        t.name AS TableName,
        SUM(CASE WHEN ps.index_id IN (0,1) THEN ps.row_count ELSE 0 END) AS RowCount,
        SUM(a.total_pages) * 8.0 / 1024.0 AS TotalMB,
        SUM(a.used_pages) * 8.0 / 1024.0 AS UsedMB,
        SUM(a.data_pages) * 8.0 / 1024.0 AS DataMB
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    INNER JOIN sys.indexes i
        ON i.object_id = t.object_id
    INNER JOIN sys.partitions p
        ON p.object_id = i.object_id
       AND p.index_id = i.index_id
    INNER JOIN sys.allocation_units a
        ON a.container_id = p.partition_id
    INNER JOIN sys.dm_db_partition_stats ps
        ON ps.object_id = p.object_id
       AND ps.index_id = p.index_id
       AND ps.partition_id = p.partition_id
    WHERE t.is_ms_shipped = 0
    GROUP BY s.name, t.name
)
SELECT TOP 50
    SchemaName,
    TableName,
    RowCount,
    CAST(TotalMB AS decimal(18,2)) AS TotalMB,
    CAST(UsedMB AS decimal(18,2)) AS UsedMB,
    CAST(DataMB AS decimal(18,2)) AS DataMB
FROM TableSpace
ORDER BY TotalMB DESC, RowCount DESC;

CREATE TABLE #GrowthSnapshot
(
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL,
    Last7Days bigint NULL,
    Last30Days bigint NULL,
    Last90Days bigint NULL,
    TotalRows bigint NULL
);

INSERT INTO #GrowthSnapshot (TableName, DateColumn)
VALUES
    (N'notifications', N'CreatedAt'),
    (N'notification_recipients', N'CreatedAt'),
    (N'support_tickets', N'CreatedAt'),
    (N'support_ticket_messages', N'CreatedAt'),
    (N'audit_logs', N'OccurredAt'),
    (N'outbound_email_logs', N'CreatedAt'),
    (N'user_sessions', N'CreatedAt'),
    (N'payment_receipts', N'CreatedAt');

DECLARE @TableName sysname;
DECLARE @DateColumn sysname;
DECLARE @Sql nvarchar(max);
DECLARE @Last7 bigint;
DECLARE @Last30 bigint;
DECLARE @Last90 bigint;
DECLARE @Total bigint;

DECLARE GrowthCursor CURSOR LOCAL FAST_FORWARD FOR
SELECT TableName, DateColumn
FROM #GrowthSnapshot;

OPEN GrowthCursor;
FETCH NEXT FROM GrowthCursor INTO @TableName, @DateColumn;

WHILE @@FETCH_STATUS = 0
BEGIN
    IF OBJECT_ID(@TableName) IS NOT NULL AND COL_LENGTH(@TableName, @DateColumn) IS NOT NULL
    BEGIN
        SET @Sql = N'
            SELECT
                @Last7Out = SUM(CASE WHEN ' + QUOTENAME(@DateColumn) + N' >= DATEADD(DAY, -7, @NowIn) THEN 1 ELSE 0 END),
                @Last30Out = SUM(CASE WHEN ' + QUOTENAME(@DateColumn) + N' >= DATEADD(DAY, -30, @NowIn) THEN 1 ELSE 0 END),
                @Last90Out = SUM(CASE WHEN ' + QUOTENAME(@DateColumn) + N' >= DATEADD(DAY, -90, @NowIn) THEN 1 ELSE 0 END),
                @TotalOut = COUNT_BIG(1)
            FROM ' + QUOTENAME(@TableName) + N';';

        EXEC sp_executesql
            @Sql,
            N'@NowIn datetime2, @Last7Out bigint OUTPUT, @Last30Out bigint OUTPUT, @Last90Out bigint OUTPUT, @TotalOut bigint OUTPUT',
            @NowIn = @Now,
            @Last7Out = @Last7 OUTPUT,
            @Last30Out = @Last30 OUTPUT,
            @Last90Out = @Last90 OUTPUT,
            @TotalOut = @Total OUTPUT;

        UPDATE #GrowthSnapshot
        SET Last7Days = ISNULL(@Last7, 0),
            Last30Days = ISNULL(@Last30, 0),
            Last90Days = ISNULL(@Last90, 0),
            TotalRows = ISNULL(@Total, 0)
        WHERE TableName = @TableName;
    END
    ELSE
    BEGIN
        UPDATE #GrowthSnapshot
        SET Last7Days = NULL,
            Last30Days = NULL,
            Last90Days = NULL,
            TotalRows = NULL
        WHERE TableName = @TableName;
    END;

    FETCH NEXT FROM GrowthCursor INTO @TableName, @DateColumn;
END;

CLOSE GrowthCursor;
DEALLOCATE GrowthCursor;

SELECT
    TableName,
    DateColumn,
    Last7Days,
    Last30Days,
    Last90Days,
    TotalRows
FROM #GrowthSnapshot
ORDER BY ISNULL(Last30Days, 0) DESC, ISNULL(TotalRows, 0) DESC;

SELECT TOP 50
    OBJECT_SCHEMA_NAME(i.object_id) AS SchemaName,
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    COALESCE(us.user_seeks, 0) AS UserSeeks,
    COALESCE(us.user_scans, 0) AS UserScans,
    COALESCE(us.user_lookups, 0) AS UserLookups,
    COALESCE(us.user_updates, 0) AS UserUpdates,
    us.last_user_seek,
    us.last_user_scan,
    us.last_user_update
FROM sys.indexes i
LEFT JOIN sys.dm_db_index_usage_stats us
    ON us.database_id = DB_ID()
   AND us.object_id = i.object_id
   AND us.index_id = i.index_id
WHERE i.object_id > 100
  AND i.name IS NOT NULL
ORDER BY COALESCE(us.user_updates, 0) DESC,
         COALESCE(us.user_seeks, 0) + COALESCE(us.user_scans, 0) DESC;

PRINT 'Phase 29.3 capacity and growth dashboard completed.';
