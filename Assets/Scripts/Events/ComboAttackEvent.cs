using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Common;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Special attack event for combo system
    /// Handles multi-hit attacks with different properties per hit
    /// </summary>
    public class ComboAttackEvent : Simulation.Event
    {
        public Player player;
        public int comboStep;
        public string comboId;
        public float damageMultiplier = 1.0f;
        public float knockbackMultiplier = 1.0f;

        public override bool Precondition()
        {
            return player != null && 
                   player.Health.IsAlive && 
                   !player.Stamina.IsExhausted;
        }

        public override void Execute()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            
            Debug.Log($"[ComboAttackEvent] Executing combo attack: {comboId}, step {comboStep}");
            
            // CRITICAL: Player uses layered animation system  
            // Step 1: Activate the action layer
            player.Animator.SetLayerWeight(Constants.PlayerActionLayerIndex, 1.0f);
            
            // Step 2: Set special animation for combo attacks
            int comboAnimation = GetComboAnimation(comboStep);
            player.Animator.SetInteger("action_animation", comboAnimation);
            
            // IMPORTANT: Transition from Preparing to Acting
            if (playerAction.attackState == ActionStates.Preparing)
            {
                playerAction.attackState = ActionStates.Acting;
                Debug.Log($"[ComboAttackEvent] Transitioned to Acting state");
            }
            
            // Apply combo damage multiplier for this hit
            // TODO: Implement combo damage application
            
            // Schedule completion after animation duration
            float attackDuration = player.AttackAnimationDuration;
            if (attackDuration <= 0) attackDuration = 1.0f; // Fallback
            
            var completeEvent = Simulation.Schedule<PlayerAttackCompleteEvent>(attackDuration);
            completeEvent.player = player;
            
            // REMOVED: Automatic combo window scheduling
            // Combo windows are now only created by successful combo matches in ComboSystem
        }

        private int GetComboAnimation(int step)
        {
            // Return different animations based on combo step
            return step switch
            {
                1 => 23,  // First attack
                2 => 24,  // Second attack
                3 => 25,  // Finisher
                _ => 23
            };
        }

        private bool HasNextComboStep()
        {
            // Check if this combo has more steps
            var comboSystem = Simulation.GetModel<ComboSystem>();
            // Would check combo definition for max steps
            return comboStep < 3; // Simplified for example
        }
    }

    /// <summary>
    /// Manages combo input windows
    /// </summary>
    public class ComboWindowEvent : Simulation.Event
    {
        public Player player;
        public string comboId;
        public int nextStep;
        public float windowDuration = 0.5f;

        public override void Execute()
        {
            // This event marks when the next combo input can be accepted
            Debug.Log($"Combo window open for {comboId} step {nextStep}");
        }
    }
}