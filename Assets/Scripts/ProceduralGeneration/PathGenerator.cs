using NeonLadder.Common;
using NeonLadder.Mechanics.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;            // For string-building / encoding if needed
using UnityEngine;           // For Random, etc.
using Newtonsoft.Json;
using UnityEngine.UI;       // Example of JSON serialization library (Newtonsoft)

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Represents an entire procedural path generation system.
    /// This version can accept or generate a seed string for reproducibility.
    /// </summary>
    public class PathGenerator //: InputField
    {
        private static readonly int[] EncounterProbabilities = { 25, 50, 75 }; // Adjusted to only roundable to 25
        private static readonly string[] Bosses = BossTransformations.bossTransformations.Keys.ToArray();

        public Dictionary<string, string> BossLocations;
        public void SetCurrentSeed(string seed)
        {
            CurrentSeed = seed;
        }

          // QUESTION: Do we want to store the seed for reference?
        // If so, keep a private or public field here.
        public string CurrentSeed { get { return Constants.Seed; } set { Constants.Seed = value; } }

        /// <summary>
        /// If BossLocations is not initialized, sets up a default map from Boss name -> "Location" string.
        /// </summary>
        private void EnsureBossLocationsInitialized()
        {
            if (BossLocations == null)
            {
                var locations = new[]
                {
                    "Grand Cathedral of Hubris",
                    "The Necropolis of Vengeance",
                    "The Vault of Avarice",
                    "The Community Center",
                    "Infinite Forest (Eden of Desires)",
                    "The Feast of Infinity",
                    "The Lethargy Lounge",
                    "The Final Level"
                };

                BossLocations = Bosses
                    .Select((boss, index) => new { boss, location = locations.ElementAtOrDefault(index) ?? "Unknown" })
                    .ToDictionary(x => x.boss.ToLower(), x => x.location);
            }
        }

        /// <summary>
        /// The main entry point that uses a provided seed to generate paths deterministically.
        /// </summary>
        /// <param name="seedString">An optional alphanumeric or other string that can be hashed to generate the RNG seed. or a NULL generates a new one</param>
        /// <returns></returns>
        public Dictionary<string, Path> GeneratePaths()
        {
            // Use the provided seed or generate a new one
            this.CurrentSeed = (string.IsNullOrWhiteSpace(Constants.Seed) || 
                                Constants.Seed.ToLower() == "seed" || 
                                Constants.LastSeed == Constants.Seed) 
                                ? GenerateRandomSeed(12) 
                                : Constants.Seed;// Store the seed

            Constants.LastSeed = Constants.Seed;

            // Convert the seed string to an integer for Random.InitState
            int seedValue = ConvertSeedToInt(CurrentSeed);

            // Initialize Unity's RNG with the computed seed
            UnityEngine.Random.InitState(seedValue);

            // Proceed with your normal generation logic
            EnsureBossLocationsInitialized();

            var finalBoss = Mechanics.Enums.Bosses.Devil.ToString();
            var paths = new Dictionary<string, Path>();

            var remainingBosses = Bosses.Except(Constants.DefeatedBosses).ToList();
            int pathCount = (remainingBosses.Count > 3) ? 3 : remainingBosses.Count;

            var activeBosses = remainingBosses
                .Where(rb => rb != finalBoss)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(pathCount);

            // Add the final path only if all other bosses are defeated
            if (Constants.DefeatedBosses.Count == 7 && !Constants.DefeatedBosses.Contains(finalBoss))
            {
                paths[finalBoss.ToLower()] = GeneratePath(finalBoss);
            }
            else
            {
                foreach (var boss in activeBosses)
                {
                    paths[boss.ToLower()] = GeneratePath(boss);
                }
            }

            return paths;
        }


        /// <summary>
        /// Demonstrates how you might convert an alphanumeric seed string to an integer for Random.InitState.
        /// This example does a simple hash, but you can do more sophisticated conversions if desired.
        /// </summary>
        /// <param name="seedString">String seed input</param>
        /// <returns>An integer to use in Random.InitState</returns>
        private int ConvertSeedToInt(string seedString)
        {
            unchecked
            {
                // Simple Fowler–Noll–Vo (FNV) or any other hash approach
                // This is just an example. 
                // Or you could parse as Base36, Base64, etc.
                int hash = 23;
                foreach (char c in seedString)
                {
                    hash = (hash * 31) + c;
                }
                return hash;
            }
        }

        /// <summary>
        /// Example of generating a random alphanumeric seed of specified length.
        /// This can be used whenever we need a brand-new seed that is "non-human-readable".
        /// </summary>
        /// <param name="length">Length of the random seed string.</param>
        /// <returns>An alphanumeric string seed.</returns>
        public string GenerateRandomSeed(int length = 16)
        {
            // QUESTION: Should this be truly random each time (e.g., using System.Security.Cryptography)?
            // Or can we rely on UnityEngine.Random for convenience?
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            // If you want cryptographically-strong random, consider using System.Security.Cryptography RNGCryptoServiceProvider.
            // For most in-game seeds, Unity's pseudo-random is often sufficient.

            // We'll do a quick approach using UnityEngine.Random
            StringBuilder sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                int index = UnityEngine.Random.Range(0, chars.Length);
                sb.Append(chars[index]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Core generation logic for a single path leading to a specific boss.
        /// Called internally after the seed is set.
        /// </summary>
        private Path GeneratePath(string boss)
        {
            EnsureBossLocationsInitialized();

            var path = new Path
            {
                Location = boss,
                Nodes = new List<PathNode>()
            };

            var nodes = new List<PathNode> {
                GenerateMixedEncounterNode("transition-1", GenerateFixedEncounterProbabilities()),
                GenerateMixedEncounterNode("transition-2", GenerateFixedEncounterProbabilities()),
                GenerateEventNode("transition-3", GetRandomEventType()),
                GenerateRestAndShopNode("transition-4"),
                GenerateBossNode(boss)
            };

            path.Nodes.AddRange(nodes);

            return path;
        }

        /// <summary>
        /// Spawns an encounter node containing a mix of MinorEnemy/MajorEnemy with certain probabilities.
        /// </summary>
        private static PathNode GenerateMixedEncounterNode(string gameObjectName, Dictionary<EncounterType, int> encounterProbabilities)
        {
            var description = string.Join(", ", encounterProbabilities.Select(e => $"{e.Key}-{e.Value}%"));
            return new PathNode
            {
                GameObjectName = gameObjectName,
                Encounter = new Encounter
                {
                    Type = null, // Mixed type
                    Probability = 100,
                    Description = description
                }
            };
        }

        /// <summary>
        /// Generates a dictionary of EncounterType->percent for MinorEnemy vs MajorEnemy.
        /// Probabilities sum to 100.
        /// </summary>
        private static Dictionary<EncounterType, int> GenerateFixedEncounterProbabilities()
        {
            var probabilities = new Dictionary<EncounterType, int>();
            var availableProbabilities = EncounterProbabilities
                .OrderBy(_ => UnityEngine.Random.value)
                .ToArray();

            probabilities[EncounterType.MinorEnemy] = availableProbabilities[0];
            probabilities[EncounterType.MajorEnemy] = 100 - probabilities[EncounterType.MinorEnemy];

            return probabilities;
        }

        /// <summary>
        /// Spawns an event node (e.g., TreasureChest, MysteriousAltar, Riddle).
        /// </summary>
        private static PathNode GenerateEventNode(string gameObjectName, EventType eventType)
        {
            return new PathNode
            {
                GameObjectName = gameObjectName,
                Encounter = new Encounter
                {
                    EventType = eventType,
                    Probability = 100,
                    Description = eventType switch
                    {
                        EventType.TreasureChest => "A treasure chest containing loot.",
                        EventType.MysteriousAltar => "A mysterious altar offering a choice.",
                        EventType.Riddle => "A statue challenges you with a riddle.",
                        _ => "An unknown event occurs."
                    }
                }
            };
        }

        /// <summary>
        /// Spawns a node that includes a rest stop and a shop.
        /// </summary>
        private static PathNode GenerateRestAndShopNode(string gameObjectName)
        {
            return new PathNode
            {
                GameObjectName = gameObjectName,
                Encounter = new Encounter
                {
                    Type = EncounterType.RestShop,
                    Probability = 100,
                    Description = "A rest stop and shop are available before the boss."
                }
            };
        }

        /// <summary>
        /// Spawns a node containing the final boss encounter for a path.
        /// </summary>
        private static PathNode GenerateBossNode(string boss)
        {
            return new PathNode
            {
                GameObjectName = $"boss-{boss.ToUpper()}",
                Encounter = new Encounter
                {
                    Type = EncounterType.MajorEnemy,
                    Probability = 100,
                    Description = $"The final boss of this path: {boss.ToUpper()}.",
                }
            };
        }

        /// <summary>
        /// Returns a random EventType from the enumeration.
        /// </summary>
        private static EventType GetRandomEventType()
        {
            var values = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();
            return values[UnityEngine.Random.Range(0, values.Count)];
        }

        // OPTIONAL: If you want to store the final dictionary in a single string, you can serialize to JSON.
        // This approach allows you to save the *entire* resulting paths—useful if you want to 
        // guarantee the exact same run without re-calling random, or if code changes in the future.
        // Also helpful for debugging or sharing a "snapshot" of the run beyond just the seed.
        public string SerializePathsToJson(Dictionary<string, Path> paths)
        {
            return JsonConvert.SerializeObject(paths);
        }

        /// <summary>
        /// OPTIONAL: For re-loading a run from a JSON snapshot (instead of re-generating from seed).
        /// This ensures the exact same data is used, even if your generation logic changes.
        /// </summary>
        public Dictionary<string, Path> DeserializePathsFromJson(string json)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, Path>>(json);
        }
    }

    // Enum for encounter types
    public enum EncounterType
    {
        MajorEnemy,  // "ME": 1-3 larger, more powerful foes
        MinorEnemy,  // "mE": 1-5 smaller, less dangerous foes
        RestShop     // Combined Rest and Shop encounter
    }

    // Enum for event types
    public enum EventType
    {
        TreasureChest,    // Treasure chest containing loot
        MysteriousAltar,  // A mysterious altar offering a choice
        Riddle            // Statue challenges you with a riddle
    }

    // Classes for representing the structure
    public class Path
    {
        public string Location { get; set; }
        public List<PathNode> Nodes { get; set; } = new List<PathNode>();
    }

    public class PathNode
    {
        // QUESTION: Currently all nodes default to "transition" except for boss nodes, 
        // but you might want to store the actual scene name here.
        public string Scene { get; set; } = "transition";

        public string GameObjectName { get; set; }
        public Encounter Encounter { get; set; }
    }

    public class Encounter
    {
        public EncounterType? Type { get; set; }       // Strongly typed encounter types
        public EventType? EventType { get; set; }      // Nullable for non-event encounters
        public int Probability { get; set; }           // Encounter chance
        public string Description { get; set; }        // Additional info for encounters/events
    }
}
