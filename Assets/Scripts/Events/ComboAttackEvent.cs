using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
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
            
            // Set special animation for combo attacks
            int comboAnimation = GetComboAnimation(comboStep);
            player.Animator.SetInteger("attack_animation", comboAnimation);
            
            // Schedule attack validation with combo modifiers
            var attackEvent = Simulation.Schedule<EnemyAttackValidationEvent>(0.1f);
            // Apply combo damage multiplier
            
            // Schedule next input window
            if (HasNextComboStep())
            {
                var windowEvent = Simulation.Schedule<ComboWindowEvent>(0.3f);
                windowEvent.player = player;
                windowEvent.comboId = comboId;
                windowEvent.nextStep = comboStep + 1;
            }
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