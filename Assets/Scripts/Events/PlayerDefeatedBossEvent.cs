using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.ProceduralGeneration;
using UnityEngine;

namespace NeonLadder.Events
{
    public class PlayerDefeatedBossEvent : BaseGameEvent<PlayerDefeatedBossEvent>
    {
        public override void Execute()
        {

            // 1. Disable player controls
            var player = Game.Instance?.model?.Player;
            if (player != null)
            {
                Debug.Log("[PlayerDefeatedBossEvent] Found player, disabling controls");
                var playerMediator = player.GetComponent<PlayerStateMediator>();
                if (playerMediator != null)
                {
                    playerMediator.DisablePlayerActionMap();
                    Debug.Log("[PlayerDefeatedBossEvent] Player controls disabled successfully");
                }
                else
                {
                    Debug.LogWarning("[PlayerDefeatedBossEvent] PlayerStateMediator not found on player");
                }
            }
            else
            {
                Debug.LogError("[PlayerDefeatedBossEvent] Player not found!");
                return;
            }

            //move the plyer to the right a hair to face the correct direction.
            player.IsFacingLeft = false;
            player.Orient();

            // 2. Set player dancing animation (locomotion_animation = 9044)
            var playerAnimator = player.transform.parent.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetInteger("locomotion_animation", 9044);
                Debug.Log("[PlayerDefeatedBossEvent] Player victory dance animation started (9044)");
            }
            else
            {
                Debug.LogWarning("[PlayerDefeatedBossEvent] Player Animator not found");
            }

            // 3. Schedule scene transition after 5 seconds using Simulation system
            Debug.Log("[PlayerDefeatedBossEvent] Scheduling scene transition to ReturnToStaging in 5 seconds");
            var transitionEvent = Simulation.Schedule<ReturnToStagingEvent>(5f);
        }
    }
}