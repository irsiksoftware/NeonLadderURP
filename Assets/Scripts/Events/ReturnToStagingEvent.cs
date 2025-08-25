using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.ProceduralGeneration;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event to handle returning to the Staging scene after boss victory
    /// </summary>
    public class ReturnToStagingEvent : BaseGameEvent<ReturnToStagingEvent>
    {
        public Boss completedBoss;

        public override void Execute()
        {
            Debug.Log($"[ReturnToStagingEvent] Transitioning to ReturnToStaging after defeating: {completedBoss?.gameObject?.name ?? "NULL"}");
            
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene("ReturnToStaging");
                Debug.Log("[ReturnToStagingEvent] Scene transition initiated successfully");
            }
            else
            {
                Debug.LogError("[ReturnToStagingEvent] SceneTransitionManager.Instance is null!");
            }
        }
    }
}