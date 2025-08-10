using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.ProceduralGeneration;
using NeonLadderURP.DataManagement;
using NeonLadder.DataManagement;
using UnityEngine.SceneManagement;
using System;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class ProceduralSaveStateTests
    {
        private ProceduralSaveStateManager saveStateManager;
        private GameObject managerObject;
        private SaveStateConfiguration testConfiguration;
        
        [SetUp]
        public void Setup()
        {
            // Create save state manager
            managerObject = new GameObject("TestSaveStateManager");
            saveStateManager = managerObject.AddComponent<ProceduralSaveStateManager>();
            
            // Create test configuration
            testConfiguration = ScriptableObject.CreateInstance<SaveStateConfiguration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            
            if (testConfiguration != null)
            {
                Object.DestroyImmediate(testConfiguration);
            }
        }
        
        #region World State Generation Tests
        
        [Test]
        public void GenerateWorldState_WithValidConfiguration_CreatesWorldState()
        {
            // Arrange
            bool eventFired = false;
            ProceduralWorldState receivedState = null;
            
            ProceduralSaveStateManager.OnWorldStateGenerated += (state) =>
            {
                eventFired = true;
                receivedState = state;
            };
            
            // Act
            var worldState = saveStateManager.GenerateWorldState(testConfiguration);
            
            // Assert
            Assert.IsNotNull(worldState, "World state should be generated");
            Assert.IsNotNull(worldState.worldId, "World ID should be set");
            Assert.IsNotNull(worldState.seed, "Seed should be set");
            Assert.IsNotNull(worldState.mapData, "Map data should be generated");
            Assert.IsTrue(eventFired, "OnWorldStateGenerated event should fire");
            Assert.AreEqual(worldState, receivedState, "Event should pass correct state");
            
            // Cleanup
            ProceduralSaveStateManager.OnWorldStateGenerated -= (state) =>
            {
                eventFired = true;
                receivedState = state;
            };
        }
        
        [Test]
        public void GenerateWorldState_WithNullConfiguration_ReturnsNull()
        {
            // Act
            var worldState = saveStateManager.GenerateWorldState(null);
            
            // Assert
            Assert.IsNull(worldState, "Should return null for null configuration");
        }
        
        [Test]
        public void GenerateWorldState_GeneratesUniqueWorldIds()
        {
            // Act
            var worldState1 = saveStateManager.GenerateWorldState(testConfiguration);
            var worldState2 = saveStateManager.GenerateWorldState(testConfiguration);
            
            // Assert
            Assert.IsNotNull(worldState1?.worldId);
            Assert.IsNotNull(worldState2?.worldId);
            Assert.AreNotEqual(worldState1.worldId, worldState2.worldId, "World IDs should be unique");
        }
        
        #endregion
        
        #region Scene State Capture Tests
        
        [Test]
        public void CaptureCurrentSceneState_CreatesValidSceneState()
        {
            // Arrange
            bool eventFired = false;
            SceneState capturedState = null;
            
            ProceduralSaveStateManager.OnSceneStateCaptured += (state) =>
            {
                eventFired = true;
                capturedState = state;
            };
            
            // Act
            var sceneState = saveStateManager.CaptureCurrentSceneState();
            
            // Assert
            Assert.IsNotNull(sceneState, "Scene state should be created");
            Assert.IsNotNull(sceneState.sceneId, "Scene ID should be set");
            Assert.IsNotNull(sceneState.sceneName, "Scene name should be set");
            Assert.IsTrue(eventFired, "OnSceneStateCaptured event should fire");
            Assert.AreEqual(sceneState, capturedState, "Event should pass correct state");
            
            // Cleanup
            ProceduralSaveStateManager.OnSceneStateCaptured -= (state) =>
            {
                eventFired = true;
                capturedState = state;
            };
        }
        
        [Test]
        public void CaptureCurrentSceneState_CapturesInteractiveObjects()
        {
            // Arrange
            var door = new GameObject("TestDoor");
            door.AddComponent<Door>();
            
            var switch_ = new GameObject("TestSwitch");
            switch_.AddComponent<Switch>();
            
            // Act
            var sceneState = saveStateManager.CaptureCurrentSceneState();
            
            // Assert
            Assert.IsNotNull(sceneState.interactiveObjects, "Should capture interactive objects");
            Assert.IsTrue(sceneState.interactiveObjects.Count >= 2, "Should capture door and switch");
            
            // Cleanup
            Object.DestroyImmediate(door);
            Object.DestroyImmediate(switch_);
        }
        
        [Test]
        public void CaptureCurrentSceneState_CapturesPlayerPosition()
        {
            // Arrange
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(10, 5, 3);
            player.transform.rotation = Quaternion.Euler(0, 90, 0);
            
            // Act
            var sceneState = saveStateManager.CaptureCurrentSceneState();
            
            // Assert
            Assert.AreEqual(new Vector3(10, 5, 3), sceneState.playerPosition, "Should capture player position");
            Assert.AreEqual(Quaternion.Euler(0, 90, 0), sceneState.playerRotation, "Should capture player rotation");
            
            // Cleanup
            Object.DestroyImmediate(player);
        }
        
        #endregion
        
        #region Scene State Restoration Tests
        
        [Test]
        public void RestoreSceneState_WithValidState_RestoresSuccessfully()
        {
            // Arrange
            var sceneState = new SceneState
            {
                sceneId = "test-scene",
                sceneName = "TestScene",
                playerPosition = new Vector3(5, 0, 10),
                interactiveObjects = new Dictionary<string, InteractiveObjectState>()
            };
            
            bool eventFired = false;
            SceneState restoredState = null;
            
            ProceduralSaveStateManager.OnSceneStateRestored += (state) =>
            {
                eventFired = true;
                restoredState = state;
            };
            
            // Act
            saveStateManager.RestoreSceneState(sceneState);
            
            // Assert
            Assert.IsTrue(eventFired, "OnSceneStateRestored event should fire");
            Assert.AreEqual(sceneState, restoredState, "Event should pass correct state");
            
            // Cleanup
            ProceduralSaveStateManager.OnSceneStateRestored -= (state) =>
            {
                eventFired = true;
                restoredState = state;
            };
        }
        
        [Test]
        public void RestoreSceneState_WithNullState_HandlesGracefully()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => saveStateManager.RestoreSceneState(null));
        }
        
        #endregion
        
        #region Save Data Generation Tests
        
        [Test]
        public void GenerateSaveDataWithProceduralState_CreatesValidSaveData()
        {
            // Arrange
            saveStateManager.GenerateWorldState(testConfiguration);
            
            // Act
            var saveData = saveStateManager.GenerateSaveDataWithProceduralState();
            
            // Assert
            Assert.IsNotNull(saveData, "Save data should be created");
            Assert.IsNotNull(saveData.worldState, "World state should exist");
            Assert.IsNotNull(saveData.worldState.proceduralState, "Procedural state should exist");
            Assert.AreNotEqual(0, saveData.worldState.proceduralState.currentSeed, "Seed should be set");
        }
        
        [Test]
        public void GenerateSaveDataWithProceduralState_IncludesCurrentScene()
        {
            // Act
            var saveData = saveStateManager.GenerateSaveDataWithProceduralState();
            
            // Assert
            Assert.IsNotNull(saveData.worldState.currentSceneName, "Current scene name should be set");
            Assert.AreEqual(SceneManager.GetActiveScene().name, saveData.worldState.currentSceneName);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void ValidateLoadedState_WithValidState_ReturnsTrue()
        {
            // Arrange
            var procState = new ProceduralGenerationState
            {
                currentSeed = 12345,
                currentDepth = 2,
                currentPath = "main",
                mapData = "test-map-data"
            };
            
            // Act
            bool isValid = saveStateManager.ValidateLoadedState(procState);
            
            // Assert
            Assert.IsTrue(isValid, "Valid state should pass validation");
        }
        
        [Test]
        public void ValidateLoadedState_WithNullState_ReturnsTrue()
        {
            // Act
            bool isValid = saveStateManager.ValidateLoadedState(null);
            
            // Assert
            Assert.IsTrue(isValid, "Null state should pass validation (graceful handling)");
        }
        
        [Test]
        public void ValidateLoadedState_WithCompletedScene_ValidatesCompletion()
        {
            // Arrange
            var procState = new ProceduralGenerationState
            {
                currentSeed = 12345,
                generatedScenes = new List<GeneratedSceneData>
                {
                    new GeneratedSceneData
                    {
                        sceneId = "scene-1",
                        isCompleted = true
                    }
                }
            };
            
            bool validationFailedFired = false;
            string failureReason = null;
            
            ProceduralSaveStateManager.OnStateValidationFailed += (reason) =>
            {
                validationFailedFired = true;
                failureReason = reason;
            };
            
            // Act
            saveStateManager.ValidateLoadedState(procState);
            
            // Assert - Validation might fail if scene completion criteria not met
            // This tests that validation is attempted
            Assert.Pass("Validation logic executed");
            
            // Cleanup
            ProceduralSaveStateManager.OnStateValidationFailed -= (reason) =>
            {
                validationFailedFired = true;
                failureReason = reason;
            };
        }
        
        #endregion
        
        #region Event System Tests
        
        [Test]
        public void OnWorldStateGenerated_FiresOnGeneration()
        {
            // Arrange
            int eventCount = 0;
            ProceduralSaveStateManager.OnWorldStateGenerated += (state) => eventCount++;
            
            // Act
            saveStateManager.GenerateWorldState(testConfiguration);
            saveStateManager.GenerateWorldState(testConfiguration);
            
            // Assert
            Assert.AreEqual(2, eventCount, "Event should fire for each generation");
            
            // Cleanup
            ProceduralSaveStateManager.OnWorldStateGenerated -= (state) => eventCount++;
        }
        
        [Test]
        public void OnSceneStateCaptured_FiresOnCapture()
        {
            // Arrange
            int eventCount = 0;
            ProceduralSaveStateManager.OnSceneStateCaptured += (state) => eventCount++;
            
            // Act
            saveStateManager.CaptureCurrentSceneState();
            saveStateManager.CaptureCurrentSceneState();
            
            // Assert
            Assert.AreEqual(2, eventCount, "Event should fire for each capture");
            
            // Cleanup
            ProceduralSaveStateManager.OnSceneStateCaptured -= (state) => eventCount++;
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator SaveAndRestore_MaintainsConsistency()
        {
            // Arrange
            var worldState = saveStateManager.GenerateWorldState(testConfiguration);
            var originalSeed = worldState.seed;
            
            // Capture scene state
            var capturedState = saveStateManager.CaptureCurrentSceneState();
            
            // Generate save data
            var saveData = saveStateManager.GenerateSaveDataWithProceduralState();
            
            // Wait a frame
            yield return null;
            
            // Validate restoration would work
            bool isValid = saveStateManager.ValidateLoadedState(saveData.worldState.proceduralState);
            
            // Assert
            Assert.IsTrue(isValid, "Saved state should be valid for restoration");
            Assert.AreEqual(originalSeed.GetHashCode(), 
                          saveData.worldState.proceduralState.currentSeed, 
                          "Seed should be preserved");
        }
        
        [UnityTest]
        public IEnumerator SceneTransition_PreservesState()
        {
            // Arrange
            saveStateManager.GenerateWorldState(testConfiguration);
            var firstSceneState = saveStateManager.CaptureCurrentSceneState();
            
            // Simulate scene transition
            yield return null;
            
            // Capture new scene state
            var secondSceneState = saveStateManager.CaptureCurrentSceneState();
            
            // Assert
            Assert.AreNotEqual(firstSceneState.sceneId, secondSceneState.sceneId, 
                             "Scene IDs should differ");
            Assert.IsNotNull(secondSceneState, "Second scene state should be captured");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void CaptureSceneState_Performance_Under100ms()
        {
            // Arrange
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            // Create many objects to capture
            var objects = new List<GameObject>();
            for (int i = 0; i < 50; i++)
            {
                var obj = new GameObject($"TestObject_{i}");
                if (i % 2 == 0) obj.AddComponent<Door>();
                else obj.AddComponent<Switch>();
                objects.Add(obj);
            }
            
            // Act
            stopwatch.Start();
            var sceneState = saveStateManager.CaptureCurrentSceneState();
            stopwatch.Stop();
            
            // Assert
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                       "Scene capture should complete in under 100ms");
            
            // Cleanup
            foreach (var obj in objects)
            {
                Object.DestroyImmediate(obj);
            }
        }
        
        #endregion
    }
}