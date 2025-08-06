#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using UnityEditor;
using System.Collections;
using NeonLadder.Dialog;
using NeonLadder.Dialog.Editor;
using PixelCrushers.DialogueSystem;

namespace NeonLadder.Tests.Editor
{
    /// <summary>
    /// EditMode unit tests for DialogueSceneGenerator
    /// Tests scene generation, component creation, and integration with Unity Editor
    /// </summary>
    public class DialogueSceneGeneratorTests
    {
        private DialogueSceneConfiguration testConfig;
        private GameObject testCharacterPrefab;
        private GameObject secondTestCharacterPrefab;
        private GameObject generatedScene;

        [SetUp]
        public void Setup()
        {
            // Suppress dialogs during tests
            DialogueSceneGenerator.SetDialogSuppression(true);
            
            // Create test character prefabs
            testCharacterPrefab = new GameObject("TestBoss");
            testCharacterPrefab.AddComponent<Animator>();
            testCharacterPrefab.AddComponent<DialogueActor>();
            
            secondTestCharacterPrefab = new GameObject("TestPlayer");
            secondTestCharacterPrefab.AddComponent<Animator>();
            secondTestCharacterPrefab.AddComponent<DialogueActor>();
            
            // Create valid test configuration
            testConfig = ScriptableObject.CreateInstance<DialogueSceneConfiguration>();
            testConfig.sceneName = "Generated Test Scene";
            testConfig.description = "Auto-generated test scene";
            
            testConfig.leftCharacter = new DialogueCharacterConfig
            {
                characterName = "Boss",
                characterPrefab = testCharacterPrefab,
                position = new Vector3(-2f, 0f, 0f),
                rotation = new Vector3(0f, 45f, 0f),
                scale = Vector3.one,
                actorName = "BossActor",
                role = DialogueCharacterRole.Boss
            };
            
            testConfig.rightCharacter = new DialogueCharacterConfig
            {
                characterName = "Player",
                characterPrefab = secondTestCharacterPrefab,
                position = new Vector3(2f, 0f, 0f),
                rotation = new Vector3(0f, -45f, 0f),
                scale = Vector3.one,
                actorName = "PlayerActor",
                role = DialogueCharacterRole.Player
            };
            
            testConfig.conversationName = "TestConversation";
            testConfig.triggerBoxSize = new Vector3(3f, 2f, 3f);
            testConfig.triggerBoxOffset = Vector3.zero;
            testConfig.interactionType = DialogueInteractionType.FullConversation;
            testConfig.autoSetupPixelCrushersComponents = true;
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up created objects
            if (generatedScene != null)
                Object.DestroyImmediate(generatedScene);
            if (testCharacterPrefab != null)
                Object.DestroyImmediate(testCharacterPrefab);
            if (secondTestCharacterPrefab != null)
                Object.DestroyImmediate(secondTestCharacterPrefab);
            if (testConfig != null)
                Object.DestroyImmediate(testConfig);
                
            // Reset dialog suppression
            DialogueSceneGenerator.SetDialogSuppression(false);
        }

        #region Scene Generation Tests

        [Test]
        public void GenerateScene_CreatesSceneRootWithCorrectName()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNotNull(generatedScene, "Scene root should be created");
            Assert.AreEqual("Generated Test Scene", generatedScene.name);
        }

        [Test]
        public void GenerateScene_FailsForInvalidConfiguration()
        {
            // Create invalid configuration (missing conversation name)
            testConfig.conversationName = "";
            
            // Should show error dialog and not create scene
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNull(generatedScene, "Scene should not be created for invalid configuration");
        }

