using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for validating and executing enemy attacks
    /// Prevents attack spam through cooldown validation
    /// </summary>
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
                // Execute validated attack
                target.Health.Decrement(enemy.attackDamage);
                
                // Schedule attack animation and effects
                var animEvent = Simulation.Schedule<EnemyAnimationEvent>(0f);
                animEvent.enemy = enemy;
                animEvent.animationType = EnemyAnimationType.Attack;
                
                // Update last attack time
                lastAttackTime = Time.time;
            }
        }

        internal override void Cleanup()
        {
            enemy = null;
            target = null;
        }
    }
}