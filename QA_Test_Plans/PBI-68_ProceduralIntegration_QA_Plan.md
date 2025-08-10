# QA Test Plan: PBI-68 - Procedural Scene Integration

## Test Overview
This test plan validates the integration between SaveStateConfiguration and procedural scene generation, ensuring saved games maintain consistent procedural content and player progression.

## Pre-Test Setup
1. Open Unity project (6000.0.26f1)
2. Enable Developer Console (F1)
3. Have SaveStateConfiguration test assets ready
4. Ensure ProceduralIntegrationManager is in scene

## Test Cases

### Test 1: Deterministic Map Generation
**Priority**: P0 - Critical
**Steps**:
1. Create new SaveStateConfiguration asset
2. Set procedural seed to "test-seed-123"
3. Generate map and note the layout
4. Save the game
5. Quit and reload
6. Load the saved game
7. Verify map layout matches exactly

**Expected Results**:
- Same seed produces identical map layout
- Boss rooms appear in same locations
- Path connections are identical
- Special rooms maintain positions

### Test 2: Save/Load Procedural State
**Priority**: P0 - Critical
**Steps**:
1. Start new game with procedural generation
2. Progress through 3 procedural rooms
3. Interact with doors/switches in rooms
4. Save game at checkpoint
5. Quit to main menu
6. Load saved game
7. Verify all rooms and interactions preserved

**Expected Results**:
- Player spawns at correct checkpoint
- Previously opened doors remain open
- Collected items stay collected
- Room layouts match exactly
- Current depth/path preserved

### Test 3: Scene Transition Persistence
**Priority**: P0 - Critical
**Steps**:
1. Enter procedural scene at depth 2
2. Note room layout and enemy positions
3. Transition to next scene (depth 3)
4. Return to previous scene (depth 2)
5. Verify room state preserved

**Expected Results**:
- Room layout unchanged
- Defeated enemies stay defeated
- Interactive objects maintain state
- No regeneration occurs
- Performance remains smooth

### Test 4: Boss Room Integration
**Priority**: P1 - High
**Steps**:
1. Progress to boss room (use test config)
2. Note boss type and arena layout
3. Start boss fight but don't complete
4. Save and quit
5. Load save
6. Verify boss room consistency

**Expected Results**:
- Boss type matches (Pride, Wrath, etc.)
- Arena layout identical
- Boss health state preserved
- Special mechanics work correctly

### Test 5: Multiple Save Slots
**Priority**: P1 - High
**Steps**:
1. Create Save A with seed "alpha"
2. Progress through 5 rooms
3. Create Save B with seed "beta"
4. Progress through 3 rooms
5. Load Save A
6. Verify correct procedural state
7. Load Save B
8. Verify different procedural state

**Expected Results**:
- Each save maintains unique seed
- Room layouts differ between saves
- Progress tracked independently
- No cross-contamination

### Test 6: SaveStateConfiguration Loading
**Priority**: P1 - High
**Steps**:
1. Create custom SaveStateConfiguration
2. Set specific seed, depth, and path
3. Set player level to 10
4. Set currency values
5. Apply configuration in-game
6. Verify all settings applied

**Expected Results**:
- Procedural seed applied correctly
- Player starts at specified depth
- Stats match configuration
- Currency values set properly
- World state initialized

### Test 7: Interactive Object Persistence
**Priority**: P2 - Medium
**Steps**:
1. Enter procedural room with switches/doors
2. Activate 2 of 4 switches
3. Open 1 of 3 doors
4. Collect some items
5. Save and reload
6. Verify object states

**Expected Results**:
- Activated switches remain on
- Opened doors stay open
- Collected items gone
- Uncollected items present
- States persist through reload

### Test 8: Performance Under Load
**Priority**: P2 - Medium
**Steps**:
1. Generate large map (20+ rooms)
2. Visit all rooms quickly
3. Save game
4. Monitor memory usage
5. Load saved game
6. Check loading time

**Expected Results**:
- Memory usage stays under 2GB
- Save file size < 10MB
- Load time < 10 seconds
- No memory leaks
- Smooth transitions

### Test 9: Error Recovery
**Priority**: P2 - Medium
**Steps**:
1. Corrupt procedural seed in save file
2. Attempt to load save
3. Verify graceful handling
4. Test with missing scene data
5. Test with invalid depth values

**Expected Results**:
- No crashes on bad data
- Fallback to default generation
- Error logged to console
- Player notified of issue
- Game remains playable

### Test 10: Procedural Validation
**Priority**: P1 - High
**Steps**:
1. Enable validation in settings
2. Generate map with seed "validate"
3. Save game
4. Manually edit save file seed
5. Load game
6. Check validation result

**Expected Results**:
- Validation detects mismatch
- Warning displayed
- Option to regenerate or continue
- Original map can be restored
- No data loss

## Performance Metrics

### Target Metrics:
- Map generation: < 500ms
- Scene loading: < 3 seconds
- Save operation: < 1 second
- Memory overhead: < 100MB
- Validation check: < 100ms

### Monitoring Points:
- FPS during transitions
- Memory allocation spikes
- Save file sizes
- Load times per depth
- Cache hit rates

## Regression Testing

After changes, verify:
1. Existing saves still load
2. Old procedural seeds work
3. Scene transitions smooth
4. No new memory leaks
5. Performance unchanged

## Edge Cases to Test

1. **Seed Overflow**: Very large seed numbers
2. **Deep Recursion**: 50+ depth levels
3. **Rapid Transitions**: Quick scene changes
4. **Concurrent Saves**: Multiple save operations
5. **Missing Assets**: Deleted room prefabs

## Integration Points

Verify integration with:
- PathGenerator system
- SaveStateConfiguration
- EnhancedSaveSystem
- ProceduralSceneLoader
- Scene transition system

## Sign-off Criteria
- [ ] All P0 tests pass
- [ ] Deterministic generation verified
- [ ] Save/load cycle works perfectly
- [ ] No memory leaks detected
- [ ] Performance targets met
- [ ] Boss rooms generate correctly
- [ ] Interactive objects persist

## Notes for QA Team
- Use debug console to verify seeds
- Check profiler during transitions
- Test with both new and existing saves
- Document any generation inconsistencies
- Report exact reproduction steps for issues

## Test Data Requirements
- Multiple SaveStateConfiguration assets
- Test seeds: "alpha", "beta", "test-123"
- Save files at various depths
- Corrupted save files for error testing