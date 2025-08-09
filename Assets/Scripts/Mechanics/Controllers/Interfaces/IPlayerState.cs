using UnityEngine;

namespace NeonLadder.Mechanics.Controllers.Interfaces
{
    /// <summary>
    /// Interface defining player state information to break circular dependency
    /// between Player and PlayerAction classes.
    /// This interface provides read-only access to player state.
    /// </summary>
    public interface IPlayerState
    {
        // Movement State
        Vector3 Velocity { get; }
        bool IsGrounded { get; }
        bool IsFacingLeft { get; }
        bool IsMovingInZDimension { get; }
        
        // Combat State
        bool IsUsingMelee { get; }
        float AttackAnimationDuration { get; }
        
        // Health & Resources
        bool IsAlive { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        float CurrentStamina { get; }
        float MaxStamina { get; }
        
        // Components
        Animator Animator { get; }
        Transform Transform { get; }
        
        // Z-Movement Control
        void EnableZMovement();
        void DisableZMovement();
    }
}