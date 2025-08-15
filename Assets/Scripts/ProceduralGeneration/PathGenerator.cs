using NeonLadder.Common;
using NeonLadder.Mechanics.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// The Mystical PathGenerator - A Slay the Spire inspired procedural generation system
    /// By Stephen Strange, Master of the Mystic Arts
    /// 
    /// "I've seen 14,000,605 possible game designs. This is the one that works."
    /// </summary>
    public class PathGenerator
    {
        // The Seven Deadly Sins Progression (6 layers + final boss)
        private static readonly string[] LayerBosses = new[]
        {
            "Pride",     // Layer 1
            "Wrath",     // Layer 2  
            "Greed",     // Layer 3
            "Envy",      // Layer 4
            "Lust",      // Layer 5
            "Gluttony",  // Layer 6
            "Sloth",     // Layer 7
            "Devil"      // Final Layer (if all sins defeated)
        };

        // Boss locations are now managed through BossLocationData class
        // This preserves both display names for localization/title cards 
        // and simplified identifiers for procedural generation

        // Random instance seeded from the mystical string
        private System.Random _seededRandom;
        private string _currentSeed;

        /// <summary>
        /// Generates a complete mystical map from any seed string
        /// </summary>
        public MysticalMap GenerateMap(string seedString = null)
        {
            return GenerateMapWithRules(seedString, GenerationRules.CreateBalancedRules());
        }

        /// <summary>
        /// Generates a complete mystical map from any seed string with custom rules
        /// </summary>
        public MysticalMap GenerateMapWithRules(string seedString = null, GenerationRules rules = null)
        {
            // If no seed provided, generate a mystical one
            if (string.IsNullOrWhiteSpace(seedString))
            {
                seedString = GenerateMysticalSeed();
            }

            // Use default rules if none provided
            if (rules == null)
            {
                rules = GenerationRules.CreateBalancedRules();
            }

            _currentSeed = seedString;
            
            // Convert the mystical string to deterministic randomness
            _seededRandom = new System.Random(ConvertSeedToInt(seedString));

            var map = new MysticalMap
            {
                Seed = seedString,
                Layers = new List<MapLayer>()
            };

            // Generate the six layers of the seven deadly sins
            for (int layerIndex = 0; layerIndex < 6; layerIndex++)
            {
                var adjustedRules = rules.GetAdjustedRulesForLayer(layerIndex, _seededRandom);
                var layer = GenerateLayerWithRules(layerIndex, LayerBosses[layerIndex], adjustedRules);
                map.Layers.Add(layer);
            }

            // Add the final boss layer if all sins are vanquished
            if (ShouldIncludeFinalBoss())
            {
                var finalLayer = GenerateFinalBossLayer();
                map.Layers.Add(finalLayer);
            }

            return map;
        }

        /// <summary>
        /// Mystical seed generation using the ancient arts of cryptography
        /// </summary>
        private string GenerateMysticalSeed(int length = 16)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new System.Random();
            var result = new StringBuilder(length);
            
            for (int i = 0; i < length; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Converts any mystical string into deterministic chaos using SHA256
        /// </summary>
        private int ConvertSeedToInt(string seedString)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(seedString));
                // Take first 4 bytes and convert to int for deterministic seeding
                return BitConverter.ToInt32(hashBytes, 0);
            }
        }

        /// <summary>
        /// Generates a single layer of the mystical map
        /// </summary>
        private MapLayer GenerateLayer(int layerIndex, string boss)
        {
            return GenerateLayerWithRules(layerIndex, boss, GenerationRules.CreateBalancedRules());
        }

        /// <summary>
        /// Generates a single layer of the mystical map with custom rules
        /// </summary>
        private MapLayer GenerateLayerWithRules(int layerIndex, string boss, GenerationRules rules)
        {
            var layer = new MapLayer
            {
                LayerIndex = layerIndex,
                Boss = boss,
                Location = BossLocationData.GetByLayer(layerIndex)?.DisplayName ?? "Unknown Location",
                Nodes = new List<MapNode>()
            };

            // Generate paths according to rules
            int pathCount = _seededRandom.Next(rules.minPathsPerLayer, rules.maxPathsPerLayer + 1);
            
            // Track required encounters to ensure rules are met
            var requiredNodeTypes = new List<NodeType>();
            
            // Add guaranteed encounters
            for (int i = 0; i < rules.guaranteedCombatPerLayer; i++)
            {
                requiredNodeTypes.Add(NodeType.Encounter);
            }
            
            if (rules.guaranteedRestShopPerLayer)
            {
                requiredNodeTypes.Add(NodeType.RestShop);
            }
            
            // Generate paths with rule enforcement
            for (int pathIndex = 0; pathIndex < pathCount; pathIndex++)
            {
                int nodesPerPath = _seededRandom.Next(rules.minNodesPerPath, rules.maxNodesPerPath + 1);
                var pathNodes = GeneratePathNodesWithRules(layerIndex, pathIndex, nodesPerPath, boss, rules, requiredNodeTypes);
                layer.Nodes.AddRange(pathNodes);
            }
            
            // Ensure we met minimum requirements by adding nodes if necessary
            EnsureRuleCompliance(layer, rules);

            return layer;
        }

        /// <summary>
        /// Calculates nodes per path based on layer depth (gets longer as you progress)
        /// </summary>
        private int CalculateNodesPerPath(int layerIndex)
        {
            // Early layers: 3-4 nodes, Later layers: 4-6 nodes
            int baseNodes = 3 + (layerIndex / 2);
            return baseNodes + _seededRandom.Next(0, 2);
        }

        /// <summary>
        /// Generates the mystical nodes along a single path
        /// </summary>
        private List<MapNode> GeneratePathNodes(int layerIndex, int pathIndex, int nodeCount, string boss)
        {
            var nodes = new List<MapNode>();

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                MapNode node;

                // Last node is always the boss
                if (nodeIndex == nodeCount - 1)
                {
                    node = CreateBossNode(layerIndex, pathIndex, nodeIndex, boss);
                }
                // Second to last is usually rest/shop
                else if (nodeIndex == nodeCount - 2)
                {
                    node = CreateRestShopNode(layerIndex, pathIndex, nodeIndex);
                }
                // Other nodes are encounters or events
                else
                {
                    node = CreateRandomNode(layerIndex, pathIndex, nodeIndex);
                }

                nodes.Add(node);
            }

            return nodes;
        }

        /// <summary>
        /// Generates path nodes with rule enforcement
        /// </summary>
        private List<MapNode> GeneratePathNodesWithRules(int layerIndex, int pathIndex, int nodeCount, string boss, GenerationRules rules, List<NodeType> requiredNodeTypes)
        {
            var nodes = new List<MapNode>();

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                MapNode node;

                // Last node is always the boss
                if (nodeIndex == nodeCount - 1)
                {
                    node = CreateBossNode(layerIndex, pathIndex, nodeIndex, boss);
                }
                // Second to last follows rest/shop rule
                else if (nodeIndex == nodeCount - 2 && rules.restShopBeforeBoss)
                {
                    node = CreateRestShopNode(layerIndex, pathIndex, nodeIndex);
                    requiredNodeTypes.Remove(NodeType.RestShop); // Mark as satisfied
                }
                // Other nodes follow rule requirements
                else
                {
                    // Try to satisfy required node types first
                    if (requiredNodeTypes.Count > 0 && nodeIndex < nodeCount - 2)
                    {
                        var requiredType = requiredNodeTypes[0];
                        requiredNodeTypes.RemoveAt(0);
                        
                        node = requiredType switch
                        {
                            NodeType.Encounter => CreateEncounterNodeWithRules(layerIndex, pathIndex, nodeIndex, rules),
                            NodeType.RestShop => CreateRestShopNode(layerIndex, pathIndex, nodeIndex),
                            NodeType.Event => CreateEventNode(layerIndex, pathIndex, nodeIndex),
                            _ => CreateRandomNodeWithRules(layerIndex, pathIndex, nodeIndex, rules)
                        };
                    }
                    else
                    {
                        node = CreateRandomNodeWithRules(layerIndex, pathIndex, nodeIndex, rules);
                    }
                }

                // Prevent adjacent same types if rule enabled
                if (rules.preventAdjacentSameType && nodes.Count > 0)
                {
                    var lastNode = nodes[nodes.Count - 1];
                    if (lastNode.Type == node.Type && node.Type != NodeType.Boss)
                    {
                        // Generate a different type
                        node = CreateAlternativeNode(layerIndex, pathIndex, nodeIndex, lastNode.Type, rules);
                    }
                }

                nodes.Add(node);
            }

            return nodes;
        }

        /// <summary>
        /// Ensures layer meets all rule requirements
        /// </summary>
        private void EnsureRuleCompliance(MapLayer layer, GenerationRules rules)
        {
            // Count current node types
            var nodeTypeCounts = layer.Nodes.GroupBy(n => n.Type).ToDictionary(g => g.Key, g => g.Count());
            
            var combatCount = nodeTypeCounts.GetValueOrDefault(NodeType.Encounter, 0);
            var restShopCount = nodeTypeCounts.GetValueOrDefault(NodeType.RestShop, 0);
            
            // Check combat encounters
            var combatNodes = layer.Nodes.Where(n => n.Type == NodeType.Encounter).ToList();
            var minorEnemyCount = combatNodes.Count(n => 
                n.Properties.ContainsKey("EncounterType") && 
                (EncounterType)n.Properties["EncounterType"] == EncounterType.MinorEnemy);
            
            // Add missing minor enemies
            if (minorEnemyCount < rules.guaranteedMinorEnemiesPerLayer)
            {
                var deficit = rules.guaranteedMinorEnemiesPerLayer - minorEnemyCount;
                for (int i = 0; i < deficit; i++)
                {
                    var nodeIndex = layer.Nodes.Count;
                    var minorEnemyNode = new MapNode
                    {
                        Id = $"L{layer.LayerIndex}_COMPLIANCE_{nodeIndex}",
                        Type = NodeType.Encounter,
                        LayerIndex = layer.LayerIndex,
                        PathIndex = 0, // Add to first path
                        NodeIndex = nodeIndex,
                        Properties = new Dictionary<string, object>
                        {
                            ["EncounterType"] = EncounterType.MinorEnemy,
                            ["EnemyCount"] = _seededRandom.Next(2, 5),
                            ["Difficulty"] = layer.LayerIndex + 1,
                            ["RewardMultiplier"] = _seededRandom.NextDouble() * 0.5 + 0.75
                        }
                    };
                    layer.Nodes.Add(minorEnemyNode);
                }
            }
            
            // Add missing rest/shop if required
            if (rules.guaranteedRestShopPerLayer && restShopCount == 0)
            {
                var nodeIndex = layer.Nodes.Count;
                var restShopNode = CreateRestShopNode(layer.LayerIndex, 0, nodeIndex);
                restShopNode.Id = $"L{layer.LayerIndex}_COMPLIANCE_REST_{nodeIndex}";
                layer.Nodes.Add(restShopNode);
            }
        }

        /// <summary>
        /// Creates an encounter node with rule considerations
        /// </summary>
        private MapNode CreateEncounterNodeWithRules(int layerIndex, int pathIndex, int nodeIndex, GenerationRules rules)
        {
            // Determine encounter type based on rules
            var encounterType = EncounterType.MinorEnemy; // Default to minor
            
            // Check if we can add major enemies
            var existingMajorCount = 0; // Would need to count existing in layer, simplified for now
            if (existingMajorCount < rules.maxMajorEnemiesPerLayer && _seededRandom.NextDouble() < 0.3)
            {
                encounterType = EncounterType.MajorEnemy;
            }
            
            return new MapNode
            {
                Id = $"L{layerIndex}_P{pathIndex}_N{nodeIndex}",
                Type = NodeType.Encounter,
                LayerIndex = layerIndex,
                PathIndex = pathIndex,
                NodeIndex = nodeIndex,
                Properties = new Dictionary<string, object>
                {
                    ["EncounterType"] = encounterType,
                    ["EnemyCount"] = encounterType == EncounterType.MinorEnemy ? _seededRandom.Next(2, 6) : _seededRandom.Next(1, 3),
                    ["Difficulty"] = layerIndex + 1,
                    ["RewardMultiplier"] = _seededRandom.NextDouble() * 0.5 + 0.75
                }
            };
        }

        /// <summary>
        /// Creates a random node with rule considerations
        /// </summary>
        private MapNode CreateRandomNodeWithRules(int layerIndex, int pathIndex, int nodeIndex, GenerationRules rules)
        {
            // Calculate event chance based on rules
            double eventChance = rules.baseEventChance;
            if (rules.eventChanceScalesWithDepth)
            {
                eventChance += layerIndex * 0.1f;
            }
            
            bool isEvent = _seededRandom.NextDouble() < eventChance;
            
            if (isEvent)
            {
                return CreateEventNode(layerIndex, pathIndex, nodeIndex);
            }
            else
            {
                return CreateEncounterNodeWithRules(layerIndex, pathIndex, nodeIndex, rules);
            }
        }

        /// <summary>
        /// Creates an alternative node type to prevent adjacency
        /// </summary>
        private MapNode CreateAlternativeNode(int layerIndex, int pathIndex, int nodeIndex, NodeType avoidType, GenerationRules rules)
        {
            var alternatives = new[] { NodeType.Encounter, NodeType.Event, NodeType.RestShop }
                .Where(t => t != avoidType).ToList();
            
            var chosenType = alternatives[_seededRandom.Next(alternatives.Count)];
            
            return chosenType switch
            {
                NodeType.Encounter => CreateEncounterNodeWithRules(layerIndex, pathIndex, nodeIndex, rules),
                NodeType.Event => CreateEventNode(layerIndex, pathIndex, nodeIndex),
                NodeType.RestShop => CreateRestShopNode(layerIndex, pathIndex, nodeIndex),
                _ => CreateEncounterNodeWithRules(layerIndex, pathIndex, nodeIndex, rules)
            };
        }

        /// <summary>
        /// Creates a boss encounter node
        /// </summary>
        private MapNode CreateBossNode(int layerIndex, int pathIndex, int nodeIndex, string boss)
        {
            return new MapNode
            {
                Id = $"L{layerIndex}_P{pathIndex}_N{nodeIndex}",
                Type = NodeType.Boss,
                LayerIndex = layerIndex,
                PathIndex = pathIndex,
                NodeIndex = nodeIndex,
                Properties = new Dictionary<string, object>
                {
                    ["BossName"] = boss,
                    ["Location"] = BossLocationData.GetByLayer(layerIndex)?.DisplayName ?? "Unknown Location",
                    ["Difficulty"] = layerIndex + 1
                }
            };
        }

        /// <summary>
        /// Creates a rest and shop node
        /// </summary>
        private MapNode CreateRestShopNode(int layerIndex, int pathIndex, int nodeIndex)
        {
            return new MapNode
            {
                Id = $"L{layerIndex}_P{pathIndex}_N{nodeIndex}",
                Type = NodeType.RestShop,
                LayerIndex = layerIndex,
                PathIndex = pathIndex,
                NodeIndex = nodeIndex,
                Properties = new Dictionary<string, object>
                {
                    ["RestEfficiency"] = _seededRandom.NextDouble() * 0.5 + 0.5, // 50-100% efficiency
                    ["ShopQuality"] = _seededRandom.Next(1, 4) // Shop tier 1-3
                }
            };
        }

        /// <summary>
        /// Creates a random encounter or event node
        /// </summary>
        private MapNode CreateRandomNode(int layerIndex, int pathIndex, int nodeIndex)
        {
            // Higher chance of encounters early, more events later
            double encounterChance = 0.8 - (layerIndex * 0.1);
            bool isEncounter = _seededRandom.NextDouble() < encounterChance;

            if (isEncounter)
            {
                return CreateEncounterNode(layerIndex, pathIndex, nodeIndex);
            }
            else
            {
                return CreateEventNode(layerIndex, pathIndex, nodeIndex);
            }
        }

        /// <summary>
        /// Creates an enemy encounter node
        /// </summary>
        private MapNode CreateEncounterNode(int layerIndex, int pathIndex, int nodeIndex)
        {
            // Determine encounter difficulty
            var encounterTypes = new[] { EncounterType.MinorEnemy, EncounterType.MajorEnemy };
            var encounterType = encounterTypes[_seededRandom.Next(encounterTypes.Length)];

            return new MapNode
            {
                Id = $"L{layerIndex}_P{pathIndex}_N{nodeIndex}",
                Type = NodeType.Encounter,
                LayerIndex = layerIndex,
                PathIndex = pathIndex,
                NodeIndex = nodeIndex,
                Properties = new Dictionary<string, object>
                {
                    ["EncounterType"] = encounterType,
                    ["EnemyCount"] = encounterType == EncounterType.MinorEnemy ? _seededRandom.Next(2, 6) : _seededRandom.Next(1, 3),
                    ["Difficulty"] = layerIndex + 1,
                    ["RewardMultiplier"] = _seededRandom.NextDouble() * 0.5 + 0.75 // 75-125% rewards
                }
            };
        }

        /// <summary>
        /// Creates a mystical event node
        /// </summary>
        private MapNode CreateEventNode(int layerIndex, int pathIndex, int nodeIndex)
        {
            var eventTypes = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToArray();
            var eventType = eventTypes[_seededRandom.Next(eventTypes.Length)];

            return new MapNode
            {
                Id = $"L{layerIndex}_P{pathIndex}_N{nodeIndex}",
                Type = NodeType.Event,
                LayerIndex = layerIndex,
                PathIndex = pathIndex,
                NodeIndex = nodeIndex,
                Properties = new Dictionary<string, object>
                {
                    ["EventType"] = eventType,
                    ["EventPower"] = _seededRandom.Next(1, layerIndex + 2), // Scales with layer
                    ["RiskLevel"] = _seededRandom.NextDouble() // 0-100% risk
                }
            };
        }

        /// <summary>
        /// Generates the final boss layer for ultimate challenge
        /// </summary>
        private MapLayer GenerateFinalBossLayer()
        {
            return new MapLayer
            {
                LayerIndex = 6,
                Boss = "Sloth", // The final sin
                Location = "The Lethargy Lounge",
                Nodes = new List<MapNode>
                {
                    new MapNode
                    {
                        Id = "FINAL_BOSS",
                        Type = NodeType.Boss,
                        LayerIndex = 6,
                        PathIndex = 0,
                        NodeIndex = 0,
                        Properties = new Dictionary<string, object>
                        {
                            ["BossName"] = "Sloth",
                            ["Location"] = "The Lethargy Lounge",
                            ["Difficulty"] = 10,
                            ["IsFinalBoss"] = true
                        }
                    }
                }
            };
        }

        /// <summary>
        /// Determines if the final boss should be included
        /// </summary>
        private bool ShouldIncludeFinalBoss()
        {
            // Include final boss if all other bosses are defeated
            return Constants.DefeatedBosses?.Count >= 6;
        }

        /// <summary>
        /// Serializes the mystical map for persistence
        /// </summary>
        public string SerializeMap(MysticalMap map)
        {
            return JsonConvert.SerializeObject(map, Formatting.Indented);
        }

        /// <summary>
        /// Deserializes a mystical map from the astral plane
        /// </summary>
        public MysticalMap DeserializeMap(string json)
        {
            return JsonConvert.DeserializeObject<MysticalMap>(json);
        }
    }

    /// <summary>
    /// The complete mystical map structure
    /// </summary>
    [Serializable]
    public class MysticalMap
    {
        public string Seed;
        public List<MapLayer> Layers = new List<MapLayer>();
    }

    /// <summary>
    /// A single layer in the mystical progression
    /// </summary>
    [Serializable]
    public class MapLayer
    {
        public int LayerIndex { get; set; }
        public string Boss { get; set; }
        public string Location { get; set; }
        public List<MapNode> Nodes { get; set; } = new List<MapNode>();
    }

    /// <summary>
    /// A single node in the mystical map - fully extensible for future magic
    /// </summary>
    [Serializable]
    public class MapNode
    {
        public string Id { get; set; }
        public NodeType Type { get; set; }
        public int LayerIndex { get; set; }
        public int PathIndex { get; set; }
        public int NodeIndex { get; set; }
        
        // Extensible properties for future mystical enhancements
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        // Navigation connections (for future branching complexity)
        public List<string> ConnectedNodeIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// The mystical node types that define reality
    /// </summary>
    public enum NodeType
    {
        Encounter,   // Combat encounters
        Event,       // Mystical events
        RestShop,    // Rest and shopping
        Boss,        // Boss encounters
        Treasure,    // Future: Treasure rooms
        Elite,       // Future: Elite encounters
        Mystery,     // Future: Mystery nodes
        Start,       // Starting node (MainCityHub)
        Connection   // Connection scenes between areas
    }

    /// <summary>
    /// Combat encounter classifications
    /// </summary>
    public enum EncounterType
    {
        MinorEnemy,  // 2-5 smaller foes
        MajorEnemy,  // 1-2 powerful foes
        Elite        // Single elite enemy
    }

    /// <summary>
    /// Mystical events that bend reality
    /// </summary>
    public enum EventType
    {
        TreasureChest,    // Loot chest
        MysteriousAltar,  // Choice-based altar
        Riddle,           // Puzzle challenge
        Merchant,         // Traveling merchant
        Shrine,           // Blessing shrine
        Curse,            // Cursed encounter
        Portal            // Dimensional portal
    }

    // Legacy classes for backward compatibility
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
        public EncounterType? Type { get; set; }
        public EventType? EventType { get; set; }
        public int Probability { get; set; }
        public string Description { get; set; }
    }
}