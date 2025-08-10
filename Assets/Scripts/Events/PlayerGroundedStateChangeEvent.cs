using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for handling player grounded state changes
    /// Manages jump count resets and landing audio/effects
    /// </summary>
    public class PlayerGroundedStateChangeEvent : Simulation.Event
    {
        public Player player;
        public bool isGrounded;
        public bool previousGroundedState;

        public override bool Precondition()
        {
            return player != null && isGrounded != previousGroundedState;
        }

        public override void Execute()
        {
            if (player != null && isGrounded)
            {
                // Reset jump count when player lands
                var mediator = player.GetComponent<PlayerStateMediator>();
                mediator?.ResetJumpCount();
                
                // Trigger landing audio
                var audioEvent = Simulation.Schedule<PlayerAudioEvent>(0f);
                audioEvent.player = player;
                audioEvent.audioType = PlayerAudioType.Land;
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }
}