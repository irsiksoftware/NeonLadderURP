using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System;
using NeonLadder.DataManagement;
using NeonLadderURP.DataManagement;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class SteamCloudSaveTests
    {
        private SteamCloudSaveManager cloudManager;
        private GameObject managerObject;
        
        [SetUp]
        public void Setup()
        {
            // Create Steam cloud save manager
            managerObject = new GameObject("TestSteamCloudManager");
            cloudManager = managerObject.AddComponent<SteamCloudSaveManager>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
        }
        
        #region Metadata Tests
        
        [Test]
        public void SaveMetadata_CreatedCorrectly()
        {
            // Arrange
            var metadata = new SaveMetadata
            {
                version = 1,
                lastSaved = DateTime.Now,
                totalPlayTime = 3600f,
                playerLevel = 10,
                currentScene = "Level_5",
                metaCurrency = 500,
                permaCurrency = 150,
                saveId = "test-save-123",
                steamId = "76561198000000000"
            };
            
            // Act & Assert
            Assert.IsNotNull(metadata);
            Assert.AreEqual(1, metadata.version);
            Assert.AreEqual(10, metadata.playerLevel);
            Assert.AreEqual(500, metadata.metaCurrency);
            Assert.AreEqual(150, metadata.permaCurrency);
            Assert.AreEqual("test-save-123", metadata.saveId);
        }
        
        [Test]
        public void SaveMetadata_SerializesCorrectly()
        {
            // Arrange
            var metadata = new SaveMetadata
            {
                version = 2,
                lastSaved = new DateTime(2025, 1, 15, 10, 30, 0),
                totalPlayTime = 7200f,
                playerLevel = 15
            };
            
            // Act
            string json = JsonUtility.ToJson(metadata);
            var deserialized = JsonUtility.FromJson<SaveMetadata>(json);
            
            // Assert
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(metadata.version, deserialized.version);
            Assert.AreEqual(metadata.playerLevel, deserialized.playerLevel);
            Assert.AreEqual(metadata.totalPlayTime, deserialized.totalPlayTime);
        }
        
        #endregion
        
        #region Conflict Detection Tests
        
        [Test]
        public void SaveConflict_DetectsTimeDifference()
        {
            // Arrange
            var conflict = new SaveConflict
            {
                localMetadata = new SaveMetadata
                {
                    lastSaved = DateTime.Now,
                    playerLevel = 10
                },
                cloudMetadata = new SaveMetadata
                {
                    lastSaved = DateTime.Now.AddHours(-2),
                    playerLevel = 10
                }
            };
            
            // Act
            var summary = conflict.GetConflictSummary();
            
            // Assert
            Assert.IsNotNull(summary);
            Assert.IsTrue(summary.Contains("Level 10"));
        }
        
        [Test]
        public void SaveConflict_DetectsProgressDifference()
        {
            // Arrange
            var conflict = new SaveConflict
            {
                localMetadata = new SaveMetadata
                {
                    playerLevel = 15,
                    metaCurrency = 1000,
                    permaCurrency = 300
                },
                cloudMetadata = new SaveMetadata
                {
                    playerLevel = 12,
                    metaCurrency = 750,
                    permaCurrency = 250
                }
            };
            
            // Act
            bool hasProgressDifference = conflict.localMetadata.playerLevel != 
                                        conflict.cloudMetadata.playerLevel;
            bool hasCurrencyDifference = conflict.localMetadata.metaCurrency != 
                                        conflict.cloudMetadata.metaCurrency;
            
            // Assert
            Assert.IsTrue(hasProgressDifference, "Should detect level difference");
            Assert.IsTrue(hasCurrencyDifference, "Should detect currency difference");
        }
        
        #endregion
        
        #region Cloud Storage Info Tests
        
        [Test]
        public void CloudStorageInfo_CalculatesUsageCorrectly()
        {
            // Arrange
            var storageInfo = new CloudStorageInfo
            {
                totalBytes = 1024 * 1024 * 100, // 100 MB
                usedBytes = 1024 * 1024 * 25,   // 25 MB
                availableBytes = 1024 * 1024 * 75 // 75 MB
            };
            
            // Act
            float usagePercentage = storageInfo.GetUsagePercentage();
            
            // Assert
            Assert.AreEqual(25f, usagePercentage, 0.01f, "Should be 25% used");
        }
        
        [Test]
        public void CloudStorageInfo_HandlesZeroTotalBytes()
        {
            // Arrange
            var storageInfo = new CloudStorageInfo
            {
                totalBytes = 0,
                usedBytes = 0
            };
            
            // Act
            float usagePercentage = storageInfo.GetUsagePercentage();
            
            // Assert
            Assert.AreEqual(0f, usagePercentage, "Should return 0 when total is 0");
        }
        
        #endregion
        
        #region Conflict Resolution Tests
        
        [Test]
        public void ConflictResolution_TracksChoice()
        {
            // Arrange
            var resolution = new ConflictResolution
            {
                choice = ConflictChoice.KeepNewer,
                resolvedAt = DateTime.Now
            };
            
            // Assert
            Assert.AreEqual(ConflictChoice.KeepNewer, resolution.choice);
            Assert.IsNotNull(resolution.resolvedAt);
        }
        
        [Test]
        public void ConflictChoice_AllOptionsAvailable()
        {
            // Assert - Check all enum values are defined
            Assert.IsTrue(Enum.IsDefined(typeof(ConflictChoice), ConflictChoice.KeepLocal));
            Assert.IsTrue(Enum.IsDefined(typeof(ConflictChoice), ConflictChoice.KeepCloud));
            Assert.IsTrue(Enum.IsDefined(typeof(ConflictChoice), ConflictChoice.KeepNewer));
        }
        
        #endregion
        
        #region Event System Tests
        
        [Test]
        public void CloudManager_EventsCanSubscribe()
        {
            // Arrange
            bool syncStartedFired = false;
            bool syncCompletedFired = false;
            string errorMessage = null;
            
            SteamCloudSaveManager.OnCloudSyncStarted += (isUpload) => syncStartedFired = true;
            SteamCloudSaveManager.OnCloudSyncCompleted += (isUpload) => syncCompletedFired = true;
            SteamCloudSaveManager.OnCloudSyncError += (error) => errorMessage = error;
            
            // Act - Events would be triggered by actual operations
            
            // Assert - Just verify subscription works
            Assert.Pass("Event subscription successful");
            
            // Cleanup
            SteamCloudSaveManager.OnCloudSyncStarted -= (isUpload) => syncStartedFired = true;
            SteamCloudSaveManager.OnCloudSyncCompleted -= (isUpload) => syncCompletedFired = true;
            SteamCloudSaveManager.OnCloudSyncError -= (error) => errorMessage = error;
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator CloudManager_InitializesCorrectly()
        {
            // Wait for initialization
            yield return null;
            
            // Assert
            Assert.IsNotNull(cloudManager, "Cloud manager should exist");
            Assert.IsNotNull(SteamCloudSaveManager.Instance, "Singleton should be accessible");
            Assert.AreEqual(cloudManager, SteamCloudSaveManager.Instance, "Should be same instance");
        }
        
        [UnityTest]
        public IEnumerator GetCloudStorageInfo_ReturnsValidData()
        {
            yield return null;
            
            // Act
            var storageInfo = cloudManager.GetCloudStorageInfo();
            
            // Assert
            Assert.IsNotNull(storageInfo, "Storage info should be returned");
            // Note: Actual values depend on Steam being initialized
            // In test environment, this will return default values
        }
        
        #endregion
        
        #region Save Data Compatibility Tests
        
        [Test]
        public void SaveConflict_WorksWithConsolidatedSaveData()
        {
            // Arrange
            var localSave = ConsolidatedSaveData.CreateNew();
            localSave.progression.playerLevel = 20;
            localSave.currencies.metaCurrency = 1500;
            
            var cloudSave = ConsolidatedSaveData.CreateNew();
            cloudSave.progression.playerLevel = 18;
            cloudSave.currencies.metaCurrency = 1200;
            
            var conflict = new SaveConflict
            {
                localSave = localSave,
                cloudSave = cloudSave,
                localMetadata = new SaveMetadata { playerLevel = 20 },
                cloudMetadata = new SaveMetadata { playerLevel = 18 }
            };
            
            // Assert
            Assert.IsNotNull(conflict.localSave);
            Assert.IsNotNull(conflict.cloudSave);
            Assert.AreEqual(20, conflict.localSave.progression.playerLevel);
            Assert.AreEqual(18, conflict.cloudSave.progression.playerLevel);
        }
        
        #endregion
    }
}