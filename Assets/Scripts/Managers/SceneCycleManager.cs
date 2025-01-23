using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneCycleManager : MonoBehaviour
{
    public List<string> ScenesToIterateFromMainMenu = new();
    public float StartLoadingNewSceneAfterXSeconds = 3f;
    public float MinSceneDuration = 5f; // Minimum time a scene must be displayed
    private int currentSceneIndex = 0;
    private bool isFirstSceneLoaded = false;
    private float sceneStartTime = 0f;

    void Start()
    {
        // Preload the first scene but do not switch to it yet
        if (ScenesToIterateFromMainMenu.Count > 0 && IsSceneInBuildSettings(ScenesToIterateFromMainMenu[0]))
        {
            string firstScene = ScenesToIterateFromMainMenu[0];
            StartCoroutine(PreloadScene(firstScene));
        }
    }

    public void CycleThroughScenesFromMainMenu()
    {
        if (!isFirstSceneLoaded && ScenesToIterateFromMainMenu.Count > 0)
        {
            // Perform a hard scene change to the first scene
            string firstScene = ScenesToIterateFromMainMenu[0];
            SceneManager.LoadScene(firstScene, LoadSceneMode.Single);
            isFirstSceneLoaded = true;
            sceneStartTime = Time.time; // Record when the scene starts
            currentSceneIndex = 1; // Set to the next scene in the list

            // Start automatic cycling after the first scene swap
            StartCoroutine(LoadSceneSequence());
        }
    }

    private IEnumerator LoadSceneSequence()
    {
        while (currentSceneIndex < ScenesToIterateFromMainMenu.Count)
        {
            string sceneToLoad = ScenesToIterateFromMainMenu[currentSceneIndex];

            // Wait for the minimum scene duration before loading the next scene
            float timeElapsed = Time.time - sceneStartTime;
            if (timeElapsed < MinSceneDuration)
            {
                yield return new WaitForSeconds(MinSceneDuration - timeElapsed);
            }

            // Wait for the specified duration before starting to load the next scene
            yield return new WaitForSeconds(StartLoadingNewSceneAfterXSeconds);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);

            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            sceneStartTime = Time.time; // Record when the new scene starts
            currentSceneIndex++;
            if (currentSceneIndex >= ScenesToIterateFromMainMenu.Count)
            {
                // Preload the Title scene but do not load it immediately
                StartCoroutine(PreloadScene("Title"));

                // Wait for the minimum duration
                timeElapsed = Time.time - sceneStartTime;
                if (timeElapsed < MinSceneDuration)
                {
                    yield return new WaitForSeconds(MinSceneDuration - timeElapsed);
                }

                // Perform a hard scene change back to the Title scene
                SceneManager.LoadScene("Title", LoadSceneMode.Single);
                currentSceneIndex = 0; // Reset to the first scene
                yield break;
            }
        }
    }

    private IEnumerator PreloadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                break;
            }
            yield return null;
        }
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
