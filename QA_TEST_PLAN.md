# 🎮 QA TEST PLAN - HUMAN TESTING REQUIRED
*Generated: 2025-08-09*

## 🚨 CRITICAL: PRs REQUIRING HUMAN QA TESTING

### ❌ PR #101 - Player/PlayerAction Circular Dependency
**STATUS: NEEDS HUMAN TESTING**
**Risk Level: EXTREME** - Core player controller refactoring

#### Required Manual Tests:
1. **Movement Feel Test** (15 min)
   - [ ] Walk left/right - feels responsive?
   - [ ] Run/Sprint - acceleration feels right?
   - [ ] Jump - height and arc unchanged?
   - [ ] Double jump - timing window preserved?
   - [ ] Z-axis movement - still works in 3D sections?

2. **Combat Integration** (20 min)
   - [ ] Melee attacks - animation plays correctly?
   - [ ] Ranged attacks - switching weapons works?
   - [ ] Attack while moving - no animation glitches?
   - [ ] Attack canceling - can still cancel mid-animation?
   - [ ] Combo attacks - timing windows feel right?

3. **Input Responsiveness** (10 min)
   - [ ] No input lag introduced
   - [ ] Button buffering still works
   - [ ] Simultaneous inputs handled correctly
   - [ ] Controller AND keyboard both work

4. **Edge Cases** (15 min)
   - [ ] Pause mid-action - resume correctly?
   - [ ] Die while attacking - proper state reset?
   - [ ] Scene transitions - player state preserved?
   - [ ] Save/Load - all states persist correctly?

**QA Verdict Required**: "The mediator pattern doesn't break player feel"

---

### ❌ PR #94 - Combo Attack System
**STATUS: NEEDS HUMAN TESTING**
**Risk Level: HIGH** - Combat feel critical

#### Required Manual Tests:
1. **Combo Timing Windows** (30 min)
   - [ ] 0.5s window - too tight for casual players?
   - [ ] Test with 100ms network latency simulation
   - [ ] Test with different frame rates (30/60/120 fps)
   - [ ] Controller vs Keyboard timing differences?

2. **Damage Balancing** (20 min)
   - [ ] Basic combo: 1.0x → 1.2x → 1.5x multipliers balanced?
   - [ ] Aerial Strike: 2.0x damage overpowered?
   - [ ] Lightning Rush: Stamina cost worth the damage?

3. **Visual Feedback** (15 min)
   - [ ] Combo counter visible?
   - [ ] Hit effects scale with combo level?
   - [ ] Audio feedback for combo progression?
   - [ ] Clear indication when combo window expires?

**QA Verdict Required**: "Combat feels satisfying and balanced"

---

### ❌ PR #95 - Procedural Scene Loading
**STATUS: NEEDS PERFORMANCE TESTING**
**Risk Level: HIGH** - Can corrupt saves

#### Required Manual Tests:
1. **Performance Testing** (45 min)
   - [ ] Load 50 scenes in sequence - memory leaks?
   - [ ] Profile on MIN SPEC machine (GTX 1050, 8GB RAM)
   - [ ] Test on Steam Deck - maintains 30fps?
   - [ ] Loading screen duration acceptable? (<3 seconds)

2. **Save System Stress Test** (30 min)
   - [ ] Save during scene transition - corrupts?
   - [ ] Load 100MB save file - works?
   - [ ] Rapid save/load cycling - stable?
   - [ ] Power loss simulation - save recoverable?

**QA Verdict Required**: "Stable under real-world conditions"

---

## ✅ PRs OK with Automated Testing Only

### PR #99 - Euler Angle Caching
**AUTOMATED SUFFICIENT** ✓
- Performance optimization
- No gameplay changes
- Just needs profiler verification

### PR #98 - String Comparison Optimization  
**AUTOMATED SUFFICIENT** ✓
- Pure performance optimization
- No user-facing changes

### PR #100 - Package Export Automation
**AUTOMATED SUFFICIENT** ✓
- Development tool only
- No gameplay impact

---

## 📋 PROPER QA WORKFLOW (What We Should Be Doing)

### 1. **Create QA Build**
```bash
# Tag PR for QA
gh pr edit 101 --add-label "needs-qa"

# Create test build
./Scripts/build-qa.sh --pr 101 --platform Windows
```

