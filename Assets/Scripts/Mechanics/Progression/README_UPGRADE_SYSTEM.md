# NeonLadder Upgrade System

## 📋 Overview

The NeonLadder Upgrade System is a comprehensive roguelite progression framework supporting both temporary (Meta) and permanent (Perma) currency upgrades. It features a professional Unity Editor tool for designing, validating, and managing upgrades without writing code.

## 🏗️ Architecture

### Core Components

#### 1. **UpgradeSystem** (`Assets/Scripts/Mechanics/Progression/UpgradeSystem.cs`)
- **Purpose**: Main MonoBehaviour managing all upgrade logic
- **Responsibilities**:
  - Purchase validation and currency deduction
  - Prerequisite and mutual exclusion checking
  - Upgrade effect application/removal
  - Meta upgrade resets between runs
  - Event broadcasting for UI updates
- **Dependencies**: Requires Meta and Perma currency components

#### 2. **UpgradeData** (`Assets/Scripts/Mechanics/Progression/UpgradeData.cs`)
- **Purpose**: ScriptableObject defining individual upgrades
- **Creation**: `Create → NeonLadder → Progression → Upgrade`
- **Features**:
  - Multi-level progression with cost scaling
  - Effect system for stat modifications
  - Prerequisites and mutual exclusions
  - Category and currency type assignment

#### 3. **Interfaces** 
- **IUpgrade** (`Assets/Scripts/Mechanics/Progression/IUpgrade.cs`): Contract for upgrades
- **IUpgradeSystem** (`Assets/Scripts/Mechanics/Progression/IUpgradeSystem.cs`): System interface

#### 4. **Enums**
- **UpgradeCategory** (`Assets/Scripts/Mechanics/Progression/UpgradeCategory.cs`): 7 categories
- **CurrencyType**: Meta (temporary) vs Perma (persistent)

### Editor Tools

#### **Upgrade Designer Window** (`Assets/Scripts/Editor/UpgradeSystem/UpgradeSystemEditor.cs`)
- **Access**: `Unity Menu → NeonLadder → Upgrade System → Upgrade Designer`
- **Purpose**: Visual upgrade management and testing tool

## 🎨 Unity Editor Integration

### Upgrade Designer Window

```
┌─────────────────────────────────────────────────┐
│ NeonLadder Upgrade System Designer              │
├─────────────────────────────────────────────────┤
│ [Refresh] ─────────── [Filters ▼] [Create New]  │
├─────────────────────────────────────────────────┤
│ ┌─ Upgrades List ──┐ ┌─ Upgrade Details ──────┐ │
│ │                  │ │                         │ │
│ │ • Damage Boost   │ │ Name: Damage Boost      │ │
│ │   Cost: 50 Meta  │ │ Category: Offense       │ │
│ │   Max Lvl: 3     │ │ Cost: 50 → 75 → 112     │ │
│ │                  │ │                         │ │
│ │ • Vitality       │ │ Effects:                │ │
│ │   Cost: 75 Meta  │ │ - Damage +10% per level │ │
│ │   Max Lvl: 2     │ │                         │ │
│ └──────────────────┘ └─────────────────────────┘ │
├─────────────────────────────────────────────────┤
│ [Create Examples] [Export Tree] [Duplicate]      │
└─────────────────────────────────────────────────┘
```

### Window Features

1. **Upgrade Browser**
   - Search by name or ID
   - Filter by currency type and category
   - Visual indicators (Cyan = Meta, Yellow = Perma)
   - Quick selection and navigation

2. **Detail Inspector**
   - Full upgrade property editing
   - Real-time validation
   - Effect configuration
   - Prerequisite management

3. **Bulk Operations**
   - **Create Example Upgrades**: Generate starter set
   - **Export Upgrade Tree**: Markdown documentation
   - **Duplicate Selected**: Quick variation creation

4. **Play Mode Testing**
   - Test upgrades during gameplay
   - Apply effects temporarily
   - Verify integration

## 📖 Implementation Guide

### Creating Upgrades

#### Via Editor (Recommended)
1. Open Upgrade Designer: `NeonLadder → Upgrade System → Upgrade Designer`
2. Click "Create New" or right-click in Project: `Create → NeonLadder → Progression → Upgrade`
3. Configure properties in the inspector

#### Via Code
```csharp
// Create upgrade asset
var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
upgrade.Id = "speed_boost";
upgrade.Name = "Swift Strike";
upgrade.Description = "Increases movement speed";
upgrade.CurrencyType = CurrencyType.Meta;
upgrade.Category = UpgradeCategory.Utility;
upgrade.BaseCost = 30;
upgrade.MaxLevel = 5;

// Save as asset
AssetDatabase.CreateAsset(upgrade, "Assets/Data/Upgrades/speed_boost.asset");
```

