using System;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Base interface for all game services.
    /// Services implementing this interface can be registered with the service container.
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Initializes the service.
        /// Called when the service is registered.
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// Shuts down the service.
        /// Called when the service is unregistered.
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Gets whether the service is initialized.
        /// </summary>
        bool IsInitialized { get; }
    }
}