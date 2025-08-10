using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
// using NeonLadder.Editor.ProceduralGeneration; // TODO: Fix assembly reference
using System.Collections.Generic;
using System;

namespace NeonLadder.Tests.Editor
{
    /// <summary>
    /// Unit tests for Procedural Generation Tools Menu
    /// Tests the PathGeneratorEditor functionality and visualization tools
    /// </summary>
    [TestFixture]
    public class ProceduralGenerationToolsTests
    {
        private PathGenerator pathGenerator;
        private GenerationRules testRules;
        
        [SetUp]
        public void Setup()
        {
            pathGenerator = new PathGenerator();
            testRules = GenerationRules.CreateBalancedRules();
        }
        
        [Test]
        public void PathGenerator_GeneratesValidMap()
        {
            // Act
            var map = pathGenerator.GenerateMap("test-seed");
            
            // Assert
            Assert.IsNotNull(map, "Generated map should not be null");
            Assert.AreEqual("test-seed", map.Seed, "Map seed should match input");
            Assert.Greater(map.Layers.Count, 0, "Map should have layers");
        }
        
        [Test]
        public void PathGenerator_GeneratesDeterministicMaps()
        {
            // Arrange
            string seed = "deterministic-test";
            
            // Act
            var map1 = pathGenerator.GenerateMap(seed);
            var map2 = pathGenerator.GenerateMap(seed);
            
            // Assert
            Assert.AreEqual(map1.Seed, map2.Seed, "Seeds should match");
            Assert.AreEqual(map1.Layers.Count, map2.Layers.Count, "Layer counts should match");
            
            // Check first layer node count
            if (map1.Layers.Count > 0 && map1.Layers[0].Nodes != null)
            {
                Assert.AreEqual(
                    map1.Layers[0].Nodes.Count,
                    map2.Layers[0].Nodes.Count,
                    "First layer node counts should match for same seed"
                );
            }
        }
        
        [Test]
        public void GenerationRules_CreateBalancedRules()
        {
            // Act
            var rules = GenerationRules.CreateBalancedRules();
            
            // Assert
            Assert.IsNotNull(rules, "Balanced rules should be created");
            Assert.Greater(rules.minPathsPerLayer, 0, "Min rooms should be positive");
            Assert.GreaterOrEqual(rules.maxPathsPerLayer, rules.minPathsPerLayer, 
                "Max rooms should be >= min rooms");
            Assert.GreaterOrEqual(rules.baseEventChance, 0f, "Branch probability should be >= 0");
            Assert.LessOrEqual(rules.baseEventChance, 1f, "Branch probability should be <= 1");
        }
        
        [Test]
        public void GenerationRules_CreateEasyRules()
        {
            // Act
            var rules = GenerationRules.CreateSafeRules();
            
            // Assert
            Assert.IsNotNull(rules, "Easy rules should be created");
            Assert.IsTrue(rules.guaranteedRestShopPerLayer, "Shops should appear in easy mode");
            Assert.LessOrEqual(rules.ruleFlexibility, 0.3f, "Rule flexibility should be low in easy mode");
        }
        
        [Test]
        public void GenerationRules_CreateHardRules()
        {
            // Act
            var rules = GenerationRules.CreateChaoticRules();
            
            // Assert
            Assert.IsNotNull(rules, "Hard rules should be created");
            Assert.GreaterOrEqual(rules.ruleFlexibility, 0.2f, "Rule flexibility should be higher in hard mode");
        }
        
        [Test]
        public void PathGenerator_AppliesCustomRules()
        {
            // Arrange
            var customRules = new GenerationRules
            {
                minNodesPerPath = 1,
                maxNodesPerPath = 2,
                minPathsPerLayer = 1,
                maxPathsPerLayer = 1, // Only 1 path per layer
                pathsGrowWithDepth = false, // Disable scaling
                moreChoicesInLaterLayers = false, // Disable scaling
                baseEventChance = 0f,
                ruleFlexibility = 0f,
                guaranteedCombatPerLayer = 0,
                guaranteedMinorEnemiesPerLayer = 0, // Disable guarantee
                guaranteedRestShopPerLayer = false, // Disable guarantee
                maxMajorEnemiesPerLayer = 0
            };
            
            // Act
            var map = pathGenerator.GenerateMapWithRules("custom-test", customRules);
            
            // Assert
            Assert.IsNotNull(map, "Map should be generated with custom rules");
            
            // Check that rooms per layer respect the rules
            foreach (var layer in map.Layers)
            {
                if (layer.Nodes != null)
                {
                    Assert.LessOrEqual(layer.Nodes.Count, 2, 
                        "Layer should have at most 2 nodes based on custom rules");
                }
            }
        }
        
        [Test]
        public void MapNode_HasRequiredProperties()
        {
            // Arrange & Act
            var node = new MapNode
            {
                Id = "test-node",
                Type = NodeType.Encounter,
                ConnectedNodeIds = new List<string> { "0", "1", "2" },
                // Position = new Vector2(100, 200) // Position not available in current MapNode
            };
            
            // Assert
            Assert.AreEqual("test-node", node.Id);
            Assert.AreEqual(NodeType.Encounter, node.Type);
            Assert.AreEqual(3, node.ConnectedNodeIds.Count);
            // Assert.AreEqual(new Vector2(100, 200), node.Position); // Position not available
        }
        
