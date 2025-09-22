using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.ProceduralGeneration;
using System.Collections.Generic;

namespace NeonLadder.Testing
{
    /// <summary>
    /// Test script to verify boss selection logic and prevent duplicate selections
    /// </summary>
    public class BossSelectionTest : MonoBehaviour
    {
        [Header("Boss Selection Testing")]
        [SerializeField] private bool testOnStart = true;
        [SerializeField] private string testSeed = "LV6TZ0";

        private void Start()
        {
            if (testOnStart)
            {
                TestBossSelectionLogic();
            }
        }

        [ContextMenu("Test Boss Selection Logic")]
        public void TestBossSelectionLogic()
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BossSelectionTest] Testing boss selection logic...");

            var pathTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions == null)
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[BossSelectionTest] ProceduralPathTransitions not found!");
                return;
            }

            // Set a specific seed for testing
            SetTestSeed(pathTransitions, testSeed);

            // Test initial selections
            TestInitialSelections(pathTransitions);

            // Test selections after defeating bosses
            TestSelectionsAfterDefeats(pathTransitions);

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BossSelectionTest] Boss selection testing completed!");
        }

        private void SetTestSeed(ProceduralPathTransitions pathTransitions, string seed)
        {
            var gameSeedField = typeof(ProceduralPathTransitions).GetField("gameSeed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (gameSeedField != null)
            {
                gameSeedField.SetValue(pathTransitions, seed);

                // Re-initialize to apply the new seed
                var initMethod = typeof(ProceduralPathTransitions).GetMethod("InitializeComponent",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                initMethod?.Invoke(pathTransitions, null);

                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[BossSelectionTest] Set test seed: {seed}");
            }
        }

        private void TestInitialSelections(ProceduralPathTransitions pathTransitions)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BossSelectionTest] === Testing Initial Selections ===");

            // Clear defeated bosses for clean test
            ClearDefeatedBosses(pathTransitions);

            // Test multiple selections to see if we get duplicates
            var leftBoss1 = pathTransitions.SelectNextBoss(true);
            var rightBoss1 = pathTransitions.SelectNextBoss(false);

            if (leftBoss1 != null && rightBoss1 != null)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[BossSelectionTest] Visit 1 - LEFT: {leftBoss1.DisplayName} ({leftBoss1.Boss})");
                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[BossSelectionTest] Visit 1 - RIGHT: {rightBoss1.DisplayName} ({rightBoss1.Boss})");

                if (leftBoss1.Boss == rightBoss1.Boss)
                {
                    Debugger.LogError(LogCategory.ProceduralGeneration,
                        "[BossSelectionTest] ✗ FAIL: Got same boss for both paths!");
                }
                else
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        "[BossSelectionTest] ✓ PASS: Different bosses selected for each path");
                }

                // Test dynamic behavior: defeat one boss and check if new options appear
                pathTransitions.MarkBossAsDefeated(leftBoss1.Boss);

                var leftBoss2 = pathTransitions.SelectNextBoss(true);
                var rightBoss2 = pathTransitions.SelectNextBoss(false);

                if (leftBoss2 != null && rightBoss2 != null)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BossSelectionTest] Visit 2 - LEFT: {leftBoss2.DisplayName} ({leftBoss2.Boss})");
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BossSelectionTest] Visit 2 - RIGHT: {rightBoss2.DisplayName} ({rightBoss2.Boss})");

                    // Verify defeated boss doesn't reappear
                    if (leftBoss2.Boss == leftBoss1.Boss || rightBoss2.Boss == leftBoss1.Boss)
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            "[BossSelectionTest] ✗ FAIL: Defeated boss reappeared!");
                    }
                    else
                    {
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            "[BossSelectionTest] ✓ PASS: Defeated boss does not reappear");
                    }

                    // Verify we get different options (dynamic behavior)
                    if ((leftBoss2.Boss != leftBoss1.Boss || rightBoss2.Boss != rightBoss1.Boss))
                    {
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            "[BossSelectionTest] ✓ PASS: Dynamic boss selection working - got new options");
                    }
                    else
                    {
                        Debugger.LogWarning(LogCategory.ProceduralGeneration,
                            "[BossSelectionTest] ? OPTIONS: Boss options didn't change (may be normal with limited pool)");
                    }
                }
            }
            else
            {
                Debugger.LogError(LogCategory.ProceduralGeneration,
                    "[BossSelectionTest] Failed to select bosses for testing");
            }
        }

        private void TestSelectionsAfterDefeats(ProceduralPathTransitions pathTransitions)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                "[BossSelectionTest] === Testing Selections After Defeats ===");

            // Clear defeated bosses for clean test
            ClearDefeatedBosses(pathTransitions);

            // Track selected bosses
            var selectedBosses = new HashSet<string>();

            // Test defeating bosses one by one and ensure no repeats
            for (int i = 0; i < 5; i++) // Test first 5 selections
            {
                var leftBoss = pathTransitions.SelectNextBoss(true);
                if (leftBoss != null)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[BossSelectionTest] Round {i + 1} LEFT: {leftBoss.DisplayName} ({leftBoss.Boss})");

                    if (selectedBosses.Contains(leftBoss.Boss))
                    {
                        Debugger.LogError(LogCategory.ProceduralGeneration,
                            $"[BossSelectionTest] ✗ FAIL: Boss {leftBoss.Boss} selected again!");
                    }
                    else
                    {
                        selectedBosses.Add(leftBoss.Boss);
                        // Mark as defeated
                        pathTransitions.MarkBossAsDefeated(leftBoss.Boss);
                        Debugger.LogInformation(LogCategory.ProceduralGeneration,
                            $"[BossSelectionTest] Marked {leftBoss.Boss} as defeated");
                    }
                }
            }

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[BossSelectionTest] Total unique bosses selected: {selectedBosses.Count}");
        }

        private void ClearDefeatedBosses(ProceduralPathTransitions pathTransitions)
        {
            var defeatedBossesField = typeof(ProceduralPathTransitions).GetField("defeatedBosses",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (defeatedBossesField != null)
            {
                var defeatedBosses = defeatedBossesField.GetValue(pathTransitions) as System.Collections.Generic.List<string>;
                defeatedBosses?.Clear();

                // Update available bosses
                var updateMethod = typeof(ProceduralPathTransitions).GetMethod("UpdateAvailableBosses",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                updateMethod?.Invoke(pathTransitions, null);

                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    "[BossSelectionTest] Cleared defeated bosses list");
            }
        }

        [ContextMenu("Test Specific Seed")]
        public void TestSpecificSeed()
        {
            if (Application.isPlaying)
            {
                TestBossSelectionLogic();
            }
        }

        [ContextMenu("Test Preview System")]
        public void TestPreviewSystem()
        {
            if (!Application.isPlaying)
                return;

            var pathTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions == null)
                return;

            SetTestSeed(pathTransitions, testSeed);
            ClearDefeatedBosses(pathTransitions);

            var (leftChoice, rightChoice) = pathTransitions.PreviewNextChoices();

            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[BossSelectionTest] Preview - LEFT: {leftChoice?.DisplayName}, RIGHT: {rightChoice?.DisplayName}");

            if (leftChoice != null && rightChoice != null && leftChoice.Boss == rightChoice.Boss)
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration,
                    "[BossSelectionTest] Preview shows same boss for both paths!");
            }
        }
    }
}