### 2. **QA Test Package Contents**
```
/QA_Builds/PR-101/
├── NeonLadder_PR101.exe
├── TestPlan_PR101.pdf
├── BugReportTemplate.txt
├── KnownIssues.txt
└── RollbackInstructions.txt
```

### 3. **QA Feedback Loop**
```yaml
QA Tester Actions:
  - Run test plan checklist
  - Record gameplay video
  - Note "feel" differences
  - File bugs with repro steps
  - Sign off or reject

Developer Response:
  - Fix critical issues
  - Document "won't fix" items
  - Request re-test
```

### 4. **QA Sign-off Requirements**
Before ANY merge:
- [ ] Functional tests pass
- [ ] Performance acceptable
- [ ] No regression from main
- [ ] "Game feel" preserved
- [ ] Edge cases handled

---

## 🔴 BLOCKING ISSUES FOUND

### PR #101 - Mediator Pattern
**POTENTIAL ISSUE**: One frame delay in input?
- Mediator adds indirection
- Could introduce 16ms latency at 60fps
- **NEEDS TESTING**: A/B test with old system

### PR #94 - Combo System  
**CONFIRMED ISSUE**: No difficulty scaling
- Combo timing same for all difficulty levels
- Accessibility concern for casual players
- **FIX REQUIRED**: Add difficulty multipliers

### PR #95 - Scene Loading
**CRITICAL ISSUE**: No rollback mechanism
- If scene load fails, player stuck
- No fallback to last known good state
- **FIX REQUIRED**: Implement scene load recovery

---

## 📊 Testing Priority Matrix

| PR | Impact | Risk | Test Time | Priority |
|----|--------|------|-----------|----------|
| #101 | Core Systems | EXTREME | 60 min | P0 - CRITICAL |
| #94 | Combat | HIGH | 65 min | P0 - CRITICAL |
| #95 | Save System | HIGH | 75 min | P0 - CRITICAL |
| #96 | Editor Tools | LOW | 15 min | P2 - Nice to have |
| #99 | Performance | LOW | 10 min | P1 - Should test |

---

## 🎯 Action Items for Professional QA

### Immediate (Before ANY Merges):
1. **Set up QA environment**
   ```bash
   # Create QA branch combining all PRs
   git checkout -b qa/sprint-8
   git merge feature/pbi-37-break-circular-dependency
   git merge feature/pbi-75-combo-system
   git merge feature/pbi-69-procedural-scenes
   ```

2. **Create automated smoke tests**
   ```csharp
   [Test, Order(1)]
   public void SmokeTest_PlayerCanMove() { }
   
   [Test, Order(2)]  
   public void SmokeTest_PlayerCanAttack() { }
   
   [Test, Order(3)]
   public void SmokeTest_SceneTransitions() { }
   ```

3. **Set up performance benchmarks**
   - Baseline FPS in combat
   - Memory usage over time
   - Loading time targets

### This Week:
1. **Find QA testers** (even if it's just you)
2. **Create video capture setup** for bug reports
3. **Define "feel" metrics** (what makes combat feel good?)
4. **Set up A/B testing** for old vs new systems

### Before Steam Launch:
1. **Professional QA service** (2 week minimum)
2. **Platform certification** (Steam Deck verified)
3. **Localization QA** (if applicable)
4. **Multiplayer stress test** (if applicable)

---

## ⚠️ Current State Assessment

**We are NOT testing like a production game dev would.**

What we're doing:
- ✅ Unit tests (good start)
- ✅ Automated checks (helpful)
- ❌ Human playtesting (MISSING)
- ❌ Performance profiling (MISSING)
- ❌ Platform testing (MISSING)
- ❌ User experience testing (MISSING)

What production does:
- Dedicated QA team or contractor
- Test plans with 100+ checkpoints
- Multiple hardware configurations
- Performance regression tracking
- Gameplay recording for all tests
- Bug database with severity levels
- Daily smoke tests
- Release candidate certification

---

## 📝 Recommended Next Steps

1. **STOP** merging PRs without human testing
2. **CREATE** a simple QA checklist for each PR
3. **PLAY** the game for 30 minutes after each major change
4. **RECORD** gameplay to compare before/after
5. **MEASURE** performance with actual profiler
6. **TEST** on your minimum spec hardware

**Bottom line**: Unit tests can't tell you if your game is fun or if combat feels good. You need human hands on controllers.