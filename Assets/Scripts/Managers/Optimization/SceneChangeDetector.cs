using System;

namespace NeonLadder.Managers.Optimization
{
    /// <summary>
    /// High-performance scene change detector that eliminates per-frame string comparisons.
    /// 
    /// This class caches the current scene name and only performs string comparisons when
    /// the scene actually changes, providing significant performance improvements for
    /// frame-rate critical applications.
    /// </summary>
    public class SceneChangeDetector
    {
        private readonly ISceneProvider sceneProvider;
        private string cachedSceneName;
        private string previousSceneName;
        private bool isInitialized;

        /// <summary>
        /// Event triggered when a scene change is detected.
        /// Parameters: (previousScene, newScene)
        /// </summary>
        public event Action<string, string> OnSceneChangeDetected;

        /// <summary>
        /// Initializes a new SceneChangeDetector with the specified scene provider.
        /// </summary>
        /// <param name="sceneProvider">The scene provider to use for scene information</param>
        public SceneChangeDetector(ISceneProvider sceneProvider)
        {
            this.sceneProvider = sceneProvider ?? throw new ArgumentNullException(nameof(sceneProvider));
            this.isInitialized = false;
        }

        /// <summary>
        /// Efficiently checks if the scene has changed since the last call.
        /// 
        /// Performance characteristics:
        /// - First call: Caches current scene, returns false
        /// - Subsequent calls with same scene: Fast string comparison with cached value
        /// - Scene change detected: Updates cache, triggers event, returns true once
        /// </summary>
        /// <returns>True if scene changed since last call, false otherwise</returns>
        public bool HasSceneChanged()
        {
            var currentSceneName = sceneProvider.GetCurrentSceneName();
            
            // Initialize cache on first call
            if (!isInitialized)
            {
                cachedSceneName = currentSceneName;
                isInitialized = true;
                return false;
            }
            
            // Fast string comparison with cached value
            if (currentSceneName != cachedSceneName)
            {
                // Scene change detected - update cache and notify
                previousSceneName = cachedSceneName;
                cachedSceneName = currentSceneName;
                
                OnSceneChangeDetected?.Invoke(previousSceneName, cachedSceneName);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Gets the currently cached scene name without triggering change detection.
        /// </summary>
        /// <returns>The cached scene name, or null if not initialized</returns>
        public string GetCachedSceneName()
        {
            return cachedSceneName;
        }

        /// <summary>
        /// Gets the previous scene name (before the last detected change).
        /// </summary>
        /// <returns>The previous scene name, or null if no change has been detected</returns>
        public string GetPreviousSceneName()
        {
            return previousSceneName;
        }

        /// <summary>
        /// Resets the detector state, forcing re-initialization on next call.
        /// Useful for testing or when scene state needs to be cleared.
        /// </summary>
        public void Reset()
        {
            cachedSceneName = null;
            previousSceneName = null;
            isInitialized = false;
        }
    }
}