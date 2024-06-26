namespace NeonLadder.Mechanics.Controllers
{
    public class PermaCurrencyController : BaseCurrencyController
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            currencyTextMeshPro.text = player.PermaCurrency.current.ToString();
        }
    }
}