# NeonLadder - Unity 2.5D Action Platformer with Roguelite Elements

## Current Game State (January 2025)

**NeonLadder** is a 2.5D action platformer with roguelite elements, featuring procedural level generation and a seven deadly sins theme. The game combines classic platforming mechanics with modern roguelite progression systems in a visually striking neon-lit world. Currently in mature development with most core systems implemented and functional.

### Key Features Implemented ✅
- **Player System**: Full-featured character with health, stamina, dual currency systems
- **Combat**: Melee and ranged attacks with damage numbers and visual effects  
- **Enemy AI**: State machine-based enemies (Minor, Major, Flying variants, Bosses)
- **Procedural Generation**: Seeded path generation for reproducible level layouts
- **Roguelite Progression**: Death carries forward permanent upgrades while resetting run progress
- **Dual Currency Economy**: Meta (temporary per-run) and Perma (persistent) currencies with shop systems
- **Steam Integration**: Achievements and platform features via Steamworks.NET
- **Save/Load System**: Comprehensive player progression persistence across runs
- **Audio/Visual**: Spatial audio, particle effects, modern UI components with neon aesthetic

### Technical Architecture
- **Unity 6** (6000.0.26f1) with Universal Render Pipeline
- **Event-driven simulation system** with custom HeapQueue for efficient scheduling
- **Assembly definitions** for proper code organization and testing
- **Comprehensive testing infrastructure** with 1000+ lines of unit tests
- **Manager pattern** with modular system coordination

## For AI Development Teams - Marvel Scrum Simulation 🦸

### 🚨 REVOLUTIONARY: World's First AI Scrum Team for Solo Developers 🚨

**NeonLadder features a complete Marvel-themed AI Scrum team simulation** - the ultimate collaborative development experience for solo developers. Instead of random AI persona selection, you get **role-based expertise on demand**.

### 🔧 First-Time Setup for Claude Code

**If you don't have `.claude/settings.local.json`, copy from the bootstrap template:**

```bash
# Copy the bootstrap template
copy .claude\settings-bootstrap.json .claude\settings.local.json
```

**Or create manually with this content:**
```json
{
  "external_memory_system": {
    "description": "NeonLadder External Claude Configuration - Load team memory from Google Drive",
    "startup_command": "Claude, load external settings"
  },
  "permissions": {
    "allow": [
      "Bash(\"C:\\tools\\gdrive\" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --destination .claude/settings.local.json --overwrite)",
      "Bash(sync-claude-settings.bat)",
      "Read", "Write", "Edit", "MultiEdit", "Glob", "Grep", "LS", "TodoWrite",
      "Bash(git:*)", "Bash(powershell:*)", "Bash(\"C:\\tools\\gdrive\":*)",
      "Bash(\"C:\\Program Files\\Unity\\Hub\\Editor\\*\")"
    ],
    "deny": []
  }
}
```

**Then immediately run:**
```bash
# This loads the full external configuration with Marvel team system
sync-claude-settings.bat
```

**Or manually:**
```bash
"C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --destination .claude/settings.local.json --overwrite
```

After loading external settings, you'll have access to 8 Marvel personas with complete project context.

**🎯 How It Works:**
```bash
# Need architectural guidance?
"@tony-stark, help architect this new feature"

# Want a code review?
"@sue-storm, review this PR for AC alignment" 

# Creating user stories?
"@jean-grey, break down this epic into stories"

# Setting up CI/CD?
"@wolverine, configure our GitHub Actions pipeline"
```

**🦸 Meet Your Development Team:**

**Avengers (Core Development):**
- `@tony-stark` - Technical Lead / Principal Architect
- `@steve-rogers` - Scrum Master / Team Coordination
- `@bruce-banner` - Senior QA Engineer / Test Strategy
- `@natasha-romanoff` - Security Engineer / Code Auditing

**X-Men (Product & Infrastructure):**
- `@charles-xavier` - Product Owner / Vision Keeper
- `@jean-grey` - Business Analyst / Requirements Specialist
- `@wolverine` - DevOps Engineer / Infrastructure
- `@storm` - UX/UI Designer / Experience Architect

**Fantastic Four (Quality & Innovation):**
- `@sue-storm` - Senior Code Reviewer / Quality Guardian
- `@reed-richards` - R&D Lead / Innovation Specialist
- `@johnny-storm` - Performance Engineer / Optimization
- `@ben-grimm` - Legacy Systems / Technical Debt Fighter

