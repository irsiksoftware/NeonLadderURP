using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for centralized enemy animation management
    /// Coordinates animation states with AI behavior
    /// </summary>
    public class EnemyAnimationEvent : Simulation.Event
    {
        public Enemy enemy;
        public EnemyAnimationType animationType;

        public override bool Precondition()
        {
            return enemy != null && enemy.Animator != null;
        }

        public override void Execute()
        {
            if (enemy?.Animator != null)
            {
                int animationId = GetAnimationIdForType(animationType);
                enemy.Animator.SetInteger("animation", animationId);
            }
        }

        private int GetAnimationIdForType(EnemyAnimationType type)
        {
            return type switch
            {
                EnemyAnimationType.Idle => 1,
                EnemyAnimationType.Walk => 2,
                EnemyAnimationType.Attack => 3,
                EnemyAnimationType.Death => 5,
                _ => 1
            };
        }

        internal override void Cleanup()
        {
            enemy = null;
        }
    }

    /// <summary>
    /// Types of animations for enemy characters
    /// </summary>
    public enum EnemyAnimationType
    {
        Idle,
        Walk,
        Attack,
        Death
    }
}