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
    /// </summary>
    public class HealthRegenerationEvent : Simulation.Event
    {
        public Health health;
        public float amount;
        public bool triggerEffects = true;

        public override bool Precondition()
        {
            return health != null && health.IsAlive && amount > 0;
        }

        public override void Execute()
        {
            if (health != null)
            {
                // Apply healing
                health.Increment(amount);
                
                if (triggerEffects)
                {
                    // Schedule healing number display
                    var damageNumberController = health.GetComponent<DamageNumberController>();
                    if (damageNumberController != null)
                    {
                        var damageNumberEvent = Simulation.Schedule<DamageNumberEvent>(0f);
                        damageNumberEvent.controller = damageNumberController;
                        damageNumberEvent.amount = amount;
                        damageNumberEvent.numberType = DamageNumberType.Healing;
                    }
                    
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