**Guardians (Support & Documentation):**
- `@peter-quill` - Junior Developer / Fresh Perspective
- `@gamora` - Database Specialist / Data Architecture
- `@rocket` - Technical Writer / Documentation Expert

**🧠 Persistent Team Memory:**
Each team member **remembers previous conversations**, learns from project discoveries, and maintains continuity across development sessions. No more explaining the same context repeatedly!

**🤝 Professional Conflict Resolution:**
When team members disagree (Product Owner vs Tech Lead, QA vs Speed), they use **safe-for-work friendly inquisition** to explore alternatives and present options - never blocking progress.

**📋 Automated Story Creation:**
Business Analyst personas can read Google Drive initiative documents and automatically generate proper user stories with acceptance criteria, story points, and technical tasks.

**🚀 Getting Started:**
1. **New Contributors**: Follow project setup below, then read `CLAUDE.md` for development workflows
2. **Claude AI Models**: Load team configuration from `CLAUDE.md` and respond to `@role-name` requests  
3. **Solo Developers**: Use Marvel team personas for genuine collaborative development experience
4. **Open Source Enthusiasts**: Try the world's first AI Scrum team simulation system!

### Quick Start for Contributors

**Technical Details**: See `CLAUDE.md` for complete development workflows, testing infrastructure, and detailed priorities.

**📚 Documentation Hub**: See [`.claude/documentation/README.md`](.claude/documentation/README.md) for comprehensive technical specifications, business analysis, and team documentation.

## 🎯 Current Project Status (January 2025)

### **Active Sprints & Priorities**

**🚨 SPRINT 1: Steam Launch Readiness (Current)**
- **PBI-001**: Fix SaveState Z-position bug during scene transitions (CRITICAL - **@tony-stark**)
- **PBI-002**: Remove per-frame string comparisons in ManagerController (HIGH - **@johnny-storm**)
- **PBI-003**: Cache Quaternion to Euler conversions in Player movement (HIGH - **@johnny-storm**)
- **PBI-004**: Complete Steam achievements implementation (HIGH - **@wolverine**)
- **PBI-005**: Implement Steam Cloud saves (HIGH - **@gamora**)

**📋 Product Backlog Summary**
- **85 total story points** across 4 major epics
- **34 points** committed to Sprint 1 (Steam Launch Readiness)
- **21 stories** in technical debt reduction pipeline
- **10+ PBIs** for Disco Elysium dialog system integration

### **Key Epic Status**
1. **Technical Debt Reduction** (High Priority) - 30% development velocity increase expected
2. **Steam Platform Integration** (High Priority) - Required for Q1 2025 launch
3. **Mobile Platform Expansion** (Medium Priority) - $500K+ revenue potential
4. **Disco Elysium Dialog System** (Medium Priority) - 85 story points, 6 sprints estimated

### **Development Infrastructure Status**
- 🚨 **CLI Testing**: **BLOCKED** - 100+ compilation errors preventing test execution
- ✅ **Google Drive Integration**: 30GB asset management operational  
- ✅ **AI Team Simulation**: 21 Marvel personas with persistent memory
- ⚠️ **GitHub CLI**: Installed, authentication pending
- ⚠️ **Unity CLI**: Ready but blocked by compilation failures

### **🚨 CRITICAL BLOCKERS (Immediate Action Required)**
1. **Missing Save System Files** - 4 critical DataManagement files not found
2. **Namespace Conflicts** - PersonalityTrait and CurrencyType enum conflicts
3. **Missing Dependencies** - DamageNumbersPro, Michsky.MUIP packages missing
4. **Dialog System Integration** - 50+ errors from incomplete Disco Elysium integration
5. **Scene Management API** - Method signature changes breaking existing code

## 📋 **COMPREHENSIVE TODO LIST** (From PBI Analysis)

### **🔥 SPRINT 1: Steam Launch Readiness** (34 Story Points)
**CRITICAL Priority:**
- [ ] **PBI-001**: Fix SaveState Z-position bug during scene transitions (**@tony-stark** - 5 pts)
- [ ] **PBI-DIALOG-001**: Fix compilation errors - namespace conflicts (**@sue-storm** - 2 pts)
- [ ] **PBI-INPUT-001**: Unit test Xbox/PlayStation/Switch controller mappings (**@bruce-banner** - 3 pts)

