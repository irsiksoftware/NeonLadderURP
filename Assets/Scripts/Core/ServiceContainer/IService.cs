using System;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Base interface for all services in the service container.
    /// Services should be stateless or manage their own state safely.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Called when the service is registered with the container.
        /// Use for one-time initialization that doesn't depend on other services.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Called when all services have been registered.
        /// Use for initialization that depends on other services.
        /// </summary>
        void OnServicesReady();
        
        /// <summary>
        /// Called when the service is being removed from the container.
        /// Use for cleanup and resource disposal.
        /// </summary>
        void Shutdown();
    }
    
    /// <summary>
    /// Interface for services that need to be updated each frame.
    /// </summary>
    public interface IUpdatableService : IService
    {
        /// <summary>
        /// Called once per frame when the service is active.
        /// </summary>
        void Update();
        
        /// <summary>
        /// Whether this service should receive Update calls.
        /// </summary>
        bool IsActive { get; set; }
    }
    
    /// <summary>
    /// Interface for services that need scene-specific behavior.
    /// </summary>
    public interface ISceneAwareService : IService
    {
        /// <summary>
        /// Called when the scene changes.
        /// </summary>
        void OnSceneChanged(string previousScene, string newScene);
        
        /// <summary>
        /// Returns true if this service should be active in the given scene.
        /// </summary>
        bool IsActiveInScene(string sceneName);
    }
}