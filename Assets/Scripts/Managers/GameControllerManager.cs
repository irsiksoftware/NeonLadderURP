using NeonLadder.Debugging;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.UI;
using NeonLadder.Utilities;
using Unity.Cinemachine;
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
    private string scene;

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
        if (SceneManager.GetActiveScene().name != scene)
        {
            scene = SceneEnumResolver.Resolve(SceneManager.GetActiveScene().name);
            InitializeControllers();
            AdjustControllersBasedOnScene();
        }
    }

    private void InitializeControllers()
    {
        if (scene == Scenes.Core.Title)
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag(Tags.Player.ToString()).GetComponentInChildren<Player>();
                if (player == null)
                {
                    Debugger.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null && player != null)
            {
                playerActions = player.GetComponent<PlayerAction>();
                if (playerActions == null)
                {
                    Debugger.LogError("PlayerAction not found in scene.");
                }
            }
        }

        else if (scene != Scenes.Core.Credits)
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
                    Debugger.LogError("GameController not found in scene.");
                }
            }

            if (player == null && gameController != null)
            {
                player = gameController.GetComponentInChildren<Player>();
                if (player == null)
                {
                    Debugger.LogError("Player not found in scene.");
                }
            }

            if (playerActions == null && player != null)
            {
                playerActions = player.GetComponent<PlayerAction>();
                if (playerActions == null)
                {
                    Debugger.LogError("PlayerAction not found in scene.");
                }
            }
        }
    }

    public void AdjustControllersBasedOnScene()
    {
        switch (scene)
        {
            case var s when s == Scenes.Core.Title:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
                playerActions.enabled = false;
                break;
            case var s when s == Scenes.Core.Staging:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = true;
                Schedule<LoadGame>();
                gameController.gameObject.GetComponent<MetaGameController>().enabled = false;
                gameController.gameObject.GetComponent<MetaGameController>().enabled = true;
                break;
            case var s when s == Scenes.Cutscene.BossDefeated:
            case var s2 when s2 == Scenes.Cutscene.Death:
                player.transform.parent.GetComponentInChildren<Canvas>().enabled = false;
                break;
            case var s when s == Scenes.Core.Start:
            case var s2 when s2 == Scenes.Legacy.SidePath1:
                break;
            case var s when s == Scenes.Core.MetaShop:
                break;
            case var s when s == Scenes.Core.PermaShop:
                break;
            case var s when s == Scenes.Core.Credits:
                break;
            default:
                //playerActions.enabled = true;
                //gameController.gameObject.SetActive(true);
                break;
        }
        
        // Apply camera configuration for the current scene
        ApplyCameraConfiguration();
    }
    
    private void ApplyCameraConfiguration()
    {
        if (Game.Instance == null || Game.Instance.model == null) return;
        
        var virtualCamera = Game.Instance.model.VirtualCamera;
        if (virtualCamera == null)
        {
            Debugger.LogWarning($"No VirtualCamera found to apply configuration for scene: {scene}");
            return;
        }
        
        var composer = virtualCamera.GetComponent<CinemachinePositionComposer>();
        if (composer == null)
        {
            Debugger.LogWarning($"No CinemachinePositionComposer found on VirtualCamera for scene: {scene}");
            return;
        }
        
        // Get and apply the camera configuration for this scene
        var config = NeonLadder.Cameras.CameraSettings.GetConfigForScene(scene);
        NeonLadder.Cameras.CameraSettings.ApplyConfig(composer, config);
        
        Debugger.Log($"Applied camera config for {scene}: Distance={config.cameraDistance}, Offset={config.targetOffset}");
    }
}
}
