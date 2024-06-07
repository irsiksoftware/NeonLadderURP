using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Items
{
    public class MetaCurrency : Collectible
    {
        public int amount;
        public override void OnCollect()
        {
            model.Player.AddMetaCurrency(amount);
            Destroy(gameObject);
        }
    }
}
