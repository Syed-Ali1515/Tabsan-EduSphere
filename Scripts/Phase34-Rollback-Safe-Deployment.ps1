#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$ServerInstance = "localhost",

    [Parameter(Mandatory = $false)]
    [string]$DatabaseName = "Tabsan-EduSphere",

    [Parameter(Mandatory = $false)]
    [string]$ScriptsRoot = ".\\Scripts",

    [Parameter(Mandatory = $false)]
    [string]$BackupDirectory = ".\\Artifacts\\Phase34\\RollbackBackups",

    [Parameter(Mandatory = $false)]
    [switch]$UseTrustedConnection = $true,

    [Parameter(Mandatory = $false)]
    [string]$SqlUser,

    [Parameter(Mandatory = $false)]
    [string]$SqlPassword,

    [Parameter(Mandatory = $false)]
    [ValidateSet("Demo", "Clean")]
    [string]$DeploymentMode = "Demo",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$scriptSequence = if ($DeploymentMode -eq "Clean") {
    @(
        @{ Name = "01-Schema-Current.sql"; Database = "master" },
        @{ Name = "Seed-Core-Clean.sql"; Database = $DatabaseName },
        @{ Name = "04-Maintenance-Indexes-And-Views.sql"; Database = $DatabaseName },
        @{ Name = "05-PostDeployment-Checks-Clean.sql"; Database = $DatabaseName }
    )
}
else {
    @(
        @{ Name = "01-Schema-Current.sql"; Database = "master" },
        @{ Name = "02-Seed-Core.sql"; Database = $DatabaseName },
        @{ Name = "03-FullDummyData.sql"; Database = $DatabaseName },
        @{ Name = "04-Maintenance-Indexes-And-Views.sql"; Database = $DatabaseName },
        @{ Name = "05-PostDeployment-Checks.sql"; Database = $DatabaseName }
    )
}

function Get-SqlcmdAuthArgs {
    if ($UseTrustedConnection.IsPresent) {
        return @("-E")
    }

    if ([string]::IsNullOrWhiteSpace($SqlUser) -or [string]::IsNullOrWhiteSpace($SqlPassword)) {
        throw "SQL authentication selected but SqlUser/SqlPassword were not provided."
    }

    return @("-U", $SqlUser, "-P", $SqlPassword)
}

function Invoke-SqlcmdScriptFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Database,

        [Parameter(Mandatory = $true)]
        [string]$ScriptPath
    )

    $authArgs = Get-SqlcmdAuthArgs
    $args = @("-S", $ServerInstance) + $authArgs + @("-d", $Database, "-b", "-i", $ScriptPath)

    if ($DryRun) {
        Write-Host "[DryRun] sqlcmd $($args -join ' ')" -ForegroundColor Yellow
        return
    }

    & sqlcmd @args
}

function Invoke-SqlcmdQuery {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Database,

        [Parameter(Mandatory = $true)]
        [string]$Query
    )

    $authArgs = Get-SqlcmdAuthArgs
    $args = @("-S", $ServerInstance) + $authArgs + @("-d", $Database, "-b", "-Q", $Query, "-h", "-1", "-W")

    if ($DryRun) {
        Write-Host "[DryRun] sqlcmd $($args -join ' ')" -ForegroundColor Yellow
        return ""
    }

    (& sqlcmd @args | Out-String).Trim()
}

if (-not $DryRun -and -not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd is required but was not found in PATH."
}

foreach ($step in $scriptSequence) {
    $path = Join-Path $ScriptsRoot $step.Name
    if (-not (Test-Path $path)) {
        throw "Missing script file: $path"
    }
}

New-Item -ItemType Directory -Path $BackupDirectory -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$preBackupFile = Join-Path $BackupDirectory ("{0}-predeploy-{1}.bak" -f $DatabaseName, $timestamp)

Write-Host "[Phase34] Rollback-safe deployment started" -ForegroundColor Cyan
Write-Host "[Phase34] Server: $ServerInstance | Database: $DatabaseName"
Write-Host "[Phase34] DeploymentMode: $DeploymentMode"

$databaseExistsQuery = "SELECT CASE WHEN DB_ID(N'$DatabaseName') IS NULL THEN 0 ELSE 1 END;"
$databaseExists = Invoke-SqlcmdQuery -Database "master" -Query $databaseExistsQuery
$hadPreBackup = $false

if ($databaseExists -eq "1") {
    Write-Host "[Phase34] Existing database detected. Taking pre-deployment backup..."
    $backupQuery = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$preBackupFile'
WITH INIT, COPY_ONLY, CHECKSUM, STATS = 10;
"@
    Invoke-SqlcmdQuery -Database "master" -Query $backupQuery | Out-Null
    $hadPreBackup = $true
    Write-Host "[Phase34] Pre-deployment backup complete: $preBackupFile" -ForegroundColor Green
}
else {
    Write-Host "[Phase34] Database does not exist yet. No pre-deployment backup required."
}

try {
    foreach ($step in $scriptSequence) {
        $path = Join-Path $ScriptsRoot $step.Name
        Write-Host "[Phase34] Executing $($step.Name) on [$($step.Database)]"
        Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath $path
    }

    Write-Host "[Phase34] Deployment completed successfully." -ForegroundColor Green
    if ($hadPreBackup) {
        Write-Host "[Phase34] Rollback backup retained at: $preBackupFile"
    }
}
catch {
    Write-Error "[Phase34] Deployment failed: $($_.Exception.Message)"

    if ($hadPreBackup) {
        Write-Host "[Phase34] Restoring pre-deployment backup..." -ForegroundColor Yellow

        $restoreQuery = @"
ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE [$DatabaseName]
FROM DISK = N'$preBackupFile'
WITH REPLACE, RECOVERY, CHECKSUM;
ALTER DATABASE [$DatabaseName] SET MULTI_USER;
"@
        Invoke-SqlcmdQuery -Database "master" -Query $restoreQuery | Out-Null
        Write-Host "[Phase34] Rollback restore completed." -ForegroundColor Green
    }
    else {
        Write-Warning "[Phase34] No pre-deployment backup exists; automatic rollback skipped."
    }

    throw
}
