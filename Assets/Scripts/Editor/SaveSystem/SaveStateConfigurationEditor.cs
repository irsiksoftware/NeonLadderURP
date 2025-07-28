using UnityEngine;
using UnityEditor;
using NeonLadderURP.DataManagement;
using NeonLadderURP.DataManagement.Examples;
using System.IO;

namespace NeonLadderURP.Editor.SaveSystem
{
    /// <summary>
    /// Custom editor for SaveStateConfiguration with utility functions for creating example configurations.
    /// </summary>
    [CustomEditor(typeof(SaveStateConfiguration))]
    public class SaveStateConfigurationEditor : UnityEditor.Editor
    {
        private SaveStateConfiguration config;
        
        void OnEnable()
        {
            config = (SaveStateConfiguration)target;
        }
        
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Save System Actions", EditorStyles.boldLabel);
            
            // Action buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Create Save Data", GUILayout.Height(30)))
            {
                CreateSaveDataFromConfig();
            }
            
            if (GUILayout.Button("Load From Current Save", GUILayout.Height(30)))
            {
                LoadFromCurrentSave();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Apply to Session", GUILayout.Height(30)))
            {
                ApplyToCurrentSession();
            }
            
            if (GUILayout.Button("Export Save File", GUILayout.Height(30)))
            {
                ExportSaveFile();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Debug Information", EditorStyles.boldLabel);
            
            // Show save file information
            var saveInfo = EnhancedSaveSystem.GetSaveFileInfo();
            if (saveInfo != null)
            {
                EditorGUILayout.LabelField($"Save File: {Path.GetFileName(saveInfo.FilePath)}");
                EditorGUILayout.LabelField($"Size: {saveInfo.FileSizeFormatted}");
                EditorGUILayout.LabelField($"Last Modified: {saveInfo.LastModified:yyyy-MM-dd HH:mm:ss}");
                EditorGUILayout.LabelField($"Save Version: {saveInfo.SaveVersion}");
                EditorGUILayout.LabelField($"Play Time: {saveInfo.PlayTimeFormatted}");
                EditorGUILayout.LabelField($"Current Run: #{saveInfo.CurrentRun}");
            }
            else
            {
                EditorGUILayout.LabelField("No save file exists");
            }
            
            // Show save file path
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Save Location:", EditorStyles.miniLabel);
            EditorGUILayout.SelectableLabel(EnhancedSaveSystem.SaveFilePath, EditorStyles.textField, GUILayout.Height(16));
            
            if (GUILayout.Button("Open Save Directory"))
            {
                OpenSaveDirectory();
            }
        }
        
        private void CreateSaveDataFromConfig()
        {
            try
            {
                var saveData = config.CreateSaveData();
                Debug.Log($"[SaveStateConfigurationEditor] Created save data with version {saveData.saveVersion}");
                Debug.Log($"Player Level: {saveData.progression.playerLevel}, Meta Currency: {saveData.currencies.metaCurrency}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveStateConfigurationEditor] Failed to create save data: {ex.Message}");
            }
        }
        
        private void LoadFromCurrentSave()
        {
            try
            {
                if (EnhancedSaveSystem.SaveExists())
                {
                    var saveData = EnhancedSaveSystem.Load();
                    config.LoadFromSaveData(saveData);
                    EditorUtility.SetDirty(config);
                    Debug.Log("[SaveStateConfigurationEditor] Loaded configuration from current save file");
                }
                else
                {
                    Debug.LogWarning("[SaveStateConfigurationEditor] No save file exists to load from");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveStateConfigurationEditor] Failed to load from save: {ex.Message}");
            }
        }
        
        private void ApplyToCurrentSession()
        {
            try
            {
                config.ApplyToCurrentSession();
                Debug.Log("[SaveStateConfigurationEditor] Applied configuration to current session");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveStateConfigurationEditor] Failed to apply to session: {ex.Message}");
            }
        }
        
        private void ExportSaveFile()
        {
            string exportPath = EditorUtility.SaveFilePanel(
                "Export Save File",
                "",
                "NeonLadderSave_Export.json",
                "json"
            );
            
            if (!string.IsNullOrEmpty(exportPath))
            {
                if (EnhancedSaveSystem.ExportSave(exportPath))
                {
                    Debug.Log($"[SaveStateConfigurationEditor] Save exported to: {exportPath}");
                }
                else
                {
                    Debug.LogError("[SaveStateConfigurationEditor] Failed to export save file");
                }
            }
        }
        
        private void OpenSaveDirectory()
        {
            string directory = Path.GetDirectoryName(EnhancedSaveSystem.SaveFilePath);
            if (Directory.Exists(directory))
            {
                EditorUtility.RevealInFinder(directory);
            }
            else
            {
                Debug.LogWarning($"[SaveStateConfigurationEditor] Save directory does not exist: {directory}");
            }
        }
    }
    
    /// <summary>
    /// Menu items for creating example save configurations
    /// </summary>
    public static class SaveConfigurationMenuItems
    {
        private const string MenuRoot = "Assets/Create/NeonLadder/Save Configurations/";
        private const string AssetPath = "Assets/Settings/SaveConfigurations/";
        
        [MenuItem(MenuRoot + "New Player Configuration")]
        public static void CreateNewPlayerConfig()
        {
            CreateConfigAsset("NewPlayerConfig", ExampleSaveConfigurations.CreateNewPlayerConfig());
        }
        
        [MenuItem(MenuRoot + "Mid-Game Configuration")]
        public static void CreateMidGameConfig()
        {
            CreateConfigAsset("MidGameConfig", ExampleSaveConfigurations.CreateMidGameConfig());
        }
        
        [MenuItem(MenuRoot + "End-Game Configuration")]
        public static void CreateEndGameConfig()
        {
            CreateConfigAsset("EndGameConfig", ExampleSaveConfigurations.CreateEndGameConfig());
        }
        
        [MenuItem(MenuRoot + "Testing Configuration")]
        public static void CreateTestingConfig()
        {
            CreateConfigAsset("TestingConfig", ExampleSaveConfigurations.CreateTestingConfig());
        }
        
        [MenuItem(MenuRoot + "Regression Test Configuration")]
        public static void CreateRegressionTestConfig()
        {
            CreateConfigAsset("RegressionTestConfig", ExampleSaveConfigurations.CreateRegressionTestConfig());
        }
        
        private static void CreateConfigAsset(string fileName, SaveStateConfiguration config)
        {
            // Ensure directory exists
            if (!Directory.Exists(AssetPath))
            {
                Directory.CreateDirectory(AssetPath);
            }
            
            string assetPath = $"{AssetPath}{fileName}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
            
            AssetDatabase.CreateAsset(config, assetPath);
            AssetDatabase.SaveAssets();
            
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = config;
            
            Debug.Log($"[SaveConfigurationMenuItems] Created save configuration: {assetPath}");
        }
    }
}