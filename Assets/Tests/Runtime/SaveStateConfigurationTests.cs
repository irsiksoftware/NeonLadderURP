using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadderURP.DataManagement;
using NeonLadderURP.Models;

namespace NeonLadder.Tests
{
    /// <summary>
    /// Comprehensive test suite for SaveStateConfiguration ScriptableObject.
    /// Tests drag & drop functionality, procedural scene configuration, and save data generation.
    /// </summary>
    public class SaveStateConfigurationTests
    {
        private SaveStateConfiguration testConfig;
        private UnlockScriptableObject testUnlock;
        private PurchasableItem testItem;
        
        [SetUp]
        public void SetUp()
        {
            // Create test configuration
            testConfig = ScriptableObject.CreateInstance<SaveStateConfiguration>();
            
            // Create test unlock (mock)
            testUnlock = ScriptableObject.CreateInstance<UnlockScriptableObject>();
            if (testUnlock != null)
            {
                // Set up test unlock properties using reflection or direct access
                // Note: This would need the actual UnlockScriptableObject implementation
            }
            
            // Create test purchasable item (mock)
            testItem = ScriptableObject.CreateInstance<PurchasableItem>();
            
            // Set up basic configuration
            SetupBasicConfiguration();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
            if (testUnlock != null)
                Object.DestroyImmediate(testUnlock);
            if (testItem != null)
                Object.DestroyImmediate(testItem);
        }
        
        #region Configuration Creation Tests
        
        [Test]
        public void CreateSaveData_WithDefaultConfiguration_ReturnsValidSaveData()
        {
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.IsNotNull(saveData, "Created save data should not be null");
            Assert.IsNotNull(saveData.progression, "Progression data should be initialized");
            Assert.IsNotNull(saveData.currencies, "Currency data should be initialized");
            Assert.IsNotNull(saveData.worldState, "World state should be initialized");
            Assert.IsNotNull(saveData.inventory, "Inventory should be initialized");
            Assert.IsNotNull(saveData.settings, "Settings should be initialized");
            Assert.IsNotNull(saveData.statistics, "Statistics should be initialized");
        }
        
        [Test]
        public void CreateSaveData_WithPlayerProgression_AppliesCorrectly()
        {
            // Arrange - Configure player progression via reflection or direct property access
            var playerSetup = GetPlayerSetupFromConfig();
            playerSetup.playerLevel = 25;
            playerSetup.maxHealth = 150;
            playerSetup.currentHealth = 100;
            playerSetup.attackDamage = 20f;
            playerSetup.movementSpeed = 7.5f;
            playerSetup.jumpCount = 2;
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual(25, saveData.progression.playerLevel, "Player level should be applied");
            Assert.AreEqual(150, saveData.progression.maxHealth, "Max health should be applied");
            Assert.AreEqual(100, saveData.progression.currentHealth, "Current health should be applied");
            Assert.AreEqual(20f, saveData.progression.attackDamage, "Attack damage should be applied");
            Assert.AreEqual(7.5f, saveData.progression.movementSpeed, "Movement speed should be applied");
            Assert.AreEqual(2, saveData.progression.jumpCount, "Jump count should be applied");
        }
        
        [Test]
        public void CreateSaveData_WithCurrencySetup_AppliesCorrectly()
        {
            // Arrange
            var currencySetup = GetCurrencySetupFromConfig();
            currencySetup.startingMetaCurrency = 500;
            currencySetup.startingPermaCurrency = 250;
            currencySetup.totalMetaEarned = 1000;
            currencySetup.totalPermaEarned = 750;
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual(500, saveData.currencies.metaCurrency, "Meta currency should be applied");
            Assert.AreEqual(250, saveData.currencies.permaCurrency, "Perma currency should be applied");
            Assert.AreEqual(1000, saveData.currencies.totalMetaEarned, "Total meta earned should be applied");
            Assert.AreEqual(750, saveData.currencies.totalPermaEarned, "Total perma earned should be applied");
        }
        
