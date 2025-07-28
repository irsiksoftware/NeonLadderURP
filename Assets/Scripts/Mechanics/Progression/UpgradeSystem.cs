using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Mechanics.Progression
{
    /// <summary>
    /// Core upgrade system implementation
    /// Handles both Meta (per-run) and Perma (persistent) upgrade trees
    /// Wade's Note: "The secret sauce that makes players go 'just one more upgrade!'"
    /// Stephen's Note: "The mystical engine of progression mastery."
    /// </summary>
    public class UpgradeSystem : MonoBehaviour, IUpgradeSystem
    {
        [Header("Upgrade Configuration")]
        [SerializeField] private UpgradeData[] availableUpgrades;
        
        private Player player;
        private Dictionary<string, IUpgrade> upgradeRegistry;
        private HashSet<string> ownedUpgrades;
        
        public event System.Action<IUpgrade> OnUpgradePurchased;
        
        void Awake()
        {
            InitializeSystem();
        }
        
        private void InitializeSystem()
        {
            player = GetComponent<Player>();
            upgradeRegistry = new Dictionary<string, IUpgrade>();
            ownedUpgrades = new HashSet<string>();
            
            // Register all available upgrades
            foreach (var upgrade in availableUpgrades)
            {
                if (!upgradeRegistry.ContainsKey(upgrade.Id))
                {
                    upgradeRegistry[upgrade.Id] = upgrade;
                }
                else
                {
                    Debug.LogWarning($"Duplicate upgrade ID detected: {upgrade.Id}");
                }
            }
        }
        
        #region IUpgradeSystem Implementation
        
        public bool PurchaseUpgrade(string upgradeId, CurrencyType currencyType)
        {
            var upgrade = GetUpgrade(upgradeId);
            if (upgrade == null)
            {
                Debug.LogWarning($"Upgrade not found: {upgradeId}");
                return false;
            }
            
            // Validate currency type matches
            if (upgrade.CurrencyType != currencyType)
            {
                Debug.LogWarning($"Currency type mismatch for upgrade {upgradeId}. Expected {upgrade.CurrencyType}, got {currencyType}");
                return false;
            }
            
            // Check if already at max level
            if (upgrade.IsMaxLevel)
            {
                Debug.LogWarning($"Upgrade {upgradeId} is already at max level");
                return false;
            }
            
            // Check affordability
            if (!CanAffordUpgrade(upgradeId, currencyType))
            {
                return false;
            }
            
            // Check prerequisites
            if (!ArePrerequisitesMet(upgrade))
            {
                Debug.LogWarning($"Prerequisites not met for upgrade {upgradeId}");
                return false;
            }
            
            // Check mutual exclusions
            if (HasMutuallyExclusiveUpgrade(upgrade))
            {
                Debug.LogWarning($"Mutually exclusive upgrade already owned for {upgradeId}");
                return false;
            }
            
            // Deduct currency
            if (!DeductCurrency(upgrade.Cost, currencyType))
            {
                return false;
            }
            
            // Apply upgrade
            upgrade.CurrentLevel++;
            ownedUpgrades.Add(upgradeId);
            
            // Apply effects immediately
            upgrade.ApplyEffect(gameObject);
            
            // Fire event
            OnUpgradePurchased?.Invoke(upgrade);
            
            Debug.Log($"Purchased upgrade: {upgrade.Name} (Level {upgrade.CurrentLevel})");
            return true;
        }
        
        public bool CanAffordUpgrade(string upgradeId, CurrencyType currencyType)
        {
            var upgrade = GetUpgrade(upgradeId);
            if (upgrade == null) return false;
            
            var currency = GetCurrencyComponent(currencyType);
            return currency != null && currency.current >= upgrade.Cost;
        }
        
        public IEnumerable<IUpgrade> GetAvailableUpgrades(CurrencyType currencyType)
        {
            return upgradeRegistry.Values
                .Where(u => u.CurrencyType == currencyType)
                .Where(u => !u.IsMaxLevel)
                .Where(u => ArePrerequisitesMet(u))
                .Where(u => !HasMutuallyExclusiveUpgrade(u));
        }
        
        public IEnumerable<IUpgrade> GetOwnedUpgrades(CurrencyType currencyType)
        {
            return upgradeRegistry.Values
                .Where(u => u.CurrencyType == currencyType)
                .Where(u => u.IsOwned);
        }
        
        public bool HasUpgrade(string upgradeId)
        {
            return ownedUpgrades.Contains(upgradeId);
        }
        
        public IUpgrade GetUpgrade(string upgradeId)
        {
            upgradeRegistry.TryGetValue(upgradeId, out var upgrade);
            return upgrade;
        }
        
        public void ResetMetaUpgrades()
        {
            var metaUpgrades = upgradeRegistry.Values
                .Where(u => u.CurrencyType == CurrencyType.Meta)
                .Where(u => u.IsOwned)
                .ToList();
            
            foreach (var upgrade in metaUpgrades)
            {
                // Remove effects
                upgrade.RemoveEffect(gameObject);
                
                // Reset level
                upgrade.CurrentLevel = 0;
                
                // Remove from owned set
                ownedUpgrades.Remove(upgrade.Id);
            }
            
            Debug.Log($"Reset {metaUpgrades.Count} Meta upgrades");
        }
        
        public void ApplyUpgradeEffects()
        {
            foreach (var upgrade in upgradeRegistry.Values.Where(u => u.IsOwned))
            {
                upgrade.ApplyEffect(gameObject);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool ArePrerequisitesMet(IUpgrade upgrade)
        {
            return upgrade.Prerequisites.All(prereqId => HasUpgrade(prereqId));
        }
        
        private bool HasMutuallyExclusiveUpgrade(IUpgrade upgrade)
        {
            return upgrade.MutuallyExclusive.Any(excludeId => HasUpgrade(excludeId));
        }
        
        private bool DeductCurrency(int amount, CurrencyType currencyType)
        {
            var currency = GetCurrencyComponent(currencyType);
            if (currency == null || currency.current < amount)
            {
                return false;
            }
            
            currency.Decrement(amount);
            return true;
        }
        
        private BaseCurrency GetCurrencyComponent(CurrencyType currencyType)
        {
            switch (currencyType)
            {
                case CurrencyType.Meta:
                    return player?.MetaCurrency;
                case CurrencyType.Perma:
                    return player?.PermaCurrency;
                default:
                    return null;
            }
        }
        
        #endregion
        
        #region Unity Editor Support
        
        [ContextMenu("Debug: List All Upgrades")]
        private void DebugListUpgrades()
        {
            Debug.Log($"=== Upgrade System Debug ({upgradeRegistry.Count} upgrades) ===");
            foreach (var upgrade in upgradeRegistry.Values)
            {
                string status = upgrade.IsOwned ? $"OWNED (Level {upgrade.CurrentLevel})" : "Available";
                Debug.Log($"{upgrade.Name} ({upgrade.Id}) - {upgrade.CurrencyType} - Cost: {upgrade.Cost} - {status}");
            }
        }
        
        [ContextMenu("Debug: Apply All Effects")]
        private void DebugApplyAllEffects()
        {
            ApplyUpgradeEffects();
            Debug.Log("Applied all upgrade effects");
        }
        
        #endregion
    }
}