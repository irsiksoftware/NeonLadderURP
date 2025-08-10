using System;
using System.Text;
using UnityEngine;
using Steamworks;
using NeonLadder.Debugging;
using NeonLadderURP.DataManagement;
using System.Collections;

namespace NeonLadder.DataManagement
{
    /// <summary>
    /// Manages Steam Cloud save synchronization for NeonLadder.
    /// Provides automatic cloud backup and conflict resolution.
    /// </summary>
    public class SteamCloudSaveManager : MonoBehaviour
    {
        private static SteamCloudSaveManager instance;
        public static SteamCloudSaveManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SteamCloudSaveManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SteamCloudSaveManager");
                        instance = go.AddComponent<SteamCloudSaveManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Configuration")]
        [SerializeField] private bool autoSyncEnabled = true;
        [SerializeField] private bool verboseLogging = false;
        [SerializeField] private float syncCheckInterval = 30f; // Check for cloud updates every 30 seconds
        
        [Header("File Names")]
        private const string SAVE_FILE_NAME = "NeonLadderSave.dat";
        private const string BACKUP_FILE_NAME = "NeonLadderSave_Backup.dat";
        private const string METADATA_FILE_NAME = "NeonLadderSave_Meta.dat";
        
        [Header("Status")]
        [SerializeField] private bool steamInitialized = false;
        [SerializeField] private bool cloudEnabled = false;
        [SerializeField] private ulong totalCloudBytes = 0;
        [SerializeField] private ulong availableCloudBytes = 0;
        
        // Events
        public static event Action<bool> OnCloudSyncComplete;
        public static event Action<ConflictResolution> OnConflictResolved;
        public static event Action<string> OnCloudError;
        
        // Conflict resolution delegate
        public delegate ConflictResolution ConflictResolver(SaveFileMetadata local, SaveFileMetadata cloud);
        public ConflictResolver conflictResolver;
        
        public enum ConflictResolution
        {
            UseLocal,
            UseCloud,
            Cancel
        }
        
        [Serializable]
        public class SaveFileMetadata
        {
            public DateTime timestamp;
            public string gameVersion;
            public int playTime;
            public int playerLevel;
            public string checksum;
            
            public static SaveFileMetadata FromSaveData(ConsolidatedSaveData saveData)
            {
                return new SaveFileMetadata
                {
                    timestamp = saveData.lastSaved,
                    gameVersion = saveData.gameVersion,
                    playTime = Mathf.RoundToInt(saveData.totalPlayTime),
                    playerLevel = saveData.progression.playerLevel,
                    checksum = ComputeChecksum(JsonUtility.ToJson(saveData))
                };
            }
            
            private static string ComputeChecksum(string data)
            {
                // Simple checksum for validation
                int hash = 0;
                foreach (char c in data)
                {
                    hash = ((hash << 5) - hash) + c;
                }
                return hash.ToString("X8");
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSteamCloud();
        }
        
        private void Start()
        {
            if (autoSyncEnabled && steamInitialized)
            {
                StartCoroutine(AutoSyncRoutine());
            }
        }
        
        private void InitializeSteamCloud()
        {
            try
            {
                if (!SteamManager.Initialized)
                {
                    LogMessage("Steam is not initialized. Cloud saves disabled.", true);
                    steamInitialized = false;
                    return;
                }
                
                steamInitialized = true;
                
                // Check if Steam Cloud is enabled for this user/game
                cloudEnabled = SteamRemoteStorage.IsCloudEnabledForAccount() && 
                              SteamRemoteStorage.IsCloudEnabledForApp();
                
                if (!cloudEnabled)
                {
                    LogMessage("Steam Cloud is disabled for this account or app.", true);
                    return;
                }
                
                // Get cloud storage quota
                SteamRemoteStorage.GetQuota(out totalCloudBytes, out availableCloudBytes);
                
                LogMessage($"Steam Cloud initialized. Available space: {availableCloudBytes / 1024}KB / {totalCloudBytes / 1024}KB");
                
                // Check for existing cloud saves on startup
                CheckForCloudSaves();
            }
            catch (Exception ex)
            {
                LogError($"Failed to initialize Steam Cloud: {ex.Message}");
                steamInitialized = false;
            }
        }
        
        #region Public API
        
