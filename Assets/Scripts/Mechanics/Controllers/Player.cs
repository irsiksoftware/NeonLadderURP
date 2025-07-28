using Michsky.MUIP;
using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Events;
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
            IsMovingInZDimension = Mathf.Abs(velocity.z) > Constants.Physics.Movement.ZAxisThreshold;

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
            if (staminaRegenTimer >= Constants.Physics.Stamina.RegenInterval) // Check if 1/10th of a second has passed
            {
                // Replace direct modification with event-driven regeneration
                ScheduleStaminaRegeneration(0f); // Immediate event scheduling
                staminaRegenTimer -= Constants.Physics.Stamina.RegenInterval; // Decrease the timer by 0.1f instead of resetting to 0
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

                // Handle jumping through event system
                if (Actions.isJumping)
                {
                    Actions.ScheduleJump(0f); // Immediate jump validation
                }
            }
        }

        private void HandleAnimations()
        {
            if (MiscPose != 0)
            {
                Animator.SetInteger(nameof(PlayerAnimationLayers.misc_animation), MiscPose);
                Animator.SetLayerWeight(Constants.MiscActionLayerIndex, 1);
            }
            else
            {
                Animator.SetLayerWeight(Constants.MiscActionLayerIndex, 0);
            }

            if (Animator.GetInteger(nameof(PlayerAnimationLayers.locomotion_animation)) > 9000 /* dances */ ||
                Animator.GetInteger(nameof(PlayerAnimationLayers.locomotion_animation)) == 5 || /* death */
                Animator.GetInteger(nameof(PlayerAnimationLayers.locomotion_animation)) == 3)
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
                    Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), rollAnimation); // roll
                }
                else
                {
                    Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), jumpAnimation); // jump
                }
            }
            else if (velocity.y < -2)
            {
                Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), fallAnimation); // fall
            }
            else if (Math.Abs(velocity.x) < 0.1 && Math.Abs(velocity.z) < 0.1)
            {
                Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), idleAnimation); // idle
            }
            else if (Math.Abs(velocity.x) > 4 || Math.Abs(velocity.z) > 4)
            {
                Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), runAnimation); // run
            }
            else if (Math.Abs(velocity.x) > 0.1 || Math.Abs(velocity.z) > 0.1)
            {
                Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), walkAnimation); // walk
            }
        }

        public void AddMetaCurrency(int amount)
        {
            ScheduleCurrencyChange(CurrencyType.Meta, amount, 0f);
        }

        public void AddPermanentCurrency(int amount)
        {
            ScheduleCurrencyChange(CurrencyType.Perma, amount, 0f);
        }

        private void UpdateHealthBar()
        {
            if (HealthBar != null && Health != null)
            {
                HealthBar.currentPercent = (Health.current / Health.max) * Constants.UI.PercentageMultiplier;
            }
        }
        private void UpdateStaminaBar()
        {
            if (StaminaBar != null && Stamina != null)
            {
                StaminaBar.currentPercent = (Stamina.current / Stamina.max) * Constants.UI.PercentageMultiplier;
            }
        }

        // Event-driven methods to replace direct stat modifications
        public void ScheduleDamage(float amount, float delay = 0f)
        {
            var damageEvent = Simulation.Schedule<HealthDamageEvent>(delay);
            damageEvent.health = Health;
            damageEvent.damageAmount = amount;
            damageEvent.triggerEffects = true;
        }

        public void ScheduleHealing(float amount, float delay = 0f)
        {
            var healingEvent = Simulation.Schedule<HealthRegenerationEvent>(delay);
            healingEvent.health = Health;
            healingEvent.amount = amount;
        }

        public void ScheduleStaminaDamage(float amount, float delay = 0f)
        {
            // Schedule stamina change event instead of direct modification
            var staminaEvent = Simulation.Schedule<StaminaChangeEvent>(delay);
            staminaEvent.stamina = Stamina;
            staminaEvent.amount = -amount; // Negative for damage, positive for healing
            
            // Schedule visual effects for positive damage amounts only
            if (amount > 0)
            {
                var damageNumberController = GetComponent<DamageNumberController>();
                if (damageNumberController != null)
                {
                    var damageNumberEvent = Simulation.Schedule<DamageNumberEvent>(delay);
                    damageNumberEvent.controller = damageNumberController;
                    damageNumberEvent.amount = amount;
                    damageNumberEvent.numberType = DamageNumberType.StaminaLoss;
                }
            }
        }

        public void ScheduleAudioEvent(AudioEventType audioType, float delay = 0f)
        {
            var audioEvent = Simulation.Schedule<AudioEvent>(delay);
            audioEvent.audioSource = audioSource;
            audioEvent.audioType = audioType;
            
            // Set specific audio clips based on type
            audioEvent.audioClip = audioType switch
            {
                AudioEventType.Jump => jumpAudio,
                AudioEventType.Land => landOnGroundAudio,
                AudioEventType.Damage => ouchAudio,
                AudioEventType.Respawn => respawnAudio,
                _ => null
            };
        }

        public void ScheduleCurrencyChange(CurrencyType currencyType, int amount, float delay = 0f)
        {
            var currencyEvent = Simulation.Schedule<CurrencyChangeEvent>(delay);
            currencyEvent.player = this;
            currencyEvent.currencyType = currencyType;
            currencyEvent.amount = amount;
        }

        // Event-driven movement methods to replace direct modifications
        public void ScheduleVelocityChange(Vector3 newVelocity, float delay = 0f)
        {
            var velocityEvent = Simulation.Schedule<PlayerVelocityValidationEvent>(delay);
            velocityEvent.player = this;
            velocityEvent.requestedVelocity = newVelocity;
        }

        public void ScheduleGroundedStateChange(bool isGrounded, float delay = 0f)
        {
            var groundedEvent = Simulation.Schedule<PlayerGroundedStateChangeEvent>(delay);
            groundedEvent.player = this;
            groundedEvent.isGrounded = isGrounded;
            groundedEvent.previousGroundedState = !isGrounded; // Toggle for testing
        }

        public void SchedulePlayerAudioEvent(PlayerAudioType audioType, float delay = 0f)
        {
            var audioEvent = Simulation.Schedule<PlayerAudioEvent>(delay);
            audioEvent.player = this;
            audioEvent.audioType = audioType;
        }

        // Replace direct stamina regeneration with event-driven approach
        public void ScheduleStaminaRegeneration(float delay = Constants.Physics.Stamina.RegenInterval)
        {
            if (Stamina.current < Stamina.max)
            {
                var regenEvent = Simulation.Schedule<StaminaRegenerationEvent>(delay);
                regenEvent.stamina = Stamina;
                regenEvent.amount = Constants.Physics.Stamina.RegenAmount;
            }
        }
    }
}
