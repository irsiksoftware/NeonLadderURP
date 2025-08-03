using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers.Interfaces;
using NeonLadder.Mechanics.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NeonLadder.Debugging;

namespace NeonLadder.Mechanics.Controllers
{
    public class PlayerAction : BaseAction, IControllable
    {
        private PlayerCameraPositionManager playerPositionManager;
        private Player player;
        public Vector2 playerInput = new Vector2(0, 0);
        private float sprintTimeAccumulator = 0f;
        public bool isClimbing { get; set; }

        #region Jumping
        public bool isJumping { get; set; }
        public float jumpForce = Constants.DefaultJumpTakeOffSpeed;
        private int jumpCount = 0;
        public int JumpCount => jumpCount;
        private int maxJumps = 1;
        public int MaxJumps => maxJumps;
        #endregion

        public InputActionMap playerActionMap;
        public InputActionMap uiActionMap;

        #region Sprinting  
        [SerializeField]
        public float sprintSpeed = Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
        [SerializeField]
        public float sprintDuration = Constants.SprintDuration; // seconds
        [SerializeField]
        public ActionStates sprintState = ActionStates.Ready;
        public bool? IsSprinting => sprintState == ActionStates.Acting;
        [SerializeField]
        public bool stopSprint;
        #endregion

        #region Attacking
        [SerializeField]
        private float attackAnimationDuration = 1.0f; // Default attack duration
        public float AttackAnimationDuration => attackAnimationDuration;

        private int meleeAttackAnimation = 23;
        private int rangedAttackAnimation = 75;

        [SerializeField]
        public ActionStates attackState = ActionStates.Ready;

        [SerializeField]
        public bool stopAttack;
        #endregion

        public List<GameObject> meleeWeaponGroups;
        public List<GameObject> rangedWeaponGroups;



        protected void Start()
        {
            player = GetComponent<Player>();
            var managerObj = GameObject.FindGameObjectWithTag(Tags.Managers.ToString());
            if (managerObj == null)
            {
                Debugger.Log("Managers prefab not found in the scene.");
            }
            else
            {
                playerPositionManager = managerObj.GetComponentInChildren<PlayerCameraPositionManager>();
                ConfigureControls(player);
                ControllerDebugging.PrintDebugControlConfiguration(player);
                if (meleeWeaponGroups == null || meleeWeaponGroups.Count == 0)
                {
                    meleeWeaponGroups = new List<GameObject>(GameObject.FindGameObjectsWithTag("MeleeWeapons"));
                }
                if (rangedWeaponGroups == null || rangedWeaponGroups.Count == 0)
                {
                    rangedWeaponGroups = new List<GameObject>(GameObject.FindGameObjectsWithTag("Firearms"));
                }

                // Initialize weapon states to match player.IsUsingMelee
                InitializeWeaponStates();
            }
        }

        protected override void Update()
        {
            if (playerActionMap.enabled)
            {
                UpdateSprintState(ref player.velocity);

                // NOTE: UpdateAttackState() disabled - new InputBufferEvent system handles attacks
                // UpdateAttackState();
            }

            if (AnimationDebuggingText != null)
            {
                AnimationDebuggingText.gameObject.SetActive(Constants.DisplayAnimationDebugInfo);
                if (Constants.DisplayAnimationDebugInfo)
                {
                    AnimationDebuggingText.text = AnimationDebugging.GetAnimationParameters(player.Animator);
                }
            }

            if (PlayerActionsDebugText != null)
            {
                PlayerActionsDebugText.gameObject.SetActive(Constants.DisplayPlayerActionDebugInfo);
                if (Constants.DisplayPlayerActionDebugInfo)
                {
                    PlayerActionsDebugText.text = PlayerActionDebugging.GetPlayerActionParameters(player, this);
                }
            }

            base.Update();
        }

        protected new void OnDestroy()
        {
            UnsubscribeFromInputActions();
            base.OnDestroy();
        }

        protected new void OnDisable()
        {
            UnsubscribeFromInputActions();
            base.OnDisable();
        }

