using UnityEngine;
using UnityEngine.Events;
using NeonLadder.Events;
using NeonLadder.Mechanics.Progression;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Debugging;
using System;

namespace NeonLadder.Models
{
    [CreateAssetMenu(fileName = "New Purchasable Item", menuName = "NeonLadder/Items/Purchasable Item")]
    public class PurchasableItem : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private string flavorText;
        [SerializeField] private Sprite icon;
        [SerializeField] private Sprite shopIcon; // Different icon for shop display
        
        [Header("Currency & Cost")]
        [SerializeField] private NeonLadder.Mechanics.Progression.CurrencyType currencyType;
        [SerializeField] private int baseCost;
        [SerializeField] private bool canPurchaseMultiple = false;
        [SerializeField] private int maxPurchases = 1;
        
        [Header("Item Type & Effects")]
        [SerializeField] private ItemType itemType;
        [SerializeField] private ItemEffect[] effects;
        
        [Header("Prerequisites & Availability")]
        [SerializeField] private string[] requiredUnlocks;
        [SerializeField] private bool isAvailableInMetaShop = true;
        [SerializeField] private bool isAvailableInPermaShop = false;
        
        [Header("Visual & Audio")]
        [SerializeField] private GameObject purchaseVFX;
        [SerializeField] private AudioClip purchaseSound;
        [SerializeField] private Color rarityColor = Color.white;
        
        // Runtime state
        private int timesPurchased = 0;
        
        // Properties
        public string ItemId => string.IsNullOrEmpty(itemId) ? name : itemId;
        public string ItemName => string.IsNullOrEmpty(itemName) ? name : itemName;
        public string Description => string.IsNullOrEmpty(description) ? "No description available" : description;
        public string FlavorText => flavorText;
        public Sprite Icon => icon;
        public Sprite ShopIcon => shopIcon != null ? shopIcon : icon;
        public NeonLadder.Mechanics.Progression.CurrencyType CurrencyType => currencyType;
        public int Cost => baseCost > 0 ? CalculateCost() : 1;
        public ItemType Type => itemType;
        public bool CanPurchase => timesPurchased < maxPurchases;
        public int TimesPurchased => timesPurchased;
        public int MaxPurchases => maxPurchases > 0 ? maxPurchases : 1;
        public string[] RequiredUnlocks => requiredUnlocks;
        public bool IsAvailableInMetaShop => isAvailableInMetaShop;
        public bool IsAvailableInPermaShop => isAvailableInPermaShop;
        public Color RarityColor => rarityColor;
        
        private int CalculateCost()
        {
            if (!canPurchaseMultiple) return baseCost;
            
            // Scaling cost for multiple purchases (like Slay the Spire card removal)
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.2f, timesPurchased));
        }
        
        public bool CanAfford(int availableCurrency)
        {
            return availableCurrency >= Cost && CanPurchase;
        }
        
        public void Purchase(Player player)
        {
            if (!CanPurchase)
            {
                Debugger.LogWarning(LogCategory.Progression, $"Cannot purchase {ItemName} - already at max purchases ({timesPurchased}/{maxPurchases})");
                return;
            }
            
            timesPurchased++;
            
            // Apply effects
            if (effects != null)
            {
                foreach (var effect in effects)
                {
                    effect?.Apply(player);
                }
            }
            
            // Schedule VFX and audio
            if (purchaseVFX != null)
            {
                // Could schedule a VFX event here
                Debugger.Log(LogCategory.Progression, $"Playing purchase VFX for {ItemName}");
            }
            
            if (purchaseSound != null)
            {
                // Could schedule an audio event here
                Debugger.Log(LogCategory.Progression, $"Playing purchase sound for {ItemName}");
            }
            
            Debugger.Log(LogCategory.Progression, $"Purchased {ItemName} (Purchase #{timesPurchased})");
        }
        
        public void ResetPurchases()
        {
            if (currencyType == NeonLadder.Mechanics.Progression.CurrencyType.Meta)
            {
                timesPurchased = 0;
            }
            // Perma purchases persist across runs
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }
        }
    }
    
    [Serializable]
    public enum ItemType
    {
        Consumable,     // Health/Stamina potions
        Ability,        // Double jump, dash, etc.
        Upgrade,        // Stat boosts
        Unlock,         // New content access
        Cosmetic        // Visual changes
    }
    
    [Serializable]
    public class ItemEffect
    {
        [SerializeField] private EffectType effectType;
        [SerializeField] private float value;
        [SerializeField] private string targetProperty;
        [SerializeField] private bool isPermanent = false;
        [SerializeField] private float duration = 0f; // For temporary effects
        
        public void Apply(Player player)
        {
            switch (effectType)
            {
                case EffectType.Heal:
                    player.ScheduleHealing((int)value, 0.1f);
                    break;
                    
                case EffectType.RestoreStamina:
                    player.ScheduleStaminaDamage(-(int)value, 0.1f);
                    break;
                    
                case EffectType.GrantAbility:
                    ApplyAbilityEffect(player);
                    break;
                    
                case EffectType.StatBoost:
                    ApplyStatBoost(player);
                    break;
                    
                case EffectType.CustomEvent:
                    ApplyCustomEffect(player);
                    break;
            }
        }
        
        private void ApplyAbilityEffect(Player player)
        {
            switch (targetProperty.ToLower())
            {
                case "double_jump":
                case "doublejump":
                    var mediator = player.GetComponent<PlayerStateMediator>();
                    mediator?.IncrementAvailableMidAirJumps();
                    break;
                    
                case "dash":
                    // Enable dash ability
                    Debugger.Log(LogCategory.Progression, "Granting dash ability");
                    break;
                    
                default:
                    Debugger.LogWarning(LogCategory.Progression, $"Unknown ability effect: {targetProperty}");
                    break;
            }
        }
        
        private void ApplyStatBoost(Player player)
        {
            // This would integrate with your stat system
            Debugger.Log(LogCategory.Progression, $"Applying stat boost: {targetProperty} += {value}");
            
            // Example implementations:
            switch (targetProperty.ToLower())
            {
                case "health":
                case "maxhealth":
                    // Increase max health
                    Debugger.Log(LogCategory.Progression, $"Increasing max health by {value}");
                    break;
                    
                case "speed":
                case "movespeed":
                    // Increase movement speed
                    Debugger.Log(LogCategory.Progression, $"Increasing move speed by {value}");
                    break;
                    
                case "damage":
                case "attackdamage":
                    // Increase attack damage
                    Debugger.Log(LogCategory.Progression, $"Increasing attack damage by {value}");
                    break;
            }
        }
        
        private void ApplyCustomEffect(Player player)
        {
            // Custom effect logic based on targetProperty
            Debugger.Log(LogCategory.Progression, $"Applying custom effect: {targetProperty}");
        }
    }
    
    [Serializable]
    public enum EffectType
    {
        Heal,           // Restore health
        RestoreStamina, // Restore stamina
        GrantAbility,   // Grant new ability (double jump, dash, etc.)
        StatBoost,      // Increase stats (health, damage, speed, etc.)
        CustomEvent     // Custom behavior defined by targetProperty
    }
}