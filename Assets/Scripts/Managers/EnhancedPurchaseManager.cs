using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Models;
using NeonLadder.Events;
using NeonLadder.Mechanics.Progression;
using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Currency;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Enhanced purchase manager that handles both PurchasableItems and Upgrades
    /// Replaces the hardcoded LootPurchaseManager with a flexible, event-driven system
    /// </summary>
    public class EnhancedPurchaseManager : MonoBehaviour
    {
        [Header("Available Items")]
        [SerializeField] private PurchasableItem[] availableItems;
        [SerializeField] private UpgradeData[] availableUpgrades;
        
        [Header("Dependencies")]
        [SerializeField] private Player player;
        [SerializeField] private UpgradeSystem upgradeSystem;
        
        private Dictionary<string, PurchasableItem> itemDatabase;
        private Dictionary<string, UpgradeData> upgradeDatabase;
        
        // Events
        public System.Action<PurchasableItem> OnItemPurchased;
        public System.Action<UpgradeData> OnUpgradePurchased;
        public System.Action<string> OnPurchaseFailed;
        
        private void Awake()
        {
            InitializeDatabases();
        }
        
        private void Start()
        {
            // Auto-find dependencies if not set
            if (player == null)
                player = FindObjectOfType<Player>();
            if (upgradeSystem == null)
                upgradeSystem = FindObjectOfType<UpgradeSystem>();
        }
        
        private void InitializeDatabases()
        {
            Debug.Log($"InitializeDatabases called from: {System.Environment.StackTrace.Split('\n')[1]}");
            Debug.Log($"availableItems is {(availableItems == null ? "null" : $"array with {availableItems.Length} items")}");
            
            // Initialize item database
            itemDatabase = new Dictionary<string, PurchasableItem>();
            if (availableItems != null)
            {
                foreach (var item in availableItems)
                {
                    if (item != null)
                    {
                        itemDatabase[item.ItemId] = item;
                    }
                }
            }
            
            // Initialize upgrade database
            upgradeDatabase = new Dictionary<string, UpgradeData>();
            if (availableUpgrades != null)
            {
                foreach (var upgrade in availableUpgrades)
                {
                    if (upgrade != null)
                    {
                        upgradeDatabase[upgrade.Id] = upgrade;
                    }
                }
            }
            
            Debug.Log($"Initialized purchase manager with {itemDatabase.Count} items and {upgradeDatabase.Count} upgrades");
        }
        
        #region Item Purchases
        
        /// <summary>
        /// Purchase a PurchasableItem by ID
        /// </summary>
        public bool PurchaseItem(string itemId)
        {
            if (!itemDatabase.TryGetValue(itemId, out var item))
            {
                Debug.LogWarning($"Item '{itemId}' not found in database");
                OnPurchaseFailed?.Invoke($"Item '{itemId}' not found");
                return false;
            }
            
            return PurchaseItem(item);
        }
        
        /// <summary>
        /// Purchase a PurchasableItem directly
        /// </summary>
        public bool PurchaseItem(PurchasableItem item)
        {
            if (item == null)
            {
                OnPurchaseFailed?.Invoke("Item is null");
                return false;
            }
            
            if (player == null)
            {
                Debug.LogWarning("Player reference is null");
                OnPurchaseFailed?.Invoke("Player not found");
                return false;
            }
            
            // Check prerequisites
            if (!ArePrerequisitesMet(item))
            {
                OnPurchaseFailed?.Invoke($"Prerequisites not met for {item.ItemName}");
                return false;
            }
            
            // Check if can purchase
            if (!item.CanPurchase)
            {
                OnPurchaseFailed?.Invoke($"{item.ItemName} is at maximum purchases ({item.MaxPurchases})");
                return false;
            }
            
            // Check currency
            var currency = GetCurrency(ConvertCurrencyType(item.CurrencyType));
            if (currency == null || !item.CanAfford(currency.current))
            {
                var currentAmount = currency?.current ?? 0;
                OnPurchaseFailed?.Invoke($"Not enough {item.CurrencyType} currency for {item.ItemName} (Need: {item.Cost}, Have: {currentAmount})");
                return false;
            }
            
            // Deduct currency using event system for immediate feedback
            BatchCurrencyEvent.CollectCurrencyImmediate(player, ConvertCurrencyType(item.CurrencyType), -item.Cost);
            
            // Purchase the item
            item.Purchase(player);
            
            // Fire event
            OnItemPurchased?.Invoke(item);
            
            Debug.Log($"Successfully purchased {item.ItemName} for {item.Cost} {item.CurrencyType} currency");
            return true;
        }
        
        #endregion
        
        #region Upgrade Purchases
        
        /// <summary>
        /// Purchase an upgrade via the upgrade system
        /// </summary>
        public bool PurchaseUpgrade(string upgradeId, NeonLadder.Mechanics.Progression.CurrencyType currencyType)
        {
            if (upgradeSystem == null)
            {
                Debug.LogWarning("UpgradeSystem not found");
                OnPurchaseFailed?.Invoke("Upgrade system not available");
                return false;
            }
            
            bool success = upgradeSystem.PurchaseUpgrade(upgradeId, currencyType);
            
            if (success && upgradeDatabase.TryGetValue(upgradeId, out var upgrade))
            {
                OnUpgradePurchased?.Invoke(upgrade);
            }
            else if (!success)
            {
                OnPurchaseFailed?.Invoke($"Failed to purchase upgrade '{upgradeId}'");
            }
            
            return success;
        }
        
        #endregion
        
        #region Shop Queries
        
        /// <summary>
        /// Get all items available for a specific currency type
        /// </summary>
        public List<PurchasableItem> GetAvailableItems(NeonLadder.Events.CurrencyType currencyType, bool metaShop = true)
        {
            return itemDatabase.Values
                .Where(item => ConvertCurrencyType(item.CurrencyType) == currencyType)
                .Where(item => metaShop ? item.IsAvailableInMetaShop : item.IsAvailableInPermaShop)
                .Where(item => ArePrerequisitesMet(item))
                .Where(item => item.CanPurchase)
                .ToList();
        }
        
        /// <summary>
        /// Get all upgrades available for a specific currency type
        /// </summary>
        public List<UpgradeData> GetAvailableUpgrades(NeonLadder.Mechanics.Progression.CurrencyType currencyType)
        {
            if (upgradeSystem == null) return new List<UpgradeData>();
            
            return upgradeSystem.GetAvailableUpgrades(currencyType).Cast<UpgradeData>().ToList();
        }
        
        /// <summary>
        /// Get all affordable items for current currency
        /// </summary>
        public List<PurchasableItem> GetAffordableItems(NeonLadder.Events.CurrencyType currencyType, bool metaShop = true)
        {
            var currency = GetCurrency(currencyType);
            if (currency == null) return new List<PurchasableItem>();
            
            return GetAvailableItems(currencyType, metaShop)
                .Where(item => item.CanAfford(currency.current))
                .ToList();
        }
        
        /// <summary>
        /// Check if player can afford an item
        /// </summary>
        public bool CanAffordItem(string itemId)
        {
            Debug.Log($"CanAffordItem called with itemId: {itemId}, itemDatabase.Count: {itemDatabase?.Count ?? -1}");
            
            if (!itemDatabase.TryGetValue(itemId, out var item))
            {
                Debug.Log($"Item '{itemId}' not found in database. Available items: {(itemDatabase != null ? string.Join(", ", itemDatabase.Keys) : "null")}");
                return false;
            }
                
            var currency = GetCurrency(ConvertCurrencyType(item.CurrencyType));
            if (currency == null)
            {
                Debug.Log($"Currency is null for item {itemId}");
                return false;
            }
            
            bool canAfford = item.CanAfford(currency.current);
            Debug.Log($"Item {itemId}: currency.current={currency.current}, item.Cost={item.Cost}, item.CanPurchase={item.CanPurchase}, result={canAfford}");
            
            return canAfford;
        }
        
        /// <summary>
        /// Check if player can afford an upgrade
        /// </summary>
        public bool CanAffordUpgrade(string upgradeId, NeonLadder.Mechanics.Progression.CurrencyType currencyType)
        {
            if (upgradeSystem == null) return false;
            return upgradeSystem.CanAffordUpgrade(upgradeId, currencyType);
        }
        
        #endregion
        
        #region Reset & Lifecycle
        
        /// <summary>
        /// Reset meta purchases on death/run end
        /// </summary>
        public void ResetMetaPurchases()
        {
            foreach (var item in itemDatabase.Values)
            {
                if (item.CurrencyType == NeonLadder.Mechanics.Progression.CurrencyType.Meta)
                {
                    item.ResetPurchases();
                }
            }
            
            // Also reset upgrade system meta upgrades
            upgradeSystem?.ResetMetaUpgrades();
            
            Debug.Log("Reset all meta purchases");
        }
        
        /// <summary>
        /// Apply all persistent effects on run start
        /// </summary>
        public void ApplyPersistentEffects()
        {
            upgradeSystem?.ApplyUpgradeEffects();
            
            // Apply persistent item effects
            foreach (var item in itemDatabase.Values)
            {
                if (item.CurrencyType == NeonLadder.Mechanics.Progression.CurrencyType.Perma && item.TimesPurchased > 0)
                {
                    // Re-apply persistent item effects
                    for (int i = 0; i < item.TimesPurchased; i++)
                    {
                        item.Purchase(player);
                    }
                }
            }
            
            Debug.Log("Applied all persistent effects");
        }
        
        #endregion
        
        #region Helper Methods
        
        private bool ArePrerequisitesMet(PurchasableItem item)
        {
            if (item?.RequiredUnlocks != null)
            {
                foreach (var requiredUnlock in item.RequiredUnlocks)
                {
                    // Check if prerequisite is met (would integrate with your unlock system)
                    // For now, assume all prerequisites are met
                    Debug.Log($"Checking prerequisite: {requiredUnlock}");
                }
            }
            return true;
        }
        
        private NeonLadder.Events.CurrencyType ConvertCurrencyType(NeonLadder.Mechanics.Progression.CurrencyType progressionCurrency)
        {
            switch (progressionCurrency)
            {
                case NeonLadder.Mechanics.Progression.CurrencyType.Meta:
                    return NeonLadder.Events.CurrencyType.Meta;
                case NeonLadder.Mechanics.Progression.CurrencyType.Perma:
                    return NeonLadder.Events.CurrencyType.Perma;
                default:
                    return NeonLadder.Events.CurrencyType.Meta; // Default fallback
            }
        }
        
        private BaseCurrency GetCurrency(NeonLadder.Events.CurrencyType currencyType)
        {
            if (player == null) 
            {
                Debug.Log("GetCurrency: player is null");
                return null;
            }
            
            Debug.Log($"GetCurrency: currencyType={currencyType}, player.MetaCurrency={player.MetaCurrency}, player.PermaCurrency={player.PermaCurrency}");
            
            BaseCurrency result = null;
            switch (currencyType)
            {
                case NeonLadder.Events.CurrencyType.Meta:
                    result = player.MetaCurrency;
                    Debug.Log($"Meta currency: {(result != null ? $"current={result.current}" : "null")}");
                    break;
                case NeonLadder.Events.CurrencyType.Perma:
                    result = player.PermaCurrency;
                    Debug.Log($"Perma currency: {(result != null ? $"current={result.current}" : "null")}");
                    break;
                default:
                    result = null;
                    break;
            }
            
            return result;
        }
        
        /// <summary>
        /// Public method to reinitialize databases (useful for tests)
        /// </summary>
        public void ReinitializeDatabases()
        {
            InitializeDatabases();
        }
        
        /// <summary>
        /// Set available items and upgrades for testing
        /// </summary>
        public void SetTestData(PurchasableItem[] items, UpgradeData[] upgrades, Player testPlayer, UpgradeSystem testUpgradeSystem)
        {
            Debug.Log($"SetTestData called: items.Length={items?.Length ?? -1}, upgrades.Length={upgrades?.Length ?? -1}");
            
            availableItems = items;
            availableUpgrades = upgrades;
            player = testPlayer;
            upgradeSystem = testUpgradeSystem;
            
            Debug.Log($"Before InitializeDatabases: availableItems.Length={availableItems?.Length ?? -1}");
            InitializeDatabases();
            Debug.Log($"After InitializeDatabases: itemDatabase.Count={itemDatabase?.Count ?? -1}");
        }
        
        #endregion
        
        #region Debug & Editor Tools
        
        #if UNITY_EDITOR
        [ContextMenu("Debug: List All Items")]
        private void DebugListItems()
        {
            Debug.Log("=== Purchase Manager Debug ===");
            Debug.Log($"Items ({itemDatabase.Count}):");
            foreach (var kvp in itemDatabase)
            {
                var item = kvp.Value;
                Debug.Log($"  {item.ItemName} ({item.ItemId}): {item.Cost} {item.CurrencyType} - Purchased {item.TimesPurchased}/{item.MaxPurchases} times");
            }
            
            Debug.Log($"Upgrades ({upgradeDatabase.Count}):");
            foreach (var kvp in upgradeDatabase)
            {
                var upgrade = kvp.Value;
                Debug.Log($"  {upgrade.Name} ({upgrade.Id}): {upgrade.Cost} {upgrade.CurrencyType}");
            }
        }
        
        [ContextMenu("Debug: Test Purchase Flow")]
        private void DebugTestPurchaseFlow()
        {
            if (Application.isPlaying && itemDatabase.Count > 0)
            {
                var firstItem = itemDatabase.Values.First();
                Debug.Log($"Testing purchase of: {firstItem.ItemName}");
                PurchaseItem(firstItem);
            }
        }
        #endif
        
        #endregion
    }
}