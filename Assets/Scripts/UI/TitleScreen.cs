using NeonLadder.Core;
using NeonLadder.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        Simulation.Schedule<LoadGame>();
        SceneManager.LoadScene("SampleScene");
    }

    public void OnApplicationQuit()
    {
        // Save the game data when the application is quit
        Application.Quit();
    
    }
}
