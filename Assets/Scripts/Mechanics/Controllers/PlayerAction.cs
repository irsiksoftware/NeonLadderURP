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
        private Vector2 playerInput = new Vector2(0, 0);
        private float sprintTimeAccumulator = 0f;
        public bool isClimbing { get; set; }
        private bool isUsingMelee = true;

        #region Jumping
        [SerializeField]
        public ActionStates jumpState = ActionStates.Ready;
        [SerializeField]
        public bool jump;
        public bool IsJumping => jumpState == ActionStates.Preparing || jumpState == ActionStates.Acting || jumpState == ActionStates.InAction || !stopJump;
        [SerializeField]
        public bool stopJump;
        #endregion

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
            player = GetComponent<Player>();
            ConfigureControls(player);
            initialAttackDuration = attackDuration; // Initialize the initial attack duration
        }

        protected override void Update()
        {
            if (player.controlEnabled)
            {
                UpdateJumpState(player.IsGrounded);
                UpdateSprintState(playerInput, ref player.velocity);
                if (IsAttacking ?? false)
                {
                    UpdateAttackState(playerInput, ref player.velocity);
                    //TryAttackEnemy(); // this needs to check for attack state and only be performed once.
                }
                UpdateJumpAnimationParameters(player);
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
            var playerActionMap = player.controls.FindActionMap("Player");
            playerActionMap.Enable();

            var jumpAction = playerActionMap.FindAction("Jump");
            jumpAction.performed += OnJumpPerformed;
            jumpAction.canceled += OnJumpCanceled;

            var sprintAction = playerActionMap.FindAction("Sprint");
            sprintAction.performed += OnSprintPerformed;
            sprintAction.canceled += OnSprintCanceled;

            var moveLeftAction = playerActionMap.FindAction("MoveLeft");
            moveLeftAction.performed += OnMoveLeftPerformed;
            moveLeftAction.canceled += OnMoveCanceled;

            var moveRightAction = playerActionMap.FindAction("MoveRight");
            moveRightAction.performed += OnMoveRightPerformed;
            moveRightAction.canceled += OnMoveCanceled;

            var upAction = playerActionMap.FindAction("Up");
            upAction.performed += OnUpPerformed;

            var meleeAction = playerActionMap.FindAction("MeleeAttack");
            meleeAction.performed += OnMeleeAttackPerformed;
            meleeAction.canceled += OnMeleeAttackCanceled;
            base.ConfigureControls(player);

            var dashAction = playerActionMap.FindAction("Dash");
            dashAction.performed += OnDashPerformed;
            dashAction.canceled += OnDashCanceled;


            var weaponSwapAction = playerActionMap.FindAction("WeaponSwap");
            weaponSwapAction.performed += OnWeaponSwap;

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

        private void OnDashCanceled(InputAction.CallbackContext context)
        {
            //GetComponentInParent<DashAfterImage>().StopDash();
        }

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            //GetComponentInParent<DashAfterImage>().StartDash();
        }

        private void OnMeleeAttackPerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Attack Performed: Current State - " + attackState);
            if (!player.stamina.IsExhausted)
            {
                if (attackState == ActionStates.Ready)
                {
                    Debug.Log("Starting Attack");
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
                Debug.Log("Melee attack cancel requested");
            }
        }

        public void UpdateSprintState(Vector2 move, ref Vector3 velocity)
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

                        velocity.x = player.moveDirection * (Constants.SprintSpeedMultiplier * Constants.DefaultMaxSpeed);
                        sprintDuration -= Time.deltaTime;
                    }
                    break;

                case ActionStates.Acted:
                    sprintState = ActionStates.Ready;
                    stopSprint = false;
                    break;
            }
        }

        public void UpdateAttackState(Vector2 move, ref Vector3 velocity)
        {
            switch (attackState)
            {
                case ActionStates.Preparing:
                    attackDuration = initialAttackDuration; // Reset the attack duration
                    attackState = ActionStates.Acting;
                    Debug.Log("Transitioned to Acting");
                    break;

                case ActionStates.Acting:
                    if (attackDuration > 0)
                    {
                        attackDuration -= Time.deltaTime;
                        //Debug.Log($"Attacking... {attackDuration} seconds remaining.");

                        // Trigger the attack halfway through the attack duration
                        if (attackDuration <= initialAttackDuration / 2 && attackDuration > initialAttackDuration / 2 - Time.deltaTime)
                        {
                            TryAttackEnemy(); // Perform attack
                        }
                    }
                    else
                    {
                        attackState = ActionStates.Ready; // Automatically reset to Ready after completing the cycle
                        Debug.Log("Attack state reset to Ready");
                    }
                    break;

                case ActionStates.Acted:
                    // Optionally, handle post-attack logic here, like returning to a neutral stance
                    break;
            }
        }

        public void UpdateJumpState(bool IsGrounded)
        {
            jump = false;
            switch (jumpState)
            {
                case ActionStates.Preparing:
                    jumpState = ActionStates.Acting;
                    jump = true;
                    stopJump = false;
                    break;
                case ActionStates.Acting:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>();
                        jumpState = ActionStates.InAction;
                    }
                    break;
                case ActionStates.InAction:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>();
                        jumpState = ActionStates.Acted;
                    }
                    break;
                case ActionStates.Acted:
                    jumpState = ActionStates.Ready;
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

        private void OnMoveLeftPerformed(InputAction.CallbackContext context)
        {
            Keypresses.Add(context.action.name);
            player.moveDirection = -1;
            playerInput = new Vector2(-1, 0);
            UpdateRotation(-1);
        }

        private void OnMoveRightPerformed(InputAction.CallbackContext context)
        {
            Keypresses.Add(context.action.name);
            player.moveDirection = 1;
            playerInput = new Vector2(1, 0); // Assuming right movement is along the positive x-axis
            UpdateRotation(1);
        }

        private void OnUpPerformed(InputAction.CallbackContext context)
        {
            InitAction(player.animator, "isClimbing", Constants.ClimbDuration);
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            Keypresses.Remove(context.action.name);
            player.UpdateMoveDirection(0);
            playerInput = Vector2.zero; // No movement input
            UpdateRotation(0);
        }

        //used by signals
        public void Jump()
        {
            OnJumpPerformed(new InputAction.CallbackContext());
        }

        public void OnJumpPerformed(InputAction.CallbackContext context)
        {
            if (player.stamina.IsExhausted) return;
            player.stamina.Decrement(Constants.JumpStaminaCost);
            if (jumpState == ActionStates.Ready)
            {
                jumpState = ActionStates.Preparing;
                player.velocity.y = Constants.JumpTakeOffSpeed * Constants.JumpModifier;
            }

            stopJump = true;
            Schedule<PlayerStopJump>();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (IsJumping)
            {
                player.velocity.y *= Constants.JumpCutOffFactor;
                stopJump = true;
            }
        }

        private void TryAttackEnemy()
        {
            // Adjust the ray origin to be higher up on the character's body
            Vector3 rayOrigin = transform.parent.position + new Vector3(0, 1.0f, 0);  // Adjust 1.0f to suit the character size
            Vector3 rayDirection = transform.parent.forward;
            float attackRange = Constants.AttackRange;

            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
            {
     
                Debug.DrawRay(rayOrigin, rayDirection * attackRange, Color.red);
                if ( hit.collider.CompareTag("Boss") || hit.collider.CompareTag("Major") || hit.collider.CompareTag("Minor"))
                {
                    Health enemyHealth = hit.collider.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Decrement(Constants.AttackDamage);
                        Debug.Log("Attacked " + hit.collider.name + " for " + attackDuration + " damage.");
                    }
                }
            }
        }
    }
}
