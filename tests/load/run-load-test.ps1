#!/usr/bin/env pwsh

param(
    [Parameter(Position = 0)]
    [ValidateSet('auth', 'core')]
    [string]$Suite = 'auth',

    [Parameter(Position = 1)]
    [ValidateSet('smoke', 'load', 'stress', 'max', 'max-50k', 'max-100k', 'max-1m')]
    [string]$Profile = 'smoke',

    [Parameter(Position = 2)]
    [ValidateSet('local', 'staging', 'production')]
    [string]$Environment = 'local',

    [string]$BaseUrl,
    [string]$TestUsername,
    [string]$TestPassword,
    [string]$TestUsersJson,
    [switch]$OutputJson,
    [switch]$AllowRawOutput,
    [switch]$Distributed,
    [int]$GeneratorTotal = 1,
    [int]$GeneratorIndex = 1,
    [switch]$NoQuiet
)

$ProjectRoot = 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere'
$TestsDir = Join-Path $ProjectRoot 'tests\load'

$Suites = @{
    auth = 'k6-auth-current.js'
    core = 'k6-core-current.js'
}

$EnvironmentUrls = @{
    local = 'http://localhost:5181'
    staging = 'https://staging-api.example.com'
    production = 'https://api.example.com'
}

function Resolve-K6Path {
    $cmd = Get-Command k6 -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $fallback = 'C:\Program Files\k6\k6.exe'
    if (Test-Path $fallback) { return $fallback }

    throw 'k6 not found. Install k6 or add it to PATH.'
}

$targetUrl = if ($BaseUrl) { $BaseUrl } else { $EnvironmentUrls[$Environment] }
$scriptName = $Suites[$Suite]
$scriptPath = Join-Path $TestsDir $scriptName

if (-not (Test-Path $scriptPath)) {
    throw "Script file not found: $scriptPath"
}

$k6Exe = Resolve-K6Path
$k6Args = @('run')

$resultsDir = Join-Path $TestsDir 'results'
if (-not (Test-Path $resultsDir)) {
    New-Item -ItemType Directory -Path $resultsDir | Out-Null
}

$runId = Get-Date -Format 'yyyyMMdd-HHmmss'
$summaryJsonPath = Join-Path $resultsDir ("summary-$Suite-$Profile-$runId.json")
$summaryTxtPath = Join-Path $resultsDir ("summary-$Suite-$Profile-$runId.txt")

if (-not $NoQuiet) {
    # Final-Touches Phase 34 Stage 5.3 — keep day-to-day output concise and summary-first.
    $k6Args += '--quiet'
}

$k6Args += @('--summary-export', $summaryJsonPath)

if ($OutputJson) {
    if (-not $AllowRawOutput) {
        throw 'Raw JSON output is disabled by default. Use -AllowRawOutput with -OutputJson for diagnostics runs.'
    }

    $jsonPath = Join-Path $resultsDir ("raw-$Suite-$Profile-$runId.json")
    $k6Args += @('--out', "json=$jsonPath")
}

$k6Args += @('-e', "BASE_URL=$targetUrl")
$k6Args += @('-e', "TEST_PROFILE=$Profile")
$k6Args += @('-e', "SUMMARY_TXT_PATH=$summaryTxtPath")

if ($Profile -in @('max', 'max-50k', 'max-100k', 'max-1m')) {
    $maxUsers = switch ($Profile) {
        'max-50k' { 50000 }
        'max-100k' { 100000 }
        'max-1m' { 1000000 }
        default { 10000 }
    }

    $k6Args += @('-e', "MAX_USERS=$maxUsers")
    $k6Args += @('-e', 'MAX_RAMP_1=3m')
    $k6Args += @('-e', 'MAX_RAMP_2=8m')
    $k6Args += @('-e', 'MAX_HOLD=15m')
    $k6Args += @('-e', 'MAX_RAMP_DOWN=3m')
}

if ($TestUsername) {
    $k6Args += @('-e', "TEST_USERNAME=$TestUsername")
}
if ($TestPassword) {
    $k6Args += @('-e', "TEST_PASSWORD=$TestPassword")
}
if ($TestUsersJson) {
    $k6Args += @('-e', "TEST_USERS_JSON=$TestUsersJson")
}

if ($Distributed) {
    # Final-Touches Phase 34 Stage 5.2 — pass shard metadata so multiple generators can split load safely.
    if ($GeneratorTotal -lt 2) {
        throw 'When -Distributed is enabled, -GeneratorTotal must be >= 2.'
    }

    if ($GeneratorIndex -lt 1 -or $GeneratorIndex -gt $GeneratorTotal) {
        throw '-GeneratorIndex must be between 1 and GeneratorTotal when -Distributed is enabled.'
    }

    $k6Args += @('-e', "GENERATOR_TOTAL=$GeneratorTotal")
    $k6Args += @('-e', "GENERATOR_INDEX=$GeneratorIndex")
}

$k6Args += $scriptPath

Write-Host "Suite       : $Suite"
Write-Host "Profile     : $Profile"
Write-Host "Environment : $Environment"
Write-Host "Base URL    : $targetUrl"
Write-Host "Script      : $scriptName"
Write-Host "Summary JSON: $summaryJsonPath"
Write-Host "Summary TXT : $summaryTxtPath"
if ($Distributed) {
    Write-Host "Distributed : enabled ($GeneratorIndex/$GeneratorTotal)"
}
if ($OutputJson) {
    Write-Host "Raw Output  : $jsonPath"
}
Write-Host ""

& $k6Exe @k6Args
exit $LASTEXITCODE
