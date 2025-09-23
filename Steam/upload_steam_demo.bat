@echo off
setlocal enabledelayedexpansion

REM ============================================
REM Steam Demo Upload Script for NeonLadder
REM ============================================
REM This script uploads the Steam Demo build to Steam using steamcmd
REM App ID: 3089980 (Demo)
REM Depot ID: 3089881 (Demo Depot - Windows 64-bit)
REM ============================================

REM Make sure we're in the project root, not the Steam subfolder
cd /d "C:\Users\Ender\NeonLadder"

echo.
echo ========================================
echo NeonLadder Steam Demo Upload Script
echo ========================================
echo Current directory: %CD%
echo.

REM Check if we have a steam-demo build and get the latest one
set "BUILD_FOLDER="
set "LATEST_DATE="

for /f "tokens=*" %%i in ('dir "Builds\Windows\steam-demo-*" /b /ad /od 2^>nul') do (
    set "BUILD_FOLDER=%%i"
)

if "%BUILD_FOLDER%"=="" (
    echo ERROR: No steam-demo build found in Builds\Windows\
    echo Current directory: %CD%
    echo Available builds:
    dir "Builds\Windows\" /b
    echo Please build the Steam Demo profile first!
    pause
    exit /b 1
)

echo Found latest build folder: %BUILD_FOLDER%
echo.

REM Update the VDF file to use the specific build folder
echo Updating VDF file with specific build folder: %BUILD_FOLDER%
powershell -Command "(Get-Content 'Steam\app_build_3089980_demo.vdf') -replace 'steam-demo-\*', '%BUILD_FOLDER%' | Set-Content 'Steam\app_build_3089980_demo_temp.vdf'"
set VDF_PATH=C:\Users\Ender\NeonLadder\Steam\app_build_3089980_demo_temp.vdf

REM Set paths
set STEAMCMD_PATH=C:\Tools\steamworks\tools\ContentBuilder\builder\steamcmd.exe

REM Check if steamcmd exists
if not exist "%STEAMCMD_PATH%" (
    echo ERROR: steamcmd.exe not found at %STEAMCMD_PATH%
    echo Please ensure Steamworks SDK is installed
    pause
    exit /b 1
)

REM Check if VDF exists
if not exist "%VDF_PATH%" (
    echo ERROR: app_build_3089980_demo.vdf not found
    echo Expected at: %VDF_PATH%
    pause
    exit /b 1
)

REM Copy VDF files to ContentBuilder scripts folder for steamcmd
echo Copying VDF files to steamcmd scripts folder...
copy /Y "Steam\app_build_3089980_demo_temp.vdf" "C:\Tools\steamworks\tools\ContentBuilder\scripts\app_build_3089980_demo.vdf"
copy /Y "Steam\depot_build_3089881.vdf" "C:\Tools\steamworks\tools\ContentBuilder\scripts\"

REM Prompt for Steam credentials
echo.
echo Please enter your Steam credentials (with Steam Guard if enabled)
echo NOTE: This should be an account with upload permissions for App ID 3089980
echo.

set /p STEAM_USERNAME=Steam Username:
set /p STEAM_PASSWORD=Steam Password:

echo.
echo Starting upload to Steam...
echo This may take several minutes depending on build size and connection speed
echo.

REM Change to ContentBuilder directory (steamcmd expects to run from here)
cd /d "C:\Tools\steamworks\tools\ContentBuilder\builder"

REM Run steamcmd with the build script
"%STEAMCMD_PATH%" +login %STEAM_USERNAME% %STEAM_PASSWORD% +run_app_build ..\scripts\app_build_3089980_demo.vdf +quit

REM Check if upload succeeded
if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo Upload completed successfully!
    echo ========================================
    echo.
    echo Next steps:
    echo 1. Go to https://partner.steamgames.com/apps/builds/3089980
    echo 2. Find your build in the list
    echo 3. Set it live on the 'default' branch
    echo 4. Ensure depot 3089881 is referenced in your package
    echo 5. Test the demo in Steam client
    echo.
) else (
    echo.
    echo ========================================
    echo ERROR: Upload failed with error code %ERRORLEVEL%
    echo ========================================
    echo Common issues:
    echo - Invalid credentials
    echo - No upload permissions for this app
    echo - Steam Guard code needed
    echo - Network connection issues
    echo.
)

REM Return to project directory
cd /d "C:\Users\Ender\NeonLadder"

pause