using Assets.Scripts;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static NeonLadder.Core.Simulation;

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

        public int moveDirection { get; set; } = 0;
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

        Vector3 move;

        public Directions? CameFrom { get; set; }

        public void EnablePlayerControl()
        {
            controlEnabled = true;
        }

        public void DisablePlayerControl()
        {
            controlEnabled = false;
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

        public void OnCollisionEnter2D(Collision2D collision)
        {
            //Debug.Log("Collision detected with name: " + collision.collider.name + "\n Collision detected with tag: " + collision.collider.tag);
            //if (collision.collider.name == "Enemy" && playerActions.isDashing)
            //{
            //    //Debug.Log("Detected Enemy in front of player");
            //    CapsuleCollider2D enemyCollider = collision.collider.GetComponent<CapsuleCollider2D>();
            //    enemyCollider.isTrigger = true;
            //    StartCoroutine(ResetColliderAfterSlide(enemyCollider));
            //}           
        }
        IEnumerator ResetColliderAfterSlide(CapsuleCollider2D collider)
        {
            yield return new WaitForSeconds(Constants.DashDuration);
            collider.isTrigger = false;
        }

        protected override void Update()
        {
            RegenerateStamina();
            base.Update();
            Tick();
            //Debug.Log(stamina.current);
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

        public void UpdateMoveDirection(int direction)
        {
            moveDirection = direction;
        }

        public void StopSlide()
        {
            // Reset velocity or modify movement logic to stop sliding
            velocity = Vector2.zero; // Or appropriate logic to stop sliding
        }


        protected override void ComputeVelocity()
        {

            if (controlEnabled)
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

                if (playerActions?.jumpState == ActionStates.Acting)
                {
                    JumpAnimation();
                }

                if (playerActions.attackState == ActionStates.Acting)
                {
                    animator.SetInteger("animation", 23);
                }

                if (playerActions?.jump ?? false && IsGrounded)
                {
                    velocity.y = Constants.JumpTakeOffSpeed; // Initial jump velocity
                }

                else if (playerActions?.stopJump ?? false)
                {
                    if (velocity.y > 0)
                    {
                        velocity.y *= Constants.JumpDeceleration;
                    }
                }


                if (moveDirection != 0)
                {
                    move.x = moveDirection;
                }
                else
                {
                    move.x = 0; // Stop movement when moveDirection is zero
                }


                if (playerActions?.IsSprinting ?? false)
                {
                    targetVelocity.x = moveDirection * (Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier);
                }
                else
                {
                    targetVelocity.x = move.x * Constants.DefaultMaxSpeed;
                }
            }

            Default3DGravity();
            GroundedAnimation();
        }

        public void ApplyGravity(float gravity = Constants.DefaultGravity)
        {
            if (!IsGrounded)
            {
                velocity.y += -gravity * Time.deltaTime;
                velocity.y += -gravity * Time.deltaTime;
            }
        }

        public void Default3DGravity()
        {
            float gravity = Physics.gravity.y * gravityModifier; // Modify this as necessary
            velocity.y += gravity * Time.deltaTime;
        }

        public void GroundedAnimation()
        {
            //animator.SetBool("grounded", IsGrounded);
            //animator.SetFloat("velocityY", Mathf.Abs(velocity.x) / Constants.DefaultMaxSpeed);
        }

        public void JumpAnimation()
        {
            //animator.SetBool("grounded", !IsGrounded);
            //animator.SetFloat("velocityY", velocity.y);
            //animator.SetInteger("animation", 13);
        }



        public void StartWalking(bool right)
        {
            if (right)
            {
                rigidbody.velocity = new Vector2(Constants.CutsceneProtagonistWalkSpeed, rigidbody.velocity.y);
            }
            else
            {
                rigidbody.velocity = new Vector2(-Constants.CutsceneProtagonistWalkSpeed, rigidbody.velocity.y);

            }
        }
        public void DisableAnimator()
        {
            animator.enabled = false;
        }

        public void EnableAnimator()
        {
            animator.enabled = true;
        }
        public void StopWalking()
        {

            rigidbody.velocity = new Vector2(0f, rigidbody.velocity.y);
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