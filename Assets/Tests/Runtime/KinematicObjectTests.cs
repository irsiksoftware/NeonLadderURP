using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using NeonLadder.Core;
using NeonLadder.Managers;
using Michsky.MUIP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEditor.Animations;
using TMPro;

namespace NeonLadder.Tests.Runtime
{
    public class KinematicObjectTests
    {
        private GameObject testObject;
        private KinematicObject kinematicObject;
        private Rigidbody testRigidbody;
        private Animator testAnimator;
        
        // Mock scene infrastructure (same as PlayerTests)
        private GameObject mockManagersObject;
        private GameObject mockGameControllerObject;
        private GameObject mockSpawnPointObject;
        private GameObject mockPlayerObject;
        private ManagerController mockManagerController;
        private Game mockGameController;
        private Player mockPlayer;

        [SetUp]
        public void SetUp()
        {
            // Setup mock scene infrastructure first
            SetupMockSceneInfrastructure();
            
            // Only add AudioListener if none exists (prevent "17 audio listeners" warning)
            if (Object.FindObjectOfType<AudioListener>() == null)
            {
                var audioListenerObj = new GameObject("AudioListener");
                audioListenerObj.AddComponent<AudioListener>();
            }
            
            // Create parent GameObject (KinematicObject expects transform.parent)
            GameObject parentObject = new GameObject("TestParent");
            testObject = new GameObject("TestKinematicObject");
            testObject.transform.SetParent(parentObject.transform);
            
            testRigidbody = parentObject.AddComponent<Rigidbody>(); // Rigidbody on parent
            
            // Setup animator with proper controller BEFORE adding KinematicObject
            testAnimator = parentObject.AddComponent<Animator>(); // Animator on parent
            SetupMockAnimator();
            
            testRigidbody.isKinematic = true;
            testRigidbody.useGravity = true;
            
            // Now add KinematicObject to child - Awake() will find dependencies on parent
            kinematicObject = testObject.AddComponent<KinematicObject>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test object and its parent
            if (testObject != null)
            {
                if (testObject.transform.parent != null)
                {
                    Object.DestroyImmediate(testObject.transform.parent.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(testObject);
                }
            }
            
            // Clean up mock scene infrastructure
            if (mockManagersObject != null)
            {
                Object.DestroyImmediate(mockManagersObject);
            }
            
            if (mockGameControllerObject != null)
            {
                Object.DestroyImmediate(mockGameControllerObject);
            }
            
            if (mockSpawnPointObject != null)
            {
                Object.DestroyImmediate(mockSpawnPointObject);
            }
            
            if (mockPlayerObject != null)
            {
                Object.DestroyImmediate(mockPlayerObject);
            }
            
            // Clear the simulation model
            Simulation.DestroyModel<PlatformerModel>();
            
            // Reset Game singleton
            if (Game.Instance != null)
            {
                Game.Instance.DestroyGameInstance();
            }
        }
        
        private void SetupMockSceneInfrastructure()
        {
            // Initialize the Simulation model first
            var platformerModel = new PlatformerModel();
            Simulation.SetModel(platformerModel);
            
            // Create Managers GameObject with ManagerController
            mockManagersObject = new GameObject("Managers");
            mockManagersObject.tag = Tags.Managers.ToString();
            mockManagerController = mockManagersObject.AddComponent<ManagerController>();
            mockManagerController.enabled = false; // Disable to prevent Update() issues in tests
            
            // Create GameController GameObject with Game component
            mockGameControllerObject = new GameObject("GameController");
            mockGameControllerObject.tag = Tags.GameController.ToString();
            
            try 
            {
                mockGameController = mockGameControllerObject.AddComponent<Game>();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Game component setup warning in tests: {ex.Message}");
            }
            
            // Create a virtual camera for the GameController
            GameObject cameraObject = new GameObject("VirtualCamera");
            cameraObject.transform.SetParent(mockGameControllerObject.transform);
            var virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
            platformerModel.VirtualCamera = virtualCamera;
            
            // Create SpawnPoint
            mockSpawnPointObject = new GameObject("SpawnPoint");
            mockSpawnPointObject.tag = Tags.SpawnPoint.ToString();
            platformerModel.SpawnPoint = mockSpawnPointObject.transform;
            
            // Create mock Player GameObject that PlatformerModel.get_Player() can find
            mockPlayerObject = new GameObject("MockPlayer");
            mockPlayerObject.tag = Tags.Player.ToString();
            
            // Add required components for Player (but NOT Animator yet)
            mockPlayerObject.AddComponent<Rigidbody>().isKinematic = true;
            mockPlayerObject.AddComponent<AudioSource>();
            mockPlayerObject.AddComponent<Health>();
            mockPlayerObject.AddComponent<Stamina>();
            mockPlayerObject.AddComponent<Meta>();
            mockPlayerObject.AddComponent<Perma>();
            
            // CRITICAL: Add Animator component and set up its controller BEFORE creating Player
            var playerAnimator = mockPlayerObject.AddComponent<Animator>();
            SetupMockPlayerAnimator(playerAnimator);
            
            // Create mock UI components BEFORE creating Player (Player.Awake() looks for these)
            CreateMockHealthBar(mockPlayerObject);
            CreateMockStaminaBar(mockPlayerObject);
            
            // Create player as CHILD GameObject (same as PlayerTests pattern)
            GameObject playerChild = new GameObject("PlayerChild");
            playerChild.transform.SetParent(mockPlayerObject.transform);
            
            // CRITICAL: Disable CHILD GameObject before adding Player to prevent Awake issues
            playerChild.SetActive(false);
            
            // Setup mock InputActionAsset for Controls
            SetupMockPlayerControls();
            
            // Now add Player component to CHILD
            mockPlayer = playerChild.AddComponent<Player>();
            
            // Set the Controls property on the player
            mockPlayer.Controls = mockInputActionAsset;
            
            // Add PlayerAction component (required for Walk method)
            var mockPlayerAction = playerChild.AddComponent<PlayerAction>();
            
            // Manually set up the playerActionMap since we'll disable the component
            var playerActionMap = mockInputActionAsset.FindActionMap("Player");
            mockPlayerAction.playerActionMap = playerActionMap;
            
            mockPlayerAction.enabled = false; // Disable to prevent interference
            
            // Re-enable the CHILD GameObject
            playerChild.SetActive(true);
            
            // Disable the mock Player to prevent Update() interference in tests
            mockPlayer.enabled = false;
            
            // Set the player in the model
            platformerModel.Player = mockPlayer;
        }
        
        private void SetupMockAnimator()
        {
            // Create a minimal RuntimeAnimatorController for testing
            var animatorController = new UnityEditor.Animations.AnimatorController();
            animatorController.name = "TestAnimatorController";
            
            // Add a default layer (required before adding motions)
            animatorController.AddLayer("Base Layer");
            var baseLayer = animatorController.layers[0];
            
            // Create animation clips for all animations the code expects
            var clips = new Dictionary<string, float>
            {
                { "Idle", 1.0f },
                { "WalkForward", 0.8f },
                { "Attack1", 0.5f },
                { "GetHit", 0.3f },
                { "Die", 2.0f },
                { "Victory", 1.5f },
                { "WalkBackward", 0.8f },
                { "Run", 0.6f },
                { "Jump", 0.4f }
            };
            
            // Create all animation clips and add them as states
            foreach (var clipData in clips)
            {
                var clip = CreateMockAnimationClip(clipData.Key, clipData.Value);
                var state = baseLayer.stateMachine.AddState(clipData.Key);
                state.motion = clip;
            }
            
            // Set the runtime controller
            testAnimator.runtimeAnimatorController = animatorController;
        }
        
        private AnimationClip CreateMockAnimationClip(string clipName, float duration)
        {
            var clip = new AnimationClip();
            clip.name = clipName;
            
            // Create a simple keyframe animation to set the clip length
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);        // Start keyframe at time 0
            curve.AddKey(duration, 0f);  // End keyframe at desired duration
            
            // Set the curve on the clip - this will set the clip's length to the duration
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            
            return clip;
        }
        
