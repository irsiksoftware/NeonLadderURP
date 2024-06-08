namespace NeonLadder.Items
{
    public class PermaCurrencyReplenishment : Collectible
    {
        public override void OnCollect()
        {
            model.Player.AddPermanentCurrency(amount);
            Destroy(gameObject);
        }
    }
}
