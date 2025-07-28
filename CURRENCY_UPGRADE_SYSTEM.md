# NeonLadder Currency & Upgrade System 🎮

## 🦸 Analysis by @stephen-strange & @wade-wilson

**@stephen-strange**: "Having mastered the progression systems of 14,000,605 roguelites, this architecture channels the addictive excellence of Hades and Slay the Spire."

**@wade-wilson**: "Maximum effort went into making this system more addictive than chimichangas! We studied every platinum trophy pattern to create the ultimate 'just one more upgrade' experience."

---

## 🎯 System Overview

The NeonLadder Currency & Upgrade System implements dual-currency progression inspired by the most successful roguelites:

- **Meta Currency** (Temporary per-run) - Like Slay the Spire gold or Hades coins
- **Perma Currency** (Persistent across runs) - Like Hades Darkness for Mirror of Night upgrades

### 🏆 Psychological Design Principles

**@wade-wilson's Addiction Formula:**
1. **Layer multiple progression timescales** - Always have 3+ goals visible
2. **Make failure feel like progress** - Death advances permanent upgrades
3. **Front-load rewards, back-load challenges** - Early upgrades are cheap and impactful
4. **Variable ratio reinforcement** - RNG + player choice creates dopamine hits

---

## 🏗️ Architecture Components

### Core Interfaces
- `IUpgradeSystem` - Main upgrade management
- `IUpgrade` - Individual upgrade definition
- `CurrencyType` enum - Meta vs Perma classification
- `UpgradeCategory` enum - Offense/Defense/Utility/Special/Core/Unlocks/Quality

### Implementation Classes
- `UpgradeSystem` - Core system implementation
- `UpgradeData` - ScriptableObject-based upgrade definitions
- `BaseCurrency` - Currency management base class
- `Meta`/`Perma` - Specific currency implementations

### Editor Tools
- `UpgradeSystemEditor` - Unity Editor window for upgrade creation
- `UpgradeEffectDrawer` - Custom property drawer for effects
- `ExampleUpgrades` - Pre-built upgrade templates

---

## 🛠️ Design Team Workflows

### 1. Creating New Upgrades

**Step 1: Create Upgrade Asset**
```
Right-click in Project → Create → NeonLadder → Progression → Upgrade
```

**Step 2: Configure Basic Properties**
- **Upgrade ID**: Unique identifier (e.g., "damage_boost_tier1")
- **Name**: Display name (e.g., "Blade Mastery")
- **Description**: What the upgrade does
- **Flavor Text**: Wade's personality touches

**Step 3: Set Currency & Cost**
- **Currency Type**: Meta (per-run) or Perma (persistent)
- **Category**: Offense/Defense/Utility/Special/Core/Unlocks/Quality
- **Cost**: Balance using the psychological pricing guide below
- **Max Level**: 1 for binary upgrades, higher for scaling

**Step 4: Configure Dependencies**
- **Prerequisites**: Required upgrades (creates upgrade trees)
- **Mutually Exclusive**: Alternative choices (like Hades Mirror)

**Step 5: Define Effects**
- **Target Property**: What stat/behavior to modify
- **Base Value**: Effect at level 1
- **Per Level Increase**: Scaling for multi-level upgrades
- **Is Percentage**: True for percentage bonuses

### 2. Using the Upgrade Designer

**Open the Designer:**
```
Menu → NeonLadder → Upgrade System → Upgrade Designer
```

**Features:**
- **Search & Filter**: Find upgrades by currency, category, or name
- **Visual Inspector**: Edit all upgrade properties in one place
- **Validation**: Check for errors and circular dependencies
- **Duplication**: Copy existing upgrades as templates
- **Export**: Generate upgrade tree documentation

### 3. Testing Upgrades

**In-Editor Testing:**
1. Select upgrade in Designer
2. Click "Test in Play Mode"
3. System applies upgrade effects for testing

**Automated Testing:**
- Run Unity Test Runner
- UpgradeSystemTests validates all functionality
- Tests cover purchase flow, prerequisites, mutual exclusions

---

## 💰 Currency Balance Guidelines

### Meta Currency (Per-Run)
**Purpose**: Like Slay the Spire gold - enables run customization

