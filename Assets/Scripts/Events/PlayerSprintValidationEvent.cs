using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Common;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for validating and managing player sprint actions
    /// Enforces stamina requirements and speed limits for sprinting
    /// </summary>
    public class PlayerSprintValidationEvent : Simulation.Event
    {
        public Player player;
        public float requestedSpeedMultiplier;

        public override bool Precondition()
        {
            // Validate sprint conditions
            return player != null && 
                   player.Health.IsAlive && 
                   player.Stamina.current > Constants.Physics.Stamina.SprintCost;
        }

        public override void Execute()
        {
            if (player != null)
            {
                // Consume stamina for sprinting
                player.Stamina.Decrement(Constants.Physics.Stamina.SprintCost * Time.fixedDeltaTime);
                
                // Sprint speed modification is handled in ComputeVelocity
                // This event validates that sprinting is allowed
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }
}