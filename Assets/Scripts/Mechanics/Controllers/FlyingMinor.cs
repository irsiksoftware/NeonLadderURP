namespace NeonLadder.Mechanics.Controllers
{
    public class FlyingMinor : Minor
    {
        protected override float attackRange { get; set; } = 5f;
        protected override bool retreatWhenTooClose { get; set; } = true;
    }
}
