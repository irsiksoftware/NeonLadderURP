using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for regenerating health with visual and audio coordination
    /// Replaces direct Health.Increment() calls for healing throughout the codebase
    /// Prevents regeneration for dead entities and respects max health limits
    /// </summary>
    public class HealthRegenerationEvent : Simulation.Event
    {
        public Health health;
        public float amount;
        public bool triggerEffects = true;

        public override bool Precondition()
        {
            // Don't regenerate health for dead entities or if already at max
            return health != null && health.IsAlive && health.current < health.max && amount > 0;
        }

        public override void Execute()
        {
            if (health != null)
            {
                // Apply healing using Increment which now handles healing numbers automatically
                health.Increment(amount);
                
                if (triggerEffects)
                {
                    // Schedule healing audio
                    var audioSource = health.GetComponent<AudioSource>();
                    if (audioSource != null)
                    {
                        var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                        audioEvent.audioSource = audioSource;
                        audioEvent.audioType = AudioEventType.Healing;
                        audioEvent.volume = 0.5f;
                    }
                }
            }
        }

        internal override void Cleanup()
        {
            health = null;
        }
    }
}