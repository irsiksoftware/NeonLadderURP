using NeonLadder.Core;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Events
{
    /// <summary>
    /// Captures and buffers player input through the simulation system
    /// Enables combo systems, buffered inputs during animations, and frame-perfect execution
    /// </summary>
    public class InputBufferEvent : Simulation.Event
    {
        public Player player;
        public InputType inputType;
        public InputAction.CallbackContext context;
        public float bufferWindow = 0.2f; // How long this input remains valid
        public int priority = 0; // Higher priority inputs can override lower ones
        
        // For directional inputs
        public Vector2 inputVector;
        
        // For combo tracking
        public int comboStep = 0;
        public string comboId = "";

        public override bool Precondition()
        {
            // Input is only valid if player is alive and within buffer window
            return player != null && 
                   player.Health.IsAlive && 
                   Time.time <= tick + bufferWindow;
        }

        public override void Execute()
        {
            // Try to consume this input
            var consumeEvent = Simulation.Schedule<InputConsumeEvent>(0f);
            consumeEvent.player = player;
            consumeEvent.bufferedInput = this;
        }

        internal override void Cleanup()
        {
            player = null;
            context = default;
        }
    }

    /// <summary>
    /// Attempts to consume a buffered input and execute the appropriate action
    /// </summary>
    public class InputConsumeEvent : Simulation.Event
    {
        public Player player;
        public InputBufferEvent bufferedInput;

        public override bool Precondition()
        {
            if (player == null || bufferedInput == null) return false;
            
            // Check if we can consume this input based on current state
            switch (bufferedInput.inputType)
            {
                case InputType.Jump:
                    return CanJump();
                case InputType.Attack:
                    return CanAttack();
                case InputType.Sprint:
                    return CanSprint();
                case InputType.WeaponSwap:
                    return CanWeaponSwap();
                default:
                    return true;
            }
        }

        public override void Execute()
        {
            switch (bufferedInput.inputType)
            {
                case InputType.Jump:
                    ExecuteJump();
                    break;
                case InputType.Attack:
                    ExecuteAttack();
                    break;
                case InputType.Sprint:
                    ExecuteSprint();
                    break;
                case InputType.Move:
                    ExecuteMove();
                    break;
                case InputType.WeaponSwap:
                    ExecuteWeaponSwap();
                    break;
            }
            
            // Check for combo continuation
            CheckCombos();
        }

        private bool CanJump()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            return player.IsGrounded || playerAction.JumpCount < playerAction.MaxJumps;
        }

        private bool CanAttack()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            return !player.Stamina.IsExhausted && 
                   playerAction.attackState == ActionStates.Ready;
        }

        private bool CanSprint()
        {
            return !player.Stamina.IsExhausted && 
                   player.velocity.x != 0;
        }

        private bool CanWeaponSwap()
        {
            // Can swap weapons anytime except mid-attack
            var playerAction = player.GetComponent<PlayerAction>();
            return playerAction.attackState != ActionStates.Acting;
        }

        private void ExecuteJump()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            if (playerAction != null)
            {
                // Set the jump flag that ComputeVelocity checks
                playerAction.isJumping = true;
                // The actual jump will be handled by Player.ComputeVelocity -> ScheduleJump
            }
        }

        private void ExecuteAttack()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            
            if (playerAction != null && playerAction.attackState == ActionStates.Ready)
            {
                // Set attack state to preparing
                playerAction.attackState = ActionStates.Preparing;
                playerAction.stopAttack = false;
                
                // Check if this is part of a combo
                if (IsComboAttack())
                {
                    var comboEvent = Simulation.Schedule<ComboAttackEvent>(0f);
                    comboEvent.player = player;
                    comboEvent.comboStep = bufferedInput.comboStep + 1;
                    comboEvent.comboId = DetermineComboId();
                }
                else
                {
                    var attackEvent = Simulation.Schedule<PlayerAttackEvent>(0f);
                    attackEvent.player = player;
                    attackEvent.isRanged = !player.IsUsingMelee;
                }
            }
        }

        private void ExecuteSprint()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            if (playerAction != null && playerAction.sprintState == ActionStates.Ready)
            {
                // Set sprint state to preparing - UpdateSprintState will handle the rest
                playerAction.sprintState = ActionStates.Preparing;
                playerAction.stopSprint = false;
                // IsSprinting is read-only and will automatically become true when sprintState changes to Acting
            }
        }

        private void ExecuteMove()
        {
            // Movement is typically applied directly, but we can still track it
            var playerAction = player.GetComponent<PlayerAction>();
            playerAction.playerInput = bufferedInput.inputVector;
        }

        private void ExecuteWeaponSwap()
        {
            // Trigger weapon swap through PlayerAction component
            var playerAction = player.GetComponent<PlayerAction>();
            if (playerAction != null)
            {
                // GUARD: Don't allow weapon swap during attack
                if (playerAction.attackState == ActionStates.Acting || playerAction.attackState == ActionStates.Preparing)
                {
                    return; // Block weapon swap during attack sequence
                }
                
                // Swap the weapon state
                player.IsUsingMelee = !player.IsUsingMelee;
                
                // Handle the visual weapon swap
                if (player.IsUsingMelee)
                {
                    // Switch from ranged to melee
                    SwapWeaponGroups(playerAction.rangedWeaponGroups, playerAction.meleeWeaponGroups);
                }
                else
                {
                    // Switch from melee to ranged
                    SwapWeaponGroups(playerAction.meleeWeaponGroups, playerAction.rangedWeaponGroups);
                }
            }
        }
        
        private void SwapWeaponGroups(List<GameObject> currentWeapons, List<GameObject> newWeapons)
        {
            if (currentWeapons != null)
            {
                foreach (GameObject weaponGroup in currentWeapons)
                {
                    if (weaponGroup != null && weaponGroup.transform.childCount > 0)
                    {
                        var weapon = weaponGroup.transform.GetChild(0).gameObject;
                        weapon.SetActive(false);
                    }
                }
            }
            
            if (newWeapons != null)
            {
                foreach (GameObject weaponGroup in newWeapons)
                {
                    if (weaponGroup != null && weaponGroup.transform.childCount > 0)
                    {
                        var weapon = weaponGroup.transform.GetChild(0).gameObject;
                        weapon.SetActive(true);
                    }
                }
            }
        }

        private bool IsComboAttack()
        {
            // Check if we're within combo window of last attack
            var comboSystem = Simulation.GetModel<ComboSystem>();
            return comboSystem != null && comboSystem.IsInComboWindow(player);
        }

        private string DetermineComboId()
        {
            // Build combo ID based on input sequence
            return $"{bufferedInput.comboId}_ATK";
        }

        private void CheckCombos()
        {
            var comboSystem = Simulation.GetModel<ComboSystem>();
            comboSystem?.CheckComboCompletion(player, bufferedInput);
        }

        internal override void Cleanup()
        {
            player = null;
            bufferedInput = null;
        }
    }

    public enum InputType
    {
        Move,
        Jump,
        Attack,
        Sprint,
        WeaponSwap,
        Special1,
        Special2
    }
}