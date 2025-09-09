using UnityEngine;
using System;
using System.Collections.Generic;

namespace NeonLadder.Debugging
{
    /// <summary>
    /// ScriptableObject configuration for the NeonLadder Centralized Logging System
    /// Following the established NeonLadder pattern (like DialogSystemConfig, SaveSystemConfig)
    /// Consolidates scattered Debug.Log usage throughout the project with unified management
    /// </summary>
    [CreateAssetMenu(fileName = "Logging System Config", menuName = "NeonLadder/Debug/Logging System Config")]
    public class LoggingSystemConfig : ScriptableObject
    {
        [Header("🔍 Logging System Configuration")]
        [Tooltip("Master toggle - Enable/disable ALL logging system functionality")]
        public bool enableLogging = false;
        
        [Tooltip("Name of this logging system configuration preset")]
        public string configurationName = "Default Logging Config";
        
        [TextArea(3, 5)]
        [Tooltip("Description of this logging configuration's purpose and settings")]
        public string description = "Centralized logging system for debugging, performance monitoring, and user feedback";

        [Header("📊 Log Level Settings")]
        [Tooltip("Minimum log level to display/record")]
        public LogLevel minimumLogLevel = LogLevel.Info;
        
        [Tooltip("Enable different log categories")]
        public List<LogCategorySettings> categorySettings = new List<LogCategorySettings>();
        
        [Tooltip("Show stack traces for errors and exceptions")]
        public bool showStackTraces = true;
        
        [Tooltip("Include timestamp in log messages")]
        public bool includeTimestamp = true;

        [Header("🎮 In-Game Debug Display")]
        [Tooltip("Enable in-game debug overlay UI")]
        public bool enableDebugOverlay = false;
        
        [Tooltip("Maximum number of log entries to display in overlay")]
        [Range(10, 100)]
        public int maxOverlayEntries = 50;
        
        [Tooltip("Auto-scroll to newest messages in overlay")]
        public bool autoScrollToNewest = true;
        
        [Tooltip("Allow filtering by log level in overlay")]
        public bool enableOverlayFiltering = true;
        
        [Tooltip("Display overlay on game start if enabled")]
        public bool showOverlayOnStart = false;

        [Header("📁 File-Based Logging")]
        [Tooltip("Save logs to persistent file")]
        public bool enableFileLogging = true;
        
        [Tooltip("Log file name (without extension)")]
        public string logFileName = "neonladder_debug";
        
        [Tooltip("Maximum log file size in MB before rotation")]
        [Range(1, 100)]
        public int maxLogFileSizeMB = 10;
        
        [Tooltip("Number of rotated log files to keep")]
        [Range(1, 10)]
        public int logFileRotationCount = 3;
        
        [Tooltip("Include Unity console logs in file")]
        public bool includeUnityConsoleLogs = true;

        [Header("⚡ Performance Settings")]
        [Tooltip("Buffer log messages to reduce I/O operations")]
        public bool enableLogBuffering = true;
        
        [Tooltip("Enable debug drawing (DrawLine, DrawRay, etc.)")]
        public bool enableDebugDrawing = true;
        
        [Tooltip("Number of log messages to buffer before writing")]
        [Range(1, 100)]
        public int logBufferSize = 20;
        
        [Tooltip("Maximum time to buffer logs (seconds)")]
        [Range(1f, 30f)]
        public float maxBufferTime = 5f;
        
        [Tooltip("Async file writing to prevent frame drops")]
        public bool enableAsyncFileWriting = true;

        [Header("🎯 Category-Specific Settings")]
        [Tooltip("Special handling for performance-critical logging")]
        public PerformanceLoggingSettings performanceSettings = new PerformanceLoggingSettings();
        
        [Tooltip("Integration with existing TimedLogger")]
        public bool integrateWithTimedLogger = true;
        
        [Tooltip("Enhanced PerformanceProfiler integration")]
        public bool enhancePerformanceProfiler = true;

        [Header("🚨 Error Handling")]
        [Tooltip("Send critical errors to external service (when implemented)")]
        public bool enableErrorReporting = false;
        
        [Tooltip("Create crash dumps for severe errors")]
        public bool enableCrashDumps = false;
        
        [Tooltip("Email address for critical error notifications (dev builds only)")]
        public string developerEmail = "";

        [Header("🎨 Display Formatting")]
        [Tooltip("Color coding for different log levels")]
        public LogColorTheme colorTheme = new LogColorTheme();
        
        [Tooltip("Formatting template for log messages")]
        [TextArea(2, 4)]
        public string logMessageTemplate = "[{timestamp}] [{level}] [{category}] {message}";
        
        [Tooltip("Custom formatting for different categories")]
        public List<CategoryFormatOverride> categoryFormats = new List<CategoryFormatOverride>();

