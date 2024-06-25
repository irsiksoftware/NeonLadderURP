using NeonLadder.Common;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : BaseGameEvent<PlayerSpawn>
    {
        public override void Execute()
        {
            if (model.Player.audioSource && model.Player.respawnAudio)
            {
                model.Player.audioSource.PlayOneShot(model.Player.respawnAudio);
            }

            model.Player.Health.Increment(model.Player.Health.max);
            model.Player.Stamina.Increment(model.Player.Stamina.max);
            model.Player.animator.enabled = true;
            model.Player.animator.SetInteger("locomotion_animation", 777);
            model.Player.Actions.playerActionMap.Enable();
            model.Player.controlEnabled = true;
        }
    }
}