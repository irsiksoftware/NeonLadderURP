using UnityEngine;
using NeonLadder.Debugging;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Base class for manager services that wrap existing Unity MonoBehaviour managers.
    /// Provides common functionality for initialization and lifecycle management.
    /// </summary>
    public abstract class BaseManagerService : ISceneAwareService
    {
        protected MonoBehaviour manager;
        protected bool isInitialized = false;
        
        public BaseManagerService(MonoBehaviour manager)
        {
            this.manager = manager;
        }
        
        public virtual void Initialize()
        {
            if (manager == null)
            {
                Debugger.LogError($"[{GetType().Name}] Manager is null during initialization");
                return;
            }
            
            isInitialized = true;
            Debugger.Log($"[{GetType().Name}] Initialized with manager: {manager.GetType().Name}");
        }
        
        public virtual void OnServicesReady()
        {
            // Override in derived classes if needed
        }
        
        public virtual void Shutdown()
        {
            isInitialized = false;
            manager = null;
        }
        
        public virtual void OnSceneChanged(string previousScene, string newScene)
        {
            // Override in derived classes to handle scene-specific logic
        }
        
        public abstract bool IsActiveInScene(string sceneName);
        
        protected void SetManagerEnabled(bool enabled)
        {
            if (manager != null)
            {
                manager.enabled = enabled;
            }
        }
    }
}