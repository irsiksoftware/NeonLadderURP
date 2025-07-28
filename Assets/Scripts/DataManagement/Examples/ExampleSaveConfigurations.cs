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
            
            // Configure player setup
            config.PlayerSetup.playerLevel = 1;
            config.PlayerSetup.experiencePoints = 0f;
            config.PlayerSetup.maxHealth = 100;
            config.PlayerSetup.currentHealth = 100;
            config.PlayerSetup.maxStamina = 100f;
            config.PlayerSetup.currentStamina = 100f;
            config.PlayerSetup.attackDamage = 10f;
            config.PlayerSetup.attackSpeed = 1f;
            config.PlayerSetup.movementSpeed = 5f;
            config.PlayerSetup.jumpCount = 1;
            
            // Configure currency setup
            config.CurrencySetup.startingMetaCurrency = 0;
            config.CurrencySetup.startingPermaCurrency = 0;
            config.CurrencySetup.totalMetaEarned = 0;
            config.CurrencySetup.totalPermaEarned = 0;
            
            // Configure world setup
            config.WorldSetup.currentSceneName = "SampleScene";
            config.WorldSetup.playerPosition = Vector3.zero;
            config.WorldSetup.currentCheckpoint = "";
            config.WorldSetup.currentDepth = 0;
            config.WorldSetup.runNumber = 1;
            config.WorldSetup.isActiveRun = true;
            
            return config;
        }
        
        /// <summary>
        /// Create a mid-game configuration with some progression
        /// </summary>
        public static SaveStateConfiguration CreateMidGameConfig()
        {
            var config = CreateNewPlayerConfig();
            
            // Configure mid-game progression
            config.PlayerSetup.playerLevel = 10;
            config.PlayerSetup.experiencePoints = 2500f;
            config.PlayerSetup.maxHealth = 150;
            config.PlayerSetup.currentHealth = 120;
            config.PlayerSetup.maxStamina = 120f;
            config.PlayerSetup.currentStamina = 90f;
            config.PlayerSetup.attackDamage = 15f;
            config.PlayerSetup.attackSpeed = 1.2f;
            config.PlayerSetup.movementSpeed = 6f;
            config.PlayerSetup.jumpCount = 2; // Double jump unlocked
            
            // Configure currency
            config.CurrencySetup.startingMetaCurrency = 250;
            config.CurrencySetup.startingPermaCurrency = 50;
            config.CurrencySetup.totalMetaEarned = 1000;
            config.CurrencySetup.totalPermaEarned = 200;
            
            // Configure world state
            config.WorldSetup.currentSceneName = "Level_5";
            config.WorldSetup.playerPosition = new Vector3(15f, 3f, 0f);
            config.WorldSetup.currentCheckpoint = "checkpoint_3";
            config.WorldSetup.currentDepth = 5;
            config.WorldSetup.runNumber = 3;
            config.WorldSetup.isActiveRun = true;
            
            // Add some completed content
            config.WorldSetup.completedScenes.AddRange(new[]
            {
                "Tutorial", "Level_1", "Level_2", "Level_3", "Level_4"
            });
            
            config.WorldSetup.defeatedBosses.AddRange(new[]
            {
                "TutorialBoss", "ForestBoss"
            });
            
            config.WorldSetup.discoveredAreas.AddRange(new[]
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
            
            // Configure end-game progression
            config.PlayerSetup.playerLevel = 50;
            config.PlayerSetup.experiencePoints = 125000f;
            config.PlayerSetup.maxHealth = 300;
            config.PlayerSetup.currentHealth = 300;
            config.PlayerSetup.maxStamina = 200f;
            config.PlayerSetup.currentStamina = 200f;
            config.PlayerSetup.attackDamage = 50f;
            config.PlayerSetup.attackSpeed = 2f;
            config.PlayerSetup.movementSpeed = 10f;
            config.PlayerSetup.jumpCount = 3; // Triple jump
            
            // Configure end-game currency
            config.CurrencySetup.startingMetaCurrency = 5000;
            config.CurrencySetup.startingPermaCurrency = 2000;
            config.CurrencySetup.totalMetaEarned = 50000;
            config.CurrencySetup.totalPermaEarned = 10000;
            
            // Configure end-game world state
            config.WorldSetup.currentSceneName = "FinalBossArena";
            config.WorldSetup.playerPosition = new Vector3(0f, 10f, 0f);
            config.WorldSetup.currentCheckpoint = "final_checkpoint";
            config.WorldSetup.currentDepth = 25;
            config.WorldSetup.runNumber = 15;
            config.WorldSetup.isActiveRun = true;
            
            // Add extensive completed content
            for (int i = 1; i <= 25; i++)
            {
                config.WorldSetup.completedScenes.Add($"Level_{i}");
                if (i % 5 == 0) // Boss every 5 levels
                {
                    config.WorldSetup.defeatedBosses.Add($"Boss_Level_{i}");
                }
            }
            
            config.WorldSetup.discoveredAreas.AddRange(new[]
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
            
            // Configure testing-specific setup
            config.PlayerSetup.playerLevel = 5;
            config.PlayerSetup.maxHealth = 200; // High health for testing
            config.PlayerSetup.currentHealth = 200;
            config.PlayerSetup.maxStamina = 200f;
            config.PlayerSetup.currentStamina = 200f;
            config.PlayerSetup.attackDamage = 999f; // High damage for quick testing
            config.PlayerSetup.movementSpeed = 15f; // Fast movement for testing
            config.PlayerSetup.jumpCount = 5; // Multiple jumps for testing
            
            // Configure testing currency
            config.CurrencySetup.startingMetaCurrency = 9999;
            config.CurrencySetup.startingPermaCurrency = 9999;
            
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
            
            // Apply the scene set to the configuration
            config.CurrentSceneSet = testSceneSet;
            
            return config;
        }
        
        /// <summary>
        /// Create a regression testing configuration for bug reproduction
        /// </summary>
        public static SaveStateConfiguration CreateRegressionTestConfig()
        {
            var config = CreateMidGameConfig();
            
            // Configure regression testing world setup
            config.WorldSetup.currentSceneName = "BugReproductionScene";
            config.WorldSetup.playerPosition = new Vector3(5.5f, 2.3f, 0f); // Specific position that might trigger bugs
            config.WorldSetup.currentDepth = 7;
            config.WorldSetup.runNumber = 4;
            config.WorldSetup.isActiveRun = true;
            
            // Add specific state that might cause issues
            config.WorldSetup.completedScenes.AddRange(new[]
            {
                "Scene_With_Special_Characters!@#",
                "VeryLongSceneNameThatMightCauseIssuesWithSerialization_12345",
                "" // Empty scene name to test edge cases
            });
            
            return config;
        }
    }
}