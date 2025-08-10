using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Service interface for scene management.
    /// </summary>
    public interface ISceneService : IGameService
    {
        SceneChangeManager SceneChangeManager { get; }
        SceneCycleManager SceneCycleManager { get; }
        SceneExitAssignmentManager SceneExitAssignmentManager { get; }
        PlayerCameraPositionManager PlayerCameraPositionManager { get; }
        
        Scenes CurrentScene { get; }
        void ChangeScene(Scenes newScene);
    }
}