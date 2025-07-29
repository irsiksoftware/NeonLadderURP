using UnityEngine;
using System.Collections.Generic;
using NeonLadder.Debug;

namespace NeonLadder.Utilities
{
    /// <summary>
    /// Legacy TimedLogger class - now integrated with centralized logging system
    /// Maintained for backward compatibility with existing code
    /// </summary>
    public static class TimedLogger
    {
        private static Dictionary<string, float> logTimers = new Dictionary<string, float>();

        /// <summary>
        /// Log a message with timed interval (legacy method)
        /// Now uses centralized logging system when available
        /// </summary>
        public static void Log(string message, float intervalInSeconds)
        {
            if (!logTimers.ContainsKey(message))
            {
                logTimers[message] = Time.time + intervalInSeconds;
                LogMessage(message, true);
            }
            else if (Time.time >= logTimers[message])
            {
                LogMessage(message, false);
                logTimers[message] = Time.time + intervalInSeconds;
            }
        }

        /// <summary>
        /// Log a message with category and timed interval (enhanced method)
        /// </summary>
        public static void Log(LogCategory category, string message, float intervalInSeconds)
        {
            string key = $"{category}_{message}";
            if (!logTimers.ContainsKey(key))
            {
                logTimers[key] = Time.time + intervalInSeconds;
                LogMessage(category, message, true);
            }
            else if (Time.time >= logTimers[key])
            {
                LogMessage(category, message, false);
                logTimers[key] = Time.time + intervalInSeconds;
            }
        }

        /// <summary>
        /// Clear all timed log intervals (useful for scene transitions)
        /// </summary>
        public static void ClearAllTimers()
        {
            int timerCount = logTimers.Count;
            logTimers.Clear();
            LogMessage(LogCategory.General, $"🧹 Cleared {timerCount} timed log intervals");
        }

        /// <summary>
        /// Check if a message should be logged based on timing (without actually logging)
        /// </summary>
        public static bool ShouldLog(string message, float intervalInSeconds)
        {
            if (!logTimers.ContainsKey(message))
            {
                logTimers[message] = Time.time + intervalInSeconds;
                return true;
            }
            else if (Time.time >= logTimers[message])
            {
                logTimers[message] = Time.time + intervalInSeconds;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the number of active timed log entries
        /// </summary>
        public static int GetActiveTimerCount()
        {
            return logTimers.Count;
        }

        private static void LogMessage(string message, bool isFirstLog = false)
        {
            // Try to use centralized logging system if available
            if (LoggingManager.Instance != null)
            {
                string timedMessage = isFirstLog ? $"[TIMED] {message}" : $"[TIMED] {message}";
                LoggingManager.LogInfo(LogCategory.General, timedMessage);
            }
            else
            {
                // Fallback to Unity Debug.Log
                NLDebug.Log($"[TIMED] {message}");
            }
        }

        private static void LogMessage(LogCategory category, string message, bool isFirstLog = false)
        {
            // Try to use centralized logging system if available
            if (LoggingManager.Instance != null)
            {
                string timedMessage = isFirstLog ? $"[TIMED] {message}" : $"[TIMED] {message}";
                LoggingManager.LogInfo(category, timedMessage);
            }
            else
            {
                // Fallback to Unity Debug.Log
                NLDebug.Log($"[TIMED] [{category}] {message}");
            }
        }
    }
}