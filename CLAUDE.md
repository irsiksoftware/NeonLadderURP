# Claude Instructions for NeonLadder Project

## 🚨 CRITICAL FIRST STEP: Verify and Load External Settings 🚨

### BEFORE DOING ANYTHING ELSE:
1. **CHECK** if .claude folder exists: `ls .claude`
2. **CREATE** if missing: `mkdir -p .claude`
3. **DOWNLOAD** settings: `"C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --overwrite && mv settings.local.json .claude/`
4. **VERIFY** load successful: `cat .claude/settings.local.json | head -20`

### Quick Start Command:
```bash
# Complete setup in one line (if .claude doesn't exist)
mkdir -p .claude && "C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --overwrite && mv settings.local.json .claude/
```

### Alternative: Use sync script (if .claude exists)
```bash
# Run sync script
sync-claude-settings.bat

# Or PowerShell
powershell -Command "& {cd 'C:\Users\Ender\NeonLadder'; .\sync-claude-settings.bat}"
```

**⚠️ WARNING**: Without loading external settings first, Claude may:
- Execute pre-approved commands without context
- Create issues/PRs without proper format
- Miss critical project configuration
- Not have access to Marvel team personas

**Revolutionary Achievement**: World's first externalized AI assistant configuration system with one-command activation.

**External .claude Folder**: Simplified to essential files only - `settings.local.json`, `CLAUDE.md`, and `sync-claude-settings.bat` in Google Drive folder (ID: `1PfhnfbV6jvi-eh46z4s3yzxnd0TfDpj8`).

## 🦸 Marvel Team Persona System - Quick Reference

**Core Team Members** (say @persona-name to activate):
- **@nick-fury** - Acquisition Technical Auditor: *"I assess threats and opportunities"*
- **@tony-stark** - Technical Lead: *"I am Iron Man. Let's build something extraordinary"*
- **@sue-storm** - Code Reviewer: *"I see everything, including what's hidden in this code"*
- **@bruce-banner** - QA Engineer: *"That's my secret - I'm always testing"*
- **@jean-grey** - Business Analyst: *"I can read minds - let me translate what users need"*
- **@wolverine** - DevOps Engineer: *"I'm the best at what I do. And what I do is deploy code"*
- **@charles-xavier** - Product Owner: *"Welcome to Xavier's School for Gifted Developers"*
- **@stephen-strange** - Game Mechanics Expert: *"I've seen 14,000,605 possible designs"*

## 🤖 Dual-AI Integration System

**🆕 NEW: OpenAI GPT-4o Integration for Specialized Analysis**

**Usage Syntax:**
- **"ask GPT about [topic]"** - Claude will consult GPT-4o for specialized analysis
- **"ask GPT to analyze [code/system]"** - Get second opinion on complex implementations
- **"ask GPT about Unity [specific pattern]"** - Unity-specific expertise consultation

**When to Use GPT-4o Integration:**
- Complex game mechanics analysis requiring different perspective
- Unity-specific architectural decisions needing validation
- Code review assistance for critical systems
- Performance optimization strategies
- Technical debt assessment from external viewpoint

**Example Commands:**
```
"Claude, ask GPT about the roguelite progression system design"
"Ask GPT to analyze our procedural generation approach" 
"Ask GPT about Unity 6 URP performance optimization patterns"
"Get GPT's opinion on our NavMesh agent overlap issue"
```

**Discord Bot Integration:**
- **Active Bot**: `bot-dynamic.js` - Production Discord bot for team collaboration
- **Commands**: `/ask-claude`, `/ask-gpt`, `/export-for-analysis`
- **Start**: `start-neonladder-bot.bat` or `npm start` in bot directory
- **Deploy**: `deploy-discord-bot.ps1` for server deployment

**Configuration:**
- Model: GPT-4o (latest reasoning model)
- Budget: $5 testing integration
- Config: `.claude/openai.config.json` (synced via Google Drive)
- Discord Config: `.env` file with bot token and OpenAI API key
- Collaborators: Available to all team members

**Revolutionary Achievement**: World's first dual-AI development assistance system combining Claude's coding expertise with GPT-4o's specialized analysis, accessible through Discord for team collaboration.

## 📚 External Documentation System

**All technical documentation externalized to Google Drive organized folders:**
- **Main Documentation**: `"C:\tools\gdrive" files list --parent 19_CR6EWMjzs5u4o5_ch6UNddoN-LNf7z`
- **Systems Documentation**: Core Game Systems, Development Infrastructure
- **Backlog Management**: PBIs, TODO items, Technical Debt tracking