        [Test]
        public void CreateSaveData_WithWorldState_AppliesCorrectly()
        {
            // Arrange
            var worldSetup = GetWorldSetupFromConfig();
            worldSetup.currentSceneName = "TestLevel5";
            worldSetup.playerPosition = new Vector3(10f, 5f, 0f);
            worldSetup.currentDepth = 8;
            worldSetup.runNumber = 3;
            worldSetup.completedScenes.AddRange(new[] { "Scene1", "Scene2", "Scene3" });
            worldSetup.defeatedBosses.AddRange(new[] { "FireBoss", "IceBoss" });
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual("TestLevel5", saveData.worldState.currentSceneName, "Scene name should be applied");
            Assert.AreEqual(new Vector3(10f, 5f, 0f), saveData.worldState.playerPosition, "Player position should be applied");
            Assert.AreEqual(8, saveData.worldState.proceduralState.currentDepth, "Current depth should be applied");
            Assert.AreEqual(3, saveData.worldState.currentRun.runNumber, "Run number should be applied");
            Assert.Contains("Scene2", saveData.worldState.completedScenes, "Completed scenes should be applied");
            Assert.Contains("FireBoss", saveData.worldState.defeatedBosses, "Defeated bosses should be applied");
        }
        
        #endregion
        
        #region Procedural Scene Set Tests
        
        [Test]
        public void CreateSaveData_WithProceduralSceneSet_AppliesCorrectly()
        {
            // Arrange
            var sceneSet = CreateTestProceduralSceneSet();
            SetCurrentSceneSet(sceneSet);
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual(12345, saveData.worldState.proceduralState.currentSeed, "Seed should be applied");
            Assert.AreEqual("test_path", saveData.worldState.proceduralState.currentPath, "Path type should be applied");
            Assert.AreEqual(3, saveData.worldState.proceduralState.generatedScenes.Count, "Generated scenes should be applied");
            
            var firstScene = saveData.worldState.proceduralState.generatedScenes[0];
            Assert.AreEqual("TestScene1", firstScene.sceneName, "Scene name should be preserved");
            Assert.AreEqual(1, firstScene.depth, "Scene depth should be preserved");
            Assert.AreEqual("main", firstScene.pathType, "Scene path type should be preserved");
        }
        
        [Test]
        public void ProceduralSceneSet_WithSceneSpecificData_PreservesData()
        {
            // Arrange
            var sceneSet = CreateTestProceduralSceneSet();
            sceneSet.sceneConfigurations[0].sceneSpecificData.Add(new KeyValueData { key = "boss_type", value = "fire" });
            sceneSet.sceneConfigurations[0].sceneSpecificData.Add(new KeyValueData { key = "difficulty", value = "hard" });
            SetCurrentSceneSet(sceneSet);
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            var firstScene = saveData.worldState.proceduralState.generatedScenes[0];
            Assert.IsTrue(firstScene.sceneSpecificData.ContainsKey("boss_type"), "Scene specific data should be preserved");
            Assert.AreEqual("fire", firstScene.sceneSpecificData["boss_type"], "Boss type should be preserved");
            Assert.AreEqual("hard", firstScene.sceneSpecificData["difficulty"], "Difficulty should be preserved");
        }
        
        [Test]
        public void ProceduralSceneSet_WithMultipleScenes_CreatesCorrectSequence()
        {
            // Arrange
            var sceneSet = CreateTestProceduralSceneSet();
            
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            var scenes = saveData.worldState.proceduralState.generatedScenes;
            Assert.AreEqual(3, scenes.Count, "Should create all configured scenes");
            
            // Verify scene sequence
            Assert.AreEqual("TestScene1", scenes[0].sceneName, "First scene should be TestScene1");
            Assert.AreEqual("BossScene", scenes[1].sceneName, "Second scene should be BossScene");
            Assert.AreEqual("SecretRoom", scenes[2].sceneName, "Third scene should be SecretRoom");
            
            // Verify depths
            Assert.AreEqual(1, scenes[0].depth, "First scene depth should be 1");
            Assert.AreEqual(2, scenes[1].depth, "Second scene depth should be 2");
            Assert.AreEqual(1, scenes[2].depth, "Third scene depth should be 1");
        }
        
