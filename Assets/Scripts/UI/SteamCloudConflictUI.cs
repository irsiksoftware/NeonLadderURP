using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonLadder.DataManagement;
using NeonLadderURP.DataManagement;

namespace NeonLadder.UI
{
    /// <summary>
    /// UI component for resolving Steam Cloud save conflicts.
    /// Presents user with clear choice between local and cloud saves.
    /// </summary>
    public class SteamCloudConflictUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject conflictPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Local Save Display")]
        [SerializeField] private GameObject localSaveInfo;
        [SerializeField] private TextMeshProUGUI localTimestampText;
        [SerializeField] private TextMeshProUGUI localPlayTimeText;
        [SerializeField] private TextMeshProUGUI localLevelText;
        [SerializeField] private TextMeshProUGUI localProgressText;
        [SerializeField] private Button useLocalButton;
        
        [Header("Cloud Save Display")]
        [SerializeField] private GameObject cloudSaveInfo;
        [SerializeField] private TextMeshProUGUI cloudTimestampText;
        [SerializeField] private TextMeshProUGUI cloudPlayTimeText;
        [SerializeField] private TextMeshProUGUI cloudLevelText;
        [SerializeField] private TextMeshProUGUI cloudProgressText;
        [SerializeField] private Button useCloudButton;
        
        [Header("Additional Options")]
        [SerializeField] private Button cancelButton;
        [SerializeField] private Toggle rememberChoiceToggle;
        [SerializeField] private TextMeshProUGUI warningText;
        
        private SteamCloudSaveManager.SaveFileMetadata localMetadata;
        private SteamCloudSaveManager.SaveFileMetadata cloudMetadata;
        private Action<SteamCloudSaveManager.ConflictResolution> onResolutionSelected;
        
        private void Awake()
        {
            // Setup button listeners
            if (useLocalButton != null)
                useLocalButton.onClick.AddListener(() => SelectResolution(SteamCloudSaveManager.ConflictResolution.UseLocal));
            
            if (useCloudButton != null)
                useCloudButton.onClick.AddListener(() => SelectResolution(SteamCloudSaveManager.ConflictResolution.UseCloud));
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(() => SelectResolution(SteamCloudSaveManager.ConflictResolution.Cancel));
            
            // Hide panel by default
            if (conflictPanel != null)
                conflictPanel.SetActive(false);
        }
        
        private void OnEnable()
        {
            // Register as the conflict resolver
            if (SteamCloudSaveManager.Instance != null)
            {
                SteamCloudSaveManager.Instance.conflictResolver = ResolveConflict;
            }
        }
        
        private void OnDisable()
        {
            // Unregister conflict resolver
            if (SteamCloudSaveManager.Instance != null)
            {
                SteamCloudSaveManager.Instance.conflictResolver = null;
            }
        }
        
        /// <summary>
        /// Called by SteamCloudSaveManager when a conflict needs resolution
        /// </summary>
        public SteamCloudSaveManager.ConflictResolution ResolveConflict(
            SteamCloudSaveManager.SaveFileMetadata local, 
            SteamCloudSaveManager.SaveFileMetadata cloud)
        {
            // Store metadata
            localMetadata = local;
            cloudMetadata = cloud;
            
            // Update UI
            UpdateConflictDisplay();
            
            // Show panel and wait for user input
            ShowConflictPanel();
            
            // This would normally block, but in Unity we need to handle it async
            // Return a default for now, actual resolution will come from button clicks
            return SteamCloudSaveManager.ConflictResolution.Cancel;
        }
        
        /// <summary>
        /// Shows the conflict resolution UI with save information
        /// </summary>
        public void ShowConflictWithCallback(
            SteamCloudSaveManager.SaveFileMetadata local,
            SteamCloudSaveManager.SaveFileMetadata cloud,
            Action<SteamCloudSaveManager.ConflictResolution> callback)
        {
            localMetadata = local;
            cloudMetadata = cloud;
            onResolutionSelected = callback;
            
            UpdateConflictDisplay();
            ShowConflictPanel();
        }
        
