using NeonLadder.Mechanics.Controllers;
using Newtonsoft.Json;
using NeonLadder.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Represents an entire procedural path generation system
namespace NeonLadder.ProceduralGeneration
{
    public class PathGenerator
    {
        private static readonly int[] EncounterProbabilities = { 25, 50, 75 }; // Adjusted to only roundable to 25
        private static readonly string[] Bosses = BossTransformations.bossTransformations.Keys.ToArray();
        private static Dictionary<string, string> BossLocations;

        private static void EnsureBossLocationsInitialized()
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

        public static Dictionary<string, Path> GeneratePaths()
        {
            EnsureBossLocationsInitialized();

            var paths = new Dictionary<string, Path>();
            var remainingBosses = Bosses.Except(Constants.DefeatedBosses).ToList();

            int pathCount = remainingBosses.Count > 3 ? 3 : remainingBosses.Count;
            var activeBosses = remainingBosses.Where(rb => rb != Mechanics.Enums.Bosses.Devil.ToString())
                                              .OrderBy(_ => Random.value)
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

        private static Path GeneratePath(string boss)
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

        public static string GenerateJSON(Dictionary<string, Path> paths)
        {
            return JsonConvert.SerializeObject(new
            {
                legend = new
                {
                    arrows = "-> / <- (Optional rooms you can enter within the scene)",
                    rooms = "() = (room, e.g., Shop or Rest)",
                    encounters = new
                    {
                        ME = "Major Enemy (1-3 larger, more powerful foes)",
                        mE = "Minor Enemy (1-5 smaller, less dangerous foes)",
                        Event = new
                        {
                            TreasureChest = "Treasure chest containing loot",
                            MysteriousAltar = "A mysterious altar offering a choice",
                            Riddle = "Statue challenges you with a riddle"
                        }
                    }
                },
                map = new
                {
                    paths = paths,
                    start = new
                    {
                        location = "City Entrance",
                        staging = true,
                        scene = "transition",
                        gameObject = "city-transition-1"
                    },
                    backtracking = true
                }
            }, Formatting.Indented);
        }

        public static string GenerateIndentedText(Dictionary<string, Path> paths)
        {
            var text = new System.Text.StringBuilder();

            text.AppendLine("City Entrance");
            foreach (var path in paths)
            {
                text.AppendLine("====================");
                //text.AppendLine($"- {path.Key.ToUpper()}");
                foreach (var node in path.Value.Nodes)
                {
                    text.AppendLine($"  - {node.GameObjectName}");
                    var description = $"    - {node.Encounter.Description}";
                    if (node.Encounter.Description.Contains("final boss"))
                    {
                        description += $" - Location: {BossLocations[path.Key]}";
                    }
                    text.AppendLine(description);
                }
                text.AppendLine(); // Add empty line after each path

            }

            return text.ToString();
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
