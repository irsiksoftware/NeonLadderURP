# NeonLadder Centralized Logging System

## 📋 Overview

The NeonLadder Logging System provides a centralized, category-based logging framework with runtime toggle capabilities, performance monitoring, and an in-game debug overlay. It's designed to replace scattered `Debug.Log` calls with a unified, filterable logging solution.

## 🏗️ Architecture

### Core Components

#### 1. **LoggingManager** (`Assets/Scripts/Debug/LoggingManager.cs`)
- **Purpose**: Central logging service that processes all log messages
- **Features**:
  - Master on/off toggle (disabled by default)
  - Category-based filtering
  - Log level filtering (Debug, Info, Warning, Error, Critical)
  - Performance metrics tracking
  - File logging with rotation
  - In-game debug overlay (F12 to toggle)
- **Location**: Add to scene as GameObject with LoggingManager component

#### 2. **LoggingSystemConfig** (`ScriptableObject`)
- **Purpose**: Configuration container for logging settings
- **Location**: `Assets/Resources/Default Logging Config.asset`
- **Features**:
  - Enable/disable logging globally
  - Set minimum log level
  - Configure category filters
  - Enable/disable features (overlay, file logging, Unity console)
  - Presets for different environments (Development, QA, Production)

#### 3. **NLDebug** (`Assets/Scripts/Debug/NLDebug.cs`)
- **Purpose**: Static API for logging throughout the codebase
- **Usage**: Direct replacement for `Debug.Log` with category support
- **Example**: `NLDebug.Info(LogCategory.Player, "Player spawned");`

#### 4. **LoggingSystemSetup** (`Assets/Scripts/Debug/Editor/LoggingSystemSetup.cs`)
- **Purpose**: Unity Editor utilities and menu items
- **Location**: Editor-only script providing menu integration

### Supporting Classes

- **LogCategory** (`Assets/Scripts/Debug/LogCategory.cs`): Enum defining 16 log categories
- **LogLevel** (`Assets/Scripts/Debug/LogLevel.cs`): Enum for log severity levels
- **DebugOverlay** (`Assets/Scripts/Debug/DebugOverlay.cs`): In-game UI for viewing logs
- **PerformanceMonitor** (`Assets/Scripts/Debug/PerformanceMonitor.cs`): FPS and memory tracking

## 🎯 Unity Menu Items

Access these via the Unity menu bar under `NeonLadder → Debug`:

### Menu Items (in order of appearance)

1. **Create Logging System Config** (priority 100)
   - Creates the ScriptableObject configuration file
   - Auto-creates Resources folder if needed
   - Selects and pings the created asset

2. **Setup Complete Logging System** (priority 101)
   - One-click setup: creates config + LoggingManager GameObject
   - Assigns config to manager automatically
   - Best starting point for new scenes

3. **Quick Enable Logging** (priority 102)
   - Instantly enables logging (when currently disabled)
   - Grays out when logging is already enabled

4. **Quick Disable Logging** (priority 103)
   - Instantly disables logging (when currently enabled)
   - Grays out when logging is already disabled

5. **Show Current Logging Status** (priority 150)
   - Displays comprehensive system status in console
   - Shows config state, enabled categories, and manager presence

6. **Open Logging Documentation** (priority 200)
   - Outputs detailed usage guide to console
   - Quick reference for API and features

## 📖 Usage Guide

### Quick Start

1. **Initial Setup**:
   ```
   Unity Menu → NeonLadder → Debug → Setup Complete Logging System
   ```

2. **Enable Logging**:
   ```
   Unity Menu → NeonLadder → Debug → Quick Enable Logging
   ```

3. **In Your Code**:
   ```csharp
   using NeonLadder.Debugging;
   
   // Instead of Debug.Log
   NLDebug.Info(LogCategory.Player, "Player spawned at position", transform.position);
   
   // Different severity levels
   NLDebug.Warning(LogCategory.Combat, "Low health warning");
   NLDebug.Error(LogCategory.SaveSystem, "Failed to save game");
   
   // Performance tracking
   NLDebug.Performance("LoadTime", loadTime, "ms");
   ```

### Log Categories

```csharp
public enum LogCategory
{
    General,              // Default category
    Player,               // Player actions, state
    Enemy,                // Enemy behavior, AI
    Combat,               // Combat system, damage
    UI,                   // UI interactions, menus
    Audio,                // Sound system, music
    SaveSystem,           // Save/load operations
    Progression,          // Upgrades, unlocks
    Performance,          // FPS, memory, profiling
    Networking,           // Multiplayer, connections
    AI,                   // AI decisions, pathfinding
    Packages,             // Third-party packages and plugins
    Physics,              // Physics interactions
    Animation,            // Animation events
    ProceduralGeneration, // Level generation
    Steam,                // Steam integration
    Dialog                // Dialog system
}
```

### Log Levels

