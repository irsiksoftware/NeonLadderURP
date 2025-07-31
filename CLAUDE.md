# Claude Instructions for NeonLadder Project

## 🚨 FIRST STEP: Check Local Permissions 🚨
**ALWAYS run these commands FIRST when starting work:**
```bash
cat .claude/settings.local.json
cat .claude/extended-state.json
```
The first file contains officially supported Claude permissions. The second contains project context, TMNT personas, GitHub automation, and important development notes.

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

## 📋 External Backlog & PBI System

### **🗂️ NeonLadder Backlog Management (Google Drive)**

**All Product Backlog Items, TODOs, and technical debt organized for sprint planning:**

#### **Quick Access Commands**
```bash
# List all backlog categories
"C:\tools\gdrive" files list --parent 1LJ8X-5whHNaMh2NeHGdzkLwTYgoDjL0p

# Download complete backlog index
"C:\tools\gdrive" files download 1foERIGyhNubkN7HSmxL7Ie6wJb6zSdcg
```

#### **📁 Backlog Categories**

**PBIs (Product Backlog Items)** (Folder ID: `1wJ4jPSB41hdbMcaZc5l7I50IKBzr0zR0`)
- **Procedural Scene Loading PBI** - ID: `18Oz6yXOyVVK-AK4gi9CTufBjmXUbCugJ` (13 story points)

**TODO Items** (Folder ID: `1NyF4XwqS88Wj4K00bym6nUge_IIzhlm5`)
- **URP Configuration TODO** - ID: `1lk6r7HcydUiTOsJ-d0sNpJc-vReL86cm` (Rendering pipeline setup)

**Technical Debt** (Folder ID: `1d9bo8RyXXFW5KYunyB-grKuZLdTvYByR`)
- **Code TODO Analysis** - ID: `1MbgyXSbBbdqV-UyUMBfLWA78XfUf0Rs9` (100+ files analyzed)

#### **🚨 Steam Launch Priority Access**
```bash
# P0 Launch Blockers
"C:\tools\gdrive" files download 18Oz6yXOyVVK-AK4gi9CTufBjmXUbCugJ  # Save system PBI
"C:\tools\gdrive" files download 1lk6r7HcydUiTOsJ-d0sNpJc-vReL86cm  # URP configuration
"C:\tools\gdrive" files download 1MbgyXSbBbdqV-UyUMBfLWA78XfUf0Rs9  # Technical debt priority matrix
```

**Sprint Planning**: Backlog index contains priority matrix and story point estimates for velocity planning

### 🍕 Pizza Party Coding Day - Complete Development Priorities

**🍕 MEGA TODO LIST - 50+ ITEMS FOR EPIC REFACTORING MARATHON! 🐢⚡**

#### **🔥 LEVEL 2: CRITICAL PRIORITIES (Shell-shocking performance wins!)**
- **2a** - Fix SaveState Z-movement bug blocking scene transitions (CRITICAL)
- **2b** - Remove per-frame string comparisons in ManagerController.Update()
- **2c** - Cache Quaternion to Euler conversions in Player movement (10-15% FPS boost)
- **2d** - Break Player ↔ PlayerAction circular dependency cycle
- **2e** - Replace ManagerController singleton pattern with service container
- **2f** - Decouple scene-specific manager logic from ManagerController
- **2g** - Implement proper dependency injection container for managers

#### **🎯 LEVEL 3: MEDIUM PRIORITIES (Game feel boosters!)**
- **3a** - Extract Player animation IDs (walkAnimation=6, etc.) to enum/ScriptableObject
- **3b** - Move hard-coded physics values (attack ranges, timers) to Constants.cs
- **3c** - Extract UI layout values and debug settings to configuration
- **3d** - Replace integer-based animation IDs with string hash system
- **3e** - Implement animation event callbacks for precise timing
- **3f** - Optimize blend tree transitions for smoother movement
- **3g** - Create animation cancellation system for responsive controls
- **3h** - Split KinematicObject.cs (329 lines) into focused components
- **3i** - Separate UI update logic from Player.cs gameplay code
- **3j** - Fix brittle parent/child component dependencies in Player.cs
- **3k** - Clean up PathGenerator dependency issues
- **3l** - Fix memory allocations in PathGenerator LINQ operations
- **3m** - Make RaycastHit buffer size (16) configurable constant
- **3n** - Add unit tests for ManagerController scene switching logic
- **3o** - Create tests for PathGenerator seed determinism and boss placement
- **3p** - Add validation tests for PerformanceProfiler memory allocation patterns
- **3q** - Replace Resources.Load with Addressables in Player.cs controls loading
- **3r** - Add cancellation token support to KinematicObject coroutines
- **3s** - Remove static Simulation.GetModel dependencies for better testability
- **3t** - Build Pipeline Automation - Steam builds, CI/CD

#### **🎨 LEVEL 4: POLISH & MAINTENANCE (Making it maintainable!)**
- **4a** - Implement audio pooling for frequent sounds (jump, ouch, respawn)
- **4b** - Add proper spatial audio configuration for 2.5D gameplay
- **4c** - Create dynamic audio mixing system for gameplay context
- **4d** - Implement event-driven UI updates with proper MVP pattern
- **4e** - Create centralized debug overlay system to replace scattered debug UI
- **4f** - Decouple health/stamina bar updates from Player.cs via events
- **4g** - Standardize Manager vs Controller naming across all classes
- **4h** - Fix inconsistent event naming (noun vs past tense verbs)
- **4i** - Ensure consistent camelCase/PascalCase usage across properties
- **4j** - Add XML documentation to KinematicObject public methods (29% → 100%)
- **4k** - Add XML documentation to Player class properties (15% → 100%)
- **4l** - Add XML documentation to all Manager interfaces (0% → 100%)
- **4m** - Remove unclear comments like 'do we need this?' and 'what a hack'
- **4n** - Remove commented-out code like velocity resets in KinematicObject
- **4o** - Clean up GitHub URL references in code comments
- **4p** - Standardize naming conventions

#### **⚡ LEVEL 5: QUICK WINS (1-day pizza-fueled tasks!)**
- **5a** - QUICK WIN: Extract Player animation IDs to enum (1-day task)
- **5b** - QUICK WIN: Cache frequently accessed transform references
- **5c** - QUICK WIN: Add missing null checks in manager operations
- **5d** - QUICK WIN: Replace string.Format with StringBuilder in hot paths

#### **🚀 LEVEL 6: FUTURE ARCHITECTURE (Long-term vision!)**
- **6a** - FUTURE: Design and implement IGameServices container pattern
- **6b** - FUTURE: Create centralized IEventBus for system decoupling
- **6c** - FUTURE: Build ScriptableObject-based configuration system

**Target**: 15-20 items for pizza party day. Ask user "What's the status on 2c?" for specific item updates.

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

### Unity-Specific Guidelines
- Unity version: Check ProjectSettings/ProjectVersion.txt for the current version
- Test framework: Unity Test Framework (if tests are needed)
- Assembly definitions are already set up in Assets/Scripts/NeonLadder.Scripts.asmdef

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