using UnityEngine;
using System.Collections.Generic;
using NeonLadder.Items.Core;
using NeonLadder.Items.Enums;

namespace NeonLadder.Items.Loot
{
    /// <summary>
    /// Improved loot table that works with ItemDefinitions instead of prefabs
    /// </summary>
    [CreateAssetMenu(fileName = "New Improved Loot Table", menuName = "NeonLadder/Items/Improved Loot Table")]
    public class ImprovedLootTable : ScriptableObject
    {
        [System.Serializable]
        public class LootEntry
        {
            [Tooltip("The item definition to drop")]
            public ItemDefinition itemDefinition;
            
            [Tooltip("Weight for weighted random selection")]
            public float dropWeight = 1f;
            
            [Tooltip("Minimum stack size when dropped")]
            public int minStacks = 1;
            
            [Tooltip("Maximum stack size when dropped")]
            public int maxStacks = 1;
            
            [Tooltip("Player health threshold (0-1) below which this can drop")]
            public float healthThreshold = 1f; // 1 = always can drop
            
            [Tooltip("Additional drop chance modifier (0-100)")]
            public float dropChance = 100f;
            
            [Tooltip("Required player level/tier to drop this item")]
            public ItemTier requiredTier = ItemTier.Tier1;
        }
        
        [System.Serializable]
        public class LootPool
        {
            public string poolName = "Common Pool";
            public List<LootEntry> entries = new List<LootEntry>();
            
            [Tooltip("Guaranteed number of items to drop from this pool")]
            public int guaranteedDrops = 0;
            
            [Tooltip("Additional rolls for items from this pool")]
            public int bonusRolls = 1;
            
            [Tooltip("Chance (0-100) to roll this pool at all")]
            public float poolChance = 100f;
        }
        
        [Header("Loot Configuration")]
        [SerializeField] private List<LootPool> lootPools = new List<LootPool>();
        
        [Header("Global Modifiers")]
        [Tooltip("Global luck modifier applied to all drops")]
        [SerializeField] private float baseLuckModifier = 1f;
        
        [Tooltip("Increase rare drops based on player progression")]
        [SerializeField] private bool scaleWithProgression = true;
        
        [Tooltip("Chance to upgrade rarity (0-100)")]
        [SerializeField] private float rarityUpgradeChance = 5f;
        
        /// <summary>
        /// Generate loot items based on this table
        /// </summary>
        public List<ItemInstance> GenerateLoot(float playerLuck = 1f, ItemTier playerTier = ItemTier.Tier1, float playerHealthPercent = 1f)
        {
            List<ItemInstance> loot = new List<ItemInstance>();
            float totalLuck = baseLuckModifier * playerLuck;
            
            foreach (var pool in lootPools)
            {
                // Check if pool should be rolled
                if (Random.Range(0f, 100f) > pool.poolChance)
                    continue;
                
                // Process guaranteed drops
                for (int i = 0; i < pool.guaranteedDrops; i++)
                {
                    var item = SelectItemFromPool(pool, playerTier, playerHealthPercent, totalLuck);
                    if (item != null)
                        loot.Add(item);
                }
                
                // Process bonus rolls
                for (int i = 0; i < pool.bonusRolls; i++)
                {
                    // Luck affects bonus roll chance
                    if (Random.Range(0f, 100f) <= (50f * totalLuck))
                    {
                        var item = SelectItemFromPool(pool, playerTier, playerHealthPercent, totalLuck);
                        if (item != null)
                            loot.Add(item);
                    }
                }
            }
            
            return loot;
        }
        
        /// <summary>
        /// Select a single item from a loot pool
        /// </summary>
        private ItemInstance SelectItemFromPool(LootPool pool, ItemTier playerTier, float playerHealthPercent, float luck)
        {
            // Filter valid entries
            List<LootEntry> validEntries = new List<LootEntry>();
            float totalWeight = 0f;
            
            foreach (var entry in pool.entries)
            {
                if (entry.itemDefinition == null)
                    continue;
                    
                // Check tier requirement
                if (entry.requiredTier > playerTier)
                    continue;
                    
                // Check health threshold
                if (playerHealthPercent > entry.healthThreshold)
                    continue;
                    
                // Check drop chance
                if (Random.Range(0f, 100f) > entry.dropChance)
                    continue;
                
                validEntries.Add(entry);
                
                // Apply luck to weight for rarer items
                float weightModifier = 1f;
                if (entry.itemDefinition.Rarity >= ItemRarity.Rare)
                {
                    weightModifier = luck;
                }
                
                totalWeight += entry.dropWeight * weightModifier;
            }
            
            if (validEntries.Count == 0 || totalWeight <= 0)
                return null;
            
            // Weighted random selection
            float roll = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            foreach (var entry in validEntries)
            {
                float weightModifier = 1f;
                if (entry.itemDefinition.Rarity >= ItemRarity.Rare)
                {
                    weightModifier = luck;
                }
                
                currentWeight += entry.dropWeight * weightModifier;
                
                if (roll <= currentWeight)
                {
                    // Create item instance
                    int stacks = Random.Range(entry.minStacks, entry.maxStacks + 1);
                    var instance = new ItemInstance(entry.itemDefinition, stacks);
                    
                    // Check for rarity upgrade
                    if (scaleWithProgression && Random.Range(0f, 100f) < rarityUpgradeChance * luck)
                    {
                        // This could upgrade the item to a better version
                        // For now, just add bonus stacks
                        instance.AddStacks(Random.Range(1, 3));
                    }
                    
                    return instance;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Get all possible items this table can drop (for UI/preview)as
        /// </summary>
        public List<ItemDefinition> GetAllPossibleDrops()
        {
            HashSet<ItemDefinition> items = new HashSet<ItemDefinition>();
            
            foreach (var pool in lootPools)
            {
                foreach (var entry in pool.entries)
                {
                    if (entry.itemDefinition != null)
                    {
                        items.Add(entry.itemDefinition);
                    }
                }
            }
            
            return new List<ItemDefinition>(items);
        }
    }
}