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
    private PlayerPositionManager playerPositionManager;

    private void Awake()
    {
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
        playerPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerPositionManager>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(Tags.Player.ToString()))
        {
            SceneManager.LoadScene(SceneName);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (player == null)
        {
            return;
        }

         var cameraAdjustment = model.VirtualCamera.GetComponent<DynamicCameraAdjustment>();
        if (cameraAdjustment != null)
        {
            cameraAdjustment.RefreshRenderers();
        }
        else
        {
            Debug.LogWarning("DynamicCameraAdjustment not found in the scene.");
        }

        player.DisableZMovement();
        player.RevertCameraProperties();

        Vector3 spawnPosition;
        if (playerPositionManager.TryGetLastPlayerPosition(scene.name, out spawnPosition))
        {
            // Use the saved position to place the player in the correct spot
            player.transform.position = spawnPosition;
        }
        else
        {
            // Find the spawn point in the new scene
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
    }
}
