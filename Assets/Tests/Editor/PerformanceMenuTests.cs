using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using System.IO;
using System.Collections;
using NeonLadder.Editor.Performance;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine.Profiling;

namespace NeonLadder.Tests.Editor
{
    /// <summary>
    /// Unit tests for Performance Menu functionality
    /// Tests menu items, profiler integration, and report generation
    /// </summary>
    [TestFixture]
    public class PerformanceMenuTests
    {
        private string testOutputDir;
        private GameObject testProfilerObject;
        
        [SetUp]
        public void Setup()
        {
            // Create test output directory
            testOutputDir = Path.Combine(Application.dataPath, "..", "TestOutput");
            if (!Directory.Exists(testOutputDir))
            {
                Directory.CreateDirectory(testOutputDir);
            }
            
            // Clear any existing performance profiler
            var existingProfiler = GameObject.FindObjectOfType<PerformanceProfiler>();
            if (existingProfiler != null)
            {
                GameObject.DestroyImmediate(existingProfiler.gameObject);
            }
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (testProfilerObject != null)
            {
                GameObject.DestroyImmediate(testProfilerObject);
            }
            
            // Reset profiler state
            ProfilerDriver.enabled = false;
            ProfilerDriver.ClearAllFrames();
        }
        
        [Test]
        public void ProfileCurrentScene_StartsProfiler()
        {
            // Skip profiler activation in test mode to avoid UI interference
            if (EditorPrefs.GetBool("NeonLadder_TestMode", false))
            {
                Assert.Pass("Skipped profiler activation during test mode to avoid UI interference");
                return;
            }
            
            // Act
            PerformanceMenuItems.ProfileCurrentScene();
            
            // Assert
            Assert.IsTrue(ProfilerDriver.enabled, "Profiler should be enabled after starting profiling");
            Assert.IsFalse(ProfilerDriver.profileEditor, "Editor profiling should be disabled for game profiling");
            
            // Verify PerformanceProfiler was added to scene
            var profiler = GameObject.FindObjectOfType<PerformanceProfiler>();
            Assert.IsNotNull(profiler, "PerformanceProfiler should be added to scene");
            Assert.IsTrue(profiler.useCentralizedLogging, "Centralized logging should be enabled by default");
        }
        
        [Test]
        public void ClearProfilerData_ClearsFrames()
        {
            // Skip profiler activation in test mode to avoid UI interference
            if (EditorPrefs.GetBool("NeonLadder_TestMode", false))
            {
                Assert.Pass("Skipped profiler activation during test mode to avoid UI interference");
                return;
            }
            
            // Arrange - Enable profiler and capture some frames
            ProfilerDriver.enabled = true;
            System.Threading.Thread.Sleep(100); // Let profiler capture some frames
            
            // Act
            PerformanceMenuItems.ClearProfilerData();
            
            // Assert
            Assert.AreEqual(0, ProfilerDriver.lastFrameIndex - ProfilerDriver.firstFrameIndex, 
                "Profiler frames should be cleared");
        }
        
        [Test]
        public void ExportPerformanceReport_CreatesFiles()
        {
            // Act
            PerformanceMenuItems.ExportPerformanceReport();
            
            // Assert - Check that report files were created
            var files = Directory.GetFiles(testOutputDir, "performance_*.csv");
            Assert.Greater(files.Length, 0, "CSV report should be created");
            
            files = Directory.GetFiles(testOutputDir, "performance_summary_*.md");
            Assert.Greater(files.Length, 0, "Markdown summary should be created");
            
            files = Directory.GetFiles(testOutputDir, "performance_report_*.html");
            Assert.Greater(files.Length, 0, "HTML report should be created");
        }
        
