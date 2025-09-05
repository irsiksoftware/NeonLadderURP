using UnityEngine;
using NeonLadder.Items.Loot;
using NeonLadder.Items.Enums;
using NeonLadder.Items.Core;
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using NeonLadder.Managers;

namespace NeonLadder.Items.Shop
{
    /// <summary>
    /// Component for NPCs that run shops
    /// </summary>
    public class NPCShopSource : MonoBehaviour, ILootSource
    {
        [Header("Shop Configuration")]
        [SerializeField] private ShopInventory shopInventory;
        [SerializeField] private string shopKeeperName = "Merchant";
        [SerializeField] private Sprite shopKeeperPortrait;
        
        [Header("Alternative Loot")]
        [Tooltip("Optional: Loot table for if the NPC is killed/robbed")]
        [SerializeField] private ImprovedLootTable robberyLootTable;
        
        [Header("Shop Behavior")]
        [SerializeField] private bool autoOpenOnApproach = true;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private GameObject shopUICanvas;
        
        [Header("Dialogue")]
        [SerializeField] private string[] greetingLines;
        [SerializeField] private string[] purchaseLines;
        [SerializeField] private string[] cantAffordLines;
        [SerializeField] private string[] farewellLines;
        
        private bool isShopOpen = false;
        private Player currentCustomer;
        
        private void Start()
        {
            // Initialize shop on start
            if (shopInventory != null)
            {
                shopInventory.InitializeShop();
            }
            
            // Find or create shop UI
            if (shopUICanvas == null)
            {
                shopUICanvas = GameObject.FindGameObjectWithTag("ShopkeeperWindow");
            }
        }
        
        #region ILootSource Implementation
        
        public ImprovedLootTable GetLootTable()
        {
            // Return robbery loot table if applicable
            return robberyLootTable;
        }
        
        public ShopInventory GetShopInventory()
        {
            return shopInventory;
        }
        
        public LootSourceType SourceType => LootSourceType.Shop;
        
        public Transform GetDropPosition()
        {
            // Items spawn near the NPC
            return transform;
        }
        
        public float GetLuckModifier()
        {
            // Shops don't use luck for pricing
            return 1f;
        }
        
        public ItemTier GetRequiredTier()
        {
            // Shop might have tier requirements
            return ItemTier.Tier1;
        }
        
        #endregion
        
        #region Shop Interaction
        
        private void OnTriggerEnter(Collider other)
        {
            if (!autoOpenOnApproach) return;
            
            if (other.CompareTag("Player"))
            {
                currentCustomer = other.GetComponent<Player>();
                OpenShop();
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                CloseShop();
                currentCustomer = null;
            }
        }
        
        public void OpenShop()
        {
            if (shopInventory == null)
            {
                Debug.LogWarning($"NPC {shopKeeperName} has no shop inventory!");
                return;
            }
            
            isShopOpen = true;
            
            // Show greeting
            if (greetingLines != null && greetingLines.Length > 0)
            {
                string greeting = greetingLines[Random.Range(0, greetingLines.Length)];
                Debug.Log($"{shopKeeperName}: {greeting}");
            }
            
            // Update shop UI
            if (shopUICanvas != null)
            {
                shopUICanvas.SetActive(true);
                UpdateShopUI();
            }
            
            // Disable player attack - TODO: Implement when input system is accessible
            if (currentCustomer != null)
            {
                Debug.Log("Shop opened - player attack disabled (not yet implemented)");
            }
        }
        
        public void CloseShop()
        {
            isShopOpen = false;
            
            // Show farewell
            if (farewellLines != null && farewellLines.Length > 0)
            {
                string farewell = farewellLines[Random.Range(0, farewellLines.Length)];
                Debug.Log($"{shopKeeperName}: {farewell}");
            }
            
            // Hide shop UI
            if (shopUICanvas != null)
            {
                shopUICanvas.SetActive(false);
            }
            
            // Re-enable player attack - TODO: Implement when input system is accessible
            if (currentCustomer != null)
            {
                Debug.Log("Shop closed - player attack re-enabled (not yet implemented)");
            }
        }
        
        #endregion
        
        #region Shop Transactions
        
