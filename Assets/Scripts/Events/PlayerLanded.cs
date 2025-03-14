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
                model.Player.audioSource.PlayOneShot(model.Player.landOnGroundAudio);
        }
    }
}