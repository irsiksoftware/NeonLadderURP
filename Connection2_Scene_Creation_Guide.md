# Connection2 Scene Creation Guide

## 🎯 Overview
This guide provides step-by-step instructions for creating Connection2 scenes that bridge Connection1 scenes to Boss Arena scenes. These scenes are the final transition before boss encounters and must be configured to work seamlessly with the Scene System.

## 📋 Prerequisites
- Scene System completed and merged (PR #138)
- Start.unity scene completed with exit triggers
- All 7 Connection1 scenes completed
- Unity Editor open with NeonLadder project
- Basic-Loadable-Scene-Template-v2.unity available

## 🗺️ Connection2 Scene Types Needed

Based on the 7 Deadly Sins boss destinations, create these Connection2 scenes:

| Boss Arena | Connection2 Scene Name | Theme/Environment |
|------------|------------------------|-------------------|
| Banquet | `Banquet_Connection2.unity` | Kitchen approach, service corridors |
| Cathedral | `Cathedral_Connection2.unity` | Sacred antechamber, confession booths |
| Necropolis | `Necropolis_Connection2.unity` | Crypt entrance, ancient doors |
| Mirage | `Mirage_Connection2.unity` | VIP shopping area, exclusive displays |
| Garden | `Garden_Connection2.unity` | Eden gateway, paradise threshold |
| Lounge | `Lounge_Connection2.unity` | Executive lounge, private elevator area |
| Vault | `Vault_Connection2.unity` | Vault antechamber, security checkpoint |

## 🚀 Step-by-Step Implementation

### Step 1: Create Scene from Template

For **each** Connection2 scene (repeat this process 7 times):

1. **Copy Scene Template**
   ```
   Navigate to: Assets/Scenes/Claude-Tests/
   Right-click: Basic-Loadable-Scene-Template-v2.unity
   Select: Duplicate
   Rename to: Banquet_Connection2.unity (or appropriate name)
   ```

2. **Move to Proper Location**
   ```
   Drag scene file to: Assets/Scenes/
   (Keep it with other main game scenes)
   ```

3. **Open the Scene**
   ```
   Double-click: Banquet_Connection2.unity
   ```

4. **Add to Build Settings**
   ```
   File → Build Settings
   Click: "Add Open Scenes"
   Verify scene appears in Scenes In Build list
   ```

### Step 2: Configure Core Components

1. **Verify Essential Prefabs**
   ```
   Check scene contains:
   ├── Managers.prefab (should exist from template)
   └── GameController.prefab (should exist from template)
   
   If missing: Drag from Assets/Prefabs/ into scene
   ```

2. **Scene Naming Verification**
   ```
   Scene name MUST match pattern: {BossName}_Connection2
   Examples:
   - Banquet_Connection2.unity ✓
   - Cathedral_Connection2.unity ✓  
   - Wrath_Connection2.unity ✗ (use Necropolis_Connection2)
   ```

### Step 3: Create Spawn Points

Create exactly **2 spawn points** for each Connection2 scene:

#### SpawnPoint_FromRight (Entry from Connection1)
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "SpawnPoint_FromRight"
   ```

2. **Add SpawnPoint Component**
   ```
   Add Component → Scripts → Gameplay → SpawnPoint
   ```

3. **Configure Component**
   ```
   SpawnPoint Component:
   ├── Spawn Point Name: "FromRight"
   ├── Direction: Left
   ├── Ground Layer: Default
   └── Use Ground Validation: ✓
   ```

4. **Position and Tag**
   ```
   Position: Right side of scene (where player enters from Connection1)
   Rotation: (0, 90, 0) - facing left toward boss arena
   Tag: "SpawnPoint"
   ```

#### SpawnPoint_FromLeft (Return from Boss Arena)
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty  
   Name: "SpawnPoint_FromLeft"
   ```

2. **Add SpawnPoint Component**
   ```
   Add Component → Scripts → Gameplay → SpawnPoint
   ```

3. **Configure Component**
   ```
   SpawnPoint Component:
   ├── Spawn Point Name: "FromLeft"
   ├── Direction: Right
   ├── Ground Layer: Default
   └── Use Ground Validation: ✓
   ```

4. **Position and Tag**
   ```
   Position: Left side of scene (where player returns from boss defeat/victory)
   Rotation: (0, 270, 0) - facing right back toward Connection1
   Tag: "SpawnPoint"
   ```

### Step 4: Create Transition Triggers

Create exactly **2 transition triggers** for each Connection2 scene:

#### LeftExitTrigger (To Boss Arena)
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "LeftExitTrigger"
   ```

2. **Add BoxCollider (3D)**
   ```
   Add Component → Physics → Box Collider
   ```

3. **Configure Collider**
   ```
   Box Collider:
   ├── Is Trigger: ✓
   ├── Center: (0, 0, 0)
   └── Size: (2, 3, 1)
   ```

4. **Add SceneTransitionTrigger**
   ```
   Add Component → Scripts → Procedural Generation → Scene Transition Trigger
   ```

5. **Configure SceneTransitionTrigger**
   ```
   Scene Transition Trigger:
   ├── Transition Mode: Automatic
   ├── Direction: Left
   ├── Is One Way: ✓
   ├── Requires Key: ✗
   ├── Override Scene Name: [LEAVE EMPTY - let SceneRouter determine boss arena]
   ├── Override Spawn Point: [LEAVE EMPTY]
   └── Interactive Prompt: [LEAVE EMPTY]
   ```

6. **Position**
   ```
   Position: Far left side of scene (exit to boss arena)
   ```

#### RightExitTrigger (Back to Connection1)
1. **Create GameObject**
   ```
   Right-click in Hierarchy → Create Empty
   Name: "RightExitTrigger"
   ```

2. **Add BoxCollider (3D)**
   ```
   Add Component → Physics → Box Collider
   ```

3. **Configure Collider**
   ```
   Box Collider:
   ├── Is Trigger: ✓
   ├── Center: (0, 0, 0)
   └── Size: (2, 3, 1)
   ```

4. **Add SceneTransitionTrigger**
   ```
   Add Component → Scripts → Procedural Generation → Scene Transition Trigger
   ```

5. **Configure SceneTransitionTrigger**
   ```
   Scene Transition Trigger:
   ├── Transition Mode: Automatic
   ├── Direction: Right
   ├── Is One Way: ✗ (allow bidirectional)
   ├── Requires Key: ✗
   ├── Override Scene Name: [LEAVE EMPTY - let SceneRouter determine Connection1]
   ├── Override Spawn Point: "FromLeft"
   └── Interactive Prompt: [LEAVE EMPTY]
   ```

6. **Position**
   ```
   Position: Far right side of scene (return to Connection1)
   ```

### Step 5: Scene Layout Guidelines

For each Connection2 scene, create a final approach layout:

```
[RightExit] ←------ [Path] ------→ [LeftExit to BOSS]
     ↑                              ↑
[SpawnFromRight]              [SpawnFromLeft]
(enter from Connection1)      (return from boss defeat)
```

#### Environmental Layout
1. **Anticipation Building**
   ```
   - Create atmosphere appropriate to upcoming boss
   - Add environmental storytelling elements
   - Build tension as player approaches final encounter
   ```

2. **Boss Arena Preparation**
   ```
   - Visual hints about the boss theme
   - Environmental cues matching boss arena
   - Clear indication this is the final approach
   ```

3. **Path Flow**
   ```
   - Clear visual indication of left/right directions
   - Right side = retreat to Connection1 (safety)
   - Left side = enter boss arena (point of no return)
   ```

### Step 6: Validate Each Scene

After creating each Connection2 scene:

1. **Use Scene Connection Validator**
   ```
   Tools → Neon Ladder → Scene Connection Validator (Ctrl+Shift+V)
   Select the scene in dropdown
   Click "Validate Scene"
   ```

2. **Check for Issues**
   ```
   Ensure no errors for:
   ├── Scene in Build Settings ✓
   ├── Spawn Points configured ✓
   ├── Transition Triggers configured ✓
   ├── No circular references ✓
   └── Proper naming convention ✓
   ```

## 🧪 Testing Connection2 Scenes

### Test Scenario 1: Connection1 → Connection2
1. **Setup**
   ```
   Open any Connection1 scene
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to LeftExitTrigger in Connection1 scene
   Should load corresponding Connection2 scene
   Player should spawn at SpawnPoint_FromRight
   ```

### Test Scenario 2: Connection2 → Boss Arena
1. **Setup**
   ```
   Open any Connection2 scene manually
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to LeftExitTrigger
   Should load corresponding boss arena (when created)
   For now: may show error - this is expected until boss scenes exist
   ```

### Test Scenario 3: Connection2 → Connection1 (Return)
1. **Setup**
   ```
   Open any Connection2 scene manually
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to RightExitTrigger
   Should load corresponding Connection1 scene
   Player should spawn at SpawnPoint_FromLeft in Connection1
   ```

## ✅ Completion Checklist

For **each** Connection2 scene, verify:

- ✅ Scene created from Basic-Loadable-Scene-Template-v2.unity
- ✅ Scene named with pattern: `{BossName}_Connection2.unity`
- ✅ Scene added to Build Settings
- ✅ Managers.prefab and GameController.prefab present
- ✅ SpawnPoint_FromRight configured (entry from Connection1)
- ✅ SpawnPoint_FromLeft configured (return from boss arena)
- ✅ LeftExitTrigger configured (to boss arena)
- ✅ RightExitTrigger configured (back to Connection1)
- ✅ Scene Connection Validator shows no errors
- ✅ Test transitions work properly

## 📊 Expected Deliverables

After completing this guide, you should have:

1. **7 Connection2 Scenes** completing the path to boss arenas
2. **Full Connection Path**: Start → Connection1 → Connection2 → (future Boss)
3. **Bidirectional Movement**: Players can retreat at any point
4. **Clean Validation**: No errors in Scene Connection Validator
5. **Ready for Boss Arenas**: All exit triggers configured for final step

## 🎯 Next Steps

Once all Connection2 scenes are complete:
1. Test full Start → Connection1 → Connection2 flow
2. Proceed to **Boss_Arena_Scene_Creation_Guide.md**
3. Complete end-to-end path validation including boss encounters

Remember: These are the final approach scenes - build atmosphere and anticipation while maintaining functional simplicity!