        public bool TryPurchaseItem(ItemDefinition item, int quantity = 1)
        {
            if (currentCustomer == null || shopInventory == null)
                return false;
            
            // Get item listing
            var listings = shopInventory.GetAvailableItems();
            ShopListing listing = null;
            
            foreach (var l in listings)
            {
                if (l.Item == item)
                {
                    listing = l;
                    break;
                }
            }
            
            if (listing == null)
            {
                Debug.Log($"Item {item.DisplayName} not available in shop");
                return false;
            }
            
            // Check currency
            int playerCurrency = shopInventory.AcceptedCurrency == CurrencyType.Meta ?
                currentCustomer.MetaCurrency.current : currentCustomer.PermaCurrency.current;
            
            int totalCost = listing.BuyPrice * quantity;
            
            if (playerCurrency < totalCost)
            {
                // Can't afford
                if (cantAffordLines != null && cantAffordLines.Length > 0)
                {
                    string cantAfford = cantAffordLines[Random.Range(0, cantAffordLines.Length)];
                    Debug.Log($"{shopKeeperName}: {cantAfford}");
                }
                return false;
            }
            
            // Try to purchase from inventory
            if (!shopInventory.TryPurchaseItem(item.ItemId, quantity))
            {
                Debug.Log($"Item {item.DisplayName} is out of stock");
                return false;
            }
            
            // Deduct currency
            if (shopInventory.AcceptedCurrency == CurrencyType.Meta)
            {
                currentCustomer.AddMetaCurrency(-totalCost);
            }
            else
            {
                currentCustomer.AddPermanentCurrency(-totalCost);
            }
            
            // Give item to player
            var itemInstance = new ItemInstance(item, quantity);
            GiveItemToPlayer(itemInstance);
            
            // Show purchase dialogue
            if (purchaseLines != null && purchaseLines.Length > 0)
            {
                string purchase = purchaseLines[Random.Range(0, purchaseLines.Length)];
                Debug.Log($"{shopKeeperName}: {purchase}");
            }
            
            UpdateShopUI();
            return true;
        }
        
        public bool TrySellItem(ItemInstance item)
        {
            if (currentCustomer == null || shopInventory == null || item == null)
                return false;
            
            int sellPrice = shopInventory.GetSellPrice(item.Definition);
            
            // Give currency to player
            if (shopInventory.AcceptedCurrency == CurrencyType.Meta)
            {
                currentCustomer.AddMetaCurrency(sellPrice * item.Stacks);
            }
            else
            {
                currentCustomer.AddPermanentCurrency(sellPrice * item.Stacks);
            }
            
            Debug.Log($"Sold {item.GetDisplayName()} for {sellPrice * item.Stacks} currency");
            
            UpdateShopUI();
            return true;
        }
        
        private void GiveItemToPlayer(ItemInstance item)
        {
            // For now, just log that the item was received
            // Later this will add to inventory or apply effects
            Debug.Log($"Player received: {item.GetDisplayName()} x{item.Stacks}");
            
            // TODO: Add to player inventory when implemented
            // TODO: Apply item effects when implemented
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateShopUI()
        {
            // This would update the actual shop UI with current inventory
            // For now, just log the available items
            
            if (shopInventory == null) return;
            
            var listings = shopInventory.GetAvailableItems();
            Debug.Log($"=== {shopKeeperName}'s Shop ===");
            
            foreach (var listing in listings)
            {
                string stockText = listing.IsUnlimited ? "∞" : listing.StockRemaining.ToString();
                Debug.Log($"- {listing.Item.DisplayName}: {listing.BuyPrice} {shopInventory.AcceptedCurrency} (Stock: {stockText})");
            }
        }
        
        #endregion
        
        /// <summary>
        /// Handle what happens if the NPC is killed
        /// </summary>
        public void OnNPCKilled()
        {
            if (robberyLootTable != null)
            {
                // Drop robbery loot
                var lootManager = ImprovedLootDropManager.Instance;
                if (lootManager != null)
                {
                    lootManager.DropLoot(robberyLootTable, transform.position, currentCustomer);
                }
            }
            
            CloseShop();
        }
    }
}