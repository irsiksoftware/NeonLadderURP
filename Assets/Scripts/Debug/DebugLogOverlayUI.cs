using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;

namespace NeonLadder.Debugging
{
    /// <summary>
    /// In-game debug overlay UI for displaying centralized logging system messages
    /// Provides real-time log viewing with filtering and customization options
    /// Integrates with NeonLadder's Modern UI Pack styling
    /// </summary>
    public class DebugLogOverlayUI : MonoBehaviour
    {
        [Header("🎮 UI References")]
        [Tooltip("Main panel containing the debug overlay")]
        public GameObject overlayPanel;
        
        [Tooltip("ScrollRect for the log content")]
        public ScrollRect logScrollRect;
        
        [Tooltip("Text component for displaying log messages")]
        public TextMeshProUGUI logTextField;
        
        [Tooltip("Button to toggle overlay visibility")]
        public Button toggleButton;
        
        [Tooltip("Button to clear all logs")]
        public Button clearButton;
        
        [Tooltip("Dropdown for log level filtering")]
        public TMP_Dropdown logLevelFilter;
        
        [Tooltip("Dropdown for category filtering")]
        public TMP_Dropdown categoryFilter;
        
        [Tooltip("Toggle for auto-scroll")]
        public Toggle autoScrollToggle;
        
        [Tooltip("Text showing current log count")]
        public TextMeshProUGUI logCountText;

        [Header("🎨 Visual Settings")]
        [Tooltip("Background color for the overlay panel")]
        public Color overlayBackgroundColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        
        [Tooltip("Font size for log messages")]
        [Range(8, 24)]
        public int logFontSize = 12;
        
        [Tooltip("Maximum number of log entries to display")]
        [Range(50, 500)]
        public int maxDisplayedLogs = 200;
        
        [Tooltip("Enable rich text formatting for log colors")]
        public bool enableRichText = true;

        [Header("🔧 Behavior Settings")]
        [Tooltip("Key to toggle overlay visibility")]
        public KeyCode toggleKey = KeyCode.F12;
        
        [Tooltip("Show overlay on application start")]
        public bool showOnStart = false;
        
        [Tooltip("Remember filter settings between sessions")]
        public bool rememberFilters = true;
        
        [Tooltip("Fade animation duration")]
        [Range(0.1f, 1f)]
        public float fadeAnimationDuration = 0.3f;

        // Internal state
        private LoggingSystemConfig config;
        private List<LogEntry> filteredLogs = new List<LogEntry>();
        private LogLevel currentLevelFilter = LogLevel.Debug;
        private LogCategory currentCategoryFilter = LogCategory.General;
        private bool showAllCategories = true;
        private bool isVisible = false;
        private CanvasGroup overlayCanvasGroup;
        private Coroutine fadeCoroutine;

        // Performance optimization
        private readonly StringBuilder logStringBuilder = new StringBuilder(8192);
        private float lastUIUpdate = 0f;
        private const float UI_UPDATE_INTERVAL = 0.1f; // Update UI 10 times per second max

        private void Awake()
        {
            InitializeUI();
        }

        private void Start()
        {
            // Get logging configuration
            if (LoggingManager.Instance != null && LoggingManager.Instance.config != null)
            {
                config = LoggingManager.Instance.config;
                
                // If logging is disabled globally, don't show overlay
                if (!config.enableLogging)
                {
                    SetOverlayVisibility(false);
                    return;
                }
                
                showOnStart = config.showOverlayOnStart;
                maxDisplayedLogs = config.maxOverlayEntries;
            }

            // Subscribe to logging events
            LoggingManager.OnLogEntryAdded += OnLogEntryAdded;
            LoggingManager.OnLogsCleared += OnLogsCleared;

            // Initialize visibility
            SetOverlayVisibility(showOnStart);
            
            // Setup UI callbacks
            SetupUICallbacks();
            
            // Load saved filter settings
            if (rememberFilters)
            {
                LoadFilterSettings();
            }

            // Initialize filters
            PopulateFilterDropdowns();
            
            LoggingManager.LogInfo(LogCategory.UI, "🎮 Debug Log Overlay UI initialized");
        }

