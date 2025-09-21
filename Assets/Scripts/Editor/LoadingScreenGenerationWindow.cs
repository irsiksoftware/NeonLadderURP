using UnityEngine;
using UnityEditor;
using NeonLadder.UI;
using System.Collections.Generic;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Editor window for configuring loading screen content generation defaults
    /// </summary>
    public class LoadingScreenGenerationWindow : EditorWindow
    {
        private LoadingScreenContentDatabase database;
        private LoadingScreenGenerationSettings settings;
        private LoadingScreenGenerationDefaults defaults;

        // Window state
        private Vector2 scrollPosition;
        private bool showAdvancedSettings = false;

        [System.Serializable]
        public class LoadingScreenGenerationDefaults
        {
            [Header("Model Settings")]
            public Vector3 displayScale = Vector3.one;
            public float cameraDistanceOverride = 0f; // 0 = use default

            [Header("Animation Settings")]
            public bool disableAnimator = false;
            public string animatorParameterName = "animation";
            public int animationValue = 0; // Default to idle

            [Header("Category-Specific Overrides")]
            public bool useMinorEnemyOverrides = false;
            public LoadingModelDefaults minorEnemyDefaults = new LoadingModelDefaults();

            public bool useMajorEnemyOverrides = false;
            public LoadingModelDefaults majorEnemyDefaults = new LoadingModelDefaults();


            public bool useBossOverrides = false;
            public LoadingModelDefaults bossDefaults = new LoadingModelDefaults();

            public bool useBossTransformationOverrides = false;
            public LoadingModelDefaults bossTransformationDefaults = new LoadingModelDefaults();

            public bool usePlayerOverrides = false;
            public LoadingModelDefaults playerDefaults = new LoadingModelDefaults();

            public bool useNPCOverrides = false;
            public LoadingModelDefaults npcDefaults = new LoadingModelDefaults();
        }

        [System.Serializable]
        public class LoadingModelDefaults
        {
            [Header("Transform")]
            public Vector3 displayScale = Vector3.one;
            public Vector3 positionOffset = Vector3.zero;

            [Header("Camera")]
            public float cameraDistanceOverride = 0f;

            [Header("Animation")]
            public bool disableAnimator = false;
            public string animatorParameterName = "animation";
            public int animationValue = 0;
        }

        [MenuItem("Tools/NeonLadder/Generate Loading Screen Content (Advanced)")]
        public static void ShowWindow()
        {
            var window = GetWindow<LoadingScreenGenerationWindow>("Loading Screen Generator");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // Initialize with reasonable defaults
            if (defaults == null)
            {
                defaults = new LoadingScreenGenerationDefaults();

                // Set some sensible category-specific defaults
                defaults.minorEnemyDefaults.displayScale = Vector3.one * 0.8f; // Smaller enemies
                defaults.minorEnemyDefaults.cameraDistanceOverride = 4f;

                defaults.majorEnemyDefaults.displayScale = Vector3.one * 0.5f; // Major enemies at 50% size as requested
                defaults.majorEnemyDefaults.cameraDistanceOverride = 6f;

                defaults.bossDefaults.displayScale = Vector3.one * 1.5f; // Huge bosses
                defaults.bossDefaults.cameraDistanceOverride = 8f;
                defaults.bossDefaults.disableAnimator = true; // Bosses look imposing when still

                defaults.playerDefaults.displayScale = Vector3.one;
                defaults.playerDefaults.cameraDistanceOverride = 5f;
                defaults.playerDefaults.animationValue = 0; // Idle pose

                defaults.npcDefaults.displayScale = Vector3.one;
                defaults.npcDefaults.cameraDistanceOverride = 5f;
                defaults.npcDefaults.animationValue = 0; // Idle pose
            }

            // Load or find database
            database = LoadingScreenContentGenerator.FindOrCreateDatabase();
            if (database != null)
            {
                settings = database.GetGenerationSettings();
            }
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Loading Screen Content Generation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (database == null)
            {
                EditorGUILayout.HelpBox("No LoadingScreenContentDatabase found. One will be created when you generate content.", MessageType.Info);
                if (GUILayout.Button("Find/Create Database"))
                {
                    database = LoadingScreenContentGenerator.FindOrCreateDatabase();
                    if (database != null)
                    {
                        settings = database.GetGenerationSettings();
                    }
                }
                EditorGUILayout.EndScrollView();
                return;
            }

            // Content categories
            EditorGUILayout.LabelField("Content Categories", EditorStyles.boldLabel);
            settings.includeMinorEnemies = EditorGUILayout.Toggle("Include Minor Enemies", settings.includeMinorEnemies);
            settings.includeMajorEnemies = EditorGUILayout.Toggle("Include Major Enemies", settings.includeMajorEnemies);
            settings.includeBossEnemies = EditorGUILayout.Toggle("Include Boss Enemies", settings.includeBossEnemies);
            settings.includeBossTransformations = EditorGUILayout.Toggle("Include Boss Transformations", settings.includeBossTransformations);
            settings.includeNPCs = EditorGUILayout.Toggle("Include NPCs", settings.includeNPCs);
            settings.includePlayers = EditorGUILayout.Toggle("Include Players", settings.includePlayers);

            EditorGUILayout.Space();

            // Global defaults
            EditorGUILayout.LabelField("Global Defaults", EditorStyles.boldLabel);
            defaults.displayScale = EditorGUILayout.Vector3Field("Display Scale", defaults.displayScale);
            defaults.cameraDistanceOverride = EditorGUILayout.FloatField("Camera Distance Override", defaults.cameraDistanceOverride);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Defaults", EditorStyles.boldLabel);
            defaults.disableAnimator = EditorGUILayout.Toggle("Disable Animator", defaults.disableAnimator);
            if (!defaults.disableAnimator)
            {
                defaults.animatorParameterName = EditorGUILayout.TextField("Animator Parameter", defaults.animatorParameterName);
                defaults.animationValue = EditorGUILayout.IntField("Animation Value", defaults.animationValue);
            }

            EditorGUILayout.Space();

            // Advanced category-specific settings
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Category-Specific Overrides", true);
            if (showAdvancedSettings)
            {
                EditorGUI.indentLevel++;

                if (settings.includeMinorEnemies)
                {
                    DrawCategoryDefaults("Minor Enemies", ref defaults.useMinorEnemyOverrides, defaults.minorEnemyDefaults);
                }

                if (settings.includeMajorEnemies)
                {
                    DrawCategoryDefaults("Major Enemies", ref defaults.useMajorEnemyOverrides, defaults.majorEnemyDefaults);
                }

                if (settings.includeBossEnemies)
                {
                    DrawCategoryDefaults("Boss Enemies", ref defaults.useBossOverrides, defaults.bossDefaults);
                }

                if (settings.includeBossTransformations)
                {
                    DrawCategoryDefaults("Boss Transformations", ref defaults.useBossTransformationOverrides, defaults.bossTransformationDefaults);
                }

                if (settings.includePlayers)
                {
                    DrawCategoryDefaults("Players", ref defaults.usePlayerOverrides, defaults.playerDefaults);
                }

                if (settings.includeNPCs)
                {
                    DrawCategoryDefaults("NPCs", ref defaults.useNPCOverrides, defaults.npcDefaults);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Action buttons
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate with Defaults", GUILayout.Height(30)))
            {
                GenerateContent();
            }

            if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
            {
                defaults = new LoadingScreenGenerationDefaults();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Status info
            if (database != null)
            {
                var allModels = database.GetAllModels();
                EditorGUILayout.HelpBox($"Current database contains {allModels.Count} total models", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCategoryDefaults(string categoryName, ref bool useOverrides, LoadingModelDefaults categoryDefaults)
        {
            EditorGUILayout.BeginVertical("box");
            useOverrides = EditorGUILayout.Toggle($"Override {categoryName}", useOverrides);

            if (useOverrides)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Transform", EditorStyles.boldLabel);
                categoryDefaults.displayScale = EditorGUILayout.Vector3Field("Display Scale", categoryDefaults.displayScale);
                categoryDefaults.positionOffset = EditorGUILayout.Vector3Field("Position Offset", categoryDefaults.positionOffset);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Camera", EditorStyles.boldLabel);
                categoryDefaults.cameraDistanceOverride = EditorGUILayout.FloatField("Camera Distance", categoryDefaults.cameraDistanceOverride);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
                categoryDefaults.disableAnimator = EditorGUILayout.Toggle("Disable Animator", categoryDefaults.disableAnimator);

                if (!categoryDefaults.disableAnimator)
                {
                    categoryDefaults.animatorParameterName = EditorGUILayout.TextField("Animator Parameter", categoryDefaults.animatorParameterName);
                    categoryDefaults.animationValue = EditorGUILayout.IntField("Animation Value", categoryDefaults.animationValue);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        private void GenerateContent()
        {
            if (database == null)
            {
                Debug.LogError("[LoadingScreenGenerationWindow] No database available");
                return;
            }

            // Save the configuration to the database for future regeneration
            database.SaveGenerationSettings(settings);
            var storedDefaults = ConvertToStoredDefaults(defaults);
            database.SaveGenerationDefaults(storedDefaults);

            // Generate content with the configured defaults
            var generatedModels = LoadingScreenContentGenerator.GenerateModelsFromPrefabsWithDefaults(settings, defaults);

            // Update database
            database.RegenerateAutoModels(generatedModels);

            Debug.Log($"[LoadingScreenGenerationWindow] Generated {generatedModels.Count} loading screen models with custom defaults and saved configuration");

            // Save assets
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Close window
            Close();
        }

        private StoredGenerationDefaults ConvertToStoredDefaults(LoadingScreenGenerationDefaults editorDefaults)
        {
            var stored = new StoredGenerationDefaults();

            // Convert global defaults
            stored.displayScale = editorDefaults.displayScale;
            stored.cameraDistanceOverride = editorDefaults.cameraDistanceOverride;
            stored.disableAnimator = editorDefaults.disableAnimator;
            stored.animatorParameterName = editorDefaults.animatorParameterName;
            stored.animationValue = editorDefaults.animationValue;

            // Convert category overrides
            stored.useMinorEnemyOverrides = editorDefaults.useMinorEnemyOverrides;
            if (editorDefaults.useMinorEnemyOverrides)
                ConvertModelDefaults(editorDefaults.minorEnemyDefaults, stored.minorEnemyDefaults);

            stored.useMajorEnemyOverrides = editorDefaults.useMajorEnemyOverrides;
            if (editorDefaults.useMajorEnemyOverrides)
                ConvertModelDefaults(editorDefaults.majorEnemyDefaults, stored.majorEnemyDefaults);

            stored.useBossOverrides = editorDefaults.useBossOverrides;
            if (editorDefaults.useBossOverrides)
                ConvertModelDefaults(editorDefaults.bossDefaults, stored.bossDefaults);

            stored.useBossTransformationOverrides = editorDefaults.useBossTransformationOverrides;
            if (editorDefaults.useBossTransformationOverrides)
                ConvertModelDefaults(editorDefaults.bossTransformationDefaults, stored.bossTransformationDefaults);

            stored.usePlayerOverrides = editorDefaults.usePlayerOverrides;
            if (editorDefaults.usePlayerOverrides)
                ConvertModelDefaults(editorDefaults.playerDefaults, stored.playerDefaults);

            stored.useNPCOverrides = editorDefaults.useNPCOverrides;
            if (editorDefaults.useNPCOverrides)
                ConvertModelDefaults(editorDefaults.npcDefaults, stored.npcDefaults);

            return stored;
        }

        private void ConvertModelDefaults(LoadingModelDefaults source, StoredModelDefaults target)
        {
            target.displayScale = source.displayScale;
            target.positionOffset = source.positionOffset;
            target.cameraDistanceOverride = source.cameraDistanceOverride;
            target.disableAnimator = source.disableAnimator;
            target.animatorParameterName = source.animatorParameterName;
            target.animationValue = source.animationValue;
        }
    }
}