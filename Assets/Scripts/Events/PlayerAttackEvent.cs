using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Common;
using NeonLadder.Debugging;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Basic player attack event for non-combo attacks
    /// </summary>
    public class PlayerAttackEvent : Simulation.Event
    {
        public Player player;
        public bool isRanged;
        public float staminaCost = 5f;

        public override bool Precondition()
        {
            return player != null && 
                   player.Health.IsAlive && 
                   player.Stamina.current >= staminaCost;
        }

        public override void Execute()
        {
            Debugger.Log("🔹 STEP 5: PlayerAttackEvent.Execute - ANIMATION AND COLLIDERS");
            
            var playerAction = player.GetComponent<PlayerAction>();
            
            // CRITICAL: Player uses layered animation system
            // Step 1: Activate the action layer  
            Debugger.Log($"Activating action layer {Constants.PlayerActionLayerIndex} with weight 1");
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1.0f);
            
            // Step 2: Set the action animation parameter
            int attackAnimation = isRanged ? 75 : 23; // 75 = ranged, 23 = melee
            string weaponType = isRanged ? "ranged" : "melee";
            Debugger.Log($"Setting action_animation to {attackAnimation} ({weaponType} attack)");
            player.Animator.SetInteger("action_animation", attackAnimation);
            
            // Simple state management
            playerAction.attackState = ActionStates.Acting;
            
            // Schedule weapon collider activation for damage detection
            ScheduleWeaponColliderEvents(playerAction);
            
            // Schedule reset after 2 seconds
            var completeEvent = Simulation.Schedule<PlayerAttackCompleteEvent>(2.0f);
            completeEvent.player = player;
            
            Debugger.Log("Action layer activated + weapon colliders scheduled - attack should be fully functional!");
        }
        
        private void ScheduleWeaponColliderEvents(PlayerAction playerAction)
        {
            var attackComponents = playerAction.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                       .Where(c => c.gameObject != playerAction.transform.parent.gameObject).ToList();
            
            Debugger.Log($"Found {attackComponents?.Count ?? 0} weapon colliders on player");
            
            if (attackComponents != null && attackComponents.Count > 0)
            {
                // Calculate the duration to ignore based on the percentage
                float ignoreDuration = playerAction.AttackAnimationDuration * Constants.Animation.IgnorePercentage;
                float totalDuration = playerAction.AttackAnimationDuration;

                Debugger.Log($"Scheduling collider activation after {ignoreDuration}s, deactivation after {totalDuration}s");

                // Schedule collider activation
                var activateEvent = Simulation.Schedule<WeaponColliderActivateEvent>(ignoreDuration);
                activateEvent.attackComponents = attackComponents;
                
                // Schedule collider deactivation
                var deactivateEvent = Simulation.Schedule<WeaponColliderDeactivateEvent>(totalDuration);
                deactivateEvent.attackComponents = attackComponents;
            }
            else
            {
                Debugger.LogWarning("No weapon colliders found - enemies won't take damage!");
            }
        }
    }

    /// <summary>
    /// Marks the end of an attack animation
    /// </summary>
    public class PlayerAttackCompleteEvent : Simulation.Event
    {
        public Player player;

        public override void Execute()
        {
            Debugger.Log("=== ATTACK COMPLETE - RESETTING LAYERS ===");
            
            var playerAction = player.GetComponent<PlayerAction>();
            playerAction.attackState = ActionStates.Ready;
            
            // Reset action animation and deactivate layer
            player.Animator.SetInteger("action_animation", 0);
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0.0f);
            
            Debugger.Log("Action layer deactivated - back to locomotion");
        }
    }

    /// <summary>
    /// Activates weapon colliders for attack detection
    /// </summary>
    public class WeaponColliderActivateEvent : Simulation.Event
    {
        public System.Collections.Generic.List<Collider> attackComponents;

        public override void Execute()
        {
            if (attackComponents != null)
            {
                Debugger.Log($"ACTIVATING {attackComponents.Count} weapon colliders to Battle layer");
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer("Battle");
                    Debugger.Log($"  - {attackComponent.gameObject.name} → Battle layer");
                }
            }
            else
            {
                Debugger.LogWarning("No weapon colliders found to activate!");
            }
        }
    }

    /// <summary>
    /// Deactivates weapon colliders after attack
    /// </summary>
    public class WeaponColliderDeactivateEvent : Simulation.Event
    {
        public System.Collections.Generic.List<Collider> attackComponents;

        public override void Execute()
        {
            if (attackComponents != null)
            {
                Debugger.Log($"DEACTIVATING {attackComponents.Count} weapon colliders back to Default layer");
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }
}