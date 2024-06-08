using NeonLadder.Events;

namespace NeonLadder.Mechanics.Currency
{
    public class Perma : BaseCurrency
    {

        public bool HasPermaCurrency => !IsDepleted;

        new void Awake()
        {
            //max = Constants.MaxHealth; 
            base.Awake();
        }
    }
}
