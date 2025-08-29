using UnityEngine;
using NeonLadder.Core;
using NeonLadder.Events;

namespace NeonLadder.Mechanics.Zones
{
    /// <summary>
    /// Trigger zone that detects when the player falls into an inescapable area
    /// Schedules a respawn event when triggered
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FallDeathTrigger : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool instantRespawn = false;
        [SerializeField] private float respawnDelay = 0.5f;
        [SerializeField] private bool debugLogging = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool triggerDeathAnimation = true;
        [SerializeField] private bool playFallSound = true;
        
        private bool isProcessingRespawn = false;
        
        private void Awake()
        {
            // Ensure the collider is set as a trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
            
            // Tag for identification
            if (string.IsNullOrEmpty(gameObject.tag))
            {
                gameObject.tag = "DeathZone";
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if it's the player
            if (other.CompareTag("Player") && !isProcessingRespawn)
            {
                HandlePlayerFall(other.gameObject);
            }
        }
        
        private void HandlePlayerFall(GameObject player)
        {
            isProcessingRespawn = true;
            
            if (debugLogging)
            {
                Debug.Log($"FallDeathTrigger: Player fell at position {player.transform.position}");
            }
            
            // Schedule respawn request event
            float delay = instantRespawn ? 0f : respawnDelay;
            var respawnEvent = Simulation.Schedule<PlayerRespawnRequest>(delay);
            respawnEvent.reason = RespawnReason.FellOutOfBounds;
            respawnEvent.fallPosition = player.transform.position;
            
            // Optional: Trigger death animation first
            if (triggerDeathAnimation && !instantRespawn)
            {
                var deathEvent = Simulation.Schedule<PlayerDeath>(0f);
            }
            
            // Optional: Play fall sound
            if (playFallSound)
            {
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioType = AudioEventType.PlayerFall;
            }
            
            // Reset flag after a delay to prevent multiple triggers
            Invoke(nameof(ResetProcessingFlag), respawnDelay + 1f);
        }
        
        private void ResetProcessingFlag()
        {
            isProcessingRespawn = false;
        }
        
        #region Editor Helpers
        
        private void OnDrawGizmos()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                
                if (collider is BoxCollider box)
                {
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                    Gizmos.DrawCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius * transform.localScale.x);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.color = Color.red;
                
                if (collider is BoxCollider box)
                {
                    Matrix4x4 oldMatrix = Gizmos.matrix;
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                    Gizmos.DrawWireCube(box.center, box.size);
                    Gizmos.matrix = oldMatrix;
                }
                else if (collider is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius * transform.localScale.x);
                }
            }
        }
        
        #endregion
    }
}