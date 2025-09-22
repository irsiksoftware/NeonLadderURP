using UnityEditor;
using UnityEngine;
using System.Linq;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Ultra-fast development build settings for rapid iteration
    /// </summary>
    public static class FastDevelopmentBuilder
    {
        [MenuItem("NeonLadder/Build/Ultra Fast Development Build", priority = 0)]
        public static void BuildUltraFast()
        {
            Debug.Log("[FastDevelopmentBuilder] Starting ultra-fast development build...");

            // Fastest possible settings
            var buildOptions = BuildOptions.Development;

            // Only build current scene or minimal scenes
            string[] scenesToBuild;
            if (EditorBuildSettings.scenes.Length > 0)
            {
                // Just use the first scene (typically TitleScreen)
                scenesToBuild = new[] { EditorBuildSettings.scenes[0].path };
            }
            else
            {
                Debug.LogError("No scenes in build settings!");
                return;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenesToBuild,
                locationPathName = "Builds/UltraFast/NeonLadder.exe",
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions
            };

            // Record start time
            var startTime = System.DateTime.Now;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            var buildTime = System.DateTime.Now - startTime;

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[FastDevelopmentBuilder] Ultra-fast build completed in {buildTime.TotalSeconds:F1} seconds!");
                Debug.Log($"[FastDevelopmentBuilder] Build size: {report.summary.totalSize / 1024 / 1024:F1} MB");

                // Auto-open build folder
                EditorUtility.RevealInFinder("Builds/UltraFast/NeonLadder.exe");
            }
            else
            {
                Debug.LogError($"[FastDevelopmentBuilder] Build failed: {report.summary.result}");
            }
        }

        [MenuItem("NeonLadder/Build/Fast Multi-Scene Build", priority = 1)]
        public static void BuildFastMultiScene()
        {
            Debug.Log("[FastDevelopmentBuilder] Starting fast multi-scene build...");

            var buildOptions = BuildOptions.Development;

            // Use only essential scenes
            var essentialScenes = new[]
            {
                "Assets/Scenes/TitleScreen.unity",
                "Assets/Scenes/MainCityHub.unity"
            };

            // Filter to only scenes that exist
            var scenesToBuild = essentialScenes
                .Where(scene => System.IO.File.Exists(scene))
                .ToArray();

            if (scenesToBuild.Length == 0)
            {
                Debug.LogError("No essential scenes found! Using all build settings scenes.");
                scenesToBuild = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenesToBuild,
                locationPathName = "Builds/FastMulti/NeonLadder.exe",
                target = BuildTarget.StandaloneWindows64,
                options = buildOptions
            };

            var startTime = System.DateTime.Now;
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var buildTime = System.DateTime.Now - startTime;

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[FastDevelopmentBuilder] Fast multi-scene build completed in {buildTime.TotalSeconds:F1} seconds!");
                Debug.Log($"[FastDevelopmentBuilder] Scenes included: {scenesToBuild.Length}");
                Debug.Log($"[FastDevelopmentBuilder] Build size: {report.summary.totalSize / 1024 / 1024:F1} MB");

                EditorUtility.RevealInFinder("Builds/FastMulti/NeonLadder.exe");
            }
            else
            {
                Debug.LogError($"[FastDevelopmentBuilder] Build failed: {report.summary.result}");
            }
        }
    }
}