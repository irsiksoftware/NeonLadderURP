using Cinemachine;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.UI;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;
using static NeonLadder.Core.Simulation;

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

    // Update is called once per frame
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
                player = GameObject.FindGameObjectWithTag(Tags.Player.ToString()).GetComponent<Player>();
                if (player == null)
                {
                    Debug.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null)
            {
                playerActions = player.GetComponentInChildren<PlayerAction>();
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

            if (player == null)
            {
                player = gameController.GetComponentInChildren<Player>();
                if (player == null)
                {
                    Debug.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null)
            {
                playerActions = player.GetComponentInChildren<PlayerAction>();
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
                player.GetComponentInChildren<Canvas>().enabled = false;
                playerActions.enabled = false;
                //gameController.gameObject.GetComponentInChildren<Camera>().enabled = false;
                if (gameController != null)
                {
                    gameController.gameObject.SetActive(false);
                }
                break;
            case Scenes.Staging:
                //dynamicCameraAdjustment.enabled = false;
                //set framingTransposer follow distance to 6
                player.GetComponentInChildren<Canvas>().enabled = true;
                Schedule<LoadGame>();
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 6;
                gameController.gameObject.GetComponent<MetaGameController>().enabled = false;
                gameController.gameObject.GetComponent<MetaGameController>().enabled = true;
                break;
            case Scenes.ReturnToStaging:
                player.GetComponentInChildren<Canvas>().enabled = false;
                break;
            case Scenes.Start:
                //dynamicCameraAdjustment.enabled = true;
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 6;
                break;
            case Scenes.MetaShop:
                // Adjust components for MetaShop scene
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 2.2f;
                break;
            case Scenes.PermaShop:
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 2.2f;
                // Adjust components for PermaShop scene
                break;
            case Scenes.Credits:
                // Adjust components for Credits scene
                break;
            default:
                playerActions.enabled = true;
                gameController.gameObject.SetActive(true);
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 6;
                break;
        }
    }
}
