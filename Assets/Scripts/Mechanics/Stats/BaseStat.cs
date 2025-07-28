//using DamageNumbersPro;
using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Mechanics.Stats
{
    public abstract class BaseStat : MonoBehaviour
    {
        [SerializeField]
        public float current;
        public float max = 100;
        public bool IsDepleted => current == 0;
        protected DamageNumberController damageNumberController;

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
                    if (damageNumberController != null)
                    {
                        damageNumberController.SpawnPopup(amount);
                    }
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
            current = 0;
        }

        protected virtual void OnDepleted()
        {
            
        }

        protected virtual void Awake()
        {
            RestoreToMax();
            damageNumberController = GetComponent<DamageNumberController>();
        }

        protected virtual void RestoreToMax()
        {
            current = max;
        }

        // Event-driven methods to replace direct Increment/Decrement calls
        public void ScheduleDamage(float amount, float delay = 0f)
        {
            // Schedule immediate or delayed damage
            current = Mathf.Clamp(current - amount, 0, max);
            
            // Trigger damage visual effects immediately for responsiveness
            TriggerDamageEffects(amount);
            
            if (current == 0)
            {
                OnDepleted();
            }
        }

        public void ScheduleRegeneration(float amount, float delay)
        {
            if (this is Health health)
            {
                var regenEvent = Simulation.Schedule<HealthRegenerationEvent>(delay);
                regenEvent.health = health;
                regenEvent.amount = amount;
            }
            else if (this is Stamina stamina)
            {
                var regenEvent = Simulation.Schedule<StaminaRegenerationEvent>(delay);
                regenEvent.stamina = stamina;
                regenEvent.amount = amount;
            }
        }

        public void ScheduleContinuousRegeneration(float regenPerSecond, float duration, float startDelay = 0f)
        {
            if (this is Stamina stamina)
            {
                var continuousEvent = Simulation.Schedule<StaminaContinuousRegenerationEvent>(startDelay);
                continuousEvent.stamina = stamina;
                continuousEvent.regenPerSecond = regenPerSecond;
                continuousEvent.duration = duration;
            }
        }

        private void TriggerDamageEffects(float amount)
        {
            switch (this)
            {
                case Health:
                    if (damageNumberController != null)
                    {
                        damageNumberController.SpawnPopup(amount);
                    }
                    break;
                case Stamina:
                    //damageNumber.Spawn(transform.position, amount.ToString());
                    break;
            }
        }
    }
}
