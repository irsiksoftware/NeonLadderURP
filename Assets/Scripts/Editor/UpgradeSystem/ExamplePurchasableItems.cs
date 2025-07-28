using UnityEngine;
using UnityEditor;
using NeonLadder.Models;
using NeonLadder.Events;
using System.IO;

namespace NeonLadder.Editor.UpgradeSystem
{
    /// <summary>
    /// Creates example purchasable items for the enhanced purchase system
    /// Includes the double jump example and other common roguelite items
    /// </summary>
    public static class ExamplePurchasableItems
    {
        private const string AssetPath = "Assets/Data/PurchasableItems";
        
        [MenuItem("NeonLadder/Examples/Create Example Purchasable Items")]
        public static void CreateExampleItems()
        {
            CreateDirectoryIfNeeded();
            
            // Meta Currency Items (Per-Run)
            CreateHealthPotion();
            CreateStaminaPotion();
            CreateDamageBoost();
            CreateSpeedBoost();
            
            // Perma Currency Items (Persistent)
            CreateDoubleJump();
            CreateDashAbility();
            CreateHealthUpgrade();
            CreateStartingGold();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Created example purchasable items in " + AssetPath);
        }
        
        private static void CreateDirectoryIfNeeded()
        {
            if (!Directory.Exists(AssetPath))
            {
                Directory.CreateDirectory(AssetPath);
                AssetDatabase.Refresh();
            }
        }
        
        private static void CreateHealthPotion()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            // Use reflection to set private fields
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "health_potion");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Health Potion");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Restores 25 health instantly");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"A classic red potion. Tastes like victory.\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 25);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Consumable);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 99);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.green);
            
            // Create effect
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.Heal, 25f, "health")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "HealthPotion.asset"));
        }
        
        private static void CreateStaminaPotion()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "stamina_potion");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Stamina Potion");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Restores 20 stamina instantly");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Energy drink? More like energy DRINK!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 20);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Consumable);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 99);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.blue);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.RestoreStamina, 20f, "stamina")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "StaminaPotion.asset"));
        }
        
        private static void CreateDamageBoost()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "damage_boost");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Berserker's Might");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Increases damage by 15% for this run");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"More damage equals more fun. It's simple math!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 75);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 3);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.red);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.StatBoost, 15f, "damage")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "DamageBoost.asset"));
        }
        
        private static void CreateSpeedBoost()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "speed_boost");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Swift Stride");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Increases movement speed by 20% for this run");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Gotta go fast! Wait, wrong franchise...\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 50);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 2);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.cyan);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.StatBoost, 20f, "speed")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "SpeedBoost.asset"));
        }
        
        private static void CreateDoubleJump()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "double_jump");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Double Jump");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Grants the ability to jump once while in mid-air");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Defying physics since... well, since video games!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 150);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.yellow);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.GrantAbility, 1f, "double_jump")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "DoubleJump.asset"));
        }
        
        private static void CreateDashAbility()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "dash_ability");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Shadow Dash");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Grants the ability to dash through enemies and obstacles");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Teleports behind you... 'Nothing personnel, kid'\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 300);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.magenta);
            
            // Requires double jump as prerequisite
            var requiredUnlocks = new string[] { "double_jump" };
            type.GetField("requiredUnlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, requiredUnlocks);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.GrantAbility, 1f, "dash")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "DashAbility.asset"));
        }
        
        private static void CreateHealthUpgrade()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "health_upgrade");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Vitality Enhancement");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Permanently increases maximum health by 25");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"More health means more mistakes I can make!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 100);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 10);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.green);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.StatBoost, 25f, "maxhealth")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "HealthUpgrade.asset"));
        }
        
        private static void CreateStartingGold()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "starting_gold");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Inheritance");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Start each run with +50 Meta currency");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"My rich uncle finally paid up!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 200);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 5);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Color.yellow);
            
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.CustomEvent, 50f, "starting_meta_currency")
            };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "StartingGold.asset"));
        }
        
        private static ItemEffect CreateItemEffect(EffectType effectType, float value, string targetProperty)
        {
            var effect = new ItemEffect();
            var type = typeof(ItemEffect);
            
            type.GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, effectType);
            type.GetField("value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, value);
            type.GetField("targetProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, targetProperty);
            type.GetField("isPermanent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, true);
            
            return effect;
        }
    }
}