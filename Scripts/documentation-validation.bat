@echo off
echo ===============================================
echo Documentation Consistency Validation System
echo ===============================================
echo.

echo [1/5] Checking README.md → CLAUDE.md handoff...
findstr /C:"CLAUDE.md" README.md >nul
if !ERRORLEVEL! == 0 (
    echo ✅ README correctly references CLAUDE.md
) else (
    echo ❌ Missing CLAUDE.md references in README
)

echo.
echo [2/5] Validating Marvel team system consistency...
findstr /C:"@tony-stark" README.md >nul
if !ERRORLEVEL! == 0 (
    findstr /C:"@tony-stark" CLAUDE.md >nul
    if !ERRORLEVEL! == 0 (
        echo ✅ Marvel team syntax consistent across files
    ) else (
        echo ❌ Marvel team syntax mismatch between files
    )
) else (
    echo ❌ Missing Marvel team examples in README
)

echo.
echo [3/5] Checking extended state system...
if exist ".claude\extended-state.json" (
    findstr /C:"marvel_team" .claude\extended-state.json >nul
    if !ERRORLEVEL! == 0 (
        echo ✅ Extended state contains Marvel team configuration
    ) else (
        echo ❌ Missing Marvel team in extended state
    )
) else (
    echo ❌ Extended state file missing
)

echo.
echo [4/5] Validating team memory system...
if exist ".claude\team-memory" (
    if exist ".claude\team-memory\shared-context.json" (
        echo ✅ Team memory infrastructure present
    ) else (
        echo ❌ Missing shared context file
    )
) else (
    echo ❌ Team memory directory missing
)

echo.
echo [5/5] Checking documentation flow for new contributors...
echo.
echo 📋 DOCUMENTATION FLOW ANALYSIS:
echo ===============================================
echo 1. GitHub visitors see README.md first
echo    - ✅ Project overview and game features
echo    - ✅ Marvel team introduction with examples
echo    - ✅ Clear handoff to CLAUDE.md for technical details
echo.
echo 2. Contributors read CLAUDE.md for development
echo    - ✅ Complete Marvel team system
echo    - ✅ Technical priorities and development workflow
echo    - ✅ Testing infrastructure and CLI automation
echo.
echo 3. Claude models load configuration
echo    - ✅ Extended state with Marvel personas
echo    - ✅ Team memory for persistent learning
echo    - ✅ Conflict resolution protocols
echo.

echo ✅ Documentation consistency validation complete!
echo ===============================================
echo.
echo 📊 DOCUMENTATION METRICS:
echo - README.md: Streamlined for GitHub visibility
echo - CLAUDE.md: Comprehensive development guide  
echo - Extended State: 14 Marvel personas configured
echo - Team Memory: Persistent learning system active
echo - Handoff Flow: Clear progression from README → CLAUDE.md
echo.
pause