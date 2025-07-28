# 🎨 Visual PathGenerator Configuration Guide
*By Stephen Strange - "See the unseen, configure the infinite"*

> "Through the Eye of Agamotto, I have seen the perfect interface. This is it."

## 🚀 Quick Start: Create Your First Configuration

### Step 1: Create a PathGenerator Config Asset
1. **Right-click** in your Project window
2. Navigate to `Create` → `NeonLadder` → `Procedural Generation` → `Path Generator Config`
3. **Name it**: `MyGameRules.asset`
4. **Double-click** to open in Inspector

### Step 2: Configure Basic Settings
```
🔮 Mystical Configuration
├── Configuration Name: "My Game Rules"
├── Description: "Custom rules for my game mode"
```

### Step 3: Adjust Generation Rules
```
📜 Generation Rules
├── 🏗️ Path Structure Rules
│   ├── Min Nodes Per Path: 3
│   ├── Max Nodes Per Path: 5
│   ├── Min Paths Per Layer: 3
│   └── Max Paths Per Layer: 4
├── ⚔️ Guaranteed Encounters  
│   ├── Guaranteed Combat Per Layer: 1
│   ├── Guaranteed Minor Enemies Per Layer: 1
│   ├── Guaranteed Rest Shop Per Layer: ✓
│   └── Max Major Enemies Per Layer: 2
└── 🎲 Event Distribution
    ├── Min Events Per Layer: 0
    ├── Max Events Per Layer: 1
    ├── Event Chance Scales With Depth: ✓
    └── Base Event Chance: 0.25
```

### Step 4: Test Your Configuration
1. **Add test seeds** in the Testing section:
   ```
   🧪 Testing & Validation
   ├── Test Seeds:
   │   ├── "TestSeed123"
   │   ├── "MyGameSeed"
   │   └── "🎮SpecialChars!@#"
   ├── Show Validation Details: ✓
   └── Auto Validate On Change: ✓
   ```

2. **Right-click the asset** → `🔍 Validate Configuration`
3. **Check Console** for validation results

## 🎛️ Detailed Inspector Interface

### 🔮 **Mystical Configuration Section**

```
┌─ 🔮 Mystical Configuration ─────────────────────────┐
│ Configuration Name: [Text Field]                    │
│ Description: [Multi-line Text Area]                 │
└─────────────────────────────────────────────────────┘
```

**Purpose**: Basic identification and documentation
- **Configuration Name**: Appears in logs and exports
- **Description**: Explains the ruleset's purpose and style

### 📜 **Generation Rules Section**

#### 🏗️ Path Structure Rules
```
┌─ Path Structure Rules ──────────────────────────────┐
│ Min Nodes Per Path:     [3    ] ← Slider (1-10)    │
│ Max Nodes Per Path:     [6    ] ← Slider (1-10)    │
│ Min Paths Per Layer:    [3    ] ← Slider (1-8)     │
│ Max Paths Per Layer:    [5    ] ← Slider (1-8)     │
└─────────────────────────────────────────────────────┘
```

**Controls**: 
- **Min/Max Nodes**: How long each path is (encounters before boss)
- **Min/Max Paths**: How many route choices players get

#### ⚔️ Guaranteed Encounters
```
┌─ Guaranteed Encounters ─────────────────────────────┐
│ Guaranteed Combat Per Layer:        [2    ] ← Int  │
│ Guaranteed Minor Enemies Per Layer: [1    ] ← Int  │
│ Guaranteed Rest Shop Per Layer:     [✓] ← Checkbox │
│ Max Major Enemies Per Layer:        [2    ] ← Int  │
└─────────────────────────────────────────────────────┘
```

**Purpose**: Ensures playable, balanced progression
- **Combat Guarantee**: Minimum fights per layer
- **Minor Enemies**: Easier encounters for warming up
- **Rest Shop**: Recovery opportunities 
- **Major Enemy Limit**: Prevents overwhelming difficulty

#### 🎲 Event Distribution
```
┌─ Event Distribution ────────────────────────────────┐
│ Min Events Per Layer:             [0    ] ← Int    │
│ Max Events Per Layer:             [2    ] ← Int    │
│ Event Chance Scales With Depth:  [✓] ← Checkbox   │
│ Base Event Chance:                [0.30] ← Slider  │
└─────────────────────────────────────────────────────┘
```

**Controls**:
- **Event Limits**: How many non-combat encounters
- **Depth Scaling**: More events in later layers
- **Base Chance**: Starting probability for events

