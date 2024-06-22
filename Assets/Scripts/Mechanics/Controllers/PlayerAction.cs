using Assets.Scripts;
using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NeonLadder.Mechanics.Controllers
{
    public class PlayerAction : BaseAction
    {
        private PlayerAndCameraPositionManager playerPositionManager;
        private Player player;
        public Vector2 playerInput = new Vector2(0, 0);
        private float sprintTimeAccumulator = 0f;
        public bool isClimbing { get; set; }
        public bool isUsingMelee = true;
        public bool isJumping { get; set; }
        public InputActionMap playerActionMap;

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
        public float attackDuration; // seconds
        private float initialAttackDuration; // Store the initial attack duration

        [SerializeField]
        public ActionStates attackState = ActionStates.Ready;

        [SerializeField]
        public bool stopAttack;
        #endregion

        public List<GameObject> meleeWeaponGroups;
        public List<GameObject> rangedWeaponGroups;

        protected void Start()
        {
            player = GetComponentInParent<Player>();
            playerPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerAndCameraPositionManager>();
            ConfigureControls(player);
            initialAttackDuration = attackDuration; // Initialize the initial attack duration
                                                    // Cache the weapon groups here if not assigned via Inspector
            if (meleeWeaponGroups == null || meleeWeaponGroups.Count == 0)
            {
                meleeWeaponGroups = new List<GameObject>(GameObject.FindGameObjectsWithTag("MeleeWeapons"));
            }
            if (rangedWeaponGroups == null || rangedWeaponGroups.Count == 0)
            {
                rangedWeaponGroups = new List<GameObject>(GameObject.FindGameObjectsWithTag("Firearms"));
            }
        }

        protected override void Update()
        {
            if (player.controlEnabled)
            {
                UpdateSprintState(ref player.velocity);

                UpdateAttackState();

            }

            if (AnimationDebuggingText != null)
            {
                AnimationDebuggingText.gameObject.SetActive(Constants.DisplayAnimationDebugInfo);
                if (Constants.DisplayAnimationDebugInfo)
                {
                    AnimationDebuggingText.text = AnimationDebugging.GetAnimationParameters(player.animator);
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

        protected override void ConfigureControls(Player player)
        {
            playerActionMap = player.controls.FindActionMap("Player");
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
                playerPositionManager.SaveState(SceneManager.GetActiveScene().name, player.transform.position, 
                    Game.Instance.model.VirtualCamera.gameObject.transform.position, 
                    Game.Instance.model.VirtualCamera.gameObject.transform.rotation);
                player.EnableZMovement();
            }
        }

        public void SetZMovementZone(bool inZone)
        {
            isInZMovementZone = inZone;
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (player.IsGrounded)
            {
                isJumping = true;
            }
        }

        private void OnWeaponSwap(InputAction.CallbackContext context)
        {
            if (isUsingMelee)
            {
                SwapWeapons(meleeWeaponGroups, rangedWeaponGroups);
            }
            else
            {
                SwapWeapons(rangedWeaponGroups, meleeWeaponGroups);
            }

            isUsingMelee = !isUsingMelee;
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
            if (!player.stamina.IsExhausted)
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
                            player.stamina.Decrement(staminaCostPerTenthSecond); // Decrement stamina
                            sprintTimeAccumulator -= 0.1f; // Subtract 0.1 seconds from the accumulator
                        }

                        //velocity.x = player.move.x * (Constants.SprintSpeedMultiplier * Constants.DefaultMaxSpeed);
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
                    attackDuration = initialAttackDuration;
                    attackState = ActionStates.Acting;
                    break;

                case ActionStates.Acting:
                    if (attackDuration > 0)
                    {
                        attackDuration -= Time.deltaTime;

                        if (attackDuration <= initialAttackDuration / 2 && attackDuration > initialAttackDuration / 2 - Time.deltaTime)
                        {
                            TryAttackEnemy();
                        }
                    }
                    else
                    {
                        attackState = ActionStates.Ready;

                    }
                    break;

                case ActionStates.Acted:

                    break;
            }
        }

        private void OnSprintPerformed(InputAction.CallbackContext context)
        {
            if (!player.stamina.IsExhausted)
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
            playerInput = context.ReadValue<Vector2>();
            if (playerInput.x != 0)
            {
                float yRotation = playerInput.x > 0 ? 90 : -90;
                transform.parent.localRotation = Quaternion.Euler(0, yRotation, 0);
            }
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            playerInput = Vector2.zero; // No movement input
        }

        private void TryAttackEnemy()
        {
            Vector3 rayOrigin = transform.parent.position + new Vector3(0, 1.0f, 0);  // Adjust 1.0f to suit the character size
            Vector3 rayDirection = transform.parent.forward;
            float attackRange = Constants.AttackRange;

            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDirection, attackRange);

            Debug.DrawRay(rayOrigin, rayDirection * attackRange, Color.red);
            List<string> enemiesCaughtInRaycast = new List<string>();
            foreach (RaycastHit hit in hits)
            {
                enemiesCaughtInRaycast.Add(hit.collider.gameObject.name);
                if (hit.collider.CompareTag("Boss") || hit.collider.CompareTag("Major") || hit.collider.CompareTag("Minor"))
                {
                    Health enemyHealth = hit.collider.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Decrement(Constants.AttackDamage);
                    }
                }
            }
        }

    }
}
