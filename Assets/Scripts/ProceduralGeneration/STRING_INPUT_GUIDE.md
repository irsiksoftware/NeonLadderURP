# 🎮 NeonLadder String Input & Generation Rules Guide
*By Stephen Strange, Master of Procedural Arts*

> "Every string contains infinite possibilities. Our rules ensure those possibilities remain playable."

## 🔮 Overview: From String to Adventure

The NeonLadder PathGenerator transforms **any string input** from your title screen into a complete, balanced roguelite adventure. This guide explains how string inputs work, what rules govern generation, and how to configure everything visually.

## 📝 String Input Rules & Flexibility

### ✨ **What Strings Are Accepted?**

**ANYTHING!** The system accepts any string input:

```csharp
// All of these work perfectly:
"MyAwesomeSeed123"           // Alphanumeric  
"🎮🔮✨🌟💫"                  // Emojis & Unicode
"!@#$%^&*()_+-="             // Special characters
"A very long sentence with spaces and MIXED case 123456" // Mixed content
"x"                          // Single character
""                           // Empty (generates random seed)
```

### 🎯 **How String Input Works**

```mermaid
graph LR
    A[User Input String] --> B[SHA256 Hash]
    B --> C[Deterministic Seed]
    C --> D[Apply Generation Rules]
    D --> E[Validate Against Rules]
    E --> F[Complete Map]
```

1. **Any String** → SHA256 hash → **Deterministic Integer**
2. **Rules Applied** → Guaranteed encounters added
3. **Validation** → Ensure playable experience
4. **Final Map** → Ready for gameplay

## ⚖️ Generation Rules: Slay the Spire Style Guarantees

### 🛡️ **Core Rule Philosophy**

Just like Slay the Spire, our system ensures every generated path is:
- **Playable**: Always possible to complete
- **Balanced**: Fair risk/reward progression  
- **Varied**: Different each time, but structured
- **Predictable**: Same seed = identical experience

### 📋 **Rule Categories**

#### **🏗️ Structure Rules**
```csharp
// Path Layout
minPathsPerLayer = 3        // At least 3 choice paths per layer
maxPathsPerLayer = 5        // At most 5 paths (prevent choice paralysis)
minNodesPerPath = 3         // Minimum encounters per path
maxNodesPerPath = 6         // Maximum path length

// Layer Progression  
pathsGrowWithDepth = true   // Later layers get longer
moreChoicesInLaterLayers = true  // More paths in deeper layers
```

#### **⚔️ Combat Guarantee Rules**
```csharp
// Required Encounters
guaranteedCombatPerLayer = 1         // Must fight at least once per layer
guaranteedMinorEnemiesPerLayer = 1   // At least 1 minor enemy encounter
maxMajorEnemiesPerLayer = 2          // Prevent overwhelming difficulty

// Balance Protection
enforcePathBalance = true            // Each path has fair risk/reward
```

#### **🏪 Service Guarantee Rules**
```csharp
// Player Support
guaranteedRestShopPerLayer = true    // Always have recovery opportunity
restShopBeforeBoss = true           // Never fight boss without prep chance

// Progression Support
difficultyScalesWithDepth = true    // Gradual difficulty increase
```

#### **🎲 Variety & Events Rules**
```csharp
// Event Distribution
baseEventChance = 0.3f              // 30% base chance for events
eventChanceScalesWithDepth = true   // More events in later layers
maxEventsPerLayer = 2               // Prevent event overload

// Pacing Rules
preventAdjacentSameType = true      // No boring repetition
allowSeedRuleVariation = true       // 10% flexibility for variety
```

## 🎛️ Visual Configuration System

### 📁 **PathGenerator ScriptableObject**

Create and configure generation rules visually:

1. **Right-click in Project** → `Create` → `NeonLadder` → `Procedural Generation` → `Path Generator Config`
2. **Name your config**: `MyCustomRules.asset`
3. **Configure visually** in the Inspector

### 🔧 **Configuration Interface**

```csharp
[Header("🔮 Mystical Configuration")]
public string configurationName = "My Custom Rules";
public string description = "Describe your rule set purpose";

[Header("📜 Generation Rules")]  
public GenerationRules rules;    // Visual rule configuration

[Header("🧪 Testing & Validation")]
public List<string> testSeeds;   // Seeds to test with
public bool autoValidateOnChange = true; // Auto-test when rules change
```

