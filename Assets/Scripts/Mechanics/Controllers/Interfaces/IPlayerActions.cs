using UnityEngine;

namespace NeonLadder.Mechanics.Controllers.Interfaces
{
    /// <summary>
    /// Interface defining player action inputs and states to break circular dependency.
    /// This provides a contract for action-related data without coupling to concrete implementation.
    /// </summary>
    public interface IPlayerActions
    {
        // Input State
        Vector2 PlayerInput { get; }
        bool IsJumping { get; }
        bool IsClimbing { get; }
        bool? IsSprinting { get; }
        
        // Jump State
        int JumpCount { get; }
        int MaxJumps { get; }
        float JumpForce { get; }
        
        // Combat State
        bool IsAttacking { get; }
        float AttackAnimationDuration { get; }
        
        // Action Methods
        void ResetJumpCount();
        void ScheduleJump(float delay);
        void StartAttack();
        void StopAttack();
        void StartSprint();
        void StopSprint();
    }
}