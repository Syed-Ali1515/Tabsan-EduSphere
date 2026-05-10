#!/usr/bin/env pwsh

param(
    [Parameter(Position = 0)]
    [ValidateSet('auth', 'core')]
    [string]$Suite = 'auth',

    [Parameter(Position = 1)]
    [ValidateSet('smoke', 'load', 'stress', 'max')]
    [string]$Profile = 'smoke',

    [Parameter(Position = 2)]
    [ValidateSet('local', 'staging', 'production')]
    [string]$Environment = 'local',

    [string]$BaseUrl,
    [string]$TestUsername,
    [string]$TestPassword,
    [string]$TestUsersJson,
    [switch]$OutputJson
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

$k6Args += @('--summary-export', $summaryJsonPath)

if ($OutputJson) {
    $jsonPath = Join-Path $resultsDir ("raw-$Suite-$Profile-$runId.json")
    $k6Args += @('--out', "json=$jsonPath")
}

$k6Args += @('-e', "BASE_URL=$targetUrl")
$k6Args += @('-e', "TEST_PROFILE=$Profile")
$k6Args += @('-e', "SUMMARY_TXT_PATH=$summaryTxtPath")

if ($Profile -eq 'max') {
    $k6Args += @('-e', 'MAX_USERS=10000')
    $k6Args += @('-e', 'MAX_RAMP_1=2m')
    $k6Args += @('-e', 'MAX_RAMP_2=4m')
    $k6Args += @('-e', 'MAX_HOLD=10m')
    $k6Args += @('-e', 'MAX_RAMP_DOWN=2m')
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

$k6Args += $scriptPath

Write-Host "Suite       : $Suite"
Write-Host "Profile     : $Profile"
Write-Host "Environment : $Environment"
Write-Host "Base URL    : $targetUrl"
Write-Host "Script      : $scriptName"
Write-Host "Summary JSON: $summaryJsonPath"
Write-Host "Summary TXT : $summaryTxtPath"
if ($OutputJson) {
    Write-Host "Raw Output  : $jsonPath"
}
Write-Host ""

& $k6Exe @k6Args
exit $LASTEXITCODE
