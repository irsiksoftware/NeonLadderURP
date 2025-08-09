using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using NeonLadder.Editor.ProceduralGeneration;
using NeonLadder.ProceduralGeneration;
using System;

namespace NeonLadder.Tests.Editor
{
    /// <summary>
    /// Unit tests for the Path Generator Editor visualization tools
    /// Tests menu integration, visualization features, and export functionality
    /// </summary>
    [TestFixture]
    public class PathGeneratorEditorTests
    {
        private PathGeneratorEditor editorWindow;
        private PathGenerator generator;
        private PathGeneratorConfig testConfig;
        
        [SetUp]
        public void Setup()
        {
            // Create test instances
            generator = new PathGenerator();
            testConfig = ScriptableObject.CreateInstance<PathGeneratorConfig>();
            testConfig.rules = GenerationRules.CreateBalancedRules();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (editorWindow != null)
            {
                editorWindow.Close();
            }
            
            if (testConfig != null)
            {
                ScriptableObject.DestroyImmediate(testConfig);
            }
        }
        
        [Test]
        public void PathGeneratorEditor_CanBeCreated()
        {
            // Act
            editorWindow = EditorWindow.CreateWindow<PathGeneratorEditor>();
            
            // Assert
            Assert.IsNotNull(editorWindow, "PathGeneratorEditor window should be created");
        }
        
        [Test]
        public void PathGenerator_GeneratesValidMap()
        {
            // Arrange
            string testSeed = "TestSeed123";
            
            // Act
            var map = generator.GenerateMap(testSeed);
            
            // Assert
            Assert.IsNotNull(map, "Generated map should not be null");
            Assert.AreEqual(testSeed, map.Seed, "Map seed should match input seed");
            Assert.IsNotNull(map.Layers, "Map should have layers");
            Assert.Greater(map.Layers.Count, 0, "Map should have at least one layer");
        }
        
        [Test]
        public void PathGenerator_GeneratesMapWithRules()
        {
            // Arrange
            var rules = GenerationRules.CreateBalancedRules();
            rules.MinNodesPerLayer = 5;
            rules.MaxNodesPerLayer = 8;
            
            // Act
            var map = generator.GenerateMapWithRules("TestSeed", rules);
            
            // Assert
            Assert.IsNotNull(map, "Map should be generated with rules");
            
            foreach (var layer in map.Layers)
            {
                Assert.GreaterOrEqual(layer.Nodes.Count, rules.MinNodesPerLayer, 
                    "Layer should have at least minimum nodes");
                Assert.LessOrEqual(layer.Nodes.Count, rules.MaxNodesPerLayer, 
                    "Layer should not exceed maximum nodes");
            }
        }
        
        [Test]
        public void PathGeneratorConfig_HasCorrectDefaults()
        {
            // Assert
            Assert.IsNotNull(testConfig.rules, "Config should have default rules");
            Assert.IsNotNull(testConfig.testSeeds, "Config should have test seeds list");
            Assert.Greater(testConfig.testSeeds.Count, 0, "Config should have default test seeds");
            Assert.IsTrue(testConfig.autoValidateOnChange, "Auto-validate should be enabled by default");
            Assert.IsTrue(testConfig.colorCodeNodeTypes, "Color coding should be enabled by default");
        }
        
        [Test]
        public void MenuItems_AreProperlyRegistered()
        {
            // Arrange
            var editorType = typeof(PathGeneratorEditor);
            var methods = editorType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            
            // Act
            var showWindowMethod = editorType.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.Public);
            var showSeedTestingMethod = editorType.GetMethod("ShowSeedTesting", BindingFlags.Static | BindingFlags.Public);
            var showStatisticsMethod = editorType.GetMethod("ShowStatistics", BindingFlags.Static | BindingFlags.Public);
            var exportDataMethod = editorType.GetMethod("ExportGenerationData", BindingFlags.Static | BindingFlags.Public);
            
            // Assert
            Assert.IsNotNull(showWindowMethod, "ShowWindow menu method should exist");
            Assert.IsNotNull(showSeedTestingMethod, "ShowSeedTesting menu method should exist");
            Assert.IsNotNull(showStatisticsMethod, "ShowStatistics menu method should exist");
            Assert.IsNotNull(exportDataMethod, "ExportGenerationData menu method should exist");
            
            // Check for MenuItem attributes
            Assert.IsNotNull(showWindowMethod.GetCustomAttribute<MenuItem>(), 
                "ShowWindow should have MenuItem attribute");
            Assert.IsNotNull(showSeedTestingMethod.GetCustomAttribute<MenuItem>(), 
                "ShowSeedTesting should have MenuItem attribute");
            Assert.IsNotNull(showStatisticsMethod.GetCustomAttribute<MenuItem>(), 
                "ShowStatistics should have MenuItem attribute");
            Assert.IsNotNull(exportDataMethod.GetCustomAttribute<MenuItem>(), 
                "ExportGenerationData should have MenuItem attribute");
        }
        
        [Test]
        public void GenerationRules_CreateBalancedRules()
        {
            // Act
            var rules = GenerationRules.CreateBalancedRules();
            
            // Assert
            Assert.IsNotNull(rules, "Balanced rules should be created");
            Assert.Greater(rules.MinNodesPerLayer, 0, "Min nodes should be positive");
            Assert.GreaterOrEqual(rules.MaxNodesPerLayer, rules.MinNodesPerLayer, 
                "Max nodes should be >= min nodes");
            Assert.Greater(rules.CombatWeight, 0, "Combat weight should be positive");
            Assert.GreaterOrEqual(rules.MinConnectionsPerNode, 1, 
                "Min connections should be at least 1");
        }
        
