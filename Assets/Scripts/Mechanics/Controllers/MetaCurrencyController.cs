namespace NeonLadder.Mechanics.Controllers
{
    public class MetaCurrencyController : BaseCurrencyController
    {
        protected override void Update()
        {
            currencyTextMeshPro.text = player.MetaCurrency.current.ToString();
        }
    }
}
