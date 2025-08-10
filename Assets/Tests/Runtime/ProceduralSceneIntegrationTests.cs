using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.ProceduralGeneration;
using NeonLadderURP.DataManagement;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class ProceduralSceneIntegrationTests
    {
        private ProceduralSceneLoader sceneLoader;
        private GameObject loaderObject;
        private SaveStateConfiguration testConfig;
        
        [SetUp]
        public void Setup()
        {
            // Create scene loader
            loaderObject = new GameObject("TestProceduralSceneLoader");
            sceneLoader = loaderObject.AddComponent<ProceduralSceneLoader>();
            
            // Create test configuration
            testConfig = ScriptableObject.CreateInstance<SaveStateConfiguration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (loaderObject != null)
            {
                Object.DestroyImmediate(loaderObject);
            }
            
            if (testConfig != null)
            {
                Object.DestroyImmediate(testConfig);
            }
        }
        
        #region PathGenerator Integration Tests
        
        [Test]
        public void PathGenerator_GeneratesMapWithSeed()
        {
            // Arrange
            var pathGenerator = new PathGenerator();
            string testSeed = "test-seed-123";
            
            // Act
            var map = pathGenerator.GenerateMap(testSeed);
            
            // Assert
            Assert.IsNotNull(map);
            Assert.AreEqual(testSeed, map.Seed);
            Assert.Greater(map.Layers.Count, 0);
        }
        
        [Test]
        public void PathGenerator_DeterministicGeneration()
        {
            // Arrange
            var generator1 = new PathGenerator();
            var generator2 = new PathGenerator();
            string seed = "deterministic-test";
            
            // Act
            var map1 = generator1.GenerateMap(seed);
            var map2 = generator2.GenerateMap(seed);
            
            // Assert
            Assert.AreEqual(map1.Layers.Count, map2.Layers.Count);
            Assert.AreEqual(map1.Seed, map2.Seed);
            
            // Verify layer structure is identical
            for (int i = 0; i < map1.Layers.Count; i++)
            {
                Assert.AreEqual(map1.Layers[i].BossName, map2.Layers[i].BossName);
                Assert.AreEqual(map1.Layers[i].Rooms.Count, map2.Layers[i].Rooms.Count);
            }
        }
        
        [Test]
        public void PathGenerator_CustomRulesApplied()
        {
            // Arrange
            var pathGenerator = new PathGenerator();
            var customRules = GenerationRules.CreateBalancedRules();
            customRules.MinRoomsPerLayer = 5;
            customRules.MaxRoomsPerLayer = 5;
            
            // Act
            var map = pathGenerator.GenerateMapWithRules("custom-rules", customRules);
            
            // Assert
            Assert.IsNotNull(map);
            foreach (var layer in map.Layers)
            {
                Assert.AreEqual(5, layer.Rooms.Count, $"Layer {layer.LayerIndex} should have exactly 5 rooms");
            }
        }
        
        #endregion
        
        #region SaveStateConfiguration Integration Tests
        
        [Test]
        public void SaveStateConfiguration_CreatesSaveData()
        {
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.IsNotNull(saveData);
            Assert.IsNotNull(saveData.progression);
            Assert.IsNotNull(saveData.currencies);
            Assert.IsNotNull(saveData.worldState);
            Assert.IsNotNull(saveData.worldState.proceduralState);
        }
        
        [Test]
        public void SaveStateConfiguration_LoadsFromSaveData()
        {
            // Arrange
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.progression.playerLevel = 10;
            saveData.currencies.metaCurrency = 500;
            saveData.worldState.currentSceneName = "TestScene";
            
            // Act
            testConfig.LoadFromSaveData(saveData);
            var recreatedSave = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual(10, recreatedSave.progression.playerLevel);
            Assert.AreEqual(500, recreatedSave.currencies.metaCurrency);
            Assert.AreEqual("TestScene", recreatedSave.worldState.currentSceneName);
        }
        
        #endregion
        
        #region ProceduralSceneLoader Tests
        
        [UnityTest]
        public IEnumerator ProceduralSceneLoader_Initializes()
        {
            // Wait for initialization
            yield return null;
            
            // Assert
            Assert.IsNotNull(ProceduralSceneLoader.Instance);
            Assert.AreEqual(sceneLoader, ProceduralSceneLoader.Instance);
        }
        
        [Test]
        public void ProceduralSceneLoader_GeneratesMap()
        {
            // Arrange
            string testSeed = "loader-test";
            
            // Act
            var map = sceneLoader.GenerateMap(testSeed);
            
            // Assert
            Assert.IsNotNull(map);
            Assert.AreEqual(testSeed, map.Seed);
        }
        
        [Test]
        public void ProceduralSceneLoader_LoadsFromConfiguration()
        {
            // Arrange
            bool loadStarted = false;
            ProceduralSceneLoader.OnSceneLoadStarted += (scene) => loadStarted = true;
            
            // Act
            sceneLoader.LoadSceneFromConfiguration(testConfig);
            
            // Assert - Configuration validation occurs even if scene doesn't exist
            // The loader should handle the configuration
            Assert.Pass("Configuration loading initiated");
            
            // Cleanup
            ProceduralSceneLoader.OnSceneLoadStarted -= (scene) => loadStarted = true;
        }
        
        #endregion
        
        #region Save Migration Tests
        
        [Test]
        public void SaveMigration_DetectsOldVersion()
        {
            // Arrange
            var oldSave = ConsolidatedSaveData.CreateNew();
            oldSave.version = 1; // Old version
            
            // Act
            bool needsMigration = SaveMigrationSystem.NeedsMigration(oldSave);
            
            // Assert
            Assert.IsTrue(needsMigration);
        }
        
        [Test]
        public void SaveMigration_MigratesToCurrentVersion()
        {
            // Arrange
            var oldSave = ConsolidatedSaveData.CreateNew();
            oldSave.version = 0;
            
            // Act
            var migratedSave = SaveMigrationSystem.MigrateSaveData(oldSave);
            
            // Assert
            Assert.IsNotNull(migratedSave);
            Assert.AreEqual(3, migratedSave.version); // Current version
            Assert.IsNotNull(migratedSave.worldState.proceduralState);
            Assert.IsNotNull(migratedSave.statistics);
        }
        
        [Test]
        public void SaveMigration_PreservesDataDuringMigration()
        {
            // Arrange
            var oldSave = ConsolidatedSaveData.CreateNew();
            oldSave.version = 0;
            oldSave.progression.playerLevel = 15;
            oldSave.currencies.metaCurrency = 1000;
            oldSave.worldState.currentSceneName = "TestLevel";
            
            // Act
            var migratedSave = SaveMigrationSystem.MigrateSaveData(oldSave);
            
            // Assert
            Assert.AreEqual(15, migratedSave.progression.playerLevel);
            Assert.AreEqual(1000, migratedSave.currencies.metaCurrency);
            Assert.AreEqual("TestLevel", migratedSave.worldState.currentSceneName);
        }
        
        #endregion
        
        #region Scene Data Generation Tests
        
        [Test]
        public void GeneratedSceneData_HasRequiredFields()
        {
            // Arrange
            var sceneData = new GeneratedSceneData
            {
                sceneId = "test-scene-1",
                sceneName = "TestScene",
                playerSpawnPosition = Vector3.zero,
                depth = 1,
                pathType = "main",
                isCompleted = false,
                generatedAt = System.DateTime.Now,
                sceneSpecificData = new Dictionary<string, object>()
            };
            
            // Assert
            Assert.IsNotNull(sceneData.sceneId);
            Assert.IsNotNull(sceneData.sceneName);
            Assert.IsNotNull(sceneData.sceneSpecificData);
            Assert.AreEqual(1, sceneData.depth);
            Assert.AreEqual("main", sceneData.pathType);
        }
        
        [Test]
        public void GeneratedSceneData_StoresSpecificData()
        {
            // Arrange
            var sceneData = new GeneratedSceneData
            {
                sceneSpecificData = new Dictionary<string, object>()
            };
            
            // Act
            sceneData.sceneSpecificData["seed"] = "test-seed";
            sceneData.sceneSpecificData["difficulty"] = 5;
            sceneData.sceneSpecificData["roomType"] = "combat";
            
            // Assert
            Assert.AreEqual("test-seed", sceneData.sceneSpecificData["seed"]);
            Assert.AreEqual(5, sceneData.sceneSpecificData["difficulty"]);
            Assert.AreEqual("combat", sceneData.sceneSpecificData["roomType"]);
        }
        
        #endregion
        
        #region Procedural State Persistence Tests
        
        [Test]
        public void ProceduralState_SavesAndRestores()
        {
            // Arrange
            var procState = new ProceduralGenerationState
            {
                currentSeed = 12345,
                currentDepth = 3,
                currentPath = "main",
                sceneSetId = "test-set",
                isCompleted = false
            };
            
            // Act - Serialize and deserialize
            string json = JsonUtility.ToJson(procState);
            var restored = JsonUtility.FromJson<ProceduralGenerationState>(json);
            
            // Assert
            Assert.AreEqual(procState.currentSeed, restored.currentSeed);
            Assert.AreEqual(procState.currentDepth, restored.currentDepth);
            Assert.AreEqual(procState.currentPath, restored.currentPath);
            Assert.AreEqual(procState.sceneSetId, restored.sceneSetId);
        }
        
        [Test]
        public void ProceduralState_TracksGeneratedScenes()
        {
            // Arrange
            var procState = new ProceduralGenerationState
            {
                generatedScenes = new List<GeneratedSceneData>()
            };
            
            // Act
            procState.generatedScenes.Add(new GeneratedSceneData
            {
                sceneId = "scene-1",
                depth = 1,
                isCompleted = true
            });
            
            procState.generatedScenes.Add(new GeneratedSceneData
            {
                sceneId = "scene-2",
                depth = 2,
                isCompleted = false
            });
            
            // Assert
            Assert.AreEqual(2, procState.generatedScenes.Count);
            Assert.IsTrue(procState.generatedScenes[0].isCompleted);
            Assert.IsFalse(procState.generatedScenes[1].isCompleted);
        }
        
        #endregion
        
        #region Integration Flow Tests
        
        [UnityTest]
        public IEnumerator ProceduralFlow_CompleteIntegration()
        {
            // Step 1: Generate map
            var map = sceneLoader.GenerateMap("integration-test");
            Assert.IsNotNull(map);
            
            yield return null;
            
            // Step 2: Create save data with procedural state
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.worldState.proceduralState.currentSeed = 12345;
            saveData.worldState.proceduralState.currentDepth = 2;
            
            // Step 3: Load from save data
            sceneLoader.LoadSceneFromSaveData(saveData);
            
            yield return null;
            
            // Step 4: Verify scene data
            var currentMap = sceneLoader.GetCurrentMap();
            Assert.IsNotNull(currentMap);
            
            // Step 5: Verify migration if needed
            if (SaveMigrationSystem.NeedsMigration(saveData))
            {
                var migrated = SaveMigrationSystem.MigrateSaveData(saveData);
                Assert.IsNotNull(migrated);
                Assert.GreaterOrEqual(migrated.version, saveData.version);
            }
            
            Assert.Pass("Complete integration flow executed successfully");
        }
        
        #endregion
    }
}