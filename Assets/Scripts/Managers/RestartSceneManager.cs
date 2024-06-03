using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSceneManager : MonoBehaviour
{
    public static RestartSceneManager Instance { get; set; }

    void Awake()
    {
        // If there is an instance, and it's not me, destroy myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }



    public void RestartScene()
    {
        //Debug.Log("RestartScene called");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}