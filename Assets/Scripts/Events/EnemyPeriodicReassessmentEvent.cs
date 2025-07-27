using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for periodic enemy AI reassessment
    /// Replaces constant Update() polling with scheduled evaluations
    /// </summary>
    public class EnemyPeriodicReassessmentEvent : Simulation.Event
    {
        public Enemy enemy;
        public Player player;
        public float reassessmentInterval;
        public float duration;
        public float elapsed = 0f;

        public override bool Precondition()
        {
            return enemy != null && player != null && elapsed < duration && enemy.health.IsAlive;
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
                if (elapsed < duration && enemy.health.IsAlive)
                {
                    tick = Time.time + reassessmentInterval;
                }
            }
        }

        internal override void Cleanup()
        {
            enemy = null;
            player = null;
        }
    }
}