-- ============================================================
-- Phase 29 Stage 29.3 - Capacity and Growth Dashboard Queries
-- ============================================================
-- PURPOSE:
--   1) Surface current table size and row-count distribution.
--   2) Estimate recent growth (last 30 days) on key operational tables.
--
-- NOTE:
--   This script is read-only and safe to execute anytime.
-- ============================================================

USE [TabsanEduSphere];
GO

SET NOCOUNT ON;

-- Final-Touches Phase 29 Stage 29.3 - current capacity snapshot
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(CASE WHEN p.index_id IN (0,1) THEN p.rows ELSE 0 END) AS RowCount,
    CAST(SUM(a.total_pages) * 8.0 / 1024.0 AS decimal(18,2)) AS ReservedMB,
    CAST(SUM(a.used_pages) * 8.0 / 1024.0 AS decimal(18,2)) AS UsedMB,
    CAST((SUM(a.total_pages) - SUM(a.used_pages)) * 8.0 / 1024.0 AS decimal(18,2)) AS UnusedMB
FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.indexes i ON i.object_id = t.object_id
JOIN sys.partitions p ON p.object_id = i.object_id AND p.index_id = i.index_id
JOIN sys.allocation_units a ON a.container_id = p.partition_id
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name
ORDER BY ReservedMB DESC, RowCount DESC;

-- Recent growth estimates for key high-volume tables
DECLARE @AsOfUtc datetime2 = SYSUTCDATETIME();

IF OBJECT_ID('tempdb..#Growth') IS NOT NULL DROP TABLE #Growth;
CREATE TABLE #Growth
(
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL,
    TotalRows bigint NOT NULL,
    RowsLast30Days bigint NOT NULL,
    RowsLast7Days bigint NOT NULL
);

DECLARE @Targets TABLE
(
    Id int IDENTITY(1,1) PRIMARY KEY,
    TableName sysname NOT NULL,
    DateColumn sysname NOT NULL
);

INSERT INTO @Targets (TableName, DateColumn)
VALUES
    (N'audit_logs', N'OccurredAt'),
    (N'user_sessions', N'CreatedAt'),
    (N'notification_recipients', N'CreatedAt'),
    (N'payment_receipts', N'CreatedAt'),
    (N'quiz_attempts', N'CreatedAt'),
    (N'support_tickets', N'CreatedAt'),
    (N'support_ticket_messages', N'CreatedAt'),
    (N'graduation_applications', N'CreatedAt'),
    (N'results', N'CreatedAt');

DECLARE
    @Id int = 1,
    @MaxId int,
    @TableName sysname,
    @DateColumn sysname,
    @Sql nvarchar(max);

SELECT @MaxId = MAX(Id) FROM @Targets;

WHILE @Id <= @MaxId
BEGIN
    SELECT @TableName = TableName, @DateColumn = DateColumn
    FROM @Targets
    WHERE Id = @Id;

    IF OBJECT_ID(N'dbo.' + QUOTENAME(@TableName), N'U') IS NOT NULL
       AND COL_LENGTH(N'dbo.' + @TableName, @DateColumn) IS NOT NULL
    BEGIN
        SET @Sql = N'
            INSERT INTO #Growth (TableName, DateColumn, TotalRows, RowsLast30Days, RowsLast7Days)
            SELECT
                @TableName,
                @DateColumn,
                COUNT_BIG(1),
                SUM(CASE WHEN ' + QUOTENAME(@DateColumn) + N' >= DATEADD(DAY, -30, @AsOfUtc) THEN 1 ELSE 0 END),
                SUM(CASE WHEN ' + QUOTENAME(@DateColumn) + N' >= DATEADD(DAY, -7, @AsOfUtc) THEN 1 ELSE 0 END)
            FROM dbo.' + QUOTENAME(@TableName) + N';';

        EXEC sp_executesql
            @Sql,
            N'@TableName sysname, @DateColumn sysname, @AsOfUtc datetime2',
            @TableName = @TableName,
            @DateColumn = @DateColumn,
            @AsOfUtc = @AsOfUtc;
    END;

    SET @Id += 1;
END;

SELECT
    TableName,
    DateColumn,
    TotalRows,
    RowsLast30Days,
    RowsLast7Days,
    CAST(RowsLast30Days / 30.0 AS decimal(18,2)) AS AvgRowsPerDay30,
    CAST(RowsLast7Days / 7.0 AS decimal(18,2)) AS AvgRowsPerDay7
FROM #Growth
ORDER BY RowsLast30Days DESC, TotalRows DESC;
