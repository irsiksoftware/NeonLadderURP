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
        public GameObject targetObject;
        public float amount;
        public Vector3 worldPosition;
        public DamageNumberType numberType = DamageNumberType.Damage;
        public Color? customColor;
        public float? customScale;

        public override bool Precondition()
        {
            return amount > 0;
        }

        public override void Execute()
        {
            // Get damage number system from simulation model
            var dnSystem = Simulation.GetModel<DamageNumberSystem>();
            if (dnSystem != null)
            {
                // Determine position - use world position if set, otherwise get from target
                Vector3 spawnPosition = worldPosition;
                if (spawnPosition == Vector3.zero && targetObject != null)
                {
                    spawnPosition = targetObject.transform.position;
                    
                    // Try to get offset from DamageNumberController if present
                    var controller = targetObject.GetComponent<DamageNumberController>();
                    if (controller != null)
                    {
                        spawnPosition += new Vector3(0, controller.YOffset, 0);
                    }
                }
                
                dnSystem.SpawnNumber(spawnPosition, amount, numberType, customColor, customScale);
            }
        }

        internal override void Cleanup()
        {
            targetObject = null;
            customColor = null;
            customScale = null;
            worldPosition = Vector3.zero;
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