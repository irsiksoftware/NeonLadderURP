using NeonLadder.Core;
using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for scheduled health regeneration with precondition checks
    /// Prevents regeneration for dead entities and respects max health limits
    /// </summary>
    public class HealthRegenerationEvent : Simulation.Event
    {
        public Health health;
        public float amount;

        public override bool Precondition()
        {
            // Don't regenerate health for dead entities or if already at max
            return health != null && health.IsAlive && health.current < health.max;
        }

        public override void Execute()
        {
            if (health != null)
            {
                health.Increment(amount);
            }
        }

        internal override void Cleanup()
        {
            health = null;
        }
    }
}