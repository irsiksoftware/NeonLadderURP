using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
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
                // Schedule respawn audio through event system
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioSource = model.Player.audioSource;
                audioEvent.audioClip = model.Player.respawnAudio;
                audioEvent.audioType = AudioEventType.Respawn;
            }

            // Schedule health and stamina restoration through event system
            model.Player.ScheduleHealing(model.Player.Health.max, 0f);
            model.Player.ScheduleStaminaDamage(-model.Player.Stamina.max, 0f); // Negative damage = healing

            // Start the coroutine to reset the Animator - THIS MUST be done inside of a coroutine for WebGL to function properly.
            model.Player.StartCoroutine(ResetAnimatorDelayed(model.Player));
        }

        private IEnumerator ResetAnimatorDelayed(Player player)
        {
            yield return null; // Wait for one frame

            player.Animator.enabled = true;
            player.Animator.SetInteger(nameof(PlayerAnimationLayers.locomotion_animation), 777);
            var mediator = player.GetComponent<PlayerStateMediator>();
            mediator?.EnablePlayerActionMap();
        }
    }
}
