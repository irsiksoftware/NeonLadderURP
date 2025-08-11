using System;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Boss location data containing both display names for localization/title cards
    /// and simplified identifiers for procedural generation and scene management
    /// Extends the base LocationData with boss-specific functionality
    /// </summary>
    [Serializable]
    public class BossLocationData : LocationData
    {
        public string Boss { get; }

        public BossLocationData(string identifier, string displayName, string boss)
            : base(identifier, displayName, $"boss.{identifier.ToLower()}")
        {
            Boss = boss;
        }

        // The Seven Deadly Sins locations + Devil finale
        public static readonly BossLocationData[] Locations = new[]
        {
            new BossLocationData("Cathedral", "Grand Cathedral of Hubris", "Pride"),
            new BossLocationData("Necropolis", "The Necropolis of Vengeance", "Wrath"),
            new BossLocationData("Vault", "The Vault of Avarice", "Greed"),
            new BossLocationData("Mirage", "Envious Mirage: The Jewelry Store of Covetous Reflections", "Envy"),
            new BossLocationData("Garden", "Infinite Garden (Eden of Desires)", "Lust"),
            new BossLocationData("Banquet", "The Banquet of Infinity", "Gluttony"),
            new BossLocationData("Lounge", "The Lethargy Lounge", "Sloth"),
            new BossLocationData("Finale", "Finale", "Devil")
        };

        /// <summary>
        /// Get location data by layer index
        /// </summary>
        public static BossLocationData GetByLayer(int layerIndex)
        {
            if (layerIndex < 0 || layerIndex >= Locations.Length)
                return null;
            return Locations[layerIndex];
        }

        /// <summary>
        /// Get location data by identifier
        /// </summary>
        public static BossLocationData GetByIdentifier(string identifier)
        {
            foreach (var location in Locations)
            {
                if (location.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                    return location;
            }
            return null;
        }

        /// <summary>
        /// Get location data by boss name
        /// </summary>
        public static BossLocationData GetByBoss(string bossName)
        {
            foreach (var location in Locations)
            {
                if (location.Boss.Equals(bossName, StringComparison.OrdinalIgnoreCase))
                    return location;
            }
            return null;
        }

        public override string ToString()
        {
            return $"{Boss} at {DisplayName} ({Identifier})";
        }
    }
}