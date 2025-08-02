@echo off
echo 🚨 NICK FURY: Synchronizing external Claude settings from .claude folder...
echo.

REM Create .claude folder if it doesn't exist
if not exist ".claude" (
    echo 📁 Creating .claude folder...
    mkdir .claude
)

REM Download real settings from Google Drive to temp, then move
"C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --overwrite
if %ERRORLEVEL% == 0 (
    move settings.local.json .claude\settings.local.json >nul
)

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