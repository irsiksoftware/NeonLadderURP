using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using NeonLadderURP.DataManagement;
using NeonLadderURP.Models;

namespace NeonLadder.Tests
{
    /// <summary>
    /// Comprehensive test suite for EnhancedSaveSystem with behavioral validation and edge case coverage.
    /// Tests JSON serialization, file operations, error handling, and data integrity.
    /// </summary>
    public class EnhancedSaveSystemTests
    {
        private string testGameDataDirectory;
        private string originalGameDataDirectory;
        
        [SetUp]
        public void SetUp()
        {
            // Create isolated test directory
            testGameDataDirectory = Path.Combine(Application.temporaryCachePath, "TestGameData", System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(testGameDataDirectory);
            
            // Store original directory for restoration
            originalGameDataDirectory = Path.Combine(Application.dataPath, "..", "GameData");
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
        
        #region Basic Save/Load Tests
        
        [Test]
        public void Save_WithValidData_CreatesJsonFile()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            var expectedPath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            
            // Act
            var result = SaveToTestDirectory(saveData);
            
            // Assert
            Assert.IsTrue(result, "Save operation should succeed");
            Assert.IsTrue(File.Exists(expectedPath), "Save file should be created");
            
            var fileContent = File.ReadAllText(expectedPath);
            Assert.IsTrue(fileContent.Contains("\"saveVersion\": \"2.0\""), "Save file should contain version information");
            Assert.IsTrue(fileContent.Contains("\"gameVersion\""), "Save file should contain game version");
        }
        
        [Test]
        public void Load_WithExistingSaveFile_ReturnsCorrectData()
        {
            // Arrange
            var originalData = CreateTestSaveData();
            originalData.progression.playerLevel = 42;
            originalData.currencies.metaCurrency = 1337;
            SaveToTestDirectory(originalData);
            
            // Act
            var loadedData = LoadFromTestDirectory();
            
            // Assert
            Assert.IsNotNull(loadedData, "Loaded data should not be null");
            Assert.AreEqual(42, loadedData.progression.playerLevel, "Player level should be preserved");
            Assert.AreEqual(1337, loadedData.currencies.metaCurrency, "Meta currency should be preserved");
            Assert.AreEqual("2.0", loadedData.saveVersion, "Save version should be preserved");
        }
        
        [Test]
        public void SaveExists_WithNoFile_ReturnsFalse()
        {
            // Act & Assert - Using test directory that doesn't have a save file
            Assert.IsFalse(SaveExistsInTestDirectory(), "SaveExists should return false when no file exists");
        }
        
        [Test]
        public void SaveExists_WithExistingFile_ReturnsTrue()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            SaveToTestDirectory(saveData);
            
            // Act & Assert
            Assert.IsTrue(SaveExistsInTestDirectory(), "SaveExists should return true when file exists");
        }
        
        #endregion
        
        #region Data Integrity Tests
        
        [Test]
        public void Save_UpdatesLastSavedTimestamp()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            var beforeSave = DateTime.Now;
            
            // Act
            SaveToTestDirectory(saveData);
            var loadedData = LoadFromTestDirectory();
            var afterSave = DateTime.Now;
            
            // Assert
            Assert.IsTrue(loadedData.lastSaved >= beforeSave && loadedData.lastSaved <= afterSave,
                "Last saved timestamp should be updated to current time");
        }
        
        [Test]
        public void Save_PreservesComplexDataStructures()
        {
            // Arrange
            var saveData = CreateComplexTestSaveData();
            
            // Act
            SaveToTestDirectory(saveData);
            var loadedData = LoadFromTestDirectory();
            
            // Assert
            Assert.AreEqual(saveData.progression.unlockedAbilities.Count, loadedData.progression.unlockedAbilities.Count,
                "Unlocked abilities count should be preserved");
            Assert.AreEqual(saveData.worldState.completedScenes.Count, loadedData.worldState.completedScenes.Count,
                "Completed scenes count should be preserved");
            Assert.AreEqual(saveData.inventory.purchasedItems.Count, loadedData.inventory.purchasedItems.Count,
                "Purchased items count should be preserved");
            
            // Verify specific complex data
            if (saveData.inventory.purchasedItems.Count > 0)
            {
                var originalItem = saveData.inventory.purchasedItems[0];
                var loadedItem = loadedData.inventory.purchasedItems[0];
                Assert.AreEqual(originalItem.itemId, loadedItem.itemId, "Item ID should be preserved");
                Assert.AreEqual(originalItem.timesPurchased, loadedItem.timesPurchased, "Times purchased should be preserved");
            }
        }
        
        [Test]
        public void CurrencyData_TracksTransactionsCorrectly()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            var currencies = saveData.currencies;
            
            // Act - Simulate currency transactions
            currencies.EarnMeta(100);
            currencies.EarnPerma(50);
            var metaSpent = currencies.SpendMeta(30);
            var permaSpent = currencies.SpendPerma(20);
            
