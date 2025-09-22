using UnityEditor;
using UnityEngine;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Quick scene management for development builds
    /// </summary>
    public static class DevSceneProfiler
    {
        [MenuItem("NeonLadder/Build/Quick Scene Sets/Essential Only (5 scenes)", priority = 1)]
        public static void SetEssentialScenesOnly()
        {
            string[] essentialScenes = {
                "Assets/Scenes/Title.unity",
                "Assets/Scenes/Staging.unity",
                "Assets/Scenes/Generated/BossArenas/Cathedral.unity",
                "Assets/Scenes/Generated/BossArenas/Finale.unity",
                "Assets/Scenes/Credits.unity"
            };

            SetSpecificScenes(essentialScenes, "Essential scenes set (5 total) - ~1 minute builds");
        }

        [MenuItem("NeonLadder/Build/Quick Scene Sets/Core Gameplay (10 scenes)", priority = 2)]
        public static void SetCoreGameplayScenes()
        {
            string[] coreScenes = {
                "Assets/Scenes/Title.unity",
                "Assets/Scenes/Staging.unity",
                "Assets/Scenes/PermaShop.unity",
                "Assets/Scenes/MetaShop.unity",
                "Assets/Scenes/Generated/BossArenas/Cathedral.unity",
                "Assets/Scenes/Generated/BossArenas/Garden.unity",
                "Assets/Scenes/Generated/BossArenas/Finale.unity",
                "Assets/Scenes/Cutscenes/Death.unity",
                "Assets/Scenes/Cutscenes/BossDefeated.unity",
                "Assets/Scenes/Credits.unity"
            };

            SetSpecificScenes(coreScenes, "Core gameplay scenes set (10 total) - ~2 minute builds");
        }

        [MenuItem("NeonLadder/Build/Quick Scene Sets/Testing Scenes (Test + 3 bosses)", priority = 3)]
        public static void SetTestingScenes()
        {
            string[] testScenes = {
                "Assets/Scenes/Title.unity",
                "Assets/Scenes/Test/BossBrawl.unity",
                "Assets/Scenes/Generated/BossArenas/Cathedral.unity",
                "Assets/Scenes/Generated/BossArenas/Garden.unity",
                "Assets/Scenes/Generated/BossArenas/Finale.unity"
            };

            SetSpecificScenes(testScenes, "Testing scenes set (5 total) - for quick iteration");
        }

        [MenuItem("NeonLadder/Build/Quick Scene Sets/Restore All 57 Scenes", priority = 10)]
        public static void RestoreAllScenes()
        {
            // Re-enable all scenes that exist
            var allScenes = EditorBuildSettings.scenes;
            for (int i = 0; i < allScenes.Length; i++)
            {
                allScenes[i].enabled = true;
            }
            EditorBuildSettings.scenes = allScenes;

            Debug.Log($"✅ Restored all {allScenes.Length} scenes - builds will take ~10 minutes");
            EditorUtility.DisplayDialog("All Scenes Restored",
                $"All {allScenes.Length} scenes restored to build settings.\\n\\nBuild time: ~10 minutes", "OK");
        }

        [MenuItem("NeonLadder/Build/Scene Info/Show Current Scene Count", priority = 20)]
        public static void ShowCurrentSceneCount()
        {
            var scenes = EditorBuildSettings.scenes;
            int enabledCount = 0;
            int totalCount = scenes.Length;

            foreach (var scene in scenes)
            {
                if (scene.enabled) enabledCount++;
            }

            float estimatedBuildTime = enabledCount * 0.175f; // ~10.5 seconds per scene

            Debug.Log($"=== BUILD SETTINGS SCENE COUNT ===");
            Debug.Log($"Enabled scenes: {enabledCount}/{totalCount}");
            Debug.Log($"Estimated build time: {estimatedBuildTime:F1} minutes");

            EditorUtility.DisplayDialog("Scene Count Info",
                $"Enabled: {enabledCount}/{totalCount} scenes\\n\\nEstimated build time: {estimatedBuildTime:F1} minutes", "OK");
        }

        private static void SetSpecificScenes(string[] scenePaths, string message)
        {
            var allScenes = EditorBuildSettings.scenes;

            // Disable all scenes first
            for (int i = 0; i < allScenes.Length; i++)
            {
                allScenes[i].enabled = false;
            }

            // Enable only specified scenes that exist
            int enabledCount = 0;
            foreach (string scenePath in scenePaths)
            {
                for (int i = 0; i < allScenes.Length; i++)
                {
                    if (allScenes[i].path == scenePath)
                    {
                        allScenes[i].enabled = true;
                        enabledCount++;
                        break;
                    }
                }
            }

            EditorBuildSettings.scenes = allScenes;

            Debug.Log($"✅ {message}");
            EditorUtility.DisplayDialog("Scene Set Applied",
                $"{message}\\n\\nEnabled {enabledCount} scenes", "OK");
        }
    }
}