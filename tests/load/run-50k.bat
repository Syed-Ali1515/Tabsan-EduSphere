@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "RESULTS_DIR=%SCRIPT_DIR%results"
if not exist "%RESULTS_DIR%" mkdir "%RESULTS_DIR%"

set "TS=%DATE:~-4%%DATE:~4,2%%DATE:~7,2%-%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%"
set "TS=%TS: =0%"

set "SUMMARY_JSON=%RESULTS_DIR%\summary-50k-%TS%.json"
set "SUMMARY_TXT=%RESULTS_DIR%\summary-50k-%TS%.txt"
set "RAW_JSON=%RESULTS_DIR%\raw-50k-%TS%.json"

set "BASE_URL=http://localhost:5181"
if not "%~1"=="" set "BASE_URL=%~1"
set "RAW_MODE=%~2"
set "LOCAL_VU_CAP=16000"
if not "%~3"=="" set "LOCAL_VU_CAP=%~3"
set "GENERATOR_TOTAL=1"
if not "%~4"=="" set "GENERATOR_TOTAL=%~4"
set "GENERATOR_INDEX=1"
if not "%~5"=="" set "GENERATOR_INDEX=%~5"
set "TARGET_RPS=120"
if not "%~6"=="" set "TARGET_RPS=%~6"

set "K6_EXE=C:\Program Files\k6\k6.exe"
if exist "%K6_EXE%" goto run
set "K6_EXE=k6"

:run
REM Final-Touches Phase 34 Stage 5.2/5.3 — distributed shard controls + quiet summary-first output discipline.
echo Running 50K test against %BASE_URL% (generator %GENERATOR_INDEX%/%GENERATOR_TOTAL%, target RPS %TARGET_RPS%)
if /I "%RAW_MODE%"=="raw" (
	"%K6_EXE%" run --quiet --summary-export "%SUMMARY_JSON%" --out "json=%RAW_JSON%" -e BASE_URL=%BASE_URL% -e LOCAL_VU_CAP=%LOCAL_VU_CAP% -e GENERATOR_TOTAL=%GENERATOR_TOTAL% -e GENERATOR_INDEX=%GENERATOR_INDEX% -e TARGET_RPS=%TARGET_RPS% -e SUMMARY_TXT_PATH="%SUMMARY_TXT%" "%SCRIPT_DIR%k6-scale-50k.js"
) else (
	"%K6_EXE%" run --quiet --summary-export "%SUMMARY_JSON%" -e BASE_URL=%BASE_URL% -e LOCAL_VU_CAP=%LOCAL_VU_CAP% -e GENERATOR_TOTAL=%GENERATOR_TOTAL% -e GENERATOR_INDEX=%GENERATOR_INDEX% -e TARGET_RPS=%TARGET_RPS% -e SUMMARY_TXT_PATH="%SUMMARY_TXT%" "%SCRIPT_DIR%k6-scale-50k.js"
)

exit /b %errorlevel%
