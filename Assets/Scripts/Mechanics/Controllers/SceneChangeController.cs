using NeonLadder.Core;
using NeonLadder.Managers;
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
            model.VirtualCamera.enabled = false;
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
                player.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogWarning("No SpawnPoint found in the scene.");
            }
        }
        model.VirtualCamera.enabled = true;
        player.gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
        if (player.transform.position.y < 0)
        {
            player.transform.position = new Vector3(player.transform.position.x, 0.01f, player.transform.position.z);
        }
    }
}
