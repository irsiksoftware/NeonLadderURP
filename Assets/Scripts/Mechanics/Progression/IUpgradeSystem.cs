using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Mechanics.Progression
{
    /// <summary>
    /// Core upgrade system interface - inspired by Hades Mirror of Night & Slay the Spire relics
    /// Handles both Meta (per-run) and Perma (persistent) upgrade trees
    /// </summary>
    public interface IUpgradeSystem
    {
        /// <summary>
        /// Purchase an upgrade using the appropriate currency
        /// Returns true if purchase successful, false if insufficient funds/already owned
        /// </summary>
        bool PurchaseUpgrade(string upgradeId, CurrencyType currencyType);
        
        /// <summary>
        /// Check if player can afford specific upgrade
        /// </summary>
        bool CanAffordUpgrade(string upgradeId, CurrencyType currencyType);
        
        /// <summary>
        /// Get all available upgrades for purchase (not yet owned)
        /// </summary>
        IEnumerable<IUpgrade> GetAvailableUpgrades(CurrencyType currencyType);
        
        /// <summary>
        /// Get all owned upgrades (active bonuses)
        /// </summary>
        IEnumerable<IUpgrade> GetOwnedUpgrades(CurrencyType currencyType);
        
        /// <summary>
        /// Check if specific upgrade is owned
        /// </summary>
        bool HasUpgrade(string upgradeId);
        
        /// <summary>
        /// Get upgrade by ID for inspection
        /// </summary>
        IUpgrade GetUpgrade(string upgradeId);
        
        /// <summary>
        /// Reset Meta upgrades (on death/run end)
        /// </summary>
        void ResetMetaUpgrades();
        
        /// <summary>
        /// Apply all owned upgrade effects to player
        /// </summary>
        void ApplyUpgradeEffects();
        
        /// <summary>
        /// Event fired when upgrade is purchased
        /// </summary>
        event System.Action<IUpgrade> OnUpgradePurchased;
    }
}