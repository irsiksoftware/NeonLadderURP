namespace NeonLadder.Items
{
    public class MetaCurrencyReplenishment : Collectible
    {
        public override void OnCollect()
        {
            model.Player.AddMetaCurrency(amount);
            Destroy(gameObject);
        }
    }
}
