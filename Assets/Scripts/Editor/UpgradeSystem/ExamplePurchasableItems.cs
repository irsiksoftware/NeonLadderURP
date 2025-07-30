using UnityEngine;
using UnityEditor;
using NeonLadder.Models;
using NeonLadder.Events;
using System.IO;
using static NeonLadder.Models.ActionPlatformerConstants;

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
            CreateManaPotion();
            CreateDamageBoost();
            CreateSpeedBoost();
            CreateDefenseBoost();
            
            // Basic Abilities (Perma Currency)
            CreateDoubleJump();
            CreateWallJump();
            CreateDashAbility();
            
            // Advanced Abilities (Perma Currency)
            CreateAirDash();
            CreateGlide();
            CreateGroundPound();
            
            // Combat Abilities (Perma Currency)
            CreateFireball();
            CreateShieldBlock();
            
            // Permanent Upgrades (Perma Currency)
            CreateHealthUpgrade();
            CreateSpeedUpgrade();
            CreateDamageUpgrade();
            
            // Utility Items (Perma Currency)
            CreateStartingGold();
            CreateExtraLife();
            CreateCoinMagnet();
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created {GetExampleItemCount()} example purchasable items in {AssetPath}");
        }
        
        private static int GetExampleItemCount() => 18; // Update when adding more items
        
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
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.HealthPotion);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Consumable);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 99);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Common);
            
            // Create effect
            var effects = new ItemEffect[]
            {
                CreateItemEffect(EffectType.Heal, 25f, Stats.Health)
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
        
        #region New Action Platformer Items
        
        private static void CreateManaPotion()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "mana_potion");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Mana Potion");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Restores 30 mana for special abilities");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Blue raspberry flavor. My favorite!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.ManaPotion);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Consumable);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 99);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Common);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.CustomEvent, 30f, "restore_mana") };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "ManaPotion.asset"));
        }
        
        private static void CreateDefenseBoost()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "defense_boost");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Turtle Shell Shield");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Increases defense by 25% for this run");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Cowabunga! This shell's got my back!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Meta);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.DefenseBoost);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 3);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Uncommon);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.StatBoost, 25f, Stats.DefenseRating) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "DefenseBoost.asset"));
        }
        
        private static void CreateWallJump()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "wall_jump");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Wall Jump");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Jump off walls to reach new heights");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Spider-who? I can climb walls too!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.WallJump);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Rare);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.WallJump) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "WallJump.asset"));
        }
        
        private static void CreateAirDash()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "air_dash");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Air Dash");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Dash while airborne for advanced mobility");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Like teleporting, but with style!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.AirDash);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Epic);
            
            var requiredUnlocks = new string[] { Abilities.Dash };
            type.GetField("requiredUnlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, requiredUnlocks);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.AirDash) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "AirDash.asset"));
        }
        
        private static void CreateGlide()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "glide");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Glide Wings");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Hold jump to glide slowly down from high places");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"I believe I can fly... or at least fall with style!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.Glide);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Rare);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.Glide) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "Glide.asset"));
        }
        
        private static void CreateGroundPound()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "ground_pound");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Ground Pound");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Slam down from the air to damage enemies and break blocks");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"It's-a me, Wade-io! Time to ground pound!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 250);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Uncommon);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.GroundPound) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "GroundPound.asset"));
        }
        
        private static void CreateFireball()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "fireball");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Fireball");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Launch fireballs at enemies from a distance");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Some people like it hot. I LOVE it blazing!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 350);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Epic);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.Fireball) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "Fireball.asset"));
        }
        
        private static void CreateShieldBlock()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "shield_block");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Shield Block");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Hold to block incoming attacks with reduced damage");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Captain America, eat your heart out!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 300);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Ability);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Rare);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.GrantAbility, 1f, Abilities.ShieldBlock) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "ShieldBlock.asset"));
        }
        
        private static void CreateSpeedUpgrade()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "speed_upgrade");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Speed Enhancement");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Permanently increases movement speed by 15%");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Gotta go fast! But not too fast, I still want to see the scenery.\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.SpeedUpgrade);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 5);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Uncommon);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.StatBoost, 15f, Stats.MoveSpeed) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "SpeedUpgrade.asset"));
        }
        
        private static void CreateDamageUpgrade()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "damage_upgrade");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Power Enhancement");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Permanently increases attack damage by 20%");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"More power! Always need more power!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.DamageUpgrade);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 5);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Rare);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.StatBoost, 20f, Stats.AttackDamage) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "DamageUpgrade.asset"));
        }
        
        private static void CreateExtraLife()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "extra_life");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Extra Life");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Start each run with an additional life");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Nine lives? Try infinite lives!\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.ExtraLife);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 3);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Epic);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.CustomEvent, 1f, CustomEvents.BonusLives) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "ExtraLife.asset"));
        }
        
        private static void CreateCoinMagnet()
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            var type = typeof(PurchasableItem);
            
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "coin_magnet");
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Coin Magnet");
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "Automatically collect coins from a greater distance");
            type.GetField("flavorText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, "\"Money talks, but mine screams 'Come to Papa!'\" - Wade Wilson");
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, CurrencyType.Perma);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, Costs.CoinMagnet);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, ItemType.Upgrade);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 3);
            type.GetField("isAvailableInPermaShop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, true);
            type.GetField("rarityColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, RarityColors.Uncommon);
            
            var effects = new ItemEffect[] { CreateItemEffect(EffectType.StatBoost, 50f, Stats.CoinMagnetRange) };
            type.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            AssetDatabase.CreateAsset(item, Path.Combine(AssetPath, "CoinMagnet.asset"));
        }
        
        #endregion
        
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