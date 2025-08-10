using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.ProceduralGeneration;

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Handles migration of save data from old formats to the new consolidated format.
    /// Ensures backward compatibility when loading legacy saves.
    /// </summary>
    public static class SaveMigrationSystem
    {
        #region Configuration
        
        private const int CURRENT_SAVE_VERSION = 3;
        private const string BACKUP_FOLDER = "SaveBackups";
        private static readonly string BackupPath = Path.Combine(Application.persistentDataPath, BACKUP_FOLDER);
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Migrate save data to the current version
        /// </summary>
        public static ConsolidatedSaveData MigrateSaveData(ConsolidatedSaveData saveData)
        {
            if (saveData == null)
            {
                Debugger.LogWarning("[SaveMigration] No save data to migrate");
                return null;
            }
            
            // Check if migration is needed
            if (saveData.version >= CURRENT_SAVE_VERSION)
            {
                Debugger.Log($"[SaveMigration] Save data already at current version {CURRENT_SAVE_VERSION}");
                return saveData;
            }
            
            // Create backup before migration
            CreateBackup(saveData);
            
            // Perform migration based on version
            while (saveData.version < CURRENT_SAVE_VERSION)
            {
                saveData = MigrateToNextVersion(saveData);
            }
            
            Debugger.Log($"[SaveMigration] Migration complete. Version: {saveData.version}");
            return saveData;
        }
        
        /// <summary>
        /// Check if a save file needs migration
        /// </summary>
        public static bool NeedsMigration(ConsolidatedSaveData saveData)
        {
            return saveData != null && saveData.version < CURRENT_SAVE_VERSION;
        }
        
        /// <summary>
        /// Attempt to load and migrate a legacy save file
        /// </summary>
        public static ConsolidatedSaveData LoadLegacySave(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Debugger.LogWarning($"[SaveMigration] Legacy save file not found: {filePath}");
                    return null;
                }
                
                string jsonData = File.ReadAllText(filePath);
                
                // Try to deserialize as old format
                var legacyData = TryDeserializeLegacyFormat(jsonData);
                if (legacyData != null)
                {
                    return ConvertLegacyToConsolidated(legacyData);
                }
                
                // Try as current format but old version
                var saveData = JsonUtility.FromJson<ConsolidatedSaveData>(jsonData);
                return MigrateSaveData(saveData);
            }
            catch (Exception e)
            {
                Debugger.LogError($"[SaveMigration] Failed to load legacy save: {e.Message}");
                return null;
            }
        }
        
        #endregion
        
        #region Version Migrations
        
        private static ConsolidatedSaveData MigrateToNextVersion(ConsolidatedSaveData saveData)
        {
            switch (saveData.version)
            {
                case 0:
                    return MigrateV0ToV1(saveData);
                case 1:
                    return MigrateV1ToV2(saveData);
                case 2:
                    return MigrateV2ToV3(saveData);
                default:
                    Debugger.LogWarning($"[SaveMigration] Unknown version: {saveData.version}");
                    saveData.version = CURRENT_SAVE_VERSION;
                    return saveData;
            }
        }
        
        /// <summary>
        /// Migrate from version 0 to version 1
        /// Adds procedural generation state
        /// </summary>
        private static ConsolidatedSaveData MigrateV0ToV1(ConsolidatedSaveData saveData)
        {
            Debugger.Log("[SaveMigration] Migrating from v0 to v1: Adding procedural generation state");
            
            // Initialize procedural state if missing
            if (saveData.worldState == null)
            {
                saveData.worldState = new WorldState();
            }
            
            if (saveData.worldState.proceduralState == null)
            {
                saveData.worldState.proceduralState = new ProceduralGenerationState
                {
                    currentSeed = 0,
                    currentDepth = 0,
                    currentPath = "main",
                    generatedScenes = new List<GeneratedSceneData>(),
                    pathGeneratorState = new Dictionary<string, object>(),
                    isCompleted = false
                };
            }
            
            // Initialize scene transitions if missing
            if (saveData.worldState.sceneTransitions == null)
            {
                saveData.worldState.sceneTransitions = new List<SceneTransition>();
            }
            
            saveData.version = 1;
            return saveData;
        }
        
        /// <summary>
        /// Migrate from version 1 to version 2
        /// Adds dialogue system integration
        /// </summary>
        private static ConsolidatedSaveData MigrateV1ToV2(ConsolidatedSaveData saveData)
        {
            Debugger.Log("[SaveMigration] Migrating from v1 to v2: Adding dialogue system integration");
            
            // Initialize dialogue data if missing
            if (string.IsNullOrEmpty(saveData.dialogueDataJson))
            {
                saveData.dialogueDataJson = "{}";
            }
            
            // Initialize NPC relationships if missing
            if (saveData.npcRelationships == null)
            {
                saveData.npcRelationships = new Dictionary<string, int>();
            }
            
            // Initialize story flags if missing
            if (saveData.storyFlags == null)
            {
                saveData.storyFlags = new Dictionary<string, bool>();
            }
            
            saveData.version = 2;
            return saveData;
        }
        
        /// <summary>
        /// Migrate from version 2 to version 3
        /// Adds Steam Cloud integration and enhanced procedural features
        /// </summary>
        private static ConsolidatedSaveData MigrateV2ToV3(ConsolidatedSaveData saveData)
        {
            Debugger.Log("[SaveMigration] Migrating from v2 to v3: Adding Steam Cloud and enhanced procedural features");
            
            // Add save metadata if missing
            if (saveData.lastSaved == default(DateTime))
            {
                saveData.lastSaved = DateTime.Now;
            }
            
            if (string.IsNullOrEmpty(saveData.saveId))
            {
                saveData.saveId = Guid.NewGuid().ToString();
            }
            
            // Enhance procedural state with scene set support
            if (saveData.worldState?.proceduralState != null)
            {
                var procState = saveData.worldState.proceduralState;
                
                // Add scene set ID if missing
                if (string.IsNullOrEmpty(procState.sceneSetId))
                {
                    procState.sceneSetId = "default";
                }
                
                // Initialize path generator state if missing
                if (procState.pathGeneratorState == null)
                {
                    procState.pathGeneratorState = new Dictionary<string, object>();
                }
            }
            
            // Add player statistics if missing
            if (saveData.statistics == null)
            {
                saveData.statistics = new PlayerStatistics
                {
                    totalPlayTime = saveData.totalPlayTime,
                    enemiesDefeated = 0,
                    deathCount = 0,
                    roomsCleared = 0,
                    bossesDefeated = new List<string>()
                };
            }
            
            saveData.version = 3;
            return saveData;
        }
        
        #endregion
        
        #region Legacy Format Conversion
        
        /// <summary>
        /// Try to deserialize old save format
        /// </summary>
        private static LegacySaveData TryDeserializeLegacyFormat(string jsonData)
        {
            try
            {
                // Attempt to parse as legacy format
                var legacyData = JsonUtility.FromJson<LegacySaveData>(jsonData);
                
                // Validate it's actually legacy format
                if (legacyData != null && legacyData.playerLevel > 0)
                {
                    return legacyData;
                }
            }
            catch
            {
                // Not legacy format
            }
            
            return null;
        }
        
        /// <summary>
        /// Convert legacy save format to consolidated format
        /// </summary>
        private static ConsolidatedSaveData ConvertLegacyToConsolidated(LegacySaveData legacy)
        {
            Debugger.Log("[SaveMigration] Converting legacy save format to consolidated format");
            
            var consolidated = ConsolidatedSaveData.CreateNew();
            
            // Convert player progression
            consolidated.progression.playerLevel = legacy.playerLevel;
            consolidated.progression.maxHealth = legacy.maxHealth;
            consolidated.progression.currentHealth = legacy.currentHealth;
            consolidated.progression.maxStamina = legacy.maxStamina;
            consolidated.progression.currentStamina = legacy.currentStamina;
            consolidated.progression.attackDamage = legacy.attackDamage;
            consolidated.progression.movementSpeed = legacy.movementSpeed;
            consolidated.progression.jumpCount = legacy.jumpCount;
            
            // Convert currencies
            consolidated.currencies.metaCurrency = legacy.metaCurrency;
            consolidated.currencies.permaCurrency = legacy.permaCurrency;
            
            // Convert world state
            consolidated.worldState.currentSceneName = legacy.currentScene;
            consolidated.worldState.playerPosition = legacy.playerPosition;
            consolidated.worldState.playerRotation = legacy.playerRotation;
            
            // Convert unlocks
            if (legacy.unlockedAbilities != null)
            {
                consolidated.unlocks.unlockedAbilities = new List<string>(legacy.unlockedAbilities);
            }
            
            if (legacy.purchasedItems != null)
            {
                consolidated.unlocks.purchasedItems = new List<string>(legacy.purchasedItems);
            }
            
            // Set initial version
            consolidated.version = 0;
            
            // Now migrate to current version
            return MigrateSaveData(consolidated);
        }
        
        #endregion
        
        #region Backup System
        
        /// <summary>
        /// Create a backup of save data before migration
        /// </summary>
        private static void CreateBackup(ConsolidatedSaveData saveData)
        {
            try
            {
                // Ensure backup directory exists
                if (!Directory.Exists(BackupPath))
                {
                    Directory.CreateDirectory(BackupPath);
                }
                
                // Generate backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFile = Path.Combine(BackupPath, $"save_backup_v{saveData.version}_{timestamp}.json");
                
                // Serialize and save backup
                string jsonData = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(backupFile, jsonData);
                
                Debugger.Log($"[SaveMigration] Created backup: {backupFile}");
                
                // Clean old backups (keep last 10)
                CleanOldBackups();
            }
            catch (Exception e)
            {
                Debugger.LogError($"[SaveMigration] Failed to create backup: {e.Message}");
            }
        }
        
        /// <summary>
        /// Clean old backup files, keeping only the most recent ones
        /// </summary>
        private static void CleanOldBackups()
        {
            try
            {
                if (!Directory.Exists(BackupPath))
                    return;
                
                var backupFiles = Directory.GetFiles(BackupPath, "save_backup_*.json");
                
                if (backupFiles.Length <= 10)
                    return;
                
                // Sort by creation time
                Array.Sort(backupFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));
                
                // Delete oldest files
                int filesToDelete = backupFiles.Length - 10;
                for (int i = 0; i < filesToDelete; i++)
                {
                    File.Delete(backupFiles[i]);
                    Debugger.Log($"[SaveMigration] Deleted old backup: {Path.GetFileName(backupFiles[i])}");
                }
            }
            catch (Exception e)
            {
                Debugger.LogError($"[SaveMigration] Failed to clean old backups: {e.Message}");
            }
        }
        
        /// <summary>
        /// Restore from a backup file
        /// </summary>
        public static ConsolidatedSaveData RestoreFromBackup(string backupFileName)
        {
            try
            {
                string backupPath = Path.Combine(BackupPath, backupFileName);
                
                if (!File.Exists(backupPath))
                {
                    Debugger.LogError($"[SaveMigration] Backup file not found: {backupFileName}");
                    return null;
                }
                
                string jsonData = File.ReadAllText(backupPath);
                var saveData = JsonUtility.FromJson<ConsolidatedSaveData>(jsonData);
                
                // Migrate if needed
                if (NeedsMigration(saveData))
                {
                    saveData = MigrateSaveData(saveData);
                }
                
                Debugger.Log($"[SaveMigration] Restored from backup: {backupFileName}");
                return saveData;
            }
            catch (Exception e)
            {
                Debugger.LogError($"[SaveMigration] Failed to restore backup: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get list of available backup files
        /// </summary>
        public static string[] GetAvailableBackups()
        {
            if (!Directory.Exists(BackupPath))
                return new string[0];
            
            var files = Directory.GetFiles(BackupPath, "save_backup_*.json");
            
            // Return just the filenames, not full paths
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }
            
            return files;
        }
        
        #endregion
        
        #region Legacy Save Data Structure
        
        /// <summary>
        /// Legacy save data format (for migration purposes)
        /// </summary>
        [Serializable]
        private class LegacySaveData
        {
            public int playerLevel;
            public int maxHealth;
            public int currentHealth;
            public int maxStamina;
            public int currentStamina;
            public int attackDamage;
            public float movementSpeed;
            public int jumpCount;
            
            public int metaCurrency;
            public int permaCurrency;
            
            public string currentScene;
            public Vector3 playerPosition;
            public Quaternion playerRotation;
            
            public List<string> unlockedAbilities;
            public List<string> purchasedItems;
            public List<string> defeatedBosses;
            
            public float totalPlayTime;
        }
        
        #endregion
    }
}