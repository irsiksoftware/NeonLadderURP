@echo off
setlocal enabledelayedexpansion

REM ============================================
REM SteamCmd Setup and Verification Script
REM ============================================
REM This script downloads and sets up SteamCmd for NeonLadder Steam builds
REM Ensures SteamCmd is available in the expected location
REM ============================================

echo.
echo ========================================
echo NeonLadder SteamCmd Setup Script
echo ========================================
echo.

REM Set paths
set "PROJECT_ROOT=%~dp0.."
set "STEAM_DIR=%~dp0"
set "STEAMCMD_LOCAL=%STEAM_DIR%steamcmd.exe"
set "STEAMCMD_TOOLS=C:\Tools\steamworks\tools\ContentBuilder\builder\steamcmd.exe"

echo Project Root: %PROJECT_ROOT%
echo Steam Directory: %STEAM_DIR%
echo.

REM Check if SteamCmd already exists locally
if exist "%STEAMCMD_LOCAL%" (
    echo [✓] SteamCmd found locally at: %STEAMCMD_LOCAL%
    goto :verify_steamcmd
)

REM Check if SteamCmd exists in Steamworks SDK location
if exist "%STEAMCMD_TOOLS%" (
    echo [✓] SteamCmd found in Steamworks SDK at: %STEAMCMD_TOOLS%
    echo [i] Creating symlink to local Steam directory...
    mklink "%STEAMCMD_LOCAL%" "%STEAMCMD_TOOLS%" >nul 2>&1
    if !ERRORLEVEL! EQU 0 (
        echo [✓] Symlink created successfully
    ) else (
        echo [!] Could not create symlink, copying file instead...
        copy "%STEAMCMD_TOOLS%" "%STEAMCMD_LOCAL%" >nul
        if !ERRORLEVEL! EQU 0 (
            echo [✓] SteamCmd copied successfully
        ) else (
            echo [✗] Failed to copy SteamCmd
            goto :download_steamcmd
        )
    )
    goto :verify_steamcmd
)

:download_steamcmd
echo [i] SteamCmd not found, downloading from Valve...
echo.

REM Download SteamCmd from Valve
echo Downloading SteamCmd...
powershell -Command "& {
    try {
        $ProgressPreference = 'SilentlyContinue'
        $url = 'https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip'
        $output = '%STEAM_DIR%steamcmd.zip'
        Write-Host '[i] Downloading from: ' + $url
        Invoke-WebRequest -Uri $url -OutFile $output -UseBasicParsing
        Write-Host '[✓] Download completed'

        Write-Host '[i] Extracting steamcmd.zip...'
        Expand-Archive -Path $output -DestinationPath '%STEAM_DIR%' -Force
        Write-Host '[✓] Extraction completed'

        Remove-Item $output -Force
        Write-Host '[✓] Cleanup completed'
    } catch {
        Write-Host '[✗] Download failed: ' + $_.Exception.Message
        exit 1
    }
}"

if %ERRORLEVEL% NEQ 0 (
    echo [✗] Failed to download SteamCmd
    echo Please download manually from: https://developer.valvesoftware.com/wiki/SteamCMD
    pause
    exit /b 1
)

:verify_steamcmd
echo.
echo ========================================
echo Verifying SteamCmd Installation
echo ========================================

REM Test SteamCmd
if not exist "%STEAMCMD_LOCAL%" (
    echo [✗] SteamCmd not found at: %STEAMCMD_LOCAL%
    echo [!] Manual setup required
    pause
    exit /b 1
)

echo [✓] SteamCmd found at: %STEAMCMD_LOCAL%

REM Get file size
for %%A in ("%STEAMCMD_LOCAL%") do set "STEAMCMD_SIZE=%%~zA"
set /a "STEAMCMD_SIZE_MB=%STEAMCMD_SIZE% / 1048576"

echo [i] File size: %STEAMCMD_SIZE_MB% MB
echo.

REM Test SteamCmd execution (quick help command)
echo [i] Testing SteamCmd execution...
"%STEAMCMD_LOCAL%" +help +quit >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [✓] SteamCmd is executable and responding
) else (
    echo [!] SteamCmd may need first-run initialization
    echo [i] Running first-time setup...
    "%STEAMCMD_LOCAL%" +quit
)

echo.
echo ========================================
echo Setup Complete!
echo ========================================
echo.
echo SteamCmd is ready for use. You can now run:
echo - upload_steam_demo.bat (for Steam uploads)
echo - Direct steamcmd commands from: %STEAMCMD_LOCAL%
echo.
echo First-time usage will download additional Steam client files (~300MB)
echo This is normal and only happens once.
echo.

pause