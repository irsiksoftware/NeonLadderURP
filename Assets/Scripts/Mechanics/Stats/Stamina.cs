using NeonLadder.Common;
using NeonLadder.Events;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Stats
{
    public class Stamina : BaseStat
    {
        public bool IsExhausted => IsDepleted;

        new void Awake()
        {
            max = Constants.MaxStamina;
            base.Awake();
        }

        protected override void OnDepleted()
        {
            Schedule<StaminaIsZero>();
        }

        public void Exhaust()
        {
            Deplete();
        }
    }
}
