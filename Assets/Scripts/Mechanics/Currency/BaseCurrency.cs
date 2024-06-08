using UnityEngine;

namespace NeonLadder.Mechanics.Currency
{
    public class BaseCurrency : MonoBehaviour
    {

        [SerializeField]
        public int current;
        public int max = int.MaxValue;
        public bool IsDepleted => current == 0;
        //public DamageNumber damageNumber;

        public void Increment(int amount = 1)
        {
            current += amount;
        }

        public void Decrement(int amount = 1)
        {
            current -= amount;

            switch (this)
            {
                case Meta:
                    //damageNumber.Spawn(transform.position, amount.ToString());
                    break;
                case Perma:
                    //damageNumber.Spawn(transform.position, amount.ToString());
                    break;
            }

            if (current == 0)
            {
                OnDepleted();
            }
        }

        public void Deplete()
        {
            current = 0;
        }

        protected virtual void OnDepleted()
        {

        }

        protected virtual void Awake()
        {

        }

        protected virtual void RestoreToMax()
        {
            current = max;
        }
    }
}
