using UnityEngine;

namespace NeonLadder.Mechanics.Progression
{
    public enum CurrencyType
    {
        Meta,       // Temporary per-run currency (like Slay the Spire gold)
        Perma       // Persistent currency (like Hades Darkness for Mirror of Night)
    }
    
    public enum UpgradeCategory
    {
        // Hades-inspired categories
        Offense,    // Damage, attack speed, critical chance
        Defense,    // Health, armor, damage reduction
        Utility,    // Movement speed, dash cooldown, resource generation
        Special,    // Unique abilities, weapon-specific bonuses
        
        // Meta progression (Perma currency)
        Core,       // Base stats that carry between runs
        Unlocks,    // New weapons, abilities, areas
        Quality     // Starting bonuses, item quality improvements
    }
    
    /// <summary>
    /// Individual upgrade definition - like a single Mirror of Night or Slay the Spire relic
    /// </summary>
    public interface IUpgrade
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string FlavorText { get; }  // Wade insisted on this for personality
        
        CurrencyType CurrencyType { get; }
        UpgradeCategory Category { get; }
        
        int Cost { get; }
        int MaxLevel { get; }  // 1 for binary upgrades, higher for scaling upgrades
        int CurrentLevel { get; set; }
        
        bool IsMaxLevel { get; }
        bool IsOwned { get; }
        
        /// <summary>
        /// Prerequisites that must be owned before this upgrade becomes available
        /// Enables upgrade trees like Hades weapon aspects
        /// </summary>
        string[] Prerequisites { get; }
        
        /// <summary>
        /// Mutually exclusive upgrades (choose one path)
        /// Like Hades Mirror alternatives
        /// </summary>
        string[] MutuallyExclusive { get; }
        
        /// <summary>
        /// Apply this upgrade's effect to the player
        /// </summary>
        void ApplyEffect(GameObject player);
        
        /// <summary>
        /// Remove this upgrade's effect (for Meta upgrades on death)
        /// </summary>
        void RemoveEffect(GameObject player);
        
        /// <summary>
        /// Get the effect description for current level
        /// "Increases damage by 15%" etc.
        /// </summary>
        string GetEffectDescription();
        
        /// <summary>
        /// Get the effect description for next level (preview)
        /// "Next: Increases damage by 20%"
        /// </summary>
        string GetNextLevelDescription();
    }
}