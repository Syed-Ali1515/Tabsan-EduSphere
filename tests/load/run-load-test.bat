@echo off
REM ============================================================================
REM K6 Load Testing Batch Script for Tabsan-EduSphere
REM 
REM Quick-start commands for common load testing scenarios
REM Usage: run-load-test.bat [scenario] [environment]
REM 
REM Examples:
REM   run-load-test.bat light local
REM   run-load-test.bat medium staging
REM   run-load-test.bat extreme production
REM ============================================================================

setlocal enabledelayedexpansion

REM Define colors for output
set /A GREEN=10
set /A YELLOW=14
set /A RED=12
set /A BLUE=9

REM ============================================================================
REM CONFIGURATION
REM ============================================================================

set PROJECT_ROOT=c:\Users\alin\Desktop\Prj\Tabsan-EduSphere
set TESTS_DIR=%PROJECT_ROOT%\tests\load
set SCRIPT_FILE=%TESTS_DIR%\login-load-test.js

REM Environment URL mapping
set LOCAL_URL=http://localhost:5000
set STAGING_URL=https://staging-api.example.com
set PRODUCTION_URL=https://api.example.com

REM ============================================================================
REM ARGUMENT PARSING
REM ============================================================================

set SCENARIO=%1
set ENVIRONMENT=%2

if "%SCENARIO%"=="" (
    set SCENARIO=light
)

if "%ENVIRONMENT%"=="" (
    set ENVIRONMENT=local
)

echo.
echo ============================================
echo    K6 LOAD TESTING SCRIPT
echo ============================================
echo Scenario: %SCENARIO%
echo Environment: %ENVIRONMENT%
echo.

REM ============================================================================
REM ENVIRONMENT URL SELECTION
REM ============================================================================

if "%ENVIRONMENT%"=="local" (
    set TARGET_URL=!LOCAL_URL!
    echo Target URL: %TARGET_URL%
) else if "%ENVIRONMENT%"=="staging" (
    set TARGET_URL=!STAGING_URL!
    echo Target URL: %TARGET_URL%
) else if "%ENVIRONMENT%"=="production" (
    set TARGET_URL=!PRODUCTION_URL!
    echo Target URL: %TARGET_URL%
    echo.
    echo WARNING: Running against PRODUCTION environment!
    echo Press Ctrl+C to cancel, or any key to continue...
    pause >nul
) else (
    echo ERROR: Unknown environment '%ENVIRONMENT%'
    echo Valid options: local, staging, production
    goto :EOF
)

REM ============================================================================
REM SCENARIO EXECUTION
REM ============================================================================

if "%SCENARIO%"=="light" (
    echo Running LIGHT LOAD test (10 VUs, 4 minutes)...
    k6 run --stage "1m:10" --stage "2m:10" --stage "1m:0" %SCRIPT_FILE%

) else if "%SCENARIO%"=="medium" (
    echo Running MEDIUM LOAD test (1000 VUs, 14 minutes)...
    k6 run ^
        --stage "2m:100" ^
        --stage "5m:1000" ^
        --stage "5m:1000" ^
        --stage "2m:0" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="high" (
    echo Running HIGH LOAD test (10000 VUs, 27 minutes)...
    k6 run ^
        --stage "2m:100" ^
        --stage "5m:1000" ^
        --stage "5m:10000" ^
        --stage "10m:10000" ^
        --stage "2m:0" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="extreme" (
    echo Running EXTREME LOAD test (1M VUs, 60+ minutes)...
    echo This may take a long time. Please wait...
    k6 run ^
        --stage "2m:100" ^
        --stage "5m:1000" ^
        --stage "5m:10000" ^
        --stage "10m:100000" ^
        --stage "10m:1000000" ^
        --stage "15m:1000000" ^
        --stage "5m:0" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="spike" (
    echo Running SPIKE TEST (sudden 100k load)...
    k6 run ^
        --stage "2m:100" ^
        --stage "1m:100000" ^
        --stage "2m:100000" ^
        --stage "1m:100" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="soak" (
    echo Running SOAK TEST (steady load for 2 hours)...
    echo This is a long test. Press Ctrl+C to stop.
    k6 run ^
        --stage "5m:1000" ^
        --stage "120m:1000" ^
        --stage "5m:0" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="stress" (
    echo Running STRESS TEST (find breaking point)...
    k6 run ^
        --stage "2m:1000" ^
        --stage "2m:5000" ^
        --stage "2m:10000" ^
        --stage "2m:50000" ^
        --stage "2m:100000" ^
        --stage "2m:500000" ^
        --stage "5m:0" ^
        %SCRIPT_FILE%

) else if "%SCENARIO%"=="custom" (
    echo.
    echo Running CUSTOM test. Enter parameters:
    echo (Example: --stage "2m:1000" --stage "5m:1000" --stage "1m:0")
    set /p CUSTOM_ARGS="Enter custom arguments: "
    k6 run %CUSTOM_ARGS% %SCRIPT_FILE%

) else (
    echo ERROR: Unknown scenario '%SCENARIO%'
    echo Valid options: light, medium, high, extreme, spike, soak, stress, custom
    goto :EOF
)

REM ============================================================================
REM POST-TEST INSTRUCTIONS
REM ============================================================================

echo.
echo ============================================
echo    TEST COMPLETED
echo ============================================
echo.
echo Next Steps:
echo 1. Review the results above
echo 2. Check for error rate (should be < 1%%)
echo 3. Verify P95 response time (should be < 500ms)
echo 4. If tests failed, check:
echo    - Backend API is running
echo    - Database is accessible
echo    - Network connectivity to target
echo 5. For detailed analysis, run with JSON output:
echo    k6 run --out json=results.json %SCRIPT_FILE%
echo.
echo View complete documentation in:
echo %TESTS_DIR%\LOAD_TESTING_GUIDE.md
echo.

goto :EOF

REM ============================================================================
REM HELP FUNCTION
REM ============================================================================

:PRINT_HELP
echo Usage: run-load-test.bat [scenario] [environment]
echo.
echo Scenarios:
echo   light      - Light load (10 VUs, quick test)
echo   medium     - Medium load (1000 VUs)
echo   high       - High load (10000 VUs)
echo   extreme    - Extreme load (1M VUs)
echo   spike      - Spike test (sudden surge)
echo   soak       - Soak test (2 hour steady)
echo   stress     - Stress test (find limits)
echo   custom     - Custom parameters
echo.
echo Environments:
echo   local      - http://localhost:5000
echo   staging    - https://staging-api.example.com
echo   production - https://api.example.com (requires confirmation)
echo.
echo Examples:
echo   run-load-test.bat light local
echo   run-load-test.bat high staging
echo   run-load-test.bat extreme production
echo.
goto :EOF
