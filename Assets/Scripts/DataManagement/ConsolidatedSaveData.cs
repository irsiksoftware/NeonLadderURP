using System;
using System.Collections.Generic;
using UnityEngine;
using NeonLadderURP.Models;

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Comprehensive save data structure containing all progression, scene, and player information.
    /// Easily accessible JSON format for external manipulation and debugging.
    /// </summary>
    [Serializable]
    public class ConsolidatedSaveData
    {
        [Header("Save Meta Information")]
        public string saveVersion = "2.0";
        public string gameVersion = "1.0.0";
        public DateTime lastSaved = DateTime.Now;
        public string playerId = System.Guid.NewGuid().ToString();
        public float totalPlayTime = 0f;
        
        [Header("Player Progression")]
        public PlayerProgressionData progression = new PlayerProgressionData();
        
        [Header("Scene & World State")]
        public WorldStateData worldState = new WorldStateData();
        
        [Header("Currency Systems")]
        public CurrencyData currencies = new CurrencyData();
        
        [Header("Items & Purchases")]
        public InventoryData inventory = new InventoryData();
        
        [Header("Settings & Preferences")]
        public PlayerSettings settings = new PlayerSettings();
        
        [Header("Statistics & Analytics")]
        public GameStatistics statistics = new GameStatistics();
        
        [Header("Dialogue System")]
        public DialogueSaveData dialogueData = new DialogueSaveData();
        
        public void UpdateLastSaved()
        {
            lastSaved = DateTime.Now;
        }
        
        public void IncrementPlayTime(float deltaTime)
        {
            totalPlayTime += deltaTime;
        }
    }
    
    [Serializable]
    public class PlayerProgressionData
    {
        public int playerLevel = 1;
        public float experiencePoints = 0f;
        public float experienceToNextLevel = 100f;
        
        // Health & Stamina progression
        public int maxHealth = 100;
        public int currentHealth = 100;
        public float maxStamina = 100f;
        public float currentStamina = 100f;
        
        // Abilities and unlocks
        public List<string> unlockedAbilities = new List<string>();
        public List<PurchasedItemData> purchasedItems = new List<PurchasedItemData>();
        public List<UnlockData> permanentUnlocks = new List<UnlockData>();
        
        // Combat progression
        public float attackDamage = 10f;
        public float attackSpeed = 1f;
        public float movementSpeed = 5f;
        public int jumpCount = 1; // Number of jumps available (1 = single, 2 = double, etc.)
    }
    
    [Serializable]
    public class WorldStateData
    {
        // Current scene and position
        public string currentSceneName = "SampleScene";
        public Vector3 playerPosition = Vector3.zero;
        public string currentCheckpoint = "";
        
        // Procedural generation state
        public ProceduralGenerationState proceduralState = new ProceduralGenerationState();
        
        // Completed content tracking
        public List<string> completedScenes = new List<string>();
        public List<string> discoveredAreas = new List<string>();
        public List<string> defeatedBosses = new List<string>();
        
        // Current run information (roguelite)
        public RunData currentRun = new RunData();
    }
    
    [Serializable]
    public class ProceduralGenerationState
    {
        public int currentSeed = 0;
        public int currentDepth = 0;
        public string currentPath = "";
        public List<GeneratedSceneData> generatedScenes = new List<GeneratedSceneData>();
        
        // Boss and special room tracking
        public List<string> availableBossRooms = new List<string>();
        public List<string> usedBossRooms = new List<string>();
        public Dictionary<string, object> pathGeneratorState = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class GeneratedSceneData
    {
        public string sceneId;
        public string sceneName;
        public Vector3 playerSpawnPosition;
        public int depth;
        public string pathType; // "main", "branch", "boss", "secret"
        public bool isCompleted;
        public DateTime generatedAt;
        public Dictionary<string, object> sceneSpecificData = new Dictionary<string, object>();
    }
    
    [Serializable]
    public class RunData
    {
        public int runNumber = 1;
        public DateTime runStartTime = DateTime.Now;
        public int runDepth = 0;
        public int enemiesKilled = 0;
        public float runTime = 0f;
        public bool isActive = true;
        
        // Run-specific progression
        public Dictionary<string, int> runStats = new Dictionary<string, int>();
        public List<string> runSpecificUpgrades = new List<string>();
    }
    
    [Serializable]
    public class CurrencyData
    {
        // Dual currency system
        public int metaCurrency = 0;        // Per-run temporary currency
        public int permaCurrency = 0;       // Persistent across runs
        
        // Currency tracking
        public int totalMetaEarned = 0;
        public int totalPermaEarned = 0;
        public int totalMetaSpent = 0;
        public int totalPermaSpent = 0;
        
        public void EarnMeta(int amount)
        {
            metaCurrency += amount;
            totalMetaEarned += amount;
        }
        
        public void EarnPerma(int amount)
        {
            permaCurrency += amount;
            totalPermaEarned += amount;
        }
        
        public bool SpendMeta(int amount)
        {
            if (metaCurrency >= amount)
            {
                metaCurrency -= amount;
                totalMetaSpent += amount;
                return true;
            }
            return false;
        }
        
        public bool SpendPerma(int amount)
        {
            if (permaCurrency >= amount)
            {
                permaCurrency -= amount;
                totalPermaSpent += amount;
                return true;
            }
            return false;
        }
    }
    
    [Serializable]
    public class InventoryData
    {
        public List<PurchasedItemData> purchasedItems = new List<PurchasedItemData>();
        public List<string> consumableItems = new List<string>();
        public Dictionary<string, int> itemQuantities = new Dictionary<string, int>();
        
        // Shop state
        public List<string> availableInMetaShop = new List<string>();
        public List<string> availableInPermaShop = new List<string>();
        public DateTime lastShopRefresh = DateTime.Now;
    }
    
    [Serializable]
    public class PurchasedItemData
    {
        public string itemId;
        public string itemName;
        public int timesPurchased;
        public DateTime firstPurchased;
        public DateTime lastPurchased;
        public bool isActive;
        public string purchaseType; // "meta" or "perma"
    }
    
    [Serializable]
    public class UnlockData
    {
        public string unlockId;
        public string unlockName;
        public bool isUnlocked;
        public DateTime unlockedAt;
        public int costPaid;
        public string unlockType; // "ability", "upgrade", "content"
    }
    
    [Serializable]
    public class GameStatistics
    {
        // General stats
        public int totalDeaths = 0;
        public int totalRunsCompleted = 0;
        public int totalEnemiesKilled = 0;
        public float totalDistanceTraveled = 0f;
        public int totalJumps = 0;
        public int totalAttacks = 0;
        
        // Best records
        public float bestRunTime = float.MaxValue;
        public int deepestDepthReached = 0;
        public int highestMetaCurrencyEarned = 0;
        public int longestSurvivalTime = 0;
        
        // Achievement tracking
        public List<string> unlockedAchievements = new List<string>();
        public Dictionary<string, float> achievementProgress = new Dictionary<string, float>();
        
        // Session data
        public int currentSessionDeaths = 0;
        public DateTime sessionStartTime = DateTime.Now;
    }
}