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
    RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting index maintenance script.', 16, 1);
    RETURN;
END;
GO

/*
  Phase 29.3 - Index Maintenance Script
  Safe defaults:
    - @Execute = 0 (plan-only)
    - executes only for indexes above @MinPageCount
*/
DECLARE @Execute bit = 0;
DECLARE @MinPageCount int = 1000;
DECLARE @RebuildThreshold float = 30.0;
DECLARE @ReorganizeThreshold float = 5.0;

CREATE TABLE #IndexPlan
(
    SchemaName sysname NOT NULL,
    TableName sysname NOT NULL,
    IndexName sysname NOT NULL,
    AvgFragmentationPercent float NOT NULL,
    PageCount bigint NOT NULL,
    ActionName nvarchar(20) NOT NULL,
    CommandText nvarchar(max) NOT NULL,
    Executed bit NOT NULL DEFAULT 0,
    ErrorMessage nvarchar(4000) NULL
);

INSERT INTO #IndexPlan (SchemaName, TableName, IndexName, AvgFragmentationPercent, PageCount, ActionName, CommandText)
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    i.name AS IndexName,
    ips.avg_fragmentation_in_percent,
    ips.page_count,
    CASE
        WHEN ips.avg_fragmentation_in_percent >= @RebuildThreshold THEN N'REBUILD'
        WHEN ips.avg_fragmentation_in_percent >= @ReorganizeThreshold THEN N'REORGANIZE'
        ELSE N'SKIP'
    END AS ActionName,
    CASE
        WHEN ips.avg_fragmentation_in_percent >= @RebuildThreshold
            THEN N'ALTER INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' REBUILD;'
        WHEN ips.avg_fragmentation_in_percent >= @ReorganizeThreshold
            THEN N'ALTER INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' REORGANIZE;'
        ELSE N''
    END AS CommandText
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
INNER JOIN sys.indexes i
    ON i.object_id = ips.object_id
   AND i.index_id = ips.index_id
INNER JOIN sys.tables t
    ON t.object_id = i.object_id
INNER JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE i.name IS NOT NULL
  AND i.is_disabled = 0
  AND i.is_hypothetical = 0
  AND ips.alloc_unit_type_desc = 'IN_ROW_DATA'
  AND ips.page_count >= @MinPageCount;

SELECT
    SchemaName,
    TableName,
    IndexName,
    AvgFragmentationPercent,
    PageCount,
    ActionName,
    CommandText,
    Executed,
    ErrorMessage
FROM #IndexPlan
ORDER BY AvgFragmentationPercent DESC, PageCount DESC;

IF @Execute = 1
BEGIN
    DECLARE @SchemaName sysname;
    DECLARE @TableName sysname;
    DECLARE @IndexName sysname;
    DECLARE @ActionName nvarchar(20);
    DECLARE @CommandText nvarchar(max);

    DECLARE PlanCursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT SchemaName, TableName, IndexName, ActionName, CommandText
    FROM #IndexPlan
    WHERE ActionName IN (N'REORGANIZE', N'REBUILD');

    OPEN PlanCursor;
    FETCH NEXT FROM PlanCursor INTO @SchemaName, @TableName, @IndexName, @ActionName, @CommandText;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        BEGIN TRY
            EXEC sp_executesql @CommandText;
            UPDATE #IndexPlan
            SET Executed = 1
            WHERE SchemaName = @SchemaName
              AND TableName = @TableName
              AND IndexName = @IndexName;
        END TRY
        BEGIN CATCH
            UPDATE #IndexPlan
            SET Executed = 0,
                ErrorMessage = ERROR_MESSAGE()
            WHERE SchemaName = @SchemaName
              AND TableName = @TableName
              AND IndexName = @IndexName;
        END CATCH;

        FETCH NEXT FROM PlanCursor INTO @SchemaName, @TableName, @IndexName, @ActionName, @CommandText;
    END;

    CLOSE PlanCursor;
    DEALLOCATE PlanCursor;
END;

SELECT
    SchemaName,
    TableName,
    IndexName,
    AvgFragmentationPercent,
    PageCount,
    ActionName,
    Executed,
    ErrorMessage
FROM #IndexPlan
ORDER BY AvgFragmentationPercent DESC, PageCount DESC;

PRINT 'Phase 29.3 index maintenance script completed.';
