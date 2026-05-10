#!/usr/bin/env pwsh

param(
    [Parameter(Position = 0)]
    [ValidateSet('auth', 'core')]
    [string]$Suite = 'auth',

    [Parameter(Position = 1)]
    [ValidateSet('smoke', 'load', 'stress')]
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

function Get-ProfileStages {
    param([string]$SelectedProfile)

    switch ($SelectedProfile) {
        'smoke' {
            return @('--stage', '30s:5', '--stage', '30s:10', '--stage', '20s:0')
        }
        'load' {
            return @('--stage', '1m:20', '--stage', '3m:80', '--stage', '3m:120', '--stage', '1m:0')
        }
        'stress' {
            return @('--stage', '1m:50', '--stage', '4m:200', '--stage', '4m:400', '--stage', '1m:0')
        }
        default {
            return @()
        }
    }
}

$targetUrl = if ($BaseUrl) { $BaseUrl } else { $EnvironmentUrls[$Environment] }
$scriptName = $Suites[$Suite]
$scriptPath = Join-Path $TestsDir $scriptName

if (-not (Test-Path $scriptPath)) {
    throw "Script file not found: $scriptPath"
}

$k6Exe = Resolve-K6Path
$k6Args = @('run')

if ($OutputJson) {
    $jsonPath = Join-Path $TestsDir ("results-$Suite-$Profile-" + (Get-Date -Format 'yyyyMMdd-HHmmss') + '.json')
    $k6Args += @('--out', "json=$jsonPath")
}

$k6Args += @('-e', "BASE_URL=$targetUrl")

if ($TestUsername) {
    $k6Args += @('-e', "TEST_USERNAME=$TestUsername")
}
if ($TestPassword) {
    $k6Args += @('-e', "TEST_PASSWORD=$TestPassword")
}
if ($TestUsersJson) {
    $k6Args += @('-e', "TEST_USERS_JSON=$TestUsersJson")
}

$k6Args += Get-ProfileStages -SelectedProfile $Profile
$k6Args += $scriptPath

Write-Host "Suite       : $Suite"
Write-Host "Profile     : $Profile"
Write-Host "Environment : $Environment"
Write-Host "Base URL    : $targetUrl"
Write-Host "Script      : $scriptName"
Write-Host ""

& $k6Exe @k6Args
exit $LASTEXITCODE