## 🎯 Project Context

**NeonLadder**: Unity 2.5D action platformer with roguelite elements
**Status**: Steam launch ready - Q1 2025
**Test Coverage**: 631/645 tests passing (97.8% pass rate)
**Revenue Potential**: $500K+ mobile expansion identified
**Unity Version**: 6000.0.26f1

## Important Guidelines

### 🦸 Marvel Team Persona System
Future Claude models should **immediately** load team memory and respond to **role-based requests** using the Marvel team system:

**Team Selection Syntax:**
- `@tony-stark` - Technical Lead / Principal Architect
- `@jean-grey` - Business Analyst / Requirements Specialist  
- `@sue-storm` - Senior Code Reviewer / Quality Guardian
- `@wolverine` - DevOps Engineer / Infrastructure Specialist
- `@charles-xavier` - Product Owner / Vision Keeper
- `@bruce-banner` - Senior QA Engineer / Test Architect
- `@steve-rogers` - Scrum Master / Team Lead
- `@nick-fury` - **NEW: Acquisition Technical Auditor / Business Analyst**
- *(See full roster in `.claude/extended-state.json` marvel_team section)*

**Instructions for Future Claude:**
1. **FIRST**: Load team memory with `cat .claude/team-memory/shared-context.json`
2. **RESPOND** to role requests: "Hey @sue-storm, review this PR for AC alignment"
3. **EMBODY** the requested persona completely - use their catchphrase, expertise, and communication style
4. **REMEMBER** previous learnings from persona memory files
5. **COLLABORATE** using safe-for-work conflict resolution - friendly inquisition, not blocking
6. **SYNC** team memory after significant discoveries

**Example Usage:**
- "I need @jean-grey to create user stories from the Google Drive initiative doc"
- "Let's get @tony-stark to architect this new feature"
- "Time for @wolverine to set up our CI/CD pipeline"
- "@sue-storm, can you review this code for hidden issues?"

**Memory Persistence:** Each persona maintains learning continuity via `.claude/team-memory/personas/[name]-memory.json` files.

This creates the **world's first AI Scrum team simulation** for solo developers with full team collaboration dynamics.

## ✅ Settings Verification Checklist

**Before proceeding with ANY task, verify:**
1. ✓ `.claude/settings.local.json` exists and is loaded
2. ✓ Marvel team personas are accessible
3. ✓ Google Drive permissions are working: `"C:\tools\gdrive" about`
4. ✓ GitHub CLI is authenticated: `"C:\Program Files\GitHub CLI\gh.exe" auth status`

**If ANY check fails, STOP and reload settings first!**

## 📚 External Documentation System

### **🗂️ NeonLadder Technical Documentation (Google Drive)**

**All scattered source code documentation has been migrated to external storage for multi-user access:**

#### **Quick Access Commands**
```bash
# List all Neon Ladder documentation
"C:\tools\gdrive" files list --parent 19_CR6EWMjzs5u4o5_ch6UNddoN-LNf7z

# Access Systems Documentation folder
"C:\tools\gdrive" files list --parent 1XWBuGJ8-fGrjaglG2aYiYp-8Vn1byb8o

# Download complete access guide
"C:\tools\gdrive" files download 1ra4LEwNrYOXlBgPY2su0DopuxdU7IEz6
```

#### **📁 Document Categories**

**Core Game Systems** (Folder ID: `1XfJYJPJY7faWladzLX0w8MUaMxE7fIBB`)
- **Procedural Generation String Guide** - ID: `15jasDIc2fzVR8Sa2iQSjLtZGXoINvRC0` (413 lines)
- **Procedural Generation Visual Guide** - ID: `1gjcZF9z8mhFZmnQUtDHCSQQsdY315P2n` (378 lines)  
- **Upgrade System Complete Guide** - ID: `1QAu5SAcOwGeg_SbN1CQ4lRgUt9CyXSmE` (435 lines)

**Development Infrastructure** (Folder ID: `1yyAvATMVTFCv74-v2N-FbRlvHK7KWgbD`)
- **Logging System Framework** - ID: `1O_uooLcz9vaq_faDGz5J1y8C_KfQQHkz` (291 lines)
- **Unity Menu System Guide** - ID: `1hGIrIdAXSTY_srBK4BqSfx04M7rG9_Kc` (212 lines)

