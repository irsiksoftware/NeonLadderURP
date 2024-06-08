using NeonLadder.Core;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Items
{
    public abstract class Collectible : MonoBehaviour
    {
        public int amount;

        protected PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public abstract void OnCollect();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                OnCollect();
            }
        }
    }
}
