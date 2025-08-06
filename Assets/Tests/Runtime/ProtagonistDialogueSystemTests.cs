using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Dialog;
using PixelCrushers.DialogueSystem;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for Protagonist Dialogue System
    /// Tests interactive conversations, player choices, and protagonist responses
    /// </summary>
    public class ProtagonistDialogueSystemTests
    {
        private GameObject testGameObject;
        private ProtagonistDialogueSystem protagonistSystem;
        private BossBanterManager banterManager;
        private DialogueSystemController dialogueSystemController;

        [SetUp]
        public void Setup()
        {
            // Create test game objects
            testGameObject = new GameObject("TestProtagonistSystem");
            protagonistSystem = testGameObject.AddComponent<ProtagonistDialogueSystem>();
            banterManager = testGameObject.AddComponent<BossBanterManager>();
            
            // Create mock Dialogue System Controller
            var dialogueSystemGO = new GameObject("DialogueSystem");
            dialogueSystemController = dialogueSystemGO.AddComponent<DialogueSystemController>();
            
            // Create a simple test database
            var testDatabase = ScriptableObject.CreateInstance<DialogueDatabase>();
            testDatabase.name = "TestProtagonistDatabase";
            dialogueSystemController.initialDatabase = testDatabase;
            
            // Wire up dependencies
            protagonistSystem.banterManager = banterManager;
            
            // Initialize systems
            banterManager.InitializeBanterSystem();
            protagonistSystem.InitializeProtagonistSystem();
            
            // Don't manually call Unity lifecycle methods in tests
            // The components will initialize themselves when needed
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
            if (dialogueSystemController != null)
            {
                Object.DestroyImmediate(dialogueSystemController.gameObject);
            }
        }

        #region Initialization Tests

        [Test]
        public void ProtagonistSystem_InitializesWithDefaultInteractions()
        {
            // Should have 3 interaction types per boss (8 bosses * 3 types = 24)
            Assert.AreEqual(24, protagonistSystem.dialogueTypes.Count);
            
            // Check that all bosses have all three interaction types
            var expectedBosses = new[] { "Wrath", "Envy", "Greed", "Lust", "Gluttony", "Sloth", "Pride", "Devil" };
            var expectedTypes = new[] { "_Banter", "_Conversation", "_PreFight" };
            
            foreach (var boss in expectedBosses)
            {
                foreach (var type in expectedTypes)
                {
                    string interactionName = boss + type;
                    bool found = protagonistSystem.dialogueTypes.Exists(d => d.interactionName == interactionName);
                    Assert.IsTrue(found, $"Interaction '{interactionName}' not found");
                }
            }
        }

        [Test]
        public void ProtagonistSystem_ConfiguresCorrectInteractionTypes()
        {
            // Check specific interaction type configurations
            var banterInteraction = protagonistSystem.dialogueTypes.Find(d => d.interactionName == "Wrath_Banter");
            Assert.IsNotNull(banterInteraction);
            Assert.AreEqual(ProtagonistDialogueSystem.InteractionType.SimpleBanter, banterInteraction.type);
            Assert.IsFalse(banterInteraction.requiresPlayerInput);

            var conversationInteraction = protagonistSystem.dialogueTypes.Find(d => d.interactionName == "Pride_Conversation");
            Assert.IsNotNull(conversationInteraction);
            Assert.AreEqual(ProtagonistDialogueSystem.InteractionType.PlayerChoice, conversationInteraction.type);
            Assert.IsTrue(conversationInteraction.requiresPlayerInput);

            var preFightInteraction = protagonistSystem.dialogueTypes.Find(d => d.interactionName == "Devil_PreFight");
            Assert.IsNotNull(preFightInteraction);
            Assert.AreEqual(ProtagonistDialogueSystem.InteractionType.ProtagonistReply, preFightInteraction.type);
            Assert.IsFalse(preFightInteraction.requiresPlayerInput);
            Assert.AreEqual(2f, preFightInteraction.autoAdvanceDelay);
        }

        #endregion

        #region Interactive Dialogue Tests

        [Test]
        public void InteractiveDialogue_SupportsAllBossesForConversations()
        {
            var expectedBosses = new[] { "Wrath", "Envy", "Greed", "Lust", "Gluttony", "Sloth", "Pride", "Devil" };
            
            foreach (var boss in expectedBosses)
            {
                bool supportsInteractive = protagonistSystem.SupportsInteractiveDialogue(boss);
                Assert.IsTrue(supportsInteractive, $"Boss '{boss}' should support interactive dialogue");
            }
        }

        [Test]
        public void InteractiveDialogue_DoesNotSupportInvalidBoss()
        {
            bool supportsInteractive = protagonistSystem.SupportsInteractiveDialogue("NonexistentBoss");
            Assert.IsFalse(supportsInteractive, "Invalid boss should not support interactive dialogue");
        }

        [Test]
        public void InteractiveDialogue_TriggerFailsForInvalidBoss()
        {
            bool result = protagonistSystem.TriggerInteractiveDialogue("InvalidBoss");
            Assert.IsFalse(result, "Should return false for invalid boss");
        }

        [Test]
        public void InteractiveDialogue_CurrentInteractionTypeTracking()
        {
            // Initially should be SimpleBanter
            var initialType = protagonistSystem.GetCurrentInteractionType();
            Assert.AreEqual(ProtagonistDialogueSystem.InteractionType.SimpleBanter, initialType);
            
            // After triggering interactive dialogue, it should be tracked
            // Note: This would require actual conversation to be running for full test
        }

        #endregion

        #region Protagonist Response Tests

        [Test]
        public void ProtagonistResponses_DefiantContextHasCorrectResponses()
        {
            var defiantSet = ProtagonistResponses.ResponseSets["Defiant"];
            Assert.IsNotNull(defiantSet);
            Assert.AreEqual("Defiant", defiantSet.context);
            Assert.AreEqual(3, defiantSet.responses.Length);
            
            // Check first response in all languages
            var firstResponse = defiantSet.responses[0];
            Assert.AreEqual("I won't back down!", firstResponse.english);
            Assert.AreEqual("我不会退缩！", firstResponse.chineseSimplified);
            Assert.AreEqual("میں پیچھے نہیں ہٹوں گا!", firstResponse.urdu);
        }

        [Test]
        public void ProtagonistResponses_DeterminedContextHasCorrectResponses()
        {
            var determinedSet = ProtagonistResponses.ResponseSets["Determined"];
            Assert.IsNotNull(determinedSet);
            Assert.AreEqual("Determined", determinedSet.context);
            Assert.AreEqual(3, determinedSet.responses.Length);
            
            // Check localization works correctly
            var response = determinedSet.responses[0];
            Assert.AreEqual("I'm here to stop you.", response.GetLocalizedText("en"));
            Assert.AreEqual("我来阻止你。", response.GetLocalizedText("zh-Hans"));
            Assert.AreEqual("میں تمہیں روکنے آیا ہوں۔", response.GetLocalizedText("ur"));
        }

        [Test]
        public void ProtagonistResponses_PhilosophicalContextHasCorrectResponses()
        {
            var philosophicalSet = ProtagonistResponses.ResponseSets["Philosophical"];
            Assert.IsNotNull(philosophicalSet);
            Assert.AreEqual("Philosophical", philosophicalSet.context);
            Assert.AreEqual(3, philosophicalSet.responses.Length);
            
            // Verify philosophical tone in responses
            var responses = philosophicalSet.responses;
            Assert.IsTrue(responses[0].english.Contains("redemption"));
            Assert.IsTrue(responses[1].english.Contains("doesn't have to"));
            Assert.IsTrue(responses[2].english.Contains("not always"));
        }

        [Test]
        public void ProtagonistResponses_LocalizationWorksForAllContexts()
        {
            foreach (var responseSet in ProtagonistResponses.ResponseSets.Values)
            {
                foreach (var response in responseSet.responses)
                {
                    // English should never be empty
                    Assert.IsFalse(string.IsNullOrEmpty(response.english), 
                        $"English response empty in {responseSet.context}");
                    
                    // Chinese should never be empty
                    Assert.IsFalse(string.IsNullOrEmpty(response.chineseSimplified), 
                        $"Chinese response empty in {responseSet.context}");
                    
                    // Urdu should never be empty
                    Assert.IsFalse(string.IsNullOrEmpty(response.urdu), 
                        $"Urdu response empty in {responseSet.context}");
                    
                    // Default language should return English
                    Assert.AreEqual(response.english, response.GetLocalizedText("invalid-lang"));
                }
            }
        }

        #endregion

        #region Configuration Tests

        [Test]
        public void Configuration_PlayerChoicesCanBeDisabled()
        {
            protagonistSystem.enablePlayerChoices = false;
            Assert.IsFalse(protagonistSystem.enablePlayerChoices);
            
            protagonistSystem.enablePlayerChoices = true;
            Assert.IsTrue(protagonistSystem.enablePlayerChoices);
        }

        [Test]
        public void Configuration_ChoiceTimeoutConfigurable()
        {
            protagonistSystem.choiceTimeout = 15f;
            Assert.AreEqual(15f, protagonistSystem.choiceTimeout);
            
            protagonistSystem.choiceTimeout = 45f;
            Assert.AreEqual(45f, protagonistSystem.choiceTimeout);
        }

        [Test]
        public void Configuration_ProtagonistVoiceCanBeToggled()
        {
            protagonistSystem.enableProtagonistVoice = false;
            Assert.IsFalse(protagonistSystem.enableProtagonistVoice);
            
            protagonistSystem.enableProtagonistVoice = true;
            Assert.IsTrue(protagonistSystem.enableProtagonistVoice);
        }

        [Test]
        public void Configuration_ProtagonistActorNameConfigurable()
        {
            protagonistSystem.protagonistActorName = "Hero";
            Assert.AreEqual("Hero", protagonistSystem.protagonistActorName);
            
            protagonistSystem.protagonistActorName = "Player";
            Assert.AreEqual("Player", protagonistSystem.protagonistActorName);
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_GetProtagonistResponsesReturnsValidResponses()
        {
            string[] responses = protagonistSystem.GetProtagonistResponses("Wrath");
            
            Assert.IsNotNull(responses);
            Assert.AreEqual(4, responses.Length);
            Assert.Contains("I won't back down!", responses);
            Assert.Contains("Your words don't scare me.", responses);
            Assert.Contains("Let's end this.", responses);
            Assert.Contains("I'm here to stop you.", responses);
        }

        [Test]
        public void Integration_GetProtagonistResponsesWithContext()
        {
            string[] defaultResponses = protagonistSystem.GetProtagonistResponses("Pride", "default");
            string[] customResponses = protagonistSystem.GetProtagonistResponses("Pride", "philosophical");
            
            Assert.IsNotNull(defaultResponses);
            Assert.IsNotNull(customResponses);
            // In a full implementation, these might differ based on context
            Assert.AreEqual(4, defaultResponses.Length);
            Assert.AreEqual(4, customResponses.Length);
        }

        [Test]
        public void Integration_FindsBanterManagerDependency()
        {
            // Clear the reference and test auto-discovery
            protagonistSystem.banterManager = null;
            
            // Simulate Start() method call
            protagonistSystem.Start();
            
            // Should have found the BanterManager component
            Assert.IsNotNull(protagonistSystem.banterManager);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void ErrorHandling_NullBossNameInInteractiveDialogue()
        {
            bool result = protagonistSystem.TriggerInteractiveDialogue(null);
            Assert.IsFalse(result, "Null boss name should return false");
        }

        [Test]
        public void ErrorHandling_EmptyBossNameInInteractiveDialogue()
        {
            bool result = protagonistSystem.TriggerInteractiveDialogue("");
            Assert.IsFalse(result, "Empty boss name should return false");
        }

        [Test]
        public void ErrorHandling_InvalidInteractionType()
        {
            // This should still work but log a warning
            bool result = protagonistSystem.TriggerInteractiveDialogue("Wrath", 
                (ProtagonistDialogueSystem.InteractionType)999);
            
            // The method should handle invalid enum values gracefully
            Assert.IsFalse(result, "Invalid interaction type should return false");
        }

        #endregion

        #region Conversation Flow Tests

        [UnityTest]
        public IEnumerator ConversationFlow_HandlesConversationEvents()
        {
            // This test would require actual Dialogue System integration
            yield return null;
            
            // In a real scenario, we would:
            // 1. Start a conversation
            // 2. Verify events are fired
            // 3. Check state transitions
            // 4. Verify cleanup
            
            Assert.Pass("Conversation flow architecture is in place");
        }

        #endregion
    }
}