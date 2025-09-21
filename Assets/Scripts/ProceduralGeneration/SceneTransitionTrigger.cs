using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using System;
using System.Collections;
using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Simplified scene transition trigger that handles scene changes in NeonLadder.
    /// Supports both procedural (seed-based) and manual scene selection.
    /// Also acts as its own event when scheduled through the Simulation system.
    /// </summary>
    public class SceneTransitionTrigger : MonoBehaviour
    {
        public enum DestinationType
        {
            None,       // No scene transition - trigger does nothing
            Procedural, // Use seed-based scene selection
            Manual      // Use manually specified scene name
        }
        
        [Header("Transition Configuration")]
        [Tooltip("The GameObject with a trigger collider that detects player entry")]
        [SerializeField] private GameObject triggerColliderObject;
        
        [Header("Destination Settings")]
        [Tooltip("How to determine the destination scene")]
        [SerializeField] private DestinationType destinationType = DestinationType.Procedural;
        
        [Tooltip("Scene name to load when using Manual destination type")]
        [SerializeField] private string overrideSceneName = ""; // Match editor property name
        
        [Header("Player Movement Settings")]
        [Tooltip("Reset player velocity after scene transition")]
        [SerializeField] private bool resetPlayerVelocity = true;
        
        [Tooltip("Lock player to Z=0 for 2.5D movement")]
        [SerializeField] private bool lockPlayerZAxis = true;
        
        [Header("Spawn Point Settings")]
        [Tooltip("Player Should Spawn At: In the destination scene, use spawn point with this type")]
        [SerializeField] private SpawnPointType spawnPointType = SpawnPointType.Auto;
        
        [Tooltip("Custom spawn point name (only used when spawnPointType is Custom)")]
        [SerializeField] private string customSpawnPointName = "";
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = false;

        [Header("Path Visualization")]
        [Tooltip("Show which boss will be selected in the editor gizmo")]
        [SerializeField] private bool showPathPreview = true;
        
        // Minimal remaining fields for editor compatibility
        [SerializeField] private EventType transitionType = EventType.Portal;
        [SerializeField] private bool oneWayOnly = false;
        
        // Runtime state
        private bool isTransitioning = false;
        private GameObject currentPlayer;
        
        // Events
        public static event Action<SceneTransitionTrigger> OnTransitionStarted;
        public static event Action<SceneTransitionTrigger> OnTransitionCompleted;
        public static event Action<SceneTransitionTrigger, string> OnTransitionFailed;
        
        // Public properties for compatibility
        public bool CanExitHere => true;  // All triggers can exit
        public bool CanSpawnHere => spawnPointType != SpawnPointType.None; // Can spawn unless explicitly disabled
        public Vector3 SpawnPosition => transform.position;
        public SpawnPointType SpawnType => spawnPointType;
        public string CustomSpawnName => customSpawnPointName;
        
        private void Awake()
        {
            ValidateConfiguration();
        }
        
        private void ValidateConfiguration()
        {
            if (triggerColliderObject == null)
            {
                if (enableDebugLogs)
                    NeonLadder.Debugging.Debugger.LogError(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] {gameObject.name}: No trigger collider object assigned!", this);
                return;
            }
            
            var collider = triggerColliderObject.GetComponent<Collider>();
            if (collider == null)
            {
                if (enableDebugLogs)
                    NeonLadder.Debugging.Debugger.LogError(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] {gameObject.name}: Assigned trigger object '{triggerColliderObject.name}' has no Collider component!", this);
                return;
            }
            
            if (!collider.isTrigger)
            {
                if (enableDebugLogs)
                    NeonLadder.Debugging.Debugger.LogWarning(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] {gameObject.name}: Collider on '{triggerColliderObject.name}' is not set as trigger. Auto-fixing...", this);
                collider.isTrigger = true;
            }
        }
        
        private void Start()
        {
            // Set up event-based trigger detection on the assigned collider object
            if (triggerColliderObject != null)
            {
                // Replace old detector with new event-based trigger
                var oldDetector = triggerColliderObject.GetComponent<SceneTransitionTriggerDetector>();
                if (oldDetector != null)
                {
                    DestroyImmediate(oldDetector);
                }
                
                // Add original trigger detector
                var detector = triggerColliderObject.GetComponent<SceneTransitionTriggerDetector>();
                if (detector == null)
                {
                    detector = triggerColliderObject.AddComponent<SceneTransitionTriggerDetector>();
                }
                detector.Initialize(this);
            }
        }
        
        /// <summary>
        /// Called when player enters the trigger area
        /// </summary>
        public void OnPlayerEnterTrigger(Collider playerCollider)
        {
            if (isTransitioning || !IsPlayer(playerCollider)) 
                return;
            
            currentPlayer = playerCollider.gameObject;
            
            // CRITICAL: Immediately disable Z movement to stop auto-walk
            // This prevents the player from continuing to walk forward during/after transition
            var player = currentPlayer.GetComponentInChildren<Player>();
            if (player != null)
            {
                //player.DisableZMovement();
                //player.velocity = Vector3.zero; // Stop all movement
                //UnityEngine.Debug.Log("[SceneTransitionTrigger] Disabled Z movement and stopped player velocity");
            }
            
            // Immediately trigger the transition
            TriggerTransition();
        }
        
        /// <summary>
        /// Called when player exits the trigger area
        /// </summary>
        public void OnPlayerExitTrigger(Collider playerCollider)
        {
            if (!IsPlayer(playerCollider)) 
                return;
            
            currentPlayer = null;
        }
        
        /// <summary>
        /// Triggers the scene transition
        /// </summary>
        private void TriggerTransition()
        {
            if (isTransitioning) 
                return;
            
            isTransitioning = true;
            OnTransitionStarted?.Invoke(this);
            
            // Store transition context for the next scene
            SceneTransitionContext.SetTransitioned(true);
            
            // Determine destination scene
            string destinationScene = GetDestinationScene();
            
            if (string.IsNullOrEmpty(destinationScene))
            {
                OnTransitionFailed?.Invoke(this, "No valid destination scene");
                isTransitioning = false;
                return;
            }
            
            if (enableDebugLogs)
                NeonLadder.Debugging.Debugger.LogInformation(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] Transitioning to: {destinationScene}");
            
            // Set spawn context in SceneTransitionManager - just pass the spawn type to match
            SceneTransitionManager.Instance.SetSpawnContext(spawnPointType, customSpawnPointName);
            
            // Use SceneTransitionManager for the transition
            SceneTransitionManager.Instance.TransitionToScene(destinationScene);
            
            // Reset transitioning flag after a delay (manager handles the actual transition)
            StartCoroutine(ResetTransitioningFlag());
        }
        
        /// <summary>
        /// Reset the transitioning flag after the manager takes over
        /// </summary>
        private IEnumerator ResetTransitioningFlag()
        {
            yield return new WaitForSeconds(0.5f);
            isTransitioning = false;
            OnTransitionCompleted?.Invoke(this);
        }
        
        /// <summary>
        /// Gets the destination scene based on configuration
        /// </summary>
        private string GetDestinationScene()
        {
            switch (destinationType)
            {
                case DestinationType.None:
                    return null; // No transition
                    
                case DestinationType.Manual:
                    return overrideSceneName; // Use the editor-compatible field name
                    
                case DestinationType.Procedural:
                    return GetProceduralDestination();
                    
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// Gets procedurally generated destination using the ProceduralPathTransitions system
        /// </summary>
        private string GetProceduralDestination()
        {
            // Get the ProceduralPathTransitions component from SceneTransitionManager
            var pathTransitions = SceneTransitionManager.Instance?.GetComponent<ProceduralPathTransitions>();
            if (pathTransitions == null)
            {
                NeonLadder.Debugging.Debugger.LogError(NeonLadder.Debugging.LogCategory.ProceduralGeneration, "[SceneTransitionTrigger] ProceduralPathTransitions component not found on SceneTransitionManager! Using fallback selection.");
                return GetFallbackDestination();
            }

            // Determine if this is a left or right path trigger based on trigger position or name
            bool isLeftPath = DeterminePathDirection();

            // Use the procedural system to select the next boss
            var selectedBoss = pathTransitions.SelectNextBoss(isLeftPath);

            if (selectedBoss == null)
            {
                NeonLadder.Debugging.Debugger.LogWarning(NeonLadder.Debugging.LogCategory.ProceduralGeneration, "[SceneTransitionTrigger] No boss selected by procedural system. All bosses may be defeated!");
                return null;
            }

            // Route to Connection1 scene for the selected boss (not directly to boss arena)
            string connection1Scene = $"{selectedBoss.Identifier}_Connection1";

            if (enableDebugLogs)
            {
                string pathDirection = pathTransitions.IsPathsConverged ? "CONVERGED" : (isLeftPath ? "LEFT" : "RIGHT");
                NeonLadder.Debugging.Debugger.LogInformation(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] Path {pathDirection} selected boss: {selectedBoss.DisplayName} ({selectedBoss.Boss})");
                NeonLadder.Debugging.Debugger.LogInformation(NeonLadder.Debugging.LogCategory.ProceduralGeneration, $"[SceneTransitionTrigger] Routing through connection scene: {connection1Scene}");
            }

            return connection1Scene;
        }

        /// <summary>
        /// Determine if this trigger represents a left or right path
        /// Based on trigger name, position, or explicit configuration
        /// </summary>
        private bool DeterminePathDirection()
        {
            // Check trigger name for direction hints
            string triggerName = gameObject.name.ToLower();
            if (triggerName.Contains("left"))
                return true;
            if (triggerName.Contains("right"))
                return false;

            // Check trigger collider object name
            if (triggerColliderObject != null)
            {
                string colliderName = triggerColliderObject.name.ToLower();
                if (colliderName.Contains("left"))
                    return true;
                if (colliderName.Contains("right"))
                    return false;
            }

            // Check position relative to scene center (fallback)
            // If X position is negative, assume left path
            if (transform.position.x < 0)
                return true;

            // Default to right path if no clear indication
            return false;
        }

        /// <summary>
        /// Fallback destination selection when ProceduralPathTransitions is unavailable
        /// </summary>
        private string GetFallbackDestination()
        {
            // Get the current seed from Game instance
            string seed = "default_seed";
            if (Game.Instance != null && !string.IsNullOrEmpty(Game.Instance.ProceduralMap.Seed))
            {
                seed = Game.Instance.ProceduralMap.Seed;
            }

            // Create deterministic random based on seed
            var random = new System.Random(seed.GetHashCode());

            // Available scenes (simplified for now)
            string[] possibleScenes = new string[]
            {
                "Banquet_Connection1",
                "Cathedral_Connection1",
                "Necropolis_Connection1",
                "Vault_Connection1",
                "Garden_Connection1",
                "Mirage_Connection1",
                "Lounge_Connection1"
            };

            // Pick a random scene based on seed
            int index = random.Next(possibleScenes.Length);
            return possibleScenes[index];
        }
        
        // Note: LoadSceneAsync method removed - SceneTransitionManager handles all scene loading now
        
        // Note: GetSpawnKeyFromSpawnType method removed - no longer needed as SceneTransitionManager handles spawn logic
        
        /// <summary>
        /// Checks if the collider belongs to the player
        /// </summary>
        private bool IsPlayer(Collider collider)
        {
            return collider.CompareTag("Player") || 
                   collider.GetComponent<Player>() != null ||
                   collider.GetComponentInParent<Player>() != null;
        }
        
        #region Editor Visualization
        
        private void OnDrawGizmos()
        {
            DrawGizmo(0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawGizmo(1f);
        }
        
        private void DrawGizmo(float alpha)
        {
            if (triggerColliderObject == null)
                return;

            Collider collider = triggerColliderObject.GetComponent<Collider>();
            if (collider == null)
                return;

            // Draw trigger area
            Color gizmoColor = Color.green;
            gizmoColor.a = alpha;
            Gizmos.color = gizmoColor;

            Transform t = triggerColliderObject.transform;
            if (collider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.localScale);
                Gizmos.DrawWireCube(box.center, box.size);
                gizmoColor.a = 0.1f * alpha;
                Gizmos.color = gizmoColor;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (collider is SphereCollider sphere)
            {
                Vector3 center = t.position + sphere.center;
                Gizmos.DrawWireSphere(center, sphere.radius * t.localScale.x);
            }

            // Draw label with path preview
            #if UNITY_EDITOR
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            string label = gameObject.name;

            if (destinationType == DestinationType.Manual && !string.IsNullOrEmpty(overrideSceneName))
            {
                label += $"\n→ {overrideSceneName}";
            }
            else if (destinationType == DestinationType.Procedural && showPathPreview)
            {
                string preview = GetPathPreview();
                if (!string.IsNullOrEmpty(preview))
                {
                    label += $"\n→ {preview}";
                }
            }

            UnityEditor.Handles.Label(labelPos, label);
            #endif
        }

        /// <summary>
        /// Get path preview for editor visualization
        /// </summary>
        private string GetPathPreview()
        {
            try
            {
                // Try to get the procedural system for preview
                var sceneTransitionManager = FindObjectOfType<SceneTransitionManager>();
                if (sceneTransitionManager == null)
                    return "STM Not Found";

                var pathTransitions = sceneTransitionManager.GetComponent<ProceduralPathTransitions>();
                if (pathTransitions == null)
                    return "PPT Not Found";

                bool isLeftPath = DeterminePathDirection();
                string pathDirection = isLeftPath ? "LEFT" : "RIGHT";

                // Check if paths are converged
                if (pathTransitions.IsPathsConverged)
                {
                    return "CONVERGED (Final Boss)";
                }

                // Get preview of what would be selected
                var (leftChoice, rightChoice) = pathTransitions.PreviewNextChoices();

                if (isLeftPath && leftChoice != null)
                {
                    return $"{pathDirection}: {leftChoice.DisplayName}";
                }
                else if (!isLeftPath && rightChoice != null)
                {
                    return $"{pathDirection}: {rightChoice.DisplayName}";
                }
                else
                {
                    return $"{pathDirection}: No Boss Available";
                }
            }
            catch (System.Exception)
            {
                return "Preview Unavailable";
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets the assigned trigger collider object
        /// </summary>
        public GameObject GetTriggerColliderObject() => triggerColliderObject;
        
        /// <summary>
        /// Sets the trigger collider object and validates configuration
        /// </summary>
        public void SetTriggerColliderObject(GameObject colliderObject)
        {
            triggerColliderObject = colliderObject;
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Forces a transition to occur immediately
        /// </summary>
        public void ForceTransition()
        {
            TriggerTransition();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Simple static class to track scene transition context
    /// </summary>
    public static class SceneTransitionContext
    {
        private static bool hasTransitioned = false;
        
        public static bool HasTransitioned => hasTransitioned;
        
        public static void SetTransitioned(bool value)
        {
            hasTransitioned = value;
        }
        
        public static void Clear()
        {
            hasTransitioned = false;
        }
    }
}