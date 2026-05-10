@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "RESULTS_DIR=%SCRIPT_DIR%results"
if not exist "%RESULTS_DIR%" mkdir "%RESULTS_DIR%"

set "TS=%DATE:~-4%%DATE:~4,2%%DATE:~7,2%-%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%"
set "TS=%TS: =0%"

set "SUMMARY_JSON=%RESULTS_DIR%\summary-1m-%TS%.json"
set "SUMMARY_TXT=%RESULTS_DIR%\summary-1m-%TS%.txt"
set "RAW_JSON=%RESULTS_DIR%\raw-1m-%TS%.json"

set "BASE_URL=http://localhost:5181"
if not "%~1"=="" set "BASE_URL=%~1"

set "K6_EXE=C:\Program Files\k6\k6.exe"
if exist "%K6_EXE%" goto run
set "K6_EXE=k6"

:run
echo Running 1M test against %BASE_URL%
"%K6_EXE%" run --summary-export "%SUMMARY_JSON%" --out "json=%RAW_JSON%" -e BASE_URL=%BASE_URL% -e SUMMARY_TXT_PATH="%SUMMARY_TXT%" "%SCRIPT_DIR%k6-scale-1m.js"

exit /b %errorlevel%
