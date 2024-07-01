using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Enums;
using NeonLadderURP.DataManagement;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public void Start()
    {
        if (SaveSystem.SaveExists())
        {
            GameObject.FindGameObjectWithTag(Tags.PlayButton.ToString()).GetComponentInChildren<TextMeshProUGUI>().text = "Resume Game";
        }
    }
    public void StartGame()
    {
        SceneManager.LoadScene(Scenes.Staging.ToString());
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}
