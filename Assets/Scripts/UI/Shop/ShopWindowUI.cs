using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonLadder.Items.Shop;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.UI.Shop
{
    /// <summary>
    /// Main UI manager for shop windows that displays items dynamically
    /// </summary>
    public class ShopWindowUI : MonoBehaviour
    {
        [Header("UI Layout")]
        [SerializeField] private Transform itemContainer; // Parent for shop item slots
        [SerializeField] private GameObject itemSlotPrefab; // Prefab with ShopItemUI component
        [SerializeField] private ScrollRect scrollRect; // For scrollable item list
        
        [Header("Shop Info")]
        [SerializeField] private TextMeshProUGUI shopNameText;
        [SerializeField] private TextMeshProUGUI shopDescriptionText;
        [SerializeField] private Image shopBannerImage;
        
        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI metaCurrencyText;
        [SerializeField] private TextMeshProUGUI permaCurrencyText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        
        [Header("Controls")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Toggle showSellToggle;
        
        [Header("Filters")]
        [SerializeField] private TMP_Dropdown categoryFilter;
        [SerializeField] private TMP_Dropdown rarityFilter;
        [SerializeField] private TMP_InputField searchField;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private AudioClip sellSound;
        [SerializeField] private AudioClip cantAffordSound;
        [SerializeField] private AudioClip openShopSound;
        [SerializeField] private AudioClip closeShopSound;
        
        // Runtime data
        private ShopInventory currentShop;
        private NPCShopSource currentNPC;
        private Player currentPlayer;
        private List<ShopItemUI> itemSlots = new List<ShopItemUI>();
        private List<ShopListing> currentListings = new List<ShopListing>();
        
        // Filter settings
        private ItemType filterCategory = (ItemType)(-1); // -1 = all
        private ItemRarity filterRarity = (ItemRarity)(-1); // -1 = all
        private string searchQuery = "";
        
        public ShopInventory CurrentShop => currentShop;
        
        private void Awake()
        {
            // Setup button listeners
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseShop);
            
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshShop);
            
            if (showSellToggle != null)
                showSellToggle.onValueChanged.AddListener(OnShowSellToggled);
            
            // Setup filter listeners
            if (categoryFilter != null)
                categoryFilter.onValueChanged.AddListener(OnCategoryFilterChanged);
            
            if (rarityFilter != null)
                rarityFilter.onValueChanged.AddListener(OnRarityFilterChanged);
            
            if (searchField != null)
                searchField.onValueChanged.AddListener(OnSearchChanged);
            
            // Initialize filters
            InitializeFilters();
        }
        
        private void Start()
        {
            // Hide shop initially
            gameObject.SetActive(false);
        }
        
        public void OpenShop(NPCShopSource npcSource, Player player)
        {
            if (npcSource?.GetShopInventory() == null)
            {
                Debug.LogError("Cannot open shop - no inventory provided");
                return;
            }
            
            currentNPC = npcSource;
            currentShop = npcSource.GetShopInventory();
            currentPlayer = player;
            
            // Show the window
            gameObject.SetActive(true);
            
            // Play open sound
            PlaySound(openShopSound);
            
            // Update shop info
            UpdateShopInfo();
            
            // Update player info
            UpdatePlayerInfo();
            
            // Refresh item display
            RefreshShop();
            
            // Reset scroll position
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f; // Top
            }
        }
        
        public void CloseShop()
        {
            // Play close sound
            PlaySound(closeShopSound);
            
            // Hide the window
            gameObject.SetActive(false);
            
            // Clear references
            currentShop = null;
            currentNPC = null;
            currentPlayer = null;
            
            // Clear item slots
            ClearItemSlots();
        }
        
        public void RefreshShop()
        {
            if (currentShop == null || currentPlayer == null)
                return;
            
            // Get current listings
            // TODO: Pass actual player tier once progression system is implemented
            currentListings = currentShop.GetAvailableItems(ItemTier.Tier1);
            
            // Apply filters
            var filteredListings = ApplyFilters(currentListings);
            
            // Update UI
            DisplayItems(filteredListings);
            UpdatePlayerInfo(); // Update currency display
        }
        
        private void UpdateShopInfo()
        {
            if (currentShop == null) return;
            
            if (shopNameText != null)
                shopNameText.text = currentShop.ShopName;
            
            // Shop description and banner would be set if available
            // For now, these are basic properties
        }
        
        private void UpdatePlayerInfo()
        {
            if (currentPlayer == null) return;
            
            if (metaCurrencyText != null)
            {
                metaCurrencyText.text = $"Ⓜ {currentPlayer.MetaCurrency.current}";
            }
            
            if (permaCurrencyText != null)
            {
                permaCurrencyText.text = $"Ⓟ {currentPlayer.PermaCurrency.current}";
            }
            
            if (playerNameText != null)
            {
                playerNameText.text = "Player"; // TODO: Get actual player name
            }
        }
        
        private void DisplayItems(List<ShopListing> listings)
        {
            // Clear existing slots
            ClearItemSlots();
            
            // Create slots for each listing
            foreach (var listing in listings)
            {
                CreateItemSlot(listing);
            }
        }
        
        private void CreateItemSlot(ShopListing listing)
        {
            if (itemSlotPrefab == null || itemContainer == null)
                return;
            
            // Instantiate slot
            GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);
            ShopItemUI itemUI = slotObj.GetComponent<ShopItemUI>();
            
            if (itemUI == null)
            {
                Debug.LogError("itemSlotPrefab must have ShopItemUI component!");
                Destroy(slotObj);
                return;
            }
            
            // Setup the item UI
            itemUI.Setup(this);
            
            // Check if player can afford
            int playerCurrency = currentShop.AcceptedCurrency == CurrencyType.Meta ?
                currentPlayer.MetaCurrency.current : currentPlayer.PermaCurrency.current;
            bool canAfford = playerCurrency >= listing.BuyPrice;
            
            // Display the item
            itemUI.DisplayItem(listing, canAfford, showSellToggle?.isOn ?? false);
            
            itemSlots.Add(itemUI);
        }
        
        private void ClearItemSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            itemSlots.Clear();
        }
        
        public void PurchaseItem(ShopListing listing)
        {
            if (currentNPC == null || listing == null)
                return;
            
            bool success = currentNPC.TryPurchaseItem(listing.Item, 1);
            
            if (success)
            {
                PlaySound(purchaseSound);
                RefreshShop(); // Update display
            }
            else
            {
                PlaySound(cantAffordSound);
            }
        }
        
        public void SellItem(ItemInstance itemInstance)
        {
            if (currentNPC == null || itemInstance == null)
                return;
            
            bool success = currentNPC.TrySellItem(itemInstance);
            
            if (success)
            {
                PlaySound(sellSound);
                RefreshShop(); // Update display
            }
        }
        
        #region Filters
        
        private void InitializeFilters()
        {
            // Initialize category filter
            if (categoryFilter != null)
            {
                categoryFilter.options.Clear();
                categoryFilter.options.Add(new TMP_Dropdown.OptionData("All Categories"));
                
                foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
                {
                    categoryFilter.options.Add(new TMP_Dropdown.OptionData(type.ToString()));
                }
                
                categoryFilter.value = 0;
            }
            
            // Initialize rarity filter
            if (rarityFilter != null)
            {
                rarityFilter.options.Clear();
                rarityFilter.options.Add(new TMP_Dropdown.OptionData("All Rarities"));
                
                foreach (ItemRarity rarity in System.Enum.GetValues(typeof(ItemRarity)))
                {
                    rarityFilter.options.Add(new TMP_Dropdown.OptionData(rarity.ToString()));
                }
                
                rarityFilter.value = 0;
            }
        }
        
        private List<ShopListing> ApplyFilters(List<ShopListing> listings)
        {
            var filtered = new List<ShopListing>();
            
            foreach (var listing in listings)
            {
                // Category filter
                if (filterCategory != (ItemType)(-1) && listing.Item.Type != filterCategory)
                    continue;
                
                // Rarity filter
                if (filterRarity != (ItemRarity)(-1) && listing.Item.Rarity != filterRarity)
                    continue;
                
                // Search filter
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    string itemName = listing.Item.DisplayName.ToLower();
                    string query = searchQuery.ToLower();
                    if (!itemName.Contains(query))
                        continue;
                }
                
                filtered.Add(listing);
            }
            
            return filtered;
        }
        
        private void OnCategoryFilterChanged(int value)
        {
            filterCategory = value <= 0 ? (ItemType)(-1) : (ItemType)(value - 1);
            RefreshShop();
        }
        
        private void OnRarityFilterChanged(int value)
        {
            filterRarity = value <= 0 ? (ItemRarity)(-1) : (ItemRarity)(value - 1);
            RefreshShop();
        }
        
        private void OnSearchChanged(string query)
        {
            searchQuery = query;
            RefreshShop();
        }
        
        private void OnShowSellToggled(bool showSell)
        {
            RefreshShop(); // Refresh to show/hide sell buttons
        }
        
        #endregion
        
        #region Audio
        
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
    }
}