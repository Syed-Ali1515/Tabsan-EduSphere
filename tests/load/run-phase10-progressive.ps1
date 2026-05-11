#!/usr/bin/env pwsh

param(
    [Parameter(Position = 0)]
    [ValidateSet('progressive', 'extended')]
    [string]$Plan = 'progressive',

    [Parameter(Position = 1)]
    [string]$BaseUrl = 'http://localhost:5181',

    [switch]$Distributed,
    [int]$GeneratorTotal = 1,
    [int]$GeneratorIndex = 1,
    [string]$TestUsername,
    [string]$TestPassword,
    [switch]$AllowRawOutput,
    [switch]$OutputJson,
    [int]$RetestCount = 1
)

$ProjectRoot = 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere'
$TestsDir = Join-Path $ProjectRoot 'tests\load'
$ScriptPath = Join-Path $TestsDir 'k6-phase10-progressive.js'

function Resolve-K6Path {
    $cmd = Get-Command k6 -ErrorAction SilentlyContinue
    if ($cmd) {
        return $cmd.Source
    }

    $fallback = 'C:\Program Files\k6\k6.exe'
    if (Test-Path $fallback) {
        return $fallback
    }

    throw 'k6 not found. Install k6 or add it to PATH.'
}

function Get-Phase10GatePlan {
    param([string]$PlanName)

    $basePlan = @(
        [pscustomobject]@{ Name = '10k'; TargetUsers = 10000; TargetRps = 120; RampUp = '2m'; Hold = '6m'; RampDown = '2m'; P95 = 2500; ErrorRate = 0.05 },
        [pscustomobject]@{ Name = '20k'; TargetUsers = 20000; TargetRps = 180; RampUp = '2m'; Hold = '8m'; RampDown = '2m'; P95 = 2750; ErrorRate = 0.06 },
        [pscustomobject]@{ Name = '50k'; TargetUsers = 50000; TargetRps = 300; RampUp = '3m'; Hold = '10m'; RampDown = '3m'; P95 = 3000; ErrorRate = 0.08 },
        [pscustomobject]@{ Name = '80k'; TargetUsers = 80000; TargetRps = 450; RampUp = '4m'; Hold = '12m'; RampDown = '4m'; P95 = 3500; ErrorRate = 0.10 },
        [pscustomobject]@{ Name = '100k'; TargetUsers = 100000; TargetRps = 600; RampUp = '4m'; Hold = '15m'; RampDown = '4m'; P95 = 4000; ErrorRate = 0.12 }
    )

    if ($PlanName -eq 'extended') {
        $basePlan += @(
            [pscustomobject]@{ Name = '250k'; TargetUsers = 250000; TargetRps = 1200; RampUp = '5m'; Hold = '18m'; RampDown = '5m'; P95 = 5000; ErrorRate = 0.15 },
            [pscustomobject]@{ Name = '500k'; TargetUsers = 500000; TargetRps = 1800; RampUp = '8m'; Hold = '25m'; RampDown = '6m'; P95 = 6500; ErrorRate = 0.20 },
            [pscustomobject]@{ Name = '1m'; TargetUsers = 1000000; TargetRps = 2400; RampUp = '10m'; Hold = '30m'; RampDown = '8m'; P95 = 8000; ErrorRate = 0.25 }
        )
    }

    return $basePlan
}

function Get-BottleneckClass {
    param(
        [double]$P95,
        [double]$ErrorRate,
        [double]$ThresholdP95,
        [double]$ThresholdErrorRate,
        [double]$Rate429,
        [double]$Rate5xx,
        [double]$Rate4xx,
        [double]$PreviousP95
    )

    if ($Rate429 -ge 0.01) {
        return 'infra/rate-limit'
    }

    if ($Rate5xx -ge 0.01) {
        return 'api/dependency'
    }

    if ($ErrorRate -gt $ThresholdErrorRate -and $Rate4xx -ge 0.25) {
        return 'contract/authz'
    }

    if ($P95 -gt $ThresholdP95 -and $ErrorRate -le $ThresholdErrorRate) {
        if ($PreviousP95 -gt 0 -and $P95 -gt ($PreviousP95 * 1.15)) {
            return 'database/dependency'
        }

        return 'api'
    }

    if ($P95 -gt ($ThresholdP95 * 1.25) -and $ErrorRate -gt $ThresholdErrorRate) {
        return 'infra'
    }

    return 'none'
}