        private void Update()
        {
            // Skip if logging is disabled
            if (config == null || !config.enableLogging)
                return;

            // Handle toggle key
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleOverlay();
            }

            // Throttled UI updates for performance
            if (Time.time - lastUIUpdate >= UI_UPDATE_INTERVAL)
            {
                UpdateLogCountDisplay();
                lastUIUpdate = Time.time;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            LoggingManager.OnLogEntryAdded -= OnLogEntryAdded;
            LoggingManager.OnLogsCleared -= OnLogsCleared;
            
            // Save filter settings
            if (rememberFilters)
            {
                SaveFilterSettings();
            }
        }

        #region UI Initialization

        private void InitializeUI()
        {
            // Create overlay canvas group for fading
            overlayCanvasGroup = overlayPanel.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = overlayPanel.AddComponent<CanvasGroup>();
            }

            // Set up initial visual state
            if (overlayPanel.TryGetComponent<Image>(out Image panelImage))
            {
                panelImage.color = overlayBackgroundColor;
            }

            // Configure log text field
            if (logTextField != null)
            {
                logTextField.fontSize = logFontSize;
                logTextField.richText = enableRichText;
                logTextField.enableWordWrapping = true;
                logTextField.overflowMode = TextOverflowModes.Ellipsis;
            }

            // Configure scroll rect
            if (logScrollRect != null)
            {
                logScrollRect.vertical = true;
                logScrollRect.horizontal = false;
                logScrollRect.scrollSensitivity = 20f;
            }
        }

        private void SetupUICallbacks()
        {
            // Toggle button
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(ToggleOverlay);
            }

            // Clear button
            if (clearButton != null)
            {
                clearButton.onClick.AddListener(ClearLogs);
            }

            // Auto-scroll toggle
            if (autoScrollToggle != null)
            {
                autoScrollToggle.isOn = config?.autoScrollToNewest ?? true;
                autoScrollToggle.onValueChanged.AddListener(OnAutoScrollToggleChanged);
            }

            // Filter dropdowns
            if (logLevelFilter != null)
            {
                logLevelFilter.onValueChanged.AddListener(OnLogLevelFilterChanged);
            }
            