        private void UpdateConflictDisplay()
        {
            // Update title and description
            if (titleText != null)
                titleText.text = "Steam Cloud Save Conflict";
            
            if (descriptionText != null)
                descriptionText.text = "Your local save and Steam Cloud save are different. Which would you like to use?";
            
            // Update local save information
            if (localMetadata != null)
            {
                if (localTimestampText != null)
                    localTimestampText.text = $"Saved: {localMetadata.timestamp:yyyy-MM-dd HH:mm:ss}";
                
                if (localPlayTimeText != null)
                    localPlayTimeText.text = $"Play Time: {FormatPlayTime(localMetadata.playTime)}";
                
                if (localLevelText != null)
                    localLevelText.text = $"Level: {localMetadata.playerLevel}";
                
                if (localProgressText != null)
                {
                    // Determine which is newer
                    bool isNewer = localMetadata.timestamp > cloudMetadata.timestamp;
                    localProgressText.text = isNewer ? "<color=green>NEWER</color>" : "<color=yellow>OLDER</color>";
                }
            }
            
            // Update cloud save information
            if (cloudMetadata != null)
            {
                if (cloudTimestampText != null)
                    cloudTimestampText.text = $"Saved: {cloudMetadata.timestamp:yyyy-MM-dd HH:mm:ss}";
                
                if (cloudPlayTimeText != null)
                    cloudPlayTimeText.text = $"Play Time: {FormatPlayTime(cloudMetadata.playTime)}";
                
                if (cloudLevelText != null)
                    cloudLevelText.text = $"Level: {cloudMetadata.playerLevel}";
                
                if (cloudProgressText != null)
                {
                    // Determine which is newer
                    bool isNewer = cloudMetadata.timestamp > localMetadata.timestamp;
                    cloudProgressText.text = isNewer ? "<color=green>NEWER</color>" : "<color=yellow>OLDER</color>";
                }
            }
            
            // Update warning text
            if (warningText != null)
            {
                TimeSpan timeDiff = Math.Abs((localMetadata.timestamp - cloudMetadata.timestamp).Duration());
                if (timeDiff.TotalDays > 1)
                {
                    warningText.text = $"<color=orange>Warning: These saves are {timeDiff.Days} days apart!</color>";
                    warningText.gameObject.SetActive(true);
                }
                else
                {
                    warningText.gameObject.SetActive(false);
                }
            }
        }
        
        private void ShowConflictPanel()
        {
            if (conflictPanel != null)
            {
                conflictPanel.SetActive(true);
                
                // Pause game while showing conflict resolution
                Time.timeScale = 0f;
                
                // Focus on the panel
                if (useLocalButton != null && localMetadata.timestamp > cloudMetadata.timestamp)
                {
                    // Local is newer, suggest using it
                    useLocalButton.Select();
                }
                else if (useCloudButton != null)
                {
                    // Cloud is newer, suggest using it
                    useCloudButton.Select();
                }
            }
        }
        
        private void HideConflictPanel()
        {
            if (conflictPanel != null)
            {
                conflictPanel.SetActive(false);
                
                // Resume game
                Time.timeScale = 1f;
            }
        }
        
        private void SelectResolution(SteamCloudSaveManager.ConflictResolution resolution)
        {
            // Check if user wants to remember choice
            if (rememberChoiceToggle != null && rememberChoiceToggle.isOn)
            {
                SaveResolutionPreference(resolution);
            }
            
            // Hide panel
            HideConflictPanel();
            
            // Invoke callback if set
            onResolutionSelected?.Invoke(resolution);
            
            // Log the resolution
            Debug.Log($"[SteamCloudConflictUI] User selected: {resolution}");
        }
        
        private string FormatPlayTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            
            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            else
            {
                return $"{minutes}m";
            }
        }
        
        private void SaveResolutionPreference(SteamCloudSaveManager.ConflictResolution resolution)
        {
            // Save preference for automatic resolution in the future
            PlayerPrefs.SetString("SteamCloudConflictPreference", resolution.ToString());
            PlayerPrefs.Save();
        }
        
        public static SteamCloudSaveManager.ConflictResolution? GetSavedPreference()
        {
            if (PlayerPrefs.HasKey("SteamCloudConflictPreference"))
            {
                string pref = PlayerPrefs.GetString("SteamCloudConflictPreference");
                if (Enum.TryParse<SteamCloudSaveManager.ConflictResolution>(pref, out var resolution))
                {
                    return resolution;
                }
            }
            return null;
        }
        
        public static void ClearSavedPreference()
        {
            PlayerPrefs.DeleteKey("SteamCloudConflictPreference");
            PlayerPrefs.Save();
        }
    }
}