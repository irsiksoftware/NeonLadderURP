namespace NeonLadder.Items.Loot
{
    [System.Serializable]
    public class LootItem
    {
        public Collectible collectiblePrefab;
        public float dropProbability;
        public int minAmount; 
        public int maxAmount; 
        public float healthThreshold; 
        public bool AlwaysDrop => dropProbability >= 100;
    }
}