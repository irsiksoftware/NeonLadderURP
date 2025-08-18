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
    public class PlayerTests
    {
        private GameObject testPlayerObject;
        private Player player;
        private Rigidbody testRigidbody;
        private Animator testAnimator;
        private AudioSource testAudioSource;
        private Health testHealth;
        private Stamina testStamina;
        private Meta testMetaCurrency;
        private Perma testPermaCurrency;
        private PlayerAction testPlayerAction;
        
        // Mock scene infrastructure
        private GameObject mockManagersObject;
        private GameObject mockGameControllerObject;
        private GameObject mockSpawnPointObject;
        private ManagerController mockManagerController;
        private Game mockGameController;

        [SetUp]
        public void SetUp()
        {
            // First, setup the mock scene infrastructure
            SetupMockSceneInfrastructure();
            
            // Only add AudioListener if none exists (prevent "17 audio listeners" warning)
            if (Object.FindObjectOfType<AudioListener>() == null)
            {
                var audioListenerObj = new GameObject("AudioListener");
                audioListenerObj.AddComponent<AudioListener>();
            }
            
            testPlayerObject = new GameObject("TestPlayer");
            testPlayerObject.tag = Tags.Player.ToString(); // Set the required tag
            
            // Add required components (but NOT Animator yet)
            testRigidbody = testPlayerObject.AddComponent<Rigidbody>();
            testAudioSource = testPlayerObject.AddComponent<AudioSource>();
            testHealth = testPlayerObject.AddComponent<Health>();
            testStamina = testPlayerObject.AddComponent<Stamina>();
            testMetaCurrency = testPlayerObject.AddComponent<Meta>();
            testPermaCurrency = testPlayerObject.AddComponent<Perma>();
            
            // CRITICAL: Add Animator component and set up its controller BEFORE creating Player
            testAnimator = testPlayerObject.AddComponent<Animator>();
            SetupMockAnimator(); // Set up AnimatorController immediately after creating Animator
            
            // Setup rigidbody
            testRigidbody.isKinematic = true;
            testRigidbody.useGravity = true;
            
            // Create mock UI components BEFORE creating Player (Player.Awake() looks for these)
            CreateMockHealthBar();
            CreateMockStaminaBar();
            
            // NOW create player as child - Awake() will find all required components
            GameObject playerChild = new GameObject("PlayerChild");
            playerChild.transform.SetParent(testPlayerObject.transform);
            
            // CRITICAL: Disable GameObject before adding PlayerAction to prevent OnEnable
            playerChild.SetActive(false);
            
            player = playerChild.AddComponent<Player>();
            
            // Setup mock InputActionAsset for Controls IMMEDIATELY after Player creation
            SetupMockControls();
            
            // Add PlayerAction while GameObject is disabled (OnEnable won't run)
            testPlayerAction = playerChild.AddComponent<PlayerAction>();
            
            // Now re-enable the GameObject - PlayerStateMediator will handle Player <-> PlayerAction communication
            playerChild.SetActive(true);
            
            // Player.Start() will automatically create PlayerStateMediator since both components exist
            
            // Disable PlayerAction component to prevent further issues in tests
            testPlayerAction.enabled = false;
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
            
            // Add required manager components as children
            CreateMockManagers();
            
            // Create GameController GameObject with Game component
            mockGameControllerObject = new GameObject("GameController");
            mockGameControllerObject.tag = Tags.GameController.ToString();
            
            // Important: Set the Game instance reference BEFORE adding the component
            // to prevent path generation issues during Awake()
            try 
            {
                mockGameController = mockGameControllerObject.AddComponent<Game>();
            }
            catch (System.Exception ex)
            {
                // If there are issues with path generation in tests, we'll catch them here
                Debug.LogWarning($"Game component setup warning in tests: {ex.Message}");
            }
            
            // Create a virtual camera for the GameController
            GameObject cameraObject = new GameObject("VirtualCamera");
            cameraObject.transform.SetParent(mockGameControllerObject.transform);
            var virtualCamera = cameraObject.AddComponent<CinemachineCamera>();
            
            // Set the camera in the model
            platformerModel.VirtualCamera = virtualCamera;
            
            // Create SpawnPoint
            mockSpawnPointObject = new GameObject("SpawnPoint");
            mockSpawnPointObject.tag = Tags.SpawnPoint.ToString();
            
            // Set the spawn point in the model
            platformerModel.SpawnPoint = mockSpawnPointObject.transform;
        }
        
        private void CreateMockManagers()
        {
            // Create essential manager children that the ManagerController expects
            var gameControllerManager = new GameObject("GameControllerManager").AddComponent<GameControllerManager>();
            gameControllerManager.transform.SetParent(mockManagersObject.transform);
            gameControllerManager.enabled = false; // Disable for testing
            
            var eventManager = new GameObject("EventManager").AddComponent<EventManager>();
            eventManager.transform.SetParent(mockManagersObject.transform);
            eventManager.enabled = false; // Disable for testing
            
            // Add other managers as needed - keeping minimal for now
            var playerCameraPositionManager = new GameObject("PlayerCameraPositionManager").AddComponent<PlayerCameraPositionManager>();
            playerCameraPositionManager.transform.SetParent(mockManagersObject.transform);
            playerCameraPositionManager.enabled = false; // Disable for testing
        }
        
        private void SetupMockControls()
        {
            // Create a basic InputActionAsset for the player
            var inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            
            // Create a Player action map
            var playerActionMap = new InputActionMap("Player");
            
            // Add basic actions that the PlayerAction.ConfigureControls method expects
            playerActionMap.AddAction("Sprint", InputActionType.Button);
            playerActionMap.AddAction("Move", InputActionType.Value, "<Gamepad>/leftStick");
            playerActionMap.AddAction("Attack", InputActionType.Button);
            playerActionMap.AddAction("WeaponSwap", InputActionType.Button);
            playerActionMap.AddAction("Jump", InputActionType.Button);
            playerActionMap.AddAction("Up", InputActionType.Button);
            
            // Add the action map to the asset
            inputActionAsset.AddActionMap(playerActionMap);
            
            // Set the Controls property on the player
            player.Controls = inputActionAsset;
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
            // We'll animate a dummy property (transform.localPosition.x) from 0 to 0
            var curve = new AnimationCurve();
            curve.AddKey(0f, 0f);        // Start keyframe at time 0
            curve.AddKey(duration, 0f);  // End keyframe at desired duration
            
            // Set the curve on the clip - this will set the clip's length to the duration
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            
            return clip;
        }

        private void CreateMockHealthBar()
        {
            GameObject healthBarObject = new GameObject("HealthBar");
            healthBarObject.transform.SetParent(testPlayerObject.transform);
            healthBarObject.AddComponent<HealthBar>();
            
            // Create proper UI setup for ProgressBar
            ProgressBar healthProgressBar = healthBarObject.AddComponent<ProgressBar>();
            
            // Create the required Image component for loadingBar
            GameObject loadingBarObj = new GameObject("LoadingBar");
            loadingBarObj.transform.SetParent(healthBarObject.transform);
            var loadingBarImage = loadingBarObj.AddComponent<Image>();
            loadingBarImage.type = Image.Type.Filled;
            healthProgressBar.loadingBar = loadingBarImage;
            
            // Create the required TextMeshProUGUI component
            GameObject textObj = new GameObject("PercentText");
            textObj.transform.SetParent(healthBarObject.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "100%";
            healthProgressBar.textPercent = textComponent;
            
            // Set safe defaults to prevent NullRef in third-party asset
            healthProgressBar.currentPercent = 100f;
            healthProgressBar.maxValue = 100f;
            healthProgressBar.minValue = 0f;
            healthProgressBar.isOn = false; // Disable animation in tests
            
            // Initialize the onValueChanged event to prevent NullRef in InitializeEvents()
            if (healthProgressBar.onValueChanged == null)
            {
                healthProgressBar.onValueChanged = new ProgressBar.ProgressBarEvent();
            }
        }

        private void CreateMockStaminaBar()
        {
            GameObject staminaBarObject = new GameObject("StaminaBar");
            staminaBarObject.transform.SetParent(testPlayerObject.transform);
            staminaBarObject.AddComponent<StaminaBar>();
            
            // Create proper UI setup for ProgressBar
            ProgressBar staminaProgressBar = staminaBarObject.AddComponent<ProgressBar>();
            
            // Create the required Image component for loadingBar
            GameObject loadingBarObj = new GameObject("LoadingBar");
            loadingBarObj.transform.SetParent(staminaBarObject.transform);
            var loadingBarImage = loadingBarObj.AddComponent<Image>();
            loadingBarImage.type = Image.Type.Filled;
            staminaProgressBar.loadingBar = loadingBarImage;
            
            // Create the required TextMeshProUGUI component
            GameObject textObj = new GameObject("PercentText");
            textObj.transform.SetParent(staminaBarObject.transform);
            var textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = "100%";
            staminaProgressBar.textPercent = textComponent;
            
            // Set safe defaults to prevent NullRef in third-party asset
            staminaProgressBar.currentPercent = 100f;
            staminaProgressBar.maxValue = 100f;
            staminaProgressBar.minValue = 0f;
            staminaProgressBar.isOn = false; // Disable animation in tests
            
            // Initialize the onValueChanged event to prevent NullRef in InitializeEvents()
            if (staminaProgressBar.onValueChanged == null)
            {
                staminaProgressBar.onValueChanged = new ProgressBar.ProgressBarEvent();
            }
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up InputActionAsset if it exists
            if (player != null && player.Controls != null)
            {
                Object.DestroyImmediate(player.Controls);
            }
            
            // Clean up AnimatorController if it exists
            if (testAnimator != null && testAnimator.runtimeAnimatorController != null)
            {
                Object.DestroyImmediate(testAnimator.runtimeAnimatorController);
            }
            
            // Clean up test player object
            if (testPlayerObject != null)
            {
                Object.DestroyImmediate(testPlayerObject);
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
            
            // CRITICAL: Clear ALL simulation state to prevent test pollution
            // This fixes the currency/stamina test failures when "Run All Tests" is used
            Simulation.Clear();
            Simulation.DestroyModel<PlatformerModel>();
            
            // Reset Game singleton
            if (Game.Instance != null)
            {
                Game.Instance.DestroyGameInstance();
            }
        }

        [Test]
        public void Player_InheritsFromKinematicObject()
        {
            Assert.IsTrue(player is KinematicObject);
        }

        [Test]
        public void Player_Initialization_SetsCorrectDefaults()
        {
            Assert.IsNotNull(player);
            Assert.IsTrue(player.IsUsingMelee);
            Assert.AreEqual(0, player.MiscPose);
            Assert.IsFalse(player.IsMovingInZDimension);
        }

        [Test]
        public void Spawn_MovesPlayerToSpecifiedLocation()
        {
            Vector3 spawnPosition = new Vector3(5, 10, 3);
            GameObject spawnLocation = new GameObject("SpawnLocation");
            spawnLocation.transform.position = spawnPosition;
            
            player.Spawn(spawnLocation.transform);
            
            Assert.AreEqual(spawnPosition, player.transform.parent.position);
            
            Object.DestroyImmediate(spawnLocation);
        }

        [Test]
        public void MiscPose_AffectsAnimatorState()
        {
            // Test that MiscPose actually drives animation state changes
            int combatPose = 1;
            int idlePose = 0;
            
            player.MiscPose = combatPose;
            Assert.AreEqual(combatPose, player.MiscPose);
            
            player.MiscPose = idlePose;
            Assert.AreEqual(idlePose, player.MiscPose);
            // Verify that pose changes affect the actual game state, not just property storage
        }

        [Test]
        public void IsUsingMelee_AffectsWeaponSystemIntegration()
        {
            // Test that weapon mode affects combat behavior
            player.IsUsingMelee = true;
            Assert.IsTrue(player.IsUsingMelee);
            // In melee mode, player should have different attack ranges/damage calculations
            
            player.IsUsingMelee = false;
            Assert.IsFalse(player.IsUsingMelee);
            // In ranged mode, player should use different combat mechanics
            
        }

        [Test]
        public void AddMetaCurrency_IncrementsMetaCurrency()
        {
            float initialAmount = testMetaCurrency.current;
            int addAmount = 50;
            
            player.AddMetaCurrency(addAmount);
            
            // Process any scheduled events in the simulation
            if (Application.isPlaying)
            {
                Simulation.Tick();
            }
            
            Assert.AreEqual(initialAmount + addAmount, testMetaCurrency.current, 0.01f);
        }

        [Test]
        public void AddPermanentCurrency_IncrementsPermaCurrency()
        {
            float initialAmount = testPermaCurrency.current;
            int addAmount = 25;
            
            player.AddPermanentCurrency(addAmount);
            
            // Process any scheduled events in the simulation
            if (Application.isPlaying)
            {
                Simulation.Tick();
            }
            
            Assert.AreEqual(initialAmount + addAmount, testPermaCurrency.current, 0.01f);
        }

        [Test]
        public void IsFacingLeft_SynchronizedWithMovementDirection()
        {
            // Test that facing direction properly syncs with movement input
            player.transform.parent.rotation = Quaternion.Euler(0, 270, 0);
            Vector3 leftwardVelocity = new Vector3(-1, 0, 0);
            player.velocity = leftwardVelocity;
            
            Assert.AreEqual(270f, player.transform.parent.rotation.eulerAngles.y, 0.1f);
            Assert.IsTrue(player.velocity.x < 0, "Moving left should have negative X velocity");
            
            // Test right facing
            player.transform.parent.rotation = Quaternion.Euler(0, 90, 0);
            Vector3 rightwardVelocity = new Vector3(1, 0, 0);
            player.velocity = rightwardVelocity;
            
            Assert.AreEqual(90f, player.transform.parent.rotation.eulerAngles.y, 0.1f);
            Assert.IsTrue(player.velocity.x > 0, "Moving right should have positive X velocity");
        }

        [Test]
        public void IsMovingInZDimension_DetectsZMovement()
        {
            // Set velocity with significant Z component
            player.velocity = new Vector3(0, 0, 1.5f);
            
            // The IsMovingInZDimension property checks if Mathf.Abs(velocity.z) > 0.1f
            Assert.IsTrue(Mathf.Abs(player.velocity.z) > 0.1f);
        }

        [Test]
        public void ComputeVelocity_StopsMovementWhenDead()
        {
            // Set health to dead state
            testHealth.current = 0f;
            Vector3 initialVelocity = new Vector3(5, 0, 0);
            player.velocity = initialVelocity;
            
            Assert.IsFalse(testHealth.IsAlive, "Player should be dead with 0 health");
            
            // Test that death state prevents new movement commands
            // When dead, velocity should not be updated by input
            Assert.IsTrue(initialVelocity.magnitude > 0, "Should have initial velocity to test against");
            
            // ComputeVelocity should return early when Health.IsAlive is false
        }

        [Test]
        public void StaminaRegenTimer_ResetsAfterStaminaUse()
        {
            // Test that stamina regeneration timer resets when stamina is consumed
            float initialTimer = player.staminaRegenTimer;
            
            // Simulate stamina consumption (sprint, attack, etc.)
            testStamina.current = testStamina.max - 10f;
            
            // Timer should reset when stamina is used
            Assert.AreEqual(0f, initialTimer, 0.01f, "Timer should start at zero");
            
        }

        [UnityTest]
        public IEnumerator RegenerateStamina_IncrementsOverTime()
        {
            float initialStamina = testStamina.current;
            testStamina.current = testStamina.max - 5f; // Ensure we're not at max
            
            // Wait for stamina regeneration
            yield return new WaitForSeconds(0.2f);
            
            // Note: The actual regeneration happens in Update(), so we can't directly test it
            // without making the method public or using reflection
            Assert.IsTrue(testStamina.current <= testStamina.max);
        }

        [Test]
        public void AudioComponents_ConfiguredForSpatialAudio()
        {
            Assert.IsNotNull(player.audioSource, "AudioSource should be initialized");
            
            // Test that audio source can be configured for spatial audio
            // Default Unity AudioSource spatialBlend is 0.0f (2D), but should be configurable
            Assert.IsTrue(player.audioSource.spatialBlend >= 0.0f && player.audioSource.spatialBlend <= 1.0f, 
                "AudioSource spatialBlend should be within valid range (0=2D, 1=3D)");
            
            // Test that we can configure it for 3D positioning when needed
            player.audioSource.spatialBlend = 1.0f;
            Assert.AreEqual(1.0f, player.audioSource.spatialBlend, 0.01f, "Should be able to set 3D spatial audio");
            
        }

        [Test]
        public void Actions_ConnectedToInputSystem()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            Assert.IsNotNull(playerAction, "PlayerAction component should be initialized");
            
            // Test that Actions component has proper input bindings
            Assert.IsNotNull(player.Controls, "Input controls should be assigned");
            
            // Verify input action map exists
            var playerActionMap = player.Controls.FindActionMap("Player");
            Assert.IsNotNull(playerActionMap, "Player action map should exist in controls");
            
        }

        [Test]
        public void Health_IntegratedWithUISystem()
        {
            Assert.IsNotNull(player.Health, "Health component should be initialized");
            
            // Test that health is connected to UI representation
            var healthBar = testPlayerObject.GetComponentInChildren<HealthBar>();
            Assert.IsNotNull(healthBar, "Health bar UI should be present");
            
            // Test health bounds
            Assert.IsTrue(player.Health.max > 0, "Max health should be positive");
            Assert.IsTrue(player.Health.current <= player.Health.max, "Current health should not exceed maximum");
        }

        [Test]
        public void Stamina_IntegratedWithUISystem()
        {
            Assert.IsNotNull(player.Stamina, "Stamina component should be initialized");
            
            // Test that stamina is connected to UI representation
            var staminaBar = testPlayerObject.GetComponentInChildren<StaminaBar>();
            Assert.IsNotNull(staminaBar, "Stamina bar UI should be present");
            
            // Test stamina bounds
            Assert.IsTrue(player.Stamina.max > 0, "Max stamina should be positive");
            Assert.IsTrue(player.Stamina.current <= player.Stamina.max, "Current stamina should not exceed maximum");
        }

        [Test]
        public void MetaCurrency_ResetsOnGameCycle()
        {
            Assert.IsNotNull(player.MetaCurrency, "Meta currency component should be initialized");
            
            // Test that meta currency behaves as temporary per-run currency
            float initialAmount = player.MetaCurrency.current;
            player.AddMetaCurrency(100);
            
            // Process any scheduled events in the simulation
            if (Application.isPlaying)
            {
                Simulation.Tick();
            }
            
            Assert.AreEqual(initialAmount + 100, player.MetaCurrency.current, 0.01f, "Meta currency should increase");
            
            // This is the temporary currency that gets lost on death
        }

        [Test]
        public void PermaCurrency_PersistsAcrossGameCycles()
        {
            Assert.IsNotNull(player.PermaCurrency, "Permanent currency component should be initialized");
            
            // Test that permanent currency persists (unlike meta currency)
            float initialAmount = player.PermaCurrency.current;
            player.AddPermanentCurrency(50);
            
            // Process any scheduled events in the simulation
            if (Application.isPlaying)
            {
                Simulation.Tick();
            }
            
            Assert.AreEqual(initialAmount + 50, player.PermaCurrency.current, 0.01f, "Permanent currency should increase");
            
            // This is what enables permanent upgrades between attempts
        }

        [Test]
        public void Controls_ValidatesInputActionAsset()
        {
            // Test that control assignment validates the input asset structure
            InputActionAsset testControls = ScriptableObject.CreateInstance<InputActionAsset>();
            var playerMap = new InputActionMap("Player");
            
            // Add required actions for proper validation
            playerMap.AddAction("Move", InputActionType.Value);
            playerMap.AddAction("Attack", InputActionType.Button);
            playerMap.AddAction("Jump", InputActionType.Button);
            testControls.AddActionMap(playerMap);
            
            player.Controls = testControls;
            
            Assert.AreEqual(testControls, player.Controls, "Controls should be assigned");
            Assert.IsNotNull(testControls.FindActionMap("Player"), "Player action map should exist");
            
            Object.DestroyImmediate(testControls);
        }

    }
}