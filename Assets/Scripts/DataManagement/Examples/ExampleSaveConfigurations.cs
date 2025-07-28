using UnityEngine;
using NeonLadderURP.DataManagement;
using System.Collections.Generic;

namespace NeonLadderURP.DataManagement.Examples
{
    /// <summary>
    /// Example save configurations for testing and demonstration purposes.
    /// These can be created as ScriptableObject assets for quick testing scenarios.
    /// </summary>
    public static class ExampleSaveConfigurations
    {
        /// <summary>
        /// Create a new player configuration with default starting values
        /// </summary>
        public static SaveStateConfiguration CreateNewPlayerConfig()
        {
            var config = ScriptableObject.CreateInstance<SaveStateConfiguration>();
            
            // Basic new player setup
            var playerSetup = new PlayerProgressionSetup
            {
                playerLevel = 1,
                experiencePoints = 0f,
                maxHealth = 100,
                currentHealth = 100,
                maxStamina = 100f,
                currentStamina = 100f,
                attackDamage = 10f,
                attackSpeed = 1f,
                movementSpeed = 5f,
                jumpCount = 1
            };
            
            var currencySetup = new CurrencySetup
            {
                startingMetaCurrency = 0,
                startingPermaCurrency = 0,
                totalMetaEarned = 0,
                totalPermaEarned = 0
            };
            
            var worldSetup = new WorldStateSetup
            {
                currentSceneName = "SampleScene",
                playerPosition = Vector3.zero,
                currentCheckpoint = "",
                currentDepth = 0,
                runNumber = 1,
                isActiveRun = true
            };
            
            return config;
        }
        
        /// <summary>
        /// Create a mid-game configuration with some progression
        /// </summary>
        public static SaveStateConfiguration CreateMidGameConfig()
        {
            var config = CreateNewPlayerConfig();
            
            // Mid-game progression
            var playerSetup = new PlayerProgressionSetup
            {
                playerLevel = 10,
                experiencePoints = 2500f,
                maxHealth = 150,
                currentHealth = 120,
                maxStamina = 120f,
                currentStamina = 90f,
                attackDamage = 15f,
                attackSpeed = 1.2f,
                movementSpeed = 6f,
                jumpCount = 2 // Double jump unlocked
            };
            
            var currencySetup = new CurrencySetup
            {
                startingMetaCurrency = 250,
                startingPermaCurrency = 50,
                totalMetaEarned = 1000,
                totalPermaEarned = 200
            };
            
            var worldSetup = new WorldStateSetup
            {
                currentSceneName = "Level_5",
                playerPosition = new Vector3(15f, 3f, 0f),
                currentCheckpoint = "checkpoint_3",
                currentDepth = 5,
                runNumber = 3,
                isActiveRun = true
            };
            
            // Add some completed content
            worldSetup.completedScenes.AddRange(new[]
            {
                "Tutorial", "Level_1", "Level_2", "Level_3", "Level_4"
            });
            
            worldSetup.defeatedBosses.AddRange(new[]
            {
                "TutorialBoss", "ForestBoss"
            });
            
            worldSetup.discoveredAreas.AddRange(new[]
            {
                "SecretCave", "HiddenShop", "BonusRoom"
            });
            
            return config;
        }
        
        /// <summary>
        /// Create an end-game configuration with maximum progression
        /// </summary>
        public static SaveStateConfiguration CreateEndGameConfig()
        {
            var config = CreateMidGameConfig();
            
            // End-game progression
            var playerSetup = new PlayerProgressionSetup
            {
                playerLevel = 50,
                experiencePoints = 125000f,
                maxHealth = 300,
                currentHealth = 300,
                maxStamina = 200f,
                currentStamina = 200f,
                attackDamage = 50f,
                attackSpeed = 2f,
                movementSpeed = 10f,
                jumpCount = 3 // Triple jump
            };
            
            var currencySetup = new CurrencySetup
            {
                startingMetaCurrency = 5000,
                startingPermaCurrency = 2000,
                totalMetaEarned = 50000,
                totalPermaEarned = 10000
            };
            
            var worldSetup = new WorldStateSetup
            {
                currentSceneName = "FinalBossArena",
                playerPosition = new Vector3(0f, 10f, 0f),
                currentCheckpoint = "final_checkpoint",
                currentDepth = 25,
                runNumber = 15,
                isActiveRun = true
            };
            
            // Add extensive completed content
            for (int i = 1; i <= 25; i++)
            {
                worldSetup.completedScenes.Add($"Level_{i}");
                if (i % 5 == 0) // Boss every 5 levels
                {
                    worldSetup.defeatedBosses.Add($"Boss_Level_{i}");
                }
            }
            
            worldSetup.discoveredAreas.AddRange(new[]
            {
                "SecretCave", "HiddenShop", "BonusRoom", "TreasureVault",
                "AncientLibrary", "CrystalCaverns", "FloatingIslands"
            });
            
            return config;
        }
        
