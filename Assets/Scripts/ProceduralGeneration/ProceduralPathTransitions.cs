using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// ProceduralPathTransitions - Manages procedural paths to all 8 boss arenas
    /// Generates deterministic paths with 2 connector scenes per boss destination
    /// Integrates with SceneRouter and SpawnPointManager for seamless transitions
    /// </summary>
    public class ProceduralPathTransitions : MonoBehaviour
    {
        [Header("Boss Path Configuration")]
        [SerializeField] private BossPath[] allBossPaths;
        [SerializeField] private int currentSeed;
        [SerializeField] private BossPath activePath;
        
        [Header("Path Generation Settings")]
        [SerializeField] private bool useDeterministicGeneration = true;
        [SerializeField] private int maxConnectorScenes = 2;
        [SerializeField] private bool enablePathCaching = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        [Header("Integration Components")]
        [SerializeField] private SceneRouter sceneRouter;
        [SerializeField] private SceneRoutingContext routingContext;
        
        // Path caching for performance
        private Dictionary<string, BossPath> cachedPaths = new Dictionary<string, BossPath>();
        private Dictionary<int, List<MapNode>> generatedPathsCache = new Dictionary<int, List<MapNode>>();
        
        // Save state tracking
        private BossPathSaveState currentSaveState;
        
        // Events
        public event Action<BossPath> OnPathGenerated;
        public event Action<BossPath> OnPathActivated;
        public event Action<string> OnPathCompleted;
        public event Action<BossPathSaveState> OnPathSaved;
        public event Action<BossPathSaveState> OnPathLoaded;
        
        // Properties
        public BossPath ActivePath => activePath;
        public int CurrentSeed => currentSeed;
        public BossPath[] AllBossPaths => allBossPaths;
        public bool HasActivePath => activePath != null;
        
        private void Awake()
        {
            InitializeBossPaths();
            InitializeIntegrationComponents();
        }
        
        private void Start()
        {
            if (useDeterministicGeneration && currentSeed == 0)
            {
                currentSeed = UnityEngine.Random.Range(1, int.MaxValue);
                LogInfo($"Generated initial seed: {currentSeed}");
            }
        }
        
        private void InitializeBossPaths()
        {
            if (allBossPaths == null || allBossPaths.Length == 0)
            {
                allBossPaths = CreateDefaultBossPaths();
                LogInfo("Initialized default boss paths");
            }
            
            // Validate all boss paths
            foreach (var bossPath in allBossPaths)
            {
                ValidateBossPath(bossPath);
            }
        }
        
        private void InitializeIntegrationComponents()
        {
            if (sceneRouter == null)
                sceneRouter = SceneRouter.Instance;
                
            if (routingContext == null)
                routingContext = SceneRoutingContext.Instance;
        }
        
        private BossPath[] CreateDefaultBossPaths()
        {
            return new BossPath[]
            {
                CreateBossPath("Pride", "Cathedral", PathTheme.Divine, 
                    new[] { "Cathedral_Connection1", "Cathedral_Connection2" }),
                    
                CreateBossPath("Wrath", "Necropolis", PathTheme.Combat, 
                    new[] { "Necropolis_Connection1", "Necropolis_Connection2" }),
                    
                CreateBossPath("Greed", "Vault", PathTheme.Treasure, 
                    new[] { "Vault_Connection1", "Vault_Connection2" }),
                    
                CreateBossPath("Envy", "Mirage", PathTheme.Illusion, 
                    new[] { "Mirage_Connection1", "Mirage_Connection2" }),
                    
                CreateBossPath("Lust", "Garden", PathTheme.Natural, 
                    new[] { "Garden_Connection1", "Garden_Connection2" }),
                    
                CreateBossPath("Gluttony", "Banquet", PathTheme.Indulgence, 
                    new[] { "Banquet_Connection1", "Banquet_Connection2" }),
                    
                CreateBossPath("Sloth", "Lounge", PathTheme.Laziness, 
                    new[] { "Lounge_Connection1", "Lounge_Connection2" }),
                    
                CreateBossPath("Devil", "Finale", PathTheme.Finale, 
                    new[] { "Finale_Connection1", "Finale_Connection2" }, true)
            };
        }
        
        private BossPath CreateBossPath(string bossName, string sceneName, PathTheme theme, string[] connectors, bool isFinale = false)
        {
            return new BossPath
            {
                bossName = bossName,
                bossSceneName = sceneName,
                possibleConnectors = connectors.ToList(),
                requiredConnections = maxConnectorScenes,
                pathTheme = theme,
                isFinaleUnlocked = !isFinale,
                difficultyModifier = isFinale ? 2.0f : 1.0f,
                unlockRequirements = isFinale ? new[] { "Pride", "Wrath", "Greed", "Envy", "Lust", "Gluttony", "Sloth" } : new string[0]
            };
        }
        
        /// <summary>
        /// Generates a procedural path to the specified boss
        /// </summary>
        public BossPath GeneratePathToBoss(string bossName, int? customSeed = null)
        {
            if (string.IsNullOrEmpty(bossName))
            {
                LogError("Cannot generate path to boss with empty name");
                return null;
            }
            
            // Find boss configuration
            var bossConfig = allBossPaths.FirstOrDefault(bp => 
                bp.bossName.Equals(bossName, StringComparison.OrdinalIgnoreCase));
                
            if (bossConfig == null)
            {
                LogError($"Boss configuration not found: {bossName}");
                return null;
            }
            
            // Check unlock requirements
            if (!IsBossUnlocked(bossConfig))
            {
                LogWarning($"Boss {bossName} is not yet unlocked");
                return null;
            }
            
            // Use provided seed or current seed
            int pathSeed = customSeed ?? currentSeed;
            
            // Check cache first
            string cacheKey = $"{bossName}_{pathSeed}";
            if (enablePathCaching && cachedPaths.TryGetValue(cacheKey, out BossPath cachedPath))
            {
                LogInfo($"Using cached path for {bossName}");
                return cachedPath;
            }
            
            // Generate new path
            var generatedPath = GeneratePathInternal(bossConfig, pathSeed);
            
            // Cache the result
            if (enablePathCaching)
            {
                cachedPaths[cacheKey] = generatedPath;
            }
            
            LogInfo($"Generated new path to {bossName} with {generatedPath.selectedConnectors.Count} connectors");
            OnPathGenerated?.Invoke(generatedPath);
            
            return generatedPath;
        }
        
        private BossPath GeneratePathInternal(BossPath bossConfig, int seed)
        {
            // Create seeded random for deterministic generation
            var random = new System.Random(seed);
            
            // Clone the boss configuration
            var generatedPath = new BossPath
            {
                bossName = bossConfig.bossName,
                bossSceneName = bossConfig.bossSceneName,
                possibleConnectors = new List<string>(bossConfig.possibleConnectors),
                requiredConnections = bossConfig.requiredConnections,
                pathTheme = bossConfig.pathTheme,
                isFinaleUnlocked = bossConfig.isFinaleUnlocked,
                difficultyModifier = bossConfig.difficultyModifier,
                unlockRequirements = bossConfig.unlockRequirements,
                pathSeed = seed,
                selectedConnectors = new List<string>()
            };
            
            // Procedurally select connector scenes
            var availableConnectors = new List<string>(bossConfig.possibleConnectors);
            
            for (int i = 0; i < bossConfig.requiredConnections && availableConnectors.Count > 0; i++)
            {
                int selectedIndex = random.Next(availableConnectors.Count);
                string selectedConnector = availableConnectors[selectedIndex];
                
                generatedPath.selectedConnectors.Add(selectedConnector);
                availableConnectors.RemoveAt(selectedIndex); // Ensure no duplicates
            }
            
            // Generate full path nodes for SceneRouter integration
            generatedPath.pathNodes = GeneratePathNodes(generatedPath);
            
            return generatedPath;
        }
        
        private List<MapNode> GeneratePathNodes(BossPath bossPath)
        {
            var pathNodes = new List<MapNode>();
            
            // Start node (MainCityHub)
            pathNodes.Add(new MapNode
            {
                Id = $"start_{bossPath.bossName}",
                Type = NodeType.Start,
                LayerIndex = 0,
                PathIndex = 0,
                NodeIndex = 0,
                Properties = new Dictionary<string, object>
                {
                    { "SceneName", "MainCityHub" },
                    { "TargetBoss", bossPath.bossName }
                }
            });
            
            // Connector nodes
            for (int i = 0; i < bossPath.selectedConnectors.Count; i++)
            {
                pathNodes.Add(new MapNode
                {
                    Id = $"connector_{bossPath.bossName}_{i + 1}",
                    Type = NodeType.Connection,
                    LayerIndex = 0,
                    PathIndex = 0,
                    NodeIndex = i + 1,
                    Properties = new Dictionary<string, object>
                    {
                        { "SceneName", bossPath.selectedConnectors[i] },
                        { "TargetBoss", bossPath.bossName },
                        { "ConnectionIndex", i + 1 },
                        { "PathTheme", bossPath.pathTheme.ToString() }
                    }
                });
            }
            
            // Boss node
            pathNodes.Add(new MapNode
            {
                Id = $"boss_{bossPath.bossName}",
                Type = NodeType.Boss,
                LayerIndex = 0,
                PathIndex = 0,
                NodeIndex = bossPath.selectedConnectors.Count + 1,
                Properties = new Dictionary<string, object>
                {
                    { "SceneName", bossPath.bossSceneName },
                    { "BossName", bossPath.bossName },
                    { "PathTheme", bossPath.pathTheme.ToString() },
                    { "DifficultyModifier", bossPath.difficultyModifier }
                }
            });
            
            return pathNodes;
        }
        
        /// <summary>
        /// Activates a path and updates the routing context
        /// </summary>
        public bool ActivatePath(BossPath path)
        {
            if (path == null)
            {
                LogError("Cannot activate null path");
                return false;
            }
            
            if (!IsBossUnlocked(path))
            {
                LogError($"Cannot activate path to locked boss: {path.bossName}");
                return false;
            }
            
            activePath = path;
            
            // Update routing context
            if (routingContext != null && path.pathNodes != null)
            {
                routingContext.CurrentPath = path.pathNodes;
                routingContext.CurrentPathIndex = 0;
                routingContext.SetPersistentData("ActiveBossPath", path.bossName);
                routingContext.SetPersistentData("PathSeed", path.pathSeed);
            }
            
            // Update scene router
            if (sceneRouter != null && path.pathNodes != null)
            {
                sceneRouter.SetCurrentPath(path.pathNodes);
            }
            
            LogInfo($"Activated path to {path.bossName}");
            OnPathActivated?.Invoke(path);
            
            return true;
        }
        
        /// <summary>
        /// Checks if a boss is unlocked based on requirements
        /// </summary>
        public bool IsBossUnlocked(BossPath bossPath)
        {
            if (bossPath == null || bossPath.unlockRequirements == null || bossPath.unlockRequirements.Length == 0)
                return true;
            
            // Check if all required bosses have been defeated
            foreach (string requiredBoss in bossPath.unlockRequirements)
            {
                if (!IsBossDefeated(requiredBoss))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Checks if a boss has been defeated (implementation depends on game save system)
        /// </summary>
        public bool IsBossDefeated(string bossName)
        {
            if (routingContext != null)
            {
                return routingContext.GetPersistentData($"Boss_{bossName}_Defeated", false);
            }
            
            // Fallback: check PlayerPrefs
            return PlayerPrefs.GetInt($"Boss_{bossName}_Defeated", 0) == 1;
        }
        
        /// <summary>
        /// Marks a boss as defeated
        /// </summary>
        public void MarkBossAsDefeated(string bossName)
        {
            if (routingContext != null)
            {
                routingContext.SetPersistentData($"Boss_{bossName}_Defeated", true);
            }
            
            PlayerPrefs.SetInt($"Boss_{bossName}_Defeated", 1);
            PlayerPrefs.Save();
            
            LogInfo($"Marked boss as defeated: {bossName}");
            OnPathCompleted?.Invoke(bossName);
        }
        
        /// <summary>
        /// Creates a save state of the current path progress
        /// </summary>
        public BossPathSaveState CreateSaveState()
        {
            var saveState = new BossPathSaveState
            {
                currentBossPath = activePath?.bossName ?? "",
                currentSceneIndex = routingContext?.CurrentPathIndex ?? 0,
                completedBosses = GetCompletedBosses(),
                pathSeed = currentSeed,
                saveTimestamp = DateTime.Now,
                playerProgress = new PlayerProgressData
                {
                    // These would be populated by the actual game systems
                    health = 100,
                    currency = routingContext?.GetPersistentData("Currency", 0) ?? 0,
                    currentScene = routingContext?.CurrentScene ?? ""
                }
            };
            
            currentSaveState = saveState;
            OnPathSaved?.Invoke(saveState);
            
            return saveState;
        }
        
        /// <summary>
        /// Loads a save state and restores path progress
        /// </summary>
        public bool LoadSaveState(BossPathSaveState saveState)
        {
            if (saveState == null)
            {
                LogError("Cannot load null save state");
                return false;
            }
            
            // Restore seed
            currentSeed = saveState.pathSeed;
            
            // Restore defeated bosses
            foreach (string defeatedBoss in saveState.completedBosses)
            {
                MarkBossAsDefeated(defeatedBoss);
            }
            
            // Restore active path if specified
            if (!string.IsNullOrEmpty(saveState.currentBossPath))
            {
                var path = GeneratePathToBoss(saveState.currentBossPath, saveState.pathSeed);
                if (path != null)
                {
                    ActivatePath(path);
                    
                    // Restore path progress
                    if (routingContext != null)
                    {
                        routingContext.CurrentPathIndex = saveState.currentSceneIndex;
                    }
                }
            }
            
            currentSaveState = saveState;
            LogInfo($"Loaded save state from {saveState.saveTimestamp}");
            OnPathLoaded?.Invoke(saveState);
            
            return true;
        }
        
        private List<string> GetCompletedBosses()
        {
            var completedBosses = new List<string>();
            
            foreach (var bossPath in allBossPaths)
            {
                if (IsBossDefeated(bossPath.bossName))
                {
                    completedBosses.Add(bossPath.bossName);
                }
            }
            
            return completedBosses;
        }
        
        private void ValidateBossPath(BossPath bossPath)
        {
            if (bossPath == null)
            {
                LogError("Boss path is null");
                return;
            }
            
            if (string.IsNullOrEmpty(bossPath.bossName))
            {
                LogError("Boss path has empty boss name");
            }
            
            if (string.IsNullOrEmpty(bossPath.bossSceneName))
            {
                LogError($"Boss path {bossPath.bossName} has empty scene name");
            }
            
            if (bossPath.possibleConnectors == null || bossPath.possibleConnectors.Count == 0)
            {
                LogWarning($"Boss path {bossPath.bossName} has no possible connectors");
            }
            
            if (bossPath.requiredConnections > bossPath.possibleConnectors?.Count)
            {
                LogError($"Boss path {bossPath.bossName} requires more connections than available connectors");
            }
        }
        
        private void LogInfo(string message)
        {
            if (enableDebugLogging)
                Debug.Log($"[ProceduralPathTransitions] {message}");
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[ProceduralPathTransitions] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[ProceduralPathTransitions] {message}");
        }
    }
    
    /// <summary>
    /// Configuration for a boss path with procedural connector selection
    /// </summary>
    [System.Serializable]
    public class BossPath
    {
        [Header("Boss Configuration")]
        public string bossName;              // "Pride", "Wrath", etc.
        public string bossSceneName;         // "Cathedral", "Necropolis", etc.
        public List<string> possibleConnectors; // Pool of connector scenes
        public int requiredConnections = 2;  // Number of connectors before boss
        public PathTheme pathTheme;          // Visual/gameplay theme
        
        [Header("Unlock Requirements")]
        public bool isFinaleUnlocked = true;
        public string[] unlockRequirements;  // Required defeated bosses
        public float difficultyModifier = 1.0f;
        
        [Header("Generated Path Data")]
        public int pathSeed;                 // Seed used for generation
        public List<string> selectedConnectors; // Actually selected connectors
        public List<MapNode> pathNodes;      // Full path for SceneRouter
        
        public BossPath()
        {
            possibleConnectors = new List<string>();
            selectedConnectors = new List<string>();
            pathNodes = new List<MapNode>();
            unlockRequirements = new string[0];
        }
    }
    
    /// <summary>
    /// Path themes for visual and gameplay variety
    /// </summary>
    public enum PathTheme
    {
        Divine,      // Cathedral/Pride - holy architecture
        Combat,      // Necropolis/Wrath - battle-focused
        Treasure,    // Vault/Greed - wealth and riches
        Illusion,    // Mirage/Envy - deceptive visuals
        Natural,     // Garden/Lust - organic environments
        Indulgence,  // Banquet/Gluttony - excess and luxury
        Laziness,    // Lounge/Sloth - comfort and slowing
        Finale       // Devil - ultimate challenge
    }
    
    /// <summary>
    /// Save state for path progress persistence
    /// </summary>
    [System.Serializable]
    public class BossPathSaveState
    {
        [Header("Path State")]
        public string currentBossPath;       // Currently active boss path
        public int currentSceneIndex;        // Progress within the path
        public List<string> completedBosses; // Defeated bosses
        public int pathSeed;                 // Seed for deterministic regeneration
        public DateTime saveTimestamp;       // When this state was saved
        
        [Header("Player Progress")]
        public PlayerProgressData playerProgress; // Player state data
        
        public BossPathSaveState()
        {
            completedBosses = new List<string>();
            playerProgress = new PlayerProgressData();
        }
    }
    
    /// <summary>
    /// Player progress data for save states
    /// </summary>
    [System.Serializable]
    public class PlayerProgressData
    {
        public int health = 100;
        public int currency = 0;
        public string currentScene = "";
        // Additional player progress fields can be added here
    }
}