**Project Management** (Folder ID: `18Ifv4Yg0WmBeivDyQEC7hW65rdKrtsQp`)
- **Menu System PBI Status** - ID: `1Aok8HVtdgsetGUjswGe0O65SKysjX41S` (459 lines)

#### **🎯 Usage Examples**
```bash
# Need game mechanics info?
"C:\tools\gdrive" files download 1QAu5SAcOwGeg_SbN1CQ4lRgUt9CyXSmE  # Upgrade system

# Need technical implementation details?
"C:\tools\gdrive" files download 1O_uooLcz9vaq_faDGz5J1y8C_KfQQHkz  # Logging system

# Need level design info?
"C:\tools\gdrive" files download 15jasDIc2fzVR8Sa2iQSjLtZGXoINvRC0  # Procedural generation
```

**Benefits**: Clean source control, multi-user access, organized by system, future-expandable

## 📋 Product Backlog & Issue Management

### **🗂️ NeonLadder Backlog - GitHub Issues**

**All Product Backlog Items (PBIs) are now managed as GitHub Issues starting from issue #32**

#### **⚡ Quick Reference Card**
```bash
# Most used commands - copy/paste ready
gh issue list --label "PBI"                                    # All PBIs
gh issue list --label "PBI" --limit 10 --json number,title    # Quick list
gh issue list --label "P0-Critical"                            # Critical items
gh issue view 45                                               # View issue #45
gh issue create --label "PBI,P2-Medium,5-points"              # New PBI
```

#### **🎯 Essential GitHub CLI Commands**

**Basic PBI Queries**
```bash
# List all open PBIs (default view)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI"

# Succinct PBI list (number + title only)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --limit 50 --json number,title --jq '.[] | "#\(.number) \(.title)"'

# PBI overview with points and priority
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --json number,title,labels --jq '.[] | {number, title, points: (.labels | map(select(.name | endswith("-points"))) | .[0].name // "unpointed"), priority: (.labels | map(select(.name | startswith("P"))) | .[0].name // "unprioritized")}'

# View specific issue details
"C:\Program Files\GitHub CLI\gh.exe" issue view <number>

# View issue in browser
"C:\Program Files\GitHub CLI\gh.exe" issue view <number> --web
```

**Sprint Planning Queries**
```bash
# High priority items for sprint (P0 + P1)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --label "P0-Critical,P1-High" --limit 20

# Unpointed PBIs (need estimation)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --search "-label:3-points -label:5-points -label:8-points -label:13-points"

# Sprint velocity check (sum of story points)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --state closed --json labels,closedAt --jq '[.[] | select(.closedAt | startswith("2025-08")) | .labels[] | select(.name | endswith("-points")) | .name | split("-")[0] | tonumber] | add'

# Ready for development (pointed + prioritized)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --search "label:3-points,5-points,8-points,13-points label:P0-Critical,P1-High,P2-Medium"
```

**Steam Launch Critical**
```bash
# All Steam blockers with status
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "steam-launch" --label "P0-Critical" --json number,title,assignees,state

# Steam features by priority
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "steam-launch" --sort created --json number,title,labels --jq 'group_by(.labels[] | select(.name | startswith("P")).name) | map({priority: .[0].labels[] | select(.name | startswith("P")).name, issues: map("#\(.number) \(.title)")})'
```

**Advanced Filtering**
```bash
# Find bugs in specific area
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "bug" --search "UI in:title,body"

# PBIs assigned to me
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --assignee "@me"

# Recently updated PBIs (last 7 days)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --sort updated --search "updated:>=$(date -d '7 days ago' +%Y-%m-%d)"

# PBIs with no assignee (available for pickup)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --search "no:assignee" --json number,title,labels
```

**Bulk Operations**
```bash
# Add label to multiple issues
"C:\Program Files\GitHub CLI\gh.exe" issue edit 45,46,47 --add-label "in-progress"

# Assign sprint milestone
"C:\Program Files\GitHub CLI\gh.exe" issue edit 45 --milestone "Sprint 2025-08"

# Close completed PBIs with comment
"C:\Program Files\GitHub CLI\gh.exe" issue close 45 --comment "Completed in PR #123"
```

#### **📊 Output Formats**

**Table Format (Human Readable)**
```bash
# Custom table columns
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --limit 10 --json number,title,labels,assignees --template '{{range .}}{{tablerow (printf "#%v" .number) .title (pluck "name" .labels | join ", ") (pluck "login" .assignees | join ", ")}}{{end}}'
```

