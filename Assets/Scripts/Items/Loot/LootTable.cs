using UnityEngine;

namespace NeonLadder.Items.Loot
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "ScriptableObjects/LootTable", order = 1)]
    public class LootTable : ScriptableObject
    {
        public DropGroup[] dropGroups;
    }
}