using NeonLadder.Common;
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
        private bool ForceRightCameraPivot = true;
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
        public PlayerAction Actions { get; private set; }
        public PlayerUnlock Unlocks { get; private set; }
        public Health Health { get; private set; }
        public Stamina Stamina { get; private set; }
        public Meta MetaCurrency { get; private set; }
        public Perma PermaCurrency { get; private set; }
        [SerializeField]
        public bool controlEnabled;
        [SerializeField]
        public InputActionAsset controls;
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public float DeathAnimationDuration => 3.333f;

        public Animator animator { get; private set; }

        // Variables to store default CVC properties
        private Vector3 defaultTrackedObjectOffset;
        private Vector3 defaultRotation;

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

        protected override void Awake()
        {
            base.Awake();
            Actions = GetComponentInChildren<PlayerAction>();
            Unlocks = GetComponentInChildren<PlayerUnlock>();
            audioSource = GetComponent<AudioSource>();
            animator = GetComponent<Animator>();
            rigidbody = GetComponent<Rigidbody>();
            Health = GetComponent<Health>();
            Stamina = GetComponent<Stamina>();
            HealthBar = GetComponentInChildren<HealthBar>().gameObject.GetComponent<ProgressBar>();
            StaminaBar = GetComponentInChildren<StaminaBar>().gameObject.GetComponent<ProgressBar>();
            MetaCurrency = GetComponent<Meta>();
            PermaCurrency = GetComponent<Perma>();
            CinemachineVirtualCamera cvc = model.VirtualCamera;
            if (cvc != null)
            {
                CinemachineFramingTransposer transposer = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (transposer != null)
                {
                    defaultTrackedObjectOffset = transposer.m_TrackedObjectOffset;
                    defaultRotation = cvc.transform.eulerAngles;
                }
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
            if (Health.IsAlive)
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

                if (ForceRightCameraPivot)
                {
                    pivotDirection = -1;
                }

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

        public void RevertCameraProperties()
        {
            CinemachineVirtualCamera cvc = model.VirtualCamera;
            if (cvc != null)
            {
                CinemachineFramingTransposer transposer = cvc.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (transposer != null)
                {
                    transposer.m_TrackedObjectOffset = defaultTrackedObjectOffset;
                    cvc.transform.eulerAngles = defaultRotation;
                }
            }
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
            rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;
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
            if (Actions.attackState == ActionStates.Acting)
            {
                if (Actions.isUsingMelee)
                {
                    animator.SetInteger("action_animation", 23); // sword attack
                    animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1); // Activate action layer
                }
                else
                {
                    animator.SetInteger("action_animation", 75); // shoot guns
                    animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1); // Activate action layer
                }
            }
            else
            {
                animator.SetInteger("action_animation", 0); // no action
                animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0); // Deactivate action layer
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
