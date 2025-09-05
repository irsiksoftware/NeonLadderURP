namespace NeonLadder.Items.Enums
{
    /// <summary>
    /// Defines the category of an item, determining its behavior and usage
    /// </summary>
    public enum ItemType
    {
        Currency,       // Meta and Perma currencies
        Consumable,     // Health potions, temporary buffs
        Relic,          // Permanent passive effects (Slay the Spire style)
        Artifact,       // Activated abilities with cooldowns
        Equipment,      // Weapons, armor pieces
        KeyItem,        // Quest/progression items
        Material,       // Crafting/upgrade materials
        Upgrade,        // Permanent upgrade items
        Special         // Special items with unique effects
    }

    /// <summary>
    /// Rarity tier affecting drop rates and item power
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,     // Gray - 60% drop rate
        Uncommon = 1,   // Green - 30% drop rate
        Rare = 2,       // Blue - 8% drop rate
        Epic = 3,       // Purple - 1.8% drop rate
        Legendary = 4,  // Orange - 0.2% drop rate
        Mythic = 5      // Red - Special conditions only
    }

    /// <summary>
    /// Progression tier for gating items by player advancement
    /// </summary>
    public enum ItemTier
    {
        Tier1 = 1,  // Early game
        Tier2 = 2,  // Mid game
        Tier3 = 3,  // Late game
        Tier4 = 4,  // End game
        Tier5 = 5   // Post-game/NG+
    }

    /// <summary>
    /// Currency types in the game
    /// </summary>
    public enum CurrencyType
    {
        Meta,       // Run-specific currency
        Permanent   // Persistent across runs
    }
}