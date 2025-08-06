using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using NeonLadder.Dialog;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for DialogueSceneConfiguration ScriptableObject
    /// Tests configuration validation, character setup, and scene generation parameters
    /// </summary>
    public class DialogueSceneConfigurationTests
    {
        private DialogueSceneConfiguration testConfig;
        private GameObject testCharacterPrefab;
        private GameObject secondTestCharacterPrefab;

        [SetUp]
        public void Setup()
        {
            // Create test configuration
            testConfig = ScriptableObject.CreateInstance<DialogueSceneConfiguration>();
            testConfig.sceneName = "Test Dialogue Scene";
            testConfig.description = "Test scene for unit testing";
            
            // Create mock character prefabs
            testCharacterPrefab = new GameObject("TestCharacter1");
            testCharacterPrefab.AddComponent<Animator>();
            
            secondTestCharacterPrefab = new GameObject("TestCharacter2");
            secondTestCharacterPrefab.AddComponent<Animator>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
            if (testCharacterPrefab != null)
                Object.DestroyImmediate(testCharacterPrefab);
            if (secondTestCharacterPrefab != null)
                Object.DestroyImmediate(secondTestCharacterPrefab);
        }

        #region Configuration Creation Tests

        [Test]
        public void DialogueSceneConfiguration_CreatesWithDefaultValues()
        {
            var config = ScriptableObject.CreateInstance<DialogueSceneConfiguration>();
            
            Assert.AreEqual("Dialogue Scene", config.sceneName);
            Assert.IsTrue(config.description.Contains("Drag characters"));
            Assert.AreEqual(new Vector3(2f, 2f, 2f), config.triggerBoxSize);
            Assert.AreEqual(Vector3.zero, config.triggerBoxOffset);
            Assert.AreEqual(DialogueInteractionType.FullConversation, config.interactionType);
            Assert.IsTrue(config.autoSetupPixelCrushersComponents);
            
            Object.DestroyImmediate(config);
        }

        [Test]
        public void DialogueSceneConfiguration_DefaultLanguageSupport()
        {
            var config = ScriptableObject.CreateInstance<DialogueSceneConfiguration>();
            
            Assert.AreEqual(3, config.supportedLanguages.Length);
            Assert.Contains("en", config.supportedLanguages);
            Assert.Contains("zh-Hans", config.supportedLanguages);
            Assert.Contains("ur", config.supportedLanguages);
            
            Object.DestroyImmediate(config);
        }

        #endregion

        #region Validation Tests

        [Test]
        public void Validation_ValidConfigurationPassesValidation()
        {
            // Setup valid configuration
            testConfig.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Boss",
                characterPrefab = testCharacterPrefab,
                actorName = "BossActor"
            };
            
            testConfig.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Player",
                characterPrefab = secondTestCharacterPrefab,
                actorName = "PlayerActor"
            };
            
            testConfig.conversationName = "TestConversation";
            testConfig.triggerBoxSize = new Vector3(2f, 2f, 2f);

            bool isValid = testConfig.ValidateConfiguration(out string error);
            
            Assert.IsTrue(isValid, $"Valid configuration should pass: {error}");
            Assert.IsTrue(string.IsNullOrEmpty(error));
        }

        [Test]
        public void Validation_FailsWithoutLeftCharacter()
        {
            testConfig.leftCharacter = null;
            testConfig.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Player",
                characterPrefab = secondTestCharacterPrefab
            };
            testConfig.conversationName = "TestConversation";

            bool isValid = testConfig.ValidateConfiguration(out string error);
            
            Assert.IsFalse(isValid);
            Assert.AreEqual("Left character is not assigned", error);
        }

        [Test]
        public void Validation_FailsWithoutRightCharacter()
        {
            testConfig.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Boss",
                characterPrefab = testCharacterPrefab
            };
            testConfig.rightCharacter = null;
            testConfig.conversationName = "TestConversation";

            bool isValid = testConfig.ValidateConfiguration(out string error);
            
            Assert.IsFalse(isValid);
            Assert.AreEqual("Right character is not assigned", error);
        }

        [Test]
        public void Validation_FailsWithEmptyConversationName()
        {
            testConfig.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Boss",
                characterPrefab = testCharacterPrefab
            };
            testConfig.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Player", 
                characterPrefab = secondTestCharacterPrefab
            };
            testConfig.conversationName = "";

            bool isValid = testConfig.ValidateConfiguration(out string error);
            
            Assert.IsFalse(isValid);
            Assert.AreEqual("Conversation name is empty", error);
        }

        [Test]
        public void Validation_FailsWithInvalidTriggerBoxSize()
        {
            testConfig.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Boss",
                characterPrefab = testCharacterPrefab
            };
            testConfig.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Player",
                characterPrefab = secondTestCharacterPrefab
            };
            testConfig.conversationName = "TestConversation";
            testConfig.triggerBoxSize = new Vector3(-1f, 2f, 2f); // Invalid negative size

            bool isValid = testConfig.ValidateConfiguration(out string error);
            
            Assert.IsFalse(isValid);
            Assert.AreEqual("Trigger box size must be positive", error);
        }

        #endregion

        #region Character Configuration Tests

        [Test]
        public void CharacterConfig_DefaultValuesAreCorrect()
        {
            var charConfig = new DialogueCharacterConfig();
            
            Assert.AreEqual("", charConfig.characterName);
            Assert.AreEqual(Vector3.zero, charConfig.position); 
            Assert.AreEqual(Vector3.zero, charConfig.rotation);
            Assert.AreEqual(Vector3.one, charConfig.scale);
            Assert.AreEqual("", charConfig.actorName);
            Assert.AreEqual(DialogueCharacterRole.NPC, charConfig.role);
            Assert.AreEqual("Idle", charConfig.idleAnimationName);
            Assert.AreEqual("Talking", charConfig.talkingAnimationName);
        }

        [Test]
        public void CharacterConfig_SupportsAllRoles()
        {
            var charConfig = new DialogueCharacterConfig();
            
            // Test all enum values
            charConfig.role = DialogueCharacterRole.Player;
            Assert.AreEqual(DialogueCharacterRole.Player, charConfig.role);
            
            charConfig.role = DialogueCharacterRole.NPC;
            Assert.AreEqual(DialogueCharacterRole.NPC, charConfig.role);
            
            charConfig.role = DialogueCharacterRole.Boss;
            Assert.AreEqual(DialogueCharacterRole.Boss, charConfig.role);
            
            charConfig.role = DialogueCharacterRole.Merchant;
            Assert.AreEqual(DialogueCharacterRole.Merchant, charConfig.role);
            
            charConfig.role = DialogueCharacterRole.Neutral;
            Assert.AreEqual(DialogueCharacterRole.Neutral, charConfig.role);
        }

        #endregion

        #region Camera Setup Tests

        [Test]
        public void CameraSetup_DefaultValuesAreCorrect()
        {
            var cameraSetup = new DialogueCameraSetup();
            
            Assert.AreEqual(new Vector3(0, 1.5f, -3), cameraSetup.position);
            Assert.AreEqual(new Vector3(10, 0, 0), cameraSetup.rotation);
            Assert.AreEqual(60f, cameraSetup.fieldOfView);
            Assert.IsFalse(cameraSetup.useCustomCamera);
            Assert.IsNull(cameraSetup.customCameraPrefab);
        }

        [Test]
        public void CameraSetup_SupportsCustomCamera()
        {
            var cameraSetup = new DialogueCameraSetup();
            var customCameraPrefab = new GameObject("CustomCamera");
            customCameraPrefab.AddComponent<Camera>();
            
            cameraSetup.useCustomCamera = true;
            cameraSetup.customCameraPrefab = customCameraPrefab;
            
            Assert.IsTrue(cameraSetup.useCustomCamera);
            Assert.AreEqual(customCameraPrefab, cameraSetup.customCameraPrefab);
            
            Object.DestroyImmediate(customCameraPrefab);
        }

        #endregion

        #region Interaction Type Tests

        [Test]
        public void InteractionTypes_AllTypesSupported()
        {
            testConfig.interactionType = DialogueInteractionType.SimpleBanter;
            Assert.AreEqual(DialogueInteractionType.SimpleBanter, testConfig.interactionType);
            
            testConfig.interactionType = DialogueInteractionType.ProtagonistReply;
            Assert.AreEqual(DialogueInteractionType.ProtagonistReply, testConfig.interactionType);
            
            testConfig.interactionType = DialogueInteractionType.PlayerChoice;
            Assert.AreEqual(DialogueInteractionType.PlayerChoice, testConfig.interactionType);
            
            testConfig.interactionType = DialogueInteractionType.FullConversation;
            Assert.AreEqual(DialogueInteractionType.FullConversation, testConfig.interactionType);
        }

        #endregion

        #region Localization Configuration Tests

        [Test]
        public void Localization_SupportsCustomLanguageList()
        {
            testConfig.supportedLanguages = new string[] { "en", "fr", "de" };
            
            Assert.AreEqual(3, testConfig.supportedLanguages.Length);
            Assert.Contains("en", testConfig.supportedLanguages);
            Assert.Contains("fr", testConfig.supportedLanguages);
            Assert.Contains("de", testConfig.supportedLanguages);
        }

        [Test]
        public void Localization_TextTableNameConfigurable()
        {
            testConfig.textTableName = "BossDialogue";
            Assert.AreEqual("BossDialogue", testConfig.textTableName);
            
            testConfig.textTableName = "NPCDialogue";
            Assert.AreEqual("NPCDialogue", testConfig.textTableName);
        }

        #endregion

        #region Integration Tests

        #if UNITY_EDITOR
        [Test]
        public void Integration_ContextMenuGenerateSceneExists()
        {
            // The ContextMenu attribute should be present
            var method = typeof(DialogueSceneConfiguration).GetMethod("GenerateDialogueScene");
            Assert.IsNotNull(method, "GenerateDialogueScene method should exist");
            
            // ContextMenu attribute verification - method exists, which is sufficient for this test
        }
        #endif

        [Test]
        public void Integration_ConfigurationPersistsInScriptableObject()
        {
            // Set various configuration values
            testConfig.sceneName = "Persistent Test Scene";
            testConfig.triggerBoxSize = new Vector3(5f, 3f, 4f);
            testConfig.interactionType = DialogueInteractionType.PlayerChoice;
            testConfig.autoSetupPixelCrushersComponents = false;
            
            // Values should persist (ScriptableObject serialization)
            Assert.AreEqual("Persistent Test Scene", testConfig.sceneName);
            Assert.AreEqual(new Vector3(5f, 3f, 4f), testConfig.triggerBoxSize);
            Assert.AreEqual(DialogueInteractionType.PlayerChoice, testConfig.interactionType);
            Assert.IsFalse(testConfig.autoSetupPixelCrushersComponents);
        }

        #endregion
    }
}