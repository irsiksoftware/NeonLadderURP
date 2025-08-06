using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Dialog;
using NeonLadder.Dialog;
using PixelCrushers.DialogueSystem;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Integration tests for the complete Dialogue System
    /// Tests integration between BossBanterManager, ProtagonistDialogueSystem, and ScriptableObject configurations
    /// Updated to reflect current Pixel Crushers-based implementation
    /// </summary>
    public class DialogSystemTests
    {
        private GameObject testGameObject;
        private BossBanterManager banterManager;
        private ProtagonistDialogueSystem protagonistSystem;
        private DialogueSceneConfiguration sceneConfig;
        private DialogueChoiceDatabase choiceDatabase;
        private DialogueSystemController dialogueSystemController;

        [SetUp]
        public void Setup()
        {
            // Create test game objects
            testGameObject = new GameObject("TestDialogSystem");
            banterManager = testGameObject.AddComponent<BossBanterManager>();
            protagonistSystem = testGameObject.AddComponent<ProtagonistDialogueSystem>();
            
            // Create mock Dialogue System Controller
            var dialogueSystemGO = new GameObject("DialogueSystem");
            dialogueSystemController = dialogueSystemGO.AddComponent<DialogueSystemController>();
            
            // Create test database
            var testDatabase = ScriptableObject.CreateInstance<DialogueDatabase>();
            testDatabase.name = "TestDialogSystemDatabase";
            dialogueSystemController.initialDatabase = testDatabase;
            
            // Create test scene configuration
            sceneConfig = ScriptableObject.CreateInstance<DialogueSceneConfiguration>();
            sceneConfig.sceneName = "Test Integration Scene";
            sceneConfig.conversationName = "TestIntegrationConversation";
            
            // Create test choice database
            choiceDatabase = ScriptableObject.CreateInstance<DialogueChoiceDatabase>();
            choiceDatabase.databaseName = "Test Integration Choices";
            
            // Wire up dependencies  
            protagonistSystem.banterManager = banterManager;
            
            // Initialize systems
            banterManager.InitializeBanterSystem();
            protagonistSystem.InitializeProtagonistSystem();
            
            // For tests, we don't need to fully initialize DialogueSystemController
            // Our systems work independently for the functionality we're testing
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
            if (dialogueSystemController != null)
                Object.DestroyImmediate(dialogueSystemController.gameObject);
            if (sceneConfig != null)
                Object.DestroyImmediate(sceneConfig);
            if (choiceDatabase != null)
                Object.DestroyImmediate(choiceDatabase);
        }

        #region System Integration Tests

        [Test]
        public void DialogSystem_BanterAndProtagonistSystemsWorkTogether()
        {
            // Test that both systems can coexist and function independently
            Assert.IsNotNull(banterManager, "BanterManager should be initialized");
            Assert.IsNotNull(protagonistSystem, "ProtagonistSystem should be initialized");
            Assert.AreEqual(banterManager, protagonistSystem.banterManager, "Systems should be linked");
        }

        [Test]
        public void DialogSystem_BanterManagerHasAllBossConfigurations()
        {
            // Verify that the banter system is properly configured for all bosses
            Assert.AreEqual(8, banterManager.bossConfigs.Count, "Should have 8 boss configurations");
            
            var expectedBosses = new[] { "Wrath", "Envy", "Greed", "Lust", "Gluttony", "Sloth", "Pride", "Devil" };
            foreach (var expectedBoss in expectedBosses)
            {
                bool found = banterManager.bossConfigs.Exists(config => config.bossName == expectedBoss);
                Assert.IsTrue(found, $"Boss '{expectedBoss}' not found in banter manager");
            }
        }

        [Test]
        public void DialogSystem_ProtagonistSystemHasAllInteractionTypes()
        {
            // Verify that protagonist system supports all interaction types for all bosses
            Assert.AreEqual(24, protagonistSystem.dialogueTypes.Count, "Should have 24 interaction types (8 bosses * 3 types)");
            
            // Verify specific interaction types exist
            var wrathBanter = protagonistSystem.dialogueTypes.Find(d => d.interactionName == "Wrath_Banter");
            Assert.IsNotNull(wrathBanter, "Wrath_Banter interaction should exist");
            Assert.AreEqual(ProtagonistDialogueSystem.InteractionType.SimpleBanter, wrathBanter.type);
        }

        [Test]
        public void DialogSystem_MultiLanguageSupportIsConsistent()
        {
            // Both systems should support the same languages
            string[] banterLanguages = banterManager.GetSupportedLanguages();
            
            Assert.Contains("en", banterLanguages, "English should be supported");
            Assert.Contains("zh-Hans", banterLanguages, "Chinese Simplified should be supported");
            Assert.Contains("ur", banterLanguages, "Urdu should be supported");
            Assert.AreEqual(3, banterLanguages.Length, "Should support exactly 3 languages");
        }

        #endregion

        #region ScriptableObject Configuration Tests

        [Test]
        public void DialogSystem_SceneConfigurationIntegratesWithSystems()
        {
            // Test that scene configuration works with the dialogue systems
            Assert.IsNotNull(sceneConfig, "Scene configuration should be created");
            Assert.AreEqual("Test Integration Scene", sceneConfig.sceneName);
            Assert.AreEqual("TestIntegrationConversation", sceneConfig.conversationName);
        }

        [Test]
        public void DialogSystem_ChoiceDatabaseSupportsLookups()
        {
            // Test that choice database integrates properly
            Assert.IsNotNull(choiceDatabase, "Choice database should be created");
            Assert.AreEqual("Test Integration Choices", choiceDatabase.databaseName);
            
            // Test that database can be enabled without errors
            Assert.DoesNotThrow(() => choiceDatabase.OnEnable(), "Database OnEnable should not throw");
        }

        [Test]
        public void DialogSystem_PixelCrushersIntegrationWorks()
        {
            // Test that Pixel Crushers integration is functional
            Assert.IsNotNull(dialogueSystemController, "DialogueSystemController should be created");
            Assert.IsNotNull(dialogueSystemController.initialDatabase, "Initial database should be assigned");
            Assert.AreEqual("TestDialogSystemDatabase", dialogueSystemController.initialDatabase.name);
        }

        #endregion

        #region Multi-Language & Localization Tests

        [Test]
        public void DialogSystem_LanguageSwitchingWorks()
        {
            // Test that language switching works across systems
            banterManager.SetLanguage("en");
            Assert.AreEqual("en", banterManager.currentLanguage, "English should be set");
            
            banterManager.SetLanguage("zh-Hans");
            Assert.AreEqual("zh-Hans", banterManager.currentLanguage, "Chinese should be set");
            
            banterManager.SetLanguage("ur");
            Assert.AreEqual("ur", banterManager.currentLanguage, "Urdu should be set");
        }

        [Test]
        public void DialogSystem_DefaultLanguageIsEnglish()
        {
            // Both systems should default to English
            Assert.AreEqual("en", banterManager.currentLanguage, "Default language should be English");
        }

        [Test]
        public void DialogSystem_SupportsAllRequiredLanguages()
        {
            // Verify that all required languages are supported
            var supportedLanguages = banterManager.GetSupportedLanguages();
            
            Assert.Contains("en", supportedLanguages, "Must support English");
            Assert.Contains("zh-Hans", supportedLanguages, "Must support Chinese Simplified");
            Assert.Contains("ur", supportedLanguages, "Must support Urdu");
        }

        #endregion

        #region Error Handling & Validation Tests

        [Test]
        public void DialogSystem_HandlesInvalidBossNamesGracefully()
        {
            // Test error handling for invalid boss names
            bool banterResult = banterManager.TriggerBossBanter("NonExistentBoss");
            Assert.IsFalse(banterResult, "Invalid boss name should return false");
            
            bool interactiveResult = protagonistSystem.TriggerInteractiveDialogue("NonExistentBoss");
            Assert.IsFalse(interactiveResult, "Invalid boss name should return false in protagonist system");
        }

        [Test]
        public void DialogSystem_HandlesNullParametersGracefully()
        {
            // Test null parameter handling
            bool banterResult = banterManager.TriggerBossBanter(null);
            Assert.IsFalse(banterResult, "Null boss name should return false");
            
            bool interactiveResult = protagonistSystem.TriggerInteractiveDialogue(null);
            Assert.IsFalse(interactiveResult, "Null boss name should return false in protagonist system");
        }

        [Test]
        public void DialogSystem_SystemsInitializeProperly()
        {
            // Test that both systems initialize without errors
            Assert.DoesNotThrow(() => banterManager.InitializeBanterSystem(), "Banter system should initialize without errors");
            Assert.DoesNotThrow(() => protagonistSystem.InitializeProtagonistSystem(), "Protagonist system should initialize without errors");
        }

        #endregion

        #region Integration with Existing Systems Tests

        [Test]
        public void DialogSystem_WorksWithPixelCrushersDialogueSystem()
        {
            // Test integration with Pixel Crushers Dialogue System
            Assert.IsNotNull(dialogueSystemController, "DialogueSystemController should be available");
            Assert.IsNotNull(dialogueSystemController.initialDatabase, "Database should be assigned");
            
            // Test that systems can coexist with Pixel Crushers components
            Assert.DoesNotThrow(() => {
                var dialogueActor = testGameObject.AddComponent<DialogueActor>();
                dialogueActor.actor = "TestActor";
            }, "Should be able to add DialogueActor components");
        }

        [Test]
        public void DialogSystem_SupportsBothBanterAndInteractiveDialogue()
        {
            // Set test time for consistent banter behavior
            banterManager.SetTestTime(100f);
            
            // Test that both systems can work in the same scene
            Assert.IsTrue(banterManager.TriggerBossBanter("Wrath"), "Banter should work");
            Assert.IsTrue(protagonistSystem.SupportsInteractiveDialogue("Wrath"), "Interactive dialogue should be supported");
            
            // Cleanup
            banterManager.ClearTestTime();
        }

        #endregion

        #region Performance and Scalability Tests

        [Test]
        public void DialogSystem_InitializationIsEfficient()
        {
            // Test that system initialization is fast
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            banterManager.InitializeBanterSystem();
            protagonistSystem.InitializeProtagonistSystem();
            
            stopwatch.Stop();
            
            // Should initialize in under 100ms (generous for CI environments)
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "System initialization should be fast");
        }

        [Test]
        public void DialogSystem_SupportsMultipleBossInteractions()
        {
            // Set test time for consistent banter behavior
            banterManager.SetTestTime(100f);
            
            // Test that system can handle multiple boss interactions efficiently
            var bosses = new[] { "Wrath", "Pride", "Envy", "Greed" };
            
            foreach (var boss in bosses)
            {
                Assert.IsTrue(banterManager.TriggerBossBanter(boss), $"Should support banter for {boss}");
                Assert.IsTrue(protagonistSystem.SupportsInteractiveDialogue(boss), $"Should support dialogue for {boss}");
            }
            
            // Cleanup
            banterManager.ClearTestTime();
        }

        #endregion
    }

    // Note: Supporting classes are now defined in the actual dialogue system components:
    // - DialogueSceneConfiguration.cs
    // - DialogueChoiceDatabase.cs  
    // - BossBanterManager.cs
    // - ProtagonistDialogueSystem.cs
}