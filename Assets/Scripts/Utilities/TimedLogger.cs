using UnityEngine;
using System.Collections.Generic;

namespace NeonLadder.Utilities
{
    public static class TimedLogger
    {
        private static Dictionary<string, float> logTimers = new Dictionary<string, float>();

        public static void Log(string message, float intervalInSeconds)
        {
            if (!logTimers.ContainsKey(message))
            {
                logTimers[message] = Time.time + intervalInSeconds;
                Debug.Log(message);
            }
            else if (Time.time >= logTimers[message])
            {
                Debug.Log(message);
                logTimers[message] = Time.time + intervalInSeconds;
            }
        }
    }
}