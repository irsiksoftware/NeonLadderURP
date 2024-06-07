using UnityEngine;

namespace NeonLadder.Items
{
    [System.Serializable]
    public class DropItem
    {
        public Collectible collectiblePrefab;
        public float dropProbability; // In percentage (0-100)
    }

    [CreateAssetMenu(fileName = "DropConfig", menuName = "ScriptableObjects/DropConfig", order = 1)]
    public class DropConfig : ScriptableObject
    {
        public DropItem[] dropItems;
    }
}
