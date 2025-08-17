using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Common;
using NeonLadder.Debugging;
using System.Collections;
using System.Collections.Generic;
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
            
            // Debug logging
            Debugger.Log($"[PlayerAttackEvent] Executing attack. Current state: {playerAction.attackState}, IsRanged: {isRanged}");
            
            // CRITICAL: Player uses layered animation system
            // Step 1: Activate the action layer  
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1.0f);
            
            // Step 2: Set the action animation parameter
            int attackAnimation = isRanged ? 75 : 23; // 75 = ranged, 23 = melee
            player.Animator.SetInteger("action_animation", attackAnimation);
            
            // IMPORTANT: Transition from Preparing to Acting
            if (playerAction.attackState == ActionStates.Preparing)
            {
                playerAction.attackState = ActionStates.Acting;
                Debugger.Log($"[PlayerAttackEvent] Transitioned to Acting state");
            }
            else
            {
                Debugger.LogWarning($"[PlayerAttackEvent] Unexpected state during attack: {playerAction.attackState}");
                playerAction.attackState = ActionStates.Acting; // Force to Acting
            }
            
            // Schedule weapon collider activation for damage detection
            ScheduleWeaponColliderEvents(playerAction);
            
            // Schedule reset after actual animation duration (not hardcoded)
            float attackDuration = player.AttackAnimationDuration;
            if (attackDuration <= 0) attackDuration = 1.0f; // Fallback if not cached
            
            Debugger.Log($"[PlayerAttackEvent] Scheduling completion in {attackDuration}s");
            var completeEvent = Simulation.Schedule<PlayerAttackCompleteEvent>(attackDuration);
            completeEvent.player = player;
        }
        
        private void ScheduleWeaponColliderEvents(PlayerAction playerAction)
        {
            var attackComponents = playerAction.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                       .Where(c => c.gameObject != playerAction.transform.parent.gameObject).ToList();
            
            
            if (attackComponents != null && attackComponents.Count > 0)
            {
                // Get actual animation duration from cached values
                float baseDuration = player.AttackAnimationDuration;
                if (baseDuration <= 0) baseDuration = 1.0f; // Fallback if not cached
                
                // Calculate timing based on animation percentage
                float ignoreDuration = baseDuration * Constants.Animation.IgnorePercentage;
                
                // Schedule collider activation after windup
                var activateEvent = Simulation.Schedule<WeaponColliderActivateEvent>(ignoreDuration);
                activateEvent.attackComponents = attackComponents;
                
                // Schedule collider deactivation at end of animation
                var deactivateEvent = Simulation.Schedule<WeaponColliderDeactivateEvent>(baseDuration);
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
            
            // Ensure attack state is reset even if it got stuck
            playerAction.attackState = ActionStates.Ready;
            playerAction.stopAttack = false; // Clear any stuck stop flags
            
            // Reset action animation and deactivate layer
            player.Animator.SetInteger("action_animation", 0);
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 0.0f);
            
            // Ensure weapon colliders are reset to default layer
            var attackComponents = playerAction.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                .Where(c => c.gameObject != playerAction.transform.parent.gameObject).ToList();
            foreach (var collider in attackComponents)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("Battle"))
                {
                    collider.gameObject.layer = LayerMask.NameToLayer("Default");
                }
            }
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