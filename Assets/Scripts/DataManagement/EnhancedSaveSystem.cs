using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Enhanced save system with JSON accessibility and comprehensive data management.
    /// Saves to game directory for easy external access and manipulation.
    /// </summary>
    public static class EnhancedSaveSystem
    {
        // File paths - saves in game directory instead of hidden Unity paths
        private static readonly string GameDataDirectory = Path.Combine(Application.dataPath, "..", "GameData");
        private static readonly string SaveFileName = "NeonLadderSave.json";
        private static readonly string BackupFileName = "NeonLadderSave_Backup.json";
        private static readonly string TempFileName = "NeonLadderSave_Temp.json";
        
        // Full file paths
        public static string SaveFilePath => Path.Combine(GameDataDirectory, SaveFileName);
        public static string BackupFilePath => Path.Combine(GameDataDirectory, BackupFileName);
        public static string TempFilePath => Path.Combine(GameDataDirectory, TempFileName);
        
        // Events for save/load notifications
        public static event Action<ConsolidatedSaveData> OnSaveCompleted;
        public static event Action<ConsolidatedSaveData> OnLoadCompleted;
        public static event Action<string> OnSaveError;
        
        static EnhancedSaveSystem()
        {
            EnsureGameDataDirectoryExists();
        }
        
        /// <summary>
        /// Save comprehensive game data with backup and error handling
        /// </summary>
        public static bool Save(ConsolidatedSaveData saveData)
        {
            try
            {
                EnsureGameDataDirectoryExists();
                
                // Update save metadata
                saveData.UpdateLastSaved();
                
                // Create backup of existing save
                CreateBackup();
                
                // Serialize with Unity's JsonUtility (pretty formatting enabled)
                string json = JsonUtility.ToJson(saveData, true);
                
                // Write to temporary file first (atomic operation)
                File.WriteAllText(TempFilePath, json);
                
                // Move temp file to actual save file (atomic operation)
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                File.Move(TempFilePath, SaveFilePath);
                
                Debug.Log($"[EnhancedSaveSystem] Game saved successfully to: {SaveFilePath}");
                OnSaveCompleted?.Invoke(saveData);
                
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"[EnhancedSaveSystem] Save failed: {ex.Message}";
                Debug.LogError(errorMessage);
                OnSaveError?.Invoke(errorMessage);
                
                // Attempt to restore from backup if save failed
                RestoreFromBackup();
                return false;
            }
        }
        
        /// <summary>
        /// Load comprehensive game data with fallback handling
        /// </summary>
        public static ConsolidatedSaveData Load()
        {
            try
            {
                if (!SaveExists())
                {
                    Debug.Log("[EnhancedSaveSystem] No save file found, creating new save data");
                    return CreateNewSaveData();
                }
                
                string json = File.ReadAllText(SaveFilePath);
                
                ConsolidatedSaveData saveData = JsonUtility.FromJson<ConsolidatedSaveData>(json);
                
                // Validate save data
                if (ValidateSaveData(saveData))
                {
                    Debug.Log($"[EnhancedSaveSystem] Game loaded successfully from: {SaveFilePath}");
                    OnLoadCompleted?.Invoke(saveData);
                    return saveData;
                }
                else
                {
                    Debug.LogWarning("[EnhancedSaveSystem] Save data validation failed, attempting backup restore");
                    return LoadFromBackup();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"[EnhancedSaveSystem] Load failed: {ex.Message}";
                Debug.LogError(errorMessage);
                OnSaveError?.Invoke(errorMessage);
                
                // Try to load from backup
                return LoadFromBackup();
            }
        }
        
        /// <summary>
        /// Check if save file exists
        /// </summary>
        public static bool SaveExists()
        {
            return File.Exists(SaveFilePath);
        }
        
        /// <summary>
        /// Get save file info for debugging
        /// </summary>
        public static SaveFileInfo GetSaveFileInfo()
        {
            if (!SaveExists())
                return null;
                
            FileInfo fileInfo = new FileInfo(SaveFilePath);
            
            try
            {
                string json = File.ReadAllText(SaveFilePath);
                ConsolidatedSaveData saveData = JsonUtility.FromJson<ConsolidatedSaveData>(json);
                
                return new SaveFileInfo
                {
                    FilePath = SaveFilePath,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    SaveVersion = saveData.saveVersion,
                    GameVersion = saveData.gameVersion,
                    TotalPlayTime = saveData.totalPlayTime,
                    LastSaved = saveData.lastSaved,
                    CurrentRun = saveData.worldState.currentRun.runNumber
                };
            }
            catch
            {
                return new SaveFileInfo
                {
                    FilePath = SaveFilePath,
                    FileSize = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    SaveVersion = "Unknown",
                    GameVersion = "Unknown"
                };
            }
        }
        
        /// <summary>
        /// Export save data to a specific location for sharing/backup
        /// </summary>
        public static bool ExportSave(string exportPath)
        {
            try
            {
                if (!SaveExists())
                {
                    Debug.LogError("[EnhancedSaveSystem] No save file to export");
                    return false;
                }
                
                File.Copy(SaveFilePath, exportPath, true);
                Debug.Log($"[EnhancedSaveSystem] Save exported to: {exportPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnhancedSaveSystem] Export failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Import save data from external file
        /// </summary>
        public static bool ImportSave(string importPath)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    Debug.LogError($"[EnhancedSaveSystem] Import file not found: {importPath}");
                    return false;
                }
                
                // Validate import file
                string json = File.ReadAllText(importPath);
                ConsolidatedSaveData testData = JsonUtility.FromJson<ConsolidatedSaveData>(json);
                
                if (!ValidateSaveData(testData))
                {
                    Debug.LogError("[EnhancedSaveSystem] Import file validation failed");
                    return false;
                }
                
                // Create backup before import
                CreateBackup();
                
                // Copy import file to save location
                File.Copy(importPath, SaveFilePath, true);
                
                Debug.Log($"[EnhancedSaveSystem] Save imported from: {importPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnhancedSaveSystem] Import failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Delete save file and backup
        /// </summary>
        public static bool DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
                    
                if (File.Exists(BackupFilePath))
                    File.Delete(BackupFilePath);
                    
                Debug.Log("[EnhancedSaveSystem] Save files deleted");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnhancedSaveSystem] Delete failed: {ex.Message}");
                return false;
            }
        }
        
        #region Private Helper Methods
        
        private static void EnsureGameDataDirectoryExists()
        {
            if (!Directory.Exists(GameDataDirectory))
            {
                Directory.CreateDirectory(GameDataDirectory);
                Debug.Log($"[EnhancedSaveSystem] Created GameData directory: {GameDataDirectory}");
            }
        }
        
        private static void CreateBackup()
        {
            if (File.Exists(SaveFilePath))
            {
                File.Copy(SaveFilePath, BackupFilePath, true);
            }
        }
        
        private static void RestoreFromBackup()
        {
            if (File.Exists(BackupFilePath))
            {
                File.Copy(BackupFilePath, SaveFilePath, true);
                Debug.Log("[EnhancedSaveSystem] Restored from backup");
            }
        }
        
        private static ConsolidatedSaveData LoadFromBackup()
        {
            try
            {
                if (File.Exists(BackupFilePath))
                {
                    string json = File.ReadAllText(BackupFilePath);
                    ConsolidatedSaveData saveData = JsonUtility.FromJson<ConsolidatedSaveData>(json);
                    
                    if (ValidateSaveData(saveData))
                    {
                        Debug.Log("[EnhancedSaveSystem] Loaded from backup successfully");
                        return saveData;
                    }
                }
                
                Debug.LogWarning("[EnhancedSaveSystem] Backup load failed, creating new save");
                return CreateNewSaveData();
            }
            catch
            {
                Debug.LogWarning("[EnhancedSaveSystem] Backup corrupted, creating new save");
                return CreateNewSaveData();
            }
        }
        
        private static ConsolidatedSaveData CreateNewSaveData()
        {
            var newSave = new ConsolidatedSaveData();
            
            // Initialize with default values
            newSave.progression.currentHealth = newSave.progression.maxHealth;
            newSave.progression.currentStamina = newSave.progression.maxStamina;
            newSave.worldState.currentRun.runStartTime = DateTime.Now;
            newSave.statistics.sessionStartTime = DateTime.Now;
            
            Debug.Log("[EnhancedSaveSystem] Created new save data");
            return newSave;
        }
        
        private static bool ValidateSaveData(ConsolidatedSaveData saveData)
        {
            if (saveData == null) return false;
            if (saveData.progression == null) return false;
            if (saveData.worldState == null) return false;
            if (saveData.currencies == null) return false;
            if (saveData.settings == null) return false;
            
            // Additional validation can be added here
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Information about a save file for debugging and UI display
    /// </summary>
    [Serializable]
    public class SaveFileInfo
    {
        public string FilePath;
        public long FileSize;
        public DateTime LastModified;
        public string SaveVersion;
        public string GameVersion;
        public float TotalPlayTime;
        public DateTime LastSaved;
        public int CurrentRun;
        
        public string FileSizeFormatted => FormatFileSize(FileSize);
        public string PlayTimeFormatted => FormatPlayTime(TotalPlayTime);
        
        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            return $"{bytes / (1024 * 1024):F1} MB";
        }
        
        private string FormatPlayTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600);
            int minutes = Mathf.FloorToInt((seconds % 3600) / 60);
            int secs = Mathf.FloorToInt(seconds % 60);
            
            if (hours > 0)
                return $"{hours}h {minutes}m {secs}s";
            else if (minutes > 0)
                return $"{minutes}m {secs}s";
            else
                return $"{secs}s";
        }
    }
}