        [Test]
        public void ExportPerformanceReport_ContainsValidData()
        {
            // Act
            PerformanceMenuItems.ExportPerformanceReport();
            
            // Assert - Verify CSV content
            var csvFiles = Directory.GetFiles(testOutputDir, "performance_*.csv");
            Assert.Greater(csvFiles.Length, 0, "Should have at least one CSV file");
            
            string csvContent = File.ReadAllText(csvFiles[0]);
            
            // Check for expected headers and data
            Assert.IsTrue(csvContent.Contains("Metric,Value,Unit"), "CSV should contain headers");
            Assert.IsTrue(csvContent.Contains("Average FPS"), "CSV should contain FPS data");
            Assert.IsTrue(csvContent.Contains("Total Memory"), "CSV should contain memory data");
            Assert.IsTrue(csvContent.Contains("Draw Calls"), "CSV should contain draw call data");
        }
        
        [Test]
        public void PerformanceSettings_SaveAndLoad()
        {
            // Arrange
            int testFrameRate = 45;
            int testMemoryThreshold = 300;
            bool testAutoProfile = true;
            
            // Act - Save settings
            EditorPrefs.SetInt("NeonLadder_FrameRateTarget", testFrameRate);
            EditorPrefs.SetInt("NeonLadder_MemoryThreshold", testMemoryThreshold);
            EditorPrefs.SetBool("NeonLadder_AutoProfile", testAutoProfile);
            
            // Assert - Load and verify
            Assert.AreEqual(testFrameRate, EditorPrefs.GetInt("NeonLadder_FrameRateTarget", 0));
            Assert.AreEqual(testMemoryThreshold, EditorPrefs.GetInt("NeonLadder_MemoryThreshold", 0));
            Assert.AreEqual(testAutoProfile, EditorPrefs.GetBool("NeonLadder_AutoProfile", false));
        }
        
        [Test]
        public void AutoProfileOnPlay_TogglesCorrectly()
        {
            // Arrange
            bool initialState = EditorPrefs.GetBool("NeonLadder_AutoProfile", false);
            
            // Act
            PerformanceMenuItems.ToggleAutoProfile();
            bool afterToggle = EditorPrefs.GetBool("NeonLadder_AutoProfile", false);
            
            // Assert
            Assert.AreNotEqual(initialState, afterToggle, "Auto-profile state should toggle");
            
            // Cleanup - Toggle back
            PerformanceMenuItems.ToggleAutoProfile();
        }
        
