using NeonLadder.Mechanics.Controllers;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player performs a Jump.
    /// </summary>
    /// <typeparam name="PlayerJumped"></typeparam>
    public class PlayerJumped : BaseGameEvent<PlayerJumped>
    {
        public override void Execute()
        {
            if (model.Player?.audioSource && model.Player?.jumpAudio)
            {
                // Schedule jump audio through event system
                var audioEvent = Schedule<AudioEvent>(0f);
                audioEvent.audioSource = model.Player.audioSource;
                audioEvent.audioClip = model.Player.jumpAudio;
                audioEvent.audioType = AudioEventType.Jump;
            }
        }
    }
}