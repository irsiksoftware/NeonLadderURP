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
    private DynamicCameraAdjustment cameraAdjustment;


    private void Awake()
    {
        
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
        //remove this line
        playerAndCameraPositionManager = GameObject.FindGameObjectWithTag(Tags.Managers.ToString()).GetComponentInChildren<PlayerCameraPositionManager>();
        cameraAdjustment = model.VirtualCamera.GetComponent<DynamicCameraAdjustment>();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(Tags.Player.ToString()))
        {
            //Debug.Log("Disabling DynamicCameraAdjustment");
            //replace with manager call
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
        //replace with manager call
        cameraAdjustment.enabled = true;
        //Debug.Log("DynamicCameraAdjustment enabled state: " + cameraAdjustment.enabled);
        player.DisableZMovement();

        Vector3 playerPosition;
        Quaternion cameraRotation;

        if (playerAndCameraPositionManager.TryGetState(scene.name, out playerPosition, out cameraRotation))
        {
            Debug.Log($"Restoring player position to {playerPosition} and camera rotation to {cameraRotation} in scene {scene.name}.");
            // Set the player and camera positions and rotations
            player.transform.position = playerPosition;
            //model.VirtualCamera.transform.position = cameraPosition;
            model.VirtualCamera.transform.rotation = cameraRotation;
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

        //set players transform rotation y = 90 (facing to the right)
        player.gameObject.transform.rotation = Quaternion.Euler(0, 90, 0);
        player.RevertCameraProperties();
    }
}
