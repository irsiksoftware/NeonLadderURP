using NeonLadder.Managers;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Service interface for loot management.
    /// </summary>
    public interface ILootService : IGameService
    {
        LootDropManager LootDropManager { get; }
        LootPurchaseManager LootPurchaseManager { get; }
    }
}