        protected void OnEnable()
        {
            var managerObj = GameObject.FindGameObjectWithTag(Tags.Managers.ToString());
            if (managerObj == null)
            {
                Debugger.Log("Managers prefab not found in the scene.");
            }
            else
            {
                playerPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerCameraPositionManager>();
                if (playerPositionManager == null)
                {
                    Debugger.Log("PlayerPositionManager not found in the scene.");
                }

                if (playerActionMap == null)
                {
                    ConfigureControls(player);
                }

                playerActionMap.Enable();
            }
        }

        private void UnsubscribeFromInputActions()
        {
            if (playerActionMap != null)
            {
                var sprintAction = playerActionMap.FindAction("Sprint");
                if (sprintAction != null)
                {
                    sprintAction.performed -= OnSprintPerformed;
                    sprintAction.canceled -= OnSprintCanceled;
                }

                var moveAction = playerActionMap.FindAction("Move");
                if (moveAction != null)
                {
                    moveAction.performed -= OnMovePerformed;
                    moveAction.canceled -= OnMoveCanceled;
                }

                var attack = playerActionMap.FindAction("Attack");
                if (attack != null)
                {
                    attack.performed -= OnAttackPerformed;
                    attack.canceled -= OnAttackCanceled;
                }

                var weaponSwapAction = playerActionMap.FindAction("WeaponSwap");
                if (weaponSwapAction != null)
                {
                    weaponSwapAction.performed -= OnWeaponSwap;
                }

                var jumpAction = playerActionMap.FindAction("Jump");
                if (jumpAction != null)
                {
                    jumpAction.performed -= OnJumpPerformed;
                }

                var upAction = playerActionMap.FindAction("Up");
                if (upAction != null)
                {
                    upAction.performed -= OnUpPerformed;
                }
            }
        }

        protected override void ConfigureControls(Player player)
        {
            playerActionMap = player.Controls.FindActionMap("Player");
            playerActionMap.Enable();

            var sprintAction = playerActionMap.FindAction("Sprint");
            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            var moveAction = playerActionMap.FindAction("Move");
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;

            var attack = playerActionMap.FindAction("Attack");
            attack.performed += OnAttackPerformed;
            attack.canceled += OnAttackCanceled;

            var weaponSwapAction = playerActionMap.FindAction("WeaponSwap");
            weaponSwapAction.performed += OnWeaponSwap;

            var jumpAction = playerActionMap.FindAction("Jump");
            jumpAction.performed += OnJumpPerformed;

            var upAction = playerActionMap.FindAction("Up");
            upAction.performed += OnUpPerformed;

            ControllerDebugging.PrintDebugControlConfiguration(player);
        }

        private bool isInZMovementZone = false;

        private void OnUpPerformed(InputAction.CallbackContext context)
        {
            if (isInZMovementZone)
            {
                var sceneName = SceneManager.GetActiveScene().name;
                
                // Find main camera position
                var cameraGO = GameObject.FindGameObjectWithTag(Tags.MainCamera.ToString());
                if (cameraGO == null)
                {
                    Debug.LogError("MainCamera not found!");
                    return;
                }
                var cameraPosition = cameraGO.transform.position;
                
                // Get camera rotation using the new provider
                var cameraRotation = NeonLadder.Cameras.CameraRotationProvider.GetCameraRotation();

                playerPositionManager.SaveState(sceneName,
                                                player.transform.parent.position,
                                                cameraPosition,
                                                cameraRotation);

                player.EnableZMovement();
            }
        }

        public void SetZMovementZone(bool inZone)
        {
            isInZMovementZone = inZone;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            // Schedule input buffer event for jump
            var inputEvent = Simulation.Schedule<InputBufferEvent>(0f);
            inputEvent.player = player;
            inputEvent.inputType = InputType.Jump;
            inputEvent.context = context;
            inputEvent.bufferWindow = 0.15f;
            inputEvent.priority = 1;
        }

        private void OnWeaponSwap(InputAction.CallbackContext context)
        {
            // Schedule input buffer event for weapon swap
            var inputEvent = Simulation.Schedule<InputBufferEvent>(0f);
            inputEvent.player = player;
            inputEvent.inputType = InputType.WeaponSwap;
            inputEvent.context = context;
            inputEvent.bufferWindow = 0.1f;
            inputEvent.priority = 0;
        }

