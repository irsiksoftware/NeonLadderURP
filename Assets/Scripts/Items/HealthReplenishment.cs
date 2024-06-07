using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Items
{
    public class HealthReplenishment : Collectible
    {
        public int healthAmount;

        public override void OnCollect()
        {
            var healthComponent = model.Player.GetComponent<Health>();

            if (healthComponent != null && healthComponent.IsAlive)
            {
                healthComponent.Increment(healthAmount);
                Destroy(gameObject);
            }
        }
    }
}
