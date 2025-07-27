using Unity.Cinemachine;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.UI;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Managers
{
    public class GameControllerManager : MonoBehaviour
{
    private Game gameController;
    private Player player;
    private PlayerAction playerActions;
    private Scenes scene;

    // Start is called before the first frame update
    void Start()
    {
        scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
        InitializeControllers();
        AdjustControllersBasedOnScene();
    }

    private void Awake()
    {
        enabled = false;
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != scene.ToString())
        {
            scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
            InitializeControllers();
            AdjustControllersBasedOnScene();
        }
    }

    private void InitializeControllers()
    {
        if (scene == Scenes.Title)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag(Tags.Player.ToString()).GetComponentInChildren<Player>();
                if (player == null)
                {
                    Debug.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null && player != null)
            {
                playerActions = player.GetComponent<PlayerAction>();
                if (playerActions == null)
                {
                    Debug.LogError("PlayerAction not found in scene.");
                }
            }
        }

        else if (scene != Scenes.Credits)
        {
            if (gameController == null)
            {
                var gameControllerObj = GameObject.FindGameObjectWithTag(Tags.GameController.ToString());
                if (gameControllerObj != null)
                {
                    gameController = gameControllerObj.GetComponent<Game>();
                }
                if (gameController == null)
                {
                    Debug.LogError("GameController not found in scene.");
                }
            }

            if (player == null && gameController != null)
            {
                player = gameController.GetComponentInChildren<Player>();
                if (player == null)
                {
                    Debug.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null && player != null)
            {
                playerActions = player.GetComponent<PlayerAction>();
                if (playerActions == null)
                {
                    Debug.LogError("PlayerAction not found in scene.");
                }
            }
        }
    }

    public void AdjustControllersBasedOnScene()
    {
        switch (scene)
        {
            case Scenes.Title:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
                playerActions.enabled = false;
                break;
            case Scenes.Staging:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = true;
                Schedule<LoadGame>();
                Game.Instance.model.VirtualCamera.GetComponent<CinemachinePositionComposer>().CameraDistance = 6;
                gameController.gameObject.GetComponent<MetaGameController>().enabled = false;
                gameController.gameObject.GetComponent<MetaGameController>().enabled = true;
                break;
            case Scenes.ReturnToStaging:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
                break;
            case Scenes.Start:
            case Scenes.SidePath1:
                Game.Instance.model.VirtualCamera.GetComponent<CinemachinePositionComposer>().CameraDistance = 6;
                break;
            case Scenes.MetaShop:
                Game.Instance.model.VirtualCamera.GetComponent<CinemachinePositionComposer>().CameraDistance = 2.2f;
                break;
            case Scenes.PermaShop:
                Game.Instance.model.VirtualCamera.GetComponent<CinemachinePositionComposer>().CameraDistance = 2.2f;
                break;
            case Scenes.Credits:
                break;
            default:
                playerActions.enabled = true;
                gameController.gameObject.SetActive(true);
                Game.Instance.model.VirtualCamera.GetComponent<CinemachinePositionComposer>().CameraDistance = 6;
                break;
        }
    }
}
}
