using NeonLadder.Common;
using NeonLadder.Mechanics.Enums;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    public class PlayerDeath : BaseGameEvent<PlayerDeath>
    {
        public override void Execute()
        {
            //model.Player.controlEnabled = false;
            model.Player.Animator.SetInteger("action_animation", 0);
            model.Player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0); // Deactivate action layer
            model.Player.Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), 5);
            model.Player.MetaCurrency.Deplete();
            model.Player.Stamina.Deplete();
            model.Player.Actions.playerActionMap.Disable();
            model.Player.StartCoroutine(HandleDeathAnimation());
        }

        private IEnumerator HandleDeathAnimation()
        {
            yield return new WaitForSecondsRealtime(model.Player.DeathAnimationDuration);
            SceneManager.LoadScene(Scenes.ReturnToStaging.ToString());
        }
    }
}
