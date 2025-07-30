using UnityEngine;

namespace NeonLadder.Testing.Mocks
{
    /// <summary>
    /// Mock Steam Manager for testing purposes.
    /// Provides testable Steam functionality without requiring actual Steam client or Steamworks.NET.
    /// 
    /// Author: @storm - Lightning Conductor
    /// "Like channeling the power of lightning, we must have precise control over our test environment!"
    /// </summary>
    public class MockSteamManager : MonoBehaviour
    {
        public static bool IsInitialized => MockSteamworksAPI.Initialized;

        /// <summary>
        /// Mock implementation of UnlockAchievement functionality
        /// </summary>
        public void UnlockAchievement(string achievementID)
        {
            if (!IsInitialized)
            {
                Debug.LogError("SteamManager not initialized.");
                return;
            }

            MockSteamworksAPI.SetAchievement(achievementID);
            MockSteamworksAPI.StoreStats();
            Debug.Log($"Achievement {achievementID} unlocked.");
        }

        /// <summary>
        /// Check if an achievement is unlocked
        /// </summary>
        public bool IsAchievementUnlocked(string achievementID)
        {
            return MockSteamworksAPI.WasAchievementSet(achievementID);
        }

        /// <summary>
        /// Mock Steam initialization
        /// </summary>
        public void Initialize()
        {
            MockSteamworksAPI.SetInitialized(true);
            MockSteamworksAPI.SimulateStatsReceived();
        }

        /// <summary>
        /// Mock Steam shutdown
        /// </summary>
        public void Shutdown()
        {
            MockSteamworksAPI.SetInitialized(false);
        }
    }
}