using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.DataManagement;
using NeonLadderURP.DataManagement;
using NeonLadder.Models;
using NeonLadderURP.Models;

namespace NeonLadder.Editor.SaveSystem
{
    /// <summary>
    /// Tony Stark's Save System Command Center - The most advanced save debugging tool in the multiverse
    /// Built with @gamora for comprehensive save state management and testing
    /// </summary>
    public class SaveSystemCommandCenter : EditorWindow
    {
        #region FRIDAY Interface Variables
        private Vector2 scrollPosition;
        private int selectedTabIndex = 0;
        private readonly string[] tabNames = { "🔍 Viewer", "📤 Export", "📥 Import", "🔄 Convert", "🧪 Test Data", "⚙️ Settings" };
        
        // Save file analysis
        private SaveSystemConfig currentConfig;
        private ConsolidatedSaveData loadedSaveData;
        private string saveDataJson = "";
        private bool showRawJson = false;
        
        // Import/Export
        private string importFilePath = "";
        private string exportFileName = "NeonLadder_Save_Export";
        private SaveFormat exportFormat = SaveFormat.JSON;
        
        // Test data generation
        private TestDataProfile testProfile = new TestDataProfile();
        
        // UI State
        private bool isAnalyzing = false;
        private Color starkBlue = new Color(0.1f, 0.6f, 1f);
        private Color starkGold = new Color(1f, 0.8f, 0.1f);
        #endregion

        [MenuItem("NeonLadder/Saves/Save System Command Center")]
        public static void ShowWindow()
        {
            var window = GetWindow<SaveSystemCommandCenter>("Save Command Center");
            window.titleContent = new GUIContent("🦾 Save Command Center", "Tony Stark's Advanced Save System Manager");
            window.minSize = new Vector2(800, 600);
            window.Show();
        }

        private void OnEnable()
        {
            // Load default config if available
            LoadDefaultConfig();
            RefreshSaveData();
        }

        private void OnGUI()
        {
            DrawStarkHeader();
            
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex, tabNames);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            switch (selectedTabIndex)
            {
                case 0: DrawViewerTab(); break;
                case 1: DrawExportTab(); break;
                case 2: DrawImportTab(); break;
                case 3: DrawConvertTab(); break;
                case 4: DrawTestDataTab(); break;
                case 5: DrawSettingsTab(); break;
            }
            
            EditorGUILayout.EndScrollView();
            
            DrawStatusBar();
        }

        #region FRIDAY Interface Methods
        
