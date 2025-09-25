# 180-Degree Analog Aiming System

## Overview

This system implements **Metroid-style 180-degree analog aiming** using the right controller stick. Players can smoothly aim within a forward-facing semicircle while maintaining precise analog control over their weapon direction.

### Key Features

- ✅ **180-degree arc constraint** - Prevents backward aiming while allowing full forward coverage
- ✅ **Smooth analog tracking** - Responsive right stick input with configurable sensitivity
- ✅ **Hybrid input support** - Seamless fallback between controller and mouse
- ✅ **Automatic facing direction** - Smart character flipping with hysteresis
- ✅ **Animation integration** - Drive upper-body poses with normalized values
- ✅ **Zero breaking changes** - Fully backward compatible with existing systems

## Architecture

### Components

1. **`AnalogAim180.cs`** - Core aiming logic and input processing
2. **`RangedAttackController.cs`** - Enhanced with analog support
3. **`AnalogAimingTestWindow.cs`** - Editor debugging and validation tool

### Integration Points

- **Input System**: Uses existing `PlayerControls.inputactions` → `Aim` action
- **Weapon System**: Integrates with `RangedAttackController` projectile firing
- **Player System**: Works alongside existing `Player` and `PlayerAction` classes
- **Animation System**: Optional `Animator` parameter support

## Setup Instructions

### Step 1: Add Components to Player GameObject

```csharp
// Your Player GameObject should have:
- Player.cs (existing)
- RangedAttackController.cs (existing, auto-updated)
- AnalogAim180.cs (NEW - add this component)
```

### Step 2: Create AimPivot Transform Hierarchy

```
PlayerRoot
├── Sprite/Model (existing)
├── AimPivot (NEW - empty GameObject)
│   └── WeaponSpawnPoint (move existing spawnPoint here if needed)
└── Other components...
```

**AimPivot Positioning:**
- Position at shoulder/chest level where weapon rotation looks natural
- This transform will rotate to follow analog input
- Child objects (weapon, muzzle effects) will inherit rotation

### Step 3: Configure AnalogAim180 Component

**Inspector Settings:**
```
References:
  Aim Pivot: [Assign the AimPivot GameObject]
  Animator: [Optional - for upper-body poses]
  Sprite Renderer: [Player's SpriteRenderer for facing detection]

Input Configuration:
  Aim Stick: [Set to PlayerControls → Player → Aim]

Aiming Parameters:
  Deadzone: 0.2 (ignore small stick movements)
  Rotate Speed: 720°/sec (how fast aim tracks stick)
  Hysteresis: 10° (prevents facing flip chatter)

Angle Constraints:
  Min Local: -90° (down-forward limit)
  Max Local: +90° (up-forward limit)
```

### Step 4: Configure RangedAttackController Integration

The `RangedAttackController` will automatically detect and use the `AnalogAim180` component. You can customize behavior in the Inspector:

```
Analog Aiming Integration:
  ☑ Analog Overrides Mouse: When stick active, ignore mouse
  ☑ Enable Mouse Fallback: Use mouse when stick inactive
```

### Step 5: Test Integration

Use **Window → NeonLadder → Tools → Analog Aiming Test Window** to validate setup:

- Auto-finds components in scene
- Real-time status monitoring during Play Mode
- Setup validation checklist
- Integration health diagnostics

## Usage Patterns

### For Players

**Controller (Recommended Experience):**
- Move left stick to walk/run
- Move right stick to aim in 180° arc
- Press attack button to fire in aim direction
- Release right stick to return to neutral/mouse aiming

**Mouse + Keyboard (Fallback):**
- Move with WASD
- Aim with mouse cursor
- Click to attack
- Right stick input will override mouse when detected

### For Developers

**Reading Aim State:**
```csharp
// Get the analog aiming component
AnalogAim180 analogAim = GetComponent<AnalogAim180>();

// Check if analog aiming is active
if (analogAim.IsAnalogActive)
{
    // Get current aim direction (Vector2)
    Vector2 aimDir = analogAim.GetAimDirection2D();

    // Get target position at specific distance
    Vector3 targetPos = analogAim.GetAimTargetPosition(10f);

    // Get normalized angle for animations (-1 to +1)
    float normalizedAngle = analogAim.CurrentAimAngleNormalized;
}
```

