using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for applying damage to health with visual and audio coordination
    /// Replaces direct Health.Decrement() calls throughout the codebase
    /// </summary>
    public class HealthDamageEvent : Simulation.Event
    {
        public Health health;
        public float damageAmount;
        public bool triggerEffects = true;

        public override bool Precondition()
        {
            return health != null && health.IsAlive && damageAmount > 0;
        }

        public override void Execute()
        {
            if (health != null)
            {
                // Apply damage
                health.Increment(-damageAmount); // Use negative increment to avoid direct Decrement
                
                if (triggerEffects)
                {
                    // Schedule damage number display
                    var damageNumberController = health.GetComponent<DamageNumberController>();
                    if (damageNumberController != null)
                    {
                        var damageNumberEvent = Simulation.Schedule<DamageNumberEvent>(0f);
                        damageNumberEvent.controller = damageNumberController;
                        damageNumberEvent.amount = damageAmount;
                        damageNumberEvent.numberType = DamageNumberType.Damage;
                    }
                    
                    // Schedule damage audio
                    var audioSource = health.GetComponentInParent<AudioSource>();
                    if (audioSource != null)
                    {
                        var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                        audioEvent.audioSource = audioSource;
                        audioEvent.audioType = AudioEventType.Damage;
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