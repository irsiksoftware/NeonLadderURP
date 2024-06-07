using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Items
{
    public class MetaCurrency : Collectible
    {
        public int amount;
        public override void OnCollect()
        {
            model.player.AddMetaCurrency(amount);
            Destroy(gameObject);
        }
    }
}
