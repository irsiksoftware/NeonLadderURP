using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Items.Enums;

namespace NeonLadder.Items.Core
{
    /// <summary>
    /// ScriptableObject that defines the properties and behaviors of an item.
    /// This is the data template - actual instances in game use ItemInstance.
    /// </summary>
    [CreateAssetMenu(fileName = "New Item", menuName = "NeonLadder/Items/Item Definition")]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Classification")]
        [SerializeField] private ItemType itemType = ItemType.Consumable;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private ItemTier tier = ItemTier.Tier1;

        [Header("Stacking")]
        [SerializeField] private bool isStackable = false;
        [SerializeField] private int maxStack = 99;

        [Header("Visuals")]
        [Tooltip("Prefab to spawn in the world when dropped")]
        [SerializeField] private GameObject worldPrefab;
        [Tooltip("VFX to play when item is picked up")]
        [SerializeField] private GameObject pickupVFX;
        [Tooltip("Audio clip to play on pickup")]
        [SerializeField] private AudioClip pickupSound;

        [Header("Drop Configuration")]
        [Tooltip("Base drop weight for loot tables")]
        [SerializeField] private float baseDropWeight = 1f;
        [Tooltip("Offset from drop origin when spawned")]
        [SerializeField] private Vector3 dropPositionOffset = new Vector3(0, 0.5f, 0);
        [Tooltip("Scale of the dropped item in world")]
        [SerializeField] private Vector3 worldScale = Vector3.one;

        [Header("Value")]
        [SerializeField] private int sellValue = 0;
        [SerializeField] private int buyValue = 0;

        // Public accessors
        public string ItemId => itemId;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite Icon => icon;
        public ItemType Type => itemType;
        public ItemRarity Rarity => rarity;
        public ItemTier Tier => tier;
        public bool IsStackable => isStackable;
        public int MaxStack => maxStack;
        public GameObject WorldPrefab => worldPrefab;
        public GameObject PickupVFX => pickupVFX;
        public AudioClip PickupSound => pickupSound;
        public float BaseDropWeight => baseDropWeight;
        public Vector3 DropPositionOffset => dropPositionOffset;
        public Vector3 WorldScale => worldScale;
        public int SellValue => sellValue;
        public int BuyValue => buyValue;

        /// <summary>
        /// Generates a formatted description
        /// </summary>
        public string GetFullDescription(int stacks = 1)
        {
            return description;
        }

        /// <summary>
        /// Gets the color associated with this item's rarity
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => Color.gray,
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
                ItemRarity.Rare => new Color(0.2f, 0.4f, 1f), // Blue
                ItemRarity.Epic => new Color(0.6f, 0.2f, 0.8f), // Purple
                ItemRarity.Legendary => new Color(1f, 0.5f, 0f), // Orange
                ItemRarity.Mythic => new Color(0.8f, 0.2f, 0.2f), // Red
                _ => Color.white
            };
        }

        private void OnValidate()
        {
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = name.ToLower().Replace(" ", "_");
            }

            // Ensure stackable items have reasonable max stack
            if (isStackable && maxStack < 1)
            {
                maxStack = 1;
            }

            // Validate buy/sell prices
            if (sellValue > buyValue && buyValue > 0)
            {
                Debug.LogWarning($"Item {displayName} has sell value higher than buy value!");
            }
        }
    }
}