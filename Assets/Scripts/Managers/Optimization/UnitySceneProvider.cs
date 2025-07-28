using UnityEngine.SceneManagement;

/// <summary>
    /// Unity-specific implementation of ISceneProvider that wraps SceneManager calls.
    /// This provides the actual scene information in production code.
/// </summary>
public class UnitySceneProvider : ISceneProvider
    {
        /// <summary>
        /// Gets the current scene name from Unity's SceneManager.
        /// </summary>
        /// <returns>The name of the currently active Unity scene</returns>
        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }
    }