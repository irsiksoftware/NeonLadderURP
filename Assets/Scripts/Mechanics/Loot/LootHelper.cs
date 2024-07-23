using NeonLadder.Common;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

public static class LootHelper
{
    public static LootTable LoadLootTable(Enemy mob)
    {
        LootTable RuntimeLootTable = null;
        switch (mob)
        {
            case FlyingMinor:
            case Minor:
                RuntimeLootTable = Resources.Load<LootTable>(Constants.MinorEnemyLootTablePath);
                break;
            case Major:
                RuntimeLootTable = Resources.Load<LootTable>(Constants.MajorEnemyLootTablePath);
                break;
            case Boss:
                RuntimeLootTable = Resources.Load<LootTable>(Constants.BossEnemyLootTablePath);
                break;
            default:
                Debug.LogError($"LootTable not found for enemy: {mob}");
                break;
        }


        if (RuntimeLootTable == null)
        {
            Debug.LogError($"LootTable not found for enemy: {mob}");
        }
        return RuntimeLootTable;
    }
}