### 🎮 **Built-in Presets**

#### **⚖️ Balanced (Recommended)**
```csharp
// Slay the Spire inspired - reliable progression
- 3-4 paths per layer, 3-5 nodes each
- Guaranteed: 1 combat, 1 minor enemy, 1 rest/shop
- 25% event chance, scaling with depth
- Rest/shop always before boss
```

#### **🌪️ Chaotic (High Risk)**
```csharp
// Unpredictable and challenging
- 2-6 paths per layer, 2-7 nodes each
- Minimal guarantees, up to 4 major enemies
- 40% event chance, more variety
- Rules can be bent 30% of the time
```

#### **🛡️ Safe (Guaranteed Progress)**
```csharp
// Conservative, always completable  
- Exactly 3 paths, 4 nodes each
- Guaranteed rest/shop, max 1 major enemy
- Lower event chance, predictable structure
- No rule flexibility, strict enforcement
```

## 🧪 Testing & Validation System

### ✅ **Automatic Validation**

Every configuration automatically validates:

```csharp
[ContextMenu("🔍 Validate Configuration")]
public ValidationStats ValidateConfiguration()
{
    // Tests all seed strings against rules
    // Reports success rate and violations
    // Provides detailed feedback
}
```

### 📊 **Test Results**

```
🔮 Validating PathGenerator Config: My Custom Rules
✅ Seed 'TestSeed123': Valid
✅ Seed 'AnotherSeed': Valid  
⚠️ Seed 'ChaoticSeed': 2 violations
   • Layer 2 has 0 minor enemy encounters, minimum required: 1
   • Layer 4 missing guaranteed rest/shop opportunity

📊 Validation Summary for My Custom Rules:
   ✅ Valid: 8/10 (80.0%)
   ⚠️ Invalid: 2
   ❌ Errors: 0
```

### 🗺️ **Map Preview System**

```csharp
[ContextMenu("🗺️ Generate Preview Maps")]
public void GeneratePreviewMaps()
{
    // Generates sample maps for inspection
    // Shows node distribution and structure
    // Validates rule compliance visually
}
```

## 🎯 How to Use in Your Game

### 1. **Title Screen Integration**

```csharp
public class TitleScreenManager : MonoBehaviour
{
    [SerializeField] private PathGeneratorConfig gameRules;
    [SerializeField] private TMP_InputField seedInputField;
    
    public void StartNewGame()
    {
        string playerSeed = seedInputField.text;
        
        var generator = new PathGenerator();
        var gameMap = generator.GenerateMapWithRules(playerSeed, gameRules.rules);
        
        // Store map for gameplay
        GameManager.Instance.SetCurrentMap(gameMap);
        
        // Load first layer
        SceneManager.LoadScene("MapSelection");
    }
}
```

### 2. **Runtime Map Navigation**

```csharp
public class MapNavigationManager : MonoBehaviour  
{
    private MysticalMap currentMap;
    private int currentLayerIndex = 0;
    
    public void PlayerChoosesPath(int pathIndex)
    {
        var currentLayer = currentMap.Layers[currentLayerIndex];
        var pathNodes = currentLayer.Nodes
            .Where(n => n.PathIndex == pathIndex)
            .OrderBy(n => n.NodeIndex)
            .ToList();
            
        // Process each node in the chosen path
        foreach (var node in pathNodes)
        {
            ProcessNode(node);
        }
    }
    
    private void ProcessNode(MapNode node)
    {
        var sceneToLoad = SceneMapper.MapNodeToScene(node);
        
        // Configure scene based on node properties
        ConfigureSceneFromNode(node);
        
        // Load the scene
        SceneManager.LoadScene(SceneMapper.GetSceneName(sceneToLoad));
    }
}
```

### 3. **Node Property Usage**

```csharp
public class CombatManager : MonoBehaviour
{
    public void ConfigureFromNode(MapNode node)
    {
        if (node.Type == NodeType.Encounter)
        {
            var encounterType = (EncounterType)node.Properties["EncounterType"];
            var enemyCount = (int)node.Properties["EnemyCount"];
            var difficulty = (int)node.Properties["Difficulty"];
            var rewardMultiplier = (double)node.Properties["RewardMultiplier"];
            
            // Setup combat based on properties
            SpawnEnemies(encounterType, enemyCount, difficulty);
            SetRewardMultiplier(rewardMultiplier);
        }
    }
}
```

