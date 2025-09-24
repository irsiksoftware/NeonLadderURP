# NeonLadder - Unity 2.5D Action Platformer with Roguelite Elements

## 🎮 Game Overview

**NeonLadder** is a 2.5D action platformer with roguelite elements, featuring procedural level generation and a seven deadly sins theme. The game combines classic platforming mechanics with modern roguelite progression systems in a visually striking neon-lit world. Currently in mature development with most core systems implemented and functional, targeting Steam launch Q1 2025.

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
- **Unity 6** (6000.2.5f1) with Universal Render Pipeline
- **Event-driven simulation system** with custom HeapQueue for efficient scheduling
- **Assembly definitions** for proper code organization and testing
- **Comprehensive testing infrastructure** with 645 tests (631 passing, 97.8% pass rate)
- **Manager pattern** with modular system coordination
- **🤖 Dual-AI Development System** - Claude + GPT-4o integration for specialized analysis

### 🧠 AI-Assisted Development
**Revolutionary dual-AI system combining Claude's coding expertise with GPT-4o specialized analysis:**

**Usage:** Ask Claude to "ask GPT about [topic]" for:
- Complex game mechanics analysis
- Unity-specific architectural validation  
- Performance optimization strategies
- Technical debt assessment
- Code review assistance

**Example:** `"Claude, ask GPT about our roguelite progression system design"`

## 🧪 Test Reports - Live HTML Coverage

