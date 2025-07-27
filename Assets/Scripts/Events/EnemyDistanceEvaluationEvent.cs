using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for periodic distance evaluation between enemy and player
    /// Replaces per-frame distance polling with scheduled evaluations
    /// </summary>
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

        internal override void Cleanup()
        {
            enemy = null;
            player = null;
        }
    }
}