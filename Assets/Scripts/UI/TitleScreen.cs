using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void Awake()
    {
        try
        {
            var gameController = GameObject.FindGameObjectWithTag(Tags.GameController.ToString());
            gameController.gameObject.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }
    public void StartGame()
    {
        Simulation.Schedule<LoadGame>();
        SceneManager.LoadScene(Scenes.Staging.ToString());
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
