using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using NeonLadder.Managers;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Optimized version of ManagerController with zero per-frame string comparisons.
    /// Uses cached scene indices and event-driven architecture for maximum performance.
    /// </summary>
    public class OptimizedManagerController : MonoBehaviour
    {
        public static OptimizedManagerController Instance;
        [SerializeField] private bool dontDestroyOnLoad = true;

        // Cached scene data - no strings in Update loop
        private int currentSceneIndex = -1;
        private Scenes currentSceneEnum = Scenes.Title;
        private bool sceneChanged = false;
        
        // Performance tracking
        private int framesSinceLastCheck = 0;
        private const int SCENE_CHECK_INTERVAL = 30; // Check every 30 frames instead of every frame
        
        // Manager references
        public EnemyDefeatedManager enemyDefeatedManager;
        public DialogueManager dialogueManager;
        public SceneExitAssignmentManager sceneExitAssignmentManager;
        public LootDropManager lootDropManager;
        public LootPurchaseManager lootPurchaseManager;
        public MonsterGroupActivationManager monsterGroupActivationManager;
        public PlayerCameraPositionManager playerCameraPositionManager;
        public GameControllerManager gameControllerManager;
        public SceneCycleManager sceneCycleManager;
        public SceneChangeManager sceneChangeManager;
        public EventManager eventManager;
        
        // Events for performance monitoring
        public event Action<Scenes, Scenes> OnSceneChangeDetected;
        public event Action<float> OnPerformanceMetricUpdated;
        
        // Performance metrics
        private float lastUpdateTime = 0f;
        private float averageUpdateTime = 0f;
        private int updateCount = 0;

        void Awake()
        {
            InitializeChildComponents();

            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                if (dontDestroyOnLoad)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
        }

        void Start()
        {
            // Cache initial scene index - no string operations
            currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            currentSceneEnum = GetSceneEnumFromIndex(currentSceneIndex);
            
            // Subscribe to scene change events instead of polling
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            ToggleManagers();
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void Update()
        {
            // OPTIMIZED: No string comparisons at all
            // Only check for scene changes at intervals, not every frame
            framesSinceLastCheck++;
            
            if (framesSinceLastCheck >= SCENE_CHECK_INTERVAL)
            {
                framesSinceLastCheck = 0;
                CheckSceneChangeOptimized();
            }
            
            // Process any pending scene change
            if (sceneChanged)
            {
                sceneChanged = false;
                ToggleManagers();
            }
            
            // Track performance metrics
            TrackUpdatePerformance();
        }
        
        /// <summary>
        /// Optimized scene change check using integer comparison only
        /// </summary>
        private void CheckSceneChangeOptimized()
        {
            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
            
            // Integer comparison is much faster than string comparison
            if (activeSceneIndex != currentSceneIndex)
            {
                var oldScene = currentSceneEnum;
                currentSceneIndex = activeSceneIndex;
                currentSceneEnum = GetSceneEnumFromIndex(currentSceneIndex);
                
                OnSceneChangeDetected?.Invoke(oldScene, currentSceneEnum);
                sceneChanged = true;
            }
        }
        
        /// <summary>
        /// Event-driven scene change handler (most efficient)
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // This is called automatically by Unity when scene changes
            // No polling required!
            var oldScene = currentSceneEnum;
            currentSceneIndex = scene.buildIndex;
            currentSceneEnum = GetSceneEnumFromIndex(currentSceneIndex);
            
            OnSceneChangeDetected?.Invoke(oldScene, currentSceneEnum);
            ToggleManagers();
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            // Handle scene unload if needed
        }
        
        /// <summary>
        /// Convert scene index to enum without string operations
        /// </summary>
        private Scenes GetSceneEnumFromIndex(int buildIndex)
        {
            // Map build indices to scene enums
            // This mapping should be maintained when scenes are added/removed
            switch (buildIndex)
            {
                case 0: return Scenes.Title;
                case 1: return Scenes.Staging;
                case 2: return Scenes.Start;
                case 3: return Scenes.Recife_2050_Final;
                case 4: return Scenes.MetaShop;
                case 5: return Scenes.PermaShop;
                default:
                    // For any unmapped scenes, we can fall back to a single string operation
                    // But this only happens once per scene change, not per frame
                    return SceneEnumResolver.Resolve(SceneManager.GetSceneByBuildIndex(buildIndex).name);
            }
        }

        private void InitializeChildComponents()
        {
            enemyDefeatedManager = GetComponentInChildren<EnemyDefeatedManager>();
            sceneExitAssignmentManager = GetComponentInChildren<SceneExitAssignmentManager>();
            lootDropManager = GetComponentInChildren<LootDropManager>();
            lootPurchaseManager = GetComponentInChildren<LootPurchaseManager>();
            playerCameraPositionManager = GetComponentInChildren<PlayerCameraPositionManager>();
            gameControllerManager = GetComponentInChildren<GameControllerManager>();
            sceneChangeManager = GetComponentInChildren<SceneChangeManager>();
            dialogueManager = GetComponentInChildren<DialogueManager>();
            eventManager = GetComponentInChildren<EventManager>();
            monsterGroupActivationManager = GetComponentInChildren<MonsterGroupActivationManager>();
            sceneCycleManager = GetComponentInChildren<SceneCycleManager>();
        }

        public void ToggleManagers()
        {
            if (eventManager != null)
                eventManager.enabled = true;
                
            switch (currentSceneEnum)
            {
                case Scenes.Title:
                    if (gameControllerManager != null)
                        gameControllerManager.enabled = true;
                    break;
                    
                case Scenes.Staging:
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = true;
                    if (playerCameraPositionManager != null)
                    {
                        playerCameraPositionManager.enabled = true;
                        playerCameraPositionManager.EmptySceneStates();
                    }
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = false;
                    }
                    break;
                    
                case Scenes.Start:
                case Scenes.Recife_2050_Final:
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = true;
                    }
                    if (lootDropManager != null)
                        lootDropManager.enabled = true;
                    break;
                    
                case Scenes.MetaShop:
                    if (lootDropManager != null)
                        lootDropManager.enabled = false;
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = true;
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = false;
                    }
                    break;
                    
                case Scenes.PermaShop:
                    if (lootDropManager != null)
                        lootDropManager.enabled = false;
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = true;
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = false;
                    }
                    break;
                    
                default:
                    break;
            }
        }
        
        /// <summary>
        /// Track Update() performance for monitoring
        /// </summary>
        private void TrackUpdatePerformance()
        {
            float currentTime = Time.realtimeSinceStartup;
            float deltaTime = currentTime - lastUpdateTime;
            
            if (lastUpdateTime > 0)
            {
                updateCount++;
                averageUpdateTime = ((averageUpdateTime * (updateCount - 1)) + deltaTime) / updateCount;
                
                // Report performance metrics every 1000 frames
                if (updateCount % 1000 == 0)
                {
                    OnPerformanceMetricUpdated?.Invoke(averageUpdateTime * 1000f); // Convert to milliseconds
                }
            }
            
            lastUpdateTime = currentTime;
        }
        
        #region Public API
        
        /// <summary>
        /// Gets the current scene enum value
        /// </summary>
        public Scenes GetCurrentScene()
        {
            return currentSceneEnum;
        }
        
        /// <summary>
        /// Force a scene change check (for testing)
        /// </summary>
        public void ForceSceneCheck()
        {
            CheckSceneChangeOptimized();
        }
        
        /// <summary>
        /// Get average Update() execution time in milliseconds
        /// </summary>
        public float GetAverageUpdateTime()
        {
            return averageUpdateTime * 1000f;
        }
        
        /// <summary>
        /// Reset performance metrics
        /// </summary>
        public void ResetPerformanceMetrics()
        {
            averageUpdateTime = 0f;
            updateCount = 0;
            lastUpdateTime = 0f;
        }
        
        #endregion
    }
}