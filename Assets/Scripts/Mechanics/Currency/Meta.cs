namespace NeonLadder.Mechanics.Currency
{
    public class Meta : BaseCurrency
    {
        public bool HasMetaCurrency => !IsDepleted;

        new void Awake()
        {
            //max = Constants.MaxHealth; 
            base.Awake();
        }
    }
}
