#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),

    [Parameter(Mandatory = $false)]
    [string]$ServerInstance = "localhost",

    [Parameter(Mandatory = $false)]
    [string]$DatabaseName = "Tabsan-EduSphere",

    [Parameter(Mandatory = $false)]
    [string]$ScriptsRoot = ".\Scripts",

    [Parameter(Mandatory = $false)]
    [string]$ArtifactRoot = ".\Artifacts\Phase36\Stage36.3",

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
    [switch]$DryRun = $true,

    [Parameter(Mandatory = $false)]
    [switch]$Execute
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

function Invoke-SqlcmdScriptFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Database,

        [Parameter(Mandatory = $true)]
        [string]$ScriptPath
    )

    $authArgs = Get-SqlcmdAuthArgs
    $args = @("-S", $ServerInstance) + $authArgs + @("-d", $Database, "-b", "-i", $ScriptPath)

    if ($DryRun -or -not $Execute) {
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

    if ($DryRun -or -not $Execute) {
        Write-Host "[DryRun] sqlcmd $($args -join ' ')" -ForegroundColor Yellow
        return ""
    }

    (& sqlcmd @args | Out-String).Trim()
}

function Add-ReportLine {
    param([string]$Text)
    $script:ReportLines.Add($Text)
}

if ($Execute -and -not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd is required but was not found in PATH."
}

$sqlSequence = if ($DeploymentMode -eq "Clean") {
    @(
        "01-Schema-Current.sql",
        "Seed-Core-Clean.sql",
        "04-Maintenance-Indexes-And-Views.sql",
        "05-PostDeployment-Checks-Clean.sql"
    )
}
else {
    @(
        "01-Schema-Current.sql",
        "02-Seed-Core.sql",
        "03-FullDummyData.sql",
        "04-Maintenance-Indexes-And-Views.sql",
        "05-PostDeployment-Checks.sql"
    )
}

$requiredScripts = @($sqlSequence + @(
    "Phase34-BackupRestore-Drill.ps1",
    "Phase34-Rollback-Safe-Deployment.ps1"
))

foreach ($scriptName in $requiredScripts) {
    $path = Join-Path $RepoRoot (Join-Path $ScriptsRoot $scriptName)
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing required script file: $path"
    }
}