```csharp
public enum LogLevel
{
    Debug = 0,    // Detailed diagnostic info
    Info = 1,     // General information
    Warning = 2,  // Potential issues
    Error = 3,    // Recoverable errors
    Critical = 4  // Severe errors
}
```

## 🛠️ Best Practices

### 1. **Category Selection**
- Use the most specific category available
- Default to `General` only when no other category fits
- Consider adding new categories for major systems

### 2. **Log Level Guidelines**
- **Debug**: Detailed info for development (e.g., "Entering state machine state: Idle")
- **Info**: Important events (e.g., "Level loaded: Forest_01")
- **Warning**: Potential issues (e.g., "Enemy spawn point blocked")
- **Error**: Failures that can be recovered (e.g., "Failed to load texture")
- **Critical**: Game-breaking issues (e.g., "Save file corrupted")

### 3. **Performance Considerations**
- Logging is disabled by default in builds
- Use conditional compilation for debug-only logs:
  ```csharp
  #if UNITY_EDITOR
  NLDebug.Debug(LogCategory.AI, "Detailed pathfinding info");
  #endif
  ```

### 4. **Migration from Debug.Log**
```csharp
// Old
Debug.Log("Player took damage: " + damage);

// New
NLDebug.Info(LogCategory.Combat, "Player took damage", damage);

// Old
Debug.LogError("Failed to save!");

// New
NLDebug.Error(LogCategory.SaveSystem, "Failed to save game");
```

### 5. **Configuration Presets**
- **Development**: All categories enabled, Debug level, overlay on
- **QA Testing**: Most categories, Info level, file logging on
- **Production**: Minimal logging, Error level only

## 🎮 Runtime Features

### Debug Overlay (F12)
- Real-time log viewer in-game
- Filter by category and level
- Clear logs button
- Performance metrics display
- Adjustable window size

### File Logging
- Logs saved to: `[Persistent Data Path]/Logs/`
- Automatic rotation (1MB max file size)
- ISO 8601 timestamps
- Category and level included

### Performance Monitoring
- FPS counter with averages
- Memory usage tracking
- Custom performance metrics
- Automatic alerts for performance drops

## 🔧 Advanced Features

### Custom Implementation
```csharp
// Direct LoggingManager usage
LoggingManager.Instance.LogWithContext(
    LogLevel.Info,
    LogCategory.Player,
    "Complex message",
    gameObject,
    additionalData: new { health = 100, score = 5000 }
);

// Conditional logging
if (LoggingManager.Instance.ShouldLog(LogLevel.Debug, LogCategory.AI))
{
    // Expensive operation only when logging
    var debugInfo = GatherExpensiveDebugInfo();
    NLDebug.Debug(LogCategory.AI, debugInfo);
}
```

### Integration Points
- **SaveSystem**: Automatic integration with TimedLogger
- **Performance**: Links to PerformanceProfiler
- **UI**: Debug overlay uses Unity UI system
- **Editor**: Full integration with Unity menus

## 📂 File Locations

```
Assets/
├── Scripts/
│   └── Debug/
│       ├── LoggingManager.cs          # Core manager
│       ├── LoggingSystemConfig.cs     # ScriptableObject config
│       ├── NLDebug.cs                 # Static API
│       ├── LogCategory.cs             # Categories enum
│       ├── LogLevel.cs                # Severity levels
│       ├── DebugOverlay.cs            # In-game UI
│       ├── PerformanceMonitor.cs      # FPS/Memory tracking
│       └── Editor/
│           └── LoggingSystemSetup.cs  # Menu items & utilities
└── Resources/
    └── Default Logging Config.asset   # Configuration file
```

## 🚨 Important Notes

1. **Disabled by Default**: Logging starts disabled to avoid performance impact
2. **Backward Compatible**: Works alongside existing Debug.Log calls
3. **No Null Checks Needed**: LoggingManager handles null references gracefully
4. **Thread Safe**: Can be called from background threads
5. **Zero Allocation Mode**: Available for performance-critical sections

## 🔍 Troubleshooting

### Logs Not Appearing
1. Check if logging is enabled: `NeonLadder → Debug → Show Current Logging Status`
2. Verify category is enabled in config
3. Check minimum log level setting

### Performance Impact
1. Disable file logging for better performance
2. Increase minimum log level to reduce output
3. Disable debug overlay when not needed

### Missing LoggingManager
Run `NeonLadder → Debug → Setup Complete Logging System` to create all required components.

## 📊 Benefits

- **Centralized Control**: Enable/disable all logging with one toggle
- **Categorization**: Filter noise, focus on relevant systems
- **Production Ready**: Different configurations for dev/QA/production
- **Performance Aware**: Minimal overhead when disabled
- **Developer Friendly**: Simple API, helpful menu items
- **Debugging Power**: In-game overlay for live troubleshooting

---

*The NeonLadder Logging System - Because `Debug.Log` spam is so 2019! 🚀*