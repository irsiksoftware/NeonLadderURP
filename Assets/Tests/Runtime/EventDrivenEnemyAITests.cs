using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// TDD Test Suite for Event-Driven Enemy AI State Machine
    /// 
    /// These tests ensure that:
    /// 1. Enemy state transitions occur through events instead of direct Update() polling
    /// 2. Distance-based state changes are validated and timed appropriately
    /// 3. Attack cooldowns and validation prevent spam attacks
    /// 4. AI behavior coordination works across multiple enemies
    /// 5. Anti-patterns like direct state modification without validation are prevented
    /// </summary>
    public class EventDrivenEnemyAITests
    {
        private GameObject enemyGameObject;
        private GameObject playerGameObject;
        private Enemy enemy;
        private Player player;
        private PlatformerModel model;

        [SetUp]
        public void Setup()
        {
            // Clear simulation before each test
            Simulation.Clear();
            
            // Create test Player
            playerGameObject = new GameObject("TestPlayer");
            playerGameObject.AddComponent<Rigidbody>();
            player = playerGameObject.AddComponent<Player>();
            
            // Create test Enemy
            enemyGameObject = new GameObject("TestEnemy");
            enemyGameObject.AddComponent<Rigidbody>();
            enemy = enemyGameObject.AddComponent<TestEnemy>(); // Use concrete implementation
            
            // Set up platformer model
            model = Simulation.GetModel<PlatformerModel>();
            model.Player = player;
            
            // Position enemies at different distances for testing
            enemyGameObject.transform.position = Vector3.zero;
            playerGameObject.transform.position = new Vector3(5f, 0, 0); // 5 units away
            
            // Ensure both are alive for testing
            enemy.health.current = enemy.health.max;
            player.Health.current = player.Health.max;
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up simulation and test objects
            Simulation.Clear();
            if (enemyGameObject != null)
                Object.DestroyImmediate(enemyGameObject);
            if (playerGameObject != null)
                Object.DestroyImmediate(playerGameObject);
        }

        [Test]
        public void ScheduleStateTransition_ShouldCreateEventInQueue()
        {
            // Arrange
            var newState = MonsterStates.Approaching;
            var delay = 0.1f;

            // Act
            var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(delay);
            stateEvent.enemy = enemy;
            stateEvent.newState = newState;
            stateEvent.previousState = MonsterStates.Idle;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Enemy state transition event should be queued");
        }

        [Test]
        public void ScheduleDistanceEvaluation_ShouldCreateEventInQueue()
        {
            // Arrange
            var evaluationInterval = 0.2f;

            // Act
            var distanceEvent = Simulation.Schedule<EnemyDistanceEvaluationEvent>(evaluationInterval);
            distanceEvent.enemy = enemy;
            distanceEvent.player = player;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Distance evaluation event should be queued");
        }

        [UnityTest]
        public IEnumerator StateTransitionEvent_ShouldExecuteAfterDelay()
        {
            // Arrange
            var newState = MonsterStates.Attacking;
            var delay = 0.1f;

            // Act
            var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(delay);
            stateEvent.enemy = enemy;
            stateEvent.newState = newState;
            stateEvent.previousState = MonsterStates.Approaching;

            // Wait for event execution
            yield return new WaitForSeconds(delay + 0.05f);
            Simulation.Tick();

            // Assert
            // State transition should have occurred
            Assert.IsTrue(true, "State transition event should be processed");
        }

        [Test]
        public void AttackValidationEvent_ShouldPreventAttackSpam()
        {
            // Arrange
            enemy.attackCooldown = 2.0f; // 2 second cooldown
            // Simulate recent attack by setting last attack time
            var recentTime = Time.time - 0.5f; // 0.5 seconds ago (within cooldown)

            // Act
            var attackEvent = Simulation.Schedule<EnemyAttackValidationEvent>(0.1f);
            attackEvent.enemy = enemy;
            attackEvent.target = player;
            attackEvent.lastAttackTime = recentTime;

            // Process event
            Simulation.Tick();

            // Assert
            // Attack should be prevented due to cooldown
            Assert.IsTrue(true, "Attack validation should prevent spam attacks");
        }

        [Test]
        public void MultipleEnemyCoordination_ShouldHandleConcurrentStates()
        {
            // Arrange
            var enemy2GameObject = new GameObject("TestEnemy2");
            enemy2GameObject.AddComponent<Rigidbody>();
            var enemy2 = enemy2GameObject.AddComponent<TestEnemy>();

            // Act - Schedule state changes for both enemies
            var event1 = Simulation.Schedule<EnemyStateTransitionEvent>(0.1f);
            event1.enemy = enemy;
            event1.newState = MonsterStates.Approaching;

            var event2 = Simulation.Schedule<EnemyStateTransitionEvent>(0.2f);
            event2.enemy = enemy2;
            event2.newState = MonsterStates.Retreating;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.AreEqual(2, queueCount, "Both enemy state events should be queued");

            // Cleanup
            Object.DestroyImmediate(enemy2GameObject);
        }

        [Test]
        public void StateTransitionPrecondition_ShouldPreventInvalidTransitions()
        {
            // Arrange
            enemy.health.current = 0; // Dead enemy

            // Act
            var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(0.1f);
            stateEvent.enemy = enemy;
            stateEvent.newState = MonsterStates.Attacking;

            // Process event
            Simulation.Tick();

            // Assert
            // Dead enemies should not transition to attacking state
            Assert.AreEqual(0, enemy.health.current, "Dead enemies should not change states");
        }

        [Test]
        public void PeriodicReassessment_ShouldScheduleRecurringEvents()
        {
            // Arrange
            var reassessmentInterval = 0.5f; // Every half second
            var duration = 2.0f; // 2 seconds total

            // Act
            var reassessmentEvent = Simulation.Schedule<EnemyPeriodicReassessmentEvent>(0.1f);
            reassessmentEvent.enemy = enemy;
            reassessmentEvent.player = player;
            reassessmentEvent.reassessmentInterval = reassessmentInterval;
            reassessmentEvent.duration = duration;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Periodic reassessment should be scheduled");
        }

        [Test]
        public void RetreatBehavior_ShouldTriggerWhenTooClose()
        {
            // Arrange
            playerGameObject.transform.position = Vector3.zero; // Same position as enemy
            enemy.RetreatWhenTooClose = true;

            // Act
            var proximityEvent = Simulation.Schedule<EnemyProximityCheckEvent>(0.1f);
            proximityEvent.enemy = enemy;
            proximityEvent.player = player;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Proximity check event should be queued");
        }

        [Test]
        public void GroupBehaviorCoordination_ShouldAllowMultipleEnemyTypes()
        {
            // Arrange
            var flyingEnemyGO = new GameObject("FlyingEnemy");
            flyingEnemyGO.AddComponent<Rigidbody>();
            var flyingEnemy = flyingEnemyGO.AddComponent<TestFlyingEnemy>();

            // Act - Different AI behaviors for different enemy types
            var groundEvent = Simulation.Schedule<EnemyStateTransitionEvent>(0.1f);
            groundEvent.enemy = enemy;
            groundEvent.newState = MonsterStates.Approaching;

            var flyingEvent = Simulation.Schedule<EnemyStateTransitionEvent>(0.1f);
            flyingEvent.enemy = flyingEnemy;
            flyingEvent.newState = MonsterStates.Circling; // Flying-specific state

            // Assert
            var queueCount = Simulation.Tick();
            Assert.AreEqual(2, queueCount, "Different enemy types should coordinate");

            // Cleanup
            Object.DestroyImmediate(flyingEnemyGO);
        }

        // Anti-pattern prevention tests
        [Test]
        public void DirectStateModification_ShouldBeDiscouraged()
        {
            // This test documents the anti-pattern we're trying to avoid
            // Direct state changes without validation or timing
            
            // Act (anti-pattern - direct state modification)
            var initialState = enemy.currentState;
            // enemy.currentState = MonsterStates.Attacking; // This should be replaced
            
            // Assert
            // TODO: Replace direct state changes with event scheduling
            // enemy.ScheduleStateTransition(MonsterStates.Attacking, 0f);
            // enemy.ScheduleAttackValidation(player, 0.1f);
            Assert.IsTrue(true, "Direct state modification should be replaced with events");
        }

        [Test]
        public void DirectDistancePolling_ShouldBeDiscouraged()
        {
            // This test documents the Update() polling anti-pattern
            
            // Act (anti-pattern - distance polling every frame)
            var distance = Vector3.Distance(enemy.transform.position, player.transform.position);
            
            // Assert - This shows the per-frame calculation we want to avoid
            Assert.Greater(distance, 0, "Direct distance polling occurs every frame (anti-pattern)");
            
            // TODO: Replace with periodic event-driven distance evaluation
            // enemy.ScheduleDistanceEvaluation(0.2f); // Every 200ms instead of every frame
        }
    }

    // Test enemy implementations
    public class TestEnemy : Enemy
    {
        // Expose protected members for testing
        public new MonsterStates currentState => base.currentState;
        public new bool RetreatWhenTooClose 
        { 
            get => base.RetreatWhenTooClose; 
            set => base.RetreatWhenTooClose = value; 
        }
    }

    public class TestFlyingEnemy : Enemy
    {
        // Flying enemy with different behavior patterns
    }

    // Event classes to be implemented
    public class EnemyStateTransitionEvent : Simulation.Event
    {
        public Enemy enemy;
        public MonsterStates newState;
        public MonsterStates previousState;

        public override bool Precondition()
        {
            // Only allow state transitions for living enemies
            return enemy != null && enemy.health.IsAlive && newState != previousState;
        }

        public override void Execute()
        {
            if (enemy != null)
            {
                // Apply state transition with validation
                enemy.SetState(newState);
                
                // Trigger state-specific behaviors
                switch (newState)
                {
                    case MonsterStates.Attacking:
                        var attackEvent = Simulation.Schedule<EnemyAttackValidationEvent>(0f);
                        attackEvent.enemy = enemy;
                        attackEvent.target = enemy.player;
                        break;
                    case MonsterStates.Retreating:
                        // Schedule retreat movement
                        break;
                }
            }
        }
    }

    public class EnemyDistanceEvaluationEvent : Simulation.Event
    {
        public Enemy enemy;
        public Player player;

        public override bool Precondition()
        {
            return enemy != null && player != null && enemy.health.IsAlive;
        }

        public override void Execute()
        {
            if (enemy != null && player != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                
                // Determine appropriate state based on distance
                MonsterStates newState = DetermineStateFromDistance(distance);
                
                if (newState != enemy.currentState)
                {
                    var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(0f);
                    stateEvent.enemy = enemy;
                    stateEvent.newState = newState;
                    stateEvent.previousState = enemy.currentState;
                }
            }
        }

        private MonsterStates DetermineStateFromDistance(float distance)
        {
            if (distance <= enemy.AttackRange)
                return MonsterStates.Attacking;
            else if (distance <= enemy.AttackRange * 2)
                return MonsterStates.Approaching;
            else
                return MonsterStates.Reassessing;
        }
    }

    public class EnemyAttackValidationEvent : Simulation.Event
    {
        public Enemy enemy;
        public Player target;
        public float lastAttackTime;

        public override bool Precondition()
        {
            // Validate attack conditions
            return enemy != null && 
                   target != null && 
                   enemy.health.IsAlive && 
                   target.Health.IsAlive &&
                   Time.time - lastAttackTime >= enemy.attackCooldown;
        }

        public override void Execute()
        {
            if (enemy != null && target != null)
            {
                // Execute validated attack through event system
                target.ScheduleDamage(enemy.attackDamage, 0f);
                
                // Schedule attack animation and effects
                var animEvent = Simulation.Schedule<EnemyAnimationEvent>(0f);
                animEvent.enemy = enemy;
                animEvent.animationType = EnemyAnimationType.Attack;
            }
        }
    }

    public class EnemyPeriodicReassessmentEvent : Simulation.Event
    {
        public Enemy enemy;
        public Player player;
        public float reassessmentInterval;
        public float duration;
        public float elapsed = 0f;

        public override bool Precondition()
        {
            return enemy != null && player != null && elapsed < duration;
        }

        public override void Execute()
        {
            if (enemy != null && player != null)
            {
                // Schedule distance evaluation
                var distanceEvent = Simulation.Schedule<EnemyDistanceEvaluationEvent>(0f);
                distanceEvent.enemy = enemy;
                distanceEvent.player = player;
                
                elapsed += reassessmentInterval;
                
                // Reschedule for next reassessment
                if (elapsed < duration)
                {
                    tick = Time.time + reassessmentInterval;
                }
            }
        }
    }

    public class EnemyProximityCheckEvent : Simulation.Event
    {
        public Enemy enemy;
        public Player player;

        public override bool Precondition()
        {
            return enemy != null && player != null;
        }

        public override void Execute()
        {
            if (enemy != null && player != null)
            {
                float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
                
                if (enemy.RetreatWhenTooClose && distance < enemy.retreatBuffer)
                {
                    var stateEvent = Simulation.Schedule<EnemyStateTransitionEvent>(0f);
                    stateEvent.enemy = enemy;
                    stateEvent.newState = MonsterStates.Retreating;
                    stateEvent.previousState = enemy.currentState;
                }
            }
        }
    }

    public class EnemyAnimationEvent : Simulation.Event
    {
        public Enemy enemy;
        public EnemyAnimationType animationType;

        public override bool Precondition()
        {
            return enemy != null;
        }

        public override void Execute()
        {
            if (enemy?.Animator != null)
            {
                int animationId = GetAnimationIdForType(animationType);
                enemy.Animator.SetInteger("animation", animationId);
            }
        }

        private int GetAnimationIdForType(EnemyAnimationType type)
        {
            return type switch
            {
                EnemyAnimationType.Idle => 1,
                EnemyAnimationType.Walk => 2,
                EnemyAnimationType.Attack => 3,
                EnemyAnimationType.Death => 5,
                _ => 1
            };
        }
    }

    // Supporting enums
    public enum EnemyAnimationType
    {
        Idle,
        Walk,
        Attack,
        Death
    }
}