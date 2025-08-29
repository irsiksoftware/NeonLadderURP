using NeonLadder.Mechanics.Enums;
using NeonLadderURP.DataManagement;
using NeonLadder.ProceduralGeneration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.UI
{
    public class TitleScreen : MonoBehaviour
{
    public void Start()
    {
        // Check if save data exists using the new EnhancedSaveSystem
        var saveData = EnhancedSaveSystem.Load();
        if (saveData != null)
        {
            var playButton = GameObject.FindGameObjectWithTag(Tags.PlayButton.ToString());
            if (playButton != null)
            {
                var buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Resume Game";
                }
            }
        }
    }
    public void StartGame()
    {
        // Check if this is a new game or resume
        var saveData = EnhancedSaveSystem.Load();
        
        if (saveData == null)
        {
            // New game - clear any existing checkpoints
            if (CheckpointManager.Instance != null)
            {
                CheckpointManager.Instance.ClearCheckpoint();
            }
        }
        
        // Use SceneTransitionManager for proper scene transitions with fade effects
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(Scenes.Staging.ToString());
        }
        else
        {
            // Fallback to direct scene loading if transition manager not available
            Debug.LogWarning("SceneTransitionManager not found, using direct scene load");
            SceneManager.LoadScene(Scenes.Staging.ToString());
        }
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
    
    /// <summary>
    /// Called when player clicks "New Game" button (if separate from Start Game)
    /// </summary>
    public void NewGame()
    {
        // Clear any existing save data and checkpoints
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.ClearCheckpoint();
        }
        
        // Clear save data for fresh start
        EnhancedSaveSystem.DeleteSave();
        
        // Start new game
        StartGame();
    }
    
    /// <summary>
    /// Load existing save game
    /// </summary>
    public void LoadGame()
    {
        var saveData = EnhancedSaveSystem.Load();
        if (saveData != null && !string.IsNullOrEmpty(saveData.worldState.currentSceneName))
        {
            // Load the saved scene
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(saveData.worldState.currentSceneName);
            }
            else
            {
                SceneManager.LoadScene(saveData.worldState.currentSceneName);
            }
        }
        else
        {
            // No valid save data, start new game
            Debug.LogWarning("No valid save data found, starting new game");
            NewGame();
        }
    }
}
}
