using UnityEngine;
using NeonLadder.Core;
using NeonLadder.Items.Core;
using NeonLadder.Items;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event fired when an item needs to be spawned in the world.
    /// This decouples item spawning from the loot system.
    /// </summary>
    public class ItemSpawnEvent : BaseGameEvent<ItemSpawnEvent>
    {
        public ItemInstance itemInstance;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation = Quaternion.identity;
        public Vector3 spawnVelocity = Vector3.zero;
        public bool applyRandomForce = false;
        public float randomForceStrength = 5f;

        public override void Execute()
        {
            if (itemInstance?.Definition?.WorldPrefab == null)
            {
                Debug.LogWarning($"Cannot spawn item {itemInstance?.Definition?.DisplayName ?? "null"} - no world prefab assigned");
                return;
            }

            // Apply position offset from definition
            Vector3 finalPosition = spawnPosition + itemInstance.Definition.DropPositionOffset;

            // Instantiate the world object
            GameObject worldObject = Object.Instantiate(
                itemInstance.Definition.WorldPrefab,
                finalPosition,
                spawnRotation
            );

            // Apply scale from definition
            worldObject.transform.localScale = itemInstance.Definition.WorldScale;

            // Try to get or add a Collectible component
            var collectible = worldObject.GetComponent<Collectible>();
            if (collectible == null)
            {
                Debug.LogError($"Item prefab {itemInstance.Definition.DisplayName} missing Collectible component!");
                Object.Destroy(worldObject);
                return;
            }

            // Pass the item instance to the collectible
            collectible.SetItemInstance(itemInstance);

            // Apply physics if requested
            if (applyRandomForce)
            {
                var rb = worldObject.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = worldObject.AddComponent<Rigidbody>();
                }

                // Apply random upward/outward force for item scatter effect
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized;

                rb.linearVelocity = spawnVelocity + (randomDirection * randomForceStrength);
            }

            // Play spawn VFX if available
            if (itemInstance.Definition.PickupVFX != null)
            {
                // Could play a spawn effect here instead of pickup
                // For now, pickup VFX is played when collected
            }

            Debug.Log($"Spawned {itemInstance.GetDisplayName()} at {finalPosition}");
        }
    }
}