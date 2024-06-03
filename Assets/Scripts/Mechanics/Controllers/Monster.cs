using Assets.Scripts;
using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Controllers
{
    public class Monster : KinematicObject
    {
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        public int moveDirection;

        public Health health { get; private set; }
        public Stamina stamina { get; private set; }
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public SpriteRenderer spriteRenderer { get; private set; }
        internal Animator animator;
        public Health targetHealth;

        private bool shouldChasePlayer = true;

        [SerializeField]
        private int attackDamage = 10; // Damage per attack
        [SerializeField]
        private float attackRange = 3.0f; // Range within which the monster can attack
        [SerializeField]
        private float attackCooldown = 1.0f; // Cooldown time between attacks in seconds
        private float lastAttackTime = 0; // Timestamp of the last attack


        // Animation states
        private int animationIdle = 0;
        private int animationWalk = 1;
        private int animationAttack = 2;
        private int animationHurt = 3;
        private int animationDeath = 4;

        public GameObject target;  // The main protagonist "player"

        public bool IsFacingLeft { get; private set; }

        [SerializeField]
        private MonsterStates currentState = MonsterStates.Idle;


        void OnCollisionEnter(Collision collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                var ev = Schedule<PlayerEnemyCollision>();
                ev.enemy = this;
            }
        }

        protected void Awake()
        {
            animator = GetComponentInParent<Animator>();
            health = GetComponentInParent<Health>();

            if (target == null)
            {
                Debug.Log("Target not set, setting to player");
                target = GameObject.Find(Constants.ProtagonistModel);
                targetHealth = target.GetComponent<Health>();
            }
        }

        protected override void ComputeVelocity()
        {
            if (health.IsAlive)
            {
                base.ComputeVelocity();  // Ensures that base class logic like physics calculations are still executed

                if (moveDirection != 0)
                {
                    animator.SetInteger("animation", animationWalk);
                    targetVelocity.x = moveDirection * Constants.DefaultMaxSpeed;
                }

                velocity.x = targetVelocity.x;  // Apply horizontal movement
            }
        }

        protected override void Update()
        {
            base.Update();  // Ensures that base class Update is called

            if (health.IsAlive)
            {
                if (!targetHealth.IsAlive)
                {
                    currentState = MonsterStates.Idle;
                    animator.SetInteger("animation", animationIdle);
                    return; // Early return to stop processing other states
                }

                else
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                    switch (currentState)
                    {
                        case MonsterStates.Idle:
                        case MonsterStates.Reassessing:
                            if (distanceToTarget > attackRange)
                            {
                                currentState = MonsterStates.Approaching;
                            }
                            else
                            {
                                currentState = MonsterStates.Attacking;
                            }
                            break;
                        case MonsterStates.Approaching:
                            if (distanceToTarget <= attackRange)
                            {
                                // Stop and reassess before attacking
                                moveDirection = 0;
                                animator.SetInteger("animation", animationIdle);
                                currentState = MonsterStates.Reassessing;
                            }
                            else
                            {
                                ChasePlayer();
                            }
                            break;
                        case MonsterStates.Attacking:
                            if (Time.time > lastAttackTime + attackCooldown)
                            {
                                AttackPlayer();
                                // After attacking, reassess the situation
                                currentState = MonsterStates.Reassessing;
                            }
                            break;
                    }
                }
            }
        }

        private void AttackPlayer()
        {
            lastAttackTime = Time.time; // Update the last attack time
            targetHealth.Decrement(attackDamage); // Apply damage
            animator.SetInteger("animation", animationAttack); // Trigger attack animation
            currentState = MonsterStates.Reassessing; // Move to reassessing after attack
        }

        private void ChasePlayer()
        {
            var IsFacingLeft = (target.transform.position.x - transform.parent.position.x) < 0 ? true : false;
            transform.parent.rotation = Quaternion.Euler(0, IsFacingLeft ? -90 : 90, 0);
            moveDirection = IsFacingLeft ? -1 : 1;
            animator.SetInteger("animation", animationWalk);
        }
    }
}
