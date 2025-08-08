using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Editor.BuildSystem
{
    /// <summary>
    /// Utility to fix shader compilation issues during builds
    /// Helps identify and optionally disable problematic shaders
    /// </summary>
    public static class ShaderFixUtility
    {
        [MenuItem("NeonLadder/Build & Deploy/Diagnostics/List All Shaders", priority = 200)]
        public static void ListAllShaders()
        {
            var shaders = AssetDatabase.FindAssets("t:Shader");
            UnityEngine.Debug.Log($"=== FOUND {shaders.Length} SHADERS IN PROJECT ===");
            
            var artngameShaders = new List<string>();
            
            foreach (var guid in shaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.Contains("ARTnGAME"))
                {
                    artngameShaders.Add(path);
                }
            }
            
            UnityEngine.Debug.Log($"=== {artngameShaders.Count} ARTnGAME SHADERS ===");
            foreach (var shader in artngameShaders)
            {
                UnityEngine.Debug.Log($"  - {shader}");
            }
        }
        
        [MenuItem("NeonLadder/Build & Deploy/Diagnostics/Find Problematic Shaders", priority = 201)]
        public static void FindProblematicShaders()
        {
            UnityEngine.Debug.Log("=== CHECKING FOR PROBLEMATIC SHADERS ===");
            
            // These are the shaders causing crashes based on the logs
            string[] problematicShaderNames = new string[]
            {
                "LAYERS_FOGWAR",
                "FullVolumeCloudsURP",
                "VolumetricLighting",
                "BlitPassAtmosSRP",
                "BlitPassVolumeFogSRP",
                "BlitPassFullVolumeCloudsSRP"
            };
            
            var shaders = AssetDatabase.FindAssets("t:Shader");
            var foundProblematic = new List<string>();
            
            foreach (var guid in shaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                
                if (shader != null)
                {
                    foreach (var problemName in problematicShaderNames)
                    {
                        if (shader.name.Contains(problemName) || path.Contains(problemName))
                        {
                            foundProblematic.Add($"{shader.name} at {path}");
                            break;
                        }
                    }
                }
            }
            
            if (foundProblematic.Count > 0)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Found {foundProblematic.Count} problematic shaders:");
                foreach (var shader in foundProblematic)
                {
                    UnityEngine.Debug.LogWarning($"  - {shader}");
                }
                
                UnityEngine.Debug.Log("\n🔧 RECOMMENDED FIXES:");
                UnityEngine.Debug.Log("1. Update ARTnGAME packages to Unity 6 compatible versions");
                UnityEngine.Debug.Log("2. Temporarily disable these shaders in Graphics Settings");
                UnityEngine.Debug.Log("3. Use shader stripping to exclude them from builds");
                UnityEngine.Debug.Log("4. Contact ARTnGAME support for Unity 6 compatibility");
            }
            else
            {
                UnityEngine.Debug.Log("✅ No obviously problematic shaders found by name");
            }
        }
        
        [MenuItem("NeonLadder/Build & Deploy/Diagnostics/Create Shader Variant Stripping", priority = 202)]
        public static void CreateShaderVariantStripping()
        {
            UnityEngine.Debug.Log("=== SHADER STRIPPING CONFIGURATION ===");
            UnityEngine.Debug.Log("To exclude problematic shaders from builds:");
            UnityEngine.Debug.Log("1. Go to Edit > Project Settings > Graphics");
            UnityEngine.Debug.Log("2. Under 'Shader Stripping', set:");
            UnityEngine.Debug.Log("   - Lightmap Modes: Manual");
            UnityEngine.Debug.Log("   - Fog Modes: Manual");
            UnityEngine.Debug.Log("   - Instancing Variants: Strip Unused");
            UnityEngine.Debug.Log("3. Under 'Always Included Shaders', remove any ARTnGAME shaders");
            UnityEngine.Debug.Log("4. Consider using Shader Variant Collections to control what's included");
            
            EditorUtility.DisplayDialog("Shader Stripping Guide",
                "Check the Console for instructions on configuring shader stripping.\n\n" +
                "This can help exclude problematic shaders from builds.",
                "Open Graphics Settings");
                
            SettingsService.OpenProjectSettings("Project/Graphics");
        }
        
        [MenuItem("NeonLadder/Build & Deploy/Quick Fix/Disable ARTnGAME Compute Shaders", priority = 203)]
        public static void DisableProblematicComputeShaders()
        {
            UnityEngine.Debug.Log("=== ATTEMPTING TO DISABLE PROBLEMATIC COMPUTE SHADERS ===");
            
            // Find and disable compute shaders that are crashing
            var computeShaders = AssetDatabase.FindAssets("t:ComputeShader");
            int disabledCount = 0;
            
            foreach (var guid in computeShaders)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                
                if (path.Contains("ARTnGAME") && path.Contains("VolumetricLighting"))
                {
                    UnityEngine.Debug.Log($"Found problematic compute shader: {path}");
                    
                    // Rename to .backup to exclude from build
                    var backupPath = path + ".backup";
                    if (!System.IO.File.Exists(backupPath))
                    {
                        AssetDatabase.MoveAsset(path, backupPath);
                        disabledCount++;
                        UnityEngine.Debug.Log($"  ✅ Disabled by renaming to: {backupPath}");
                    }
                }
            }
            
            if (disabledCount > 0)
            {
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"✅ Disabled {disabledCount} problematic compute shaders");
                UnityEngine.Debug.Log("💡 To re-enable, rename .backup files back to .compute");
            }
            else
            {
                UnityEngine.Debug.Log("No compute shaders were disabled");
            }
        }
        
        [MenuItem("NeonLadder/Build & Deploy/Quick Fix/Restore Disabled Shaders", priority = 204)]
        public static void RestoreDisabledShaders()
        {
            var backupFiles = System.IO.Directory.GetFiles(Application.dataPath, "*.backup", System.IO.SearchOption.AllDirectories);
            int restoredCount = 0;
            
            foreach (var backupFile in backupFiles)
            {
                if (backupFile.Contains("ARTnGAME"))
                {
                    var originalPath = backupFile.Replace(".backup", "");
                    System.IO.File.Move(backupFile, originalPath);
                    restoredCount++;
                    UnityEngine.Debug.Log($"Restored: {originalPath}");
                }
            }
            
            if (restoredCount > 0)
            {
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"✅ Restored {restoredCount} shader files");
            }
        }
    }
}