using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using NeonLadder.Debugging;
using NeonLadder.Managers;
using NeonLadderURP.DataManagement;

#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace NeonLadder.DataManagement
{
    /// <summary>
    /// Manages Steam Cloud save synchronization for NeonLadder.
    /// Handles automatic cloud backup, conflict resolution, and multi-device sync.
    /// </summary>
    public class SteamCloudSaveManager : MonoBehaviour
    {
        #region Singleton
        
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
        
        #endregion
        
        #region Configuration
        
        [Header("Cloud Save Settings")]
        [SerializeField] private bool enableCloudSaves = true;
        [SerializeField] private bool autoSyncOnSave = true;
        [SerializeField] private bool autoSyncOnLoad = true;
        [SerializeField] private float syncRetryDelay = 2f;
        [SerializeField] private int maxRetryAttempts = 3;
        
        [Header("File Management")]
        [SerializeField] private string primarySaveFileName = "neonladder_save.json";
        [SerializeField] private string backupSaveFileName = "neonladder_save_backup.json";
        [SerializeField] private string metadataFileName = "neonladder_save_meta.json";
        
        [Header("Conflict Resolution")]
        [SerializeField] private bool autoResolveConflicts = false;
        [SerializeField] private ConflictResolutionStrategy defaultStrategy = ConflictResolutionStrategy.PreferNewest;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        private const string SAVE_FILE_NAME = "neonladder_save.json";
        private const string BACKUP_FILE_NAME = "neonladder_save_backup.json";
        private const string METADATA_FILE_NAME = "neonladder_save_meta.json";
        
        private bool isSteamInitialized = false;
        private bool isSyncing = false;
        private int currentRetryCount = 0;
        
        private ConsolidatedSaveData pendingSaveData;
        private SaveMetadata localMetadata;
        private SaveMetadata cloudMetadata;
        
        #endregion
        
        #region Events
        
        public static event Action<bool> OnCloudSyncStarted;
        public static event Action<bool> OnCloudSyncCompleted;
        public static event Action<string> OnCloudSyncError;
        public static event Action<SaveConflict> OnSaveConflictDetected;
        public static event Action<ConflictResolution> OnConflictResolved;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Initialize()
        {
#if !DISABLESTEAMWORKS
            // Check if Steam is initialized
            if (SteamManager.Initialized)
            {
                isSteamInitialized = true;
                LogDebug("Steam Cloud Save Manager initialized successfully");
                
                // Check if Steam Cloud is enabled for this app
                if (SteamRemoteStorage.IsCloudEnabledForAccount() && SteamRemoteStorage.IsCloudEnabledForApp())
                {
                    LogDebug("Steam Cloud is enabled for account and app");
                }
                else
                {
                    LogWarning("Steam Cloud is disabled for account or app");
                    enableCloudSaves = false;
                }
            }
            else
            {
                LogWarning("Steam is not initialized, cloud saves disabled");
                enableCloudSaves = false;
            }
#else
            LogWarning("Steamworks is disabled, cloud saves unavailable");
            enableCloudSaves = false;
#endif
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveCompleted += HandleLocalSaveCompleted;
            EnhancedSaveSystem.OnLoadRequested += HandleLoadRequested;
        }
        
        private void OnDestroy()
        {
            EnhancedSaveSystem.OnSaveCompleted -= HandleLocalSaveCompleted;
            EnhancedSaveSystem.OnLoadRequested -= HandleLoadRequested;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Manually sync save data to Steam Cloud
        /// </summary>
        public bool SaveToCloud(ConsolidatedSaveData saveData)
        {
            if (!CanUseCloudSaves())
            {
                LogWarning("Cannot save to cloud - Steam not initialized or cloud saves disabled");
                return false;
            }
            
            if (isSyncing)
            {
                LogWarning("Already syncing, please wait");
                return false;
            }
            
#if !DISABLESTEAMWORKS
            try
            {
                isSyncing = true;
                OnCloudSyncStarted?.Invoke(true);
                
                // Serialize save data
                string jsonData = JsonUtility.ToJson(saveData, true);
                byte[] dataBytes = Encoding.UTF8.GetBytes(jsonData);
                
                // Write primary save file
                bool success = SteamRemoteStorage.FileWrite(SAVE_FILE_NAME, dataBytes, dataBytes.Length);
                
                if (success)
                {
                    // Create and upload metadata
                    var metadata = CreateMetadata(saveData);
                    string metaJson = JsonUtility.ToJson(metadata, true);
                    byte[] metaBytes = Encoding.UTF8.GetBytes(metaJson);
                    SteamRemoteStorage.FileWrite(METADATA_FILE_NAME, metaBytes, metaBytes.Length);
                    
                    // Create backup
                    SteamRemoteStorage.FileWrite(BACKUP_FILE_NAME, dataBytes, dataBytes.Length);
                    
                    LogDebug($"Successfully saved to Steam Cloud ({dataBytes.Length} bytes)");
                    OnCloudSyncCompleted?.Invoke(true);
                    return true;
                }
                else
                {
                    LogError("Failed to write save file to Steam Cloud");
                    OnCloudSyncError?.Invoke("Failed to write to Steam Cloud");
                    return false;
                }
            }
            catch (Exception e)
            {
                LogError($"Exception during cloud save: {e.Message}");
                OnCloudSyncError?.Invoke(e.Message);
                return false;
            }
            finally
            {
                isSyncing = false;
            }
#else
            return false;
#endif
        }
        
        /// <summary>
        /// Load save data from Steam Cloud
        /// </summary>
        public ConsolidatedSaveData LoadFromCloud()
        {
            if (!CanUseCloudSaves())
            {
                LogWarning("Cannot load from cloud - Steam not initialized or cloud saves disabled");
                return null;
            }
            
#if !DISABLESTEAMWORKS
            try
            {
                OnCloudSyncStarted?.Invoke(false);
                
                // Check if save file exists in cloud
                if (!SteamRemoteStorage.FileExists(SAVE_FILE_NAME))
                {
                    LogDebug("No cloud save file found");
                    OnCloudSyncCompleted?.Invoke(false);
                    return null;
                }
                
                // Get file size
                int fileSize = SteamRemoteStorage.GetFileSize(SAVE_FILE_NAME);
                if (fileSize <= 0)
                {
                    LogWarning("Cloud save file is empty");
                    OnCloudSyncCompleted?.Invoke(false);
                    return null;
                }
                
                // Read file data
                byte[] dataBytes = new byte[fileSize];
                int bytesRead = SteamRemoteStorage.FileRead(SAVE_FILE_NAME, dataBytes, fileSize);
                
                if (bytesRead != fileSize)
                {
                    LogError($"Failed to read complete file. Expected {fileSize}, got {bytesRead}");
                    OnCloudSyncError?.Invoke("Incomplete file read from cloud");
                    return null;
                }
                
                // Deserialize save data
                string jsonData = Encoding.UTF8.GetString(dataBytes);
                ConsolidatedSaveData saveData = JsonUtility.FromJson<ConsolidatedSaveData>(jsonData);
                
                // Load metadata if available
                if (SteamRemoteStorage.FileExists(METADATA_FILE_NAME))
                {
                    LoadCloudMetadata();
                }
                
                LogDebug($"Successfully loaded from Steam Cloud ({fileSize} bytes)");
                OnCloudSyncCompleted?.Invoke(false);
                return saveData;
            }
            catch (Exception e)
            {
                LogError($"Exception during cloud load: {e.Message}");
                OnCloudSyncError?.Invoke(e.Message);
                return null;
            }
#else
            return null;
#endif
        }
        
        /// <summary>
        /// Check for save conflicts between local and cloud
        /// </summary>
        public SaveConflict CheckForConflicts()
        {
            if (!CanUseCloudSaves())
                return null;
            
            // Load local save and metadata
            var localSave = EnhancedSaveSystem.Load();
            if (localSave == null)
                return null;
            
            localMetadata = CreateMetadata(localSave);
            
            // Load cloud save and metadata
            var cloudSave = LoadFromCloud();
            if (cloudSave == null)
                return null;
            
            LoadCloudMetadata();
            
            // Compare saves
            if (IsConflict(localMetadata, cloudMetadata))
            {
                var conflict = new SaveConflict
                {
                    localSave = localSave,
                    cloudSave = cloudSave,
                    localMetadata = localMetadata,
                    cloudMetadata = cloudMetadata,
                    detectedAt = DateTime.Now
                };
                
                OnSaveConflictDetected?.Invoke(conflict);
                return conflict;
            }
            
            return null;
        }
        
        /// <summary>
        /// Resolve save conflict with user choice
        /// </summary>
        public void ResolveConflict(SaveConflict conflict, ConflictChoice choice)
        {
            if (conflict == null)
                return;
            
            ConsolidatedSaveData chosenSave = null;
            
            switch (choice)
            {
                case ConflictChoice.KeepLocal:
                    chosenSave = conflict.localSave;
                    // Upload local to cloud
                    SaveToCloud(chosenSave);
                    break;
                    
                case ConflictChoice.KeepCloud:
                    chosenSave = conflict.cloudSave;
                    // Save cloud version locally
                    EnhancedSaveSystem.Save(chosenSave);
                    break;
                    
                case ConflictChoice.KeepNewer:
                    if (conflict.localMetadata.lastSaved > conflict.cloudMetadata.lastSaved)
                    {
                        chosenSave = conflict.localSave;
                        SaveToCloud(chosenSave);
                    }
                    else
                    {
                        chosenSave = conflict.cloudSave;
                        EnhancedSaveSystem.Save(chosenSave);
                    }
                    break;
            }
            
            var resolution = new ConflictResolution
            {
                conflict = conflict,
                choice = choice,
                resolvedSave = chosenSave,
                resolvedAt = DateTime.Now
            };
            
            OnConflictResolved?.Invoke(resolution);
            LogDebug($"Conflict resolved: {choice}");
        }
        
        /// <summary>
        /// Get Steam Cloud usage information
        /// </summary>
        public CloudStorageInfo GetCloudStorageInfo()
        {
#if !DISABLESTEAMWORKS
            if (!CanUseCloudSaves())
                return new CloudStorageInfo();
            
            ulong totalBytes;
            ulong availableBytes;
            SteamRemoteStorage.GetQuota(out totalBytes, out availableBytes);
            
            int fileCount = SteamRemoteStorage.GetFileCount();
            long usedBytes = 0;
            
            for (int i = 0; i < fileCount; i++)
            {
                int fileSize;
                string fileName;
                SteamRemoteStorage.GetFileNameAndSize(i, out fileSize, out fileName);
                usedBytes += fileSize;
            }
            
            return new CloudStorageInfo
            {
                totalBytes = (long)totalBytes,
                usedBytes = usedBytes,
                availableBytes = (long)availableBytes,
                fileCount = fileCount,
                isEnabled = enableCloudSaves
            };
#else
            return new CloudStorageInfo();
#endif
        }
        
        #endregion
        
        #region Private Methods
        
        private bool CanUseCloudSaves()
        {
#if !DISABLESTEAMWORKS
            return enableCloudSaves && isSteamInitialized && SteamManager.Initialized;
#else
            return false;
#endif
        }
        
        private SaveMetadata CreateMetadata(ConsolidatedSaveData saveData)
        {
            return new SaveMetadata
            {
                version = saveData.version,
                lastSaved = saveData.lastSaved,
                totalPlayTime = saveData.totalPlayTime,
                playerLevel = saveData.progression.playerLevel,
                currentScene = saveData.worldState.currentSceneName,
                metaCurrency = saveData.currencies.metaCurrency,
                permaCurrency = saveData.currencies.permaCurrency,
                saveId = Guid.NewGuid().ToString(),
                steamId = GetSteamId()
            };
        }
        
        private void LoadCloudMetadata()
        {
#if !DISABLESTEAMWORKS
            if (!SteamRemoteStorage.FileExists(METADATA_FILE_NAME))
            {
                cloudMetadata = null;
                return;
            }
            
            int fileSize = SteamRemoteStorage.GetFileSize(METADATA_FILE_NAME);
            byte[] dataBytes = new byte[fileSize];
            int bytesRead = SteamRemoteStorage.FileRead(METADATA_FILE_NAME, dataBytes, fileSize);
            
            if (bytesRead == fileSize)
            {
                string jsonData = Encoding.UTF8.GetString(dataBytes);
                cloudMetadata = JsonUtility.FromJson<SaveMetadata>(jsonData);
            }
#endif
        }
        
        private bool IsConflict(SaveMetadata local, SaveMetadata cloud)
        {
            if (local == null || cloud == null)
                return false;
            
            // Check if saves are from different sessions
            if (local.saveId == cloud.saveId)
                return false;
            
            // Check for significant differences
            bool timeDifference = Math.Abs((local.lastSaved - cloud.lastSaved).TotalMinutes) > 1;
            bool progressDifference = Math.Abs(local.playerLevel - cloud.playerLevel) > 0;
            bool currencyDifference = local.metaCurrency != cloud.metaCurrency || 
                                     local.permaCurrency != cloud.permaCurrency;
            
            return timeDifference || progressDifference || currencyDifference;
        }
        
        private string GetSteamId()
        {
#if !DISABLESTEAMWORKS
            if (SteamManager.Initialized)
            {
                return SteamUser.GetSteamID().ToString();
            }
#endif
            return "offline";
        }
        
        private IEnumerator RetryCloudOperation(Action operation)
        {
            currentRetryCount = 0;
            
            while (currentRetryCount < maxRetryAttempts)
            {
                try
                {
                    operation?.Invoke();
                    yield break; // Success
                }
                catch (Exception e)
                {
                    currentRetryCount++;
                    LogWarning($"Cloud operation failed (attempt {currentRetryCount}/{maxRetryAttempts}): {e.Message}");
                    
                    if (currentRetryCount < maxRetryAttempts)
                    {
                        yield return new WaitForSeconds(syncRetryDelay);
                    }
                }
            }
            
            OnCloudSyncError?.Invoke($"Failed after {maxRetryAttempts} attempts");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleLocalSaveCompleted(ConsolidatedSaveData saveData)
        {
            if (autoSyncOnSave && CanUseCloudSaves())
            {
                StartCoroutine(SyncToCloudCoroutine(saveData));
            }
        }
        
        private void HandleLoadRequested()
        {
            if (autoSyncOnLoad && CanUseCloudSaves())
            {
                var conflict = CheckForConflicts();
                if (conflict != null && !autoResolveConflicts)
                {
                    // Let UI handle conflict resolution
                    return;
                }
                
                if (conflict != null && autoResolveConflicts)
                {
                    // Auto-resolve based on strategy
                    ConflictChoice choice = ConflictChoice.KeepNewer;
                    if (defaultStrategy == ConflictResolutionStrategy.PreferLocal)
                        choice = ConflictChoice.KeepLocal;
                    else if (defaultStrategy == ConflictResolutionStrategy.PreferCloud)
                        choice = ConflictChoice.KeepCloud;
                    
                    ResolveConflict(conflict, choice);
                }
            }
        }
        
        private IEnumerator SyncToCloudCoroutine(ConsolidatedSaveData saveData)
        {
            yield return new WaitForSeconds(0.5f); // Small delay to ensure local save is complete
            
            pendingSaveData = saveData;
            yield return StartCoroutine(RetryCloudOperation(() => SaveToCloud(pendingSaveData)));
        }
        
        #endregion
        
        #region Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debugger.Log($"[SteamCloud] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debugger.LogWarning($"[SteamCloud] {message}");
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[SteamCloud] {message}");
        }
        
        #endregion
    }
    
    #region Data Classes
    
    /// <summary>
    /// Save file metadata for conflict detection
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public int version;
        public DateTime lastSaved;
        public float totalPlayTime;
        public int playerLevel;
        public string currentScene;
        public int metaCurrency;
        public int permaCurrency;
        public string saveId;
        public string steamId;
    }
    
    /// <summary>
    /// Represents a save conflict between local and cloud
    /// </summary>
    [Serializable]
    public class SaveConflict
    {
        public ConsolidatedSaveData localSave;
        public ConsolidatedSaveData cloudSave;
        public SaveMetadata localMetadata;
        public SaveMetadata cloudMetadata;
        public DateTime detectedAt;
        
        public string GetConflictSummary()
        {
            var localTime = localMetadata?.lastSaved ?? DateTime.MinValue;
            var cloudTime = cloudMetadata?.lastSaved ?? DateTime.MinValue;
            
            return $"Local: Level {localMetadata?.playerLevel ?? 0}, saved {localTime:g}\n" +
                   $"Cloud: Level {cloudMetadata?.playerLevel ?? 0}, saved {cloudTime:g}";
        }
    }
    
    /// <summary>
    /// Resolution of a save conflict
    /// </summary>
    [Serializable]
    public class ConflictResolution
    {
        public SaveConflict conflict;
        public ConflictChoice choice;
        public ConsolidatedSaveData resolvedSave;
        public DateTime resolvedAt;
    }
    
    /// <summary>
    /// Cloud storage usage information
    /// </summary>
    [Serializable]
    public class CloudStorageInfo
    {
        public long totalBytes;
        public long usedBytes;
        public long availableBytes;
        public int fileCount;
        public bool isEnabled;
        
        public float GetUsagePercentage()
        {
            if (totalBytes <= 0) return 0;
            return (float)usedBytes / totalBytes * 100f;
        }
    }
    
    /// <summary>
    /// Conflict resolution strategies
    /// </summary>
    public enum ConflictResolutionStrategy
    {
        PreferNewest,
        PreferLocal,
        PreferCloud,
        AskUser
    }
    
    /// <summary>
    /// User choices for conflict resolution
    /// </summary>
    public enum ConflictChoice
    {
        KeepLocal,
        KeepCloud,
        KeepNewer
    }
    
    #endregion
}