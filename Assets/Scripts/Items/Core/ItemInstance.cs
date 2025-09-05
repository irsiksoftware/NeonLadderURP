using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Items.Core
{
    /// <summary>
    /// Runtime instance of an item. This represents an actual item in the game world or inventory.
    /// While ItemDefinition is the template, ItemInstance is the actual item with runtime state.
    /// </summary>
    [Serializable]
    public class ItemInstance
    {
        [SerializeField] private ItemDefinition definition;
        [SerializeField] private int stacks = 1;
        [SerializeField] private Dictionary<string, object> metadata;
        
        // Runtime state
        private float createdTime;
        private string uniqueId;

        /// <summary>
        /// Create a new item instance from a definition
        /// </summary>
        public ItemInstance(ItemDefinition itemDef, int stackCount = 1)
        {
            definition = itemDef;
            stacks = Mathf.Max(1, stackCount);
            metadata = new Dictionary<string, object>();
            createdTime = Time.time;
            uniqueId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// The item definition this instance is based on
        /// </summary>
        public ItemDefinition Definition => definition;

        /// <summary>
        /// Current stack count for this item
        /// </summary>
        public int Stacks
        {
            get => stacks;
            set => stacks = Mathf.Clamp(value, 0, definition.IsStackable ? definition.MaxStack : 1);
        }

        /// <summary>
        /// Unique identifier for this specific instance
        /// </summary>
        public string UniqueId => uniqueId;

        /// <summary>
        /// Game time when this instance was created
        /// </summary>
        public float CreatedTime => createdTime;

        /// <summary>
        /// Add stacks to this item instance
        /// </summary>
        /// <returns>Number of stacks that couldn't be added (overflow)</returns>
        public int AddStacks(int amount)
        {
            if (!definition.IsStackable)
            {
                return amount; // Can't stack, return all as overflow
            }

            int availableSpace = definition.MaxStack - stacks;
            int toAdd = Mathf.Min(amount, availableSpace);
            stacks += toAdd;
            
            return amount - toAdd; // Return overflow
        }

        /// <summary>
        /// Remove stacks from this item instance
        /// </summary>
        /// <returns>True if the item still has stacks remaining</returns>
        public bool RemoveStacks(int amount)
        {
            stacks = Mathf.Max(0, stacks - amount);
            return stacks > 0;
        }

        /// <summary>
        /// Check if this instance can merge with another
        /// </summary>
        public bool CanMergeWith(ItemInstance other)
        {
            if (other == null || other.definition != definition)
                return false;
                
            if (!definition.IsStackable)
                return false;
                
            return stacks < definition.MaxStack;
        }

        /// <summary>
        /// Try to merge another instance into this one
        /// </summary>
        /// <returns>Number of stacks that were merged</returns>
        public int MergeFrom(ItemInstance other)
        {
            if (!CanMergeWith(other))
                return 0;

            int overflow = AddStacks(other.stacks);
            int merged = other.stacks - overflow;
            other.RemoveStacks(merged);
            
            return merged;
        }

        /// <summary>
        /// Set metadata for this instance (for special properties)
        /// </summary>
        public void SetMetadata(string key, object value)
        {
            if (metadata == null)
                metadata = new Dictionary<string, object>();
                
            metadata[key] = value;
        }

        /// <summary>
        /// Get metadata value
        /// </summary>
        public T GetMetadata<T>(string key, T defaultValue = default)
        {
            if (metadata == null || !metadata.ContainsKey(key))
                return defaultValue;
                
            try
            {
                return (T)metadata[key];
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Check if metadata key exists
        /// </summary>
        public bool HasMetadata(string key)
        {
            return metadata != null && metadata.ContainsKey(key);
        }

        /// <summary>
        /// Create a clone of this instance
        /// </summary>
        public ItemInstance Clone()
        {
            var clone = new ItemInstance(definition, stacks);
            
            if (metadata != null)
            {
                clone.metadata = new Dictionary<string, object>(metadata);
            }
            
            return clone;
        }

        /// <summary>
        /// Split this stack into two instances
        /// </summary>
        public ItemInstance Split(int amount)
        {
            if (!definition.IsStackable || amount >= stacks)
                return null;

            amount = Mathf.Min(amount, stacks - 1);
            RemoveStacks(amount);
            
            return new ItemInstance(definition, amount);
        }

        /// <summary>
        /// Get display name including stack count
        /// </summary>
        public string GetDisplayName()
        {
            if (definition.IsStackable && stacks > 1)
            {
                return $"{definition.DisplayName} x{stacks}";
            }
            return definition.DisplayName;
        }

        /// <summary>
        /// Get total value of this stack
        /// </summary>
        public int GetTotalValue(bool selling = false)
        {
            int baseValue = selling ? definition.SellValue : definition.BuyValue;
            return baseValue * stacks;
        }

        public override string ToString()
        {
            return $"ItemInstance: {GetDisplayName()} [ID: {uniqueId.Substring(0, 8)}]";
        }
    }
}