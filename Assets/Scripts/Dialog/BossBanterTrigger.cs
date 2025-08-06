using UnityEngine;
using PixelCrushers.DialogueSystem;
using NeonLadder.Dialog;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// Trigger component for boss banter
    /// Attach to boss GameObjects to enable automatic banter triggering
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BossBanterTrigger : MonoBehaviour
    {
        [Header("Boss Configuration")]
        [SerializeField] private string bossName = "";
        
        [Header("Trigger Settings")]
        [SerializeField] private bool triggerOnProximity = true;
        [SerializeField] private float proximityDistance = 5f;
        [SerializeField] private bool triggerOnPlayerApproach = true;
        [SerializeField] private LayerMask playerLayerMask = 1;

        [Header("Debug")]
        [SerializeField] private bool enableDebugGizmos = true;

        private BossBanterManager banterManager;
        private Transform playerTransform;
        private bool playerInProximity = false;
        private float lastProximityCheck = 0f;
        private const float PROXIMITY_CHECK_INTERVAL = 0.5f;

        void Start()
        {
            InitializeBanterTrigger();
        }

        void Update()
        {
            if (triggerOnProximity && playerTransform != null)
            {
                CheckPlayerProximity();
            }
        }

        /// <summary>
        /// Initialize the banter trigger system
        /// </summary>
        private void InitializeBanterTrigger()
        {
            // Find the BossBanterManager
            banterManager = FindObjectOfType<BossBanterManager>();
            if (banterManager == null)
            {
                Debug.LogError($"BossBanterTrigger on {gameObject.name}: No BossBanterManager found in scene!");
                enabled = false;
                return;
            }

            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Auto-detect boss name if not set
            if (string.IsNullOrEmpty(bossName))
            {
                bossName = gameObject.name;
                // Clean up common suffixes
                bossName = bossName.Replace("(Clone)", "").Replace("_Boss", "").Replace("Boss", "").Trim();
            }

            // Ensure trigger is set up correctly
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning($"BossBanterTrigger on {gameObject.name}: Collider should be set as trigger!");
            }

            Debug.Log($"BossBanterTrigger initialized for boss: {bossName}");
        }

        /// <summary>
        /// Check if player is within proximity distance
        /// </summary>
        private void CheckPlayerProximity()
        {
            if (Time.time - lastProximityCheck < PROXIMITY_CHECK_INTERVAL)
                return;

            lastProximityCheck = Time.time;
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool nowInProximity = distance <= proximityDistance;

            // Player just entered proximity
            if (nowInProximity && !playerInProximity)
            {
                OnPlayerApproach();
            }

            playerInProximity = nowInProximity;
        }

        /// <summary>
        /// Called when player approaches the boss
        /// </summary>
        private void OnPlayerApproach()
        {
            if (triggerOnPlayerApproach && banterManager != null)
            {
                banterManager.TriggerBossBanter(bossName);
            }
        }

        /// <summary>
        /// Manual trigger for banter (can be called from other systems)
        /// </summary>
        public bool TriggerBanter()
        {
            if (banterManager != null)
            {
                return banterManager.TriggerBossBanter(bossName);
            }
            return false;
        }

        /// <summary>
        /// Unity trigger events for alternative triggering method
        /// </summary>
        void OnTriggerEnter(Collider other)
        {
            if (!triggerOnPlayerApproach) return;
            
            if (IsPlayer(other))
            {
                OnPlayerApproach();
            }
        }

        /// <summary>
        /// Check if the collider belongs to the player
        /// </summary>
        private bool IsPlayer(Collider other)
        {
            return ((1 << other.gameObject.layer) & playerLayerMask) != 0 || 
                   other.CompareTag("Player");
        }

        /// <summary>
        /// Reset banter rotation for this boss (useful for testing)
        /// </summary>
        [ContextMenu("Reset Banter Rotation")]
        public void ResetBanterRotation()
        {
            if (banterManager != null)
            {
                banterManager.ResetBossRotation(bossName);
            }
        }

        /// <summary>
        /// Test banter trigger (useful for debugging)
        /// </summary>
        [ContextMenu("Test Banter")]
        public void TestBanter()
        {
            bool success = TriggerBanter();
            Debug.Log($"Test banter for {bossName}: {(success ? "Success" : "Failed/Cooldown")}");
        }

        /// <summary>
        /// Draw debug gizmos in scene view
        /// </summary>
        void OnDrawGizmosSelected()
        {
            if (!enableDebugGizmos || !triggerOnProximity) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, proximityDistance);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, proximityDistance * 0.8f);
        }
    }
}