using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Monitors build post-processing times to identify bottlenecks
    /// </summary>
    public class BuildPostProcessMonitor : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        private static readonly Dictionary<string, Stopwatch> processTimers = new Dictionary<string, Stopwatch>();
        private static Stopwatch totalBuildTimer;

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            // Hook into various Unity events to monitor processing times
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            // This runs constantly, so we only want to log during builds
            if (BuildPipeline.isBuildingPlayer)
            {
                if (totalBuildTimer == null)
                {
                    totalBuildTimer = Stopwatch.StartNew();
                    UnityEngine.Debug.Log("[BuildMonitor] Build started - monitoring post-processing times...");
                }
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (totalBuildTimer != null)
            {
                totalBuildTimer.Stop();

                UnityEngine.Debug.Log("=== BUILD POST-PROCESSING ANALYSIS ===");
                UnityEngine.Debug.Log($"Total Build Time: {totalBuildTimer.Elapsed.TotalSeconds:F2} seconds");
                UnityEngine.Debug.Log($"Build Result: {report.summary.result}");
                UnityEngine.Debug.Log($"Build Size: {report.summary.totalSize / 1024 / 1024:F2} MB");

                // Analyze build steps
                AnalyzeBuildSteps(report);

                // Analyze assets
                AnalyzeAssetProcessing(report);

                // Reset for next build
                totalBuildTimer = null;
                processTimers.Clear();
            }
        }

        private void AnalyzeBuildSteps(BuildReport report)
        {
            UnityEngine.Debug.Log("\n=== BUILD STEPS ANALYSIS ===");

            var steps = report.steps;
            for (int i = 0; i < steps.Length; i++)
            {
                var step = steps[i];
                UnityEngine.Debug.Log($"Step {i + 1}: {step.name} - {step.duration.TotalSeconds:F2}s");

                // Log slow steps
                if (step.duration.TotalSeconds > 5)
                {
                    UnityEngine.Debug.LogWarning($"⚠️ SLOW STEP: {step.name} took {step.duration.TotalSeconds:F2} seconds!");
                }
            }
        }

        private void AnalyzeAssetProcessing(BuildReport report)
        {
            UnityEngine.Debug.Log("\n=== ASSET PROCESSING ANALYSIS ===");

            var assets = report.GetFiles();
            var largeAssets = new List<BuildFile>();
            long totalSize = 0;

            foreach (var asset in assets)
            {
                totalSize += (long)asset.size;

                // Flag assets larger than 1MB
                if (asset.size > 1024 * 1024)
                {
                    largeAssets.Add(asset);
                }
            }

            UnityEngine.Debug.Log($"Total Assets: {assets.Length}");
            UnityEngine.Debug.Log($"Total Asset Size: {totalSize / 1024 / 1024:F2} MB");

            // Show largest assets
            largeAssets.Sort((a, b) => b.size.CompareTo(a.size));
            UnityEngine.Debug.Log($"\n=== LARGEST ASSETS (>{1}MB) ===");

            for (int i = 0; i < Mathf.Min(10, largeAssets.Count); i++)
            {
                var asset = largeAssets[i];
                UnityEngine.Debug.Log($"{i + 1}. {asset.path} - {asset.size / 1024 / 1024:F2} MB");
            }
        }

        /// <summary>
        /// Manual method to check current post-processors
        /// </summary>
        [MenuItem("NeonLadder/Build/Debug/List Active Post-Processors")]
        public static void ListActivePostProcessors()
        {
            UnityEngine.Debug.Log("=== ACTIVE BUILD POST-PROCESSORS ===");

            // Get all IPreprocessBuildWithReport implementations
            var preprocessors = GetBuildCallbacks<IPreprocessBuildWithReport>();
            UnityEngine.Debug.Log($"\nPre-processors ({preprocessors.Count}):");
            foreach (var processor in preprocessors)
            {
                UnityEngine.Debug.Log($"  • {processor.GetType().Name} (Order: {processor.callbackOrder})");
            }

            // Get all IPostprocessBuildWithReport implementations
            var postprocessors = GetBuildCallbacks<IPostprocessBuildWithReport>();
            UnityEngine.Debug.Log($"\nPost-processors ({postprocessors.Count}):");
            foreach (var processor in postprocessors)
            {
                UnityEngine.Debug.Log($"  • {processor.GetType().Name} (Order: {processor.callbackOrder})");
            }
        }

        private static List<T> GetBuildCallbacks<T>() where T : class, IOrderedCallback
        {
            var callbacks = new List<T>();
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (typeof(T).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                        {
                            try
                            {
                                var instance = System.Activator.CreateInstance(type) as T;
                                if (instance != null)
                                {
                                    callbacks.Add(instance);
                                }
                            }
                            catch
                            {
                                // Skip types that can't be instantiated
                            }
                        }
                    }
                }
                catch
                {
                    // Skip assemblies we can't access
                }
            }

            callbacks.Sort((a, b) => a.callbackOrder.CompareTo(b.callbackOrder));
            return callbacks;
        }

        /// <summary>
        /// Check for common build slowdown causes
        /// </summary>
        [MenuItem("NeonLadder/Build/Debug/Diagnose Build Slowdowns")]
        public static void DiagnoseBuildSlowdowns()
        {
            UnityEngine.Debug.Log("=== BUILD SLOWDOWN DIAGNOSIS ===");

            // Check scene count
            var sceneCount = EditorBuildSettings.scenes.Length;
            var enabledScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Count();
            UnityEngine.Debug.Log($"Scenes in build: {enabledScenes}/{sceneCount}");

            if (enabledScenes > 20)
            {
                UnityEngine.Debug.LogWarning($"⚠️ HIGH SCENE COUNT: {enabledScenes} scenes enabled. Consider reducing for development builds.");
            }

            // Check for large textures
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D");
            var largeTextures = 0;

            foreach (var guid in textureGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (importer != null)
                {
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (texture != null && (texture.width > 2048 || texture.height > 2048))
                    {
                        largeTextures++;
                        if (largeTextures <= 5) // Only log first 5
                        {
                            UnityEngine.Debug.Log($"Large texture: {path} ({texture.width}x{texture.height})");
                        }
                    }
                }
            }

            if (largeTextures > 0)
            {
                UnityEngine.Debug.LogWarning($"⚠️ LARGE TEXTURES: Found {largeTextures} textures larger than 2048px. These slow down builds.");
            }

            // Check scripting backend
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            UnityEngine.Debug.Log($"Scripting Backend: {backend}");

            if (backend == ScriptingImplementation.IL2CPP)
            {
                UnityEngine.Debug.LogWarning("⚠️ IL2CPP DETECTED: Switch to Mono for faster development builds!");
            }

            // Check compression method
            UnityEngine.Debug.Log("💡 TIP: For fastest development builds, use LZ4 compression in Build Settings");
        }

        /// <summary>
        /// Enable verbose build logging
        /// </summary>
        [MenuItem("NeonLadder/Build/Debug/Enable Verbose Build Logging")]
        public static void EnableVerboseBuildLogging()
        {
            // Set Unity to show detailed build logs
            EditorPrefs.SetBool("VerboseBuildOutput", true);
            UnityEngine.Debug.Log("✅ Verbose build logging enabled. Check Console during next build for detailed output.");
        }

        [MenuItem("NeonLadder/Build/Debug/Disable Verbose Build Logging")]
        public static void DisableVerboseBuildLogging()
        {
            EditorPrefs.SetBool("VerboseBuildOutput", false);
            UnityEngine.Debug.Log("Verbose build logging disabled.");
        }
    }
}