#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),

    [Parameter(Mandatory = $false)]
    [string]$ArtifactRoot = ".\Artifacts\Phase38",

    [Parameter(Mandatory = $false)]
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-ReportLine {
    param([string]$Text)
    $script:ReportLines.Add($Text)
}

if (-not [System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $RepoRoot $ArtifactRoot
}

$assetFolders = @(
    "Docs",
    "PPT",
    "Project startup Docs",
    "Scripts",
    "UAT-SAT docs",
    "User Guide",
    "New Enhancements"
)

New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null

$packageRoot = Join-Path $ArtifactRoot "NonRuntimeAssets"
if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $packageRoot -Force | Out-Null

$reportPath = Join-Path $ArtifactRoot "NonRuntime-Asset-Separation-20260515.md"
$reportLines = [System.Collections.Generic.List[string]]::new()
$script:ReportLines = $reportLines

$results = [System.Collections.Generic.List[object]]::new()

Add-ReportLine "# Phase 38 Non-Runtime Asset Separation Report"
Add-ReportLine ""
Add-ReportLine "- Generated (UTC): $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))"
Add-ReportLine "- Execute mode: $Execute"
Add-ReportLine ""
Add-ReportLine "| Folder | Result | Details |"
Add-ReportLine "|---|---|---|"

foreach ($folder in $assetFolders) {
    $source = Join-Path $RepoRoot $folder
    $target = Join-Path $packageRoot $folder

    if (-not (Test-Path -LiteralPath $source)) {
        $results.Add([pscustomobject]@{ Folder = $folder; Result = "FAIL"; Details = "Source folder missing" })
        Add-ReportLine "| $folder | FAIL | Source folder missing |"
        continue
    }

    if ($Execute) {
        Copy-Item -LiteralPath $source -Destination $target -Recurse -Force
        $results.Add([pscustomobject]@{ Folder = $folder; Result = "PASS"; Details = "Copied to packaging root" })
        Add-ReportLine "| $folder | PASS | Copied to packaging root |"
    }
    else {
        $results.Add([pscustomobject]@{ Folder = $folder; Result = "PASS"; Details = "Dry-run planned copy" })
        Add-ReportLine "| $folder | PASS | Dry-run planned copy |"
    }
}

$archivePath = Join-Path $ArtifactRoot "NonRuntime-Assets-20260515.zip"
if ($Execute) {
    if (Test-Path -LiteralPath $archivePath) { Remove-Item -LiteralPath $archivePath -Force }
    Compress-Archive -Path (Join-Path $packageRoot "*") -DestinationPath $archivePath -CompressionLevel Optimal
    Add-ReportLine ""
    Add-ReportLine "- Non-runtime package: $archivePath"
}
else {
    Add-ReportLine ""
    Add-ReportLine "- [DryRun] Non-runtime package path: $archivePath"
}

$failed = @($results | Where-Object { $_.Result -eq "FAIL" }).Count
Add-ReportLine ""
Add-ReportLine "## Summary"
Add-ReportLine "- Total folders: $($results.Count)"
Add-ReportLine "- Passed: $($results.Count - $failed)"
Add-ReportLine "- Failed: $failed"
Add-ReportLine "- Phase 38 status: $(if ($failed -eq 0) { 'PASS' } else { 'FAIL' })"

Set-Content -LiteralPath $reportPath -Value $reportLines -Encoding UTF8
Write-Host "Phase 38 report written: $reportPath"

if ($Execute -and $failed -gt 0) {
    throw "Phase 38 non-runtime asset separation failed ($failed folders)."
}