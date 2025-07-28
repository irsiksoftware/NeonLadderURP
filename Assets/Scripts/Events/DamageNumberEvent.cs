using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for displaying damage numbers through DamageNumbersPro
    /// Centralizes all damage number spawning through the simulation queue
    /// </summary>
    public class DamageNumberEvent : Simulation.Event
    {
        public DamageNumberController controller;
        public float amount;
        public Vector3 position;
        public DamageNumberType numberType;

        public override bool Precondition()
        {
            return controller != null && amount > 0;
        }

        public override void Execute()
        {
            if (controller != null)
            {
                // Spawn damage number through DamageNumbersPro
                controller.SpawnPopup(amount);
            }
        }

        internal override void Cleanup()
        {
            controller = null;
        }
    }

    /// <summary>
    /// Types of damage numbers for different visual styles
    /// </summary>
    public enum DamageNumberType
    {
        Damage,
        Healing,
        CriticalDamage,
        StaminaLoss
    }
}