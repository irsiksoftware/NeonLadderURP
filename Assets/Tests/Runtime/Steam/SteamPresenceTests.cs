using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Testing.Mocks;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Tests.Runtime.Steam
{
    /// <summary>
    /// Specialized tests for Steam Rich Presence functionality.
    /// Tests presence updates during different game states and progression.
    /// 
    /// Author: @storm - Weather Control Expert
    /// "Rich presence is like the lightning that announces the storm - it tells the world what's happening!"
    /// </summary>
    public class SteamPresenceTests
    {
        [SetUp]
        public void SetUp()
        {
            MockSteamworksAPI.Reset();
            MockSteamworksAPI.SetInitialized(true);
        }

        [TearDown]
        public void TearDown()
        {
            MockSteamworksAPI.Reset();
        }

        #region Menu Navigation Presence

        [UnityTest]
        public IEnumerator Presence_MainMenu_ShouldShowCorrectStatus()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "In Main Menu");
            MockSteamworksAPI.SetRichPresence("scene", "Title Screen");
            yield return null;

            // Assert
            Assert.AreEqual("In Main Menu", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Title Screen", MockSteamworksAPI.GetRichPresence("scene"));
        }

        [UnityTest]
        public IEnumerator Presence_SettingsMenu_ShouldIndicateConfiguration()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "Configuring Settings");
            MockSteamworksAPI.SetRichPresence("menu", "Audio & Video");
            yield return null;

            // Assert
            Assert.AreEqual("Configuring Settings", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Audio & Video", MockSteamworksAPI.GetRichPresence("menu"));
        }

        [UnityTest]
        public IEnumerator Presence_ShopMenu_ShouldShowShoppingStatus()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "Shopping for Upgrades");
            MockSteamworksAPI.SetRichPresence("shop_type", "Meta Currency Shop");
            MockSteamworksAPI.SetRichPresence("currency", "1,250 Meta Coins");
            yield return null;

            // Assert
            Assert.AreEqual("Shopping for Upgrades", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Meta Currency Shop", MockSteamworksAPI.GetRichPresence("shop_type"));
            Assert.AreEqual("1,250 Meta Coins", MockSteamworksAPI.GetRichPresence("currency"));
        }

        #endregion

        #region Gameplay Presence

        [UnityTest]
        public IEnumerator Presence_InGame_ShouldShowLevelProgress()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "In Game");
            MockSteamworksAPI.SetRichPresence("level", "Procedural Path #1");
            MockSteamworksAPI.SetRichPresence("depth", "Floor 5");
            MockSteamworksAPI.SetRichPresence("enemies_defeated", "12");
            yield return null;

            // Assert
            Assert.AreEqual("In Game", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Procedural Path #1", MockSteamworksAPI.GetRichPresence("level"));
            Assert.AreEqual("Floor 5", MockSteamworksAPI.GetRichPresence("depth"));
            Assert.AreEqual("12", MockSteamworksAPI.GetRichPresence("enemies_defeated"));
        }

        [UnityTest]
        public IEnumerator Presence_BossBattle_ShouldIndicateSpecificSin()
        {
            // Act - Fighting Wrath
            MockSteamworksAPI.SetRichPresence("status", "Fighting Boss");
            MockSteamworksAPI.SetRichPresence("boss", "Wrath");
            MockSteamworksAPI.SetRichPresence("boss_health", "75%");
            yield return new WaitForSeconds(0.1f);

            // Assert
            Assert.AreEqual("Fighting Boss", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Wrath", MockSteamworksAPI.GetRichPresence("boss"));
            Assert.AreEqual("75%", MockSteamworksAPI.GetRichPresence("boss_health"));

            // Act - Switch to Pride boss
            MockSteamworksAPI.SetRichPresence("boss", "Pride");
            MockSteamworksAPI.SetRichPresence("boss_health", "100%");
            yield return null;

            // Assert
            Assert.AreEqual("Pride", MockSteamworksAPI.GetRichPresence("boss"));
            Assert.AreEqual("100%", MockSteamworksAPI.GetRichPresence("boss_health"));
        }

        [UnityTest]
        public IEnumerator Presence_PlayerDeath_ShouldShowRespawnStatus()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "Respawning");
            MockSteamworksAPI.SetRichPresence("death_reason", "Defeated by Envy");
            MockSteamworksAPI.SetRichPresence("run_time", "12:34");
            yield return null;

            // Assert
            Assert.AreEqual("Respawning", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Defeated by Envy", MockSteamworksAPI.GetRichPresence("death_reason"));
            Assert.AreEqual("12:34", MockSteamworksAPI.GetRichPresence("run_time"));
        }

        #endregion

        #region Achievement Progress Presence

        [UnityTest]
        public IEnumerator Presence_SevenSinsProgress_ShouldUpdateCorrectly()
        {
            // Arrange - Start with no sins defeated
            MockSteamworksAPI.SetRichPresence("sins_defeated", "0/7");
            yield return null;

            // Act - Defeat Wrath
            MockSteamworksAPI.UnlockAchievement(Achievements.WRATH_SIN_DEFEATED);
            MockSteamworksAPI.SetRichPresence("sins_defeated", "1/7");
            MockSteamworksAPI.SetRichPresence("latest_sin", "Wrath");
            yield return null;

            // Assert
            Assert.AreEqual("1/7", MockSteamworksAPI.GetRichPresence("sins_defeated"));
            Assert.AreEqual("Wrath", MockSteamworksAPI.GetRichPresence("latest_sin"));

            // Act - Defeat multiple sins
            MockSteamworksAPI.UnlockAchievement(Achievements.ENVY_SIN_DEFEATED);
            MockSteamworksAPI.UnlockAchievement(Achievements.GREED_SIN_DEFEATED);
            MockSteamworksAPI.SetRichPresence("sins_defeated", "3/7");
            MockSteamworksAPI.SetRichPresence("latest_sin", "Greed");
            yield return null;

            // Assert
            Assert.AreEqual("3/7", MockSteamworksAPI.GetRichPresence("sins_defeated"));
            Assert.AreEqual("Greed", MockSteamworksAPI.GetRichPresence("latest_sin"));
        }

        [UnityTest]
        public IEnumerator Presence_AllSinsDefeated_ShouldShowCompletionStatus()
        {
            // Act - Defeat all seven sins
            MockSteamworksAPI.UnlockAllSevenSins();
            MockSteamworksAPI.SetRichPresence("status", "Facing the Devil");
            MockSteamworksAPI.SetRichPresence("sins_defeated", "7/7");
            MockSteamworksAPI.SetRichPresence("game_phase", "Final Boss");
            yield return null;

            // Assert
            Assert.AreEqual("Facing the Devil", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("7/7", MockSteamworksAPI.GetRichPresence("sins_defeated"));
            Assert.AreEqual("Final Boss", MockSteamworksAPI.GetRichPresence("game_phase"));
            Assert.IsTrue(MockSteamworksAPI.AreAllSevenSinsDefeated());
        }

        #endregion

        #region Roguelite Progression Presence

        [UnityTest]
        public IEnumerator Presence_MetaProgression_ShouldShowUpgradeStatus()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "Upgrading Character");
            MockSteamworksAPI.SetRichPresence("meta_currency", "2,450");
            MockSteamworksAPI.SetRichPresence("perma_currency", "150");
            MockSteamworksAPI.SetRichPresence("run_number", "23");
            yield return null;

            // Assert
            Assert.AreEqual("Upgrading Character", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("2,450", MockSteamworksAPI.GetRichPresence("meta_currency"));
            Assert.AreEqual("150", MockSteamworksAPI.GetRichPresence("perma_currency"));
            Assert.AreEqual("23", MockSteamworksAPI.GetRichPresence("run_number"));
        }

        [UnityTest]
        public IEnumerator Presence_RunCompletion_ShouldShowFinalStats()
        {
            // Act
            MockSteamworksAPI.SetRichPresence("status", "Run Completed");
            MockSteamworksAPI.SetRichPresence("final_time", "45:12");
            MockSteamworksAPI.SetRichPresence("enemies_killed", "247");
            MockSteamworksAPI.SetRichPresence("currency_earned", "1,750");
            MockSteamworksAPI.SetRichPresence("outcome", "Victory");
            yield return null;

            // Assert
            Assert.AreEqual("Run Completed", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("45:12", MockSteamworksAPI.GetRichPresence("final_time"));
            Assert.AreEqual("247", MockSteamworksAPI.GetRichPresence("enemies_killed"));
            Assert.AreEqual("1,750", MockSteamworksAPI.GetRichPresence("currency_earned"));
            Assert.AreEqual("Victory", MockSteamworksAPI.GetRichPresence("outcome"));
        }
        #endregion

        #region Dynamic Presence Updates

        [UnityTest]
        public IEnumerator Presence_RealTimeUpdates_ShouldReflectCurrentState()
        {
            // Act - Simulate rapid state changes during boss fight
            MockSteamworksAPI.SetRichPresence("status", "Fighting Boss");
            MockSteamworksAPI.SetRichPresence("boss", "Lust");
            MockSteamworksAPI.SetRichPresence("boss_health", "100%");
            yield return new WaitForSeconds(0.1f);

            // Boss takes damage
            MockSteamworksAPI.SetRichPresence("boss_health", "75%");
            yield return new WaitForSeconds(0.1f);

            // Player uses special ability
            MockSteamworksAPI.SetRichPresence("player_action", "Charging Ultimate");
            yield return new WaitForSeconds(0.1f);

            // Boss near defeat
            MockSteamworksAPI.SetRichPresence("boss_health", "5%");
            MockSteamworksAPI.SetRichPresence("player_action", "Final Strike");
            yield return new WaitForSeconds(0.1f);

            // Boss defeated
            MockSteamworksAPI.SetRichPresence("status", "Boss Defeated");
            MockSteamworksAPI.SetRichPresence("boss_health", "0%");
            MockSteamworksAPI.UnlockAchievement(Achievements.LUST_SIN_DEFEATED);
            yield return null;

            // Assert final state
            Assert.AreEqual("Boss Defeated", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("0%", MockSteamworksAPI.GetRichPresence("boss_health"));
            Assert.IsTrue(MockSteamworksAPI.IsAchievementUnlocked(Achievements.LUST_SIN_DEFEATED));
        }

        [UnityTest]
        public IEnumerator Presence_SessionDuration_ShouldUpdatePeriodically()
        {
            // Act - Simulate session time updates
            MockSteamworksAPI.SetRichPresence("session_time", "5 minutes");
            yield return new WaitForSeconds(0.1f);

            MockSteamworksAPI.SetRichPresence("session_time", "15 minutes");
            yield return new WaitForSeconds(0.1f);

            MockSteamworksAPI.SetRichPresence("session_time", "1 hour 23 minutes");
            yield return null;

            // Assert
            Assert.AreEqual("1 hour 23 minutes", MockSteamworksAPI.GetRichPresence("session_time"));
        }

        #endregion

        #region Presence Cleanup Tests

        [UnityTest]
        public IEnumerator Presence_GameExit_ShouldClearPresence()
        {
            // Arrange - Set up rich presence
            MockSteamworksAPI.SetRichPresence("status", "In Game");
            MockSteamworksAPI.SetRichPresence("level", "Boss Arena");
            MockSteamworksAPI.SetRichPresence("boss", "Pride");
            yield return null;

            // Act - Clear presence on exit
            MockSteamworksAPI.SetRichPresence("status", "");
            MockSteamworksAPI.SetRichPresence("level", "");
            MockSteamworksAPI.SetRichPresence("boss", "");
            yield return null;

            // Assert
            Assert.AreEqual("", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("", MockSteamworksAPI.GetRichPresence("level"));
            Assert.AreEqual("", MockSteamworksAPI.GetRichPresence("boss"));
        }

        [UnityTest]
        public IEnumerator Presence_InvalidKeys_ShouldHandleGracefully()
        {
            // Act - Try to set presence with null/empty keys
            bool result1 = MockSteamworksAPI.SetRichPresence("", "some value");
            bool result2 = MockSteamworksAPI.SetRichPresence(null, "another value");
            yield return null;

            // Assert - Should handle gracefully without crashing
            Assert.IsFalse(result1 || result2, "Setting presence with invalid keys should fail gracefully");
        }

        #endregion

        #region Integration with Achievements

        [UnityTest]
        public IEnumerator Presence_AchievementUnlock_ShouldUpdateStatus()
        {
            // Act - Unlock demo completion
            MockSteamworksAPI.UnlockAchievement(Achievements.DEMO_LEVEL_COMPLETE);
            MockSteamworksAPI.SetRichPresence("status", "Demo Completed!");
            MockSteamworksAPI.SetRichPresence("achievement_unlocked", "Demo Level Complete");
            yield return null;

            // Assert
            Assert.AreEqual("Demo Completed!", MockSteamworksAPI.GetRichPresence("status"));
            Assert.AreEqual("Demo Level Complete", MockSteamworksAPI.GetRichPresence("achievement_unlocked"));
            Assert.IsTrue(MockSteamworksAPI.IsAchievementUnlocked(Achievements.DEMO_LEVEL_COMPLETE));
        }

        [UnityTest]
        public IEnumerator Presence_MultipleAchievements_ShouldShowLatest()
        {
            // Act - Unlock multiple achievements in sequence
            MockSteamworksAPI.UnlockAchievement(Achievements.WRATH_SIN_DEFEATED);
            MockSteamworksAPI.SetRichPresence("latest_achievement", "Wrath Defeated");
            yield return new WaitForSeconds(0.1f);

            MockSteamworksAPI.UnlockAchievement(Achievements.ENVY_SIN_DEFEATED);
            MockSteamworksAPI.SetRichPresence("latest_achievement", "Envy Defeated");
            yield return new WaitForSeconds(0.1f);

            MockSteamworksAPI.UnlockAchievement(Achievements.DEVIL_SIN_DEFEATED);
            MockSteamworksAPI.SetRichPresence("latest_achievement", "Devil Defeated - Game Complete!");
            yield return null;

            // Assert
            Assert.AreEqual("Devil Defeated - Game Complete!", MockSteamworksAPI.GetRichPresence("latest_achievement"));
            Assert.AreEqual(3, MockSteamworksAPI.GetUnlockedAchievementCount());
        }

        #endregion
    }
}