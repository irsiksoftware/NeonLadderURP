using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using UnityEngine;

namespace NeonLadder.Items
{
    public abstract class Collectible : MonoBehaviour
    {
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
