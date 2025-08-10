using UnityEngine;
using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Debugging;

namespace NeonLadder.Core.ServiceContainer.Services
{
    #region Event Service
    
    public interface IEventService : IService
    {
        void TriggerEvent(string eventName, object data = null);
        void RegisterListener(string eventName, System.Action<object> listener);
        void UnregisterListener(string eventName, System.Action<object> listener);
    }
    
    public class EventService : BaseManagerService, IEventService
    {
        private EventManager eventManager;
        
        public EventService(EventManager manager) : base(manager)
        {
            eventManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            return true; // Always active
        }
        
        public void TriggerEvent(string eventName, object data = null)
        {
            // Implementation depends on EventManager's public API
        }
        
        public void RegisterListener(string eventName, System.Action<object> listener)
        {
            // Implementation depends on EventManager's public API
        }
        
        public void UnregisterListener(string eventName, System.Action<object> listener)
        {
            // Implementation depends on EventManager's public API
        }
    }
    
    #endregion
    
    #region Dialogue Service
    
    public interface IDialogueService : IService
    {
        void StartDialogue(string dialogueId);
        void AdvanceDialogue();
        bool IsDialogueActive();
    }
    
    public class DialogueService : BaseManagerService, IDialogueService
    {
        private DialogueManager dialogueManager;
        
        public DialogueService(DialogueManager manager) : base(manager)
        {
            dialogueManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            return true; // Active in all scenes
        }
        
        public void StartDialogue(string dialogueId)
        {
            // Implementation depends on DialogueManager's public API
        }
        
        public void AdvanceDialogue()
        {
            // Implementation depends on DialogueManager's public API
        }
        
        public bool IsDialogueActive()
        {
            // Implementation depends on DialogueManager's public API
            return false;
        }
    }
    
    #endregion
    
    #region Loot Service
    
    public interface ILootService : ISceneAwareService
    {
        void DropLoot(Vector3 position, int amount);
        void PurchaseItem(string itemId);
    }
    
    public class LootDropService : BaseManagerService, ILootService, IUpdatableService
    {
        private LootDropManager lootDropManager;
        private LootPurchaseManager lootPurchaseManager;
        public bool IsActive { get; set; }
        
        public LootDropService(LootDropManager dropManager, LootPurchaseManager purchaseManager) 
            : base(dropManager)
        {
            lootDropManager = dropManager;
            lootPurchaseManager = purchaseManager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            var scene = ParseScene(sceneName);
            switch (scene)
            {
                case Scenes.Start:
                case Scenes.Recife_2050_Final:
                    SetManagerEnabled(true);
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = false;
                    return true;
                    
                case Scenes.Staging:
                case Scenes.MetaShop:
                case Scenes.PermaShop:
                    SetManagerEnabled(false);
                    if (lootPurchaseManager != null)
                        lootPurchaseManager.enabled = true;
                    return true;
                    
                default:
                    return false;
            }
        }
        
        public override void OnSceneChanged(string previousScene, string newScene)
        {
            base.OnSceneChanged(previousScene, newScene);
            IsActive = IsActiveInScene(newScene);
        }
        
        public void Update()
        {
            // Any per-frame updates if needed
        }
        
        public void DropLoot(Vector3 position, int amount)
        {
            // Implementation depends on LootDropManager's public API
        }
        
        public void PurchaseItem(string itemId)
        {
            // Implementation depends on LootPurchaseManager's public API
        }
        
        private Scenes ParseScene(string sceneName)
        {
            if (System.Enum.TryParse<Scenes>(sceneName, out Scenes scene))
                return scene;
            return Scenes.Title;
        }
    }
    
    #endregion
    
    #region Camera Service
    
    public interface ICameraService : ISceneAwareService
    {
        void SetCameraPosition(Vector3 position);
        void ResetCameraPosition();
        void EmptySceneStates();
    }
    
    public class CameraPositionService : BaseManagerService, ICameraService
    {
        private PlayerCameraPositionManager cameraManager;
        
        public CameraPositionService(PlayerCameraPositionManager manager) : base(manager)
        {
            cameraManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            var scene = ParseScene(sceneName);
            return scene == Scenes.Staging;
        }
        
        public override void OnSceneChanged(string previousScene, string newScene)
        {
            base.OnSceneChanged(previousScene, newScene);
            
            if (ParseScene(newScene) == Scenes.Staging)
            {
                SetManagerEnabled(true);
                EmptySceneStates();
            }
            else
            {
                SetManagerEnabled(false);
            }
        }
        
