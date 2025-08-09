# 🎮 HUMAN TESTING REQUIREMENTS FOR OPEN PRs
*Making a game, not just code!*

## 🔴 CRITICAL - BLOCK MERGE WITHOUT HUMAN TESTING

### PR #94 - Combo Attack System
**HUMAN TESTING REQUIRED BEFORE MERGE**

```bash
# Build QA version
./Scripts/create-qa-build.sh 94 Mac

# Test Checklist (30 min minimum)
```

**Combat Feel Test:**
- [ ] Play 10 combat encounters
- [ ] Successfully execute 3-hit combo 5 times
- [ ] Timing window feels: TOO TIGHT / GOOD / TOO LOOSE
- [ ] Visual feedback clear when combo connects
- [ ] Audio feedback satisfying
- [ ] Damage multipliers feel balanced
- [ ] Works with controller AND keyboard

**Sign-off Required:**
```
Tester: _______________
Date: _________________
Verdict: SHIP / FIX
Issue: Combo timing too tight/loose/perfect
```

---

### PR #101 - Player/PlayerAction Refactor
**HUMAN TESTING REQUIRED BEFORE MERGE**

**Critical Test: Input Lag**
- [ ] A/B test: Old system vs New system
- [ ] Jump response time identical?
- [ ] Attack feels immediate?
- [ ] No new input lag introduced?
- [ ] Movement as responsive as before?

**15 Minute Play Test:**
- [ ] Complete a full level
- [ ] No weird control glitches
- [ ] All abilities work
- [ ] Feels identical to main branch

**Sign-off Required:**
```
Tester: _______________
Controls feel: SAME / WORSE / BROKEN
Can ship: YES / NO
```

---

### PR #95 - Procedural Scene Loading
**PERFORMANCE TESTING REQUIRED**

**Memory Leak Test (45 min):**
1. Start game, note memory usage: _____MB
2. Load 20 different scenes
3. Memory after 20 scenes: _____MB
4. Load 20 more scenes
5. Memory after 40 scenes: _____MB
6. Memory leak detected: YES / NO

**Save Corruption Test:**
- [ ] Save during scene transition (spam save button)
- [ ] Force quit during save
- [ ] Load save - corrupted? YES / NO
- [ ] Repeat 5 times

---

### PR #93 - Dialog Save State
**NARRATIVE TESTING REQUIRED**

**Dialog Persistence Test:**
- [ ] Have conversation with NPC
- [ ] Make specific choice
- [ ] Save and quit
- [ ] Load - choice remembered? YES / NO
- [ ] NPC reacts to previous choice? YES / NO
- [ ] Relationship values correct? YES / NO

---

## 🟡 MODERATE - SHOULD TEST BUT NOT BLOCKING

### PR #99 - Euler Angle Caching
**Performance Validation:**
- [ ] Profile before: _____ FPS
- [ ] Profile after: _____ FPS  
- [ ] Actual improvement: ____%
- [ ] Any visual glitches? YES / NO

### PR #96 - Procedural Generation Tools
**UX Testing:**
- [ ] Can artist/designer use it without help?
- [ ] UI intuitive? YES / NO
- [ ] Crashes Unity? YES / NO

---

## 🟢 LOW PRIORITY - AUTOMATED TESTS SUFFICIENT

### PR #100 - Package Export Tool
✅ No gameplay impact - automated tests OK

### PR #98 - String Comparison Fix
✅ Pure optimization - automated tests OK

### PR #97 - Build Menu
✅ Dev tool only - automated tests OK

### PR #90 - Documentation Generator
✅ No gameplay impact - automated tests OK

---

## 📋 TESTING WORKFLOW

### For Each Gameplay PR:
1. **Create Test Build**
   ```bash
   git checkout PR-branch
   ./Scripts/create-qa-build.sh [PR-NUM] Mac
   ```

2. **Run Test Protocol**
   - 15 min minimum for simple changes
   - 45 min for major systems
   - Multiple testers for combat/controls

3. **Document Results**
   ```
   PR #___: [TITLE]
   Tester: [NAME]
   Time: [DURATION]
   Pass: YES/NO
   Issues: [LIST]
   ```

4. **Make Decision**
   - SHIP: Merge immediately
   - FIX: Address issues first
   - REJECT: Fundamental problems

---

## ⚠️ THE GOLDEN RULE

**"If it affects what the player sees, feels, or does - a human must test it"**

Bad code = bugs (fixable)
Bad game feel = bad reviews (permanent)

---

## 🎯 Priority Order for Testing

1. **PR #94** - Combo System (GAMEPLAY)
2. **PR #101** - Control Refactor (FEEL)
3. **PR #95** - Scene Loading (STABILITY)
4. **PR #93** - Dialog Saves (CONTENT)
5. Others as time permits

**Estimated Total Test Time: 3-4 hours**
**Revenue at Risk if Untested: Everything**