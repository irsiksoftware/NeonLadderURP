using NeonLadder.Core;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for continuous stamina regeneration over time
    /// Automatically reschedules itself until duration is complete or stamina is full
    /// </summary>
    public class StaminaContinuousRegenerationEvent : Simulation.Event
    {
        public Stamina stamina;
        public float regenPerSecond;
        public float duration;
        public float elapsed = 0f;

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
                    tick = Time.time + Time.fixedDeltaTime; // Reschedule
                }
            }
        }

        internal override void Cleanup()
        {
            stamina = null;
        }
    }
}