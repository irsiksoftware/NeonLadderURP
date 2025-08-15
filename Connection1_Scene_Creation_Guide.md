# Connection1 Scene Creation Guide

## 🎯 Overview
This guide provides step-by-step instructions for creating Connection1 scenes that bridge the Start scene to Connection2 scenes. These scenes are the first step in each procedural path and must be configured to work seamlessly with the Scene System.

## 📋 Prerequisites
- Scene System completed and merged (PR #138)
- Start.unity scene completed with exit triggers
- Unity Editor open with NeonLadder project
- Basic-Loadable-Scene-Template-v2.unity available

## 🗺️ Connection1 Scene Types Needed

Based on the 7 Deadly Sins boss destinations, create these Connection1 scenes:

| Boss Arena | Connection1 Scene Name | Theme/Environment |
|------------|------------------------|-------------------|
| Banquet | `Banquet_Connection1.unity` | Food market alleys, steam vents |
| Cathedral | `Cathedral_Connection1.unity` | Gothic bridges, stained glass |
| Necropolis | `Necropolis_Connection1.unity` | Graveyard paths, fog |
| Mirage | `Mirage_Connection1.unity` | Jewelry district, neon reflections |
| Garden | `Garden_Connection1.unity` | Overgrown pathways, vines |
| Lounge | `Lounge_Connection1.unity` | Luxury hotel corridors |
| Vault | `Vault_Connection1.unity` | Bank district, metal structures |

## 🚀 Step-by-Step Implementation

### Step 1: Create Scene from Template

For **each** Connection1 scene (repeat this process 7 times):

1. **Copy Scene Template**
   ```
   Navigate to: Assets/Scenes/Claude-Tests/
   Right-click: Basic-Loadable-Scene-Template-v2.unity
   Select: Duplicate
   Rename to: Banquet_Connection1.unity (or appropriate name)
   ```

2. **Move to Proper Location**
   ```
   Drag scene file to: Assets/Scenes/
   (Keep it with other main game scenes)
   ```

3. **Open the Scene**
   ```
   Double-click: Banquet_Connection1.unity
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
   Scene name MUST match pattern: {BossName}_Connection1
   Examples:
   - Banquet_Connection1.unity ✓
   - Cathedral_Connection1.unity ✓  
   - Wrath_Connection1.unity ✗ (use Necropolis_Connection1)
   ```

### Step 3: Create Spawn Points

Create exactly **2 spawn points** for each Connection1 scene:

#### SpawnPoint_FromRight (Entry Point)
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
   Position: Right side of scene (where player enters from Start scene)
   Rotation: (0, 90, 0) - facing left toward the path
   Tag: "SpawnPoint"
   ```

#### SpawnPoint_FromLeft (Return Point)
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
   Position: Left side of scene (where player returns from Connection2)
   Rotation: (0, 270, 0) - facing right back toward Start
   Tag: "SpawnPoint"
   ```

### Step 4: Create Transition Triggers

Create exactly **2 transition triggers** for each Connection1 scene:

#### LeftExitTrigger (To Connection2)
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
   ├── Override Scene Name: [LEAVE EMPTY]
   ├── Override Spawn Point: [LEAVE EMPTY]
   └── Interactive Prompt: [LEAVE EMPTY]
   ```

6. **Position**
   ```
   Position: Far left side of scene (exit to Connection2)
   ```

#### RightExitTrigger (Back to Start)
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
   ├── Override Scene Name: "Start"
   ├── Override Spawn Point: "FromLeft"
   └── Interactive Prompt: [LEAVE EMPTY]
   ```

6. **Position**
   ```
   Position: Far right side of scene (return to Start)
   ```

### Step 5: Scene Layout Guidelines

For each Connection1 scene, create a linear path layout:

```
[RightExit] ←------ [Path] ------→ [LeftExit]
     ↑                              ↑
[SpawnFromRight]              [SpawnFromLeft]
(enter from Start)            (return from Connection2)
```

#### Environmental Layout
1. **Ground/Floor**
   ```
   - Add ground plane with collider
   - Ensure continuous walkable path from right to left
   - No gaps that could trap player
   ```

2. **Visual Boundaries**
   ```
   - Add walls/barriers to guide player along path
   - Use environmental theme appropriate to boss destination
   - Keep layout simple - this is a transition scene
   ```

3. **Path Flow**
   ```
   - Clear visual indication of left/right directions
   - Right side = return to Start (safety)
   - Left side = continue to Connection2 (progression)
   ```

### Step 6: Validate Each Scene

After creating each Connection1 scene:

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

3. **Test Scene Individually**
   ```
   Enter Play Mode in the scene:
   - Player should spawn properly
   - Triggers should show gizmos
   - No console errors
   ```

### Step 7: Batch Validation

After creating **all 7** Connection1 scenes:

1. **Full System Validation**
   ```
   Tools → Neon Ladder → Scene Connection Validator
   Click "Validate All Scenes"
   ```

2. **Build Settings Check**
   ```
   File → Build Settings
   Verify all scenes appear:
   ├── Start.unity
   ├── Banquet_Connection1.unity
   ├── Cathedral_Connection1.unity
   ├── Necropolis_Connection1.unity
   ├── Mirage_Connection1.unity
   ├── Garden_Connection1.unity
   ├── Lounge_Connection1.unity
   └── Vault_Connection1.unity
   ```

## 🧪 Testing Connection1 Scenes

### Test Scenario 1: Start → Connection1
1. **Setup**
   ```
   Open Start.unity scene
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to left or right exit trigger in Start scene
   Should load one of the Connection1 scenes
   Player should spawn at SpawnPoint_FromRight
   ```

3. **Expected Behavior**
   ```
   Console output:
   [SceneRouter] Current scene: Start
   [SceneRouter] Direction: Left
   [SceneRouter] Selected scene: Banquet_Connection1
   [SpawnPointManager] Spawning at: FromRight
   ```

### Test Scenario 2: Connection1 → Start (Return)
1. **Setup**
   ```
   Open any Connection1 scene manually
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to RightExitTrigger
   Should load Start.unity
   Player should spawn at SpawnPoint_FromLeft in Start scene
   ```

### Test Scenario 3: Connection1 → Connection2 (Forward)
1. **Setup**
   ```
   Open any Connection1 scene manually
   Enter Play Mode
   ```

2. **Execute Test**
   ```
   Move player to LeftExitTrigger
   Should load corresponding Connection2 scene (when created)
   For now: may show error - this is expected until Connection2 scenes exist
   ```

## 🔧 Troubleshooting

### Common Issues

#### Issue: "Scene not found by SceneRouter"
**Solution:**
- Check scene naming follows exact pattern: `{BossName}_Connection1`
- Verify scene is in Build Settings
- Check SceneNameMapper configuration

#### Issue: "Player spawns in wrong location"
**Solution:**
- Verify spawn point names: "FromRight", "FromLeft"
- Check spawn point tags: "SpawnPoint"
- Ensure SpawnPoint component configured correctly

#### Issue: "Trigger not responding"
**Solution:**
- Verify BoxCollider "Is Trigger" is checked
- Check player has "Player" tag
- Ensure trigger covers entire pathway

#### Issue: "Return to Start scene not working"
**Solution:**
- Check RightExitTrigger Override Scene Name: "Start"
- Verify Override Spawn Point: "FromLeft"
- Ensure Start scene has SpawnPoint_FromLeft configured

## ✅ Completion Checklist

For **each** Connection1 scene, verify:

- ✅ Scene created from Basic-Loadable-Scene-Template-v2.unity
- ✅ Scene named with pattern: `{BossName}_Connection1.unity`
- ✅ Scene added to Build Settings
- ✅ Managers.prefab and GameController.prefab present
- ✅ SpawnPoint_FromRight configured (entry from Start)
- ✅ SpawnPoint_FromLeft configured (return from Connection2)
- ✅ LeftExitTrigger configured (to Connection2)
- ✅ RightExitTrigger configured (back to Start)
- ✅ Scene Connection Validator shows no errors
- ✅ Test transitions work properly

## 📊 Expected Deliverables

After completing this guide, you should have:

1. **7 Connection1 Scenes** ready for procedural selection
2. **Proper Scene Flow**: Start → Connection1 → (future Connection2)
3. **Bidirectional Movement**: Players can return to Start from any Connection1
4. **Clean Validation**: No errors in Scene Connection Validator
5. **Ready for Connection2**: All exit triggers configured for next step

## 🎯 Next Steps

Once all Connection1 scenes are complete:
1. Test full Start → Connection1 flow
2. Proceed to **Connection2_Scene_Creation_Guide.md**
3. Eventually implement boss arena scenes
4. Complete end-to-end path validation

Remember: These are transition scenes - keep layouts simple and functional. Focus on proper component configuration over environmental art!