**HIGH Priority:**
- [ ] **PBI-002**: Remove per-frame string comparisons in ManagerController (**@johnny-storm** - 3 pts)
- [ ] **PBI-003**: Cache Quaternion to Euler conversions in Player movement (**@johnny-storm** - 3 pts)
- [ ] **PBI-004**: Complete Steam achievements implementation (**@wolverine** - 5 pts)
- [ ] **PBI-005**: Implement Steam Cloud saves (**@gamora** - 3 pts)
- [ ] **PBI-006**: Add Steam Rich Presence (**@wolverine** - 2 pts)

**MEDIUM Priority:**
- [ ] **PBI-007**: Implement Heat/Corruption difficulty system (**@stephen-strange** - 8 pts)
- [ ] **PBI-008**: Add ability synergy detection system (**@stephen-strange** - 5 pts)

### **🎯 TECHNICAL DEBT REDUCTION** (High Business Value)
- [ ] **PBI-009**: Break Player ↔ PlayerAction circular dependency (**@sue-storm** - 5 pts)
- [ ] **PBI-010**: Replace ManagerController singleton with service container (**@tony-stark** - 8 pts)  
- [ ] **PBI-011**: Decouple scene-specific manager logic (**@reed-richards** - 5 pts)

### **🎮 DISCO ELYSIUM DIALOG SYSTEM** (85 Story Points, 6 Sprints)
- [ ] **PBI-DIALOG-002**: Integrate Dialogue System for Unity Database (**@jean-grey** - 5 pts)
- [ ] **PBI-DIALOG-003**: Build Enhanced Dialog UI with Consequence Preview (**@storm** - 8 pts)
- [ ] **PBI-DIALOG-004**: Implement Voice Acting Pipeline (**@wolverine** - 13 pts)
- [ ] **PBI-DIALOG-005**: Connect Dialog Rewards to Currency System (**@bruce-banner** - 3 pts)
- [ ] **PBI-DIALOG-006**: Create Dialog Content for All Characters (**@charles-xavier** + **@jean-grey** - 21 pts)
- [ ] **PBI-DIALOG-007**: Implement Save System Integration (**@reed-richards** - 5 pts)
- [ ] **PBI-DIALOG-008**: Performance Optimization for Mobile (**@johnny-storm** - 8 pts)
- [ ] **PBI-DIALOG-009**: QA Test Suite for Dialog System (**@bruce-banner** - 8 pts)
- [ ] **PBI-DIALOG-010**: Dialog Analytics and Telemetry (**@nick-fury** - 5 pts)

### **📱 MOBILE PLATFORM EXPANSION** ($500K+ Revenue Potential)
- [ ] **PBI-012**: Implement touch controls (8 pts)
- [ ] **PBI-013**: Mobile UI/UX optimization (5 pts)
- [ ] **PBI-014**: Performance optimization for mobile (8 pts)

### **💰 MONETIZATION ENHANCEMENT** (25% Revenue Increase)
- [ ] **PBI-015**: Implement cosmetic shop (5 pts)
- [ ] **PBI-016**: Add season pass system (8 pts)
- [ ] **PBI-017**: Create IAP infrastructure (5 pts)

### **🔥 PIZZA PARTY QUICK WINS** (1-Day Tasks)
- [ ] **QW-001**: Extract animation IDs to enum (1 pt)
- [ ] **QW-002**: Add missing null checks (1 pt)
- [ ] **QW-003**: Cache transform references (2 pts)

### **🚀 FUTURE ARCHITECTURE** (Long-term Vision)
- [ ] **TI-001**: Unity 6 URP optimization
- [ ] **TI-002**: Addressables migration  
- [ ] **TI-003**: CI/CD pipeline enhancement

## 🎮 **INPUT SYSTEM TESTING SPECIFICATION**

**PBI-INPUT-001 Details** (**@bruce-banner** assigned):
- **Scope**: `Assets/Resources/Controls/PlayerControls.inputactions` mapping validation
- **Target Actions**: Sprint, Move, Attack, WeaponSwap, Jump, Up (see PlayerAction.cs:207-225)
- **Platforms**: Xbox (XInput), PlayStation (DualShock4/DualSense), Nintendo Switch (Pro Controller)
- **Test Coverage**: Verify each platform controller properly triggers PlayerActionMap functions
- **Validation**: Create automated tests for button press → action mapping consistency
- **Critical**: Multi-platform input validation essential for Q1 2025 Steam launch

**Architecture Highlights:**
- Unity 6 with Universal Render Pipeline
- Event-driven simulation system
- Comprehensive testing with 36+ behavioral tests
- CLI test automation ready for CI/CD

