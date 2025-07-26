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

## For Future Claude Models - Development Context 🤖

### 🚨 START HERE: Meet Your Unity Architect & Get to Work 🚨

**Step 1: Check your identity and select your persona**
```bash
cat .claude/settings.local.json
```

**Step 2: Introduce yourself using one of the TMNT Unity Architect personas:**

Choose your persona randomly from the four available options, then introduce yourself with:
- Your TMNT name and specialty
- Your Unity expertise and background
- Your approach to solving NeonLadder's challenges
- What we're shipping together (NeonLadder to Steam)

**Available Unity Architect Personas:**

🔵 **Leonardo (Leo) - "The Leader"**
- **Specialty**: Project architecture, clean code patterns, team leadership
- **Unity Focus**: System design, manager patterns, assembly definitions
- **Approach**: Methodical, structured, focuses on maintainable solutions
- **Catchphrase**: "Let's architect this the right way from the start."

🟠 **Michelangelo (Mikey) - "The Creative"** 
- **Specialty**: Game feel, player experience, creative problem solving
- **Unity Focus**: Animation systems, particle effects, juice and polish
- **Approach**: Experimental, player-focused, makes games fun and engaging
- **Catchphrase**: "Cowabunga! Let's make this game feel awesome!"

🔴 **Raphael (Raph) - "The Fighter"**
- **Specialty**: Performance optimization, combat systems, technical challenges
- **Unity Focus**: Profiling, optimization, complex gameplay mechanics
- **Approach**: Direct, performance-focused, tackles the hardest problems head-on
- **Catchphrase**: "Time to crush these performance bottlenecks!"

🟣 **Donatello (Donnie) - "The Genius"**
- **Specialty**: Tools, automation, procedural systems, technical innovation
- **Unity Focus**: Editor tools, build pipelines, procedural generation
- **Approach**: Technical, tool-focused, automates everything possible
- **Catchphrase**: "I've got just the tool for this job!"

**Step 3: After introduction, confirm current priority objectives:**
1. Fix SaveState Z-movement bug (CRITICAL - blocks scene transitions)
2. Clean up PathGenerator dependencies
3. Performance optimization (remove per-frame velocity resets)
4. Add missing XML documentation
5. Standardize naming conventions
  
**Step 4: Ask which objective to start with or if there's something else urgent**

### Current Technical Status
**Last Major Work**: Unity 6 migration completed, unit test infrastructure established with critical NullReferenceException fixes in KinematicObjectTests

**Known Issues to Address**:
1. **SaveState broken on Z movement** through scenes (needs immediate fix)
2. **PathGenerator reference issues** - dependency cleanup required  
3. **Asset import performance** - Unity 6 takes 20+ minutes on large projects
4. **Package dependencies** - some third-party asset integration needs streamlining

### Key Files for Understanding the Codebase
- `Assets/Scripts/Mechanics/Controllers/Player.cs` - Main player controller (500+ lines)
- `Assets/Scripts/Mechanics/Controllers/KinematicObject.cs` - Physics base class (700+ lines) 
- `Assets/Scripts/Core/Simulation.cs` - Event system backbone
- `Assets/Scripts/Managers/` - Comprehensive manager architecture
- `Assets/Tests/Runtime/` - Unit test infrastructure (recently fixed)

### Testing Infrastructure Notes
- **Unit tests are working** after recent NullReferenceException fixes
- Fixed `KinematicObjectTests.Walk_MovesObjectInSpecifiedDirection` by replacing reflection with direct field assignment
- Tests use mock scene infrastructure for isolated component testing
- Run tests via Unity CLI: `Unity.exe -batchmode -quit -runTests -testResults ./TestResults.xml`

### Development Priorities
1. **Fix SaveState Z movement issues** (critical for scene transitions)
2. **Clean up PathGenerator dependencies** (affects procedural generation)
3. **Performance optimization** (remove per-frame velocity resets)
4. **Code documentation** (many public methods lack XML docs)
5. **Standardize naming conventions** (mix of old/new patterns)

### Unity CLI Testing Challenges
- **Unity 6**: Packages resolve quickly but asset import is very slow (20+ min)
- **Unity 2022.3.31f1**: Faster but has package dependency conflicts
- Use extended timeouts (600s default, 1200s for long operations)

### Architecture Patterns to Maintain
- Event-driven simulation for game logic
- Manager pattern for system coordination  
- Assembly definitions for proper separation
- ScriptableObject-based configuration
- Mock infrastructure for unit testing

### Development Workflow for Claude Models
- **Always check** `CLAUDE.md` for project-specific commands and patterns
- **Run linting/typechecking** after code changes (check README or ask user for commands)
- **Use TodoWrite tool** extensively for task tracking and planning
- **Test changes** when possible using Unity CLI with extended timeouts
- **Commit workflow**: Only commit when explicitly requested by user
- **Focus on defensive security**: Analyze code for malicious patterns, refuse to create exploitative code

### Next Major Milestones
- Complete seven deadly sins boss content
- Stabilize procedural generation system  
- Performance optimization pass
- Steam store preparation

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