        [Test]
        public void GenerateScene_CreatesCharactersContainer()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            
            Assert.IsNotNull(charactersContainer, "Characters container should be created");
            Assert.AreEqual("Characters", charactersContainer.name);
        }

        [Test]
        public void GenerateScene_InstantiatesLeftCharacterCorrectly()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            var leftCharacter = charactersContainer.Find("Boss (Left)");
            
            Assert.IsNotNull(leftCharacter, "Left character should be instantiated");
            Assert.AreEqual(new Vector3(-2f, 0f, 0f), leftCharacter.localPosition);
            Assert.AreEqual(Quaternion.Euler(0f, 45f, 0f), leftCharacter.localRotation);
            Assert.AreEqual(Vector3.one, leftCharacter.localScale);
        }

        [Test]
        public void GenerateScene_InstantiatesRightCharacterCorrectly()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            var rightCharacter = charactersContainer.Find("Player (Right)");
            
            Assert.IsNotNull(rightCharacter, "Right character should be instantiated");
            Assert.AreEqual(new Vector3(2f, 0f, 0f), rightCharacter.localPosition);
            Assert.AreEqual(Quaternion.Euler(0f, -45f, 0f), rightCharacter.localRotation);
            Assert.AreEqual(Vector3.one, rightCharacter.localScale);
        }

        #endregion

        #region Trigger Box Generation Tests

        [Test]
        public void GenerateScene_CreatesTriggerBoxWithCorrectProperties()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var triggerBox = generatedScene.transform.Find("Dialogue Trigger");
            
            Assert.IsNotNull(triggerBox, "Trigger box should be created");
            
            var boxCollider = triggerBox.GetComponent<BoxCollider>();
            Assert.IsNotNull(boxCollider, "BoxCollider should be added");
            Assert.IsTrue(boxCollider.isTrigger, "BoxCollider should be set as trigger");
            Assert.AreEqual(new Vector3(3f, 2f, 3f), boxCollider.size);
        }

        [Test]
        public void GenerateScene_AddsDialogueSystemTriggerComponent()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var triggerBox = generatedScene.transform.Find("Dialogue Trigger");
            var dialogueTrigger = triggerBox.GetComponent<DialogueSystemTrigger>();
            
            Assert.IsNotNull(dialogueTrigger, "DialogueSystemTrigger should be added");
            Assert.AreEqual(DialogueSystemTriggerEvent.OnTriggerEnter, dialogueTrigger.trigger);
            Assert.AreEqual("TestConversation", dialogueTrigger.conversation);
        }

        [Test]
        public void GenerateScene_TriggerBoxPositionedCorrectly()
        {
            testConfig.triggerBoxOffset = new Vector3(1f, 0.5f, -1f);
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var triggerBox = generatedScene.transform.Find("Dialogue Trigger");
            
            Assert.AreEqual(new Vector3(1f, 0.5f, -1f), triggerBox.localPosition);
        }

        #endregion

        #region Dialogue Components Generation Tests

        [Test]
        public void GenerateScene_CreatesDialogueSystemContainer()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            
            Assert.IsNotNull(dialogueContainer, "Dialogue System container should be created");
        }

        [Test]
        public void GenerateScene_AddsCorrectComponentForSimpleBanter()
        {
            testConfig.interactionType = DialogueInteractionType.SimpleBanter;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            var banterManager = dialogueContainer.GetComponent<BossBanterManager>();
            
            Assert.IsNotNull(banterManager, "BossBanterManager should be added for SimpleBanter");
        }

        [Test]
        public void GenerateScene_AddsCorrectComponentForPlayerChoice()
        {
            testConfig.interactionType = DialogueInteractionType.PlayerChoice;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            var protagonistSystem = dialogueContainer.GetComponent<ProtagonistDialogueSystem>();
            
            Assert.IsNotNull(protagonistSystem, "ProtagonistDialogueSystem should be added for PlayerChoice");
        }

        [Test]
        public void GenerateScene_AddsCorrectComponentForFullConversation()
        {
            testConfig.interactionType = DialogueInteractionType.FullConversation;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            var protagonistSystem = dialogueContainer.GetComponent<ProtagonistDialogueSystem>();
            
            Assert.IsNotNull(protagonistSystem, "ProtagonistDialogueSystem should be added for FullConversation");
        }

        [Test]
        public void GenerateScene_AddsDialogueSystemControllerWhenNeeded()
        {
            // Ensure no existing DialogueSystemController in scene
            var existing = Object.FindObjectOfType<DialogueSystemController>();
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);
            
            testConfig.autoSetupPixelCrushersComponents = true;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            var dialogueSystemController = dialogueContainer.GetComponent<DialogueSystemController>();
            
            Assert.IsNotNull(dialogueSystemController, "DialogueSystemController should be added when autoSetup is true");
        }

        [Test]
        public void GenerateScene_DoesNotAddDuplicateDialogueSystemController()
        {
            // Create existing DialogueSystemController
            var existingGO = new GameObject("ExistingDialogueSystem");
            existingGO.AddComponent<DialogueSystemController>();
            
            testConfig.autoSetupPixelCrushersComponents = true;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var dialogueContainer = generatedScene.transform.Find("Dialogue System");
            var dialogueSystemController = dialogueContainer.GetComponent<DialogueSystemController>();
            
            Assert.IsNull(dialogueSystemController, "Should not add duplicate DialogueSystemController");
            
            Object.DestroyImmediate(existingGO);
        }

        #endregion

        #region Camera Generation Tests

        [Test]
        public void GenerateScene_CreatesDefaultCameraSetup()
        {
            testConfig.cameraSetup = new DialogueCameraSetup
            {
                position = new Vector3(0f, 1.5f, -4f),
                rotation = new Vector3(10f, 0f, 0f),
                fieldOfView = 60f,
                useCustomCamera = false
            };
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var cameraContainer = generatedScene.transform.Find("Dialogue Camera");
            
            Assert.IsNotNull(cameraContainer, "Camera container should be created");
            Assert.AreEqual(new Vector3(0f, 1.5f, -4f), cameraContainer.localPosition);
            Assert.AreEqual(Quaternion.Euler(10f, 0f, 0f), cameraContainer.localRotation);
            
            var camera = cameraContainer.GetComponent<Camera>();
            Assert.IsNotNull(camera, "Camera component should be added");
            Assert.AreEqual(60f, camera.fieldOfView);
            Assert.IsFalse(camera.enabled, "Camera should start disabled");
        }

        [Test]
        public void GenerateScene_InstantiatesCustomCameraPrefab()
        {
            var customCameraPrefab = new GameObject("CustomCamera");
            customCameraPrefab.AddComponent<Camera>();
            
            testConfig.cameraSetup = new DialogueCameraSetup
            {
                useCustomCamera = true,
                customCameraPrefab = customCameraPrefab
            };
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var customCamera = generatedScene.transform.Find("Custom Dialogue Camera");
            
            Assert.IsNotNull(customCamera, "Custom camera should be instantiated");
            Assert.AreEqual("Custom Dialogue Camera", customCamera.name);
            
            Object.DestroyImmediate(customCameraPrefab);
        }

        #endregion

        #region UI Generation Tests

        [Test]
        public void GenerateScene_CreatesBasicUICanvasWhenNoPrefab()
        {
            testConfig.dialogueUIPrefab = null;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var uiContainer = generatedScene.transform.Find("Dialogue UI");
            
            Assert.IsNotNull(uiContainer, "UI container should be created");
            
            var canvas = uiContainer.GetComponent<Canvas>();
            Assert.IsNotNull(canvas, "Canvas should be added");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, canvas.renderMode);
            Assert.AreEqual(100, canvas.sortingOrder);
            
            var canvasScaler = uiContainer.GetComponent<UnityEngine.UI.CanvasScaler>();
            Assert.IsNotNull(canvasScaler, "CanvasScaler should be added");
            Assert.AreEqual(new Vector2(1920, 1080), canvasScaler.referenceResolution);
            
            var graphicRaycaster = uiContainer.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            Assert.IsNotNull(graphicRaycaster, "GraphicRaycaster should be added");
        }

        [Test]
        public void GenerateScene_InstantiatesCustomUIPrefab()
        {
            var customUIPrefab = new GameObject("CustomDialogueUI");
            customUIPrefab.AddComponent<Canvas>();
            
            testConfig.dialogueUIPrefab = customUIPrefab;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var customUI = generatedScene.transform.Find("Dialogue UI");
            
            Assert.IsNotNull(customUI, "Custom UI should be instantiated");
            Assert.AreEqual("Dialogue UI", customUI.name);
            
            Object.DestroyImmediate(customUIPrefab);
        }

        #endregion

        #region Character Actor Tests

        [Test]
        public void GenerateScene_AddsDialogueActorToCharacters()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            var leftCharacter = charactersContainer.Find("Boss (Left)");
            var rightCharacter = charactersContainer.Find("Player (Right)");
            
            var leftActor = leftCharacter.GetComponent<DialogueActor>();
            var rightActor = rightCharacter.GetComponent<DialogueActor>();
            
            Assert.IsNotNull(leftActor, "Left character should have DialogueActor");
            Assert.AreEqual("BossActor", leftActor.actor);
            
            Assert.IsNotNull(rightActor, "Right character should have DialogueActor");
            Assert.AreEqual("PlayerActor", rightActor.actor);
        }

        [Test]
        public void GenerateScene_AddsAnimatorWhenSpecified()
        {
            var tempPath = "Assets/test_animator.controller";
            var animatorController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(tempPath);
            
            testConfig.leftCharacter.animatorController = animatorController;
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            var leftCharacter = charactersContainer.Find("Boss (Left)");
            
            var animator = leftCharacter.GetComponent<Animator>();
            Assert.IsNotNull(animator, "Animator should be added when controller is specified");
            Assert.AreEqual(animatorController, animator.runtimeAnimatorController);
            
            Object.DestroyImmediate(animatorController, true);
        }

        [Test]
        public void GenerateScene_AddsAudioSourceWhenVoiceClipsSpecified()
        {
            var voiceClip = AudioClip.Create("TestVoiceClip", 44100, 1, 44100, false);
            testConfig.leftCharacter.voiceClips = new AudioClip[] { voiceClip };
            
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            var leftCharacter = charactersContainer.Find("Boss (Left)");
            
            var audioSource = leftCharacter.GetComponent<AudioSource>();
            Assert.IsNotNull(audioSource, "AudioSource should be added when voice clips are specified");
            Assert.IsFalse(audioSource.playOnAwake);
            Assert.AreEqual(0.5f, audioSource.spatialBlend);
            
            Object.DestroyImmediate(voiceClip);
        }

        #endregion

        #region Menu Item Tests

        [Test]
        public void GenerateDialogueSceneFromMenu_WorksWithSelectedConfiguration()
        {
            // Simulate selecting the configuration in the Project window
            Selection.activeObject = testConfig;
            
            DialogueSceneGenerator.GenerateDialogueSceneFromMenu();
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNotNull(generatedScene, "Scene should be generated from menu item");
        }

        [Test]
        public void GenerateDialogueSceneFromMenu_ShowsErrorForNoSelection()
        {
            // Clear selection
            Selection.activeObject = null;
            
            // Should show error dialog and not create scene
            DialogueSceneGenerator.GenerateDialogueSceneFromMenu();
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNull(generatedScene, "Scene should not be created when no configuration is selected");
        }

        [Test]
        public void GenerateDialogueSceneFromMenu_ShowsErrorForWrongSelectionType()
        {
            // Select wrong type of object
            Selection.activeObject = new GameObject("NotAConfiguration");
            
            DialogueSceneGenerator.GenerateDialogueSceneFromMenu();
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNull(generatedScene, "Scene should not be created when wrong type is selected");
            
            Object.DestroyImmediate(Selection.activeObject);
        }

        [Test]
        public void GenerateDialogueSceneFromMenu_SelectsGeneratedScene()
        {
            Selection.activeObject = testConfig;
            
            DialogueSceneGenerator.GenerateDialogueSceneFromMenu();
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.AreEqual(generatedScene, Selection.activeGameObject, "Generated scene should be selected");
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void GenerateScene_HandlesNullCharacterPrefabsGracefully()
        {
            testConfig.leftCharacter.characterPrefab = null;
            testConfig.rightCharacter.characterPrefab = null;
            
            // Should not throw exception
            Assert.DoesNotThrow(() => DialogueSceneGenerator.GenerateScene(testConfig));
            
            generatedScene = GameObject.Find("Generated Test Scene");
            var charactersContainer = generatedScene.transform.Find("Characters");
            
            // Characters container should still be created
            Assert.IsNotNull(charactersContainer, "Characters container should be created even with null prefabs");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Integration_GeneratedSceneWorksWithUnityEditorUndo()
        {
            DialogueSceneGenerator.GenerateScene(testConfig);
            
            generatedScene = GameObject.Find("Generated Test Scene");
            Assert.IsNotNull(generatedScene, "Scene should be created");
            
            // Test that undo is registered (Undo.RegisterCreatedObjectUndo was called)
            Undo.PerformUndo();
            
            // After undo, the scene should be destroyed
            var sceneAfterUndo = GameObject.Find("Generated Test Scene");
            Assert.IsNull(sceneAfterUndo, "Scene should be undone");
            
            // Clear the reference since it's been destroyed by undo
            generatedScene = null;
        }

        #endregion
    }
}
#endif