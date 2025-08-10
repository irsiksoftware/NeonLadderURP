using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using NeonLadder.Core.ServiceContainer;
using NeonLadder.Core.ServiceContainer.Services;
using NeonLadder.Debugging;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Refactored ManagerController that uses ServiceContainer instead of singleton pattern.
    /// This provides better testability, loose coupling, and dependency injection.
    /// </summary>
    public class RefactoredManagerController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool useServiceContainer = true;
        
        [Header("Manager References")]
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
        
        private ServiceContainer serviceContainer;
        private Scenes currentScene;
        private SceneChangeDetector sceneChangeDetector;
        private ISceneProvider sceneProvider;
        
        // Events for compatibility
        public event Action OnStringComparisonPerformed;
        public event Action<Scenes, Scenes> OnSceneChangeDetected;
        
        // Legacy singleton for backward compatibility during migration
        public static ManagerController Instance { get; private set; }
        
        void Awake()
        {
            // Initialize child components
            InitializeChildComponents();
            
            if (useServiceContainer)
            {
                InitializeServiceContainer();
            }
            else
            {
                // Legacy singleton behavior for backward compatibility
                if (Instance != null && Instance != this)
                {
                    Destroy(gameObject);
                    return;
                }
                Instance = this as ManagerController;
            }
            
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        
        void Start()
        {
            // Initialize scene detection
            sceneProvider = new UnitySceneProvider();
            sceneChangeDetector = new SceneChangeDetector(sceneProvider);
            sceneChangeDetector.OnSceneChangeDetected += HandleSceneChange;
            
            currentScene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
            
            if (useServiceContainer)
            {
                // Notify service container of initial scene
                serviceContainer.NotifySceneChange("", currentScene.ToString());
            }
            else
            {
                // Legacy manager toggling
                ToggleManagers();
            }
        }
        
        void Update()
        {
            // Check for scene changes using optimized detection
            if (sceneChangeDetector != null && sceneChangeDetector.HasSceneChanged())
            {
                var newSceneName = sceneChangeDetector.GetCachedSceneName();
                var oldScene = currentScene;
                currentScene = SceneEnumResolver.Resolve(newSceneName);
                
                OnSceneChangeDetected?.Invoke(oldScene, currentScene);
                
                if (useServiceContainer)
                {
                    // Notify service container of scene change
                    serviceContainer.NotifySceneChange(oldScene.ToString(), currentScene.ToString());
                }
                else
                {
                    // Legacy manager toggling
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
            sceneCycleManager = GetComponentInChildren<SceneCycleManager>();
        }
        
        private void InitializeServiceContainer()
        {
            // Get or create service container
            serviceContainer = ServiceContainer.Instance;
            
            // Register all manager services
            RegisterManagerServices();
            
            // Initialize all services
            serviceContainer.InitializeServices();
            
            Debugger.Log("[RefactoredManagerController] Service container initialized with all managers");
        }
        
        private void RegisterManagerServices()
        {
            // Register core services
            if (eventManager != null)
            {
                serviceContainer.Register<IEventService, EventService>(new EventService(eventManager));
            }
            
            if (enemyDefeatedManager != null)
            {
                serviceContainer.Register<IEnemyDefeatedService, EnemyDefeatedService>(
                    new EnemyDefeatedService(enemyDefeatedManager));
            }
            
            if (dialogueManager != null)
            {
                serviceContainer.Register<IDialogueService, DialogueService>(
                    new DialogueService(dialogueManager));
            }
            
            if (lootDropManager != null)
            {
                serviceContainer.Register<ILootService, LootDropService>(
                    new LootDropService(lootDropManager, lootPurchaseManager));
            }
            
            if (playerCameraPositionManager != null)
            {
                serviceContainer.Register<ICameraService, CameraPositionService>(
                    new CameraPositionService(playerCameraPositionManager));
            }
            
            if (gameControllerManager != null)
            {
                serviceContainer.Register<IGameControllerService, GameControllerService>(
                    new GameControllerService(gameControllerManager));
            }
            
            if (sceneChangeManager != null)
            {
                serviceContainer.Register<ISceneChangeService, SceneChangeService>(
                    new SceneChangeService(sceneChangeManager, sceneExitAssignmentManager, sceneCycleManager));
            }
            
            if (monsterGroupActivationManager != null)
            {
                serviceContainer.Register<IMonsterGroupService, MonsterGroupService>(
                    new MonsterGroupService(monsterGroupActivationManager));
            }
        }
        
        // Legacy method for backward compatibility
        public void ToggleManagers()
        {
            if (eventManager != null)
                eventManager.enabled = true;
                
            switch (currentScene)
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
                        monsterGroupActivationManager.enabled = false;
                    break;
                    
                case Scenes.Start:
                case Scenes.Recife_2050_Final:
                    if (monsterGroupActivationManager != null)
                        monsterGroupActivationManager.enabled = true;
                    if (lootDropManager != null)
                        lootDropManager.enabled = true;
                    break;
                    
                case Scenes.MetaShop:
                case Scenes.PermaShop:
                    if (lootDropManager != null)
                        lootDropManager.enabled = false;
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = true;
                    if (monsterGroupActivationManager != null)
                        monsterGroupActivationManager.enabled = false;
                    break;
                    
                default:
                    break;
            }
        }
        
        private void HandleSceneChange(string previousSceneName, string newSceneName)
        {
            // Scene change is handled in Update method
        }
        
        private void OnDestroy()
        {
            if (useServiceContainer && serviceContainer != null)
            {
                // Service container handles its own cleanup
                Debugger.Log("[RefactoredManagerController] Shutting down...");
            }
            
            if (Instance == this as ManagerController)
            {
                Instance = null;
            }
        }
        
        #region Public API for Service Access
        
        /// <summary>
        /// Get a service from the container.
        /// </summary>
        public T GetService<T>() where T : class, IService
        {
            if (!useServiceContainer)
            {
                Debugger.LogWarning("[RefactoredManagerController] Service container not enabled");
                return null;
            }
            
            return serviceContainer.Get<T>();
        }
        
        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public bool HasService<T>() where T : class, IService
        {
            if (!useServiceContainer)
                return false;
                
            return serviceContainer.Has<T>();
        }
        
        #endregion
        
        #region Compatibility Methods
        
        public Scenes GetCurrentScene()
        {
            return currentScene;
        }
        
        public void SetCurrentScene(Scenes newScene)
        {
            currentScene = newScene;
        }
        
        #endregion
    }
}