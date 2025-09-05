using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;

namespace NeonLadder.Items.Shop
{
    /// <summary>
    /// Represents a shop's inventory that can be attached to NPCs, vending machines, etc.
    /// </summary>
    [CreateAssetMenu(fileName = "New Shop Inventory", menuName = "NeonLadder/Items/Shop Inventory")]
    public class ShopInventory : ScriptableObject
    {
        [Header("Shop Identity")]
        [SerializeField] private string shopId;
        [SerializeField] private string shopName = "General Store";
        [SerializeField] private string shopDescription;
        [SerializeField] private Sprite shopBanner;
        
        [Header("Currency Settings")]
        [SerializeField] private CurrencyType acceptedCurrency = CurrencyType.Meta;
        [SerializeField] private float buyPriceMultiplier = 1f; // Markup from base price
        [SerializeField] private float sellPriceMultiplier = 0.5f; // Player sells for less
        
        [Header("Stock Configuration")]
        [SerializeField] private List<ShopItem> stockedItems = new List<ShopItem>();
        [SerializeField] private bool restockOnRun = true; // Restock each run
        [SerializeField] private bool restockOnFloor = false; // Restock each floor
        
        [Header("Special Conditions")]
        [SerializeField] private ItemTier minTierRequired = ItemTier.Tier1;
        [SerializeField] private ItemTier maxTierAllowed = ItemTier.Tier5;
        [SerializeField] private bool scaleWithProgression = true;
        
        // Runtime state
        private Dictionary<string, int> currentStock = new Dictionary<string, int>();
        private bool isInitialized = false;
        
        /// <summary>
        /// Shop item configuration
        /// </summary>
        [System.Serializable]
        public class ShopItem
        {
            public ItemDefinition itemDefinition;
            public int baseStock = -1; // -1 = unlimited
            public int restockAmount = 1;
            public float priceOverride = -1f; // -1 = use item's base price
            public bool requiresUnlock = false;
            public string unlockId;
            
            [Header("Availability")]
            public bool alwaysAvailable = true;
            public float availabilityChance = 100f; // Chance this item appears in shop
            public ItemTier requiredPlayerTier = ItemTier.Tier1;
        }
        
        /// <summary>
        /// Initialize or reset the shop inventory
        /// </summary>
        public void InitializeShop(ItemTier playerTier = ItemTier.Tier1, float luck = 1f)
        {
            currentStock.Clear();
            
            foreach (var shopItem in stockedItems)
            {
                if (shopItem.itemDefinition == null) continue;
                
                // Check tier requirements
                if (shopItem.requiredPlayerTier > playerTier) continue;
                
                // Check availability chance
                float roll = Random.Range(0f, 100f);
                if (!shopItem.alwaysAvailable && roll > shopItem.availabilityChance * luck)
                    continue;
                
                // Set initial stock
                string itemId = shopItem.itemDefinition.ItemId;
                currentStock[itemId] = shopItem.baseStock;
            }
            
            isInitialized = true;
        }
        
        /// <summary>
        /// Get all currently available items for display
        /// </summary>
        public List<ShopListing> GetAvailableItems(ItemTier playerTier = ItemTier.Tier1)
        {
            if (!isInitialized)
            {
                InitializeShop(playerTier);
            }
            
            var listings = new List<ShopListing>();
            
            foreach (var shopItem in stockedItems)
            {
                if (shopItem.itemDefinition == null) continue;
                
                string itemId = shopItem.itemDefinition.ItemId;
                
                // Check if item is in stock
                if (!currentStock.ContainsKey(itemId)) continue;
                if (currentStock[itemId] == 0) continue; // Out of stock
                
                // Check tier requirements
                if (shopItem.requiredPlayerTier > playerTier) continue;
                if (shopItem.itemDefinition.Tier > maxTierAllowed) continue;
                if (shopItem.itemDefinition.Tier < minTierRequired) continue;
                
                // Create listing
                var listing = new ShopListing
                {
                    Item = shopItem.itemDefinition,
                    BuyPrice = CalculateBuyPrice(shopItem),
                    SellPrice = CalculateSellPrice(shopItem.itemDefinition),
                    StockRemaining = currentStock[itemId],
                    RequiresUnlock = shopItem.requiresUnlock,
                    UnlockId = shopItem.unlockId
                };
                
                listings.Add(listing);
            }
            
            return listings;
        }
        
        /// <summary>
        /// Attempt to purchase an item from the shop
        /// </summary>
        public bool TryPurchaseItem(string itemId, int quantity = 1)
        {
            if (!currentStock.ContainsKey(itemId))
                return false;
            
            int stock = currentStock[itemId];
            
            // Check unlimited stock
            if (stock == -1)
                return true;
            
            // Check sufficient stock
            if (stock < quantity)
                return false;
            
            // Reduce stock
            currentStock[itemId] = stock - quantity;
            return true;
        }
        
        /// <summary>
        /// Sell an item to the shop (if it accepts items)
        /// </summary>
        public int GetSellPrice(ItemDefinition item)
        {
            if (item == null) return 0;
            return CalculateSellPrice(item);
        }
        
        /// <summary>
        /// Restock the shop
        /// </summary>
        public void RestockShop()
        {
            foreach (var shopItem in stockedItems)
            {
                if (shopItem.itemDefinition == null) continue;
                
                string itemId = shopItem.itemDefinition.ItemId;
                
                if (shopItem.baseStock == -1)
                {
                    currentStock[itemId] = -1; // Unlimited
                }
                else if (currentStock.ContainsKey(itemId))
                {
                    currentStock[itemId] = Mathf.Min(
                        currentStock[itemId] + shopItem.restockAmount,
                        shopItem.baseStock
                    );
                }
                else
                {
                    currentStock[itemId] = shopItem.baseStock;
                }
            }
        }
        
        private int CalculateBuyPrice(ShopItem shopItem)
        {
            if (shopItem.priceOverride > 0)
                return Mathf.RoundToInt(shopItem.priceOverride);
            
            int basePrice = shopItem.itemDefinition.BuyValue;
            
            // Apply shop markup
            float finalPrice = basePrice * buyPriceMultiplier;
            
            // Apply rarity multiplier
            float rarityMult = shopItem.itemDefinition.Rarity switch
            {
                ItemRarity.Common => 1f,
                ItemRarity.Uncommon => 1.5f,
                ItemRarity.Rare => 2.5f,
                ItemRarity.Epic => 4f,
                ItemRarity.Legendary => 8f,
                ItemRarity.Mythic => 16f,
                _ => 1f
            };
            
            finalPrice *= rarityMult;
            
            return Mathf.Max(1, Mathf.RoundToInt(finalPrice));
        }
        
        private int CalculateSellPrice(ItemDefinition item)
        {
            return Mathf.Max(1, Mathf.RoundToInt(item.SellValue * sellPriceMultiplier));
        }
        
        // Properties
        public string ShopId => shopId;
        public string ShopName => shopName;
        public CurrencyType AcceptedCurrency => acceptedCurrency;
        public bool RestockOnRun => restockOnRun;
        public bool RestockOnFloor => restockOnFloor;
    }
    
    /// <summary>
    /// Runtime representation of an item in the shop
    /// </summary>
    public class ShopListing
    {
        public ItemDefinition Item;
        public int BuyPrice;
        public int SellPrice;
        public int StockRemaining; // -1 = unlimited
        public bool RequiresUnlock;
        public string UnlockId;
        
        public bool IsInStock => StockRemaining != 0;
        public bool IsUnlimited => StockRemaining == -1;
    }
}