using UnityEditor;
using UnityEngine;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Quick helper for managing test mode settings
    /// Provides easy access to disable performance popups during development
    /// </summary>
    public static class TestModeHelper
    {
        private const string MENU_ROOT = "NeonLadder/Test Mode/";
        
        /// <summary>
        /// Quick toggle for test mode (disables performance popups)
        /// </summary>
        [MenuItem(MENU_ROOT + "Enable Test Mode (No Popups) %#t", false, 100)]
        public static void EnableTestMode()
        {
            EditorPrefs.SetBool("NeonLadder_TestMode", true);
            Debug.Log("[TestMode] ✅ Test mode ENABLED - Performance popups and auto-profiling disabled");
        }
        
        /// <summary>
        /// Disable test mode (restore normal behavior)
        /// </summary>
        [MenuItem(MENU_ROOT + "Disable Test Mode (Restore Popups) %#r", false, 101)]
        public static void DisableTestMode()
        {
            EditorPrefs.SetBool("NeonLadder_TestMode", false);
            Debug.Log("[TestMode] ⚠️ Test mode DISABLED - Performance popups and auto-profiling restored");
        }
        
        /// <summary>
        /// Check current test mode status
        /// </summary>
        [MenuItem(MENU_ROOT + "Check Test Mode Status", false, 200)]
        public static void CheckTestModeStatus()
        {
            bool isTestMode = EditorPrefs.GetBool("NeonLadder_TestMode", false);
            string status = isTestMode ? "ENABLED (popups disabled)" : "DISABLED (popups enabled)";
            
            Debug.Log($"[TestMode] Current status: {status}");
            
            if (isTestMode)
            {
                EditorUtility.DisplayDialog("Test Mode Status", 
                    "🔇 Test Mode is ENABLED\n\n" +
                    "Performance popups and auto-profiling are disabled.\n" +
                    "This is ideal for running tests or development work.", 
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Test Mode Status", 
                    "🔊 Test Mode is DISABLED\n\n" +
                    "Performance popups and auto-profiling are enabled.\n" +
                    "This is the normal interactive mode.", 
                    "OK");
            }
        }
        
        // Menu validation
        [MenuItem(MENU_ROOT + "Enable Test Mode (No Popups) %#t", true)]
        public static bool EnableTestModeValidate()
        {
            return !EditorPrefs.GetBool("NeonLadder_TestMode", false);
        }
        
        [MenuItem(MENU_ROOT + "Disable Test Mode (Restore Popups) %#r", true)]
        public static bool DisableTestModeValidate()
        {
            return EditorPrefs.GetBool("NeonLadder_TestMode", false);
        }
    }
}