        private void DrawStarkHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            GUI.color = starkGold;
            GUILayout.Label("🦾 FRIDAY - Save System Analysis", EditorStyles.boldLabel);
            GUI.color = Color.white;
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("🔄 Refresh", EditorStyles.toolbarButton))
            {
                RefreshSaveData();
            }
            
            if (GUILayout.Button("📊 Diagnostics", EditorStyles.toolbarButton))
            {
                RunDiagnostics();
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
        
        private void DrawViewerTab()
        {
            EditorGUILayout.LabelField("🔍 Save Data Viewer", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            // Config selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Config Source:", GUILayout.Width(100));
            currentConfig = EditorGUILayout.ObjectField(currentConfig, typeof(SaveSystemConfig), false) as SaveSystemConfig;
            if (GUILayout.Button("Auto-Find", GUILayout.Width(80)))
            {
                LoadDefaultConfig();
            }
            EditorGUILayout.EndHorizontal();
            
            if (currentConfig == null)
            {
                EditorGUILayout.HelpBox("⚠️ No SaveSystemConfig selected. Please assign one above.", MessageType.Warning);
                return;
            }
            
            EditorGUILayout.Space();
            
            // Save file status
            string savePath = currentConfig.GetSaveFilePath();
            bool saveExists = File.Exists(savePath);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("💾 Save File Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Path: {savePath}");
            
            GUI.color = saveExists ? Color.green : Color.red;
            EditorGUILayout.LabelField($"Status: {(saveExists ? "✅ Found" : "❌ Not Found")}");
            GUI.color = Color.white;
            
            if (saveExists)
            {
                var fileInfo = new FileInfo(savePath);
                EditorGUILayout.LabelField($"Size: {fileInfo.Length} bytes");
                EditorGUILayout.LabelField($"Modified: {fileInfo.LastWriteTime}");
            }
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (!saveExists)
            {
                EditorGUILayout.HelpBox("❌ No save file found. Generate test data or load a game to create one.", MessageType.Info);
                return;
            }
            
            // Load and display save data
            if (GUILayout.Button("🔄 Load Save Data", GUILayout.Height(30)))
            {
                LoadSaveData();
            }
            
            if (loadedSaveData != null)
            {
                DrawSaveDataViewer();
            }
        }
        
        private void DrawSaveDataViewer()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("📊 Save Data Analysis", EditorStyles.boldLabel);
            
            // Quick stats
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📈 Quick Stats", EditorStyles.boldLabel);
            
            if (loadedSaveData.progression != null)
            {
                EditorGUILayout.LabelField($"Player Level: {loadedSaveData.progression.playerLevel}");
                EditorGUILayout.LabelField($"Health: {loadedSaveData.progression.currentHealth}/{loadedSaveData.progression.maxHealth}");
                EditorGUILayout.LabelField($"Experience: {loadedSaveData.progression.experiencePoints}");
            }
            
            if (loadedSaveData.currencies != null)
            {
                EditorGUILayout.LabelField($"Meta Currency: {loadedSaveData.currencies.metaCurrency}");
                EditorGUILayout.LabelField($"Perma Currency: {loadedSaveData.currencies.permaCurrency}");
            }
            
            if (loadedSaveData.worldState != null)
            {
                EditorGUILayout.LabelField($"Current Scene: {loadedSaveData.worldState.currentSceneName}");
                EditorGUILayout.LabelField($"Run Number: {loadedSaveData.worldState.currentRun?.runNumber ?? 0}");
                EditorGUILayout.LabelField($"Depth: {loadedSaveData.worldState.proceduralState?.currentDepth ?? 0}");
            }
            
            EditorGUILayout.LabelField($"Game Version: {loadedSaveData.gameVersion}");
            EditorGUILayout.LabelField($"Last Saved: {loadedSaveData.lastSaved}");
            
            EditorGUILayout.EndVertical();
            
            // Raw JSON viewer
            EditorGUILayout.Space();
            showRawJson = EditorGUILayout.Foldout(showRawJson, "🔍 Raw JSON Data");
            if (showRawJson)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Raw Save Data:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("📋 Copy to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer = saveDataJson;
                    Debug.Log("💾 Save data copied to clipboard");
                }
                if (GUILayout.Button("💾 Save as File"))
                {
                    SaveJsonToFile();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.TextArea(saveDataJson, GUILayout.Height(200));
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawExportTab()
        {
            EditorGUILayout.LabelField("📤 Export Save Data", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Export Settings", EditorStyles.boldLabel);
            
            exportFileName = EditorGUILayout.TextField("Export Filename:", exportFileName);
            exportFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format:", exportFormat);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("📤 Export Current Save", GUILayout.Height(30)))
            {
                ExportSaveData();
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("💡 Exported files include metadata and can be imported back into the game or shared with team members.", MessageType.Info);
        }
        
        private void DrawImportTab()
        {
            EditorGUILayout.LabelField("📥 Import Save Data", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Import Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Import File:", importFilePath);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                BrowseForImportFile();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(importFilePath));
            if (GUILayout.Button("📥 Import Save Data", GUILayout.Height(30)))
            {
                ImportSaveData();
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("⚠️ Importing will backup your current save file before replacing it. This action can be undone by restoring the backup.", MessageType.Warning);
        }
        
        private void DrawConvertTab()
        {
            EditorGUILayout.LabelField("🔄 Format Converter", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Dual Format Conversion", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField("Convert between Unity Binary and JSON formats for Steam Cloud compatibility and debugging.");
            EditorGUILayout.Space();
            
            if (GUILayout.Button("🔄 JSON → Unity Binary", GUILayout.Height(25)))
            {
                ConvertJsonToBinary();
            }
            
            if (GUILayout.Button("🔄 Unity Binary → JSON", GUILayout.Height(25)))
            {
                ConvertBinaryToJson();
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("💡 Steam Cloud requires Unity's binary serialization for optimal performance. JSON format is perfect for debugging and manual editing.", MessageType.Info);
        }
        
        private void DrawTestDataTab()
        {
            EditorGUILayout.LabelField("🧪 Test Data Generator", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Test Profile Configuration", EditorStyles.boldLabel);
            
            testProfile.profileName = EditorGUILayout.TextField("Profile Name:", testProfile.profileName);
            testProfile.playerLevel = EditorGUILayout.IntSlider("Player Level:", testProfile.playerLevel, 1, 100);
            testProfile.metaCurrency = EditorGUILayout.IntField("Meta Currency:", testProfile.metaCurrency);
            testProfile.permaCurrency = EditorGUILayout.IntField("Perma Currency:", testProfile.permaCurrency);
            testProfile.currentDepth = EditorGUILayout.IntSlider("Depth:", testProfile.currentDepth, 0, 50);
            testProfile.runNumber = EditorGUILayout.IntField("Run Number:", testProfile.runNumber);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("🧪 Generate Test Save Data", GUILayout.Height(30)))
            {
                GenerateTestData();
            }
            
            EditorGUILayout.EndVertical();
            
            // Quick presets
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("⚡ Quick Presets", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🆕 New Player"))
            {
                LoadNewPlayerPreset();
            }
            if (GUILayout.Button("💪 Mid Game"))
            {
                LoadMidGamePreset();
            }
            if (GUILayout.Button("🏆 End Game"))
            {
                LoadEndGamePreset();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSettingsTab()
        {
            EditorGUILayout.LabelField("⚙️ Command Center Settings", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Configuration", EditorStyles.boldLabel);
            
            currentConfig = EditorGUILayout.ObjectField("Save System Config:", currentConfig, typeof(SaveSystemConfig), false) as SaveSystemConfig;
            
            EditorGUILayout.Space();
            
            if (currentConfig != null)
            {
                EditorGUILayout.LabelField($"Save Path: {currentConfig.GetSaveFilePath()}");
                EditorGUILayout.LabelField($"Format: {currentConfig.saveFormat}");
                EditorGUILayout.LabelField($"Encryption: {(currentConfig.enableEncryption ? "Enabled" : "Disabled")}");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("🔧 Open Save System Config"))
            {
                if (currentConfig != null)
                {
                    Selection.activeObject = currentConfig;
                }
            }
        }
        
        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (isAnalyzing)
            {
                GUI.color = starkBlue;
                GUILayout.Label("🔄 Analyzing...", EditorStyles.miniLabel);
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label($"💾 Ready - Save System Command Center v2.0", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.Label($"🦾 Stark Industries © 2025", EditorStyles.miniLabel);
            
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion

        #region Core Functionality
        
        private void RefreshSaveData()
        {
            if (currentConfig == null) return;
            
            LoadSaveData();
        }
        
        private void LoadDefaultConfig()
        {
            // Try to find a SaveSystemConfig in the project
            string[] guids = AssetDatabase.FindAssets("t:SaveSystemConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                currentConfig = AssetDatabase.LoadAssetAtPath<SaveSystemConfig>(path);
                Debug.Log($"🔍 Auto-loaded SaveSystemConfig: {currentConfig.name}");
            }
        }
        
        private void LoadSaveData()
        {
            if (currentConfig == null) return;
            
            isAnalyzing = true;
            
            try
            {
                string savePath = currentConfig.GetSaveFilePath();
                if (!File.Exists(savePath))
                {
                    Debug.LogWarning($"❌ Save file not found: {savePath}");
                    return;
                }
                
                string rawData = File.ReadAllText(savePath);
                saveDataJson = rawData;
                
                // Try to deserialize based on format
                if (currentConfig.saveFormat == SaveFormat.JSON)
                {
                    loadedSaveData = JsonUtility.FromJson<ConsolidatedSaveData>(rawData);
                }
                else
                {
                    // TODO: Implement binary deserialization
                    Debug.LogWarning("⚠️ Binary format deserialization not yet implemented");
                }
                
                Debug.Log("✅ Save data loaded successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to load save data: {ex.Message}");
            }
            finally
            {
                isAnalyzing = false;
            }
        }
        
        private void RunDiagnostics()
        {
            Debug.Log("🔍 Running FRIDAY diagnostics...");
            
            if (currentConfig != null)
            {
                var result = currentConfig.ValidateConfiguration();
                if (result.IsValid)
                {
                    Debug.Log("✅ Save system configuration is valid");
                }
            }
            
            Debug.Log("🦾 Diagnostics complete");
        }
        
        private void ExportSaveData()
        {
            if (currentConfig == null || loadedSaveData == null)
            {
                Debug.LogError("❌ No save data loaded for export");
                return;
            }
            
            string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string fileName = $"{exportFileName}_{timestamp}.json";
            string exportPath = EditorUtility.SaveFilePanel("Export Save Data", Application.dataPath, fileName, "json");
            
            if (!string.IsNullOrEmpty(exportPath))
            {
                try
                {
                    var exportData = new SaveExportData
                    {
                        exportTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        gameVersion = Application.version,
                        saveFileName = currentConfig.saveFileName,
                        configurationName = currentConfig.configurationName,
                        originalSaveData = saveDataJson
                    };
                    
                    string exportJson = JsonUtility.ToJson(exportData, true);
                    File.WriteAllText(exportPath, exportJson);
                    
                    Debug.Log($"✅ Save data exported to: {exportPath}");
                    EditorUtility.RevealInFinder(exportPath);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Export failed: {ex.Message}");
                }
            }
        }
        
        private void BrowseForImportFile()
        {
            string path = EditorUtility.OpenFilePanel("Import Save Data", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                importFilePath = path;
            }
        }
        
        private void ImportSaveData()
        {
            if (string.IsNullOrEmpty(importFilePath) || !File.Exists(importFilePath))
            {
                Debug.LogError("❌ Invalid import file path");
                return;
            }
            
            try
            {
                string importJson = File.ReadAllText(importFilePath);
                var importData = JsonUtility.FromJson<SaveExportData>(importJson);
                
                if (importData == null || string.IsNullOrEmpty(importData.originalSaveData))
                {
                    Debug.LogError("❌ Invalid save data format");
                    return;
                }
                
                // Create backup
                string currentSavePath = currentConfig.GetSaveFilePath();
                if (File.Exists(currentSavePath))
                {
                    string backupPath = currentSavePath + ".backup_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    File.Copy(currentSavePath, backupPath);
                    Debug.Log($"💾 Created backup: {backupPath}");
                }
                
                // Import the data
                File.WriteAllText(currentSavePath, importData.originalSaveData);
                
                Debug.Log($"✅ Save data imported successfully");
                RefreshSaveData();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Import failed: {ex.Message}");
            }
        }
        
        private void ConvertJsonToBinary()
        {
            if (currentConfig == null)
            {
                Debug.LogError("❌ No SaveSystemConfig selected");
                return;
            }
            
            string jsonPath = EditorUtility.OpenFilePanel("Select JSON Save File", Application.persistentDataPath, "json");
            if (string.IsNullOrEmpty(jsonPath)) return;
            
            string binaryPath = EditorUtility.SaveFilePanel("Save Binary File", Application.persistentDataPath, "save_binary", "dat");
            if (string.IsNullOrEmpty(binaryPath)) return;
            
            bool success = SaveFormatConverter.ConvertJsonToBinary(jsonPath, binaryPath);
            if (success)
            {
                Debug.Log("✅ JSON → Binary conversion completed successfully!");
                EditorUtility.RevealInFinder(binaryPath);
            }
        }
        
        private void ConvertBinaryToJson()
        {
            if (currentConfig == null)
            {
                Debug.LogError("❌ No SaveSystemConfig selected");
                return;
            }
            
            string binaryPath = EditorUtility.OpenFilePanel("Select Binary Save File", Application.persistentDataPath, "dat");
            if (string.IsNullOrEmpty(binaryPath)) return;
            
            string jsonPath = EditorUtility.SaveFilePanel("Save JSON File", Application.persistentDataPath, "save_readable", "json");
            if (string.IsNullOrEmpty(jsonPath)) return;
            
            bool success = SaveFormatConverter.ConvertBinaryToJson(binaryPath, jsonPath);
            if (success)
            {
                Debug.Log("✅ Binary → JSON conversion completed successfully!");
                EditorUtility.RevealInFinder(jsonPath);
            }
        }
        
        private void GenerateTestData()
        {
            Debug.Log($"🧪 Generating test data: {testProfile.profileName}");
            
            var testSaveData = new ConsolidatedSaveData
            {
                gameVersion = Application.version,
                lastSaved = System.DateTime.Now,
                progression = new PlayerProgressionData
                {
                    playerLevel = testProfile.playerLevel,
                    experiencePoints = testProfile.playerLevel * 1000f,
                    maxHealth = 100 + (testProfile.playerLevel * 10),
                    currentHealth = 100 + (testProfile.playerLevel * 10),
                    maxStamina = 100f + (testProfile.playerLevel * 5f),
                    currentStamina = 100f + (testProfile.playerLevel * 5f),
                    attackDamage = 10f + (testProfile.playerLevel * 2f),
                    movementSpeed = 5f + (testProfile.playerLevel * 0.1f)
                },
                currencies = new CurrencyData
                {
                    metaCurrency = testProfile.metaCurrency,
                    permaCurrency = testProfile.permaCurrency,
                    totalMetaEarned = testProfile.metaCurrency * 2,
                    totalPermaEarned = testProfile.permaCurrency
                },
                worldState = new WorldStateData
                {
                    currentSceneName = "TestScene",
                    playerPosition = Vector3.zero,
                    proceduralState = new ProceduralGenerationState
                    {
                        currentDepth = testProfile.currentDepth,
                        currentSeed = 12345
                    },
                    currentRun = new RunData
                    {
                        runNumber = testProfile.runNumber,
                        runDepth = testProfile.currentDepth,
                        isActive = true
                    }
                }
            };
            
            if (currentConfig != null)
            {
                string testJson = JsonUtility.ToJson(testSaveData, true);
                File.WriteAllText(currentConfig.GetSaveFilePath(), testJson);
                Debug.Log($"✅ Test data generated: {testProfile.profileName}");
                RefreshSaveData();
            }
        }
        
        private void LoadNewPlayerPreset()
        {
            testProfile = new TestDataProfile
            {
                profileName = "New Player",
                playerLevel = 1,
                metaCurrency = 0,
                permaCurrency = 0,
                currentDepth = 0,
                runNumber = 1
            };
        }
        
        private void LoadMidGamePreset()
        {
            testProfile = new TestDataProfile
            {
                profileName = "Mid Game",
                playerLevel = 25,
                metaCurrency = 500,
                permaCurrency = 100,
                currentDepth = 15,
                runNumber = 5
            };
        }
        
        private void LoadEndGamePreset()
        {
            testProfile = new TestDataProfile
            {
                profileName = "End Game",
                playerLevel = 50,
                metaCurrency = 2000,
                permaCurrency = 500,
                currentDepth = 40,
                runNumber = 20
            };
        }
        
        private void SaveJsonToFile()
        {
            if (string.IsNullOrEmpty(saveDataJson)) return;
            
            string path = EditorUtility.SaveFilePanel("Save JSON Data", Application.dataPath, "save_data", "json");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, saveDataJson);
                Debug.Log($"💾 JSON saved to: {path}");
                EditorUtility.RevealInFinder(path);
            }
        }
        
        #endregion
    }
    
    [System.Serializable]
    public class TestDataProfile
    {
        public string profileName = "Test Profile";
        public int playerLevel = 1;
        public int metaCurrency = 0;
        public int permaCurrency = 0;
        public int currentDepth = 0;
        public int runNumber = 1;
    }
}