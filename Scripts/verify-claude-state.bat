@echo off
echo ===============================================
echo Claude Extended State Verification System
echo ===============================================
echo.

echo [1/3] Checking Claude local settings...
if exist ".claude\settings.local.json" (
    echo ✅ Official settings found: .claude\settings.local.json
    echo.
    echo Validating JSON structure...
    powershell -Command "try { Get-Content '.claude\settings.local.json' | ConvertFrom-Json | Out-Null; Write-Host '✅ Valid JSON structure' -ForegroundColor Green } catch { Write-Host '❌ Invalid JSON structure' -ForegroundColor Red; exit 1 }"
) else (
    echo ❌ Official settings missing: .claude\settings.local.json
    echo Please run: cat .claude/settings.local.json
    exit /b 1
)

echo.
echo [2/3] Checking extended state configuration...
if exist ".claude\extended-state.json" (
    echo ✅ Extended state found: .claude\extended-state.json
    echo.
    echo Validating JSON structure...
    powershell -Command "try { Get-Content '.claude\extended-state.json' | ConvertFrom-Json | Out-Null; Write-Host '✅ Valid JSON structure' -ForegroundColor Green } catch { Write-Host '❌ Invalid JSON structure' -ForegroundColor Red; exit 1 }"
    echo.
    echo Checking key sections...
    powershell -Command "$config = Get-Content '.claude\extended-state.json' | ConvertFrom-Json; if ($config.personas) { Write-Host '✅ TMNT Personas loaded' -ForegroundColor Green } else { Write-Host '❌ Missing personas section' -ForegroundColor Red }"
    powershell -Command "$config = Get-Content '.claude\extended-state.json' | ConvertFrom-Json; if ($config.project_context) { Write-Host '✅ Project context loaded' -ForegroundColor Green } else { Write-Host '❌ Missing project context' -ForegroundColor Red }"
    powershell -Command "$config = Get-Content '.claude\extended-state.json' | ConvertFrom-Json; if ($config.github_automation) { Write-Host '✅ GitHub automation configured' -ForegroundColor Green } else { Write-Host '❌ Missing GitHub automation' -ForegroundColor Red }"
) else (
    echo ❌ Extended state missing: .claude\extended-state.json
    echo Please run: cat .claude/extended-state.json
    exit /b 1
)

echo.
echo [3/3] Checking CLAUDE.md integration...
if exist "CLAUDE.md" (
    echo ✅ CLAUDE.md found
    findstr /C:"cat .claude/extended-state.json" CLAUDE.md >nul
    if !ERRORLEVEL! == 0 (
        echo ✅ Extended state loading instructions present
    ) else (
        echo ❌ Missing extended state loading instructions in CLAUDE.md
    )
) else (
    echo ❌ CLAUDE.md missing
    exit /b 1
)

echo.
echo ===============================================
echo ✅ Claude Extended State System Operational
echo ===============================================
echo.
echo Next steps for new Claude sessions:
echo 1. Run: cat .claude/settings.local.json
echo 2. Run: cat .claude/extended-state.json  
echo 3. Choose random TMNT persona from extended-state.json
echo 4. Introduce yourself with persona details
echo.
pause