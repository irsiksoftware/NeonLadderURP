using NeonLadder.Common;
using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers.Interfaces;
using NeonLadder.Mechanics.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
        private float attackAnimationDuration;
        public float AttackAnimationDuration;

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
                Debug.Log("Managers prefab not found in the scene.");
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
            }
        }

        protected override void Update()
        {
            if (playerActionMap.enabled)
            {
                UpdateSprintState(ref player.velocity);

                UpdateAttackState();
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
                Debug.Log("Managers prefab not found in the scene.");
            }
            else
            {
                playerPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerCameraPositionManager>();
                if (playerPositionManager == null)
                {
                    Debug.Log("PlayerPositionManager not found in the scene.");
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
                var cameraPosition = GameObject.FindGameObjectWithTag(Tags.MainCamera.ToString()).transform.position;
                var cvcRotation = Game.Instance.model.VirtualCamera.gameObject.transform.rotation;

                //playerPositionManager.SaveState(sceneName,
                //                                player.transform.parent.position,
                //                                cameraPosition,
                //                                cvcRotation);

                player.EnableZMovement();
            }
        }

        public void SetZMovementZone(bool inZone)
        {
            isInZMovementZone = inZone;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (player.IsGrounded || jumpCount < maxJumps)
            {
                isJumping = true;
            }
        }

        private void OnWeaponSwap(InputAction.CallbackContext context)
        {
            if (player.IsUsingMelee)
            {
                SwapWeapons(meleeWeaponGroups, rangedWeaponGroups);
            }
            else
            {
                SwapWeapons(rangedWeaponGroups, meleeWeaponGroups);
            }

            player.IsUsingMelee = !player.IsUsingMelee;
        }

        private void SwapWeapons(List<GameObject> currentWeapons, List<GameObject> newWeapons)
        {
            foreach (GameObject weaponGroup in currentWeapons)
            {
                var weapon = weaponGroup.transform.GetChild(0).gameObject;
                weapon.SetActive(false);
            }

            foreach (GameObject weaponGroup in newWeapons)
            {
                var weapon = weaponGroup.transform.GetChild(0).gameObject;
                weapon.SetActive(true);
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            if (!player.Stamina.IsExhausted)
            {
                if (attackState == ActionStates.Ready)
                {
                    attackState = ActionStates.Preparing;
                    stopAttack = false;
                }
            }
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
                            player.Stamina.Decrement(staminaCostPerTenthSecond); // Decrement stamina
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
            if (!player.Stamina.IsExhausted)
            {
                if (sprintState == ActionStates.Ready)
                {
                    sprintState = ActionStates.Preparing;
                    stopSprint = false;
                }
            }
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
        private float percentageOfAnimationToIgnore = Constants.AnimationIgnorePercentage; // Adjust this value to experiment with different timings

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
            Debug.Log("Player controls disabled.");
        }

        public void EnableControls()
        {
            OnEnable();
            Debug.Log("Player controls enabled.");
        }
    }
}