        public void SetCameraPosition(Vector3 position)
        {
            // Implementation depends on PlayerCameraPositionManager's public API
        }
        
        public void ResetCameraPosition()
        {
            // Implementation depends on PlayerCameraPositionManager's public API
        }
        
        public void EmptySceneStates()
        {
            if (cameraManager != null)
            {
                cameraManager.EmptySceneStates();
            }
        }
        
        private Scenes ParseScene(string sceneName)
        {
            if (System.Enum.TryParse<Scenes>(sceneName, out Scenes scene))
                return scene;
            return Scenes.Title;
        }
    }
    
    #endregion
    
    #region Game Controller Service
    
    public interface IGameControllerService : ISceneAwareService
    {
        void SetControllerEnabled(bool enabled);
    }
    
    public class GameControllerService : BaseManagerService, IGameControllerService
    {
        private GameControllerManager gameControllerManager;
        
        public GameControllerService(GameControllerManager manager) : base(manager)
        {
            gameControllerManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            var scene = ParseScene(sceneName);
            return scene == Scenes.Title;
        }
        
        public override void OnSceneChanged(string previousScene, string newScene)
        {
            base.OnSceneChanged(previousScene, newScene);
            SetManagerEnabled(IsActiveInScene(newScene));
        }
        
        public void SetControllerEnabled(bool enabled)
        {
            SetManagerEnabled(enabled);
        }
        
        private Scenes ParseScene(string sceneName)
        {
            if (System.Enum.TryParse<Scenes>(sceneName, out Scenes scene))
                return scene;
            return Scenes.Title;
        }
    }
    
    #endregion
    
    #region Scene Change Service
    
    public interface ISceneChangeService : IService
    {
        void ChangeScene(string sceneName);
        void AssignSceneExit(string exitId, string targetScene);
        void CycleToNextScene();
    }
    
    public class SceneChangeService : BaseManagerService, ISceneChangeService
    {
        private SceneChangeManager sceneChangeManager;
        private SceneExitAssignmentManager exitManager;
        private SceneCycleManager cycleManager;
        
        public SceneChangeService(SceneChangeManager changeManager, 
                                 SceneExitAssignmentManager exitMgr,
                                 SceneCycleManager cycleMgr) 
            : base(changeManager)
        {
            sceneChangeManager = changeManager;
            exitManager = exitMgr;
            cycleManager = cycleMgr;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            return true; // Active in all scenes
        }
        
        public void ChangeScene(string sceneName)
        {
            // Implementation depends on SceneChangeManager's public API
        }
        
        public void AssignSceneExit(string exitId, string targetScene)
        {
            // Implementation depends on SceneExitAssignmentManager's public API
        }
        
        public void CycleToNextScene()
        {
            // Implementation depends on SceneCycleManager's public API
        }
    }
    
    #endregion
    
    #region Monster Group Service
    
    public interface IMonsterGroupService : ISceneAwareService
    {
        void ActivateMonsterGroup(string groupId);
        void DeactivateAllGroups();
    }
    
    public class MonsterGroupService : BaseManagerService, IMonsterGroupService
    {
        private MonsterGroupActivationManager monsterManager;
        
        public MonsterGroupService(MonsterGroupActivationManager manager) : base(manager)
        {
            monsterManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            var scene = ParseScene(sceneName);
            switch (scene)
            {
                case Scenes.Start:
                case Scenes.Recife_2050_Final:
                    return true;
                    
                case Scenes.Staging:
                case Scenes.MetaShop:
                case Scenes.PermaShop:
                    return false;
                    
                default:
                    return false;
            }
        }
        
        public override void OnSceneChanged(string previousScene, string newScene)
        {
            base.OnSceneChanged(previousScene, newScene);
            SetManagerEnabled(IsActiveInScene(newScene));
        }
        
        public void ActivateMonsterGroup(string groupId)
        {
            // Implementation depends on MonsterGroupActivationManager's public API
        }
        
        public void DeactivateAllGroups()
        {
            // Implementation depends on MonsterGroupActivationManager's public API
        }
        
        private Scenes ParseScene(string sceneName)
        {
            if (System.Enum.TryParse<Scenes>(sceneName, out Scenes scene))
                return scene;
            return Scenes.Title;
        }
    }
    
    #endregion
}