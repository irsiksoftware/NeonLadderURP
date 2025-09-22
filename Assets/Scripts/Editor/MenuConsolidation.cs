using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Consolidated menu structure for NeonLadder project
    /// All menu items should be under "NeonLadder/" for consistency
    /// </summary>
    public static class MenuConsolidation
    {
        // Menu paths for organization
        public const string ROOT = "NeonLadder/";

        // Main categories
        public const string BUILD_DEPLOY = ROOT + "Build & Deploy/";
        public const string PROCEDURAL = ROOT + "Procedural/";
        public const string TESTING = ROOT + "Testing/";
        public const string DEBUG = ROOT + "Debug/";
        public const string UPGRADE_SYSTEM = ROOT + "Upgrade System/";
        public const string SAVES = ROOT + "Saves/";
        public const string PERFORMANCE = ROOT + "Performance/";
        public const string INPUT = ROOT + "Input System/";
        public const string DIALOGUE = ROOT + "Dialogue/";
        public const string TOOLS = ROOT + "Tools/";
        public const string EXAMPLES = ROOT + "Examples/";

        // Tool subcategories
        public const string TOOLS_RENAME = TOOLS + "Rename Helpers/";
        public const string TOOLS_ANIMATION = TOOLS + "Animation/";
        public const string TOOLS_SCENE = TOOLS + "Scene Management/";
        public const string LOADING = ROOT + "Loading/";
        public const string BUILD = ROOT + "Build/";

        /// <summary>
        /// Shows the current menu organization
        /// </summary>
        [MenuItem(ROOT + "About Menu Organization", priority = 1000)]
        public static void ShowMenuOrganization()
        {
            string message = @"NeonLadder Menu Organization:

Main Categories:
• Build - Quick build commands for Development and Release
• Build & Deploy - Build profiles, deployment, shader fixes
• Loading - Loading screen generation and 3D navigation setup
• Procedural - Scene generation, path visualization
• Testing - Test runners, analysis tools
• Debug - Logging system, debug utilities
• Upgrade System - Upgrade designer, item creation
• Saves - Save system command center
• Performance - Profiling, performance analysis
• Input System - Controller mapping
• Dialogue - Dialogue system tools
• Tools - Various utility tools organized by category
• Examples - Example item creation

All menu items are now consolidated under 'NeonLadder/'
Unity Build menu items remain separate for convention";

            EditorUtility.DisplayDialog("NeonLadder Menu Organization", message, "OK");
        }

        /// <summary>
        /// Menu item to open Build Profile Manager from NeonLadder menu
        /// </summary>
        [MenuItem(BUILD_DEPLOY + "Open Build Profile Manager", priority = 10)]
        public static void OpenBuildProfileManager()
        {
            NeonLadder.BuildSystem.BuildProfileWindow.ShowWindow();
        }

        /// <summary>
        /// Quick access to loading screen generation
        /// </summary>
        [MenuItem(LOADING + "Quick Generate Loading Content", priority = 1)]
        public static void QuickGenerateLoadingContent()
        {
            // Calls the existing loading screen generator
            var generatorType = System.Type.GetType("NeonLadder.Editor.LoadingScreenContentGenerator");
            var method = generatorType?.GetMethod("GenerateLoadingScreenContent");
            method?.Invoke(null, null);
        }

        /// <summary>
        /// Build Profile Manager access
        /// </summary>
        [MenuItem(BUILD + "Build Profile Manager", priority = 1)]
        public static void OpenBuildProfileManagerFromBuild()
        {
            NeonLadder.BuildSystem.BuildProfileWindow.ShowWindow();
        }

        /// <summary>
        /// Create default build configurations
        /// </summary>
        [MenuItem(BUILD + "Create Default Configurations", priority = 2)]
        public static void CreateDefaultConfigurations()
        {
            NeonLadder.BuildSystem.BuildProfileManager.CreateDefaultConfigurationsMenu();
        }
    }
}