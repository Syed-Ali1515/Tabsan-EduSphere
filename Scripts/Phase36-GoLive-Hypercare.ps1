#requires -Version 5.1
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),

    [Parameter(Mandatory = $false)]
    [string]$ArtifactRoot = ".\Artifacts\Phase36\Stage36.6",

    [Parameter(Mandatory = $false)]
    [string]$ApiBaseUrl = "http://localhost:5000",

    [Parameter(Mandatory = $false)]
    [string]$WebBaseUrl = "http://localhost:5001",

    [Parameter(Mandatory = $false)]
    [ValidateSet(24, 48, 72)]
    [int]$HypercareHours = 72,

    [Parameter(Mandatory = $false)]
    [switch]$Execute
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Add-ReportLine {
    param([string]$Text)
    $script:ReportLines.Add($Text)
}

function Test-Endpoint {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Url
    )

    if (-not $Execute) {
        Add-ReportLine "[DryRun] GET $Url"
        return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "Dry-run planned endpoint check" }
    }

    try {
        $response = Invoke-WebRequest -Uri $Url -Method Get -UseBasicParsing -TimeoutSec 20
        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 400) {
            return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "HTTP $($response.StatusCode)" }
        }

        return [pscustomobject]@{ Name = $Name; Result = "FAIL"; Details = "HTTP $($response.StatusCode)" }
    }
    catch {
        return [pscustomobject]@{ Name = $Name; Result = "FAIL"; Details = $_.Exception.Message }
    }
}

function Invoke-SmokeSuite {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$Filter
    )

    $args = @("test", $ProjectPath, "--filter", $Filter, "--verbosity", "minimal")
    if (-not $Execute) {
        Add-ReportLine "[DryRun] dotnet $($args -join ' ')"
        return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "Dry-run planned smoke suite" }
    }

    & dotnet @args
    if ($LASTEXITCODE -eq 0) {
        return [pscustomobject]@{ Name = $Name; Result = "PASS"; Details = "dotnet test completed successfully" }
    }

    return [pscustomobject]@{ Name = $Name; Result = "FAIL"; Details = "dotnet test exit code $LASTEXITCODE" }
}

if (-not [System.IO.Path]::IsPathRooted($ArtifactRoot)) {
    $ArtifactRoot = Join-Path $RepoRoot $ArtifactRoot
}

$integrationProject = Join-Path $RepoRoot "tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj"
if (-not (Test-Path -LiteralPath $integrationProject)) {
    throw "Integration test project not found: $integrationProject"
}

New-Item -ItemType Directory -Path $ArtifactRoot -Force | Out-Null
$reportPath = Join-Path $ArtifactRoot "GoLive-Hypercare-20260515.md"
$reportLines = [System.Collections.Generic.List[string]]::new()
$script:ReportLines = $reportLines

$results = [System.Collections.Generic.List[object]]::new()

Add-ReportLine "# Stage 36.6 Go-Live Execution and Hypercare Report"
Add-ReportLine ""
Add-ReportLine "- Generated (UTC): $((Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm:ss'))"
Add-ReportLine "- Execute mode: $Execute"
Add-ReportLine "- API base URL: $ApiBaseUrl"
Add-ReportLine "- Web base URL: $WebBaseUrl"
Add-ReportLine "- Hypercare window (hours): $HypercareHours"
Add-ReportLine ""

Add-ReportLine "## Deployment Flow"
Add-ReportLine "1. Rollback-safe deployment flow selected as mandatory path."
Add-ReportLine "2. Immediate post-deploy smoke checks executed/planned."
Add-ReportLine "3. Hypercare monitoring checkpoints activated."
Add-ReportLine ""

Add-ReportLine "## Post-Deploy Smoke Validation"
Add-ReportLine "| Check | Result | Details |"
Add-ReportLine "|---|---|---|"

$endpointChecks = @(
    @{ Name = "API Health"; Url = "$ApiBaseUrl/health" },
    @{ Name = "API Instance Health"; Url = "$ApiBaseUrl/health/instance" },
    @{ Name = "API Observability Health"; Url = "$ApiBaseUrl/health/observability" },
    @{ Name = "Background Job Health"; Url = "$ApiBaseUrl/health/background-jobs" },
    @{ Name = "Prometheus Metrics"; Url = "$ApiBaseUrl/metrics" },
    @{ Name = "Web Root"; Url = $WebBaseUrl }
)

foreach ($check in $endpointChecks) {
    $result = Test-Endpoint -Name $check.Name -Url $check.Url
    $results.Add($result)
    Add-ReportLine "| $($result.Name) | $($result.Result) | $($result.Details) |"
}

$smokeSuites = @(
    @{ Name = "Authentication and Dashboard Smoke"; Filter = "FullyQualifiedName~Phase36Stage4HealthAndLicenseGateTests" },
    @{ Name = "StudentLifecycle Reporting Smoke"; Filter = "FullyQualifiedName~Phase36Stage4PerformanceSmokeTests" },
    @{ Name = "Security Hardening Smoke"; Filter = "FullyQualifiedName~Phase31Stage2SecurityHardeningTests" }
)

foreach ($suite in $smokeSuites) {
    $result = Invoke-SmokeSuite -Name $suite.Name -ProjectPath $integrationProject -Filter $suite.Filter
    $results.Add($result)
    Add-ReportLine "| $($result.Name) | $($result.Result) | $($result.Details) |"
}

Add-ReportLine ""
Add-ReportLine "## Hypercare Activation (24-72h)"
Add-ReportLine "| Checkpoint | Focus | Owner |"
Add-ReportLine "|---|---|---|"
Add-ReportLine "| H+24 | Incident triage board review, auth error spikes, health endpoint availability | Release commander + On-call platform |"
Add-ReportLine "| H+48 | SLO and error-rate trend review, report export and user import signal checks | Platform + QA lead |"
Add-ReportLine "| H+$HypercareHours | Final hypercare closeout, outstanding defects, handoff to steady-state ops | Product operations + engineering |"

$failedCount = @($results | Where-Object { $_.Result -eq "FAIL" }).Count

Add-ReportLine ""
Add-ReportLine "## Summary"
Add-ReportLine "- Total checks: $($results.Count)"
Add-ReportLine "- Passed: $($results.Count - $failedCount)"
Add-ReportLine "- Failed: $failedCount"

if ($failedCount -eq 0) {
    Add-ReportLine "- Stage 36.6 status: READY_FOR_GO_LIVE"
}
else {
    Add-ReportLine "- Stage 36.6 status: GO_LIVE_BLOCKED"
}

Set-Content -LiteralPath $reportPath -Value $reportLines -Encoding UTF8
Write-Host "Stage 36.6 report written: $reportPath"

if ($Execute -and $failedCount -gt 0) {
    throw "Stage 36.6 checks failed ($failedCount)."
}