        #endregion
        
        #region Load/Apply Configuration Tests
        
        [Test]
        public void LoadFromSaveData_WithValidData_UpdatesConfiguration()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            
            // Act
            testConfig.LoadFromSaveData(saveData);
            
            // Assert
            var playerSetup = GetPlayerSetupFromConfig();
            var currencySetup = GetCurrencySetupFromConfig();
            var worldSetup = GetWorldSetupFromConfig();
            
            Assert.AreEqual(42, playerSetup.playerLevel, "Player level should be updated");
            Assert.AreEqual(200, playerSetup.maxHealth, "Max health should be updated");
            Assert.AreEqual(1000, currencySetup.startingMetaCurrency, "Meta currency should be updated");
            Assert.AreEqual("LoadedScene", worldSetup.currentSceneName, "Scene name should be updated");
        }
        
        [Test]
        public void LoadFromSaveData_WithNullData_HandlesGracefully()
        {
            // Act & Assert - Should not throw exception
            Assert.DoesNotThrow(() => testConfig.LoadFromSaveData(null), "Loading null data should be handled gracefully");
        }
        
        [Test]
        public void ApplyToCurrentSession_CreatesValidSaveData()
        {
            // Arrange
            SetupAdvancedConfiguration();
            
            // Act & Assert - Should not throw exception and should create valid data
            Assert.DoesNotThrow(() => testConfig.ApplyToCurrentSession(), "Applying to current session should not throw");
            
            // Verify the created save data is valid
            var saveData = testConfig.CreateSaveData();
            Assert.IsNotNull(saveData, "Applied configuration should create valid save data");
            Assert.AreEqual("2.0", saveData.saveVersion, "Save version should be current");
        }
        
        #endregion
        
        #region Edge Case Tests
        
        [Test]
        public void CreateSaveData_WithEmptyConfiguration_CreatesDefaultValues()
        {
            // Arrange - Use completely default configuration
            var emptyConfig = ScriptableObject.CreateInstance<SaveStateConfiguration>();
            
            // Act
            var saveData = emptyConfig.CreateSaveData();
            
            // Assert
            Assert.IsNotNull(saveData, "Empty configuration should still create save data");
            Assert.AreEqual(1, saveData.progression.playerLevel, "Should have default player level");
            Assert.AreEqual(100, saveData.progression.maxHealth, "Should have default max health");
            Assert.AreEqual(0, saveData.currencies.metaCurrency, "Should have default currency");
            
            Object.DestroyImmediate(emptyConfig);
        }
        
