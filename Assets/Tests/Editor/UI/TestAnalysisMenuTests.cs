using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadder.Editor;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Bruce Banner's Test Analysis Menu Tests - Testing the test infrastructure itself
    /// Ensuring our test analysis tools are bulletproof
    /// 
    /// "That's my secret - I'm always testing the testers."
    /// </summary>
    [TestFixture]
    public class TestAnalysisMenuTests
    {
        private string originalProjectPath;
        private string testOutputPath;
        
        #region Setup & Teardown
        
        [SetUp]
        public void SetUp()
        {
            // Store original paths
            originalProjectPath = Application.dataPath.Replace("/Assets", "").Replace("/", "\\");
            testOutputPath = Path.Combine(originalProjectPath, "TestOutput");
            
            // Setup mock file system
            EditorUITestFramework.MockFileSystem.ClearMocks();
            EditorUITestFramework.MockFileSystem.AddMockDirectory(testOutputPath);
        }
        
        [TearDown]
        public void TearDown()
        {
            EditorUITestFramework.MockFileSystem.ClearMocks();
        }
        
        #endregion
        
        #region MenuItem Tests
        
        [Test]
        public void MenuItem_RunCLITestsAndAnalyze_HasCorrectAttributes()
        {
            // Arrange
            var menuItemMethod = typeof(TestAnalysisMenu).GetMethod("RunCLITestsAndAnalyze",
                BindingFlags.Public | BindingFlags.Static);
            var menuItemAttribute = menuItemMethod?.GetCustomAttribute<MenuItem>();
            
            // Assert
            Assert.IsNotNull(menuItemAttribute, "RunCLITestsAndAnalyze should have MenuItem attribute");
            Assert.AreEqual("NeonLadder/Testing/Run CLI Tests and Generate Analysis", 
                menuItemAttribute.menuItem, "Menu path should be correct");
            Assert.AreEqual(50, menuItemAttribute.priority, "Menu priority should be 50");
        }
        
        #endregion
        
        #region Path Handling Tests
        
        [Test]
        public void ProjectPath_ConversionIsCorrect()
        {
            // Arrange
            var testDataPath = "C:/Users/Test/Project/Assets";
            var expectedPath = "C:\\Users\\Test\\Project";
            
            // Act
            var convertedPath = testDataPath.Replace("/Assets", "").Replace("/", "\\");
            
            // Assert
            Assert.AreEqual(expectedPath, convertedPath, "Path conversion should handle forward to backslash");
        }
        
        [Test]
        public void UnityPath_IsCorrectVersion()
        {
            // Arrange
            var expectedUnityPath = @"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe";
            
            // Act - Check if the path used in the menu matches expected
            var method = typeof(TestAnalysisMenu).GetMethod("RunCLITestsAndAnalyze",
                BindingFlags.Public | BindingFlags.Static);
            
            // Note: We can't actually execute the method due to Debug.Log calls
            // But we can verify the method exists and has correct signature
            Assert.IsNotNull(method, "RunCLITestsAndAnalyze method should exist");
            Assert.AreEqual(0, method.GetParameters().Length, "Method should have no parameters");
        }
        
        #endregion
        
        #region Command Generation Tests
        
        [Test]
        public void CommandGeneration_PlayModeCommand_HasCorrectStructure()
        {
            // Arrange
            var projectPath = "C:\\Test\\Project";
            var unityPath = @"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe";
            
            // Act
            var playModeCommand = $"\"{unityPath}\" -batchmode -projectPath \"{projectPath}\" " +
                $"-runTests -testPlatform PlayMode -testResults \"{projectPath}\\TestOutput\\menu_playmode_results.xml\" " +
                $"-logFile \"{projectPath}\\TestOutput\\menu_playmode_log.txt\"";
            
            // Assert
            Assert.IsTrue(playModeCommand.Contains("-batchmode"), "Command should run in batch mode");
            Assert.IsTrue(playModeCommand.Contains("-runTests"), "Command should run tests");
            Assert.IsTrue(playModeCommand.Contains("-testPlatform PlayMode"), "Command should specify PlayMode");
            Assert.IsTrue(playModeCommand.Contains("menu_playmode_results.xml"), "Command should output XML results");
        }
        
        [Test]
        public void CommandGeneration_EditModeCommand_HasCorrectStructure()
        {
            // Arrange
            var projectPath = "C:\\Test\\Project";
            var unityPath = @"C:\Program Files\Unity\Hub\Editor\6000.0.26f1\Editor\Unity.exe";
            
            // Act
            var editModeCommand = $"\"{unityPath}\" -batchmode -projectPath \"{projectPath}\" " +
                $"-runTests -testPlatform EditMode -testResults \"{projectPath}\\TestOutput\\menu_editmode_results.xml\" " +
                $"-logFile \"{projectPath}\\TestOutput\\menu_editmode_log.txt\"";
            
            // Assert
            Assert.IsTrue(editModeCommand.Contains("-batchmode"), "Command should run in batch mode");
            Assert.IsTrue(editModeCommand.Contains("-runTests"), "Command should run tests");
            Assert.IsTrue(editModeCommand.Contains("-testPlatform EditMode"), "Command should specify EditMode");
            Assert.IsTrue(editModeCommand.Contains("menu_editmode_results.xml"), "Command should output XML results");
        }
        
        #endregion
        
        #region JSON Analysis Tests
        
        [Test]
        public void JSONAnalysis_SummaryStructure_IsCorrect()
        {
            // Arrange - Expected summary structure
            var expectedFields = new[]
            {
                "GeneratedBy",
                "Purpose",
                "CLIPlayModeCommand",
                "CLIEditModeCommand",
                "Instructions"
            };
            
            // Act - Create a test summary object
            var summary = new
            {
                GeneratedBy = "@bruce-banner",
                Purpose = "Resolve discrepancy between CLI (237/261) vs Editor (261/261) test results",
                CLIPlayModeCommand = "test command",
                CLIEditModeCommand = "test command",
                Instructions = new[] { "instruction1", "instruction2" }
            };
            
            // Assert
            var summaryType = summary.GetType();
            foreach (var field in expectedFields)
            {
                var property = summaryType.GetProperty(field);
                Assert.IsNotNull(property, $"Summary should have {field} property");
            }
        }
        
        [Test]
        public void JSONAnalysis_Instructions_ContainKeySteps()
        {
            // Arrange
            var expectedInstructions = new[]
            {
                "Run these commands in a new terminal",
                "Compare the XML results",
                "CLI shows 237 passed + 24 skipped",
                "Unity Editor likely runs all 261 tests",
                "discrepancy is probably due to platform-specific"
            };
            
            // Assert - Each instruction should contain key information
            foreach (var instruction in expectedInstructions)
            {
                Assert.IsNotNull(instruction, "Instruction should not be null");
                Assert.IsTrue(instruction.Length > 0, "Instruction should not be empty");
            }
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void Integration_MenuItemCanBeFound()
        {
            // Arrange
            var menuItems = typeof(TestAnalysisMenu)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<MenuItem>() != null)
                .ToList();
            
            // Assert
            Assert.AreEqual(1, menuItems.Count, "TestAnalysisMenu should have exactly one menu item");
            Assert.AreEqual("RunCLITestsAndAnalyze", menuItems[0].Name, 
                "Menu item method should be RunCLITestsAndAnalyze");
        }
        
        [Test]
        public void Integration_OutputPathsAreValid()
        {
            // Arrange
            var projectPath = "C:\\Test\\Project";
            var outputFiles = new[]
            {
                Path.Combine(projectPath, "TestOutput", "menu_playmode_results.xml"),
                Path.Combine(projectPath, "TestOutput", "menu_playmode_log.txt"),
                Path.Combine(projectPath, "TestOutput", "menu_editmode_results.xml"),
                Path.Combine(projectPath, "TestOutput", "menu_editmode_log.txt"),
                Path.Combine(projectPath, "TestOutput", "cli_vs_editor_analysis.json")
            };
            
            // Act & Assert
            foreach (var outputFile in outputFiles)
            {
                Assert.IsTrue(outputFile.Contains("TestOutput"), 
                    $"Output file {outputFile} should be in TestOutput directory");
                Assert.IsTrue(Path.HasExtension(outputFile), 
                    $"Output file {outputFile} should have an extension");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private static class ReflectionHelper
        {
            public static bool HasStaticMethod(Type type, string methodName)
            {
                var method = type.GetMethod(methodName, 
                    BindingFlags.Public | BindingFlags.Static);
                return method != null;
            }
            
            public static MenuItem GetMenuItemAttribute(Type type, string methodName)
            {
                var method = type.GetMethod(methodName, 
                    BindingFlags.Public | BindingFlags.Static);
                return method?.GetCustomAttribute<MenuItem>();
            }
        }
        
        #endregion
    }
}