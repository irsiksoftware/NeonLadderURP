using Assets.Scripts;
using NeonLadder.Events;
using NeonLadder.Items;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using NeonLadder.Models;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Controllers
{
    public class Enemy : KinematicObject
    {
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        protected PlatformerModel model = GetModel<PlatformerModel>();
        public Player target => model.player;

        public DropConfig dropConfig = null;
        public int moveDirection;

        public Health health { get; private set; }
        public Stamina stamina { get; private set; }
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public SpriteRenderer spriteRenderer { get; private set; }
        internal Animator animator;

        [SerializeField]
        private int attackDamage = 10; // Damage per attack
        [SerializeField]
        private float attackRange = 3.0f; // Range within which the monster can attack
        [SerializeField]
        private float attackCooldown = 1.0f; // Cooldown time between attacks in seconds
        private float lastAttackTime = 0; // Timestamp of the last attack


        // Animation states
        public int idleAnimation = 0;
        public int walkForwardAnimation = 1;
        public int attackAnimation = 2;
        public int hurtAnimation = 3;
        public int deathAnimation = 4;

        public float deathAnimationDuration = 3.5f;



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
        }

        protected override void ComputeVelocity()
        {
            if (health.IsAlive)
            {
                base.ComputeVelocity();  // Ensures that base class logic like physics calculations are still executed

                if (moveDirection != 0)
                {
                    animator.SetInteger("animation", walkForwardAnimation);
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
                if (!target.health.IsAlive)
                {
                    currentState = MonsterStates.Idle;
                    animator.SetInteger("animation", idleAnimation);
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
                                animator.SetInteger("animation", idleAnimation);
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

        public void DropItems()
        {
            if (dropConfig != null)
            {
                foreach (var dropItem in dropConfig?.dropItems)
                {
                    if (Random.Range(0f, 100f) <= dropItem.dropProbability)
                    {
                        Instantiate(dropItem.collectiblePrefab, transform.position, Quaternion.identity);
                    }
                }
            }
        }

        private void AttackPlayer()
        {
            lastAttackTime = Time.time;
            target.health.Decrement(attackDamage);
            animator.SetInteger("animation", attackAnimation);
            currentState = MonsterStates.Reassessing;
        }

        private void ChasePlayer()
        {
            var IsFacingLeft = (target.transform.position.x - transform.parent.position.x) < 0 ? true : false;
            transform.parent.rotation = Quaternion.Euler(0, IsFacingLeft ? -90 : 90, 0);
            moveDirection = IsFacingLeft ? -1 : 1;
            animator.SetInteger("animation", walkForwardAnimation);
        }
    }
}
