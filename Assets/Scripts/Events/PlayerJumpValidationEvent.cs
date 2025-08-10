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
            return player != null && 
                   player.Health.IsAlive && 
                   mediator != null &&
                   mediator.GetJumpCount() < mediator.GetMaxJumps();
        }

        public override void Execute()
        {
            var mediator = player?.GetComponent<PlayerStateMediator>();
            if (player != null && mediator != null)
            {
                // Apply jump force through validated path
                player.velocity.y = requestedJumpForce;
                mediator.IncrementJumpCount();
                // Note: isJumping flag is handled internally by PlayerAction
                
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