        /// <summary>
        /// Saves data to both local storage and Steam Cloud
        /// </summary>
        public bool SaveToCloud(ConsolidatedSaveData saveData)
        {
            if (!CanUseSteamCloud())
            {
                LogMessage("Steam Cloud not available, saving locally only.");
                return EnhancedSaveSystem.Save(saveData);
            }
            
            try
            {
                // First save locally
                bool localSaveSuccess = EnhancedSaveSystem.Save(saveData);
                if (!localSaveSuccess)
                {
                    LogError("Failed to save locally, aborting cloud save.");
                    return false;
                }
                
                // Prepare save data for cloud
                string jsonData = JsonUtility.ToJson(saveData, true);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
                
                // Check if we have enough cloud space
                if (dataBytes.Length > availableCloudBytes)
                {
                    LogError($"Not enough Steam Cloud space. Need {dataBytes.Length} bytes, have {availableCloudBytes} bytes.");
                    return localSaveSuccess; // At least we saved locally
                }
                
                // Write main save file to cloud
                bool cloudSaveSuccess = SteamRemoteStorage.FileWrite(SAVE_FILE_NAME, dataBytes, dataBytes.Length);
                
                if (cloudSaveSuccess)
                {
                    // Also save metadata
                    SaveMetadataToCloud(SaveFileMetadata.FromSaveData(saveData));
                    
                    LogMessage($"Saved to Steam Cloud successfully ({dataBytes.Length} bytes)");
                    OnCloudSyncComplete?.Invoke(true);
                }
                else
                {
                    LogError("Failed to write to Steam Cloud.");
                }
                
                return localSaveSuccess && cloudSaveSuccess;
            }
            catch (Exception ex)
            {
                LogError($"Cloud save failed: {ex.Message}");
                OnCloudError?.Invoke(ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Loads data from Steam Cloud, handling conflicts if necessary
        /// </summary>
        public ConsolidatedSaveData LoadFromCloud()
        {
            if (!CanUseSteamCloud())
            {
                LogMessage("Steam Cloud not available, loading locally only.");
                return EnhancedSaveSystem.Load();
            }
            
            try
            {
                // Check if cloud save exists
                if (!SteamRemoteStorage.FileExists(SAVE_FILE_NAME))
                {
                    LogMessage("No cloud save found, loading local save.");
                    return EnhancedSaveSystem.Load();
                }
                
                // Load cloud save
                int fileSize = SteamRemoteStorage.GetFileSize(SAVE_FILE_NAME);
                byte[] dataBytes = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(SAVE_FILE_NAME, dataBytes, fileSize);
                
                if (bytesRead != fileSize)
                {
                    LogError($"Cloud save read error. Expected {fileSize} bytes, got {bytesRead} bytes.");
                    return EnhancedSaveSystem.Load();
                }
                
                string jsonData = Encoding.UTF8.GetString(dataBytes);
                ConsolidatedSaveData cloudSave = JsonUtility.FromJson<ConsolidatedSaveData>(jsonData);
                
                // Load local save for comparison
                ConsolidatedSaveData localSave = EnhancedSaveSystem.Load();
                
                // Handle conflicts
                if (localSave != null && NeedsConflictResolution(localSave, cloudSave))
                {
                    return ResolveConflict(localSave, cloudSave);
                }
                
                // Use cloud save if no conflict or no local save
                LogMessage("Loaded from Steam Cloud successfully");
                OnCloudSyncComplete?.Invoke(true);
                return cloudSave;
            }
            catch (Exception ex)
            {
                LogError($"Cloud load failed: {ex.Message}");
                OnCloudError?.Invoke(ex.Message);
                return EnhancedSaveSystem.Load(); // Fallback to local
            }
        }
        
        /// <summary>
        /// Deletes cloud saves
        /// </summary>
        public bool DeleteCloudSaves()
        {
            if (!CanUseSteamCloud())
            {
                return false;
            }
            
            bool success = true;
            
            if (SteamRemoteStorage.FileExists(SAVE_FILE_NAME))
                success &= SteamRemoteStorage.FileDelete(SAVE_FILE_NAME);
                
            if (SteamRemoteStorage.FileExists(BACKUP_FILE_NAME))
                success &= SteamRemoteStorage.FileDelete(BACKUP_FILE_NAME);
                
            if (SteamRemoteStorage.FileExists(METADATA_FILE_NAME))
                success &= SteamRemoteStorage.FileDelete(METADATA_FILE_NAME);
            
            if (success)
            {
                LogMessage("Cloud saves deleted successfully");
            }
            else
            {
                LogError("Failed to delete some cloud saves");
            }
            
            return success;
        }
        
        /// <summary>
        /// Forces a sync with Steam Cloud
        /// </summary>
        public void ForceCloudSync()
        {
            if (!CanUseSteamCloud())
            {
                LogMessage("Cannot sync - Steam Cloud not available");
                return;
            }
            
            StartCoroutine(PerformCloudSync());
        }
        
        #endregion
        
        #region Private Methods
        
        private bool CanUseSteamCloud()
        {
            return steamInitialized && cloudEnabled && SteamManager.Initialized;
        }
        
        private void CheckForCloudSaves()
        {
            if (!CanUseSteamCloud())
                return;
            
            int fileCount = SteamRemoteStorage.GetFileCount();
            LogMessage($"Found {fileCount} files in Steam Cloud");
            
            for (int i = 0; i < fileCount; i++)
            {
                int fileSize;
                string fileName = SteamRemoteStorage.GetFileNameAndSize(i, out fileSize);
                LogMessage($"  - {fileName} ({fileSize} bytes)");
            }
        }
        
        private bool NeedsConflictResolution(ConsolidatedSaveData local, ConsolidatedSaveData cloud)
        {
            // If timestamps are significantly different (> 1 minute), we have a conflict
            TimeSpan timeDiff = local.lastSaved - cloud.lastSaved;
            return Math.Abs(timeDiff.TotalMinutes) > 1;
        }
        
        private ConsolidatedSaveData ResolveConflict(ConsolidatedSaveData local, ConsolidatedSaveData cloud)
        {
            var localMeta = SaveFileMetadata.FromSaveData(local);
            var cloudMeta = SaveFileMetadata.FromSaveData(cloud);
            
            ConflictResolution resolution;
            
            if (conflictResolver != null)
            {
                // Use custom resolver if provided
                resolution = conflictResolver(localMeta, cloudMeta);
            }
            else
            {
                // Default: Use most recent
                resolution = local.lastSaved > cloud.lastSaved ? 
                           ConflictResolution.UseLocal : 
                           ConflictResolution.UseCloud;
                
                LogMessage($"Auto-resolving conflict: Using {resolution}");
            }
            
            OnConflictResolved?.Invoke(resolution);
            
            switch (resolution)
            {
                case ConflictResolution.UseLocal:
                    // Upload local to cloud
                    SaveToCloud(local);
                    return local;
                    
                case ConflictResolution.UseCloud:
                    // Save cloud version locally
                    EnhancedSaveSystem.Save(cloud);
                    return cloud;
                    
                default:
                    // Cancel - use local without syncing
                    return local;
            }
        }
        
        private void SaveMetadataToCloud(SaveFileMetadata metadata)
        {
            try
            {
                string metaJson = JsonUtility.ToJson(metadata);
                byte[] metaBytes = Encoding.UTF8.GetBytes(metaJson);
                SteamRemoteStorage.FileWrite(METADATA_FILE_NAME, metaBytes, metaBytes.Length);
            }
            catch (Exception ex)
            {
                LogError($"Failed to save metadata: {ex.Message}");
            }
        }
        
        private SaveFileMetadata LoadMetadataFromCloud()
        {
            try
            {
                if (!SteamRemoteStorage.FileExists(METADATA_FILE_NAME))
                    return null;
                
                int fileSize = SteamRemoteStorage.GetFileSize(METADATA_FILE_NAME);
                byte[] metaBytes = new byte[fileSize];
                SteamRemoteStorage.FileRead(METADATA_FILE_NAME, metaBytes, fileSize);
                
                string metaJson = Encoding.UTF8.GetString(metaBytes);
                return JsonUtility.FromJson<SaveFileMetadata>(metaJson);
            }
            catch (Exception ex)
            {
                LogError($"Failed to load metadata: {ex.Message}");
                return null;
            }
        }
        
        private IEnumerator AutoSyncRoutine()
        {
            while (autoSyncEnabled)
            {
                yield return new WaitForSeconds(syncCheckInterval);
                
                if (CanUseSteamCloud())
                {
                    yield return StartCoroutine(PerformCloudSync());
                }
            }
        }
        
        private IEnumerator PerformCloudSync()
        {
            LogMessage("Performing cloud sync...");
            
            // This is a simplified sync - in production you'd want more sophisticated logic
            var localSave = EnhancedSaveSystem.Load();
            if (localSave != null)
            {
                var cloudMeta = LoadMetadataFromCloud();
                var localMeta = SaveFileMetadata.FromSaveData(localSave);
                
                // Only upload if local is newer or cloud doesn't exist
                if (cloudMeta == null || localMeta.timestamp > cloudMeta.timestamp)
                {
                    SaveToCloud(localSave);
                }
            }
            
            yield return null;
        }
        
        #endregion
        
        #region Logging
        
        private void LogMessage(string message, bool forceLog = false)
        {
            if (verboseLogging || forceLog)
            {
                Debugger.Log($"[SteamCloud] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[SteamCloud] {message}");
        }
        
        #endregion
        
        #region Steam Callbacks
        
        private void OnEnable()
        {
            // Subscribe to Steam callbacks if needed
            if (SteamManager.Initialized)
            {
                // You can add Steam event callbacks here
            }
        }
        
        private void OnDisable()
        {
            // Unsubscribe from Steam callbacks
        }
        
        #endregion
    }
}