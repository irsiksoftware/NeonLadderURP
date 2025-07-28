using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerLanded : BaseGameEvent<PlayerLanded>
    {
        public Player Player { get; set; }  
        public override void Execute()
        {
            if (model.Player.audioSource && model.Player.landOnGroundAudio)
            {
                // Schedule landing audio through event system
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioSource = model.Player.audioSource;
                audioEvent.audioClip = model.Player.landOnGroundAudio;
                audioEvent.audioType = AudioEventType.Land;
            }
        }
    }
}