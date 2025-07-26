namespace NeonLadder.Managers.Optimization
{
    /// <summary>
    /// Interface for providing scene information to the SceneChangeDetector.
    /// This abstraction allows for dependency injection and testability.
    /// </summary>
    public interface ISceneProvider
    {
        /// <summary>
        /// Gets the current scene name from the underlying scene management system.
        /// </summary>
        /// <returns>The name of the currently active scene</returns>
        string GetCurrentSceneName();
    }
}