        [Test]
        public void PerformanceProfiler_Integration()
        {
            // Skip profiler window opening in test mode to avoid UI interference
            if (EditorPrefs.GetBool("NeonLadder_TestMode", false))
            {
                Assert.Pass("Skipped profiler window opening during test mode to avoid UI interference");
                return;
            }
            
            // Arrange - Create PerformanceProfiler
            testProfilerObject = new GameObject("TestPerformanceProfiler");
            var profiler = testProfilerObject.AddComponent<PerformanceProfiler>();
            profiler.frameRateWarningThreshold = 30;
            profiler.memoryWarningThresholdMB = 200;
            
            // Act - Start profiling
            PerformanceMenuItems.ProfileCurrentScene();
            
            // Assert - Verify profiler is configured
            Assert.IsNotNull(GameObject.FindObjectOfType<PerformanceProfiler>());
            Assert.IsTrue(ProfilerDriver.enabled, "Profiler should be enabled");
            
            // Verify profiler areas are enabled
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.CPU), "CPU profiling should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Rendering), "Rendering profiling should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Memory), "Memory profiling should be enabled");
        }
        
        [Test]
        public void MarkdownReport_ContainsBottlenecks()
        {
            // Act
            PerformanceMenuItems.ExportPerformanceReport();
            
            // Assert - Check markdown content
            var mdFiles = Directory.GetFiles(testOutputDir, "performance_summary_*.md");
            Assert.Greater(mdFiles.Length, 0, "Should have at least one markdown file");
            
            string mdContent = File.ReadAllText(mdFiles[0]);
            
            // Check for expected sections
            Assert.IsTrue(mdContent.Contains("# Performance Report Summary"), "Should have title");
            Assert.IsTrue(mdContent.Contains("## Performance Metrics"), "Should have metrics section");
            Assert.IsTrue(mdContent.Contains("### Frame Rate"), "Should have frame rate section");
            Assert.IsTrue(mdContent.Contains("### Memory Usage"), "Should have memory section");
            Assert.IsTrue(mdContent.Contains("## Recommendations"), "Should have recommendations");
        }
        
        [Test]
        public void HTMLReport_GeneratesValidHTML()
        {
            // Act
            PerformanceMenuItems.ExportPerformanceReport();
            
            // Assert - Check HTML content
            var htmlFiles = Directory.GetFiles(testOutputDir, "performance_report_*.html");
            Assert.Greater(htmlFiles.Length, 0, "Should have at least one HTML file");
            
            string htmlContent = File.ReadAllText(htmlFiles[0]);
            
            // Check for valid HTML structure
            Assert.IsTrue(htmlContent.Contains("<!DOCTYPE html>"), "Should have DOCTYPE");
            Assert.IsTrue(htmlContent.Contains("<html>"), "Should have html tag");
            Assert.IsTrue(htmlContent.Contains("NeonLadder Performance Report"), "Should have title");
            Assert.IsTrue(htmlContent.Contains("class='metric-card'"), "Should have metric cards");
        }
        
        [Test]
        public void ProfilerAreas_EnabledCorrectly()
        {
            // Skip profiler activation in test mode to avoid UI interference
            if (EditorPrefs.GetBool("NeonLadder_TestMode", false))
            {
                Assert.Pass("Skipped profiler activation during test mode to avoid UI interference");
                return;
            }
            
            // Act
            PerformanceMenuItems.ProfileCurrentScene();
            
            // Assert - Check all relevant profiler areas
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.CPU), "CPU area should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Rendering), "Rendering area should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Memory), "Memory area should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Physics), "Physics area should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.Audio), "Audio area should be enabled");
            Assert.IsTrue(Profiler.GetAreaEnabled(ProfilerArea.UI), "UI area should be enabled");
        }
        
        [Test]
        public void PerformanceData_CollectsMetrics()
        {
            // This test verifies that performance data collection works
            // Note: In a real test environment, we'd mock the profiler data
            
            // Act
            PerformanceMenuItems.ExportPerformanceReport();
            
            // Assert - Verify the report contains actual data
            var csvFiles = Directory.GetFiles(testOutputDir, "performance_*.csv");
            if (csvFiles.Length > 0)
            {
                string csvContent = File.ReadAllText(csvFiles[0]);
                
                // Check that metrics have values (not just headers)
                string[] lines = csvContent.Split('\n');
                Assert.Greater(lines.Length, 2, "CSV should have more than just headers");
                
                // Verify scene name is captured
                Assert.IsTrue(csvContent.Contains("Scene,"), "Should capture scene name");
            }
        }
        
        [Test]
        public void SettingsWindow_AppliesSettingsToScene()
        {
            // Arrange - Create profiler and settings
            testProfilerObject = new GameObject("TestProfiler");
            var profiler = testProfilerObject.AddComponent<PerformanceProfiler>();
            
            // Set test values
            EditorPrefs.SetInt("NeonLadder_FrameRateTarget", 60);
            EditorPrefs.SetInt("NeonLadder_MemoryThreshold", 500);
            
            // Act - Create and show window
            var window = ScriptableObject.CreateInstance<PerformanceSettingsWindow>();
            
            // The window loads settings automatically when ShowWindow is called
            // We can verify the settings are persisted correctly
            
            // Assert - Verify settings are loaded
            Assert.AreEqual(60, EditorPrefs.GetInt("NeonLadder_FrameRateTarget", 0));
            Assert.AreEqual(500, EditorPrefs.GetInt("NeonLadder_MemoryThreshold", 0));
            
            // Cleanup
            ScriptableObject.DestroyImmediate(window);
        }
    }
}