using UnityEngine;
using UnityEditor;
using NeonLadder.Items.Shop;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;

namespace NeonLadder.Editor
{
    public static class SetupShopInventory
    {
        [MenuItem("NeonLadder/Setup Dispensable Goods Shop")]
        public static void SetupDispensableGoodsShop()
        {
            string shopPath = "Assets/Resources/ScriptableObjects/Shops/DispensableGoodsShop.asset";
            string itemsPath = "Assets/Resources/ScriptableObjects/Items/";
            
            // Ensure directory exists
            string shopDir = System.IO.Path.GetDirectoryName(shopPath);
            if (!AssetDatabase.IsValidFolder(shopDir))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/../" + shopDir);
                AssetDatabase.Refresh();
            }
            
            // Create or load shop inventory
            ShopInventory shopInventory = AssetDatabase.LoadAssetAtPath<ShopInventory>(shopPath);
            if (shopInventory == null)
            {
                shopInventory = ScriptableObject.CreateInstance<ShopInventory>();
                AssetDatabase.CreateAsset(shopInventory, shopPath);
            }
            
            // Configure shop settings using reflection
            var type = typeof(ShopInventory);
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            type.GetField("shopId", bindingFlags).SetValue(shopInventory, "dispensable_goods");
            type.GetField("shopName", bindingFlags).SetValue(shopInventory, "Dispensable Goods");
            type.GetField("shopDescription", bindingFlags).SetValue(shopInventory, "Your one-stop shop for all your combat and survival needs!");
            type.GetField("acceptedCurrency", bindingFlags).SetValue(shopInventory, CurrencyType.Meta);
            type.GetField("buyPriceMultiplier", bindingFlags).SetValue(shopInventory, 1.2f);
            type.GetField("sellPriceMultiplier", bindingFlags).SetValue(shopInventory, 0.5f);
            type.GetField("restockOnRun", bindingFlags).SetValue(shopInventory, true);
            type.GetField("restockOnFloor", bindingFlags).SetValue(shopInventory, false);
            type.GetField("scaleWithProgression", bindingFlags).SetValue(shopInventory, true);
            
            // Create shop items list
            var shopItems = new List<ShopInventory.ShopItem>();
            
            // Add consumables
            AddShopItem(shopItems, itemsPath + "HealthPotion.asset", 10, 75);
            AddShopItem(shopItems, itemsPath + "StaminaPotion.asset", 10, 60);
            AddShopItem(shopItems, itemsPath + "SpeedBoost.asset", 5, 100);
            AddShopItem(shopItems, itemsPath + "DamageBoost.asset", 3, 200);
            AddShopItem(shopItems, itemsPath + "ShieldBoost.asset", 5, 120);
            
            // Add upgrades (limited stock)
            AddShopItem(shopItems, itemsPath + "WeaponSharpener.asset", 1, 600, requiresTier: ItemTier.Tier2);
            AddShopItem(shopItems, itemsPath + "ArmorPlating.asset", 1, 550, requiresTier: ItemTier.Tier2);
            
            // Add special items
            AddShopItem(shopItems, itemsPath + "LifeFragment.asset", 1, 1000, requiresTier: ItemTier.Tier3, availabilityChance: 50f);
            AddShopItem(shopItems, itemsPath + "BossToken.asset", -1, 0, alwaysAvailable: false); // Not for sale, just for display
            AddShopItem(shopItems, itemsPath + "UpgradeCore.asset", 2, 400, requiresTier: ItemTier.Tier2);
            
            // Set the shop items list using reflection
            type.GetField("stockedItems", bindingFlags).SetValue(shopInventory, shopItems);
            
            // Save the changes
            EditorUtility.SetDirty(shopInventory);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"✓ Dispensable Goods Shop configured with {shopItems.Count} items at {shopPath}");
            
            // Now try to find and update the NPCShopSource in the scene
            UpdateSceneShopReferences(shopInventory);
        }
        
        private static void AddShopItem(List<ShopInventory.ShopItem> shopItems, string itemPath, 
            int stock = -1, float priceOverride = -1f, ItemTier requiresTier = ItemTier.Tier1, 
            bool alwaysAvailable = true, float availabilityChance = 100f)
        {
            var itemDef = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
            if (itemDef == null)
            {
                Debug.LogWarning($"Item not found at {itemPath}");
                return;
            }
            
            var shopItem = new ShopInventory.ShopItem
            {
                itemDefinition = itemDef,
                baseStock = stock,
                restockAmount = stock > 0 ? stock / 2 : 1,
                priceOverride = priceOverride,
                requiresUnlock = false,
                alwaysAvailable = alwaysAvailable,
                availabilityChance = availabilityChance,
                requiredPlayerTier = requiresTier
            };
            
            shopItems.Add(shopItem);
        }
        
        private static void UpdateSceneShopReferences(ShopInventory shopInventory)
        {
            // Find all NPCShopSource components in the scene
            var shopSources = GameObject.FindObjectsOfType<NPCShopSource>();
            
            foreach (var shopSource in shopSources)
            {
                // Check if this is the Dispensable Goods shop
                if (shopSource.name.Contains("Dispensable") || shopSource.name.Contains("Shop"))
                {
                    // Set the shop inventory using reflection
                    var type = typeof(NPCShopSource);
                    var field = type.GetField("shopInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(shopSource, shopInventory);
                        EditorUtility.SetDirty(shopSource);
                        Debug.Log($"✓ Updated shop inventory reference for {shopSource.name}");
                    }
                }
            }
            
            // Also check for ShopKeeperWindow components
            var shopWindows = GameObject.FindObjectsOfType<ShopKeeperWindow>();
            foreach (var window in shopWindows)
            {
                // Make sure it has an NPCShopSource component
                var shopSource = window.GetComponent<NPCShopSource>();
                if (shopSource == null)
                {
                    shopSource = window.gameObject.AddComponent<NPCShopSource>();
                }
                
                // Set the shop inventory
                var type = typeof(NPCShopSource);
                var field = type.GetField("shopInventory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(shopSource, shopInventory);
                    
                    // Also set some default dialogue
                    type.GetField("shopKeeperName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(shopSource, "Vendor");
                    type.GetField("greetingLines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(shopSource, new string[] { 
                            "Welcome to Dispensable Goods!",
                            "Looking to survive the climb?",
                            "Best prices in the Neon Ladder!"
                        });
                    type.GetField("purchaseLines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(shopSource, new string[] { 
                            "Excellent choice!",
                            "That'll keep you alive.",
                            "May it serve you well."
                        });
                    type.GetField("cantAffordLines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(shopSource, new string[] { 
                            "You'll need more coin for that.",
                            "Come back when you're... richer.",
                            "No credit in the Neon Ladder."
                        });
                    type.GetField("farewellLines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(shopSource, new string[] { 
                            "Good luck out there!",
                            "Stay alive, climber.",
                            "See you next run... maybe."
                        });
                    
                    EditorUtility.SetDirty(shopSource);
                    Debug.Log($"✓ Added NPCShopSource to {window.name} with shop inventory");
                }
            }
        }
    }
}