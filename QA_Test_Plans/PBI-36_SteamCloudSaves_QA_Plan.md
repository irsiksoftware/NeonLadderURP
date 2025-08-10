# QA Test Plan: PBI-36 - Steam Cloud Saves Implementation

## Test Overview
This test plan validates Steam Cloud save synchronization, ensuring reliable multi-device support and protection against progress loss.

## Pre-Test Setup
1. Launch NeonLadder through Steam client
2. Verify Steam Cloud is enabled for the game
3. Check Steam > Settings > Cloud > Enable Steam Cloud
4. Add SteamCloudSaveManager to scene
5. Configure conflict resolution UI

## Test Cases

### Test 1: Basic Cloud Save Upload
**Priority**: P0 - Critical
**Steps**:
1. Start new game
2. Progress to level 5
3. Save game locally
4. Check Steam client cloud status
5. Verify upload indicator

**Expected Results**:
- Save automatically uploads to cloud
- Steam shows sync in progress
- Cloud icon shows synchronized
- No errors in console
- Save completes within 5 seconds

### Test 2: Cloud Save Download
**Priority**: P0 - Critical
**Steps**:
1. Save game with progress (Level 10)
2. Verify cloud sync complete
3. Delete local save files
4. Launch game
5. Check for cloud save prompt

**Expected Results**:
- Game detects missing local save
- Downloads cloud save automatically
- Progress restored to Level 10
- All currencies preserved
- Procedural state maintained

### Test 3: Multi-Device Synchronization
**Priority**: P0 - Critical
**Steps**:
1. Save on Device A (Level 15, 1000 Meta)
2. Wait for cloud sync
3. Launch on Device B
4. Load game
5. Continue playing on Device B
6. Save and return to Device A

**Expected Results**:
- Device B shows Level 15 progress
- Currency values match exactly
- Can continue from Device A state
- Device A gets Device B updates
- Seamless transition

### Test 4: Save Conflict Detection
**Priority**: P0 - Critical
**Steps**:
1. Play offline on Device A (reach Level 8)
2. Play offline on Device B (reach Level 12)
3. Go online on Device A and save
4. Go online on Device B
5. Observe conflict UI

**Expected Results**:
- Conflict detected immediately
- UI shows both save details
- Clear level/time differences
- Newer save highlighted
- Can choose either save

### Test 5: Conflict Resolution - Keep Local
**Priority**: P1 - High
**Steps**:
1. Create conflict scenario
2. When prompted, choose "Keep Local"
3. Verify resolution
4. Check cloud status
5. Launch on other device

**Expected Results**:
- Local save preserved
- Cloud updated with local
- Confirmation message shown
- Other device gets local save
- No data corruption

### Test 6: Conflict Resolution - Keep Cloud
**Priority**: P1 - High
**Steps**:
1. Create conflict scenario
2. Choose "Keep Cloud"
3. Verify local update
4. Check save data

**Expected Results**:
- Cloud save downloaded
- Local save replaced
- Progress matches cloud
- Currencies updated
- Smooth transition

### Test 7: Conflict Resolution - Keep Newer
**Priority**: P1 - High
**Steps**:
1. Create conflict with time difference
2. Choose "Keep Newer"
3. Verify correct save kept
4. Check both local and cloud

**Expected Results**:
- Newer save identified correctly
- Timestamp comparison accurate
- Correct save preserved
- Both locations synchronized
- UI indicates which was kept

### Test 8: Offline Play Handling
**Priority**: P1 - High
**Steps**:
1. Disable internet/Steam offline
2. Play and save multiple times
3. Progress significantly
4. Re-enable internet
5. Check sync behavior

**Expected Results**:
- Game playable offline
- Local saves work normally
- On reconnect, syncs to cloud
- No progress loss
- Handles accumulated saves

### Test 9: Cloud Storage Quota
**Priority**: P2 - Medium
**Steps**:
1. Check cloud storage info
2. Note used/available space
3. Save game multiple times
4. Monitor storage usage
5. Verify within limits

**Expected Results**:
- Storage info displayed correctly
- Usage percentage accurate
- Stays within Steam quota
- Old backups managed
- Compression working

### Test 10: Auto-Save Cloud Sync
**Priority**: P1 - High
**Steps**:
1. Enable auto-save
2. Play through checkpoint
3. Trigger auto-save
4. Check cloud status
5. Verify no blocking

**Expected Results**:
- Auto-save triggers cloud sync
- Background upload
- No gameplay interruption
- Status indicator updates
- Save confirmed in cloud

### Test 11: Network Interruption
**Priority**: P2 - Medium
**Steps**:
1. Start cloud save upload
2. Disconnect internet mid-sync
3. Observe error handling
4. Reconnect internet
5. Check retry behavior

**Expected Results**:
- Error handled gracefully
- No crash or freeze
- Retry attempts made
- Eventually succeeds
- User notified of issues

### Test 12: Steam Deck Compatibility
**Priority**: P1 - High (if applicable)
**Steps**:
1. Save on PC
2. Launch on Steam Deck
3. Verify save loads
4. Play and save on Deck
5. Return to PC

**Expected Results**:
- Seamless Deck support
- Saves transfer perfectly
- UI scales correctly
- Conflict resolution works
- Performance maintained

## Edge Cases

### Test 13: Corrupted Cloud Save
**Steps**:
1. Manually corrupt cloud save
2. Attempt to load
3. Check error handling

**Expected**:
- Corruption detected
- Falls back to local/backup
- Error logged
- User notified
- Can continue playing

### Test 14: Rapid Save Switching
**Steps**:
1. Save frequently (every 30s)
2. Switch between devices rapidly
3. Create multiple conflicts

**Expected**:
- All conflicts detected
- UI doesn't stack/break
- Each resolved independently
- No save corruption

## Performance Metrics

### Target Metrics:
- Upload time: < 3 seconds
- Download time: < 5 seconds
- Conflict detection: < 1 second
- UI response: Immediate
- Background sync: No FPS impact

### Storage Limits:
- Save file size: < 1MB
- Backup included: < 2MB total
- Metadata: < 10KB
- Within Steam quota

## Integration Testing

Verify integration with:
- EnhancedSaveSystem
- SteamManager
- UI notification system
- Settings persistence
- Achievement system

## Regression Testing

After implementation verify:
1. Local saves still work
2. Save/load performance unchanged
3. No new Steam dependencies
4. Offline play unaffected
5. Save file format compatible

## Sign-off Criteria
- [ ] All P0 tests pass
- [ ] Multi-device sync verified
- [ ] Conflict resolution works
- [ ] No data loss scenarios
- [ ] Performance targets met
- [ ] Steam Deck compatible
- [ ] Offline play supported
- [ ] Within storage quotas

## Notes for QA Team
- Test with actual Steam accounts
- Use multiple PCs if possible
- Test with slow internet
- Verify Steam Cloud enabled
- Check different time zones
- Test with family sharing

## Required Test Environment
- 2+ devices with Steam
- Steam Cloud enabled
- Internet connection
- Test Steam account
- Admin access for offline testing