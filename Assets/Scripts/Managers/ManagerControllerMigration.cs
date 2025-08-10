using NeonLadder.Core.ServiceContainer;
using NeonLadder.Core.ServiceContainer.Services;
using UnityEngine;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Migration helper to transition from ManagerController singleton to service container.
    /// Provides backward compatibility while migrating to new architecture.
    /// </summary>
    public static class ManagerControllerMigration
    {
        private static GameServiceManager gameServiceManager;
        
        /// <summary>
        /// Gets the game service manager instance.
        /// Creates compatibility wrapper for legacy code.
        /// </summary>
        public static GameServiceManager GetGameServiceManager()
        {
            if (gameServiceManager == null)
            {
                // Try to get from service container first
                if (ServiceLocator.Instance.TryGet<GameServiceManager>(out var manager))
                {
                    gameServiceManager = manager;
                }
                else
                {
                    // Fallback: find in scene
                    gameServiceManager = GameObject.FindObjectOfType<GameServiceManager>();
                    if (gameServiceManager != null)
                    {
                        ServiceLocator.Instance.Register<GameServiceManager>(gameServiceManager);
                    }
                }
            }
            
            return gameServiceManager;
        }
        
        /// <summary>
        /// Gets the enemy defeated manager.
        /// Migration path: Replace ManagerController.Instance.enemyDefeatedManager
        /// with ManagerControllerMigration.GetEnemyDefeatedManager()
        /// </summary>
        public static EnemyDefeatedManager GetEnemyDefeatedManager()
        {
            if (ServiceLocator.Instance.TryGet<IEnemyService>(out var service))
            {
                return service.Manager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<EnemyDefeatedManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the dialogue manager.
        /// Migration path: Replace ManagerController.Instance.dialogueManager
        /// with ManagerControllerMigration.GetDialogueManager()
        /// </summary>
        public static DialogueManager GetDialogueManager()
        {
            if (ServiceLocator.Instance.TryGet<IDialogueService>(out var service))
            {
                return service.Manager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<DialogueManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the scene change manager.
        /// Migration path: Replace ManagerController.Instance.sceneChangeManager
        /// with ManagerControllerMigration.GetSceneChangeManager()
        /// </summary>
        public static SceneChangeManager GetSceneChangeManager()
        {
            if (ServiceLocator.Instance.TryGet<ISceneService>(out var service))
            {
                return service.SceneChangeManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<SceneChangeManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the loot drop manager.
        /// Migration path: Replace ManagerController.Instance.lootDropManager
        /// with ManagerControllerMigration.GetLootDropManager()
        /// </summary>
        public static LootDropManager GetLootDropManager()
        {
            if (ServiceLocator.Instance.TryGet<ILootService>(out var service))
            {
                return service.LootDropManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<LootDropManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the loot purchase manager.
        /// Migration path: Replace ManagerController.Instance.lootPurchaseManager
        /// with ManagerControllerMigration.GetLootPurchaseManager()
        /// </summary>
        public static LootPurchaseManager GetLootPurchaseManager()
        {
            if (ServiceLocator.Instance.TryGet<ILootService>(out var service))
            {
                return service.LootPurchaseManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<LootPurchaseManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the player camera position manager.
        /// Migration path: Replace ManagerController.Instance.playerCameraPositionManager
        /// with ManagerControllerMigration.GetPlayerCameraPositionManager()
        /// </summary>
        public static PlayerCameraPositionManager GetPlayerCameraPositionManager()
        {
            if (ServiceLocator.Instance.TryGet<ISceneService>(out var service))
            {
                return service.PlayerCameraPositionManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<PlayerCameraPositionManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the event manager.
        /// Migration path: Replace ManagerController.Instance.eventManager
        /// with ServiceLocator.Instance.Get<EventManager>()
        /// </summary>
        public static EventManager GetEventManager()
        {
            if (ServiceLocator.Instance.TryGet<EventManager>(out var eventManager))
            {
                return eventManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<EventManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the game controller manager.
        /// Migration path: Replace ManagerController.Instance.gameControllerManager
        /// with ServiceLocator.Instance.Get<GameControllerManager>()
        /// </summary>
        public static GameControllerManager GetGameControllerManager()
        {
            if (ServiceLocator.Instance.TryGet<GameControllerManager>(out var gameController))
            {
                return gameController;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<GameControllerManager>();
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets the monster group activation manager.
        /// Migration path: Replace ManagerController.Instance.monsterGroupActivationManager
        /// with ServiceLocator.Instance.Get<MonsterGroupActivationManager>()
        /// </summary>
        public static MonsterGroupActivationManager GetMonsterGroupActivationManager()
        {
            if (ServiceLocator.Instance.TryGet<MonsterGroupActivationManager>(out var monsterManager))
            {
                return monsterManager;
            }
            
            // Fallback for legacy code
            var manager = GetGameServiceManager();
            if (manager != null)
            {
                return manager.GetComponentInChildren<MonsterGroupActivationManager>();
            }
            
            return null;
        }
    }
}