### Using the Upgrade System

#### Setup
```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private UpgradeSystem upgradeSystem;
    [SerializeField] private Meta metaCurrency;
    [SerializeField] private Perma permaCurrency;
    
    void Start()
    {
        // Wire up dependencies
        upgradeSystem.metaCurrency = metaCurrency;
        upgradeSystem.permaCurrency = permaCurrency;
        
        // Subscribe to events
        upgradeSystem.OnUpgradePurchased += HandleUpgradePurchased;
    }
}
```

#### Purchasing Upgrades
```csharp
// Attempt purchase
bool success = upgradeSystem.PurchaseUpgrade("damage_boost", CurrencyType.Meta);

if (success)
{
    Debug.Log("Upgrade purchased successfully!");
}
else
{
    // Check why it failed
    if (!upgradeSystem.CanAffordUpgrade("damage_boost", CurrencyType.Meta))
        Debug.Log("Not enough currency");
    else if (upgradeSystem.GetUpgrade("damage_boost").IsMaxLevel)
        Debug.Log("Already at max level");
}
```

#### Checking Upgrade Status
```csharp
// Check if owned
if (upgradeSystem.HasUpgrade("weapon_mastery"))
{
    // Unlock advanced weapons
    EnableAdvancedWeapons();
}

// Get upgrade details
var upgrade = upgradeSystem.GetUpgrade("health_boost");
Debug.Log($"Current Level: {upgrade.CurrentLevel}/{upgrade.MaxLevel}");
```

#### Meta Upgrade Reset (Between Runs)
```csharp
// Call when starting new run
upgradeSystem.ResetMetaUpgrades();

// Reapply permanent upgrades
upgradeSystem.ApplyUpgradeEffects();
```

## 🎯 Upgrade Categories

```csharp
public enum UpgradeCategory
{
    Offense,    // Damage, crit, attack speed
    Defense,    // Health, armor, resistance  
    Utility,    // Movement, cooldowns, convenience
    Special,    // Unique abilities, synergies
    Core,       // Base stats (Perma upgrades)
    Unlocks,    // New content access (Perma)
    Quality     // Starting bonuses (Perma)
}
```

### Category Guidelines
- **Meta Upgrades**: Use Offense, Defense, Utility, Special
- **Perma Upgrades**: Use Core, Unlocks, Quality
- Categories help with UI organization and filtering

## 💰 Currency System

### Dual Currency Design
- **Meta Currency**: Earned and spent within a single run, resets on death
- **Perma Currency**: Persistent across runs, account-wide progression

### Cost Scaling Formula
```csharp
Cost = BaseCost × 1.5^(Level-1)
```

Examples:
- Level 1: 100 currency
- Level 2: 150 currency (100 × 1.5)
- Level 3: 225 currency (100 × 2.25)
- Level 4: 337 currency (100 × 3.375)

## 🔧 Upgrade Effects

### Effect System
```csharp
[Serializable]
public class UpgradeEffect
{
    public string targetProperty;    // e.g., "maxHealth"
    public float baseValue;         // e.g., 10
    public float perLevelIncrease;  // e.g., 5
    public bool isPercentage;       // true = %, false = flat
}
```

### Effect Examples
```csharp
// Flat health increase
targetProperty: "maxHealth"
baseValue: 20
perLevelIncrease: 10
isPercentage: false
// Result: +20 HP (L1), +30 HP (L2), +40 HP (L3)

// Percentage damage boost
targetProperty: "damageMultiplier"
baseValue: 10
perLevelIncrease: 5
isPercentage: true
// Result: +10% (L1), +15% (L2), +20% (L3)
```

### Implementing Effects
Currently, effects log to console. Integrate with your stat system:

```csharp
public void ApplyUpgradeEffect(UpgradeEffect effect, int level)
{
    float value = effect.baseValue + (effect.perLevelIncrease * (level - 1));
    
    switch (effect.targetProperty)
    {
        case "maxHealth":
            playerStats.maxHealth += value;
            break;
        case "damageMultiplier":
            playerStats.damageMultiplier *= (1 + value / 100f);
            break;
    }
}
```

## 📊 Best Practices

### 1. **Naming Conventions**
- **IDs**: lowercase_with_underscores (e.g., `swift_strike`)
- **Names**: Title Case (e.g., "Swift Strike")
- **Assets**: Match ID (e.g., `swift_strike.asset`)

### 2. **Balance Guidelines**
- **Meta Upgrades**: 3-5 levels max, moderate power increase
- **Perma Upgrades**: 1-10 levels, smaller increments
- **Cost Scaling**: 1.5x multiplier is standard, adjust for balance

