using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class DamageController : MonoBehaviour
    {
        public Health health { get; private set; }

        private void Start()
        {
            health = GetComponent<Health>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(Layers.Battle.ToString()))
            {
                if (TryGetComponentInHierarchy(other.transform, out ProjectileController projectile))
                {
                    Destroy(other.gameObject);
                    ApplyDamage(projectile.Damage);
                }
                else if (TryGetComponentInHierarchy(other.transform, out MeleeController melee))
                {
                    ApplyDamage(melee.Damage);
                }
            }
        }

        private void ApplyDamage(float damage)
        {
            health.Decrement(damage);
        }

        private bool TryGetComponentInHierarchy<T>(Transform transform, out T component) where T : Component
        {
            component = transform.GetComponent<T>();
            while (component == null && transform.parent != null)
            {
                transform = transform.parent;
                component = transform.GetComponent<T>();
            }
            return component != null;
        }
    }
}
