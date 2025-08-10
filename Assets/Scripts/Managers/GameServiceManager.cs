using NeonLadder.Core.ServiceContainer;
using NeonLadder.Core.ServiceContainer.Services;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using NeonLadder.Managers;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Manages game services using dependency injection pattern.
    /// Replaces singleton ManagerController with service container architecture.
    /// </summary>
    public class GameServiceManager : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        
        private ServiceLocator serviceContainer;
        private Scenes currentScene;
        private SceneChangeDetector sceneChangeDetector;
        private ISceneProvider sceneProvider;
        
        // Manager references (will be registered as services)
        private EnemyDefeatedManager enemyDefeatedManager;
        private DialogueManager dialogueManager;
        private SceneExitAssignmentManager sceneExitAssignmentManager;
        private LootDropManager lootDropManager;
        private LootPurchaseManager lootPurchaseManager;
        private MonsterGroupActivationManager monsterGroupActivationManager;
        private PlayerCameraPositionManager playerCameraPositionManager;
        private GameControllerManager gameControllerManager;
        private SceneCycleManager sceneCycleManager;
        private SceneChangeManager sceneChangeManager;
        private EventManager eventManager;
        
        // Events for state changes
        public event Action<Scenes, Scenes> OnSceneChangeDetected;
        
        /// <summary>
        /// Gets the service container for accessing registered services.
        /// </summary>
        public IServiceContainer Services => serviceContainer;
        
        void Awake()
        {
            // Initialize service container
            serviceContainer = ServiceLocator.Instance;
            
            // Initialize child components
            InitializeChildComponents();
            
            // Register this manager with the service container
            serviceContainer.Register<GameServiceManager>(this);
            
            // Register individual services
            RegisterServices();
            
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
            ToggleManagers();
        }
        
        void Update()
        {
            if (sceneChangeDetector != null)
            {
                if (sceneChangeDetector.HasSceneChanged())
                {
                    var newSceneName = sceneChangeDetector.GetCachedSceneName();
                    var oldScene = currentScene;
                    currentScene = SceneEnumResolver.Resolve(newSceneName);
                    
                    OnSceneChangeDetected?.Invoke(oldScene, currentScene);
                    ToggleManagers();
                }
            }
        }
        
        void OnDestroy()
        {
            // Clean up services when destroyed
            UnregisterServices();
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
        
        private void RegisterServices()
        {
            // Register scene service
            var sceneService = new SceneServiceImpl(
                sceneChangeManager,
                sceneCycleManager,
                sceneExitAssignmentManager,
                playerCameraPositionManager,
                () => currentScene,
                ChangeScene
            );
            serviceContainer.Register<ISceneService>(sceneService);
            
            // Register enemy service
            if (enemyDefeatedManager != null)
            {
                var enemyService = new EnemyServiceImpl(enemyDefeatedManager);
                serviceContainer.Register<IEnemyService>(enemyService);
            }
            
            // Register dialogue service
            if (dialogueManager != null)
            {
                var dialogueService = new DialogueServiceImpl(dialogueManager);
                serviceContainer.Register<IDialogueService>(dialogueService);
            }
            
            // Register loot service
            if (lootDropManager != null || lootPurchaseManager != null)
            {
                var lootService = new LootServiceImpl(lootDropManager, lootPurchaseManager);
                serviceContainer.Register<ILootService>(lootService);
            }
            
            // Register event manager directly
            if (eventManager != null)
            {
                serviceContainer.Register<EventManager>(eventManager);
            }
            
            // Register game controller manager
            if (gameControllerManager != null)
            {
                serviceContainer.Register<GameControllerManager>(gameControllerManager);
            }
            
            // Register monster group activation manager
            if (monsterGroupActivationManager != null)
            {
                serviceContainer.Register<MonsterGroupActivationManager>(monsterGroupActivationManager);
            }
        }
        
        private void UnregisterServices()
        {
            // Unregister all services
            serviceContainer.Unregister<ISceneService>();
            serviceContainer.Unregister<IEnemyService>();
            serviceContainer.Unregister<IDialogueService>();
            serviceContainer.Unregister<ILootService>();
            serviceContainer.Unregister<EventManager>();
            serviceContainer.Unregister<GameControllerManager>();
            serviceContainer.Unregister<MonsterGroupActivationManager>();
            serviceContainer.Unregister<GameServiceManager>();
        }
        
        private void HandleSceneChange(string previousSceneName, string newSceneName)
        {
            // Scene change is handled in Update method
        }
        
        private void ChangeScene(Scenes newScene)
        {
            currentScene = newScene;
            ToggleManagers();
        }
        
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
        /// Gets the current scene.
        /// </summary>
        public Scenes GetCurrentScene()
        {
            return currentScene;
        }
    }
    
    // Service implementations
    internal class SceneServiceImpl : ISceneService
    {
        private readonly Func<Scenes> getCurrentScene;
        private readonly Action<Scenes> changeScene;
        
        public SceneChangeManager SceneChangeManager { get; }
        public SceneCycleManager SceneCycleManager { get; }
        public SceneExitAssignmentManager SceneExitAssignmentManager { get; }
        public PlayerCameraPositionManager PlayerCameraPositionManager { get; }
        
        public bool IsInitialized { get; private set; }
        
        public Scenes CurrentScene => getCurrentScene();
        
        public SceneServiceImpl(
            SceneChangeManager sceneChangeManager,
            SceneCycleManager sceneCycleManager,
            SceneExitAssignmentManager sceneExitAssignmentManager,
            PlayerCameraPositionManager playerCameraPositionManager,
            Func<Scenes> getCurrentScene,
            Action<Scenes> changeScene)
        {
            SceneChangeManager = sceneChangeManager;
            SceneCycleManager = sceneCycleManager;
            SceneExitAssignmentManager = sceneExitAssignmentManager;
            PlayerCameraPositionManager = playerCameraPositionManager;
            this.getCurrentScene = getCurrentScene;
            this.changeScene = changeScene;
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
        }
        
        public void ChangeScene(Scenes newScene)
        {
            changeScene(newScene);
        }
    }
    
    internal class EnemyServiceImpl : IEnemyService
    {
        public EnemyDefeatedManager Manager { get; }
        public bool IsInitialized { get; private set; }
        
        public EnemyServiceImpl(EnemyDefeatedManager manager)
        {
            Manager = manager;
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
        }
    }
    
    internal class DialogueServiceImpl : IDialogueService
    {
        public DialogueManager Manager { get; }
        public bool IsInitialized { get; private set; }
        
        public DialogueServiceImpl(DialogueManager manager)
        {
            Manager = manager;
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
        }
    }
    
    internal class LootServiceImpl : ILootService
    {
        public LootDropManager LootDropManager { get; }
        public LootPurchaseManager LootPurchaseManager { get; }
        public bool IsInitialized { get; private set; }
        
        public LootServiceImpl(LootDropManager lootDropManager, LootPurchaseManager lootPurchaseManager)
        {
            LootDropManager = lootDropManager;
            LootPurchaseManager = lootPurchaseManager;
        }
        
        public void Initialize()
        {
            IsInitialized = true;
        }
        
        public void Shutdown()
        {
            IsInitialized = false;
        }
    }
}