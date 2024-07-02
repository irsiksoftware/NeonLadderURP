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
                //Debug.Log("Bullet hit the enemy!");
                Destroy(other.gameObject);
                health.Decrement(other.GetComponent<ProjectileController>().Damage);
            }
        }
    }
}
