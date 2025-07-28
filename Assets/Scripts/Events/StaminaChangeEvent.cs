using NeonLadder.Core;
using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for stamina modifications (damage/regeneration)
    /// Replaces direct Stamina.Increment/Decrement calls
    /// </summary>
    public class StaminaChangeEvent : Simulation.Event
    {
        public Stamina stamina;
        public float amount; // Positive for healing, negative for damage

        public override bool Precondition()
        {
            return stamina != null && amount != 0;
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