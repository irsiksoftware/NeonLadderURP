using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.Managers
{
    public class ManagerController : MonoBehaviour
    {
        public static ManagerController Instance;
        [SerializeField] private bool dontDestroyOnLoad = true;

        private Scenes scene;
        public EnemyDefeatedManager enemyDefeatedManager;
        public DialogueManager dialogueManager;
        public SceneExitAssignmentManager sceneExitAssignmentManager;
        public LootDropManager lootDropManager;
        public LootPurchaseManager lootPurchaseManager;
        public MonsterGroupActivationManager monsterGroupActivationManager;
        public PlayerCameraPositionManager playerCameraPositionManager;
        public GameControllerManager gameControllerManager;
        public SceneChangeManager sceneChangeManager;
        public EventManager eventManager;

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
            scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
            ToggleManagers();
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != scene.ToString())
            {
                scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
                ToggleManagers();
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
        }

        public void ToggleManagers()
        {
            eventManager.enabled = true;
            switch (scene)
            {
                case Scenes.Title:
                    gameControllerManager.enabled = true;
                    break;
                case Scenes.Staging:
                    lootDropManager.enabled = false;
                    playerCameraPositionManager.enabled = true;
                    playerCameraPositionManager.EmptySceneStates();
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = false;
                    }
                    break;
                case Scenes.Start:
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = true;
                    }
                    lootDropManager.enabled = true;
                    break;
                case Scenes.MetaShop:
                    lootDropManager.enabled = false;
                    lootPurchaseManager.enabled = true;
                    if (monsterGroupActivationManager != null)
                    {
                        monsterGroupActivationManager.enabled = false;
                    }
                    break;
                case Scenes.PermaShop:
                    lootDropManager.enabled = false;
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
    }
}
