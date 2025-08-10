# PBI-36: Steam Cloud Saves Implementation - Human QA Test Plan

## Overview
This PBI implements Steam Cloud save synchronization for NeonLadder, ensuring players can access their roguelite progression from any Steam-compatible device and never lose progress due to hardware failures.

## Critical Risk Assessment
**Risk Level: MEDIUM-HIGH** - Save system reliability is crucial for player trust
- Cloud sync must not corrupt or lose save data
- Conflict resolution must be clear and intuitive
- Performance impact must be minimal
- Steam integration must handle offline mode gracefully

## Human Testing Requirements

### 1. Basic Steam Cloud Functionality Testing
**Priority: CRITICAL**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Initial Cloud Save Test**
   - Launch game through Steam
   - Start new game and make progress (reach level 3+)
   - Save game normally
   - Open Steam → View → Settings → Cloud
   - **Expected**: NeonLadder shows cloud sync activity
   - **Expected**: Save file size displayed in Steam

2. **Cloud Save Verification**
   - Exit game completely
   - Check Steam cloud status
   - **Expected**: "Synchronized" status shown
   - **Expected**: Last sync time is recent

3. **Offline Mode Test**
   - Put Steam in offline mode
   - Launch game
   - Make progress and save
   - **Expected**: Game saves locally without errors
   - Go back online
   - **Expected**: Automatic sync when online

### 2. Multi-Device Synchronization Testing
**Priority: CRITICAL**
**Testing Time: 30 minutes**

#### Test Steps:
1. **Device A Setup**
   - Play on Device A (primary computer)
   - Reach level 5, collect 100+ Meta currency
   - Unlock at least 2 abilities
   - Save and exit game
   - Wait for Steam sync confirmation

2. **Device B Load**
   - Log into same Steam account on Device B
   - Install and launch NeonLadder
   - Load save game
   - **Expected**: Exact same progress as Device A
   - **Expected**: All currency amounts match
   - **Expected**: All unlocks present

3. **Cross-Device Progression**
   - Continue playing on Device B
   - Gain 2 more levels
   - Save and exit
   - Return to Device A
   - **Expected**: Device A shows Device B's progress

### 3. Save Conflict Resolution Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Create Conflict Scenario**
   - Play on Device A offline for 10 minutes
   - Save game (Level 10, 500 Meta)
   - Play on Device B online for 5 minutes  
   - Save game (Level 8, 300 Meta)
   - Go online with Device A
   - **Expected**: Conflict resolution UI appears

2. **UI Validation**
   - Examine conflict resolution screen
   - **Expected**: Clear display of both saves
   - **Expected**: Timestamps visible
   - **Expected**: Player levels shown
   - **Expected**: "NEWER/OLDER" indicators

3. **Resolution Choices**
   - Test "Use Local Save" option
   - **Expected**: Local save preserved, cloud updated
   - Create another conflict
   - Test "Use Cloud Save" option
   - **Expected**: Cloud save applied, local overwritten

4. **Remember Choice Test**
   - Enable "Remember my choice" toggle
   - Choose "Always use newer"
   - Create another conflict
   - **Expected**: Auto-resolves without UI

### 4. Performance Impact Testing
**Priority: MEDIUM**
**Testing Time: 10 minutes**

#### Test Steps:
1. **Save Performance**
   - Time a normal save operation
   - Enable Steam Cloud
   - Time save with cloud sync
   - **Expected**: < 0.5 second difference

2. **Load Performance**
   - Time loading from local save
   - Time loading with cloud check
   - **Expected**: < 1 second difference

3. **Background Sync**
   - Play for 30 minutes continuously
   - Monitor for stutters during auto-sync
   - **Expected**: No noticeable frame drops

### 5. Storage Management Testing
**Priority: MEDIUM**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Storage Limits**
   - Check Steam Cloud quota (View → Settings → Cloud)
   - Note NeonLadder usage
   - Play for extended session (2+ hours)
   - Check usage again
   - **Expected**: Reasonable growth (< 1MB)

