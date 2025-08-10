using UnityEngine;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;

namespace NeonLadder.DataManagement
{
    /// <summary>
    /// Simple integration layer between EnhancedSaveSystem and Steam Cloud.
    /// Automatically handles cloud synchronization when saving/loading.
    /// </summary>
    public static class SteamCloudIntegration
    {
        private static bool useSteamCloud = true;
        
        /// <summary>
        /// Enable or disable Steam Cloud integration
        /// </summary>
        public static bool UseSteamCloud
        {
            get => useSteamCloud;
            set => useSteamCloud = value;
        }
        
        /// <summary>
        /// Save with automatic Steam Cloud sync
        /// </summary>
        public static bool SaveWithCloud(ConsolidatedSaveData saveData)
        {
            if (!useSteamCloud || SteamCloudSaveManager.Instance == null)
            {
                // Fallback to local save only
                return EnhancedSaveSystem.Save(saveData);
            }
            
            // Use Steam Cloud manager for saving
            return SteamCloudSaveManager.Instance.SaveToCloud(saveData);
        }
        
        /// <summary>
        /// Load with automatic Steam Cloud sync
        /// </summary>
        public static ConsolidatedSaveData LoadWithCloud()
        {
            if (!useSteamCloud || SteamCloudSaveManager.Instance == null)
            {
                // Fallback to local load only
                return EnhancedSaveSystem.Load();
            }
            
            // Use Steam Cloud manager for loading
            return SteamCloudSaveManager.Instance.LoadFromCloud();
        }
        
        /// <summary>
        /// Delete all saves including cloud
        /// </summary>
        public static bool DeleteAllSaves()
        {
            bool localDeleted = EnhancedSaveSystem.DeleteSave();
            bool cloudDeleted = true;
            
            if (useSteamCloud && SteamCloudSaveManager.Instance != null)
            {
                cloudDeleted = SteamCloudSaveManager.Instance.DeleteCloudSaves();
            }
            
            return localDeleted && cloudDeleted;
        }
        
        /// <summary>
        /// Force sync with Steam Cloud
        /// </summary>
        public static void ForceSyncWithCloud()
        {
            if (useSteamCloud && SteamCloudSaveManager.Instance != null)
            {
                SteamCloudSaveManager.Instance.ForceCloudSync();
            }
            else
            {
                Debugger.LogWarning("[SteamCloudIntegration] Cannot sync - Steam Cloud not available");
            }
        }
        
        /// <summary>
        /// Check if Steam Cloud is available and enabled
        /// </summary>
        public static bool IsCloudAvailable()
        {
            return useSteamCloud && 
                   SteamCloudSaveManager.Instance != null && 
                   Steamworks.SteamManager.Initialized;
        }
        
        /// <summary>
        /// Get cloud storage information
        /// </summary>
        public static (bool available, ulong totalBytes, ulong usedBytes) GetCloudStorageInfo()
        {
            if (!IsCloudAvailable())
            {
                return (false, 0, 0);
            }
            
            ulong total, available;
            Steamworks.SteamRemoteStorage.GetQuota(out total, out available);
            ulong used = total - available;
            
            return (true, total, used);
        }
    }
}