-- ============================================================
-- Phase 29 Stage 29.3 - Index Rebuild/Reorganize Maintenance
-- ============================================================
-- PURPOSE:
--   1) Detect index fragmentation on user tables.
--   2) Plan REORGANIZE vs REBUILD operations by threshold.
--   3) Optionally execute maintenance operations.
--
-- SAFETY:
--   - Dry-run by default (@ApplyChanges = 0).
--   - Ignores tiny indexes using @MinPageCount.
-- ============================================================

USE [TabsanEduSphere];
GO

SET NOCOUNT ON;

DECLARE @ApplyChanges bit = 0;
DECLARE @MinPageCount int = 1000;
DECLARE @ReorganizeMinFrag decimal(5,2) = 10.0;
DECLARE @RebuildMinFrag decimal(5,2) = 30.0;

IF OBJECT_ID('tempdb..#Frag') IS NOT NULL DROP TABLE #Frag;
CREATE TABLE #Frag
(
    RowId int IDENTITY(1,1) PRIMARY KEY,
    SchemaName sysname NOT NULL,
    TableName sysname NOT NULL,
    IndexName sysname NOT NULL,
    FragmentationPercent decimal(9,2) NOT NULL,
    PageCount bigint NOT NULL,
    PlannedAction nvarchar(20) NOT NULL
);

-- Final-Touches Phase 29 Stage 29.3 - fragmentation sampling
INSERT INTO #Frag (SchemaName, TableName, IndexName, FragmentationPercent, PageCount, PlannedAction)
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    i.name AS IndexName,
    CONVERT(decimal(9,2), ips.avg_fragmentation_in_percent) AS FragmentationPercent,
    ips.page_count AS PageCount,
    CASE
        WHEN ips.avg_fragmentation_in_percent >= @RebuildMinFrag THEN N'REBUILD'
        WHEN ips.avg_fragmentation_in_percent >= @ReorganizeMinFrag THEN N'REORGANIZE'
        ELSE N'SKIP'
    END AS PlannedAction
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
JOIN sys.indexes i
    ON i.object_id = ips.object_id
   AND i.index_id = ips.index_id
JOIN sys.tables t
    ON t.object_id = i.object_id
JOIN sys.schemas s
    ON s.schema_id = t.schema_id
WHERE ips.index_id > 0
  AND ips.page_count >= @MinPageCount
  AND i.is_hypothetical = 0
  AND t.is_ms_shipped = 0;

SELECT
    SchemaName,
    TableName,
    IndexName,
    FragmentationPercent,
    PageCount,
    PlannedAction
FROM #Frag
WHERE PlannedAction <> N'SKIP'
ORDER BY FragmentationPercent DESC, PageCount DESC;

IF @ApplyChanges = 0
BEGIN
    PRINT 'Dry-run mode only. Set @ApplyChanges = 1 to execute maintenance.';
    RETURN;
END;

DECLARE
    @RowId int,
    @SchemaName sysname,
    @TableName sysname,
    @IndexName sysname,
    @PlannedAction nvarchar(20),
    @Sql nvarchar(max);

DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
SELECT RowId, SchemaName, TableName, IndexName, PlannedAction
FROM #Frag
WHERE PlannedAction IN (N'REORGANIZE', N'REBUILD')
ORDER BY FragmentationPercent DESC;

OPEN cur;
FETCH NEXT FROM cur INTO @RowId, @SchemaName, @TableName, @IndexName, @PlannedAction;

WHILE @@FETCH_STATUS = 0
BEGIN
    BEGIN TRY
        IF @PlannedAction = N'REORGANIZE'
        BEGIN
            SET @Sql = N'ALTER INDEX ' + QUOTENAME(@IndexName) +
                      N' ON ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) +
                      N' REORGANIZE;';
            EXEC(@Sql);
        END
        ELSE
        BEGIN
            BEGIN TRY
                SET @Sql = N'ALTER INDEX ' + QUOTENAME(@IndexName) +
                          N' ON ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) +
                          N' REBUILD WITH (ONLINE = ON);';
                EXEC(@Sql);
            END TRY
            BEGIN CATCH
                -- Fallback when ONLINE rebuild is not supported.
                SET @Sql = N'ALTER INDEX ' + QUOTENAME(@IndexName) +
                          N' ON ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) +
                          N' REBUILD;';
                EXEC(@Sql);
            END CATCH;
        END;
    END TRY
    BEGIN CATCH
        PRINT CONCAT('Index maintenance failed for ', @SchemaName, '.', @TableName, ' -> ', @IndexName, ': ', ERROR_MESSAGE());
    END CATCH;

    FETCH NEXT FROM cur INTO @RowId, @SchemaName, @TableName, @IndexName, @PlannedAction;
END;

CLOSE cur;
DEALLOCATE cur;

PRINT 'Index maintenance completed.';
