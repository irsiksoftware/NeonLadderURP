using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonLadder.Items.Shop;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;

namespace NeonLadder.UI.Shop
{
    /// <summary>
    /// UI component for displaying individual shop items
    /// </summary>
    public class ShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI stockText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Button sellButton;
        
        [Header("Visual Feedback")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject outOfStockOverlay;
        [SerializeField] private GameObject cantAffordOverlay;
        
        [Header("Colors")]
        [SerializeField] private Color defaultBackgroundColor = Color.white;
        [SerializeField] private Color cantAffordColor = Color.red;
        [SerializeField] private Color outOfStockColor = Color.gray;
        
        private ShopListing currentListing;
        private ShopWindowUI parentWindow;
        private bool isSetup = false;
        
        public void Setup(ShopWindowUI parent)
        {
            parentWindow = parent;
            
            // Setup button listeners
            if (purchaseButton != null)
            {
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }
            
            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }
            
            isSetup = true;
        }
        
        public void DisplayItem(ShopListing listing, bool canAfford, bool showSellOption = false)
        {
            if (!isSetup || listing == null)
            {
                gameObject.SetActive(false);
                return;
            }
            
            currentListing = listing;
            gameObject.SetActive(true);
            
            // Set item icon
            if (itemIcon != null && listing.Item.Icon != null)
            {
                itemIcon.sprite = listing.Item.Icon;
                itemIcon.color = Color.white;
            }
            else if (itemIcon != null)
            {
                // Show default icon or hide
                itemIcon.color = Color.clear;
            }
            
            // Set item name with rarity coloring
            if (itemNameText != null)
            {
                itemNameText.text = listing.Item.DisplayName;
                itemNameText.color = listing.Item.GetRarityColor();
            }
            
            // Set description
            if (itemDescriptionText != null)
            {
                itemDescriptionText.text = listing.Item.GetFullDescription();
            }
            
            // Set price
            if (priceText != null)
            {
                string currencySymbol = GetCurrencySymbol();
                priceText.text = $"{listing.BuyPrice} {currencySymbol}";
                priceText.color = canAfford ? Color.white : cantAffordColor;
            }
            
            // Set stock
            if (stockText != null)
            {
                if (listing.IsUnlimited)
                {
                    stockText.text = "∞";
                }
                else
                {
                    stockText.text = $"Stock: {listing.StockRemaining}";
                }
                stockText.gameObject.SetActive(!listing.IsUnlimited);
            }
            
            // Set rarity border
            if (rarityBorder != null)
            {
                rarityBorder.color = listing.Item.GetRarityColor();
            }
            
            // Update visual state
            UpdateVisualState(canAfford);
            
            // Show/hide sell button
            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(showSellOption);
                if (showSellOption)
                {
                    var sellPriceText = sellButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (sellPriceText != null)
                    {
                        string currencySymbol = GetCurrencySymbol();
                        sellPriceText.text = $"Sell: {listing.SellPrice} {currencySymbol}";
                    }
                }
            }
        }
        
        private void UpdateVisualState(bool canAfford)
        {
            bool outOfStock = !currentListing.IsInStock;
            
            // Update purchase button
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford && !outOfStock;
            }
            
            // Update background color
            if (backgroundImage != null)
            {
                if (outOfStock)
                {
                    backgroundImage.color = outOfStockColor;
                }
                else if (!canAfford)
                {
                    backgroundImage.color = cantAffordColor * 0.5f; // Semi-transparent red
                }
                else
                {
                    backgroundImage.color = defaultBackgroundColor;
                }
            }
            
            // Show/hide overlays
            if (outOfStockOverlay != null)
            {
                outOfStockOverlay.SetActive(outOfStock);
            }
            
            if (cantAffordOverlay != null)
            {
                cantAffordOverlay.SetActive(!canAfford && !outOfStock);
            }
        }
        
        private void OnPurchaseClicked()
        {
            if (currentListing == null || parentWindow == null)
                return;
            
            parentWindow.PurchaseItem(currentListing);
        }
        
        private void OnSellClicked()
        {
            if (currentListing == null || parentWindow == null)
                return;
            
            // Create item instance for selling
            var itemInstance = new ItemInstance(currentListing.Item, 1);
            parentWindow.SellItem(itemInstance);
        }
        
        private string GetCurrencySymbol()
        {
            if (parentWindow != null && parentWindow.CurrentShop != null)
            {
                return parentWindow.CurrentShop.AcceptedCurrency switch
                {
                    CurrencyType.Meta => "Ⓜ", // Meta currency symbol
                    CurrencyType.Permanent => "Ⓟ", // Permanent currency symbol
                    _ => "¤" // Generic currency symbol
                };
            }
            return "¤";
        }
        
        /// <summary>
        /// Show tooltip with detailed item information
        /// </summary>
        public void ShowTooltip()
        {
            if (currentListing?.Item == null) return;
            
            // This would show a detailed tooltip
            // For now, just log to console
            Debug.Log($"=== {currentListing.Item.DisplayName} ===");
            Debug.Log($"Type: {currentListing.Item.Type}");
            Debug.Log($"Rarity: {currentListing.Item.Rarity}");
            Debug.Log($"Tier: {currentListing.Item.Tier}");
            Debug.Log($"Description: {currentListing.Item.GetFullDescription()}");
            Debug.Log($"Buy Price: {currentListing.BuyPrice}");
            Debug.Log($"Sell Price: {currentListing.SellPrice}");
        }
        
        /// <summary>
        /// Hide tooltip
        /// </summary>
        public void HideTooltip()
        {
            // Hide tooltip implementation
        }
        
        public void Clear()
        {
            currentListing = null;
            gameObject.SetActive(false);
        }
    }
}