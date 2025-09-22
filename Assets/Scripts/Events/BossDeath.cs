using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.ProceduralGeneration;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Events
{
    public class BossDeath : BaseGameEvent<BossDeath>
    {
        public Boss boss;

        public override void Execute()
        {
            string bossName = boss.gameObject.name;
            Debugger.LogInformation(LogCategory.ProceduralGeneration,
                $"[BossDeath] EXECUTING BossDeath event for boss: '{bossName}'");

            Constants.DefeatedBosses.Add(bossName);

            // Mark boss as defeated in the procedural path system
            string baseBossName = GetBaseBossName(bossName);
            if (!string.IsNullOrEmpty(baseBossName))
            {
                SceneTransitionManager.Instance?.MarkBossAsDefeated(baseBossName);
                Debugger.LogInformation(LogCategory.ProceduralGeneration,
                    $"[BossDeath] Boss '{bossName}' defeated, marked base boss '{baseBossName}' as defeated in procedural system");
            }
            else
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration,
                    $"[BossDeath] Could not determine base boss name for defeated boss '{bossName}'");
            }

            boss.GetComponent<Collider>().enabled = false;
            boss.GetComponent<Animator>().enabled = false;
            if (boss.audioSource && boss.ouchAudio)
            {
                // Schedule death audio through event system
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioSource = boss.audioSource;
                audioEvent.audioClip = boss.ouchAudio;
                audioEvent.audioType = AudioEventType.Death;
            }
        }

        /// <summary>
        /// Convert boss GameObject name to base boss name for procedural system
        /// Handles both transformed names (e.g., "Onyscidus") and direct names (e.g., "Pride")
        /// </summary>
        private string GetBaseBossName(string bossGameObjectName)
        {
            // First check if it's a transformed boss (e.g., "Onyscidus" -> "Pride")
            var bossTransformations = BossTransformations.bossTransformations;
            var baseBoss = bossTransformations.FirstOrDefault(kvp => kvp.Value == bossGameObjectName).Key;

            if (!string.IsNullOrEmpty(baseBoss))
            {
                return baseBoss; // Found transformed boss mapping
            }

            // If not found in transformations, check if it's already a base boss name
            if (bossTransformations.ContainsKey(bossGameObjectName))
            {
                return bossGameObjectName; // It's already a base boss name
            }

            // If neither, it might be a direct boss name like "Devil"
            // Check if it matches any of the boss location identifiers
            var bossLocationData = BossLocationData.Locations;
            var matchingLocation = bossLocationData.FirstOrDefault(loc =>
                loc.Boss.Equals(bossGameObjectName, System.StringComparison.OrdinalIgnoreCase));

            if (matchingLocation != null)
            {
                return matchingLocation.Boss; // Found in boss location data
            }

            return null; // Could not determine base boss name
        }
    }
}
