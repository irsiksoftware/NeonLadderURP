using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System;
using System.IO;

namespace NeonLadder.Editor.BuildSystem
{
    /// <summary>
    /// Automated build testing system for debugging build issues
    /// </summary>
    public static class BuildSystemTester
    {
        [MenuItem("NeonLadder/Build & Deploy/Test Steam Build (Auto-Fix)", priority = 50)]
        public static void TestSteamBuildAutoFix()
        {
            UnityEngine.Debug.Log("=== AUTOMATED STEAM BUILD TEST STARTED ===");
            
            // Clear console for clean output
            var logEntries = System.Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            clearMethod.Invoke(null, null);
            
            // Test compilation first
            if (!TestCompilation())
            {
                UnityEngine.Debug.LogError("❌ Compilation failed - cannot proceed with build test");
                return;
            }
            
            // Run the actual build test
            TestSteamBuild();
        }
        
        private static bool TestCompilation()
        {
            UnityEngine.Debug.Log("🔍 Testing compilation...");
            
            // Force recompile to catch any compilation errors
            AssetDatabase.Refresh();
            
            // Check for compilation errors
            var hasErrors = EditorUtility.scriptCompilationFailed;
            
            if (hasErrors)
            {
                UnityEngine.Debug.LogError("❌ Compilation errors detected:");
                return false;
            }
            
            UnityEngine.Debug.Log("✅ Compilation successful");
            return true;
        }
        
        private static void TestSteamBuild()
        {
            UnityEngine.Debug.Log("🚀 Running Steam build test...");
            
            try
            {
                // Create test build configuration
                var buildPath = Path.Combine("TestOutput", "SteamBuildTest", "NeonLadder.exe");
                var buildDir = Path.GetDirectoryName(buildPath);
                
                UnityEngine.Debug.Log($"📁 Creating build directory: {buildDir}");
                Directory.CreateDirectory(buildDir);
                
                var buildOptions = new BuildPlayerOptions
                {
                    scenes = GetEnabledScenes(),
                    locationPathName = buildPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development, // Use development build for faster testing
                    targetGroup = BuildTargetGroup.Standalone
                };
                
                UnityEngine.Debug.Log($"🎯 Build target: {buildOptions.target}");
                UnityEngine.Debug.Log($"📍 Build path: {buildOptions.locationPathName}");
                UnityEngine.Debug.Log($"🎬 Scenes: {buildOptions.scenes.Length} scenes");
                
                // Execute build
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var report = BuildPipeline.BuildPlayer(buildOptions);
                stopwatch.Stop();
                
                // Analyze results
                AnalyzeBuildResults(report, stopwatch.Elapsed);
                
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"❌ Build test exception: {e.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }
        
        private static void AnalyzeBuildResults(BuildReport report, TimeSpan buildTime)
        {
            UnityEngine.Debug.Log($"=== BUILD RESULTS ANALYSIS ===");
            UnityEngine.Debug.Log($"📊 Result: {report.summary.result}");
            UnityEngine.Debug.Log($"⏱️ Build Time: {buildTime:mm\\:ss}");
            UnityEngine.Debug.Log($"📦 Total Size: {report.summary.totalSize / (1024f * 1024f):F2} MB");
            UnityEngine.Debug.Log($"❌ Errors: {report.summary.totalErrors}");
            UnityEngine.Debug.Log($"⚠️ Warnings: {report.summary.totalWarnings}");
            UnityEngine.Debug.Log($"📁 Output: {report.summary.outputPath}");
            
            // Check if build succeeded
            if (report.summary.result == BuildResult.Succeeded)
            {
                UnityEngine.Debug.Log("🎉 BUILD TEST PASSED! Steam build is working.");
                
                // Verify output files exist
                if (File.Exists(report.summary.outputPath))
                {
                    var fileInfo = new FileInfo(report.summary.outputPath);
                    UnityEngine.Debug.Log($"✅ Executable created: {fileInfo.Length / (1024f * 1024f):F2} MB");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("⚠️ Build reported success but executable not found");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("❌ BUILD TEST FAILED!");
                
                // Log detailed error information
                LogBuildErrors(report);
                
                // Suggest fixes
                SuggestBuildFixes(report);
            }
        }
        
        private static void LogBuildErrors(BuildReport report)
        {
            UnityEngine.Debug.Log("=== DETAILED ERROR ANALYSIS ===");
            
            foreach (var step in report.steps)
            {
                if (step.messages.Length > 0)
                {
                    UnityEngine.Debug.Log($"📝 Step: {step.name}");
                    foreach (var message in step.messages)
                    {
                        if (message.type == LogType.Error)
                        {
                            UnityEngine.Debug.LogError($"  ❌ {message.content}");
                        }
                        else if (message.type == LogType.Warning)
                        {
                            UnityEngine.Debug.LogWarning($"  ⚠️ {message.content}");
                        }
                    }
                }
            }
        }
        
        private static void SuggestBuildFixes(BuildReport report)
        {
            UnityEngine.Debug.Log("=== SUGGESTED FIXES ===");
            
            if (report.summary.totalErrors > 0)
            {
                UnityEngine.Debug.Log("🔧 Compilation errors detected:");
                UnityEngine.Debug.Log("  1. Check for missing assembly references in .asmdef files");
                UnityEngine.Debug.Log("  2. Verify all using statements are correct");
                UnityEngine.Debug.Log("  3. Check for missing packages in Package Manager");
            }
            
            if (report.summary.totalSize == 0)
            {
                UnityEngine.Debug.Log("🔧 Zero-size build detected:");
                UnityEngine.Debug.Log("  1. Build likely failed during compilation phase");
                UnityEngine.Debug.Log("  2. Check Unity Console for compilation errors");
                UnityEngine.Debug.Log("  3. Fix all red errors before attempting build");
            }
        }
        
        private static string[] GetEnabledScenes()
        {
            var scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }
        
        [MenuItem("NeonLadder/Build & Deploy/Quick Compilation Test", priority = 49)]
        public static void QuickCompilationTest()
        {
            UnityEngine.Debug.Log("=== QUICK COMPILATION TEST ===");
            
            // Force recompile
            AssetDatabase.Refresh();
            
            // Wait for compilation to complete
            if (EditorApplication.isCompiling)
            {
                UnityEngine.Debug.Log("⏳ Waiting for compilation to complete...");
                EditorApplication.delayCall += () => {
                    if (!EditorApplication.isCompiling)
                    {
                        CheckCompilationResults();
                    }
                };
            }
            else
            {
                CheckCompilationResults();
            }
        }
        
        private static void CheckCompilationResults()
        {
            if (EditorUtility.scriptCompilationFailed)
            {
                UnityEngine.Debug.LogError("❌ COMPILATION FAILED - Fix errors before building");
            }
            else
            {
                UnityEngine.Debug.Log("✅ COMPILATION SUCCESSFUL - Ready for build");
            }
        }
    }
}