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
    private DynamicCameraAdjustment cameraAdjustment;


    private void Awake()
    {
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
        playerPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerPositionManager>();
        cameraAdjustment = model.VirtualCamera.GetComponent<DynamicCameraAdjustment>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(Tags.Player.ToString()))
        {
            //Debug.Log("Disabling DynamicCameraAdjustment");
            cameraAdjustment.enabled = false;
            //Debug.Log("DynamicCameraAdjustment enabled state: " + cameraAdjustment.enabled);
            SceneManager.LoadScene(SceneName); //breakpoint here
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (player == null)
        {
            return;
        }

        //Debug.Log("Enabling DynamicCameraAdjustment");
        cameraAdjustment.enabled = true;
        //Debug.Log("DynamicCameraAdjustment enabled state: " + cameraAdjustment.enabled);
        player.DisableZMovement();

        Vector3 spawnPosition;
        if (playerPositionManager.TryGetLastPlayerPosition(scene.name, out spawnPosition))
        {
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
