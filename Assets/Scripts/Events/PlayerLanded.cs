using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class PlayerLanded : BaseGameEvent<PlayerLanded>
    {
        public override void Execute()
        {
            if (model.player.audioSource && model.player.landOnGroundAudio)
                model.player.audioSource.PlayOneShot(model.player.landOnGroundAudio);
        }
    }
}