//using DamageNumbersPro;
using UnityEngine;

namespace NeonLadder.Mechanics.Stats
{
    public abstract class BaseStat : MonoBehaviour
    {
        [SerializeField]
        public float current;
        public float max = 100;
        public bool IsDepleted => current == 0;
        //public DamageNumber damageNumber;

        public void Increment(float amount = 1)
        {
            current = Mathf.Clamp(current + amount, 0, max);
        }

        public void Decrement(float amount = 1)
        {
            current = Mathf.Clamp(current - amount, 0, max);

            //use parent objects type to determine if damage or stamina

            switch (this)
            {
                case Health:
                    //damageNumber.Spawn(transform.position, amount.ToString());
                    break;
                case Stamina:
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
            while (current > 0) Decrement();
        }

        protected virtual void OnDepleted()
        {
            
        }

        protected virtual void Awake()
        {
            //current = max;
            RestoreToMax();
        }

        protected virtual void RestoreToMax()
        {
            current = max;
        }
    }
}