**Psychological Pricing:**
- **Early upgrades**: 25-50 (immediate gratification)
- **Mid-tier upgrades**: 75-150 (meaningful choices)
- **Premium upgrades**: 200-400 (significant investment)

**Categories:**
- **Offense**: Damage, crit chance, attack speed
- **Defense**: Health boosts, damage reduction
- **Utility**: Movement speed, cooldown reduction
- **Special**: Unique abilities, synergies

### Perma Currency (Persistent)
**Purpose**: Like Hades Darkness - enables meta-progression

**Psychological Pricing:**
- **Core upgrades**: 50-200 (foundational improvements)
- **Unlock upgrades**: 100-500 (new content access)
- **Quality upgrades**: 75-300 (starting bonuses)

**Categories:**
- **Core**: Base stats that carry between runs
- **Unlocks**: New weapons, abilities, areas
- **Quality**: Starting bonuses, item quality

---

## 🎮 Hades & Slay the Spire Inspiration

### From Hades Mirror of Night:
- **Prerequisite Chains**: Advanced upgrades require basic ones
- **Mutually Exclusive Choices**: Alternative upgrade paths
- **Scaling Costs**: Higher levels cost more
- **Immediate Application**: Effects apply instantly

### From Slay the Spire Relics:
- **Categorical Organization**: Clear upgrade types
- **Stacking Effects**: Multiple upgrades compound
- **Run Reset**: Meta upgrades reset on death
- **Preview System**: See next level effects

### From Vampire Survivors:
- **Power Fantasy Escalation**: Late upgrades feel overpowered
- **Screen-Clear Progression**: Effects become visually impressive

---

## 🔧 Technical Integration

### Adding the System to Your Scene

**Step 1: Add Components to Player**
```csharp
// Player GameObject needs:
Player player = GetComponent<Player>();
UpgradeSystem upgradeSystem = GetComponent<UpgradeSystem>();
Meta metaCurrency = GetComponent<Meta>();
Perma permaCurrency = GetComponent<Perma>();
```

**Step 2: Configure Upgrade System**
```csharp
// In UpgradeSystem inspector:
[SerializeField] private UpgradeData[] availableUpgrades;
// Drag your upgrade assets here
```

**Step 3: Connect Currency UI**
```csharp
// Currency controllers automatically update UI
MetaCurrencyController metaUI = GetComponent<MetaCurrencyController>();
PermaCurrencyController permaUI = GetComponent<PermaCurrencyController>();
```

### Purchasing Upgrades from Code

```csharp
// Get the upgrade system
var upgradeSystem = FindObjectOfType<UpgradeSystem>();

// Purchase an upgrade
bool success = upgradeSystem.PurchaseUpgrade("damage_boost", CurrencyType.Meta);

// Check if player can afford upgrade
bool canAfford = upgradeSystem.CanAffordUpgrade("health_boost", CurrencyType.Meta);

// Get available upgrades for shop UI
var availableUpgrades = upgradeSystem.GetAvailableUpgrades(CurrencyType.Meta);
```

### Handling Death & Run Reset

```csharp
// On player death (Hades-style reset)
upgradeSystem.ResetMetaUpgrades(); // Resets per-run upgrades
// Perma currency and upgrades persist automatically

// On new run start
upgradeSystem.ApplyUpgradeEffects(); // Re-applies persistent upgrades
```

---

## 🧪 Test-Driven Development

### Running Tests

**Command Line:**
```bash
Scripts\run-tests-cli.bat
```

**Unity Editor:**
```
Window → General → Test Runner → Run All Tests
```

### Test Coverage

**UpgradeSystemTests.cs** validates:
- Currency deduction and affordability checks
- Prerequisite chain enforcement
- Mutual exclusion logic
- Meta vs Perma upgrade separation
- Multi-level upgrade progression
- Event firing and player integration
- Edge cases and error handling

**Current Status**: 36/36 tests passing ✅

---

## 🎨 Design Best Practices

### Upgrade Tree Design

**@stephen-strange's Mystical Guidelines:**

1. **Start Simple**: Early upgrades should be obvious improvements
2. **Create Synergies**: Later upgrades should combine interestingly
3. **Offer Choices**: Multiple viable paths through the tree
4. **Gate Power**: Strongest upgrades require investment

