using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.Debugging;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Thread-safe service container that manages service lifecycle and dependencies.
    /// Replaces singleton pattern with dependency injection.
    /// </summary>
    public class ServiceContainer : MonoBehaviour
    {
        private static ServiceContainer instance;
        private readonly Dictionary<Type, IService> services = new Dictionary<Type, IService>();
        private readonly List<IUpdatableService> updatableServices = new List<IUpdatableService>();
        private readonly List<ISceneAwareService> sceneAwareServices = new List<ISceneAwareService>();
        private readonly object serviceLock = new object();
        private bool isInitialized = false;
        
        /// <summary>
        /// Singleton instance of the service container.
        /// </summary>
        public static ServiceContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ServiceContainer>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ServiceContainer");
                        instance = go.AddComponent<ServiceContainer>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            // Update all active updatable services
            foreach (var service in updatableServices)
            {
                if (service.IsActive)
                {
                    try
                    {
                        service.Update();
                    }
                    catch (Exception e)
                    {
                        Debugger.LogError($"[ServiceContainer] Error updating service {service.GetType().Name}: {e.Message}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Register a service with the container.
        /// </summary>
        public void Register<T>(T service) where T : class, IService
        {
            lock (serviceLock)
            {
                Type serviceType = typeof(T);
                
                if (services.ContainsKey(serviceType))
                {
                    Debugger.LogWarning($"[ServiceContainer] Service {serviceType.Name} already registered, replacing...");
                    Unregister<T>();
                }
                
                services[serviceType] = service;
                
                // Track updatable services
                if (service is IUpdatableService updatable)
                {
                    updatableServices.Add(updatable);
                }
                
                // Track scene-aware services
                if (service is ISceneAwareService sceneAware)
                {
                    sceneAwareServices.Add(sceneAware);
                }
                
                // Initialize the service
                try
                {
                    service.Initialize();
                    Debugger.Log($"[ServiceContainer] Registered service: {serviceType.Name}");
                }
                catch (Exception e)
                {
                    Debugger.LogError($"[ServiceContainer] Failed to initialize service {serviceType.Name}: {e.Message}");
                    services.Remove(serviceType);
                    throw;
                }
                
                // If container is already initialized, notify the new service
                if (isInitialized)
                {
                    service.OnServicesReady();
                }
            }
        }
        
        /// <summary>
        /// Register a service by its interface type.
        /// </summary>
        public void Register<TInterface, TImplementation>(TImplementation service) 
            where TInterface : class, IService
            where TImplementation : class, TInterface
        {
            lock (serviceLock)
            {
                Type interfaceType = typeof(TInterface);
                
                if (services.ContainsKey(interfaceType))
                {
                    Debugger.LogWarning($"[ServiceContainer] Service {interfaceType.Name} already registered, replacing...");
                    UnregisterByType(interfaceType);
                }
                
                services[interfaceType] = service;
                
                // Track updatable services
                if (service is IUpdatableService updatable)
                {
                    updatableServices.Add(updatable);
                }
                
                // Track scene-aware services
                if (service is ISceneAwareService sceneAware)
                {
                    sceneAwareServices.Add(sceneAware);
                }
                
                // Initialize the service
                try
                {
                    service.Initialize();
                    Debugger.Log($"[ServiceContainer] Registered service: {interfaceType.Name} -> {service.GetType().Name}");
                }
                catch (Exception e)
                {
                    Debugger.LogError($"[ServiceContainer] Failed to initialize service {interfaceType.Name}: {e.Message}");
                    services.Remove(interfaceType);
                    throw;
                }
                
                // If container is already initialized, notify the new service
                if (isInitialized)
                {
                    service.OnServicesReady();
                }
            }
        }
        
        /// <summary>
        /// Get a registered service.
        /// </summary>
        public T Get<T>() where T : class, IService
        {
            lock (serviceLock)
            {
                Type serviceType = typeof(T);
                
                if (services.TryGetValue(serviceType, out IService service))
                {
                    return service as T;
                }
                
                Debugger.LogError($"[ServiceContainer] Service {serviceType.Name} not found");
                return null;
            }
        }
        
        /// <summary>
        /// Try to get a registered service.
        /// </summary>
        public bool TryGet<T>(out T service) where T : class, IService
        {
            lock (serviceLock)
            {
                Type serviceType = typeof(T);
                
                if (services.TryGetValue(serviceType, out IService foundService))
                {
                    service = foundService as T;
                    return service != null;
                }
                
                service = null;
                return false;
            }
        }
        
        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        public bool Has<T>() where T : class, IService
        {
            lock (serviceLock)
            {
                return services.ContainsKey(typeof(T));
            }
        }
        
        /// <summary>
        /// Unregister a service.
        /// </summary>
        public void Unregister<T>() where T : class, IService
        {
            UnregisterByType(typeof(T));
        }
        
        private void UnregisterByType(Type serviceType)
        {
            lock (serviceLock)
            {
                if (services.TryGetValue(serviceType, out IService service))
                {
                    // Remove from tracking lists
                    if (service is IUpdatableService updatable)
                    {
                        updatableServices.Remove(updatable);
                    }
                    
                    if (service is ISceneAwareService sceneAware)
                    {
                        sceneAwareServices.Remove(sceneAware);
                    }
                    
                    // Shutdown the service
                    try
                    {
                        service.Shutdown();
                    }
                    catch (Exception e)
                    {
                        Debugger.LogError($"[ServiceContainer] Error shutting down service {serviceType.Name}: {e.Message}");
                    }
                    
                    services.Remove(serviceType);
                    Debugger.Log($"[ServiceContainer] Unregistered service: {serviceType.Name}");
                }
            }
        }
        
        /// <summary>
        /// Initialize all registered services.
        /// Call this after all services have been registered.
        /// </summary>
        public void InitializeServices()
        {
            lock (serviceLock)
            {
                if (isInitialized)
                {
                    Debugger.LogWarning("[ServiceContainer] Services already initialized");
                    return;
                }
                
                // Notify all services that registration is complete
                foreach (var service in services.Values)
                {
                    try
                    {
                        service.OnServicesReady();
                    }
                    catch (Exception e)
                    {
                        Debugger.LogError($"[ServiceContainer] Error in OnServicesReady for {service.GetType().Name}: {e.Message}");
                    }
                }
                
                isInitialized = true;
                Debugger.Log($"[ServiceContainer] Initialized {services.Count} services");
            }
        }
        
        /// <summary>
        /// Notify all scene-aware services of a scene change.
        /// </summary>
        public void NotifySceneChange(string previousScene, string newScene)
        {
            foreach (var service in sceneAwareServices)
            {
                try
                {
                    service.OnSceneChanged(previousScene, newScene);
                    
                    // Update active state for updatable services
                    if (service is IUpdatableService updatable)
                    {
                        updatable.IsActive = service.IsActiveInScene(newScene);
                    }
                }
                catch (Exception e)
                {
                    Debugger.LogError($"[ServiceContainer] Error notifying scene change to {service.GetType().Name}: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Shutdown all services and clear the container.
        /// </summary>
        public void Shutdown()
        {
            lock (serviceLock)
            {
                // Shutdown services in reverse order
                var serviceList = services.Values.ToList();
                serviceList.Reverse();
                
                foreach (var service in serviceList)
                {
                    try
                    {
                        service.Shutdown();
                    }
                    catch (Exception e)
                    {
                        Debugger.LogError($"[ServiceContainer] Error shutting down service {service.GetType().Name}: {e.Message}");
                    }
                }
                
                services.Clear();
                updatableServices.Clear();
                sceneAwareServices.Clear();
                isInitialized = false;
                
                Debugger.Log("[ServiceContainer] All services shutdown");
            }
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                Shutdown();
                instance = null;
            }
        }
        
        /// <summary>
        /// Get diagnostic information about registered services.
        /// </summary>
        public string GetDiagnostics()
        {
            lock (serviceLock)
            {
                var diagnostics = $"[ServiceContainer Diagnostics]\n";
                diagnostics += $"Total Services: {services.Count}\n";
                diagnostics += $"Updatable Services: {updatableServices.Count}\n";
                diagnostics += $"Scene-Aware Services: {sceneAwareServices.Count}\n";
                diagnostics += $"Initialized: {isInitialized}\n\n";
                
                diagnostics += "Registered Services:\n";
                foreach (var kvp in services)
                {
                    var service = kvp.Value;
                    diagnostics += $"  - {kvp.Key.Name}";
                    
                    if (service is IUpdatableService updatable)
                    {
                        diagnostics += $" [Updatable, Active: {updatable.IsActive}]";
                    }
                    
                    if (service is ISceneAwareService)
                    {
                        diagnostics += " [Scene-Aware]";
                    }
                    
                    diagnostics += "\n";
                }
                
                return diagnostics;
            }
        }
    }
}