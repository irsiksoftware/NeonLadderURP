using NeonLadder.Mechanics.Controllers;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    public class PlayerDeath : BaseGameEvent<PlayerDeath>
    {
        float DeathAnimationDuration = 2.5f;
        public override void Execute()
        {
            model.player.controlEnabled = false;
            model.player.animator.SetInteger("animation", 5);
            model.player.StartCoroutine(HandleDeathAnimation(model.player));
        }

        private IEnumerator HandleDeathAnimation(Player player)
        {
            yield return new WaitForSeconds(DeathAnimationDuration);
            player.animator.enabled = false;

            // Optionally, you can schedule a respawn or other event here
            // Simulation.Schedule<PlayerSpawn>(2);
        }
    }
}
