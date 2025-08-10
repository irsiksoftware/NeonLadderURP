# PBI-38: Service Container Architecture - Human QA Test Plan

## Overview
This PBI replaces the singleton `ManagerController.Instance` pattern with a dependency injection service container architecture. This is a critical architectural change that affects how game managers communicate throughout the codebase.

## Critical Risk Assessment
**Risk Level: HIGH** - This change affects core game infrastructure
- All manager access patterns have changed
- Service registration timing is critical
- Backward compatibility layer must work perfectly

## Human Testing Requirements

### 1. Game Startup & Initialization Testing
**Priority: CRITICAL**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Cold Start Test**
   - Launch game from Unity Editor
   - Verify no null reference exceptions in console
   - Confirm "Managers" prefab loads correctly
   - Check that all UI elements appear

2. **Scene Transition Test**
   - Start from Title scene
   - Navigate to Staging scene
   - Enter gameplay (Start scene)
   - Visit MetaShop
   - Visit PermaShop
   - **Expected**: Smooth transitions, no manager-related errors

3. **Manager Availability Test**
   - During gameplay, pause game
   - Check Debug UI (if available) for manager status
   - **Expected**: All managers show as "Initialized"

### 2. Combat System Integration Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Collision Detection Test**
   - Engage in combat with enemies
   - Get hit by enemy attacks
   - Attack enemies with melee
   - Attack enemies with ranged
   - **Expected**: Damage numbers appear, health updates correctly

2. **Event System Test**
   - Defeat an enemy
   - Collect loot drops
   - Trigger dialogue sequences
   - **Expected**: Events fire correctly, no "EventManager not found" errors

3. **Death & Respawn Test**
   - Let player character die
   - Observe death animation
   - Respawn at checkpoint
   - **Expected**: Clean death/respawn cycle

### 3. Performance & Memory Testing
**Priority: MEDIUM**
**Testing Time: 10 minutes**

#### Test Steps:
1. **FPS Monitoring**
   - Enable Unity Statistics window
   - Play through combat-heavy scene
   - Monitor FPS during manager-heavy operations
   - **Expected**: No FPS drops compared to previous build

2. **Memory Leak Check**
   - Play for 5 minutes switching scenes
   - Monitor memory usage in Profiler
   - **Expected**: No memory growth trend

3. **Service Registration Timing**
   - Rapidly switch scenes (5 times)
   - Check for race conditions
   - **Expected**: No "Service not registered" errors

### 4. Edge Case Testing
**Priority: HIGH**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Manager Destruction Test**
   - Load a scene
   - Use Unity Inspector to manually destroy GameServiceManager
   - Continue playing
   - **Expected**: Graceful degradation or error messages, no crashes

2. **Late Service Registration**
   - Start game
   - Wait 10 seconds in Title
   - Then proceed to gameplay
   - **Expected**: Services register correctly regardless of timing

3. **Concurrent Access Test**
   - Trigger multiple manager-dependent actions simultaneously:
     - Attack enemy while collecting loot
     - Trigger dialogue during combat
     - Scene transition during enemy defeat
   - **Expected**: All actions complete correctly

### 5. Migration Compatibility Testing
**Priority: CRITICAL**
**Testing Time: 10 minutes**

#### Test Steps:
1. **Legacy Code Path Test**
   - Verify old `ManagerController.Instance` references still work
   - Check console for deprecation warnings
   - **Expected**: No breaking changes for unmigrated code

2. **Service Fallback Test**
   - Temporarily disable service registration (comment out in GameServiceManager)
   - Play game
   - **Expected**: Migration helper provides fallback access

### 6. Gameplay Feel Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Responsiveness Test**
   - Test all player inputs during combat
   - Check menu navigation speed
   - Monitor ability activation delays
   - **Expected**: No increased latency

2. **Game Flow Test**
   - Complete a full gameplay loop:
     - Start → Combat → Loot → Shop → Upgrade → Combat
   - **Expected**: Natural flow, no stutters or delays

3. **Polish Check**
   - Look for any visual glitches
   - Check audio cue timing
   - Verify particle effects trigger correctly
   - **Expected**: Same polish level as before

## Acceptance Criteria

### Must Pass (Blockers):
- [ ] No null reference exceptions related to managers
- [ ] All scene transitions work
- [ ] Combat system fully functional
- [ ] Event system triggers all events
- [ ] No performance degradation

### Should Pass (High Priority):
- [ ] Clean error messages when services unavailable
- [ ] Graceful degradation on manager failure
- [ ] Memory usage stable
- [ ] All UI elements responsive

### Nice to Have:
- [ ] Improved startup time
- [ ] Better error recovery
- [ ] Cleaner console output

## Testing Environment
- **Unity Version**: 6000.0.26f1
- **Platform**: [Specify target platform]
- **Build Type**: Development Build with Profiler
- **Debug Settings**: Enable verbose logging for ServiceLocator

## Regression Testing Checklist
After service container implementation, verify these existing features still work:

- [ ] Player movement and controls
- [ ] Enemy AI behavior
- [ ] Loot collection
- [ ] Shop purchases
- [ ] Save/Load system
- [ ] Achievement unlocks
- [ ] Audio playback
- [ ] Visual effects
- [ ] UI navigation
- [ ] Pause menu

## Notes for QA Team
- Pay special attention to game startup - this is when services register
- Watch console for any "Service not registered" warnings
- If you see "Using fallback" messages, note them but they're not failures
- Test both quick scene switches and slow, methodical gameplay
- **This is architectural plumbing** - if it works, players won't notice; if it fails, everything breaks

## Sign-off Requirements
- [ ] All critical tests pass
- [ ] No performance regression
- [ ] Combat feels responsive
- [ ] No new bugs introduced
- [ ] QA Lead approval: _______________
- [ ] Date tested: _______________

---

**Remember: We're making a game, not just code. If it doesn't feel right, it's not right.**