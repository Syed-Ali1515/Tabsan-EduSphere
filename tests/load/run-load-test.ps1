#!/usr/bin/env pwsh

<#
.SYNOPSIS
    K6 Load Testing Script for Tabsan-EduSphere Login Endpoint

.DESCRIPTION
    Comprehensive load testing runner with multiple scenarios from light to extreme loads.
    Supports local, staging, and production environments.

.PARAMETER Scenario
    Test scenario: light, medium, high, extreme, spike, soak, stress, custom
    Default: light

.PARAMETER Environment
    Target environment: local, staging, production
    Default: local

.PARAMETER OutputJson
    Export results to JSON file for analysis
    Default: $false

.PARAMETER CloudRun
    Run test on K6 Cloud instead of locally
    Default: $false

.EXAMPLE
    .\run-load-test.ps1 -Scenario light -Environment local
    .\run-load-test.ps1 -Scenario high -Environment staging -OutputJson
    .\run-load-test.ps1 -Scenario extreme -Environment production

.NOTES
    Author: DevOps Team
    Version: 1.0
    Date: May 2026
#>

param(
    [Parameter(Position = 0)]
    [ValidateSet('light', 'medium', 'high', 'extreme', 'spike', 'soak', 'stress', 'custom')]
    [string]$Scenario = 'light',

    [Parameter(Position = 1)]
    [ValidateSet('local', 'staging', 'production')]
    [string]$Environment = 'local',

    [switch]$OutputJson,

    [switch]$CloudRun,

    [string]$CustomArgs
)

# ============================================================================
# CONFIGURATION
# ============================================================================

$ProjectRoot = 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere'
$TestsDir = Join-Path $ProjectRoot 'tests\load'
$ScriptFile = Join-Path $TestsDir 'login-load-test.js'

$EnvironmentUrls = @{
    'local'       = 'http://localhost:5000'
    'staging'     = 'https://staging-api.example.com'
    'production'  = 'https://api.example.com'
}

# ============================================================================
# HELPER FUNCTIONS
# ============================================================================

