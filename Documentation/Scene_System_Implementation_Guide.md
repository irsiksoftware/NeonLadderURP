# Scene System Implementation Guide

## 🎯 Overview
This guide provides step-by-step instructions for implementing the Scene System in Unity Editor. Follow these instructions exactly to ensure proper integration with the SceneRouter, SpawnPointManager, and SceneTransitionTrigger systems.

## 📋 Prerequisites
- Scene System completed and merged (PR #138)
- Unity Editor open with NeonLadder project
- Start.unity scene accessible

## 🚀 Step-by-Step Implementation

### Step 1: Open and Prepare Start Scene

1. **Open Start Scene**
   ```
   File → Open Scene → Assets/Scenes/Start.unity
   ```

2. **Verify Core Components Exist**
   - Check that `Managers.prefab` is present in scene
   - Check that `GameController.prefab` is present in scene
   - If missing, drag from `Assets/Prefabs/` into scene

3. **Verify Build Settings Registration**
   ```
   File → Build Settings → Scenes In Build
   ```
   - Ensure Start.unity is listed and has a build index
   - If missing: Click "Add Open Scenes"

### Step 2: Create Spawn Points

Create 3 empty GameObjects for spawn points:

#### SpawnPoint_Default
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "SpawnPoint_Default"
   ```

2. **Add SpawnPoint Component**
   ```
   Add Component → Scripts → Gameplay → SpawnPoint
   ```

3. **Configure SpawnPoint**
   ```
   SpawnPoint Component:
   ├── Spawn Point Name: "Default"
   ├── Direction: Forward
   ├── Ground Layer: Default
   └── Use Ground Validation: ✓ (checked)
   ```

4. **Set Transform**
   ```
   Position: Center of scene where player should spawn initially
   Rotation: (0, 0, 0) - facing forward
   ```

5. **Add Tag**
   ```
   Tag: "SpawnPoint"
   ```
   *(If "SpawnPoint" tag doesn't exist, create it via Tags & Layers)*

#### SpawnPoint_FromLeft
1. **Duplicate SpawnPoint_Default**
   ```
   Select SpawnPoint_Default → Ctrl+D
   Rename to: "SpawnPoint_FromLeft"
   ```

2. **Configure for Left Return**
   ```
   SpawnPoint Component:
   ├── Spawn Point Name: "FromLeft"
   ├── Direction: Right
   ├── Ground Layer: Default
   └── Use Ground Validation: ✓
   ```

3. **Position**
   ```
   Position: Right side of scene (where player returns from left path)
   Rotation: (0, 270, 0) - facing right toward center
   ```

#### SpawnPoint_FromRight
1. **Duplicate SpawnPoint_Default**
   ```
   Select SpawnPoint_Default → Ctrl+D
   Rename to: "SpawnPoint_FromRight"
   ```

2. **Configure for Right Return**
   ```
   SpawnPoint Component:
   ├── Spawn Point Name: "FromRight"
   ├── Direction: Left
   ├── Ground Layer: Default
   └── Use Ground Validation: ✓
   ```

3. **Position**
   ```
   Position: Left side of scene (where player returns from right path)
   Rotation: (0, 90, 0) - facing left toward center
   ```

### Step 3: Create Transition Triggers

#### LeftExitTrigger
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "LeftExitTrigger"
   ```

2. **Add BoxCollider (3D - NOT 2D!)**
   ```
   Add Component → Physics → Box Collider
   ```

3. **Configure Collider**
   ```
   Box Collider:
   ├── Is Trigger: ✓ (checked)
   ├── Center: (0, 0, 0)
   └── Size: (2, 3, 1) - adjust to cover exit area
   ```

4. **Add SceneTransitionTrigger Component**
   ```
   Add Component → Scripts → Procedural Generation → Scene Transition Trigger
   ```

5. **Configure SceneTransitionTrigger**
   ```
   Scene Transition Trigger:
   ├── Transition Mode: Automatic
   ├── Direction: Left
   ├── Is One Way: ✓ (checked)
   ├── Requires Key: ✗ (unchecked)
   ├── Override Scene Name: [LEAVE EMPTY - let SceneRouter decide]
   ├── Override Spawn Point: [LEAVE EMPTY - let SpawnPointManager decide]
   └── Interactive Prompt: [LEAVE EMPTY for automatic mode]
   ```

6. **Position Trigger**
   ```
   Position: Left side of scene where player should exit
   Ensure trigger covers the entire exit pathway
   ```

#### RightExitTrigger
1. **Duplicate LeftExitTrigger**
   ```
   Select LeftExitTrigger → Ctrl+D
   Rename to: "RightExitTrigger"
   ```

2. **Update Configuration**
   ```
   Scene Transition Trigger:
   ├── Direction: Right (CHANGE THIS!)
   └── [All other settings same as LeftExitTrigger]
   ```

3. **Position Trigger**
   ```
   Position: Right side of scene where player should exit
   ```

### Step 4: Test Basic Setup

1. **Use Scene Connection Validator**
   ```
   Tools → Neon Ladder → Scene Connection Validator (Ctrl+Shift+V)
   ```

2. **Run Validation**
   - Click "Validate All Scenes"
   - Check for any errors or warnings
   - Fix any issues before proceeding

3. **Visual Verification**
   - Scene view should show colored gizmos for triggers and spawn points
   - Green = functional trigger
   - Blue = spawn point with direction indicator

### Step 5: Test Scene Transitions

#### Testing Method 1: Play Mode
1. **Enter Play Mode**
   ```
   Click Play button in Unity Editor
   ```

2. **Move Player to Trigger**
   - Use WASD or arrow keys to move player
   - Walk into LeftExitTrigger or RightExitTrigger
   - Observe console for SceneRouter debug messages

3. **Expected Behavior**
   ```
   Console should show:
   - "SceneRouter: Loading scene for direction: Left"
   - "SceneRouter: Selected scene: [ProcedualSceneName]"
   - Scene should transition to Connection1 scene
   ```

#### Testing Method 2: Force Transition (Debug)
1. **Select Trigger in Scene**
   - Click on LeftExitTrigger in Hierarchy

2. **Use Inspector Debug**
   ```
   In Scene Transition Trigger component:
   Click "Force Transition" button (if available)
   ```

### Step 6: Verify Procedural Generation

#### Expected SceneRouter Behavior
When player triggers left/right exit, SceneRouter should:

1. **Determine Current Path State**
   - Check SceneRoutingContext for current procedural path
   - Identify available connection scenes

2. **Select Procedural Destination**
   ```
   Example destinations based on direction:
   - Left path: "Banquet_Connection1", "Cathedral_Connection1", etc.
   - Right path: Different procedural selection
   ```

3. **Set Spawn Context**
   - SpawnPointManager receives direction context
   - Next scene spawns player at appropriate spawn point

#### Debug Verification
Enable debug logs to trace decisions:
```csharp
// In console, you should see:
[SceneRouter] Current scene: Start
[SceneRouter] Transition direction: Left
[SceneRouter] Selected destination: Banquet_Connection1
[SpawnPointManager] Setting transition context: Left
[SpawnPointManager] Next spawn point: FromRight
```

## 🔧 Troubleshooting

### Common Issues

#### Issue: "SceneTransitionTrigger requires Collider component"
**Solution:** Ensure you added BoxCollider (3D), not BoxCollider2D

#### Issue: "No spawn points found in scene"
**Solution:** 
- Verify spawn points have "SpawnPoint" tag
- Check SpawnPoint component is properly configured
- Ensure scene has been saved after adding spawn points

#### Issue: "SceneRouter returning null scene name"
**Solution:**
- Check that SceneRoutingContext singleton exists
- Verify BossLocationData is properly configured
- Check console for SceneRouter error messages

#### Issue: Player not transitioning on trigger
**Solution:**
- Ensure player GameObject has "Player" tag
- Verify BoxCollider "Is Trigger" is checked
- Check that player has Rigidbody component
- Confirm trigger is positioned correctly

#### Issue: Wrong spawn point in destination scene
**Solution:**
- Verify destination scenes have proper spawn points
- Check SpawnPointManager is receiving direction context
- Ensure spawn point names match expected pattern

### Scene Validation Checklist

Before testing, verify:
- ✅ Start.unity is in Build Settings
- ✅ Managers.prefab exists in scene
- ✅ GameController.prefab exists in scene
- ✅ 3 spawn points created with proper tags and configuration
- ✅ 2 transition triggers with 3D BoxColliders
- ✅ SceneTransitionTrigger components configured correctly
- ✅ Scene Connection Validator shows no errors
- ✅ Player GameObject exists and has "Player" tag

## 🎯 Success Criteria

When implementation is complete, you should have:

1. **Functional Scene Transitions**
   - Player walks into left trigger → loads Connection1 scene
   - Player walks into right trigger → loads different Connection1 scene
   - Deterministic: same seed + same choice = same destination

2. **Proper Spawn Positioning**
   - Initial entry: spawns at SpawnPoint_Default
   - Return from left: spawns at SpawnPoint_FromLeft
   - Return from right: spawns at SpawnPoint_FromRight

3. **Clean Console Output**
   - SceneRouter debug messages showing decision process
   - No errors or warnings in console
   - Scene Connection Validator reports all green

4. **Visual Confirmation**
   - Scene gizmos visible in Scene view
   - Triggers positioned correctly at scene exits
   - Spawn points positioned correctly for directional flow

## 📞 Support

If you encounter issues:
1. Check console for error messages
2. Use Scene Connection Validator tool
3. Verify each step was completed exactly as described
4. Check that Scene System PR #138 is properly merged

Remember: The complex logic is already implemented - you're just placing and configuring GameObjects!