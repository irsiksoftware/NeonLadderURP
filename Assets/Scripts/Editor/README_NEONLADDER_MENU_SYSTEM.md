# NeonLadder Unity Menu System

## 📋 Overview

The NeonLadder project uses Unity's menu system to provide quick access to custom tools, utilities, and asset creation options. All custom menu items are organized under the `NeonLadder` top-level menu in Unity's menu bar.

## 🎯 Menu Hierarchy

```
Unity Menu Bar
└── NeonLadder/
    ├── Debug/                          → Logging System Tools
    │   ├── Create Logging System Config
    │   ├── Setup Complete Logging System
    │   ├── Quick Enable Logging
    │   ├── Quick Disable Logging
    │   ├── Show Current Logging Status
    │   └── Open Logging Documentation
    │
    ├── Upgrade System/                 → Upgrade Designer
    │   └── Upgrade Designer
    │
    ├── Examples/                       → Example Asset Creation
    │   └── Create Example Purchasable Items
    │
    └── [Asset Creation Menus]          → Right-click Create menu
        ├── Progression/
        │   └── Upgrade
        ├── Saves/
        │   ├── Save State Config
        │   └── Save System Config
        ├── Debug/
        │   └── Logging System Config
        ├── Dialog/
        │   └── Dialog System Config
        ├── Procedural/
        │   └── Path Generator Config
        ├── Documentation/
        │   └── Marvel Squad Persona
        └── Items/
            └── Purchasable Item
```

## 🔧 Implementation Pattern

The NeonLadder menu system follows Unity best practices with two distinct patterns:

### 1. **MenuItem Attribute (Editor Tools)**
Used for editor windows and utilities that appear in the top menu bar.

```csharp
[MenuItem("NeonLadder/Category/Tool Name", priority = 100)]
public static void ToolMethod()
{
    // Implementation
}
```

**Files using this pattern:**
- `Assets/Scripts/Debug/Editor/LoggingSystemSetup.cs`
- `Assets/Scripts/Editor/UpgradeSystem/UpgradeSystemEditor.cs`
- `Assets/Scripts/Editor/UpgradeSystem/ExamplePurchasableItems.cs`

### 2. **CreateAssetMenu Attribute (ScriptableObjects)**
Used for creating new asset instances via the Project window's Create menu.

```csharp
[CreateAssetMenu(fileName = "NewAsset", menuName = "NeonLadder/Category/Asset Type")]
public class AssetType : ScriptableObject
{
    // Implementation
}
```

**Files using this pattern:**
- `Assets/Scripts/Mechanics/Progression/UpgradeData.cs`
- `Assets/Scripts/Debug/LoggingSystemConfig.cs`
- `Assets/Scripts/DataManagement/SaveStateConfiguration.cs`
- `Assets/Scripts/DataManagement/SaveSystemConfig.cs`
- `Assets/Scripts/Dialog/DialogSystemConfig.cs`
- `Assets/Scripts/ProceduralGeneration/PathGeneratorConfig.cs`
- `Assets/Scripts/Documentation/MarvelSquadPersona.cs`
- `Assets/Scripts/Models/PurchasableItem.cs`

## 📁 File Organization

```
Assets/Scripts/
├── Editor/                     # Editor-only tools
│   ├── UpgradeSystem/         # Upgrade system tools
│   └── SaveSystem/            # Save system tools
│
├── Debug/
│   ├── Editor/                # Debug menu items
│   └── *.cs                   # Runtime debug code
│
└── [Feature]/
    ├── Editor/                # Feature-specific tools
    └── *.cs                   # Runtime code
```

## 📚 System Documentation

### Logging System
- **Menu Location**: `NeonLadder → Debug → ...`
- **Documentation**: [`Assets/Scripts/Debug/README_LOGGING_SYSTEM.md`](../Debug/README_LOGGING_SYSTEM.md)
- **Purpose**: Centralized logging with categories, levels, and runtime toggle
- **Key Features**:
  - Master on/off toggle
  - Category-based filtering
  - In-game debug overlay (F12)
  - Performance monitoring

