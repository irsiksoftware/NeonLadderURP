using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
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
        protected virtual int attackDamage { get; set; } = Constants.Physics.Combat.EnemyDamage; // Damage per attack

        public float deathBuffer = Constants.Physics.Combat.DeathAnimationBuffer;

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
                    return boxCollider != null ? boxCollider.size.x / 2 + Constants.Physics.Combat.BoxColliderOffset : Constants.Physics.Combat.DefaultAttackRange; // Default value if no BoxCollider found
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
        protected virtual float retreatBuffer { get; set; } = Constants.Physics.Combat.RetreatBuffer;
        [SerializeField]
        private float _attackCooldown = Constants.Physics.Combat.AttackCooldown; // Default value if not set in the editor or overridden

        public virtual float attackCooldown
        {
            get => _attackCooldown;
            set => _attackCooldown = value;
        }


        private bool isIdlePlaying = false;

        protected override void Awake()
        {
            if (attackCooldown <= AttackAnimationDuration)
            {
                Debug.LogWarning($"Attack cooldown is less than or equal to attack animation duration for enemy: {transform.parent.name}");
            }
            base.Awake();
            health = GetComponentInParent<Health>();
            if (lootTable == null)
            {
                RuntimeLootTable = LootHelper.LoadLootTable(this);
            }
        }

        protected virtual void Start()
        {
            // Initialize event-driven AI system
            if (ShouldEngagePlayer && health.IsAlive)
            {
                // Start periodic reassessment instead of per-frame polling
                SchedulePeriodicReassessment(player, 0.3f, 60f); // Every 300ms for 60 seconds
                
                // Initial distance evaluation
                ScheduleDistanceEvaluation(player, 0.1f);
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
                    targetVelocity.x /= 2;
                }

                velocity.x = targetVelocity.x;  // Apply horizontal movement
            }
        }

        protected override void Update()
        {
            float distanceToTarget = Vector3.Distance(transform.parent.position, player.transform.parent.position);
            IsFacingLeft = player.transform.parent.position.x < transform.parent.position.x;
            if (health.IsAlive)
            {
                Orient();
                if (Animator.GetInteger("animation") == 3)
                {
                    return;
                }
            }

            if (ShouldEngagePlayer && !isIdlePlaying)
            {

                base.Update();
                if (health.IsAlive)
                {
                    if (ShouldEngagePlayer && player.Health.IsAlive)
                    {
                        //if (currentState != MonsterStates.Attacking)
                        //{
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
                        //}
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
                Animator.SetInteger("animation", (int)Animations.Idle);
                Debug.Log($"{transform.parent.name} starting Idle animation. Expected duration: {IdleAnimationDuration}");
                yield return new WaitForSeconds(IdleAnimationDuration);
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
                Animator.SetInteger("animation", (int)Animations.WalkForward);
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
                Animator.SetInteger("animation", (int)Animations.WalkBackward);
            }
        }

        private IEnumerator PlayIdleAnimation()
        {
            isIdlePlaying = true;
            moveDirection = 0;
            Animator.SetInteger("animation", (int)Animations.Idle);

            if (Vector3.Distance(transform.parent.position, player.transform.parent.position) <= AttackRange && Time.time > lastAttackTime + attackCooldown)
            {
                isIdlePlaying = false;
                currentState = MonsterStates.Attacking;
                yield break;
            }

            isIdlePlaying = false;
        }

        private void Retreat()
        {
            moveDirection = IsFacingLeft ? 1 : -1;
            Animator.SetInteger("animation", (int)Animations.WalkBackward);
        }

        private IEnumerator PlayVictoryAnimation()
        {
            Animator.SetInteger("animation", (int)Animations.Victory);
            yield return new WaitForSeconds(VictoryAnimationDuration);
        }

        private IEnumerator PlayDeathAnimation()
        {
            Animator.SetInteger("animation", (int)Animations.Die);
            yield return new WaitForSeconds(DeathAnimationDuration);
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

                Animator.SetInteger("animation", (int)Animations.Attack1);
                yield return new WaitForSeconds(AttackAnimationDuration);

                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Default));
                }
            }

            ReassessState(Vector3.Distance(transform.parent.position, player.transform.parent.position));
        }

        // Event-driven AI methods to replace direct state management
        public void ScheduleStateTransition(MonsterStates newState, float delay = 0f)
        {
            var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(delay);
            stateEvent.enemy = this;
            stateEvent.newState = newState;
            stateEvent.previousState = currentState;
        }

        public void ScheduleDistanceEvaluation(Player target, float delay = 0.2f)
        {
            var distanceEvent = Simulation.Schedule<EnemyDistanceEvaluationEvent>(delay);
            distanceEvent.enemy = this;
            distanceEvent.player = target;
        }

        public void ScheduleAttackValidation(Player target, float delay = 0f)
        {
            var attackEvent = Simulation.Schedule<EnemyAttackValidationEvent>(delay);
            attackEvent.enemy = this;
            attackEvent.target = target;
            attackEvent.lastAttackTime = lastAttackTime;
        }

        public void SchedulePeriodicReassessment(Player target, float interval = 0.5f, float duration = 10f)
        {
            var reassessmentEvent = Simulation.Schedule<EnemyPeriodicReassessmentEvent>(0f);
            reassessmentEvent.enemy = this;
            reassessmentEvent.player = target;
            reassessmentEvent.reassessmentInterval = interval;
            reassessmentEvent.duration = duration;
        }

        public void ScheduleAnimationChange(EnemyAnimationType animationType, float delay = 0f)
        {
            var animEvent = Simulation.Schedule<EnemyAnimationEvent>(delay);
            animEvent.enemy = this;
            animEvent.animationType = animationType;
        }

        // Expose currentState for event system (fix field visibility)
        public MonsterStates CurrentState => currentState;

        public void SetState(MonsterStates newState)
        {
            currentState = newState;
        }

        // Public accessors for event system
        public int AttackDamage => attackDamage;
        public float AttackCooldown => attackCooldown;
        public float AttackRangeValue => AttackRange;
    }
}
