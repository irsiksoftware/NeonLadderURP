using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using NeonLadderURP.Models;
using NeonLadder.Models;
using NeonLadder.Debugging;

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Designer-friendly ScriptableObject for configuring save states and procedural generation scenarios.
    /// Allows drag & drop configuration of save data and predefined scene sets.
    /// </summary>
    [CreateAssetMenu(fileName = "New Save State Config", menuName = "NeonLadder/Save System/Save State Configuration")]
    public class SaveStateConfiguration : ScriptableObject
    {
        [Header("Configuration Info")]
        [SerializeField] private string configurationName = "Default Save State";
        [SerializeField, TextArea(3, 5)] private string description = "Describe this save state configuration...";
        [SerializeField] private bool isTestingConfiguration = false;
        
        [Header("Player Progression Setup")]
        [SerializeField] private PlayerProgressionSetup playerSetup = new PlayerProgressionSetup();
        
        [Header("Currency Configuration")]
        [SerializeField] private CurrencySetup currencySetup = new CurrencySetup();
        
        [Header("Procedural Scene Sets")]
        [SerializeField] private List<ProceduralSceneSet> sceneSetPresets = new List<ProceduralSceneSet>();
        [SerializeField] private ProceduralSceneSet currentSceneSet;
        
        [Header("Item & Unlock Configuration")]
        [SerializeField] private List<UnlockScriptableObject> preUnlockedAbilities = new List<UnlockScriptableObject>();
        [SerializeField] private List<PurchasableItem> prePurchasedItems = new List<PurchasableItem>();
        
        [Header("World State Setup")]
        [SerializeField] private WorldStateSetup worldSetup = new WorldStateSetup();
        
        [Header("Testing & Debug")]
        [SerializeField] private bool enableDebugMode = false;
        [SerializeField] private List<TestingScenario> testingScenarios = new List<TestingScenario>();
        
        /// <summary>
        /// Apply this configuration to create a ConsolidatedSaveData
        /// </summary>
        public ConsolidatedSaveData CreateSaveData()
        {
            var saveData = new ConsolidatedSaveData();
            
            // Apply player progression
            ApplyPlayerProgression(saveData);
            
            // Apply currency setup
            ApplyCurrencySetup(saveData);
            
            // Apply world state
            ApplyWorldState(saveData);
            
            // Apply items and unlocks
            ApplyItemsAndUnlocks(saveData);
            
            // Apply procedural scene configuration
            if (currentSceneSet != null)
            {
                ApplyProceduralSceneSet(saveData);
            }
            
            // Set metadata
            saveData.gameVersion = Application.version;
            saveData.lastSaved = DateTime.Now;
            
            return saveData;
        }
        
        /// <summary>
        /// Load this configuration from existing save data (for editing)
        /// </summary>
        public void LoadFromSaveData(ConsolidatedSaveData saveData)
        {
            if (saveData == null) return;
            
            // Load player progression
            playerSetup.playerLevel = saveData.progression.playerLevel;
            playerSetup.maxHealth = saveData.progression.maxHealth;
            playerSetup.currentHealth = saveData.progression.currentHealth;
            playerSetup.maxStamina = saveData.progression.maxStamina;
            playerSetup.currentStamina = saveData.progression.currentStamina;
            playerSetup.attackDamage = saveData.progression.attackDamage;
            playerSetup.movementSpeed = saveData.progression.movementSpeed;
            playerSetup.jumpCount = saveData.progression.jumpCount;
            
            // Load currency
            currencySetup.startingMetaCurrency = saveData.currencies.metaCurrency;
            currencySetup.startingPermaCurrency = saveData.currencies.permaCurrency;
            
            // Load world state
            worldSetup.currentSceneName = saveData.worldState.currentSceneName;
            worldSetup.playerPosition = saveData.worldState.playerPosition;
            worldSetup.currentDepth = saveData.worldState.proceduralState.currentDepth;
            worldSetup.runNumber = saveData.worldState.currentRun.runNumber;
        }
        
        /// <summary>
        /// Apply this configuration to current game session
        /// </summary>
        public void ApplyToCurrentSession()
        {
            var saveData = CreateSaveData();
            EnhancedSaveSystem.Save(saveData);
            
            if (enableDebugMode)
            {
                Debugger.Log($"[SaveStateConfiguration] Applied configuration '{configurationName}' to current session");
            }
        }
        
        #region Private Application Methods
        
        private void ApplyPlayerProgression(ConsolidatedSaveData saveData)
        {
            saveData.progression.playerLevel = playerSetup.playerLevel;
            saveData.progression.experiencePoints = playerSetup.experiencePoints;
            saveData.progression.maxHealth = playerSetup.maxHealth;
            saveData.progression.currentHealth = playerSetup.currentHealth;
            saveData.progression.maxStamina = playerSetup.maxStamina;
            saveData.progression.currentStamina = playerSetup.currentStamina;
            saveData.progression.attackDamage = playerSetup.attackDamage;
            saveData.progression.attackSpeed = playerSetup.attackSpeed;
            saveData.progression.movementSpeed = playerSetup.movementSpeed;
            saveData.progression.jumpCount = playerSetup.jumpCount;
        }
        
        private void ApplyCurrencySetup(ConsolidatedSaveData saveData)
        {
            saveData.currencies.metaCurrency = currencySetup.startingMetaCurrency;
            saveData.currencies.permaCurrency = currencySetup.startingPermaCurrency;
            saveData.currencies.totalMetaEarned = currencySetup.totalMetaEarned;
            saveData.currencies.totalPermaEarned = currencySetup.totalPermaEarned;
        }
        
        private void ApplyWorldState(ConsolidatedSaveData saveData)
        {
            saveData.worldState.currentSceneName = worldSetup.currentSceneName;
            saveData.worldState.playerPosition = worldSetup.playerPosition;
            saveData.worldState.currentCheckpoint = worldSetup.currentCheckpoint;
            saveData.worldState.proceduralState.currentDepth = worldSetup.currentDepth;
            saveData.worldState.currentRun.runNumber = worldSetup.runNumber;
            saveData.worldState.currentRun.runDepth = worldSetup.currentDepth;
            saveData.worldState.currentRun.isActive = worldSetup.isActiveRun;
            
            // Apply completed content
            saveData.worldState.completedScenes.AddRange(worldSetup.completedScenes);
            saveData.worldState.discoveredAreas.AddRange(worldSetup.discoveredAreas);
            saveData.worldState.defeatedBosses.AddRange(worldSetup.defeatedBosses);
        }
        
        private void ApplyItemsAndUnlocks(ConsolidatedSaveData saveData)
        {
            // Apply pre-unlocked abilities
            foreach (var unlock in preUnlockedAbilities)
            {
                if (unlock != null)
                {
                    var unlockData = new UnlockData
                    {
                        unlockId = unlock.name,
                        unlockName = unlock.unlockName,
                        isUnlocked = true,
                        unlockedAt = DateTime.Now,
                        costPaid = unlock.cost,
                        unlockType = unlock.type.ToString()
                    };
                    saveData.progression.permanentUnlocks.Add(unlockData);
                    saveData.progression.unlockedAbilities.Add(unlock.unlockName);
                }
            }
            
            // Apply pre-purchased items
            foreach (var item in prePurchasedItems)
            {
                if (item != null)
                {
                    var purchaseData = new PurchasedItemData
                    {
                        itemId = item.ItemId,
                        itemName = item.ItemName,
                        timesPurchased = 1,
                        firstPurchased = DateTime.Now,
                        lastPurchased = DateTime.Now,
                        isActive = true,
                        purchaseType = item.CurrencyType.ToString().ToLower()
                    };
                    saveData.inventory.purchasedItems.Add(purchaseData);
                }
            }
        }
        
        private void ApplyProceduralSceneSet(ConsolidatedSaveData saveData)
        {
            saveData.worldState.proceduralState.currentSeed = currentSceneSet.seed;
            saveData.worldState.proceduralState.currentPath = currentSceneSet.pathType;
            
            // Generate scenes based on the preset
            for (int i = 0; i < currentSceneSet.sceneConfigurations.Count; i++)
            {
                var sceneConfig = currentSceneSet.sceneConfigurations[i];
                var generatedScene = new GeneratedSceneData
                {
                    sceneId = $"scene_{currentSceneSet.seed}_{i}",
                    sceneName = sceneConfig.sceneName,
                    playerSpawnPosition = sceneConfig.spawnPosition,
                    depth = sceneConfig.depth,
                    pathType = sceneConfig.pathType,
                    isCompleted = sceneConfig.isPreCompleted,
                    generatedAt = DateTime.Now
                };
                
                // Add scene-specific data
                foreach (var data in sceneConfig.sceneSpecificData)
                {
                    generatedScene.sceneSpecificData[data.key] = data.value;
                }
                
                saveData.worldState.proceduralState.generatedScenes.Add(generatedScene);
            }
        }
        
        #endregion
        
        #region Context Menu Actions
        
        [ContextMenu("Apply to Current Session")]
        private void ApplyToCurrentSessionMenu()
        {
            ApplyToCurrentSession();
        }
        
        [ContextMenu("Load From Current Save")]
        private void LoadFromCurrentSaveMenu()
        {
            var currentSave = EnhancedSaveSystem.Load();
            LoadFromSaveData(currentSave);
        }
        
        [ContextMenu("Create New Scene Set")]
        private void CreateNewSceneSetMenu()
        {
            var newSceneSet = new ProceduralSceneSet
            {
                setName = "New Scene Set",
                description = "Describe this scene set...",
                seed = UnityEngine.Random.Range(1000, 9999)
            };
            sceneSetPresets.Add(newSceneSet);
        }
        
        [ContextMenu("Randomize All Data (Testing)")]
        private void RandomizeAllDataMenu()
        {
            RandomizeAllConfigurationData();
            Debugger.Log("🎲 Save State Configuration randomized for testing!");
        }
        
        // Public method for easier access - can be called from custom inspector
        public void RandomizeAllDataForTesting()
        {
            RandomizeAllConfigurationData();
            Debugger.Log("🎲 Save State Configuration randomized for testing!");
        }
        
        #endregion
        
        #region Public Properties for Testing
        
        /// <summary>
        /// Public access to player setup for testing
        /// </summary>
        public PlayerProgressionSetup PlayerSetup => playerSetup;
        
        /// <summary>
        /// Public access to currency setup for testing
        /// </summary>
        public CurrencySetup CurrencySetup => currencySetup;
        
        /// <summary>
        /// Public access to world setup for testing
        /// </summary>
        public WorldStateSetup WorldSetup => worldSetup;
        
        /// <summary>
        /// Public access to current scene set for testing
        /// </summary>
        public ProceduralSceneSet CurrentSceneSet
        {
            get => currentSceneSet;
            set => currentSceneSet = value;
        }
        
        #endregion
        
        #region Randomization Methods
        
        /// <summary>
        /// Tony Stark's Comprehensive Data Randomizer - Perfect for rapid testing scenarios!
        /// Shuffles all configuration data with realistic but varied values.
        /// </summary>
        private void RandomizeAllConfigurationData()
        {
            // Randomize player progression
            RandomizePlayerProgression();
            
            // Randomize currency values
            RandomizeCurrencySetup();
            
            // Randomize world state
            RandomizeWorldSetup();
            
            // Randomize purchased items
            RandomizePurchasedItems();
            
            // Randomize procedural scene set
            RandomizeCurrentSceneSet();
            
            // Mark as dirty for Unity Editor
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
        
        private void RandomizePlayerProgression()
        {
            // Realistic level progression (1-50)
            playerSetup.playerLevel = UnityEngine.Random.Range(1, 51);
            
            // Health values (50-500, max >= current) - NOTE: These are int types
            playerSetup.maxHealth = UnityEngine.Random.Range(50, 501);
            playerSetup.currentHealth = UnityEngine.Random.Range(10, playerSetup.maxHealth + 1);
            
            // Stamina values (30-200, max >= current)  
            playerSetup.maxStamina = UnityEngine.Random.Range(30f, 200f);
            playerSetup.currentStamina = UnityEngine.Random.Range(5f, playerSetup.maxStamina);
            
            // Experience points (0-10000) - NOTE: It's experiencePoints not currentExperience
            playerSetup.experiencePoints = UnityEngine.Random.Range(0f, 10000f);
            
            Debugger.Log($"🎲 Randomized Player: Lvl {playerSetup.playerLevel}, HP {playerSetup.currentHealth}/{playerSetup.maxHealth}");
        }
        
        private void RandomizeCurrencySetup()
        {
            // Meta currency (temporary, 0-5000) - NOTE: Correct property names
            currencySetup.startingMetaCurrency = UnityEngine.Random.Range(0, 5001);
            
            // Perma currency (persistent, 0-50000)
            currencySetup.startingPermaCurrency = UnityEngine.Random.Range(0, 50001);
            
            // Lifetime earned (always >= current perma)
            currencySetup.totalPermaEarned = currencySetup.startingPermaCurrency + UnityEngine.Random.Range(0, 100000);
            
            Debugger.Log($"🎲 Randomized Currency: Meta {currencySetup.startingMetaCurrency}, Perma {currencySetup.startingPermaCurrency}");
        }
        
        private void RandomizeWorldSetup()
        {
            // Random scene names for testing
            string[] testScenes = { "TestLevel1", "TestLevel2", "TestLevel3", "BossArena", "TreasureRoom", "SecretArea" };
            worldSetup.currentSceneName = testScenes[UnityEngine.Random.Range(0, testScenes.Length)];
            
            // Random player position (-50 to 50 in X/Y, 0 in Z for 2.5D)
            worldSetup.playerPosition = new Vector3(
                UnityEngine.Random.Range(-50f, 50f),
                UnityEngine.Random.Range(-20f, 20f),
                0f
            );
            
            // Random depth and run number (1-20)
            worldSetup.currentDepth = UnityEngine.Random.Range(1, 21);
            worldSetup.runNumber = UnityEngine.Random.Range(1, 21);
            
            // Randomize completed content lists
            RandomizeCompletedContent();
            
            Debugger.Log($"🎲 Randomized World: {worldSetup.currentSceneName}, Depth {worldSetup.currentDepth}, Run {worldSetup.runNumber}");
        }
        
        private void RandomizeCompletedContent()
        {
            // Clear existing lists
            worldSetup.completedScenes.Clear();
            worldSetup.discoveredAreas.Clear();
            worldSetup.defeatedBosses.Clear();
            
            // Add random completed scenes (2-6 scenes)
            string[] scenePool = { "Level1", "Level2", "Level3", "BossRoom1", "TreasureVault", "SecondLevel", "ThirdLevel", "HiddenArea" };
            int sceneCount = UnityEngine.Random.Range(2, 7);
            for (int i = 0; i < sceneCount; i++)
            {
                string scene = scenePool[UnityEngine.Random.Range(0, scenePool.Length)];
                if (!worldSetup.completedScenes.Contains(scene))
                {
                    worldSetup.completedScenes.Add(scene);
                }
            }
            
            // Add random discovered areas (1-4 areas)
            string[] areaPool = { "DeepCaves", "AncientRuins", "CrystalChamber", "LavaFlows", "IceGrove", "ShadowRealm" };
            int areaCount = UnityEngine.Random.Range(1, 5);
            for (int i = 0; i < areaCount; i++)
            {
                string area = areaPool[UnityEngine.Random.Range(0, areaPool.Length)];
                if (!worldSetup.discoveredAreas.Contains(area))
                {
                    worldSetup.discoveredAreas.Add(area);
                }
            }
            
            // Add random defeated bosses (0-3 bosses)
            string[] bossPool = { "FireDragon", "IceGolem", "ShadowLord", "CrystalSpider", "VoidKnight", "StormTitan" };
            int bossCount = UnityEngine.Random.Range(0, 4);
            for (int i = 0; i < bossCount; i++)
            {
                string boss = bossPool[UnityEngine.Random.Range(0, bossPool.Length)];
                if (!worldSetup.defeatedBosses.Contains(boss))
                {
                    worldSetup.defeatedBosses.Add(boss);
                }
            }
            
            Debugger.Log($"🎲 Populated Lists: {worldSetup.completedScenes.Count} scenes, {worldSetup.discoveredAreas.Count} areas, {worldSetup.defeatedBosses.Count} bosses");
        }
        
        private void RandomizePurchasedItems()
        {
            // Clear existing items - NOTE: Using correct field name prePurchasedItems
            prePurchasedItems.Clear();
            preUnlockedAbilities.Clear();
            
            // Try to find existing assets in the project
            #if UNITY_EDITOR
            RandomizeScriptableObjectLists();
            #endif
            
            Debugger.Log($"🎲 Randomized Items: Cleared asset lists (ready for manual assignment or auto-population)");
        }
        
        #if UNITY_EDITOR
        private void RandomizeScriptableObjectLists()
        {
            // Find PurchasableItem assets
            string[] purchasableGuids = UnityEditor.AssetDatabase.FindAssets("t:PurchasableItem");
            foreach (string guid in purchasableGuids.Take(UnityEngine.Random.Range(2, 6)))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var item = UnityEditor.AssetDatabase.LoadAssetAtPath<PurchasableItem>(path);
                if (item != null)
                {
                    prePurchasedItems.Add(item);
                }
            }
            
            // Find UnlockScriptableObject assets
            string[] unlockGuids = UnityEditor.AssetDatabase.FindAssets("t:UnlockScriptableObject");
            foreach (string guid in unlockGuids.Take(UnityEngine.Random.Range(1, 4)))
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var unlock = UnityEditor.AssetDatabase.LoadAssetAtPath<UnlockScriptableObject>(path);
                if (unlock != null)
                {
                    preUnlockedAbilities.Add(unlock);
                }
            }
            
            Debugger.Log($"🎲 Found and added {prePurchasedItems.Count} PurchasableItems and {preUnlockedAbilities.Count} Unlocks");
        }
        #endif
        
        private void RandomizeCurrentSceneSet()
        {
            if (currentSceneSet == null)
            {
                currentSceneSet = new ProceduralSceneSet();
            }
            
            // Random seed for procedural generation
            currentSceneSet.seed = UnityEngine.Random.Range(1000, 99999);
            
            // Random path type
            string[] pathTypes = { "linear", "branching", "circular", "maze", "boss_rush" };
            currentSceneSet.pathType = pathTypes[UnityEngine.Random.Range(0, pathTypes.Length)];
            
            // Random set name
            string[] adjectives = { "Chaotic", "Mysterious", "Dangerous", "Epic", "Twisted", "Ancient" };
            string[] nouns = { "Depths", "Labyrinth", "Stronghold", "Catacombs", "Sanctum", "Ruins" };
            currentSceneSet.setName = $"{adjectives[UnityEngine.Random.Range(0, adjectives.Length)]} {nouns[UnityEngine.Random.Range(0, nouns.Length)]}";
            
            // Generate 2-6 random scene configurations
            currentSceneSet.sceneConfigurations.Clear();
            int sceneCount = UnityEngine.Random.Range(2, 7);
            
            for (int i = 0; i < sceneCount; i++)
            {
                var sceneConfig = new SceneConfiguration
                {
                    sceneName = $"ProcScene_{i + 1}",
                    depth = i + 1,
                    pathType = i == sceneCount - 1 ? "boss" : "main",
                    spawnPosition = new Vector3(UnityEngine.Random.Range(-10f, 10f), 0f, 0f),
                    isPreCompleted = UnityEngine.Random.value < 0.3f // 30% chance pre-completed
                };
                
                currentSceneSet.sceneConfigurations.Add(sceneConfig);
            }
            
            Debugger.Log($"🎲 Randomized Scene Set: '{currentSceneSet.setName}' with {sceneCount} scenes, seed {currentSceneSet.seed}");
        }
        
        #endregion
    }
    
    #region Configuration Data Structures
    
    [Serializable]
    public class PlayerProgressionSetup
    {
        [Header("Level & Experience")]
        public int playerLevel = 1;
        public float experiencePoints = 0f;
        
        [Header("Health & Stamina")]
        public int maxHealth = 100;
        public int currentHealth = 100;
        public float maxStamina = 100f;
        public float currentStamina = 100f;
        
        [Header("Combat Stats")]
        public float attackDamage = 10f;
        public float attackSpeed = 1f;
        
        [Header("Movement")]
        public float movementSpeed = 5f;
        public int jumpCount = 1;
    }
    
    [Serializable]
    public class CurrencySetup
    {
        [Header("Starting Currency")]
        public int startingMetaCurrency = 0;
        public int startingPermaCurrency = 0;
        
        [Header("Lifetime Totals")]
        public int totalMetaEarned = 0;
        public int totalPermaEarned = 0;
    }
    
    [Serializable]
    public class WorldStateSetup
    {
        [Header("Current Location")]
        public string currentSceneName = "SampleScene";
        public Vector3 playerPosition = Vector3.zero;
        public string currentCheckpoint = "";
        
        [Header("Run Information")]
        public int currentDepth = 0;
        public int runNumber = 1;
        public bool isActiveRun = true;
        
        [Header("Completed Content")]
        public List<string> completedScenes = new List<string>();
        public List<string> discoveredAreas = new List<string>();
        public List<string> defeatedBosses = new List<string>();
    }
    
    [Serializable]
    public class ProceduralSceneSet
    {
        [Header("Scene Set Info")]
        public string setName = "Scene Set";
        [TextArea(2, 3)]
        public string description = "Describe this scene configuration...";
        
        [Header("Generation Parameters")]
        public int seed = 1234;
        public string pathType = "main";
        public int maxDepth = 10;
        
        [Header("Scene Configurations")]
        public List<SceneConfiguration> sceneConfigurations = new List<SceneConfiguration>();
        
        [Header("Boss Configuration")]
        public List<string> availableBossScenes = new List<string>();
        public List<string> guaranteedBossSpawns = new List<string>();
    }
    
    [Serializable]
    public class SceneConfiguration
    {
        [Header("Scene Identity")]
        public string sceneName;
        public int depth;
        public string pathType = "main";
        
        [Header("Player Setup")]
        public Vector3 spawnPosition = Vector3.zero;
        public bool isPreCompleted = false;
        
        [Header("Scene-Specific Data")]
        public List<KeyValueData> sceneSpecificData = new List<KeyValueData>();
    }
    
    [Serializable]
    public class KeyValueData
    {
        public string key;
        public string value;
    }
    
    [Serializable]
    public class TestingScenario
    {
        public string scenarioName;
        [TextArea(2, 3)]
        public string description;
        public ProceduralSceneSet sceneSet;
        public PlayerProgressionSetup playerSetup;
        public CurrencySetup currencySetup;
    }
    
    #endregion
}