**@wade-wilson's Addiction Hooks:**

1. **Immediate Gratification**: First upgrade should feel amazing
2. **Visible Progress**: Always show next upgrade goal
3. **FOMO Psychology**: Limited upgrade slots create tension
4. **Power Fantasy**: End-game upgrades should feel broken

### Balancing Guidelines

**Meta Currency Flow:**
- Players should earn 100-300 per run
- Early game: 50-75 per upgrade purchase
- Late game: 150-250 per upgrade purchase

**Perma Currency Flow:**
- Players should earn 25-75 per run
- Upgrades cost 50-500 depending on impact
- Create "save up" goals for expensive unlocks

### Upgrade Effect Guidelines

**Percentage vs Flat Bonuses:**
- **Flat**: Early game, additive improvements (+10 damage)
- **Percentage**: Late game, multiplicative scaling (+15% damage)

**Multi-Level Scaling:**
- Level 1: Noticeable improvement
- Level 2-3: Meaningful investment
- Max Level: Significant power spike

---

## 🐛 Troubleshooting

### Common Issues

**"Upgrade not appearing in shop"**
- Check prerequisites are met
- Verify currency type matches shop filter
- Ensure not at max level

**"Purchase fails with sufficient currency"**
- Check mutually exclusive upgrades
- Verify upgrade ID is correct
- Look for validation errors in console

**"Effects not applying"**
- Ensure `ApplyUpgradeEffects()` is called
- Check target property exists on player
- Verify effect implementation

### Debug Tools

**Upgrade System Inspector:**
- Right-click UpgradeSystem → "Debug: List All Upgrades"
- Shows all upgrades with ownership status

**Test in Play Mode:**
- Use Upgrade Designer "Test in Play Mode" button
- Applies upgrade effects immediately for testing

---

## 📊 Analytics & Metrics

### Tracking Player Behavior

**Key Metrics to Monitor:**
- Average upgrades purchased per run
- Most/least popular upgrade paths
- Currency earning vs spending rates
- Time between upgrade purchases

**Balancing Indicators:**
- **Too Easy**: Players buy everything they want
- **Too Hard**: Players hoard currency without spending
- **Perfect**: Players always have tough choices

---

## 🚀 Future Enhancements

### Planned Features

**@stephen-strange's Roadmap:**
- Visual upgrade tree display
- Upgrade recommendation system
- A/B testing for upgrade costs
- Advanced prerequisite logic (OR conditions)

**@wade-wilson's Wishlist:**
- Upgrade combination effects (relic synergies)
- Time-limited upgrade offers
- Achievement-based upgrade unlocks
- Social comparison features

### Extension Points

The system is designed for easy extension:
- Add new `UpgradeCategory` values
- Create custom `UpgradeEffect` types
- Implement new currency types
- Build shop UI components

---

## 📝 Quick Reference

### Essential Files
- `IUpgradeSystem.cs` - Core interface
- `UpgradeSystem.cs` - Main implementation
- `UpgradeData.cs` - ScriptableObject definitions
- `UpgradeSystemEditor.cs` - Unity Editor tools
- `UpgradeSystemTests.cs` - TDD test suite

### Key Concepts
- **Meta Currency**: Temporary per-run (Slay the Spire gold)
- **Perma Currency**: Persistent across runs (Hades Darkness)
- **Prerequisites**: Upgrade tree gating
- **Mutual Exclusion**: Either/or upgrade choices
- **Multi-Level**: Upgrades with scaling effects

### Quick Actions
- Create upgrade: `Right-click → Create → NeonLadder → Progression → Upgrade`
- Open designer: `Menu → NeonLadder → Upgrade System → Upgrade Designer`
- Run tests: `Scripts\run-tests-cli.bat`
- Test upgrade: Select in designer → "Test in Play Mode"

---

**@stephen-strange**: "With this system, you have glimpsed the optimal path through 14,000,605 possible progression designs."

**@wade-wilson**: "Maximum effort achieved! Now go forth and create the most addictive upgrade system since sliced bread... or Hades. Whichever came first."

---

*Generated with maximum effort by @wade-wilson and mystical precision by @stephen-strange* 🎯