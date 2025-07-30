using UnityEngine;
using System;
using System.Collections.Generic;
using NeonLadder.Debugging;

namespace NeonLadder.DataManagement
{
    /// <summary>
    /// ScriptableObject configuration for the NeonLadder Save/Load system
    /// Following the established NeonLadder pattern (like UpgradeData, PathGeneratorConfig)
    /// Provides Unity Editor tooltips and configuration management
    /// </summary>
    [CreateAssetMenu(fileName = "Save System Config", menuName = "NeonLadder/Data Management/Save System Config")]
    public class SaveSystemConfig : ScriptableObject
    {
        [Header("🗄️ Save System Configuration")]
        [Tooltip("Name of this save system configuration preset")]
        public string configurationName = "Default Save Config";
        
        [TextArea(3, 5)]
        [Tooltip("Description of this save configuration's purpose and settings")]
        public string description = "Standard save system configuration for NeonLadder roguelite progression";

        [Header("📁 File Management")]
        [Tooltip("Name of the save file (without extension)")]
        public string saveFileName = "playerData";
        
        [Tooltip("File format for save data")]
        public SaveFormat saveFormat = SaveFormat.JSON;
        
        [Tooltip("Enable encryption for save files (recommended for release builds)")]
        public bool enableEncryption = true;
        
        [Tooltip("Encryption key (leave empty to auto-generate)")]
        [SerializeField] private string encryptionKey = "";

        [Header("💾 Save Behavior")]
        [Tooltip("Automatically save when player gains currency")]
        public bool autoSaveOnCurrencyChange = true;
        
        [Tooltip("Automatically save when completing levels")]
        public bool autoSaveOnLevelComplete = true;
        
        [Tooltip("Automatically save when purchasing upgrades")]
        public bool autoSaveOnUpgradePurchase = true;
        
        [Tooltip("Time interval for periodic auto-saves (seconds, 0 = disabled)")]
        [Range(0, 300)]
        public float autoSaveInterval = 60f;

        [Header("🔄 Backup System")]
        [Tooltip("Number of backup save files to maintain")]
        [Range(0, 5)]
        public int backupCount = 2;
        
        [Tooltip("Create backup before each save operation")]
        public bool createBackupBeforeSave = true;
        
        [Tooltip("Validate save file integrity on load")]
        public bool validateOnLoad = true;

        [Header("🏆 Progress Tracking")]
        [Tooltip("List of unlockable features and their requirements")]
        public List<UnlockableFeature> unlockableFeatures = new List<UnlockableFeature>();
        
        [Tooltip("Default player settings for new saves")]
        public DefaultPlayerSettings defaultSettings = new DefaultPlayerSettings();

        [Header("🧪 Testing & Debug")]
        [Tooltip("Enable debug logging for save operations")]
        public bool enableDebugLogging = false;
        
        [Tooltip("Path for debug save files (empty = use default)")]
        public string debugSavePath = "";
        
        [Tooltip("Reset all save data (USE WITH CAUTION)")]
        [SerializeField] private bool resetAllSaveData = false;

        [Header("📊 Statistics")]
        [SerializeField, HideInInspector]
        private SaveSystemStats lastOperationStats = new SaveSystemStats();

        /// <summary>
        /// Gets the full save file path with extension
        /// </summary>
        public string GetSaveFilePath()
        {
            string extension = saveFormat == SaveFormat.JSON ? ".json" : ".dat";
            string basePath = string.IsNullOrEmpty(debugSavePath) ? 
                Application.persistentDataPath : debugSavePath;
            
            // Create GameData subdirectory for save files organization
            string gameDataPath = System.IO.Path.Combine(basePath, "GameData");
            
            return System.IO.Path.Combine(gameDataPath, saveFileName + extension);
        }

