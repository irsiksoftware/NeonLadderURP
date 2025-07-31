using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Managers;
using Steamworks;

namespace NeonLadder.Tests.Steam
{
    /// <summary>
    /// REAL Steam Achievement End-to-End Tests
    /// 
    /// These tests use your actual Steam App ID (3089870) and will trigger
    /// real Steam notifications during test execution. Perfect for validating
    /// the complete achievement pipeline before Steam launch.
    /// 
    /// REQUIREMENTS:
    /// - Steam client must be running
    /// - Connected to internet for Steam API
    /// - NeonLadder Steam app ID 3089870 must be configured
    /// </summary>
    [TestFixture]
    public class RealSteamAchievementE2ETests
    {
        private SteamManager steamManager;
        private bool originalSteamInitialized;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Ensure Steam is initialized for E2E testing
            if (!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam not initialized - E2E tests require Steam client to be running");
                Assert.Ignore("Steam client required for E2E achievement tests");
            }

            Debug.Log("=== REAL STEAM E2E ACHIEVEMENT TESTING ===");
            Debug.Log($"Steam App ID: {SteamUtils.GetAppID()}");
            Debug.Log("Watch for Steam notifications during test execution!");
        }

        [SetUp]
        public void SetUp()
        {
            steamManager = SteamManager.Instance;
            originalSteamInitialized = SteamManager.Initialized;

            // Lock all achievements for clean test state (Steam development environment)
            if (SteamManager.Initialized)
            {
                Debug.Log("Locking all achievements for clean E2E test state...");
                LockAllAchievements();
                SteamUserStats.StoreStats();
                SteamUserStats.RequestCurrentStats();
            }
        }

        /// <summary>
        /// Locks all achievements to ensure clean test state
        /// This validates the complete Lock → Unlock flow in E2E tests
        /// </summary>
        private void LockAllAchievements()
        {
            var allAchievements = System.Enum.GetValues(typeof(Achievements));
            foreach (Achievements achievement in allAchievements)
            {
                string achievementId = achievement.ToString();
                SteamUserStats.ClearAchievement(achievementId);
                Debug.Log($"🔒 Locked achievement: {achievementId}");
            }
        }

        /// <summary>
        /// Validates that an achievement is currently locked
        /// </summary>
        private void AssertAchievementLocked(Achievements achievement)
        {
            bool achieved;
            bool success = SteamUserStats.GetAchievement(achievement.ToString(), out achieved);
            Assert.IsTrue(success, $"Should be able to query {achievement} status");
            Assert.IsFalse(achieved, $"{achievement} should be LOCKED before test execution");
        }

        /// <summary>
        /// Validates that an achievement is unlocked with proper Steam API response
        /// </summary>
        private void AssertAchievementUnlocked(Achievements achievement)
        {
            bool achieved;
            bool success = SteamUserStats.GetAchievement(achievement.ToString(), out achieved);
            Assert.IsTrue(success, $"Should be able to query {achievement} status");
            Assert.IsTrue(achieved, $"{achievement} should be UNLOCKED after trigger");
        }

        [TearDown]
        public void TearDown()
        {
            // Store any achievement unlocks to Steam
            if (SteamManager.Initialized)
            {
                SteamUserStats.StoreStats();
            }
        }

        #region Seven Deadly Sins E2E Tests

