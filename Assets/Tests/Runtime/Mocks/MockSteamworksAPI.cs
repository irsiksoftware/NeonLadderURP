using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Testing.Mocks
{
    /// <summary>
    /// Mock implementation of Steamworks API for testing purposes.
    /// Provides testable simulation of Steam functionality without requiring actual Steam client.
    /// 
    /// Author: @storm - X-Men's Weather Goddess
    /// "Like controlling the weather, we must control our test environment to ensure reliable results!"
    /// </summary>
    public static class MockSteamworksAPI
    {
        #region Mock State Variables
        
        private static bool isInitialized = false;
        private static Dictionary<string, bool> unlockedAchievements = new Dictionary<string, bool>();
        private static Dictionary<string, object> stats = new Dictionary<string, object>();
        private static Dictionary<string, string> richPresence = new Dictionary<string, string>();
        private static int storeStatsCallCount = 0;
        private static bool statsWereReset = false;
        private static bool statsReceived = false;

        #endregion

        #region Public API - Test Control Methods

        /// <summary>
        /// Reset all mock state - call this in test setup/teardown
        /// </summary>
        public static void Reset()
        {
            isInitialized = false;
            unlockedAchievements.Clear();
            stats.Clear();
            richPresence.Clear();
            storeStatsCallCount = 0;
            statsWereReset = false;
            statsReceived = false;
        }

        /// <summary>
        /// Set whether Steam API should report as initialized
        /// </summary>
        public static void SetInitialized(bool initialized)
        {
            isInitialized = initialized;
        }

        /// <summary>
        /// Simulate receiving stats from Steam servers
        /// </summary>
        public static void SimulateStatsReceived()
        {
            statsReceived = true;
            
            // Initialize default stats
            SetStatInternal("NumGames", 0);
            SetStatInternal("NumWins", 0);
            SetStatInternal("NumLosses", 0);
            SetStatInternal("FeetTraveled", 0f);
            SetStatInternal("MaxFeetTraveled", 0f);
            SetStatInternal("AverageSpeed", 0f);
        }

        #endregion

        #region Mock Steam API Implementation

        /// <summary>
        /// Mock SteamManager.Initialized property
        /// </summary>
        public static bool Initialized => isInitialized;

        /// <summary>
        /// Mock SteamUserStats.SetAchievement
        /// </summary>
        public static bool SetAchievement(string achievementId)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(achievementId))
                return false;

            unlockedAchievements[achievementId] = true;
            Debug.Log($"[MockSteam] Achievement unlocked: {achievementId}");
            return true;
        }

        /// <summary>
        /// Mock SteamUserStats.GetAchievement
        /// </summary>
        public static bool GetAchievement(string achievementId, out bool achieved)
        {
            achieved = false;
            if (!isInitialized || string.IsNullOrWhiteSpace(achievementId))
                return false;

            achieved = unlockedAchievements.ContainsKey(achievementId) && unlockedAchievements[achievementId];
            return true;
        }

        /// <summary>
        /// Mock SteamUserStats.StoreStats
        /// </summary>
        public static bool StoreStats()
        {
            if (!isInitialized) return false;
            
            storeStatsCallCount++;
            Debug.Log($"[MockSteam] Stats stored (call #{storeStatsCallCount})");
            return true;
        }

        /// <summary>
        /// Mock SteamUserStats.SetStat for integer values
        /// </summary>
        public static bool SetStat(string statName, int value)
        {
            return SetStatInternal(statName, value);
        }

        /// <summary>
        /// Mock SteamUserStats.SetStat for float values
        /// </summary>
        public static bool SetStat(string statName, float value)
        {
            return SetStatInternal(statName, value);
        }

        /// <summary>
        /// Mock SteamUserStats.GetStat for integer values
        /// </summary>
        public static bool GetStat(string statName, out int value)
        {
            value = 0;
            if (!isInitialized || !stats.ContainsKey(statName))
                return false;

            if (stats[statName] is int intValue)
            {
                value = intValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Mock SteamUserStats.GetStat for float values
        /// </summary>
        public static bool GetStat(string statName, out float value)
        {
            value = 0f;
            if (!isInitialized || !stats.ContainsKey(statName))
                return false;

            if (stats[statName] is float floatValue)
            {
                value = floatValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Mock SteamUserStats.UpdateAvgRateStat
        /// </summary>
        public static bool UpdateAvgRateStat(string statName, float countThisSession, double sessionLength)
        {
            if (!isInitialized) return false;
            
            // Simple average calculation for mock
            if (sessionLength > 0)
            {
                float averageRate = countThisSession / (float)sessionLength;
                SetStatInternal(statName, averageRate);
            }
            
            Debug.Log($"[MockSteam] Average rate updated: {statName} = {countThisSession}/{sessionLength}");
            return true;
        }

        /// <summary>
        /// Mock SteamUserStats.ResetAllStats
        /// </summary>
        public static bool ResetAllStats(bool achievementsToo)
        {
            if (!isInitialized) return false;
            
            stats.Clear();
            if (achievementsToo)
            {
                unlockedAchievements.Clear();
            }
            
            statsWereReset = true;
            Debug.Log($"[MockSteam] All stats reset (achievements too: {achievementsToo})");
            return true;
        }

        /// <summary>
        /// Mock SteamFriends.SetRichPresence
        /// </summary>
        public static bool SetRichPresence(string key, string value)
        {
            if (!isInitialized) return false;
            
            // Handle null or empty keys gracefully
            if (string.IsNullOrWhiteSpace(key))
            {
                Debug.LogWarning("[MockSteam] SetRichPresence called with null or empty key");
                return false;
            }
            
            richPresence[key] = value;
            Debug.Log($"[MockSteam] Rich presence set: {key} = {value}");
            return true;
        }

        /// <summary>
        /// Mock SteamFriends.GetFriendRichPresence (for testing purposes)
        /// </summary>
        public static string GetRichPresence(string key)
        {
            if (!isInitialized || !richPresence.ContainsKey(key))
                return "";
            
            return richPresence[key];
        }

        #endregion

        #region Test Verification Methods

        /// <summary>
        /// Check if a specific achievement was unlocked during the test
        /// </summary>
        public static bool WasAchievementSet(string achievementId)
        {
            return unlockedAchievements.ContainsKey(achievementId) && unlockedAchievements[achievementId];
        }

        /// <summary>
        /// Get total count of unlocked achievements
        /// </summary>
        public static int GetUnlockedAchievementCount()
        {
            return unlockedAchievements.Count(kvp => kvp.Value);
        }

        /// <summary>
        /// Check if stats were stored during the test
        /// </summary>
        public static bool WereStatsStored()
        {
            return storeStatsCallCount > 0;
        }

        /// <summary>
        /// Get number of times StoreStats was called
        /// </summary>
        public static int GetStoreStatsCallCount()
        {
            return storeStatsCallCount;
        }

        /// <summary>
        /// Check if a specific stat was set during the test
        /// </summary>
        public static bool WasStatSet(string statName)
        {
            return stats.ContainsKey(statName);
        }

        /// <summary>
        /// Get the current value of a stat (for detailed assertions)
        /// </summary>
        public static object GetStatValue(string statName)
        {
            return stats.ContainsKey(statName) ? stats[statName] : null;
        }

        /// <summary>
        /// Check if stats were reset during the test
        /// </summary>
        public static bool WereStatsReset()
        {
            return statsWereReset;
        }

        /// <summary>
        /// Get all currently set rich presence values
        /// </summary>
        public static Dictionary<string, string> GetAllRichPresence()
        {
            return new Dictionary<string, string>(richPresence);
        }

        /// <summary>
        /// Get list of all unlocked achievement IDs
        /// </summary>
        public static List<string> GetUnlockedAchievements()
        {
            return unlockedAchievements.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
        }

        #endregion

        #region Private Helper Methods

        private static bool SetStatInternal(string statName, object value)
        {
            if (!isInitialized || string.IsNullOrWhiteSpace(statName))
                return false;

            stats[statName] = value;
            Debug.Log($"[MockSteam] Stat set: {statName} = {value}");
            return true;
        }

        #endregion

        #region Achievement Enum Extensions (for testing convenience)

        /// <summary>
        /// Unlock an achievement using the enum directly
        /// </summary>
        public static bool UnlockAchievement(Achievements achievement)
        {
            return SetAchievement(achievement.ToString());
        }

        /// <summary>
        /// Check if an achievement enum value was unlocked
        /// </summary>
        public static bool IsAchievementUnlocked(Achievements achievement)
        {
            return WasAchievementSet(achievement.ToString());
        }

        /// <summary>
        /// Simulate unlocking all Seven Sins achievements for testing
        /// </summary>
        public static void UnlockAllSevenSins()
        {
            var sins = new[]
            {
                Achievements.WRATH_SIN_DEFEATED,
                Achievements.ENVY_SIN_DEFEATED,
                Achievements.GREED_SIN_DEFEATED,
                Achievements.LUST_SIN_DEFEATED,
                Achievements.GLUTTONY_SIN_DEFEATED,
                Achievements.SLOTH_SIN_DEFEATED,
                Achievements.PRIDE_SIN_DEFEATED
            };

            foreach (var sin in sins)
            {
                SetAchievement(sin.ToString());
            }
        }

        /// <summary>
        /// Check if all Seven Sins achievements are unlocked
        /// </summary>
        public static bool AreAllSevenSinsDefeated()
        {
            var sins = new[]
            {
                Achievements.WRATH_SIN_DEFEATED,
                Achievements.ENVY_SIN_DEFEATED,
                Achievements.GREED_SIN_DEFEATED,
                Achievements.LUST_SIN_DEFEATED,
                Achievements.GLUTTONY_SIN_DEFEATED,
                Achievements.SLOTH_SIN_DEFEATED,
                Achievements.PRIDE_SIN_DEFEATED
            };

            return sins.All(sin => WasAchievementSet(sin.ToString()));
        }

        #endregion
    }

    /// <summary>
    /// Mock Steam callback structures for testing
    /// </summary>
    public static class MockSteamCallbacks
    {
        public struct MockUserStatsReceived
        {
            public uint m_nGameID;
            public int m_eResult; // EResult equivalent
        }

        public struct MockUserStatsStored
        {
            public uint m_nGameID;
            public int m_eResult; // EResult equivalent
        }

        public struct MockUserAchievementStored
        {
            public uint m_nGameID;
            public string m_rgchAchievementName;
            public uint m_nCurProgress;
            public uint m_nMaxProgress;
        }
    }
}