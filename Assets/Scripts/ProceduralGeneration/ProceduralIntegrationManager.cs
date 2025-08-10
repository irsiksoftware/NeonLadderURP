using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;
using NeonLadder.DataManagement;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Manages the integration between SaveStateConfiguration and procedural generation.
    /// Ensures saved games maintain consistent procedural content and player progression.
    /// </summary>
    public class ProceduralIntegrationManager : MonoBehaviour
    {
        #region Singleton
        
        private static ProceduralIntegrationManager instance;
        public static ProceduralIntegrationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ProceduralIntegrationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ProceduralIntegrationManager");
                        instance = go.AddComponent<ProceduralIntegrationManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Integration Settings")]
        [SerializeField] private bool maintainDeterministicGeneration = true;
        [SerializeField] private bool validateGenerationConsistency = true;
        [SerializeField] private bool autoSaveOnSceneTransition = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        private PathGenerator pathGenerator;
        private ProceduralSceneLoader sceneLoader;
        private SaveStateConfiguration currentConfiguration;
        private MysticalMap currentMap;
        private Dictionary<string, GeneratedSceneData> sceneDataCache;
        private ProceduralGenerationState currentProceduralState;
        
        #endregion
        
        #region Events
        
        public static event Action<ProceduralGenerationState> OnProceduralStateChanged;
        public static event Action<string, MysticalMap> OnMapGenerated;
        public static event Action<GeneratedSceneData> OnSceneDataRestored;
        public static event Action<string> OnValidationFailed;
        
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
            sceneLoader = ProceduralSceneLoader.Instance;
            sceneDataCache = new Dictionary<string, GeneratedSceneData>();
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveRequested += HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
            
            // Subscribe to scene events
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ProceduralSceneLoader.OnSceneGenerationCompleted += HandleSceneGenerated;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            EnhancedSaveSystem.OnSaveRequested -= HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            
            if (ProceduralSceneLoader.Instance != null)
            {
                ProceduralSceneLoader.OnSceneGenerationCompleted -= HandleSceneGenerated;
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Load procedural content from SaveStateConfiguration
        /// </summary>
        public void LoadFromConfiguration(SaveStateConfiguration configuration)
        {
            if (configuration == null)
            {
                LogError("Cannot load from null SaveStateConfiguration");
                return;
            }
            
            currentConfiguration = configuration;
            var saveData = configuration.CreateSaveData();
            
            LoadProceduralContent(saveData);
        }
        
        /// <summary>
        /// Load procedural content from ConsolidatedSaveData
        /// </summary>
        public void LoadProceduralContent(ConsolidatedSaveData saveData)
        {
            if (saveData == null)
            {
                LogError("Cannot load procedural content from null save data");
                return;
            }
            
            var procState = saveData.worldState.proceduralState;
            currentProceduralState = procState;
            
            // Generate or restore the map
            if (!string.IsNullOrEmpty(procState.currentSeed.ToString()))
            {
                RestoreProceduralMap(procState);
            }
            else
            {
                GenerateNewMap();
            }
            
            // Load the appropriate scene
            if (!string.IsNullOrEmpty(procState.currentPath))
            {
                LoadProceduralScene(procState);
            }
            
            OnProceduralStateChanged?.Invoke(procState);
        }
        
        /// <summary>
        /// Generate a new procedural map with optional seed
        /// </summary>
        public MysticalMap GenerateNewMap(string seed = null, GenerationRules rules = null)
        {
            seed = seed ?? GenerateSeed();
            currentMap = pathGenerator.GenerateMapWithRules(seed, rules);
            
            // Update procedural state
            if (currentProceduralState == null)
            {
                currentProceduralState = new ProceduralGenerationState();
            }
            
            currentProceduralState.currentSeed = seed.GetHashCode();
            currentProceduralState.mapData = SerializeMap(currentMap);
            
            OnMapGenerated?.Invoke(seed, currentMap);
            LogDebug($"Generated new map with seed: {seed}");
            
            return currentMap;
        }
        
        /// <summary>
        /// Validate that current procedural state matches saved state
        /// </summary>
        public bool ValidateProceduralConsistency(ProceduralGenerationState savedState)
        {
            if (!validateGenerationConsistency)
                return true;
            
            if (savedState == null || currentProceduralState == null)
                return false;
            
            // Check seed consistency
            if (savedState.currentSeed != currentProceduralState.currentSeed)
            {
                OnValidationFailed?.Invoke("Seed mismatch");
                return false;
            }
            
            // Check depth consistency
            if (savedState.currentDepth != currentProceduralState.currentDepth)
            {
                OnValidationFailed?.Invoke("Depth mismatch");
                return false;
            }
            
            // Regenerate map with saved seed and compare
            if (maintainDeterministicGeneration)
            {
                var testMap = pathGenerator.GenerateMapWithRules(savedState.currentSeed.ToString(), null);
                var testMapData = SerializeMap(testMap);
                
                if (testMapData != savedState.mapData)
                {
                    OnValidationFailed?.Invoke("Map generation inconsistency");
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Get current procedural save state
        /// </summary>
        public ProceduralGenerationState GetCurrentState()
        {
            return currentProceduralState;
        }
        
        /// <summary>
        /// Apply procedural parameters to scene
        /// </summary>
        public void ApplyProceduralParameters(GeneratedSceneData sceneData)
        {
            if (sceneData == null) return;
            
            // Cache scene data for restoration
            string sceneKey = GenerateSceneKey(sceneData);
            sceneDataCache[sceneKey] = sceneData;
            
            // Apply to current scene
            ApplySceneConfiguration(sceneData);
            
            LogDebug($"Applied procedural parameters to scene: {sceneData.sceneName}");
        }
        
        #endregion
        
        #region Private Methods
        
        private void RestoreProceduralMap(ProceduralGenerationState procState)
        {
            // Regenerate map with saved seed
            currentMap = pathGenerator.GenerateMapWithRules(procState.currentSeed.ToString(), null);
            
            // Validate consistency if enabled
            if (validateGenerationConsistency)
            {
                var generatedMapData = SerializeMap(currentMap);
                if (generatedMapData != procState.mapData)
                {
                    LogWarning("Map generation inconsistency detected - using saved map data");
                    currentMap = DeserializeMap(procState.mapData);
                }
            }
            
            LogDebug($"Restored procedural map from seed: {procState.currentSeed}");
        }
        
        private void LoadProceduralScene(ProceduralGenerationState procState)
        {
            // Determine scene parameters
            var sceneData = new GeneratedSceneData
            {
                sceneId = GenerateSceneId(procState),
                sceneName = DetermineSceneName(procState),
                depth = procState.currentDepth,
                seed = procState.currentSeed,
                pathType = procState.currentPath,
                playerSpawnPosition = Vector3.zero // Will be set from scene data or checkpoint
            };
            
            // Check cache for existing scene data
            string sceneKey = GenerateSceneKey(sceneData);
            if (sceneDataCache.TryGetValue(sceneKey, out GeneratedSceneData cachedData))
            {
                sceneData = cachedData;
                OnSceneDataRestored?.Invoke(sceneData);
            }
            
            // Load scene through ProceduralSceneLoader
            sceneLoader.LoadProceduralScene(
                procState.currentSeed.ToString(),
                procState.currentDepth,
                procState.currentPath
            );
        }
        
        private void ApplySceneConfiguration(GeneratedSceneData sceneData)
        {
            // Find and configure scene objects based on procedural data
            var sceneObjects = FindObjectsOfType<ProceduralSceneObject>();
            
            foreach (var obj in sceneObjects)
            {
                obj.Configure(sceneData);
            }
            
            // Apply interactive object states
            if (sceneData.interactiveObjectStates != null)
            {
                foreach (var kvp in sceneData.interactiveObjectStates)
                {
                    var interactiveObj = GameObject.Find(kvp.Key);
                    if (interactiveObj != null)
                    {
                        ApplyObjectState(interactiveObj, kvp.Value);
                    }
                }
            }
            
            // Set player spawn position
            var player = GameObject.FindWithTag("Player");
            if (player != null && sceneData.playerSpawnPosition != Vector3.zero)
            {
                player.transform.position = sceneData.playerSpawnPosition;
            }
        }
        
        private void ApplyObjectState(GameObject obj, string state)
        {
            // Apply state based on object type
            var door = obj.GetComponent<Door>();
            if (door != null)
            {
                door.SetState(state == "open");
                return;
            }
            
            var collectible = obj.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.gameObject.SetActive(state != "collected");
                return;
            }
            
            var switch_ = obj.GetComponent<Switch>();
            if (switch_ != null)
            {
                switch_.SetState(state == "activated");
                return;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleSaveRequested(ConsolidatedSaveData saveData)
        {
            if (currentProceduralState != null)
            {
                // Update save data with current procedural state
                saveData.worldState.proceduralState = currentProceduralState;
                
                // Save current scene data
                if (sceneLoader.GetCurrentSceneData() != null)
                {
                    var sceneData = sceneLoader.GetCurrentSceneData();
                    // Add to generated scenes list if not already there
                    if (!currentProceduralState.generatedScenes.Any(s => s.sceneId == sceneData.sceneId))
                    {
                        currentProceduralState.generatedScenes.Add(sceneData);
                    }
                }
            }
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            LoadProceduralContent(saveData);
        }
        
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (autoSaveOnSceneTransition && mode == LoadSceneMode.Single)
            {
                // Auto-save procedural state on scene transition
                var saveData = EnhancedSaveSystem.Load();
                if (saveData != null && currentProceduralState != null)
                {
                    saveData.worldState.proceduralState = currentProceduralState;
                    saveData.worldState.currentSceneName = scene.name;
                    EnhancedSaveSystem.Save(saveData);
                }
            }
        }
        
        private void HandleSceneGenerated(GeneratedSceneData sceneData)
        {
            ApplyProceduralParameters(sceneData);
        }
        
        #endregion
        
        #region Utility Methods
        
        private string GenerateSeed()
        {
            return Guid.NewGuid().ToString();
        }
        
        private string GenerateSceneId(ProceduralGenerationState procState)
        {
            return $"{procState.currentSeed}_{procState.currentDepth}_{procState.currentPath}";
        }
        
        private string GenerateSceneKey(GeneratedSceneData sceneData)
        {
            return $"{sceneData.seed}_{sceneData.depth}_{sceneData.pathType}";
        }
        
        private string DetermineSceneName(ProceduralGenerationState procState)
        {
            // Map depth and path type to actual scene names
            if (procState.currentDepth == 0)
                return "Hub";
            
            if (procState.currentPath.Contains("Boss"))
                return $"BossRoom_{procState.currentDepth}";
            
            if (procState.currentPath.Contains("Elite"))
                return $"EliteRoom_{procState.currentDepth}";
            
            return $"ProceduralRoom_{procState.currentDepth}";
        }
        
        private string SerializeMap(MysticalMap map)
        {
            if (map == null) return "";
            return JsonUtility.ToJson(map);
        }
        
        private MysticalMap DeserializeMap(string mapData)
        {
            if (string.IsNullOrEmpty(mapData)) return null;
            return JsonUtility.FromJson<MysticalMap>(mapData);
        }
        
        private string SerializeSceneData(GeneratedSceneData sceneData)
        {
            if (sceneData == null) return "";
            return JsonUtility.ToJson(sceneData);
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debugger.Log($"[ProceduralIntegration] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debugger.LogWarning($"[ProceduralIntegration] {message}");
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[ProceduralIntegration] {message}");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Base class for procedural scene objects that need configuration
    /// </summary>
    public abstract class ProceduralSceneObject : MonoBehaviour
    {
        public abstract void Configure(GeneratedSceneData sceneData);
    }
    
    /// <summary>
    /// Door component placeholder for procedural scenes
    /// </summary>
    public class Door : ProceduralSceneObject
    {
        private bool isOpen;
        
        public void SetState(bool open)
        {
            isOpen = open;
            // Apply visual state
            gameObject.SetActive(!isOpen);
        }
        
        public override void Configure(GeneratedSceneData sceneData)
        {
            // Configure door based on scene data
        }
    }
    
    /// <summary>
    /// Collectible component placeholder for procedural scenes
    /// </summary>
    public class Collectible : ProceduralSceneObject
    {
        public override void Configure(GeneratedSceneData sceneData)
        {
            // Configure collectible based on scene data
        }
    }
    
    /// <summary>
    /// Switch component placeholder for procedural scenes
    /// </summary>
    public class Switch : ProceduralSceneObject
    {
        private bool isActivated;
        
        public void SetState(bool activated)
        {
            isActivated = activated;
            // Apply visual state
        }
        
        public override void Configure(GeneratedSceneData sceneData)
        {
            // Configure switch based on scene data
        }
    }
}