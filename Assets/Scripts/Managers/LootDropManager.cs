using NeonLadder.Items;
using NeonLadder.Items.Loot;
using NeonLadder.Mechanics.Controllers;
using System.Collections;
using UnityEngine;

namespace NeonLadder.Managers
{
    public class LootDropManager : MonoBehaviour
    {
        void Start() { }

        void Awake()
        {
            enabled = false;
        }

        public void DropLoot(Player target, Enemy enemy)
        {
            if (enemy.RuntimeLootTable == null)
                return;

            foreach (var dropGroup in enemy.RuntimeLootTable.dropGroups)
            {
                int itemsToDrop = Random.Range(dropGroup.minDrops, dropGroup.maxDrops + 1);

                foreach (var lootItem in dropGroup.lootItems)
                {
                    if (itemsToDrop <= 0)
                        break;

                    if (ShouldDropItem(lootItem, target))
                    {
                        StartCoroutine(DelayedDropItem(lootItem, enemy.transform, enemy.DeathAnimationDuration + enemy.deathBuffer));
                        itemsToDrop--;
                    }
                }
            }
        }

        private IEnumerator DelayedDropItem(LootItem lootItem, Transform enemyTransform, float secondsDelay)
        {
            yield return new WaitForSeconds(secondsDelay);
            DropItem(lootItem, enemyTransform);
        }

        private static bool ShouldDropItem(LootItem lootItem, Player target)
        {
            if (lootItem.AlwaysDrop)
                return true;

            bool passesDropChance = Random.Range(0f, 100f) <= lootItem.dropProbability;
            bool belowHealthThreshold = target.Health.current / target.Health.max <= lootItem.healthThreshold;

            return passesDropChance && belowHealthThreshold;
        }

        private static void DropItem(LootItem lootItem, Transform enemyTransform)
        {
            if (enemyTransform != null)
            {
                Vector3 spawnPosition = enemyTransform.position;
                if (lootItem.collectiblePrefab != null)
                {
                    spawnPosition += StandardizedLootDropTransformations(lootItem.collectiblePrefab);
                }

                int amountToDrop = Random.Range(lootItem.minAmount, lootItem.maxAmount + 1);
                lootItem.collectiblePrefab.amount = amountToDrop;
                Instantiate(lootItem.collectiblePrefab, spawnPosition, Quaternion.identity);
            }
        }

        private static Vector3 StandardizedLootDropTransformations(Collectible prefab)
        {
            Vector3 deltaPosition = Vector3.zero;

            switch (prefab)
            {
                case HealthReplenishment:
                    deltaPosition = new Vector3(0f, .8f, 0f);
                    prefab.transform.localScale = new Vector3(.35f, .35f, .35f);
                    break;
                case MetaCurrencyReplenishment:
                    deltaPosition = new Vector3(0f, 0f, 0f);
                    prefab.transform.localScale = new Vector3(1f, 1f, 1f);
                    break;
                case PermaCurrencyReplenishment:
                    deltaPosition = new Vector3(0f, 1f, 0f);
                    prefab.transform.localScale = new Vector3(.75f, .75f, .75f);
                    break;
            }

            return deltaPosition;
        }
    }
}
