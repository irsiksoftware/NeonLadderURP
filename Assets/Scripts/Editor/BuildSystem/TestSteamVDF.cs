using UnityEngine;
using UnityEditor;
using System.Linq;
using NeonLadder.BuildSystem;

namespace NeonLadder.BuildSystem
{
    public static class TestSteamVDF
    {
        [MenuItem("NeonLadder/Build/Test Steam VDF Generation")]
        public static void TestGeneration()
        {
            var manager = BuildProfileManager.Instance;
            var steamProfile = manager.profiles.FirstOrDefault(p => p.profileName == "Steam Demo");

            if (steamProfile == null)
            {
                Debug.LogError("Steam Demo profile not found!");
                return;
            }

            Debug.Log("Testing Steam VDF generation...");
            bool success = SteamVDFGenerator.GenerateVDFFiles(steamProfile);

            if (success)
            {
                Debug.Log("✅ Steam VDF generation test completed successfully!");
            }
            else
            {
                Debug.LogError("❌ Steam VDF generation test failed!");
            }
        }
    }
}