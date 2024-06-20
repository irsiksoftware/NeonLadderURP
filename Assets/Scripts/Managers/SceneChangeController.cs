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


    private void Awake()
    {
        model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
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
        Debug.Log("Scene loaded: " + scene.name);
        if (player == null)
        {
            Debug.LogWarning("Player not found in the scene.");
            return;
        }


        if (player != null)
        {
            model.VirtualCamera.GetComponent<DynamicCameraAdjustment>().RefreshRenderers();
            player.DisableZMovement();
            player.RevertCameraProperties();
            
            // Find the spawn point in the new scene
            GameObject spawnPoint = GameObject.FindGameObjectWithTag(Tags.SpawnPoint.ToString());
            if (spawnPoint != null)
            {
                Debug.Log("Player spawned at: " + spawnPoint.transform.position);
                player.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.LogWarning("No SpawnPoint found in the scene.");
            }
        }
    }
}
