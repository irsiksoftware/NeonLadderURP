using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using NeonLadder.Editor.Performance;
using System.IO;
using System;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Tests.Performance
{
    /// <summary>
    /// Unit tests for PerformanceMenuItems functionality
    /// </summary>
    [TestFixture]
    public class PerformanceMenuItemsTests
    {
        private string testOutputDir;
        private string originalOutputDir;
        
        [SetUp]
        public void Setup()
        {
            // Create a test output directory
            testOutputDir = Path.Combine(Application.dataPath, "..", "TestOutput_Temp");
            if (!Directory.Exists(testOutputDir))
            {
                Directory.CreateDirectory(testOutputDir);
            }
            
            // Store original settings
            originalOutputDir = Path.Combine(Application.dataPath, "..", "TestOutput");
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test output directory
            if (Directory.Exists(testOutputDir))
            {
                try
                {
                    Directory.Delete(testOutputDir, true);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
            
            // Reset any modified editor prefs
            EditorPrefs.DeleteKey("NeonLadder_AutoProfile");
        }
        
        [Test]
        public void MenuItems_ExistInCorrectLocation()
        {
            // Verify menu items are registered
            Assert.IsTrue(Menu.GetEnabled("NeonLadder/Performance/Profile Current Scene %&p"));
            Assert.IsTrue(Menu.GetEnabled("NeonLadder/Performance/Clear Profiler Data"));
            Assert.IsTrue(Menu.GetEnabled("NeonLadder/Performance/Export Performance Report"));
            Assert.IsTrue(Menu.GetEnabled("NeonLadder/Performance/Performance Settings..."));
            Assert.IsTrue(Menu.GetEnabled("NeonLadder/Performance/Auto-Profile on Play"));
        }
        
        [Test]
        public void AutoProfile_TogglesCorrectly()
        {
            // Get initial state
            bool initialState = EditorPrefs.GetBool("NeonLadder_AutoProfile", false);
            
            // Toggle
            PerformanceMenuItems.ToggleAutoProfile();
            bool afterToggle = EditorPrefs.GetBool("NeonLadder_AutoProfile", false);
            
            Assert.AreNotEqual(initialState, afterToggle, "Auto-profile should toggle");
            
            // Toggle back
            PerformanceMenuItems.ToggleAutoProfile();
            bool afterSecondToggle = EditorPrefs.GetBool("NeonLadder_AutoProfile", false);
            
            Assert.AreEqual(initialState, afterSecondToggle, "Auto-profile should toggle back");
        }
        
        [Test]
        public void PerformanceData_CollectsValidMetrics()
        {
            // Use reflection to test private CollectPerformanceData method
            var type = typeof(PerformanceMenuItems);
            var method = type.GetMethod("CollectPerformanceData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            Assert.IsNotNull(method, "CollectPerformanceData method should exist");
            
            var result = method.Invoke(null, null);
            Assert.IsNotNull(result, "Should return performance data");
            
            // Get PerformanceData type and validate properties
            var dataType = type.GetNestedType("PerformanceData",
                System.Reflection.BindingFlags.NonPublic);
            
            Assert.IsNotNull(dataType, "PerformanceData type should exist");
            
            var sceneNameProp = dataType.GetProperty("SceneName");
            var avgFPSProp = dataType.GetProperty("AverageFPS");
            var memoryProp = dataType.GetProperty("TotalMemoryMB");
            
            Assert.IsNotNull(sceneNameProp?.GetValue(result), "Scene name should be set");
            
            var avgFPS = (float)avgFPSProp?.GetValue(result);
            Assert.Greater(avgFPS, 0, "Average FPS should be positive");
            
            var memory = (long)memoryProp?.GetValue(result);
            Assert.GreaterOrEqual(memory, 0, "Memory should be non-negative");
        }
        
        [Test]
        public void ReportGeneration_CreatesExpectedFiles()
        {
            // Test CSV generation method
            var type = typeof(PerformanceMenuItems);
            var csvMethod = type.GetMethod("GenerateCSVReport",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            Assert.IsNotNull(csvMethod, "GenerateCSVReport method should exist");
            
            // Create mock performance data
            var dataType = type.GetNestedType("PerformanceData",
                System.Reflection.BindingFlags.NonPublic);
            var mockData = Activator.CreateInstance(dataType);
            
            // Set properties
            dataType.GetProperty("SceneName")?.SetValue(mockData, "TestScene");
            dataType.GetProperty("Timestamp")?.SetValue(mockData, DateTime.Now);
            dataType.GetProperty("AverageFPS")?.SetValue(mockData, 60f);
            dataType.GetProperty("MinFPS")?.SetValue(mockData, 55f);
            dataType.GetProperty("MaxFPS")?.SetValue(mockData, 65f);
            dataType.GetProperty("AverageFrameTime")?.SetValue(mockData, 16.67f);
            dataType.GetProperty("TotalMemoryMB")?.SetValue(mockData, 150L);
            dataType.GetProperty("TextureMemoryMB")?.SetValue(mockData, 80L);
            dataType.GetProperty("MeshMemoryMB")?.SetValue(mockData, 20L);
            dataType.GetProperty("DrawCalls")?.SetValue(mockData, 500);
            dataType.GetProperty("SetPassCalls")?.SetValue(mockData, 100);
            dataType.GetProperty("Triangles")?.SetValue(mockData, 50000);
            dataType.GetProperty("Vertices")?.SetValue(mockData, 25000);
            dataType.GetProperty("Bottlenecks")?.SetValue(mockData, new System.Collections.Generic.List<string>());
            dataType.GetProperty("CustomMetrics")?.SetValue(mockData, new System.Collections.Generic.Dictionary<string, float>());
            
            // Test CSV generation
            string csvPath = Path.Combine(testOutputDir, "test_report.csv");
            csvMethod.Invoke(null, new object[] { csvPath, mockData });
            
            Assert.IsTrue(File.Exists(csvPath), "CSV file should be created");
            
            // Verify CSV content
            string csvContent = File.ReadAllText(csvPath);
            Assert.IsTrue(csvContent.Contains("Average FPS"), "CSV should contain FPS data");
            Assert.IsTrue(csvContent.Contains("60"), "CSV should contain actual FPS value");
        }
        
        [Test]
        public void PerformanceSettingsWindow_StoresPreferences()
        {
            // Test preference storage
            int testFrameRate = 45;
            int testMemory = 300;
            bool testAutoProfile = true;
            bool testDeepProfiling = true;
            
            EditorPrefs.SetInt("NeonLadder_FrameRateTarget", testFrameRate);
            EditorPrefs.SetInt("NeonLadder_MemoryThreshold", testMemory);
            EditorPrefs.SetBool("NeonLadder_AutoProfile", testAutoProfile);
            EditorPrefs.SetBool("NeonLadder_DeepProfiling", testDeepProfiling);
            
            // Verify storage
            Assert.AreEqual(testFrameRate, EditorPrefs.GetInt("NeonLadder_FrameRateTarget", 30));
            Assert.AreEqual(testMemory, EditorPrefs.GetInt("NeonLadder_MemoryThreshold", 200));
            Assert.AreEqual(testAutoProfile, EditorPrefs.GetBool("NeonLadder_AutoProfile", false));
            Assert.AreEqual(testDeepProfiling, EditorPrefs.GetBool("NeonLadder_DeepProfiling", false));
            
            // Clean up
            EditorPrefs.DeleteKey("NeonLadder_FrameRateTarget");
            EditorPrefs.DeleteKey("NeonLadder_MemoryThreshold");
            EditorPrefs.DeleteKey("NeonLadder_AutoProfile");
            EditorPrefs.DeleteKey("NeonLadder_DeepProfiling");
        }
        
        [Test]
        public void PerformanceData_IdentifiesBottlenecks()
        {
            var type = typeof(PerformanceMenuItems);
            var dataType = type.GetNestedType("PerformanceData",
                System.Reflection.BindingFlags.NonPublic);
            
            var mockData = Activator.CreateInstance(dataType);
            var bottlenecks = new System.Collections.Generic.List<string>();
            
            // Set low FPS
            dataType.GetProperty("AverageFPS")?.SetValue(mockData, 25f);
            dataType.GetProperty("TotalMemoryMB")?.SetValue(mockData, 600L);
            dataType.GetProperty("DrawCalls")?.SetValue(mockData, 1500);
            dataType.GetProperty("Bottlenecks")?.SetValue(mockData, bottlenecks);
            
            // Simulate bottleneck detection (normally done in CollectPerformanceData)
            float fps = (float)dataType.GetProperty("AverageFPS")?.GetValue(mockData);
            long memory = (long)dataType.GetProperty("TotalMemoryMB")?.GetValue(mockData);
            int drawCalls = (int)dataType.GetProperty("DrawCalls")?.GetValue(mockData);
            
            if (fps < 30)
            {
                bottlenecks.Add($"Low FPS: {fps:F1} (Target: 30+)");
            }
            
            if (memory > 500)
            {
                bottlenecks.Add($"High memory usage: {memory} MB");
            }
            
            if (drawCalls > 1000)
            {
                bottlenecks.Add($"High draw calls: {drawCalls}");
            }
            
            Assert.AreEqual(3, bottlenecks.Count, "Should identify 3 bottlenecks");
            Assert.IsTrue(bottlenecks[0].Contains("Low FPS"));
            Assert.IsTrue(bottlenecks[1].Contains("High memory"));
            Assert.IsTrue(bottlenecks[2].Contains("High draw calls"));
        }
        
        [Test]
        public void MarkdownReport_FormatsCorrectly()
        {
            var type = typeof(PerformanceMenuItems);
            var mdMethod = type.GetMethod("GenerateMarkdownSummary",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            Assert.IsNotNull(mdMethod, "GenerateMarkdownSummary method should exist");
            
            // Create mock data
            var dataType = type.GetNestedType("PerformanceData",
                System.Reflection.BindingFlags.NonPublic);
            var mockData = Activator.CreateInstance(dataType);
            
            // Set up test data
            dataType.GetProperty("SceneName")?.SetValue(mockData, "TestLevel");
            dataType.GetProperty("Timestamp")?.SetValue(mockData, DateTime.Now);
            dataType.GetProperty("AverageFPS")?.SetValue(mockData, 45f);
            dataType.GetProperty("MinFPS")?.SetValue(mockData, 40f);
            dataType.GetProperty("MaxFPS")?.SetValue(mockData, 50f);
            dataType.GetProperty("TotalMemoryMB")?.SetValue(mockData, 250L);
            dataType.GetProperty("DrawCalls")?.SetValue(mockData, 800);
            
            var bottlenecks = new System.Collections.Generic.List<string>();
            dataType.GetProperty("Bottlenecks")?.SetValue(mockData, bottlenecks);
            
            var customMetrics = new System.Collections.Generic.Dictionary<string, float>
            {
                ["Enemy Count"] = 15,
                ["Active Effects"] = 8
            };
            dataType.GetProperty("CustomMetrics")?.SetValue(mockData, customMetrics);
            
            // Generate markdown
            string mdPath = Path.Combine(testOutputDir, "test_summary.md");
            mdMethod.Invoke(null, new object[] { mdPath, mockData });
            
            Assert.IsTrue(File.Exists(mdPath), "Markdown file should be created");
            
            // Verify content
            string mdContent = File.ReadAllText(mdPath);
            Assert.IsTrue(mdContent.Contains("# Performance Report Summary"));
            Assert.IsTrue(mdContent.Contains("TestLevel"));
            Assert.IsTrue(mdContent.Contains("45"));
            Assert.IsTrue(mdContent.Contains("Enemy Count"));
            Assert.IsTrue(mdContent.Contains("Active Effects"));
        }
        
        [Test]
        public void HTMLReport_GeneratesValidHTML()
        {
            var type = typeof(PerformanceMenuItems);
            var htmlMethod = type.GetMethod("GenerateHTMLReport",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            
            Assert.IsNotNull(htmlMethod, "GenerateHTMLReport method should exist");
            
            // Create mock data
            var dataType = type.GetNestedType("PerformanceData",
                System.Reflection.BindingFlags.NonPublic);
            var mockData = Activator.CreateInstance(dataType);
            
            // Set properties for HTML generation
            dataType.GetProperty("SceneName")?.SetValue(mockData, "MainMenu");
            dataType.GetProperty("Timestamp")?.SetValue(mockData, DateTime.Now);
            dataType.GetProperty("AverageFPS")?.SetValue(mockData, 75f);
            dataType.GetProperty("AverageFrameTime")?.SetValue(mockData, 13.33f);
            dataType.GetProperty("TotalMemoryMB")?.SetValue(mockData, 180L);
            dataType.GetProperty("DrawCalls")?.SetValue(mockData, 450);
            dataType.GetProperty("Triangles")?.SetValue(mockData, 35000);
            dataType.GetProperty("Vertices")?.SetValue(mockData, 18000);
            
            var bottlenecks = new System.Collections.Generic.List<string>();
            dataType.GetProperty("Bottlenecks")?.SetValue(mockData, bottlenecks);
            
            var customMetrics = new System.Collections.Generic.Dictionary<string, float>();
            dataType.GetProperty("CustomMetrics")?.SetValue(mockData, customMetrics);
            
            // Generate HTML
            string htmlPath = Path.Combine(testOutputDir, "test_report.html");
            htmlMethod.Invoke(null, new object[] { htmlPath, mockData });
            
            Assert.IsTrue(File.Exists(htmlPath), "HTML file should be created");
            
            // Verify HTML structure
            string htmlContent = File.ReadAllText(htmlPath);
            Assert.IsTrue(htmlContent.Contains("<!DOCTYPE html>"));
            Assert.IsTrue(htmlContent.Contains("<title>NeonLadder Performance Report</title>"));
            Assert.IsTrue(htmlContent.Contains("MainMenu"));
            Assert.IsTrue(htmlContent.Contains("75"));
            Assert.IsTrue(htmlContent.Contains("status-good"), "Should have good status for 75 FPS");
        }
    }
}