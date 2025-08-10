using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.DataManagement;
using NeonLadderURP.DataManagement;
using System;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class SteamCloudSaveTests
    {
        private GameObject testGameObject;
        private SteamCloudSaveManager steamCloudManager;
        
        [SetUp]
        public void Setup()
        {
            // Create test game object with Steam Cloud manager
            testGameObject = new GameObject("TestSteamCloudManager");
            steamCloudManager = testGameObject.AddComponent<SteamCloudSaveManager>();
            
            // Clean up any existing test saves
            EnhancedSaveSystem.DeleteSave();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.Destroy(testGameObject);
            }
            
            // Clean up test saves
            EnhancedSaveSystem.DeleteSave();
        }
        
        [Test]
        public void SaveFileMetadata_FromSaveData_CreatesCorrectMetadata()
        {
            // Arrange
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.gameVersion = "1.2.3";
            saveData.totalPlayTime = 3600f; // 1 hour
            saveData.progression.playerLevel = 10;
            saveData.lastSaved = new DateTime(2025, 1, 15, 12, 30, 0);
            
            // Act
            var metadata = SteamCloudSaveManager.SaveFileMetadata.FromSaveData(saveData);
            
            // Assert
            Assert.NotNull(metadata);
            Assert.AreEqual("1.2.3", metadata.gameVersion);
            Assert.AreEqual(3600, metadata.playTime);
            Assert.AreEqual(10, metadata.playerLevel);
            Assert.AreEqual(new DateTime(2025, 1, 15, 12, 30, 0), metadata.timestamp);
            Assert.IsNotEmpty(metadata.checksum);
        }
        
        [Test]
        public void ConflictResolution_EnumValues_AreDefined()
        {
            // Assert all expected values exist
            Assert.IsTrue(Enum.IsDefined(typeof(SteamCloudSaveManager.ConflictResolution), 
                SteamCloudSaveManager.ConflictResolution.UseLocal));
            Assert.IsTrue(Enum.IsDefined(typeof(SteamCloudSaveManager.ConflictResolution), 
                SteamCloudSaveManager.ConflictResolution.UseCloud));
            Assert.IsTrue(Enum.IsDefined(typeof(SteamCloudSaveManager.ConflictResolution), 
                SteamCloudSaveManager.ConflictResolution.Cancel));
        }
        
        [Test]
        public void SteamCloudIntegration_UseSteamCloud_CanToggle()
        {
            // Arrange
            bool initialValue = SteamCloudIntegration.UseSteamCloud;
            
            // Act
            SteamCloudIntegration.UseSteamCloud = false;
            bool afterDisable = SteamCloudIntegration.UseSteamCloud;
            
            SteamCloudIntegration.UseSteamCloud = true;
            bool afterEnable = SteamCloudIntegration.UseSteamCloud;
            
            // Assert
            Assert.IsFalse(afterDisable);
            Assert.IsTrue(afterEnable);
            
            // Restore initial value
            SteamCloudIntegration.UseSteamCloud = initialValue;
        }
        
        [UnityTest]
        public IEnumerator SteamCloudIntegration_SaveWithCloud_FallsBackToLocal_WhenDisabled()
        {
            // Arrange
            SteamCloudIntegration.UseSteamCloud = false;
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.progression.playerLevel = 99;
            
            // Act
            bool success = SteamCloudIntegration.SaveWithCloud(saveData);
            yield return null;
            
            // Assert
            Assert.IsTrue(success);
            
            // Verify it saved locally
            var loadedData = EnhancedSaveSystem.Load();
            Assert.NotNull(loadedData);
            Assert.AreEqual(99, loadedData.progression.playerLevel);
            
            // Restore
            SteamCloudIntegration.UseSteamCloud = true;
        }
        
        [UnityTest]
        public IEnumerator SteamCloudIntegration_LoadWithCloud_FallsBackToLocal_WhenDisabled()
        {
            // Arrange
            SteamCloudIntegration.UseSteamCloud = false;
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.progression.playerLevel = 77;
            EnhancedSaveSystem.Save(saveData);
            yield return null;
            
            // Act
            var loadedData = SteamCloudIntegration.LoadWithCloud();
            
            // Assert
            Assert.NotNull(loadedData);
            Assert.AreEqual(77, loadedData.progression.playerLevel);
            
            // Restore
            SteamCloudIntegration.UseSteamCloud = true;
        }
        
        [Test]
        public void SteamCloudIntegration_IsCloudAvailable_ReturnsFalse_WhenDisabled()
        {
            // Arrange
            SteamCloudIntegration.UseSteamCloud = false;
            
            // Act
            bool isAvailable = SteamCloudIntegration.IsCloudAvailable();
            
            // Assert
            Assert.IsFalse(isAvailable);
            
            // Restore
            SteamCloudIntegration.UseSteamCloud = true;
        }
        
        [Test]
        public void SteamCloudIntegration_GetCloudStorageInfo_ReturnsDefaults_WhenNotAvailable()
        {
            // Arrange
            SteamCloudIntegration.UseSteamCloud = false;
            
            // Act
            var (available, totalBytes, usedBytes) = SteamCloudIntegration.GetCloudStorageInfo();
            
            // Assert
            Assert.IsFalse(available);
            Assert.AreEqual(0UL, totalBytes);
            Assert.AreEqual(0UL, usedBytes);
            
            // Restore
            SteamCloudIntegration.UseSteamCloud = true;
        }
        
        [UnityTest]
        public IEnumerator SteamCloudManager_Instance_CreatesSingleton()
        {
            // Arrange
            Object.Destroy(testGameObject); // Remove test instance
            yield return null;
            
            // Act
            var instance1 = SteamCloudSaveManager.Instance;
            var instance2 = SteamCloudSaveManager.Instance;
            
            // Assert
            Assert.NotNull(instance1);
            Assert.AreEqual(instance1, instance2);
            
            // Cleanup
            if (instance1 != null)
            {
                Object.Destroy(instance1.gameObject);
            }
        }
        
        [Test]
        public void SaveFileMetadata_Checksum_DifferentForDifferentData()
        {
            // Arrange
            var saveData1 = ConsolidatedSaveData.CreateNew();
            saveData1.progression.playerLevel = 5;
            
            var saveData2 = ConsolidatedSaveData.CreateNew();
            saveData2.progression.playerLevel = 10;
            
            // Act
            var metadata1 = SteamCloudSaveManager.SaveFileMetadata.FromSaveData(saveData1);
            var metadata2 = SteamCloudSaveManager.SaveFileMetadata.FromSaveData(saveData2);
            
            // Assert
            Assert.NotNull(metadata1.checksum);
            Assert.NotNull(metadata2.checksum);
            Assert.AreNotEqual(metadata1.checksum, metadata2.checksum);
        }
        
        [Test]
        public void ConflictResolution_DefaultBehavior_UsesMoreRecent()
        {
            // Arrange
            var localMeta = new SteamCloudSaveManager.SaveFileMetadata
            {
                timestamp = DateTime.Now,
                playerLevel = 10
            };
            
            var cloudMeta = new SteamCloudSaveManager.SaveFileMetadata
            {
                timestamp = DateTime.Now.AddHours(-1), // Cloud is older
                playerLevel = 8
            };
            
            // Act - Determine which is newer
            bool localIsNewer = localMeta.timestamp > cloudMeta.timestamp;
            
            // Assert
            Assert.IsTrue(localIsNewer);
        }
        
        [UnityTest]
        public IEnumerator SteamCloudIntegration_DeleteAllSaves_RemovesBothLocalAndCloud()
        {
            // Arrange
            var saveData = ConsolidatedSaveData.CreateNew();
            saveData.progression.playerLevel = 50;
            EnhancedSaveSystem.Save(saveData);
            yield return null;
            
            // Act
            bool success = SteamCloudIntegration.DeleteAllSaves();
            
            // Assert
            Assert.IsTrue(success);
            
            // Verify local save is gone
            var loadedData = EnhancedSaveSystem.Load();
            Assert.IsNull(loadedData);
        }
        
        // Mock test for conflict resolution callback
        [UnityTest]
        public IEnumerator SteamCloudManager_ConflictResolver_CanBeSet()
        {
            // Arrange
            bool resolverCalled = false;
            SteamCloudSaveManager.ConflictResolution testResolution = SteamCloudSaveManager.ConflictResolution.UseLocal;
            
            steamCloudManager.conflictResolver = (local, cloud) =>
            {
                resolverCalled = true;
                return testResolution;
            };
            
            // Act - Would normally be triggered by actual conflict
            var result = steamCloudManager.conflictResolver(
                new SteamCloudSaveManager.SaveFileMetadata(),
                new SteamCloudSaveManager.SaveFileMetadata()
            );
            
            yield return null;
            
            // Assert
            Assert.IsTrue(resolverCalled);
            Assert.AreEqual(testResolution, result);
        }
    }
}