        // REMOVED: SwapWeapons method - now handled by InputBufferEvent system

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            Debugger.Log("🔹 STEP 1: OnAttackPerformed - scheduling InputBufferEvent");
            
            // Schedule input buffer event for attack
            var inputEvent = Simulation.Schedule<InputBufferEvent>(0f);
            inputEvent.player = player;
            inputEvent.inputType = InputType.Attack;
            inputEvent.context = context;
            inputEvent.bufferWindow = 0.2f;
            inputEvent.priority = 2;
            
            Debugger.Log("🔹 InputBufferEvent scheduled successfully");
        }

        public void OnAttackCanceled(InputAction.CallbackContext context)
        {
            if (attackState == ActionStates.Acting)
            {
                stopAttack = true;
            }
        }

        public void UpdateSprintState(ref Vector3 velocity)
        {
            float staminaCostPerTenthSecond = Constants.SprintStaminaCost * 0.1f;
            switch (sprintState)
            {
                case ActionStates.Preparing:
                    sprintDuration = Constants.SprintDuration; // Reset the sprint duration
                    sprintState = ActionStates.Acting;
                    stopSprint = false;
                    sprintTimeAccumulator = 0f; // Reset the time accumulator
                    break;

                case ActionStates.Acting:
                    if (stopSprint || sprintDuration <= 0)
                    {
                        sprintState = ActionStates.Acted;
                    }
                    else
                    {
                        sprintTimeAccumulator += Time.deltaTime;
                        if (sprintTimeAccumulator >= 0.1f)
                        {
                            player.ScheduleStaminaDamage(staminaCostPerTenthSecond, 0f); // Schedule stamina consumption
                            sprintTimeAccumulator -= 0.1f; // Subtract 0.1 seconds from the accumulator
                        }

                        sprintDuration -= Time.deltaTime;
                    }
                    break;

                case ActionStates.Acted:
                    sprintState = ActionStates.Ready;
                    stopSprint = false;
                    break;
            }
        }

        // DEPRECATED: UpdateAttackState - replaced by InputBufferEvent → PlayerAttackEvent system
        // Kept for reference only - not called anywhere
        public void UpdateAttackState()
        {
            switch (attackState)
            {
                case ActionStates.Preparing:
                    attackState = ActionStates.Acting;
                    break;
                case ActionStates.Acting:
                    StartCoroutine(TryAttackEnemy());
                    attackState = ActionStates.Ready;
                    break;
                case ActionStates.Acted:
                    break;
            }
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            // Schedule input buffer event for sprint
            var inputEvent = Simulation.Schedule<InputBufferEvent>(0f);
            inputEvent.player = player;
            inputEvent.inputType = InputType.Sprint;
            inputEvent.context = context;
            inputEvent.bufferWindow = 0.1f;
            inputEvent.priority = 0;
        }

        private void OnSprintCanceled(InputAction.CallbackContext context)
        {
            if (IsSprinting ?? false)
            {
                stopSprint = true;
            }
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            if (player.Health.IsAlive)
            {
                playerInput = context.ReadValue<Vector2>();
                if (playerInput.x != 0)
                {
                    float yRotation = playerInput.x > 0 ? 90 : -90;
                    transform.parent.localRotation = Quaternion.Euler(0, yRotation, 0);
                }
            }
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            playerInput = Vector2.zero; // No movement input
        }

        [SerializeField]
        private float percentageOfAnimationToIgnore = Constants.Animation.IgnorePercentage; // Adjust this value to experiment with different timings

