using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for managing enemy AI state transitions
    /// Replaces direct state modifications with validated, timed transitions
    /// </summary>
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
                        // Schedule retreat movement with reduced speed
                        break;
                    case MonsterStates.Approaching:
                        // Schedule approach movement
                        break;
                }
            }
        }

        internal override void Cleanup()
        {
            enemy = null;
        }
    }
}