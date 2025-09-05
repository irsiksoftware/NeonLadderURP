using UnityEngine;
using UnityEditor;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;

namespace NeonLadder.Editor
{
    public static class GenerateShopItemsSimplified
    {
        [MenuItem("NeonLadder/Generate Basic Shop Items")]
        public static void GenerateBasicItems()
        {
            string basePath = "Assets/Resources/ScriptableObjects/Items/";

            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(basePath.TrimEnd('/')))
            {
                System.IO.Directory.CreateDirectory(Application.dataPath + "/../" + basePath);
                AssetDatabase.Refresh();
            }

            int itemsCreated = 0;

            // Create Consumables
            itemsCreated += CreateItem(basePath, "HealthPotion", "Health Potion", 
                "Restores health when consumed", ItemType.Consumable, 
                ItemRarity.Common, ItemTier.Tier1, 50, 25, true, 99);
                
            itemsCreated += CreateItem(basePath, "StaminaPotion", "Stamina Potion", 
                "Restores stamina when consumed", ItemType.Consumable, 
                ItemRarity.Common, ItemTier.Tier1, 40, 20, true, 99);
                
            itemsCreated += CreateItem(basePath, "SpeedBoost", "Speed Boost", 
                "Temporarily increases movement speed", ItemType.Consumable, 
                ItemRarity.Uncommon, ItemTier.Tier2, 100, 50, true, 20);
                
            itemsCreated += CreateItem(basePath, "DamageBoost", "Damage Boost", 
                "Temporarily increases weapon damage", ItemType.Consumable, 
                ItemRarity.Uncommon, ItemTier.Tier2, 150, 75, true, 20);
                
            itemsCreated += CreateItem(basePath, "ShieldBoost", "Shield Boost", 
                "Temporarily reduces damage taken", ItemType.Consumable, 
                ItemRarity.Rare, ItemTier.Tier2, 200, 100, true, 10);

            // Create Currency
            itemsCreated += CreateItem(basePath, "MetaCoin", "Meta Coin", 
                "Currency for this run only", ItemType.Currency, 
                ItemRarity.Common, ItemTier.Tier1, 1, 1, true, 9999);
                
            itemsCreated += CreateItem(basePath, "PermaCoin", "Perma Coin", 
                "Persistent currency across runs", ItemType.Currency, 
                ItemRarity.Uncommon, ItemTier.Tier1, 1, 1, true, 9999);

            // Create Upgrades
            itemsCreated += CreateItem(basePath, "WeaponSharpener", "Weapon Sharpener", 
                "Permanently increases weapon damage", ItemType.Upgrade, 
                ItemRarity.Rare, ItemTier.Tier3, 500, 250, false, 1);
                
            itemsCreated += CreateItem(basePath, "ArmorPlating", "Armor Plating", 
                "Permanently increases defense", ItemType.Upgrade, 
                ItemRarity.Rare, ItemTier.Tier3, 500, 250, false, 1);

            // Create Special Items
            itemsCreated += CreateItem(basePath, "LifeFragment", "Life Fragment", 
                "Collect 3 to increase max health", ItemType.Special, 
                ItemRarity.Epic, ItemTier.Tier4, 1000, 500, true, 3);
                
            itemsCreated += CreateItem(basePath, "BossToken", "Boss Token", 
                "Proof of defeating a boss", ItemType.KeyItem, 
                ItemRarity.Legendary, ItemTier.Tier5, 0, 0, true, 7);
                
            itemsCreated += CreateItem(basePath, "UpgradeCore", "Upgrade Core", 
                "Used to upgrade equipment", ItemType.Material, 
                ItemRarity.Rare, ItemTier.Tier3, 300, 150, true, 50);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✓ Successfully created {itemsCreated} basic shop items in {basePath}");
        }

        private static int CreateItem(string basePath, string itemId, string displayName, 
            string description, ItemType type, ItemRarity rarity, ItemTier tier, 
            int buyValue, int sellValue, bool stackable, int maxStack)
        {
            string assetPath = basePath + itemId + ".asset";
            
            // Check if already exists
            var existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);
            if (existing != null)
            {
                Debug.Log($"Item {itemId} already exists, skipping...");
                return 0;
            }

            // Create new item
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            
            // Use reflection to set private fields
            var itemType = item.GetType();
            var bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            itemType.GetField("itemId", bindingFlags).SetValue(item, itemId.ToLower());
            itemType.GetField("displayName", bindingFlags).SetValue(item, displayName);
            itemType.GetField("description", bindingFlags).SetValue(item, description);
            itemType.GetField("itemType", bindingFlags).SetValue(item, type);
            itemType.GetField("rarity", bindingFlags).SetValue(item, rarity);
            itemType.GetField("tier", bindingFlags).SetValue(item, tier);
            itemType.GetField("isStackable", bindingFlags).SetValue(item, stackable);
            itemType.GetField("maxStack", bindingFlags).SetValue(item, maxStack);
            itemType.GetField("buyValue", bindingFlags).SetValue(item, buyValue);
            itemType.GetField("sellValue", bindingFlags).SetValue(item, sellValue);
            
            // Set default drop weight based on rarity
            float dropWeight = rarity switch
            {
                ItemRarity.Common => 10f,
                ItemRarity.Uncommon => 5f,
                ItemRarity.Rare => 2.5f,
                ItemRarity.Epic => 1f,
                ItemRarity.Legendary => 0.5f,
                ItemRarity.Mythic => 0.1f,
                _ => 1f
            };
            itemType.GetField("baseDropWeight", bindingFlags).SetValue(item, dropWeight);

            // Create and save the asset
            AssetDatabase.CreateAsset(item, assetPath);
            Debug.Log($"Created item: {displayName} at {assetPath}");
            
            return 1;
        }
    }
}