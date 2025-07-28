using System;

namespace NeonLadder.Mechanics.Progression
{
    [Serializable]
    public enum UpgradeCategory
    {
        Offense,    // Damage, crit, attack speed
        Defense,    // Health, armor, resistance
        Utility,    // Movement, cooldowns, convenience
        Special,    // Unique abilities, synergies
        Core,       // Base stats (Perma upgrades)
        Unlocks,    // New content access (Perma upgrades)
        Quality     // Starting bonuses (Perma upgrades)
    }
}