        [Header("🧪 Development Features")]
        [Tooltip("Enable verbose debug mode (dev builds only)")]
        public bool enableVerboseMode = false;
        
        [Tooltip("Log method names and line numbers")]
        public bool includeMethodInfo = false;
        
        [Tooltip("Memory usage tracking in logs")]
        public bool trackMemoryUsage = false;

        /// <summary>
        /// Gets the full log file path with extension
        /// </summary>
        public string GetLogFilePath()
        {
            string basePath = Application.persistentDataPath;
            return System.IO.Path.Combine(basePath, logFileName + ".log");
        }

        /// <summary>
        /// Checks if a specific log category is enabled
        /// </summary>
        public bool IsCategoryEnabled(LogCategory category)
        {
            var setting = categorySettings.Find(s => s.category == category);
            return setting?.enabled ?? true; // Default to enabled if not found
        }

        /// <summary>
        /// Gets the minimum log level for a specific category
        /// </summary>
        public LogLevel GetCategoryMinLevel(LogCategory category)
        {
            var setting = categorySettings.Find(s => s.category == category);
            return setting?.minimumLevel ?? minimumLogLevel;
        }

        /// <summary>
        /// Validates the current logging configuration
        /// </summary>
        [ContextMenu("🔍 Validate Logging Configuration")]
        public ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(logFileName))
            {
                result.AddError("Log file name cannot be empty");
            }
            
            if (maxLogFileSizeMB < 1)
            {
                result.AddError("Log file size must be at least 1MB");
            }
            
            if (enableFileLogging && logBufferSize > 50)
            {
                result.AddWarning("Large log buffer size may cause memory issues");
            }
            
            if (enableDebugOverlay && maxOverlayEntries > 100)
            {
                result.AddWarning("High overlay entry count may impact UI performance");
            }
            
            if (enableErrorReporting && string.IsNullOrEmpty(developerEmail))
            {
                result.AddWarning("Error reporting enabled but no developer email specified");
            }

            // Validate category settings
            foreach (var category in categorySettings)
            {
                if (category.enabled && category.minimumLevel > LogLevel.Error)
                {
                    result.AddWarning($"Category {category.category} has very restrictive log level");
                }
            }

            // Log results
            if (result.HasErrors)
            {
                UnityEngine.Debug.LogError($"❌ Logging System Config validation failed:\n{result.GetSummary()}");
            }
            else if (result.HasWarnings)
            {
                UnityEngine.Debug.LogWarning($"⚠️ Logging System Config has warnings:\n{result.GetSummary()}");
            }
            else
            {
                UnityEngine.Debug.Log($"✅ Logging System Config validation passed: {configurationName}");
            }
            
