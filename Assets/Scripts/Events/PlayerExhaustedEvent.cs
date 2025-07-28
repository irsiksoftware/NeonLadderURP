using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    public class PlayerExhaustedEvent : BaseGameEvent<PlayerExhaustedEvent>
    {
        public Player player;

        public override void Execute()
        {
            // Player is exhausted and cannot perform stamina-intensive actions
            // This could trigger UI feedback, audio cues, or movement restrictions
            if (player != null && player.Stamina.IsExhausted)
            {
                // Optional: Add audio feedback or visual effects
                // Optional: Temporarily reduce movement speed or disable sprint
            }
        }
    }
}