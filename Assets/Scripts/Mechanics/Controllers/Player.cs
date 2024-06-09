using Assets.Scripts;
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
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip jumpAudio;
        public AudioClip landOnGroundAudio;
        public AudioClip landOnEnemyAudio;
        public PlayerAction playerActions { get; private set; }
        public Health health { get; private set; }
        public Stamina stamina { get; private set; }
        public Meta metaCurrency { get; private set; }
        public Perma permaCurrency { get; private set; }
        [SerializeField]
        public bool controlEnabled;
        [SerializeField]
        public InputActionAsset controls;
        [SerializeField]
        public float staminaRegenTimer = 0f;
        public Animator animator { get; private set; }

        public InputActionAsset Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public void Spawn(Transform location)
        {
            transform.parent.position = location.position;
        }

        void Awake()
        {
            playerActions = GetComponentInChildren<PlayerAction>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            health = GetComponent<Health>();
            stamina = GetComponent<Stamina>();
            metaCurrency = GetComponent<Meta>();
            permaCurrency = GetComponent<Perma>();
        }

        protected override void Update()
        {
            RegenerateStamina();
            base.Update();
        }

        private void RegenerateStamina()
        {
            staminaRegenTimer += Time.deltaTime;
            if (staminaRegenTimer >= 0.1f) // Check if 1/10th of a second has passed
            {
                stamina.Increment(0.1f); // Increment stamina by 1/10th of a unit
                staminaRegenTimer -= 0.1f; // Decrease the timer by 0.1f instead of resetting to 0
            }
        }


        protected override void ComputeVelocity()
        {
            if (!controlEnabled)
                return;

            HandleAnimations();
            targetVelocity.x = playerActions.playerInput.x * (Constants.DefaultMaxSpeed) * ((playerActions?.IsSprinting ?? false) ? Constants.SprintSpeedMultiplier : 1);
        }

        private void HandleAnimations()
        {
            if (animator.GetInteger("animation") > 9000) //dances, non locomotion animations
            {
                return;
            }

            if (Math.Abs(velocity.x) < 0.1)
            {
                animator.SetInteger("animation", 1);
            }
            else if (Math.Abs(velocity.x) > 4)
            {
                animator.SetInteger("animation", 10);
            }
            else if (Math.Abs(velocity.x) > 0.1)
            {
                animator.SetInteger("animation", 6);
            }

            if (playerActions.attackState == ActionStates.Acting)
            {
                animator.SetInteger("animation", 23);
            }
        }

        internal void AddMetaCurrency(int amount)
        {
            metaCurrency.Increment(amount);
        }

        internal void AddPermanentCurrency(int amount)
        {
            permaCurrency.Increment(amount);
        }
    }
}