## 📈 Advanced Rule Configuration

### 🔄 **Dynamic Rule Adjustment**

```csharp
// Rules can change based on layer depth
public GenerationRules GetAdjustedRulesForLayer(int layerIndex, System.Random seededRandom)
{
    var adjusted = new GenerationRules();
    CopyTo(adjusted);
    
    // Later layers get more complex
    if (pathsGrowWithDepth)
    {
        adjusted.minNodesPerPath += layerIndex / 2;
        adjusted.maxNodesPerPath += layerIndex / 2;
    }
    
    // More events in deeper layers
    if (eventChanceScalesWithDepth)
    {
        adjusted.baseEventChance += layerIndex * 0.1f;
    }
    
    return adjusted;
}
```

### 🎲 **Seed-Based Flexibility**

```csharp
// 10% of the time, rules can be bent for variety
if (allowSeedRuleVariation && seededRandom.NextDouble() < ruleFlexibility)
{
    // Occasionally reduce guaranteed encounters
    adjusted.guaranteedMinorEnemiesPerLayer = Math.Max(0, guaranteedMinorEnemiesPerLayer - 1);
    
    // Sometimes allow more major enemies  
    adjusted.maxMajorEnemiesPerLayer = Math.Min(4, maxMajorEnemiesPerLayer + 1);
}
```

## 🚀 Export & Share Configurations

### 📤 **Export Configuration**

```csharp
[ContextMenu("📤 Export Configuration")]
public string ExportConfiguration()
{
    // Exports to JSON format
    // Copies to clipboard automatically
    // Includes validation statistics
}
```

### 📥 **Import Configuration**

```csharp
public bool ImportConfiguration(string json)
{
    // Import from JSON
    // Auto-validates after import
    // Preserves test seeds and settings
}
```

## 🎯 Best Practices

### ✅ **Do's**
- **Test extensively** with various seed inputs
- **Use presets** as starting points for custom rules
- **Enable auto-validation** during development
- **Document your custom rules** with clear descriptions
- **Export configurations** for team sharing

### ❌ **Don'ts**
- **Don't disable all guarantees** (makes unplayable maps)
- **Don't set impossibly strict rules** (causes generation failures)  
- **Don't ignore validation warnings** (leads to poor player experience)
- **Don't make paths too short** (reduces meaningful choice)

## 🔮 Rule Examples

### 🏆 **"Tournament Mode"** - Competitive Balance
```csharp
var tournamentRules = new GenerationRules
{
    minPathsPerLayer = 3, maxPathsPerLayer = 3,    // Exactly 3 choices
    guaranteedCombatPerLayer = 2,                   // Consistent challenge
    guaranteedRestShopPerLayer = true,              // Fair recovery
    allowSeedRuleVariation = false,                 // No randomness
    ruleFlexibility = 0f                           // Strict enforcement
};
```

### 🎲 **"Speedrun Mode"** - Fast & Chaotic
```csharp
var speedrunRules = new GenerationRules
{
    minNodesPerPath = 2, maxNodesPerPath = 4,      // Shorter paths
    baseEventChance = 0.1f,                        // Fewer events
    guaranteedRestShopPerLayer = false,            // Higher risk
    maxMajorEnemiesPerLayer = 3                    // More challenge
};
```

### 🌟 **"Story Mode"** - Rich Experience
```csharp
var storyRules = new GenerationRules
{
    minNodesPerPath = 4, maxNodesPerPath = 7,      // Longer journeys
    baseEventChance = 0.5f,                        // Lots of events
    minEventsPerLayer = 2,                         // Rich narrative
    guaranteedRestShopPerLayer = true              // Player-friendly
};
```

## 📚 Summary

The NeonLadder string input system provides:

- ✨ **Universal Input**: Any string works - emojis, Unicode, special chars
- ⚖️ **Guaranteed Balance**: Slay the Spire inspired rule system
- 🎛️ **Visual Configuration**: ScriptableObject-based rule editing
- 🧪 **Comprehensive Testing**: Automatic validation and preview
- 🔄 **Perfect Reproducibility**: Same seed = identical experience
- 🎯 **Flexible Rules**: Customizable for any gameplay style

*"The ancient one taught me that with great procedural power comes great responsibility to the player experience."*

**- Stephen Strange, Master of Balanced Chaos** 🔮