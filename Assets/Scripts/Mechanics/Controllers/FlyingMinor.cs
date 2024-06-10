namespace NeonLadder.Mechanics.Controllers
{
    public class FlyingMinor : Minor
    {
        protected override float AttackRange { get; set; } = 5f;
        protected override bool RetreatWhenTooClose { get; set; } = true;
    }
}
