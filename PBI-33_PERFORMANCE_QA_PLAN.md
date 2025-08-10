# PBI-33: Remove Per-Frame String Comparisons - Performance QA Test Plan

## Overview
This PBI optimizes the ManagerController by eliminating expensive per-frame string comparisons, replacing them with integer comparisons and event-driven architecture for significant performance improvements.

## Critical Risk Assessment
**Risk Level: MEDIUM** - Core system optimization with wide impact
- Must maintain all existing functionality
- Performance gains must be measurable
- No new bugs or regressions allowed
- Scene transitions must remain smooth

## Performance Improvements Expected
- **5-15% CPU reduction** in Update() overhead
- **95% reduction** in scene checks per second
- **Minimal memory allocations** (< 10KB per 100 checks)
- **Better frame consistency** and reduced stuttering

## Human Testing Requirements

### 1. Performance Validation Testing
**Priority: CRITICAL**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Baseline Performance Test**
   - Launch game with FPS counter visible
   - Play through Title → Staging → Gameplay sequence
   - Note average FPS and any stutters
   - Open Unity Profiler (if available)
   - **Record**: Average FPS, 1% low FPS

2. **Optimized Version Test**
   - Same sequence with optimized ManagerController
   - Monitor FPS counter throughout
   - **Expected**: 5-10 FPS improvement in complex scenes
   - **Expected**: Fewer frame drops/stutters
   - **Record**: New average FPS, 1% low FPS

3. **CPU Usage Comparison**
   - Use Task Manager/Activity Monitor
   - Monitor CPU usage during gameplay
   - **Expected**: 5-15% lower CPU usage
   - **Expected**: More consistent CPU usage pattern

### 2. Functional Integrity Testing
**Priority: CRITICAL**
**Testing Time: 25 minutes**

#### Test Steps:
1. **Scene Transition Test**
   - Navigate through all game scenes:
     - Title → Staging
     - Staging → Start
     - Start → MetaShop
     - MetaShop → PermaShop
     - PermaShop → Title
   - **Expected**: All transitions work correctly
   - **Expected**: No delays or hangs

2. **Manager Toggle Verification**
   - In each scene, verify correct managers active:
     - **Title**: GameControllerManager enabled
     - **Staging**: LootPurchaseManager, PlayerCameraPositionManager enabled
     - **Start**: MonsterGroupActivationManager, LootDropManager enabled
     - **MetaShop**: LootPurchaseManager enabled
     - **PermaShop**: LootPurchaseManager enabled
   - **Expected**: Correct managers for each scene

3. **Event System Test**
   - Trigger various game events:
     - Enemy defeat
     - Item pickup
     - Dialogue interaction
     - Shop purchase
   - **Expected**: All events fire correctly
   - **Expected**: No missing functionality

### 3. Memory Usage Testing
**Priority: HIGH**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Memory Leak Test**
   - Note initial memory usage
   - Play for 15 minutes, changing scenes frequently
   - Check memory usage again
   - **Expected**: No significant memory growth
   - **Expected**: Stable memory after garbage collection

2. **Allocation Test**
   - Open Profiler (if available)
   - Monitor GC allocations during gameplay
   - Focus on scene transitions
   - **Expected**: Minimal allocations per frame
   - **Expected**: No string allocations in Update()

3. **Long Session Test**
   - Play continuously for 30+ minutes
   - Monitor memory and performance
   - **Expected**: No degradation over time
   - **Expected**: Consistent performance

### 4. Frame Consistency Testing
**Priority: HIGH**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Frame Time Analysis**
   - Enable frame time graph (if available)
   - Play through combat-heavy scene
   - **Expected**: More consistent frame times
   - **Expected**: Fewer frame spikes

2. **Stutter Detection**
   - Rapidly move camera during gameplay
   - Trigger multiple particle effects
   - Spawn multiple enemies
   - **Expected**: Smoother motion
   - **Expected**: Reduced micro-stutters

3. **60 FPS Stability**
   - Target 60 FPS gameplay
   - Monitor drops below 60 FPS
   - **Expected**: Fewer drops
   - **Expected**: Quicker recovery from drops

### 5. Edge Case Testing
**Priority: MEDIUM**
**Testing Time: 10 minutes**

#### Test Steps:
1. **Rapid Scene Switching**
   - Switch scenes as fast as possible
   - Use debug commands if available
   - **Expected**: No crashes or errors
   - **Expected**: Managers toggle correctly

2. **Alt-Tab Test**
   - Alt-tab out during scene transition
   - Return to game
   - **Expected**: Scene detection still works
   - **Expected**: No stuck states

3. **Pause Menu Test**
   - Pause during various scenes
   - Check manager states
   - Resume gameplay
   - **Expected**: Everything resumes correctly

### 6. Profiler Validation (If Available)
**Priority: HIGH**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Update() Performance**
   - Profile ManagerController.Update()
   - Check time per call
   - **Expected**: < 0.1ms per Update()
   - **Expected**: No string operations visible

2. **Scene Check Frequency**
   - Count scene checks per second
   - **Expected**: ~2 checks/second (not 60)
   - **Expected**: Event-driven for actual changes

3. **Allocation Tracking**
   - Monitor allocations in Update()
   - **Expected**: Zero allocations
   - **Expected**: No garbage generation

## Performance Benchmarks

### Target Metrics:
- **Update() Time**: < 0.1ms average
- **Scene Checks**: 2 per second (vs 60 before)
- **Memory Allocations**: < 10KB per 1000 frames
- **CPU Usage**: 5-15% reduction
- **Frame Consistency**: 20% reduction in frame variance

### Measurement Tools:
- Unity Profiler
- Stats window (FPS counter)
- Platform-specific profilers
- Custom performance metrics in build

## Regression Testing

Verify these features still work correctly:
- [ ] All scene transitions
- [ ] Manager initialization
- [ ] Event system
- [ ] Save/Load functionality
- [ ] Dialogue system
- [ ] Combat system
- [ ] Shop systems
- [ ] Player progression

## Platform-Specific Testing

### Windows
- [ ] Performance gains visible
- [ ] No new issues introduced

### Mac
- [ ] Performance gains visible
- [ ] Metal renderer compatibility

### Steam Deck
- [ ] Improved battery life
- [ ] Better thermal performance

## Sign-off Requirements
- [ ] All functional tests pass
- [ ] Performance improvements measured
- [ ] No regressions found
- [ ] Memory usage stable
- [ ] Frame consistency improved
- [ ] QA Lead approval: _______________
- [ ] Performance metrics recorded: _______________
- [ ] Date tested: _______________

---

**Remember: Performance optimization is invisible when done right, but game-breaking when done wrong.**