        // DEPRECATED: TryAttackEnemy - replaced by PlayerAttackEvent with weapon collider events
        // Kept for reference only - not called anywhere  
        private IEnumerator TryAttackEnemy()
        {
            var attackComponents = transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                           .Where(c => c.gameObject != transform.parent.gameObject).ToList();
            
            if (attackComponents != null && attackComponents.Count > 0)
            {
                player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1); // Activate action layer

                // Start the attack animation
                player.Animator.SetInteger(nameof(PlayerAnimationLayers.action_animation), (player.IsUsingMelee) ? meleeAttackAnimation : rangedAttackAnimation);

                // Calculate the duration to ignore based on the percentage
                float ignoreDuration = player.AttackAnimationDuration * percentageOfAnimationToIgnore;

                // Wait for the ignore duration
                yield return new WaitForSeconds(ignoreDuration);

                // Change the attack components to the battle layer
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Battle));
                }

                // Wait for the remaining duration of the attack animation
                yield return new WaitForSeconds(player.AttackAnimationDuration - ignoreDuration);

                // Reset the attack components back to the default layer
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Default));
                }

                // Reset the action layer weight and animation state
                player.Animator.SetInteger(nameof(PlayerAnimationLayers.action_animation), 0);
                player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0); // Deactivate action layer
            }
            else
            {
                // CRITICAL FIX: Always reset state even if no attack components
                // Still play attack animation even without valid targets
                player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1);
                player.Animator.SetInteger(nameof(PlayerAnimationLayers.action_animation), 
                    (player.IsUsingMelee) ? meleeAttackAnimation : rangedAttackAnimation);
                
                yield return new WaitForSeconds(player.AttackAnimationDuration);
                
                // Reset animation
                player.Animator.SetInteger(nameof(PlayerAnimationLayers.action_animation), 0);
                player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0);
            }
        }

        private void InitializeWeaponStates()
        {
            if (player.IsUsingMelee)
            {
                // Player starts with melee - activate melee, deactivate ranged
                SetWeaponGroupsActive(meleeWeaponGroups, true);
                SetWeaponGroupsActive(rangedWeaponGroups, false);
            }
            else
            {
                // Player starts with ranged - activate ranged, deactivate melee
                SetWeaponGroupsActive(rangedWeaponGroups, true);
                SetWeaponGroupsActive(meleeWeaponGroups, false);
            }
        }

        private void SetWeaponGroupsActive(List<GameObject> weaponGroups, bool active)
        {
            if (weaponGroups != null)
            {
                foreach (GameObject weaponGroup in weaponGroups)
                {
                    if (weaponGroup != null && weaponGroup.transform.childCount > 0)
                    {
                        var weapon = weaponGroup.transform.GetChild(0).gameObject;
                        weapon.SetActive(active);
                    }
                }
            }
        }

        public void IncrementAvailableMidAirJumps()
        {
            maxJumps++;
        }

        public void IncrementJumpCount()
        {
            jumpCount++;
        }

        public void ResetJumpCount()
        {
            jumpCount = 0;
        }

        public void DisableControls()
        {
            playerInput = Vector2.zero; // Stop movement
            playerActionMap.Disable(); // Disable input
            //OnDisable();
            Debugger.Log("Player controls disabled.");
        }

        public void EnableControls()
        {
            OnEnable();
            Debugger.Log("Player controls enabled.");
        }

        // Event-driven methods to replace direct action execution
        public void ScheduleJump(float delay = 0f)
        {
            if (player != null)
            {
                var jumpEvent = Simulation.Schedule<PlayerJumpValidationEvent>(delay);
                jumpEvent.player = player;
                jumpEvent.requestedJumpForce = jumpForce;
            }
        }

        public void ScheduleSprintValidation(float delay = 0f)
        {
            if (player != null)
            {
                var sprintEvent = Simulation.Schedule<PlayerSprintValidationEvent>(delay);
                sprintEvent.player = player;
                sprintEvent.requestedSpeedMultiplier = Constants.SprintSpeedMultiplier;
            }
        }

        public void ScheduleMovementStateChange(PlayerMovementState newState, float delay = 0f)
        {
            if (player != null)
            {
                var stateEvent = Simulation.Schedule<PlayerMovementStateChangeEvent>(delay);
                stateEvent.player = player;
                stateEvent.newState = newState;
                stateEvent.previousState = GetCurrentMovementState();
            }
        }

        private PlayerMovementState GetCurrentMovementState()
        {
            // Determine current state based on velocity and animation
            if (player.velocity.y > 2) return PlayerMovementState.Jumping;
            if (player.velocity.y < -2) return PlayerMovementState.Falling;
            if (System.Math.Abs(player.velocity.x) > 4 || System.Math.Abs(player.velocity.z) > 4) 
                return PlayerMovementState.Running;
            if (System.Math.Abs(player.velocity.x) > 0.1 || System.Math.Abs(player.velocity.z) > 0.1) 
                return PlayerMovementState.Walking;
            return PlayerMovementState.Idle;
        }
    }
}
