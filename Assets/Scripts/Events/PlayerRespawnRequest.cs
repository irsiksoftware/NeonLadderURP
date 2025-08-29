using UnityEngine;
using NeonLadder.Core;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Events
{
    /// <summary>
    /// Reason for respawn request
    /// </summary>
    public enum RespawnReason
    {
        FellOutOfBounds,
        ManualRespawn,
        DeathByDamage,
        StuckInGeometry,
        Debug
    }
    
    /// <summary>
    /// Event that handles the full respawn sequence including fade effects and checkpoint restoration
    /// </summary>
    public class PlayerRespawnRequest : BaseGameEvent<PlayerRespawnRequest>
    {
        public RespawnReason reason = RespawnReason.FellOutOfBounds;
        public Vector3 fallPosition;
        public bool skipFadeEffects = false;
        
        public override void Execute()
        {
            if (!CheckpointManager.Instance.HasCheckpoint())
            {
                Debug.LogWarning("PlayerRespawnRequest: No checkpoint saved, falling back to scene restart");
                // Fallback to restarting the scene if no checkpoint exists
                var restartEvent = Simulation.Schedule<RestartScene>(0.5f);
                return;
            }
            
            var checkpointPos = CheckpointManager.Instance.GetLastCheckpoint();
            
            if (Debug.isDebugBuild)
            {
                Debug.Log($"PlayerRespawnRequest: Respawning player due to {reason} at checkpoint {checkpointPos}");
            }
            
            if (!skipFadeEffects)
            {
                // Schedule fade out
                var fadeOut = Simulation.Schedule<FadeOutCamera>(0f);
                // FadeOutCamera uses a fixed duration internally
                
                // Schedule the actual respawn after fade out
                var spawnEvent = Simulation.Schedule<PlayerSpawn>(0.5f);
                spawnEvent.spawnPosition = checkpointPos;
                
                // Schedule fade in after respawn
                var fadeIn = Simulation.Schedule<FadeInCamera>(0.8f);
                // FadeInCamera uses a fixed duration internally
            }
            else
            {
                // Immediate respawn without effects
                var spawnEvent = Simulation.Schedule<PlayerSpawn>(0f);
                spawnEvent.spawnPosition = checkpointPos;
            }
            
            // Invoke checkpoint restored event
            CheckpointManager.Instance.RestoreToCheckpoint();
            
            // Log respawn for analytics/debugging
            LogRespawnEvent();
        }
        
        private void LogRespawnEvent()
        {
            switch (reason)
            {
                case RespawnReason.FellOutOfBounds:
                    Debug.Log($"Player fell out of bounds at {fallPosition}");
                    break;
                case RespawnReason.DeathByDamage:
                    Debug.Log("Player died from damage and respawned");
                    break;
                case RespawnReason.ManualRespawn:
                    Debug.Log("Player manually triggered respawn");
                    break;
                case RespawnReason.StuckInGeometry:
                    Debug.Log("Player was stuck and respawned");
                    break;
                case RespawnReason.Debug:
                    Debug.Log("Debug respawn triggered");
                    break;
            }
        }
    }
}