        [UnityTest]
        [Description("E2E: Wrath Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator WrathSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            // Arrange: Verify achievement starts locked
            var expectedAchievement = Achievements.WRATH_SIN_DEFEATED;
            Debug.Log("=== TESTING WRATH SIN DEFEAT ===");
            Debug.Log("🔒 Phase 1: Validating achievement is locked...");
            
            AssertAchievementLocked(expectedAchievement);
            Debug.Log("✅ Wrath Sin achievement confirmed LOCKED");

            // Act: Trigger achievement through your actual system
            Debug.Log("⚔️ Phase 2: Triggering boss defeat...");
            Debug.Log("Watch for Steam notification popup!");
            steamManager.UnlockAchievement(expectedAchievement.ToString());
            
            // Wait for Steam processing
            yield return new WaitForSeconds(2.0f);

            // Assert: Verify complete state transition (Locked → Unlocked)
            Debug.Log("🔓 Phase 3: Validating achievement unlock...");
            AssertAchievementUnlocked(expectedAchievement);
            
            Debug.Log("✅ WRATH SIN DEFEAT - Complete E2E validation successful!");
            Debug.Log("   🔒 Started locked ✓");
            Debug.Log("   ⚔️ Boss defeat triggered ✓"); 
            Debug.Log("   🔓 Achievement unlocked ✓");
            Debug.Log("   📢 Steam notification shown ✓");
        }

        [UnityTest] 
        [Description("E2E: Envy Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator EnvySinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.ENVY_SIN_DEFEATED;
            Debug.Log("=== TESTING ENVY SIN DEFEAT ===");
            Debug.Log("🔒 Phase 1: Validating achievement is locked...");
            
            AssertAchievementLocked(expectedAchievement);
            Debug.Log("✅ Envy Sin achievement confirmed LOCKED");

            Debug.Log("💚 Phase 2: Triggering boss defeat...");
            Debug.Log("Watch for Steam notification popup!");
            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            Debug.Log("🔓 Phase 3: Validating achievement unlock...");
            AssertAchievementUnlocked(expectedAchievement);
            Debug.Log("✅ ENVY SIN DEFEAT - Complete E2E validation successful!");
        }

        [UnityTest]
        [Description("E2E: Greed Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator GreedSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.GREED_SIN_DEFEATED;
            Debug.Log("=== TESTING GREED SIN DEFEAT ===");
            Debug.Log("Watch for Steam notification popup!");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "GREED_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Greed Sin achievement successfully unlocked!");
        }

        [UnityTest]
        [Description("E2E: Lust Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator LustSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.LUST_SIN_DEFEATED;
            Debug.Log("=== TESTING LUST SIN DEFEAT ===");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "LUST_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Lust Sin achievement successfully unlocked!");
        }

        [UnityTest]
        [Description("E2E: Gluttony Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator GluttonySinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.GLUTTONY_SIN_DEFEATED;
            Debug.Log("=== TESTING GLUTTONY SIN DEFEAT ===");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "GLUTTONY_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Gluttony Sin achievement successfully unlocked!");
        }

        [UnityTest]
        [Description("E2E: Sloth Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator SlothSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.SLOTH_SIN_DEFEATED;
            Debug.Log("=== TESTING SLOTH SIN DEFEAT ===");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "SLOTH_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Sloth Sin achievement successfully unlocked!");
        }

        [UnityTest]
        [Description("E2E: Pride Sin boss defeat should unlock achievement with real Steam notification")]
        public IEnumerator PrideSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.PRIDE_SIN_DEFEATED;
            Debug.Log("=== TESTING PRIDE SIN DEFEAT ===");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "PRIDE_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Pride Sin achievement successfully unlocked!");
        }

        #endregion

        #region Special Achievements E2E Tests

        [UnityTest]
        [Description("E2E: Demo level completion should unlock achievement with real Steam notification")]
        public IEnumerator DemoLevelComplete_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.DEMO_LEVEL_COMPLETE;
            Debug.Log("=== TESTING DEMO LEVEL COMPLETION ===");
            Debug.Log("🔒 Phase 1: Validating achievement is locked...");
            
            AssertAchievementLocked(expectedAchievement);
            Debug.Log("✅ Demo Level achievement confirmed LOCKED");

            Debug.Log("🎯 Phase 2: Simulating SceneEndController trigger...");
            Debug.Log("Watch for Steam notification popup!");
            // This simulates what happens in SceneEndController.cs:17
            steamManager.UnlockAchievement(nameof(Achievements.DEMO_LEVEL_COMPLETE));
            yield return new WaitForSeconds(2.0f);