            return result;
        }

        /// <summary>
        /// Creates a production-ready logging configuration
        /// </summary>
        [ContextMenu("🚀 Load Production Preset")]
        public void LoadProductionPreset()
        {
            configurationName = "Production (Optimized & Secure)";
            description = "Production logging with minimal overhead and essential error tracking";
            enableLogging = false; // Disabled by default in production
            minimumLogLevel = LogLevel.Warning;
            enableDebugOverlay = false;
            enableFileLogging = true;
            maxLogFileSizeMB = 5;
            logFileRotationCount = 2;
            enableLogBuffering = true;
            logBufferSize = 50;
            enableAsyncFileWriting = true;
            enableErrorReporting = true;
            enableCrashDumps = false;
            enableVerboseMode = false;
            includeMethodInfo = false;
            trackMemoryUsage = false;
            showStackTraces = false; // Reduce log size
            
            InitializeDefaultCategories();
            UnityEngine.Debug.Log("🚀 Loaded production preset for logging system");
        }

        /// <summary>
        /// Creates a development/debugging configuration
        /// </summary>
        [ContextMenu("🔧 Load Development Preset")]
        public void LoadDevelopmentPreset()
        {
            configurationName = "Development (Verbose & Interactive)";
            description = "Development logging with full debugging features and overlay";
            enableLogging = true; // Enabled for development
            minimumLogLevel = LogLevel.Debug;
            enableDebugOverlay = true;
            showOverlayOnStart = true;
            maxOverlayEntries = 100;
            enableFileLogging = true;
            maxLogFileSizeMB = 20;
            logFileRotationCount = 5;
            enableLogBuffering = false; // Immediate logging for debugging
            enableAsyncFileWriting = false;
            enableErrorReporting = false;
            enableCrashDumps = true;
            enableVerboseMode = true;
            includeMethodInfo = true;
            trackMemoryUsage = true;
            showStackTraces = true;
            
            InitializeDefaultCategories();
            UnityEngine.Debug.Log("🔧 Loaded development preset for logging system");
        }

        /// <summary>
        /// Initialize default category settings for NeonLadder systems
        /// </summary>
        [ContextMenu("🏗️ Initialize Default Categories")]
        public void InitializeDefaultCategories()
        {
            categorySettings.Clear();
            categorySettings.AddRange(new[]
            {
                new LogCategorySettings { category = LogCategory.Player, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Enemy, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Combat, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.UI, enabled = true, minimumLevel = LogLevel.Warning },
                new LogCategorySettings { category = LogCategory.Audio, enabled = true, minimumLevel = LogLevel.Warning },
                new LogCategorySettings { category = LogCategory.SaveSystem, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Progression, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Performance, enabled = true, minimumLevel = LogLevel.Debug },
                new LogCategorySettings { category = LogCategory.Networking, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.AI, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Packages, enabled = true, minimumLevel = LogLevel.Warning },
                new LogCategorySettings { category = LogCategory.Physics, enabled = true, minimumLevel = LogLevel.Warning },
                new LogCategorySettings { category = LogCategory.Animation, enabled = true, minimumLevel = LogLevel.Warning },
                new LogCategorySettings { category = LogCategory.ProceduralGeneration, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Steam, enabled = true, minimumLevel = LogLevel.Info },
                new LogCategorySettings { category = LogCategory.Dialog, enabled = true, minimumLevel = LogLevel.Info }
            });
            
            UnityEngine.Debug.Log("🏗️ Initialized default logging categories for NeonLadder systems");
        }

        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            maxOverlayEntries = Mathf.Clamp(maxOverlayEntries, 10, 200);
            maxLogFileSizeMB = Mathf.Clamp(maxLogFileSizeMB, 1, 100);
            logFileRotationCount = Mathf.Clamp(logFileRotationCount, 1, 10);
            logBufferSize = Mathf.Clamp(logBufferSize, 1, 100);
            maxBufferTime = Mathf.Clamp(maxBufferTime, 1f, 60f);
            
            // Auto-generate config name if empty
            if (string.IsNullOrEmpty(configurationName))
            {
                configurationName = name;
            }
        }
    }

    [Serializable]
    public enum LogLevel
    {
        Debug = 0,      // Detailed debugging information
        Info = 1,       // General information messages
        Warning = 2,    // Warning messages
        Error = 3,      // Error messages
        Critical = 4    // Critical errors that may crash the game
    }

    [Serializable]
    public enum LogCategory
    {
        General,
        Player,
        Enemy,
        Combat,
        UI,
        Audio,
        SaveSystem,
        Progression,
        Performance,
        Networking,
        AI,
        Packages,
        Physics,
        Animation,
        ProceduralGeneration,
        Steam,
        Dialog
    }

    [Serializable]
    public class LogCategorySettings
    {
        [Tooltip("The category this setting applies to")]
        public LogCategory category;
        
        [Tooltip("Enable logging for this category")]
        public bool enabled = true;
        
        [Tooltip("Minimum log level for this category")]
        public LogLevel minimumLevel = LogLevel.Info;
        
        [Tooltip("Custom color for this category in UI")]
        public Color categoryColor = Color.white;
    }

    [Serializable]
    public class PerformanceLoggingSettings
    {
        [Tooltip("Log frame rate drops below this threshold")]
        [Range(15, 60)]
        public int frameRateThreshold = 30;
        
        [Tooltip("Log memory allocations above this threshold (MB)")]
        [Range(1f, 100f)]
        public float memoryThresholdMB = 10f;
        
        [Tooltip("Sample performance data every N frames")]
        [Range(1, 300)]
        public int performanceSampleRate = 60;
        
        [Tooltip("Enable detailed Unity Profiler integration")]
        public bool enableProfilerIntegration = false;
    }

    [Serializable]
    public class LogColorTheme
    {
        [Tooltip("Color for debug messages")]
        public Color debugColor = Color.gray;
        
        [Tooltip("Color for info messages")]
        public Color infoColor = Color.white;
        
        [Tooltip("Color for warning messages")]
        public Color warningColor = Color.yellow;
        
        [Tooltip("Color for error messages")]
        public Color errorColor = Color.red;
        
        [Tooltip("Color for critical messages")]
        public Color criticalColor = new Color(1f, 0.5f, 0f); // Orange-red
    }

    [Serializable]
    public class CategoryFormatOverride
    {
        [Tooltip("Category this format applies to")]
        public LogCategory category;
        
        [Tooltip("Custom format template for this category")]
        [TextArea(2, 3)]
        public string formatTemplate = "[{timestamp}] [{level}] {message}";
    }

    [Serializable]
    public class ValidationResult
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        
        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;
        public bool IsValid => !HasErrors;
        
        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);
        
        public string GetSummary()
        {
            var summary = "";
            if (HasErrors)
            {
                summary += "Errors:\n" + string.Join("\n", errors) + "\n";
            }
            if (HasWarnings)
            {
                summary += "Warnings:\n" + string.Join("\n", warnings);
            }
            return summary.Trim();
        }
    }
}