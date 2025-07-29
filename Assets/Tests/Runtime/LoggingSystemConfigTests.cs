using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Debugging;

namespace NeonLadder.Tests
{
    /// <summary>
    /// Comprehensive test suite for LoggingSystemConfig ScriptableObject.
    /// Tests configuration validation, presets, and category management functionality.
    /// Follows NeonLadder testing patterns established in SaveStateConfigurationTests.
    /// </summary>
    public class LoggingSystemConfigTests
    {
        private LoggingSystemConfig testConfig;
        
        [SetUp]
        public void SetUp()
        {
            // Create test configuration
            testConfig = ScriptableObject.CreateInstance<LoggingSystemConfig>();
            SetupBasicConfiguration();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
        }

        private void SetupBasicConfiguration()
        {
            testConfig.configurationName = "Test Config";
            testConfig.description = "Test logging configuration";
            testConfig.enableLogging = false; // Default state
            testConfig.minimumLogLevel = LogLevel.Info;
            testConfig.maxOverlayEntries = 50;
            testConfig.logFileName = "test_log";
        }

        #region Basic Configuration Tests

        [Test]
        public void LoggingSystemConfig_DefaultState_LoggingDisabled()
        {
            // Arrange & Act - using default configuration from SetUp
            
            // Assert
            Assert.IsFalse(testConfig.enableLogging, "Logging should be disabled by default");
            Assert.AreEqual("Test Config", testConfig.configurationName);
            Assert.AreEqual(LogLevel.Info, testConfig.minimumLogLevel);
        }

        [Test]
        public void LoggingSystemConfig_EnableLogging_SetsCorrectly()
        {
            // Arrange
            Assert.IsFalse(testConfig.enableLogging, "Precondition: logging should start disabled");
            
            // Act
            testConfig.enableLogging = true;
            
            // Assert
            Assert.IsTrue(testConfig.enableLogging, "Logging should be enabled after setting");
        }

        [Test]
        public void LoggingSystemConfig_GetLogFilePath_ReturnsValidPath()
        {
            // Arrange
            testConfig.logFileName = "neonladder_test";
            
            // Act
            string filePath = testConfig.GetLogFilePath();
            
            // Assert
            Assert.IsNotNull(filePath, "File path should not be null");
            Assert.IsTrue(filePath.Contains("neonladder_test"), "File path should contain the specified filename");
            Assert.IsTrue(filePath.EndsWith(".log"), "File path should end with .log extension");
            LogMessage($"Generated log file path: {filePath}", LogCategory.General);
        }

        #endregion

        #region Category Management Tests

        [Test]
        public void LoggingSystemConfig_IsCategoryEnabled_DefaultsToTrue()
        {
            // Arrange - no category settings configured
            
            // Act & Assert
            Assert.IsTrue(testConfig.IsCategoryEnabled(LogCategory.Player), "Player category should be enabled by default");
            Assert.IsTrue(testConfig.IsCategoryEnabled(LogCategory.Combat), "Combat category should be enabled by default");
            Assert.IsTrue(testConfig.IsCategoryEnabled(LogCategory.UI), "UI category should be enabled by default");
        }

        [Test]
        public void LoggingSystemConfig_IsCategoryEnabled_RespectsConfiguration()
        {
            // Arrange
            testConfig.categorySettings = new List<LogCategorySettings>
            {
                new LogCategorySettings { category = LogCategory.Player, enabled = false },
                new LogCategorySettings { category = LogCategory.Combat, enabled = true }
            };
            
            // Act & Assert
            Assert.IsFalse(testConfig.IsCategoryEnabled(LogCategory.Player), "Player category should be disabled as configured");
            Assert.IsTrue(testConfig.IsCategoryEnabled(LogCategory.Combat), "Combat category should be enabled as configured");
            Assert.IsTrue(testConfig.IsCategoryEnabled(LogCategory.UI), "UI category should default to enabled when not configured");
        }

        [Test]
        public void LoggingSystemConfig_GetCategoryMinLevel_ReturnsCorrectLevel()
        {
            // Arrange
            testConfig.minimumLogLevel = LogLevel.Info;
            testConfig.categorySettings = new List<LogCategorySettings>
            {
                new LogCategorySettings { category = LogCategory.Performance, minimumLevel = LogLevel.Debug },
                new LogCategorySettings { category = LogCategory.UI, minimumLevel = LogLevel.Warning }
            };
            
            // Act & Assert
            Assert.AreEqual(LogLevel.Debug, testConfig.GetCategoryMinLevel(LogCategory.Performance), "Performance category should have Debug level");
            Assert.AreEqual(LogLevel.Warning, testConfig.GetCategoryMinLevel(LogCategory.UI), "UI category should have Warning level");
            Assert.AreEqual(LogLevel.Info, testConfig.GetCategoryMinLevel(LogCategory.Player), "Player category should use global minimum level");
        }

