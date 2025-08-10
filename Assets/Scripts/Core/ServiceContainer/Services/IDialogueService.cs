using NeonLadder.Managers;

namespace NeonLadder.Core.ServiceContainer.Services
{
    /// <summary>
    /// Service interface for dialogue management.
    /// </summary>
    public interface IDialogueService : IGameService
    {
        DialogueManager Manager { get; }
    }
}