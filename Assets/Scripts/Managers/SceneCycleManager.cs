using NeonLadder.Debugging;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCycleManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    public List<string> ScenesToIterateFromMainMenu = new();

    [Tooltip("When checked, if no scenes are specified above, iterate through all package scenes")]
    public bool IterateAllPackagesIfNoneSpecified = true;

    [Header("Timing Settings")]
    public float StartLoadingNewSceneAfterXSeconds = 3f;
    public float MinSceneDuration = 5f; // Minimum time a scene must be displayed

    private int currentSceneIndex = 0;
    private bool isCycling = false;
    private Coroutine cyclingCoroutine;

    void Start()
    {
        if (ScenesToIterateFromMainMenu.Count == 0 && IterateAllPackagesIfNoneSpecified)
        {
            // If no scenes were manually specified in Inspector and checkbox is enabled, use all package scenes
            ScenesToIterateFromMainMenu = GetPackageScenes();
            Debugger.Log($"SceneCycleManager: No scenes specified in Inspector and IterateAllPackagesIfNoneSpecified is enabled. Using {ScenesToIterateFromMainMenu.Count} package scenes as default.");
        }
        else if (ScenesToIterateFromMainMenu.Count == 0)
        {
            Debugger.LogWarning("SceneCycleManager: No scenes specified and IterateAllPackagesIfNoneSpecified is disabled. Scene cycling will not work.");
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
    /// Get all package scenes from the nested Scenes static class structure.
    /// Uses the new nested static class pattern introduced in PBI #148.
    /// </summary>
    private List<string> GetPackageScenes()
    {
        // Return all the package scenes from the Packaged nested class
        // Filter by what's actually in build settings
        return Scenes.Packaged.All
            .Where(sceneName => IsSceneInBuildSettings(sceneName))
            .ToList();
    }

    public void CycleThroughScenesFromMainMenu()
    {
        // Validate we have scenes to cycle through
        if (ScenesToIterateFromMainMenu == null || ScenesToIterateFromMainMenu.Count == 0)
        {
            if (IterateAllPackagesIfNoneSpecified)
            {
                Debugger.LogWarning("SceneCycleManager: No scenes to iterate through! IterateAllPackagesIfNoneSpecified is enabled but no package scenes were found.");
            }
            else
            {
                Debugger.LogWarning("SceneCycleManager: No scenes to iterate through! Add scenes to the list or enable 'IterateAllPackagesIfNoneSpecified'.");
            }
            return;
        }
        
        // Don't start if already cycling
        if (isCycling)
        {
            Debugger.Log("SceneCycleManager: Already cycling through scenes");
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
                Debugger.Log("SceneCycleManager: Completed all scenes, returning to Title");
                
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
                Debugger.LogWarning($"SceneCycleManager: Skipping empty scene name at index {currentSceneIndex}");
                currentSceneIndex++;
                continue;
            }
            
            // Validate scene is in build settings
            if (!IsSceneInBuildSettings(sceneToLoad))
            {
                Debugger.LogWarning($"SceneCycleManager: Scene '{sceneToLoad}' not in build settings, skipping");
                currentSceneIndex++;
                continue;
            }
            
            Debugger.Log($"SceneCycleManager: Loading scene '{sceneToLoad}' ({currentSceneIndex + 1}/{ScenesToIterateFromMainMenu.Count})");
            
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
        Debugger.Log("SceneCycleManager: Stopped cycling");
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