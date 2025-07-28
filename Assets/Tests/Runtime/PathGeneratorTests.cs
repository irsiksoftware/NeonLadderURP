using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Comprehensive unit tests for the Mystical PathGenerator
    /// By Stephen Strange - "Testing across 14,000,605 possible realities"
    /// </summary>
    [TestFixture]
    public class PathGeneratorTests
    {
        private PathGenerator generator;
        
        [SetUp]
        public void Setup()
        {
            generator = new PathGenerator();
        }

        #region Seed Reproducibility Tests

        [Test]
        public void SameSeed_ProducesIdenticalMaps()
        {
            // Arrange
            const string testSeed = "MYSTICAL_TEST_SEED_42";
            
            // Act
            var map1 = generator.GenerateMap(testSeed);
            var map2 = generator.GenerateMap(testSeed);
            
            // Assert
            Assert.AreEqual(map1.Seed, map2.Seed, "Seeds should be identical");
            Assert.AreEqual(map1.Layers.Count, map2.Layers.Count, "Layer counts should be identical");
            
            for (int i = 0; i < map1.Layers.Count; i++)
            {
                var layer1 = map1.Layers[i];
                var layer2 = map2.Layers[i];
                
                Assert.AreEqual(layer1.LayerIndex, layer2.LayerIndex, $"Layer {i} index should match");
                Assert.AreEqual(layer1.Boss, layer2.Boss, $"Layer {i} boss should match");
                Assert.AreEqual(layer1.Location, layer2.Location, $"Layer {i} location should match");
                Assert.AreEqual(layer1.Nodes.Count, layer2.Nodes.Count, $"Layer {i} node count should match");
                
                for (int j = 0; j < layer1.Nodes.Count; j++)
                {
                    AssertNodesAreIdentical(layer1.Nodes[j], layer2.Nodes[j], $"Layer {i}, Node {j}");
                }
            }
        }

        [Test]
        public void DifferentSeeds_ProduceDifferentMaps()
        {
            // Arrange
            var seeds = new[] { "Seed1", "Seed2", "Seed3", "DifferentSeed", "AnotherOne" };
            var maps = new List<MysticalMap>();
            
            // Act
            foreach (var seed in seeds)
            {
                maps.Add(generator.GenerateMap(seed));
            }
            
            // Assert
            for (int i = 0; i < maps.Count; i++)
            {
                for (int j = i + 1; j < maps.Count; j++)
                {
                    Assert.AreNotEqual(maps[i].Seed, maps[j].Seed, 
                        $"Maps {i} and {j} should have different seeds");
                    
                    // At least one layer should be different
                    bool foundDifference = false;
                    for (int layerIndex = 0; layerIndex < maps[i].Layers.Count && layerIndex < maps[j].Layers.Count; layerIndex++)
                    {
                        if (maps[i].Layers[layerIndex].Nodes.Count != maps[j].Layers[layerIndex].Nodes.Count)
                        {
                            foundDifference = true;
                            break;
                        }
                    }
                    
                    // If node counts are same, check node types or properties
                    if (!foundDifference && maps[i].Layers.Count > 0 && maps[j].Layers.Count > 0)
                    {
                        var layer1 = maps[i].Layers[0];
                        var layer2 = maps[j].Layers[0];
                        
                        if (layer1.Nodes.Count > 0 && layer2.Nodes.Count > 0)
                        {
                            foundDifference = layer1.Nodes[0].Type != layer2.Nodes[0].Type ||
                                            layer1.Nodes[0].Properties.Count != layer2.Nodes[0].Properties.Count;
                        }
                    }
                    
                    Assert.IsTrue(foundDifference, $"Maps from different seeds should produce different results");
                }
            }
        }

        [Test]
        public void EmptySeed_GeneratesValidRandomMap()
        {
            // Act
            var map1 = generator.GenerateMap("");
            var map2 = generator.GenerateMap(null);
            var map3 = generator.GenerateMap("   ");
            
            // Assert
            Assert.IsNotNull(map1, "Empty seed should generate valid map");
            Assert.IsNotNull(map2, "Null seed should generate valid map");
            Assert.IsNotNull(map3, "Whitespace seed should generate valid map");
            
            Assert.IsNotEmpty(map1.Seed, "Generated seed should not be empty");
            Assert.IsNotEmpty(map2.Seed, "Generated seed should not be empty");
            Assert.IsNotEmpty(map3.Seed, "Generated seed should not be empty");
            
            // Random seeds should be different
            Assert.AreNotEqual(map1.Seed, map2.Seed, "Random seeds should be different");
            Assert.AreNotEqual(map2.Seed, map3.Seed, "Random seeds should be different");
        }

        [Test]
        public void ExtremeSeedInputs_HandleGracefully()
        {
            // Arrange
            var extremeSeeds = new[]
            {
                new string('A', 10000), // Very long seed
                "🎮🔮✨🌟💫",           // Unicode/emoji seed
                "!@#$%^&*()_+-=[]{}|;':\",./<>?", // Special characters
                "seed with spaces and CAPS and 123456789", // Mixed case and spaces
                "1",                    // Single character
                "",                     // Empty (handled above but included for completeness)
            };
            
            // Act & Assert
            foreach (var seed in extremeSeeds)
            {
                Assert.DoesNotThrow(() =>
                {
                    var map = generator.GenerateMap(seed);
                    Assert.IsNotNull(map, $"Should handle extreme seed: '{seed}'");
                    Assert.IsTrue(map.Layers.Count > 0, $"Should generate layers for seed: '{seed}'");
                }, $"Should handle extreme seed gracefully: '{seed}'");
            }
        }

        #endregion

        #region Map Structure Tests

        [Test]
        public void GeneratedMap_HasCorrectStructure()
        {
            // Act
            var map = generator.GenerateMap("StructureTest");
            
            // Assert
            Assert.IsNotNull(map, "Map should not be null");
            Assert.IsNotEmpty(map.Seed, "Map should have a seed");
            Assert.IsNotNull(map.Layers, "Map should have layers collection");
            Assert.IsTrue(map.Layers.Count >= 6, "Map should have at least 6 layers (sins)");
            Assert.IsTrue(map.Layers.Count <= 7, "Map should have at most 7 layers (including final boss)");
            
            // Check layer progression
            for (int i = 0; i < map.Layers.Count; i++)
            {
                var layer = map.Layers[i];
                Assert.AreEqual(i, layer.LayerIndex, $"Layer {i} should have correct index");
                Assert.IsNotEmpty(layer.Boss, $"Layer {i} should have a boss");
                Assert.IsNotEmpty(layer.Location, $"Layer {i} should have a location");
                Assert.IsNotNull(layer.Nodes, $"Layer {i} should have nodes collection");
                Assert.IsTrue(layer.Nodes.Count > 0, $"Layer {i} should have at least one node");
            }
        }

        [Test]
        public void GeneratedNodes_HaveValidProperties()
        {
            // Act
            var map = generator.GenerateMap("NodeValidationTest");
            
            // Assert
            foreach (var layer in map.Layers)
            {
                foreach (var node in layer.Nodes)
                {
                    Assert.IsNotEmpty(node.Id, "Node should have a valid ID");
                    Assert.IsTrue(node.LayerIndex >= 0, "Node should have valid layer index");
                    Assert.IsTrue(node.PathIndex >= 0, "Node should have valid path index");
                    Assert.IsTrue(node.NodeIndex >= 0, "Node should have valid node index");
                    Assert.IsNotNull(node.Properties, "Node should have properties collection");
                    
                    // Validate node type specific properties
                    switch (node.Type)
                    {
                        case NodeType.Boss:
                            Assert.IsTrue(node.Properties.ContainsKey("BossName"), "Boss node should have BossName");
                            Assert.IsTrue(node.Properties.ContainsKey("Location"), "Boss node should have Location");
                            Assert.IsTrue(node.Properties.ContainsKey("Difficulty"), "Boss node should have Difficulty");
                            break;
                            
                        case NodeType.Encounter:
                            Assert.IsTrue(node.Properties.ContainsKey("EncounterType"), "Encounter node should have EncounterType");
                            Assert.IsTrue(node.Properties.ContainsKey("EnemyCount"), "Encounter node should have EnemyCount");
                            Assert.IsTrue(node.Properties.ContainsKey("RewardMultiplier"), "Encounter node should have RewardMultiplier");
                            break;
                            
                        case NodeType.RestShop:
                            Assert.IsTrue(node.Properties.ContainsKey("RestEfficiency"), "RestShop node should have RestEfficiency");
                            Assert.IsTrue(node.Properties.ContainsKey("ShopQuality"), "RestShop node should have ShopQuality");
                            break;
                            
                        case NodeType.Event:
                            Assert.IsTrue(node.Properties.ContainsKey("EventType"), "Event node should have EventType");
                            Assert.IsTrue(node.Properties.ContainsKey("RiskLevel"), "Event node should have RiskLevel");
                            break;
                    }
                }
            }
        }

        [Test]
        public void BossNodes_AppearAtEndOfPaths()
        {
            // Act
            var map = generator.GenerateMap("BossPositionTest");
            
            // Assert
            foreach (var layer in map.Layers)
            {
                var bossNodes = layer.Nodes.Where(n => n.Type == NodeType.Boss).ToList();
                Assert.IsTrue(bossNodes.Count > 0, $"Layer {layer.LayerIndex} should have at least one boss node");
                
                // Group nodes by path
                var pathGroups = layer.Nodes.GroupBy(n => n.PathIndex);
                
                foreach (var pathGroup in pathGroups)
                {
                    var pathNodes = pathGroup.OrderBy(n => n.NodeIndex).ToList();
                    var lastNode = pathNodes.Last();
                    
                    Assert.AreEqual(NodeType.Boss, lastNode.Type, 
                        $"Last node in Layer {layer.LayerIndex}, Path {pathGroup.Key} should be a boss");
                }
            }
        }

        #endregion

        #region Scene Mapping Tests

        [Test]
        public void NodeToSceneMapping_ProducesValidScenes()
        {
            // Act
            var map = generator.GenerateMap("SceneMappingTest");
            
            // Assert
            foreach (var layer in map.Layers)
            {
                foreach (var node in layer.Nodes)
                {
                    Assert.DoesNotThrow(() =>
                    {
                        var scene = SceneMapper.MapNodeToScene(node);
                        Assert.IsTrue(System.Enum.IsDefined(typeof(GameScene), scene), 
                            $"Node {node.Id} should map to a valid scene");
                        
                        var config = SceneMapper.GetSceneConfig(scene);
                        Assert.IsNotEmpty(config.SceneName, $"Scene {scene} should have a valid scene name");
                        
                    }, $"Node {node.Id} should map to a valid scene");
                }
            }
        }

        [Test]
        public void BossNodes_MapToCorrectBossScenes()
        {
            // Arrange
            var expectedBossMappings = new Dictionary<string, GameScene>
            {
                { "Pride", GameScene.PrideBossArena },
                { "Wrath", GameScene.WrathBossArena },
                { "Greed", GameScene.GreedBossArena },
                { "Envy", GameScene.EnvyBossArena },
                { "Lust", GameScene.LustBossArena },
                { "Gluttony", GameScene.GluttonyBossArena },
                { "Sloth", GameScene.SlothBossArena }
            };
            
            // Act & Assert
            foreach (var mapping in expectedBossMappings)
            {
                var testNode = new MapNode
                {
                    Type = NodeType.Boss,
                    Properties = new Dictionary<string, object>
                    {
                        ["BossName"] = mapping.Key
                    }
                };
                
                var mappedScene = SceneMapper.MapNodeToScene(testNode);
                Assert.AreEqual(mapping.Value, mappedScene, 
                    $"Boss {mapping.Key} should map to scene {mapping.Value}");
            }
        }

        #endregion

        #region Serialization Tests

        [Test]
        public void MapSerialization_PreservesAllData()
        {
            // Arrange
            var originalMap = generator.GenerateMap("SerializationTest");
            
            // Act
            var serialized = generator.SerializeMap(originalMap);
            var deserializedMap = generator.DeserializeMap(serialized);
            
            // Assert
            Assert.IsNotNull(deserializedMap, "Deserialized map should not be null");
            Assert.AreEqual(originalMap.Seed, deserializedMap.Seed, "Seed should be preserved");
            Assert.AreEqual(originalMap.Layers.Count, deserializedMap.Layers.Count, "Layer count should be preserved");
            
            for (int i = 0; i < originalMap.Layers.Count; i++)
            {
                var originalLayer = originalMap.Layers[i];
                var deserializedLayer = deserializedMap.Layers[i];
                
                Assert.AreEqual(originalLayer.LayerIndex, deserializedLayer.LayerIndex, $"Layer {i} index should be preserved");
                Assert.AreEqual(originalLayer.Boss, deserializedLayer.Boss, $"Layer {i} boss should be preserved");
                Assert.AreEqual(originalLayer.Location, deserializedLayer.Location, $"Layer {i} location should be preserved");
                Assert.AreEqual(originalLayer.Nodes.Count, deserializedLayer.Nodes.Count, $"Layer {i} node count should be preserved");
                
                for (int j = 0; j < originalLayer.Nodes.Count; j++)
                {
                    AssertNodesAreIdentical(originalLayer.Nodes[j], deserializedLayer.Nodes[j], 
                        $"Serialization Layer {i}, Node {j}");
                }
            }
        }

        #endregion

        #region Performance Tests

        [Test]
        public void MapGeneration_CompletesQuickly()
        {
            // Arrange
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            // Act
            stopwatch.Start();
            var map = generator.GenerateMap("PerformanceTest");
            stopwatch.Stop();
            
            // Assert
            Assert.IsNotNull(map, "Map should generate successfully");
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100, 
                $"Map generation should complete in under 100ms, took {stopwatch.ElapsedMilliseconds}ms");
        }

        [Test]
        public void MultipleMapGeneration_MaintainsPerformance()
        {
            // Arrange
            const int mapCount = 50;
            var stopwatch = new System.Diagnostics.Stopwatch();
            
            // Act
            stopwatch.Start();
            for (int i = 0; i < mapCount; i++)
            {
                var map = generator.GenerateMap($"PerformanceTest_{i}");
                Assert.IsNotNull(map, $"Map {i} should generate successfully");
            }
            stopwatch.Stop();
            
            // Assert
            var averageTime = stopwatch.ElapsedMilliseconds / (double)mapCount;
            Assert.IsTrue(averageTime < 10, 
                $"Average map generation should be under 10ms, was {averageTime:F2}ms");
        }

        #endregion

        #region Helper Methods

        private void AssertNodesAreIdentical(MapNode node1, MapNode node2, string context)
        {
            Assert.AreEqual(node1.Id, node2.Id, $"{context}: IDs should match");
            Assert.AreEqual(node1.Type, node2.Type, $"{context}: Types should match");
            Assert.AreEqual(node1.LayerIndex, node2.LayerIndex, $"{context}: Layer indices should match");
            Assert.AreEqual(node1.PathIndex, node2.PathIndex, $"{context}: Path indices should match");
            Assert.AreEqual(node1.NodeIndex, node2.NodeIndex, $"{context}: Node indices should match");
            Assert.AreEqual(node1.Properties.Count, node2.Properties.Count, $"{context}: Property counts should match");
            
            foreach (var kvp in node1.Properties)
            {
                Assert.IsTrue(node2.Properties.ContainsKey(kvp.Key), 
                    $"{context}: Property '{kvp.Key}' should exist in both nodes");
                Assert.AreEqual(kvp.Value, node2.Properties[kvp.Key], 
                    $"{context}: Property '{kvp.Key}' values should match");
            }
        }

        #endregion
    }
}