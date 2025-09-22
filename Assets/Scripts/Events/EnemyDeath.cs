using NeonLadder.Core;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Events
{
    public class EnemyDeath : BaseGameEvent<EnemyDeath>
    {
        public Enemy enemy;
        public Animator enemyAnimator;
        private GameObject thisActorParent;

        public override void Execute()
        {
            thisActorParent = enemy.transform.parent.gameObject;
            thisActorParent.GetComponent<Rigidbody>().useGravity = true;
            thisActorParent.GetComponentInChildren<CollisionController>().enabled = false;

            //change enemy layer to FX on death so bullets pass through.
            enemy.transform.parent.gameObject.layer = LayerMask.NameToLayer(Layers.Dead.ToString());

            //have to change the layer to avoid stopping player from walking over the enemy after they've died.
            var attackComponents = enemy.transform.parent.gameObject.GetComponentsInChildren<Collider>()
                                                    .Where(c => c.gameObject != enemy.transform.parent.gameObject);

            foreach (var attackComponent in attackComponents)
            {
                attackComponent.gameObject.layer = LayerMask.NameToLayer(nameof(Layers.Dead));
            }

            GameObject.FindGameObjectWithTag(Tags.Managers.ToString())
                      .GetComponentInChildren<NeonLadder.Managers.LootDropManager>()
                      .DropLoot(model.Player, enemy);

            var achievement = AchievementResolver.Resolve(enemy.transform.parent.name);
            if (achievement.HasValue)
            {
                // This is a boss! Mark it as defeated in the procedural system
                string transformedBossName = enemy.transform.parent.name; // e.g., "Onyscidus"
                string baseBossName = GetBaseBossName(transformedBossName); // e.g., "Pride"

                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[EnemyDeath] Boss defeated! Transformed: '{transformedBossName}' -> Base: '{baseBossName}'");

                if (!string.IsNullOrEmpty(baseBossName))
                {
                    SceneTransitionManager.Instance?.MarkBossAsDefeated(baseBossName);
                    Debugger.LogInformation(LogCategory.ProceduralGeneration,
                        $"[EnemyDeath] Successfully called MarkBossAsDefeated('{baseBossName}')");
                }
                else
                {
                    Debugger.LogWarning(LogCategory.ProceduralGeneration,
                        $"[EnemyDeath] Could not find base boss name for transformed boss '{transformedBossName}'");
                }

                NeonLadder.Managers.SteamManager.Instance.UnlockAchievement(achievement.Value.ToString());
                Simulation.Schedule<PlayerDefeatedBossEvent>(1f);
            }
        }

        /// <summary>
        /// Convert transformed boss name (e.g., "Onyscidus") back to base boss name (e.g., "Pride")
        /// </summary>
        private string GetBaseBossName(string transformedBossName)
        {
            // First check if it's a transformed boss (e.g., "Onyscidus" -> "Pride")
            var bossTransformations = BossTransformations.bossTransformations;
            var baseBoss = bossTransformations.FirstOrDefault(kvp => kvp.Value == transformedBossName).Key;

            if (!string.IsNullOrEmpty(baseBoss))
            {
                return baseBoss; // Found transformed boss mapping
            }

            // If not found in transformations, check if it's already a base boss name
            if (bossTransformations.ContainsKey(transformedBossName))
            {
                return transformedBossName; // It's already a base boss name
            }

            // If neither, it might be a direct boss name like "Devil"
            // Check if it matches any of the boss location identifiers
            var bossLocationData = BossLocationData.Locations;
            var matchingLocation = bossLocationData.FirstOrDefault(loc =>
                loc.Boss.Equals(transformedBossName, System.StringComparison.OrdinalIgnoreCase));

            if (matchingLocation != null)
            {
                return matchingLocation.Boss; // Found in boss location data
            }

            return null; // Could not determine base boss name
        }
    }
}
