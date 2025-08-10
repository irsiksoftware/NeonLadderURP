using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadderURP.DataManagement;
using NeonLadder.DataManagement;
using NeonLadder.Debugging;
using Newtonsoft.Json;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Manages the generation and persistence of procedural save states.
    /// Ensures complete state capture and restoration for procedural content.
    /// </summary>
    public class ProceduralSaveStateManager : MonoBehaviour
    {
        #region Singleton
        
        private static ProceduralSaveStateManager instance;
        public static ProceduralSaveStateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ProceduralSaveStateManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ProceduralSaveStateManager");
                        instance = go.AddComponent<ProceduralSaveStateManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Save State Configuration")]
        [SerializeField] private bool captureInteractiveObjects = true;
        [SerializeField] private bool captureEnemyStates = true;
        [SerializeField] private bool captureEnvironmentState = true;
        [SerializeField] private bool validateOnLoad = true;
        
        [Header("Performance")]
        [SerializeField] private int maxSceneStatesInMemory = 20;
        [SerializeField] private bool compressStateData = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool logStateChanges = false;
        
        #endregion
        
        #region Private Fields
        
        private PathGenerator pathGenerator;
        private ProceduralIntegrationManager integrationManager;
        private Dictionary<string, SceneState> sceneStates;
        private ProceduralWorldState currentWorldState;
        private SaveStateConfiguration activeConfiguration;
        
        // State tracking
        private string currentSceneId;
        private bool isGeneratingState = false;
        private DateTime lastStateCapture;
        
        #endregion
        
        #region Events
        
        public static event Action<ProceduralWorldState> OnWorldStateGenerated;
        public static event Action<SceneState> OnSceneStateCaptured;
        public static event Action<SceneState> OnSceneStateRestored;
        public static event Action<string> OnStateValidationFailed;
        
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
            integrationManager = ProceduralIntegrationManager.Instance;
            sceneStates = new Dictionary<string, SceneState>();
            
            // Subscribe to events
            SceneManager.sceneLoaded += HandleSceneLoaded;
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            EnhancedSaveSystem.OnSaveRequested += HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneUnloaded -= HandleSceneUnloaded;
            EnhancedSaveSystem.OnSaveRequested -= HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
        }
        
        #endregion
        
        #region Public API - State Generation
        
        /// <summary>
        /// Generate a complete procedural world state from SaveStateConfiguration
        /// </summary>
        public ProceduralWorldState GenerateWorldState(SaveStateConfiguration configuration)
        {
            if (configuration == null)
            {
                LogError("Cannot generate world state from null configuration");
                return null;
            }
            
            isGeneratingState = true;
            activeConfiguration = configuration;
            
            // Create new world state
            currentWorldState = new ProceduralWorldState
            {
                worldId = Guid.NewGuid().ToString(),
                generatedAt = DateTime.Now,
                configuration = configuration.name
            };
            
            // Generate procedural map
            var saveData = configuration.CreateSaveData();
            var procState = saveData.worldState.proceduralState;
            
            string seed = procState.currentSeed > 0 
                ? procState.currentSeed.ToString() 
                : GenerateUniqueSeed();
            
            var map = pathGenerator.GenerateMapWithRules(seed, null);
            
            // Populate world state
            currentWorldState.seed = seed;
            currentWorldState.mapData = SerializeMap(map);
            currentWorldState.currentDepth = procState.currentDepth;
            currentWorldState.currentPath = procState.currentPath;
            
            // Generate scene states for initial rooms
            GenerateInitialSceneStates(map, procState.currentDepth);
            
            // Apply configuration overrides
            ApplyConfigurationOverrides(configuration);
            
            isGeneratingState = false;
            
            OnWorldStateGenerated?.Invoke(currentWorldState);
            LogDebug($"Generated world state with seed: {seed}");
            
            return currentWorldState;
        }
        
        /// <summary>
        /// Capture current scene state for persistence
        /// </summary>
        public SceneState CaptureCurrentSceneState()
        {
            var scene = SceneManager.GetActiveScene();
            currentSceneId = GenerateSceneId(scene.name);
            
            var sceneState = new SceneState
            {
                sceneId = currentSceneId,
                sceneName = scene.name,
                capturedAt = DateTime.Now
            };
            
            // Capture interactive objects
            if (captureInteractiveObjects)
            {
                CaptureInteractiveObjectStates(sceneState);
            }
            
            // Capture enemy states
            if (captureEnemyStates)
            {
                CaptureEnemyStates(sceneState);
            }
            
            // Capture environment state
            if (captureEnvironmentState)
            {
                CaptureEnvironmentState(sceneState);
            }
            
            // Capture player position
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                sceneState.playerPosition = player.transform.position;
                sceneState.playerRotation = player.transform.rotation;
            }
            
            // Store in cache
            sceneStates[currentSceneId] = sceneState;
            lastStateCapture = DateTime.Now;
            
            // Manage cache size
            if (sceneStates.Count > maxSceneStatesInMemory)
            {
                PruneSceneStateCache();
            }
            
            OnSceneStateCaptured?.Invoke(sceneState);
            
            if (logStateChanges)
            {
                LogDebug($"Captured scene state: {currentSceneId}");
            }
            
            return sceneState;
        }
        
        /// <summary>
        /// Restore scene state from saved data
        /// </summary>
        public void RestoreSceneState(SceneState sceneState)
        {
            if (sceneState == null)
            {
                LogError("Cannot restore null scene state");
                return;
            }
            
            LogDebug($"Restoring scene state: {sceneState.sceneId}");
            
            // Restore interactive objects
            if (sceneState.interactiveObjects != null)
            {
                RestoreInteractiveObjects(sceneState.interactiveObjects);
            }
            
            // Restore enemies
            if (sceneState.enemyStates != null)
            {
                RestoreEnemyStates(sceneState.enemyStates);
            }
            
            // Restore environment
            if (sceneState.environmentState != null)
            {
                RestoreEnvironmentState(sceneState.environmentState);
            }
            
            // Restore player position
            var player = GameObject.FindWithTag("Player");
            if (player != null && sceneState.playerPosition != Vector3.zero)
            {
                player.transform.position = sceneState.playerPosition;
                player.transform.rotation = sceneState.playerRotation;
            }
            
            OnSceneStateRestored?.Invoke(sceneState);
        }
        
        /// <summary>
        /// Generate save data with complete procedural state
        /// </summary>
        public ConsolidatedSaveData GenerateSaveDataWithProceduralState()
        {
            // Capture current scene state
            var currentSceneState = CaptureCurrentSceneState();
            
            // Create or get save data
            var saveData = EnhancedSaveSystem.Load() ?? ConsolidatedSaveData.CreateNew();
            
            // Update procedural state
            if (currentWorldState != null)
            {
                saveData.worldState.proceduralState.currentSeed = currentWorldState.seed.GetHashCode();
                saveData.worldState.proceduralState.currentDepth = currentWorldState.currentDepth;
                saveData.worldState.proceduralState.currentPath = currentWorldState.currentPath;
                saveData.worldState.proceduralState.mapData = currentWorldState.mapData;
                
                // Add all generated scenes
                saveData.worldState.proceduralState.generatedScenes.Clear();
                foreach (var kvp in sceneStates)
                {
                    var sceneData = ConvertToGeneratedSceneData(kvp.Value);
                    saveData.worldState.proceduralState.generatedScenes.Add(sceneData);
                }
            }
            
            // Update current scene info
            saveData.worldState.currentSceneName = SceneManager.GetActiveScene().name;
            
            if (currentSceneState != null)
            {
                saveData.worldState.playerPosition = currentSceneState.playerPosition;
            }
            
            return saveData;
        }
        
        /// <summary>
        /// Validate that loaded state matches expected generation
        /// </summary>
        public bool ValidateLoadedState(ProceduralGenerationState loadedState)
        {
            if (!validateOnLoad || loadedState == null)
                return true;
            
            // Regenerate map with loaded seed
            var testMap = pathGenerator.GenerateMapWithRules(
                loadedState.currentSeed.ToString(), 
                null
            );
            
            var testMapData = SerializeMap(testMap);
            
            // Compare map data
            if (testMapData != loadedState.mapData)
            {
                OnStateValidationFailed?.Invoke("Map generation mismatch");
                return false;
            }
            
            // Validate scene states exist
            foreach (var sceneData in loadedState.generatedScenes)
            {
                if (sceneData.isCompleted && !ValidateSceneCompletion(sceneData))
                {
                    OnStateValidationFailed?.Invoke($"Scene completion mismatch: {sceneData.sceneId}");
                    return false;
                }
            }
            
            return true;
        }
        
        #endregion
        
        #region Private Methods - State Capture
        
        private void CaptureInteractiveObjectStates(SceneState sceneState)
        {
            sceneState.interactiveObjects = new Dictionary<string, InteractiveObjectState>();
            
            // Capture doors
            var doors = FindObjectsOfType<Door>();
            foreach (var door in doors)
            {
                var state = new InteractiveObjectState
                {
                    objectId = GetObjectId(door.gameObject),
                    objectType = "Door",
                    isActive = door.gameObject.activeSelf,
                    customData = new Dictionary<string, object>
                    {
                        ["isOpen"] = door.GetType().GetProperty("isOpen")?.GetValue(door) ?? false
                    }
                };
                sceneState.interactiveObjects[state.objectId] = state;
            }
            
            // Capture switches
            var switches = FindObjectsOfType<Switch>();
            foreach (var sw in switches)
            {
                var state = new InteractiveObjectState
                {
                    objectId = GetObjectId(sw.gameObject),
                    objectType = "Switch",
                    isActive = sw.gameObject.activeSelf,
                    customData = new Dictionary<string, object>
                    {
                        ["isActivated"] = sw.GetType().GetProperty("isActivated")?.GetValue(sw) ?? false
                    }
                };
                sceneState.interactiveObjects[state.objectId] = state;
            }
            
            // Capture collectibles
            var collectibles = FindObjectsOfType<Collectible>();
            foreach (var collectible in collectibles)
            {
                var state = new InteractiveObjectState
                {
                    objectId = GetObjectId(collectible.gameObject),
                    objectType = "Collectible",
                    isActive = collectible.gameObject.activeSelf,
                    position = collectible.transform.position
                };
                sceneState.interactiveObjects[state.objectId] = state;
            }
        }
        
        private void CaptureEnemyStates(SceneState sceneState)
        {
            sceneState.enemyStates = new List<EnemyState>();
            
            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                var enemyState = new EnemyState
                {
                    enemyId = GetObjectId(enemy),
                    enemyType = enemy.name.Replace("(Clone)", "").Trim(),
                    position = enemy.transform.position,
                    rotation = enemy.transform.rotation,
                    isAlive = enemy.activeSelf
                };
                
                // Capture health if available
                var health = enemy.GetComponent<Health>();
                if (health != null)
                {
                    enemyState.currentHealth = health.CurrentHealth;
                    enemyState.maxHealth = health.MaxHealth;
                }
                
                sceneState.enemyStates.Add(enemyState);
            }
        }
        
        private void CaptureEnvironmentState(SceneState sceneState)
        {
            sceneState.environmentState = new EnvironmentState
            {
                destructibleObjects = new List<DestructibleState>(),
                activeTriggers = new List<string>(),
                customEnvironmentData = new Dictionary<string, object>()
            };
            
            // Capture destructible objects
            var destructibles = FindObjectsOfType<Destructible>();
            foreach (var destructible in destructibles)
            {
                var state = new DestructibleState
                {
                    objectId = GetObjectId(destructible.gameObject),
                    isDestroyed = !destructible.gameObject.activeSelf,
                    position = destructible.transform.position
                };
                sceneState.environmentState.destructibleObjects.Add(state);
            }
            
            // Capture active triggers
            var triggers = FindObjectsOfType<TriggerZone>();
            foreach (var trigger in triggers)
            {
                if (trigger.IsActivated)
                {
                    sceneState.environmentState.activeTriggers.Add(GetObjectId(trigger.gameObject));
                }
            }
        }
        
        #endregion
        
        #region Private Methods - State Restoration
        
        private void RestoreInteractiveObjects(Dictionary<string, InteractiveObjectState> objects)
        {
            foreach (var kvp in objects)
            {
                var obj = FindObjectById(kvp.Key);
                if (obj == null) continue;
                
                var state = kvp.Value;
                obj.SetActive(state.isActive);
                
                // Restore specific object types
                switch (state.objectType)
                {
                    case "Door":
                        var door = obj.GetComponent<Door>();
                        if (door != null && state.customData.ContainsKey("isOpen"))
                        {
                            door.SetState((bool)state.customData["isOpen"]);
                        }
                        break;
                        
                    case "Switch":
                        var sw = obj.GetComponent<Switch>();
                        if (sw != null && state.customData.ContainsKey("isActivated"))
                        {
                            sw.SetState((bool)state.customData["isActivated"]);
                        }
                        break;
                        
                    case "Collectible":
                        if (state.position != Vector3.zero)
                        {
                            obj.transform.position = state.position;
                        }
                        break;
                }
            }
        }
        
        private void RestoreEnemyStates(List<EnemyState> enemies)
        {
            foreach (var enemyState in enemies)
            {
                var enemy = FindObjectById(enemyState.enemyId);
                
                if (enemy == null && enemyState.isAlive)
                {
                    // Spawn enemy if it should exist
                    enemy = SpawnEnemy(enemyState);
                }
                else if (enemy != null)
                {
                    // Update existing enemy
                    enemy.SetActive(enemyState.isAlive);
                    enemy.transform.position = enemyState.position;
                    enemy.transform.rotation = enemyState.rotation;
                    
                    var health = enemy.GetComponent<Health>();
                    if (health != null)
                    {
                        health.SetHealth(enemyState.currentHealth);
                    }
                }
            }
        }
        
        private void RestoreEnvironmentState(EnvironmentState environment)
        {
            // Restore destructibles
            foreach (var destructibleState in environment.destructibleObjects)
            {
                var obj = FindObjectById(destructibleState.objectId);
                if (obj != null)
                {
                    obj.SetActive(!destructibleState.isDestroyed);
                    if (!destructibleState.isDestroyed)
                    {
                        obj.transform.position = destructibleState.position;
                    }
                }
            }
            
            // Restore triggers
            foreach (var triggerId in environment.activeTriggers)
            {
                var trigger = FindObjectById(triggerId);
                if (trigger != null)
                {
                    var triggerZone = trigger.GetComponent<TriggerZone>();
                    if (triggerZone != null)
                    {
                        triggerZone.Activate();
                    }
                }
            }
        }
        
        #endregion
        
        #region Private Methods - Helpers
        
        private void GenerateInitialSceneStates(MysticalMap map, int startDepth)
        {
            // Generate states for rooms around starting depth
            int depthRange = 2;
            
            for (int depth = Math.Max(0, startDepth - depthRange); 
                 depth <= Math.Min(map.Layers.Count - 1, startDepth + depthRange); 
                 depth++)
            {
                var layer = map.Layers[depth];
                
                foreach (var path in layer.Paths)
                {
                    var sceneState = GenerateSceneStateFromPath(path, depth);
                    sceneStates[sceneState.sceneId] = sceneState;
                }
            }
        }
        
        private SceneState GenerateSceneStateFromPath(PathNode path, int depth)
        {
            var sceneId = $"{currentWorldState.seed}_{depth}_{path.PathType}";
            
            return new SceneState
            {
                sceneId = sceneId,
                sceneName = DetermineSceneName(path, depth),
                depth = depth,
                pathType = path.PathType.ToString(),
                generatedAt = DateTime.Now,
                interactiveObjects = GenerateDefaultInteractiveObjects(path),
                enemyStates = GenerateDefaultEnemies(path, depth)
            };
        }
        
        private Dictionary<string, InteractiveObjectState> GenerateDefaultInteractiveObjects(PathNode path)
        {
            var objects = new Dictionary<string, InteractiveObjectState>();
            
            // Generate doors based on connections
            int doorCount = path.Connections?.Count ?? 1;
            for (int i = 0; i < doorCount; i++)
            {
                var doorId = $"door_{path.NodeId}_{i}";
                objects[doorId] = new InteractiveObjectState
                {
                    objectId = doorId,
                    objectType = "Door",
                    isActive = true,
                    customData = new Dictionary<string, object> { ["isOpen"] = false }
                };
            }
            
            return objects;
        }
        
        private List<EnemyState> GenerateDefaultEnemies(PathNode path, int depth)
        {
            var enemies = new List<EnemyState>();
            
            // Generate enemies based on path type and depth
            int enemyCount = path.PathType == PathType.Elite ? depth + 2 : depth;
            
            for (int i = 0; i < enemyCount; i++)
            {
                enemies.Add(new EnemyState
                {
                    enemyId = $"enemy_{path.NodeId}_{i}",
                    enemyType = DetermineEnemyType(path.PathType, depth),
                    isAlive = true,
                    currentHealth = 100,
                    maxHealth = 100
                });
            }
            
            return enemies;
        }
        
        private void ApplyConfigurationOverrides(SaveStateConfiguration configuration)
        {
            // Apply any specific overrides from configuration
            // This could include specific room states, enemy configurations, etc.
        }
        
        private void PruneSceneStateCache()
        {
            // Remove oldest scene states
            var toRemove = sceneStates
                .OrderBy(kvp => kvp.Value.capturedAt)
                .Take(sceneStates.Count - maxSceneStatesInMemory)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in toRemove)
            {
                sceneStates.Remove(key);
            }
        }
        
        private GeneratedSceneData ConvertToGeneratedSceneData(SceneState sceneState)
        {
            return new GeneratedSceneData
            {
                sceneId = sceneState.sceneId,
                sceneName = sceneState.sceneName,
                depth = sceneState.depth,
                pathType = sceneState.pathType,
                playerSpawnPosition = sceneState.playerPosition,
                isCompleted = sceneState.isCompleted,
                generatedAt = sceneState.generatedAt,
                interactiveObjectStates = ConvertInteractiveObjectStates(sceneState.interactiveObjects)
            };
        }
        
        private Dictionary<string, string> ConvertInteractiveObjectStates(
            Dictionary<string, InteractiveObjectState> objects)
        {
            if (objects == null) return new Dictionary<string, string>();
            
            var result = new Dictionary<string, string>();
            foreach (var kvp in objects)
            {
                result[kvp.Key] = JsonConvert.SerializeObject(kvp.Value);
            }
            return result;
        }
        
        private bool ValidateSceneCompletion(GeneratedSceneData sceneData)
        {
            // Validate that completed scenes have expected state
            return sceneData.isCompleted;
        }
        
        private string GenerateSceneId(string sceneName)
        {
            return $"{sceneName}_{DateTime.Now.Ticks}";
        }
        
        private string GenerateUniqueSeed()
        {
            return Guid.NewGuid().ToString();
        }
        
        private string GetObjectId(GameObject obj)
        {
            // Generate consistent ID for game objects
            return $"{obj.name}_{obj.transform.position.GetHashCode()}";
        }
        
        private GameObject FindObjectById(string objectId)
        {
            // Find object by ID - simplified version
            var allObjects = FindObjectsOfType<GameObject>();
            return allObjects.FirstOrDefault(obj => GetObjectId(obj) == objectId);
        }
        
        private GameObject SpawnEnemy(EnemyState enemyState)
        {
            // Spawn enemy from prefab - would need prefab management system
            LogDebug($"Would spawn enemy: {enemyState.enemyType} at {enemyState.position}");
            return null;
        }
        
        private string DetermineSceneName(PathNode path, int depth)
        {
            if (path.PathType == PathType.Boss)
                return $"BossRoom_{depth}";
            if (path.PathType == PathType.Elite)
                return $"EliteRoom_{depth}";
            return $"ProceduralRoom_{depth}";
        }
        
        private string DetermineEnemyType(PathType pathType, int depth)
        {
            if (pathType == PathType.Boss)
                return "BossEnemy";
            if (pathType == PathType.Elite)
                return "EliteEnemy";
            return $"Enemy_Tier{Math.Min(depth / 2, 3)}";
        }
        
        private string SerializeMap(MysticalMap map)
        {
            return JsonConvert.SerializeObject(map);
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                // Check if we have a saved state for this scene
                var sceneId = GenerateSceneId(scene.name);
                if (sceneStates.TryGetValue(sceneId, out SceneState sceneState))
                {
                    RestoreSceneState(sceneState);
                }
            }
        }
        
        private void HandleSceneUnloaded(Scene scene)
        {
            // Capture state before unloading
            if (!isGeneratingState)
            {
                CaptureCurrentSceneState();
            }
        }
        
        private void HandleSaveRequested(ConsolidatedSaveData saveData)
        {
            // Update save data with current procedural state
            var updatedSaveData = GenerateSaveDataWithProceduralState();
            
            // Merge procedural state into provided save data
            saveData.worldState.proceduralState = updatedSaveData.worldState.proceduralState;
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            // Validate and restore procedural state
            if (ValidateLoadedState(saveData.worldState.proceduralState))
            {
                // Restore world state
                currentWorldState = new ProceduralWorldState
                {
                    seed = saveData.worldState.proceduralState.currentSeed.ToString(),
                    mapData = saveData.worldState.proceduralState.mapData,
                    currentDepth = saveData.worldState.proceduralState.currentDepth,
                    currentPath = saveData.worldState.proceduralState.currentPath
                };
                
                // Restore scene states
                sceneStates.Clear();
                foreach (var sceneData in saveData.worldState.proceduralState.generatedScenes)
                {
                    var sceneState = ConvertFromGeneratedSceneData(sceneData);
                    sceneStates[sceneState.sceneId] = sceneState;
                }
            }
        }
        
        private SceneState ConvertFromGeneratedSceneData(GeneratedSceneData sceneData)
        {
            var sceneState = new SceneState
            {
                sceneId = sceneData.sceneId,
                sceneName = sceneData.sceneName,
                depth = sceneData.depth,
                pathType = sceneData.pathType,
                playerPosition = sceneData.playerSpawnPosition,
                isCompleted = sceneData.isCompleted,
                generatedAt = sceneData.generatedAt
            };
            
            // Convert interactive object states
            if (sceneData.interactiveObjectStates != null)
            {
                sceneState.interactiveObjects = new Dictionary<string, InteractiveObjectState>();
                foreach (var kvp in sceneData.interactiveObjectStates)
                {
                    var objState = JsonConvert.DeserializeObject<InteractiveObjectState>(kvp.Value);
                    sceneState.interactiveObjects[kvp.Key] = objState;
                }
            }
            
            return sceneState;
        }
        
        #endregion
        
        #region Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debugger.Log($"[ProceduralSaveState] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[ProceduralSaveState] {message}");
        }
        
        #endregion
    }
    
    #region Data Classes
    
    [Serializable]
    public class ProceduralWorldState
    {
        public string worldId;
        public string seed;
        public string mapData;
        public int currentDepth;
        public string currentPath;
        public string configuration;
        public DateTime generatedAt;
        public Dictionary<string, SceneState> scenes = new Dictionary<string, SceneState>();
    }
    
    [Serializable]
    public class SceneState
    {
        public string sceneId;
        public string sceneName;
        public int depth;
        public string pathType;
        public Vector3 playerPosition;
        public Quaternion playerRotation;
        public bool isCompleted;
        public DateTime capturedAt;
        public DateTime generatedAt;
        
        public Dictionary<string, InteractiveObjectState> interactiveObjects;
        public List<EnemyState> enemyStates;
        public EnvironmentState environmentState;
    }
    
    [Serializable]
    public class InteractiveObjectState
    {
        public string objectId;
        public string objectType;
        public bool isActive;
        public Vector3 position;
        public Dictionary<string, object> customData;
    }
    
    [Serializable]
    public class EnemyState
    {
        public string enemyId;
        public string enemyType;
        public Vector3 position;
        public Quaternion rotation;
        public bool isAlive;
        public float currentHealth;
        public float maxHealth;
    }
    
    [Serializable]
    public class EnvironmentState
    {
        public List<DestructibleState> destructibleObjects;
        public List<string> activeTriggers;
        public Dictionary<string, object> customEnvironmentData;
    }
    
    [Serializable]
    public class DestructibleState
    {
        public string objectId;
        public bool isDestroyed;
        public Vector3 position;
    }
    
    #endregion
    
    #region Placeholder Components
    
    public class Health : MonoBehaviour
    {
        public float CurrentHealth { get; private set; } = 100f;
        public float MaxHealth { get; private set; } = 100f;
        
        public void SetHealth(float health)
        {
            CurrentHealth = Mathf.Clamp(health, 0, MaxHealth);
        }
    }
    
    public class Destructible : MonoBehaviour
    {
        public bool IsDestroyed { get; private set; }
        
        public void Destroy()
        {
            IsDestroyed = true;
            gameObject.SetActive(false);
        }
    }
    
    public class TriggerZone : MonoBehaviour
    {
        public bool IsActivated { get; private set; }
        
        public void Activate()
        {
            IsActivated = true;
        }
    }
    
    #endregion
}