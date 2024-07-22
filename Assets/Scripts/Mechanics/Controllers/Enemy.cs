using NeonLadder.Common;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using NeonLadder.Utilities;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public abstract class Enemy : KinematicObject
    {
        public bool ShouldEngagePlayer = true; // Publicly accessible flag, default to always engage.
        public AudioSource audioSource;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip jumpAudio;
        public AudioClip landAudio;
        private int moveDirection;

        public bool IsFacingLeft { get; set; }
        public bool IsFacingRight => !IsFacingLeft;

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

        public float deathBuffer = 1f;

        [SerializeField]
        private float attackRange = 0f; // Default value

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
        private MonsterStates currentState = MonsterStates.Idle;
        [SerializeField]
        protected virtual float retreatBuffer { get; set; } = 1.0f;
        [SerializeField]
        private float _attackCooldown = 3.0f; // Default value if not set in the editor or overridden

        public virtual float attackCooldown
        {
            get => _attackCooldown;
            set => _attackCooldown = value;
        }
        protected virtual float lastAttackTime { get; set; } = -10f;
        protected virtual int idleAnimation { get; set; } = 0;
        protected virtual int walkForwardAnimation { get; set; } = 1;
        protected virtual int walkBackwardAnimation { get; set; } = 6;
        protected virtual int attackAnimation { get; set; } = 2;
        protected virtual int hurtAnimation { get; set; } = 3;
        protected virtual int victoryAnimation { get; set; } = 5;
        protected virtual int deathAnimation { get; set; } = 4;

        private bool isIdlePlaying = false;

        protected override void Awake()
        {
            if (attackCooldown <= attackAnimationDuration)
            {
                Debug.LogWarning($"Attack cooldown is less than or equal to attack animation duration for enemy: {transform.parent.name}");
            }
            base.Awake();
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
            IsFacingLeft = player.transform.parent.position.x < transform.parent.position.x;
            if (health.IsAlive)
            {
                Orient();
            }

            if (ShouldEngagePlayer && !isIdlePlaying)
            {
                
                base.Update();
                if (health.IsAlive)
                {
                    if (ShouldEngagePlayer && player.Health.IsAlive)
                    {
                        float distanceToTarget = Vector3.Distance(transform.parent.position, player.transform.parent.position);
                        switch (currentState)
                        {
                            case MonsterStates.Idle:
                                StartCoroutine(PlayIdleAndReassess(distanceToTarget));
                                break;
                            case MonsterStates.Reassessing:
                                HandleReassessingState(distanceToTarget);
                                break;
                            case MonsterStates.Approaching:
                                HandleApproachingState(distanceToTarget);
                                break;
                            case MonsterStates.Retreating:
                                HandleRetreatingState(distanceToTarget);
                                break;
                            case MonsterStates.Attacking:
                                StartCoroutine(AttackPlayer());
                                isIdlePlaying = false; // Ensure idle animation stops
                                break;
                        }
                    }
                }
                else
                {
                    StartCoroutine(PlayDeathAnimation());
                    isIdlePlaying = false; // Ensure idle animation stops
                }
            }
            else
            {
                //play victory animation
                StartCoroutine(PlayVictoryAnimation());
            }
        }

        private IEnumerator PlayIdleAndReassess(float distanceToTarget)
        {
            // Reassess state first
            ReassessState(distanceToTarget);

            // If state is still idle, play idle animation
            if (currentState == MonsterStates.Idle)
            {
                isIdlePlaying = true;
                moveDirection = 0;
                animator.SetInteger("animation", idleAnimation);
                Debug.Log($"{transform.parent.name} starting Idle animation. Expected duration: {idleAnimationDuration}");
                yield return new WaitForSeconds(idleAnimationDuration);
                Debug.Log($"{transform.parent.name} completed Idle animation.");
                isIdlePlaying = false;
                ReassessState(distanceToTarget);
            }
        }

        private void ReassessState(float distanceToTarget)
        {
            if (distanceToTarget > AttackRange)
            {
                currentState = MonsterStates.Approaching;
            }
            else if (RetreatWhenTooClose && distanceToTarget < (AttackRange - retreatBuffer))
            {
                currentState = MonsterStates.Retreating;
            }
            else if (Time.time > lastAttackTime + attackCooldown)
            {
                currentState = MonsterStates.Attacking;
            }
            else
            {
                currentState = MonsterStates.Reassessing; // Stay idle if no other actions are required
            }
        }

        private void HandleReassessingState(float distanceToTarget)
        {
            if (distanceToTarget > AttackRange)
            {
                currentState = MonsterStates.Approaching;
                isIdlePlaying = false; // Ensure idle animation stops
            }
            else if (RetreatWhenTooClose && distanceToTarget < (AttackRange - retreatBuffer))
            {
                currentState = MonsterStates.Retreating;
                isIdlePlaying = false; // Ensure idle animation stops
            }
            else if (Time.time > lastAttackTime + attackCooldown)
            {
                currentState = MonsterStates.Attacking;
                isIdlePlaying = false; // Ensure idle animation stops
            }
            else
            {
                if (!isIdlePlaying)
                {
                    //Debug.Log($"{transform.parent.name} entering Reassessing state.");
                    StartCoroutine(PlayIdleAnimation());
                }
            }
        }

        private void HandleApproachingState(float distanceToTarget)
        {
            if (distanceToTarget <= AttackRange)
            {
                currentState = MonsterStates.Reassessing;
                if (!isIdlePlaying)
                {
                    StartCoroutine(PlayIdleAnimation());
                }
            }
            else
            {
                moveDirection = IsFacingLeft ? -1 : 1; // Move towards the player
                animator.SetInteger("animation", walkForwardAnimation);
                isIdlePlaying = false; // Ensure idle animation stops
            }
        }

        private void HandleRetreatingState(float distanceToTarget)
        {
            if (distanceToTarget >= AttackRange - retreatBuffer)
            {
                currentState = MonsterStates.Reassessing;
                if (!isIdlePlaying)
                {
                    StartCoroutine(PlayIdleAnimation());
                }
            }
            else
            {
                Retreat();
                isIdlePlaying = false; // Ensure idle animation stops
                animator.SetInteger("animation", walkBackwardAnimation);
            }
        }

        private IEnumerator PlayIdleAnimation()
        {
            isIdlePlaying = true;
            moveDirection = 0;
            animator.SetInteger("animation", idleAnimation);
            //Debug.Log($"{transform.parent.name} starting Idle animation. Expected duration: {idleAnimationDuration}");

            if (Vector3.Distance(transform.parent.position, player.transform.parent.position) <= AttackRange && Time.time > lastAttackTime + attackCooldown)
            {
                //Debug.Log($"{transform.parent.name} interrupting Idle animation to attack.");
                isIdlePlaying = false;
                currentState = MonsterStates.Attacking;
                //StartCoroutine(AttackPlayer());
                yield break;
            }

            isIdlePlaying = false;
            //ReassessState(Vector3.Distance(transform.parent.position, player.transform.parent.position));
        }

        private void Retreat()
        {
            moveDirection = IsFacingLeft ? 1 : -1;
            animator.SetInteger("animation", walkBackwardAnimation);
        }

        private IEnumerator PlayVictoryAnimation()
        {
            animator.SetInteger("animation", victoryAnimation);
            yield return new WaitForSeconds(victoryAnimationDuration);
            //animator.SetInteger("animation", idleAnimation);
        }

        private IEnumerator PlayDeathAnimation()
        {
            animator.SetInteger("animation", deathAnimation);
            yield return new WaitForSeconds(deathAnimationDuration);
            transform.parent.gameObject.SetActive(false);
        }

        private IEnumerator AttackPlayer()
        {
            var attackComponents = transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                              .Where(c => c.gameObject != transform.parent.gameObject).ToList();
            if (attackComponents != null && attackComponents.Count() > 0)
            {
                lastAttackTime = Time.time;
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Battle));
                }

                animator.SetInteger("animation", attackAnimation);
                yield return new WaitForSeconds(attackAnimationDuration);

                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Default));
                }

                currentState = MonsterStates.Reassessing;
            }
            else
            {
                Debug.LogWarning($"No attack components found for enemy: {transform.parent.name} -> Resorting to {nameof(FallbackAttack)}");
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    yield return StartCoroutine(FallbackAttack());
                }
            }
        }

        private IEnumerator FallbackAttack()
        {
            lastAttackTime = Time.time;
            player.Health.Decrement(attackDamage);
            animator.SetInteger("animation", attackAnimation);
            yield return new WaitForSeconds(attackAnimationDuration);
            currentState = MonsterStates.Reassessing;
        }

        public void Orient()
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
