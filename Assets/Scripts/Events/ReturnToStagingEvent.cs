using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event to handle returning to the Staging scene after boss victory
    /// </summary>
    public class BossDefeatedCutsceneEvent : BaseGameEvent<BossDefeatedCutsceneEvent>
    {
        public Boss completedBoss;

        public override void Execute()
        {
            Debug.Log($"[BossDefeatedCutsceneEvent] Transitioning to BossDefeated cutscene after defeating: {completedBoss?.gameObject?.name ?? "NULL"}");

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(Scenes.Cutscene.BossDefeated);
                Debug.Log("[BossDefeatedCutsceneEvent] Scene transition to cutscene initiated successfully");
                Debug.Log("[BossDefeatedCutsceneEvent] AutoScrollText in BossDefeated scene will handle return to Staging");
            }
            else
            {
                Debug.LogError("[BossDefeatedCutsceneEvent] SceneTransitionManager.Instance is null!");
            }
        }
    }
}