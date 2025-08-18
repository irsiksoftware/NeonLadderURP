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
            var mediator = player?.GetComponent<PlayerStateMediator>();
            var playerAction = player?.GetComponent<PlayerAction>();
            
            // If jump validation fails, reset the isJumping flag to prevent infinite loop
            if (player == null || !player.Health.IsAlive || mediator == null || 
                mediator.GetJumpCount() >= mediator.GetMaxJumps())
            {
                if (playerAction != null)
                {
                    playerAction.isJumping = false; // Reset flag even if jump fails
                }
                return false;
            }
            
            return true;
        }

        public override void Execute()
        {
            var mediator = player?.GetComponent<PlayerStateMediator>();
            var playerAction = player?.GetComponent<PlayerAction>();
            
            if (player != null && mediator != null && playerAction != null)
            {
                // Apply jump force through validated path
                player.velocity.y = requestedJumpForce;
                mediator.IncrementJumpCount();
                
                // CRITICAL FIX: Reset isJumping flag to prevent infinite jumping
                playerAction.isJumping = false;
                
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