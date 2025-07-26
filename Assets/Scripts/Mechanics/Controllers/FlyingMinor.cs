using NeonLadder.Common;

namespace NeonLadder.Mechanics.Controllers
{
    public class FlyingMinor : Minor
    {
        protected override float AttackRange { get; set; } = Constants.FlyingEnemyAttackRange;
        protected override bool RetreatWhenTooClose { get; set; } = true;
        public override bool IsUsingMelee { get; set; } = false;
    }
}