function Invoke-Phase10Gate {
    param(
        [Parameter(Mandatory)]
        [pscustomobject]$Gate,
        [string]$SuiteBaseUrl,
        [string]$Username,
        [string]$Password,
        [int]$GeneratorTotalValue,
        [int]$GeneratorIndexValue,
        [switch]$DistributedMode,
        [switch]$AllowRaw,
        [switch]$OutputJsonEnabled
    )

    $k6Exe = Resolve-K6Path
    $resultsDir = Join-Path $TestsDir 'results\phase10'
    if (-not (Test-Path $resultsDir)) {
        New-Item -ItemType Directory -Path $resultsDir | Out-Null
    }

    $runId = Get-Date -Format 'yyyyMMdd-HHmmss'
    $summaryJsonPath = Join-Path $resultsDir ("summary-$($Gate.Name)-$runId.json")
    $summaryTxtPath = Join-Path $resultsDir ("summary-$($Gate.Name)-$runId.txt")

    $k6Args = @('run', '--quiet', '--summary-export', $summaryJsonPath)
    if ($OutputJsonEnabled) {
        if (-not $AllowRaw) {
            throw 'Raw JSON output is disabled by default. Use -AllowRawOutput with -OutputJson.'
        }

        $rawJsonPath = Join-Path $resultsDir ("raw-$($Gate.Name)-$runId.json")
        $k6Args += @('--out', "json=$rawJsonPath")
    }

    $k6Args += @(
        '-e', "BASE_URL=$SuiteBaseUrl",
        '-e', "PHASE10_GATE=$($Gate.Name)",
        '-e', "PHASE10_TARGET_USERS=$($Gate.TargetUsers)",
        '-e', "PHASE10_TARGET_RPS=$($Gate.TargetRps)",
        '-e', "PHASE10_RAMP_UP=$($Gate.RampUp)",
        '-e', "PHASE10_HOLD=$($Gate.Hold)",
        '-e', "PHASE10_RAMP_DOWN=$($Gate.RampDown)",
        '-e', "PHASE10_P95_THRESHOLD_MS=$($Gate.P95)",
        '-e', "PHASE10_ERROR_RATE_THRESHOLD=$($Gate.ErrorRate)",
        '-e', "SUMMARY_TXT_PATH=$summaryTxtPath"
    )

    if ($Username) {
        $k6Args += @('-e', "TEST_USERNAME=$Username")
    }
    if ($Password) {
        $k6Args += @('-e', "TEST_PASSWORD=$Password")
    }

    if ($DistributedMode) {
        if ($GeneratorTotalValue -lt 2) {
            throw 'When -Distributed is enabled, -GeneratorTotal must be >= 2.'
        }

        if ($GeneratorIndexValue -lt 1 -or $GeneratorIndexValue -gt $GeneratorTotalValue) {
            throw '-GeneratorIndex must be between 1 and GeneratorTotal when -Distributed is enabled.'
        }

        $k6Args += @('-e', "GENERATOR_TOTAL=$GeneratorTotalValue")
        $k6Args += @('-e', "GENERATOR_INDEX=$GeneratorIndexValue")
    }

    $k6Args += $ScriptPath

    Write-Host "Gate        : $($Gate.Name)"
    Write-Host "TargetUsers : $($Gate.TargetUsers)"
    Write-Host "TargetRPS   : $($Gate.TargetRps)"
    Write-Host "P95 Target  : $($Gate.P95)ms"
    Write-Host "Error Target: $($Gate.ErrorRate)"
    Write-Host "Summary JSON: $summaryJsonPath"
    Write-Host "Summary TXT : $summaryTxtPath"
    if ($DistributedMode) {
        Write-Host "Distributed : enabled ($GeneratorIndexValue/$GeneratorTotalValue)"
    }
    Write-Host ""

    & $k6Exe @k6Args
    $exitCode = $LASTEXITCODE

    $summary = $null
    if (Test-Path $summaryJsonPath) {
        $summary = Get-Content $summaryJsonPath -Raw | ConvertFrom-Json
    }

    $p95 = if ($summary -and $summary.metrics.api_duration) { [double]$summary.metrics.api_duration.values.'p(95)' } else { 0 }
    $errorRate = if ($summary -and $summary.metrics.api_errors) { [double]$summary.metrics.api_errors.values.rate } else { 0 }
    $rate5xx = if ($summary -and $summary.metrics.api_5xx) { [double]$summary.metrics.api_5xx.values.rate } else { 0 }
    $rate429 = if ($summary -and $summary.metrics.api_429) { [double]$summary.metrics.api_429.values.rate } else { 0 }
    $rate4xx = if ($summary -and $summary.metrics.api_4xx) { [double]$summary.metrics.api_4xx.values.rate } else { 0 }

    return [pscustomobject]@{
        Gate = $Gate
        ExitCode = $exitCode
        SummaryJsonPath = $summaryJsonPath
        SummaryTxtPath = $summaryTxtPath
        P95 = $p95
        ErrorRate = $errorRate
        Rate5xx = $rate5xx
        Rate429 = $rate429
        Rate4xx = $rate4xx
    }
}

