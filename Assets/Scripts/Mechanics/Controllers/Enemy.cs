using Assets.Scripts;
using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using NeonLadder.Models;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public abstract class Enemy : KinematicObject
    {
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip jumpAudio;
        public AudioClip landAudio;

        private Player target;

        [SerializeField]
        private LootTable lootTable; // Allow assignment in the editor
        private LootTable runtimeLootTable;
        public LootTable RuntimeLootTable
        {
            get { return runtimeLootTable; }
            private set { runtimeLootTable = value; }
        }

        private int moveDirection;

        public Health health { get; private set; }
        public Stamina stamina { get; private set; }
        [SerializeField]
        public float staminaRegenTimer = 0f;

        public SpriteRenderer spriteRenderer { get; private set; }
        internal Animator animator;

        [SerializeField]
        protected virtual int attackDamage { get; set; } = 10; // Damage per attack
        [SerializeField]
        protected virtual float attackRange { get; set; } = 3.0f; // Range within which the monster can attack
        [SerializeField]
        protected virtual float attackCooldown { get; set; } = 1.0f; // Cooldown time between attacks in seconds
        protected virtual float lastAttackTime { get; set; } = 0; // Timestamp of the last attack

        // Animation states
        public int IdleAnimation => idleAnimation;
        protected virtual int idleAnimation { get; set; } = 0;

        public int WalkForwardAnimation => walkForwardAnimation;
        protected virtual int walkForwardAnimation { get; set; } = 1;

        public int AttackAnimation => attackAnimation;
        protected virtual int attackAnimation { get; set; } = 2;

        public int HurtAnimation => hurtAnimation;
        protected virtual int hurtAnimation { get; set; } = 3;

        public int DeathAnimation => deathAnimation;
        protected virtual int deathAnimation { get; set; } = 4;

        public int VictoryAnimation => victoryAnimation;
        protected virtual int victoryAnimation { get; set; } = 5;

        public float DeathAnimationDuration => deathAnimationDuration;
        protected virtual float deathAnimationDuration { get; set; } = 3.5f;

        public bool IsFacingLeft => target.transform.position.x < transform.parent.position.x;

        [SerializeField]
        private MonsterStates currentState = MonsterStates.Idle;

        private bool enableLogging = true; // Toggle this to enable/disable logging
        private float logInterval = 1.0f; // Log every 1 second
        private float lastLogTime = 0.0f; // Track time since last log

        void OnCollisionEnter(Collision collision)
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                var ev = Simulation.Schedule<PlayerEnemyCollision>();
                ev.enemy = this;
                if (enableLogging) Debug.Log("Collision with player detected.");
            }
        }

        protected virtual void Awake()
        {
            Debug.Log($"{gameObject.name} {nameof(Enemy)} is {nameof(Awake)}");
            animator = GetComponentInParent<Animator>();
            health = GetComponentInParent<Health>();
            target = Simulation.GetModel<PlatformerModel>().Player;
            LoadLootTable();
        }

        private void LoadLootTable()
        {
            // Check if lootTable is assigned in the editor
            if (lootTable != null)
            {
                RuntimeLootTable = lootTable;
            }
            else
            {
                // Determine the appropriate loot table path based on the type of enemy
                string lootTablePath = string.Empty;

                switch (this)
                {
                    case Minor:
                        lootTablePath = Constants.MinorEnemyLootTablePath;
                        break;
                    case Major:
                        lootTablePath = Constants.MajorEnemyLootTablePath;
                        break;
                    case Boss:
                        lootTablePath = Constants.BossEnemyLootTablePath;
                        break;
                    default:
                        Debug.LogError($"LootTable not found for enemy: {this}");
                        break;
                }

                if (lootTablePath != string.Empty)
                {
                    RuntimeLootTable = Resources.Load<LootTable>(lootTablePath);
                    if (RuntimeLootTable == null)
                    {
                        Debug.LogError($"LootTable not found at path: {lootTablePath}");
                    }
                }
            }
        }


        protected override void ComputeVelocity()
        {
            base.ComputeVelocity();
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

            if (target == null)
            {
                LogMessage("Target is null");
            }

            if (target?.health == null)
            {
                LogMessage("Target health is null");
            }
            else
            {
                LogMessage(target?.health?.current.ToString());
            }

            if (health.IsAlive)
            {
                if (target.health.IsAlive)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

                    switch (currentState)
                    {
                        case MonsterStates.Idle:
                        case MonsterStates.Reassessing:
                            if (distanceToTarget > attackRange)
                            {
                                currentState = MonsterStates.Approaching;
                                LogMessage("Switching to Approaching state.");
                            }
                            else
                            {
                                currentState = MonsterStates.Attacking;
                                LogMessage("Switching to Attacking state.");
                            }
                            break;
                        case MonsterStates.Approaching:
                            if (distanceToTarget <= attackRange)
                            {
                                // Stop and reassess before attacking
                                moveDirection = 0;
                                animator.SetInteger("animation", idleAnimation);
                                currentState = MonsterStates.Reassessing;
                                LogMessage("Reassessing before attack.");
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
                                LogMessage("Attacked player. Reassessing.");
                            }

                            break;
                    }
                }
                else
                {
                    moveDirection = 0;
                    animator.SetInteger("animation", victoryAnimation);
                    StartCoroutine(PlayVictoryAnimation());
                }
            }
        }

        private IEnumerator PlayVictoryAnimation()
        {
            yield return new WaitForSeconds(3);
            animator.SetInteger("animation", idleAnimation);
        }

        private void AttackPlayer()
        {
            LogMessage("AttackPlayer called.");
            lastAttackTime = Time.time;
            target.health.Decrement(attackDamage);
            animator.SetInteger("animation", attackAnimation);
            currentState = MonsterStates.Reassessing;
        }

        private void ChasePlayer()
        {
            LogMessage("ChasePlayer called.");
            transform.parent.rotation = Quaternion.Euler(0, IsFacingLeft ? -90 : 90, 0);
            moveDirection = IsFacingLeft ? -1 : 1;
            animator.SetInteger("animation", walkForwardAnimation);
        }

        private void LogMessage(string message)
        {
            if (enableLogging && Time.time > lastLogTime + logInterval)
            {
                Debug.Log(message);
                lastLogTime = Time.time;
            }
        }
    }
}