        /// <summary>
        /// Gets the encryption key (generates one if empty)
        /// </summary>
        public string GetEncryptionKey()
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                encryptionKey = System.Guid.NewGuid().ToString("N")[..16]; // 16 char key
            }
            return encryptionKey;
        }

        /// <summary>
        /// Validates the current configuration
        /// </summary>
        [ContextMenu("🔍 Validate Configuration")]
        public ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(saveFileName))
            {
                result.AddError("Save file name cannot be empty");
            }
            
            if (enableEncryption && string.IsNullOrEmpty(encryptionKey))
            {
                result.AddWarning("Encryption enabled but no key specified - will auto-generate");
            }
            
            if (autoSaveInterval > 0 && autoSaveInterval < 10)
            {
                result.AddWarning("Auto-save interval very low - may impact performance");
            }
            
            if (backupCount > 3)
            {
                result.AddWarning("High backup count may use significant disk space");
            }

            // Log results
            if (result.HasErrors)
            {
                Debugger.LogError($"❌ Save System Config validation failed:\n{result.GetSummary()}");
            }
            else if (result.HasWarnings)
            {
                Debugger.LogWarning($"⚠️ Save System Config has warnings:\n{result.GetSummary()}");
            }
            else
            {
                Debugger.Log($"✅ Save System Config validation passed: {configurationName}");
            }
            
            return result;
        }

        /// <summary>
        /// Creates a production-ready configuration
        /// </summary>
        [ContextMenu("🚀 Load Production Preset")]
        public void LoadProductionPreset()
        {
            configurationName = "Production (Secure & Reliable)";
            description = "Production configuration with encryption, backups, and validation";
            enableEncryption = true;
            autoSaveOnCurrencyChange = true;
            autoSaveOnLevelComplete = true;
            autoSaveOnUpgradePurchase = true;
            autoSaveInterval = 120f; // 2 minutes
            backupCount = 2;
            createBackupBeforeSave = true;
            validateOnLoad = true;
            enableDebugLogging = false;
            
            Debugger.Log("🚀 Loaded production preset for save system");
        }

        /// <summary>
        /// Creates a development/testing configuration
        /// </summary>
        [ContextMenu("🔧 Load Development Preset")]
        public void LoadDevelopmentPreset()
        {
            configurationName = "Development (Fast & Debuggable)";
            description = "Development configuration with debug logging and no encryption";
            enableEncryption = false;
            autoSaveOnCurrencyChange = true;
            autoSaveOnLevelComplete = true;
            autoSaveOnUpgradePurchase = true;
            autoSaveInterval = 30f; // Frequent saves for testing
            backupCount = 1;
            createBackupBeforeSave = false;
            validateOnLoad = false;
            enableDebugLogging = true;
            
            Debugger.Log("🔧 Loaded development preset for save system");
        }

        /// <summary>
        /// Export current save data to JSON file for backup/transfer
        /// </summary>
        [ContextMenu("📤 Export Save Data to JSON")]
        public void ExportSaveDataToJSON()
        {
            try
            {
                string saveFile = GetSaveFilePath();
                if (!System.IO.File.Exists(saveFile))
                {
                    Debugger.LogWarning("❌ No save file found to export");
                    return;
                }

                // Read the current save data
                string saveData = System.IO.File.ReadAllText(saveFile);
                
                // If encrypted, decrypt first
                if (enableEncryption)
                {
                    // Note: This would need actual decryption implementation
                    Debugger.LogWarning("🔐 Encrypted save detected - implement decryption for export");
                }

                // Create export file with timestamp
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string exportPath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(saveFile), 
                    $"SaveExport_{timestamp}.json"
                );

                // Create export data with metadata
                var exportData = new SaveExportData
                {
                    exportTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    gameVersion = Application.version,
                    saveFileName = saveFileName,
                    configurationName = configurationName,
                    originalSaveData = saveData
                };

                string exportJson = JsonUtility.ToJson(exportData, true);
                System.IO.File.WriteAllText(exportPath, exportJson);

                Debugger.Log($"✅ Save data exported to: {exportPath}");
                
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.RevealInFinder(exportPath);
                #endif
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Failed to export save data: {ex.Message}");
            }
        }

        /// <summary>
        /// Import save data from JSON file
        /// </summary>
        [ContextMenu("📥 Import Save Data from JSON")]
        public void ImportSaveDataFromJSON()
        {
            try
            {
                #if UNITY_EDITOR
                string importPath = UnityEditor.EditorUtility.OpenFilePanel(
                    "Import Save Data JSON", 
                    Application.persistentDataPath, 
                    "json"
                );

                if (string.IsNullOrEmpty(importPath))
                {
                    Debugger.Log("📥 Import cancelled by user");
                    return;
                }

                if (!System.IO.File.Exists(importPath))
                {
                    Debugger.LogError("❌ Selected file does not exist");
                    return;
                }

                string importJson = System.IO.File.ReadAllText(importPath);
                var importData = JsonUtility.FromJson<SaveExportData>(importJson);

                if (importData == null || string.IsNullOrEmpty(importData.originalSaveData))
                {
                    Debugger.LogError("❌ Invalid save data format in JSON file");
                    return;
                }

                // Create backup of current save before importing
                if (createBackupBeforeSave)
                {
                    string currentSave = GetSaveFilePath();
                    if (System.IO.File.Exists(currentSave))
                    {
                        string backupPath = currentSave.Replace(
                            System.IO.Path.GetExtension(currentSave),
                            $".backup_before_import_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}{System.IO.Path.GetExtension(currentSave)}"
                        );
                        System.IO.File.Copy(currentSave, backupPath);
                        Debugger.Log($"💾 Created backup before import: {backupPath}");
                    }
                }

                // Write imported data to save file
                string saveFile = GetSaveFilePath();
                System.IO.File.WriteAllText(saveFile, importData.originalSaveData);

                Debugger.Log($"✅ Save data imported successfully from: {System.IO.Path.GetFileName(importPath)}");
                Debugger.Log($"📊 Import Details: Game Version: {importData.gameVersion}, Export Date: {importData.exportTimestamp}");
                #else
                Debugger.LogWarning("❌ Import functionality only available in Unity Editor");
                #endif
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Failed to import save data: {ex.Message}");
            }
        }

        /// <summary>
        /// Emergency reset of all save data
        /// </summary>
        [ContextMenu("⚠️ RESET ALL SAVE DATA")]
        public void ResetAllSaveData()
        {
            if (resetAllSaveData)
            {
                Debugger.LogWarning("🚨 RESETTING ALL SAVE DATA - This action cannot be undone!");
                
                string saveFile = GetSaveFilePath();
                if (System.IO.File.Exists(saveFile))
                {
                    System.IO.File.Delete(saveFile);
                    Debugger.Log($"🗑️ Deleted save file: {saveFile}");
                }
                
                // Delete backups
                for (int i = 1; i <= backupCount; i++)
                {
                    string backupFile = saveFile.Replace(System.IO.Path.GetExtension(saveFile), 
                        $".backup{i}{System.IO.Path.GetExtension(saveFile)}");
                    if (System.IO.File.Exists(backupFile))
                    {
                        System.IO.File.Delete(backupFile);
                        Debugger.Log($"🗑️ Deleted backup file: {backupFile}");
                    }
                }
                
                resetAllSaveData = false; // Reset the flag
                Debugger.Log("✅ Save data reset complete");
            }
            else
            {
                Debugger.LogWarning("❌ Reset all save data flag is not set - enable it first");
            }
        }

        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            autoSaveInterval = Mathf.Clamp(autoSaveInterval, 0, 600);
            backupCount = Mathf.Clamp(backupCount, 0, 5);
            
            // Auto-generate config name if empty
            if (string.IsNullOrEmpty(configurationName))
            {
                configurationName = name;
            }
        }
    }

    [Serializable]
    public enum SaveFormat
    {
        JSON,           // Human-readable, larger file size
        Binary,         // Compact, faster, less readable
        Compressed      // JSON + compression
    }

    [Serializable]
    public class UnlockableFeature
    {
        [Tooltip("Internal ID for this feature")]
        public string featureId;
        
        [Tooltip("Display name for this feature")]
        public string displayName;
        
        [Tooltip("Description of what this feature unlocks")]
        public string description;
        
        [Tooltip("Cost in permanent currency to unlock")]
        public int unlockCost;
        
        [Tooltip("Prerequisites that must be unlocked first")]
        public string[] prerequisites = new string[0];
        
        [Tooltip("Is this feature unlocked by default")]
        public bool unlockedByDefault = false;
    }

    [Serializable]
    public class DefaultPlayerSettings
    {
        [Tooltip("Default music volume (0-1)")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        
        [Tooltip("Default SFX volume (0-1)")]
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
        
        [Tooltip("Default screen resolution")]
        public string resolution = "1920x1080";
        
        [Tooltip("Default fullscreen mode")]
        public bool fullscreen = true;
        
        [Tooltip("Default graphics quality setting")]
        [Range(0, 5)]
        public int qualityLevel = 3;
    }

    [Serializable]
    public class SaveSystemStats
    {
        public int totalSaves;
        public int totalLoads;
        public int failedOperations;
        public float averageSaveTime;
        public float averageLoadTime;
        public System.DateTime lastOperation;
        
        public string GetSummary()
        {
            return $"Saves: {totalSaves}, Loads: {totalLoads}, Failed: {failedOperations}, " +
                   $"Avg Save: {averageSaveTime:F2}ms, Avg Load: {averageLoadTime:F2}ms";
        }
    }

    [Serializable]
    public class SaveExportData
    {
        [Tooltip("Timestamp when the save data was exported")]
        public string exportTimestamp;
        
        [Tooltip("Game version at time of export")]
        public string gameVersion;
        
        [Tooltip("Original save file name")]
        public string saveFileName;
        
        [Tooltip("Configuration name used for export")]
        public string configurationName;
        
        [Tooltip("The actual save data content")]
        public string originalSaveData;
    }

    [Serializable]
    public class ValidationResult
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        
        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;
        public bool IsValid => !HasErrors;
        
        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);
        
        public string GetSummary()
        {
            var summary = "";
            if (HasErrors)
            {
                summary += "Errors:\n" + string.Join("\n", errors) + "\n";
            }
            if (HasWarnings)
            {
                summary += "Warnings:\n" + string.Join("\n", warnings);
            }
            return summary.Trim();
        }
    }
}