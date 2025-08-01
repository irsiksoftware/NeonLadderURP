using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;

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
            
            // Set attack state
            playerAction.attackState = ActionStates.Acting;
            
            // Consume stamina
            player.Stamina.Decrement(staminaCost);
            
            // Set appropriate animation
            int attackAnimation = isRanged ? 75 : 23; // ranged vs melee
            player.Animator.SetInteger("attack_animation", attackAnimation);
            
            // Schedule attack completion
            float attackDuration = playerAction.AttackAnimationDuration;
            var completeEvent = Simulation.Schedule<PlayerAttackCompleteEvent>(attackDuration);
            completeEvent.player = player;
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
            
            // Reset animation
            player.Animator.SetInteger("attack_animation", 0);
        }
    }
}