@echo off
echo 🚨 NICK FURY: Synchronizing external Claude settings from .claude folder...
echo.

REM Download real settings from Google Drive
"C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --destination .claude/settings.local.json --overwrite

if %ERRORLEVEL% == 0 (
    echo ✅ Settings synchronized successfully.
    echo 🚨 External memory system operational.
    echo 📡 Claude ready for deployment with full permissions.
) else (
    echo ❌ Sync failed. Using cached settings.
    echo 🚨 Check Google Drive authentication and try again.
)

echo.
echo 🦸 MARVEL TEAM SYSTEM: Ready for persona activation
echo 📚 EXTERNAL DOCS: All documentation externalized  
echo 🔧 PERMISSIONS: Full Unity CLI automation enabled
echo.