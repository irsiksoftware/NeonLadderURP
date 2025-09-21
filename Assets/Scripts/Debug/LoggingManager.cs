using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace NeonLadder.Debugging
{
    /// <summary>
    /// Centralized logging manager for NeonLadder
    /// Provides unified interface to replace scattered Debug.Log usage throughout the project
    /// Integrates with existing TimedLogger and PerformanceProfiler systems
    /// </summary>
    public class LoggingManager : MonoBehaviour
    {
        private static LoggingManager _instance;
        public static LoggingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LoggingManager>();
                }
                return _instance;
            }
        }

        [Header("🔧 Configuration")]
        [Tooltip("Logging system configuration ScriptableObject")]
        public LoggingSystemConfig config;
        
        [Header("📊 Runtime Status")]
        [Tooltip("Current number of buffered log entries")]
        [SerializeField] private int bufferedLogCount;
        
        [Tooltip("Total log entries processed this session")]
        [SerializeField] private int totalLogCount;
        
        [Tooltip("Current log file size (MB)")]
        [SerializeField] private float currentLogFileSizeMB;

        // Internal logging system
        private readonly ConcurrentQueue<LogEntry> logBuffer = new ConcurrentQueue<LogEntry>();
        private readonly List<LogEntry> recentLogs = new List<LogEntry>();
        private float lastBufferFlush;
        private bool isWritingToFile;
        private StreamWriter logFileWriter;
        private string currentLogFilePath;

        // Unity console integration
        private readonly List<string> unityConsoleLogs = new List<string>();

        // Events for UI integration
        public static event Action<LogEntry> OnLogEntryAdded;
        public static event Action OnLogsCleared;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                Initialize();
            }
            else if (_instance != this)
            {
                // Another instance exists, disable this one
                enabled = false;
            }
        }

        private void Initialize()
        {
            // Load default config if none assigned
            if (config == null)
            {
                config = Resources.Load<LoggingSystemConfig>("Default Logging Config");
                if (config == null)
                {
                    UnityEngine.Debug.LogWarning("⚠️ No LoggingSystemConfig found. Creating default configuration.");
                    CreateDefaultConfig();
                }
            }

            // Early exit if logging is disabled
            if (!config.enableLogging)
            {
                UnityEngine.Debug.Log("📴 NeonLadder Logging System is DISABLED - toggle 'Enable Logging' in config to activate");
                return;
            }

            // Initialize file logging
            if (config.enableFileLogging)
            {
                InitializeFileLogging();
            }

            // Hook into Unity console if enabled
            if (config.includeUnityConsoleLogs)
            {
                Application.logMessageReceived += OnUnityLogMessageReceived;
            }

            // Log system startup
            LogInfo(LogCategory.General, "🚀 NeonLadder Logging System initialized");
            LogInfo(LogCategory.General, $"📋 Configuration: {config.configurationName}");
            LogInfo(LogCategory.General, $"📁 Log file: {(config.enableFileLogging ? currentLogFilePath : "Disabled")}");
        }

        private void Update()
        {
            // Skip all processing if logging is disabled
            if (config == null || !config.enableLogging)
                return;

            // Process log buffer periodically
            if (config.enableLogBuffering && (logBuffer.Count >= config.logBufferSize || 
                Time.time - lastBufferFlush >= config.maxBufferTime))
            {
                FlushLogBuffer();
            }

            // Update runtime stats
            bufferedLogCount = logBuffer.Count;
            if (config.enableFileLogging && File.Exists(currentLogFilePath))
            {
                currentLogFileSizeMB = new FileInfo(currentLogFilePath).Length / (1024f * 1024f);
                
                // Check if log rotation is needed
                if (currentLogFileSizeMB >= config.maxLogFileSizeMB)
                {
                    RotateLogFile();
                }
            }
        }

        private void OnDestroy()
        {
            FlushLogBuffer();
            logFileWriter?.Close();
            
            if (config != null && config.includeUnityConsoleLogs)
            {
                Application.logMessageReceived -= OnUnityLogMessageReceived;
            }
        }

        #region Public Logging Interface

        /// <summary>
        /// Log a general info message (compatible with existing codebase)
        /// </summary>
        public static void Log(string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Info, LogCategory.General, message, context);
        }

        /// <summary>
        /// Log a warning message (compatible with existing codebase)
        /// </summary>
        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Warning, LogCategory.General, message, context);
        }

        /// <summary>
        /// Log an error message (compatible with existing codebase)
        /// </summary>
        public static void LogError(string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Error, LogCategory.General, message, context);
        }

        /// <summary>
        /// Draw a debug line (for compatibility with debug drawing)
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f)
        {
            if (Instance.config != null && Instance.config.enableDebugDrawing)
            {
                UnityEngine.Debug.DrawLine(start, end, color, duration);
            }
        }

        /// <summary>
        /// Log a debug message
        /// </summary>
        public static void LogDebug(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Debug, category, message, context);
        }

        /// <summary>
        /// Log an info message
        /// </summary>
        public static void LogInfo(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Info, category, message, context);
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Warning, category, message, context);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Error, category, message, context);
        }

        /// <summary>
        /// Log a critical error message
        /// </summary>
        public static void LogCritical(LogCategory category, string message, UnityEngine.Object context = null)
        {
            Instance.Log(LogLevel.Critical, category, message, context);
        }

        /// <summary>
        /// Log with exception details
        /// </summary>
        public static void LogException(LogCategory category, Exception exception, string additionalMessage = "", UnityEngine.Object context = null)
        {
            string message = string.IsNullOrEmpty(additionalMessage) ? 
                exception.Message : $"{additionalMessage}: {exception.Message}";
            
            if (Instance.config.showStackTraces)
            {
                message += $"\nStack Trace:\n{exception.StackTrace}";
            }
            
            Instance.Log(LogLevel.Error, category, message, context);
        }

        /// <summary>
        /// Timed logging compatible with existing TimedLogger
        /// </summary>
        public static void LogTimed(LogCategory category, string message, float intervalInSeconds)
        {
            // Use a static dictionary to track timed messages per category
            string key = $"{category}_{message}";
            if (!TimedLogTracker.ShouldLog(key, intervalInSeconds))
                return;
                
            LogInfo(category, $"[TIMED] {message}");
        }

        /// <summary>
        /// Performance logging integration
        /// </summary>
        public static void LogPerformance(string metric, float value, string unit = "")
        {
            // Check if Instance exists before accessing it
            if (Instance == null || Instance.config == null)
            {
                return;
            }

            if (Instance.config.performanceSettings.enableProfilerIntegration)
            {
                LogDebug(LogCategory.Performance, $"📊 {metric}: {value:F2}{unit}");
            }
        }

        /// <summary>
        /// Clear all logged messages
        /// </summary>
        public static void ClearLogs()
        {
            Instance.recentLogs.Clear();
            OnLogsCleared?.Invoke();
        }

        /// <summary>
        /// Get recent log entries for UI display
        /// </summary>
        public static List<LogEntry> GetRecentLogs(int maxCount = -1)
        {
            var logs = Instance.recentLogs;
            if (maxCount > 0 && logs.Count > maxCount)
            {
                return logs.GetRange(logs.Count - maxCount, maxCount);
            }
            return new List<LogEntry>(logs);
        }

        #endregion

        #region Internal Logging Implementation

        private void Log(LogLevel level, LogCategory category, string message, UnityEngine.Object context = null)
        {
            // Check master logging toggle first
            if (config == null || !config.enableLogging)
                return;

            // Check if logging is enabled for this category and level
            if (!config.IsCategoryEnabled(category) || level < config.GetCategoryMinLevel(category))
                return;

            // Check global minimum level
            if (level < config.minimumLogLevel)
                return;

            // Create log entry
            var logEntry = new LogEntry
            {
                timestamp = DateTime.Now,
                level = level,
                category = category,
                message = message,
                context = context,
                frameCount = Time.frameCount
            };

            // Add method info if enabled (development builds only)
            if (config.includeMethodInfo && UnityEngine.Debug.isDebugBuild)
            {
                var stackTrace = new System.Diagnostics.StackTrace(2, true);
                if (stackTrace.FrameCount > 0)
                {
                    var frame = stackTrace.GetFrame(0);
                    logEntry.methodName = frame.GetMethod()?.Name;
                    logEntry.fileName = Path.GetFileName(frame.GetFileName());
                    logEntry.lineNumber = frame.GetFileLineNumber();
                }
            }

            // Add memory info if enabled
            if (config.trackMemoryUsage)
            {
                logEntry.memoryUsageMB = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            }

            // Store in recent logs
            recentLogs.Add(logEntry);
            if (recentLogs.Count > config.maxOverlayEntries * 2) // Keep more than display limit
            {
                recentLogs.RemoveAt(0);
            }

            // Add to buffer for file writing
            if (config.enableFileLogging)
            {
                logBuffer.Enqueue(logEntry);
            }

            // Also log to Unity console for development
            if (UnityEngine.Debug.isDebugBuild)
            {
                LogToUnityConsole(logEntry);
            }

            // Notify UI
            OnLogEntryAdded?.Invoke(logEntry);
            
            totalLogCount++;
        }

        private void LogToUnityConsole(LogEntry entry)
        {
            string formattedMessage = FormatLogMessage(entry);
            
            switch (entry.level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage, entry.context);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage, entry.context);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    UnityEngine.Debug.LogError(formattedMessage, entry.context);
                    break;
            }
        }

        private string FormatLogMessage(LogEntry entry)
        {
            string template = config.logMessageTemplate;
            
            // Check for category-specific format override
            var categoryOverride = config.categoryFormats.Find(f => f.category == entry.category);
            if (categoryOverride != null)
            {
                template = categoryOverride.formatTemplate;
            }

            // Replace template variables
            string formatted = template
                .Replace("{timestamp}", config.includeTimestamp ? entry.timestamp.ToString("HH:mm:ss.fff") : "")
                .Replace("{level}", entry.level.ToString().ToUpper())
                .Replace("{category}", entry.category.ToString())
                .Replace("{message}", entry.message)
                .Replace("{frame}", entry.frameCount.ToString());

            // Add method info if available
            if (!string.IsNullOrEmpty(entry.methodName))
            {
                formatted += $" [{entry.fileName}:{entry.lineNumber} in {entry.methodName}()]";
            }

            // Add memory info if available
            if (entry.memoryUsageMB > 0)
            {
                formatted += $" [Memory: {entry.memoryUsageMB:F1}MB]";
            }

            return formatted;
        }

        #endregion

        #region File Logging

        private void InitializeFileLogging()
        {
            try
            {
                currentLogFilePath = config.GetLogFilePath();
                
                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(currentLogFilePath));
                
                // Open log file for appending
                logFileWriter = new StreamWriter(currentLogFilePath, append: true, Encoding.UTF8);
                logFileWriter.AutoFlush = !config.enableLogBuffering;
                
                // Write session header
                logFileWriter.WriteLine($"\n=== NeonLadder Logging Session Started: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
                logFileWriter.WriteLine($"Unity Version: {Application.unityVersion}");
                logFileWriter.WriteLine($"Game Version: {Application.version}");
                logFileWriter.WriteLine($"Platform: {Application.platform}");
                logFileWriter.WriteLine($"Config: {config.configurationName}");
                logFileWriter.WriteLine("==========================================================\n");
                
                if (!config.enableLogBuffering)
                {
                    logFileWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to initialize file logging: {ex.Message}");
                config.enableFileLogging = false;
            }
        }

        private void FlushLogBuffer()
        {
            if (logFileWriter == null || logBuffer.IsEmpty)
                return;

            lastBufferFlush = Time.time;
            
            if (config.enableAsyncFileWriting)
            {
                FlushLogBufferAsync();
            }
            else
            {
                FlushLogBufferSync();
            }
        }

        private void FlushLogBufferSync()
        {
            while (logBuffer.TryDequeue(out LogEntry entry))
            {
                string formattedMessage = FormatLogMessage(entry);
                logFileWriter.WriteLine(formattedMessage);
            }
            logFileWriter.Flush();
        }

        private async void FlushLogBufferAsync()
        {
            if (isWritingToFile) return;
            
            isWritingToFile = true;
            
            var entriesToWrite = new List<LogEntry>();
            while (logBuffer.TryDequeue(out LogEntry entry))
            {
                entriesToWrite.Add(entry);
            }

            try
            {
                await Task.Run(() =>
                {
                    foreach (var entry in entriesToWrite)
                    {
                        string formattedMessage = FormatLogMessage(entry);
                        logFileWriter.WriteLine(formattedMessage);
                    }
                    logFileWriter.Flush();
                });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Async log writing failed: {ex.Message}");
            }
            finally
            {
                isWritingToFile = false;
            }
        }

        private void RotateLogFile()
        {
            try
            {
                logFileWriter?.Close();
                
                // Move existing log files
                for (int i = config.logFileRotationCount - 1; i >= 1; i--)
                {
                    string oldFile = currentLogFilePath.Replace(".log", $".{i}.log");
                    string newFile = currentLogFilePath.Replace(".log", $".{i + 1}.log");
                    
                    if (File.Exists(oldFile))
                    {
                        if (File.Exists(newFile))
                            File.Delete(newFile);
                        File.Move(oldFile, newFile);
                    }
                }
                
                // Archive current log
                string archiveFile = currentLogFilePath.Replace(".log", ".1.log");
                if (File.Exists(archiveFile))
                    File.Delete(archiveFile);
                File.Move(currentLogFilePath, archiveFile);
                
                // Create new log file
                InitializeFileLogging();
                
                LogInfo(LogCategory.General, $"📁 Log file rotated. Archived to: {Path.GetFileName(archiveFile)}");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Log file rotation failed: {ex.Message}");
            }
        }

        #endregion

        #region Unity Console Integration

        private void OnUnityLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (!config.includeUnityConsoleLogs) return;
            
            // Convert Unity LogType to our LogLevel
            LogLevel level = type switch
            {
                LogType.Log => LogLevel.Info,
                LogType.Warning => LogLevel.Warning,
                LogType.Error => LogLevel.Error,
                LogType.Exception => LogLevel.Critical,
                LogType.Assert => LogLevel.Error,
                _ => LogLevel.Info
            };

            // Create log entry for Unity console message
            var entry = new LogEntry
            {
                timestamp = DateTime.Now,
                level = level,
                category = LogCategory.General,
                message = $"[Unity Console] {logString}",
                frameCount = Time.frameCount
            };

            if (config.showStackTraces && !string.IsNullOrEmpty(stackTrace))
            {
                entry.message += $"\n{stackTrace}";
            }

            // Add to buffer
            if (config.enableFileLogging)
            {
                logBuffer.Enqueue(entry);
            }

            // Notify UI
            OnLogEntryAdded?.Invoke(entry);
        }

        #endregion

        #region Configuration Management

        private void CreateDefaultConfig()
        {
            config = ScriptableObject.CreateInstance<LoggingSystemConfig>();
            config.configurationName = "Runtime Default";
            config.description = "Auto-generated default configuration";
            config.enableLogging = false; // Disabled by default
            config.LoadProductionPreset(); // Use production preset as safe default
        }

        /// <summary>
        /// Reload configuration at runtime
        /// </summary>
        [ContextMenu("🔄 Reload Configuration")]
        public void ReloadConfiguration()
        {
            if (config != null)
            {
                LogInfo(LogCategory.General, $"🔄 Reloading logging configuration: {config.configurationName}");
                
                // Reinitialize file logging if settings changed
                if (config.enableFileLogging && logFileWriter == null)
                {
                    InitializeFileLogging();
                }
                else if (!config.enableFileLogging && logFileWriter != null)
                {
                    logFileWriter?.Close();
                    logFileWriter = null;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a single log entry
    /// </summary>
    [Serializable]
    public class LogEntry
    {
        public DateTime timestamp;
        public LogLevel level;
        public LogCategory category;
        public string message;
        public UnityEngine.Object context;
        public int frameCount;
        public string methodName;
        public string fileName;
        public int lineNumber;
        public float memoryUsageMB;
    }

    /// <summary>
    /// Helper class for timed logging functionality
    /// </summary>
    internal static class TimedLogTracker
    {
        private static readonly Dictionary<string, float> lastLogTimes = new Dictionary<string, float>();

        public static bool ShouldLog(string key, float intervalInSeconds)
        {
            if (!lastLogTimes.ContainsKey(key))
            {
                lastLogTimes[key] = Time.time;
                return true;
            }

            if (Time.time >= lastLogTimes[key] + intervalInSeconds)
            {
                lastLogTimes[key] = Time.time;
                return true;
            }

            return false;
        }
    }
}