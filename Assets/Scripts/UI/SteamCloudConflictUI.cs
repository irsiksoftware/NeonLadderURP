using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonLadder.DataManagement;
using NeonLadder.Debugging;

namespace NeonLadder.UI
{
    /// <summary>
    /// UI handler for Steam Cloud save conflict resolution.
    /// Presents clear choices to players when save conflicts are detected.
    /// </summary>
    public class SteamCloudConflictUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject conflictPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Local Save Display")]
        [SerializeField] private GameObject localSaveCard;
        [SerializeField] private TextMeshProUGUI localTitleText;
        [SerializeField] private TextMeshProUGUI localLevelText;
        [SerializeField] private TextMeshProUGUI localTimeText;
        [SerializeField] private TextMeshProUGUI localCurrencyText;
        [SerializeField] private TextMeshProUGUI localPlayTimeText;
        [SerializeField] private Button chooseLocalButton;
        
        [Header("Cloud Save Display")]
        [SerializeField] private GameObject cloudSaveCard;
        [SerializeField] private TextMeshProUGUI cloudTitleText;
        [SerializeField] private TextMeshProUGUI cloudLevelText;
        [SerializeField] private TextMeshProUGUI cloudTimeText;
        [SerializeField] private TextMeshProUGUI cloudCurrencyText;
        [SerializeField] private TextMeshProUGUI cloudPlayTimeText;
        [SerializeField] private Button chooseCloudButton;
        
        [Header("Additional Options")]
        [SerializeField] private Button chooseNewerButton;
        [SerializeField] private TextMeshProUGUI newerButtonText;
        [SerializeField] private Toggle rememberChoiceToggle;
        
        [Header("Visual Indicators")]
        [SerializeField] private GameObject localNewerBadge;
        [SerializeField] private GameObject cloudNewerBadge;
        [SerializeField] private Color32 newerColor = new Color32(76, 175, 80, 255);
        [SerializeField] private Color32 olderColor = new Color32(255, 152, 0, 255);
        
        private SaveConflict currentConflict;
        private SteamCloudSaveManager cloudManager;
        private bool isResolving = false;
        
        private void Awake()
        {
            cloudManager = SteamCloudSaveManager.Instance;
            
            // Subscribe to events
            SteamCloudSaveManager.OnSaveConflictDetected += HandleConflictDetected;
            SteamCloudSaveManager.OnConflictResolved += HandleConflictResolved;
            
            // Setup button listeners
            if (chooseLocalButton != null)
                chooseLocalButton.onClick.AddListener(() => ResolveConflict(ConflictChoice.KeepLocal));
            
            if (chooseCloudButton != null)
                chooseCloudButton.onClick.AddListener(() => ResolveConflict(ConflictChoice.KeepCloud));
            
            if (chooseNewerButton != null)
                chooseNewerButton.onClick.AddListener(() => ResolveConflict(ConflictChoice.KeepNewer));
            
            // Hide panel initially
            if (conflictPanel != null)
                conflictPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            SteamCloudSaveManager.OnSaveConflictDetected -= HandleConflictDetected;
            SteamCloudSaveManager.OnConflictResolved -= HandleConflictResolved;
        }
        
        private void HandleConflictDetected(SaveConflict conflict)
        {
            if (conflict == null || isResolving)
                return;
            
            currentConflict = conflict;
            ShowConflictUI();
        }
        
        private void HandleConflictResolved(ConflictResolution resolution)
        {
            isResolving = false;
            HideConflictUI();
            
            // Show success message
            ShowResolutionMessage(resolution);
        }
        
        private void ShowConflictUI()
        {
            if (conflictPanel == null || currentConflict == null)
                return;
            
            conflictPanel.SetActive(true);
            
            // Set title and description
            if (titleText != null)
                titleText.text = "Save Conflict Detected";
            
            if (descriptionText != null)
                descriptionText.text = "Your local save and Steam Cloud save are different. Choose which one to keep:";
            
            // Populate local save info
            PopulateLocalSaveInfo();
            
            // Populate cloud save info
            PopulateCloudSaveInfo();
            
            // Determine which is newer
            UpdateNewerIndicators();
            
            // Enable all buttons
            SetButtonsInteractable(true);
        }
        
        private void PopulateLocalSaveInfo()
        {
            if (currentConflict.localMetadata == null)
                return;
            
            var meta = currentConflict.localMetadata;
            var save = currentConflict.localSave;
            
            if (localTitleText != null)
                localTitleText.text = "Local Save";
            
            if (localLevelText != null)
                localLevelText.text = $"Level {meta.playerLevel}";
            
            if (localTimeText != null)
                localTimeText.text = $"Saved: {meta.lastSaved:g}";
            
            if (localCurrencyText != null)
                localCurrencyText.text = $"Meta: {meta.metaCurrency} | Perma: {meta.permaCurrency}";
            
            if (localPlayTimeText != null)
                localPlayTimeText.text = $"Play Time: {FormatPlayTime(meta.totalPlayTime)}";
        }
        
