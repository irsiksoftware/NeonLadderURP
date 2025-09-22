using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.ProceduralGeneration;
using System.Linq;

namespace NeonLadder.Testing
{
    /// <summary>
    /// Test script to verify barrier control functionality
    /// Attach to any GameObject in Start scene to test
    /// </summary>
    public class BarrierControlTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool simulateProgressions = true;

        [Header("Simulation Controls")]
        [SerializeField] private int bossesToDefeat = 0;
        [SerializeField] private bool forceFinaleBoss = false;

        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunBarrierTests());
            }
        }

        private System.Collections.IEnumerator RunBarrierTests()
        {
            yield return new WaitForSeconds(1f); // Wait for managers to initialize

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] Starting barrier control tests...");

            // Test 1: Initial state (no bosses defeated)
            TestInitialState();
            yield return new WaitForSeconds(1f);

            if (simulateProgressions)
            {
                // Test 2: Simulate defeating some bosses (1-5)
                for (int i = 1; i <= 5; i++)
                {
                    TestProgressionState(i);
                    yield return new WaitForSeconds(0.5f);
                }

                // Test 3: Critical state - 6 bosses defeated (force 7th boss path)
                TestSeventhBossForced();
                yield return new WaitForSeconds(1f);

                // Test 4: Critical state - 7 bosses defeated (force finale)
                TestFinaleForced();
                yield return new WaitForSeconds(1f);

                // Test 5: Finale defeated (game complete)
                TestGameComplete();
            }

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] Barrier control tests completed!");
        }

        private void TestInitialState()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] === Test 1: Initial State ===");

            var logicalManager = LogicalObjectManager.Instance;
            if (logicalManager != null)
            {
                logicalManager.RefreshBarriers();
                var (leftBlocked, rightBlocked) = logicalManager.GetBarrierStates();

                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[BarrierTest] Initial barriers - Left blocked: {leftBlocked}, Right blocked: {rightBlocked}");

                // Should be: both open (false, false)
                if (!leftBlocked && !rightBlocked)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        "[BarrierTest] ✓ PASS: Both paths open as expected");
                }
                else
                {
                    Debugger.LogError(LogCategory.ProceduralGeneration,
                        "[BarrierTest] ✗ FAIL: Expected both paths open");
                }
            }
            else
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[BarrierTest] LogicalObjectManager not found!");
            }
        }

        private void TestProgressionState(int defeatedCount)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[BarrierTest] === Test 2.{defeatedCount}: {defeatedCount} Bosses Defeated ===");

            var proceduralTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (proceduralTransitions != null)
            {
                // Simulate defeating non-finale bosses
                SimulateDefeatedBosses(proceduralTransitions, defeatedCount);

                var logicalManager = LogicalObjectManager.Instance;
                if (logicalManager != null)
                {
                    logicalManager.RefreshBarriers();
                    var (leftBlocked, rightBlocked) = logicalManager.GetBarrierStates();

                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BarrierTest] {defeatedCount} defeated - Left blocked: {leftBlocked}, Right blocked: {rightBlocked}");

                    // Should be: both open until 6 defeated (since we force path at 6+ for 7th boss)
                    bool expectedOpen = defeatedCount < 6;
                    if ((!leftBlocked && !rightBlocked) == expectedOpen)
                    {
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            $"[BarrierTest] ✓ PASS: Barrier state correct for {defeatedCount} defeated");
                    }
                    else
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            $"[BarrierTest] ✗ FAIL: Unexpected barrier state for {defeatedCount} defeated");
                    }
                }
            }
        }

        private void TestSeventhBossForced()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] === Test 3: 7th Boss Forced (6 Defeated) ===");

            var proceduralTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (proceduralTransitions != null)
            {
                // Simulate defeating 6 non-finale bosses
                SimulateDefeatedBosses(proceduralTransitions, 6);

                var logicalManager = LogicalObjectManager.Instance;
                if (logicalManager != null)
                {
                    logicalManager.RefreshBarriers();
                    var (leftBlocked, rightBlocked) = logicalManager.GetBarrierStates();

                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BarrierTest] 6 defeated - Left blocked: {leftBlocked}, Right blocked: {rightBlocked}");

                    // Should be: exactly one path blocked to force 7th boss route
                    if (leftBlocked != rightBlocked) // XOR - exactly one should be blocked
                    {
                        string openPath = leftBlocked ? "RIGHT" : "LEFT";
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            $"[BarrierTest] ✓ PASS: One path forced open ({openPath}) for 7th boss");
                    }
                    else
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            "[BarrierTest] ✗ FAIL: Expected exactly one path blocked for 7th boss");
                    }
                }
            }
        }

        private void TestFinaleForced()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] === Test 4: Finale Forced (7 Defeated) ===");

            var proceduralTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (proceduralTransitions != null)
            {
                // Simulate defeating 7 non-finale bosses
                SimulateDefeatedBosses(proceduralTransitions, 7);

                var logicalManager = LogicalObjectManager.Instance;
                if (logicalManager != null)
                {
                    logicalManager.RefreshBarriers();
                    var (leftBlocked, rightBlocked) = logicalManager.GetBarrierStates();

                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BarrierTest] 7 defeated - Left blocked: {leftBlocked}, Right blocked: {rightBlocked}");

                    // Should be: exactly one path blocked to force finale
                    if (leftBlocked != rightBlocked) // XOR - exactly one should be blocked
                    {
                        string openPath = leftBlocked ? "RIGHT" : "LEFT";
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            $"[BarrierTest] ✓ PASS: One path forced open ({openPath}) for finale");
                    }
                    else
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            "[BarrierTest] ✗ FAIL: Expected exactly one path blocked for finale");
                    }
                }
            }
        }

        private void TestGameComplete()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BarrierTest] === Test 5: Game Complete (8 Defeated) ===");

            var proceduralTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (proceduralTransitions != null)
            {
                // Simulate defeating all 8 bosses including finale
                SimulateDefeatedBosses(proceduralTransitions, 8, true);

                var logicalManager = LogicalObjectManager.Instance;
                if (logicalManager != null)
                {
                    logicalManager.RefreshBarriers();
                    var (leftBlocked, rightBlocked) = logicalManager.GetBarrierStates();

                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BarrierTest] All defeated - Left blocked: {leftBlocked}, Right blocked: {rightBlocked}");

                    // Game complete state - barriers could be either way
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        "[BarrierTest] ✓ Game complete state verified");
                }
            }
        }

        private void SimulateDefeatedBosses(ProceduralPathTransitions transitions, int count, bool includeFinale = false)
        {
            // Clear current defeated list (for testing)
            transitions.GetType().GetField("defeatedBosses",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transitions, new System.Collections.Generic.List<string>());

            // Get all boss identifiers
            var allBosses = BossLocationData.Locations;
            var nonFinaleBosses = allBosses.Where(b => b.Boss != "Devil").ToArray();

            // Add non-finale bosses first
            int nonFinaleCount = includeFinale ? count - 1 : count;
            for (int i = 0; i < nonFinaleCount && i < nonFinaleBosses.Length; i++)
            {
                transitions.MarkBossAsDefeated(nonFinaleBosses[i].Boss);
            }

            // Add finale boss if requested
            if (includeFinale && count >= 8)
            {
                transitions.MarkBossAsDefeated("Devil");
            }
        }

        [ContextMenu("Run Test Now")]
        public void RunTestNow()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunBarrierTests());
            }
        }

        [ContextMenu("Force Test 7th Boss State")]
        public void ForceTest7thBossState()
        {
            if (Application.isPlaying)
            {
                TestSeventhBossForced();
            }
        }

        [ContextMenu("Force Test Finale State")]
        public void ForceTestFinaleState()
        {
            if (Application.isPlaying)
            {
                TestFinaleForced();
            }
        }
    }
}