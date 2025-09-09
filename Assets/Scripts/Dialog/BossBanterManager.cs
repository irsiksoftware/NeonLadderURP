using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Manages boss banter system using Pixel Crushers Dialogue System
    /// Provides rotating banter lines with multi-language support
    /// </summary>
    public class BossBanterManager : MonoBehaviour
    {
        [System.Serializable]
        public class BossConfig
        {
            public string bossName;
            public string conversationName;
            public int totalBanterLines;
            public float cooldownSeconds = 3f;
        }

        [Header("Boss Configuration")]
        public List<BossConfig> bossConfigs = new List<BossConfig>
        {
            new BossConfig { bossName = "Wrath", conversationName = "Wrath_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Envy", conversationName = "Envy_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Greed", conversationName = "Greed_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Lust", conversationName = "Lust_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Gluttony", conversationName = "Gluttony_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Sloth", conversationName = "Sloth_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Pride", conversationName = "Pride_Banter", totalBanterLines = 5 },
            new BossConfig { bossName = "Devil", conversationName = "Devil_Banter", totalBanterLines = 8 }
        };

        [Header("Language Support")]
        public string currentLanguage = "en";
        
        [Header("Debug")]
        public bool enableDebugLogging = true;

        private Dictionary<string, BossConfig> bossConfigMap;
        private Dictionary<string, int> lastBanterIndex;
        private Dictionary<string, float> lastBanterTime;
        
        // For testing - allows time injection
        private float? testTimeOverride = null;

        void Awake()
        {
            InitializeBanterSystem();
        }

        /// <summary>
        /// Initialize the banter tracking system
        /// </summary>
        public void InitializeBanterSystem()
        {
            bossConfigMap = new Dictionary<string, BossConfig>();
            lastBanterIndex = new Dictionary<string, int>();
            lastBanterTime = new Dictionary<string, float>();

            foreach (var config in bossConfigs)
            {
                bossConfigMap[config.bossName.ToLower()] = config;
                lastBanterIndex[config.bossName.ToLower()] = -1; // Start at -1 so first banter is index 0
                lastBanterTime[config.bossName.ToLower()] = 0f;
            }

                Debugger.Log($"BossBanterManager initialized with {bossConfigs.Count} boss configurations");
        }

        /// <summary>
        /// Trigger banter for a specific boss
        /// Returns true if banter was shown, false if on cooldown or invalid boss
        /// </summary>
        public bool TriggerBossBanter(string bossName)
        {
            if (string.IsNullOrEmpty(bossName))
            {

                    Debugger.LogWarning("TriggerBossBanter called with null or empty boss name");
                return false;
            }
            
            string normalizedName = bossName.ToLower();
            
            if (!bossConfigMap.ContainsKey(normalizedName))
            {
                    Debugger.LogWarning($"Boss '{bossName}' not found in banter configuration");
                return false;
            }

            var config = bossConfigMap[normalizedName];
            
            // Check cooldown
            if (GetCurrentTime() - lastBanterTime[normalizedName] < config.cooldownSeconds)
            {
                    Debugger.Log($"Boss '{bossName}' banter on cooldown");
                return false;
            }

            // Get next banter line (rotating)
            int nextIndex = GetNextBanterIndex(normalizedName, config.totalBanterLines);
            string variableName = $"{config.conversationName}.CurrentBanter";
            
            // Set the current banter index in Dialogue System (if available)
            try
            {
                DialogueLua.SetVariable(variableName, nextIndex);
            }
            catch
            {
                    Debugger.Log($"DialogueLua not available - would set {variableName} = {nextIndex}");
            }
            
            // Start the banter conversation (only if Dialogue Manager is available)
            if (PixelCrushers.DialogueSystem.DialogueManager.instance != null)
            {
                PixelCrushers.DialogueSystem.DialogueManager.StartConversation(config.conversationName);
            }
            else if (enableDebugLogging)
            {
                Debugger.Log($"DialogueManager not available - simulating conversation start for '{config.conversationName}'");
            }
            
            // Update tracking
            lastBanterIndex[normalizedName] = nextIndex;
            lastBanterTime[normalizedName] = GetCurrentTime();


                Debugger.Log($"Triggered banter for '{bossName}' - Line {nextIndex + 1}/{config.totalBanterLines}");

            return true;
        }

        /// <summary>
        /// Get the next banter index for rotation
        /// </summary>
        private int GetNextBanterIndex(string bossName, int totalLines)
        {
            int currentIndex = lastBanterIndex[bossName];
            return (currentIndex + 1) % totalLines;
        }

        /// <summary>
        /// Set the language for dialogue system
        /// Supported: "en" (English), "zh-Hans" (Chinese Simplified), "ur" (Urdu)
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            currentLanguage = languageCode;
            
            // Set Pixel Crushers language
            if (PixelCrushers.DialogueSystem.DialogueManager.instance != null)
            {
                PixelCrushers.DialogueSystem.DialogueManager.instance.SetLanguage(languageCode);
                

                    Debugger.Log($"Language set to: {languageCode}");
            }
        }

        /// <summary>
        /// Get banter statistics for a boss (for testing/debugging)
        /// </summary>
        public BanterStats GetBanterStats(string bossName)
        {
            string normalizedName = bossName.ToLower();
            
            if (!bossConfigMap.ContainsKey(normalizedName))
                return null;

            var config = bossConfigMap[normalizedName];
            return new BanterStats
            {
                bossName = bossName,
                totalLines = config.totalBanterLines,
                lastIndex = lastBanterIndex[normalizedName],
                timeSinceLastBanter = GetCurrentTime() - lastBanterTime[normalizedName],
                cooldownSeconds = config.cooldownSeconds,
                canTriggerBanter = GetCurrentTime() - lastBanterTime[normalizedName] >= config.cooldownSeconds
            };
        }

        /// <summary>
        /// Force reset banter rotation for a boss (useful for testing)
        /// </summary>
        public void ResetBossRotation(string bossName)
        {
            string normalizedName = bossName.ToLower();
            if (lastBanterIndex.ContainsKey(normalizedName))
            {
                lastBanterIndex[normalizedName] = -1;
                lastBanterTime[normalizedName] = 0f;
                
                if (enableDebugLogging)
                {
                    Debugger.Log($"Reset banter rotation for '{bossName}'");
                }
            }
        }

        /// <summary>
        /// Get all supported languages
        /// </summary>
        public string[] GetSupportedLanguages()
        {
            return new string[] { "en", "zh-Hans", "ur" };
        }
        
        /// <summary>
        /// Get current time (allows test injection)
        /// </summary>
        private float GetCurrentTime()
        {
            return testTimeOverride ?? Time.time;
        }
        
        /// <summary>
        /// For testing only - inject time value
        /// </summary>
        public void SetTestTime(float time)
        {
            testTimeOverride = time;
        }
        
        /// <summary>
        /// For testing only - clear time injection
        /// </summary>
        public void ClearTestTime()
        {
            testTimeOverride = null;
        }
    }

    /// <summary>
    /// Statistics for boss banter (useful for testing and debugging)
    /// </summary>
    [System.Serializable]
    public class BanterStats
    {
        public string bossName;
        public int totalLines;
        public int lastIndex;
        public float timeSinceLastBanter;
        public float cooldownSeconds;
        public bool canTriggerBanter;
    }
}