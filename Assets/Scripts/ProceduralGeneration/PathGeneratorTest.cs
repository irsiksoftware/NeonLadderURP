using UnityEngine;
using System.Linq;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Test script to verify the mystical reproducibility of the PathGenerator
    /// By Stephen Strange - "The same seed shall always yield the same destiny"
    /// </summary>
    public class PathGeneratorTest : MonoBehaviour
    {
        [Header("Mystical Testing")]
        [SerializeField] private string testSeed = "MYSTIC_TEST_SEED_42";
        [SerializeField] private int reproductionTests = 5;
        
        private void Start()
        {
            TestSeedReproduction();
            TestVariousSeedInputs();
        }

        /// <summary>
        /// Tests that the same seed produces identical results across multiple generations
        /// </summary>
        private void TestSeedReproduction()
        {
            Debug.Log("🔮 <color=cyan>Testing Mystical Seed Reproduction...</color>");
            
            var generator = new PathGenerator();
            MysticalMap baselineMap = null;
            bool allIdentical = true;

            for (int i = 0; i < reproductionTests; i++)
            {
                var generatedMap = generator.GenerateMap(testSeed);
                
                if (i == 0)
                {
                    baselineMap = generatedMap;
                    Debug.Log($"✨ Baseline map generated with seed: <color=yellow>{testSeed}</color>");
                    LogMapSummary(baselineMap, "BASELINE");
                }
                else
                {
                    bool isIdentical = CompareMaps(baselineMap, generatedMap);
                    if (!isIdentical)
                    {
                        allIdentical = false;
                        Debug.LogError($"❌ Test {i + 1} failed - Maps are not identical!");
                        LogMapSummary(generatedMap, $"TEST_{i + 1}");
                    }
                    else
                    {
                        Debug.Log($"✅ Test {i + 1} passed - Map identical to baseline");
                    }
                }
            }

            if (allIdentical)
            {
                Debug.Log("<color=green>🎉 MYSTICAL SUCCESS: All maps are perfectly identical!</color>");
            }
            else
            {
                Debug.LogError("<color=red>💀 MYSTICAL FAILURE: Some maps differ from baseline!</color>");
            }
        }

        /// <summary>
        /// Tests various seed inputs to ensure different seeds create different maps
        /// </summary>
        private void TestVariousSeedInputs()
        {
            Debug.Log("\n🌟 <color=cyan>Testing Various Mystical Seeds...</color>");
            
            var generator = new PathGenerator();
            string[] testSeeds = {
                "TestSeed1",
                "AnotherSeed",
                "12345",
                "SpecialCharacters!@#$",
                "VeryLongSeedStringToTestLimits1234567890",
                "" // Empty seed should generate a random one
            };

            var generatedMaps = testSeeds.Select(seed => 
            {
                var map = generator.GenerateMap(seed);
                Debug.Log($"🗺️ Generated map for seed: '<color=yellow>{(string.IsNullOrEmpty(seed) ? "[RANDOM]" : seed)}</color>' -> Actual seed: '<color=cyan>{map.Seed}</color>'");
                LogMapSummary(map, seed);
                return map;
            }).ToList();

            // Verify all maps are different (except empty seeds which are random)
            bool allUnique = true;
            for (int i = 0; i < generatedMaps.Count - 1; i++)
            {
                for (int j = i + 1; j < generatedMaps.Count; j++)
                {
                    // Skip comparison if either seed was empty (random generation)
                    if (string.IsNullOrEmpty(testSeeds[i]) || string.IsNullOrEmpty(testSeeds[j]))
                        continue;
                        
                    if (CompareMaps(generatedMaps[i], generatedMaps[j]))
                    {
                        Debug.LogWarning($"⚠️ Seeds '{testSeeds[i]}' and '{testSeeds[j]}' produced identical maps!");
                        allUnique = false;
                    }
                }
            }

            if (allUnique)
            {
                Debug.Log("<color=green>🎊 DIVERSITY SUCCESS: All different seeds produced unique maps!</color>");
            }
        }

        /// <summary>
        /// Compares two mystical maps for perfect equality
        /// </summary>
        private bool CompareMaps(MysticalMap map1, MysticalMap map2)
        {
            if (map1.Seed != map2.Seed) return false;
            if (map1.Layers.Count != map2.Layers.Count) return false;

            for (int i = 0; i < map1.Layers.Count; i++)
            {
                var layer1 = map1.Layers[i];
                var layer2 = map2.Layers[i];

                if (layer1.LayerIndex != layer2.LayerIndex) return false;
                if (layer1.Boss != layer2.Boss) return false;
                if (layer1.Location != layer2.Location) return false;
                if (layer1.Nodes.Count != layer2.Nodes.Count) return false;

                for (int j = 0; j < layer1.Nodes.Count; j++)
                {
                    var node1 = layer1.Nodes[j];
                    var node2 = layer2.Nodes[j];

                    if (node1.Id != node2.Id) return false;
                    if (node1.Type != node2.Type) return false;
                    if (node1.LayerIndex != node2.LayerIndex) return false;
                    if (node1.PathIndex != node2.PathIndex) return false;
                    if (node1.NodeIndex != node2.NodeIndex) return false;

                    // Compare properties
                    if (node1.Properties.Count != node2.Properties.Count) return false;
                    foreach (var kvp in node1.Properties)
                    {
                        if (!node2.Properties.ContainsKey(kvp.Key)) return false;
                        if (!kvp.Value.Equals(node2.Properties[kvp.Key])) return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Logs a summary of a mystical map
        /// </summary>
        private void LogMapSummary(MysticalMap map, string label)
        {
            var totalNodes = map.Layers.Sum(layer => layer.Nodes.Count);
            var bossCount = map.Layers.Sum(layer => layer.Nodes.Count(node => node.Type == NodeType.Boss));
            var encounterCount = map.Layers.Sum(layer => layer.Nodes.Count(node => node.Type == NodeType.Encounter));
            var eventCount = map.Layers.Sum(layer => layer.Nodes.Count(node => node.Type == NodeType.Event));
            var restShopCount = map.Layers.Sum(layer => layer.Nodes.Count(node => node.Type == NodeType.RestShop));

            Debug.Log($"📊 [{label}] Layers: {map.Layers.Count}, Total Nodes: {totalNodes}, " +
                     $"Bosses: {bossCount}, Encounters: {encounterCount}, Events: {eventCount}, Rest/Shop: {restShopCount}");
        }

        /// <summary>
        /// Manual test method - call from inspector or other scripts
        /// </summary>
        [ContextMenu("Test Seed Reproduction")]
        public void ManualTestSeedReproduction()
        {
            TestSeedReproduction();
        }

        /// <summary>
        /// Generate and display a test map for debugging
        /// </summary>
        [ContextMenu("Generate Test Map")]
        public void GenerateTestMap()
        {
            var generator = new PathGenerator();
            var map = generator.GenerateMap(testSeed);
            
            Debug.Log($"🗺️ <color=cyan>Generated Mystical Map with seed: {map.Seed}</color>");
            
            foreach (var layer in map.Layers)
            {
                Debug.Log($"🏰 Layer {layer.LayerIndex}: {layer.Boss} at {layer.Location} ({layer.Nodes.Count} nodes)");
                
                foreach (var node in layer.Nodes)
                {
                    var props = string.Join(", ", node.Properties.Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    Debug.Log($"  🔸 {node.Id}: {node.Type} [{props}]");
                }
            }
        }
    }
}