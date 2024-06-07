using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Models;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeController : MonoBehaviour
{
    public string SceneName;
    public TextMeshPro textMeshPro;
    public bool requireInput = false;
    public Directions Direction = Directions.East;
    public AsyncOperation asyncLoad; // To handle the asynchronous scene loading

    private PlatformerModel platformerModel = Simulation.GetModel<PlatformerModel>();
    private Player player => platformerModel.Player;

    private void Awake()
    {

    }

    private void Start()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = SceneName;
            //textMeshPro.gameObject.SetActive(false); // Keep it inactive until needed
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartLoadingScene(SceneName);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (textMeshPro != null)
            {
                StartUnloadingScene(SceneName);
                //textMeshPro.gameObject.SetActive(false);
            }
        }
    }



    private void Update()
    {

        if (asyncLoad != null)
        {
            Debug.Log($"Loading progress: {asyncLoad.progress * 100}% complete");
        }

        //if (requireInput)
        //{
        //    var playerActions = GameObject.Find(Constants.ProtagonistModel).GetComponentInChildren<Player25DActionController>();
        //    if (playerActions.isClimbing && asyncLoad != null && !asyncLoad.isDone)
        //    {
        //        asyncLoad.allowSceneActivation = true; // Allow the scene to activate if it's ready
        //    }
        //}
    }

    private void StartLoadingScene(string sceneName)
    {
        Debug.Log($"Starting to load {sceneName}");
        asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Prevent the scene from being activated immediately


        //SceneManager.sceneLoaded -= (scene, mode) => { }; // Remove the previous listener (if any)
        SceneManager.sceneLoaded += (scene, mode) =>
        {

            var westSpawn = scene.GetRootGameObjects().FirstOrDefault(go => go.CompareTag("WestSpawn"));
            var eastSpawn = scene.GetRootGameObjects().FirstOrDefault(go => go.CompareTag("EastSpawn"));
            var northSpawn = scene.GetRootGameObjects().FirstOrDefault(go => go.CompareTag("NorthSpawn"));
            var southSpawn = scene.GetRootGameObjects().FirstOrDefault(go => go.CompareTag("SouthSpawn"));

            //Player25DController.Instance.health.current = player.health.current;
            //Player25DController.Instance.stamina.current = player.stamina.current;

            switch (Direction)
            {
                case Directions.East:
                    //Player25DController.Instance.Spawn(westSpawn.transform);
                    player.Spawn(westSpawn.transform); // Spawn the player in the new scene
                    break;
                case Directions.West:
                    player.Spawn(eastSpawn.transform);
                    break;
                case Directions.North:
                    player.Spawn(southSpawn.transform);
                    break;
                case Directions.South:
                    player.Spawn(northSpawn.transform);
                    break;
            }
            SceneManager.sceneLoaded -= (scene, mode) => { }; // Remove the listener after the scene has been loaded
        };


    }

    private void StartUnloadingScene(string sceneName)
    {
        Debug.Log($"Starting to Unload {sceneName}");
        asyncLoad = SceneManager.UnloadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Prevent the scene from being activated immediately
    }

    public void ChangeScene()
    {
        //player.gameObject.SetActive(false);
        if ((asyncLoad != null && asyncLoad.isDone) || (asyncLoad != null && asyncLoad.progress >= .9f)) // Ensure the scene is fully loaded, or accoding to gpt...When an asynchronous scene loading operation in Unity quickly progresses to 90% and then stalls, it is typically waiting for allowSceneActivation to be explicitly set to true. This behavior is by design in Unity's asynchronous loading system: the load operation loads the scene data up to 90%, and the final 10% is completed when the scene is actually activated.
        {
            Debug.Log("Activating loaded scene asynchronously..");
            asyncLoad.allowSceneActivation = true; // Now allow the scene to activate
        }
        else
        {
            Debug.Log("Scene failed to load asynchronously, synchronously loading instead....");
            SceneManager.LoadScene(SceneName);
        }
    }
}
