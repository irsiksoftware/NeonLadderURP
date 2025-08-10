using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Manages procedural scene loading with save state integration
    /// Handles scene transitions, procedural generation parameters, and state persistence
    /// </summary>
    public class ProceduralSceneLoader : MonoBehaviour
    {
        #region Singleton
        
        private static ProceduralSceneLoader instance;
        public static ProceduralSceneLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ProceduralSceneLoader>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ProceduralSceneLoader");
                        instance = go.AddComponent<ProceduralSceneLoader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Scene Loading Configuration")]
        [SerializeField] private float sceneTransitionDelay = 0.5f;
        [SerializeField] private bool useAsyncLoading = true;
        [SerializeField] private bool maintainProceduralState = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion
        
        #region Private Fields
        
        private PathGenerator pathGenerator;
        private MysticalMap currentMap;
        private GeneratedSceneData currentSceneData;
        private ConsolidatedSaveData currentSaveData;
        private bool isLoadingScene = false;
        
        // Scene loading queue for seamless transitions
        private Queue<SceneLoadRequest> loadQueue = new Queue<SceneLoadRequest>();
        
        #endregion
        
        #region Events
        
        public static event Action<GeneratedSceneData> OnSceneGenerationStarted;
        public static event Action<GeneratedSceneData> OnSceneGenerationCompleted;
        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoadCompleted;
        public static event Action<string> OnSceneLoadError;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Initialize()
        {
            pathGenerator = new PathGenerator();
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveCompleted += HandleSaveCompleted;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
            
            // Subscribe to scene events
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            
            LogDebug("ProceduralSceneLoader initialized");
        }
        
        private void OnDestroy()
        {
            EnhancedSaveSystem.OnSaveCompleted -= HandleSaveCompleted;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
        }
        
        #endregion
        
        #region Public API - Scene Loading
        
        /// <summary>
        /// Load a procedurally generated scene with specific parameters
        /// </summary>
        public void LoadProceduralScene(string seed, int depth, string pathType = "main")
        {
            if (isLoadingScene)
            {
                LogDebug($"Scene load already in progress, queuing request for seed: {seed}, depth: {depth}");
                loadQueue.Enqueue(new SceneLoadRequest { Seed = seed, Depth = depth, PathType = pathType });
                return;
            }
            
            StartCoroutine(LoadProceduralSceneCoroutine(seed, depth, pathType));
        }
        
        /// <summary>
        /// Load a scene from SaveStateConfiguration
        /// </summary>
        public void LoadSceneFromConfiguration(SaveStateConfiguration config)
        {
            if (config == null)
            {
                LogError("Cannot load scene: SaveStateConfiguration is null");
                return;
            }
            
            var saveData = config.CreateSaveData();
            LoadSceneFromSaveData(saveData);
        }
        
        /// <summary>
        /// Load a scene from ConsolidatedSaveData
        /// </summary>
        public void LoadSceneFromSaveData(ConsolidatedSaveData saveData)
        {
            if (saveData == null)
            {
                LogError("Cannot load scene: ConsolidatedSaveData is null");
                return;
            }
            
            currentSaveData = saveData;
            var procState = saveData.worldState.proceduralState;
            
            // Generate or restore the procedural map
            if (!string.IsNullOrEmpty(procState.currentPath))
            {
                RestoreProceduralState(procState);
            }
            else
            {
                // Generate new procedural content
                GenerateNewProceduralContent(procState.currentSeed.ToString(), procState.currentDepth);
            }
            
            // Load the specified scene
            if (!string.IsNullOrEmpty(saveData.worldState.currentSceneName))
            {
                LoadProceduralScene(procState.currentSeed.ToString(), procState.currentDepth, procState.currentPath);
            }
        }
        
        /// <summary>
        /// Generate a new procedural map with specific seed
        /// </summary>
        public MysticalMap GenerateMap(string seed = null, GenerationRules rules = null)
        {
            currentMap = pathGenerator.GenerateMapWithRules(seed, rules);
            
            // Update save data with new map information
            if (currentSaveData != null)
            {
                UpdateSaveDataWithMap(currentMap);
            }
            
            LogDebug($"Generated new map with seed: {currentMap.Seed}, {currentMap.Layers.Count} layers");
            return currentMap;
        }
        
        /// <summary>
        /// Get the current procedural map
        /// </summary>
        public MysticalMap GetCurrentMap()
        {
            return currentMap;
        }
        
        /// <summary>
        /// Get current scene generation data
        /// </summary>
        public GeneratedSceneData GetCurrentSceneData()
        {
            return currentSceneData;
        }
        
        #endregion
        
        #region Scene Loading Implementation
        
        private IEnumerator LoadProceduralSceneCoroutine(string seed, int depth, string pathType)
        {
            isLoadingScene = true;
            
            // Generate scene data
            var sceneData = GenerateSceneData(seed, depth, pathType);
            currentSceneData = sceneData;
            
            OnSceneGenerationStarted?.Invoke(sceneData);
            
            // Apply transition delay
            if (sceneTransitionDelay > 0)
            {
                yield return new WaitForSeconds(sceneTransitionDelay);
            }
            
            // Determine scene name to load
            string sceneName = DetermineSceneName(sceneData);
            
            OnSceneLoadStarted?.Invoke(sceneName);
            LogDebug($"Loading procedural scene: {sceneName} (seed: {seed}, depth: {depth})");
            
            // Load the scene
            if (useAsyncLoading)
            {
                yield return LoadSceneAsync(sceneName, sceneData);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
                ApplySceneData(sceneData);
            }
            
            OnSceneGenerationCompleted?.Invoke(sceneData);
            OnSceneLoadCompleted?.Invoke(sceneName);
            
            isLoadingScene = false;
            
            // Process queued loads
            if (loadQueue.Count > 0)
            {
                var nextRequest = loadQueue.Dequeue();
                LoadProceduralScene(nextRequest.Seed, nextRequest.Depth, nextRequest.PathType);
            }
        }
        
        private IEnumerator LoadSceneAsync(string sceneName, GeneratedSceneData sceneData)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;
            
            // Wait for scene to load
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }
            
            // Apply scene data before activation
            asyncLoad.allowSceneActivation = true;
            
            // Wait for scene activation
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Apply procedural data to the loaded scene
            ApplySceneData(sceneData);
        }
        
        #endregion
        
        #region Scene Data Generation
        
        private GeneratedSceneData GenerateSceneData(string seed, int depth, string pathType)
        {
            var sceneData = new GeneratedSceneData
            {
                sceneId = GenerateSceneId(seed, depth),
                sceneName = DetermineSceneName(seed, depth, pathType),
                playerSpawnPosition = DetermineSpawnPosition(seed, depth),
                depth = depth,
                pathType = pathType,
                isCompleted = false,
                generatedAt = DateTime.Now,
                sceneSpecificData = new Dictionary<string, object>()
            };
            
            // Add procedural generation parameters
            sceneData.sceneSpecificData["seed"] = seed;
            sceneData.sceneSpecificData["generationTime"] = DateTime.Now.ToString();
            sceneData.sceneSpecificData["difficulty"] = CalculateDifficulty(depth);
            
            // Add room type based on path
            string roomType = DetermineRoomType(pathType, depth);
            sceneData.sceneSpecificData["roomType"] = roomType;
            
            // Add enemy configuration
            if (roomType == "combat" || roomType == "elite")
            {
                sceneData.sceneSpecificData["enemyCount"] = DetermineEnemyCount(depth);
                sceneData.sceneSpecificData["enemyTypes"] = DetermineEnemyTypes(depth, seed);
            }
            
            // Add loot configuration
            if (roomType == "treasure" || roomType == "shop")
            {
                sceneData.sceneSpecificData["lootTier"] = DetermineLootTier(depth);
                sceneData.sceneSpecificData["shopItems"] = GenerateShopItems(depth, seed);
            }
            
            return sceneData;
        }
        
        private string GenerateSceneId(string seed, int depth)
        {
            return $"{seed}_{depth}_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
        
        private string DetermineSceneName(GeneratedSceneData sceneData)
        {
            return DetermineSceneName(
                sceneData.sceneSpecificData["seed"].ToString(),
                sceneData.depth,
                sceneData.pathType
            );
        }
        
        private string DetermineSceneName(string seed, int depth, string pathType)
        {
            // Map depth and path type to actual scene names
            // This should correspond to your actual Unity scene names
            
            if (pathType == "boss")
            {
                return GetBossSceneName(depth);
            }
            
            if (pathType == "secret")
            {
                return "SecretRoom";
            }
            
            if (pathType == "shop")
            {
                return "ShopRoom";
            }
            
            // Default procedural rooms based on depth
            string[] proceduralScenes = {
                "ProceduralRoom_1",
                "ProceduralRoom_2", 
                "ProceduralRoom_3",
                "ProceduralRoom_4",
                "ProceduralRoom_5"
            };
            
            // Use seed to deterministically select room
            int roomIndex = Math.Abs(seed.GetHashCode() + depth) % proceduralScenes.Length;
            return proceduralScenes[roomIndex];
        }
        
        private string GetBossSceneName(int depth)
        {
            string[] bossScenes = {
                "BossRoom_Pride",
                "BossRoom_Wrath",
                "BossRoom_Greed",
                "BossRoom_Envy",
                "BossRoom_Lust",
                "BossRoom_Gluttony",
                "BossRoom_Sloth"
            };
            
            return depth < bossScenes.Length ? bossScenes[depth] : "BossRoom_Final";
        }
        
        private Vector3 DetermineSpawnPosition(string seed, int depth)
        {
            // Generate deterministic spawn position based on seed and depth
            var random = new System.Random(seed.GetHashCode() + depth);
            
            float x = (float)(random.NextDouble() * 10 - 5); // -5 to 5
            float y = 0; // Ground level
            float z = 0;
            
            return new Vector3(x, y, z);
        }
        
        private string DetermineRoomType(string pathType, int depth)
        {
            switch (pathType)
            {
                case "boss": return "boss";
                case "secret": return "secret";
                case "shop": return "shop";
                case "branch": return depth % 3 == 0 ? "elite" : "combat";
                default: return "combat";
            }
        }
        
        private int CalculateDifficulty(int depth)
        {
            // Difficulty scales with depth
            return Mathf.Clamp(depth * 2 + 1, 1, 20);
        }
        
        private int DetermineEnemyCount(int depth)
        {
            return Mathf.Min(2 + depth / 2, 8);
        }
        
        private List<string> DetermineEnemyTypes(int depth, string seed)
        {
            var random = new System.Random(seed.GetHashCode() + depth);
            var enemyTypes = new List<string>();
            
            string[] availableEnemies = {
                "Grunt", "Warrior", "Archer", "Mage", "Assassin", "Tank"
            };
            
            int enemyCount = DetermineEnemyCount(depth);
            for (int i = 0; i < enemyCount; i++)
            {
                enemyTypes.Add(availableEnemies[random.Next(availableEnemies.Length)]);
            }
            
            return enemyTypes;
        }
        
        private int DetermineLootTier(int depth)
        {
            return Mathf.Clamp(depth / 3 + 1, 1, 5);
        }
        
        private List<string> GenerateShopItems(int depth, string seed)
        {
            var random = new System.Random(seed.GetHashCode() + depth);
            var items = new List<string>();
            
            string[] shopItems = {
                "HealthPotion", "StaminaPotion", "AttackBoost", 
                "DefenseBoost", "SpeedBoost", "JumpBoost"
            };
            
            int itemCount = random.Next(3, 6);
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(shopItems[random.Next(shopItems.Length)]);
            }
            
            return items;
        }
        
        #endregion
        
        #region Scene Data Application
        
        private void ApplySceneData(GeneratedSceneData sceneData)
        {
            if (sceneData == null) return;
            
            // Find and configure procedural elements in the scene
            ConfigureProceduralElements(sceneData);
            
            // Set player spawn position
            SetPlayerSpawnPosition(sceneData.playerSpawnPosition);
            
            // Apply room-specific configurations
            ApplyRoomConfiguration(sceneData);
            
            // Save the current scene state
            SaveSceneState(sceneData);
            
            LogDebug($"Applied scene data: {sceneData.sceneId} at depth {sceneData.depth}");
        }
        
        private void ConfigureProceduralElements(GeneratedSceneData sceneData)
        {
            // Find all procedural generators in the scene
            var generators = FindObjectsOfType<MonoBehaviour>()
                .Where(mb => mb.GetType().GetInterface("IProceduralGenerator") != null)
                .ToList();
            
            foreach (var generator in generators)
            {
                // Apply seed and configuration
                if (generator is IProceduralGenerator procGen)
                {
                    procGen.Configure(sceneData);
                }
            }
        }
        
        private void SetPlayerSpawnPosition(Vector3 position)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = position;
                LogDebug($"Set player spawn position to: {position}");
            }
        }
        
        private void ApplyRoomConfiguration(GeneratedSceneData sceneData)
        {
            if (!sceneData.sceneSpecificData.ContainsKey("roomType"))
                return;
            
            string roomType = sceneData.sceneSpecificData["roomType"].ToString();
            
            switch (roomType)
            {
                case "combat":
                case "elite":
                    ConfigureCombatRoom(sceneData);
                    break;
                case "treasure":
                case "shop":
                    ConfigureLootRoom(sceneData);
                    break;
                case "boss":
                    ConfigureBossRoom(sceneData);
                    break;
            }
        }
        
        private void ConfigureCombatRoom(GeneratedSceneData sceneData)
        {
            // Spawn enemies based on configuration
            if (sceneData.sceneSpecificData.ContainsKey("enemyTypes"))
            {
                var enemyTypes = sceneData.sceneSpecificData["enemyTypes"] as List<string>;
                // TODO: Implement enemy spawning system
                LogDebug($"Would spawn {enemyTypes?.Count ?? 0} enemies");
            }
        }
        
        private void ConfigureLootRoom(GeneratedSceneData sceneData)
        {
            // Configure loot and shop items
            if (sceneData.sceneSpecificData.ContainsKey("shopItems"))
            {
                var shopItems = sceneData.sceneSpecificData["shopItems"] as List<string>;
                // TODO: Implement shop item spawning
                LogDebug($"Would spawn {shopItems?.Count ?? 0} shop items");
            }
        }
        
        private void ConfigureBossRoom(GeneratedSceneData sceneData)
        {
            // Configure boss encounter
            string bossName = GetBossNameForDepth(sceneData.depth);
            // TODO: Implement boss spawning
            LogDebug($"Would spawn boss: {bossName}");
        }
        
        private string GetBossNameForDepth(int depth)
        {
            string[] bosses = {
                "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth"
            };
            
            return depth < bosses.Length ? bosses[depth] : "Final Boss";
        }
        
        #endregion
        
        #region State Management
        
        private void RestoreProceduralState(ProceduralGenerationState state)
        {
            if (state == null) return;
            
            // Restore the map from saved state
            if (!string.IsNullOrEmpty(state.currentPath))
            {
                currentMap = pathGenerator.GenerateMap(state.currentSeed.ToString());
                LogDebug($"Restored procedural map with seed: {state.currentSeed}");
            }
            
            // Restore scene-specific data
            if (state.generatedScenes != null && state.generatedScenes.Count > 0)
            {
                var lastScene = state.generatedScenes.LastOrDefault(s => !s.isCompleted);
                if (lastScene != null)
                {
                    currentSceneData = lastScene;
                }
            }
        }
        
        private void GenerateNewProceduralContent(string seed, int depth)
        {
            if (string.IsNullOrEmpty(seed))
            {
                seed = GenerateSeed();
            }
            
            currentMap = pathGenerator.GenerateMap(seed);
            LogDebug($"Generated new procedural content with seed: {seed}");
        }
        
        private void SaveSceneState(GeneratedSceneData sceneData)
        {
            if (currentSaveData == null) return;
            
            var procState = currentSaveData.worldState.proceduralState;
            
            // Update procedural state
            procState.currentSeed = int.TryParse(sceneData.sceneSpecificData["seed"]?.ToString(), out int seed) ? seed : 0;
            procState.currentDepth = sceneData.depth;
            procState.currentPath = sceneData.pathType;
            
            // Add to generated scenes history
            if (procState.generatedScenes == null)
            {
                procState.generatedScenes = new List<GeneratedSceneData>();
            }
            
            // Check if scene already exists in history
            var existingScene = procState.generatedScenes.FirstOrDefault(s => s.sceneId == sceneData.sceneId);
            if (existingScene != null)
            {
                // Update existing scene
                procState.generatedScenes.Remove(existingScene);
            }
            
            procState.generatedScenes.Add(sceneData);
            
            // Trigger save
            if (maintainProceduralState)
            {
                EnhancedSaveSystem.Save(currentSaveData);
            }
        }
        
        private void UpdateSaveDataWithMap(MysticalMap map)
        {
            if (currentSaveData == null || map == null) return;
            
            var procState = currentSaveData.worldState.proceduralState;
            procState.currentSeed = int.TryParse(map.Seed, out int mapSeed) ? mapSeed : 0;
            
            // Store path generator state
            procState.pathGeneratorState["mapSeed"] = map.Seed;
            procState.pathGeneratorState["layerCount"] = map.Layers.Count;
            procState.pathGeneratorState["generatedAt"] = DateTime.Now.ToString();
        }
        
        private string GenerateSeed()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogDebug($"Scene loaded: {scene.name}");
            
            // Apply any pending scene data
            if (currentSceneData != null && currentSceneData.sceneName == scene.name)
            {
                ApplySceneData(currentSceneData);
            }
        }
        
        private void HandleSceneUnloaded(Scene scene)
        {
            LogDebug($"Scene unloaded: {scene.name}");
        }
        
        private void HandleSaveCompleted(ConsolidatedSaveData saveData)
        {
            currentSaveData = saveData;
            LogDebug("Save completed, procedural state persisted");
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            currentSaveData = saveData;
            
            // Restore procedural state from save
            if (saveData?.worldState?.proceduralState != null)
            {
                RestoreProceduralState(saveData.worldState.proceduralState);
            }
            
            LogDebug("Save loaded, procedural state restored");
        }
        
        #endregion
        
        #region Utility
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debugger.Log($"[ProceduralSceneLoader] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[ProceduralSceneLoader] {message}");
            OnSceneLoadError?.Invoke(message);
        }
        
        #endregion
        
        #region Helper Classes
        
        [Serializable]
        private class SceneLoadRequest
        {
            public string Seed;
            public int Depth;
            public string PathType;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for procedural generators in scenes
    /// </summary>
    public interface IProceduralGenerator
    {
        void Configure(GeneratedSceneData sceneData);
    }
}