using NeonLadder.Common;
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using System.Linq;

// Represents an entire procedural path generation system
namespace NeonLadder.ProceduralGeneration
{
    public class PathGenerator
    {

        private static readonly int[] EncounterProbabilities = { 25, 50, 75 }; // Adjusted to only roundable to 25
        private static readonly string[] Bosses = BossTransformations.bossTransformations.Keys.ToArray();
        public Dictionary<string, string> BossLocations;

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

        public Dictionary<string, Path> GeneratePaths()
        {
            EnsureBossLocationsInitialized();

            var paths = new Dictionary<string, Path>();
            var remainingBosses = Bosses.Except(Constants.DefeatedBosses).ToList();

            int pathCount = remainingBosses.Count > 3 ? 3 : remainingBosses.Count;
            var activeBosses = remainingBosses.Where(rb => rb != Mechanics.Enums.Bosses.Devil.ToString())
                                              .OrderBy(_ => UnityEngine.Random.value)
                                              .Take(pathCount);


            // Add the final path only if all other bosses are defeated
            if (Constants.DefeatedBosses.Count == 7 && !Constants.DefeatedBosses.Contains("Devil"))
            {
                paths["devil"] = GeneratePath("Devil");
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

        private static Dictionary<EncounterType, int> GenerateFixedEncounterProbabilities()
        {
            // Generate probabilities roundable to 25
            var probabilities = new Dictionary<EncounterType, int>();
            var availableProbabilities = EncounterProbabilities.OrderBy(_ => UnityEngine.Random.value).ToArray();

            probabilities[EncounterType.MinorEnemy] = availableProbabilities[0];
            probabilities[EncounterType.MajorEnemy] = 100 - probabilities[EncounterType.MinorEnemy];

            return probabilities;
        }

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

        private static EventType GetRandomEventType()
        {
            var values = System.Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList();
            return values[UnityEngine.Random.Range(0, values.Count)];
        }
    }

    // Enum for encounter types
    public enum EncounterType
    {
        MajorEnemy, // "ME": 1-3 larger, more powerful foes
        MinorEnemy, // "mE": 1-5 smaller, less dangerous foes
        RestShop // Combined Rest and Shop encounter
    }

    // Enum for event types
    public enum EventType
    {
        TreasureChest, // Treasure chest containing loot
        MysteriousAltar, // A mysterious altar offering a choice
        Riddle // Statue challenges you with a riddle
    }

    // Classes for representing the structure
    public class Path
    {
        public string Location { get; set; }
        public List<PathNode> Nodes { get; set; } = new List<PathNode>();
    }

    public class PathNode
    {
        public string Scene { get; set; } = "transition";
        public string GameObjectName { get; set; }
        public Encounter Encounter { get; set; }
    }

    public class Encounter
    {
        public EncounterType? Type { get; set; } // Strongly typed encounter types
        public EventType? EventType { get; set; } // Nullable for non-event encounters
        public int Probability { get; set; } // Encounter chance
        public string Description { get; set; } // Additional info for encounters/events
    }
}
