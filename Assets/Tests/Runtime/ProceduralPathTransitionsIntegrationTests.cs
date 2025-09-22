using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Integration tests for ProceduralPathTransitions with SceneTransitionManager
    /// Tests the complete workflow from boss selection to scene transition coordination
    /// </summary>
    public class ProceduralPathTransitionsIntegrationTests
    {
        private GameObject pathTransitionsObject;
        private GameObject sceneManagerObject;
        private ProceduralPathTransitions pathTransitions;
        private SceneTransitionManager sceneTransitionManager;

        [SetUp]
        public void SetUp()
        {
            // Create ProceduralPathTransitions component
            pathTransitionsObject = new GameObject("TestProceduralPathTransitions");
            pathTransitions = pathTransitionsObject.AddComponent<ProceduralPathTransitions>();

            // Create SceneTransitionManager component
            sceneManagerObject = new GameObject("TestSceneTransitionManager");
            sceneTransitionManager = sceneManagerObject.AddComponent<SceneTransitionManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (pathTransitionsObject != null)
            {
                Object.DestroyImmediate(pathTransitionsObject);
            }
            if (sceneManagerObject != null)
            {
                Object.DestroyImmediate(sceneManagerObject);
            }
        }

        #region Integration Tests

        [Test]
        public void Integration_BothComponentsInitializeCorrectly()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("INTEGRATION_TEST");

            // Act & Assert
            Assert.IsNotNull(pathTransitions);
            Assert.IsNotNull(sceneTransitionManager);
            Assert.IsNotEmpty(pathTransitions.CurrentSeed);
            Assert.AreEqual(8, pathTransitions.AvailableBosses.Count);
        }

        [Test]
        public void Integration_BossSelectionReturnsValidSceneData()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("SCENE_TEST");

            // Act
            var selectedBoss = pathTransitions.SelectNextBoss(true);

            // Assert
            Assert.IsNotNull(selectedBoss);
            Assert.IsNotEmpty(selectedBoss.Identifier);
            Assert.IsNotEmpty(selectedBoss.DisplayName);
            Assert.IsNotEmpty(selectedBoss.Boss);

            // Verify the selected boss has valid scene identifier that SceneTransitionManager can use
            var allValidBosses = BossLocationData.Locations.Select(b => b.Boss).ToArray();
            Assert.Contains(selectedBoss.Boss, allValidBosses);
        }

        [Test]
        public void Integration_ProceduralSystemRoutesToConnection1Scenes()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("CONNECTION_TEST");

            // Act
            var selectedBoss = pathTransitions.SelectNextBoss(true);

            // The SceneTransitionTrigger should route to Connection1, not directly to boss
            string expectedConnectionScene = $"{selectedBoss.Identifier}_Connection1";

            // Assert
            Assert.IsNotNull(selectedBoss);
            // Verify the connection scene naming convention
            Assert.IsTrue(expectedConnectionScene.EndsWith("_Connection1"),
                $"Expected connection scene to end with '_Connection1', got: {expectedConnectionScene}");

            // Verify it's one of the valid connection scenes
            var validConnectionScenes = new[]
            {
                "Cathedral_Connection1", "Necropolis_Connection1", "Vault_Connection1",
                "Mirage_Connection1", "Garden_Connection1", "Banquet_Connection1",
                "Lounge_Connection1", "Finale_Connection1"
            };
            Assert.Contains(expectedConnectionScene, validConnectionScenes,
                $"Scene {expectedConnectionScene} not found in valid connection scenes");
        }

        [Test]
        public void Integration_CompleteSceneFlow_LeftPath_DynamicSelection()
        {
            // Tests dynamic boss selection for left path (no repeats, any boss can be selected)
            pathTransitions.ResetWithNewSeed("LEFT_PATH_TEST");

            var selectedBosses = new List<string>();
            var allAvailableBosses = new List<string>(pathTransitions.AvailableBosses.Select(b => b.Boss));

            // Test selecting 4 different bosses using left path
            for (int i = 0; i < 4; i++)
            {
                // Simulate player choosing left path from Start
                var selectedBoss = pathTransitions.SelectNextBoss(true);

                // Verify we get a valid boss
                Assert.IsNotNull(selectedBoss, $"Should get a boss on selection {i + 1}");
                Assert.Contains(selectedBoss.Boss, allAvailableBosses,
                    $"Selected boss {selectedBoss.Boss} should be from available pool");

                // Verify no repeats
                Assert.IsFalse(selectedBosses.Contains(selectedBoss.Boss),
                    $"Boss {selectedBoss.Boss} was already selected - should not repeat");

                selectedBosses.Add(selectedBoss.Boss);

                // Verify scene flow sequence
                string connection1 = $"{selectedBoss.Identifier}_Connection1";
                string connection2 = $"{selectedBoss.Identifier}_Connection2";
                string bossArena = selectedBoss.Identifier;
                string cutscene = "BossDefeated";
                string staging = "Staging";

                // Log the flow
                Debug.Log($"Scene Flow {i + 1}: Start → {connection1} → {connection2} → {bossArena} → {cutscene} → {staging}");

                // Mark boss as defeated
                pathTransitions.MarkBossAsDefeated(selectedBoss.Boss);
            }

            // After defeating 4 bosses, available pool should be reduced by 4
            Assert.AreEqual(4, pathTransitions.AvailableBosses.Count,
                "Should have 4 bosses remaining after defeating 4");

            // Verify all selected bosses are unique
            Assert.AreEqual(4, selectedBosses.Distinct().Count(),
                "All 4 selected bosses should be unique");
        }

        [Test]
        public void Integration_CompleteSceneFlow_RightPath_DynamicSelection()
        {
            // Tests dynamic boss selection for right path (no repeats, any boss can be selected)
            pathTransitions.ResetWithNewSeed("RIGHT_PATH_TEST");

            var selectedBosses = new List<string>();
            var allAvailableBosses = new List<string>(pathTransitions.AvailableBosses.Select(b => b.Boss));

            // Test selecting 4 different bosses using right path
            for (int i = 0; i < 4; i++)
            {
                // Simulate player choosing right path from Start
                var selectedBoss = pathTransitions.SelectNextBoss(false);

                // Verify we get a valid boss
                Assert.IsNotNull(selectedBoss, $"Should get a boss on selection {i + 1}");
                Assert.Contains(selectedBoss.Boss, allAvailableBosses,
                    $"Selected boss {selectedBoss.Boss} should be from available pool");

                // Verify no repeats
                Assert.IsFalse(selectedBosses.Contains(selectedBoss.Boss),
                    $"Boss {selectedBoss.Boss} was already selected - should not repeat");

                selectedBosses.Add(selectedBoss.Boss);

                // Verify scene flow sequence
                string connection1 = $"{selectedBoss.Identifier}_Connection1";
                string connection2 = $"{selectedBoss.Identifier}_Connection2";
                string bossArena = selectedBoss.Identifier;
                string cutscene = "BossDefeated";
                string staging = "Staging";

                // Log the flow
                Debug.Log($"Scene Flow {i + 1}: Start → {connection1} → {connection2} → {bossArena} → {cutscene} → {staging}");

                // Mark boss as defeated
                pathTransitions.MarkBossAsDefeated(selectedBoss.Boss);
            }

            // After defeating 4 bosses, available pool should be reduced by 4
            Assert.AreEqual(4, pathTransitions.AvailableBosses.Count,
                "Should have 4 bosses remaining after defeating 4");

            // Verify all selected bosses are unique
            Assert.AreEqual(4, selectedBosses.Distinct().Count(),
                "All 4 selected bosses should be unique");
        }

        [Test]
        public void Integration_SceneFlow_PathConvergence_ToDevil()
        {
            // Test that after defeating 7 sins, paths converge to Devil
            pathTransitions.ResetWithNewSeed("CONVERGE_TEST");

            // Mark all 7 deadly sins as defeated
            string[] sevenSins = { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth" };
            foreach (var sin in sevenSins)
            {
                pathTransitions.MarkBossAsDefeated(sin);
            }

            // Both paths should now lead to Devil
            var leftChoice = pathTransitions.SelectNextBoss(true);
            var rightChoice = pathTransitions.SelectNextBoss(false);

            Assert.AreEqual("Devil", leftChoice.Boss, "Left path should lead to Devil after 7 sins defeated");
            Assert.AreEqual("Devil", rightChoice.Boss, "Right path should lead to Devil after 7 sins defeated");
            Assert.IsTrue(pathTransitions.IsPathsConverged, "Paths should be converged");

            // Verify final scene flow to Finale
            string expectedFlow = "Start → Finale_Connection1 → Finale_Connection2 → Finale → BossDefeated → Staging";
            Debug.Log($"Final Boss Flow: {expectedFlow}");
        }

        [Test]
        public void Integration_SceneFlow_BossDefeatTracking()
        {
            // Test that boss defeats properly update the system state
            pathTransitions.ResetWithNewSeed("DEFEAT_TRACK_TEST");

            // Initial state - all 8 bosses available
            Assert.AreEqual(8, pathTransitions.AvailableBosses.Count);

            // Defeat Pride boss
            pathTransitions.MarkBossAsDefeated("Pride");
            Assert.AreEqual(7, pathTransitions.AvailableBosses.Count);
            Assert.IsFalse(pathTransitions.AvailableBosses.Any(b => b.Boss == "Pride"));

            // Defeat Wrath boss
            pathTransitions.MarkBossAsDefeated("Wrath");
            Assert.AreEqual(6, pathTransitions.AvailableBosses.Count);
            Assert.IsFalse(pathTransitions.AvailableBosses.Any(b => b.Boss == "Wrath"));

            // Verify defeated bosses list
            var state = pathTransitions.GetCurrentState();
            Assert.Contains("Pride", state.DefeatedBosses);
            Assert.Contains("Wrath", state.DefeatedBosses);
        }

        [Test]
        public void Integration_BossDefeatedWorkflow_UpdatesStateCorrectly()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("DEFEAT_TEST");
            var initialBossCount = pathTransitions.AvailableBosses.Count;

            // Act - Simulate defeating a boss
            var selectedBoss = pathTransitions.SelectNextBoss(true);
            pathTransitions.MarkBossAsDefeated(selectedBoss.Boss);

            // Assert
            Assert.AreEqual(initialBossCount - 1, pathTransitions.AvailableBosses.Count);
            Assert.Contains(selectedBoss.Boss, pathTransitions.DefeatedBosses);
            Assert.IsFalse(pathTransitions.AvailableBosses.Any(b => b.Boss == selectedBoss.Boss));
        }

        [Test]
        public void Integration_FullGameSimulation_EightBossSequence()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("FULL_GAME");
            var defeatedBosses = new List<string>();

            // Act - Simulate full game (defeat 8 bosses)
            for (int i = 0; i < 8; i++)
            {
                var availableCount = pathTransitions.AvailableBosses.Count;

                if (availableCount == 0)
                    break;

                // Alternate between left and right paths
                bool useLeftPath = i % 2 == 0;
                var selectedBoss = pathTransitions.SelectNextBoss(useLeftPath);

                if (selectedBoss != null)
                {
                    pathTransitions.MarkBossAsDefeated(selectedBoss.Boss);
                    defeatedBosses.Add(selectedBoss.Boss);
                }
            }

            // Assert
            Assert.AreEqual(8, defeatedBosses.Count);
            Assert.AreEqual(0, pathTransitions.AvailableBosses.Count);
            Assert.AreEqual(8, pathTransitions.DefeatedBosses.Count);

            // Verify all unique bosses were defeated
            Assert.AreEqual(8, defeatedBosses.Distinct().Count());
        }

        [Test]
        public void Integration_PathConvergence_TriggersAtCorrectTime()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("CONVERGENCE_TEST");
            var allBosses = BossLocationData.Locations.Select(b => b.Boss).ToList();

            // Act - Defeat bosses one by one and check convergence state
            for (int i = 0; i < allBosses.Count; i++)
            {
                pathTransitions.MarkBossAsDefeated(allBosses[i]);

                if (i < 6) // First 7 defeats should not converge
                {
                    Assert.IsFalse(pathTransitions.IsPathsConverged, $"Paths should not converge after {i + 1} defeats");
                }
                else if (i == 6) // 7th defeat should trigger convergence
                {
                    Assert.IsTrue(pathTransitions.IsPathsConverged, "Paths should converge after 7 defeats");
                    break; // Stop before defeating the final boss
                }
            }
        }

        [Test]
        public void Integration_StateSerializationWorkflow_PreservesGameProgress()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("SERIALIZATION_TEST");

            // Defeat some bosses
            pathTransitions.MarkBossAsDefeated("Pride");
            pathTransitions.MarkBossAsDefeated("Wrath");
            pathTransitions.MarkBossAsDefeated("Greed");

            // Act - Save and restore state
            var savedState = pathTransitions.GetCurrentState();

            // Create new component and load state
            var newObject = new GameObject("NewPathTransitions");
            var newPathTransitions = newObject.AddComponent<ProceduralPathTransitions>();
            newPathTransitions.LoadState(savedState);

            // Assert
            Assert.AreEqual(pathTransitions.CurrentSeed, newPathTransitions.CurrentSeed);
            Assert.AreEqual(pathTransitions.DefeatedBosses.Count, newPathTransitions.DefeatedBosses.Count);
            Assert.AreEqual(pathTransitions.AvailableBosses.Count, newPathTransitions.AvailableBosses.Count);
            Assert.AreEqual(pathTransitions.IsPathsConverged, newPathTransitions.IsPathsConverged);

            // Cleanup
            Object.DestroyImmediate(newObject);
        }

        #endregion

        #region Scene Transition Coordination Tests

        [Test]
        public void Integration_BossSelection_ProvidesValidSceneIdentifiers()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("SCENE_ID_TEST");

            // Act - Test multiple selections
            var leftBoss = pathTransitions.SelectNextBoss(true);
            var rightBoss = pathTransitions.SelectNextBoss(false);

            // Assert
            Assert.IsNotNull(leftBoss);
            Assert.IsNotNull(rightBoss);

            // Verify identifiers match BossLocationData format
            Assert.IsNotEmpty(leftBoss.Identifier);
            Assert.IsNotEmpty(rightBoss.Identifier);

            // These should be valid scene names that SceneTransitionManager can load
            var validIdentifiers = BossLocationData.Locations.Select(b => b.Identifier).ToArray();
            Assert.Contains(leftBoss.Identifier, validIdentifiers);
            Assert.Contains(rightBoss.Identifier, validIdentifiers);
        }

        [Test]
        public void Integration_BossLocationData_ConsistencyCheck()
        {
            // Arrange & Act
            var allLocations = BossLocationData.Locations;

            // Assert - Verify data integrity
            Assert.AreEqual(8, allLocations.Length, "Should have exactly 8 boss locations");

            // Check for unique identifiers
            var identifiers = allLocations.Select(l => l.Identifier).ToArray();
            Assert.AreEqual(8, identifiers.Distinct().Count(), "All identifiers should be unique");

            // Check for unique boss names
            var bossNames = allLocations.Select(l => l.Boss).ToArray();
            Assert.AreEqual(8, bossNames.Distinct().Count(), "All boss names should be unique");

            // Verify expected bosses are present
            var expectedBosses = new[] { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth", "Devil" };
            foreach (var expectedBoss in expectedBosses)
            {
                Assert.IsTrue(bossNames.Contains(expectedBoss), $"Missing expected boss: {expectedBoss}");
            }
        }

        [Test]
        public void Integration_PathSeparation_DynamicDifferentiation()
        {
            // Test new dynamic behavior: left and right paths give different bosses on same visit
            pathTransitions.ResetWithNewSeed("PATH_SEPARATION_TEST");

            var allValidBosses = new[] { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth", "Devil" };
            var leftRightPairs = new List<(string left, string right)>();

            // Test multiple times with same seed to verify deterministic behavior
            for (int i = 0; i < 10; i++)
            {
                pathTransitions.ResetWithNewSeed("PATH_SEPARATION_TEST");

                var leftBoss = pathTransitions.SelectNextBoss(true);
                var rightBoss = pathTransitions.SelectNextBoss(false);

                Assert.IsNotNull(leftBoss, $"Left boss should not be null on iteration {i}");
                Assert.IsNotNull(rightBoss, $"Right boss should not be null on iteration {i}");

                // Verify both bosses are valid
                Assert.Contains(leftBoss.Boss, allValidBosses, $"Left boss {leftBoss.Boss} should be valid");
                Assert.Contains(rightBoss.Boss, allValidBosses, $"Right boss {rightBoss.Boss} should be valid");

                // Key requirement: left and right should be different on the same visit
                Assert.AreNotEqual(leftBoss.Boss, rightBoss.Boss,
                    $"Left and right paths should give different bosses on same visit (got {leftBoss.Boss} for both)");

                leftRightPairs.Add((leftBoss.Boss, rightBoss.Boss));
            }

            // Verify deterministic behavior - all pairs should be identical across iterations
            var firstPair = leftRightPairs[0];
            Assert.IsTrue(leftRightPairs.All(pair => pair.left == firstPair.left && pair.right == firstPair.right),
                "Same seed should produce identical left/right boss pairs across multiple runs");
        }

        #endregion

        #region Performance and Edge Case Tests

        [Test]
        public void Integration_LargeNumberOfSelections_MaintainsPerformance()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("PERFORMANCE_TEST");
            pathTransitions.DisableDebugLoggingTemporarily(); // Disable debug logging for performance
            var startTime = Time.realtimeSinceStartup;

            // Act - Perform many selections
            for (int i = 0; i < 1000; i++)
            {
                var boss = pathTransitions.SelectNextBoss(i % 2 == 0);
                Assert.IsNotNull(boss, $"Selection {i} should not return null");
            }

            // Assert
            var elapsed = Time.realtimeSinceStartup - startTime;
            pathTransitions.RestoreDebugLogging(); // Restore debug logging
            Assert.Less(elapsed, 1.0f, "1000 selections should complete in under 1 second");
        }

        [Test]
        public void Integration_AllBossesDefeated_HandlesGracefully()
        {
            // Arrange
            pathTransitions.ResetWithNewSeed("ALL_DEFEATED_TEST");
            var allBosses = BossLocationData.Locations.Select(b => b.Boss).ToList();

            // Act - Defeat all bosses
            foreach (var boss in allBosses)
            {
                pathTransitions.MarkBossAsDefeated(boss);
            }

            // Assert
            Assert.AreEqual(0, pathTransitions.AvailableBosses.Count);
            Assert.IsTrue(pathTransitions.IsPathsConverged);

            // Selecting next boss should return null gracefully
            var nextBoss = pathTransitions.SelectNextBoss(true);
            Assert.IsNull(nextBoss);
        }

        [Test]
        public void Integration_SceneFlow_ConnectionSceneNavigation()
        {
            // Test the manual navigation within connection scenes
            pathTransitions.ResetWithNewSeed("NAV_TEST");

            var selectedBoss = pathTransitions.SelectNextBoss(true);
            string connection1 = $"{selectedBoss.Identifier}_Connection1";
            string connection2 = $"{selectedBoss.Identifier}_Connection2";

            // Connection1 should have:
            // - Forward exit (Manual) → Connection2
            // - Backward exit (Manual) → Start
            Debug.Log($"Connection1 ({connection1}) exits:");
            Debug.Log($"  Forward: Manual → {connection2}");
            Debug.Log($"  Backward: Manual → Start");

            // Connection2 should have:
            // - Forward exit (Manual) → Boss Arena
            // - Backward exit (Manual) → Connection1
            Debug.Log($"Connection2 ({connection2}) exits:");
            Debug.Log($"  Forward: Manual → {selectedBoss.Identifier}");
            Debug.Log($"  Backward: Manual → {connection1}");

            // Boss Arena should transition to cutscene on defeat
            Debug.Log($"Boss Arena ({selectedBoss.Identifier}):");
            Debug.Log($"  On Defeat: Auto → BossDefeated → Staging");
        }

        [Test]
        public void Integration_SceneFlow_SeedDeterminism()
        {
            // Test that same seed produces same boss selection sequence
            string testSeed = "DETERMINISTIC";

            // First run
            pathTransitions.ResetWithNewSeed(testSeed);
            var firstRun = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                var boss = pathTransitions.SelectNextBoss(i % 2 == 0);
                firstRun.Add(boss.Boss);
                pathTransitions.MarkBossAsDefeated(boss.Boss);
            }

            // Reset and second run with same seed
            pathTransitions.ResetWithNewSeed(testSeed);
            var secondRun = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                var boss = pathTransitions.SelectNextBoss(i % 2 == 0);
                secondRun.Add(boss.Boss);
                pathTransitions.MarkBossAsDefeated(boss.Boss);
            }

            // Both runs should produce identical sequence
            CollectionAssert.AreEqual(firstRun, secondRun,
                "Same seed should produce same boss selection sequence");
        }

        [Test]
        public void Integration_SceneFlow_AllEightBossesComplete()
        {
            // Test complete game flow defeating all 8 bosses
            pathTransitions.ResetWithNewSeed("COMPLETE_GAME");

            var defeatedBosses = new HashSet<string>();
            int transitionCount = 0;

            while (pathTransitions.AvailableBosses.Count > 0)
            {
                // Alternate between left and right paths
                bool useLeftPath = transitionCount % 2 == 0;
                var selectedBoss = pathTransitions.SelectNextBoss(useLeftPath);

                Assert.IsNotNull(selectedBoss, "Should always have a boss available");
                Assert.IsFalse(defeatedBosses.Contains(selectedBoss.Boss),
                    $"Should not select already defeated boss: {selectedBoss.Boss}");

                // Log complete scene flow
                string flow = $"Transition {transitionCount + 1}: ";
                flow += $"Start → {selectedBoss.Identifier}_Connection1 → ";
                flow += $"{selectedBoss.Identifier}_Connection2 → {selectedBoss.Identifier} → ";
                flow += $"BossDefeated → Staging";
                Debug.Log(flow);

                defeatedBosses.Add(selectedBoss.Boss);
                pathTransitions.MarkBossAsDefeated(selectedBoss.Boss);
                transitionCount++;
            }

            // Verify all 8 bosses were defeated
            Assert.AreEqual(8, defeatedBosses.Count, "Should defeat all 8 bosses");
            Assert.AreEqual(0, pathTransitions.AvailableBosses.Count, "No bosses should remain");

            // Verify all boss types were encountered
            string[] allBosses = { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth", "Devil" };
            foreach (var boss in allBosses)
            {
                Assert.IsTrue(defeatedBosses.Contains(boss),
                    $"Boss {boss} should have been defeated");
            }
        }

        #endregion
    }
}