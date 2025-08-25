using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using NeonLadder.Debugging;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Visual ScriptableObject for configuring and testing the Mystical PathGenerator
    /// By Stephen Strange - "Through visualization, we master the infinite possibilities"
    /// </summary>
    [CreateAssetMenu(fileName = "PathGenerator Config", menuName = "NeonLadder/Procedural Generation/Path Generator Config")]
    public class PathGeneratorConfig : ScriptableObject
    {
        [Header("🔮 Mystical Configuration")]
        [Tooltip("Name of this configuration preset")]
        public string configurationName = "Default Configuration";
        
        [TextArea(3, 5)]
        [Tooltip("Description of this configuration's purpose and characteristics")]
        public string description = "Balanced configuration similar to Slay the Spire";

        [Header("📜 Generation Rules")]
        [Tooltip("Rules that govern the procedural generation")]
        public GenerationRules rules = GenerationRules.CreateBalancedRules();

        [Header("🧪 Testing & Validation")]
        [Tooltip("Seeds to test with this configuration")]
        public List<string> testSeeds = new List<string> 
        { 
            "TestSeed123", 
            "AnotherSeed", 
            "EmptyStringTest",
            "SpecialChars!@#$",
            "VeryLongSeedForTesting1234567890"
        };

        [Tooltip("Show detailed validation results in inspector")]
        public bool showValidationDetails = true;
        
        [Tooltip("Auto-validate when configuration changes")]
        public bool autoValidateOnChange = true;

        [Header("🎮 Preview Settings")]
        [Tooltip("Number of maps to generate for preview")]
        [Range(1, 10)]
        public int previewMapCount = 3;
        
        [Tooltip("Show node properties in preview")]
        public bool showNodeProperties = false;
        
        [Tooltip("Color-code nodes by type in preview")]
        public bool colorCodeNodeTypes = true;

        [Header("📊 Statistics")]
        [SerializeField, HideInInspector]
        private ValidationStats lastValidationStats = new ValidationStats();

        // Runtime testing data
        [NonSerialized]
        private PathGenerator generator;
        [NonSerialized] 
        private List<ProceduralMap> previewMaps = new List<ProceduralMap>();
        [NonSerialized]
        private List<ValidationResult> validationResults = new List<ValidationResult>();

        /// <summary>
        /// Validates the current configuration against all test seeds
        /// </summary>
        [ContextMenu("🔍 Validate Configuration")]
        public ValidationStats ValidateConfiguration()
        {
            if (generator == null)
                generator = new PathGenerator();

            var stats = new ValidationStats();
            validationResults.Clear();
            previewMaps.Clear();

            Debugger.Log($"🔮 <color=cyan>Validating PathGenerator Config: {configurationName}</color>");

            foreach (var seed in testSeeds)
            {
                try
                {
                    // Generate map with rules
                    var map = generator.GenerateMapWithRules(seed, rules);
                    previewMaps.Add(map);
                    
                    // Validate each layer
                    var mapValidation = new ValidationResult();
                    foreach (var layer in map.Layers)
                    {
                        var layerValidation = rules.ValidateLayer(layer, new System.Random(GetSeedHash(seed)));
                        if (!layerValidation.IsValid)
                        {
                            foreach (var violation in layerValidation.Violations)
                            {
                                mapValidation.AddViolation($"[{seed}] {violation}");
                            }
                        }
                    }
                    
                    validationResults.Add(mapValidation);
                    
                    if (mapValidation.IsValid)
                    {
                        stats.validMaps++;
                        Debugger.Log($"✅ Seed '{seed}': <color=green>Valid</color>");
                    }
                    else
                    {
                        stats.invalidMaps++;
                        Debugger.LogWarning($"⚠️ Seed '{seed}': <color=yellow>{mapValidation.Violations.Count} violations</color>");
                        
                        if (showValidationDetails)
                        {
                            foreach (var violation in mapValidation.Violations)
                            {
                                Debugger.LogWarning($"   • {violation}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    stats.errorMaps++;
                    Debugger.LogError($"❌ Seed '{seed}': <color=red>Error - {ex.Message}</color>");
                }
            }

            // Calculate statistics
            stats.totalMaps = testSeeds.Count;
            stats.successRate = stats.totalMaps > 0 ? (float)stats.validMaps / stats.totalMaps : 0f;
            stats.configurationName = configurationName;
            stats.timestamp = DateTime.Now;

            lastValidationStats = stats;

            // Log summary
            Debugger.Log($"📊 <color=cyan>Validation Summary for {configurationName}:</color>");
            Debugger.Log($"   ✅ Valid: {stats.validMaps}/{stats.totalMaps} ({stats.successRate:P1})");
            Debugger.Log($"   ⚠️ Invalid: {stats.invalidMaps}");
            Debugger.Log($"   ❌ Errors: {stats.errorMaps}");

            return stats;
        }

        /// <summary>
        /// Generates and displays preview maps
        /// </summary>
        [ContextMenu("🗺️ Generate Preview Maps")]
        public void GeneratePreviewMaps()
        {
            if (generator == null)
                generator = new PathGenerator();

            Debugger.Log($"🗺️ <color=cyan>Generating {previewMapCount} preview maps for {configurationName}</color>");

            for (int i = 0; i < previewMapCount; i++)
            {
                var seed = i < testSeeds.Count ? testSeeds[i] : $"Preview_{i}_{DateTime.Now.Ticks}";
                var map = generator.GenerateMapWithRules(seed, rules);
                
                LogMapPreview(map, i + 1);
            }
        }

        /// <summary>
        /// Tests a specific seed with current configuration
        /// </summary>
        public ValidationResult TestSeed(string seed)
        {
            if (generator == null)
                generator = new PathGenerator();

            try
            {
                var map = generator.GenerateMapWithRules(seed, rules);
                var validation = new ValidationResult();

                foreach (var layer in map.Layers)
                {
                    var layerValidation = rules.ValidateLayer(layer, new System.Random(GetSeedHash(seed)));
                    if (!layerValidation.IsValid)
                    {
                        foreach (var violation in layerValidation.Violations)
                        {
                            validation.AddViolation(violation);
                        }
                    }
                }

                return validation;
            }
            catch (Exception ex)
            {
                var errorResult = new ValidationResult();
                errorResult.AddViolation($"Generation failed: {ex.Message}");
                return errorResult;
            }
        }

        /// <summary>
        /// Exports configuration to JSON
        /// </summary>
        [ContextMenu("📤 Export Configuration")]
        public string ExportConfiguration()
        {
            var exportData = new ConfigurationExport
            {
                configurationName = this.configurationName,
                description = this.description,
                rules = this.rules,
                testSeeds = this.testSeeds,
                exportTimestamp = DateTime.Now,
                validationStats = lastValidationStats
            };

            var json = JsonConvert.SerializeObject(exportData, Formatting.Indented);
            Debugger.Log($"📤 <color=green>Configuration exported:</color>\n{json}");
            
            // Copy to clipboard if possible
            GUIUtility.systemCopyBuffer = json;
            Debugger.Log("📋 Configuration copied to clipboard!");
            
            return json;
        }

        /// <summary>
        /// Imports configuration from JSON
        /// </summary>
        public bool ImportConfiguration(string json)
        {
            try
            {
                var importData = JsonConvert.DeserializeObject<ConfigurationExport>(json);
                
                configurationName = importData.configurationName;
                description = importData.description;
                rules = importData.rules;
                testSeeds = importData.testSeeds ?? new List<string>();

                Debugger.Log($"📥 <color=green>Configuration imported: {configurationName}</color>");
                
                if (autoValidateOnChange)
                {
                    ValidateConfiguration();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Debugger.LogError($"❌ Import failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Loads a preset configuration
        /// </summary>
        [ContextMenu("⚖️ Load Balanced Preset")]
        public void LoadBalancedPreset()
        {
            configurationName = "Balanced (Slay the Spire Style)";
            description = "Balanced configuration with guaranteed encounters and fair progression";
            rules = GenerationRules.CreateBalancedRules();
            
            if (autoValidateOnChange)
                ValidateConfiguration();
        }

        [ContextMenu("🌪️ Load Chaotic Preset")]
        public void LoadChaoticPreset()
        {
            configurationName = "Chaotic (High Variance)";
            description = "Unpredictable configuration with high variance and risk/reward";
            rules = GenerationRules.CreateChaoticRules();
            
            if (autoValidateOnChange)
                ValidateConfiguration();
        }

        [ContextMenu("🛡️ Load Safe Preset")]
        public void LoadSafePreset()
        {
            configurationName = "Safe (Guaranteed Progression)";
            description = "Conservative configuration ensuring player can always progress";
            rules = GenerationRules.CreateSafeRules();
            
            if (autoValidateOnChange)
                ValidateConfiguration();
        }

        /// <summary>
        /// Logs a detailed map preview
        /// </summary>
        private void LogMapPreview(ProceduralMap map, int previewIndex)
        {
            Debugger.Log($"🗺️ <color=cyan>Preview Map #{previewIndex} (Seed: {map.Seed})</color>");
            
            foreach (var layer in map.Layers)
            {
                var nodesByType = layer.Nodes.GroupBy(n => n.Type)
                    .ToDictionary(g => g.Key, g => g.Count());
                
                var nodeTypesSummary = string.Join(", ", nodesByType.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
                
                Debugger.Log($"  🏰 Layer {layer.LayerIndex}: {layer.Boss} at {layer.Location}");
                Debugger.Log($"     📊 Nodes: {nodeTypesSummary}");
                
                if (showNodeProperties)
                {
                    foreach (var node in layer.Nodes.Take(3)) // Show first 3 nodes as example
                    {
                        var props = string.Join(", ", node.Properties.Take(3).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                        Debugger.Log($"     🔸 {node.Id}: {node.Type} [{props}]");
                    }
                    if (layer.Nodes.Count > 3)
                    {
                        Debugger.Log($"     ... and {layer.Nodes.Count - 3} more nodes");
                    }
                }
            }
        }

        /// <summary>
        /// Gets hash from seed string
        /// </summary>
        private int GetSeedHash(string seed)
        {
            return string.IsNullOrEmpty(seed) ? 0 : seed.GetHashCode();
        }

        // Unity Editor integration
        private void OnValidate()
        {
            if (autoValidateOnChange && Application.isPlaying)
            {
                ValidateConfiguration();
            }
        }
    }

    /// <summary>
    /// Statistics from validation runs
    /// </summary>
    [Serializable]
    public class ValidationStats
    {
        public string configurationName;
        public int totalMaps;
        public int validMaps;
        public int invalidMaps;
        public int errorMaps;
        public float successRate;
        public DateTime timestamp;

        public string GetSummary()
        {
            return $"{configurationName}: {validMaps}/{totalMaps} valid ({successRate:P1}) - {timestamp:yyyy-MM-dd HH:mm}";
        }
    }

    /// <summary>
    /// Export/import data structure
    /// </summary>
    [Serializable]
    public class ConfigurationExport
    {
        public string configurationName;
        public string description;
        public GenerationRules rules;
        public List<string> testSeeds;
        public DateTime exportTimestamp;
        public ValidationStats validationStats;
    }
}