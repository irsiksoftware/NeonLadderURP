using NeonLadder.Core;
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
            model.Player.controlEnabled = false;
            model.Player.animator.SetInteger("locomotion_animation", 5);
            model.Player.animator.SetLayerWeight(model.Player.actionLayerIndex, 0); // Deactivate action layer
            model.Player.StartCoroutine(HandleDeathAnimation(model.Player));
            model.Player.controlEnabled = false;
            model.Player.playerActions.playerActionMap.Disable();
        }

        private IEnumerator HandleDeathAnimation(Player player)
        {
            yield return new WaitForSeconds(DeathAnimationDuration);
            player.animator.enabled = false;
        }
    }
}
