@echo off
setlocal

set SUITE=%1
if "%SUITE%"=="" set SUITE=auth

set PROFILE=%2
if "%PROFILE%"=="" set PROFILE=smoke

set ENVIRONMENT=%3
if "%ENVIRONMENT%"=="" set ENVIRONMENT=local

powershell -ExecutionPolicy Bypass -File "%~dp0run-load-test.ps1" -Suite %SUITE% -Profile %PROFILE% -Environment %ENVIRONMENT% %4 %5 %6 %7 %8 %9
exit /b %errorlevel%