            if (categoryFilter != null)
            {
                categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            }
        }

        private void PopulateFilterDropdowns()
        {
            // Populate log level filter
            if (logLevelFilter != null)
            {
                logLevelFilter.ClearOptions();
                var levelOptions = new List<string>();
                foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
                {
                    levelOptions.Add(level.ToString());
                }
                logLevelFilter.AddOptions(levelOptions);
                logLevelFilter.value = (int)currentLevelFilter;
            }

            // Populate category filter
            if (categoryFilter != null)
            {
                categoryFilter.ClearOptions();
                var categoryOptions = new List<string> { "All Categories" };
                foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
                {
                    categoryOptions.Add(category.ToString());
                }
                categoryFilter.AddOptions(categoryOptions);
                categoryFilter.value = showAllCategories ? 0 : (int)currentCategoryFilter + 1;
            }
        }

        #endregion

        #region Event Handlers

        private void OnLogEntryAdded(LogEntry logEntry)
        {
            // Apply filters
            if (ShouldDisplayLog(logEntry))
            {
                filteredLogs.Add(logEntry);
                
                // Trim old logs if exceeding limit
                if (filteredLogs.Count > maxDisplayedLogs)
                {
                    filteredLogs.RemoveAt(0);
                }

                // Update display if overlay is visible
                if (isVisible)
                {
                    UpdateLogDisplay();
                }
            }
        }

        private void OnLogsCleared()
        {
            filteredLogs.Clear();
            UpdateLogDisplay();
        }

        private void OnAutoScrollToggleChanged(bool value)
        {
            if (value && isVisible)
            {
                ScrollToBottom();
            }
        }

        private void OnLogLevelFilterChanged(int value)
        {
            currentLevelFilter = (LogLevel)value;
            RefreshFilteredLogs();
        }

        private void OnCategoryFilterChanged(int value)
        {
            if (value == 0)
            {
                showAllCategories = true;
            }
            else
            {
                showAllCategories = false;
                currentCategoryFilter = (LogCategory)(value - 1);
            }
            RefreshFilteredLogs();
        }

        #endregion

        #region Filtering and Display

        private bool ShouldDisplayLog(LogEntry logEntry)
        {
            // Check log level filter
            if (logEntry.level < currentLevelFilter)
                return false;

            // Check category filter
            if (!showAllCategories && logEntry.category != currentCategoryFilter)
                return false;

            return true;
        }

        private void RefreshFilteredLogs()
        {
            filteredLogs.Clear();
            
            var allLogs = LoggingManager.GetRecentLogs(maxDisplayedLogs * 2);
            foreach (var log in allLogs)
            {
                if (ShouldDisplayLog(log))
                {
                    filteredLogs.Add(log);
                }
            }

            UpdateLogDisplay();
        }

        private void UpdateLogDisplay()
        {
            if (logTextField == null) return;

            logStringBuilder.Clear();
            
            foreach (var logEntry in filteredLogs.TakeLast(maxDisplayedLogs))
            {
                AppendLogEntryToString(logEntry);
            }

            logTextField.text = logStringBuilder.ToString();

            // Auto-scroll to bottom if enabled
            if (autoScrollToggle != null && autoScrollToggle.isOn)
            {
                ScrollToBottom();
            }
        }

        private void AppendLogEntryToString(LogEntry logEntry)
        {
            if (enableRichText && config != null)
            {
                // Get color for log level
                Color logColor = GetLogLevelColor(logEntry.level);
                string colorHex = ColorUtility.ToHtmlStringRGBA(logColor);
                
                // Format with color and timestamp
                string timestamp = logEntry.timestamp.ToString("HH:mm:ss.fff");
                logStringBuilder.AppendLine($"<color=#{colorHex}>[{timestamp}] [{logEntry.level}] [{logEntry.category}] {logEntry.message}</color>");
            }
            else
            {
                // Plain text format
                string timestamp = logEntry.timestamp.ToString("HH:mm:ss.fff");
                logStringBuilder.AppendLine($"[{timestamp}] [{logEntry.level}] [{logEntry.category}] {logEntry.message}");
            }
        }

        private Color GetLogLevelColor(LogLevel level)
        {
            if (config?.colorTheme == null)
            {
                return level switch
                {
                    LogLevel.Debug => Color.gray,
                    LogLevel.Info => Color.white,
                    LogLevel.Warning => Color.yellow,
                    LogLevel.Error => Color.red,
                    LogLevel.Critical => new Color(1f, 0.5f, 0f),
                    _ => Color.white
                };
            }

            return level switch
            {
                LogLevel.Debug => config.colorTheme.debugColor,
                LogLevel.Info => config.colorTheme.infoColor,
                LogLevel.Warning => config.colorTheme.warningColor,
                LogLevel.Error => config.colorTheme.errorColor,
                LogLevel.Critical => config.colorTheme.criticalColor,
                _ => config.colorTheme.infoColor
            };
        }

        private void UpdateLogCountDisplay()
        {
            if (logCountText != null)
            {
                int totalLogs = LoggingManager.GetRecentLogs().Count;
                logCountText.text = $"Logs: {filteredLogs.Count}/{totalLogs}";
            }
        }

        #endregion

        #region Visibility and Animation

        public void ToggleOverlay()
        {
            SetOverlayVisibility(!isVisible);
        }

        public void SetOverlayVisibility(bool visible)
        {
            if (isVisible == visible) return;

            isVisible = visible;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (visible)
            {
                overlayPanel.SetActive(true);
                RefreshFilteredLogs(); // Refresh logs when showing
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(overlayCanvasGroup, 0f, 1f, fadeAnimationDuration));
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeCanvasGroup(overlayCanvasGroup, 1f, 0f, fadeAnimationDuration, () => {
                    overlayPanel.SetActive(false);
                }));
            }

            LoggingManager.LogDebug(LogCategory.UI, $"🎮 Debug overlay {(visible ? "shown" : "hidden")}");
        }

        private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, Action onComplete = null)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                yield return null;
            }
            
            canvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }

        private void ScrollToBottom()
        {
            if (logScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                logScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        #endregion

        #region Utility Methods

        private void ClearLogs()
        {
            LoggingManager.ClearLogs();
            LoggingManager.LogInfo(LogCategory.UI, "🧹 Debug logs cleared via overlay UI");
        }

        private void SaveFilterSettings()
        {
            PlayerPrefs.SetInt("DebugOverlay_LogLevel", (int)currentLevelFilter);
            PlayerPrefs.SetInt("DebugOverlay_Category", showAllCategories ? -1 : (int)currentCategoryFilter);
            PlayerPrefs.SetInt("DebugOverlay_AutoScroll", autoScrollToggle?.isOn == true ? 1 : 0);
        }

        private void LoadFilterSettings()
        {
            currentLevelFilter = (LogLevel)PlayerPrefs.GetInt("DebugOverlay_LogLevel", (int)LogLevel.Debug);
            int savedCategory = PlayerPrefs.GetInt("DebugOverlay_Category", -1);
            
            if (savedCategory == -1)
            {
                showAllCategories = true;
            }
            else
            {
                showAllCategories = false;
                currentCategoryFilter = (LogCategory)savedCategory;
            }

            if (autoScrollToggle != null)
            {
                autoScrollToggle.isOn = PlayerPrefs.GetInt("DebugOverlay_AutoScroll", 1) == 1;
            }
        }

        /// <summary>
        /// Create a debug overlay prefab at runtime if none exists
        /// </summary>
        public static DebugLogOverlayUI CreateRuntimeOverlay()
        {
            // Create canvas
            GameObject canvasObj = new GameObject("Debug Log Overlay Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Ensure it's on top
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();

            // Create overlay panel
            GameObject overlayObj = new GameObject("Overlay Panel");
            overlayObj.transform.SetParent(canvasObj.transform, false);
            
            RectTransform overlayRect = overlayObj.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = new Vector2(50, 50);
            overlayRect.offsetMax = new Vector2(-50, -50);
            
            Image overlayImage = overlayObj.AddComponent<Image>();
            overlayImage.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);
            
            // Create scroll view for logs
            CreateScrollView(overlayObj);
            
            // Add and configure the overlay component
            DebugLogOverlayUI overlay = canvasObj.AddComponent<DebugLogOverlayUI>();
            overlay.overlayPanel = overlayObj;
            // Additional setup would be needed for full functionality
            
            DontDestroyOnLoad(canvasObj);
            
            return overlay;
        }

        private static void CreateScrollView(GameObject parent)
        {
            // Create scroll rect
            GameObject scrollObj = new GameObject("Log Scroll View");
            scrollObj.transform.SetParent(parent.transform, false);
            
            RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = new Vector2(10, 60); // Leave space for controls
            scrollRect.offsetMax = new Vector2(-10, -10);
            
            ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.vertical = true;
            scroll.horizontal = false;
            
            // Create content area
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(scrollObj.transform, false);
            
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            
            // Create text field
            GameObject textObj = new GameObject("Log Text");
            textObj.transform.SetParent(contentObj.transform, false);
            
            TextMeshProUGUI textMesh = textObj.AddComponent<TextMeshProUGUI>();
            textMesh.text = "Debug log overlay initialized...";
            textMesh.fontSize = 12;
            textMesh.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            scroll.content = contentRect;
        }

        #endregion
    }
}