# QA Test Plan: PBI-34 - Euler Angle Cache Optimization

## Test Overview
This test plan validates the Euler angle caching system that optimizes Quaternion to Euler conversions for improved performance.

## Pre-Test Setup
1. Open Unity project in Play mode
2. Enable Unity Profiler (Window > Analysis > Profiler)
3. Add "EulerAngleCacheBenchmark" component to an empty GameObject
4. Enable Developer Console (F1 key)

## Test Cases

### Test 1: Performance Benchmark Validation
**Priority**: P0 - Critical
**Steps**:
1. Add EulerAngleCacheBenchmark component to GameObject
2. Set Benchmark Iterations to 10000
3. Set Test Object Count to 50
4. Right-click component header > "Run Benchmark"
5. Check console output

**Expected Results**:
- Performance improvement should show 10-15%
- Cache hit rate should be > 85%
- Speedup factor should be > 1.1x
- No errors in console

### Test 2: Player Movement Cache Integration
**Priority**: P0 - Critical
**Steps**:
1. Start game and enter any level
2. Open Profiler and select CPU Usage
3. Move player left and right continuously for 30 seconds
4. Search profiler for "eulerAngles" calls
5. Note the frequency of Transform.get_eulerAngles

**Expected Results**:
- eulerAngles calls should be minimal (< 5% of frames)
- Player movement should feel smooth
- No visual glitches in facing direction
- FPS should remain stable

### Test 3: Multi-Character Stress Test
**Priority**: P1 - High
**Steps**:
1. Load a level with multiple enemies (5+ enemies)
2. Open Stats window (Game view > Stats)
3. Observe FPS while enemies are moving
4. Trigger combat with multiple enemies
5. Monitor performance during intense action

**Expected Results**:
- FPS should not drop below 30 on minimum spec
- Cache statistics should show high hit rate (> 80%)
- No stuttering during combat
- Enemy facing directions update correctly

### Test 4: Cache Memory Management
**Priority**: P1 - High
**Steps**:
1. Play through 3 different levels sequentially
2. Open Profiler > Memory
3. Check memory usage after each level transition
4. Complete a full play session (15+ minutes)

**Expected Results**:
- Memory usage should remain stable
- No memory leaks from cache accumulation
- Cache cleanup on scene transitions
- Total cache size stays under limit (100 entries)

### Test 5: Facing Direction Accuracy
**Priority**: P0 - Critical
**Steps**:
1. Move player left - verify sprite faces left
2. Move player right - verify sprite faces right
3. Jump while moving - verify facing persists
4. Attack in each direction
5. Get hit from different angles

**Expected Results**:
- Facing direction always matches movement
- No flickering between directions
- Attack animations play in correct direction
- Hit reactions maintain proper orientation

### Test 6: Frame Rate Comparison
**Priority**: P1 - High
**Steps**:
1. Add benchmark component to scene
2. Right-click > "Run Frame Rate Test"
3. Wait for 10-second test to complete
4. Review FPS improvement in console

**Expected Results**:
- Cached FPS should be higher than uncached
- Improvement should be 5-15 FPS
- Test completes without errors
- Statistics show conversions saved

### Test 7: Save/Load State Preservation
**Priority**: P2 - Medium
**Steps**:
1. Play game and move character around
2. Save game at checkpoint
3. Quit to main menu
4. Load saved game
5. Verify character facing is correct

**Expected Results**:
- Character loads facing correct direction
- Cache reinitializes properly after load
- No null reference exceptions
- Performance remains optimized after load

### Test 8: Edge Cases
**Priority**: P2 - Medium
**Steps**:
1. Rapidly switch directions (spam A/D keys)
2. Rotate camera while moving (if applicable)
3. Pause/unpause game repeatedly during movement
4. Alt-tab out and back during gameplay

**Expected Results**:
- No crashes or exceptions
- Cache handles rapid changes gracefully
- Pause doesn't corrupt cache state
- Application focus changes don't break cache

## Performance Metrics to Track

### Automated Metrics (logged to console):
- Cache hit rate: Target > 85%
- Performance improvement: Target 10-15%
- Memory usage: < 1MB for cache
- Conversions saved: > 1000 per minute of gameplay

### Manual Observations:
- Smooth character movement
- Responsive controls
- Stable frame rate
- Correct visual feedback

## Regression Testing
After changes, verify:
1. All existing movement mechanics work
2. Combat system functions normally
3. Animation states transition correctly
4. No new performance bottlenecks

## Test Environment Requirements
- Unity 6000.0.26f1
- Minimum spec machine for performance validation
- Profiler tools enabled
- Developer console accessible

## Sign-off Criteria
- [ ] All P0 tests pass
- [ ] Performance improvement verified (10%+)
- [ ] No memory leaks detected
- [ ] Frame rate stable or improved
- [ ] No visual artifacts or glitches
- [ ] Cache statistics show expected behavior

## Notes for QA Team
- Focus on movement-heavy scenarios for best optimization validation
- Use profiler to verify actual performance gains
- Report any facing direction inconsistencies immediately
- Document FPS before/after measurements