2. **Multiple Saves Test**
   - Create 3 different save slots (if supported)
   - Sync all to cloud
   - **Expected**: All saves sync correctly
   - **Expected**: Combined size reasonable

3. **Cleanup Test**
   - Delete local save through game menu
   - **Expected**: Cloud save also removed
   - Verify in Steam settings

### 6. Error Recovery Testing
**Priority: HIGH**
**Testing Time: 20 minutes**

#### Test Steps:
1. **Network Interruption**
   - Start save operation
   - Disconnect network mid-save
   - **Expected**: Graceful fallback to local save
   - **Expected**: No data corruption
   - Reconnect network
   - **Expected**: Retry sync automatically

2. **Steam Service Issues**
   - Exit Steam completely while game running
   - Try to save
   - **Expected**: Local save works
   - **Expected**: Warning message about cloud unavailable
   - Restart Steam
   - **Expected**: Pending sync completes

3. **Corrupt Cloud Save**
   - Manually corrupt cloud save (dev mode)
   - Try to load
   - **Expected**: Detection of corruption
   - **Expected**: Fallback to local or backup
   - **Expected**: Clear error message

### 7. Steam Deck Compatibility Testing
**Priority: HIGH (if applicable)**
**Testing Time: 15 minutes**

#### Test Steps:
1. **Steam Deck Sync**
   - Save on PC
   - Load on Steam Deck
   - **Expected**: Full compatibility
   - **Expected**: UI scales correctly

2. **Sleep Mode Test**
   - Play on Steam Deck
   - Put device to sleep mid-game
   - Wake and continue
   - Save game
   - **Expected**: Proper cloud sync

## Acceptance Criteria

### Must Pass (Blockers):
- [ ] Saves sync to Steam Cloud successfully
- [ ] Multi-device synchronization works
- [ ] Conflict resolution UI functions
- [ ] No data loss during sync
- [ ] Offline mode works correctly

### Should Pass (High Priority):
- [ ] Performance impact minimal
- [ ] Storage usage reasonable
- [ ] Error recovery graceful
- [ ] Auto-sync works reliably

### Nice to Have:
- [ ] Steam Deck verified
- [ ] Sync status visible in-game
- [ ] Manual sync button available

## Testing Environment
- **Unity Version**: 6000.0.26f1
- **Steamworks.NET**: Version 20.2.0
- **Platform**: Windows/Mac/Linux with Steam
- **Network**: Test both stable and unstable connections
- **Steam Mode**: Test both online and offline

## Common Issues to Watch For
1. **Sync Delays** - Steam can take 10-15 seconds to sync
2. **Offline Detection** - May take time to recognize offline state
3. **Version Mismatches** - Old cloud saves with new game version
4. **Multiple Instances** - Running game on 2 devices simultaneously
5. **Steam Workshop** - Conflicts with workshop content (if applicable)

## Platform-Specific Testing

### Windows
- [ ] Steam overlay works
- [ ] Cloud sync status visible
- [ ] File permissions correct

### Mac
- [ ] Steam integration works
- [ ] Cloud saves in correct location
- [ ] Permissions for ~/Library folder

### Linux
- [ ] Proton compatibility (if needed)
- [ ] File system case sensitivity handled
- [ ] Cloud sync works

## Regression Testing
Ensure these still work with Steam Cloud enabled:
- [ ] Local saves still function
- [ ] Save file backups created
- [ ] Quick save/load performance
- [ ] Settings persistence
- [ ] Multiple save slots (if applicable)

## Sign-off Requirements
- [ ] All critical tests pass
- [ ] Multi-device sync verified
- [ ] Conflict resolution tested
- [ ] No data loss scenarios
- [ ] Performance acceptable
- [ ] QA Lead approval: _______________
- [ ] Platform tested: _______________
- [ ] Date tested: _______________

---

**Remember: Cloud saves are about trust. One lost save can mean a lost player forever.**