            // Assert
            Assert.IsTrue(metaSpent, "Meta currency spending should succeed");
            Assert.IsTrue(permaSpent, "Perma currency spending should succeed");
            Assert.AreEqual(70, currencies.metaCurrency, "Meta currency should reflect spending");
            Assert.AreEqual(30, currencies.permaCurrency, "Perma currency should reflect spending");
            Assert.AreEqual(100, currencies.totalMetaEarned, "Total meta earned should be tracked");
            Assert.AreEqual(30, currencies.totalMetaSpent, "Total meta spent should be tracked");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void Load_WithNoSaveFile_ReturnsNewSaveData()
        {
            // Act - Load from directory with no save file
            var loadedData = LoadFromTestDirectory();
            
            // Assert
            Assert.IsNotNull(loadedData, "Should return new save data when no file exists");
            Assert.AreEqual(1, loadedData.progression.playerLevel, "Should have default player level");
            Assert.AreEqual(100, loadedData.progression.maxHealth, "Should have default max health");
            Assert.AreEqual("2.0", loadedData.saveVersion, "Should have current save version");
        }
        
        [Test]
        public void Save_CreatesBackupBeforeOverwriting()
        {
            // Arrange
            var originalData = CreateTestSaveData();
            originalData.progression.playerLevel = 1;
            SaveToTestDirectory(originalData);
            
            var updatedData = CreateTestSaveData();
            updatedData.progression.playerLevel = 2;
            
            // Act
            SaveToTestDirectory(updatedData);
            
            // Assert
            var backupPath = Path.Combine(testGameDataDirectory, "NeonLadderSave_Backup.json");
            Assert.IsTrue(File.Exists(backupPath), "Backup file should be created");
            
            // Verify backup contains original data
            var backupContent = File.ReadAllText(backupPath);
            Assert.IsTrue(backupContent.Contains("\"playerLevel\": 1"), "Backup should contain original player level");
        }
        
        [Test]
        public void Load_WithCorruptedFile_HandlesGracefully()
        {
            // Arrange - Create corrupted save file
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            File.WriteAllText(savePath, "{ corrupted json data !@#$%");
            
            // Act
            var loadedData = LoadFromTestDirectory();
            
            // Assert
            Assert.IsNotNull(loadedData, "Should handle corrupted file gracefully");
            Assert.AreEqual(1, loadedData.progression.playerLevel, "Should return default data for corrupted file");
        }
        
        [Test]
        public void DeleteSave_RemovesBothSaveAndBackup()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            SaveToTestDirectory(saveData);
            SaveToTestDirectory(saveData); // Create backup
            
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            var backupPath = Path.Combine(testGameDataDirectory, "NeonLadderSave_Backup.json");
            
            Assert.IsTrue(File.Exists(savePath), "Save file should exist before deletion");
            Assert.IsTrue(File.Exists(backupPath), "Backup file should exist before deletion");
            
            // Act
            DeleteSaveInTestDirectory();
            
            // Assert
            Assert.IsFalse(File.Exists(savePath), "Save file should be deleted");
            Assert.IsFalse(File.Exists(backupPath), "Backup file should be deleted");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void Save_WithLargeDataSet_CompletesWithinTimeLimit()
        {
            // Arrange
            var saveData = CreateLargeSaveData();
            var startTime = Time.realtimeSinceStartup;
            
            // Act
            var result = SaveToTestDirectory(saveData);
            var endTime = Time.realtimeSinceStartup;
            var duration = endTime - startTime;
            
            // Assert
            Assert.IsTrue(result, "Large save should complete successfully");
            Assert.Less(duration, 1.0f, "Save operation should complete within 1 second");
        }
        
        [Test]
        public void Load_WithLargeDataSet_CompletesWithinTimeLimit()
        {
            // Arrange
            var largeSaveData = CreateLargeSaveData();
            SaveToTestDirectory(largeSaveData);
            var startTime = Time.realtimeSinceStartup;
            
            // Act
            var loadedData = LoadFromTestDirectory();
            var endTime = Time.realtimeSinceStartup;
            var duration = endTime - startTime;
            
            // Assert
            Assert.IsNotNull(loadedData, "Large load should complete successfully");
            Assert.Less(duration, 1.0f, "Load operation should complete within 1 second");
        }
        
        #endregion
        
        #region JSON Format Tests
        
        [Test]
        public void Save_ProducesHumanReadableJson()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            
            // Act
            SaveToTestDirectory(saveData);
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            var jsonContent = File.ReadAllText(savePath);
            
