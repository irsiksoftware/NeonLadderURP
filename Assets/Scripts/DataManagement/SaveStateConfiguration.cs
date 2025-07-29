using UnityEngine;
using System.Collections.Generic;
using System;
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