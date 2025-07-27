using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using NeonLadder.Managers.Optimization;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace NeonLadder.Managers
{
    public class ManagerController : MonoBehaviour
    {
        public static ManagerController Instance;
        [SerializeField] private bool dontDestroyOnLoad = true;

        private Scenes scene;
        
        // Performance optimization: Replace per-frame string comparisons with efficient scene detection
        private SceneChangeDetector sceneChangeDetector;
        private ISceneProvider sceneProvider;
        private bool useOptimizedSceneDetection = true;
        
        // Events for TDD testing
        public event Action OnStringComparisonPerformed;
        public event Action<Scenes, Scenes> OnSceneChangeDetected;
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
        // public SteamManager steamManager;

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
            // Initialize optimized scene detection
            sceneProvider = new UnitySceneProvider();
            sceneChangeDetector = new SceneChangeDetector(sceneProvider);
            sceneChangeDetector.OnSceneChangeDetected += HandleSceneChange;
            
            scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
            ToggleManagers();
        }

        public void Update()
        {
            if (useOptimizedSceneDetection)
            {
                if (sceneChangeDetector != null)
                {
                    // OPTIMIZED: Use efficient scene change detection instead of per-frame string comparison
                    if (sceneChangeDetector.HasSceneChanged())
                    {
                        var newSceneName = sceneChangeDetector.GetCachedSceneName();
                        var oldScene = scene;
                        scene = SceneEnumResolver.Resolve(newSceneName);
                        
                        OnSceneChangeDetected?.Invoke(oldScene, scene);
                        ToggleManagers();
                    }
                }
                // When in optimized mode but detector is null (like in tests), 
                // don't perform any scene change detection to avoid string comparisons
            }
            else
            {
                // LEGACY: Original per-frame string comparison (for performance comparison)
                OnStringComparisonPerformed?.Invoke();
                if (SceneManager.GetActiveScene().name != scene.ToString())
                {
                    scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
                    ToggleManagers();
                }
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
            //steamManager = GetComponentInChildren<SteamManager>();
            sceneCycleManager = GetComponentInChildren<SceneCycleManager>();
        }

        public void ToggleManagers()
        {
            if (eventManager != null)
                eventManager.enabled = true; 
            //steamManager.enabled = true;
            switch (scene)
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
        
        #region TDD Test Support Methods
        
        /// <summary>
        /// Handles scene change events from the optimized detector
        /// </summary>
        private void HandleSceneChange(string previousSceneName, string newSceneName)
        {
            // This method is called by the SceneChangeDetector when a change is detected
            // The actual scene update logic is handled in Update() method
        }
        
        /// <summary>
        /// Enables or disables optimized scene detection (for testing)
        /// </summary>
        public void EnableOptimizedSceneDetection(bool enabled)
        {
            useOptimizedSceneDetection = enabled;
        }
        
        /// <summary>
        /// Gets the cached scene name from the detector (for testing)
        /// </summary>
        public string GetCachedSceneName()
        {
            return sceneChangeDetector?.GetCachedSceneName();
        }
        
        /// <summary>
        /// Sets the current scene for testing purposes
        /// </summary>
        public void SetCurrentScene(Scenes newScene)
        {
            scene = newScene;
        }
        
        /// <summary>
        /// Gets the current scene enum value
        /// </summary>
        public Scenes GetCurrentScene()
        {
            return scene;
        }
        
        /// <summary>
        /// Simulates a scene change for testing
        /// </summary>
        public void SimulateSceneChange(Scenes fromScene, Scenes toScene)
        {
            var oldScene = scene;
            scene = toScene;
            OnSceneChangeDetected?.Invoke(oldScene, toScene);
            ToggleManagers();
        }
        
        #endregion
    }
}