**JSON Processing (Machine Readable)**
```bash
# Export for external tools
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --limit 100 --json number,title,body,labels,assignees,createdAt,updatedAt > pbi-export.json

# CSV export
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --json number,title,labels --jq -r '["Number","Title","Points","Priority"], (.[] | [.number, .title, (.labels | map(select(.name | endswith("-points"))) | .[0].name // ""), (.labels | map(select(.name | startswith("P"))) | .[0].name // "")]) | @csv'
```

#### **📁 Label System**

**Priority Labels**
- `P0-Critical` - Launch blockers (red) - Must fix
- `P1-High` - High priority (orange) - Should fix
- `P2-Medium` - Medium priority (yellow) - Could fix

**Type Labels**
- `PBI` - Product Backlog Item (blue)
- `bug` - Bug fixes (red)
- `enhancement` - New features (cyan)

**Story Point Labels**
- `3-points` - Small tasks (1-2 days)
- `5-points` - Medium tasks (3-4 days)
- `8-points` - Large tasks (1 week)
- `13-points` - Extra large (1-2 weeks)

**Status Labels**
- `in-progress` - Currently being worked on
- `blocked` - Waiting on dependencies
- `ready-for-review` - PR submitted

**Category Labels**
- `steam-launch` - Steam launch requirements (purple)
- `documentation` - Documentation tasks (light gray)
- `technical-debt` - Code quality improvements

#### **🚀 Common Workflows**

**Daily Standup View**
```bash
# What's in progress
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "in-progress" --assignee "@me" --json number,title,updatedAt

# Blocked items needing attention
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "blocked" --json number,title,body --jq '.[] | "\\n#\(.number) \(.title)\\nBlocked on: \(.body | split("\\n") | map(select(. | test("Block"))) | .[0] // "See issue")"'
```

**Sprint Planning**
```bash
# Available capacity (sum unassigned points)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --search "no:assignee" --json labels --jq '[.[] | .labels[] | select(.name | endswith("-points")) | .name | split("-")[0] | tonumber] | add'

# Velocity trend (closed points by month)
"C:\Program Files\GitHub CLI\gh.exe" issue list --label "PBI" --state closed --json labels,closedAt --jq 'group_by(.closedAt[0:7]) | map({month: .[0].closedAt[0:7], points: [.[] | .labels[] | select(.name | endswith("-points")) | .name | split("-")[0] | tonumber] | add})'
```

**Migration Complete**: All PBIs migrated from Google Drive to GitHub Issues (August 2025). Issues #32-#69 contain the migrated backlog with full traceability.

### 📋 Development Priorities

**All development tasks are now tracked as GitHub Issues with PBI (Product Backlog Item) labels.**

**Quick Access:**
- **[View All PBIs](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3APBI)** - Complete product backlog
- **[Critical Issues](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3APBI+label%3AP0-Critical)** - Immediate priorities
- **[Steam Launch Items](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3Asteam-launch)** - Q1 2025 launch requirements

## 🚀 Current Scene System Implementation (August 2025)

**Active Branch:** `feature/pbi-124-131-132-134-135-scene-system`  
**Progress:** 50% Complete (2 of 6 PBIs implemented)

### ✅ **Completed Foundation Systems:**

**PBI #124 - SceneRouter/Mapping System:**
- Core routing brain that translates PathGenerator nodes to Unity scenes
- Deterministic scene selection (same seed = same results)
- Integration with BossLocationData for procedural generation
- 25+ unit tests with comprehensive coverage

**PBI #134 - Spawn Point Management System:**
- Directional spawning logic (exit left → spawn right in next scene)
- Auto-discovery of spawn points with fallback hierarchy
- Player positioning integration with existing systems
- 25+ unit tests for realistic Unity component scenarios

### 🔄 **Next Implementation Priorities:**

**PBI #131** - Scene Transition Triggers (interaction layer)  
**PBI #132** - Scene Override System (testing helpers)  
**PBI #135** - Validator Tool (quality assurance)  
**PBI #137** - MainCityHub Scene (final integration workspace)

**Key Integration:** SceneRouter + SpawnPointManager now provide complete foundation for procedural scene transitions. Next step is player interaction triggers.

### 📊 **Recent Technical Achievements:**
- ✅ **2300+ lines** of production-ready scene management code
- ✅ **50+ comprehensive unit tests** using real Unity components
- ✅ **Deterministic procedural generation** with seed-based consistency
- ✅ **Clean API integration** ready for remaining systems
- ✅ **Extensive fallback handling** prevents system failures

