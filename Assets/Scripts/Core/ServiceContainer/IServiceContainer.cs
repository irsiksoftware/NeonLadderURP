using System;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Service container interface for dependency injection and service location.
    /// Replaces singleton pattern with more testable and maintainable architecture.
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// Registers a service with the container.
        /// </summary>
        void Register<TService>(TService service) where TService : class;
        
        /// <summary>
        /// Registers a service with the container using a specific interface type.
        /// </summary>
        void Register<TInterface, TImplementation>(TImplementation implementation) 
            where TInterface : class 
            where TImplementation : class, TInterface;
        
        /// <summary>
        /// Retrieves a service from the container.
        /// </summary>
        TService Get<TService>() where TService : class;
        
        /// <summary>
        /// Tries to retrieve a service from the container.
        /// Returns false if service is not registered.
        /// </summary>
        bool TryGet<TService>(out TService service) where TService : class;
        
        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        bool IsRegistered<TService>() where TService : class;
        
        /// <summary>
        /// Unregisters a service from the container.
        /// </summary>
        void Unregister<TService>() where TService : class;
        
        /// <summary>
        /// Clears all registered services.
        /// </summary>
        void Clear();
    }
}