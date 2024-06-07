using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Items
{
    public class PermaCurrency : Collectible
    {
        public int amount;
        public override void OnCollect()
        {
            model.Player.AddPermanentCurrency(amount);
            Destroy(gameObject);
        }
    }
}
