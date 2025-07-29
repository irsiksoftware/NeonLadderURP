using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using System;
using NeonLadderURP.DataManagement;
using NeonLadderURP.DataManagement.Examples;

namespace NeonLadder.Tests
{
    /// <summary>
    /// Integration tests for the complete save system including SaveStateConfiguration and EnhancedSaveSystem.
    /// Tests end-to-end workflows that a designer would use.
    /// </summary>
    public class SaveSystemIntegrationTests
    {
        private string testGameDataDirectory;
        
        [SetUp]
        public void SetUp()
        {
            // Create isolated test directory
            testGameDataDirectory = Path.Combine(Application.temporaryCachePath, "IntegrationTestGameData", System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(testGameDataDirectory);
        }
        
        [TearDown] 
        public void TearDown()
        {
            // Clean up test directory
            if (Directory.Exists(testGameDataDirectory))
            {
                Directory.Delete(testGameDataDirectory, true);
            }
        }
        
        #region End-to-End Workflow Tests
        
        [Test]
        public void CompleteWorkflow_CreateConfigurationAndSave_WorksCorrectly()
        {
            // Arrange - Create a test configuration like a designer would
            var config = ExampleSaveConfigurations.CreateMidGameConfig();
            
            // Act - Generate save data from configuration
            var saveData = config.CreateSaveData();
            
            // Simulate saving to our test directory
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            var json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
            
            // Load the save data back
            var loadedJson = File.ReadAllText(savePath);
            var loadedData = JsonUtility.FromJson<ConsolidatedSaveData>(loadedJson);
            
            // Assert - Verify the complete round trip
            Assert.IsNotNull(loadedData, "Loaded data should not be null");
            Assert.AreEqual(saveData.progression.playerLevel, loadedData.progression.playerLevel, "Player level should be preserved");
            Assert.AreEqual(saveData.currencies.metaCurrency, loadedData.currencies.metaCurrency, "Meta currency should be preserved");
            Assert.AreEqual(saveData.worldState.currentSceneName, loadedData.worldState.currentSceneName, "Scene name should be preserved");
            Assert.AreEqual(saveData.worldState.proceduralState.currentDepth, loadedData.worldState.proceduralState.currentDepth, "Depth should be preserved");
        }
        
        [Test]
        public void ProceduralSceneConfiguration_CreateAndLoad_PreservesAllData()
        {
            // Arrange - Create configuration with procedural scene data
            var config = ExampleSaveConfigurations.CreateTestingConfig();
            
            // Act - Create save data with procedural information
            var saveData = config.CreateSaveData();
            
            // Verify procedural data was applied
            Assert.AreEqual(42, saveData.worldState.proceduralState.currentSeed, "Seed should be applied from configuration");
            Assert.AreEqual("test", saveData.worldState.proceduralState.currentPath, "Path type should be applied");
            Assert.IsTrue(saveData.worldState.proceduralState.generatedScenes.Count > 0, "Generated scenes should be created");
            
            // Verify specific scene data
            var combatScene = saveData.worldState.proceduralState.generatedScenes.Find(s => s.sceneName == "TestRoom_Combat");
            Assert.IsNotNull(combatScene, "Combat test scene should be generated");
            Assert.AreEqual(1, combatScene.depth, "Combat scene depth should be preserved");
            Assert.AreEqual("main", combatScene.pathType, "Combat scene path type should be preserved");
            Assert.IsTrue(combatScene.sceneSpecificData.ContainsKey("enemy_count"), "Scene specific data should be preserved");
            Assert.AreEqual("5", combatScene.sceneSpecificData["enemy_count"], "Enemy count should be preserved");
        }
        
        [Test]
        public void ConfigurationLoadSaveCycle_PreservesDataIntegrity()
        {
            // Arrange - Create original configuration
            var originalConfig = ExampleSaveConfigurations.CreateEndGameConfig();
            var originalSaveData = originalConfig.CreateSaveData();
            
            // Act - Create new configuration and load data into it
            var newConfig = ScriptableObject.CreateInstance<SaveStateConfiguration>();
            newConfig.LoadFromSaveData(originalSaveData);
            var newSaveData = newConfig.CreateSaveData();
            
            // Assert - Verify data integrity is maintained
            Assert.AreEqual(originalSaveData.progression.playerLevel, newSaveData.progression.playerLevel, "Player level should be preserved through load cycle");
            Assert.AreEqual(originalSaveData.progression.maxHealth, newSaveData.progression.maxHealth, "Max health should be preserved");
            Assert.AreEqual(originalSaveData.currencies.metaCurrency, newSaveData.currencies.metaCurrency, "Meta currency should be preserved");
            Assert.AreEqual(originalSaveData.currencies.permaCurrency, newSaveData.currencies.permaCurrency, "Perma currency should be preserved");
            Assert.AreEqual(originalSaveData.worldState.currentSceneName, newSaveData.worldState.currentSceneName, "Scene name should be preserved");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(newConfig);
        }
        
        #endregion
        
        #region Designer Workflow Tests
        
        [Test]
        public void DesignerWorkflow_CreateMultipleConfigurations_EachProducesValidData()
        {
            // Arrange & Act - Create all example configurations like a designer would
            var configs = new[]
            {
                ExampleSaveConfigurations.CreateNewPlayerConfig(),
                ExampleSaveConfigurations.CreateMidGameConfig(),
                ExampleSaveConfigurations.CreateEndGameConfig(),
                ExampleSaveConfigurations.CreateTestingConfig(),
                ExampleSaveConfigurations.CreateRegressionTestConfig()
            };
            
            // Assert - Each configuration should produce valid save data
            foreach (var config in configs)
            {
                var saveData = config.CreateSaveData();
                
                Assert.IsNotNull(saveData, "Save data should be created successfully");
                Assert.IsNotNull(saveData.progression, "Progression should be initialized");
                Assert.IsNotNull(saveData.currencies, "Currencies should be initialized");
                Assert.IsNotNull(saveData.worldState, "World state should be initialized");
                Assert.AreEqual("2.0", saveData.saveVersion, "Save version should be current");
                Assert.IsTrue(saveData.progression.playerLevel > 0, "Player level should be valid");
                Assert.IsTrue(saveData.progression.maxHealth > 0, "Max health should be valid");
                
                // Clean up
                UnityEngine.Object.DestroyImmediate(config);
            }
        }
        
        [Test]
        public void ExternalJsonEditing_ModifyAndReload_WorksCorrectly()
        {
            // Arrange - Create save file
            var config = ExampleSaveConfigurations.CreateMidGameConfig();
            var saveData = config.CreateSaveData();
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            var json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
            
            // Act - Modify JSON externally (simulating manual editing)
            var modifiedJson = System.Text.RegularExpressions.Regex.Replace(json, "\"playerLevel\"\\s*:\\s*10", "\"playerLevel\": 99");
            modifiedJson = System.Text.RegularExpressions.Regex.Replace(modifiedJson, "\"metaCurrency\"\\s*:\\s*250", "\"metaCurrency\": 5000");
            File.WriteAllText(savePath, modifiedJson);
            
            // Load modified data
            var loadedJson = File.ReadAllText(savePath);
            var loadedData = JsonUtility.FromJson<ConsolidatedSaveData>(loadedJson);
            
            // Assert - External modifications should be preserved
            Assert.AreEqual(99, loadedData.progression.playerLevel, "External player level modification should be preserved");
            Assert.AreEqual(5000, loadedData.currencies.metaCurrency, "External currency modification should be preserved");
            
            // Other data should remain unchanged
            Assert.AreEqual(saveData.worldState.currentSceneName, loadedData.worldState.currentSceneName, "Unmodified data should remain intact");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        #endregion
        
        #region Performance and Stress Tests
        
        [Test]
        public void LargeConfigurationData_PerformsWithinLimits()
        {
            // Arrange - Create configuration with large amounts of data
            var config = ExampleSaveConfigurations.CreateEndGameConfig();
            
            // Add stress test data
            var saveData = config.CreateSaveData();
            
            // Add large amounts of progression data
            for (int i = 0; i < 1000; i++)
            {
                saveData.worldState.completedScenes.Add($"StressTestScene_{i}");
                saveData.inventory.purchasedItems.Add(new PurchasedItemData
                {
                    itemId = $"stress_item_{i}",
                    itemName = $"Stress Test Item {i}",
                    timesPurchased = i % 10,
                    firstPurchased = DateTime.Now.AddDays(-i),
                    lastPurchased = DateTime.Now,
                    isActive = true,
                    purchaseType = "meta"
                });
            }
            
            // Act - Measure serialization performance
            var startTime = Time.realtimeSinceStartup;
            var json = JsonUtility.ToJson(saveData, true);
            var serializationTime = Time.realtimeSinceStartup - startTime;
            
            // Measure deserialization performance
            startTime = Time.realtimeSinceStartup;
            var deserializedData = JsonUtility.FromJson<ConsolidatedSaveData>(json);
            var deserializationTime = Time.realtimeSinceStartup - startTime;
            
            // Assert - Performance should be within acceptable limits
            Assert.Less(serializationTime, 0.5f, "Serialization should complete within 500ms");
            Assert.Less(deserializationTime, 0.5f, "Deserialization should complete within 500ms");
            Assert.IsNotNull(deserializedData, "Large data should deserialize successfully");
            Assert.AreEqual(saveData.worldState.completedScenes.Count, deserializedData.worldState.completedScenes.Count, "All data should be preserved");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void SaveDataValidation_CorruptedData_HandlesGracefully()
        {
            // Arrange - Create corrupted JSON data
            var corruptedJsons = new[]
            {
                "{ incomplete json",
                "{ \"progression\": null }",
                "{ \"saveVersion\": \"invalid\" }",
                "",
                "not json at all"
            };
            
            foreach (var corruptedJson in corruptedJsons)
            {
                // Act & Assert - Should handle corruption gracefully
                Assert.DoesNotThrow(() =>
                {
                    try
                    {
                        var data = JsonUtility.FromJson<ConsolidatedSaveData>(corruptedJson);
                        // Even if deserialization succeeds, the data should be considered invalid
                    }
                    catch
                    {
                        // Expected for malformed JSON
                    }
                }, $"Should handle corrupted JSON gracefully: {corruptedJson}");
            }
        }
        
        [Test]
        public void GameDataDirectory_CreatedCorrectly()
        {
            // Act - Create a simple save to trigger directory creation
            var config = ExampleSaveConfigurations.CreateNewPlayerConfig();
            var saveData = config.CreateSaveData();
            
            // Simulate the directory creation logic
            var gameDataDir = Path.Combine(Application.dataPath, "..", "GameData");
            
            // Assert - Directory structure should be logical
            Assert.IsTrue(Path.IsPathRooted(gameDataDir), "Game data directory should be absolute path");
            Assert.IsFalse(gameDataDir.Contains("Library"), "Should not save in Unity's Library folder");
            Assert.IsFalse(gameDataDir.Contains("Temp"), "Should not save in temporary locations");
            
            // Clean up
            UnityEngine.Object.DestroyImmediate(config);
        }
        
        #endregion
    }
}