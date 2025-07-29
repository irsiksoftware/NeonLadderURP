using UnityEngine;
using UnityEditor;
using System.IO;

namespace NeonLadder.Debug.Editor
{
    /// <summary>
    /// Unity Editor utilities for setting up the NeonLadder Logging System
    /// Provides menu items and setup helpers for easy configuration
    /// </summary>
    public static class LoggingSystemSetup
    {
        private const string CONFIG_PATH = "Assets/Resources/Default Logging Config.asset";
        private const string RESOURCES_PATH = "Assets/Resources";

        [MenuItem("NeonLadder/Debug/Create Logging System Config", priority = 100)]
        public static void CreateLoggingSystemConfig()
        {
            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder(RESOURCES_PATH))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Check if config already exists
            LoggingSystemConfig existingConfig = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            if (existingConfig != null)
            {
                Selection.activeObject = existingConfig;
                EditorGUIUtility.PingObject(existingConfig);
                UnityEngine.Debug.Log("✅ Logging System Config already exists and has been selected.");
                return;
            }

            // Create new config
            LoggingSystemConfig newConfig = ScriptableObject.CreateInstance<LoggingSystemConfig>();
            newConfig.configurationName = "NeonLadder Default Config";
            newConfig.description = "Default logging configuration for NeonLadder project";
            newConfig.enableLogging = false; // Disabled by default
            
            // Initialize with development preset for easy setup
            newConfig.LoadDevelopmentPreset();
            newConfig.enableLogging = false; // Override the preset to keep it disabled

            // Save the asset
            AssetDatabase.CreateAsset(newConfig, CONFIG_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the new config
            Selection.activeObject = newConfig;
            EditorGUIUtility.PingObject(newConfig);

            UnityEngine.Debug.Log($"✅ Created Logging System Config at: {CONFIG_PATH}");
            UnityEngine.Debug.Log("📝 To enable logging, toggle 'Enable Logging' in the inspector.");
        }

        [MenuItem("NeonLadder/Debug/Setup Complete Logging System", priority = 101)]
        public static void SetupCompleteLoggingSystem()
        {
            // Create config if it doesn't exist
            CreateLoggingSystemConfig();

            // Find or create LoggingManager in scene
            LoggingManager existingManager = Object.FindObjectOfType<LoggingManager>();
            if (existingManager == null)
            {
                GameObject managerObject = new GameObject("Logging Manager");
                LoggingManager manager = managerObject.AddComponent<LoggingManager>();
                
                // Assign the config
                LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
                if (config != null)
                {
                    manager.config = config;
                }

                Selection.activeGameObject = managerObject;
                UnityEngine.Debug.Log("✅ Created LoggingManager GameObject in scene.");
            }
            else
            {
                Selection.activeGameObject = existingManager.gameObject;
                UnityEngine.Debug.Log("✅ LoggingManager already exists in scene and has been selected.");
            }

            UnityEngine.Debug.Log("🚀 Logging System setup complete!");
            UnityEngine.Debug.Log("💡 To enable logging: Select the config and toggle 'Enable Logging' checkbox.");
        }

        [MenuItem("NeonLadder/Debug/Quick Enable Logging", priority = 102)]
        public static void QuickEnableLogging()
        {
            LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            if (config == null)
            {
                UnityEngine.Debug.LogWarning("❌ No Logging System Config found. Run 'Create Logging System Config' first.");
                return;
            }

            config.enableLogging = true;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            UnityEngine.Debug.Log("✅ Logging system ENABLED! All log messages will now be processed.");
        }

        [MenuItem("NeonLadder/Debug/Quick Disable Logging", priority = 103)]
        public static void QuickDisableLogging()
        {
            LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            if (config == null)
            {
                UnityEngine.Debug.LogWarning("❌ No Logging System Config found. Run 'Create Logging System Config' first.");
                return;
            }

            config.enableLogging = false;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            UnityEngine.Debug.Log("📴 Logging system DISABLED! No log messages will be processed.");
        }

        [MenuItem("NeonLadder/Debug/Show Current Logging Status", priority = 150)]
        public static void ShowCurrentLoggingStatus()
        {
            LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            LoggingManager manager = Object.FindObjectOfType<LoggingManager>();

            UnityEngine.Debug.Log("=== NEONLADDER LOGGING SYSTEM STATUS ===");
            
            if (config == null)
            {
                UnityEngine.Debug.Log("❌ Config: NOT FOUND - Run 'Create Logging System Config'");
            }
            else
            {
                UnityEngine.Debug.Log($"✅ Config: {config.configurationName}");
                UnityEngine.Debug.Log($"🔘 Logging Enabled: {(config.enableLogging ? "YES" : "NO")}");
                UnityEngine.Debug.Log($"📊 Min Level: {config.minimumLogLevel}");
                UnityEngine.Debug.Log($"🎮 Debug Overlay: {(config.enableDebugOverlay ? "YES" : "NO")}");
                UnityEngine.Debug.Log($"📁 File Logging: {(config.enableFileLogging ? "YES" : "NO")}");
            }

            if (manager == null)
            {
                UnityEngine.Debug.Log("❌ Manager: NOT IN SCENE - Run 'Setup Complete Logging System'");
            }
            else
            {
                UnityEngine.Debug.Log("✅ Manager: Found in scene");
            }

            UnityEngine.Debug.Log("=========================================");
        }

        [MenuItem("NeonLadder/Debug/Open Logging Documentation", priority = 200)]
        public static void OpenLoggingDocumentation()
        {
            string docText = @"
=== NEONLADDER CENTRALIZED LOGGING SYSTEM ===

QUICK START:
1. NeonLadder → Debug → Setup Complete Logging System
2. NeonLadder → Debug → Quick Enable Logging
3. Use LoggingManager.LogInfo() instead of Debug.Log()

USAGE EXAMPLES:
LoggingManager.LogInfo(LogCategory.Player, ""Player spawned"");
LoggingManager.LogWarning(LogCategory.Combat, ""Low health"");
LoggingManager.LogError(LogCategory.SaveSystem, ""Save failed"");
LoggingManager.LogPerformance(""FPS"", 60.0f, "" fps"");

FEATURES:
• Master on/off toggle (disabled by default)
• 15 log categories (Player, Combat, UI, etc.)
• 5 log levels (Debug, Info, Warning, Error, Critical)
• In-game debug overlay (F12 to toggle)
• File logging with rotation
• Performance monitoring integration
• Backward compatible with TimedLogger

CATEGORIES:
General, Player, Enemy, Combat, UI, Audio, SaveSystem, 
Progression, Performance, Networking, AI, Physics, 
Animation, ProceduralGeneration, Steam, Dialog

CONTROLS:
• F12: Toggle debug overlay in-game
• Menu items for quick enable/disable
• ScriptableObject config for detailed settings
";

            UnityEngine.Debug.Log(docText);
        }

        // Validation methods for menu items
        [MenuItem("NeonLadder/Debug/Quick Enable Logging", validate = true)]
        public static bool ValidateQuickEnableLogging()
        {
            LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            return config != null && !config.enableLogging;
        }

        [MenuItem("NeonLadder/Debug/Quick Disable Logging", validate = true)]
        public static bool ValidateQuickDisableLogging()
        {
            LoggingSystemConfig config = AssetDatabase.LoadAssetAtPath<LoggingSystemConfig>(CONFIG_PATH);
            return config != null && config.enableLogging;
        }
    }
}