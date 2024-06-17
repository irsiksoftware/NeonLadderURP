using Assets.Scripts;
using Cinemachine;
using Michsky.MUIP;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    public class Player : KinematicObject
    {
        [SerializeField]
        private ProgressBar healthBar;
        [SerializeField]
        private ProgressBar staminaBar;
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
        private float jumpForce = 7f;
        public Animator animator { get; private set; }

        public int locomotionLayerIndex = 0; // Index for the locomotion layer
        public int actionLayerIndex = 1; // Index for the action layer

        public InputActionAsset Controls
        {
            get { return controls; }
            set { controls = value; }
        }

        private bool isCameraPivoted = false;
        private Coroutine cameraPivotCoroutine;

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
            if (health.IsAlive)
            {
                HandleAnimations();
                RegenerateStamina();
            }

            if (targetVelocity.z > 0.01)
            {
                if (!isCameraPivoted)
                {
                    isCameraPivoted = true;
                    cameraPivotCoroutine = StartCoroutine(PivotCameraCoroutine(Constants.ZMovementCameraPivotDurationInSeconds));
                }
            }
            else if (isCameraPivoted && cameraPivotCoroutine != null)
            {
                StopCoroutine(cameraPivotCoroutine);
                isCameraPivoted = false;
            }

            UpdateHealthBar();
            UpdateStaminaBar();

            base.Update();
        }

        private IEnumerator PivotCameraCoroutine(float duration)
        {
            CinemachineVirtualCamera cvc = model.VirtualCamera;
            CinemachineFramingTransposer transposer = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();

            if (transposer != null)
            {
                int direction = UnityEngine.Random.Range(0, 2); // Use 0 and 2 for exclusive upper bound
                float pivotDirection = (direction == 0) ? -1 : 1;

                Vector3 initialOffset = transposer.m_TrackedObjectOffset;
                Vector3 targetOffset = initialOffset + new Vector3(pivotDirection * 2, 0, 0); // Adjust 2 to desired pivot distance

                float initialYRotation = cvc.transform.eulerAngles.y;

                float elapsedTime = 0f;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / duration;

                    transposer.m_TrackedObjectOffset = Vector3.Lerp(initialOffset, targetOffset, t);

                    float newYRotation = Mathf.Lerp(initialYRotation, initialYRotation + (pivotDirection * 45), t); // Adjust 45 to desired rotation angle
                    cvc.transform.eulerAngles = new Vector3(cvc.transform.eulerAngles.x, newYRotation, cvc.transform.eulerAngles.z);

                    yield return null;
                }

                transposer.m_TrackedObjectOffset = targetOffset;
            }
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
            if (!health.IsAlive)
            {
                targetVelocity = Vector3.zero;
            }
            else if (!rigidbody.constraints.HasFlag(RigidbodyConstraints.FreezePositionZ))
            {
                targetVelocity.z = Constants.DefaultMaxSpeed / 2;
            }
            else
            {
                targetVelocity.x = playerActions.playerInput.x * Constants.DefaultMaxSpeed * ((playerActions?.IsSprinting ?? false) ? Constants.SprintSpeedMultiplier : 1);

                // Handle jumping
                if (playerActions.isJumping && IsGrounded)
                {
                    velocity.y = jumpForce;
                    playerActions.isJumping = false;
                    if (audioSource != null && jumpAudio != null)
                    {
                        audioSource.PlayOneShot(jumpAudio);
                    }
                }
            }
        }

        public void EnableZMovement()
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            controlEnabled = false;

            rigidbody.constraints = RigidbodyConstraints.FreezeRotation |
                                    RigidbodyConstraints.FreezePositionX |
                                    RigidbodyConstraints.FreezePositionY;
        }

        public void DisableZMovement()
        {
            targetVelocity.z = 0;
            controlEnabled = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation |
                                    RigidbodyConstraints.FreezePositionX |
                                    RigidbodyConstraints.FreezePositionY |
                                    RigidbodyConstraints.FreezePositionZ;
        }

        private void HandleAnimations()
        {
            if (animator.GetInteger("locomotion_animation") > 9000 || animator.GetInteger("locomotion_animation") == 5) // dances, non-locomotion animations
            {
                return;
            }

            HandleLocomotion();
            HandleAction();
        }

        private void HandleLocomotion()
        {
            if (velocity.y > 0.01)
            {
                animator.SetInteger("locomotion_animation", 11); // jump
            }
            else if (Math.Abs(velocity.x) < 0.1 && Math.Abs(velocity.z) < 0.1)
            {
                animator.SetInteger("locomotion_animation", 1); // idle
            }
            else if (Math.Abs(velocity.x) > 4 || Math.Abs(velocity.z) > 4)
            {
                animator.SetInteger("locomotion_animation", 10); // run
            }
            else if (Math.Abs(velocity.x) > 0.1 || Math.Abs(velocity.z) > 0.1)
            {
                animator.SetInteger("locomotion_animation", 6); // walk
            }
        }

        private void HandleAction()
        {
            if (playerActions.attackState == ActionStates.Acting)
            {
                if (playerActions.isUsingMelee)
                {
                    animator.SetInteger("action_animation", 23); // sword attack
                    animator.SetLayerWeight(actionLayerIndex, 1); // Activate action layer
                }
                else
                {
                    animator.SetInteger("action_animation", 75); // shoot guns
                    animator.SetLayerWeight(actionLayerIndex, 1); // Activate action layer
                }
            }
            else
            {
                animator.SetInteger("action_animation", 0); // no action
                animator.SetLayerWeight(actionLayerIndex, 0); // Deactivate action layer
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

        private void UpdateHealthBar()
        {
            if (healthBar != null && health != null)
            {
                healthBar.currentPercent = (health.current / health.max) * 100f;
            }
        }

        private void UpdateStaminaBar()
        {
            if (staminaBar != null && stamina != null)
            {
                staminaBar.currentPercent = (stamina.current / stamina.max) * 100f;
            }
        }
    }
}
