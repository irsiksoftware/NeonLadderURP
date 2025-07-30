using UnityEngine;

namespace NeonLadder.Testing.Mocks
{
    /// <summary>
    /// Mock Stats and Achievements component for testing.
    /// Simulates game progression tracking without requiring actual Steam integration.
    /// 
    /// Author: @storm - Weather Control Master
    /// "Track every raindrop, every lightning bolt - precision in all things!"
    /// </summary>
    public class MockStatsAndAchievements : MonoBehaviour
    {
        private float gameFeetTraveled;
        private int totalGamesPlayed;
        private int totalNumWins;
        private int totalNumLosses;
        private float totalFeetTraveled;
        private float maxFeetTraveled;

        /// <summary>
        /// Simulate adding distance traveled during gameplay
        /// </summary>
        public void AddDistanceTraveled(float distance)
        {
            gameFeetTraveled += distance;
        }

        /// <summary>
        /// Handle game state changes and update stats accordingly
        /// </summary>
        public void OnGameStateChange(MockGameStates newState)
        {
            if (!MockSteamworksAPI.Initialized) return;

            if (newState == MockGameStates.Active)
            {
                gameFeetTraveled = 0;
            }
            else if (newState == MockGameStates.Winner || newState == MockGameStates.Loser)
            {
                if (newState == MockGameStates.Winner)
                {
                    totalNumWins++;
                    MockSteamworksAPI.SetStat("NumWins", totalNumWins);
                }
                else
                {
                    totalNumLosses++;
                    MockSteamworksAPI.SetStat("NumLosses", totalNumLosses);
                }

                totalGamesPlayed++;
                totalFeetTraveled += gameFeetTraveled;

                if (gameFeetTraveled > maxFeetTraveled)
                    maxFeetTraveled = gameFeetTraveled;

                // Update Steam stats
                MockSteamworksAPI.SetStat("NumGames", totalGamesPlayed);
                MockSteamworksAPI.SetStat("FeetTraveled", totalFeetTraveled);
                MockSteamworksAPI.SetStat("MaxFeetTraveled", maxFeetTraveled);
                MockSteamworksAPI.StoreStats();
            }
        }

        /// <summary>
        /// Get current game distance for testing
        /// </summary>
        public float GetGameFeetTraveled()
        {
            return gameFeetTraveled;
        }

        /// <summary>
        /// Get total wins for testing
        /// </summary>
        public int GetTotalWins()
        {
            return totalNumWins;
        }

        /// <summary>
        /// Get total losses for testing
        /// </summary>
        public int GetTotalLosses()
        {
            return totalNumLosses;
        }
    }
}