### **📊 Latest Test Results (Interactive HTML)**
- **[🎨 Unity Test Report 2025-07-31](https://htmlpreview.github.io/?https://raw.githubusercontent.com/irsiksoftware/NeonLadderURP/main/TestOutput/unity_test_report_2025-07-31_12-50-04.html)** - Latest main branch report (Interactive HTML with Unity styling)
- **[📁 All Test Reports](./TestOutput/README-TESTOUTPUT.md)** - Complete test documentation and links

**Features:**
- ✅ **Collapsible Test Suites** with visual indicators (✓, ✗, ⚠)
- ✅ **Unity Dark Theme** matching Editor styling
- ✅ **Interactive JavaScript** for smooth UX
- ✅ **Performance Metrics** and test duration tracking
- ✅ **Comprehensive Coverage** of PlayMode, EditMode, and Integration tests

**Automated Generation:** Run `🚀 Run All Tests & Generate Unity-Style Report` in Unity Editor for fresh timestamped reports.

## 🤖 AI Development with Claude Code

### 🚨 REVOLUTIONARY: World's First AI Scrum Team for Solo Developers 🚨

**NeonLadder features a complete Marvel-themed AI Scrum team simulation** - the ultimate collaborative development experience for solo developers. Instead of random AI persona selection, you get **role-based expertise on demand**.

### 🔧 First-Time Setup for Claude Code

**If you don't have `.claude/settings.local.json`, create it with this default template:**

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

**Then load the full external configuration:**
```bash
# Loads Marvel team system and project context
sync-claude-settings.bat

# Or manually:
"C:\tools\gdrive" files download 1yhlUpLoL7FvI2rcQ6B1y9pcUfZKlk0aJ --destination .claude/settings.local.json --overwrite
```

**After loading external settings, you'll have access to specialized Marvel personas:**

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

## 📋 Project Management & Issue Tracking

### **GitHub Issues - Product Backlog Items (PBIs)**

**All development tasks are tracked as GitHub Issues with PBI labels:**

- **📊 [View All PBIs](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3APBI)**
- **🔥 [Critical Issues](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3APBI+label%3AP0-Critical)**
- **⚡ [Steam Launch Items](https://github.com/irsiksoftware/NeonLadderURP/issues?q=is%3Aissue+label%3Asteam-launch)**

**Recent Critical Resolutions:**
- ✅ **Issue #32**: SaveState Z-position bug (Steam launch blocker) - **RESOLVED**
- 🔄 **Issue #33**: Remove per-frame string comparisons in ManagerController
- 🔄 **Issue #34**: Cache Quaternion to Euler conversions in Player movement

### **Current Sprint Status (January 2025)**

**🚨 Steam Launch Readiness Sprint:**
- **Total PBIs**: 73+ issues with full traceability
- **Critical blockers**: Resolved (Issue #32 fixed)
- **Sprint velocity**: 15-20 story points per sprint
- **Target launch**: Q1 2025

**Key Epic Status:**
1. **Steam Platform Integration** - Q1 2025 launch ready
2. **Technical Debt Reduction** - Performance optimization ongoing  
3. **Dialog System Integration** - Enhanced narrative features
4. **Mobile Platform Expansion** - Future revenue opportunity

### **Development Infrastructure Status**
- ✅ **CLI Testing**: Fully operational with 36/36 tests passing
- ✅ **Google Drive Integration**: 30GB asset management operational  
- ✅ **AI Team Simulation**: 21 Marvel personas with persistent memory
- ✅ **GitHub CLI**: Installed and authenticated
- ✅ **Unity CLI**: Ready with automated test execution

### **Development Workflow:**
1. **Review Issues**: Check [GitHub Issues](https://github.com/irsiksoftware/NeonLadderURP/issues) for current PBIs
2. **Load AI Context**: Use `CLAUDE.md` for complete setup instructions and Marvel team personas
3. **Follow Patterns**: Manager architecture, Assembly definitions, Event-driven design
4. **Run Tests**: Ensure all 36 tests pass before committing
5. **Track Progress**: All work managed through GitHub Issues with PBI labels

**Architecture Highlights:**
- Unity 6 with Universal Render Pipeline
- Event-driven simulation system with custom HeapQueue
- Comprehensive automated testing (36+ behavioral tests)
- CLI test automation ready for CI/CD integration
- Marvel AI team simulation for solo development

## 🚀 Getting Started

### Prerequisites

1. **Unity Hub & Editor**
   - Download Unity Hub from [Official Unity Website](https://unity.com/download)
   - Install **Unity 6000.0.26f1** (specific version required)
   - Enable Windows Build Support (IL2CPP) for your platform

2. **Git**
   - Ensure Git is installed for version control
   - GitHub account for issue tracking and contributions

3. **Optional: Claude Code Integration**
   - Google Drive CLI tools for external documentation
   - Claude Code for AI-assisted development

### Project Setup

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

## 🧪 Testing & Quality Assurance

**Status**: ✅ Fully functional CLI test automation system with 36/36 tests passing

### **Automated Testing**
The project includes a robust CLI testing system ready for CI/CD integration:

```bash
# Quick test run
"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe" -batchmode -projectPath "C:\Users\YourName\NeonLadder" -executeMethod CLITestRunner.RunPlayModeTests
```

**Key Features:**
- ✅ Custom TestRunnerApi workaround for Unity 6
- ✅ 35-second execution time (Unity startup + compilation + tests)  
- ✅ Enterprise-level mock infrastructure for Unity component testing
- ✅ CI/CD ready with reliable exit codes and XML test output
- ✅ **[Interactive HTML reports](https://htmlpreview.github.io/?https://raw.githubusercontent.com/irsiksoftware/NeonLadderURP/main/TestOutput/unity_test_report_2025-07-31_12-50-04.html)** with Unity styling

### **Quality Metrics**
- **Test Coverage**: 36 behavioral validation tests
- **Code Quality**: SaveState Z-position bug resolved (Issue #32)
- **Performance**: Frame rate optimization targets identified
- **Documentation**: Complete testing infrastructure in `CLAUDE.md`

## 🤝 Contributing

1. **Check existing issues**: Browse [GitHub Issues](https://github.com/irsiksoftware/NeonLadderURP/issues) for open PBIs
2. **Follow development patterns**: See `CLAUDE.md` for coding standards and workflows
3. **Use AI assistance**: Load Claude Code configuration for specialized team personas
4. **Run tests**: Ensure all tests pass before submitting PRs
5. **Track progress**: All work items managed as GitHub Issues with PBI labels

For detailed development workflows, AI team usage, and technical specifications, see **[CLAUDE.md](./CLAUDE.md)**.
