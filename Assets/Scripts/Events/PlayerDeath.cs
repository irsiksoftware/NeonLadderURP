using NeonLadder.Common;
using NeonLadder.Mechanics.Enums;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NeonLadder.Core.Simulation;

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
            model.Player.animator.SetInteger("action_animation", 0);
            model.Player.animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0); // Deactivate action layer
            model.Player.animator.SetInteger("locomotion_animation", 5);
            model.Player.StartCoroutine(HandleDeathAnimation());
            model.Player.Actions.playerActionMap.Disable();
            SceneManager.activeSceneChanged -= OnSceneChange;
            SceneManager.activeSceneChanged += OnSceneChange;
        }

        private void OnSceneChange(Scene arg0, Scene arg1)
        {
            Schedule<PlayerSpawn>(2);
        }

        private IEnumerator HandleDeathAnimation()
        {
            yield return new WaitForSecondsRealtime(model.Player.DeathAnimationDuration);
            SceneManager.LoadScene(Scenes.Staging.ToString());
           
        }
    }
}
