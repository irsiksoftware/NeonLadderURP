namespace NeonLadder.Mechanics.Controllers
{
    public class FlyingMajor : Major
    {
        protected override float AttackRange { get; set; } = 5f;
        protected override bool RetreatWhenTooClose { get; set; } = true;
        public override bool IsUsingMelee { get; set; } = false;
    }
}
