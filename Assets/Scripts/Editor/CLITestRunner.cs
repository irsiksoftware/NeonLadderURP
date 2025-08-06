using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using System;
using System.IO;

/// <summary>
/// Unity CLI Test Runner for NeonLadder project
/// Workaround for Unity 6's broken -runTests CLI flag
/// Uses TestRunnerApi with -executeMethod approach
/// </summary>
public static class CLITestRunner
{
    /// <summary>
    /// Run PlayMode tests via CLI using TestRunnerApi workaround
    /// Unity 6's -runTests is broken, so we use -executeMethod instead
    /// </summary>
    public static void RunPlayModeTests()
    {
        Debug.Log("CLITestRunner: Starting PlayMode tests using TestRunnerApi workaround");
        
        try
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            // Create test run settings for PlayMode tests
            var filter = new Filter()
            {
                testMode = TestMode.PlayMode,
                testNames = null, // Run all tests
                groupNames = null, // Run all groups
                categoryNames = null, // Run all categories
                assemblyNames = null // Run all assemblies
            };
            
            // Set up callback to handle test results
            var callback = new TestResultCallback();
            testRunnerApi.RegisterCallbacks(callback);
            
            Debug.Log("CLITestRunner: Executing PlayMode tests with TestRunnerApi");
            
            // Execute the tests
            testRunnerApi.Execute(new ExecutionSettings(filter));
            
            // Note: In Unity 6, the callback system has timing issues in CLI mode
            // Tests will execute correctly but Unity may not auto-exit
            // This is expected behavior - check the XML results file for test outcomes
            
        }
        catch (Exception e)
        {
            Debug.LogError($"CLITestRunner: Failed to execute tests - {e.Message}");
            Debug.LogError($"CLITestRunner: Stack trace - {e.StackTrace}");
            EditorApplication.Exit(1);
        }
    }
    
    /// <summary>
    /// Run EditMode tests via CLI using TestRunnerApi workaround
    /// </summary>
    public static void RunEditModeTests()
    {
        Debug.Log("CLITestRunner: Starting EditMode tests using TestRunnerApi workaround");
        
        try
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var filter = new Filter()
            {
                testMode = TestMode.EditMode,
                testNames = null,
                groupNames = null,
                categoryNames = null,
                assemblyNames = null
            };
            
            var callback = new TestResultCallback();
            testRunnerApi.RegisterCallbacks(callback);
            
            Debug.Log("CLITestRunner: Executing EditMode tests with TestRunnerApi");
            testRunnerApi.Execute(new ExecutionSettings(filter));
            
        }
        catch (Exception e)
        {
            Debug.LogError($"CLITestRunner: Failed to execute EditMode tests - {e.Message}");
            EditorApplication.Exit(1);
        }
    }
}

/// <summary>
/// Callback handler for test results
/// Saves results to XML file for CI/CD integration
/// </summary>
public class TestResultCallback : ICallbacks
{
    private int totalTests = 0;
    private int passedTests = 0;
    private int failedTests = 0;
    private string xmlResultsPath;
    
    public void RunStarted(ITestAdaptor testsToRun)
    {
        totalTests = CountTests(testsToRun);
        xmlResultsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ShorelineGames, LLC", "NeonLadder", "TestResults.xml"
        );
        
        // Ensure directory exists
        Directory.CreateDirectory(Path.GetDirectoryName(xmlResultsPath));
        
        Debug.Log($"CLITestRunner: Starting test run with {totalTests} tests");
        Debug.Log($"CLITestRunner: Results will be saved to: {xmlResultsPath}");
    }
    
    public void RunFinished(ITestResultAdaptor result)
    {
        Debug.Log($"CLITestRunner: Test run completed");
        Debug.Log($"CLITestRunner: Total: {totalTests}, Passed: {passedTests}, Failed: {failedTests}");
        Debug.Log($"CLITestRunner: Saving results to: {xmlResultsPath}");
        
        try
        {
            // Save XML results
            SaveXmlResults(result);
            
            Debug.Log($"CLITestRunner: Results saved successfully");
            
            // Exit with appropriate code
            int exitCode = failedTests > 0 ? 1 : 0;
            Debug.Log($"CLITestRunner: Exiting with code {exitCode}");
            
            // Note: EditorApplication.Exit() may not work reliably in Unity 6 CLI mode
            // This is a known Unity 6 issue - the process may need to be killed manually
            EditorApplication.Exit(exitCode);
        }
        catch (Exception e)
        {
            Debug.LogError($"CLITestRunner: Failed to save results - {e.Message}");
            EditorApplication.Exit(1);
        }
    }
    
    public void TestStarted(ITestAdaptor test)
    {
        Debug.Log($"CLITestRunner: Starting test: {test.FullName}");
    }
    
    public void TestFinished(ITestResultAdaptor result)
    {
        if (result.TestStatus == TestStatus.Passed)
        {
            passedTests++;
            Debug.Log($"CLITestRunner: ✅ PASSED: {result.Test.FullName}");
        }
        else
        {
            failedTests++;
            Debug.Log($"CLITestRunner: ❌ FAILED: {result.Test.FullName}");
            if (!string.IsNullOrEmpty(result.Message))
            {
                Debug.Log($"CLITestRunner: Error: {result.Message}");
            }
        }
    }
    
    private int CountTests(ITestAdaptor test)
    {
        if (!test.HasChildren)
            return test.IsSuite ? 0 : 1;
        
        int count = 0;
        foreach (var child in test.Children)
        {
            count += CountTests(child);
        }
        return count;
    }
    
    private void SaveXmlResults(ITestResultAdaptor result)
    {
        // Simple XML format for CI/CD integration
        var xml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites>
    <testsuite name=""NeonLadder Tests"" tests=""{totalTests}"" failures=""{failedTests}"" time=""0"">
        {GenerateXmlTests(result)}
    </testsuite>
</testsuites>";
        
        File.WriteAllText(xmlResultsPath, xml);
    }
    
    private string GenerateXmlTests(ITestResultAdaptor result)
    {
        var xml = "";
        
        if (result.HasChildren)
        {
            foreach (var child in result.Children)
            {
                xml += GenerateXmlTests(child);
            }
        }
        else if (!result.Test.IsSuite)
        {
            var status = result.TestStatus == TestStatus.Passed ? "passed" : "failed";
            var message = result.Message?.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") ?? "";
            
            xml += $@"        <testcase name=""{result.Test.Name}"" classname=""{result.Test.FullName}"" time=""0"">
";
            
            if (result.TestStatus != TestStatus.Passed)
            {
                xml += $@"            <failure message=""{message}""></failure>
";
            }
            
            xml += $@"        </testcase>
";
        }
        
        return xml;
    }
}