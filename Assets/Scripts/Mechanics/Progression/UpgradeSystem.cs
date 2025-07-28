using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Mechanics.Currency;

namespace NeonLadder.Mechanics.Progression
{
    public class UpgradeSystem : MonoBehaviour, IUpgradeSystem
    {
        [Header("Configuration")]
        [SerializeField] private UpgradeData[] availableUpgrades = new UpgradeData[0];
        
        [Header("Dependencies")]
        [SerializeField] private Meta metaCurrency;
        [SerializeField] private Perma permaCurrency;
        [SerializeField] private GameObject player;
        
        private Dictionary<string, IUpgrade> upgradeDatabase;
        private HashSet<string> ownedUpgrades;
        
        public event Action<IUpgrade> OnUpgradePurchased;
        
        private void Awake()
        {
            InitializeUpgradeSystem();
        }
        
        private void Start()
        {
            // Auto-find dependencies if not set
            if (metaCurrency == null)
                metaCurrency = GetComponent<Meta>();
            if (permaCurrency == null)
                permaCurrency = GetComponent<Perma>();
            if (player == null)
                player = gameObject;
        }
        
        private void InitializeUpgradeSystem()
        {
            upgradeDatabase = new Dictionary<string, IUpgrade>();
            ownedUpgrades = new HashSet<string>();
            
            foreach (var upgrade in availableUpgrades)
            {
                if (upgrade != null && !upgradeDatabase.ContainsKey(upgrade.Id))
                {
                    upgradeDatabase[upgrade.Id] = upgrade;
                }
            }
        }
        
        public bool PurchaseUpgrade(string upgradeId, CurrencyType currencyType)
        {
            if (!upgradeDatabase.TryGetValue(upgradeId, out var upgrade))
            {
                Debug.LogWarning($"Upgrade '{upgradeId}' not found");
                return false;
            }
            
            if (upgrade.CurrencyType != currencyType)
            {
                Debug.LogWarning($"Upgrade '{upgradeId}' requires {upgrade.CurrencyType} currency, not {currencyType}");
                return false;
            }
            
            if (!CanAffordUpgrade(upgradeId, currencyType))
            {
                Debug.LogWarning($"Cannot afford upgrade '{upgradeId}'");
                return false;
            }
            
            if (!ArePrerequisitesMet(upgrade))
            {
                Debug.LogWarning($"Prerequisites not met for upgrade '{upgradeId}'");
                return false;
            }
            
            if (HasMutuallyExclusiveUpgrade(upgrade))
            {
                Debug.LogWarning($"Mutually exclusive upgrade already owned for '{upgradeId}'");
                return false;
            }
            
            if (upgrade.IsMaxLevel)
            {
                Debug.LogWarning($"Upgrade '{upgradeId}' is already at max level");
                return false;
            }
            
            // Deduct currency
            var currency = currencyType == CurrencyType.Meta ? metaCurrency : permaCurrency;
            currency.Decrement(upgrade.Cost);
            
            // Apply upgrade
            upgrade.CurrentLevel++;
            ownedUpgrades.Add(upgradeId);
            upgrade.ApplyEffect(player);
            
            // Fire event
            OnUpgradePurchased?.Invoke(upgrade);
            
            Debug.Log($"Purchased upgrade '{upgrade.Name}' (Level {upgrade.CurrentLevel})");
            return true;
        }
        
        public bool CanAffordUpgrade(string upgradeId, CurrencyType currencyType)
        {
            if (!upgradeDatabase.TryGetValue(upgradeId, out var upgrade))
                return false;
                
            var currency = currencyType == CurrencyType.Meta ? metaCurrency : permaCurrency;
            return currency != null && currency.current >= upgrade.Cost;
        }
        
        public bool HasUpgrade(string upgradeId)
        {
            return ownedUpgrades.Contains(upgradeId);
        }
        
        public IUpgrade GetUpgrade(string upgradeId)
        {
            upgradeDatabase.TryGetValue(upgradeId, out var upgrade);
            return upgrade;
        }
        
        public IEnumerable<IUpgrade> GetAvailableUpgrades(CurrencyType currencyType)
        {
            return upgradeDatabase.Values
                .Where(upgrade => upgrade.CurrencyType == currencyType)
                .Where(upgrade => !upgrade.IsMaxLevel || HasUpgrade(upgrade.Id));
        }
        
        public void ResetMetaUpgrades()
        {
            var metaUpgrades = upgradeDatabase.Values
                .Where(upgrade => upgrade.CurrencyType == CurrencyType.Meta && HasUpgrade(upgrade.Id))
                .ToList();
                
            foreach (var upgrade in metaUpgrades)
            {
                upgrade.RemoveEffect(player);
                upgrade.CurrentLevel = 0;
                ownedUpgrades.Remove(upgrade.Id);
            }
            
            Debug.Log($"Reset {metaUpgrades.Count} meta upgrades");
        }
        
        public void ApplyUpgradeEffects()
        {
            foreach (var upgradeId in ownedUpgrades)
            {
                if (upgradeDatabase.TryGetValue(upgradeId, out var upgrade))
                {
                    upgrade.ApplyEffect(player);
                }
            }
            
            Debug.Log($"Applied {ownedUpgrades.Count} upgrade effects");
        }
        
        private bool ArePrerequisitesMet(IUpgrade upgrade)
        {
            return upgrade.Prerequisites.All(prereq => HasUpgrade(prereq));
        }
        
        private bool HasMutuallyExclusiveUpgrade(IUpgrade upgrade)
        {
            return upgrade.MutuallyExclusive.Any(exclusive => HasUpgrade(exclusive));
        }
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: List All Upgrades")]
        private void DebugListUpgrades()
        {
            Debug.Log("=== Upgrade System Debug ===");
            foreach (var kvp in upgradeDatabase)
            {
                var upgrade = kvp.Value;
                var owned = HasUpgrade(upgrade.Id) ? $"OWNED (Level {upgrade.CurrentLevel})" : "NOT OWNED";
                Debug.Log($"{upgrade.Name} ({upgrade.Id}): {owned} - {upgrade.CurrencyType} - {upgrade.Cost} cost");
            }
        }
        #endif
    }
}