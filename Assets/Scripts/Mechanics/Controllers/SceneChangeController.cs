using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeController : MonoBehaviour
{
    public string SceneName;
    private PlatformerModel model;
    private Player player;
    private PlayerCameraPositionManager playerAndCameraPositionManager;

    private void Awake()
    {
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
        playerAndCameraPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerCameraPositionManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(Tags.Player.ToString()))
        {
            //model.VirtualCamera.enabled = false;
            SceneManager.LoadScene(SceneName); //breakpoint here
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (player == null)
        {
            return;
        }

        player.DisableZMovement();

        Vector3 playerPosition;
        Vector3 cameraPosition;
        Quaternion cameraRotation;

        if (playerAndCameraPositionManager.TryGetState(scene.name, out playerPosition, out cameraPosition, out cameraRotation))
        {
            player.Teleport(playerPosition);
            GameObject.FindGameObjectWithTag(Tags.MainCamera.ToString()).transform.position = cameraPosition;
        }
        else
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag(Tags.SpawnPoint.ToString());
            if (spawnPoint != null)
            {
                //Debug.Log($"Current Player position: {player.transform.position} -> SpawnPoint found in the scene: {spawnPoint.transform.position}");
                if (player.transform.parent.position != spawnPoint.transform.position)
                {
                    //Debug.Log("Teleporting player to SpawnPoint position.");
                    player.Teleport(spawnPoint.transform.position);
                    //player.Actions.transform.position = spawnPoint.transform.position;
                }
                //player.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogWarning("No SpawnPoint found in the scene.");
            }
        }
        model.VirtualCamera.enabled = true;
        player.transform.parent.rotation = Quaternion.Euler(0, 90, 0);
        player.velocity = new Vector3(0, 0, 0);
        if (player.transform.parent.position.y < 0)
        {
            player.transform.parent.position = new Vector3(player.transform.parent.position.x, 0.01f, player.transform.parent.position.z);
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
