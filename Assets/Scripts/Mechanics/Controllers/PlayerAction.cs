using Assets.Scripts;
using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using UnityEngine;
using UnityEngine.InputSystem;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Controllers
{
    public class PlayerAction : BaseAction
    {
        private Player player;
        public Vector2 playerInput = new Vector2(0, 0);
        private float sprintTimeAccumulator = 0f;
        public bool isClimbing { get; set; }
        private bool isUsingMelee = true;
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
        public bool? IsAttacking => attackState == ActionStates.Preparing || attackState == ActionStates.Acting || attackState == ActionStates.Acted;

        [SerializeField]
        public bool stopAttack;
        #endregion

        protected void Start()
        {
            player = GetComponentInParent<Player>();
            ConfigureControls(player);
            initialAttackDuration = attackDuration; // Initialize the initial attack duration
        }

        protected override void Update()
        {
            if (player.controlEnabled)
            {
                UpdateSprintState(ref player.velocity);
                if (IsAttacking ?? false)
                {
                    UpdateAttackState();
                }
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

            var meleeAction = playerActionMap.FindAction("MeleeAttack");
            meleeAction.performed += OnMeleeAttackPerformed;
            meleeAction.canceled += OnMeleeAttackCanceled;

            var weaponSwapAction = playerActionMap.FindAction("WeaponSwap");
            weaponSwapAction.performed += OnWeaponSwap;

            ControllerDebugging.PrintDebugControlConfiguration(player);
        }

        private void OnWeaponSwap(InputAction.CallbackContext context)
        {
            GameObject[] meleeWeaponGroups = GameObject.FindGameObjectsWithTag("MeleeWeapons"); //there are two groups, one for each hand.
            GameObject[] rangedWeaponGroups = GameObject.FindGameObjectsWithTag("Firearms"); //there are two groups, one for each hand.

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

        private void SwapWeapons(GameObject[] currentWeapons, GameObject[] newWeapons)
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

        private void OnMeleeAttackPerformed(InputAction.CallbackContext context)
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

        public void OnMeleeAttackCanceled(InputAction.CallbackContext context)
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
            UpdateRotation(playerInput.x);
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

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
            {
                Debug.DrawRay(rayOrigin, rayDirection * attackRange, Color.red);
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