**Development Workflow:**
1. Check `CLAUDE.md` for complete setup instructions
2. Use Marvel team personas for specialized expertise: `@tony-stark`, `@sue-storm`, etc.
3. Follow existing patterns: Manager architecture, Assembly definitions, Event-driven design

## Project Setup
Download and install the latest version of Unity Hub from the [Official Unity Website](https://unity.com/download).
> Scroll down to select the download link for your Operating system (Windows, Linux, Mac)

## How to Clone the Repository
Clone this repository to your local machine.
by either running this command within a Command prompt with git installed

`git clone https://github.com/DakotaIrsik/NeonLadderURP.git `

or by clicking the "Code -> Download" button and extracting the repository. (Note the location you download / clone the repository to)

## Open the Project in Unity Hub
1. Open Unity Hub.
2. Create your Unity Account (Single-Sign-On is easiest, IE Google)
3. Click on `Add` within the Unity Hub and select the directory which contains the repository

## How to Install Unity Editor
1. Unity Hub will prompt you to install the necessary editor if not installed (**Unity 6000.0.26f1**).
2. Uncheck Visual Studio from installed components list (unless you want it)
3. Scroll down and Select the build platform for the operating system you're running on IE if you're on Windows choose "Windows Build Support (IL2CPP)"
> During the installation, UAC may prompt fo the installation, choose yes.

> **Note**: Project has been upgraded to Unity 6. If you have Unity 2022.3.31f1, it may work but could have package compatibility issues.

## How to Open the Project
1. Select Projects from Unity Hub
2. Select The cloned folder (NeonLadderURP if you cloned via a shell, or NeonLadderURP-main if you downloaded and extracted)
3. Wait for the project to load. 
4. If prompted "This project is using the new input system package but the native platform backends for the new input system are not enabled in the player settings. This meants that no input fro native devices will come through, Do you want to enable the backends? Doing so will "RESTART" the editor. Click - YES

Once loaded, open the `SampleScene`:
1. Locate the `Project` tab at the bottom of the Unity Editor (should be preselected)
2. Naviagate to `Scenes/`.
3. Double-click on `SampleScene` to open it.

## Play the Scene
1. Click the `Play` button at the top of the Unity Editor.
2. Notice that this scene appears empty except for lighting.
3. Click the `Play` button again to stop the scene.

## Download Additional Assets
1. In the Project tab's search bos, search for `DownloadInstructions`.
2. Locate the DownloadInstructions.txt file which exists in the SURIYUN folder (you can see the full path of the DownloadInstructions at the bottom of the Editor, inicating the location of the path).
3. Follow the instructions provided in the `DownloadInstructions` files to download the asset.

## Relaunch Unity Editor
Exit the Unity Editor application by clicking the X at the top right of the Unity Editor application.
> **Note:** You MUST RESTART the Unity Editor so that asset metadata refreshes the relationship between the source controlled animation controllers and the package-driven animations themselves, if you don't restart the game will NOT function as intended.
Relaunch the game project from Unity Hub
1. Location the Unity Hub window (this should still be open, as we only closed the Unity Editor) if it was closed, relaunch Unity Hub
2. Select Projects from Unity Hub
3. Select project name (NeonLadderURP if you cloned via a shell, or NeonLadderURP-main if you downloaded and extracted)
 Wait for the project to load.

## Play the Scene (again)
You should now be ready to click the "Play" button once again within the Unity Editor and see the main protagonists idle animations and be able to move left and right with the A/D or <-/-> (arrow keys)

## Conclusion
Repeat Download Additional Assets Steps for all remaining packages. making sure to restart before attempting to "Play" the game
> **Note:** The import process for large packages, like the `LeartesStudios` package (~30 GB), may take HOURS depending on your PC specs.

You should now experience the game as if it were deployed to a specified build platform.

## CLI Testing & Automation

**Status**: ✅ Fully functional CLI test automation system with 36/36 tests passing

The project includes a robust CLI testing system ready for CI/CD integration. For complete testing infrastructure details, automation commands, and performance metrics, see `CLAUDE.md`.

**Quick Test Run:**
```bash
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Code\NeonLadderURP" -executeMethod CLITestRunner.RunPlayModeTests
```

**Key Features:**
- Custom TestRunnerApi workaround for Unity 6's broken `-runTests` flag
- 35-second execution time (Unity startup + compilation + tests)
- Enterprise-level mock infrastructure for Unity component testing
- CI/CD ready with reliable exit codes and XML test output