        private void PopulateCloudSaveInfo()
        {
            if (currentConflict.cloudMetadata == null)
                return;
            
            var meta = currentConflict.cloudMetadata;
            var save = currentConflict.cloudSave;
            
            if (cloudTitleText != null)
                cloudTitleText.text = "Cloud Save";
            
            if (cloudLevelText != null)
                cloudLevelText.text = $"Level {meta.playerLevel}";
            
            if (cloudTimeText != null)
                cloudTimeText.text = $"Saved: {meta.lastSaved:g}";
            
            if (cloudCurrencyText != null)
                cloudCurrencyText.text = $"Meta: {meta.metaCurrency} | Perma: {meta.permaCurrency}";
            
            if (cloudPlayTimeText != null)
                cloudPlayTimeText.text = $"Play Time: {FormatPlayTime(meta.totalPlayTime)}";
        }
        
        private void UpdateNewerIndicators()
        {
            bool localIsNewer = currentConflict.localMetadata.lastSaved > 
                               currentConflict.cloudMetadata.lastSaved;
            
            if (localNewerBadge != null)
                localNewerBadge.SetActive(localIsNewer);
            
            if (cloudNewerBadge != null)
                cloudNewerBadge.SetActive(!localIsNewer);
            
            // Update newer button text
            if (newerButtonText != null)
            {
                string newerSource = localIsNewer ? "Local" : "Cloud";
                newerButtonText.text = $"Use Newer ({newerSource})";
            }
            
            // Update card colors
            if (localSaveCard != null)
            {
                var image = localSaveCard.GetComponent<Image>();
                if (image != null)
                {
                    image.color = localIsNewer ? newerColor : olderColor;
                }
            }
            
            if (cloudSaveCard != null)
            {
                var image = cloudSaveCard.GetComponent<Image>();
                if (image != null)
                {
                    image.color = localIsNewer ? olderColor : newerColor;
                }
            }
        }
        
        private void ResolveConflict(ConflictChoice choice)
        {
            if (isResolving || currentConflict == null)
                return;
            
            isResolving = true;
            SetButtonsInteractable(false);
            
            // Check if user wants to remember choice
            if (rememberChoiceToggle != null && rememberChoiceToggle.isOn)
            {
                SavePreferredResolutionStrategy(choice);
            }
            
            // Resolve the conflict
            cloudManager.ResolveConflict(currentConflict, choice);
        }
        
        private void HideConflictUI()
        {
            if (conflictPanel != null)
                conflictPanel.SetActive(false);
            
            currentConflict = null;
        }
        
        private void SetButtonsInteractable(bool interactable)
        {
            if (chooseLocalButton != null)
                chooseLocalButton.interactable = interactable;
            
            if (chooseCloudButton != null)
                chooseCloudButton.interactable = interactable;
            
            if (chooseNewerButton != null)
                chooseNewerButton.interactable = interactable;
        }
        
        private void ShowResolutionMessage(ConflictResolution resolution)
        {
            string message = resolution.choice switch
            {
                ConflictChoice.KeepLocal => "Local save kept and uploaded to cloud",
                ConflictChoice.KeepCloud => "Cloud save downloaded and saved locally",
                ConflictChoice.KeepNewer => "Newer save kept and synchronized",
                _ => "Save conflict resolved"
            };
            
            // Show message to player (implement your notification system)
            Debugger.Log($"[Steam Cloud] {message}");
        }
        
        private void SavePreferredResolutionStrategy(ConflictChoice choice)
        {
            // Save preference to PlayerPrefs or settings
            string strategy = choice switch
            {
                ConflictChoice.KeepLocal => "PreferLocal",
                ConflictChoice.KeepCloud => "PreferCloud",
                ConflictChoice.KeepNewer => "PreferNewest",
                _ => "AskUser"
            };
            
            PlayerPrefs.SetString("SteamCloudConflictStrategy", strategy);
            PlayerPrefs.Save();
        }
        
        private string FormatPlayTime(float totalSeconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);
            
            if (time.TotalHours >= 1)
            {
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            }
            else
            {
                return $"{time.Minutes}m {time.Seconds}s";
            }
        }
        
        #region Manual Testing Methods
        
        [ContextMenu("Test Show Conflict UI")]
        private void TestShowConflictUI()
        {
            // Create fake conflict for testing
            var testConflict = new SaveConflict
            {
                localMetadata = new SaveMetadata
                {
                    playerLevel = 15,
                    lastSaved = DateTime.Now,
                    metaCurrency = 500,
                    permaCurrency = 150,
                    totalPlayTime = 7200
                },
                cloudMetadata = new SaveMetadata
                {
                    playerLevel = 12,
                    lastSaved = DateTime.Now.AddHours(-2),
                    metaCurrency = 350,
                    permaCurrency = 120,
                    totalPlayTime = 5400
                }
            };
            
            currentConflict = testConflict;
            ShowConflictUI();
        }
        
        #endregion
    }
}