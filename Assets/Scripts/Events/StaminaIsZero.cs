using NeonLadder.Mechanics.Stats;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player stamina reaches 0. This usually would result in a 
    /// PlayerExhausted event.
    /// </summary>
    /// <typeparam name="StaminaIsZero"></typeparam>
    public class StaminaIsZero : BaseGameEvent<StaminaIsZero>
    {
        public override void Execute()
        {
            Schedule<PlayerExhausted>();
        }
    }
}