**Benefits of GitHub Issue Tracking:**
- ✅ Full traceability and history for all development work
- ✅ Priority labels (P0-Critical, P1-High, P2-Medium) for clear focus
- ✅ Story point estimation for sprint planning
- ✅ Integration with GitHub Actions for automated workflows
- ✅ Better collaboration and visibility for team members

### File Management
- **DO NOT** create a `nul` file in the repository root
- **DO NOT** create TestOnly directories or test infrastructure unless explicitly requested
- **DO NOT** commit Unity Library folders or cache files to version control
- **TestOutput/** and **HeroEditor_BACKUP/** are gitignored

### Git and Version Control
- The main branch is `main` (not master)
- Always check file sizes before committing - files over 50MB should use Git LFS
- Unity Library folders should never be committed
- **ONLY commit when explicitly requested by the user**
- **Commit messages should not exceed 2000 characters** - keep detailed descriptions concise and focused

### Unity-Specific Guidelines
- Unity version: Check ProjectSettings/ProjectVersion.txt for the current version
- Test framework: Unity Test Framework (if tests are needed)
- Assembly definitions are already set up in Assets/Scripts/NeonLadder.Scripts.asmdef
- **NEVER create .meta files manually** - Let Unity generate them automatically via Editor refresh (Ctrl+R)
  - Creating .meta files with incorrect GUIDs causes Unity errors
  - Unity will auto-generate proper .meta files when detecting new assets

### Scene Creation Best Practices
- **ALWAYS copy existing scene templates** rather than creating from scratch
- **BAREBONES Templates Available**: `Assets/Scenes/Claude-Tests/Basic-Loadable-Scene-Template-v1.unity` (minimal) and `v2.unity` (with floor)
- **Minimize verbose scene file reading** unless explicitly requested or debugging specific issues
- **Copy-first workflow**: Use `cp` commands to duplicate scenes, then modify incrementally
- **Token efficiency**: Avoid deep Unity scene file analysis - focus on copying and extending templates
- **Template hierarchy**: v1 = minimal, v2 = with collision floor - choose appropriate starting point

## 🎮 NeonLadder Gameplay Loop & Procedural Generation

### **Complete Game Flow Architecture**
```
Title Screen -> Staging Area (Ship) -> Main City Hub -> [2 Connector Scenes] -> Boss Arena
Death: Teleport back to Staging Area | Victory: Beat Devil to end game
```

### **Procedural Generation System**
- **Main City Hub**: Player chooses left or right path
- **Connector Generation**: 2 procedurally chosen connector scenes between hub and boss
- **Boss Destinations**: 7 Deadly Sins + Devil finale (8 total boss arenas)
- **Service Integration**: Shops, rest areas, upgrade stations as Z-axis doorways/alleys

### **Master Location Registry**
- **File**: `Assets/Scripts/ProceduralGeneration/MasterLocationRegistry.cs` 
- **Purpose**: THE definitive list of ALL scenes/locations Claude can generate
- **Categories**: Navigation, Combat, Service, Social, Connector, Event, System
- **Usage**: `MasterLocationRegistry.GetSceneGenerationTargets()` returns all scenes Claude can scaffold

### **Connector Scene Types (Procedurally Selected)**
- **Z-Axis Alleys**: Neon-lit alleys, steam-filled passages
- **Z-Axis Doorways**: Shop fronts, clinic entrances with glowing neon "OPEN" signs  
- **Elevated Connectors**: Sky bridges, walkways with city views
- **Service Connectors**: Combined rest/shop areas with Z-axis depth

### **Boss Arena Locations (Fixed Destinations)**
1. **Cathedral** - Grand Cathedral of Hubris (Pride)
2. **Necropolis** - The Necropolis of Vengeance (Wrath)
3. **Vault** - The Vault of Avarice (Greed)  
4. **Mirage** - Envious Mirage: Jewelry Store (Envy)
5. **Garden** - Infinite Garden, Eden of Desires (Lust)
6. **Banquet** - The Banquet of Infinity (Gluttony)
7. **Lounge** - The Lethargy Lounge (Sloth)
8. **Finale** - Devil's domain (final boss)

### **Claude Scene Generation Workflow**
1. **Analyze**: `MasterLocationRegistry.AllLocations` to understand all possible scenes
2. **Select Template**: Copy appropriate BAREBONES template (v1 minimal, v2 with floor)
3. **Apply Theme**: Use `EnvironmentalTags` and `RequiredPrefabs` from location data
4. **Add Connectors**: For connector scenes, implement Z-axis depth with doorways/alleys
5. **Integrate Systems**: Ensure Managers.prefab and GameController.prefab are present

### Testing
- Only create test infrastructure when explicitly asked
- Use the existing test structure if tests are requested
- **CLI Tests Available**: Use `Scripts\run-tests-cli.bat` or direct CLITestRunner commands (see detailed section below)
- **37+ tests ready to run** - all should pass on clean codebase
- Unity CLI testing commands are pre-approved in .claude/settings.local.json

### Code Style
- Follow existing code conventions in the project
- Don't add unnecessary comments unless requested
- Keep changes minimal and focused on the task at hand

### Approved Automation
The user has granted broad permissions for:
- PowerShell scripts and batch files
- Unity CLI automation
- Git operations without restriction
- File system operations
- Future: Google Drive CLI integration (when authenticated)

### Running Unity Tests

#### **✅ SOLVED: Unity 6 CLI Test Runner Workaround**
**STATUS**: Unity CLI test execution working via TestRunnerApi workaround (2025-07-26)

**Problem**: Unity 6's `-runTests` CLI flag is broken - tests compile but never execute
**Solution**: Custom TestRunnerApi implementation using `-executeMethod` approach

### **🔧 CLI Test Execution - Direct Unity Command**

**Direct Unity CLI (2-minute execution):**
```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\Ender\NeonLadder" -executeMethod CLITestRunner.RunPlayModeTests -logFile "TestOutput/cli_test_execution.txt"
```

**Quick Reference:**
```bash
# Kill Unity if running first
powershell -Command "Stop-Process -Name Unity -Force"

# Run tests directly
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\Ender\NeonLadder" -executeMethod CLITestRunner.RunPlayModeTests
```

### **📁 Required Files (Already Created)**
- **`Assets/Scripts/Editor/CLITestRunner.cs`** - TestRunnerApi workaround implementation
- **`Assets/Scripts/Editor/NeonLadder.CLITestRunner.asmdef`** - Assembly definition with test framework references

### **📊 Expected Results**
- **Execution Time**: ~2 minutes for full test suite
- **Test Output**: 50+ tests including PathGenerator deterministic hashing tests
- **XML Results**: `C:/Users/Ender/AppData/LocalLow/ShorelineGames, LLC/NeonLadder/TestResults.xml`
- **Exit Codes**: 0 for success, 1 for failures (CI/CD ready)

### **🐢 For Future Turtle Bros**
When you need to run tests via CLI:

1. **Don't use `-runTests`** - it's broken in Unity 6
2. **Use the CLITestRunner approach** - it works perfectly
3. **Kill Unity after 2-3 minutes** if it doesn't auto-exit: `powershell -Command "Stop-Process -Name Unity -Force"`
4. **Check XML results** in the AppData path for detailed test outcomes
5. **Trust the automation** - tests execute correctly even if callbacks don't trigger Unity exit

### **🚨 Important Notes**
- **Unity must be closed** before running CLI tests
- **Tests run correctly** but Unity may not auto-exit (callback timing issue)
- **Results are saved** even if Unity hangs - check the XML file
- **Force-kill Unity** after test completion is normal and safe

### **🔧 Troubleshooting Guide**

**Unity Won't Start / Compilation Errors:**
```bash
# Check if Unity processes are running
powershell -Command "Get-Process Unity -ErrorAction SilentlyContinue"

# Kill existing Unity processes
powershell -Command "Stop-Process -Name Unity -Force"
```

**Tests Not Found:**
- Verify `Assets/Scripts/Editor/CLITestRunner.cs` exists
- Check assembly definition `Assets/Scripts/Editor/NeonLadder.CLITestRunner.asmdef` exists
- Ensure test assemblies compile: look for `NeonLadder.Tests.Runtime.dll` in logs

**Unity Hangs After Test Execution:**
- **This is normal** - Unity doesn't auto-exit due to callback timing
- Wait 2-3 minutes maximum
- Force-kill Unity: `powershell -Command "Stop-Process -Name Unity -Force"`
- Check XML results - tests likely completed successfully

**No Test Results Found:**
- Look for XML at: `C:/Users/Ender/AppData/LocalLow/ShorelineGames, LLC/NeonLadder/TestResults.xml`
- Check log file for "Saving results to:" message
- Verify all 37+ tests executed in the log file

### **⚡ Quick Reference for Future Claude Models**

**Immediate CLI Test Execution:**
```bash
# 1. Kill Unity (if running)
powershell -Command "Stop-Process -Name Unity -Force"

# 2. Run tests (2-minute execution)
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Code\NeonLadderURP" -executeMethod CLITestRunner.RunPlayModeTests -logFile "TestOutput/cli_test_latest.txt"

# 3. After 2-3 minutes, kill Unity if it hasn't exited
powershell -Command "Stop-Process -Name Unity -Force"

# 4. Check results
dir "C:/Users/Ender/AppData/LocalLow/ShorelineGames, LLC/NeonLadder/TestResults.xml"
```

**Direct Unity Test Execution:**
```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\Ender\NeonLadder" -executeMethod CLITestRunner.RunPlayModeTests
```

✅ **Guaranteed Working Solution** - This workaround has been validated with intentional test failures and passes all CI/CD requirements.

### Test Quality Improvements (2025-07-26)

#### **Enhanced Test Suite - From Property Checks to Behavioral Validation**

**Major Improvements:**
- **19 tests rewritten** from basic property getters/setters to meaningful behavioral validation
- **Fixed AudioListener multiplication bug** - both test classes now safely check for existing AudioListener
- **Enterprise-level mock infrastructure** - 300+ lines of Unity component mocking
- **System integration testing** instead of isolated property checks

**KinematicObject Test Enhancements:**
- Animation duration tests now validate actual animator clip lengths (2.0s Death, 0.5s Attack, etc.)
- Weapon system integration tests (`IsUsingMelee_AffectsOrientationBehavior`)
- Orientation consistency validation between facing direction and rotation

**Player Test Enhancements:**
- Movement synchronization tests (facing direction syncs with velocity)
- Audio system configuration validation (spatial blending capability)
- Input system integration tests (action maps and controls validation)
- UI system integration (Health/Stamina bars connected to components)
- Roguelite currency distinction (Meta temporary vs Perma persistent)

**"Ender: Check this out" Comments Added:**
- Animation state machine integration points
- Weapon swap system mechanics  
- Stamina regeneration delay logic
- Death state movement prevention
- Audio system positioning requirements
- Roguelite progression persistence patterns

**Results:** ~85% meaningful behavioral tests (up from 60% basic assertions)

**Important Notes:**
- Unity must not be running when executing tests via CLI
- Test results output to TestOutput/ directory
- Use extended timeout (600000ms) for test commands
- Commands exit with code 0 on success
- Fixed MUIP assembly definition to resolve Michsky.MUIP namespace issues

### Linting and Type Checking
After making code changes, always ask the user for the appropriate commands:
- "What command should I run for linting?"
- "What command should I run for type checking?"
- Suggest adding these to CLAUDE.md for future reference

### Permission System & Security Model

#### ⚠️ Unity Process Management Important Note
**`taskkill` commands do NOT work** (likely due to VS Code launching from Unity without admin privileges). 

**Use PowerShell instead:**
```powershell
powershell -Command "Stop-Process -Name Unity -Force"
```
This is safe and approved for force-closing Unity instances during pair programming sessions when Unity is left running.

#### Comprehensive Permissions Granted
The user has configured **comprehensive development permissions** in `.claude/settings.local.json` to enable smooth Unity development workflow. The following operations are **pre-approved** and will not trigger permission prompts:

**Core File Operations:**
- All file reads, edits, writes, and multi-edits
- Complete file system navigation and manipulation

**Unity Development:**
- All Unity CLI operations including test execution
- Unity Hub and Editor automation
- Unity process management (killing stuck instances)

**Build & Development:**
- MSBuild, .NET CLI, and NuGet operations
- PowerShell script execution (local scripts only)
- Version control (Git) operations
- Text processing tools (grep, rg, awk, sed, etc.)

**Safe System Operations:**
- Process listing and Unity-specific process termination
- Package managers (npm, pip, choco for approved packages)
- Network operations for development resources

#### Dangerous Operations Still Blocked ⚠️
The following operations will **always trigger permission prompts** because they pose security risks:

**System Modifications:**
- Registry edits (`reg *`, `bcdedit *`)
- Service control (`sc *`)
- Privilege escalation (`runas`, `Start-Process -Verb runAs`)

**Destructive File Operations:**
- Mass deletion with wildcards (`rm -rf *`, `del /s /q *`)
- Recursive force removal operations

**Critical Process Management:**
- Terminating system processes (explorer.exe, winlogon.exe, csrss.exe)
- Any process kills outside of Unity instances

**Network Security:**
- Arbitrary script downloads (`curl https://pastebin.com/*`)
- PowerShell web execution (`Invoke-WebRequest * | iex`)

**Version Control Risks:**
- Force pushes (`git push --force`)
- History rewriting (`git filter-branch`)

**Disk Operations:**
- Partitioning (`diskpart`)
- Formatting drives (`format`)

#### Claude Code Modes & Shortcuts
Claude Code has several operational modes that can be toggled:

**Keyboard Shortcuts (when user is in interactive mode):**
- `Shift + Tab` - Cycles through modes: `normal → auto-accept edit → plan mode → back to normal`

**Permission Modes:**
- `default` - Ask for permission on first use of each tool (safe default)
- `acceptEdits` - Auto-allow file edits, still prompt for bash/network operations  
- `bypassPermissions` - Skip all permission prompts ("YOLO mode")

**Current Project Settings:**
- `defaultMode: "acceptEdits"` - File operations are pre-approved
- Comprehensive allow-list covers most Unity development needs
- Dangerous operations still gated for security

#### Tested & Verified Capabilities (2025-07-26)
**✅ WORKING:**
- **File Operations**: Read, Write, Edit, MultiEdit - all function perfectly
- **Unity Process Management**: PowerShell `Stop-Process -Name Unity -Force` works reliably
- **Package Managers**: npm (v11.5.1), chocolatey (v2.4.3) both available
- **Version Control**: git (v2.47.1) fully functional
- **PowerShell**: All PowerShell commands execute successfully
- **Unity Versions**: 6000.0.26f1 and 6000.0.37f1 both installed

**❌ KNOWN ISSUES:**
- `taskkill` commands fail (non-admin VS Code launch from Unity)

#### Best Practices for Future Claude
1. **Trust the allow-list** - Pre-approved operations can be executed without hesitation
2. **Question deny-list patterns** - If you encounter blocked operations, explain why they're dangerous and suggest safer alternatives
3. **Use Plan Mode** for risky operations - Let user review before execution
4. **Document new patterns** - If new tools are needed, add them to this documentation
5. **Remember the `/permissions` command** - User can check current settings with this slash command

### GitHub CLI Setup & PR Automation

#### **Installation & Authentication (One-time setup)**

**Install GitHub CLI:**
```bash
# Install via winget (requires accepting source agreements)
winget install --id GitHub.cli --accept-source-agreements --accept-package-agreements

# Add to system PATH (requires admin privileges)
powershell -Command "Start-Process PowerShell -ArgumentList '-Command', '[Environment]::SetEnvironmentVariable(\"Path\", \$env:Path + \";C:\Program Files\GitHub CLI\", [EnvironmentVariableTarget]::Machine)' -Verb RunAs"
```

**⚠️ Important**: After adding to PATH, restart your terminal or VS Code for the `gh` command to work. Until then, use the full path.

**✅ Once PATH is configured**: You can use `gh` instead of `"C:\Program Files\GitHub CLI\gh.exe"` in all commands throughout this documentation.

**Authentication Options:**
```bash
# Option 1: Web Authentication (recommended for interactive setup)
"C:\Program Files\GitHub CLI\gh.exe" auth login --web

# Option 2: Environment Variable (for automation)
# Set GH_TOKEN environment variable with Personal Access Token
```

**Verify Installation:**
```bash
"C:\Program Files\GitHub CLI\gh.exe" --version
```

#### **PR Creation Workflow**
```bash
# 1. Create and push feature branch
git checkout -b feature/task-name
# [make changes]
git add -A
git commit -m "feat: description"
git push -u origin feature/task-name

# 2. Create PR via CLI
"C:\Program Files\GitHub CLI\gh.exe" pr create --base develop --title "Title" --body "Description"
```

#### **Common Commands for Future Turtle Bros**
```bash
# List PRs
"C:\Program Files\GitHub CLI\gh.exe" pr list

# View PR status
"C:\Program Files\GitHub CLI\gh.exe" pr status

# Check out PR locally
"C:\Program Files\GitHub CLI\gh.exe" pr checkout PR_NUMBER
```

**Note**: Full path required until CLI is added to system PATH. Authentication persists between sessions once configured.

### Security Stance
- **Defensive only**: Analyze code for malicious patterns
- **Refuse** to create, modify, or improve exploitative code
- **Allow**: Security analysis, detection rules, vulnerability explanations, defensive tools
- **Permission model**: Comprehensive development access with dangerous operations gated