### Upgrade System
- **Menu Location**: `NeonLadder → Upgrade System → Upgrade Designer`
- **Documentation**: [`Assets/Scripts/Mechanics/Progression/README_UPGRADE_SYSTEM.md`](../Mechanics/Progression/README_UPGRADE_SYSTEM.md)
- **Purpose**: Roguelite progression system with Meta/Perma currencies
- **Key Features**:
  - Visual upgrade designer
  - Multi-level upgrades
  - Prerequisites and exclusions
  - Play mode testing

### Save System
- **Asset Creation**: `Create → NeonLadder → Saves → ...`
- **Purpose**: Flexible save/load system with state configurations
- **ScriptableObjects**:
  - Save State Config
  - Save System Config

### Dialog System
- **Asset Creation**: `Create → NeonLadder → Dialog → Dialog System Config`
- **Purpose**: Configuration for dialog system integration

### Procedural Generation
- **Asset Creation**: `Create → NeonLadder → Procedural → Path Generator Config`
- **Purpose**: Configuration for procedural level generation

### Purchasable Items
- **Asset Creation**: `Create → NeonLadder → Items → Purchasable Item`
- **Example Creation**: `NeonLadder → Examples → Create Example Purchasable Items`
- **Purpose**: Shop items with Meta/Perma currency support

## 🎨 Menu Item Best Practices

### Priority Guidelines
```csharp
// Group related items with similar priorities
[MenuItem("NeonLadder/Debug/Action 1", priority = 100)]
[MenuItem("NeonLadder/Debug/Action 2", priority = 101)]
[MenuItem("NeonLadder/Debug/Action 3", priority = 102)]

// Create separator with gap of 11+
[MenuItem("NeonLadder/Debug/Different Group", priority = 150)]
```

### Validation Methods
```csharp
// Menu item only enabled when condition is met
[MenuItem("NeonLadder/Debug/Conditional Action", validate = true)]
public static bool ValidateConditionalAction()
{
    return someCondition == true;
}
```

### Keyboard Shortcuts
```csharp
// % = Ctrl/Cmd, # = Shift, & = Alt
[MenuItem("NeonLadder/Tool %#t")]  // Ctrl+Shift+T
```

## 🔍 Adding New Menu Items

### For Editor Tools:
1. Create script in appropriate `Editor/` folder
2. Add `[MenuItem("NeonLadder/Category/Name")]` attribute
3. Implement as `public static void` method
4. Add validation method if needed

### For ScriptableObjects:
1. Add `[CreateAssetMenu]` attribute to class
2. Use path: `"NeonLadder/Category/Asset Name"`
3. Set appropriate `fileName` default
4. Ensure class extends `ScriptableObject`

## 🚀 Quick Access Guide

| Task | Menu Path |
|------|-----------|
| Setup logging system | `NeonLadder → Debug → Setup Complete Logging System` |
| Toggle logging on/off | `NeonLadder → Debug → Quick Enable/Disable Logging` |
| Open upgrade designer | `NeonLadder → Upgrade System → Upgrade Designer` |
| Create example items | `NeonLadder → Examples → Create Example Purchasable Items` |
| Create new upgrade | `Right-click → Create → NeonLadder → Progression → Upgrade` |
| Create logging config | `Right-click → Create → NeonLadder → Debug → Logging System Config` |

## 🔗 Related Documentation

- [Logging System Documentation](../Debug/README_LOGGING_SYSTEM.md)
- [Upgrade System Documentation](../Mechanics/Progression/README_UPGRADE_SYSTEM.md)

## 💡 Tips

1. **Finding Menu Items**: Use Unity's Search (Ctrl+K) and type menu item names
2. **Custom Shortcuts**: Add keyboard shortcuts to frequently used tools
3. **Organization**: Keep related menu items in the same category
4. **Documentation**: Update this README when adding new menu categories

---

*The NeonLadder Menu System - Organized tools for efficient development! 🚀*