#### 📈 Layer Progression
```
┌─ Layer Progression ─────────────────────────────────┐
│ Paths Grow With Depth:           [✓] ← Checkbox    │
│ Difficulty Scales With Depth:    [✓] ← Checkbox    │
│ More Choices In Later Layers:    [✓] ← Checkbox    │
└─────────────────────────────────────────────────────┘
```

**Purpose**: Creates natural difficulty curve
- **Path Growth**: Later layers have longer paths
- **Difficulty Scaling**: Gradual challenge increase
- **More Choices**: Additional paths in deeper layers

#### ⚖️ Balancing Rules
```
┌─ Balancing Rules ───────────────────────────────────┐
│ Enforce Path Balance:             [✓] ← Checkbox   │
│ Prevent Adjacent Same Type:      [✓] ← Checkbox   │
│ Rest Shop Before Boss:           [✓] ← Checkbox   │
└─────────────────────────────────────────────────────┘
```

**Quality Controls**:
- **Path Balance**: Each route has fair risk/reward
- **Adjacent Prevention**: No boring repetition
- **Pre-Boss Rest**: Always chance to prepare

#### 🎲 Seed Flexibility
```
┌─ Seed Flexibility ──────────────────────────────────┐
│ Allow Seed Rule Variation:       [✓] ← Checkbox    │
│ Rule Flexibility:                [0.10] ← Slider   │
└─────────────────────────────────────────────────────┘
```

**Advanced Options**:
- **Seed Variation**: Some seeds can bend rules
- **Flexibility Amount**: How much rules can vary (0-50%)

### 🧪 **Testing & Validation Section**

```
┌─ 🧪 Testing & Validation ───────────────────────────┐
│ Test Seeds:                                         │
│ ├── Element 0: [TestSeed123        ] ← String      │
│ ├── Element 1: [AnotherSeed        ] ← String      │
│ ├── Element 2: [🎮SpecialChars!@#  ] ← String      │
│ └── [+] Add more test seeds                         │
│                                                     │
│ Show Validation Details:         [✓] ← Checkbox    │
│ Auto Validate On Change:         [✓] ← Checkbox    │
└─────────────────────────────────────────────────────┘
```

**Testing Tools**:
- **Test Seeds**: List of strings to validate against
- **Validation Details**: Show detailed rule violations
- **Auto Validate**: Test automatically when rules change

### 🎮 **Preview Settings Section**

```
┌─ 🎮 Preview Settings ───────────────────────────────┐
│ Preview Map Count:               [3    ] ← Slider   │
│ Show Node Properties:            [  ] ← Checkbox   │
│ Color Code Node Types:           [✓] ← Checkbox    │
└─────────────────────────────────────────────────────┘
```

**Preview Controls**:
- **Map Count**: How many sample maps to generate
- **Node Properties**: Show detailed node data
- **Color Coding**: Visual node type differentiation

## 🎯 Context Menu Commands

**Right-click the asset** to access these commands:

### 🔍 **Validate Configuration**
```
Console Output:
🔮 Validating PathGenerator Config: My Game Rules
✅ Seed 'TestSeed123': Valid
✅ Seed 'AnotherSeed': Valid
⚠️ Seed '🎮SpecialChars!@#': 1 violation
   • Layer 3 has 0 minor enemy encounters, minimum required: 1

📊 Validation Summary for My Game Rules:
   ✅ Valid: 8/10 (80.0%)
   ⚠️ Invalid: 2
   ❌ Errors: 0
```

### 🗺️ **Generate Preview Maps**
```
Console Output:
🗺️ Preview Map #1 (Seed: TestSeed123)
  🏰 Layer 0: Pride at Grand Cathedral of Hubris
     📊 Nodes: Encounter: 3, Event: 1, RestShop: 1, Boss: 1
  🏰 Layer 1: Wrath at The Necropolis of Vengeance
     📊 Nodes: Encounter: 4, Event: 1, RestShop: 1, Boss: 1
```

### 📤 **Export Configuration**
Copies JSON to clipboard:
```json
{
  "configurationName": "My Game Rules",
  "description": "Custom rules for my game mode",
  "rules": {
    "minNodesPerPath": 3,
    "maxNodesPerPath": 5,
    "guaranteedCombatPerLayer": 1,
    // ... full configuration
  },
  "testSeeds": ["TestSeed123", "AnotherSeed"],
  "exportTimestamp": "2025-01-28T10:30:00Z"
}
```

### ⚖️ **Load Presets**

#### **Load Balanced Preset**
- Slay the Spire inspired rules
- Reliable progression
- Good for most games

#### **Load Chaotic Preset**  
- High variance, unpredictable
- Challenging, risk/reward focused
- Good for experienced players

