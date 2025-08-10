# QA Test Plan: PBI-71 - Dialog Save State Integration

## Test Overview
This test plan validates the integration between PixelCrushers Dialogue System and NeonLadder's save system, ensuring all dialogue choices, CVC levels, and narrative progress persist correctly.

## Pre-Test Setup
1. Open Unity project (6000.0.26f1)
2. Import PixelCrushers Dialogue System for Unity
3. Add DialogueSaveIntegration component to scene
4. Configure test dialogue database with sample conversations
5. Enable debug logging in DialogueSaveIntegration

## Test Cases

### Test 1: Basic Dialogue Save/Load
**Priority**: P0 - Critical
**Steps**:
1. Start new game
2. Engage in dialogue with NPC
3. Make 3 specific choices
4. Save game mid-conversation
5. Quit to main menu
6. Load saved game
7. Verify conversation state

**Expected Results**:
- Conversation resumes at correct point
- Previous choices remembered
- Dialogue UI shows correct state
- No duplicate dialogue options
- Active conversation preserved

### Test 2: CVC Points Persistence
**Priority**: P0 - Critical
**Steps**:
1. Start dialogue with character
2. Make choices that award:
   - +10 Courage points
   - +15 Virtue points
   - +5 Cunning points
3. Check CVC display (total: 30)
4. Save game
5. Restart and load
6. Check CVC values

**Expected Results**:
- Total points: 30
- Courage: 10
- Virtue: 15
- Cunning: 5
- CVC Level calculated correctly
- UI displays match saved values

### Test 3: Multiple Character CVC Tracking
**Priority**: P1 - High
**Steps**:
1. Talk to Character A - earn 25 points
2. Talk to Character B - earn 50 points
3. Talk to Character C - earn 15 points
4. Save game
5. Load and verify each character

**Expected Results**:
- Character A: 25 points (Level 1)
- Character B: 50 points (Level 2)
- Character C: 15 points (Level 0)
- Total CVC: 90 points
- Individual histories preserved

### Test 4: Conversation History Tracking
**Priority**: P1 - High
**Steps**:
1. Complete Conversation A fully
2. Partially complete Conversation B
3. View 5 entries in Conversation C
4. Save and reload
5. Check dialogue options

**Expected Results**:
- Conversation A shows as completed
- Conversation B resumes at correct point
- Conversation C shows viewed entries grayed out
- No repeated dialogue
- Correct branching paths available

### Test 5: Quest State Integration
**Priority**: P0 - Critical
**Steps**:
1. Accept quest via dialogue
2. Progress quest to 50%
3. Complete sub-objective via dialogue
4. Save game
5. Load and check quest log

**Expected Results**:
- Main quest shows as active
- Progress at 50%
- Sub-objective marked complete
- Quest dialogue options correct
- Quest giver acknowledges progress

### Test 6: Relationship Persistence
**Priority**: P1 - High
**Steps**:
1. Build relationships:
   - NPC A: +75 (Friendly)
   - NPC B: 0 (Neutral)
   - NPC C: -50 (Hostile)
2. Save and reload
3. Start new conversations
4. Check dialogue options

**Expected Results**:
- NPC A shows friendly dialogue
- NPC B shows neutral dialogue
- NPC C shows hostile dialogue
- Relationship values preserved exactly
- Dialogue choices reflect relationships

### Test 7: Auto-Save on Conversation End
**Priority**: P2 - Medium
**Steps**:
1. Enable auto-save setting
2. Start long conversation
3. Complete conversation
4. Force crash/quit immediately
5. Reload game

**Expected Results**:
- Conversation marked as completed
- All choices saved
- CVC points awarded
- No need to repeat conversation
- Auto-save timestamp correct

### Test 8: Save During Active Conversation
**Priority**: P1 - High
**Steps**:
1. Start complex branching dialogue
2. Make 3 choices
3. Save mid-conversation
4. Make 2 more choices
5. Load previous save
6. Verify state

**Expected Results**:
- Returns to save point (after 3 choices)
- Last 2 choices undone
- Conversation continues normally
- No corruption or glitches
- Smooth transition

### Test 9: Cross-Scene Dialogue Persistence
**Priority**: P1 - High
**Steps**:
1. Start conversation in Scene A
2. Travel to Scene B
3. Have different conversation
4. Return to Scene A
5. Resume original conversation

**Expected Results**:
- Scene A conversation state preserved
- Scene B conversation saved
- No data loss during transitions
- Correct NPCs remember player
- Dialogue flags maintained

### Test 10: Reset Dialogue Progress
**Priority**: P2 - Medium
**Steps**:
1. Complete several conversations
2. Build CVC points to level 5
3. Complete 3 quests
4. Use reset dialogue option
5. Verify complete reset

**Expected Results**:
- All conversations reset
- CVC points zeroed
- Quest states reset
- Relationships cleared
- Fresh dialogue experience

## Edge Cases

### Test 11: Corrupted Dialogue Data
**Steps**:
1. Create valid save
2. Corrupt dialogueDataJson field
3. Attempt to load
4. Check error handling

**Expected**:
- Error logged to console
- Game doesn't crash
- Fallback to default state
- Error event fired
- Player notified

### Test 12: Version Migration
**Steps**:
1. Create save with old format
2. Update to new version
3. Load old save
4. Verify migration

**Expected**:
- Old data preserved
- New fields initialized
- No data loss
- Smooth transition

## Performance Metrics

### Target Performance:
- Save dialogue: < 100ms
- Load dialogue: < 200ms
- Auto-save impact: < 50ms
- Memory overhead: < 10MB
- No frame drops during save

### Data Validation:
- JSON properly formatted
- No null references
- Character IDs consistent
- Timestamps valid
- Data compression working

## Integration Testing

Test with:
- EnhancedSaveSystem
- ConversationPointTracker
- PixelCrushers DialogueSystem
- Quest system
- UI system updates

## Regression Testing

After changes verify:
1. Existing saves still load
2. Dialogue flow unchanged
3. CVC calculations correct
4. Quest integration works
5. Performance maintained

## Sign-off Criteria
- [ ] All P0 tests pass
- [ ] Dialogue choices persist
- [ ] CVC levels save correctly
- [ ] Quest states integrated
- [ ] Relationships maintained
- [ ] Auto-save functional
- [ ] No data loss scenarios
- [ ] Performance targets met

## Notes for QA Team
- Test with various dialogue databases
- Try rapid conversation switching
- Test with 100+ saved conversations
- Verify UI updates match saved data
- Document any dialogue flow issues
- Test with different language settings

## Required Test Data
- Sample dialogue database with:
  - 10+ conversations
  - Branching paths
  - CVC point awards
  - Quest triggers
  - Relationship modifiers
- Multiple test characters
- Save files at various stages