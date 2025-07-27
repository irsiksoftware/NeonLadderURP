using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Common;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for validating and applying player velocity changes
    /// Enforces speed limits and prevents impossible movement values
    /// </summary>
    public class PlayerVelocityValidationEvent : Simulation.Event
    {
        public Player player;
        public Vector3 requestedVelocity;

        public override bool Precondition()
        {
            // Validate velocity is within reasonable limits
            return player != null && 
                   player.Health.IsAlive &&
                   requestedVelocity.magnitude <= Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier;
        }

        public override void Execute()
        {
            if (player != null)
            {
                // Apply validated velocity with clamping
                player.targetVelocity = Vector3.ClampMagnitude(requestedVelocity, 
                    Constants.DefaultMaxSpeed * Constants.SprintSpeedMultiplier);
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }
}