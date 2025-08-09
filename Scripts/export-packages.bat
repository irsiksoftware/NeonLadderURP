@echo off
REM Cross-platform Unity package export script for Windows
REM Companion to the shell script for Unix systems

setlocal enabledelayedexpansion

REM Get the script directory
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..

echo Unity Package Export Tool
echo ================================

REM Check Python installation
where python >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    where python3 >nul 2>nul
    if %ERRORLEVEL% NEQ 0 (
        echo Error: Python is not installed or not in PATH
        echo Please install Python 3.6+ to continue
        exit /b 1
    ) else (
        set PYTHON_CMD=python3
    )
) else (
    set PYTHON_CMD=python
)

REM Verify Python version
for /f "tokens=*" %%i in ('%PYTHON_CMD% -c "import sys; print('.'.join(map(str, sys.version_info[:2])))"') do set PYTHON_VERSION=%%i
echo Using Python %PYTHON_VERSION%

REM Check if Unity is running
tasklist /FI "IMAGENAME eq Unity.exe" 2>NUL | find /I /N "Unity.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo Warning: Unity appears to be running
    echo Please close Unity before exporting packages
    echo Press any key to continue anyway, or Ctrl+C to cancel...
    pause >nul
)

REM Change to project root
cd /d "%PROJECT_ROOT%"

REM Parse command line arguments
set ARGS=
set DRY_RUN=
set SKIP_UPLOAD=

:parse_args
if "%~1"=="" goto :run_script

if "%~1"=="--dry-run" (
    set DRY_RUN=--dry-run
    echo Running in dry-run mode ^(no actual exports^)
    shift
    goto :parse_args
)

if "%~1"=="--skip-upload" (
    set SKIP_UPLOAD=--skip-upload
    echo Skipping Google Drive upload
    shift
    goto :parse_args
)

if "%~1"=="--packages" (
    set ARGS=%ARGS% --packages
    shift
    :parse_packages
    if "%~1"=="" goto :run_script
    if "%~1:~0,2%"=="--" goto :parse_args
    set ARGS=%ARGS% %~1
    shift
    goto :parse_packages
)

set ARGS=%ARGS% %~1
shift
goto :parse_args

:run_script
REM Execute the Python script
echo Starting package export...
%PYTHON_CMD% "%SCRIPT_DIR%export_unity_packages.py" ^
    --project-path "%PROJECT_ROOT%" ^
    %DRY_RUN% ^
    %SKIP_UPLOAD% ^
    %ARGS%

set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE%==0 (
    echo Export completed successfully!
) else (
    echo Export failed with code %EXIT_CODE%
)

exit /b %EXIT_CODE%