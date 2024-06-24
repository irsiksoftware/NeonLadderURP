using NeonLadder.Events;
using NeonLadder.Gameplay;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Mechanics.Stats
{
    public class Health : BaseStat
    {
        public bool IsAlive => !IsDepleted;

        new void Awake()
        {
            //max = Constants.MaxHealth; 
            base.Awake();
        }

        protected override void OnDepleted()
        {
            var ev = Schedule<HealthIsZero>();
            ev.health = this;
        }

        public void Die()
        {
            Deplete();
        }
    }
}
