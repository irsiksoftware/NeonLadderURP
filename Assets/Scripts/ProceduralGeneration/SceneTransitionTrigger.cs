using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadder.Gameplay;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.ProceduralGeneration
{
    public enum TransitionMode
    {
        Automatic,      // Trigger on enter
        Interactive     // Press key to trigger
    }

    public enum DestinationType
    {
        Procedural,     // Use procedural generation
        Manual,         // Use override scene name
        NextInPath      // Use next node in path
    }


    [RequireComponent(typeof(Collider))]
    public class SceneTransitionTrigger : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private TransitionMode mode = TransitionMode.Automatic;
        [SerializeField] private string interactionPrompt = "Press E to Enter";
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private float activationDelay = 0f;
        
        [Header("Destination")]
        [SerializeField] private DestinationType destinationType = DestinationType.Procedural;
        [SerializeField] private string overrideSceneName;
        [SerializeField] private string targetSpawnPointName;
        
        [Header("Direction")]
        [SerializeField] private TransitionDirection direction = TransitionDirection.Forward;
        
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
        
        private void Awake()
        {
            // Ensure collider is set as trigger
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isTransitioning || !IsPlayer(other)) return;
            
            playerInTrigger = true;
            currentPlayer = other.gameObject;
            
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
        
        private void OnTriggerExit(Collider other)
        {
            if (!IsPlayer(other)) return;
            
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
                SpawnPointManager.Instance.SetTransitionContext(direction, targetSpawnPointName);
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
                    // Use SceneRouter to get next procedural scene
                    if (SceneRouter.Instance != null)
                    {
                        var nextNode = GetNextProceduralNode();
                        if (nextNode != null)
                        {
                            return SceneRouter.Instance.GetSceneNameFromMapNode(nextNode);
                        }
                    }
                    break;
                    
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
            var collider = GetComponent<Collider>();
            if (collider == null) return;
            
            // Set color based on state
            Color drawColor = gizmoColor;
            if (requiresKey) drawColor = Color.yellow;
            if (oneWayOnly) drawColor = Color.red;
            drawColor.a = alpha;
            
            Gizmos.color = drawColor;
            
            // Draw collider bounds
            if (collider is BoxCollider box)
            {
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                Gizmos.DrawWireCube(box.center, box.size);
                Gizmos.color = new Color(drawColor.r, drawColor.g, drawColor.b, 0.1f * alpha);
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.matrix = oldMatrix;
            }
            else if (collider is SphereCollider sphere)
            {
                Vector3 center = transform.position + sphere.center;
                Gizmos.DrawWireSphere(center, sphere.radius * transform.localScale.x);
                Gizmos.color = new Color(drawColor.r, drawColor.g, drawColor.b, 0.1f * alpha);
                Gizmos.DrawSphere(center, sphere.radius * transform.localScale.x);
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
        
        public void SetDestination(string sceneName, string spawnPoint = null)
        {
            destinationType = DestinationType.Manual;
            overrideSceneName = sceneName;
            if (!string.IsNullOrEmpty(spawnPoint))
            {
                targetSpawnPointName = spawnPoint;
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
        
        #endregion
    }
}