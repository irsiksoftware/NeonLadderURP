using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using NeonLadder.Dialog;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Handles protagonist dialogue responses and interactive conversations
    /// Extends the boss banter system with player choice mechanics
    /// </summary>
    public class ProtagonistDialogueSystem : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueInteraction
        {
            public string interactionName;
            public string conversationName;
            public InteractionType type;
            public bool requiresPlayerInput = false;
            public float autoAdvanceDelay = 0f;
        }

        public enum InteractionType
        {
            SimpleBanter,      // Just boss speaks (current system)
            ProtagonistReply,  // Boss speaks, protagonist auto-replies
            PlayerChoice,      // Boss speaks, player chooses response
            FullConversation   // Multi-turn dialogue with choices
        }

        [Header("Protagonist Configuration")]
        public List<DialogueInteraction> dialogueTypes = new List<DialogueInteraction>();
        
        [Header("Player Choice Settings")]
        public bool enablePlayerChoices = true;
        public float choiceTimeout = 30f; // Auto-select if no choice made
        public int defaultChoiceIndex = 0;

        [Header("Protagonist Voice")]
        public bool enableProtagonistVoice = true;
        public string protagonistActorName = "Player";

        [Header("Integration")]
        public BossBanterManager banterManager;

        private Dictionary<string, DialogueInteraction> interactionMap;
        private bool isInInteractiveDialogue = false;
        private string currentConversation = "";

        void Awake()
        {
            InitializeProtagonistSystem();
        }

        public void Start()
        {
            // Find BossBanterManager if not assigned
            if (banterManager == null)
            {
                banterManager = FindObjectOfType<BossBanterManager>();
            }

            // Register for Dialogue System events
            RegisterDialogueEvents();
        }

        void OnDestroy()
        {
            UnregisterDialogueEvents();
        }

        /// <summary>
        /// Initialize the protagonist dialogue system
        /// </summary>
        public void InitializeProtagonistSystem()
        {
            interactionMap = new Dictionary<string, DialogueInteraction>();

            // Set up default interaction types
            if (dialogueTypes.Count == 0)
            {
                SetupDefaultInteractions();
            }

            foreach (var interaction in dialogueTypes)
            {
                interactionMap[interaction.interactionName] = interaction;
            }
        }

        /// <summary>
        /// Set up default interaction types for each boss
        /// </summary>
        private void SetupDefaultInteractions()
        {
            var bossNames = new[] { "Wrath", "Envy", "Greed", "Lust", "Gluttony", "Sloth", "Pride", "Devil" };

            foreach (var bossName in bossNames)
            {
                // Simple banter (current system)
                dialogueTypes.Add(new DialogueInteraction
                {
                    interactionName = $"{bossName}_Banter",
                    conversationName = $"{bossName}_Banter",
                    type = InteractionType.SimpleBanter,
                    requiresPlayerInput = false
                });

                // Interactive conversation (with player choices)
                dialogueTypes.Add(new DialogueInteraction
                {
                    interactionName = $"{bossName}_Conversation",
                    conversationName = $"{bossName}_FullDialogue",
                    type = InteractionType.PlayerChoice,
                    requiresPlayerInput = true
                });

                // Pre-fight dialogue with protagonist replies
                dialogueTypes.Add(new DialogueInteraction
                {
                    interactionName = $"{bossName}_PreFight",
                    conversationName = $"{bossName}_PreFight",
                    type = InteractionType.ProtagonistReply,
                    requiresPlayerInput = false,
                    autoAdvanceDelay = 2f
                });
            }
        }

        /// <summary>
        /// Unity message system handles event registration automatically
        /// No explicit registration needed - DialogueManager sends Unity messages
        /// </summary>
        private void RegisterDialogueEvents()
        {
            // Pixel Crushers uses Unity's message system
            // OnConversationStart, OnConversationEnd, and OnConversationLine
            // are called automatically when conversations happen
        }

        /// <summary>
        /// Unity message system handles cleanup automatically
        /// </summary>
        private void UnregisterDialogueEvents()
        {
            // No manual cleanup needed for Unity message system
        }

        /// <summary>
        /// Trigger interactive dialogue with a boss
        /// </summary>
        public bool TriggerInteractiveDialogue(string bossName, InteractionType interactionType = InteractionType.PlayerChoice)
        {
            if (string.IsNullOrEmpty(bossName))
            {
                Debug.LogWarning("TriggerInteractiveDialogue called with null or empty boss name");
                return false;
            }
            
            string interactionKey = $"{bossName}_{interactionType}";
            
            if (!interactionMap.ContainsKey(interactionKey))
            {
                Debug.LogWarning($"Interactive dialogue '{interactionKey}' not found");
                return false;
            }

            var interaction = interactionMap[interactionKey];
            
            // Set up dialogue variables (if available)
            try
            {
                DialogueLua.SetVariable("ProtagonistVoiceEnabled", enableProtagonistVoice);
                DialogueLua.SetVariable("CurrentBoss", bossName);
                DialogueLua.SetVariable("InteractionType", interactionType.ToString());
            }
            catch
            {
                Debug.Log($"DialogueLua not available - would set variables for boss '{bossName}'");
            }

            // Start the conversation (only if Dialogue Manager is available)
            if (PixelCrushers.DialogueSystem.DialogueManager.instance != null)
            {
                PixelCrushers.DialogueSystem.DialogueManager.StartConversation(interaction.conversationName);
            }
            else
            {
                Debug.Log($"DialogueManager not available - simulating conversation start for '{interaction.conversationName}'");
            }
            
            isInInteractiveDialogue = interaction.requiresPlayerInput;
            currentConversation = interaction.conversationName;

            return true;
        }

        /// <summary>
        /// Handle conversation start events
        /// </summary>
        private void OnConversationStart(Transform actor)
        {
            if (isInInteractiveDialogue)
            {
                // Set up UI for interactive dialogue
                SetupInteractiveUI();
            }
        }

        /// <summary>
        /// Handle conversation end events
        /// </summary>
        private void OnConversationEnd(Transform actor)
        {
            isInInteractiveDialogue = false;
            currentConversation = "";
            
            // Clean up UI
            CleanupInteractiveUI();
        }

        /// <summary>
        /// Unity message sent before each dialogue line
        /// </summary>
        public void OnConversationLine(Subtitle subtitle)
        {
            // Handle dialogue line preparation
            if (subtitle.speakerInfo.nameInDatabase == protagonistActorName)
            {
                ProcessProtagonistLine(subtitle);
            }
        }

        /// <summary>
        /// Process protagonist dialogue lines
        /// </summary>
        private void ProcessProtagonistLine(Subtitle subtitle)
        {
            // Add protagonist-specific processing here
            // For example: modify based on player stats, previous choices, etc.
            // subtitle.formattedText contains the dialogue text
            // subtitle.speakerInfo contains actor information
        }

        /// <summary>
        /// Set up UI for interactive dialogue
        /// </summary>
        private void SetupInteractiveUI()
        {
            // Configure dialogue UI for player choices
            var dialogueUI = GameObject.FindObjectOfType<PixelCrushers.DialogueSystem.UnityUIDialogueUI>();
            if (dialogueUI != null)
            {
                // Enable response menu, set timeout, etc.
            }
        }

        /// <summary>
        /// Clean up interactive UI
        /// </summary>
        private void CleanupInteractiveUI()
        {
            // Reset UI to default state
        }

        /// <summary>
        /// Get available protagonist responses for a boss
        /// </summary>
        public string[] GetProtagonistResponses(string bossName, string context = "default")
        {
            // This would query the dialogue database for available protagonist responses
            // For now, return sample responses
            return new string[]
            {
                "I won't back down!",
                "Your words don't scare me.",
                "Let's end this.",
                "I'm here to stop you."
            };
        }

        /// <summary>
        /// Check if a boss supports interactive dialogue
        /// </summary>
        public bool SupportsInteractiveDialogue(string bossName)
        {
            string interactionKey = $"{bossName}_Conversation";
            return interactionMap.ContainsKey(interactionKey);
        }

        /// <summary>
        /// Get the current interaction type for debugging
        /// </summary>
        public InteractionType GetCurrentInteractionType()
        {
            if (!isInInteractiveDialogue || string.IsNullOrEmpty(currentConversation))
                return InteractionType.SimpleBanter;

            var interaction = dialogueTypes.Find(d => d.conversationName == currentConversation);
            return interaction?.type ?? InteractionType.SimpleBanter;
        }
    }

    /// <summary>
    /// Sample protagonist responses for different contexts
    /// This provides the foundation for multi-language protagonist dialogue
    /// </summary>
    public static class ProtagonistResponses
    {
        public static readonly Dictionary<string, ProtagonistResponseSet> ResponseSets = new Dictionary<string, ProtagonistResponseSet>
        {
            ["Defiant"] = new ProtagonistResponseSet
            {
                context = "Defiant",
                responses = new BossBanterContent.MultiLanguageLine[]
                {
                    new BossBanterContent.MultiLanguageLine("I won't back down!", "我不会退缩！", "میں پیچھے نہیں ہٹوں گا!"),
                    new BossBanterContent.MultiLanguageLine("Your words don't scare me.", "你的话吓不到我。", "تمہاری باتیں مجھے ڈرا نہیں سکتیں۔"),
                    new BossBanterContent.MultiLanguageLine("I'm not afraid of you!", "我不怕你！", "میں تم سے نہیں ڈرتا!")
                }
            },

            ["Determined"] = new ProtagonistResponseSet
            {
                context = "Determined",
                responses = new BossBanterContent.MultiLanguageLine[]
                {
                    new BossBanterContent.MultiLanguageLine("I'm here to stop you.", "我来阻止你。", "میں تمہیں روکنے آیا ہوں۔"),
                    new BossBanterContent.MultiLanguageLine("This ends now.", "现在结束了。", "اب یہ ختم ہوتا ہے۔"),
                    new BossBanterContent.MultiLanguageLine("I won't let you hurt anyone else.", "我不会让你伤害其他人。", "میں تمہیں کسی اور کو نقصان نہیں پہنچانے دوں گا۔")
                }
            },

            ["Philosophical"] = new ProtagonistResponseSet
            {
                context = "Philosophical",
                responses = new BossBanterContent.MultiLanguageLine[]
                {
                    new BossBanterContent.MultiLanguageLine("There's still hope for redemption.", "仍有救赎的希望。", "ابھی بھی نجات کی امید ہے۔"),
                    new BossBanterContent.MultiLanguageLine("It doesn't have to be this way.", "不必如此。", "ایسا ہونا ضروری نہیں۔"),
                    new BossBanterContent.MultiLanguageLine("You were not always like this.", "你并非一直如此。", "تم ہمیشہ ایسے نہیں تھے۔")
                }
            }
        };

        [System.Serializable]
        public class ProtagonistResponseSet
        {
            public string context;
            public BossBanterContent.MultiLanguageLine[] responses;
        }
    }
}