# PBI-71: Dialog Save State Integration - Human QA Test Plan

## Overview
This PBI integrates the PixelCrushers Dialogue System for Unity with NeonLadder's save system to ensure dialogue choices, relationships, and narrative progress persist across game sessions. This is critical for maintaining player immersion and story continuity.

## Critical Risk Assessment
**Risk Level: HIGH** - Narrative continuity is essential for player engagement
- Dialogue choices must persist correctly
- CVC (Courage, Virtue, Cunning) values affect gameplay
- Quest states need perfect synchronization
- Relationship levels impact NPC interactions

## Human Testing Requirements

### 1. Basic Dialogue Persistence Testing
**Priority: CRITICAL**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Initial Conversation Test**
   - Start new game
   - Engage in dialogue with first NPC
   - Make specific dialogue choices (note them down)
   - Check CVC values display after conversation
   - Save and quit game

2. **Load and Verify Test**
   - Load saved game
   - Re-engage same NPC
   - **Expected**: NPC remembers previous conversation
   - **Expected**: Different dialogue options based on history
   - **Expected**: CVC values match end of last session

3. **Multi-Conversation Chain**
   - Have 3 different conversations with different NPCs
   - Make distinct choices in each
   - Save game
   - Load game
   - **Expected**: All 3 NPCs remember their conversations

### 2. CVC System Integration Testing
**Priority: HIGH**
**Testing Time: 15 minutes**

#### Test Steps:
1. **CVC Value Tracking**
   - Start with baseline CVC values (note them)
   - Make dialogue choice that increases Courage
   - Make dialogue choice that increases Virtue
   - Make dialogue choice that increases Cunning
   - **Expected**: UI shows real-time CVC updates

2. **CVC Persistence Test**
   - After gaining CVC points, save game
   - Completely exit to main menu
   - Load save
   - **Expected**: CVC values exactly match pre-save values

3. **CVC Impact Test**
   - Load save with high Courage (>10)
   - Enter dialogue with boss character
   - **Expected**: Courage-specific dialogue options available
   - **Expected**: Different NPC reactions based on CVC levels

### 3. Quest State Synchronization Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Quest Progress Test**
   - Accept quest from NPC
   - Complete first quest objective
   - Have follow-up dialogue about quest
   - Save game
   - Load game
   - **Expected**: Quest shows correct progress
   - **Expected**: NPC acknowledges quest status

2. **Multi-Quest State**
   - Have 2 active quests
   - 1 completed quest
   - 1 failed quest
   - Save and reload
   - **Expected**: All quest states preserved correctly
   - **Expected**: Quest log shows accurate information

3. **Quest-Dialogue Integration**
   - Complete quest objective
   - Save before turning in quest
   - Load save
   - Turn in quest through dialogue
   - **Expected**: Quest completion dialogue triggers correctly
   - **Expected**: Rewards granted properly

### 4. Relationship System Testing
**Priority: MEDIUM**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Relationship Building**
   - Interact with merchant NPC 3 times
   - Choose friendly options each time
   - Note relationship indicator (if visible)
   - Save and reload
   - **Expected**: Relationship level maintained
   - **Expected**: Merchant offers better prices/items

2. **Negative Relationship Test**
   - Antagonize guard NPC
   - Choose hostile dialogue options
   - Save game
   - Load and approach guard
   - **Expected**: Guard remembers hostility
   - **Expected**: Different dialogue tone

3. **Mixed Relationships**
   - Build positive relationship with NPC A
   - Build negative relationship with NPC B
   - Neutral with NPC C
   - Save, exit completely, reload
   - **Expected**: All three relationship states preserved

### 5. Scene Transition Persistence Testing
**Priority: CRITICAL**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Cross-Scene Dialogue State**
   - Have dialogue in Scene A
   - Travel to Scene B
   - Save in Scene B
   - Load save
   - Return to Scene A
   - **Expected**: NPCs in Scene A remember conversations

2. **Mid-Dialogue Save Test**
   - Enter dialogue
   - Progress halfway through conversation
   - Save game (if possible during dialogue)
   - Load game
   - **Expected**: Either resume dialogue or restart with progress saved

3. **Boss Dialogue Memory**
   - Encounter boss, have pre-fight dialogue
   - Die or retreat
   - Return to boss
   - **Expected**: Different dialogue acknowledging previous encounter

### 6. Edge Case Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Corrupt Save Recovery**
   - Make many dialogue choices
   - Force-quit game during save (Alt+F4)
   - Restart game
   - **Expected**: Either loads backup or handles gracefully

2. **Maximum History Test**
   - Have 50+ conversations
   - Make 100+ dialogue choices
   - Save game
   - Check save file size
   - Load game
   - **Expected**: No performance issues
   - **Expected**: All history preserved

3. **Language Switching Test**
   - Have dialogue in English
   - Save game
   - Switch to Chinese language
   - Load game
   - **Expected**: Dialogue history preserved
   - **Expected**: Correct language displayed

### 7. Performance Testing
**Priority: MEDIUM**
**Testing Time: 10 minutes**

#### Test Steps:
1. **Save Speed Test**
   - Time how long save takes with no dialogue data
   - Have 20 conversations
   - Time save operation
   - **Expected**: Save time increase < 0.5 seconds

2. **Load Speed Test**
   - Create save with extensive dialogue history
   - Time the load operation
   - **Expected**: Load time increase < 1 second

3. **Memory Usage**
   - Monitor RAM before dialogue system active
   - Have 10 conversations
   - Monitor RAM after
   - **Expected**: Memory increase < 50MB

## Acceptance Criteria

### Must Pass (Blockers):
- [ ] All dialogue choices persist between sessions
- [ ] CVC values save and load correctly
- [ ] Quest states synchronize with dialogue
- [ ] No data loss on scene transitions
- [ ] Save/load works without errors

### Should Pass (High Priority):
- [ ] NPC relationships persist
- [ ] Conversation history affects future dialogues
- [ ] Performance impact minimal
- [ ] Backup recovery works

### Nice to Have:
- [ ] Save file size optimized
- [ ] Debug UI shows dialogue state
- [ ] Migration from old saves works

## Testing Environment
- **Unity Version**: 6000.0.26f1
- **Dialogue System**: PixelCrushers Dialogue System for Unity
- **Platform**: [Specify target platform]
- **Build Type**: Development Build
- **Save Location**: GameData/NeonLadderSave.json

## Narrative Flow Validation
After technical testing, validate story experience:

- [ ] First-time player experience feels natural
- [ ] Returning player sees narrative continuity
- [ ] Choices feel impactful across sessions
- [ ] No immersion-breaking state issues
- [ ] CVC system enhances roleplay

## Common Issues to Watch For
1. **Dialogue Options Missing** - Check if prerequisites not loading
2. **NPCs Don't Remember** - Verify actor state persistence
3. **CVC Values Reset** - Check integration with save system
4. **Quest Dialogue Mismatch** - Ensure quest state sync
5. **Performance Degradation** - Monitor with extensive history

## Regression Testing
Verify these still work after dialogue integration:
- [ ] Regular save/load functionality
- [ ] Player stats persistence
- [ ] Inventory saves correctly
- [ ] Scene transitions smooth
- [ ] No new console errors

## Sign-off Requirements
- [ ] All critical tests pass
- [ ] Narrative continuity verified
- [ ] No save corruption issues
- [ ] Performance acceptable
- [ ] QA Lead approval: _______________
- [ ] Narrative Designer approval: _______________
- [ ] Date tested: _______________

---

**Remember: Dialogue is the soul of the narrative. If players lose their story progress, they lose their connection to the game world.**