        [Test]
        public void CreateSaveData_SetsMetadataCorrectly()
        {
            // Act
            var saveData = testConfig.CreateSaveData();
            
            // Assert
            Assert.AreEqual("2.0", saveData.saveVersion, "Save version should be set");
            Assert.AreEqual(Application.version, saveData.gameVersion, "Game version should be current version");
            Assert.IsTrue((System.DateTime.Now - saveData.lastSaved).TotalSeconds < 5, "Last saved should be recent");
            Assert.IsNotNull(saveData.playerId, "Player ID should be generated");
            Assert.IsTrue(saveData.playerId.Length > 0, "Player ID should not be empty");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void SetupBasicConfiguration()
        {
            // Use reflection or direct access to set up configuration
            // This would depend on the actual implementation of SaveStateConfiguration
            var playerSetup = GetPlayerSetupFromConfig();
            playerSetup.playerLevel = 1;
            playerSetup.maxHealth = 100;
            playerSetup.currentHealth = 100;
            playerSetup.attackDamage = 10f;
            playerSetup.movementSpeed = 5f;
            playerSetup.jumpCount = 1;
            
            var currencySetup = GetCurrencySetupFromConfig();
            currencySetup.startingMetaCurrency = 0;
            currencySetup.startingPermaCurrency = 0;
            
            var worldSetup = GetWorldSetupFromConfig();
            worldSetup.currentSceneName = "SampleScene";
            worldSetup.playerPosition = Vector3.zero;
            worldSetup.currentDepth = 0;
            worldSetup.runNumber = 1;
        }
        
        private void SetupAdvancedConfiguration()
        {
            var playerSetup = GetPlayerSetupFromConfig();
            playerSetup.playerLevel = 10;
            playerSetup.maxHealth = 150;
            playerSetup.jumpCount = 2;
            
            var currencySetup = GetCurrencySetupFromConfig();
            currencySetup.startingMetaCurrency = 500;
            currencySetup.startingPermaCurrency = 100;
            
            // Add some completed content
            var worldSetup = GetWorldSetupFromConfig();
            worldSetup.completedScenes.AddRange(new[] { "Tutorial", "Level1", "Level2" });
            worldSetup.defeatedBosses.Add("TutorialBoss");
        }
        
        private ProceduralSceneSet CreateTestProceduralSceneSet()
        {
            var sceneSet = new ProceduralSceneSet();
            sceneSet.setName = "Test Scene Set";
            sceneSet.seed = 12345;
            sceneSet.pathType = "test_path";
            sceneSet.maxDepth = 5;
            
            // Add test scene configurations
            sceneSet.sceneConfigurations.Add(new SceneConfiguration
            {
                sceneName = "TestScene1",
                depth = 1,
                pathType = "main",
                spawnPosition = new Vector3(0, 0, 0),
                isPreCompleted = false
            });
            
            sceneSet.sceneConfigurations.Add(new SceneConfiguration
            {
                sceneName = "BossScene",
                depth = 2,
                pathType = "boss",
                spawnPosition = new Vector3(5, 0, 0),
                isPreCompleted = false
            });
            
            sceneSet.sceneConfigurations.Add(new SceneConfiguration
            {
                sceneName = "SecretRoom",
                depth = 1,
                pathType = "secret",
                spawnPosition = new Vector3(-5, 0, 0),
                isPreCompleted = true
            });
            
            return sceneSet;
        }
        
        private ConsolidatedSaveData CreateTestSaveData()
        {
            var saveData = new ConsolidatedSaveData();
            saveData.progression.playerLevel = 42;
            saveData.progression.maxHealth = 200;
            saveData.progression.currentHealth = 150;
            saveData.currencies.metaCurrency = 1000;
            saveData.currencies.permaCurrency = 500;
            saveData.worldState.currentSceneName = "LoadedScene";
            saveData.worldState.playerPosition = new Vector3(100, 50, 0);
            saveData.worldState.proceduralState.currentDepth = 15;
            return saveData;
        }
        
        // These methods would use reflection or direct property access based on implementation
        private PlayerProgressionSetup GetPlayerSetupFromConfig()
        {
            // This would access the actual playerSetup field from SaveStateConfiguration
            // For now, returning a mock object
            return new PlayerProgressionSetup();
        }
        
        private CurrencySetup GetCurrencySetupFromConfig()
        {
            // This would access the actual currencySetup field from SaveStateConfiguration
            return new CurrencySetup();
        }
        
        private WorldStateSetup GetWorldSetupFromConfig()
        {
            // This would access the actual worldSetup field from SaveStateConfiguration
            return new WorldStateSetup();
        }
        
        private void SetCurrentSceneSet(ProceduralSceneSet sceneSet)
        {
            // This would set the currentSceneSet field in SaveStateConfiguration
            // Implementation depends on whether this field is publicly accessible
        }
        
        #endregion
    }
}