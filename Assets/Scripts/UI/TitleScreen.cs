using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

public class TitleScreen : MonoBehaviour
{
    public void StartGame()
    {
        // Ensure all first-level children are enabled before loading the main game scene
        if (Game.Instance != null)
        {
            Game.Instance.EnableAllFirstLevelChildren();
            Game.Instance.model.Player.GetComponentInChildren<PlayerAction>().enabled = true;
        }

        // Trigger the LoadGame event
        Simulation.Schedule<LoadGame>();

        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
