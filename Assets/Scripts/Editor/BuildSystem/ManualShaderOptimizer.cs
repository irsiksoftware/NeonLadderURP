using UnityEngine;
using UnityEditor;

namespace NeonLadder.BuildSystem
{
    /// <summary>
    /// Manual shader optimization for faster builds
    /// </summary>
    public static class ManualShaderOptimizer
    {
        [MenuItem("NeonLadder/Build/Manual Optimizations/Create Fast Build Settings Asset")]
        public static void CreateShaderStrippingAsset()
        {
            // Create a folder for build optimization assets
            if (!AssetDatabase.IsValidFolder("Assets/BuildOptimization"))
            {
                AssetDatabase.CreateFolder("Assets", "BuildOptimization");
            }

            // Instructions for manual shader optimization
            string instructions = @"SHADER OPTIMIZATION INSTRUCTIONS:

1. MANUAL APPROACH (Recommended):
   - Go to Project Settings → Graphics
   - At bottom, find 'Shader Stripping'
   - Enable 'Strip Unused' and configure variants

2. QUALITY SETTINGS:
   - Go to Project Settings → Quality
   - Create a 'Development' quality level with minimal settings

3. SHADER VARIANT COLLECTION:
   - Window → Rendering → Shader Variant Collection
   - Create new collection, add only shaders you use

4. GRAPHICS SETTINGS:
   - Project Settings → Graphics → Built-in Shader Settings
   - Disable shaders you don't use (like terrain, speedtree, etc.)

Your project has ~200 shaders. For a Title scene, you probably need <10.

FASTEST APPROACH:
1. Run 'NeonLadder → Build → Shader Analysis → Find ARTnGAME Shaders'
2. Consider removing ARTnGAME packages you don't use
3. Use 'Ultra Fast Development Build' with 1 scene
";

            System.IO.File.WriteAllText("Assets/BuildOptimization/ShaderOptimizationInstructions.txt", instructions);
            AssetDatabase.Refresh();

            Debug.Log("✅ Created shader optimization instructions in Assets/BuildOptimization/");
            Debug.Log("💡 Check the instructions file for manual optimization steps");

            // Open the Graphics settings
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            Debug.Log("💡 Project Settings opened - navigate to Graphics for shader settings");
        }

        [MenuItem("NeonLadder/Build/Manual Optimizations/Open Graphics Settings")]
        public static void OpenGraphicsSettings()
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            Debug.Log("💡 Project Settings opened - go to Graphics → Shader Stripping");
        }

        [MenuItem("NeonLadder/Build/Manual Optimizations/Open Quality Settings")]
        public static void OpenQualitySettings()
        {
            EditorApplication.ExecuteMenuItem("Edit/Project Settings...");
            Debug.Log("💡 Project Settings opened - go to Quality to create fast development profile");
        }

        [MenuItem("NeonLadder/Build/Diagnosis/Estimate Build Impact")]
        public static void EstimateBuildImpact()
        {
            var scenes = EditorBuildSettings.scenes;
            var enabledScenes = scenes.Length;
            var shaderCount = AssetDatabase.FindAssets("t:Shader").Length;

            Debug.Log("=== BUILD TIME IMPACT ANALYSIS ===");
            Debug.Log($"Current Settings:");
            Debug.Log($"  Scenes: {enabledScenes} enabled");
            Debug.Log($"  Shaders: {shaderCount} total");

            float currentTime = (enabledScenes * 0.175f) + (shaderCount * 0.01f);
            Debug.Log($"  Estimated Time: {currentTime:F1} minutes");

            Debug.Log($"\nOptimization Scenarios:");

            // 1 scene scenario
            float oneSceneTime = (1 * 0.175f) + (shaderCount * 0.01f);
            Debug.Log($"  1 Scene + All Shaders: {oneSceneTime:F1} minutes");

            // Shader optimization scenario
            float optimizedShaders = (enabledScenes * 0.175f) + (20 * 0.01f); // Assume 20 essential shaders
            Debug.Log($"  All Scenes + 20 Shaders: {optimizedShaders:F1} minutes");

            // Best case
            float bestCase = (1 * 0.175f) + (20 * 0.01f);
            Debug.Log($"  1 Scene + 20 Shaders: {bestCase:F1} minutes");

            Debug.Log($"\nPotential Time Savings:");
            Debug.Log($"  Current → 1 Scene: {currentTime - oneSceneTime:F1} minutes saved");
            Debug.Log($"  Current → Optimized: {currentTime - bestCase:F1} minutes saved");

            if (shaderCount > 100)
            {
                Debug.LogWarning("⚠️ HIGH SHADER COUNT is major build bottleneck!");
                Debug.LogWarning("💡 Consider removing unused packages or enabling shader stripping");
            }
        }
    }
}