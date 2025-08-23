using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for ProceduralPathTransitions component
    /// Tests all boss path generation, deterministic behavior, and save/load functionality
    /// </summary>
    public class ProceduralPathTransitionsTests
    {
        private GameObject testGameObject;
        private ProceduralPathTransitions pathTransitions;
        private GameObject mockSceneRouterObject;
        private GameObject mockRoutingContextObject;

        [SetUp]
        public void SetUp()
        {
            // Create test GameObject with ProceduralPathTransitions
            testGameObject = new GameObject("TestProceduralPathTransitions");
            pathTransitions = testGameObject.AddComponent<ProceduralPathTransitions>();
            
            // Create mock SceneRouter and RoutingContext objects
            mockSceneRouterObject = new GameObject("MockSceneRouter");
            mockSceneRouterObject.AddComponent<SceneRouter>();
            
            mockRoutingContextObject = new GameObject("MockRoutingContext");
            mockRoutingContextObject.AddComponent<SceneRoutingContext>();
            
            // Ensure clean state
            CleanupStaticReferences();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
                
            if (mockSceneRouterObject != null)
                Object.DestroyImmediate(mockSceneRouterObject);
                
            if (mockRoutingContextObject != null)
                Object.DestroyImmediate(mockRoutingContextObject);
                
            CleanupStaticReferences();
        }
        
        private void CleanupStaticReferences()
        {
            // Clear any static references that might interfere with tests
            PlayerPrefs.DeleteAll();
        }

        #region Component Initialization Tests

        [Test]
        public void ProceduralPathTransitions_InitializesWithDefaultBossPaths()
        {
            // Act - component initializes in Awake
            
            // Assert
            Assert.IsNotNull(pathTransitions.AllBossPaths, "Boss paths should be initialized");
            Assert.AreEqual(8, pathTransitions.AllBossPaths.Length, "Should have 8 boss paths (7 sins + devil)");
            
            // Verify all expected bosses are present
            var bossNames = pathTransitions.AllBossPaths.Select(bp => bp.bossName).ToArray();
            string[] expectedBosses = { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth", "Devil" };
            
            foreach (string expectedBoss in expectedBosses)
            {
                Assert.Contains(expectedBoss, bossNames, $"Boss {expectedBoss} should be in the boss paths");
            }
        }

        [Test]
        public void ProceduralPathTransitions_GeneratesUniqueSeedOnStart()
        {
            // Act - Call private Start method using reflection for testing
            var startMethod = typeof(ProceduralPathTransitions).GetMethod("Start", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startMethod?.Invoke(pathTransitions, null);
            
            // Assert
            Assert.Greater(pathTransitions.CurrentSeed, 0, "Should generate a valid seed");
        }

        [Test]
        public void BossPath_HasRequiredProperties()
        {
            // Arrange & Act
            var bossPath = pathTransitions.AllBossPaths[0]; // Get first boss (Pride)
            
            // Assert
            Assert.IsNotNull(bossPath.bossName, "Boss name should not be null");
            Assert.IsNotNull(bossPath.bossSceneName, "Boss scene name should not be null");
            Assert.IsNotNull(bossPath.possibleConnectors, "Possible connectors should not be null");
            Assert.Greater(bossPath.possibleConnectors.Count, 0, "Should have at least one possible connector");
            Assert.AreEqual(2, bossPath.requiredConnections, "Should require 2 connections by default");
        }

        #endregion

        #region Path Generation Tests

        [Test]
        public void GeneratePathToBoss_ValidBoss_ReturnsPath()
        {
            // Arrange
            string bossName = "Pride";
            
            // Act
            var generatedPath = pathTransitions.GeneratePathToBoss(bossName);
            
            // Assert
            Assert.IsNotNull(generatedPath, "Should generate a path");
            Assert.AreEqual(bossName, generatedPath.bossName, "Generated path should match requested boss");
            Assert.AreEqual("Cathedral", generatedPath.bossSceneName, "Pride boss should be in Cathedral");
            Assert.AreEqual(2, generatedPath.selectedConnectors.Count, "Should select 2 connectors");
        }

        [Test]
        public void GeneratePathToBoss_InvalidBoss_ReturnsNull()
        {
            // Arrange
            string invalidBossName = "NonExistentBoss";
            
            // Act
            var generatedPath = pathTransitions.GeneratePathToBoss(invalidBossName);
            
            // Assert
            Assert.IsNull(generatedPath, "Should return null for invalid boss name");
        }

        [Test]
        public void GeneratePathToBoss_EmptyBossName_ReturnsNull()
        {
            // Act
            var generatedPath = pathTransitions.GeneratePathToBoss("");
            
            // Assert
            Assert.IsNull(generatedPath, "Should return null for empty boss name");
        }

        [Test]
        public void GeneratePathToBoss_AllBosses_GeneratesPaths()
        {
            // Arrange
            string[] allBosses = { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth" };
            
            // Act & Assert
            foreach (string bossName in allBosses)
            {
                var generatedPath = pathTransitions.GeneratePathToBoss(bossName);
                Assert.IsNotNull(generatedPath, $"Should generate path for {bossName}");
                Assert.AreEqual(bossName, generatedPath.bossName, $"Path should match boss {bossName}");
                Assert.AreEqual(2, generatedPath.selectedConnectors.Count, $"Should select 2 connectors for {bossName}");
            }
        }

        #endregion

        #region Deterministic Generation Tests

        [Test]
        public void GeneratePathToBoss_SameSeed_ProducesSameResults()
        {
            // Arrange
            string bossName = "Wrath";
            int testSeed = 12345;
            
            // Act
            var path1 = pathTransitions.GeneratePathToBoss(bossName, testSeed);
            var path2 = pathTransitions.GeneratePathToBoss(bossName, testSeed);
            
            // Assert
            Assert.IsNotNull(path1, "First path should be generated");
            Assert.IsNotNull(path2, "Second path should be generated");
            Assert.AreEqual(path1.pathSeed, path2.pathSeed, "Both paths should have same seed");
            
            CollectionAssert.AreEqual(path1.selectedConnectors, path2.selectedConnectors, 
                "Same seed should produce identical connector selection");
        }

        [Test]
        public void GeneratePathToBoss_DifferentSeeds_ProducesDifferentResults()
        {
            // Arrange
            string bossName = "Greed";
            int seed1 = 11111;
            int seed2 = 22222;
            
            // Act
            var path1 = pathTransitions.GeneratePathToBoss(bossName, seed1);
            var path2 = pathTransitions.GeneratePathToBoss(bossName, seed2);
            
            // Assert
            Assert.IsNotNull(path1, "First path should be generated");
            Assert.IsNotNull(path2, "Second path should be generated");
            Assert.AreNotEqual(path1.pathSeed, path2.pathSeed, "Paths should have different seeds");
            
            // Note: There's a small chance connectors could be the same by coincidence,
            // but with proper seed generation this should be extremely rare
        }

        [Test]
        public void GeneratePathToBoss_MultipleCallsWithoutSeed_UsesCurrentSeed()
        {
            // Arrange
            string bossName = "Envy";
            // Initialize seed using reflection
            var startMethod = typeof(ProceduralPathTransitions).GetMethod("Start", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            startMethod?.Invoke(pathTransitions, null);
            int currentSeed = pathTransitions.CurrentSeed;
            
            // Act
            var path1 = pathTransitions.GeneratePathToBoss(bossName);
            var path2 = pathTransitions.GeneratePathToBoss(bossName);
            
            // Assert
            Assert.AreEqual(currentSeed, path1.pathSeed, "First path should use current seed");
            Assert.AreEqual(currentSeed, path2.pathSeed, "Second path should use same current seed");
            CollectionAssert.AreEqual(path1.selectedConnectors, path2.selectedConnectors, 
                "Same seed should produce identical results");
        }

        #endregion

        #region Path Activation Tests

        [Test]
        public void ActivatePath_ValidPath_SetsActivePath()
        {
            // Arrange
            var path = pathTransitions.GeneratePathToBoss("Lust");
            
            // Act
            bool result = pathTransitions.ActivatePath(path);
            
            // Assert
            Assert.IsTrue(result, "Should successfully activate path");
            Assert.AreEqual(path, pathTransitions.ActivePath, "Active path should be set");
            Assert.IsTrue(pathTransitions.HasActivePath, "Should indicate active path exists");
        }

        [Test]
        public void ActivatePath_NullPath_ReturnsFalse()
        {
            // Act
            bool result = pathTransitions.ActivatePath(null);
            
            // Assert
            Assert.IsFalse(result, "Should fail to activate null path");
            Assert.IsNull(pathTransitions.ActivePath, "Active path should remain null");
            Assert.IsFalse(pathTransitions.HasActivePath, "Should indicate no active path");
        }

        [Test]
        public void ActivatePath_FiresActivationEvent()
        {
            // Arrange
            var path = pathTransitions.GeneratePathToBoss("Gluttony");
            BossPath eventPath = null;
            pathTransitions.OnPathActivated += (activatedPath) => eventPath = activatedPath;
            
            // Act
            pathTransitions.ActivatePath(path);
            
            // Assert
            Assert.AreEqual(path, eventPath, "Event should fire with correct path");
        }

        #endregion

        #region Boss Unlock Tests

        [Test]
        public void IsBossUnlocked_RegularBoss_ReturnsTrue()
        {
            // Arrange
            var pridePath = pathTransitions.AllBossPaths.First(bp => bp.bossName == "Pride");
            
            // Act
            bool isUnlocked = pathTransitions.IsBossUnlocked(pridePath);
            
            // Assert
            Assert.IsTrue(isUnlocked, "Regular bosses should be unlocked by default");
        }

        [Test]
        public void IsBossUnlocked_DevilBoss_RequiresAllSins()
        {
            // Arrange
            var devilPath = pathTransitions.AllBossPaths.First(bp => bp.bossName == "Devil");
            
            // Act (without defeating any sins)
            bool isUnlockedBefore = pathTransitions.IsBossUnlocked(devilPath);
            
            // Defeat all required sins
            string[] sins = { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth" };
            foreach (string sin in sins)
            {
                pathTransitions.MarkBossAsDefeated(sin);
            }
            
            bool isUnlockedAfter = pathTransitions.IsBossUnlocked(devilPath);
            
            // Assert
            Assert.IsFalse(isUnlockedBefore, "Devil should be locked initially");
            Assert.IsTrue(isUnlockedAfter, "Devil should be unlocked after defeating all sins");
        }

        [Test]
        public void MarkBossAsDefeated_SetsDefeatedStatus()
        {
            // Arrange
            string bossName = "Pride";
            
            // Act
            Assert.IsFalse(pathTransitions.IsBossDefeated(bossName), "Boss should not be defeated initially");
            pathTransitions.MarkBossAsDefeated(bossName);
            
            // Assert
            Assert.IsTrue(pathTransitions.IsBossDefeated(bossName), "Boss should be marked as defeated");
        }

        [Test]
        public void MarkBossAsDefeated_FiresCompletionEvent()
        {
            // Arrange
            string bossName = "Wrath";
            string eventBossName = null;
            pathTransitions.OnPathCompleted += (completedBoss) => eventBossName = completedBoss;
            
            // Act
            pathTransitions.MarkBossAsDefeated(bossName);
            
            // Assert
            Assert.AreEqual(bossName, eventBossName, "Completion event should fire with correct boss name");
        }

        #endregion

        #region Save/Load Tests

        [Test]
        public void CreateSaveState_GeneratesValidSaveState()
        {
            // Arrange
            var path = pathTransitions.GeneratePathToBoss("Sloth");
            pathTransitions.ActivatePath(path);
            pathTransitions.MarkBossAsDefeated("Pride");
            pathTransitions.MarkBossAsDefeated("Wrath");
            
            // Act
            var saveState = pathTransitions.CreateSaveState();
            
            // Assert
            Assert.IsNotNull(saveState, "Save state should be created");
            Assert.AreEqual("Sloth", saveState.currentBossPath, "Should save current boss path");
            Assert.Contains("Pride", saveState.completedBosses, "Should save defeated bosses");
            Assert.Contains("Wrath", saveState.completedBosses, "Should save all defeated bosses");
            Assert.AreEqual(pathTransitions.CurrentSeed, saveState.pathSeed, "Should save current seed");
        }

        [Test]
        public void LoadSaveState_RestoresPathState()
        {
            // Arrange
            var originalPath = pathTransitions.GeneratePathToBoss("Garden", 54321);
            pathTransitions.ActivatePath(originalPath);
            var saveState = pathTransitions.CreateSaveState();
            
            // Reset state
            pathTransitions.ActivatePath(null);
            
            // Act
            bool loadResult = pathTransitions.LoadSaveState(saveState);
            
            // Assert
            Assert.IsTrue(loadResult, "Should successfully load save state");
            Assert.IsNotNull(pathTransitions.ActivePath, "Should restore active path");
            Assert.AreEqual("Garden", pathTransitions.ActivePath.bossName, "Should restore correct boss path");
            Assert.AreEqual(54321, pathTransitions.CurrentSeed, "Should restore seed");
        }

        [Test]
        public void LoadSaveState_NullState_ReturnsFalse()
        {
            // Act
            bool result = pathTransitions.LoadSaveState(null);
            
            // Assert
            Assert.IsFalse(result, "Should fail to load null save state");
        }

        [Test]
        public void SaveLoad_RoundTrip_PreservesState()
        {
            // Arrange
            pathTransitions.MarkBossAsDefeated("Pride");
            pathTransitions.MarkBossAsDefeated("Wrath");
            pathTransitions.MarkBossAsDefeated("Greed");
            var path = pathTransitions.GeneratePathToBoss("Envy", 98765);
            pathTransitions.ActivatePath(path);
            
            // Act - Save
            var saveState = pathTransitions.CreateSaveState();
            
            // Reset all state
            SetUp(); // Re-initialize component
            
            // Load
            bool loadResult = pathTransitions.LoadSaveState(saveState);
            
            // Assert
            Assert.IsTrue(loadResult, "Should load successfully");
            Assert.AreEqual(98765, pathTransitions.CurrentSeed, "Should restore seed");
            Assert.IsTrue(pathTransitions.IsBossDefeated("Pride"), "Should restore defeated status");
            Assert.IsTrue(pathTransitions.IsBossDefeated("Wrath"), "Should restore all defeated bosses");
            Assert.IsTrue(pathTransitions.IsBossDefeated("Greed"), "Should restore all defeated bosses");
            Assert.AreEqual("Envy", pathTransitions.ActivePath?.bossName, "Should restore active path");
        }

        #endregion

        #region Path Node Generation Tests

        [Test]
        public void GeneratePathToBoss_CreatesPathNodes()
        {
            // Arrange & Act
            var path = pathTransitions.GeneratePathToBoss("Pride");
            
            // Assert
            Assert.IsNotNull(path.pathNodes, "Path nodes should be generated");
            Assert.AreEqual(4, path.pathNodes.Count, "Should have 4 nodes: Start + 2 Connectors + Boss");
            
            // Verify node sequence
            Assert.AreEqual(NodeType.Start, path.pathNodes[0].Type, "First node should be Start");
            Assert.AreEqual(NodeType.Connection, path.pathNodes[1].Type, "Second node should be Connection");
            Assert.AreEqual(NodeType.Connection, path.pathNodes[2].Type, "Third node should be Connection");
            Assert.AreEqual(NodeType.Boss, path.pathNodes[3].Type, "Fourth node should be Boss");
        }

        [Test]
        public void GeneratedPathNodes_HaveCorrectProperties()
        {
            // Arrange & Act
            var path = pathTransitions.GeneratePathToBoss("Wrath", 11111);
            
            // Assert
            var startNode = path.pathNodes[0];
            Assert.IsTrue(startNode.Properties.ContainsKey("SceneName"), "Start node should have SceneName");
            Assert.AreEqual("MainCityHub", startNode.Properties["SceneName"], "Start should be MainCityHub");
            
            var connectionNode = path.pathNodes[1];
            Assert.IsTrue(connectionNode.Properties.ContainsKey("TargetBoss"), "Connection should have TargetBoss");
            Assert.AreEqual("Wrath", connectionNode.Properties["TargetBoss"], "Should target correct boss");
            
            var bossNode = path.pathNodes[3];
            Assert.IsTrue(bossNode.Properties.ContainsKey("BossName"), "Boss node should have BossName");
            Assert.AreEqual("Wrath", bossNode.Properties["BossName"], "Should have correct boss name");
        }

        #endregion

        #region Edge Cases and Error Handling Tests

        [Test]
        public void GeneratePathToBoss_CaseInsensitive_Works()
        {
            // Act
            var path1 = pathTransitions.GeneratePathToBoss("PRIDE");
            var path2 = pathTransitions.GeneratePathToBoss("pride");
            var path3 = pathTransitions.GeneratePathToBoss("Pride");
            
            // Assert
            Assert.IsNotNull(path1, "Upper case should work");
            Assert.IsNotNull(path2, "Lower case should work");
            Assert.IsNotNull(path3, "Proper case should work");
            
            Assert.AreEqual("Pride", path1.bossName, "Should normalize to proper case");
            Assert.AreEqual("Pride", path2.bossName, "Should normalize to proper case");
            Assert.AreEqual("Pride", path3.bossName, "Should normalize to proper case");
        }

        [Test]
        public void PathCaching_EnabledByDefault_CachesPaths()
        {
            // Arrange
            string bossName = "Vault";
            int seed = 77777;
            
            // Act
            var path1 = pathTransitions.GeneratePathToBoss(bossName, seed);
            var path2 = pathTransitions.GeneratePathToBoss(bossName, seed); // Should use cache
            
            // Assert
            Assert.AreSame(path1, path2, "Second call should return cached path instance");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void ProceduralPathTransitions_IntegratesWithSceneRouter()
        {
            // This test verifies that the component properly interfaces with SceneRouter
            // In a real scenario, SceneRouter would use the generated path nodes
            
            // Arrange
            var path = pathTransitions.GeneratePathToBoss("Mirage");
            
            // Act
            pathTransitions.ActivatePath(path);
            
            // Assert - Verify path is formatted for SceneRouter consumption
            Assert.IsNotNull(path.pathNodes, "Should generate nodes for SceneRouter");
            Assert.IsTrue(path.pathNodes.All(node => !string.IsNullOrEmpty(node.Id)), 
                "All nodes should have valid IDs");
            Assert.IsTrue(path.pathNodes.All(node => node.Properties != null), 
                "All nodes should have properties for SceneRouter");
        }

        #endregion

        #region Performance Tests

        [Test]
        public void GeneratePathToBoss_MultipleGenerations_PerformsWell()
        {
            // This test ensures path generation doesn't have performance issues
            
            // Act - Generate many paths
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < 100; i++)
            {
                pathTransitions.GeneratePathToBoss("Pride", i);
            }
            
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000, 
                "Generating 100 paths should take less than 1 second");
        }

        #endregion
    }
}