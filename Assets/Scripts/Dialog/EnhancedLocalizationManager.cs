using UnityEngine;
using UnityEngine.Localization.Tables;
using System.Collections.Generic;
using System;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Enhanced Localization Manager with multi-platform support
    /// T'Challa's vision for global accessibility
    /// </summary>
    public class EnhancedLocalizationManager : MonoBehaviour
    {
        [Header("Platform Configuration")]
        public bool autoDetectPlatformLanguage = true;
        public SystemLanguage fallbackLanguage = SystemLanguage.English;
        
        [Header("Supported Platforms")]
        public bool supportAndroid = true;
        public bool supportIOS = true;
        public bool supportWindows = true;
        public bool supportLinux = true;
        
        [Header("Language Support")]
        public List<LanguageConfig> supportedLanguages;
        
        [Header("Dialog Integration")]
        public DialogueManager legacyDialogManager;
        
        private Dictionary<SystemLanguage, StringTable> languageStringTables;
        private SystemLanguage currentLanguage;
        private Dictionary<string, Dictionary<SystemLanguage, string>> dialogCache;
        
        // Events
        public event Action<SystemLanguage> OnLanguageChanged;
        public event Action<string> OnLocalizationError;

        void Awake()
        {
            InitializeLocalizationSystem();
        }

        void Start()
        {
            SetupPlatformSpecificLanguage();
        }

        #region Initialization

        private void InitializeLocalizationSystem()
        {
            languageStringTables = new Dictionary<SystemLanguage, StringTable>();
            dialogCache = new Dictionary<string, Dictionary<SystemLanguage, string>>();
            
            InitializeSupportedLanguages();
            LoadAllStringTables();
        }

        private void InitializeSupportedLanguages()
        {
            supportedLanguages = new List<LanguageConfig>
            {
                new LanguageConfig 
                { 
                    language = SystemLanguage.English, 
                    displayName = "English", 
                    stringTablePath = "Localization/DialogueStringTable_en",
                    isDefault = true,
                    supportedPlatforms = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                },
                new LanguageConfig 
                { 
                    language = SystemLanguage.Spanish, 
                    displayName = "Español", 
                    stringTablePath = "Localization/DialogueStringTable_es",
                    supportedPlatforms = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                },
                new LanguageConfig 
                { 
                    language = SystemLanguage.ChineseSimplified, 
                    displayName = "简体中文", 
                    stringTablePath = "Localization/DialogueStringTable_zh-Hans",
                    supportedPlatforms = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                },
                new LanguageConfig 
                { 
                    language = SystemLanguage.Romanian, 
                    displayName = "Română", 
                    stringTablePath = "Localization/DialogueStringTable_ro",
                    supportedPlatforms = new[] { RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                },
                new LanguageConfig 
                { 
                    language = SystemLanguage.Russian, 
                    displayName = "Русский", 
                    stringTablePath = "Localization/DialogueStringTable_ru",
                    supportedPlatforms = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                },
                new LanguageConfig 
                { 
                    language = SystemLanguage.German, 
                    displayName = "Deutsch", 
                    stringTablePath = "Localization/DialogueStringTable_de",
                    supportedPlatforms = new[] { RuntimePlatform.Android, RuntimePlatform.IPhonePlayer, RuntimePlatform.WindowsPlayer, RuntimePlatform.LinuxPlayer }
                }
            };
        }

        private void LoadAllStringTables()
        {
            foreach (var languageConfig in supportedLanguages)
            {
                if (IsPlatformSupported(languageConfig))
                {
                    LoadStringTableForLanguage(languageConfig);
                }
            }
        }

        private void LoadStringTableForLanguage(LanguageConfig config)
        {
            try
            {
                var stringTable = Resources.Load<StringTable>(config.stringTablePath);
                if (stringTable != null)
                {
                    languageStringTables[config.language] = stringTable;
                    Debugger.Log($"EnhancedLocalizationManager: Loaded {config.displayName} string table");
                }
                else
                {
                    Debugger.LogWarning($"EnhancedLocalizationManager: Failed to load string table for {config.displayName} at {config.stringTablePath}");
                    OnLocalizationError?.Invoke($"Missing string table: {config.displayName}");
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError($"EnhancedLocalizationManager: Error loading {config.displayName}: {ex.Message}");
                OnLocalizationError?.Invoke($"Error loading {config.displayName}: {ex.Message}");
            }
        }

        #endregion

        #region Platform Detection

        private void SetupPlatformSpecificLanguage()
        {
            if (autoDetectPlatformLanguage)
            {
                var detectedLanguage = DetectPlatformLanguage();
                SetLanguage(detectedLanguage);
            }
            else
            {
                SetLanguage(fallbackLanguage);
            }
        }

        private SystemLanguage DetectPlatformLanguage()
        {
            var systemLanguage = Application.systemLanguage;
            var currentPlatform = Application.platform;
            
            Debugger.Log($"EnhancedLocalizationManager: Detected system language: {systemLanguage}, platform: {currentPlatform}");
            
            // Check if the detected language is supported on this platform
            var languageConfig = GetLanguageConfig(systemLanguage);
            if (languageConfig != null && IsLanguageSupportedOnPlatform(languageConfig, currentPlatform))
            {
                return systemLanguage;
            }
            
            // Fallback to default supported language for this platform
            return GetBestFallbackLanguageForPlatform(currentPlatform);
        }

        private SystemLanguage GetBestFallbackLanguageForPlatform(RuntimePlatform platform)
        {
            // Try English first
            var englishConfig = GetLanguageConfig(SystemLanguage.English);
            if (englishConfig != null && IsLanguageSupportedOnPlatform(englishConfig, platform))
            {
                return SystemLanguage.English;
            }
            
            // Find any supported language for this platform
            foreach (var config in supportedLanguages)
            {
                if (IsLanguageSupportedOnPlatform(config, platform))
                {
                    return config.language;
                }
            }
            
            return fallbackLanguage;
        }

        private bool IsLanguageSupportedOnPlatform(LanguageConfig config, RuntimePlatform platform)
        {
            return Array.Exists(config.supportedPlatforms, p => p == platform);
        }

        private bool IsPlatformSupported(LanguageConfig config)
        {
            var currentPlatform = Application.platform;
            return IsLanguageSupportedOnPlatform(config, currentPlatform);
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Set current language with platform validation
        /// </summary>
        public bool SetLanguage(SystemLanguage language)
        {
            var config = GetLanguageConfig(language);
            if (config == null)
            {
                Debugger.LogWarning($"EnhancedLocalizationManager: Language {language} not supported");
                return false;
            }

            if (!IsLanguageSupportedOnPlatform(config, Application.platform))
            {
                Debugger.LogWarning($"EnhancedLocalizationManager: Language {language} not supported on {Application.platform}");
                return false;
            }

            var previousLanguage = currentLanguage;
            currentLanguage = language;
            
            // Update legacy dialog manager
            if (legacyDialogManager != null)
            {
                legacyDialogManager.InitializeLanguage(language);
            }
            
            // Clear cache to force reload of localized strings
            ClearDialogCache();
            
            Debugger.Log($"EnhancedLocalizationManager: Language changed from {previousLanguage} to {currentLanguage}");
            OnLanguageChanged?.Invoke(currentLanguage);
            
            return true;
        }

        /// <summary>
        /// Get localized string with enhanced error handling
        /// </summary>
        public string GetLocalizedString(string key)
        {
            // Check cache first
            if (dialogCache.ContainsKey(key) && dialogCache[key].ContainsKey(currentLanguage))
            {
                return dialogCache[key][currentLanguage];
            }

            var localizedString = GetLocalizedStringFromTable(key, currentLanguage);
            
            // Cache the result
            CacheLocalizedString(key, currentLanguage, localizedString);
            
            return localizedString;
        }

        /// <summary>
        /// Get localized string for specific character with personality formatting
        /// </summary>
        public string GetLocalizedStringForCharacter(string key, string characterId)
        {
            var baseString = GetLocalizedString(key);
            
            // Apply character-specific formatting if needed
            return ApplyCharacterFormatting(baseString, characterId);
        }

        /// <summary>
        /// Get all available languages for current platform
        /// </summary>
        public List<LanguageConfig> GetAvailableLanguages()
        {
            var availableLanguages = new List<LanguageConfig>();
            var currentPlatform = Application.platform;
            
            foreach (var config in supportedLanguages)
            {
                if (IsLanguageSupportedOnPlatform(config, currentPlatform))
                {
                    availableLanguages.Add(config);
                }
            }
            
            return availableLanguages;
        }

        /// <summary>
        /// Get current language info
        /// </summary>
        public LanguageConfig GetCurrentLanguageConfig()
        {
            return GetLanguageConfig(currentLanguage);
        }

        /// <summary>
        /// Check if specific language is available
        /// </summary>
        public bool IsLanguageAvailable(SystemLanguage language)
        {
            var config = GetLanguageConfig(language);
            return config != null && IsLanguageSupportedOnPlatform(config, Application.platform);
        }

        /// <summary>
        /// Reload localization data (useful for live updates)
        /// </summary>
        public void ReloadLocalizationData()
        {
            ClearDialogCache();
            languageStringTables.Clear();
            LoadAllStringTables();
            
            Debugger.Log("EnhancedLocalizationManager: Localization data reloaded");
        }

        #endregion

        #region String Table Operations

        private string GetLocalizedStringFromTable(string key, SystemLanguage language)
        {
            if (!languageStringTables.ContainsKey(language))
            {
                Debugger.LogWarning($"EnhancedLocalizationManager: No string table for language {language}");
                return GetFallbackString(key);
            }

            var stringTable = languageStringTables[language];
            var entry = stringTable.GetEntry(key);
            
            if (entry == null)
            {
                Debugger.LogWarning($"EnhancedLocalizationManager: Key '{key}' not found in {language} string table");
                return GetFallbackString(key);
            }

            try
            {
                return entry.GetLocalizedString();
            }
            catch (Exception ex)
            {
                Debugger.LogError($"EnhancedLocalizationManager: Error getting localized string for key '{key}': {ex.Message}");
                return GetFallbackString(key);
            }
        }

        private string GetFallbackString(string key)
        {
            // Try English fallback
            if (currentLanguage != SystemLanguage.English && languageStringTables.ContainsKey(SystemLanguage.English))
            {
                var englishTable = languageStringTables[SystemLanguage.English];
                var entry = englishTable.GetEntry(key);
                if (entry != null)
                {
                    try
                    {
                        return $"[EN] {entry.GetLocalizedString()}";
                    }
                    catch
                    {
                        // Fallback failed, return key as-is
                    }
                }
            }
            
            // Last resort - return the key itself
            return $"[MISSING: {key}]";
        }

        #endregion

        #region Character Formatting

        private string ApplyCharacterFormatting(string baseString, string characterId)
        {
            // This could integrate with CharacterPersonalitySystem for voice-specific formatting
            switch (characterId.ToLower())
            {
                case "wrath":
                    return baseString.ToUpper();
                    
                case "sloth":
                    return baseString.Replace(" ", "... ");
                    
                case "spaceship":
                    return $"~{baseString}~";
                    
                default:
                    return baseString;
            }
        }

        #endregion

        #region Caching

        private void CacheLocalizedString(string key, SystemLanguage language, string value)
        {
            if (!dialogCache.ContainsKey(key))
            {
                dialogCache[key] = new Dictionary<SystemLanguage, string>();
            }
            
            dialogCache[key][language] = value;
        }

        private void ClearDialogCache()
        {
            dialogCache.Clear();
            Debugger.Log("EnhancedLocalizationManager: Dialog cache cleared");
        }

        #endregion

        #region Helper Methods

        private LanguageConfig GetLanguageConfig(SystemLanguage language)
        {
            return supportedLanguages.Find(config => config.language == language);
        }

        #endregion

        #region Debug and Analytics

        /// <summary>
        /// Get localization statistics
        /// </summary>
        public LocalizationAnalytics GetLocalizationAnalytics()
        {
            var analytics = new LocalizationAnalytics
            {
                currentLanguage = currentLanguage.ToString(),
                supportedLanguagesCount = GetAvailableLanguages().Count,
                totalStringTables = languageStringTables.Count,
                cachedStringsCount = dialogCache.Count,
                currentPlatform = Application.platform.ToString()
            };

            // Calculate string coverage
            if (languageStringTables.ContainsKey(SystemLanguage.English) && languageStringTables.ContainsKey(currentLanguage))
            {
                var englishTable = languageStringTables[SystemLanguage.English];
                var currentTable = languageStringTables[currentLanguage];
                
                analytics.stringCoveragePercentage = currentLanguage == SystemLanguage.English ? 100f :
                    (float)currentTable.Count / englishTable.Count * 100f;
            }

            return analytics;
        }

        /// <summary>
        /// Debug: Print localization status
        /// </summary>
        [ContextMenu("Debug: Print Localization Status")]
        public void DebugPrintLocalizationStatus()
        {
            Debugger.Log("=== Enhanced Localization Status ===");
            Debugger.Log($"Current Language: {currentLanguage}");
            Debugger.Log($"Platform: {Application.platform}");
            Debugger.Log($"Available Languages: {GetAvailableLanguages().Count}");
            
            foreach (var config in GetAvailableLanguages())
            {
                var tableStatus = languageStringTables.ContainsKey(config.language) ? "✓" : "✗";
                Debugger.Log($"  {tableStatus} {config.displayName} ({config.language})");
            }
            
            Debugger.Log($"Cached Strings: {dialogCache.Count}");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class LanguageConfig
    {
        public SystemLanguage language;
        public string displayName;
        public string stringTablePath;
        public bool isDefault;
        public RuntimePlatform[] supportedPlatforms;
    }

    [System.Serializable]
    public class LocalizationAnalytics
    {
        public string currentLanguage;
        public int supportedLanguagesCount;
        public int totalStringTables;
        public int cachedStringsCount;
        public float stringCoveragePercentage;
        public string currentPlatform;
    }

    #endregion
}