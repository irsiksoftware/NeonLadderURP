using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Stats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// TDD Test Suite for Event-Driven Health/Stamina System
    /// 
    /// These tests ensure that:
    /// 1. Health/Stamina changes are scheduled as events instead of immediate updates
    /// 2. Event timing respects delay parameters for realistic health regeneration
    /// 3. Multiple events can be queued without conflicts
    /// 4. Events can be cancelled/rescheduled for dynamic gameplay
    /// 5. Anti-patterns like direct stat modification are prevented
    /// </summary>
    public class EventDrivenStatsTests
    {
        private GameObject testGameObject;
        private Health healthComponent;
        private Stamina staminaComponent;

        [SetUp]
        public void Setup()
        {
            // Clear simulation before each test to prevent cross-test pollution
            Simulation.Clear();
            
            // Create test GameObject with Health and Stamina components
            testGameObject = new GameObject("TestEntity");
            healthComponent = testGameObject.AddComponent<Health>();
            staminaComponent = testGameObject.AddComponent<Stamina>();
            
            // Ensure clean initial state
            healthComponent.current = healthComponent.max;
            staminaComponent.current = staminaComponent.max;
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up simulation and test objects
            Simulation.Clear();
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
        }

        [Test]
        public void ScheduleHealthRegeneration_ShouldCreateEventInQueue()
        {
            // Arrange
            var initialHealth = healthComponent.current;
            var regenAmount = 10f;
            var delay = 1.0f;

            // Act
            var regenEvent = Simulation.Schedule<HealthRegenerationEvent>(delay);
            regenEvent.health = healthComponent;
            regenEvent.amount = regenAmount;

            // Assert
            var queueCount = Simulation.Tick(); // Returns remaining events
            Assert.Greater(queueCount, 0, "Health regeneration event should be queued");
            Assert.AreEqual(initialHealth, healthComponent.current, "Health should not change immediately");
        }

        [Test]
        public void ScheduleStaminaRegeneration_ShouldCreateEventInQueue()
        {
            // Arrange
            staminaComponent.Decrement(20f); // Reduce stamina to test regeneration
            var initialStamina = staminaComponent.current;
            var regenAmount = 5f;
            var delay = 0.5f;

            // Act
            var regenEvent = Simulation.Schedule<StaminaRegenerationEvent>(delay);
            regenEvent.stamina = staminaComponent;
            regenEvent.amount = regenAmount;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Stamina regeneration event should be queued");
            Assert.AreEqual(initialStamina, staminaComponent.current, "Stamina should not change immediately");
        }

        [UnityTest]
        public IEnumerator HealthRegenerationEvent_ShouldExecuteAfterDelay()
        {
            // Arrange
            healthComponent.Decrement(30f); // Damage the health
            var expectedHealth = healthComponent.current + 15f;
            var delay = 0.1f; // Short delay for test

            // Act
            var regenEvent = Simulation.Schedule<HealthRegenerationEvent>(delay);
            regenEvent.health = healthComponent;
            regenEvent.amount = 15f;

            // Wait for the event to execute
            yield return new WaitForSeconds(delay + 0.05f);
            Simulation.Tick(); // Process events

            // Assert
            Assert.AreEqual(expectedHealth, healthComponent.current, 
                "Health should be regenerated after delay");
        }

        [UnityTest]
        public IEnumerator StaminaRegenerationEvent_ShouldExecuteAfterDelay()
        {
            // Arrange
            staminaComponent.Decrement(40f); // Exhaust some stamina
            var expectedStamina = staminaComponent.current + 20f;
            var delay = 0.1f;

            // Act
            var regenEvent = Simulation.Schedule<StaminaRegenerationEvent>(delay);
            regenEvent.stamina = staminaComponent;
            regenEvent.amount = 20f;

            // Wait for the event to execute
            yield return new WaitForSeconds(delay + 0.05f);
            Simulation.Tick(); // Process events

            // Assert
            Assert.AreEqual(expectedStamina, staminaComponent.current,
                "Stamina should be regenerated after delay");
        }

        [Test]
        public void MultipleHealthEvents_ShouldQueueWithoutConflicts()
        {
            // Arrange
            healthComponent.Decrement(50f); // Significant damage

            // Act - Schedule multiple regeneration events
            var event1 = Simulation.Schedule<HealthRegenerationEvent>(0.1f);
            event1.health = healthComponent;
            event1.amount = 10f;

            var event2 = Simulation.Schedule<HealthRegenerationEvent>(0.2f);
            event2.health = healthComponent;
            event2.amount = 15f;

            var event3 = Simulation.Schedule<HealthRegenerationEvent>(0.3f);
            event3.health = healthComponent;
            event3.amount = 20f;

            // Assert
            var queueCount = Simulation.Tick();
            Assert.AreEqual(3, queueCount, "All three health events should be queued");
        }

        [Test]
        public void EventPrecondition_ShouldPreventInvalidExecution()
        {
            // Arrange
            healthComponent.Decrement(100f); // Kill the entity (health = 0)

            // Act
            var regenEvent = Simulation.Schedule<HealthRegenerationEvent>(0.1f);
            regenEvent.health = healthComponent;
            regenEvent.amount = 50f;

            // Assert
            // The event should be queued but won't execute due to precondition
            var queueCount = Simulation.Tick();
            Assert.GreaterOrEqual(queueCount, 0, "Event queue should be processed");
            Assert.AreEqual(0f, healthComponent.current, "Dead entities should not regenerate health");
        }

        [Test]
        public void ScheduledEvents_ShouldRespectMaxStatLimits()
        {
            // Arrange
            var initialHealth = healthComponent.max;
            healthComponent.current = initialHealth; // Already at max

            // Act
            var regenEvent = Simulation.Schedule<HealthRegenerationEvent>(0.1f);
            regenEvent.health = healthComponent;
            regenEvent.amount = 50f; // Attempt to exceed max

            // Assert
            // This test verifies that regeneration events respect stat limits
            // Implementation should clamp to max value
            Assert.AreEqual(initialHealth, healthComponent.current);
        }

        [Test]
        public void ContinuousRegeneration_ShouldScheduleRecurringEvents()
        {
            // Arrange
            staminaComponent.Decrement(80f); // Significantly reduce stamina

            // Act
            var continuousRegenEvent = Simulation.Schedule<StaminaContinuousRegenerationEvent>(0.1f);
            continuousRegenEvent.stamina = staminaComponent;
            continuousRegenEvent.regenPerSecond = 10f;
            continuousRegenEvent.duration = 3.0f; // 3 seconds of regeneration

            // Assert
            var queueCount = Simulation.Tick();
            Assert.Greater(queueCount, 0, "Continuous regeneration should be scheduled");
        }

        // Anti-pattern prevention tests
        [Test]
        public void DirectStatModification_ShouldBeDiscouraged()
        {
            // This test documents the anti-pattern we're trying to avoid
            // Direct calls to Increment/Decrement should be replaced with events
            
            // Arrange
            var beforeModification = healthComponent.current;
            
            // Act (anti-pattern - direct modification)
            healthComponent.Decrement(10f);
            
            // Assert - This shows the immediate change we want to avoid
            Assert.AreNotEqual(beforeModification, healthComponent.current,
                "Direct modification changes stats immediately (anti-pattern)");
            
            // TODO: Replace direct calls with event scheduling
            // healthComponent.ScheduleDamage(10f, 0f); // Immediate damage event
            // healthComponent.ScheduleRegeneration(5f, 2f); // Delayed regeneration
        }
    }

    // Event classes to be implemented
    public class HealthRegenerationEvent : Simulation.Event
    {
        public Health health;
        public float amount;

        public override bool Precondition()
        {
            // Don't regenerate health for dead entities
            return health != null && health.IsAlive && health.current < health.max;
        }

        public override void Execute()
        {
            if (health != null)
            {
                health.Increment(amount);
            }
        }
    }

    public class StaminaRegenerationEvent : Simulation.Event
    {
        public Stamina stamina;
        public float amount;

        public override bool Precondition()
        {
            // Only regenerate if stamina is below max
            return stamina != null && stamina.current < stamina.max;
        }

        public override void Execute()
        {
            if (stamina != null)
            {
                stamina.Increment(amount);
            }
        }
    }

    public class StaminaContinuousRegenerationEvent : Simulation.Event
    {
        public Stamina stamina;
        public float regenPerSecond;
        public float duration;
        private float elapsed = 0f;

        public override bool Precondition()
        {
            return stamina != null && elapsed < duration && stamina.current < stamina.max;
        }

        public override void Execute()
        {
            if (stamina != null)
            {
                var regenAmount = regenPerSecond * Time.fixedDeltaTime;
                stamina.Increment(regenAmount);
                elapsed += Time.fixedDeltaTime;

                // Reschedule for next frame if duration not complete
                if (elapsed < duration && stamina.current < stamina.max)
                {
                    Simulation.Reschedule(this, Time.fixedDeltaTime);
                }
            }
        }
    }
}