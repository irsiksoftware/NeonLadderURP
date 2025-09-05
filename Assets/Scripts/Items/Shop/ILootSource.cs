using UnityEngine;
using NeonLadder.Items.Loot;
using NeonLadder.Items.Enums;

namespace NeonLadder.Items.Shop
{
    /// <summary>
    /// Interface for anything that can provide loot - monsters, chests, shops, NPCs
    /// </summary>
    public interface ILootSource
    {
        /// <summary>
        /// Get the loot table for drops (monsters, chests)
        /// </summary>
        ImprovedLootTable GetLootTable();
        
        /// <summary>
        /// Get the shop inventory (NPCs, vending machines)
        /// </summary>
        ShopInventory GetShopInventory();
        
        /// <summary>
        /// Type of loot source
        /// </summary>
        LootSourceType SourceType { get; }
        
        /// <summary>
        /// Position to spawn drops
        /// </summary>
        Transform GetDropPosition();
        
        /// <summary>
        /// Luck modifier for this source
        /// </summary>
        float GetLuckModifier();
        
        /// <summary>
        /// Player tier requirement
        /// </summary>
        ItemTier GetRequiredTier();
    }
    
    /// <summary>
    /// Types of loot sources in the game
    /// </summary>
    public enum LootSourceType
    {
        Monster,        // Drops on death
        Chest,          // Opens when interacted
        Shop,           // Buy/sell interface
        Quest,          // Reward for completion
        Environment,    // Breakable objects
        Boss,           // Special boss drops
        RiddleRoom      // Puzzle rewards
    }
}