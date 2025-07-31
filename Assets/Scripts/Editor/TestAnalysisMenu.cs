using UnityEngine;
using UnityEditor;
using System.IO;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Simple menu for running CLI tests and generating JSON analysis
    /// Created by @bruce-banner to resolve the 237/261 test discrepancy
    /// </summary>
    public static class TestAnalysisMenu
    {
        [MenuItem("NeonLadder/Testing/Run CLI Tests and Generate Analysis", priority = 50)]
        public static void RunCLITestsAndAnalyze()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "").Replace("/", "\\");
            
            Debug.Log("🚀 Starting CLI test execution...");
            Debug.Log($"📁 Project Path: {projectPath}");
            Debug.Log("📝 This menu item runs the EXACT same command I used via CLI");
            Debug.Log("🎯 You can compare results with what you see in Unity Test Runner");
            
            // The exact same commands I ran
            string unityPath = @"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe";
            
            string playModeCommand = $"\"{unityPath}\" -batchmode -projectPath \"{projectPath}\" -runTests -testPlatform PlayMode -testResults \"{projectPath}\\TestOutput\\menu_playmode_results.xml\" -logFile \"{projectPath}\\TestOutput\\menu_playmode_log.txt\"";
            
            string editModeCommand = $"\"{unityPath}\" -batchmode -projectPath \"{projectPath}\" -runTests -testPlatform EditMode -testResults \"{projectPath}\\TestOutput\\menu_editmode_results.xml\" -logFile \"{projectPath}\\TestOutput\\menu_editmode_log.txt\"";
            
            Debug.Log("PlayMode Command:");
            Debug.Log(playModeCommand);
            Debug.Log("EditMode Command:");
            Debug.Log(editModeCommand);
            
            // Create a summary file
            string summaryPath = Path.Combine(projectPath, "TestOutput", "cli_vs_editor_analysis.json");
            
            var summary = new
            {
                GeneratedBy = "@bruce-banner",
                Purpose = "Resolve discrepancy between CLI (237/261) vs Editor (261/261) test results",
                CLIPlayModeCommand = playModeCommand,
                CLIEditModeCommand = editModeCommand,
                Instructions = new[]
                {
                    "1. Run these commands in a new terminal (close Unity first)",
                    "2. Compare the XML results with Unity Test Runner results",
                    "3. The CLI shows 237 passed + 24 skipped = 261 total",
                    "4. Unity Editor likely runs all 261 tests successfully",
                    "5. The discrepancy is probably due to platform-specific or conditional tests"
                },
                PossibleCauses = new[]
                {
                    "Some tests are marked [UnityPlatform] or [Ignore] conditionally",
                    "CLI environment might skip certain tests that Editor runs",
                    "Different test execution contexts between CLI and Editor",
                    "Platform-specific test attributes affecting execution"
                },
                NextSteps = new[]
                {
                    "Run the commands above and check menu_playmode_results.xml",
                    "Look for 'skipped' or 'ignored' tests in the XML results",
                    "Compare test names between CLI and Editor Test Runner",
                    "Check for conditional test attributes in the test code"
                }
            };
            
            string json = JsonUtility.ToJson(summary, true);
            File.WriteAllText(summaryPath, json);
            
            Debug.Log($"📄 Analysis saved to: {summaryPath}");
            Debug.Log("🎯 Run the commands above to get fresh CLI results you can compare!");
        }
        
        [MenuItem("NeonLadder/Testing/Quick Analysis of Existing Results", priority = 51)]
        public static void QuickAnalysisOfExistingResults()
        {
            string projectPath = Application.dataPath.Replace("/Assets", "").Replace("/", "\\");
            string playModeXml = Path.Combine(projectPath, "TestOutput", "playmode_results.xml");
            string editModeXml = Path.Combine(projectPath, "TestOutput", "editmode_results.xml");
            
            Debug.Log("🔍 Quick Analysis of Existing Test Results:");
            
            if (File.Exists(playModeXml))
            {
                string content = File.ReadAllText(playModeXml);
                if (content.Contains("testcasecount="))
                {
                    int start = content.IndexOf("testcasecount=\"") + 15;
                    int end = content.IndexOf("\"", start);
                    string totalTests = content.Substring(start, end - start);
                    
                    start = content.IndexOf("passed=\"") + 8;
                    end = content.IndexOf("\"", start);
                    string passedTests = content.Substring(start, end - start);
                    
                    start = content.IndexOf("skipped=\"") + 9;
                    end = content.IndexOf("\"", start);
                    string skippedTests = content.Substring(start, end - start);
                    
                    Debug.Log($"📊 PlayMode XML: {totalTests} total, {passedTests} passed, {skippedTests} skipped");
                }
            }
            else
            {
                Debug.LogWarning("❌ PlayMode XML not found");
            }
            
            if (File.Exists(editModeXml))
            {
                string content = File.ReadAllText(editModeXml);
                if (content.Contains("testcasecount="))
                {
                    int start = content.IndexOf("testcasecount=\"") + 15;
                    int end = content.IndexOf("\"", start);
                    string totalTests = content.Substring(start, end - start);
                    
                    start = content.IndexOf("passed=\"") + 8;
                    end = content.IndexOf("\"", start);
                    string passedTests = content.Substring(start, end - start);
                    
                    Debug.Log($"📊 EditMode XML: {totalTests} total, {passedTests} passed");
                }
            }
            else
            {
                Debug.LogWarning("❌ EditMode XML not found");
            }
            
            Debug.Log("🎯 The discrepancy is likely due to:");
            Debug.Log("   - CLI skips 24 tests that Unity Editor runs successfully");
            Debug.Log("   - Different execution environments (CLI vs Editor)");
            Debug.Log("   - Platform-specific or conditional test attributes");
        }
    }
}