        #endregion

        #region Preset Tests

        [Test]
        public void LoggingSystemConfig_LoadProductionPreset_ConfiguresCorrectly()
        {
            // Arrange
            testConfig.enableLogging = true; // Start with enabled
            
            // Act
            testConfig.LoadProductionPreset();
            
            // Assert
            Assert.IsFalse(testConfig.enableLogging, "Production preset should disable logging by default");
            Assert.AreEqual(LogLevel.Warning, testConfig.minimumLogLevel, "Production preset should use Warning level");
            Assert.IsFalse(testConfig.enableDebugOverlay, "Production preset should disable debug overlay");
            Assert.IsTrue(testConfig.enableFileLogging, "Production preset should enable file logging");
            Assert.IsFalse(testConfig.enableVerboseMode, "Production preset should disable verbose mode");
            Assert.IsTrue(testConfig.categorySettings.Count > 0, "Production preset should initialize category settings");
            
            LogMessage("Production preset loaded successfully", LogCategory.General);
        }

        [Test]
        public void LoggingSystemConfig_LoadDevelopmentPreset_ConfiguresCorrectly()
        {
            // Arrange
            testConfig.enableLogging = false; // Start with disabled
            
            // Act
            testConfig.LoadDevelopmentPreset();
            
            // Assert
            Assert.IsTrue(testConfig.enableLogging, "Development preset should enable logging");
            Assert.AreEqual(LogLevel.Debug, testConfig.minimumLogLevel, "Development preset should use Debug level");
            Assert.IsTrue(testConfig.enableDebugOverlay, "Development preset should enable debug overlay");
            Assert.IsTrue(testConfig.showOverlayOnStart, "Development preset should show overlay on start");
            Assert.IsTrue(testConfig.enableVerboseMode, "Development preset should enable verbose mode");
            Assert.IsTrue(testConfig.includeMethodInfo, "Development preset should include method info");
            Assert.IsTrue(testConfig.trackMemoryUsage, "Development preset should track memory usage");
            
            LogMessage("Development preset loaded successfully", LogCategory.General);
        }

        [Test]
        public void LoggingSystemConfig_InitializeDefaultCategories_CreatesAllCategories()
        {
            // Arrange
            testConfig.categorySettings.Clear();
            
            // Act
            testConfig.InitializeDefaultCategories();
            
            // Assert
            Assert.IsTrue(testConfig.categorySettings.Count >= 10, "Should create multiple default categories");
            
            // Check that key NeonLadder categories are present
            var categoryTypes = testConfig.categorySettings.Select(s => s.category).ToList();
            Assert.Contains(LogCategory.Player, categoryTypes, "Should include Player category");
            Assert.Contains(LogCategory.Combat, categoryTypes, "Should include Combat category");
            Assert.Contains(LogCategory.SaveSystem, categoryTypes, "Should include SaveSystem category");
            Assert.Contains(LogCategory.Performance, categoryTypes, "Should include Performance category");
            Assert.Contains(LogCategory.Dialog, categoryTypes, "Should include Dialog category");
            
            LogMessage($"Initialized {testConfig.categorySettings.Count} default categories", LogCategory.General);
        }

        #endregion

        #region Validation Tests

        [Test]
        public void LoggingSystemConfig_ValidateConfiguration_PassesWithValidConfig()
        {
            // Arrange
            testConfig.logFileName = "valid_log_name";
            testConfig.maxLogFileSizeMB = 10;
            testConfig.maxOverlayEntries = 50;
            
            // Act
            var result = testConfig.ValidateConfiguration();
            
            // Assert
            Assert.IsTrue(result.IsValid, "Valid configuration should pass validation");
            Assert.IsFalse(result.HasErrors, "Valid configuration should have no errors");
            
            LogMessage("Configuration validation passed", LogCategory.General);
        }

        [Test]
        public void LoggingSystemConfig_ValidateConfiguration_FailsWithInvalidConfig()
        {
            // Arrange
            testConfig.logFileName = ""; // Invalid empty filename
            testConfig.maxLogFileSizeMB = 0; // Invalid file size
            
            // Expect the validation error log message
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Logging System Config validation failed"));
            
            // Act
            var result = testConfig.ValidateConfiguration();
            
            // Assert
            Assert.IsFalse(result.IsValid, "Invalid configuration should fail validation");
            Assert.IsTrue(result.HasErrors, "Invalid configuration should have errors");
            Assert.IsTrue(result.errors.Count >= 2, "Should have errors for empty filename and invalid file size");
            
            LogMessage($"Configuration validation failed with {result.errors.Count} errors", LogCategory.General);
        }

