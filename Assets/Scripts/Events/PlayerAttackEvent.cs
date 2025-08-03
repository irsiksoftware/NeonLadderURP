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
            var playerAction = player.GetComponent<PlayerAction>();
            
            // CRITICAL: Player uses layered animation system
            // Step 1: Activate the action layer  
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1.0f);
            
            // Step 2: Set the action animation parameter
            int attackAnimation = isRanged ? 75 : 23; // 75 = ranged, 23 = melee
            player.Animator.SetInteger("action_animation", attackAnimation);
            
            // Simple state management
            playerAction.attackState = ActionStates.Acting;
            
            // Schedule weapon collider activation for damage detection
            ScheduleWeaponColliderEvents(playerAction);
            
            // Schedule reset after 2 seconds
            var completeEvent = Simulation.Schedule<PlayerAttackCompleteEvent>(2.0f);
            completeEvent.player = player;
        }
        
        private void ScheduleWeaponColliderEvents(PlayerAction playerAction)
        {
            var attackComponents = playerAction.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                       .Where(c => c.gameObject != playerAction.transform.parent.gameObject).ToList();
            
            
            if (attackComponents != null && attackComponents.Count > 0)
            {
                // Calculate the duration to ignore based on the percentage
                float baseDuration = Mathf.Max(playerAction.AttackAnimationDuration, 1.0f); // Ensure minimum 1 second
                float ignoreDuration = baseDuration * Constants.Animation.IgnorePercentage;
                float totalDuration = baseDuration;


                // Schedule collider activation
                var activateEvent = Simulation.Schedule<WeaponColliderActivateEvent>(ignoreDuration);
                activateEvent.attackComponents = attackComponents;
                
                // Schedule collider deactivation
                var deactivateEvent = Simulation.Schedule<WeaponColliderDeactivateEvent>(totalDuration);
                deactivateEvent.attackComponents = attackComponents;
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
            var playerAction = player.GetComponent<PlayerAction>();
            playerAction.attackState = ActionStates.Ready;
            
            // Reset action animation and deactivate layer
            player.Animator.SetInteger("action_animation", 0);
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0.0f);
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
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer("Battle");
                }
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
                foreach (var attackComponent in attackComponents)
                {
                    attackComponent.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
        }
    }
}