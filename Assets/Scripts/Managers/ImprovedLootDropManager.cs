using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Events;
using NeonLadder.Core;
using NeonLadder.Items.Core;
using NeonLadder.Items.Loot;
using NeonLadder.Items;
using NeonLadder.Items.Enums;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Improved loot drop manager that uses events and the new item system
    /// </summary>
    public class ImprovedLootDropManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float dropDelay = 0.5f;
        [SerializeField] private float dropSpreadRadius = 1.5f;
        [SerializeField] private bool usePhysicsScatter = true;
        [SerializeField] private float scatterForce = 5f;
        
        [Header("Legacy Support")]
        [SerializeField] private bool supportLegacyTables = true;
        
        private static ImprovedLootDropManager instance;
        public static ImprovedLootDropManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ImprovedLootDropManager>();
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
        }
        
        /// <summary>
        /// Drop loot from an improved loot table
        /// </summary>
        public void DropLoot(ImprovedLootTable lootTable, Vector3 position, Player player = null)
        {
            if (lootTable == null) return;
            
            // Calculate modifiers
            float playerLuck = CalculatePlayerLuck(player);
            ItemTier playerTier = CalculatePlayerTier(player);
            float healthPercent = player != null && player.Health != null ? 
                (float)player.Health.current / player.Health.max : 1f;
            
            // Generate loot
            List<ItemInstance> items = lootTable.GenerateLoot(playerLuck, playerTier, healthPercent);
            
            // Spawn items with delay
            StartCoroutine(SpawnItemsWithDelay(items, position));
        }
        
        /// <summary>
        /// Drop loot from a legacy loot table (backwards compatibility)
        /// </summary>
        public void DropLegacyLoot(Player target, Enemy enemy)
        {
            if (!supportLegacyTables || enemy.RuntimeLootTable == null)
                return;
            
            List<ItemInstance> items = new List<ItemInstance>();
            
            foreach (var dropGroup in enemy.RuntimeLootTable.dropGroups)
            {
                int itemsToDrop = Random.Range(dropGroup.minDrops, dropGroup.maxDrops + 1);
                
                foreach (var lootItem in dropGroup.lootItems)
                {
                    if (itemsToDrop <= 0)
                        break;
                    
                    if (ShouldDropLegacyItem(lootItem, target))
                    {
                        // Convert legacy loot item to ItemInstance
                        var instance = ConvertLegacyLootItem(lootItem);
                        if (instance != null)
                        {
                            items.Add(instance);
                            itemsToDrop--;
                        }
                    }
                }
            }
            
            // Spawn items after death animation
            float delay = enemy.DeathAnimationDuration + enemy.deathBuffer;
            StartCoroutine(SpawnItemsWithDelay(items, enemy.transform.position, delay));
        }
        
        /// <summary>
        /// Spawn items in the world using events
        /// </summary>
        private IEnumerator SpawnItemsWithDelay(List<ItemInstance> items, Vector3 position, float delay = 0f)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            yield return new WaitForSeconds(dropDelay);
            
            int itemCount = items.Count;
            for (int i = 0; i < itemCount; i++)
            {
                var item = items[i];
                if (item == null || item.Definition == null) continue;
                
                // Calculate spawn position with spread
                Vector3 spawnPos = position;
                if (itemCount > 1)
                {
                    // Arrange items in a circle
                    float angle = (360f / itemCount) * i;
                    Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * dropSpreadRadius;
                    spawnPos += offset;
                }
                
                // Create and schedule spawn event
                var spawnEvent = Simulation.Schedule<ItemSpawnEvent>();
                spawnEvent.itemInstance = item;
                spawnEvent.spawnPosition = spawnPos;
                spawnEvent.applyRandomForce = usePhysicsScatter;
                spawnEvent.randomForceStrength = scatterForce;
                
                // Small delay between spawns for visual effect
                if (i < itemCount - 1)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        
        /// <summary>
        /// Drop a single item at a position
        /// </summary>
        public void DropSingleItem(ItemDefinition itemDef, Vector3 position, int stacks = 1)
        {
            if (itemDef == null) return;
            
            var instance = new ItemInstance(itemDef, stacks);
            var spawnEvent = Simulation.Schedule<ItemSpawnEvent>();
            spawnEvent.itemInstance = instance;
            spawnEvent.spawnPosition = position;
            spawnEvent.applyRandomForce = usePhysicsScatter;
            spawnEvent.randomForceStrength = scatterForce;
        }
        
        /// <summary>
        /// Drop multiple items at once
        /// </summary>
        public void DropMultipleItems(List<ItemInstance> items, Vector3 position)
        {
            StartCoroutine(SpawnItemsWithDelay(items, position));
        }
        
        #region Helper Methods
        
        private float CalculatePlayerLuck(Player player)
        {
            // TODO: Implement luck calculation based on player stats/equipment
            // For now, return base luck
            return 1f;
        }
        
        private ItemTier CalculatePlayerTier(Player player)
        {
            // TODO: Implement tier calculation based on progression
            // For now, return Tier 1
            return ItemTier.Tier1;
        }
        
        private bool ShouldDropLegacyItem(LootItem lootItem, Player target)
        {
            if (lootItem.AlwaysDrop)
                return true;
            
            bool passesDropChance = Random.Range(0f, 100f) <= lootItem.dropProbability;
            bool belowHealthThreshold = target.Health.current / target.Health.max <= lootItem.healthThreshold;
            
            return passesDropChance && belowHealthThreshold;
        }
        
        private ItemInstance ConvertLegacyLootItem(LootItem lootItem)
        {
            // For legacy items, we need to find or create ItemDefinitions
            // This is a temporary solution - items should be properly migrated
            
            if (lootItem.collectiblePrefab == null)
                return null;
            
            // Try to get ItemDefinition from the collectible
            var collectible = lootItem.collectiblePrefab.GetComponent<Collectible>();
            if (collectible != null)
            {
                // Check if it has a fallback definition
                var definition = collectible.GetType().GetField("fallbackDefinition", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(collectible) as ItemDefinition;
                
                if (definition != null)
                {
                    int amount = Random.Range(lootItem.minAmount, lootItem.maxAmount + 1);
                    return new ItemInstance(definition, amount);
                }
            }
            
            // If no definition found, we can't convert
            Debug.LogWarning($"Cannot convert legacy loot item - no ItemDefinition found for {lootItem.collectiblePrefab.name}");
            return null;
        }
        
        #endregion
    }
}