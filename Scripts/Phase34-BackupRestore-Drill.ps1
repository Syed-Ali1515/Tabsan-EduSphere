#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ServerInstance = "localhost",

    [Parameter(Mandatory = $false)]
    [string]$DatabaseName = "Tabsan-EduSphere",

    [Parameter(Mandatory = $false)]
    [string]$DrillDatabaseName = "Tabsan-EduSphere-Drill",

    [Parameter(Mandatory = $false)]
    [string]$BackupDirectory = ".\\Artifacts\\Phase34\\Backups",

    [Parameter(Mandatory = $false)]
    [switch]$UseTrustedConnection = $true,

    [Parameter(Mandatory = $false)]
    [string]$SqlUser,

    [Parameter(Mandatory = $false)]
    [string]$SqlPassword,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-SqlcmdAuthArgs {
    if ($UseTrustedConnection.IsPresent) {
        return @("-E")
    }

    if ([string]::IsNullOrWhiteSpace($SqlUser) -or [string]::IsNullOrWhiteSpace($SqlPassword)) {
        throw "SQL authentication selected but SqlUser/SqlPassword were not provided."
    }

    return @("-U", $SqlUser, "-P", $SqlPassword)
}

function Invoke-SqlcmdQuery {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Database,

        [Parameter(Mandatory = $true)]
        [string]$Query
    )

    $authArgs = Get-SqlcmdAuthArgs
    $args = @("-S", $ServerInstance) + $authArgs + @("-d", $Database, "-b", "-Q", $Query)

    if ($DryRun) {
        Write-Host "[DryRun] sqlcmd $($args -join ' ')" -ForegroundColor Yellow
        return @()
    }

    & sqlcmd @args
}

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd is required but was not found in PATH."
}

New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupFile = Join-Path $BackupDirectory ("{0}-{1}.bak" -f $DatabaseName, $timestamp)

Write-Host "[Phase34] Starting backup/restore drill" -ForegroundColor Cyan
Write-Host "[Phase34] Server: $ServerInstance | Source DB: $DatabaseName | Drill DB: $DrillDatabaseName"
Write-Host "[Phase34] Backup file: $backupFile"

$backupQuery = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$backupFile'
WITH INIT, COPY_ONLY, CHECKSUM, STATS = 10;
"@
Invoke-SqlcmdQuery -Database "master" -Query $backupQuery | Out-Null

$verifyQuery = "RESTORE VERIFYONLY FROM DISK = N'$backupFile' WITH CHECKSUM;"
Invoke-SqlcmdQuery -Database "master" -Query $verifyQuery | Out-Null

$restoreQuery = @"
DECLARE @backupFile nvarchar(4000) = N'$backupFile';
DECLARE @drillDb sysname = N'$DrillDatabaseName';
DECLARE @dataPath nvarchar(4000) = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultDataPath'));
DECLARE @logPath nvarchar(4000) = CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultLogPath'));

IF @dataPath IS NULL SET @dataPath = N'C:\\Program Files\\Microsoft SQL Server\\MSSQL\\DATA\\';
IF @logPath IS NULL SET @logPath = @dataPath;

IF DB_ID(@drillDb) IS NOT NULL
BEGIN
    EXEC(N'ALTER DATABASE [' + @drillDb + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;');
    EXEC(N'DROP DATABASE [' + @drillDb + N'];');
END

DECLARE @files TABLE
(
    LogicalName nvarchar(128),
    PhysicalName nvarchar(260),
    Type char(1),
    FileGroupName nvarchar(128),
    [Size] numeric(20,0),
    MaxSize numeric(20,0),
    FileId bigint,
    CreateLSN numeric(25,0),
    DropLSN numeric(25,0),
    UniqueId uniqueidentifier,
    ReadOnlyLSN numeric(25,0),
    ReadWriteLSN numeric(25,0),
    BackupSizeInBytes bigint,
    SourceBlockSize int,
    FileGroupId int,
    LogGroupGUID uniqueidentifier,
    DifferentialBaseLSN numeric(25,0),
    DifferentialBaseGUID uniqueidentifier,
    IsReadOnly bit,
    IsPresent bit,
    TDEThumbprint varbinary(32),
    SnapshotURL nvarchar(360)
);

INSERT INTO @files
EXEC(N'RESTORE FILELISTONLY FROM DISK = ''' + @backupFile + N'''');

DECLARE @restoreSql nvarchar(max) = N'RESTORE DATABASE [' + @drillDb + N'] FROM DISK = N''' + @backupFile + N''' WITH RECOVERY, REPLACE';

SELECT @restoreSql = @restoreSql +
    N', MOVE N''' + LogicalName + N''' TO N''' +
    CASE WHEN [Type] = 'L'
        THEN @logPath + @drillDb + N'_' + REPLACE(LogicalName, N' ', N'_') + N'.ldf'
        ELSE @dataPath + @drillDb + N'_' + REPLACE(LogicalName, N' ', N'_') + N'.mdf'
    END + N''''
FROM @files;

EXEC(@restoreSql);
"@
Invoke-SqlcmdQuery -Database "master" -Query $restoreQuery | Out-Null

$postCheckQuery = @"
SELECT TOP (1)
    name AS DatabaseName,
    create_date AS CreatedAt,
    state_desc AS StateDescription
FROM sys.databases
WHERE name = N'$DrillDatabaseName';
"@
Invoke-SqlcmdQuery -Database "master" -Query $postCheckQuery

Write-Host "[Phase34] Backup/restore drill completed successfully." -ForegroundColor Green
Write-Host "[Phase34] Evidence: backup file '$backupFile', restored drill database '$DrillDatabaseName'."
