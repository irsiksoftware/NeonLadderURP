using UnityEngine;

namespace NeonLadder.UI.Shop
{
    /// <summary>
    /// SHOP UI SETUP GUIDE
    /// 
    /// This guide shows you how to create the UI prefabs for the new shop system.
    /// Follow this structure to get sprite icons, prices, and dynamic item display working.
    /// </summary>
    public class ShopUI_Setup_Guide : MonoBehaviour
    {
        /*
        ===========================================
        SHOP WINDOW UI HIERARCHY (Main Canvas)
        ===========================================
        
        Create this structure in a Canvas prefab:
        
        📋 ShopCanvas (Canvas, tag: "ShopkeeperWindow")
        ├── 🎨 Background (Image)
        ├── 📊 ShopHeader
        │   ├── 🏪 ShopName (TextMeshPro)
        │   ├── 📜 ShopDescription (TextMeshPro)
        │   └── 🖼️ ShopBanner (Image)
        ├── 💰 PlayerInfo
        │   ├── Ⓜ MetaCurrency (TextMeshPro) 
        │   ├── Ⓟ PermaCurrency (TextMeshPro)
        │   └── 👤 PlayerName (TextMeshPro)
        ├── 🔧 Controls
        │   ├── ❌ CloseButton (Button)
        │   ├── 🔄 RefreshButton (Button)
        │   └── 💱 ShowSellToggle (Toggle)
        ├── 🔍 Filters
        │   ├── 📂 CategoryFilter (TMP_Dropdown)
        │   ├── ⭐ RarityFilter (TMP_Dropdown)
        │   └── 🔎 SearchField (TMP_InputField)
        └── 📜 ItemScrollView (ScrollRect)
            └── 📦 ItemContainer (Vertical Layout Group)
                ├── 🛍️ ItemSlot01 (Prefab instance)
                ├── 🛍️ ItemSlot02 (Prefab instance)
                └── ... (dynamically created)
        
        ===========================================
        SHOP ITEM SLOT HIERARCHY (Item Prefab)
        ===========================================
        
        Create this as a separate prefab for individual items:
        
        🛍️ ShopItemSlot
        ├── 🎨 Background (Image) - for visual feedback
        ├── ⭐ RarityBorder (Image) - colored by rarity
        ├── 🖼️ ItemIcon (Image) - displays item sprite
        ├── 📝 ItemInfo
        │   ├── 🏷️ ItemName (TextMeshPro)
        │   └── 📄 ItemDescription (TextMeshPro)
        ├── 💰 PriceInfo
        │   ├── 💵 PriceText (TextMeshPro)
        │   └── 📊 StockText (TextMeshPro)
        ├── 🔘 Buttons
        │   ├── 🛒 PurchaseButton (Button)
        │   └── 💱 SellButton (Button)
        └── 🚫 Overlays
            ├── ❌ OutOfStockOverlay (GameObject)
            └── 💸 CantAffordOverlay (GameObject)
        
        ===========================================
        COMPONENT ASSIGNMENTS
        ===========================================
        
        1. SHOP WINDOW (Main Canvas):
           - Add ShopWindowUI component to ShopCanvas
           - Assign all UI references in inspector:
             * itemContainer → ItemContainer transform
             * itemSlotPrefab → ShopItemSlot prefab
             * scrollRect → ItemScrollView
             * shopNameText → ShopName TextMeshPro
             * metaCurrencyText → MetaCurrency TextMeshPro
             * permaCurrencyText → PermaCurrency TextMeshPro
             * closeButton → CloseButton
             * categoryFilter → CategoryFilter dropdown
             * etc.
        
        2. ITEM SLOT (Prefab):
           - Add ShopItemUI component to ShopItemSlot
           - Assign all UI references in inspector:
             * itemIcon → ItemIcon Image
             * itemNameText → ItemName TextMeshPro
             * priceText → PriceText TextMeshPro
             * purchaseButton → PurchaseButton
             * backgroundImage → Background Image
             * rarityBorder → RarityBorder Image
             * etc.
        
        3. NPC SETUP:
           - Add NPCShopSource component to NPC GameObject
           - Create ShopInventory asset and assign
           - Add ShopKeeperWindow component
           - Set useNewShopSystem = true
           - Assign shopWindowUI reference
        
        ===========================================
        EXAMPLE ITEM CREATION WORKFLOW
        ===========================================
        
        1. Create ItemDefinition:
           Assets → Create → NeonLadder → Items → Item Definition
           - Set icon sprite ← THIS IS KEY FOR VISUAL DISPLAY
           - Configure effects (CurrencyEffect, HealEffect, etc.)
           - Set buy/sell prices
           
        2. Create ShopInventory:
           Assets → Create → NeonLadder → Items → Shop Inventory
           - Add ShopItems with ItemDefinitions
           - Set stock amounts and prices
           
        3. Setup NPC:
           - Add NPCShopSource component
           - Assign ShopInventory asset
           - Configure dialogue lines
           
        4. Test:
           - Player approaches NPC
           - Shop window opens with items displayed
           - Icons from ItemDefinition.Icon render in UI
           - Purchase/sell buttons work
        
        ===========================================
        IMPORTANT NOTES
        ===========================================
        
        ✅ SPRITE ICONS:
        - ItemDefinition.Icon field contains the sprite
        - ShopItemUI.DisplayItem() assigns sprite to itemIcon.sprite
        - Make sure your ItemDefinitions have icons assigned!
        
        ✅ DYNAMIC PRICING:
        - Prices calculated from ItemDefinition + ShopInventory multipliers
        - Rarity affects pricing automatically
        - Stock management handled automatically
        
        ✅ CURRENCY SYMBOLS:
        - Ⓜ = Meta Currency
        - Ⓟ = Permanent Currency 
        - Displayed automatically based on shop settings
        
        ✅ BACKWARDS COMPATIBILITY:
        - Set useNewShopSystem = false to use old text-only system
        - Both systems can coexist during transition
        
        ✅ TESTING:
        - Run menu: NeonLadder → Items → Create Default Item Definitions
        - This creates test ItemDefinitions with effects
        - Create a test ShopInventory with these items
        */
    }
}