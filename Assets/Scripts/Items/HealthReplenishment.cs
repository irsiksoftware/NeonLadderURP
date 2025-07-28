using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Items
{
    public class HealthReplenishment : Collectible
    {
        public override void OnCollect()
        {
            var healthComponent = model.Player.GetComponentInParent<Health>(); // Health is a component that is attached to the player, refactor to collision event.

            if (healthComponent != null && healthComponent.IsAlive)
            {
                // Schedule healing through event system
                model.Player.ScheduleHealing(amount, 0f);
                Destroy(gameObject);
            }
        }
    }
}
