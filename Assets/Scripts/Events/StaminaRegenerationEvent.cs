using NeonLadder.Core;
using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for scheduled stamina regeneration with smart precondition handling
    /// Only regenerates when stamina is below maximum capacity
    /// </summary>
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

        internal override void Cleanup()
        {
            stamina = null;
        }
    }
}