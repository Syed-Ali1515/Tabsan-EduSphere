#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$ArtifactRoot = ".\Artifacts\Phase37",

    [Parameter(Mandatory = $false)]
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-ReportLine {
    param([string]$Text)
    $script:ReportLines.Add($Text)
}

function Invoke-PublishProject {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$ProjectPath,
        [Parameter(Mandatory = $true)][string]$OutputPath
    )

    $args = @("publish", $ProjectPath, "-c", $Configuration, "-o", $OutputPath)
    if (-not $Execute) {
        Add-ReportLine "[DryRun] dotnet $($args -join ' ')"
        return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "Dry-run planned publish" }
    }

    & dotnet @args
    if ($LASTEXITCODE -eq 0) {
        return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "Publish succeeded" }
    }

    return [pscustomobject]@{ Name = $Name; Result = "FAIL"; Details = "dotnet publish exit code $LASTEXITCODE" }
}

if (-not [System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $RepoRoot $ArtifactRoot
}

$appRoot = Join-Path $ArtifactRoot "App"
$licenseRoot = Join-Path $ArtifactRoot "License"

$projects = @(
    @{ Name = "API"; Project = "src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj"; Output = Join-Path $appRoot "API" },
    @{ Name = "Web"; Project = "src/Tabsan.EduSphere.Web/Tabsan.EduSphere.Web.csproj"; Output = Join-Path $appRoot "Web" },
    @{ Name = "BackgroundJobs"; Project = "src/Tabsan.EduSphere.BackgroundJobs/Tabsan.EduSphere.BackgroundJobs.csproj"; Output = Join-Path $appRoot "BackgroundJobs" },
    @{ Name = "LicenseApp"; Project = "tools/Tabsan.Lic/Tabsan.Lic.csproj"; Output = Join-Path $licenseRoot "Tabsan.Lic" }
)

foreach ($entry in $projects) {
    $fullProject = Join-Path $RepoRoot $entry.Project
    if (-not (Test-Path -LiteralPath $fullProject)) {
        throw "Project not found: $fullProject"
    }
}

New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null
New-Item -ItemType Directory -Path $appRoot -Force | Out-Null
New-Item -ItemType Directory -Path $licenseRoot -Force | Out-Null

$reportPath = Join-Path $ArtifactRoot "Publish-Separation-20260515.md"
$reportLines = [System.Collections.Generic.List[string]]::new()
$script:ReportLines = $reportLines
$results = [System.Collections.Generic.List[object]]::new()

Add-ReportLine "# Phase 37 App and License Publish Separation Report"
Add-ReportLine ""
Add-ReportLine "- Generated (UTC): $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))"
Add-ReportLine "- Execute mode: $Execute"
Add-ReportLine "- Configuration: $Configuration"
Add-ReportLine ""
Add-ReportLine "| Target | Result | Details |"
Add-ReportLine "|---|---|---|"

foreach ($entry in $projects) {
    $fullProject = Join-Path $RepoRoot $entry.Project
    $result = Invoke-PublishProject -Name $entry.Name -ProjectPath $fullProject -OutputPath $entry.Output
    $results.Add($result)
    Add-ReportLine "| $($result.Name) | $($result.Result) | $($result.Details) |"
}

$appPackage = Join-Path $ArtifactRoot "Tabsan.EduSphere-App-Publish-20260515.zip"
$licensePackage = Join-Path $ArtifactRoot "Tabsan.Lic-Publish-20260515.zip"

if ($Execute) {
    if (Test-Path -LiteralPath $appPackage) { Remove-Item -LiteralPath $appPackage -Force }
    if (Test-Path -LiteralPath $licensePackage) { Remove-Item -LiteralPath $licensePackage -Force }
    Compress-Archive -Path (Join-Path $appRoot "*") -DestinationPath $appPackage -CompressionLevel Optimal
    Compress-Archive -Path (Join-Path $licenseRoot "*") -DestinationPath $licensePackage -CompressionLevel Optimal
    Add-ReportLine ""
    Add-ReportLine "- App package: $appPackage"
    Add-ReportLine "- License package: $licensePackage"
}
else {
    Add-ReportLine ""
    Add-ReportLine "- [DryRun] App package path: $appPackage"
    Add-ReportLine "- [DryRun] License package path: $licensePackage"
}

$failed = @($results | Where-Object { $_.Result -eq "FAIL" }).Count
Add-ReportLine ""
Add-ReportLine "## Summary"
Add-ReportLine "- Total targets: $($results.Count)"
Add-ReportLine "- Passed: $($results.Count - $failed)"
Add-ReportLine "- Failed: $failed"
Add-ReportLine "- Phase 37 status: $(if ($failed -eq 0) { 'PASS' } else { 'FAIL' })"

Set-Content -LiteralPath $reportPath -Value $reportLines -Encoding UTF8
Write-Host "Phase 37 report written: $reportPath"

if ($Execute -and $failed -gt 0) {
    throw "Phase 37 publish separation failed ($failed targets)."
}