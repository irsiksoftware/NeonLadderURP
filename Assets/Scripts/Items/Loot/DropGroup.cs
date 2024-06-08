namespace NeonLadder.Items.Loot
{
    [System.Serializable]
    public class DropGroup
    {
        public LootItem[] lootItems;
        public int minDrops;
        public int maxDrops;
    }
}