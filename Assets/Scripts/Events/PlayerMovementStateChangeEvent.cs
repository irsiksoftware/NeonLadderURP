using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for managing player movement state transitions and animation updates
    /// Centralizes animation logic and prevents inconsistent state changes
    /// </summary>
    public class PlayerMovementStateChangeEvent : Simulation.Event
    {
        public Player player;
        public PlayerMovementState newState;
        public PlayerMovementState previousState;

        public override bool Precondition()
        {
            return player != null && newState != previousState;
        }

        public override void Execute()
        {
            if (player != null)
            {
                // Update animation based on movement state
                int animationId = GetAnimationIdForState(newState);
                player.Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), animationId);
                
                // Trigger state-specific audio events
                if (newState == PlayerMovementState.Landing)
                {
                    var audioEvent = Simulation.Schedule<PlayerAudioEvent>(0f);
                    audioEvent.player = player;
                    audioEvent.audioType = PlayerAudioType.Land;
                }
            }
        }

        private int GetAnimationIdForState(PlayerMovementState state)
        {
            return state switch
            {
                PlayerMovementState.Idle => 1,
                PlayerMovementState.Walking => 6,
                PlayerMovementState.Running => 10,
                PlayerMovementState.Jumping => 11,
                PlayerMovementState.Falling => 12,
                PlayerMovementState.Rolling => 13,
                _ => 1
            };
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }

    /// <summary>
    /// Player movement states for animation and logic coordination
    /// </summary>
    public enum PlayerMovementState
    {
        Idle,
        Walking,
        Running,
        Jumping,
        Falling,
        Rolling,
        Landing
    }
}