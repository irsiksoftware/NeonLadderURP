using UnityEngine;
using NeonLadder.Mechanics.Controllers.Interfaces;
using System;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// Mediator pattern implementation to decouple Player and PlayerAction classes.
    /// This eliminates circular dependencies by managing communication between components.
    /// </summary>
    public class PlayerStateMediator : MonoBehaviour
    {
        private Player player;
        private PlayerAction playerAction;
        
        // Events for decoupled communication
        public event Action<Vector3> OnVelocityChanged;
        public event Action<bool> OnGroundedStateChanged;
        public event Action<float> OnStaminaConsumed;
        public event Action OnJumpRequested;
        public event Action OnAttackRequested;
        public event Action<bool> OnSprintStateChanged;
        
        private void Awake()
        {
            // Get components
            player = GetComponent<Player>();
            playerAction = GetComponent<PlayerAction>();
            
            if (player == null)
            {
                Debug.LogError("PlayerStateMediator: Player component not found!");
            }
            
            if (playerAction == null)
            {
                Debug.LogError("PlayerStateMediator: PlayerAction component not found!");
            }
        }
        
        private void Start()
        {
            // Initialize both components with mediator
            if (player != null && playerAction != null)
            {
                InitializePlayerWithMediator();
                InitializePlayerActionWithMediator();
            }
        }
        
        private void InitializePlayerWithMediator()
        {
            // Player will use mediator to get action inputs instead of direct reference
            player.SetMediator(this);
        }
        
        private void InitializePlayerActionWithMediator()
        {
            // PlayerAction will use mediator to access player state
            playerAction.SetMediator(this);
        }
        
        #region Player State Access (for PlayerAction)
        
        /// <summary>
        /// Gets current player velocity without direct Player reference
        /// </summary>
        public Vector3 GetPlayerVelocity()
        {
            return player != null ? player.velocity : Vector3.zero;
        }
        
        /// <summary>
        /// Gets player grounded state
        /// </summary>
        public bool IsPlayerGrounded()
        {
            return player != null && player.IsGrounded;
        }
        
        /// <summary>
        /// Gets player health status
        /// </summary>
        public bool IsPlayerAlive()
        {
            return player != null && player.Health != null && player.Health.IsAlive;
        }
        
        /// <summary>
        /// Gets player animator
        /// </summary>
        public Animator GetPlayerAnimator()
        {
            return player?.Animator;
        }
        
        /// <summary>
        /// Gets player transform
        /// </summary>
        public Transform GetPlayerTransform()
        {
            return player?.transform;
        }
        
        /// <summary>
        /// Gets if player is using melee weapon
        /// </summary>
        public bool IsUsingMelee()
        {
            return player != null && player.IsUsingMelee;
        }
        
        /// <summary>
        /// Gets attack animation duration
        /// </summary>
        public float GetAttackAnimationDuration()
        {
            return player != null ? player.AttackAnimationDuration : 1.0f;
        }
        
        /// <summary>
        /// Enables Z-axis movement for the player
        /// </summary>
        public void EnablePlayerZMovement()
        {
            player?.EnableZMovement();
        }
        
        /// <summary>
        /// Schedules stamina damage for the player
        /// </summary>
        public void SchedulePlayerStaminaDamage(float amount, float delay)
        {
            player?.ScheduleStaminaDamage(amount, delay);
        }
        
        #endregion
        
        #region Action State Access (for Player)
        
        /// <summary>
        /// Gets current player input from actions
        /// </summary>
        public Vector2 GetPlayerInput()
        {
            return playerAction != null ? playerAction.playerInput : Vector2.zero;
        }
        
        /// <summary>
        /// Gets if player is sprinting
        /// </summary>
        public bool IsPlayerSprinting()
        {
            return playerAction != null && (playerAction.IsSprinting ?? false);
        }
        
        /// <summary>
        /// Gets if jump is requested
        /// </summary>
        public bool IsJumpRequested()
        {
            return playerAction != null && playerAction.isJumping;
        }
        
        /// <summary>
        /// Gets current jump count
        /// </summary>
        public int GetJumpCount()
        {
            return playerAction != null ? playerAction.JumpCount : 0;
        }
        
        /// <summary>
        /// Resets jump count when grounded
        /// </summary>
        public void ResetJumpCount()
        {
            playerAction?.ResetJumpCount();
        }
        
        /// <summary>
        /// Schedules a jump action
        /// </summary>
        public void ScheduleJump(float delay)
        {
            playerAction?.ScheduleJump(delay);
        }
        
        #endregion
        
        #region Velocity Update (Mediator Pattern)
        
        /// <summary>
        /// Updates player velocity through mediator to avoid direct reference
        /// </summary>
        public void UpdatePlayerVelocity(ref Vector3 velocity)
        {
            if (playerAction != null)
            {
                // This method will be called by Player to update velocity
                // PlayerAction can modify it without direct Player reference
                playerAction.UpdateSprintState(ref velocity);
            }
        }
        
        #endregion
    }
}