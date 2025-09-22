using UnityEngine;
using UnityEditor;
using System.Linq;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Simple shader analysis tools for build optimization
    /// </summary>
    public static class SimpleShaderTools
    {
        [MenuItem("NeonLadder/Build/Shader Analysis/Count All Shaders")]
        public static void CountAllShaders()
        {
            var shaderGuids = AssetDatabase.FindAssets("t:Shader");
            Debug.Log($"=== SHADER COUNT ANALYSIS ===");
            Debug.Log($"Total shaders in project: {shaderGuids.Length}");

            var customShaders = 0;
            var packageShaders = 0;

            foreach (var guid in shaderGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("Assets/Packages/") || path.Contains("Assets\\Packages\\"))
                {
                    customShaders++;
                }
                else if (path.Contains("Packages/") || path.Contains("Library/"))
                {
                    packageShaders++;
                }
                else
                {
                    customShaders++;
                }
            }

            Debug.Log($"Custom/Asset shaders: {customShaders}");
            Debug.Log($"Built-in/Package shaders: {packageShaders}");

            if (customShaders > 50)
            {
                Debug.LogWarning($"⚠️ HIGH SHADER COUNT: {customShaders} custom shaders!");
                Debug.LogWarning("💡 This significantly slows down builds. Consider shader optimization.");
            }
        }

        [MenuItem("NeonLadder/Build/Shader Analysis/Find ARTnGAME Shaders")]
        public static void FindARTnGAMEShaders()
        {
            var shaderGuids = AssetDatabase.FindAssets("t:Shader");
            var artnGameShaders = 0;

            Debug.Log("=== ARTnGAME SHADER SEARCH ===");

            foreach (var guid in shaderGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

                if (shader != null &&
                    (path.Contains("ARTnGAME") ||
                     shader.name.Contains("ARTnGAME") ||
                     shader.name.Contains("PLANET") ||
                     shader.name.Contains("Orion")))
                {
                    artnGameShaders++;
                    if (artnGameShaders <= 10) // Only log first 10
                    {
                        Debug.Log($"  • {shader.name} ({path})");
                    }
                }
            }

            Debug.Log($"\nFound {artnGameShaders} ARTnGAME-related shaders");

            if (artnGameShaders > 50)
            {
                Debug.LogWarning("⚠️ ARTnGAME packages contain many shaders that slow builds!");
                Debug.LogWarning("💡 Most of these are likely unused in your 2D platformer");
            }
        }

        [MenuItem("NeonLadder/Build/Quick Optimizations/Enable Graphics Jobs")]
        public static void EnableGraphicsJobs()
        {
            PlayerSettings.graphicsJobs = true;
            Debug.Log("✅ Graphics Jobs enabled - may improve build performance");
        }

        [MenuItem("NeonLadder/Build/Quick Optimizations/Disable Graphics Jobs")]
        public static void DisableGraphicsJobs()
        {
            PlayerSettings.graphicsJobs = false;
            Debug.Log("✅ Graphics Jobs disabled");
        }

        [MenuItem("NeonLadder/Build/Quick Optimizations/Set Fastest Quality")]
        public static void SetFastestQuality()
        {
            QualitySettings.SetQualityLevel(0, true); // Lowest quality for fastest builds
            Debug.Log("✅ Set to fastest quality level for development");
        }

        [MenuItem("NeonLadder/Build/Build Information/Show Current Build Settings")]
        public static void ShowCurrentBuildSettings()
        {
            Debug.Log("=== CURRENT BUILD SETTINGS ===");
            Debug.Log($"Scripting Backend: {PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone)}");
            Debug.Log($"Graphics Jobs: {PlayerSettings.graphicsJobs}");
            Debug.Log($"Quality Level: {QualitySettings.GetQualityLevel()}");
            Debug.Log($"Color Space: {PlayerSettings.colorSpace}");

            var scenes = EditorBuildSettings.scenes;
            var enabledScenes = scenes.Count(s => s.enabled);
            Debug.Log($"Enabled Scenes: {enabledScenes}/{scenes.Length}");

            var shaderCount = AssetDatabase.FindAssets("t:Shader").Length;
            Debug.Log($"Total Shaders: {shaderCount}");

            // Estimate build time
            float estimatedTime = (enabledScenes * 0.175f) + (shaderCount * 0.01f);
            Debug.Log($"Estimated Build Time: {estimatedTime:F1} minutes");
        }
    }
}