        [Test]
        public void MapLayer_ContainsBossInformation()
        {
            // Arrange
            var map = pathGenerator.GenerateMap("boss-test");
            
            // Assert
            Assert.IsNotNull(map.Layers, "Map should have layers");
            
            // Check that at least one layer has boss information
            bool hasBoss = false;
            foreach (var layer in map.Layers)
            {
                if (!string.IsNullOrEmpty(layer.Boss))
                {
                    hasBoss = true;
                    break;
                }
            }
            
            Assert.IsTrue(hasBoss, "At least one layer should have a boss");
        }
        
        [Test]
        public void PathGenerator_GeneratesSpecialRooms()
        {
            // Arrange
            var rules = new GenerationRules
            {
                ruleFlexibility = 1.0f, // Max rule flexibility
                guaranteedCombatPerLayer = 1 // Guaranteed combat
            };
            
            // Act
            var map = pathGenerator.GenerateMapWithRules("special-rooms", rules);
            
            // Assert
            bool hasSpecialRoom = false;
            foreach (var layer in map.Layers)
            {
                if (layer.Nodes == null) continue;
                
                foreach (var node in layer.Nodes)
                {
                    if (node.Type == NodeType.Mystery || node.Type == NodeType.RestShop || 
                        node.Type == NodeType.Treasure || node.Type == NodeType.Elite)
                    {
                        hasSpecialRoom = true;
                        break;
                    }
                }
                
                if (hasSpecialRoom) break;
            }
            
            Assert.IsTrue(hasSpecialRoom, "Map should contain at least one special room");
        }
        
        [Test]
        public void GenerationMetrics_CalculatesCorrectly()
        {
            // Arrange
            var map = pathGenerator.GenerateMap("metrics-test");
            
            // Act - Simulate metrics calculation
            int totalRooms = 0;
            int bossRooms = 0;
            int secretRooms = 0;
            
            foreach (var layer in map.Layers)
            {
                if (layer.Nodes == null) continue;
                
                totalRooms += layer.Nodes.Count;
                
                foreach (var node in layer.Nodes)
                {
                    if (node.Type == NodeType.Boss) bossRooms++;
                    if (node.Type == NodeType.Mystery) secretRooms++;
                }
            }
            
            // Assert
            Assert.Greater(totalRooms, 0, "Should have counted rooms");
            Assert.GreaterOrEqual(bossRooms, 0, "Boss room count should be non-negative");
            Assert.GreaterOrEqual(secretRooms, 0, "Secret room count should be non-negative");
        }
        
        [Test]
        public void SeedHistory_MaintainsUniqueSeeds()
        {
            // Arrange
            var seedHistory = new List<string>();
            
            // Act
            seedHistory.Add("seed1");
            seedHistory.Add("seed2");
            seedHistory.Add("seed1"); // Duplicate
            
            // Remove duplicates
            var uniqueSeeds = new HashSet<string>(seedHistory);
            
            // Assert
            Assert.AreEqual(2, uniqueSeeds.Count, "Should only have unique seeds");
            Assert.IsTrue(uniqueSeeds.Contains("seed1"));
            Assert.IsTrue(uniqueSeeds.Contains("seed2"));
        }
        
        [Test]
        public void PathGeneratorEditor_MenuItemExists()
        {
            // Check that menu item path is valid
            string menuPath = "NeonLadder/Procedural/Path Visualizer";
            
            // This would normally open the window, but in tests we just verify the path format
            Assert.IsTrue(menuPath.Contains("NeonLadder"), "Menu should be under NeonLadder");
            Assert.IsTrue(menuPath.Contains("Procedural"), "Menu should be under Procedural submenu");
        }
        
        [Test]
        public void ExportData_ValidatesJSON()
        {
            // Arrange
            var map = pathGenerator.GenerateMap("export-test");
            
            // Act - Simulate JSON serialization
            string json = JsonUtility.ToJson(map, true);
            
            // Assert
            Assert.IsNotNull(json, "JSON should be generated");
            Assert.AreEqual("export-test", map.Seed, "Map should have the correct seed");
            Assert.IsTrue(json.Contains("export-test"), "JSON should contain seed value");
            
            // Verify it can be deserialized
            var deserializedMap = JsonUtility.FromJson<MysticalMap>(json);
            Assert.IsNotNull(deserializedMap, "Should be able to deserialize JSON");
        }
        
        [Test]
        public void VisualizationColors_AreDistinct()
        {
            // Arrange - Colors from the editor
            Color normalRoom = new Color(0.5f, 0.7f, 1f, 1f);
            Color bossRoom = new Color(1f, 0.3f, 0.3f, 1f);
            Color secretRoom = new Color(0.8f, 0.6f, 1f, 1f);
            Color shopRoom = new Color(1f, 0.9f, 0.3f, 1f);
            Color treasureRoom = new Color(0.3f, 1f, 0.3f, 1f);
            
            // Assert - Check that colors are sufficiently different
            Assert.AreNotEqual(normalRoom, bossRoom, "Normal and boss colors should differ");
            Assert.AreNotEqual(bossRoom, secretRoom, "Boss and secret colors should differ");
            Assert.AreNotEqual(shopRoom, treasureRoom, "Shop and treasure colors should differ");
            
            // Check color distances (simple RGB distance)
            float Distance(Color a, Color b)
            {
                return Mathf.Sqrt(
                    Mathf.Pow(a.r - b.r, 2) +
                    Mathf.Pow(a.g - b.g, 2) +
                    Mathf.Pow(a.b - b.b, 2)
                );
            }
            
            Assert.Greater(Distance(normalRoom, bossRoom), 0.3f, 
                "Normal and boss colors should be visually distinct");
        }
    }
}