using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Scene management system for the Mystical PathGenerator
    /// Maps procedural nodes to actual Unity scenes
    /// By Stephen Strange - "Every node leads to its destined reality"
    /// </summary>
    
    /// <summary>
    /// All available scenes in the mystical realm
    /// </summary>
    public enum GameScene
    {
        // === NAVIGATION SCENES ===
        MainMenu,
        MapSelection,
        GameOver,
        Victory,
        
        // === COMBAT SCENES ===
        MinorEnemyEncounter,
        MajorEnemyEncounter, 
        EliteEncounter,
        
        // === BOSS SCENES ===
        PrideBossArena,        // Grand Cathedral of Hubris
        WrathBossArena,        // The Necropolis of Vengeance
        GreedBossArena,        // The Vault of Avarice
        EnvyBossArena,         // The Community Center
        LustBossArena,         // Infinite Forest (Eden of Desires)
        GluttonyBossArena,     // The Feast of Infinity
        SlothBossArena,        // The Lethargy Lounge
        
        // === SERVICE SCENES ===
        RestArea,              // Rest and recover
        ShopArea,              // Purchase upgrades
        RestShopCombined,      // Both rest and shop
        
        // === EVENT SCENES ===
        TreasureChestEvent,    // Loot discovery
        MysteriousAltarEvent,  // Choice-based altar
        RiddleStatueEvent,     // Puzzle challenge
        MerchantEncounter,     // Traveling merchant
        ShrineBlessingEvent,   // Blessing shrine
        CursedEncounterEvent,  // Cursed encounter
        DimensionalPortal,     // Portal travel
        
        // === FUTURE EXTENSIONS ===
        TreasureVault,         // Treasure rooms
        PuzzleRoom,            // Puzzle challenges
        BossRushArena,         // Multiple boss fights
        SecretArea,            // Hidden areas
        
        // === TRANSITION SCENES ===
        LoadingTransition,     // Loading between areas
        StoryNarration        // Story sequences
    }

    /// <summary>
    /// Scene configuration data for procedural setup
    /// </summary>
    [Serializable]
    public struct SceneConfig
    {
        public GameScene Scene;
        public string SceneName;           // Unity scene file name
        public string DisplayName;         // UI display name
        public string Description;         // Scene description
        public float EstimatedDuration;    // Expected play time (minutes)
        public bool RequiresSpecialSetup;  // Needs custom configuration
        
        public SceneConfig(GameScene scene, string sceneName, string displayName, 
                          string description, float duration = 5f, bool specialSetup = false)
        {
            Scene = scene;
            SceneName = sceneName;
            DisplayName = displayName;
            Description = description;
            EstimatedDuration = duration;
            RequiresSpecialSetup = specialSetup;
        }
    }

    /// <summary>
    /// Maps procedural nodes to Unity scenes with full configuration
    /// </summary>
    public static class SceneMapper
    {
        /// <summary>
        /// Complete scene configuration database
        /// </summary>
        private static readonly Dictionary<GameScene, SceneConfig> SceneConfigs = new()
        {
            // Navigation
            { GameScene.MainMenu, new(GameScene.MainMenu, "MainMenu", "Main Menu", "Game main menu", 0f) },
            { GameScene.MapSelection, new(GameScene.MapSelection, "MapSelection", "Map Selection", "Choose your path", 1f) },
            { GameScene.GameOver, new(GameScene.GameOver, "GameOver", "Game Over", "Defeat screen", 1f) },
            { GameScene.Victory, new(GameScene.Victory, "Victory", "Victory", "Victory celebration", 2f) },
            
            // Combat
            { GameScene.MinorEnemyEncounter, new(GameScene.MinorEnemyEncounter, "Combat_Minor", "Minor Encounter", "Fight smaller enemies", 3f, true) },
            { GameScene.MajorEnemyEncounter, new(GameScene.MajorEnemyEncounter, "Combat_Major", "Major Encounter", "Fight powerful enemies", 5f, true) },
            { GameScene.EliteEncounter, new(GameScene.EliteEncounter, "Combat_Elite", "Elite Encounter", "Face elite enemies", 7f, true) },
            
            // Bosses
            { GameScene.PrideBossArena, new(GameScene.PrideBossArena, "Boss_Pride", "Cathedral of Hubris", "Face the Sin of Pride", 10f, true) },
            { GameScene.WrathBossArena, new(GameScene.WrathBossArena, "Boss_Wrath", "Necropolis of Vengeance", "Face the Sin of Wrath", 10f, true) },
            { GameScene.GreedBossArena, new(GameScene.GreedBossArena, "Boss_Greed", "Vault of Avarice", "Face the Sin of Greed", 10f, true) },
            { GameScene.EnvyBossArena, new(GameScene.EnvyBossArena, "Boss_Envy", "Community Center", "Face the Sin of Envy", 10f, true) },
            { GameScene.LustBossArena, new(GameScene.LustBossArena, "Boss_Lust", "Eden of Desires", "Face the Sin of Lust", 10f, true) },
            { GameScene.GluttonyBossArena, new(GameScene.GluttonyBossArena, "Boss_Gluttony", "Feast of Infinity", "Face the Sin of Gluttony", 10f, true) },
            { GameScene.SlothBossArena, new(GameScene.SlothBossArena, "Boss_Sloth", "Lethargy Lounge", "Face the Sin of Sloth", 15f, true) },
            
            // Services
            { GameScene.RestArea, new(GameScene.RestArea, "RestArea", "Rest Stop", "Recover health and stamina", 2f, true) },
            { GameScene.ShopArea, new(GameScene.ShopArea, "ShopArea", "Merchant Shop", "Purchase upgrades", 3f, true) },
            { GameScene.RestShopCombined, new(GameScene.RestShopCombined, "RestShop", "Rest & Shop", "Rest and shop combined", 4f, true) },
            
            // Events
            { GameScene.TreasureChestEvent, new(GameScene.TreasureChestEvent, "Event_Treasure", "Treasure Chest", "Discover hidden loot", 2f, true) },
            { GameScene.MysteriousAltarEvent, new(GameScene.MysteriousAltarEvent, "Event_Altar", "Mysterious Altar", "Make a fateful choice", 3f, true) },
            { GameScene.RiddleStatueEvent, new(GameScene.RiddleStatueEvent, "Event_Riddle", "Riddle Statue", "Solve an ancient riddle", 4f, true) },
            { GameScene.MerchantEncounter, new(GameScene.MerchantEncounter, "Event_Merchant", "Traveling Merchant", "Meet a wandering trader", 3f, true) },
            { GameScene.ShrineBlessingEvent, new(GameScene.ShrineBlessingEvent, "Event_Shrine", "Blessing Shrine", "Receive a divine blessing", 2f, true) },
            { GameScene.CursedEncounterEvent, new(GameScene.CursedEncounterEvent, "Event_Curse", "Cursed Encounter", "Face a cursed challenge", 4f, true) },
            { GameScene.DimensionalPortal, new(GameScene.DimensionalPortal, "Event_Portal", "Dimensional Portal", "Travel through dimensions", 3f, true) },
            
            // Future Extensions
            { GameScene.TreasureVault, new(GameScene.TreasureVault, "TreasureVault", "Treasure Vault", "Vast treasure chamber", 5f, true) },
            { GameScene.PuzzleRoom, new(GameScene.PuzzleRoom, "PuzzleRoom", "Puzzle Chamber", "Solve complex puzzles", 6f, true) },
            { GameScene.BossRushArena, new(GameScene.BossRushArena, "BossRush", "Boss Rush Arena", "Face multiple bosses", 20f, true) },
            { GameScene.SecretArea, new(GameScene.SecretArea, "SecretArea", "Secret Area", "Hidden mystical area", 8f, true) },
            
            // Transitions
            { GameScene.LoadingTransition, new(GameScene.LoadingTransition, "Loading", "Loading", "Transitioning between areas", 0.5f) },
            { GameScene.StoryNarration, new(GameScene.StoryNarration, "Story", "Story", "Narrative sequence", 2f) }
        };

        /// <summary>
        /// Maps a procedural node to its corresponding Unity scene
        /// </summary>
        public static GameScene MapNodeToScene(MapNode node)
        {
            return node.Type switch
            {
                NodeType.Encounter => MapEncounterToScene(node),
                NodeType.Event => MapEventToScene(node),
                NodeType.RestShop => GameScene.RestShopCombined,
                NodeType.Boss => MapBossToScene(node),
                NodeType.Treasure => GameScene.TreasureVault,
                NodeType.Elite => GameScene.EliteEncounter,
                NodeType.Mystery => GameScene.SecretArea,
                _ => GameScene.LoadingTransition
            };
        }

        /// <summary>
        /// Maps encounter nodes to combat scenes
        /// </summary>
        private static GameScene MapEncounterToScene(MapNode node)
        {
            if (!node.Properties.ContainsKey("EncounterType"))
                return GameScene.MinorEnemyEncounter;

            var encounterType = (EncounterType)node.Properties["EncounterType"];
            return encounterType switch
            {
                EncounterType.MinorEnemy => GameScene.MinorEnemyEncounter,
                EncounterType.MajorEnemy => GameScene.MajorEnemyEncounter,
                EncounterType.Elite => GameScene.EliteEncounter,
                _ => GameScene.MinorEnemyEncounter
            };
        }

        /// <summary>
        /// Maps event nodes to event scenes
        /// </summary>
        private static GameScene MapEventToScene(MapNode node)
        {
            if (!node.Properties.ContainsKey("EventType"))
                return GameScene.TreasureChestEvent;

            var eventType = (EventType)node.Properties["EventType"];
            return eventType switch
            {
                EventType.TreasureChest => GameScene.TreasureChestEvent,
                EventType.MysteriousAltar => GameScene.MysteriousAltarEvent,
                EventType.Riddle => GameScene.RiddleStatueEvent,
                EventType.Merchant => GameScene.MerchantEncounter,
                EventType.Shrine => GameScene.ShrineBlessingEvent,
                EventType.Curse => GameScene.CursedEncounterEvent,
                EventType.Portal => GameScene.DimensionalPortal,
                _ => GameScene.TreasureChestEvent
            };
        }

        /// <summary>
        /// Maps boss nodes to boss arena scenes
        /// </summary>
        private static GameScene MapBossToScene(MapNode node)
        {
            if (!node.Properties.ContainsKey("BossName"))
                return GameScene.PrideBossArena;

            var bossName = (string)node.Properties["BossName"];
            return bossName.ToLower() switch
            {
                "pride" => GameScene.PrideBossArena,
                "wrath" => GameScene.WrathBossArena,
                "greed" => GameScene.GreedBossArena,
                "envy" => GameScene.EnvyBossArena,
                "lust" => GameScene.LustBossArena,
                "gluttony" => GameScene.GluttonyBossArena,
                "sloth" => GameScene.SlothBossArena,
                _ => GameScene.PrideBossArena
            };
        }

        /// <summary>
        /// Gets scene configuration data
        /// </summary>
        public static SceneConfig GetSceneConfig(GameScene scene)
        {
            return SceneConfigs.TryGetValue(scene, out var config) 
                ? config 
                : new SceneConfig(scene, scene.ToString(), scene.ToString(), "Unknown scene");
        }

        /// <summary>
        /// Gets Unity scene name for loading
        /// </summary>
        public static string GetSceneName(GameScene scene)
        {
            return GetSceneConfig(scene).SceneName;
        }

        /// <summary>
        /// Gets display name for UI
        /// </summary>
        public static string GetDisplayName(GameScene scene)
        {
            return GetSceneConfig(scene).DisplayName;
        }

        /// <summary>
        /// Checks if scene requires special configuration
        /// </summary>
        public static bool RequiresSpecialSetup(GameScene scene)
        {
            return GetSceneConfig(scene).RequiresSpecialSetup;
        }

        /// <summary>
        /// Gets all scenes of a specific type
        /// </summary>
        public static List<GameScene> GetScenesByType(NodeType nodeType)
        {
            var scenes = new List<GameScene>();
            
            foreach (var config in SceneConfigs)
            {
                // Create a dummy node to test mapping
                var testNode = new MapNode { Type = nodeType };
                
                // Add type-specific properties for proper mapping
                switch (nodeType)
                {
                    case NodeType.Boss:
                        testNode.Properties["BossName"] = "Pride"; // Test with Pride boss
                        break;
                    case NodeType.Encounter:
                        testNode.Properties["EncounterType"] = EncounterType.MinorEnemy;
                        break;
                    case NodeType.Event:
                        testNode.Properties["EventType"] = EventType.TreasureChest;
                        break;
                }
                
                var mappedScene = MapNodeToScene(testNode);
                if (mappedScene == config.Key)
                {
                    scenes.Add(config.Key);
                }
            }
            
            return scenes;
        }
    }
}