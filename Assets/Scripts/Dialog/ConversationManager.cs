using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Enhanced Dialog Manager integrating Dialogue System for Unity with Disco Elysium-inspired features
    /// T'Challa's architectural vision for narrative excellence
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        [Header("Dialogue System Integration")]
        public DialogueManager legacyDialogManager; // References existing DialogueManager.cs
        
        [Header("Conversation Enhancement")]
        public ConversationPointTracker pointTracker;
        public CharacterPersonalitySystem personalitySystem;
        public DialogChoiceValidator choiceValidator;
        
        [Header("Character Data")]
        public List<CharacterDialogConfig> characterConfigs;
        
        // Events for system integration
        public event Action<string, DialogChoice> OnDialogChoiceConfirmed;
        public event Action<string, int> OnConversationPointsAwarded;
        public event Action<string, int> OnCVCLevelChanged;

        private Dictionary<string, List<DialogHistoryEntry>> conversationHistory;
        private Dictionary<string, PlayerStats> currentPlayerStats;
        
        void Start()
        {
            InitializeConversationSystem();
        }

        private void InitializeConversationSystem()
        {
            conversationHistory = new Dictionary<string, List<DialogHistoryEntry>>();
            currentPlayerStats = new Dictionary<string, PlayerStats>();
            
            // Initialize character configs
            InitializeCharacterConfigurations();
            
            // Subscribe to legacy dialog manager events if available
            if (legacyDialogManager != null)
            {
                Debug.Log("ConversationManager: Integrating with legacy DialogueManager");
            }
        }

        #region Character Configuration

        private void InitializeCharacterConfigurations()
        {
            characterConfigs = new List<CharacterDialogConfig>
            {
                // Seven Deadly Sins Bosses
                CreateBossConfig("wrath", "Wrath incarnate - quick to anger, respects strength", PersonalityTrait.Aggressive),
                CreateBossConfig("pride", "Pride embodied - arrogant, values acknowledgment", PersonalityTrait.Arrogant),
                CreateBossConfig("envy", "Envy personified - jealous, desires what others have", PersonalityTrait.Envious),
                CreateBossConfig("lust", "Lust manifest - seductive, craves desire", PersonalityTrait.Lustful),
                CreateBossConfig("gluttony", "Gluttony alive - consumes everything, never satisfied", PersonalityTrait.Gluttonous),
                CreateBossConfig("greed", "Greed itself - hoards wealth, values transactions", PersonalityTrait.Greedy),
                CreateBossConfig("sloth", "Sloth embodied - lazy, avoids effort", PersonalityTrait.Lazy),
                
                // NPCs
                CreateNPCConfig("elli", "Perma currency vendor - wise, mysterious", PersonalityTrait.Wise),
                CreateNPCConfig("aria", "Meta currency vendor - friendly, energetic", PersonalityTrait.Friendly),
                CreateNPCConfig("merchant", "General vendor - cunning, business-minded", PersonalityTrait.Cunning),
                
                // Special Characters
                CreateBossConfig("finalboss", "The ultimate evil - manipulative, all sins combined", PersonalityTrait.Arrogant),
                CreateNPCConfig("spaceship", "Immortal AI companion - ancient wisdom, time mastery", PersonalityTrait.Wise)
            };
        }

        private CharacterDialogConfig CreateBossConfig(string id, string description, PersonalityTrait primaryTrait)
        {
            return new CharacterDialogConfig
            {
                characterId = id,
                characterType = CharacterType.Boss,
                description = description,
                primaryTrait = primaryTrait,
                conversationPointsMultiplier = 1.5f, // Bosses give more points
                hasShopInterface = false,
                availableLanguages = GetAllSupportedLanguages()
            };
        }

        private CharacterDialogConfig CreateNPCConfig(string id, string description, PersonalityTrait primaryTrait)
        {
            return new CharacterDialogConfig
            {
                characterId = id,
                characterType = CharacterType.NPC,
                description = description,
                primaryTrait = primaryTrait,
                conversationPointsMultiplier = 1.0f,
                hasShopInterface = id.Contains("elli") || id.Contains("aria") || id.Contains("merchant"),
                availableLanguages = GetAllSupportedLanguages()
            };
        }

        private SystemLanguage[] GetAllSupportedLanguages()
        {
            return new SystemLanguage[]
            {
                SystemLanguage.English,
                SystemLanguage.Spanish,
                SystemLanguage.ChineseSimplified,
                SystemLanguage.Romanian,
                SystemLanguage.Russian,
                SystemLanguage.German // Adding German for broader support
            };
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Start a conversation with enhanced Disco Elysium features
        /// </summary>
        public void StartEnhancedConversation(string characterId, string conversationContext = "general")
        {
            var character = GetCharacterConfig(characterId);
            if (character == null)
            {
                Debug.LogError($"ConversationManager: Character '{characterId}' not found");
                return;
            }

            // Get player stats for this conversation
            var playerStats = GetCurrentPlayerStats();
            
            // Generate available choices based on CVC level, history, and stats
            var availableChoices = choiceValidator.GetAvailableChoices(characterId, conversationContext, playerStats);
            
            // Show internal thoughts (Disco Elysium style)
            var internalThought = GetInternalThought(characterId, GetPlayerMentalState());
            if (!string.IsNullOrEmpty(internalThought))
            {
                ShowInternalThought(internalThought);
            }

            // Start the actual conversation
            DisplayConversationUI(characterId, availableChoices, conversationContext);
        }

        /// <summary>
        /// Process player's dialog choice with full consequence system
        /// </summary>
        public void ProcessPlayerChoice(string characterId, DialogChoice choice, string conversationContext = "general")
        {
            var character = GetCharacterConfig(characterId);
            if (character == null) return;

            // Calculate conversation points based on choice and character personality
            var points = CalculateConversationPoints(characterId, choice);
            pointTracker.AwardPoints(characterId, choice, points);

            // Record choice in history
            RecordChoice(characterId, choice, conversationContext);

            // Generate character response based on personality and history
            var response = personalitySystem.GenerateResponse(characterId, choice, conversationContext);
            
            // Check for CVC level changes
            var newCVCLevel = pointTracker.GetCVCLevel(characterId);
            CheckForCVCLevelUp(characterId, newCVCLevel);

            // Trigger events for other systems
            OnDialogChoiceConfirmed?.Invoke(characterId, choice);
            OnConversationPointsAwarded?.Invoke(characterId, points);

            // Show response and continue conversation
            DisplayCharacterResponse(characterId, response);
        }

        /// <summary>
        /// Set language with full platform support
        /// </summary>
        public void SetLanguage(SystemLanguage language)
        {
            if (legacyDialogManager != null)
            {
                legacyDialogManager.InitializeLanguage(language);
            }
            
            Debug.Log($"ConversationManager: Language set to {language}");
        }

        /// <summary>
        /// Initialize for specific platform (Android, iOS, Windows, Linux)
        /// </summary>
        public void InitializeForPlatform(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    ConfigureForMobile();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    ConfigureForMobile();
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.LinuxPlayer:
                    ConfigureForDesktop();
                    break;
                default:
                    ConfigureForDesktop();
                    break;
            }
        }

        #endregion

        #region Internal Thought System (Disco Elysium Style)

        public string GetInternalThought(string characterId, PlayerMentalState mentalState)
        {
            var character = GetCharacterConfig(characterId);
            if (character == null) return "";

            // Generate contextual internal thoughts based on character and player state
            var thoughts = GetInternalThoughtDatabase(characterId, mentalState);
            if (thoughts.Any())
            {
                return $"[{thoughts[UnityEngine.Random.Range(0, thoughts.Count)]}]";
            }

            return "";
        }

        private List<string> GetInternalThoughtDatabase(string characterId, PlayerMentalState mentalState)
        {
            var thoughts = new List<string>();

            switch (characterId.ToLower())
            {
                case "wrath":
                    if (mentalState == PlayerMentalState.Confident)
                        thoughts.Add("This rage... it mirrors something within your spaceship's core");
                    else
                        thoughts.Add("Violence begets violence. Your immortal vessel has seen this cycle before");
                    break;
                
                case "pride":
                    thoughts.Add("Pride is the foundation and the downfall of empires among the stars");
                    thoughts.Add("Your spaceship whispers: 'I have outlasted countless prideful civilizations'");
                    break;
                
                case "spaceship":
                    thoughts.Add("The eternal bond speaks through quantum entanglement");
                    thoughts.Add("Time itself bends around this ancient consciousness");
                    break;
            }

            return thoughts;
        }

        #endregion

        #region Conversation Points and CVC System

        private int CalculateConversationPoints(string characterId, DialogChoice choice)
        {
            var character = GetCharacterConfig(characterId);
            var basePoints = GetBasePointsForChoice(choice);
            var personalityMultiplier = GetPersonalityMultiplier(characterId, choice);
            
            return Mathf.RoundToInt(basePoints * personalityMultiplier * character.conversationPointsMultiplier);
        }

        private int GetBasePointsForChoice(DialogChoice choice)
        {
            switch (choice)
            {
                case DialogChoice.Diplomatic: return 15;
                case DialogChoice.Empathetic: return 20;
                case DialogChoice.Insightful: return 25;
                case DialogChoice.Philosophical: return 30;
                case DialogChoice.PsychologicalInsight: return 35;
                case DialogChoice.TimeManipulation: return 40; // Unique to spaceship abilities
                
                case DialogChoice.Aggressive: return -10;
                case DialogChoice.Hostile: return -15;
                
                default: return 10;
            }
        }

        private float GetPersonalityMultiplier(string characterId, DialogChoice choice)
        {
            var character = GetCharacterConfig(characterId);
            
            // Characters respond differently to different approaches
            switch (character.primaryTrait)
            {
                case PersonalityTrait.Aggressive: // Wrath
                    return choice == DialogChoice.Challenge ? 2.0f : 
                           choice == DialogChoice.Diplomatic ? 0.5f : 1.0f;
                
                case PersonalityTrait.Arrogant: // Pride
                    return choice == DialogChoice.Humble ? 2.0f :
                           choice == DialogChoice.Arrogant ? 0.3f : 1.0f;
                
                case PersonalityTrait.Wise: // Elli, Spaceship
                    return choice == DialogChoice.Philosophical ? 1.5f :
                           choice == DialogChoice.Insightful ? 1.3f : 1.0f;
                
                default:
                    return 1.0f;
            }
        }

        #endregion

        #region Platform Configuration

        private void ConfigureForMobile()
        {
            // Mobile-specific optimizations
            Debug.Log("ConversationManager: Configured for mobile platform");
            // Adjust UI scaling, touch interactions, etc.
        }

        private void ConfigureForDesktop()
        {
            // Desktop-specific optimizations
            Debug.Log("ConversationManager: Configured for desktop platform");
            // Keyboard shortcuts, mouse interactions, etc.
        }

        #endregion

        #region Helper Methods

        public void RecordChoice(string characterId, DialogChoice choice, string context)
        {
            if (!conversationHistory.ContainsKey(characterId))
            {
                conversationHistory[characterId] = new List<DialogHistoryEntry>();
            }

            conversationHistory[characterId].Add(new DialogHistoryEntry
            {
                choice = choice,
                context = context,
                timestamp = DateTime.Now,
                cvcLevelAtTime = pointTracker.GetCVCLevel(characterId)
            });
        }

        public string GetLocalizedDialog(string key)
        {
            if (legacyDialogManager != null)
            {
                return legacyDialogManager.GetLocalizedString(key);
            }
            return $"Dialog key: {key}";
        }

        private CharacterDialogConfig GetCharacterConfig(string characterId)
        {
            return characterConfigs.FirstOrDefault(c => c.characterId.Equals(characterId, StringComparison.OrdinalIgnoreCase));
        }

        private PlayerStats GetCurrentPlayerStats()
        {
            // Integration point with player progression system
            return new PlayerStats
            {
                timeControlMastery = 5, // Get from player progression
                spaceshipBondLevel = 3, // Get from spaceship bond system
                charismaLevel = 4       // Get from character stats
            };
        }

        private PlayerMentalState GetPlayerMentalState()
        {
            // Could be influenced by recent choices, health, etc.
            return PlayerMentalState.Focused;
        }

        private void CheckForCVCLevelUp(string characterId, int newLevel)
        {
            // Trigger level up events, unlock new dialog options
            OnCVCLevelChanged?.Invoke(characterId, newLevel);
        }

        private void ShowInternalThought(string thought)
        {
            // Display internal thought in UI
            Debug.Log($"<color=cyan>Internal Thought: {thought}</color>");
        }

        private void DisplayConversationUI(string characterId, List<DialogChoice> choices, string context)
        {
            // Show conversation UI with enhanced choices
            Debug.Log($"Starting conversation with {characterId}, context: {context}");
        }

        private void DisplayCharacterResponse(string characterId, string response)
        {
            // Display character response with personality-driven formatting
            Debug.Log($"{characterId}: {response}");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class CharacterDialogConfig
    {
        public string characterId;
        public CharacterType characterType;
        public string description;
        public PersonalityTrait primaryTrait;
        public float conversationPointsMultiplier = 1.0f;
        public bool hasShopInterface;
        public SystemLanguage[] availableLanguages;
    }

    [System.Serializable]
    public class DialogHistoryEntry
    {
        public DialogChoice choice;
        public string context;
        public DateTime timestamp;
        public int cvcLevelAtTime;
    }

    public enum CharacterType
    {
        Boss,
        NPC,
        Vendor,
        Special
    }

    // Import enums from test file
    public enum DialogChoice
    {
        Diplomatic, Empathetic, Insightful, Philosophical, PsychologicalInsight,
        Aggressive, Arrogant, Hostile, Suspicious,
        Motivational, Bargaining, Challenge, Pacifist, Humble,
        TimeManipulation, CharmingPersuasion, UnityAgainstEvil
    }

    public enum PersonalityTrait
    {
        Aggressive, Arrogant, Greedy, Lazy, Lustful, Envious, Gluttonous,
        Friendly, Mysterious, Wise, Cunning
    }

    public enum PlayerMentalState
    {
        Confident, Uncertain, Overwhelmed, Focused, Contemplative
    }

    public class PlayerStats
    {
        public int timeControlMastery;
        public int spaceshipBondLevel;
        public int charismaLevel;
    }

    #endregion
}