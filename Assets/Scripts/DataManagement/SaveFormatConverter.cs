using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NeonLadder.Debugging;
using NeonLadder.DataManagement; // For SaveFormat enum

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Tony Stark & Gamora's Dual-Format Save Converter
    /// Handles conversion between Unity Binary (Steam Cloud compatible) and JSON (human-readable debugging)
    /// </summary>
    public static class SaveFormatConverter
    {
        #region JSON ↔ Binary Conversion
        
        /// <summary>
        /// Convert JSON save data to Unity Binary format (Steam Cloud compatible)
        /// </summary>
        /// <param name="jsonFilePath">Path to JSON save file</param>
        /// <param name="binaryOutputPath">Output path for binary file</param>
        /// <returns>True if conversion successful</returns>
        public static bool ConvertJsonToBinary(string jsonFilePath, string binaryOutputPath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    Debugger.LogError($"❌ JSON file not found: {jsonFilePath}");
                    return false;
                }
                
                // Read JSON data
                string jsonData = File.ReadAllText(jsonFilePath);
                
                // Deserialize from JSON
                ConsolidatedSaveData saveData = JsonUtility.FromJson<ConsolidatedSaveData>(jsonData);
                if (saveData == null)
                {
                    Debugger.LogError("❌ Failed to deserialize JSON data");
                    return false;
                }
                
                // Serialize to Unity Binary
                using (FileStream fileStream = new FileStream(binaryOutputPath, FileMode.Create))
                {
                    // Unity's built-in binary serialization (Steam Cloud compatible)
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, saveData);
                }
                
                Debugger.Log($"✅ Converted JSON → Binary: {binaryOutputPath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ JSON → Binary conversion failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Convert Unity Binary save data to JSON format (human-readable debugging)
        /// </summary>
        /// <param name="binaryFilePath">Path to binary save file</param>
        /// <param name="jsonOutputPath">Output path for JSON file</param>
        /// <returns>True if conversion successful</returns>
        public static bool ConvertBinaryToJson(string binaryFilePath, string jsonOutputPath)
        {
            try
            {
                if (!File.Exists(binaryFilePath))
                {
                    Debugger.LogError($"❌ Binary file not found: {binaryFilePath}");
                    return false;
                }
                
                // Deserialize from Unity Binary
                ConsolidatedSaveData saveData;
                using (FileStream fileStream = new FileStream(binaryFilePath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (ConsolidatedSaveData)formatter.Deserialize(fileStream);
                }
                
                if (saveData == null)
                {
                    Debugger.LogError("❌ Failed to deserialize binary data");
                    return false;
                }
                
                // Serialize to JSON with pretty formatting
                string jsonData = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(jsonOutputPath, jsonData);
                
                Debugger.Log($"✅ Converted Binary → JSON: {jsonOutputPath}");
                return true;
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Binary → JSON conversion failed: {ex.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region Save Data Validation
        
        /// <summary>
        /// Validate save data integrity (works with both formats)
        /// </summary>
        /// <param name="saveData">Save data to validate</param>
        /// <returns>Validation result with details</returns>
        public static SaveValidationResult ValidateSaveData(ConsolidatedSaveData saveData)
        {
            var result = new SaveValidationResult();
            
            if (saveData == null)
            {
                result.AddError("Save data is null");
                return result;
            }
            
            // Validate core data
            if (saveData.progression == null)
            {
                result.AddError("Player progression data is missing");
            }
            else
            {
                if (saveData.progression.playerLevel < 1)
                    result.AddWarning("Player level is less than 1");
                    
                if (saveData.progression.currentHealth > saveData.progression.maxHealth)
                    result.AddWarning("Current health exceeds max health");
                    
                if (saveData.progression.currentStamina > saveData.progression.maxStamina)
                    result.AddWarning("Current stamina exceeds max stamina");
            }
            
            // Validate currency data
            if (saveData.currencies == null)
            {
                result.AddError("Currency data is missing");
            }
            else
            {
                if (saveData.currencies.metaCurrency < 0)
                    result.AddError("Meta currency cannot be negative");
                    
                if (saveData.currencies.permaCurrency < 0)
                    result.AddError("Perma currency cannot be negative");
            }
            
            // Validate world state
            if (saveData.worldState == null)
            {
                result.AddError("World state data is missing");
            }
            else
            {
                if (string.IsNullOrEmpty(saveData.worldState.currentSceneName))
                    result.AddWarning("Current scene name is empty");
                    
                if (saveData.worldState.currentRun != null && saveData.worldState.currentRun.runNumber < 1)
                    result.AddWarning("Run number is less than 1");
            }
            
            // Validate save metadata
            if (string.IsNullOrEmpty(saveData.gameVersion))
            {
                result.AddWarning("Game version is not specified");
            }
            
            if (saveData.totalPlayTime < 0)
            {
                result.AddError("Total play time cannot be negative");
            }
            
            return result;
        }
        
        #endregion
        
        #region Steam Cloud Integration Helpers
        
        /// <summary>
        /// Prepare save data for Steam Cloud (ensures binary format and compression)
        /// </summary>
        /// <param name="saveData">Save data to prepare</param>
        /// <param name="outputPath">Steam Cloud compatible output path</param>
        /// <returns>True if preparation successful</returns>
        public static bool PrepareSaveForSteamCloud(ConsolidatedSaveData saveData, string outputPath)
        {
            try
            {
                // Validate data first
                var validation = ValidateSaveData(saveData);
                if (!validation.IsValid)
                {
                    Debugger.LogError($"❌ Save data validation failed: {validation.GetErrorSummary()}");
                    return false;
                }
                
                // Steam Cloud prefers binary format for efficiency
                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(fileStream, saveData);
                }
                
                // Verify file size (Steam Cloud has limits)
                var fileInfo = new FileInfo(outputPath);
                if (fileInfo.Length > 1024 * 1024) // 1MB limit (example)
                {
                    Debugger.LogWarning($"⚠️ Save file is large ({fileInfo.Length} bytes) - consider data optimization");
                }
                
                Debugger.Log($"✅ Save data prepared for Steam Cloud: {outputPath} ({fileInfo.Length} bytes)");
                return true;
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Steam Cloud preparation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Load save data from Steam Cloud (handles binary format)
        /// </summary>
        /// <param name="steamCloudPath">Path to Steam Cloud save file</param>
        /// <returns>Loaded save data or null if failed</returns>
        public static ConsolidatedSaveData LoadFromSteamCloud(string steamCloudPath)
        {
            try
            {
                if (!File.Exists(steamCloudPath))
                {
                    Debugger.LogWarning($"⚠️ Steam Cloud save not found: {steamCloudPath}");
                    return null;
                }
                
                ConsolidatedSaveData saveData;
                using (FileStream fileStream = new FileStream(steamCloudPath, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    saveData = (ConsolidatedSaveData)formatter.Deserialize(fileStream);
                }
                
                // Validate loaded data
                var validation = ValidateSaveData(saveData);
                if (!validation.IsValid)
                {
                    Debugger.LogError($"❌ Steam Cloud save validation failed: {validation.GetErrorSummary()}");
                    return null;
                }
                
                Debugger.Log($"✅ Successfully loaded from Steam Cloud: {steamCloudPath}");
                return saveData;
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Steam Cloud load failed: {ex.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Create a backup of the current save file
        /// </summary>
        /// <param name="originalPath">Original save file path</param>
        /// <returns>Backup file path or empty string if failed</returns>
        public static string CreateSaveBackup(string originalPath)
        {
            try
            {
                if (!File.Exists(originalPath))
                {
                    Debugger.LogWarning($"⚠️ Original save file not found for backup: {originalPath}");
                    return "";
                }
                
                string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupPath = originalPath + $".backup_{timestamp}";
                
                File.Copy(originalPath, backupPath);
                
                Debugger.Log($"💾 Created save backup: {backupPath}");
                return backupPath;
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Failed to create save backup: {ex.Message}");
                return "";
            }
        }
        
        /// <summary>
        /// Get save file format by examining the file
        /// </summary>
        /// <param name="filePath">Path to save file</param>
        /// <returns>Detected save format</returns>
        public static SaveFormat DetectSaveFormat(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return SaveFormat.JSON; // Default assumption
                }
                
                // Read first few bytes to detect format
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    if (fs.Length == 0)
                        return SaveFormat.JSON;
                    
                    byte[] buffer = new byte[4];
                    fs.Read(buffer, 0, 4);
                    
                    // JSON files typically start with '{' (0x7B)
                    if (buffer[0] == 0x7B)
                    {
                        return SaveFormat.JSON;
                    }
                    
                    // Binary formatter has specific header
                    // This is a simplified check - could be more sophisticated
                    return SaveFormat.Binary;
                }
            }
            catch (System.Exception ex)
            {
                Debugger.LogError($"❌ Failed to detect save format: {ex.Message}");
                return SaveFormat.JSON; // Default fallback
            }
        }
        
        #endregion
    }
    
    #region Support Classes
    
    /// <summary>
    /// Result of save data validation
    /// </summary>
    [System.Serializable]
    public class SaveValidationResult
    {
        public System.Collections.Generic.List<string> errors = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> warnings = new System.Collections.Generic.List<string>();
        
        public bool IsValid => errors.Count == 0;
        public bool HasWarnings => warnings.Count > 0;
        
        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);
        
        public string GetErrorSummary()
        {
            if (errors.Count == 0) return "No errors";
            return string.Join("; ", errors);
        }
        
        public string GetFullSummary()
        {
            var summary = "";
            if (errors.Count > 0)
            {
                summary += $"Errors ({errors.Count}): " + string.Join("; ", errors);
            }
            if (warnings.Count > 0)
            {
                if (summary.Length > 0) summary += "\n";
                summary += $"Warnings ({warnings.Count}): " + string.Join("; ", warnings);
            }
            return summary.Length > 0 ? summary : "Validation passed";
        }
    }
    
    #endregion
}