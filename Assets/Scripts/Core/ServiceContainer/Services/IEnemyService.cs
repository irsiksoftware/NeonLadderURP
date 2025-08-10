using NeonLadder.Managers;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Service interface for enemy management.
    /// </summary>
    public interface IEnemyService : IGameService
    {
        EnemyDefeatedManager Manager { get; }
    }
}