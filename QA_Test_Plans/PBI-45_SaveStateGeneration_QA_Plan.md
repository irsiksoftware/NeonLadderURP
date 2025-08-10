# QA Test Plan: PBI-45 - Procedural Scene Save State Generation

## Test Overview
This test plan validates the complete save state generation system for procedural scenes, ensuring all game state is captured and can be perfectly restored.

## Pre-Test Setup
1. Open Unity project (6000.0.26f1)
2. Enable Developer Console (F1)
3. Add ProceduralSaveStateManager to scene
4. Have test SaveStateConfiguration assets ready
5. Enable debug logging in manager

## Test Cases

### Test 1: Complete World State Generation
**Priority**: P0 - Critical
**Steps**:
1. Create new SaveStateConfiguration
2. Set seed: "world-test-123"
3. Set starting depth: 3
4. Generate world state
5. Verify all components generated
6. Check console for generation logs

**Expected Results**:
- World ID generated (unique GUID)
- Map data serialized correctly
- Initial scene states created
- Depth ±2 rooms pre-generated
- No null references or errors

### Test 2: Scene State Capture - Interactive Objects
**Priority**: P0 - Critical
**Steps**:
1. Enter procedural room with:
   - 3 doors (1 open, 2 closed)
   - 4 switches (2 activated)
   - 5 collectibles (2 collected)
2. Capture scene state
3. Check captured data
4. Save game
5. Verify state in save file

**Expected Results**:
- All doors captured with correct states
- Switch states preserved (2 on, 2 off)
- Collectible positions saved
- Only 3 uncollected items in data
- Object IDs consistent

### Test 3: Enemy State Persistence
**Priority**: P0 - Critical
**Steps**:
1. Enter room with 5 enemies
2. Defeat 2 enemies
3. Damage 1 enemy to 50% health
4. Leave 2 enemies untouched
5. Capture scene state
6. Save and reload
7. Verify enemy states

**Expected Results**:
- 2 enemies marked as dead
- 1 enemy at 50% health
- 2 enemies at full health
- Positions preserved exactly
- Enemy types maintained

### Test 4: Environment State Tracking
**Priority**: P1 - High
**Steps**:
1. Enter room with destructibles
2. Destroy 3 of 5 objects
3. Activate 2 trigger zones
4. Capture state
5. Leave and return to room
6. Verify environment

**Expected Results**:
- Destroyed objects stay destroyed
- Intact objects remain
- Triggered zones stay activated
- Positions match exactly
- No regeneration occurs

### Test 5: Save/Load Full Cycle
**Priority**: P0 - Critical
**Steps**:
1. Generate new world (seed: "cycle-test")
2. Progress through 5 rooms
3. Interact with objects in each
4. Save at room 5
5. Quit game completely
6. Load save
7. Verify all 5 rooms

**Expected Results**:
- World regenerates identically
- All 5 room states preserved
- Player spawns at correct position
- Interactive states maintained
- No data loss

### Test 6: State Validation System
**Priority**: P1 - High
**Steps**:
1. Create save with seed "validate-123"
2. Progress through 3 rooms
3. Save game
4. Manually edit save file seed
5. Load save
6. Check validation result

**Expected Results**:
- Validation detects mismatch
- Error logged to console
- OnStateValidationFailed fires
- Game handles gracefully
- Option to regenerate offered

### Test 7: Multi-Scene State Management
**Priority**: P1 - High
**Steps**:
1. Visit 10 different procedural rooms
2. Interact uniquely in each
3. Check memory usage
4. Visit 11th room (cache overflow)
5. Return to room 1
6. Verify state preserved

**Expected Results**:
- Cache manages 20 rooms max
- Oldest states pruned correctly
- Recent rooms fully preserved
- Memory usage stable
- No performance degradation

### Test 8: Boss Room Special State
**Priority**: P1 - High
**Steps**:
1. Enter boss room (Pride)
2. Damage boss to 30% health
3. Activate 2 of 4 boss mechanics
4. Save mid-fight
5. Load save
6. Continue boss fight

**Expected Results**:
- Boss health at 30%
- Boss phase preserved
- Activated mechanics remain
- Arena state maintained
- Fight continues correctly

### Test 9: Player State Integration
**Priority**: P2 - Medium
**Steps**:
1. Position player at (100, 5, 50)
2. Rotate to face north
3. Have 75% health
4. Capture state
5. Move player
6. Restore state
7. Verify position

**Expected Results**:
- Player at exact position
- Rotation preserved
- Health unchanged
- No teleportation glitches
- Smooth restoration

### Test 10: Performance Under Load
**Priority**: P2 - Medium
**Steps**:
1. Create room with 100+ objects
2. Add 20 enemies
3. Many interactive elements
4. Time state capture
5. Time state restoration
6. Check frame rate

**Expected Results**:
- Capture < 100ms
- Restoration < 200ms
- No frame drops
- Memory < 100MB overhead
- Smooth gameplay

## Edge Cases

### Test 11: Corrupted State Recovery
**Steps**:
1. Create valid save
2. Corrupt interactiveObjects data
3. Attempt load
4. Verify graceful handling

**Expected**:
- No crash
- Error logged
- Partial state restored
- Game playable

### Test 12: Rapid State Changes
**Steps**:
1. Rapidly open/close doors
2. Quick enemy kills
3. Fast scene transitions
4. Save during transitions

**Expected**:
- Latest state captured
- No race conditions
- Consistent saves
- No data corruption

## Performance Metrics

### Target Performance:
- World generation: < 500ms
- Scene capture: < 100ms
- Scene restore: < 200ms
- Save operation: < 1 second
- Memory per scene: < 5MB

### Monitoring:
- Use Profiler during all tests
- Track memory allocations
- Monitor GC spikes
- Check save file sizes
- Measure load times

## Regression Testing

After changes verify:
1. Existing saves still load
2. State capture unchanged
3. Validation still works
4. Performance maintained
5. Events fire correctly

## Integration Points

Test integration with:
- ProceduralIntegrationManager
- PathGenerator
- SaveStateConfiguration
- EnhancedSaveSystem
- Scene loading system

## Data Validation

Check save files for:
- Proper JSON structure
- No null references
- Consistent IDs
- Valid timestamps
- Compressed data integrity

## Sign-off Criteria
- [ ] All P0 tests pass
- [ ] Complete state capture verified
- [ ] Perfect state restoration
- [ ] Boss rooms work correctly
- [ ] Performance targets met
- [ ] Validation system functional
- [ ] No memory leaks
- [ ] Events fire properly

## Notes for QA Team
- Use debug console to verify states
- Check save file contents directly
- Monitor memory during long sessions
- Test with various room configurations
- Document exact reproduction steps
- Test both new and migrated saves

## Required Test Assets
- Multiple SaveStateConfiguration files
- Rooms with varied object counts
- Boss room test scenarios
- Corrupted save files for testing
- Performance test rooms (100+ objects)