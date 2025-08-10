using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Service locator implementation for managing game services.
    /// Provides dependency injection and service location capabilities.
    /// </summary>
    public class ServiceLocator : IServiceContainer
    {
        private static ServiceLocator instance;
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private readonly object serviceLock = new object();
        
        /// <summary>
        /// Global service locator instance.
        /// </summary>
        public static ServiceLocator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServiceLocator();
                }
                return instance;
            }
        }
        
        /// <summary>
        /// Creates a new service locator instance for testing purposes.
        /// </summary>
        public static ServiceLocator CreateNew()
        {
            return new ServiceLocator();
        }
        
        /// <summary>
        /// Resets the global instance (for testing purposes).
        /// </summary>
        public static void Reset()
        {
            instance = null;
        }
        
        private ServiceLocator()
        {
        }
        
        public void Register<TService>(TService service) where TService : class
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }
            
            lock (serviceLock)
            {
                var type = typeof(TService);
                if (services.ContainsKey(type))
                {
                    Debug.LogWarning($"Service {type.Name} is already registered. Overwriting.");
                }
                services[type] = service;
            }
        }
        
        public void Register<TInterface, TImplementation>(TImplementation implementation) 
            where TInterface : class 
            where TImplementation : class, TInterface
        {
            if (implementation == null)
            {
                throw new ArgumentNullException(nameof(implementation));
            }
            
            lock (serviceLock)
            {
                var interfaceType = typeof(TInterface);
                if (services.ContainsKey(interfaceType))
                {
                    Debug.LogWarning($"Service {interfaceType.Name} is already registered. Overwriting.");
                }
                services[interfaceType] = implementation;
            }
        }
        
        public TService Get<TService>() where TService : class
        {
            lock (serviceLock)
            {
                var type = typeof(TService);
                if (services.TryGetValue(type, out var service))
                {
                    return service as TService;
                }
                
                throw new InvalidOperationException($"Service {type.Name} is not registered.");
            }
        }
        
        public bool TryGet<TService>(out TService service) where TService : class
        {
            lock (serviceLock)
            {
                var type = typeof(TService);
                if (services.TryGetValue(type, out var serviceObj))
                {
                    service = serviceObj as TService;
                    return service != null;
                }
                
                service = null;
                return false;
            }
        }
        
        public bool IsRegistered<TService>() where TService : class
        {
            lock (serviceLock)
            {
                return services.ContainsKey(typeof(TService));
            }
        }
        
        public void Unregister<TService>() where TService : class
        {
            lock (serviceLock)
            {
                var type = typeof(TService);
                if (services.ContainsKey(type))
                {
                    services.Remove(type);
                }
            }
        }
        
        public void Clear()
        {
            lock (serviceLock)
            {
                services.Clear();
            }
        }
    }
}