        /// <summary>
        /// Create a testing configuration with specific procedural generation setup
        /// </summary>
        public static SaveStateConfiguration CreateTestingConfig()
        {
            var config = CreateNewPlayerConfig();
            
            // Testing-specific setup
            var playerSetup = new PlayerProgressionSetup
            {
                playerLevel = 5,
                maxHealth = 200, // High health for testing
                currentHealth = 200,
                maxStamina = 200f,
                currentStamina = 200f,
                attackDamage = 999f, // High damage for quick testing
                movementSpeed = 15f, // Fast movement for testing
                jumpCount = 5 // Multiple jumps for testing
            };
            
            var currencySetup = new CurrencySetup
            {
                startingMetaCurrency = 9999,
                startingPermaCurrency = 9999
            };
            
            // Create test procedural scene set
            var testSceneSet = new ProceduralSceneSet
            {
                setName = "Testing Scene Set",
                description = "Predefined scenes for testing specific scenarios",
                seed = 42, // Fixed seed for reproducible testing
                pathType = "test",
                maxDepth = 10
            };
            
            // Add test scenes
            testSceneSet.sceneConfigurations.AddRange(new[]
            {
                new SceneConfiguration
                {
                    sceneName = "TestRoom_Combat",
                    depth = 1,
                    pathType = "main",
                    spawnPosition = Vector3.zero,
                    isPreCompleted = false,
                    sceneSpecificData = new List<KeyValueData>
                    {
                        new KeyValueData { key = "enemy_count", value = "5" },
                        new KeyValueData { key = "enemy_type", value = "basic" }
                    }
                },
                new SceneConfiguration
                {
                    sceneName = "TestRoom_Boss",
                    depth = 2,
                    pathType = "boss",
                    spawnPosition = new Vector3(0, 0, 0),
                    isPreCompleted = false,
                    sceneSpecificData = new List<KeyValueData>
                    {
                        new KeyValueData { key = "boss_type", value = "test_boss" },
                        new KeyValueData { key = "difficulty", value = "easy" }
                    }
                },
                new SceneConfiguration
                {
                    sceneName = "TestRoom_Shop",
                    depth = 1,
                    pathType = "shop",
                    spawnPosition = new Vector3(10, 0, 0),
                    isPreCompleted = false,
                    sceneSpecificData = new List<KeyValueData>
                    {
                        new KeyValueData { key = "shop_type", value = "test_shop" },
                        new KeyValueData { key = "discount", value = "50" }
                    }
                }
            });
            
            return config;
        }
        
        /// <summary>
        /// Create a regression testing configuration for bug reproduction
        /// </summary>
        public static SaveStateConfiguration CreateRegressionTestConfig()
        {
            var config = CreateMidGameConfig();
            
            var worldSetup = new WorldStateSetup
            {
                currentSceneName = "BugReproductionScene",
                playerPosition = new Vector3(5.5f, 2.3f, 0f), // Specific position that might trigger bugs
                currentDepth = 7,
                runNumber = 4,
                isActiveRun = true
            };
            
            // Add specific state that might cause issues
            worldSetup.completedScenes.AddRange(new[]
            {
                "Scene_With_Special_Characters!@#",
                "VeryLongSceneNameThatMightCauseIssuesWithSerialization_12345",
                "" // Empty scene name to test edge cases
            });
            
            return config;
        }
    }
}