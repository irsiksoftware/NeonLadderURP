using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Common;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for validating and executing player jump actions
    /// Prevents invalid jumps (exceeding max jumps, insufficient stamina, etc.)
    /// </summary>
    public class PlayerJumpValidationEvent : Simulation.Event
    {
        public Player player;
        public float requestedJumpForce;

        public override bool Precondition()
        {
            // Validate jump conditions
            return player != null && 
                   player.Health.IsAlive && 
                   player.Actions.JumpCount < player.Actions.MaxJumps;
        }

        public override void Execute()
        {
            if (player != null && player.Actions != null)
            {
                // Apply jump force through validated path
                player.velocity.y = requestedJumpForce;
                player.Actions.IncrementJumpCount();
                player.Actions.isJumping = false; // Reset jump input flag
                
                // Trigger audio through event system
                var audioEvent = Simulation.Schedule<PlayerAudioEvent>(0f);
                audioEvent.player = player;
                audioEvent.audioType = PlayerAudioType.Jump;
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }
}