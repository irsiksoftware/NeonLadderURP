using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
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
            Debug.Log($"[BossDefeatedCutsceneEvent] Transitioning to ReturnToStaging after defeating: {completedBoss?.gameObject?.name ?? "NULL"}");
            
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene("BossDefeated");
                Debug.Log("[ReturnToSBossDefeatedCutsceneEventtagingEvent] Scene transition initiated successfully");
            }
            else
            {
                Debug.LogError("[BossDefeatedCutsceneEvent] SceneTransitionManager.Instance is null!");
            }
        }
    }
}