#### **Load Safe Preset**
- Conservative, always completable
- Minimal risk, guaranteed progression
- Good for story modes

## 🎨 Visual Feedback System

### ✅ **Validation Success**
```
Inspector Header: PathGenerator Config (MyGameRules) ✅ VALID
Last Validation: 2025-01-28 10:30 - 10/10 seeds passed (100%)
```

### ⚠️ **Validation Warnings**
```
Inspector Header: PathGenerator Config (MyGameRules) ⚠️ ISSUES
Last Validation: 2025-01-28 10:30 - 8/10 seeds passed (80%)
Issues: 2 seeds failed validation - check console for details
```

### ❌ **Validation Errors**
```
Inspector Header: PathGenerator Config (MyGameRules) ❌ ERRORS  
Last Validation: 2025-01-28 10:30 - 5/10 seeds passed (50%)
Critical: Rules too restrictive, generation failures detected
```

## 📊 Statistics Display

### **Validation Stats (Read-Only)**
```
┌─ 📊 Statistics ─────────────────────────────────────┐
│ Configuration Name: My Game Rules                   │
│ Total Maps: 10                                      │
│ Valid Maps: 8                                       │
│ Invalid Maps: 2                                     │ 
│ Error Maps: 0                                       │
│ Success Rate: 80.0%                                 │
│ Timestamp: 2025-01-28 10:30:15                      │
└─────────────────────────────────────────────────────┘
```

## 🔄 Workflow Examples

### 🎯 **Creating Competitive Rules**
1. **Start with Balanced preset** (`Load Balanced Preset`)
2. **Remove randomness**: Set `Rule Flexibility` to 0
3. **Fix path counts**: Set min/max paths to same value
4. **Increase guaranteed encounters** for consistency
5. **Test extensively** with tournament seeds
6. **Export** for team use

### 🎲 **Creating Speedrun Rules**
1. **Start with Chaotic preset** (`Load Chaotic Preset`)
2. **Reduce path length**: Lower max nodes per path
3. **Reduce events**: Lower event chances
4. **Remove rest guarantees** for higher risk
5. **Test** with speedrunner-provided seeds
6. **Iterate** based on feedback

### 🌟 **Creating Story Mode Rules**
1. **Start with Safe preset** (`Load Safe Preset`)
2. **Increase events**: Raise event chances and maximums
3. **Extend paths**: Increase max nodes for richer experience
4. **Add variety**: Enable more paths in later layers
5. **Test** with narrative-focused seeds
6. **Document** for content team

## 🛠️ Troubleshooting

### ❓ **Common Issues**

#### **"Too many rule violations"**
- **Cause**: Rules too restrictive
- **Fix**: Increase maximum values or reduce guarantees
- **Example**: If requiring 3 minor enemies but max 2 nodes, impossible

#### **"Generation takes too long"**  
- **Cause**: Conflicting rules cause retry loops
- **Fix**: Simplify rules or increase flexibility
- **Example**: Prevent adjacent + guarantee all types = conflicts

#### **"Maps feel repetitive"**
- **Cause**: Rules too strict, no variety
- **Fix**: Increase flexibility or reduce guarantees
- **Example**: Same path length + same encounter count = boring

#### **"Maps too easy/hard"**
- **Cause**: Imbalanced encounter distribution
- **Fix**: Adjust major enemy limits and guarantees
- **Example**: Too many majors = frustrating, too few = boring

### 🔧 **Debug Tips**

1. **Use Console Logging**: Enable `Show Validation Details`
2. **Test Edge Cases**: Try extreme seeds like "", "x", very long strings
3. **Check All Presets**: Validate how each preset handles your seeds
4. **Monitor Performance**: Large rule sets can slow generation
5. **Export/Import**: Save working configurations before experimenting

## 📚 Best Practices Checklist

### ✅ **Before Shipping**
- [ ] Test with 50+ diverse seed strings
- [ ] Validate all custom rule configurations  
- [ ] Ensure 95%+ validation success rate
- [ ] Document rule choices for team
- [ ] Export final configurations for backup

### ✅ **During Development**
- [ ] Enable auto-validation during rule tweaking
- [ ] Use descriptive configuration names
- [ ] Test with player-provided seeds
- [ ] Share configs with team via export/import
- [ ] Monitor generation performance

### ✅ **For Players**
- [ ] Provide preset options in game
- [ ] Allow seed sharing between players
- [ ] Consider UI for basic rule customization
- [ ] Show generation rules in options menu
- [ ] Support seed validation in-game

---

*"The perfect configuration interface reveals all possibilities while hiding none of the complexity. This is the way."*

**- Stephen Strange, Master of Visual Mystic Arts** 🎨🔮