using UnityEngine;

namespace NeonLadder.Debugging
{
    /// <summary>
    /// NeonLadder custom debugger - avoids namespace conflicts with UnityEngine.Debug
    /// Provides centralized logging with category support and performance optimization
    /// </summary>
    public static class Debugger
    {
        /// <summary>
        /// Log a general info message
        /// </summary>
        public static void Log(string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.Log(message, context);
            }
            else
            {
                UnityEngine.Debug.Log($"[NL] {message}", context);
            }
        }

        /// <summary>
        /// Log a warning message
        /// </summary>
        public static void LogWarning(string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogWarning(message, context);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[NL] {message}", context);
            }
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        public static void LogError(string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogError(message, context);
            }
            else
            {
                UnityEngine.Debug.LogError($"[NL] {message}", context);
            }
        }

        /// <summary>
        /// Log with specific category
        /// </summary>
        public static void LogInformation(LogCategory category, string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogInfo(category, message, context);
            }
            else
            {
                UnityEngine.Debug.Log($"[NL-{category}] {message}", context);
            }
        }

        /// <summary>
        /// Log warning with specific category
        /// </summary>
        public static void LogWarning(LogCategory category, string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogWarning(category, message, context);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[NL-{category}] {message}", context);
            }
        }

        /// <summary>
        /// Log error with specific category
        /// </summary>
        public static void LogError(LogCategory category, string message, Object context = null)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogError(category, message, context);
            }
            else
            {
                UnityEngine.Debug.LogError($"[NL-{category}] {message}", context);
            }
        }

        /// <summary>
        /// Draw a debug line
        /// </summary>
        public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration = 0.0f)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.DrawLine(start, end, color, duration);
            }
            else
            {
                UnityEngine.Debug.DrawLine(start, end, color, duration);
            }
        }

        /// <summary>
        /// Log performance metrics
        /// </summary>
        public static void LogPerformance(string metric, float value, string unit = "")
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogPerformance(metric, value, unit);
            }
            else
            {
                UnityEngine.Debug.Log($"[NL-PERF] {metric}: {value:F2}{unit}");
            }
        }

        /// <summary>
        /// Log with timer (compatible with TimedLogger)
        /// </summary>
        public static void LogTimed(string message, float intervalInSeconds)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogTimed(LogCategory.General, message, intervalInSeconds);
            }
            else
            {
                UnityEngine.Debug.Log($"[NL-TIMED] {message}");
            }
        }

        /// <summary>
        /// Log with timer and category
        /// </summary>
        public static void LogTimed(LogCategory category, string message, float intervalInSeconds)
        {
            if (LoggingManager.Instance != null)
            {
                LoggingManager.LogTimed(category, message, intervalInSeconds);
            }
            else
            {
                UnityEngine.Debug.Log($"[NL-TIMED-{category}] {message}");
            }
        }
    }
}