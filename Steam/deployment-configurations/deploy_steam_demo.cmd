@echo off
echo ====================================
echo NeonLadder Steam Deployment Script
echo ====================================
echo.

echo Deploying NeonLadder Demo to Steam...
echo App ID: 3089980
echo Generated: 2025-09-25 17:55:10
echo Latest Build: steam-demo-2025-09-25-20250925-174624
echo.

REM Change to Steam folder
cd /d "%~dp0"

REM Verify steamcmd exists
if not exist ".\..\steamcmd.exe" (
    echo ===============================================
    echo ERROR: steamcmd.exe not found in Steam folder
    echo ===============================================
    echo.
    echo Please install SteamCMD:
    echo 1. Download from: https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip
    echo 2. Extract steamcmd.exe to: %~dp0
    echo 3. Run this script again
    echo.
    echo OR if SteamCMD is installed elsewhere:
    echo Copy steamcmd.exe to this Steam folder
    echo.
    pause
    exit /b 1
)

echo Starting Steam upload...
echo Username: irsiksoftware (update this in the script if needed)
echo.

".\..\steamcmd.exe" +login irsiksoftware +run_app_build "%~dp0app_build_3089980_demo.vdf" +quit

echo.
echo ====================================
echo Deployment complete!
echo ====================================
echo Check the output above for any errors
echo.
echo If successful, your build is now on Steam!
echo Remember to set it live in Steamworks if needed.
pause