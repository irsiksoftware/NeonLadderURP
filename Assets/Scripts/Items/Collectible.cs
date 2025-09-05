using NeonLadder.Core;
using NeonLadder.Models;
using NeonLadder.Items.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Items
{
    public abstract class Collectible : MonoBehaviour
    {
        [Header("Legacy Support")]
        public int amount; // Kept for backwards compatibility
        
        [Header("New Item System")]
        [SerializeField] protected ItemInstance itemInstance;
        [SerializeField] protected ItemDefinition fallbackDefinition; // For items placed directly in scene
        
        protected PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        protected Player player;
        
        /// <summary>
        /// Set the item instance for spawned collectibles
        /// </summary>
        public virtual void SetItemInstance(ItemInstance instance)
        {
            itemInstance = instance;
            // Update legacy amount field for backwards compatibility
            if (instance != null)
            {
                amount = instance.Stacks;
            }
        }
        
        /// <summary>
        /// Get the current item instance, creating one if needed
        /// </summary>
        protected virtual ItemInstance GetOrCreateInstance()
        {
            if (itemInstance == null && fallbackDefinition != null)
            {
                // Create instance from fallback definition for scene-placed items
                itemInstance = new ItemInstance(fallbackDefinition, amount > 0 ? amount : 1);
            }
            return itemInstance;
        }

        /// <summary>
        /// Legacy collection method - override in derived classes
        /// </summary>
        public abstract void OnCollect();
        
        /// <summary>
        /// New collection method that works with ItemInstance
        /// </summary>
        protected virtual void OnCollectItem(Player collector)
        {
            var instance = GetOrCreateInstance();
            
            if (instance?.Definition != null)
            {
                // TODO: Apply item effects when implemented
                // For now, just log the collection
                Debug.Log($"Player collected: {instance.Definition.DisplayName} x{instance.Stacks}");
                
                // Play pickup sound
                if (instance.Definition.PickupSound != null && collector.audioSource != null)
                {
                    collector.audioSource.PlayOneShot(instance.Definition.PickupSound);
                }
                
                // Play pickup VFX
                if (instance.Definition.PickupVFX != null)
                {
                    Instantiate(instance.Definition.PickupVFX, transform.position, Quaternion.identity);
                }
            }
            
            // Call legacy method for backwards compatibility
            OnCollect();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                player = other.GetComponent<Player>();
                if (player != null)
                {
                    OnCollectItem(player);
                }
            }
        }
    }
}
