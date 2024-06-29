using Cinemachine;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerManager : MonoBehaviour
{
    private Game gameController;
    //private DynamicCameraAdjustment dynamicCameraAdjustment;
    private Player player;
    private PlayerAction playerActions;
    private GameObject statUI;
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

            if (statUI == null)
            {
                statUI = player.GetComponentInChildren<StatUI>().gameObject;
                if (statUI == null)
                {
                    Debug.LogError("StatUI not found in scene.");
                }
            }
        }

        else
        {
            if (gameController == null)
            {
                gameController = GameObject.FindGameObjectWithTag(Tags.GameController.ToString()).GetComponent<Game>();
                if (gameController == null)
                {
                    Debug.LogError("GameController not found in scene.");
                }
            }

            //if (dynamicCameraAdjustment == null)
            //{
            //    dynamicCameraAdjustment = gameController.GetComponentInChildren<DynamicCameraAdjustment>();
            //    if (dynamicCameraAdjustment == null)
            //    {
            //        Debug.LogError("DynamicCameraAdjustment not found in scene.");
            //    }
            //}

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
                player.GetComponentInChildren<StatUI>().gameObject.SetActive(false);
                playerActions.enabled = false;
                break;
            case Scenes.Staging:
                //dynamicCameraAdjustment.enabled = false;
                //set framingTransposer follow distance to 6
                player.GetComponentInChildren<StatUI>().gameObject.SetActive(true);
                player.animator.SetInteger("locomotion_animation", 777); //escape death animation for webgl attempt.
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 6;
                break;
            case Scenes.ReturnToStaging:
                player.GetComponentInChildren<StatUI>().gameObject.SetActive(false);
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
            default:
                playerActions.enabled = true;
                gameController.gameObject.SetActive(true);
                Game.Instance.model.VirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_CameraDistance = 6;
                break;
        }
    }
}
