using NeonLadder.Core;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Handles weapon swapping as a proper Simulation Event
    /// This ensures proper state management and prevents race conditions
    /// </summary>
    public class WeaponSwapEvent : Simulation.Event
    {
        public Player player;
        public bool forceSwap = false; // Allow forced swap even during attacks (for special abilities)
        public int retryCount = 0; // Track retry attempts to prevent infinite loops
        private const int MaxRetries = 3; // Maximum retry attempts

        public override bool Precondition()
        {
            if (player == null || !player.Health.IsAlive) return false;
            
            var playerAction = player.GetComponent<PlayerAction>();
            if (playerAction == null) return false;
            
            // If not forcing, check if we can swap
            if (!forceSwap)
            {
                // Don't allow swap during attack unless forced
                if (playerAction.attackState == ActionStates.Acting || 
                    playerAction.attackState == ActionStates.Preparing)
                {
                    // Only retry if we haven't exceeded max attempts
                    if (retryCount < MaxRetries)
                    {
                        Debugger.Log($"[WeaponSwap] Blocked: Attack in progress (state: {playerAction.attackState}). Retry {retryCount + 1}/{MaxRetries}");
                        
                        // Schedule a retry after attack should complete
                        var retrySwap = Simulation.Schedule<WeaponSwapEvent>(0.5f);
                        retrySwap.player = player;
                        retrySwap.forceSwap = false;
                        retrySwap.retryCount = retryCount + 1;
                    }
                    else
                    {
                        Debugger.LogWarning($"[WeaponSwap] Max retries exceeded. Attack state stuck at: {playerAction.attackState}. Forcing reset.");
                        // Force reset the attack state if we've retried too many times
                        playerAction.attackState = ActionStates.Ready;
                        playerAction.stopAttack = false;
                    }
                    
                    return false;
                }
            }
            
            return true;
        }

        public override void Execute()
        {
            var playerAction = player.GetComponent<PlayerAction>();
            if (playerAction == null) 
            {
                Debugger.LogError("[WeaponSwap] PlayerAction component not found!");
                return;
            }
            
            // Log current state for debugging
            bool wasUsingMelee = player.IsUsingMelee;
            Debugger.Log($"[WeaponSwap] Starting swap: {(wasUsingMelee ? "Melee" : "Ranged")} -> {(!wasUsingMelee ? "Melee" : "Ranged")}");
            
            // Safety check: Reset stuck attack states
            if (playerAction.attackState == ActionStates.Acted)
            {
                Debugger.LogWarning("[WeaponSwap] Found stuck 'Acted' state, resetting to Ready");
                playerAction.attackState = ActionStates.Ready;
            }
            
            // Perform the swap
            player.IsUsingMelee = !player.IsUsingMelee;
            
            // Schedule visual update event
            var visualEvent = Simulation.Schedule<WeaponSwapVisualEvent>(0f);
            visualEvent.player = player;
            visualEvent.playerAction = playerAction;
            visualEvent.showMelee = player.IsUsingMelee;
            
            Debugger.Log($"[WeaponSwap] Swap complete. Now using: {(player.IsUsingMelee ? "Melee" : "Ranged")}");
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }

    /// <summary>
    /// Handles the visual aspect of weapon swapping
    /// Separated to ensure proper sequencing
    /// </summary>
    public class WeaponSwapVisualEvent : Simulation.Event
    {
        public Player player;
        public PlayerAction playerAction;
        public bool showMelee;

        public override void Execute()
        {
            Debugger.Log($"[WeaponSwapVisual] Updating visuals to show {(showMelee ? "Melee" : "Ranged")} weapons");
            
            if (showMelee)
            {
                // Hide ranged, show melee
                SetWeaponGroupsActive(playerAction.rangedWeaponGroups, false);
                SetWeaponGroupsActive(playerAction.meleeWeaponGroups, true);
            }
            else
            {
                // Hide melee, show ranged
                SetWeaponGroupsActive(playerAction.meleeWeaponGroups, false);
                SetWeaponGroupsActive(playerAction.rangedWeaponGroups, true);
            }
            
            // Ensure colliders are in correct state (not Battle layer)
            ResetWeaponColliders();
        }

        private void SetWeaponGroupsActive(List<GameObject> weaponGroups, bool active)
        {
            if (weaponGroups == null) 
            {
                Debugger.LogWarning($"[WeaponSwapVisual] Weapon group list is null");
                return;
            }
            
            int count = 0;
            foreach (GameObject weaponGroup in weaponGroups)
            {
                if (weaponGroup != null && weaponGroup.transform.childCount > 0)
                {
                    var weapon = weaponGroup.transform.GetChild(0).gameObject;
                    weapon.SetActive(active);
                    count++;
                }
            }
            
            Debugger.Log($"[WeaponSwapVisual] Set {count} weapon groups to {(active ? "active" : "inactive")}");
        }

        private void ResetWeaponColliders()
        {
            // Ensure all weapon colliders are reset to Default layer
            var attackComponents = playerAction.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                .Where(c => c.gameObject != playerAction.transform.parent.gameObject).ToList();
            
            int resetCount = 0;
            foreach (var collider in attackComponents)
            {
                if (collider.gameObject.layer == LayerMask.NameToLayer("Battle"))
                {
                    collider.gameObject.layer = LayerMask.NameToLayer("Default");
                    resetCount++;
                }
            }
            
            if (resetCount > 0)
            {
                Debugger.LogWarning($"[WeaponSwapVisual] Reset {resetCount} colliders from Battle to Default layer");
            }
        }

        internal override void Cleanup()
        {
            player = null;
            playerAction = null;
        }
    }
}