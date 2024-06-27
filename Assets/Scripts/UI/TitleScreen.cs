using NeonLadder.Mechanics.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene(Scenes.Staging.ToString());
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
