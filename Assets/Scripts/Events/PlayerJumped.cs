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
            if (model.player?.audioSource && model.player?.jumpAudio)
                model.player?.audioSource.PlayOneShot(model.player?.jumpAudio);
        }
    }
}