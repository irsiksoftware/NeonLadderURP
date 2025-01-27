using Michsky.MUIP;
using NeonLadder.Common;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    public class Player : KinematicObject
    {
        [SerializeField]
        private ProgressBar HealthBar;
        [SerializeField]
        private ProgressBar StaminaBar;
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip jumpAudio;
        public AudioClip landOnGroundAudio;
        public AudioClip landOnEnemyAudio;
        [SerializeField]
        private int miscPose = 0;
        public virtual int MiscPose
        {
            get => miscPose;
            set => miscPose = value;
        }
        public bool IsMovingInZDimension { get; private set; }


        public PlayerAction Actions { get; private set; }
        public PlayerUnlock Unlocks { get; private set; }
        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public Meta MetaCurrency { get; private set; }
        public Perma PermaCurrency { get; private set; }
        [SerializeField]
        private InputActionAsset controls;
        public InputActionAsset Controls
        {
            get { return controls; }
            set { controls = value; }
        }
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public override bool IsUsingMelee { get; set; } = true;

        private int walkAnimation = 6;
        private int runAnimation = 10;
        private int idleAnimation = 1;
        private int jumpAnimation = 11;
        private int fallAnimation = 12;
        private int rollAnimation = 13;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public void Spawn(Transform location)
        {
            transform.parent.position = location.position;
        }

        protected override void Awake()
        {
            base.Awake();
            Actions = GetComponent<PlayerAction>();
            Unlocks = GetComponent<PlayerUnlock>();
            audioSource = GetComponentInParent<AudioSource>();
            rigidbody = GetComponentInParent<Rigidbody>();
            Health = GetComponentInParent<Health>();
            Stamina = GetComponentInParent<Stamina>();
            HealthBar = transform.parent.GetComponentInChildren<HealthBar>().gameObject.GetComponent<ProgressBar>();
            StaminaBar = transform.parent.GetComponentInChildren<StaminaBar>().gameObject.GetComponent<ProgressBar>();
            MetaCurrency = GetComponentInParent<Meta>();
            PermaCurrency = GetComponentInParent<Perma>();

            if (controls == null)
            {
                controls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsGrounded)
            {
                Actions.ResetJumpCount();
            }
        }

        protected override void Update()
        {
            //do we need this?
            IsFacingLeft = transform.parent.rotation.eulerAngles.y == 270;
            // Check if moving in the Z-dimension
            IsMovingInZDimension = Mathf.Abs(velocity.z) > 0.1f;

            //TimedLogger.Log($"Player transform position: {transform.position}", 1f);
            base.Update();
            if (Health.IsAlive)
            {
                HandleAnimations();
                RegenerateStamina();
            }

            UpdateHealthBar();
            UpdateStaminaBar();


        }

        private void RegenerateStamina()
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= 0.1f) // Check if 1/10th of a second has passed
            {
                Stamina.Increment(0.1f); // Increment stamina by 1/10th of a unit
                staminaRegenTimer -= 0.1f; // Decrease the timer by 0.1f instead of resetting to 0
            }
        }

        protected override void ComputeVelocity()
        {
            if (!Health.IsAlive)
            {
                targetVelocity = Vector3.zero;
            }
            else if (!rigidbody.constraints.HasFlag(RigidbodyConstraints.FreezePositionZ))
            {
                targetVelocity.z = Constants.DefaultMaxSpeed / 2;
            }
            else
            {
                targetVelocity.x = Actions.playerInput.x * Constants.DefaultMaxSpeed * ((Actions?.IsSprinting ?? false) ? Constants.SprintSpeedMultiplier : 1);

                // Handle jumping
                if (Actions.isJumping && Actions.JumpCount < Actions.MaxJumps)
                {
                    velocity.y = Actions.jumpForce;
                    Actions.IncrementJumpCount();
                    Actions.isJumping = false;
                    if (audioSource != null && jumpAudio != null)
                    {
                        audioSource.PlayOneShot(jumpAudio);
                    }
                }
            }
        }

        public void EnableZMovement()
        {
            transform.parent.rotation = Quaternion.Euler(0, 0, 0);
            Actions.playerActionMap.Disable();
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation |
                                    RigidbodyConstraints.FreezePositionX |
                                    RigidbodyConstraints.FreezePositionY;
        }

        public void DisableZMovement()
        {
            targetVelocity.z = 0;
            Actions.playerActionMap.Enable();
            rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
        }

        private void HandleAnimations()
        {
            if (MiscPose != 0)
            {
                animator.SetInteger(nameof(PlayerAnimationLayers.misc_animation), MiscPose);
                animator.SetLayerWeight(Constants.MiscActionLayerIndex, 1);
            }
            else
            {
                animator.SetLayerWeight(Constants.MiscActionLayerIndex, 0);
            }

            if (animator.GetInteger(nameof(PlayerAnimationLayers.locomotion_animation)) > 9000 /* dances */ ||
                animator.GetInteger(nameof(PlayerAnimationLayers.locomotion_animation)) == 5) /* death */
            {
                return;
            }

            HandleLocomotion();
        }

        private void HandleLocomotion()
        {
            if (velocity.y > 2)
            {
                if (Actions.JumpCount == 2)
                {
                    animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), rollAnimation); // roll
                }
                else
                {
                    animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), jumpAnimation); // jump
                }
            }
            else if (velocity.y < -2)
            {
                animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), fallAnimation); // fall
            }
            else if (Math.Abs(velocity.x) < 0.1 && Math.Abs(velocity.z) < 0.1)
            {
                animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), idleAnimation); // idle
            }
            else if (Math.Abs(velocity.x) > 4 || Math.Abs(velocity.z) > 4)
            {
                animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), runAnimation); // run
            }
            else if (Math.Abs(velocity.x) > 0.1 || Math.Abs(velocity.z) > 0.1)
            {
                animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), walkAnimation); // walk
            }
        }


        internal void AddMetaCurrency(int amount)
        {
            MetaCurrency.Increment(amount);
        }

        internal void AddPermanentCurrency(int amount)
        {
            PermaCurrency.Increment(amount);
        }

        private void UpdateHealthBar()
        {
            if (HealthBar != null && Health != null)
            {
                HealthBar.currentPercent = (Health.current / Health.max) * 100f;
            }
        }
        private void UpdateStaminaBar()
        {
            if (StaminaBar != null && Stamina != null)
            {
                StaminaBar.currentPercent = (Stamina.current / Stamina.max) * 100f;
            }
        }
    }
}