$planEntries = Get-Phase10GatePlan -PlanName $Plan
$results = @()
$previousP95 = 0

foreach ($gate in $planEntries) {
    $attempt = 1
    $finalResult = $null

    do {
        if ($attempt -gt 1) {
            Write-Host "Re-test attempt $attempt for gate $($gate.Name)"
        }

        $finalResult = Invoke-Phase10Gate -Gate $gate -SuiteBaseUrl $BaseUrl -Username $TestUsername -Password $TestPassword -GeneratorTotalValue $GeneratorTotal -GeneratorIndexValue $GeneratorIndex -DistributedMode:$Distributed -AllowRaw:$AllowRawOutput -OutputJsonEnabled:$OutputJson
        $bottleneck = Get-BottleneckClass -P95 $finalResult.P95 -ErrorRate $finalResult.ErrorRate -ThresholdP95 $gate.P95 -ThresholdErrorRate $gate.ErrorRate -Rate429 $finalResult.Rate429 -Rate5xx $finalResult.Rate5xx -Rate4xx $finalResult.Rate4xx -PreviousP95 $previousP95

        Write-Host "Gate result : exit=$($finalResult.ExitCode) p95=$([math]::Round($finalResult.P95, 2))ms errorRate=$([math]::Round($finalResult.ErrorRate * 100, 2))% bottleneck=$bottleneck"
        $results += [pscustomobject]@{
            Gate = $gate.Name
            ExitCode = $finalResult.ExitCode
            P95 = $finalResult.P95
            ErrorRate = $finalResult.ErrorRate
            Bottleneck = $bottleneck
            SummaryJsonPath = $finalResult.SummaryJsonPath
            SummaryTxtPath = $finalResult.SummaryTxtPath
        }

        if ($finalResult.ExitCode -eq 0) {
            $previousP95 = $finalResult.P95
            break
        }

        $attempt++
    } while ($attempt -le [Math]::Max(1, $RetestCount))

    if ($finalResult.ExitCode -ne 0) {
        break
    }
}

$passed = ($results | Where-Object { $_.ExitCode -eq 0 }).Count
$failed = ($results | Where-Object { $_.ExitCode -ne 0 }).Count

Write-Host ""
Write-Host "Phase 10 summary"
Write-Host "Passed gates : $passed"
Write-Host "Failed gates : $failed"

foreach ($result in $results) {
    Write-Host ("{0,-8} p95={1,8:N2}ms errorRate={2,7:N2}% bottleneck={3}" -f $result.Gate, $result.P95, ($result.ErrorRate * 100), $result.Bottleneck)
}

if ($failed -gt 0) {
    exit 1
}
