using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Dialog;
using PixelCrushers.DialogueSystem;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for Boss Banter System using Pixel Crushers Dialogue System
    /// Tests multi-language support, rotation logic, and cooldown mechanics
    /// </summary>
    public class BossBanterSystemTests
    {
        private GameObject testGameObject;
        private BossBanterManager banterManager;
        private DialogueSystemController dialogueSystemController;

        [SetUp]
        public void Setup()
        {
            // Create test game object with banter manager
            testGameObject = new GameObject("TestBanterSystem");
            banterManager = testGameObject.AddComponent<BossBanterManager>();
            
            // Create mock Dialogue System Controller
            var dialogueSystemGO = new GameObject("DialogueSystem");
            dialogueSystemController = dialogueSystemGO.AddComponent<DialogueSystemController>();
            
            // Create a simple test database
            var testDatabase = ScriptableObject.CreateInstance<DialogueDatabase>();
            testDatabase.name = "TestBanterDatabase";
            dialogueSystemController.initialDatabase = testDatabase;
            
            // Initialize the banter system
            banterManager.InitializeBanterSystem();
            
            // For tests, we don't need to initialize the full Dialogue System
            // The BossBanterManager works independently for most functionality
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            if (dialogueSystemController != null)
            {
                Object.DestroyImmediate(dialogueSystemController.gameObject);
            }
        }

        #region Initialization Tests

        [Test]
        public void BanterManager_InitializesWithDefaultBossConfigurations()
        {
            // Assert all 8 bosses are configured
            Assert.AreEqual(8, banterManager.bossConfigs.Count);
            
            // Check that all seven sins + devil are present
            var expectedBosses = new[] { "Wrath", "Envy", "Greed", "Lust", "Gluttony", "Sloth", "Pride", "Devil" };
            foreach (var expectedBoss in expectedBosses)
            {
                bool found = banterManager.bossConfigs.Exists(config => config.bossName == expectedBoss);
                Assert.IsTrue(found, $"Boss '{expectedBoss}' not found in configuration");
            }
        }

        [Test]
        public void BanterManager_InitializesBanterTrackingCorrectly()
        {
            // Set a known time for consistent testing
            banterManager.SetTestTime(100f);
            
            // All bosses should start with no previous banter
            foreach (var bossConfig in banterManager.bossConfigs)
            {
                var stats = banterManager.GetBanterStats(bossConfig.bossName);
                Assert.IsNotNull(stats, $"Stats not initialized for {bossConfig.bossName}");
                Assert.AreEqual(-1, stats.lastIndex, $"Last index should be -1 for {bossConfig.bossName}");
                Assert.IsTrue(stats.canTriggerBanter, $"Should be able to trigger banter for {bossConfig.bossName}");
            }
            
            // Cleanup
            banterManager.ClearTestTime();
        }

        #endregion

        #region Banter Rotation Tests

        [Test]
        public void BanterRotation_CyclesThroughAllLinesSequentially()
        {
            string testBoss = "Wrath";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            int totalLines = config.totalBanterLines;
            
            // Trigger banter multiple times and verify rotation
            var triggeredIndices = new List<int>();
            
            // Disable cooldown for testing
            config.cooldownSeconds = 0f;
            
            for (int i = 0; i < totalLines * 2; i++) // Test two full cycles
            {
                bool success = banterManager.TriggerBossBanter(testBoss);
                Assert.IsTrue(success, $"Banter trigger failed on iteration {i}");
                
                var stats = banterManager.GetBanterStats(testBoss);
                triggeredIndices.Add(stats.lastIndex);
            }
            
            // Verify first cycle (indices 0, 1, 2, 3, 4)
            for (int i = 0; i < totalLines; i++)
            {
                Assert.AreEqual(i, triggeredIndices[i], $"First cycle index {i} incorrect");
            }
            
            // Verify second cycle starts over (indices 0, 1, 2, 3, 4)
            for (int i = 0; i < totalLines; i++)
            {
                Assert.AreEqual(i, triggeredIndices[totalLines + i], $"Second cycle index {i} incorrect");
            }
        }

        [Test]
        public void BanterRotation_IndependentPerBoss()
        {
            // Set no cooldown for testing
            foreach (var config in banterManager.bossConfigs)
            {
                config.cooldownSeconds = 0f;
            }
            
            // Trigger different amounts for different bosses
            banterManager.TriggerBossBanter("Wrath"); // Index 0
            banterManager.TriggerBossBanter("Wrath"); // Index 1
            banterManager.TriggerBossBanter("Pride");  // Index 0
            
            var wrathStats = banterManager.GetBanterStats("Wrath");
            var prideStats = banterManager.GetBanterStats("Pride");
            
            Assert.AreEqual(1, wrathStats.lastIndex, "Wrath should be at index 1");
            Assert.AreEqual(0, prideStats.lastIndex, "Pride should be at index 0");
        }

        #endregion

        #region Cooldown System Tests

        [Test]
        public void CooldownSystem_PreventsTriggeringTooQuickly()
        {
            string testBoss = "Envy";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            config.cooldownSeconds = 10f; // Long cooldown for testing
            
            // Set initial test time (use a reasonable value)
            banterManager.SetTestTime(100f);
            
            // First trigger should succeed
            bool firstTrigger = banterManager.TriggerBossBanter(testBoss);
            Assert.IsTrue(firstTrigger, "First banter trigger should succeed");
            
            // Time hasn't advanced - should fail
            bool secondTrigger = banterManager.TriggerBossBanter(testBoss);
            Assert.IsFalse(secondTrigger, "Second banter trigger should fail due to cooldown");
            
            var stats = banterManager.GetBanterStats(testBoss);
            Assert.IsFalse(stats.canTriggerBanter, "Should not be able to trigger banter during cooldown");
            
            // Cleanup
            banterManager.ClearTestTime();
        }

        [Test] 
        public void CooldownSystem_AllowsTriggeringAfterCooldownExpires()
        {
            string testBoss = "Greed";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            config.cooldownSeconds = 10f; // 10 second cooldown
            
            // Set initial test time (use a reasonable value)
            banterManager.SetTestTime(100f);
            
            // First trigger
            bool firstTrigger = banterManager.TriggerBossBanter(testBoss);
            Assert.IsTrue(firstTrigger, "First trigger should succeed");
            
            // Advance time past cooldown
            banterManager.SetTestTime(111f); // 111 - 100 = 11 seconds > 10 second cooldown
            
            // Second trigger should now succeed
            bool secondTrigger = banterManager.TriggerBossBanter(testBoss);
            Assert.IsTrue(secondTrigger, "Second trigger should succeed after cooldown");
            
            // Cleanup
            banterManager.ClearTestTime();
        }

        #endregion

        #region Multi-Language Support Tests

        [Test]
        public void MultiLanguage_SupportedLanguagesIncludeTargetLanguages()
        {
            string[] supportedLanguages = banterManager.GetSupportedLanguages();
            
            Assert.Contains("en", supportedLanguages, "English should be supported");
            Assert.Contains("zh-Hans", supportedLanguages, "Chinese Simplified should be supported");
            Assert.Contains("ur", supportedLanguages, "Urdu should be supported");
            Assert.AreEqual(3, supportedLanguages.Length, "Should support exactly 3 languages");
        }

        [Test]
        public void MultiLanguage_SetLanguageUpdatesCurrentLanguage()
        {
            // Test English
            banterManager.SetLanguage("en");
            Assert.AreEqual("en", banterManager.currentLanguage);
            
            // Test Chinese Simplified
            banterManager.SetLanguage("zh-Hans");
            Assert.AreEqual("zh-Hans", banterManager.currentLanguage);
            
            // Test Urdu
            banterManager.SetLanguage("ur");
            Assert.AreEqual("ur", banterManager.currentLanguage);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void ErrorHandling_InvalidBossNameReturnsFalse()
        {
            bool result = banterManager.TriggerBossBanter("NonexistentBoss");
            Assert.IsFalse(result, "Invalid boss name should return false");
        }

        [Test]
        public void ErrorHandling_EmptyBossNameReturnsFalse()
        {
            bool result = banterManager.TriggerBossBanter("");
            Assert.IsFalse(result, "Empty boss name should return false");
        }

        [Test]
        public void ErrorHandling_NullBossNameReturnsFalse()
        {
            bool result = banterManager.TriggerBossBanter(null);
            Assert.IsFalse(result, "Null boss name should return false");
        }

        [Test]
        public void ErrorHandling_CaseInsensitiveBossNames()
        {
            // Set no cooldown for testing
            var config = banterManager.bossConfigs.Find(c => c.bossName == "Lust");
            config.cooldownSeconds = 0f;
            
            // All these should work
            Assert.IsTrue(banterManager.TriggerBossBanter("lust"), "Lowercase should work");
            Assert.IsTrue(banterManager.TriggerBossBanter("LUST"), "Uppercase should work");
            Assert.IsTrue(banterManager.TriggerBossBanter("LuSt"), "Mixed case should work");
        }

        #endregion

        #region Statistics and Debugging Tests

        [Test]
        public void Statistics_GetBanterStatsReturnsCorrectInfo()
        {
            string testBoss = "Sloth";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            config.cooldownSeconds = 0f;
            
            // Initial stats
            var initialStats = banterManager.GetBanterStats(testBoss);
            Assert.AreEqual(testBoss, initialStats.bossName);
            Assert.AreEqual(config.totalBanterLines, initialStats.totalLines);
            Assert.AreEqual(-1, initialStats.lastIndex);
            Assert.IsTrue(initialStats.canTriggerBanter);
            
            // After one trigger
            banterManager.TriggerBossBanter(testBoss);
            var afterTriggerStats = banterManager.GetBanterStats(testBoss);
            Assert.AreEqual(0, afterTriggerStats.lastIndex);
        }

        [Test] 
        public void ResetFunctionality_ResetBossRotationWorksCorrectly()
        {
            string testBoss = "Pride";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            config.cooldownSeconds = 0f;
            
            // Trigger a few times
            banterManager.TriggerBossBanter(testBoss);
            banterManager.TriggerBossBanter(testBoss);
            banterManager.TriggerBossBanter(testBoss);
            
            var statsBeforeReset = banterManager.GetBanterStats(testBoss);
            Assert.AreEqual(2, statsBeforeReset.lastIndex, "Should be at index 2 before reset");
            
            // Reset
            banterManager.ResetBossRotation(testBoss);
            
            var statsAfterReset = banterManager.GetBanterStats(testBoss);
            Assert.AreEqual(-1, statsAfterReset.lastIndex, "Should be at index -1 after reset");
            Assert.IsTrue(statsAfterReset.canTriggerBanter, "Should be able to trigger after reset");
        }

        #endregion

        #region Integration Tests

        [UnityTest]
        public IEnumerator Integration_BanterSystemIntegratesWithDialogueSystem()
        {
            // This test would require actual dialogue database with banter conversations
            yield return null;
            
            string testBoss = "Devil";
            var config = banterManager.bossConfigs.Find(c => c.bossName == testBoss);
            config.cooldownSeconds = 0f;
            
            // In a real scenario, this would start a conversation
            // For now, we just verify the method doesn't throw
            Assert.DoesNotThrow(() => {
                banterManager.TriggerBossBanter(testBoss);
            });
            
            yield return null;
        }

        #endregion
    }
}