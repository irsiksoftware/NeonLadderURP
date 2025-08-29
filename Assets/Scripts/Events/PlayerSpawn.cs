using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : BaseGameEvent<PlayerSpawn>
    {
        public Vector3? spawnPosition;
        
        public override void Execute()
        {
            // Disable Z movement to restore X/Y movement and stop auto-walk
            model.Player.DisableZMovement();
            
            // Zero out velocity to stop any residual movement
            model.Player.velocity = Vector3.zero;
            model.Player.TargetVelocity = Vector3.zero;

            if (spawnPosition.HasValue)
            {
                var rigidbody = model.Player.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
                
                model.Player.velocity = Vector2.zero;
                model.Player.Teleport(spawnPosition.Value);
            }
            
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
            
            // Auto-save checkpoint at successful spawn location
            if (spawnPosition.HasValue && CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.OnPlayerSpawnCompleted(spawnPosition.Value);
            }
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