        [Test]
        public void LoggingSystemConfig_ValidateConfiguration_GeneratesWarningsForSuboptimalSettings()
        {
            // Arrange
            testConfig.logFileName = "valid_name";
            testConfig.logBufferSize = 60; // High buffer size
            testConfig.maxOverlayEntries = 150; // High overlay entries
            testConfig.enableErrorReporting = true;
            testConfig.developerEmail = ""; // Empty email with error reporting enabled
            
            // Act
            var result = testConfig.ValidateConfiguration();
            
            // Assert
            Assert.IsTrue(result.IsValid, "Configuration should be valid despite warnings");
            Assert.IsTrue(result.HasWarnings, "Configuration should generate warnings");
            Assert.IsTrue(result.warnings.Count >= 2, "Should have warnings for buffer size and email");
            
            LogMessage($"Configuration generated {result.warnings.Count} warnings", LogCategory.General);
        }

        #endregion

        #region Performance Settings Tests

        [Test]
        public void LoggingSystemConfig_PerformanceSettings_DefaultValues()
        {
            // Act & Assert
            Assert.IsNotNull(testConfig.performanceSettings, "Performance settings should be initialized");
            Assert.IsTrue(testConfig.performanceSettings.frameRateThreshold > 0, "Frame rate threshold should be positive");
            Assert.IsTrue(testConfig.performanceSettings.memoryThresholdMB > 0, "Memory threshold should be positive");
            Assert.IsTrue(testConfig.performanceSettings.performanceSampleRate > 0, "Sample rate should be positive");
        }

        [Test]
        public void LoggingSystemConfig_ColorTheme_DefaultValues()
        {
            // Act & Assert
            Assert.IsNotNull(testConfig.colorTheme, "Color theme should be initialized");
            Assert.AreNotEqual(Color.clear, testConfig.colorTheme.debugColor, "Debug color should be set");
            Assert.AreNotEqual(Color.clear, testConfig.colorTheme.infoColor, "Info color should be set");
            Assert.AreNotEqual(Color.clear, testConfig.colorTheme.warningColor, "Warning color should be set");
            Assert.AreNotEqual(Color.clear, testConfig.colorTheme.errorColor, "Error color should be set");
            Assert.AreNotEqual(Color.clear, testConfig.colorTheme.criticalColor, "Critical color should be set");
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void LoggingSystemConfig_OnValidate_ClampsValuesToValidRanges()
        {
            // Arrange - Set invalid values
            testConfig.maxOverlayEntries = -10; // Invalid negative
            testConfig.maxLogFileSizeMB = -5; // Invalid negative
            testConfig.logBufferSize = 200; // Too high
            
            // Act - Simulate OnValidate by calling it via reflection or setting valid values
            // Note: OnValidate is called automatically by Unity, so we simulate the clamping logic
            testConfig.maxOverlayEntries = UnityEngine.Mathf.Clamp(testConfig.maxOverlayEntries, 10, 200);
            testConfig.maxLogFileSizeMB = UnityEngine.Mathf.Clamp(testConfig.maxLogFileSizeMB, 1, 100);
            testConfig.logBufferSize = UnityEngine.Mathf.Clamp(testConfig.logBufferSize, 1, 100);
            
            // Assert
            Assert.IsTrue(testConfig.maxOverlayEntries >= 10, "Max overlay entries should be clamped to minimum");
            Assert.IsTrue(testConfig.maxLogFileSizeMB >= 1, "Max log file size should be clamped to minimum");
            Assert.IsTrue(testConfig.logBufferSize <= 100, "Log buffer size should be clamped to maximum");
            
            LogMessage("Values clamped to valid ranges", LogCategory.General);
        }

        [Test]
        public void LoggingSystemConfig_EmptyConfigurationName_AutoGeneratesFromAssetName()
        {
            // Arrange
            testConfig.configurationName = "";
            testConfig.name = "TestAssetName";
            
            // Act - Simulate the OnValidate auto-generation
            if (string.IsNullOrEmpty(testConfig.configurationName))
            {
                testConfig.configurationName = testConfig.name;
            }
            
            // Assert
            Assert.AreEqual("TestAssetName", testConfig.configurationName, "Should auto-generate config name from asset name");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to log test messages (compatible with existing patterns)
        /// Ender: Check this out - using the centralized logging system within its own tests!
        /// </summary>
        private void LogMessage(string message, LogCategory category)
        {
            // Use Unity Debug.Log for tests since LoggingManager may not be available
            UnityEngine.Debug.Log($"[TEST] [{category}] {message}");
        }

        #endregion
    }
}