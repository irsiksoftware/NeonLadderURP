using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameControllerManager : MonoBehaviour
{
    private Game gameController;
    private DynamicCameraAdjustment dynamicCameraAdjustment;
    private Player player;
    private PlayerAction playerActions;
    private GameObject statUI;

    // Start is called before the first frame update
    void Start()
    {
        InitializeControllers();
        AdjustControllersBasedOnScene(SceneManager.GetActiveScene().name);
    }

    // Update is called once per frame
    void Update()
    {
        AdjustControllersBasedOnScene(SceneManager.GetActiveScene().name);
    }

    private void InitializeControllers()
    {
        gameController = Game.Instance;
        if (gameController == null)
        {
            Debug.LogError("GameController not found in scene.");
        }

        dynamicCameraAdjustment = gameController.GetComponentInChildren<DynamicCameraAdjustment>();
        if (dynamicCameraAdjustment == null)
        {
            Debug.LogError("DynamicCameraAdjustment not found in scene.");
        }

        player = gameController.GetComponentInChildren<Player>();
        if (player == null)
        {
            Debug.LogError("Player not found in scene.");
        }

        playerActions = player.GetComponentInChildren<PlayerAction>();
        if (playerActions == null)
        {
            Debug.LogError("PlayerAction not found in scene.");
        }

        statUI = gameController.GetComponentInChildren<StatUI>().gameObject;
        if (statUI == null)
        {
            Debug.LogError("StatUI not found in scene.");
        }
    }

    private void AdjustControllersBasedOnScene(string sceneName)
    {
        switch (sceneName)
        {
            case nameof(Scenes.Title):
                playerActions.enabled = false;
                gameController.gameObject.SetActive(false);
                break;
            case nameof(Scenes.Staging):
                dynamicCameraAdjustment.enabled = false;
                break;
            case nameof(Scenes.Start):
                break;
            case nameof(Scenes.MetaShop):
                // Adjust components for MetaShop scene
                break;
            case nameof(Scenes.PermaShop):
                // Adjust components for PermaShop scene
                break;
            default:
                dynamicCameraAdjustment.enabled = true;
                playerActions.enabled = true;
                gameController.gameObject.SetActive(true);
                break;
        }
    }
}
