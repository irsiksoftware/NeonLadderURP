using UnityEngine;
using System;
using System.Collections.Generic;
using NeonLadder.Debug;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// ScriptableObject configuration for the NeonLadder Disco Elysium-inspired Dialog System
    /// Following the established NeonLadder pattern (like UpgradeData, PathGeneratorConfig, SaveSystemConfig)
    /// Provides Unity Editor tooltips and configuration management for the 85-story-point dialog epic
    /// </summary>
    [CreateAssetMenu(fileName = "Dialog System Config", menuName = "NeonLadder/Dialog/Dialog System Config")]
    public class DialogSystemConfig : ScriptableObject
    {
        [Header("🎭 Dialog System Configuration")]
        [Tooltip("Name of this dialog system configuration preset")]
        public string configurationName = "Default Dialog Config";
        
        [TextArea(3, 5)]
        [Tooltip("Description of this dialog configuration's purpose and settings")]
        public string description = "Disco Elysium-inspired dialog system with dynamic personality responses and skill checks";

        [Header("💬 Conversation Settings")]
        [Tooltip("Maximum number of dialog choices to display at once")]
        [Range(2, 8)]
        public int maxDialogChoices = 4;
        
        [Tooltip("Time delay between character responses (seconds)")]
        [Range(0f, 3f)]
        public float responseDelay = 1.5f;
        
        [Tooltip("Enable typing animation for dialog text")]
        public bool enableTypingAnimation = true;
        
        [Tooltip("Characters per second for typing animation")]
        [Range(10, 100)]
        public float typingSpeed = 30f;

        [Header("🎲 Skill Check System")]
        [Tooltip("Enable Disco Elysium-style skill checks in conversations")]
        public bool enableSkillChecks = true;
        
        [Tooltip("Base difficulty for skill checks (1-20)")]
        [Range(1, 20)]
        public int baseSkillCheckDifficulty = 10;
        
        [Tooltip("Show skill check odds to player before attempting")]
        public bool showSkillCheckOdds = true;
        
        [Tooltip("Allow retrying failed skill checks")]
        public bool allowSkillCheckRetries = false;

        [Header("🧠 Personality System")]
        [Tooltip("Enable dynamic personality-based responses")]
        public bool enablePersonalitySystem = true;
        
        [Tooltip("Intensity of personality modifications (0 = subtle, 2 = extreme)")]
        [Range(0f, 2f)]
        public float personalityIntensity = 1.0f;
        
        [Tooltip("Track relationship levels with NPCs")]
        public bool trackRelationships = true;
        
        [Tooltip("Relationship changes affect available dialog options")]
        public bool relationshipAffectsChoices = true;

        [Header("🎨 Visual Presentation")]
        [Tooltip("Character portraits for dialog participants")]
        public List<CharacterPortrait> characterPortraitOverrides = new List<CharacterPortrait>();
        
        [Tooltip("Color theme for dialog UI")]
        public DialogTheme dialogTheme = new DialogTheme();
        
        [Tooltip("Enable character name color coding")]
        public bool enableNameColorCoding = true;
        
        [Tooltip("Show character mood indicators")]
        public bool showMoodIndicators = true;

        [Header("🔊 Audio Integration")]
        [Tooltip("Enable voice-over for character dialog")]
        public bool enableVoiceOver = false;
        
        [Tooltip("Audio clips for character voices")]
        public List<CharacterVoiceClip> characterVoiceClips = new List<CharacterVoiceClip>();
        
        [Tooltip("Volume for dialog audio (0-1)")]
        [Range(0f, 1f)]
        public float dialogVolume = 0.8f;
        
        [Tooltip("Enable sound effects for UI interactions")]
        public bool enableDialogSFX = true;

        [Header("📊 Analytics & Debug")]
        [Tooltip("Log all dialog interactions for analytics")]
        public bool logDialogInteractions = false;
        
        [Tooltip("Enable debug overlay showing personality calculations")]
        public bool enableDebugOverlay = false;
        
        [Tooltip("Track dialog statistics (choices made, paths taken)")]
        public bool trackDialogStatistics = true;
        
        [Tooltip("Export dialog statistics to JSON file")]
        public bool exportStatistics = false;

        [Header("🔧 Advanced Settings")]
        [Tooltip("Maximum conversation history to keep in memory")]
        [Range(10, 100)]
        public int maxConversationHistory = 50;
        
        [Tooltip("Auto-save dialog progress after each choice")]
        public bool autoSaveDialogProgress = true;
        
        [Tooltip("Enable dialog branching based on player actions")]
        public bool enableActionBasedBranching = true;
        
        [Tooltip("Random seed for skill check generation (-1 = use system time)")]
        public int randomSeed = -1;

        [Header("🎯 Seven Deadly Sins Integration")]
        [Tooltip("Characters representing the Seven Deadly Sins")]
        public List<SinCharacterMapping> sinCharacters = new List<SinCharacterMapping>();
        
        [Tooltip("Enable sin-specific dialog branches")]
        public bool enableSinSpecificDialogs = true;
        
        [Tooltip("Player choices affect sin alignment")]
        public bool trackSinAlignment = true;

        /// <summary>
        /// Validates the current dialog system configuration
        /// </summary>
        [ContextMenu("🔍 Validate Dialog Configuration")]
        public ValidationResult ValidateConfiguration()
        {
            var result = new ValidationResult();
            
            if (maxDialogChoices < 2)
            {
                result.AddError("Dialog system needs at least 2 choices per conversation");
            }
            
            if (enableSkillChecks && baseSkillCheckDifficulty < 1)
            {
                result.AddError("Skill check difficulty must be at least 1");
            }
            
            if (enablePersonalitySystem && personalityIntensity <= 0)
            {
                result.AddWarning("Personality intensity is 0 - personality system effectively disabled");
            }
            
            if (enableVoiceOver && characterVoiceClips.Count == 0)
            {
                result.AddWarning("Voice-over enabled but no character voice clips configured");
            }
            
            if (sinCharacters.Count > 0 && sinCharacters.Count != 7)
            {
                result.AddWarning($"Seven Deadly Sins system configured with {sinCharacters.Count} characters (expected 7)");
            }

            // Log results
            if (result.HasErrors)
            {
                Debug.LogError($"❌ Dialog System Config validation failed:\n{result.GetSummary()}");
            }
            else if (result.HasWarnings)
            {
                Debug.LogWarning($"⚠️ Dialog System Config has warnings:\n{result.GetSummary()}");
            }
            else
            {
                Debug.Log($"✅ Dialog System Config validation passed: {configurationName}");
            }
            
            return result;
        }

        /// <summary>
        /// Creates a production-ready dialog configuration
        /// </summary>
        [ContextMenu("🚀 Load Production Preset")]
        public void LoadProductionPreset()
        {
            configurationName = "Production (Optimized & Polished)";
            description = "Production dialog system with balanced difficulty and polished presentation";
            maxDialogChoices = 4;
            responseDelay = 1.2f;
            enableTypingAnimation = true;
            typingSpeed = 35f;
            enableSkillChecks = true;
            baseSkillCheckDifficulty = 12;
            showSkillCheckOdds = true;
            allowSkillCheckRetries = false;
            enablePersonalitySystem = true;
            personalityIntensity = 1.0f;
            trackRelationships = true;
            relationshipAffectsChoices = true;
            enableNameColorCoding = true;
            showMoodIndicators = true;
            enableVoiceOver = false; // Typically disabled for production unless VO is ready
            enableDialogSFX = true;
            logDialogInteractions = false;
            enableDebugOverlay = false;
            trackDialogStatistics = true;
            exportStatistics = false;
            autoSaveDialogProgress = true;
            enableSinSpecificDialogs = true;
            trackSinAlignment = true;
            
            Debug.Log("🚀 Loaded production preset for dialog system");
        }

        /// <summary>
        /// Creates a development/testing configuration
        /// </summary>
        [ContextMenu("🔧 Load Development Preset")]
        public void LoadDevelopmentPreset()
        {
            configurationName = "Development (Fast Testing)";
            description = "Development dialog system with debug features and fast iteration";
            maxDialogChoices = 6; // More choices for testing
            responseDelay = 0.5f; // Faster for testing
            enableTypingAnimation = false; // Instant text for debugging
            typingSpeed = 50f;
            enableSkillChecks = true;
            baseSkillCheckDifficulty = 8; // Easier for testing
            showSkillCheckOdds = true;
            allowSkillCheckRetries = true; // Allow retries for testing
            enablePersonalitySystem = true;
            personalityIntensity = 1.5f; // More pronounced for testing
            trackRelationships = true;
            relationshipAffectsChoices = true;
            enableNameColorCoding = true;
            showMoodIndicators = true;
            enableVoiceOver = false;
            enableDialogSFX = false; // Disable audio for focus
            logDialogInteractions = true; // Enable for debugging
            enableDebugOverlay = true; // Show debug info
            trackDialogStatistics = true;
            exportStatistics = true; // Export for analysis
            autoSaveDialogProgress = false; // Manual control in dev
            enableSinSpecificDialogs = true;
            trackSinAlignment = true;
            
            Debug.Log("🔧 Loaded development preset for dialog system");
        }

        /// <summary>
        /// Creates the Seven Deadly Sins character mappings
        /// </summary>
        [ContextMenu("😈 Initialize Seven Deadly Sins")]
        public void InitializeSevenDeadlySins()
        {
            sinCharacters.Clear();
            sinCharacters.AddRange(new[]
            {
                new SinCharacterMapping { sinType = SinType.Wrath, characterId = "wrath", displayName = "Wrath", sinColor = new Color(0.8f, 0.1f, 0.1f) },
                new SinCharacterMapping { sinType = SinType.Pride, characterId = "pride", displayName = "Pride", sinColor = new Color(0.7f, 0.4f, 0.9f) },
                new SinCharacterMapping { sinType = SinType.Envy, characterId = "envy", displayName = "Envy", sinColor = new Color(0.2f, 0.6f, 0.2f) },
                new SinCharacterMapping { sinType = SinType.Lust, characterId = "lust", displayName = "Lust", sinColor = new Color(0.9f, 0.2f, 0.5f) },
                new SinCharacterMapping { sinType = SinType.Greed, characterId = "greed", displayName = "Greed", sinColor = new Color(0.9f, 0.7f, 0.1f) },
                new SinCharacterMapping { sinType = SinType.Gluttony, characterId = "gluttony", displayName = "Gluttony", sinColor = new Color(0.6f, 0.3f, 0.1f) },
                new SinCharacterMapping { sinType = SinType.Sloth, characterId = "sloth", displayName = "Sloth", sinColor = new Color(0.4f, 0.4f, 0.6f) }
            });
            
            Debug.Log("😈 Initialized Seven Deadly Sins character mappings");
        }

        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            maxDialogChoices = Mathf.Clamp(maxDialogChoices, 2, 8);
            responseDelay = Mathf.Clamp(responseDelay, 0f, 5f);
            typingSpeed = Mathf.Clamp(typingSpeed, 1f, 200f);
            baseSkillCheckDifficulty = Mathf.Clamp(baseSkillCheckDifficulty, 1, 20);
            personalityIntensity = Mathf.Clamp(personalityIntensity, 0f, 2f);
            dialogVolume = Mathf.Clamp01(dialogVolume);
            maxConversationHistory = Mathf.Clamp(maxConversationHistory, 10, 200);
            
            // Auto-generate config name if empty
            if (string.IsNullOrEmpty(configurationName))
            {
                configurationName = name;
            }
        }
    }

    [Serializable]
    public class CharacterPortrait
    {
        [Tooltip("Character ID this portrait applies to")]
        public string characterId;
        
        [Tooltip("Portrait sprite for this character")]
        public Sprite portraitSprite;
        
        [Tooltip("Color tint for the portrait")]
        public Color portraitTint = Color.white;
    }

    [Serializable]
    public class DialogTheme
    {
        [Tooltip("Background color for dialog box")]
        public Color backgroundColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        
        [Tooltip("Text color for dialog content")]
        public Color textColor = Color.white;
        
        [Tooltip("Color for character names")]
        public Color nameColor = new Color(0.8f, 0.9f, 1f);
        
        [Tooltip("Color for player choice text")]
        public Color choiceColor = new Color(0.9f, 0.9f, 0.7f);
        
        [Tooltip("Color for skill check text")]
        public Color skillCheckColor = new Color(0.7f, 0.9f, 0.7f);
    }

    [Serializable]
    public class CharacterVoiceClip
    {
        [Tooltip("Character ID this voice applies to")]
        public string characterId;
        
        [Tooltip("Audio clip for this character's voice")]
        public AudioClip voiceClip;
        
        [Tooltip("Pitch adjustment for this character (-3 to +3)")]
        [Range(-3f, 3f)]
        public float pitchAdjustment = 0f;
    }

    [Serializable]
    public class SinCharacterMapping
    {
        [Tooltip("The sin type this character represents")]
        public SinType sinType;
        
        [Tooltip("Character ID in the dialog system")]
        public string characterId;
        
        [Tooltip("Display name for this sin")]
        public string displayName;
        
        [Tooltip("Color associated with this sin")]
        public Color sinColor = Color.white;
    }

    [Serializable]
    public enum SinType
    {
        Wrath,      // Anger, violence, impatience
        Pride,      // Arrogance, vanity, excessive self-regard  
        Envy,       // Jealousy, resentment, bitterness
        Lust,       // Excessive desires, temptation
        Greed,      // Avarice, material obsession
        Gluttony,   // Overconsumption, never satisfied
        Sloth       // Laziness, apathy, avoiding effort
    }

    // Reuse ValidationResult from SaveSystemConfig to maintain consistency
    [Serializable]
    public class ValidationResult
    {
        public List<string> errors = new List<string>();
        public List<string> warnings = new List<string>();
        
        public bool HasErrors => errors.Count > 0;
        public bool HasWarnings => warnings.Count > 0;
        public bool IsValid => !HasErrors;
        
        public void AddError(string error) => errors.Add(error);
        public void AddWarning(string warning) => warnings.Add(warning);
        
        public string GetSummary()
        {
            var summary = "";
            if (HasErrors)
            {
                summary += "Errors:\n" + string.Join("\n", errors) + "\n";
            }
            if (HasWarnings)
            {
                summary += "Warnings:\n" + string.Join("\n", warnings);
            }
            return summary.Trim();
        }
    }
}