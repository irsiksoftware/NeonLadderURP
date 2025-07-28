using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Flexible rule system for procedural generation - Slay the Spire style guarantees
    /// By Stephen Strange - "Even chaos must follow certain mystical laws"
    /// </summary>
    
    [Serializable]
    public class GenerationRules
    {
        [Header("Path Structure Rules")]
        [Tooltip("Minimum nodes per path (excluding boss)")]
        public int minNodesPerPath = 3;
        
        [Tooltip("Maximum nodes per path (excluding boss)")]
        public int maxNodesPerPath = 6;
        
        [Tooltip("Minimum number of paths per layer")]
        public int minPathsPerLayer = 3;
        
        [Tooltip("Maximum number of paths per layer")]
        public int maxPathsPerLayer = 5;

        [Header("Guaranteed Encounters")]
        [Tooltip("Must have at least this many combat encounters per layer")]
        public int guaranteedCombatPerLayer = 1;
        
        [Tooltip("Must have at least this many minor enemy encounters per layer")]
        public int guaranteedMinorEnemiesPerLayer = 1;
        
        [Tooltip("Must have at least one rest/shop opportunity per layer")]
        public bool guaranteedRestShopPerLayer = true;
        
        [Tooltip("Maximum major enemies per layer (to prevent overwhelming difficulty)")]
        public int maxMajorEnemiesPerLayer = 2;

        [Header("Event Distribution")]
        [Tooltip("Minimum events per layer")]
        public int minEventsPerLayer = 0;
        
        [Tooltip("Maximum events per layer")]
        public int maxEventsPerLayer = 2;
        
        [Tooltip("Chance for events increases with layer depth")]
        public bool eventChanceScalesWithDepth = true;
        
        [Tooltip("Base event chance (0.0 to 1.0)")]
        [Range(0f, 1f)]
        public float baseEventChance = 0.3f;

        [Header("Layer Progression")]
        [Tooltip("Paths get longer as layers progress")]
        public bool pathsGrowWithDepth = true;
        
        [Tooltip("Difficulty scales with layer index")]
        public bool difficultyScalesWithDepth = true;
        
        [Tooltip("More path choices in later layers")]
        public bool moreChoicesInLaterLayers = true;

        [Header("Balancing Rules")]
        [Tooltip("Ensure each path has balanced risk/reward")]
        public bool enforcePathBalance = true;
        
        [Tooltip("No two adjacent nodes of same type (prevents boring sequences)")]
        public bool preventAdjacentSameType = true;
        
        [Tooltip("Boss must be preceded by rest/shop opportunity")]
        public bool restShopBeforeBoss = true;

        [Header("Seed Flexibility")]
        [Tooltip("Allow seed to influence rule application (some seeds might break rules for variety)")]
        public bool allowSeedRuleVariation = true;
        
        [Tooltip("Percentage of rules that can be bent based on seed (0.0 to 1.0)")]
        [Range(0f, 0.5f)]
        public float ruleFlexibility = 0.1f;

        /// <summary>
        /// Validates that a generated layer follows all rules
        /// </summary>
        public ValidationResult ValidateLayer(MapLayer layer, System.Random seededRandom)
        {
            var result = new ValidationResult();
            
            // Check path count
            var pathCount = layer.Nodes.GroupBy(n => n.PathIndex).Count();
            if (pathCount < minPathsPerLayer || pathCount > maxPathsPerLayer)
            {
                result.AddViolation($"Layer {layer.LayerIndex} has {pathCount} paths, expected {minPathsPerLayer}-{maxPathsPerLayer}");
            }

            // Check guaranteed encounters
            var combatNodes = layer.Nodes.Where(n => n.Type == NodeType.Encounter).ToList();
            if (combatNodes.Count < guaranteedCombatPerLayer)
            {
                result.AddViolation($"Layer {layer.LayerIndex} has {combatNodes.Count} combat encounters, minimum required: {guaranteedCombatPerLayer}");
            }

            // Check minor enemies
            var minorEnemyCount = combatNodes.Count(n => 
                n.Properties.ContainsKey("EncounterType") && 
                (EncounterType)n.Properties["EncounterType"] == EncounterType.MinorEnemy);
            
            if (minorEnemyCount < guaranteedMinorEnemiesPerLayer)
            {
                result.AddViolation($"Layer {layer.LayerIndex} has {minorEnemyCount} minor enemy encounters, minimum required: {guaranteedMinorEnemiesPerLayer}");
            }

            // Check rest/shop availability
            if (guaranteedRestShopPerLayer)
            {
                var restShopNodes = layer.Nodes.Where(n => n.Type == NodeType.RestShop).ToList();
                if (restShopNodes.Count == 0)
                {
                    result.AddViolation($"Layer {layer.LayerIndex} missing guaranteed rest/shop opportunity");
                }
            }

            // Check major enemy limits
            var majorEnemyCount = combatNodes.Count(n => 
                n.Properties.ContainsKey("EncounterType") && 
                (EncounterType)n.Properties["EncounterType"] == EncounterType.MajorEnemy);
            
            if (majorEnemyCount > maxMajorEnemiesPerLayer)
            {
                result.AddViolation($"Layer {layer.LayerIndex} has {majorEnemyCount} major enemies, maximum allowed: {maxMajorEnemiesPerLayer}");
            }

            // Check rest/shop before boss rule
            if (restShopBeforeBoss)
            {
                var pathGroups = layer.Nodes.GroupBy(n => n.PathIndex);
                foreach (var pathGroup in pathGroups)
                {
                    var pathNodes = pathGroup.OrderBy(n => n.NodeIndex).ToList();
                    if (pathNodes.Count >= 2)
                    {
                        var secondToLast = pathNodes[pathNodes.Count - 2];
                        var last = pathNodes[pathNodes.Count - 1];
                        
                        if (last.Type == NodeType.Boss && secondToLast.Type != NodeType.RestShop)
                        {
                            // Allow flexibility based on seed
                            if (!allowSeedRuleVariation || seededRandom.NextDouble() > ruleFlexibility)
                            {
                                result.AddViolation($"Layer {layer.LayerIndex}, Path {pathGroup.Key} boss not preceded by rest/shop");
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Adjusts rules based on layer depth and seed
        /// </summary>
        public GenerationRules GetAdjustedRulesForLayer(int layerIndex, System.Random seededRandom)
        {
            var adjusted = new GenerationRules();
            
            // Copy base rules
            CopyTo(adjusted);
            
            // Apply depth scaling
            if (pathsGrowWithDepth)
            {
                adjusted.minNodesPerPath = Math.Max(3, minNodesPerPath + (layerIndex / 2));
                adjusted.maxNodesPerPath = Math.Max(adjusted.minNodesPerPath + 2, maxNodesPerPath + (layerIndex / 2));
            }
            
            if (moreChoicesInLaterLayers)
            {
                adjusted.minPathsPerLayer = Math.Max(minPathsPerLayer, minPathsPerLayer + (layerIndex / 3));
                adjusted.maxPathsPerLayer = Math.Max(adjusted.minPathsPerLayer, maxPathsPerLayer + (layerIndex / 3));
            }
            
            if (eventChanceScalesWithDepth)
            {
                adjusted.baseEventChance = Math.Min(0.7f, baseEventChance + (layerIndex * 0.1f));
                adjusted.maxEventsPerLayer = Math.Max(maxEventsPerLayer, maxEventsPerLayer + (layerIndex / 2));
            }
            
            // Apply seed-based flexibility
            if (allowSeedRuleVariation && seededRandom.NextDouble() < ruleFlexibility)
            {
                // Occasionally bend rules for variety
                if (seededRandom.NextDouble() < 0.3f)
                {
                    adjusted.guaranteedMinorEnemiesPerLayer = Math.Max(0, guaranteedMinorEnemiesPerLayer - 1);
                }
                
                if (seededRandom.NextDouble() < 0.2f)
                {
                    adjusted.maxMajorEnemiesPerLayer = Math.Min(4, maxMajorEnemiesPerLayer + 1);
                }
            }
            
            return adjusted;
        }

        /// <summary>
        /// Copies rules to another instance
        /// </summary>
        public void CopyTo(GenerationRules target)
        {
            target.minNodesPerPath = minNodesPerPath;
            target.maxNodesPerPath = maxNodesPerPath;
            target.minPathsPerLayer = minPathsPerLayer;
            target.maxPathsPerLayer = maxPathsPerLayer;
            target.guaranteedCombatPerLayer = guaranteedCombatPerLayer;
            target.guaranteedMinorEnemiesPerLayer = guaranteedMinorEnemiesPerLayer;
            target.guaranteedRestShopPerLayer = guaranteedRestShopPerLayer;
            target.maxMajorEnemiesPerLayer = maxMajorEnemiesPerLayer;
            target.minEventsPerLayer = minEventsPerLayer;
            target.maxEventsPerLayer = maxEventsPerLayer;
            target.eventChanceScalesWithDepth = eventChanceScalesWithDepth;
            target.baseEventChance = baseEventChance;
            target.pathsGrowWithDepth = pathsGrowWithDepth;
            target.difficultyScalesWithDepth = difficultyScalesWithDepth;
            target.moreChoicesInLaterLayers = moreChoicesInLaterLayers;
            target.enforcePathBalance = enforcePathBalance;
            target.preventAdjacentSameType = preventAdjacentSameType;
            target.restShopBeforeBoss = restShopBeforeBoss;
            target.allowSeedRuleVariation = allowSeedRuleVariation;
            target.ruleFlexibility = ruleFlexibility;
        }

        /// <summary>
        /// Creates default "Balanced" rules similar to Slay the Spire
        /// </summary>
        public static GenerationRules CreateBalancedRules()
        {
            return new GenerationRules
            {
                minNodesPerPath = 3,
                maxNodesPerPath = 5,
                minPathsPerLayer = 3,
                maxPathsPerLayer = 4,
                guaranteedCombatPerLayer = 2,
                guaranteedMinorEnemiesPerLayer = 1,
                guaranteedRestShopPerLayer = true,
                maxMajorEnemiesPerLayer = 2,
                minEventsPerLayer = 0,
                maxEventsPerLayer = 1,
                eventChanceScalesWithDepth = true,
                baseEventChance = 0.25f,
                pathsGrowWithDepth = true,
                difficultyScalesWithDepth = true,
                moreChoicesInLaterLayers = true,
                enforcePathBalance = true,
                preventAdjacentSameType = true,
                restShopBeforeBoss = true,
                allowSeedRuleVariation = true,
                ruleFlexibility = 0.1f
            };
        }

        /// <summary>
        /// Creates "Chaotic" rules for more unpredictable generation
        /// </summary>
        public static GenerationRules CreateChaoticRules()
        {
            return new GenerationRules
            {
                minNodesPerPath = 2,
                maxNodesPerPath = 7,
                minPathsPerLayer = 2,
                maxPathsPerLayer = 6,
                guaranteedCombatPerLayer = 1,
                guaranteedMinorEnemiesPerLayer = 0,
                guaranteedRestShopPerLayer = false,
                maxMajorEnemiesPerLayer = 4,
                minEventsPerLayer = 0,
                maxEventsPerLayer = 3,
                eventChanceScalesWithDepth = true,
                baseEventChance = 0.4f,
                pathsGrowWithDepth = false,
                difficultyScalesWithDepth = true,
                moreChoicesInLaterLayers = false,
                enforcePathBalance = false,
                preventAdjacentSameType = false,
                restShopBeforeBoss = false,
                allowSeedRuleVariation = true,
                ruleFlexibility = 0.3f
            };
        }

        /// <summary>
        /// Creates "Safe" rules for guaranteed progression
        /// </summary>
        public static GenerationRules CreateSafeRules()
        {
            return new GenerationRules
            {
                minNodesPerPath = 4,
                maxNodesPerPath = 4,
                minPathsPerLayer = 3,
                maxPathsPerLayer = 3,
                guaranteedCombatPerLayer = 1,
                guaranteedMinorEnemiesPerLayer = 1,
                guaranteedRestShopPerLayer = true,
                maxMajorEnemiesPerLayer = 1,
                minEventsPerLayer = 1,
                maxEventsPerLayer = 1,
                eventChanceScalesWithDepth = false,
                baseEventChance = 0.2f,
                pathsGrowWithDepth = false,
                difficultyScalesWithDepth = false,
                moreChoicesInLaterLayers = false,
                enforcePathBalance = true,
                preventAdjacentSameType = true,
                restShopBeforeBoss = true,
                allowSeedRuleVariation = false,
                ruleFlexibility = 0f
            };
        }
    }

    /// <summary>
    /// Result of rule validation
    /// </summary>
    [Serializable]
    public class ValidationResult
    {
        public List<string> Violations { get; private set; } = new List<string>();
        public bool IsValid => Violations.Count == 0;

        public void AddViolation(string violation)
        {
            Violations.Add(violation);
        }

        public string GetSummary()
        {
            if (IsValid)
                return "✅ All rules satisfied";
            
            return $"❌ {Violations.Count} rule violations:\n" + string.Join("\n", Violations.Select(v => $"  • {v}"));
        }
    }
}