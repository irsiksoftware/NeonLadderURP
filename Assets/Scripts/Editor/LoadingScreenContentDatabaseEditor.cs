using UnityEngine;
using UnityEditor;
using NeonLadder.UI;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Custom Inspector for LoadingScreenContentDatabase with regeneration functionality
    /// </summary>
    [CustomEditor(typeof(LoadingScreenContentDatabase))]
    public class LoadingScreenContentDatabaseEditor : UnityEditor.Editor
    {
        private LoadingScreenContentDatabase database;

        private void OnEnable()
        {
            database = (LoadingScreenContentDatabase)target;
        }

        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Database Management", EditorStyles.boldLabel);

            // Regenerate button with new database creation
            if (GUILayout.Button("Create New Database Copy", GUILayout.Height(30)))
            {
                CreateNewDatabaseCopy();
            }

            EditorGUILayout.Space();

            // Info display
            var allModels = database.GetAllModels();
            var enabledModels = database.GetEnabledModels();

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Database Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Models: {allModels.Count}");
            EditorGUILayout.LabelField($"Enabled Models: {enabledModels.Count}");
            EditorGUILayout.LabelField($"Disabled Models: {allModels.Count - enabledModels.Count}");
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Model breakdown by category
            if (allModels.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Models by Category", EditorStyles.boldLabel);

                var categories = new System.Collections.Generic.Dictionary<string, int>();
                foreach (var model in allModels)
                {
                    // Extract category from model name or prefab path
                    string category = GetModelCategory(model);
                    if (!categories.ContainsKey(category))
                        categories[category] = 0;
                    categories[category]++;
                }

                foreach (var kvp in categories)
                {
                    EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value} models");
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void CreateNewDatabaseCopy()
        {
            if (database == null)
            {
                Debug.LogError("[DatabaseEditor] No database reference");
                return;
            }

            // Save any pending changes to the current database first
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            // Find a unique name for the new database
            string baseName = "LoadingScreenContentDatabase";
            string resourcesPath = "Assets/Resources";
            string newName = baseName;
            int counter = 1;

            while (AssetDatabase.LoadAssetAtPath<LoadingScreenContentDatabase>($"{resourcesPath}/{newName}.asset") != null)
            {
                newName = $"{baseName}-{counter}";
                counter++;
            }

            // Create new database instance
            var newDatabase = ScriptableObject.CreateInstance<LoadingScreenContentDatabase>();

            // Copy current settings to new database
            var currentSettings = database.GetGenerationSettings();
            newDatabase.SaveGenerationSettings(currentSettings);

            var currentDefaults = database.GetStoredDefaults();
            newDatabase.SaveGenerationDefaults(currentDefaults);

            // Generate content with current settings
            var generatedModels = LoadingScreenContentGenerator.GenerateModelsFromPrefabsWithDefaults(
                currentSettings,
                ConvertFromStoredDefaults(currentDefaults)
            );

            // Add generated models to new database
            newDatabase.RegenerateAutoModels(generatedModels);

            // Save new database
            string newAssetPath = $"{resourcesPath}/{newName}.asset";
            AssetDatabase.CreateAsset(newDatabase, newAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[DatabaseEditor] Created new database copy: {newName} with {generatedModels.Count} models");

            // Select the new database in the project
            Selection.activeObject = newDatabase;
            EditorGUIUtility.PingObject(newDatabase);
        }

        private void RegenerateWithPreservedSettings()
        {
            if (database == null)
            {
                Debug.LogError("[DatabaseEditor] No database reference");
                return;
            }

            // Save any pending changes to the database FIRST
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            // Extract current settings from the database (now with saved changes)
            var preservedSettings = ExtractCurrentSettings();

            if (preservedSettings == null)
            {
                Debug.LogError("[DatabaseEditor] Failed to extract current settings");
                return;
            }

            // Show progress bar
            EditorUtility.DisplayProgressBar("Regenerating Database", "Preserving current settings...", 0.1f);

            try
            {
                // Generate new content with preserved settings
                var newModels = LoadingScreenContentGenerator.GenerateModelsFromPrefabsWithDefaults(
                    preservedSettings.generationSettings,
                    preservedSettings.generationDefaults
                );

                EditorUtility.DisplayProgressBar("Regenerating Database", "Applying preserved settings...", 0.7f);

                // Preserve any manual customizations from existing models
                newModels = PreserveManualCustomizations(newModels);

                EditorUtility.DisplayProgressBar("Regenerating Database", "Updating database...", 0.9f);

                // Update database with new models
                database.RegenerateAutoModels(newModels);

                // Save changes
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[DatabaseEditor] Successfully regenerated {newModels.Count} models with preserved settings");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private PreservedSettings ExtractCurrentSettings()
        {
            var preserved = new PreservedSettings();

            // Extract generation settings from database
            preserved.generationSettings = database.GetGenerationSettings();

            // Try to get stored defaults first, then fall back to pattern detection
            var storedDefaults = database.GetStoredDefaults();
            preserved.generationDefaults = ConvertFromStoredDefaults(storedDefaults);

            // If no stored defaults found, try to extract from model patterns
            if (!HasAnyOverrides(preserved.generationDefaults))
            {
                ExtractCategoryDefaults(preserved.generationDefaults);
            }

            return preserved;
        }

        private bool HasAnyOverrides(LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults defaults)
        {
            // Check for category-specific overrides
            bool hasCategoryOverrides = defaults.useMinorEnemyOverrides ||
                                       defaults.useMajorEnemyOverrides ||
                                       defaults.useBossOverrides ||
                                       defaults.useBossTransformationOverrides ||
                                       defaults.usePlayerOverrides ||
                                       defaults.useNPCOverrides;

            // Check for modified global defaults
            bool hasModifiedGlobals = defaults.displayScale != Vector3.one ||
                                     defaults.cameraDistanceOverride != 0f ||
                                     defaults.disableAnimator ||
                                     defaults.animatorParameterName != "animation" ||
                                     defaults.animationValue != 0;

            return hasCategoryOverrides || hasModifiedGlobals;
        }

        private void ExtractCategoryDefaults(LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults defaults)
        {
            var allModels = database.GetAllModels();

            // Fallback pattern detection from existing models
            foreach (var model in allModels)
            {
                string category = GetModelCategory(model);

                switch (category.ToLower())
                {
                    case "minor enemy":
                        if (!defaults.useMinorEnemyOverrides && HasNonDefaultSettings(model))
                        {
                            defaults.useMinorEnemyOverrides = true;
                            CopyModelToDefaults(model, defaults.minorEnemyDefaults);
                        }
                        break;

                    case "major enemy":
                        if (!defaults.useMajorEnemyOverrides && HasNonDefaultSettings(model))
                        {
                            defaults.useMajorEnemyOverrides = true;
                            CopyModelToDefaults(model, defaults.majorEnemyDefaults);
                        }
                        break;

                    case "boss":
                    case "boss transformation":
                        if (!defaults.useBossOverrides && HasNonDefaultSettings(model))
                        {
                            defaults.useBossOverrides = true;
                            CopyModelToDefaults(model, defaults.bossDefaults);
                        }
                        break;

                    case "hero":
                        if (!defaults.usePlayerOverrides && HasNonDefaultSettings(model))
                        {
                            defaults.usePlayerOverrides = true;
                            CopyModelToDefaults(model, defaults.playerDefaults);
                        }
                        break;

                    case "npc":
                        if (!defaults.useNPCOverrides && HasNonDefaultSettings(model))
                        {
                            defaults.useNPCOverrides = true;
                            CopyModelToDefaults(model, defaults.npcDefaults);
                        }
                        break;
                }
            }
        }

        private bool HasNonDefaultSettings(Loading3DModel model)
        {
            // Check if model has any non-default settings that suggest category overrides were used
            return model.displayScale != Vector3.one ||
                   model.positionOffset != Vector3.zero ||
                   model.cameraDistanceOverride != 0f ||
                   model.disableAnimator ||
                   model.animatorParameterName != "animation" ||
                   model.animationValue != -1;
        }

        private void CopyModelToDefaults(Loading3DModel model, LoadingScreenGenerationWindow.LoadingModelDefaults defaults)
        {
            defaults.displayScale = model.displayScale;
            defaults.positionOffset = model.positionOffset;
            defaults.cameraDistanceOverride = model.cameraDistanceOverride;
            defaults.disableAnimator = model.disableAnimator;
            defaults.animatorParameterName = model.animatorParameterName;
            defaults.animationValue = model.animationValue;
        }

        private string GetModelCategory(Loading3DModel model)
        {
            if (model.modelPrefab == null) return "Unknown";

            string path = AssetDatabase.GetAssetPath(model.modelPrefab);

            if (path.Contains("/Minor/")) return "Minor Enemy";
            if (path.Contains("/Major/")) return "Major Enemy";
            if (path.Contains("/Boss/Transformations/")) return "Boss Transformation";
            if (path.Contains("/Boss/")) return "Boss";
            if (path.Contains("/NPCs/")) return "NPC";
            if (path.Contains("/Players/")) return "Hero";

            return "Unknown";
        }

        private System.Collections.Generic.List<Loading3DModel> PreserveManualCustomizations(System.Collections.Generic.List<Loading3DModel> newModels)
        {
            var currentModels = database.GetAllModels();

            // Create lookup of existing models by prefab
            var existingLookup = new System.Collections.Generic.Dictionary<GameObject, Loading3DModel>();
            foreach (var model in currentModels)
            {
                if (model.modelPrefab != null)
                {
                    existingLookup[model.modelPrefab] = model;
                }
            }

            // Apply any manual customizations to new models
            for (int i = 0; i < newModels.Count; i++)
            {
                if (newModels[i].modelPrefab != null && existingLookup.ContainsKey(newModels[i].modelPrefab))
                {
                    var existingModel = existingLookup[newModels[i].modelPrefab];

                    // Check if this model had manual customizations (different from generated defaults)
                    if (HasManualCustomizations(existingModel, newModels[i]))
                    {
                        // Preserve the manual customizations
                        PreserveModelCustomizations(existingModel, newModels[i]);
                    }
                }
            }

            return newModels;
        }

        private bool HasManualCustomizations(Loading3DModel existing, Loading3DModel generated)
        {
            // Compare key properties to see if user made manual changes
            return existing.enabled != generated.enabled ||
                   existing.displayScale != generated.displayScale ||
                   existing.positionOffset != generated.positionOffset ||
                   existing.cameraDistanceOverride != generated.cameraDistanceOverride ||
                   existing.disableAnimator != generated.disableAnimator ||
                   existing.animationValue != generated.animationValue;
        }

        private void PreserveModelCustomizations(Loading3DModel source, Loading3DModel target)
        {
            target.enabled = source.enabled;
            target.displayScale = source.displayScale;
            target.positionOffset = source.positionOffset;
            target.cameraDistanceOverride = source.cameraDistanceOverride;
            target.disableAnimator = source.disableAnimator;
            target.animatorParameterName = source.animatorParameterName;
            target.animationValue = source.animationValue;

            // Preserve loading tips if they were customized
            if (source.loadingTips.Count > 0 && HasCustomTips(source))
            {
                target.loadingTips = new System.Collections.Generic.List<LoadingTip>(source.loadingTips);
            }
        }

        private bool HasCustomTips(Loading3DModel model)
        {
            // Check if loading tips appear to be manually customized vs auto-generated
            // This is a heuristic - could be improved with metadata
            foreach (var tip in model.loadingTips)
            {
                if (!string.IsNullOrEmpty(tip.title) && !tip.title.StartsWith("About "))
                {
                    return true; // Doesn't match auto-generated pattern
                }
            }
            return false;
        }

        private LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults ConvertFromStoredDefaults(StoredGenerationDefaults stored)
        {
            var editorDefaults = new LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults();

            // Convert global defaults
            editorDefaults.displayScale = stored.displayScale;
            editorDefaults.cameraDistanceOverride = stored.cameraDistanceOverride;
            editorDefaults.disableAnimator = stored.disableAnimator;
            editorDefaults.animatorParameterName = stored.animatorParameterName;
            editorDefaults.animationValue = stored.animationValue;

            // Convert category overrides
            editorDefaults.useMinorEnemyOverrides = stored.useMinorEnemyOverrides;
            if (stored.useMinorEnemyOverrides)
                ConvertStoredToEditor(stored.minorEnemyDefaults, editorDefaults.minorEnemyDefaults);

            editorDefaults.useMajorEnemyOverrides = stored.useMajorEnemyOverrides;
            if (stored.useMajorEnemyOverrides)
                ConvertStoredToEditor(stored.majorEnemyDefaults, editorDefaults.majorEnemyDefaults);

            editorDefaults.useBossOverrides = stored.useBossOverrides;
            if (stored.useBossOverrides)
                ConvertStoredToEditor(stored.bossDefaults, editorDefaults.bossDefaults);

            editorDefaults.useBossTransformationOverrides = stored.useBossTransformationOverrides;
            if (stored.useBossTransformationOverrides)
                ConvertStoredToEditor(stored.bossTransformationDefaults, editorDefaults.bossTransformationDefaults);

            editorDefaults.usePlayerOverrides = stored.usePlayerOverrides;
            if (stored.usePlayerOverrides)
                ConvertStoredToEditor(stored.playerDefaults, editorDefaults.playerDefaults);

            editorDefaults.useNPCOverrides = stored.useNPCOverrides;
            if (stored.useNPCOverrides)
                ConvertStoredToEditor(stored.npcDefaults, editorDefaults.npcDefaults);

            return editorDefaults;
        }

        private void ConvertStoredToEditor(StoredModelDefaults source, LoadingScreenGenerationWindow.LoadingModelDefaults target)
        {
            target.displayScale = source.displayScale;
            target.positionOffset = source.positionOffset;
            target.cameraDistanceOverride = source.cameraDistanceOverride;
            target.disableAnimator = source.disableAnimator;
            target.animatorParameterName = source.animatorParameterName;
            target.animationValue = source.animationValue;
        }

        private class PreservedSettings
        {
            public LoadingScreenGenerationSettings generationSettings;
            public LoadingScreenGenerationWindow.LoadingScreenGenerationDefaults generationDefaults;
        }
    }
}