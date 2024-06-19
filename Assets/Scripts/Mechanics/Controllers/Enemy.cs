using Assets.Scripts;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
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
        internal Animator animator;
        private int moveDirection;

        [SerializeField]
        private LootTable lootTable; // Allow assignment in the editor
        private LootTable runtimeLootTable;
        public LootTable RuntimeLootTable
        {
            get { return runtimeLootTable; }
            private set { runtimeLootTable = value; }
        }

        public Health health { get; private set; }
        public Stamina stamina { get; private set; }
        [SerializeField]
        public float staminaRegenTimer = 0f;

        [SerializeField]
        protected virtual int attackDamage { get; set; } = 10; // Damage per attack

        [SerializeField]
        private float attackRange = 3.0f; // Default value
        [SerializeField]
        private bool retreatWhenTooClose = false; // Default value

        protected virtual float AttackRange
        {
            get
            {
                if (attackRange > 0)
                {
                    return attackRange;
                }
                else
                {
                    BoxCollider boxCollider = GetComponentInParent<BoxCollider>();
                    return boxCollider != null ? boxCollider.size.x / 2 + 1.0f : 3.0f; // Default value if no BoxCollider found
                }
            }
            set { attackRange = value; }
        }

        protected virtual bool RetreatWhenTooClose
        {
            get { return retreatWhenTooClose; }
            set { retreatWhenTooClose = value; }
        }

        [SerializeField]
        protected virtual float retreatBuffer { get; set; } = 1.0f;
        [SerializeField]
        protected virtual float attackCooldown { get; set; } = 1.0f; // Cooldown time between attacks in seconds
        protected virtual float lastAttackTime { get; set; } = 0; // Timestamp of the last attack
        protected virtual int idleAnimation { get; set; } = 0;
        protected virtual int walkForwardAnimation { get; set; } = 1;
        protected virtual int walkBackwardAnimation { get; set; } = 6;
        protected virtual int attackAnimation { get; set; } = 2;
        protected virtual int hurtAnimation { get; set; } = 3;
        protected virtual int victoryAnimation { get; set; } = 5;
        public int DeathAnimation => deathAnimation;
        protected virtual int deathAnimation { get; set; } = 4;
        public float DeathAnimationDuration => deathAnimationDuration;
        protected virtual float deathAnimationDuration { get; set; } = 3.5f;

        public bool IsFacingLeft { get; set; }

        [SerializeField]
        private MonsterStates currentState = MonsterStates.Idle;

        protected override void Awake()
        {
            base.Awake();
            animator = GetComponentInParent<Animator>();
            health = GetComponentInParent<Health>();
            LoadLootTable();
        }

        private void LoadLootTable()
        {
            if (lootTable != null)
            {
                RuntimeLootTable = lootTable;
            }
            else
            {
                switch (this)
                {
                    case FlyingMinor:
                    case Minor:
                        RuntimeLootTable = Resources.Load<LootTable>(Constants.MinorEnemyLootTablePath);
                        break;
                    case Major:
                        RuntimeLootTable = Resources.Load<LootTable>(Constants.MajorEnemyLootTablePath);
                        break;
                    case Boss:
                        RuntimeLootTable = Resources.Load<LootTable>(Constants.BossEnemyLootTablePath);
                        break;
                    default:
                        Debug.LogError($"LootTable not found for enemy: {this}");
                        break;
                }
            }

            if (RuntimeLootTable == null)
            {
                Debug.LogError($"LootTable not found for enemy: {this}");
            }   
        }

        protected override void ComputeVelocity()
        {
            base.ComputeVelocity();
            if (health.IsAlive)
            {
                if (moveDirection != 0)
                {
                    targetVelocity.x = moveDirection * Constants.DefaultMaxSpeed;
                }

                if (currentState == MonsterStates.Retreating)
                {
                    targetVelocity.x = targetVelocity.x / 2;
                }

                velocity.x = targetVelocity.x;  // Apply horizontal movement
            }
        }

        protected override void Update()
        {
            IsFacingLeft = player.transform.position.x < transform.parent.position.x;
            base.Update();
            if (health.IsAlive)
            {
                Orient();
                if (player.health.IsAlive)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, player.transform.position);
                    switch (currentState)
                    {
                        case MonsterStates.Idle:
                        case MonsterStates.Reassessing:
                            if (distanceToTarget > AttackRange)
                            {
                                currentState = MonsterStates.Approaching;
                            }
                            else if (RetreatWhenTooClose && distanceToTarget < (AttackRange - retreatBuffer))
                            {
                                currentState = MonsterStates.Retreating;
                            }
                            else
                            {
                                currentState = MonsterStates.Attacking;
                            }
                            break;
                        case MonsterStates.Approaching:
                            if (distanceToTarget <= AttackRange)
                            {
                                moveDirection = 0;
                                animator.SetInteger("animation", idleAnimation);
                                currentState = MonsterStates.Reassessing;
                            }
                            else
                            {
                                ChasePlayer();
                            }
                            break;
                        case MonsterStates.Retreating:
                            if (distanceToTarget >= AttackRange - retreatBuffer)
                            {
                                moveDirection = 0;
                                animator.SetInteger("animation", idleAnimation);
                                currentState = MonsterStates.Reassessing;
                            }
                            else
                            {
                                Retreat();
                            }
                            animator.SetInteger("animation", walkBackwardAnimation);
                            break;
                        case MonsterStates.Attacking:
                            if (Time.time > lastAttackTime + attackCooldown)
                            {
                                AttackPlayer();
                                currentState = MonsterStates.Reassessing;
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
            else
            {
                animator.SetInteger("animation", DeathAnimation);
                StartCoroutine(PlayDeathAnimation());
            }
        }

        private void Retreat()
        {
            moveDirection = IsFacingLeft ? 1 : -1;
            animator.SetInteger("animation", walkBackwardAnimation);
        }

        private IEnumerator PlayVictoryAnimation()
        {
            yield return new WaitForSeconds(3);
            animator.SetInteger("animation", idleAnimation);
        }

        private IEnumerator PlayDeathAnimation()
        {
            yield return new WaitForSeconds(DeathAnimationDuration);
            transform.parent.gameObject.SetActive(false);
        }

        private void AttackPlayer()
        {
            lastAttackTime = Time.time;
            player.health.Decrement(attackDamage);
            animator.SetInteger("animation", attackAnimation);
            currentState = MonsterStates.Reassessing;
        }

        private void Orient()
        {
            transform.parent.rotation = Quaternion.Euler(0, !IsFacingLeft ? 90 : -90, 0);
        }

        private void ChasePlayer()
        {
            moveDirection = IsFacingLeft ? -1 : 1;
            animator.SetInteger("animation", walkForwardAnimation);
        }
    }
}