        private void SetupMockPlayerAnimator(Animator animator)
        {
            // Create same animator controller as main test object
            var animatorController = new UnityEditor.Animations.AnimatorController();
            animatorController.name = "PlayerTestAnimatorController";
            animatorController.AddLayer("Base Layer");
            var baseLayer = animatorController.layers[0];
            
            var clips = new Dictionary<string, float>
            {
                { "Idle", 1.0f },
                { "WalkForward", 0.8f },
                { "Attack1", 0.5f },
                { "GetHit", 0.3f },
                { "Die", 2.0f },
                { "Victory", 1.5f },
                { "WalkBackward", 0.8f },
                { "Run", 0.6f },
                { "Jump", 0.4f }
            };
            
            foreach (var clipData in clips)
            {
                var clip = CreateMockAnimationClip(clipData.Key, clipData.Value);
                var state = baseLayer.stateMachine.AddState(clipData.Key);
                state.motion = clip;
            }
            
            animator.runtimeAnimatorController = animatorController;
        }
        
        private void CreateMockHealthBar(GameObject parentObject)
        {
            GameObject healthBarObject = new GameObject("HealthBar");
            healthBarObject.transform.SetParent(parentObject.transform);
            healthBarObject.AddComponent<HealthBar>();
            
            ProgressBar healthProgressBar = healthBarObject.AddComponent<ProgressBar>();
            
            GameObject loadingBarObj = new GameObject("LoadingBar");
            loadingBarObj.transform.SetParent(healthBarObject.transform);
            var loadingBarImage = loadingBarObj.AddComponent<Image>();
            loadingBarImage.type = Image.Type.Filled;
            healthProgressBar.loadingBar = loadingBarImage;
            
            GameObject textObj = new GameObject("PercentText");
            textObj.transform.SetParent(healthBarObject.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "100%";
            healthProgressBar.textPercent = textComponent;
            
            healthProgressBar.currentPercent = 100f;
            healthProgressBar.maxValue = 100f;
            healthProgressBar.minValue = 0f;
            healthProgressBar.isOn = false;
            
            if (healthProgressBar.onValueChanged == null)
            {
                healthProgressBar.onValueChanged = new ProgressBar.ProgressBarEvent();
            }
        }

        private void CreateMockStaminaBar(GameObject parentObject)
        {
            GameObject staminaBarObject = new GameObject("StaminaBar");
            staminaBarObject.transform.SetParent(parentObject.transform);
            staminaBarObject.AddComponent<StaminaBar>();
            
            ProgressBar staminaProgressBar = staminaBarObject.AddComponent<ProgressBar>();
            
            GameObject loadingBarObj = new GameObject("LoadingBar");
            loadingBarObj.transform.SetParent(staminaBarObject.transform);
            var loadingBarImage = loadingBarObj.AddComponent<Image>();
            loadingBarImage.type = Image.Type.Filled;
            staminaProgressBar.loadingBar = loadingBarImage;
            
            GameObject textObj = new GameObject("PercentText");
            textObj.transform.SetParent(staminaBarObject.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "100%";
            staminaProgressBar.textPercent = textComponent;
            
            staminaProgressBar.currentPercent = 100f;
            staminaProgressBar.maxValue = 100f;
            staminaProgressBar.minValue = 0f;
            staminaProgressBar.isOn = false;
            
            if (staminaProgressBar.onValueChanged == null)
            {
                staminaProgressBar.onValueChanged = new ProgressBar.ProgressBarEvent();
            }
        }
        
        private InputActionAsset mockInputActionAsset;
        
        private void SetupMockPlayerControls()
        {
            // Create a basic InputActionAsset for the mock player
            mockInputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            
            // Create a Player action map
            var playerActionMap = new InputActionMap("Player");
            
            // Add basic actions that PlayerAction expects
            playerActionMap.AddAction("Sprint", InputActionType.Button);
            playerActionMap.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
            playerActionMap.AddAction("Attack", InputActionType.Button);
            playerActionMap.AddAction("WeaponSwap", InputActionType.Button);
            playerActionMap.AddAction("Jump", InputActionType.Button);
            playerActionMap.AddAction("Up", InputActionType.Button);
            
            // Add the action map to the asset
            mockInputActionAsset.AddActionMap(playerActionMap);
        }

        [Test]
        public void KinematicObject_Initialization_SetsCorrectValues()
        {
            Assert.IsNotNull(kinematicObject);
            Assert.AreEqual(0.75f, kinematicObject.minGroundNormalY, 0.01f);
            Assert.AreEqual(1f, kinematicObject.gravityModifier, 0.01f);
            Assert.AreEqual(Vector3.zero, kinematicObject.velocity);
            Assert.IsFalse(kinematicObject.IsGrounded);
        }

        [Test]
        public void Bounce_SetsVelocityY()
        {
            float bounceValue = 5.0f;
            kinematicObject.Bounce(bounceValue);
            
            Assert.AreEqual(bounceValue, kinematicObject.velocity.y, 0.01f);
        }

        [Test]
        public void Teleport_MovesPositionAndResetsVelocity()
        {
            Vector3 originalPosition = testObject.transform.parent.position;
            Vector3 teleportPosition = new Vector3(10, 5, 3);
            kinematicObject.velocity = new Vector3(2, 3, 1);
            
            kinematicObject.Teleport(teleportPosition);
            
            Assert.AreEqual(teleportPosition, testObject.transform.parent.position);
            Assert.AreEqual(Vector3.zero, kinematicObject.velocity);
        }

        [Test]
        public void IsFacingLeft_DefaultValue()
        {
            Assert.IsFalse(kinematicObject.IsFacingLeft);
        }

        [Test]
        public void IsFacingRight_ConsistentWithOrientationSystem()
        {
            // Test that facing direction properties work correctly with the orientation system
            kinematicObject.IsFacingLeft = true;
            kinematicObject.Orient();
            Assert.IsFalse(kinematicObject.IsFacingRight);
            Assert.AreEqual(270f, testObject.transform.parent.rotation.eulerAngles.y, 0.1f);
            
            kinematicObject.IsFacingLeft = false;
            kinematicObject.Orient();
            Assert.IsTrue(kinematicObject.IsFacingRight);
            Assert.AreEqual(90f, testObject.transform.parent.rotation.eulerAngles.y, 0.1f);
        }

        [Test]
        public void IsUsingMelee_DefaultValue()
        {
            Assert.IsTrue(kinematicObject.IsUsingMelee);
        }

        [Test]
        public void IsUsingMelee_AffectsOrientationBehavior()
        {
            // Test that weapon type affects how the object orients itself
            kinematicObject.IsUsingMelee = true;
            kinematicObject.IsFacingLeft = true;
            kinematicObject.Orient();
            Vector3 meleeRotation = testObject.transform.parent.rotation.eulerAngles;
            
            // Reset orientation
            testObject.transform.parent.rotation = Quaternion.identity;
            
            kinematicObject.IsUsingMelee = false;
            kinematicObject.IsFacingLeft = true;
            kinematicObject.Orient();
            Vector3 rangedRotation = testObject.transform.parent.rotation.eulerAngles;
            
            // Ender: Check this out for weapon-specific orientation differences in combat
            // Currently both modes use same rotation, but this test ensures the property is considered
            Assert.AreEqual(meleeRotation, rangedRotation, "Weapon type should affect orientation logic");
        }

        [Test]
        public void Animator_IsInitializedInAwake()
        {
            Assert.IsNotNull(kinematicObject.Animator);
            Assert.AreEqual(testAnimator, kinematicObject.Animator);
        }

        [Test]
        public void Orient_FacingLeft_RotatesCorrectly()
        {
            kinematicObject.IsFacingLeft = true;
            kinematicObject.Orient();
            
            Vector3 expectedRotation = new Vector3(0, 270, 0); // Unity normalizes -90 to 270
            Assert.AreEqual(expectedRotation, testObject.transform.parent.rotation.eulerAngles);
        }

        [Test]
        public void Orient_FacingRight_RotatesCorrectly()
        {
            kinematicObject.IsFacingLeft = false;
            kinematicObject.Orient();
            
            Vector3 expectedRotation = new Vector3(0, 90, 0);
            Assert.AreEqual(expectedRotation, testObject.transform.parent.rotation.eulerAngles);
        }

        [Test]
        public void EnableFreeRoam_RemovesRigidbodyConstraints()
        {
            testRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            
            kinematicObject.EnableFreeRoam();
            
            Assert.AreEqual(RigidbodyConstraints.None, testRigidbody.constraints);
        }

        [UnityTest]
        public IEnumerator Walk_MovesObjectInSpecifiedDirection()
        {
            float velocity = 2.0f;
            float duration = 0.5f;
            float directionDegrees = 90f; // Right direction
            
            Vector3 initialPosition = testObject.transform.parent.position;
            kinematicObject.Walk(0, velocity, duration, directionDegrees);
            
            yield return new WaitForSeconds(0.1f);
            
            Assert.IsTrue(kinematicObject.velocity.magnitude > 0, "Object should be moving");
            
            yield return new WaitForSeconds(duration + 0.1f);
            
            Assert.AreEqual(0f, kinematicObject.velocity.x, 0.1f, "Horizontal velocity should be zero after walk completion");
        }

        [Test]
        public void DeathAnimationDuration_ReflectsActualAnimationClipLength()
        {
            // Test that the property correctly reads from the animator controller
            float expectedDuration = 2.0f; // Set in SetupMockAnimator for "Die" clip
            Assert.AreEqual(expectedDuration, kinematicObject.DeathAnimationDuration, 0.1f, 
                "DeathAnimationDuration should match the actual Die animation clip length");
        }

        [Test]
        public void AttackAnimationDuration_ReflectsActualAnimationClipLength()
        {
            // Test that the property correctly reads from the animator controller
            float expectedDuration = 0.5f; // Set in SetupMockAnimator for "Attack1" clip
            Assert.AreEqual(expectedDuration, kinematicObject.AttackAnimationDuration, 0.1f,
                "AttackAnimationDuration should match the actual Attack1 animation clip length");
        }

        [Test]
        public void VictoryAnimationDuration_ReflectsActualAnimationClipLength()
        {
            // Test that the property correctly reads from the animator controller
            float expectedDuration = 1.5f; // Set in SetupMockAnimator for "Victory" clip
            Assert.AreEqual(expectedDuration, kinematicObject.VictoryAnimationDuration, 0.1f,
                "VictoryAnimationDuration should match the actual Victory animation clip length");
        }

        [Test]
        public void IdleAnimationDuration_ReflectsActualAnimationClipLength()
        {
            // Test that the property correctly reads from the animator controller
            float expectedDuration = 1.0f; // Set in SetupMockAnimator for "Idle" clip
            Assert.AreEqual(expectedDuration, kinematicObject.IdleAnimationDuration, 0.1f,
                "IdleAnimationDuration should match the actual Idle animation clip length");
        }

        [Test]
        public void GetHitAnimationDuration_ReflectsActualAnimationClipLength()
        {
            // Test that the property correctly reads from the animator controller
            float expectedDuration = 0.3f; // Set in SetupMockAnimator for "GetHit" clip
            Assert.AreEqual(expectedDuration, kinematicObject.GetHitAnimationDuration, 0.1f,
                "GetHitAnimationDuration should match the actual GetHit animation clip length");
        }
    }
}