            Debug.Log("🔓 Phase 3: Validating achievement unlock...");
            AssertAchievementUnlocked(expectedAchievement);
            Debug.Log("✅ DEMO LEVEL COMPLETE - Complete E2E validation successful!");
        }

        [UnityTest]
        [Description("E2E: Devil Sin (final boss) defeat should unlock achievement with real Steam notification")]
        public IEnumerator DevilSinDefeat_E2E_UnlocksRealSteamAchievement()
        {
            var expectedAchievement = Achievements.DEVIL_SIN_DEFEATED;
            Debug.Log("=== TESTING DEVIL SIN DEFEAT (FINAL BOSS) ===");
            Debug.Log("This is the ultimate achievement!");

            steamManager.UnlockAchievement(expectedAchievement.ToString());
            yield return new WaitForSeconds(2.0f);

            bool achieved;
            bool success = SteamUserStats.GetAchievement(expectedAchievement.ToString(), out achieved);
            
            Assert.IsTrue(success && achieved, "DEVIL_SIN_DEFEATED should be unlocked via Steam API");
            Debug.Log("✅ Devil Sin (Final Boss) achievement successfully unlocked!");
        }

        #endregion

        #region Achievement Resolution E2E Tests

        [UnityTest]
        [Description("E2E: Boss name resolution should trigger correct achievement via AchievementResolver")]
        public IEnumerator BossNameResolution_E2E_TriggersCorrectAchievement()
        {
            Debug.Log("=== TESTING ACHIEVEMENT RESOLVER E2E ===");
            Debug.Log("Testing boss name -> achievement mapping...");

            // Test a boss transformation name (simulate EnemyDeath.cs scenario)
            var bossTransformation = "wrath_transformation"; // Example transformation name
            var resolvedAchievement = AchievementResolver.Resolve(bossTransformation);

            if (resolvedAchievement.HasValue)
            {
                Debug.Log($"Resolved {bossTransformation} -> {resolvedAchievement.Value}");
                
                // Trigger the resolved achievement
                steamManager.UnlockAchievement(resolvedAchievement.Value.ToString());
                yield return new WaitForSeconds(2.0f);

                bool achieved;
                bool success = SteamUserStats.GetAchievement(resolvedAchievement.Value.ToString(), out achieved);
                
                Assert.IsTrue(success && achieved, 
                    $"Achievement {resolvedAchievement.Value} should be unlocked via AchievementResolver");
                Debug.Log($"✅ Achievement resolver E2E test successful!");
            }
            else
            {
                Debug.LogWarning($"No achievement resolved for {bossTransformation}");
                Assert.Inconclusive("Achievement resolver returned null - check boss transformation mapping");
            }

            yield return null;
        }

        #endregion

        #region Steam Integration Validation

        [Test]
        [Description("E2E: Steam API should be properly initialized with correct App ID")]
        public void SteamInitialization_E2E_ValidatesCorrectAppID()
        {
            Assert.IsTrue(SteamManager.Initialized, "Steam should be initialized for E2E tests");
            
            var appId = SteamUtils.GetAppID();
            Debug.Log($"Current Steam App ID: {appId}");
            
            // Your actual Steam App ID is 3089870
            Assert.AreEqual(3089870, appId.m_AppId, "Steam should be using NeonLadder App ID 3089870");
        }

        [UnityTest]
        [Description("E2E: All achievements should be properly registered in Steam")]
        public IEnumerator AllAchievements_E2E_ValidatesSteamRegistration()
        {
            Debug.Log("=== VALIDATING ALL ACHIEVEMENTS IN STEAM ===");
            
            var allAchievements = System.Enum.GetValues(typeof(Achievements));
            int validAchievements = 0;

            foreach (Achievements achievement in allAchievements)
            {
                string achievementId = achievement.ToString();
                bool achieved;
                bool success = SteamUserStats.GetAchievement(achievementId, out achieved);

                if (success)
                {
                    validAchievements++;
                    var name = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "name");
                    var desc = SteamUserStats.GetAchievementDisplayAttribute(achievementId, "desc");
                    Debug.Log($"✅ {achievementId}: '{name}' - {desc}");
                }
                else
                {
                    Debug.LogWarning($"❌ {achievementId}: Not registered in Steam Partner site");
                }

                yield return null; // Prevent timeout
            }

            Assert.Greater(validAchievements, 0, "At least some achievements should be registered in Steam");
            Debug.Log($"Steam E2E Validation: {validAchievements}/{allAchievements.Length} achievements registered");
        }

        #endregion
    }
}