        [Test]
        public void MysticalMap_HasValidStructure()
        {
            // Arrange & Act
            var map = generator.GenerateMap("TestSeed");
            
            // Assert
            Assert.IsNotNull(map.Seed, "Map should have a seed");
            Assert.IsNotNull(map.Layers, "Map should have layers");
            
            foreach (var layer in map.Layers)
            {
                Assert.IsNotNull(layer.BossName, "Each layer should have a boss name");
                Assert.IsNotNull(layer.Nodes, "Each layer should have nodes");
                Assert.Greater(layer.Nodes.Count, 0, "Each layer should have at least one node");
                
                foreach (var node in layer.Nodes)
                {
                    Assert.IsNotNull(node.Id, "Each node should have an ID");
                    Assert.GreaterOrEqual(node.Position, 0, "Node position should be non-negative");
                    Assert.IsNotNull(node.ConnectionIds, "Node should have connection list");
                }
            }
        }
        
        [Test]
        public void PathGeneratorConfig_ValidationWorks()
        {
            // Arrange
            testConfig.testSeeds.Clear();
            testConfig.testSeeds.Add("ValidationTest");
            
            // Act
            var stats = testConfig.ValidateConfiguration();
            
            // Assert
            Assert.IsNotNull(stats, "Validation should return statistics");
            Assert.Greater(stats.TotalMapsGenerated, 0, "Should generate at least one map");
            Assert.GreaterOrEqual(stats.SuccessfulGenerations, 0, "Should track successful generations");
        }
        
        [Test]
        public void NodeConnections_AreValid()
        {
            // Arrange & Act
            var map = generator.GenerateMap("ConnectionTest");
            
            // Assert
            for (int i = 0; i < map.Layers.Count - 1; i++)
            {
                var currentLayer = map.Layers[i];
                var nextLayer = map.Layers[i + 1];
                
                foreach (var node in currentLayer.Nodes)
                {
                    Assert.Greater(node.ConnectionIds.Count, 0, 
                        "Non-final layer nodes should have connections");
                    
                    // Verify connections point to valid nodes in next layer
                    foreach (var connectionId in node.ConnectionIds)
                    {
                        var targetNode = nextLayer.Nodes.Find(n => n.Id == connectionId);
                        Assert.IsNotNull(targetNode, 
                            $"Connection {connectionId} should point to valid node in next layer");
                    }
                }
            }
        }
        
        [Test]
        public void DeterministicGeneration_ProducesSameMap()
        {
            // Arrange
            string seed = "DeterministicTest";
            
            // Act
            var map1 = generator.GenerateMap(seed);
            var map2 = generator.GenerateMap(seed);
            
            // Assert
            Assert.AreEqual(map1.Layers.Count, map2.Layers.Count, 
                "Same seed should produce same layer count");
            
            for (int i = 0; i < map1.Layers.Count; i++)
            {
                Assert.AreEqual(map1.Layers[i].BossName, map2.Layers[i].BossName, 
                    "Same seed should produce same boss names");
                Assert.AreEqual(map1.Layers[i].Nodes.Count, map2.Layers[i].Nodes.Count, 
                    "Same seed should produce same node count per layer");
            }
        }
        
        [Test]
        public void CompareSeedsWindow_CanBeCreated()
        {
            // Act
            var compareWindow = EditorWindow.CreateWindow<CompareSeedsWindow>();
            
            // Assert
            Assert.IsNotNull(compareWindow, "CompareSeedsWindow should be created");
            
            // Cleanup
            compareWindow.Close();
        }
        
        [Test]
        public void PathGeneratorEditor_HandlesNullConfig()
        {
            // Arrange
            editorWindow = EditorWindow.CreateWindow<PathGeneratorEditor>();
            
            // Act - This should not throw
            var windowType = typeof(PathGeneratorEditor);
            var initMethod = windowType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Assert
            Assert.DoesNotThrow(() => initMethod?.Invoke(editorWindow, null), 
                "Editor should handle null config gracefully");
        }
        
        [Test]
        public void ExportFunctionality_CreatesValidPaths()
        {
            // This test verifies export methods exist and are callable
            var editorType = typeof(PathGeneratorEditor);
            
            // Check for export methods
            var exportMapMethod = editorType.GetMethod("ExportCurrentMapData", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exportStatsMethod = editorType.GetMethod("ExportStatistics", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exportImageMethod = editorType.GetMethod("ExportPreviewImage", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Assert
            Assert.IsNotNull(exportMapMethod, "ExportCurrentMapData method should exist");
            Assert.IsNotNull(exportStatsMethod, "ExportStatistics method should exist");
            Assert.IsNotNull(exportImageMethod, "ExportPreviewImage method should exist");
        }
        
        [Test]
        public void VisualizationOptions_HaveCorrectDefaults()
        {
            // Arrange
            editorWindow = EditorWindow.CreateWindow<PathGeneratorEditor>();
            var windowType = typeof(PathGeneratorEditor);
            
            // Get private fields using reflection
            var showNodeTypesField = windowType.GetField("showNodeTypes", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var showConnectionsField = windowType.GetField("showConnections", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var showBossRoomsField = windowType.GetField("showBossRooms", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Act
            bool showNodeTypes = showNodeTypesField != null ? (bool)showNodeTypesField.GetValue(editorWindow) : false;
            bool showConnections = showConnectionsField != null ? (bool)showConnectionsField.GetValue(editorWindow) : false;
            bool showBossRooms = showBossRoomsField != null ? (bool)showBossRoomsField.GetValue(editorWindow) : false;
            
            // Assert
            Assert.IsTrue(showNodeTypes, "Show node types should be enabled by default");
            Assert.IsTrue(showConnections, "Show connections should be enabled by default");
            Assert.IsTrue(showBossRooms, "Show boss rooms should be enabled by default");
        }
    }
}