using NeonLadder.Mechanics.Controllers;
using System.Collections;

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

            // Start the coroutine to reset the Animator - THIS MUST be done inside of a coroutine for WebGL to function properly.
            model.Player.StartCoroutine(ResetAnimatorDelayed(model.Player));
        }

        private IEnumerator ResetAnimatorDelayed(Player player)
        {
            yield return null; // Wait for one frame

            player.animator.enabled = true;
            player.animator.SetInteger("locomotion_animation", 777);
            player.Actions.playerActionMap.Enable();
        }
    }
}
