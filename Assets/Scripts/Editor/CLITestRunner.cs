using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using System.IO;
using System.Linq;

namespace NeonLadder.Editor
{
    /// <summary>
    /// CLI Test Runner for Unity 6 - Workaround for broken -runTests flag
    /// Usage: Unity.exe -batchmode -projectPath "path" -executeMethod CLITestRunner.RunPlayModeTests -quit
    /// </summary>
    public static class CLITestRunner
{
    private static string testResultsPath = "TestOutput/TestResults_CLI.xml";
    private static TestRunnerApi testRunnerApi;
    
    /// <summary>
    /// Runs PlayMode tests via TestRunnerApi (CLI workaround)
    /// </summary>
    [MenuItem("Tests/Run PlayMode Tests (CLI)")]
    public static void RunPlayModeTests()
    {
        Debug.Log("[CLITestRunner] Starting PlayMode tests via TestRunnerApi...");
        
        // Ensure TestOutput directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(testResultsPath));
        
        // Initialize TestRunner API
        testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        
        // Configure test filter for PlayMode
        var filter = new Filter()
        {
            testMode = TestMode.PlayMode,
            assemblyNames = new[] { "NeonLadder.Tests.Runtime" }
        };
        
        // Configure execution settings
        var executionSettings = new ExecutionSettings(filter);
        
        // Register callbacks
        testRunnerApi.RegisterCallbacks(new TestCallbacks());
        
        // Execute tests
        testRunnerApi.Execute(executionSettings);
    }
    
    /// <summary>
    /// Runs EditMode tests via TestRunnerApi (CLI workaround)
    /// </summary>
    [MenuItem("Tests/Run EditMode Tests (CLI)")]
    public static void RunEditModeTests()
    {
        Debug.Log("[CLITestRunner] Starting EditMode tests via TestRunnerApi...");
        
        // Ensure TestOutput directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(testResultsPath.Replace(".xml", "_EditMode.xml")));
        
        // Initialize TestRunner API
        testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
        
        // Configure test filter for EditMode
        var filter = new Filter()
        {
            testMode = TestMode.EditMode,
            assemblyNames = new[] { "NeonLadder.Tests.Editor" }
        };
        
        // Configure execution settings
        var executionSettings = new ExecutionSettings(filter);
        
        // Register callbacks
        testRunnerApi.RegisterCallbacks(new TestCallbacks());
        
        // Execute tests
        testRunnerApi.Execute(executionSettings);
    }
}

/// <summary>
/// Test execution callbacks for CLI test runner
/// </summary>
public class TestCallbacks : ICallbacks
{
    public void RunStarted(ITestAdaptor testsToRun)
    {
        Debug.Log($"[CLITestRunner] Test run started. Running tests from: {testsToRun.FullName}");
    }

    public void RunFinished(ITestResultAdaptor result)
    {
        Debug.Log($"[CLITestRunner] Test run finished!");
        Debug.Log($"[CLITestRunner] Passed: {result.PassCount}");
        Debug.Log($"[CLITestRunner] Failed: {result.FailCount}");
        Debug.Log($"[CLITestRunner] Skipped: {result.SkipCount}");
        Debug.Log($"[CLITestRunner] Inconclusive: {result.InconclusiveCount}");
        
        // Exit with appropriate code for CI/CD
        if (result.HasChildren && result.Children.Any(child => child.TestStatus == TestStatus.Failed))
        {
            Debug.LogError("[CLITestRunner] Some tests failed!");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log("[CLITestRunner] All tests passed!");
            EditorApplication.Exit(0);
        }
    }

    public void TestStarted(ITestAdaptor test)
    {
        Debug.Log($"[CLITestRunner] Starting test: {test.FullName}");
    }

    public void TestFinished(ITestResultAdaptor result)
    {
        string status = result.TestStatus == TestStatus.Passed ? "PASSED" : 
                       result.TestStatus == TestStatus.Failed ? "FAILED" : 
                       result.TestStatus == TestStatus.Skipped ? "SKIPPED" : "INCONCLUSIVE";
        
        Debug.Log($"[CLITestRunner] Test finished: {result.Test.FullName} - {status}");
        
        if (result.TestStatus == TestStatus.Failed)
        {
            Debug.LogError($"[CLITestRunner] Test failed: {result.Message}");
            if (!string.IsNullOrEmpty(result.StackTrace))
            {
                Debug.LogError($"[CLITestRunner] Stack trace: {result.StackTrace}");
            }
        }
    }
}
}