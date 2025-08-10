using NeonLadder.Managers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Service wrapper for EnemyDefeatedManager.
    /// </summary>
    public class EnemyDefeatedService : BaseManagerService, IEnemyDefeatedService
    {
        private EnemyDefeatedManager enemyManager;
        
        public EnemyDefeatedService(EnemyDefeatedManager manager) : base(manager)
        {
            enemyManager = manager;
        }
        
        public override bool IsActiveInScene(string sceneName)
        {
            // Active in all scenes
            return true;
        }
        
        public void RegisterEnemyDefeat(string enemyId)
        {
            // Delegate to the actual manager
            // Implementation would depend on EnemyDefeatedManager's public API
        }
    }
    
    /// <summary>
    /// Interface for enemy defeat tracking service.
    /// </summary>
    public interface IEnemyDefeatedService : IService
    {
        void RegisterEnemyDefeat(string enemyId);
    }
}