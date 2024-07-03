using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.Managers
{
    public class ManagerController : MonoBehaviour
    {
        private static ManagerController Instance;
        [SerializeField] private bool dontDestroyOnLoad = true;

        private Scenes scene;
        private EnemyDefeatedManager enemyDefeatedManager;
        private DialogueManager dialogueManager;
        private SceneExitAssignmentManager sceneExitAssignmentManager;
        private LootDropManager lootDropManager;
        private LootPurchaseManager lootPurchaseManager;
        private MonsterGroupActivationManager monsterGroupActivationManager;
        private PlayerCameraPositionManager playerCameraPositionManager;
        private GameControllerManager gameControllerManager;
        private SceneChangeManager sceneChangeManager;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {

                enemyDefeatedManager = GetComponentInChildren<EnemyDefeatedManager>();
                sceneExitAssignmentManager = GetComponentInChildren<SceneExitAssignmentManager>();
                lootDropManager = GetComponentInChildren<LootDropManager>();
                lootPurchaseManager = GetComponentInChildren<LootPurchaseManager>();
                monsterGroupActivationManager = GetComponentInChildren<MonsterGroupActivationManager>();
                playerCameraPositionManager = GetComponentInChildren<PlayerCameraPositionManager>();
                gameControllerManager = GetComponentInChildren<GameControllerManager>();
                sceneChangeManager = GetComponentInChildren<SceneChangeManager>();
                dialogueManager = GetComponentInChildren<DialogueManager>();

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

        public void ToggleManagers()
        {
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