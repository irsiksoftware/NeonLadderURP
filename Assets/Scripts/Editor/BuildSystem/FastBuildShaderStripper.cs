using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Strips unused shaders for ultra-fast development builds
    /// </summary>
    public class FastBuildShaderStripper : IPreprocessShaders
    {
        public int callbackOrder => 0;

        // Flag to enable/disable aggressive shader stripping
        private static bool enableAggressiveStripping = false;

        // Essential shaders that should never be stripped
        private static readonly HashSet<string> essentialShaders = new HashSet<string>
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/Simple Lit",
            "Sprites/Default",
            "UI/Default",
            "UI/DefaultETC1",
            "Hidden/Internal-Colored",
            "Hidden/BlitCopy",
            "Legacy Shaders/Diffuse"
        };

        [MenuItem("NeonLadder/Build/Shader Optimization/Enable Fast Build Shader Stripping")]
        public static void EnableFastShaderStripping()
        {
            enableAggressiveStripping = true;
            EditorPrefs.SetBool("FastBuildShaderStripping", true);
            Debug.Log("✅ Fast build shader stripping ENABLED - development builds will be much faster!");
            Debug.Log("💡 This strips most custom shaders. Use 'Disable' for final builds.");
        }

        [MenuItem("NeonLadder/Build/Shader Optimization/Disable Fast Build Shader Stripping")]
        public static void DisableFastShaderStripping()
        {
            enableAggressiveStripping = false;
            EditorPrefs.SetBool("FastBuildShaderStripping", false);
            Debug.Log("✅ Fast build shader stripping DISABLED - all shaders will be included");
        }

        [MenuItem("NeonLadder/Build/Shader Optimization/List All Project Shaders")]
        public static void ListAllShaders()
        {
            var shaderGuids = AssetDatabase.FindAssets("t:Shader");
            var builtinShaders = new List<string>();
            var customShaders = new List<string>();

            Debug.Log("=== PROJECT SHADER ANALYSIS ===");

            foreach (var guid in shaderGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);

                if (shader != null)
                {
                    if (path.Contains("com.unity.") || path.Contains("Packages/"))
                    {
                        builtinShaders.Add(shader.name);
                    }
                    else
                    {
                        customShaders.Add(shader.name);
                    }
                }
            }

            Debug.Log($"Built-in/Package Shaders: {builtinShaders.Count}");
            Debug.Log($"Custom Project Shaders: {customShaders.Count}");
            Debug.Log($"Total Shaders: {builtinShaders.Count + customShaders.Count}");

            Debug.Log("\n=== LARGEST CUSTOM SHADER CATEGORIES ===");
            var categories = customShaders
                .GroupBy(s => s.Split('/')[0])
                .OrderByDescending(g => g.Count())
                .Take(10);

            foreach (var category in categories)
            {
                Debug.Log($"{category.Key}: {category.Count()} shaders");
            }

            if (customShaders.Count > 50)
            {
                Debug.LogWarning($"⚠️ HIGH SHADER COUNT: {customShaders.Count} custom shaders found!");
                Debug.LogWarning("💡 Consider enabling Fast Build Shader Stripping for development");
            }
        }

        [MenuItem("NeonLadder/Build/Shader Optimization/Analyze Title Scene Shaders")]
        public static void AnalyzeTitleSceneShaders()
        {
            var titleScenePath = "Assets/Scenes/Title.unity";
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;

            if (currentScene != titleScenePath)
            {
                Debug.LogWarning($"⚠️ Please open the Title scene first: {titleScenePath}");
                Debug.LogWarning($"Currently open: {currentScene}");
                return;
            }

            var renderers = Object.FindObjectsOfType<Renderer>();
            var usedShaders = new HashSet<string>();

            foreach (var renderer in renderers)
            {
                if (renderer.sharedMaterials != null)
                {
                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material != null && material.shader != null)
                        {
                            usedShaders.Add(material.shader.name);
                        }
                    }
                }
            }

            Debug.Log($"=== TITLE SCENE SHADER ANALYSIS ===");
            Debug.Log($"Renderers found: {renderers.Length}");
            Debug.Log($"Unique shaders used: {usedShaders.Count}");

            Debug.Log("\n=== SHADERS ACTUALLY USED IN TITLE SCENE ===");
            foreach (var shader in usedShaders.OrderBy(s => s))
            {
                Debug.Log($"  • {shader}");
            }

            var totalProjectShaders = AssetDatabase.FindAssets("t:Shader").Length;
            var wastedShaders = totalProjectShaders - usedShaders.Count;

            Debug.Log($"\n=== WASTE ANALYSIS ===");
            Debug.Log($"Total project shaders: {totalProjectShaders}");
            Debug.Log($"Used in Title scene: {usedShaders.Count}");
            Debug.Log($"Potentially unused: {wastedShaders} ({(float)wastedShaders / totalProjectShaders * 100:F1}%)");

            if (wastedShaders > 100)
            {
                Debug.LogWarning("💡 RECOMMENDATION: Enable Fast Build Shader Stripping for development builds!");
            }
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData)
        {
            // Check if fast stripping is enabled
            if (!EditorPrefs.GetBool("FastBuildShaderStripping", false))
                return;

            // Never strip essential shaders
            if (essentialShaders.Contains(shader.name))
                return;

            // Strip ARTnGAME shaders (major contributor to build time)
            if (shader.name.Contains("ARTnGAME") ||
                shader.name.Contains("PLANET") ||
                shader.name.Contains("Orion") ||
                shader.name.Contains("Aurora") ||
                shader.name.Contains("Erosion"))
            {
                shaderCompilerData.Clear();
                return;
            }

            // Note: Compute shader stripping not available in this Unity version

            // Strip complex post-processing shaders
            if (shader.name.Contains("Hidden/Post") ||
                shader.name.Contains("SSPR") ||
                shader.name.Contains("VolumetricFog") ||
                shader.name.Contains("DistortionFX"))
            {
                shaderCompilerData.Clear();
                return;
            }

            // Keep only the most basic variants for development
            if (shaderCompilerData.Count > 10)
            {
                // Keep only the first few variants
                var keepers = shaderCompilerData.Take(5).ToList();
                shaderCompilerData.Clear();
                foreach (var keeper in keepers)
                {
                    shaderCompilerData.Add(keeper);
                }
            }
        }

        /// <summary>
        /// Build preprocessor to log shader compilation
        /// </summary>
        [MenuItem("NeonLadder/Build/Shader Optimization/Next Build Will Log Shader Compilation")]
        public static void EnableShaderLogging()
        {
            EditorPrefs.SetBool("LogShaderCompilation", true);
            Debug.Log("✅ Next build will log shader compilation details");
        }
    }

    /// <summary>
    /// Logs shader compilation during builds
    /// </summary>
    public class ShaderCompilationLogger : IPreprocessShaders
    {
        public int callbackOrder => 100; // Run after stripper

        private static int totalShaders = 0;
        private static int strippedShaders = 0;

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> shaderCompilerData)
        {
            if (!EditorPrefs.GetBool("LogShaderCompilation", false))
                return;

            totalShaders++;

            if (shaderCompilerData.Count == 0)
            {
                strippedShaders++;
                Debug.Log($"[STRIPPED] {shader.name} ({snippet.shaderType})");
            }
            else if (shaderCompilerData.Count > 5)
            {
                Debug.Log($"[HEAVY] {shader.name}: {shaderCompilerData.Count} variants ({snippet.shaderType})");
            }

            // Log summary every 50 shaders
            if (totalShaders % 50 == 0)
            {
                Debug.Log($"[SHADER PROGRESS] Processed {totalShaders} shaders, stripped {strippedShaders}");
            }
        }
    }
}