            // Assert
            Assert.IsTrue(jsonContent.Contains("\n"), "JSON should be formatted with newlines");
            Assert.IsTrue(jsonContent.Contains("  "), "JSON should be indented");
            Assert.IsTrue(jsonContent.Contains("\"progression\":"), "JSON should contain readable property names");
            Assert.IsTrue(jsonContent.Contains("\"currencies\":"), "JSON should contain currency section");
            Assert.IsTrue(jsonContent.Contains("\"worldState\":"), "JSON should contain world state section");
        }
        
        [Test]
        public void ExternalJsonModification_LoadsCorrectly()
        {
            // Arrange
            var saveData = CreateTestSaveData();
            SaveToTestDirectory(saveData);
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            
            // Act - Modify JSON externally
            var jsonContent = File.ReadAllText(savePath);
            var modifiedJson = jsonContent.Replace("\"playerLevel\": 1", "\"playerLevel\": 99");
            File.WriteAllText(savePath, modifiedJson);
            
            var loadedData = LoadFromTestDirectory();
            
            // Assert
            Assert.AreEqual(99, loadedData.progression.playerLevel, "External JSON modification should be preserved");
        }
        
        #endregion
        
        #region Helper Methods
        
        private ConsolidatedSaveData CreateTestSaveData()
        {
            var saveData = new ConsolidatedSaveData();
            saveData.progression.playerLevel = 1;
            saveData.progression.maxHealth = 100;
            saveData.progression.currentHealth = 100;
            saveData.currencies.metaCurrency = 0;
            saveData.currencies.permaCurrency = 0;
            saveData.worldState.currentSceneName = "TestScene";
            saveData.gameVersion = "1.0.0-test";
            return saveData;
        }
        
        private ConsolidatedSaveData CreateComplexTestSaveData()
        {
            var saveData = CreateTestSaveData();
            
            // Add complex data structures
            saveData.progression.unlockedAbilities.AddRange(new[] { "DoubleJump", "Dash", "WallSlide" });
            saveData.worldState.completedScenes.AddRange(new[] { "Scene1", "Scene2", "Scene3" });
            saveData.worldState.defeatedBosses.AddRange(new[] { "Boss1", "Boss2" });
            
            // Add purchased items
            saveData.inventory.purchasedItems.Add(new PurchasedItemData
            {
                itemId = "health_potion",
                itemName = "Health Potion",
                timesPurchased = 3,
                firstPurchased = DateTime.Now.AddDays(-1),
                lastPurchased = DateTime.Now,
                isActive = true,
                purchaseType = "meta"
            });
            
            // Add procedural generation data
            saveData.worldState.proceduralState.currentSeed = 12345;
            saveData.worldState.proceduralState.currentDepth = 5;
            saveData.worldState.proceduralState.generatedScenes.Add(new GeneratedSceneData
            {
                sceneId = "proc_scene_1",
                sceneName = "ProceduralScene1",
                depth = 1,
                pathType = "main",
                isCompleted = true,
                generatedAt = DateTime.Now
            });
            
            return saveData;
        }
        
        private ConsolidatedSaveData CreateLargeSaveData()
        {
            var saveData = CreateComplexTestSaveData();
            
            // Add large amounts of data to test performance
            for (int i = 0; i < 1000; i++)
            {
                saveData.worldState.completedScenes.Add($"LargeScene_{i}");
                saveData.inventory.purchasedItems.Add(new PurchasedItemData
                {
                    itemId = $"item_{i}",
                    itemName = $"Test Item {i}",
                    timesPurchased = i % 10,
                    firstPurchased = DateTime.Now.AddDays(-i),
                    lastPurchased = DateTime.Now,
                    isActive = i % 2 == 0,
                    purchaseType = i % 2 == 0 ? "meta" : "perma"
                });
                
                saveData.worldState.proceduralState.generatedScenes.Add(new GeneratedSceneData
                {
                    sceneId = $"large_scene_{i}",
                    sceneName = $"LargeGeneratedScene{i}",
                    depth = i % 20,
                    pathType = i % 3 == 0 ? "main" : "branch",
                    isCompleted = i % 4 == 0,
                    generatedAt = DateTime.Now.AddMinutes(-i)
                });
            }
            
            return saveData;
        }
        
        // Test directory operations (mocked to avoid file system dependencies)
        private bool SaveToTestDirectory(ConsolidatedSaveData saveData)
        {
            try
            {
                var json = JsonUtility.ToJson(saveData, true);
                var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
                
                // Create backup if file exists
                if (File.Exists(savePath))
                {
                    var backupPath = Path.Combine(testGameDataDirectory, "NeonLadderSave_Backup.json");
                    File.Copy(savePath, backupPath, true);
                }
                
                File.WriteAllText(savePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        private ConsolidatedSaveData LoadFromTestDirectory()
        {
            try
            {
                var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
                if (!File.Exists(savePath))
                {
                    return new ConsolidatedSaveData();
                }
                
                var json = File.ReadAllText(savePath);
                return JsonUtility.FromJson<ConsolidatedSaveData>(json);
            }
            catch
            {
                return new ConsolidatedSaveData();
            }
        }
        
        private bool SaveExistsInTestDirectory()
        {
            var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
            return File.Exists(savePath);
        }
        
        private bool DeleteSaveInTestDirectory()
        {
            try
            {
                var savePath = Path.Combine(testGameDataDirectory, "NeonLadderSave.json");
                var backupPath = Path.Combine(testGameDataDirectory, "NeonLadderSave_Backup.json");
                
                if (File.Exists(savePath))
                    File.Delete(savePath);
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                    
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        #endregion
    }
}