@echo off
REM Unity package download script for Windows
REM Downloads packages from Google Drive links in DownloadInstructions.txt

setlocal enabledelayedexpansion

REM Get the script directory
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..

echo Unity Package Download Tool
echo ================================
echo Using 5Gbps fiber connection for fast downloads!

REM Check Python installation
where python >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    where python3 >nul 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo Error: Python is not installed or not in PATH
        exit /b 1
    ) else (
        set PYTHON_CMD=python3
    )
) else (
    set PYTHON_CMD=python
)

REM Create download directory
set DOWNLOAD_DIR=%PROJECT_ROOT%\PackageDownloads
if not exist "%DOWNLOAD_DIR%" mkdir "%DOWNLOAD_DIR%"

REM Parse command line arguments
set PACKAGES=
set VERIFY_ONLY=

:parse_args
if "%~1"=="" goto :run_script

if "%~1"=="--verify-only" (
    set VERIFY_ONLY=--verify-only
    echo Verify-only mode: checking existing downloads
    shift
    goto :parse_args
)

if "%~1"=="--packages" (
    set PACKAGES=--packages
    shift
    :parse_packages
    if "%~1"=="" goto :run_script
    if "%~1:~0,2%"=="--" goto :parse_args
    set PACKAGES=%PACKAGES% %~1
    shift
    goto :parse_packages
)

if "%~1"=="--help" (
    echo Usage: %~nx0 [options]
    echo Options:
    echo   --packages NAME1 NAME2  Download specific packages
    echo   --verify-only          Verify existing downloads
    echo   --help                 Show this help message
    exit /b 0
)

shift
goto :parse_args

:run_script
REM Execute the Python script
echo Starting package download...
echo Download directory: %DOWNLOAD_DIR%
echo.

%PYTHON_CMD% "%SCRIPT_DIR%download_unity_packages.py" ^
    --project-path "%PROJECT_ROOT%" ^
    %VERIFY_ONLY% ^
    %PACKAGES%

set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE%==0 (
    echo Download completed successfully!
    echo Check PackageDownloads\ directory for downloaded packages
) else (
    echo Download failed with code %EXIT_CODE%
)

exit /b %EXIT_CODE%