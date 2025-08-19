using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadder.Gameplay;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.ProceduralGeneration
{
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("Bidirectional Configuration")]
        [SerializeField] private bool canSpawnHere = true;
        [SerializeField] private bool canExitHere = true;
        [SerializeField] private string spawnPointName;
        
        [Header("Transition Configuration")]
        [SerializeField] private EventType transitionType = EventType.Portal;
        [SerializeField] private GameObject triggerColliderObject;
        
        [Header("Transition Settings")]
        [SerializeField] private TransitionMode mode = TransitionMode.Automatic;
        [SerializeField] private string interactionPrompt = "Press E to Enter";
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private float activationDelay = 0f;
        
        [Header("Destination Routing")]
        [SerializeField] private DestinationType destinationType = DestinationType.Procedural;
        [SerializeField] private string overrideSceneName;
        [SerializeField] private SpawnPointType spawnPointType = SpawnPointType.Auto;
        [SerializeField] private string customSpawnPointName;
        
        [Header("Direction")]
        [SerializeField] private TransitionDirection direction = TransitionDirection.Forward;
        
        [Header("Seed-Based Generation")]
        [SerializeField] private bool useSeededGeneration = true;
        [SerializeField] private string debugSeedOverride; // For testing specific seeds
        
        [Header("Restrictions")]
        [SerializeField] private bool oneWayOnly = false;
        [SerializeField] private bool requiresKey = false;
        [SerializeField] private string requiredKeyId;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showPromptUI = true;
        [SerializeField] private Vector3 promptOffset = new Vector3(0, 2, 0);
        [SerializeField] private Color gizmoColor = Color.green;
        
        // Runtime state
        private bool playerInTrigger = false;
        private bool isTransitioning = false;
        private GameObject currentPlayer;
        private Coroutine activationCoroutine;
        private GameObject promptUIInstance;
        
        // Events
        public static event Action<SceneTransitionTrigger> OnTransitionStarted;
        public static event Action<SceneTransitionTrigger> OnTransitionCompleted;
        public static event Action<SceneTransitionTrigger, string> OnTransitionFailed;
        
        // Public properties for spawn point functionality
        public Vector3 SpawnPosition => transform.position;
        public TransitionDirection Direction => direction;
        public bool CanSpawnHere => canSpawnHere;
        public bool CanExitHere => canExitHere;
        
        private void Awake()
        {
            ValidateConfiguration();
        }
        
        private void ValidateConfiguration()
        {
            if (triggerColliderObject == null)
            {
                Debug.LogError($"[SceneTransitionTrigger] {gameObject.name}: No trigger collider object assigned!", this);
                return;
            }
            
            var collider = triggerColliderObject.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError($"[SceneTransitionTrigger] {gameObject.name}: Assigned trigger object '{triggerColliderObject.name}' has no Collider component!", this);
                return;
            }
            
            if (!collider.isTrigger)
            {
                Debug.LogWarning($"[SceneTransitionTrigger] {gameObject.name}: Collider on '{triggerColliderObject.name}' is not set as trigger. Auto-fixing...", this);
                collider.isTrigger = true;
            }
        }
        
        private void Start()
        {
            // Set up trigger detection on the assigned collider object
            if (triggerColliderObject != null && canExitHere)
            {
                // Add this script's trigger detection to the collider object if needed
                var triggerDetector = triggerColliderObject.GetComponent<SceneTransitionTriggerDetector>();
                if (triggerDetector == null)
                {
                    triggerDetector = triggerColliderObject.AddComponent<SceneTransitionTriggerDetector>();
                }
                triggerDetector.Initialize(this);
            }
            
            // Register as spawn point if enabled
            if (canSpawnHere)
            {
                RegisterAsSpawnPoint();
            }
        }
        
        /// <summary>
        /// Registers this transition trigger as a spawn point for bidirectional functionality
        /// </summary>
        private void RegisterAsSpawnPoint()
        {
            var spawnManager = SpawnPointManager.Instance;
            if (spawnManager != null)
            {
                string spawnName = !string.IsNullOrEmpty(spawnPointName) ? spawnPointName : GetDefaultSpawnPointName();
                spawnManager.RegisterSpawnPoint(spawnName, transform);
                
                Debug.Log($"[SceneTransitionTrigger] Registered as spawn point: {spawnName} (Direction: {direction})");
            }
        }
        
        /// <summary>
        /// Gets the default spawn point name based on direction
        /// </summary>
        private string GetDefaultSpawnPointName()
        {
            switch (direction)
            {
                case TransitionDirection.Left:
                    return "FromRight"; // Player exits left, spawns from right
                case TransitionDirection.Right:
                    return "FromLeft"; // Player exits right, spawns from left
                case TransitionDirection.Up:
                    return "FromDown";
                case TransitionDirection.Down:
                    return "FromUp";
                default:
                    return "Default";
            }
        }
        
        public void OnPlayerEnterTrigger(Collider playerCollider)
        {
            if (isTransitioning || !IsPlayer(playerCollider)) return;
            
            playerInTrigger = true;
            currentPlayer = playerCollider.gameObject;
            
            if (mode == TransitionMode.Automatic)
            {
                if (activationDelay > 0)
                {
                    activationCoroutine = StartCoroutine(DelayedActivation());
                }
                else
                {
                    TriggerTransition();
                }
            }
            else if (mode == TransitionMode.Interactive)
            {
                ShowInteractionPrompt();
            }
        }
        
        public void OnPlayerExitTrigger(Collider playerCollider)
        {
            if (!IsPlayer(playerCollider)) return;
            
            playerInTrigger = false;
            currentPlayer = null;
            
            if (activationCoroutine != null)
            {
                StopCoroutine(activationCoroutine);
                activationCoroutine = null;
            }
            
            if (mode == TransitionMode.Interactive)
            {
                HideInteractionPrompt();
            }
        }
        
        private void Update()
        {
            if (mode == TransitionMode.Interactive && playerInTrigger && !isTransitioning)
            {
                if (Input.GetKeyDown(interactionKey))
                {
                    TriggerTransition();
                }
            }
        }
        
        private IEnumerator DelayedActivation()
        {
            yield return new WaitForSeconds(activationDelay);
            if (playerInTrigger)
            {
                TriggerTransition();
            }
        }
        
        private void TriggerTransition()
        {
            if (isTransitioning) return;
            
            // Check restrictions
            if (requiresKey && !HasRequiredKey())
            {
                OnTransitionFailed?.Invoke(this, $"Missing required key: {requiredKeyId}");
                ShowKeyRequiredFeedback();
                return;
            }
            
            isTransitioning = true;
            OnTransitionStarted?.Invoke(this);
            
            // Store transition context for spawn system
            if (SpawnPointManager.Instance != null)
            {
                string spawnPointName = GetTargetSpawnPointName();
                SpawnPointManager.Instance.SetTransitionContext(direction, spawnPointName);
            }
            
            // Determine destination scene
            string destinationScene = GetDestinationScene();
            
            if (string.IsNullOrEmpty(destinationScene))
            {
                OnTransitionFailed?.Invoke(this, "No valid destination scene");
                isTransitioning = false;
                return;
            }
            
            // Start scene transition
            StartCoroutine(LoadSceneAsync(destinationScene));
        }
        
        private string GetDestinationScene()
        {
            switch (destinationType)
            {
                case DestinationType.Manual:
                    return overrideSceneName;
                    
                case DestinationType.Procedural:
                    return GetSeededDestination();
                    
                case DestinationType.NextInPath:
                    // Get next scene from current path context
                    if (SceneRoutingContext.Instance != null)
                    {
                        return SceneRoutingContext.Instance.GetNextSceneInPath();
                    }
                    break;
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets destination scene using seed-based deterministic generation
        /// </summary>
        private string GetSeededDestination()
        {
            if (!useSeededGeneration)
            {
                Debug.LogWarning("Procedural routing requested but seeded generation is disabled!");
                return null;
            }
            
            // Get the current seed (menu selection or debug override)
            string currentSeed = GetCurrentSeed();
            
            // Use seed to determine boss assignments
            var bossAssignments = GetSeededBossAssignments(currentSeed);
            
            // Return the appropriate destination based on direction
            return GetDestinationFromAssignments(bossAssignments);
        }
        
        /// <summary>
        /// Gets the current seed from GameController or debug override
        /// </summary>
        private string GetCurrentSeed()
        {
            // Debug override takes priority
            if (!string.IsNullOrEmpty(debugSeedOverride))
            {
                return debugSeedOverride;
            }
            
            // Get from main menu selection or GameController
            if (Game.Instance != null && !string.IsNullOrEmpty(Game.Instance.ProceduralMap.Seed))
            {
                return Game.Instance.ProceduralMap.Seed;
            }
            
            // Fallback to default seed
            return "default_seed";
        }
        
        /// <summary>
        /// Uses seed to deterministically assign bosses to left/right exits
        /// This is the foundation that will expand to handle enemy spawns, powerups, etc.
        /// </summary>
        private Dictionary<TransitionDirection, string> GetSeededBossAssignments(string seed)
        {
            // Create seeded random for deterministic results
            var seededRandom = new System.Random(seed.GetHashCode());
            
            // Available boss arenas (will expand this list)
            var availableBosses = new List<string>
            {
                "Banquet",    // Gluttony
                "Cathedral",  // Pride  
                "Necropolis", // Wrath
                "Vault",      // Greed
                "Garden",     // Lust
                "Mirage",     // Envy
                "Lounge"      // Sloth
                // "Finale" - Devil (unlocked after other bosses)
            };
            
            // Shuffle the list using the seeded random
            for (int i = availableBosses.Count - 1; i > 0; i--)
            {
                int randomIndex = seededRandom.Next(i + 1);
                string temp = availableBosses[i];
                availableBosses[i] = availableBosses[randomIndex];
                availableBosses[randomIndex] = temp;
            }
            
            // Assign first two bosses to left/right paths
            var assignments = new Dictionary<TransitionDirection, string>
            {
                [TransitionDirection.Left] = availableBosses[0],
                [TransitionDirection.Right] = availableBosses[1]
            };
            
            Debug.Log($"[SceneTransitionTrigger] Seed '{seed}' assigned: Left={assignments[TransitionDirection.Left]}, Right={assignments[TransitionDirection.Right]}");
            
            return assignments;
        }
        
        /// <summary>
        /// Gets the destination scene based on this trigger's direction and boss assignments
        /// </summary>
        private string GetDestinationFromAssignments(Dictionary<TransitionDirection, string> bossAssignments)
        {
            if (!bossAssignments.TryGetValue(direction, out string bossName))
            {
                Debug.LogWarning($"No boss assigned for direction {direction}!");
                return null;
            }
            
            // For now, go directly to Connection1 scenes
            // Later this will be expanded with more complex path generation
            return $"{bossName}_Connection1";
        }
        
        private MapNode GetNextProceduralNode()
        {
            // This would integrate with PathGenerator to get the next node
            // For now, return null - will be implemented when PathGenerator integration is complete
            if (SceneRoutingContext.Instance != null)
            {
                return SceneRoutingContext.Instance.GetNextNode();
            }
            return null;
        }
        
        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Fade out or show loading screen
            yield return ShowLoadingTransition();
            
            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            while (!asyncLoad.isDone)
            {
                float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateLoadingProgress(progress);
                yield return null;
            }
            
            OnTransitionCompleted?.Invoke(this);
            isTransitioning = false;
        }
        
        private bool IsPlayer(Collider collider)
        {
            // Check for Player component or tag
            return collider.CompareTag("Player") || 
                   collider.GetComponent<Player>() != null ||
                   collider.GetComponentInParent<Player>() != null;
        }
        
        private bool HasRequiredKey()
        {
            // Check inventory or game state for required key
            // This would integrate with inventory system
            // For now, return true if no key required
            if (!requiresKey) return true;
            
            // TODO: Integrate with inventory system
            // Example: return InventoryManager.Instance.HasItem(requiredKeyId);
            
            return false;
        }
        
        private void ShowInteractionPrompt()
        {
            if (!showPromptUI) return;
            
            // Create UI prompt above trigger
            // This would integrate with UI system
            // For now, just log
            Debug.Log($"[SceneTransitionTrigger] {interactionPrompt}");
        }
        
        private void HideInteractionPrompt()
        {
            if (promptUIInstance != null)
            {
                Destroy(promptUIInstance);
                promptUIInstance = null;
            }
        }
        
        private void ShowKeyRequiredFeedback()
        {
            Debug.LogWarning($"[SceneTransitionTrigger] Key required: {requiredKeyId}");
            // TODO: Show UI feedback
        }
        
        private IEnumerator ShowLoadingTransition()
        {
            // TODO: Integrate with loading screen system
            yield return new WaitForSeconds(0.5f);
        }
        
        private void UpdateLoadingProgress(float progress)
        {
            // TODO: Update loading bar
        }
        
        private string GetTargetSpawnPointName()
        {
            switch (spawnPointType)
            {
                case SpawnPointType.Auto:
                    return null; // Let SpawnPointManager decide based on direction
                case SpawnPointType.Default:
                    return "Default";
                case SpawnPointType.FromLeft:
                    return "FromLeft";
                case SpawnPointType.FromRight:
                    return "FromRight";
                case SpawnPointType.BossArena:
                    return "BossArena";
                case SpawnPointType.Custom:
                    return customSpawnPointName;
                default:
                    return null;
            }
        }
        
        #region Editor Visualization
        
        private void OnDrawGizmos()
        {
            DrawGizmoVisualization(0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawGizmoVisualization(1f);
        }
        
        private void DrawGizmoVisualization(float alpha)
        {
            // Use the assigned trigger collider object for visualization
            Collider collider = null;
            if (triggerColliderObject != null)
            {
                collider = triggerColliderObject.GetComponent<Collider>();
            }
            
            if (collider == null) return;
            
            // Set color based on state
            Color drawColor = gizmoColor;
            if (requiresKey) drawColor = Color.yellow;
            if (oneWayOnly) drawColor = Color.red;
            drawColor.a = alpha;
            
            Gizmos.color = drawColor;
            
            // Draw collider bounds using the trigger object's transform
            Transform triggerTransform = triggerColliderObject.transform;
            if (collider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(triggerTransform.position, triggerTransform.rotation, triggerTransform.localScale);
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.color = new Color(drawColor.r, drawColor.g, drawColor.b, 0.1f * alpha);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (collider is SphereCollider sphere)
            {
                Vector3 center = triggerTransform.position + sphere.center;
                Gizmos.DrawWireSphere(center, sphere.radius * triggerTransform.localScale.x);
                Gizmos.color = new Color(drawColor.r, drawColor.g, drawColor.b, 0.1f * alpha);
                Gizmos.DrawSphere(center, sphere.radius * triggerTransform.localScale.x);
            }
            
            // Draw direction arrow
            DrawDirectionArrow();
            
            // Draw labels
            DrawGizmoLabels();
        }
        
        private void DrawDirectionArrow()
        {
            Vector3 center = transform.position;
            Vector3 arrowEnd = center;
            
            switch (direction)
            {
                case TransitionDirection.Left:
                    arrowEnd += Vector3.left * 2f;
                    break;
                case TransitionDirection.Right:
                    arrowEnd += Vector3.right * 2f;
                    break;
                case TransitionDirection.Up:
                    arrowEnd += Vector3.up * 2f;
                    break;
                case TransitionDirection.Down:
                    arrowEnd += Vector3.down * 2f;
                    break;
                case TransitionDirection.Forward:
                    arrowEnd += Vector3.forward * 2f;
                    break;
                case TransitionDirection.Backward:
                    arrowEnd += Vector3.back * 2f;
                    break;
            }
            
            if (direction != TransitionDirection.Any)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(center, arrowEnd);
                
                // Draw arrowhead
                Vector3 arrowDir = (arrowEnd - center).normalized;
                Vector3 arrowRight = Vector3.Cross(arrowDir, Vector3.forward).normalized;
                Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * 0.3f + arrowRight * 0.2f);
                Gizmos.DrawLine(arrowEnd, arrowEnd - arrowDir * 0.3f - arrowRight * 0.2f);
            }
        }
        
        private void DrawGizmoLabels()
        {
            #if UNITY_EDITOR
            Vector3 labelPos = transform.position + Vector3.up * 2.5f;
            
            string label = gameObject.name;
            if (mode == TransitionMode.Interactive)
            {
                label += $"\n[{interactionKey}]";
            }
            if (requiresKey)
            {
                label += $"\n🔑 {requiredKeyId}";
            }
            if (destinationType == DestinationType.Manual && !string.IsNullOrEmpty(overrideSceneName))
            {
                label += $"\n→ {overrideSceneName}";
            }
            
            UnityEditor.Handles.Label(labelPos, label);
            #endif
        }
        
        #endregion
        
        #region Public API
        
        public void SetDestination(string sceneName, SpawnPointType spawnType = SpawnPointType.Auto, string customSpawnPoint = null)
        {
            destinationType = DestinationType.Manual;
            overrideSceneName = sceneName;
            spawnPointType = spawnType;
            if (spawnType == SpawnPointType.Custom && !string.IsNullOrEmpty(customSpawnPoint))
            {
                customSpawnPointName = customSpawnPoint;
            }
        }
        
        // Backward compatibility overload for existing code
        public void SetDestination(string sceneName, string spawnPointName)
        {
            destinationType = DestinationType.Manual;
            overrideSceneName = sceneName;
            
            // Convert string spawn point name to enum
            if (string.IsNullOrEmpty(spawnPointName))
            {
                spawnPointType = SpawnPointType.Auto;
            }
            else
            {
                switch (spawnPointName.ToLowerInvariant())
                {
                    case "default":
                        spawnPointType = SpawnPointType.Default;
                        break;
                    case "fromleft":
                        spawnPointType = SpawnPointType.FromLeft;
                        break;
                    case "fromright":
                        spawnPointType = SpawnPointType.FromRight;
                        break;
                    case "bossarena":
                        spawnPointType = SpawnPointType.BossArena;
                        break;
                    default:
                        spawnPointType = SpawnPointType.Custom;
                        customSpawnPointName = spawnPointName;
                        break;
                }
            }
        }
        
        public void SetDirection(TransitionDirection newDirection)
        {
            direction = newDirection;
        }
        
        public void SetInteractive(bool interactive, string prompt = null)
        {
            mode = interactive ? TransitionMode.Interactive : TransitionMode.Automatic;
            if (!string.IsNullOrEmpty(prompt))
            {
                interactionPrompt = prompt;
            }
        }
        
        public void ForceTransition()
        {
            TriggerTransition();
        }
        
        public TransitionDirection GetDirection() => direction;
        public bool IsOneWay() => oneWayOnly;
        public bool RequiresKey() => requiresKey;
        public string GetRequiredKey() => requiredKeyId;
        public EventType GetTransitionType() => transitionType;
        public GameObject GetTriggerColliderObject() => triggerColliderObject;
        
        public void SetTriggerColliderObject(GameObject colliderObject)
        {
            triggerColliderObject = colliderObject;
            ValidateConfiguration();
        }
        
        #endregion
    }
}