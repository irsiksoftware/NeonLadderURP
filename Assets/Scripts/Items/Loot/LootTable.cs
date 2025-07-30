using UnityEngine;

namespace NeonLadder.Items.Loot
{
    /// <summary>
    /// ScriptableObject for configuring loot drop tables with weighted probability groups.
    /// Used by enemies and treasure chests to determine item drops.
    /// </summary>
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "NeonLadder/Items/Loot Table")]
    public class LootTable : ScriptableObject
    {
        [Header("🎲 Loot Drop Configuration")]
        [Tooltip("Groups of items with their drop probabilities. Each group is evaluated independently.")]
        public DropGroup[] dropGroups;
    }
}