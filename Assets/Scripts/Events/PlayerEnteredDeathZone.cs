using NeonLadder.Core;
using NeonLadder.Mechanics;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when a player enters a trigger with a DeathZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredDeathZone"></typeparam>
    public class PlayerEnteredDeathZone : BaseGameEvent<PlayerEnteredDeathZone>
    {
        public DeathZone deathzone;

        public override void Execute()
        {
            Simulation.Schedule<PlayerDeath>(0);
        }
    }
}