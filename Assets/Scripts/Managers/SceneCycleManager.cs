using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCycleManager : MonoBehaviour
{
    public List<string> ScenesToIterateFromMainMenu = new();
    public float StartLoadingNewSceneAfterXSeconds = 3f;
    public float MinSceneDuration = 5f; // Minimum time a scene must be displayed
    private int currentSceneIndex = 0;
    private bool isCycling = false;
    private Coroutine cyclingCoroutine;

    void Start()
    {
        if (ScenesToIterateFromMainMenu.Count == 0)
        {
            // If no scenes were manually specified in Inspector, use all package scenes
            ScenesToIterateFromMainMenu = GetPackageScenes();
            Debug.Log($"SceneCycleManager: No scenes specified in Inspector. Using {ScenesToIterateFromMainMenu.Count} package scenes as default.");
        }
    }
    
    void OnDestroy()
    {
        // Stop cycling if this object is destroyed
        if (cyclingCoroutine != null)
        {
            StopCoroutine(cyclingCoroutine);
        }
    }

    /// <summary>
    /// Temporary method to get all package scenes from the Scenes enum.
    /// This will be refactored when Scenes enum is converted to nested static classes (PBI #148)
    /// </summary>
    private List<string> GetPackageScenes()
    {
        // Return all the package scenes that were added from Unity Asset Store
        var packageScenes = new List<Scenes>
        {
            Scenes.URP_SiegeOfPonthus,
            Scenes.URP_AncientCathedral,
            Scenes.Medievil_Tavern,
            Scenes.SpaceBurgers,
            Scenes.AbandonedFactory,
            Scenes.AbandonedPoolURP,
            Scenes.Cyberpunk_Room,
            Scenes.CyberpunkURPScene,
            Scenes.URP_Rooftop_Market,
            Scenes.URP_RomanStreet,
            Scenes.Showcase,
            Scenes.FC_URP_Scene,
            Scenes.StandardizedOstrichBar
        };

        // Convert enum values to string names and filter by what's actually in build settings
        return packageScenes
            .Select(scene => scene.ToString())
            .Where(sceneName => IsSceneInBuildSettings(sceneName))
            .ToList();
    }

    public void CycleThroughScenesFromMainMenu()
    {
        // Validate we have scenes to cycle through
        if (ScenesToIterateFromMainMenu == null || ScenesToIterateFromMainMenu.Count == 0)
        {
            Debug.LogWarning("SceneCycleManager: No scenes to iterate through!");
            return;
        }
        
        // Don't start if already cycling
        if (isCycling)
        {
            Debug.Log("SceneCycleManager: Already cycling through scenes");
            return;
        }
        
        // Start the cycle
        currentSceneIndex = 0;
        isCycling = true;
        cyclingCoroutine = StartCoroutine(CycleScenes());
    }
    
    private IEnumerator CycleScenes()
    {
        while (isCycling && ScenesToIterateFromMainMenu.Count > 0)
        {
            // Get the current scene name, validating the index
            if (currentSceneIndex >= ScenesToIterateFromMainMenu.Count)
            {
                // Loop back to Title scene when we've gone through all scenes
                Debug.Log("SceneCycleManager: Completed all scenes, returning to Title");
                
                // Wait before returning to title
                yield return new WaitForSeconds(MinSceneDuration);
                
                // Use SceneTransitionManager for smooth transition back to Title
                if (SceneTransitionManager.Instance != null)
                {
                    SceneTransitionManager.Instance.TransitionToScene("Title");
                }
                else
                {
                    SceneManager.LoadScene("Title");
                }
                
                isCycling = false;
                yield break;
            }
            
            string sceneToLoad = ScenesToIterateFromMainMenu[currentSceneIndex];
            
            // Validate scene name
            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogWarning($"SceneCycleManager: Skipping empty scene name at index {currentSceneIndex}");
                currentSceneIndex++;
                continue;
            }
            
            // Validate scene is in build settings
            if (!IsSceneInBuildSettings(sceneToLoad))
            {
                Debug.LogWarning($"SceneCycleManager: Scene '{sceneToLoad}' not in build settings, skipping");
                currentSceneIndex++;
                continue;
            }
            
            Debug.Log($"SceneCycleManager: Loading scene '{sceneToLoad}' ({currentSceneIndex + 1}/{ScenesToIterateFromMainMenu.Count})");
            
            // Load the scene using SceneTransitionManager for smooth transitions
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(sceneToLoad);
            }
            else
            {
                // Fallback to direct loading if SceneTransitionManager is not available
                SceneManager.LoadScene(sceneToLoad);
            }
            
            // Move to next scene
            currentSceneIndex++;
            
            // Wait for minimum scene duration
            yield return new WaitForSeconds(MinSceneDuration);
            
            // Additional wait before loading next scene
            yield return new WaitForSeconds(StartLoadingNewSceneAfterXSeconds);
        }
    }
    
    public void StopCycling()
    {
        isCycling = false;
        if (cyclingCoroutine != null)
        {
            StopCoroutine(cyclingCoroutine);
            cyclingCoroutine = null;
        }
        Debug.Log("SceneCycleManager: Stopped cycling");
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string loadedScenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string loadedSceneName = System.IO.Path.GetFileNameWithoutExtension(loadedScenePath);

            if (loadedSceneName == sceneName)
            {
                return true;
            }
        }

        return false;
    }
}