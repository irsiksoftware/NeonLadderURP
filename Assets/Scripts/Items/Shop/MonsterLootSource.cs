using UnityEngine;
using NeonLadder.Items.Loot;
using NeonLadder.Items.Enums;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Managers;

namespace NeonLadder.Items.Shop
{
    /// <summary>
    /// Component for enemies that drop loot on death
    /// </summary>
    public class MonsterLootSource : MonoBehaviour, ILootSource
    {
        [Header("Loot Configuration")]
        [SerializeField] private ImprovedLootTable lootTable;
        [SerializeField] private LootSourceType sourceType = LootSourceType.Monster;
        [SerializeField] private float luckModifier = 1f;
        [SerializeField] private ItemTier requiredTier = ItemTier.Tier1;
        
        [Header("Boss Configuration")]
        [SerializeField] private bool isBoss = false;
        [SerializeField] private ImprovedLootTable guaranteedBossLoot;
        
        private Enemy enemy;
        
        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = GetComponentInParent<Enemy>();
            }
        }
        
        public ImprovedLootTable GetLootTable()
        {
            // Return boss loot if this is a boss and has special loot
            if (isBoss && guaranteedBossLoot != null)
            {
                return guaranteedBossLoot;
            }
            
            return lootTable;
        }
        
        public ShopInventory GetShopInventory()
        {
            // Monsters don't have shops
            return null;
        }
        
        public LootSourceType SourceType => isBoss ? LootSourceType.Boss : sourceType;
        
        public Transform GetDropPosition()
        {
            return transform;
        }
        
        public float GetLuckModifier()
        {
            // Bosses might have better luck modifiers
            return isBoss ? luckModifier * 1.5f : luckModifier;
        }
        
        public ItemTier GetRequiredTier()
        {
            return requiredTier;
        }
        
        /// <summary>
        /// Called when the monster dies to drop loot
        /// </summary>
        public void OnDeath()
        {
            var lootManager = ImprovedLootDropManager.Instance;
            if (lootManager != null && lootTable != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Player>();
                lootManager.DropLoot(lootTable, GetDropPosition().position, player);
            }
        }
    }
}