### 3. **Prerequisite Design**
```csharp
// Good: Linear progression
"basic_damage" → "advanced_damage" → "master_damage"

// Good: Branching paths
"weapon_training" → "sword_mastery" OR "bow_mastery"

// Avoid: Circular dependencies
"upgrade_a" requires "upgrade_b" requires "upgrade_a"
```

### 4. **Mutual Exclusions**
```csharp
// Example: Class specialization
"warrior_path" excludes ["mage_path", "rogue_path"]
"fire_affinity" excludes ["ice_affinity", "lightning_affinity"]
```

### 5. **Testing Workflow**
1. Create upgrades in Upgrade Designer
2. Use "Create Example Upgrades" for templates
3. Test in Play Mode with the testing button
4. Validate prerequisites and exclusions
5. Export documentation for game designers

## 🎮 Runtime Features

### Context Menu (Debug)
On UpgradeSystem component: `Context Menu → Debug: List All Upgrades`
- Shows all upgrades with ownership status
- Displays current levels and costs
- Useful for debugging progression

### Event System
```csharp
// Subscribe to purchase events
upgradeSystem.OnUpgradePurchased += (upgrade) => {
    Debug.Log($"Purchased: {upgrade.Name}");
    UpdateUI();
    PlayPurchaseSound();
};
```

## 📂 File Structure

```
Assets/
├── Scripts/
│   ├── Mechanics/
│   │   └── Progression/
│   │       ├── UpgradeSystem.cs         # Main system
│   │       ├── UpgradeData.cs           # ScriptableObject
│   │       ├── IUpgrade.cs              # Interface
│   │       ├── IUpgradeSystem.cs        # System interface
│   │       └── UpgradeCategory.cs       # Categories enum
│   └── Editor/
│       └── UpgradeSystem/
│           ├── UpgradeSystemEditor.cs   # Designer window
│           └── ExamplePurchasableItems.cs
└── Data/
    └── Upgrades/                        # Upgrade assets
        ├── Meta/
        │   ├── damage_boost.asset
        │   ├── health_boost.asset
        │   └── speed_boost.asset
        └── Perma/
            ├── base_health.asset
            ├── weapon_mastery.asset
            └── starting_gold.asset
```

## 🚨 Common Issues & Solutions

### Upgrades Not Appearing
- Refresh the Upgrade Designer window
- Check if assets are in a Resources folder
- Verify UpgradeData type in asset search

### Purchase Failing
- Check currency amounts
- Verify prerequisites are met
- Ensure no mutual exclusions
- Check if already at max level

### Effects Not Applying
- Implement effect handlers in your stat system
- Verify target GameObjects have required components
- Check effect values and calculations

## 🔍 Validation Checklist

The Upgrade Designer provides automatic validation for:
- ✓ Required fields (ID, Name)
- ✓ Positive costs and max levels
- ✓ Circular dependency detection
- ✓ Valid category assignments
- ✓ Unique upgrade IDs

## 💡 Advanced Features

### Custom Validation
```csharp
[MenuItem("NeonLadder/Upgrade System/Validate All Upgrades")]
public static void ValidateAllUpgrades()
{
    var upgrades = LoadAllUpgrades();
    foreach (var upgrade in upgrades)
    {
        ValidateUpgrade(upgrade);
    }
}
```

### Batch Operations
```csharp
// Adjust all Meta upgrade costs by 20%
foreach (var upgrade in metaUpgrades)
{
    upgrade.BaseCost = Mathf.RoundToInt(upgrade.BaseCost * 1.2f);
    EditorUtility.SetDirty(upgrade);
}
```

### Integration Examples
```csharp
// Steam Achievements
upgradeSystem.OnUpgradePurchased += (upgrade) => {
    if (upgrade.Id == "true_ending_unlock")
        SteamAchievements.Unlock("TRUE_ENDING");
};

// Analytics
upgradeSystem.OnUpgradePurchased += (upgrade) => {
    Analytics.TrackEvent("upgrade_purchased", new {
        upgrade_id = upgrade.Id,
        level = upgrade.CurrentLevel,
        currency = upgrade.CurrencyType
    });
};
```

## 📈 Benefits

- **Designer-Friendly**: No coding required for basic upgrades
- **Validation**: Catches configuration errors early
- **Scalable**: Supports hundreds of upgrades efficiently
- **Flexible**: Easy to extend with custom effects
- **Professional**: Full editor tooling and documentation
- **Roguelite-Ready**: Built for meta-progression games

---

*The NeonLadder Upgrade System - Because spreadsheet balance belongs in Unity! 🚀*