function Write-Header {
    param([string]$Message)
    Write-Host "`n" -ForegroundColor Black
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  $Message" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

# ============================================================================
# VALIDATION
# ============================================================================

Write-Header "K6 Load Testing - Tabsan-EduSphere"

# Check if k6 is installed
$k6Version = k6 version 2>$null
if (-not $k6Version) {
    Write-Error "K6 is not installed or not in PATH"
    Write-Info "Install from: https://k6.io/docs/getting-started/installation/"
    exit 1
}

Write-Success "K6 version: $k6Version"

# Check if script file exists
if (-not (Test-Path $ScriptFile)) {
    Write-Error "Script file not found: $ScriptFile"
    exit 1
}

Write-Success "Script file found: $ScriptFile"

# Get target URL
$TargetUrl = $EnvironmentUrls[$Environment]
Write-Info "Target URL: $TargetUrl"

# Production confirmation
if ($Environment -eq 'production') {
    Write-Warning "Running against PRODUCTION environment!"
    $confirmation = Read-Host "Type 'yes' to continue"
    if ($confirmation -ne 'yes') {
        Write-Info "Test cancelled"
        exit 0
    }
}

# ============================================================================
# BUILD K6 COMMAND
# ============================================================================

$k6Args = @()

# Add output option
if ($OutputJson) {
    $jsonFile = Join-Path $TestsDir "results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
    $k6Args += "--out", "json=$jsonFile"
    Write-Info "Results will be saved to: $jsonFile"
}

# Add target URL
$env:TARGET_URL = $TargetUrl

# Add scenario-specific stages
switch ($Scenario) {
    'light' {
        Write-Info "Running LIGHT LOAD test (10 VUs, 4 minutes)"
        $k6Args += @('--stage', '1m:10', '--stage', '2m:10', '--stage', '1m:0')
    }
    
    'medium' {
        Write-Info "Running MEDIUM LOAD test (1000 VUs, 14 minutes)"
        $k6Args += @(
            '--stage', '2m:100',
            '--stage', '5m:1000',
            '--stage', '5m:1000',
            '--stage', '2m:0'
        )
    }
    
    'high' {
        Write-Info "Running HIGH LOAD test (10000 VUs, 27 minutes)"
        $k6Args += @(
            '--stage', '2m:100',
            '--stage', '5m:1000',
            '--stage', '5m:10000',
            '--stage', '10m:10000',
            '--stage', '2m:0'
        )
    }
    
    'extreme' {
        Write-Warning "Running EXTREME LOAD test (1M VUs, 60+ minutes)"
        Write-Info "This may take a long time. Press Ctrl+C to stop."
        Start-Sleep -Seconds 3
        $k6Args += @(
            '--stage', '2m:100',
            '--stage', '5m:1000',
            '--stage', '5m:10000',
            '--stage', '10m:100000',
            '--stage', '10m:1000000',
            '--stage', '15m:1000000',
            '--stage', '5m:0'
        )
    }
    
    'spike' {
        Write-Info "Running SPIKE TEST (sudden 100k load)"
        $k6Args += @(
            '--stage', '2m:100',
            '--stage', '1m:100000',
            '--stage', '2m:100000',
            '--stage', '1m:100'
        )
    }
    
    'soak' {
        Write-Warning "Running SOAK TEST (steady load for 2 hours)"
        Write-Info "Press Ctrl+C to stop at any time"
        Start-Sleep -Seconds 2
        $k6Args += @(
            '--stage', '5m:1000',
            '--stage', '120m:1000',
            '--stage', '5m:0'
        )
    }
    
    'stress' {
        Write-Info "Running STRESS TEST (find breaking point)"
        $k6Args += @(
            '--stage', '2m:1000',
            '--stage', '2m:5000',
            '--stage', '2m:10000',
            '--stage', '2m:50000',
            '--stage', '2m:100000',
            '--stage', '2m:500000',
            '--stage', '5m:0'
        )
    }
    
    'custom' {
        if ([string]::IsNullOrWhiteSpace($CustomArgs)) {
            Write-Error "Custom scenario requires -CustomArgs parameter"
            Write-Info "Example: -CustomArgs '--stage 2m:1000 --stage 5m:1000'"
            exit 1
        }
        Write-Info "Running CUSTOM test with args: $CustomArgs"
        $k6Args += $CustomArgs -split ' '
    }
}

# ============================================================================
# EXECUTE TEST
# ============================================================================

Write-Host "`n"

try {
    if ($CloudRun) {
        Write-Header "Running on K6 Cloud"
        Write-Info "Authenticate with: k6 login cloud"
        & k6 cloud run @k6Args $ScriptFile
    }
    else {
        & k6 run @k6Args $ScriptFile
    }
    
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Success "Test completed successfully"
    }
    else {
        Write-Warning "Test completed with exit code: $exitCode"
    }
}
catch {
    Write-Error "Error running test: $_"
    exit 1
}

# ============================================================================
# POST-TEST SUMMARY
# ============================================================================

Write-Header "Test Completed"

Write-Info "📊 Results Summary:"
Write-Host "   • Check response times above (P95, P99)"
Write-Host "   • Review error rate (should be < 1%)"
Write-Host "   • Verify no 500 errors occurred"
Write-Host ""

Write-Info "📁 Next Steps:"
Write-Host "   1. Review the output above for performance metrics"
Write-Host "   2. Compare against baseline targets:"
Write-Host "      - P95 response time: < 500ms"
Write-Host "      - Error rate: < 0.1%"
Write-Host "      - RPS sustained: > 1000"
Write-Host "   3. For detailed analysis, run with -OutputJson flag"
Write-Host "   4. Implement optimizations from LOAD_TESTING_GUIDE.md"
Write-Host ""

if ($OutputJson) {
    Write-Success "JSON results saved for analysis"
}

Write-Info "📖 Full documentation: $TestsDir\LOAD_TESTING_GUIDE.md"
Write-Host "`n"
