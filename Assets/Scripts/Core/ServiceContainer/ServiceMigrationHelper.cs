using UnityEngine;
using NeonLadder.Core.ServiceContainer.Services;
using NeonLadder.Managers;
using NeonLadder.Debugging;

namespace NeonLadder.Core.ServiceContainer
{
    /// <summary>
    /// Helper class to facilitate migration from singleton ManagerController to ServiceContainer.
    /// Provides compatibility layer and migration utilities.
    /// </summary>
    public static class ServiceMigrationHelper
    {
        private static bool migrationWarningsEnabled = true;
        
        /// <summary>
        /// Get a service using the legacy ManagerController.Instance pattern.
        /// This method provides backward compatibility during migration.
        /// </summary>
        public static T GetManagerService<T>() where T : class, IService
        {
            // Try to get from ServiceContainer first
            if (ServiceContainer.Instance != null && ServiceContainer.Instance.Has<T>())
            {
                return ServiceContainer.Instance.Get<T>();
            }
            
            // Fallback to legacy ManagerController if available
            if (ManagerController.Instance != null)
            {
                LogMigrationWarning($"Using legacy ManagerController for {typeof(T).Name}. Please migrate to ServiceContainer.");
                
                // Map service interfaces to legacy manager properties
                return GetLegacyManager<T>();
            }
            
            Debugger.LogError($"[ServiceMigrationHelper] Could not find service {typeof(T).Name}");
            return null;
        }
        
        /// <summary>
        /// Map service interfaces to legacy manager instances.
        /// </summary>
        private static T GetLegacyManager<T>() where T : class, IService
        {
            if (ManagerController.Instance == null)
                return null;
            
            var type = typeof(T);
            
            // Map interfaces to legacy managers
            if (type == typeof(IEventService))
            {
                return new EventService(ManagerController.Instance.eventManager) as T;
            }
            else if (type == typeof(IEnemyDefeatedService))
            {
                return new EnemyDefeatedService(ManagerController.Instance.enemyDefeatedManager) as T;
            }
            else if (type == typeof(IDialogueService))
            {
                return new DialogueService(ManagerController.Instance.dialogueManager) as T;
            }
            else if (type == typeof(ILootService))
            {
                return new LootDropService(
                    ManagerController.Instance.lootDropManager,
                    ManagerController.Instance.lootPurchaseManager) as T;
            }
            else if (type == typeof(ICameraService))
            {
                return new CameraPositionService(ManagerController.Instance.playerCameraPositionManager) as T;
            }
            else if (type == typeof(IGameControllerService))
            {
                return new GameControllerService(ManagerController.Instance.gameControllerManager) as T;
            }
            else if (type == typeof(ISceneChangeService))
            {
                return new SceneChangeService(
                    ManagerController.Instance.sceneChangeManager,
                    ManagerController.Instance.sceneExitAssignmentManager,
                    ManagerController.Instance.sceneCycleManager) as T;
            }
            else if (type == typeof(IMonsterGroupService))
            {
                return new MonsterGroupService(ManagerController.Instance.monsterGroupActivationManager) as T;
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if the system is using ServiceContainer or legacy ManagerController.
        /// </summary>
        public static bool IsUsingServiceContainer()
        {
            // Check for RefactoredManagerController with service container enabled
            var refactored = Object.FindObjectOfType<RefactoredManagerController>();
            if (refactored != null)
            {
                return true;
            }
            
            // Check if ServiceContainer has services registered
            if (ServiceContainer.Instance != null)
            {
                return ServiceContainer.Instance.Has<IEventService>() ||
                       ServiceContainer.Instance.Has<IDialogueService>() ||
                       ServiceContainer.Instance.Has<ILootService>();
            }
            
            return false;
        }
        
        /// <summary>
        /// Migrate a MonoBehaviour to use ServiceContainer.
        /// </summary>
        public static void MigrateComponent(MonoBehaviour component)
        {
            if (!IsUsingServiceContainer())
            {
                LogMigrationWarning($"ServiceContainer not available for {component.GetType().Name}");
                return;
            }
            
            // Example migration patterns:
            // Replace: ManagerController.Instance.eventManager
            // With: ServiceContainer.Instance.Get<IEventService>()
            
            LogMigrationInfo($"Component {component.GetType().Name} should be migrated to use ServiceContainer");
        }
        
        /// <summary>
        /// Enable or disable migration warnings.
        /// </summary>
        public static void SetMigrationWarningsEnabled(bool enabled)
        {
            migrationWarningsEnabled = enabled;
        }
        
        private static void LogMigrationWarning(string message)
        {
            if (migrationWarningsEnabled)
            {
                Debugger.LogWarning($"[Migration] {message}");
            }
        }
        
        private static void LogMigrationInfo(string message)
        {
            if (migrationWarningsEnabled)
            {
                Debugger.Log($"[Migration] {message}");
            }
        }
        
        /// <summary>
        /// Extension methods for easier migration.
        /// </summary>
        public static class Extensions
        {
            /// <summary>
            /// Get a service from either ServiceContainer or legacy ManagerController.
            /// </summary>
            public static T GetService<T>(this MonoBehaviour component) where T : class, IService
            {
                return GetManagerService<T>();
            }
            
            /// <summary>
            /// Try to get a service, returning false if not available.
            /// </summary>
            public static bool TryGetService<T>(this MonoBehaviour component, out T service) where T : class, IService
            {
                service = GetManagerService<T>();
                return service != null;
            }
        }
    }
    
    /// <summary>
    /// Attribute to mark classes that have been migrated to use ServiceContainer.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ServiceContainerMigratedAttribute : System.Attribute
    {
        public string MigrationDate { get; set; }
        public string MigratedBy { get; set; }
        
        public ServiceContainerMigratedAttribute(string date = "", string by = "")
        {
            MigrationDate = date;
            MigratedBy = by;
        }
    }
}