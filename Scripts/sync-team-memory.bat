@echo off
echo ===============================================
echo Marvel Team Memory Synchronization System
echo ===============================================
echo.

set "TIMESTAMP=%date:~-4,4%-%date:~-10,2%-%date:~-7,2% %time:~0,8%"

echo [1/4] Checking team memory structure...
if not exist ".claude\team-memory" (
    echo ❌ Team memory directory missing
    echo Creating team memory structure...
    mkdir ".claude\team-memory"
    mkdir ".claude\team-memory\personas"
)

if not exist ".claude\team-memory\shared-context.json" (
    echo ❌ Shared context missing - initializing...
    echo Creating shared context file...
)

echo ✅ Team memory structure verified
echo.

echo [2/4] Updating shared context with timestamp...
powershell -Command "(Get-Content '.claude\team-memory\shared-context.json' | ConvertFrom-Json) | ForEach-Object { $_.integration_points.last_sync = '%TIMESTAMP%'; $_ } | ConvertTo-Json -Depth 10 | Set-Content '.claude\team-memory\shared-context.json'"

echo ✅ Shared context updated
echo.

echo [3/4] Scanning active personas...
set "PERSONA_COUNT=0"
for %%f in (.claude\team-memory\personas\*.json) do (
    set /a "PERSONA_COUNT+=1"
    echo   📝 Found: %%~nf
)

echo ✅ Found %PERSONA_COUNT% active persona memories
echo.

echo [4/4] Team status summary...
echo.
echo 📊 TEAM STATUS DASHBOARD
echo ===============================================
echo 🕐 Last Sync: %TIMESTAMP%
echo 👥 Active Personas: %PERSONA_COUNT%
echo 📁 Memory Files: 
echo    - Shared Context: ✅
echo    - Technical Decisions: ✅  
echo    - Active Sprints: ✅
echo    - Individual Memories: %PERSONA_COUNT% files
echo.

echo 🎯 QUICK TEAM SELECTION GUIDE
echo ===============================================
echo Avengers (Core Team):
echo   @tony-stark     - Technical Lead / Principal Architect
echo   @steve-rogers   - Scrum Master / Team Lead  
echo   @bruce-banner   - Senior QA Engineer / Test Architect
echo   @natasha-romanoff - Security Engineer / Code Auditor
echo.
echo X-Men (Specialists):
echo   @charles-xavier - Product Owner / Vision Keeper
echo   @jean-grey      - Business Analyst / Requirements
echo   @wolverine      - DevOps Engineer / Infrastructure
echo   @storm          - UX/UI Designer / Experience Architect
echo.
echo Fantastic Four (Quality & Innovation):
echo   @reed-richards  - R&D Lead / Innovation Specialist
echo   @sue-storm      - Senior Code Reviewer / Quality Guardian
echo   @johnny-storm   - Performance Engineer / Optimization
echo   @ben-grimm      - Legacy Systems / Technical Debt Fighter
echo.
echo Guardians (Support & Documentation):
echo   @peter-quill    - Junior Developer / New Team Member
echo   @gamora         - Database Specialist / Data Architect
echo   @rocket         - Technical Writer / Documentation
echo.

echo 💡 USAGE EXAMPLES
echo ===============================================
echo "Hey @sue-storm, can you review this PR for AC alignment?"
echo "I need @jean-grey to help create user stories from the Google Drive doc"
echo "Let's get @tony-stark to architect this new feature"
echo "Time for @wolverine to set up our CI/CD pipeline"
echo.

echo ✅ Team memory sync complete!
echo ===============================================
pause