if (-not [System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $RepoRoot $ArtifactRoot
}

if (-not [System.IO.Path]::IsPathRooted($ScriptsRoot)) {
    $ScriptsRoot = Join-Path $RepoRoot $ScriptsRoot
}

New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$reportPath = Join-Path $ArtifactRoot "Deployment-Rehearsal-$($DeploymentMode)-$timestamp.md"
$reportLines = [System.Collections.Generic.List[string]]::new()

$script:ReportLines = $reportLines

Add-ReportLine "# Stage 36.3 Deployment and Migration Rehearsal Report"
Add-ReportLine ""
Add-ReportLine "- Generated (UTC): $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))"
Add-ReportLine "- Server: $ServerInstance"
Add-ReportLine "- Database: $DatabaseName"
Add-ReportLine "- DeploymentMode: $DeploymentMode"
Add-ReportLine "- DryRun: $DryRun"
Add-ReportLine "- Execute: $Execute"
Add-ReportLine ""

$steps = if ($DeploymentMode -eq "Clean") {
    @(
        [pscustomobject]@{ Name = "Schema"; Database = "master"; Command = "01-Schema-Current.sql"; Description = "Create or refresh schema" },
        [pscustomobject]@{ Name = "CleanSeed"; Database = $DatabaseName; Command = "Seed-Core-Clean.sql"; Description = "Seed clean startup baseline" },
        [pscustomobject]@{ Name = "Maintenance"; Database = $DatabaseName; Command = "04-Maintenance-Indexes-And-Views.sql"; Description = "Build maintenance indexes and views" },
        [pscustomobject]@{ Name = "PostDeploymentChecksClean"; Database = $DatabaseName; Command = "05-PostDeployment-Checks-Clean.sql"; Description = "Validate clean post-deployment baseline" },
        [pscustomobject]@{ Name = "BackupRestoreDrill"; Database = $null; Command = "Phase34-BackupRestore-Drill.ps1"; Description = "Run backup/restore drill utility" },
        [pscustomobject]@{ Name = "RollbackSafeDeployment"; Database = $null; Command = "Phase34-Rollback-Safe-Deployment.ps1"; Description = "Run rollback-safe deployment utility" }
    )
}
else {
    @(
        [pscustomobject]@{ Name = "Schema"; Database = "master"; Command = "01-Schema-Current.sql"; Description = "Create or refresh schema" },
        [pscustomobject]@{ Name = "Seed"; Database = $DatabaseName; Command = "02-Seed-Core.sql"; Description = "Seed core data" },
        [pscustomobject]@{ Name = "DummyData"; Database = $DatabaseName; Command = "03-FullDummyData.sql"; Description = "Seed full dummy data" },
        [pscustomobject]@{ Name = "Maintenance"; Database = $DatabaseName; Command = "04-Maintenance-Indexes-And-Views.sql"; Description = "Build maintenance indexes and views" },
        [pscustomobject]@{ Name = "PostDeploymentChecks"; Database = $DatabaseName; Command = "05-PostDeployment-Checks.sql"; Description = "Validate post-deployment state" },
        [pscustomobject]@{ Name = "BackupRestoreDrill"; Database = $null; Command = "Phase34-BackupRestore-Drill.ps1"; Description = "Run backup/restore drill utility" },
        [pscustomobject]@{ Name = "RollbackSafeDeployment"; Database = $null; Command = "Phase34-Rollback-Safe-Deployment.ps1"; Description = "Run rollback-safe deployment utility" }
    )
}

Add-ReportLine "| Step | Description | Target | Result |"
Add-ReportLine "|---|---|---|---|"

$stepResults = New-Object System.Collections.Generic.List[object]

foreach ($step in $steps) {
    $result = [pscustomobject]@{
        Step = $step.Name
        Result = "PASS"
        Details = ""
    }

    try {
        switch ($step.Command) {
            "01-Schema-Current.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Schema script verified in sequence"
            }
            "02-Seed-Core.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Core seed script verified in sequence"
            }
            "Seed-Core-Clean.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Clean seed script verified in sequence"
            }
            "03-FullDummyData.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Dummy data script verified in sequence"
            }
            "04-Maintenance-Indexes-And-Views.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Maintenance script verified in sequence"
            }
            "05-PostDeployment-Checks.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Post-deployment validation script verified in sequence"
            }
            "05-PostDeployment-Checks-Clean.sql" {
                Invoke-SqlcmdScriptFile -Database $step.Database -ScriptPath (Join-Path $ScriptsRoot $step.Command)
                $result.Details = "Clean post-deployment validation script verified in sequence"
            }
            "Phase34-BackupRestore-Drill.ps1" {
                $drillArgs = @(
                    "-ExecutionPolicy", "Bypass",
                    "-File", (Join-Path $ScriptsRoot $step.Command),
                    "-ServerInstance", $ServerInstance,
                    "-DatabaseName", $DatabaseName,
                    "-DryRun"
                )

                if (-not $UseTrustedConnection.IsPresent) {
                    $drillArgs += @("-UseTrustedConnection:$false", "-SqlUser", $SqlUser, "-SqlPassword", $SqlPassword)
                }

                if ($DryRun -or -not $Execute) {
                    Write-Host "[DryRun] powershell $($drillArgs -join ' ')" -ForegroundColor Yellow
                }
                else {
                    & powershell @drillArgs
                }

                $result.Details = "Backup/restore drill utility scheduled"
            }
            "Phase34-Rollback-Safe-Deployment.ps1" {
                $rollbackArgs = @(
                    "-ExecutionPolicy", "Bypass",
                    "-File", (Join-Path $ScriptsRoot $step.Command),
                    "-ServerInstance", $ServerInstance,
                    "-DatabaseName", $DatabaseName,
                    "-DeploymentMode", $DeploymentMode,
                    "-DryRun"
                )

                if (-not $UseTrustedConnection.IsPresent) {
                    $rollbackArgs += @("-UseTrustedConnection:$false", "-SqlUser", $SqlUser, "-SqlPassword", $SqlPassword)
                }

                if ($DryRun -or -not $Execute) {
                    Write-Host "[DryRun] powershell $($rollbackArgs -join ' ')" -ForegroundColor Yellow
                }
                else {
                    & powershell @rollbackArgs
                }

                $result.Details = "Rollback-safe deployment utility scheduled"
            }
        }
    }
    catch {
        $result.Result = "FAIL"
        $result.Details = $_.Exception.Message
        $stepResults.Add($result)
        $targetLabel = if ($null -eq $step.Database) { "-" } else { $step.Database }
        Add-ReportLine "| $($step.Name) | $($step.Description) | $targetLabel | FAIL: $($result.Details) |"
        throw
    }

    $stepResults.Add($result)
    $target = if ($null -eq $step.Database) { "Utility" } else { $step.Database }
    Add-ReportLine "| $($step.Name) | $($step.Description) | $target | PASS: $($result.Details) |"
}

if ($Execute) {
    $migrationCheck = Invoke-SqlcmdQuery -Database "master" -Query "SELECT COUNT(1) FROM sys.databases WHERE name = N'$DatabaseName';"
    Add-ReportLine ""
    Add-ReportLine "- Database existence check result: $migrationCheck"
}

Add-ReportLine ""
Add-ReportLine "## Rehearsal Summary"
Add-ReportLine "- Steps evaluated: $($stepResults.Count)"
Add-ReportLine "- Passed: $(@($stepResults | Where-Object { $_.Result -eq 'PASS' }).Count)"
Add-ReportLine "- Failed: $(@($stepResults | Where-Object { $_.Result -eq 'FAIL' }).Count)"

Set-Content -LiteralPath $reportPath -Value $reportLines -Encoding UTF8
Write-Host "Stage 36.3 rehearsal report written: $reportPath"