**Animation Integration:**
```csharp
// The component automatically sets these Animator parameters:
// "AimAngle" (float): -1 = down-forward, 0 = forward, +1 = up-forward
// "AimStrength" (float): 0 = no input, 1 = full stick deflection

// Use in Blend Trees for upper-body pose layers
```

**Manual Angle Control:**
```csharp
// Force set aim angle (useful for auto-aim, cutscenes, etc.)
analogAim.SetAimAngle(45f); // Aim 45° up from forward
```

## Technical Details

### Input Processing Flow

1. **Read right stick input** from Unity's Input System
2. **Apply deadzone filtering** to ignore small movements
3. **Update facing direction** with hysteresis (prevents flipping chatter)
4. **Convert to local angle** relative to character facing
5. **Clamp to 180° constraint** (-90° to +90°)
6. **Smooth interpolation** toward target angle
7. **Update AimPivot rotation** in world space
8. **Drive Animator parameters** for upper-body poses

### Coordinate System

- **Local Space**: Angles relative to character facing direction
  - `0°` = straight forward
  - `+90°` = straight up
  - `-90°` = straight down
  - `±180°` = backward (clamped out)

- **World Space**: Unity's standard coordinate system
  - Facing Right: `0°` = world right (+X)
  - Facing Left: `180°` = world left (-X)
  - AimPivot rotation automatically handles facing conversion

### Performance Notes

- **Near-zero GC allocation** - Uses cached values and minimal heap allocation
- **Single transform rotation per frame** - Efficient for 60fps gameplay
- **Event-driven animation updates** - Only when aim state changes
- **Minimal raycasting** - Only when firing, not during aiming

## Debugging

### Visual Debug Features

**Scene View Gizmos (when AnalogAim180 selected):**
- **Red/Yellow Ray**: Current aim direction (red = active, yellow = inactive)
- **Blue Arc**: Valid aiming range visualization
- **Pivot Position**: AimPivot location and orientation

**Game View Debug Lines:**
- **Cyan Line**: Analog aiming active (from weapon to target)
- **Red Line**: Mouse aiming active (from weapon to cursor)

### Common Issues

**Problem: Aim doesn't work**
- ✅ Check AimPivot is assigned and positioned correctly
- ✅ Verify Input Action reference is set to existing "Aim" action
- ✅ Ensure PlayerControls.inputactions has right stick bindings
- ✅ Test with controller connected and recognized by Unity

**Problem: Character flips too often**
- 🔧 Increase `hysteresisDeg` value (try 15-20°)
- 🔧 Check SpriteRenderer reference is correct

**Problem: Aiming feels sluggish**
- 🔧 Increase `rotateSpeedDegPerSec` (try 900-1200)
- 🔧 Reduce `deadzone` (try 0.1-0.15)

**Problem: Mouse doesn't work when controller connected**
- 🔧 Disable `analogOverridesMouse` in RangedAttackController
- 🔧 Ensure `enableMouseFallback` is enabled

**Problem: Animations don't respond**
- ✅ Check Animator reference is assigned
- ✅ Verify Animator has "AimAngle" and "AimStrength" float parameters
- ✅ Set up Blend Trees with these parameters driving upper-body poses

## Future Enhancements

### Phase 2 Possibilities
- **Auto-aim snap** to nearby enemies within aim cone
- **Weapon recoil** integration with transient aim offset
- **Shoulder switching** for 3D characters to prevent clipping
- **Hero angle snapping** (lock to 0°, ±45°, ±90° when firing)
- **Controller haptic feedback** on successful hits
- **Crosshair UI** that follows analog aim direction

### Advanced Animation
- **IK-driven arm positioning** for 3D rigs using Animation Rigging
- **Additive pose layers** for subtle upper-body adjustments
- **Facial animation sync** (eyes follow aim direction)
- **Weapon-specific animations** (rifle vs pistol vs bow aiming poses)

---

## Credits

**Implementation**: Based on GPT-4o's Metroid-style analog aiming solution
**Integration**: Adapted for NeonLadder's existing architecture
**Testing**: Comprehensive validation tools and debugging features

**Note**: This system maintains full backward compatibility with existing NeonLadder weapon and input systems while adding modern analog aiming capabilities.