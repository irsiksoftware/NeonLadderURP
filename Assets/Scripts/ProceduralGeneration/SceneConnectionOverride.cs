using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.ProceduralGeneration
{
    public enum SceneSelectionMode
    {
        SceneName,      // Use scene name string
        BuildIndex,     // Use build settings index
        AssetPath       // Use full asset path
    }
    
    public enum ValidationStatus
    {
        NotValidated,
        Valid,
        Warning,
        Error
    }
    
    public enum ConditionType
    {
        HasItem,        // Player has specific item
        BossDefeated,   // Specific boss defeated
        PathCompleted,  // Specific path completed
        Custom          // Custom condition check
    }
    
    [Serializable]
    public class ConditionalOverride
    {
        [Header("Condition")]
        public ConditionType type = ConditionType.Custom;
        public string conditionKey = "";
        public bool invertCondition = false;
        
        [Header("Override")]
        public string overrideSceneName = "";
        public string overrideSpawnPoint = "";
        
        [Header("Display")]
        public string description = "";
        
        public bool EvaluateCondition()
        {
            bool result = false;
            
            switch (type)
            {
                case ConditionType.HasItem:
                    // TODO: Check inventory system
                    // result = InventoryManager.Instance?.HasItem(conditionKey) ?? false;
                    break;
                    
                case ConditionType.BossDefeated:
                    // Check persistent data for boss defeat
                    if (SceneRoutingContext.Instance != null)
                    {
                        result = SceneRoutingContext.Instance.GetPersistentData<bool>($"Boss_{conditionKey}_Defeated");
                    }
                    break;
                    
                case ConditionType.PathCompleted:
                    // Check if specific path was completed
                    if (SceneRoutingContext.Instance != null)
                    {
                        result = SceneRoutingContext.Instance.HasVisitedScene(conditionKey);
                    }
                    break;
                    
                case ConditionType.Custom:
                    // Custom condition evaluation
                    result = EvaluateCustomCondition(conditionKey);
                    break;
            }
            
            return invertCondition ? !result : result;
        }
        
        private bool EvaluateCustomCondition(string key)
        {
            // Override in derived classes or use delegate system
            // For now, check persistent data
            if (SceneRoutingContext.Instance != null)
            {
                return SceneRoutingContext.Instance.GetPersistentData<bool>(key);
            }
            return false;
        }
    }
    
    [RequireComponent(typeof(SceneTransitionTrigger))]
    public class SceneConnectionOverride : MonoBehaviour
    {
        [Header("Override Settings")]
        [SerializeField] private bool overrideEnabled = true;
        [SerializeField] private int priority = 100;
        
        [Header("Destination Override")]
        [SerializeField] private SceneSelectionMode selectionMode = SceneSelectionMode.SceneName;
        [SerializeField] private string targetSceneName = "";
        [SerializeField] private int targetSceneBuildIndex = -1;
        [SerializeField] private string targetSceneAssetPath = "";
        
        [Header("Spawn Point Override")]
        [SerializeField] private bool overrideSpawnPoint = false;
        [SerializeField] private string targetSpawnPointName = "";
        
        [Header("Conditional Overrides")]
        [SerializeField] private List<ConditionalOverride> conditionalOverrides = new List<ConditionalOverride>();
        
        [Header("Validation")]
        [SerializeField] private bool validateOnAwake = true;
        [SerializeField] private ValidationStatus validationStatus = ValidationStatus.NotValidated;
        [SerializeField] private string validationMessage = "";
        
        [Header("Debug")]
        [SerializeField] private bool debugLogging = false;
        [SerializeField] private Color gizmoColor = Color.cyan;
        
        // Cached references
        private SceneTransitionTrigger transitionTrigger;
        private string originalDestination;
        private string originalSpawnPoint;
        
        // Static registry for all overrides
        private static Dictionary<string, List<SceneConnectionOverride>> overrideRegistry = new Dictionary<string, List<SceneConnectionOverride>>();
        
        // Events
        public static event Action<SceneConnectionOverride, string> OnOverrideApplied;
        public static event Action<SceneConnectionOverride, ValidationStatus> OnValidationComplete;
        
        private void Awake()
        {
            transitionTrigger = GetComponent<SceneTransitionTrigger>();
            
            if (validateOnAwake)
            {
                ValidateOverride();
            }
            
            RegisterOverride();
        }
        
        private void OnEnable()
        {
            if (transitionTrigger != null)
            {
                SceneTransitionTrigger.OnTransitionStarted += HandleTransitionStarted;
            }
        }
        
        private void OnDisable()
        {
            if (transitionTrigger != null)
            {
                SceneTransitionTrigger.OnTransitionStarted -= HandleTransitionStarted;
            }
            
            UnregisterOverride();
        }
        
        private void HandleTransitionStarted(SceneTransitionTrigger trigger)
        {
            if (trigger != transitionTrigger || !overrideEnabled)
                return;
            
            ApplyOverride();
        }
        
        private void RegisterOverride()
        {
            string sceneName = gameObject.scene.name;
            if (!overrideRegistry.ContainsKey(sceneName))
            {
                overrideRegistry[sceneName] = new List<SceneConnectionOverride>();
            }
            
            if (!overrideRegistry[sceneName].Contains(this))
            {
                overrideRegistry[sceneName].Add(this);
                
                // Sort by priority
                overrideRegistry[sceneName] = overrideRegistry[sceneName]
                    .OrderByDescending(o => o.priority)
                    .ToList();
            }
        }
        
        private void UnregisterOverride()
        {
            string sceneName = gameObject.scene.name;
            if (overrideRegistry.ContainsKey(sceneName))
            {
                overrideRegistry[sceneName].Remove(this);
                
                if (overrideRegistry[sceneName].Count == 0)
                {
                    overrideRegistry.Remove(sceneName);
                }
            }
        }
        
        public void ApplyOverride()
        {
            if (!overrideEnabled || transitionTrigger == null)
                return;
            
            // Store original values
            originalDestination = null; // Would need to access private field via reflection or make it public
            originalSpawnPoint = null;
            
            // Check conditional overrides first
            foreach (var conditional in conditionalOverrides)
            {
                if (conditional.EvaluateCondition())
                {
                    ApplyDestination(conditional.overrideSceneName, conditional.overrideSpawnPoint);
                    
                    if (debugLogging)
                    {
                        Debug.Log($"[SceneConnectionOverride] Applied conditional override: {conditional.description}");
                    }
                    
                    OnOverrideApplied?.Invoke(this, conditional.overrideSceneName);
                    return;
                }
            }
            
            // Apply manual override
            string destinationScene = GetDestinationScene();
            if (!string.IsNullOrEmpty(destinationScene))
            {
                ApplyDestination(destinationScene, overrideSpawnPoint ? targetSpawnPointName : null);
                
                if (debugLogging)
                {
                    Debug.Log($"[SceneConnectionOverride] Applied manual override: {destinationScene}");
                }
                
                OnOverrideApplied?.Invoke(this, destinationScene);
            }
        }
        
        private void ApplyDestination(string sceneName, string spawnPoint)
        {
            if (transitionTrigger != null)
            {
                transitionTrigger.SetDestination(sceneName, spawnPoint);
            }
        }
        
        private string GetDestinationScene()
        {
            switch (selectionMode)
            {
                case SceneSelectionMode.SceneName:
                    return string.IsNullOrEmpty(targetSceneName) ? null : targetSceneName;
                    
                case SceneSelectionMode.BuildIndex:
                    if (targetSceneBuildIndex >= 0 && targetSceneBuildIndex < SceneManager.sceneCountInBuildSettings)
                    {
                        string path = SceneUtility.GetScenePathByBuildIndex(targetSceneBuildIndex);
                        return System.IO.Path.GetFileNameWithoutExtension(path);
                    }
                    break;
                    
                case SceneSelectionMode.AssetPath:
                    if (!string.IsNullOrEmpty(targetSceneAssetPath))
                    {
                        return System.IO.Path.GetFileNameWithoutExtension(targetSceneAssetPath);
                    }
                    break;
            }
            
            return null;
        }
        
        public ValidationStatus ValidateOverride()
        {
            validationStatus = ValidationStatus.Valid;
            validationMessage = "";
            
            // Check if trigger exists
            if (transitionTrigger == null)
            {
                validationStatus = ValidationStatus.Error;
                validationMessage = "No SceneTransitionTrigger component found";
                OnValidationComplete?.Invoke(this, validationStatus);
                return validationStatus;
            }
            
            // Validate destination scene
            string destinationScene = GetDestinationScene();
            if (string.IsNullOrEmpty(destinationScene))
            {
                validationStatus = ValidationStatus.Warning;
                validationMessage = "No destination scene specified";
            }
            else if (!IsSceneInBuildSettings(destinationScene))
            {
                validationStatus = ValidationStatus.Error;
                validationMessage = $"Scene '{destinationScene}' not found in Build Settings";
            }
            
            // Validate conditional overrides
            foreach (var conditional in conditionalOverrides)
            {
                if (string.IsNullOrEmpty(conditional.overrideSceneName))
                {
                    validationStatus = ValidationStatus.Warning;
                    validationMessage = "Conditional override has empty scene name";
                    break;
                }
                
                if (!IsSceneInBuildSettings(conditional.overrideSceneName))
                {
                    validationStatus = ValidationStatus.Error;
                    validationMessage = $"Conditional scene '{conditional.overrideSceneName}' not in Build Settings";
                    break;
                }
            }
            
            // Check for circular references
            if (HasCircularReference(destinationScene))
            {
                validationStatus = ValidationStatus.Error;
                validationMessage = $"Circular reference detected to '{destinationScene}'";
            }
            
            OnValidationComplete?.Invoke(this, validationStatus);
            return validationStatus;
        }
        
        private bool IsSceneInBuildSettings(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (name == sceneName)
                    return true;
            }
            
            return false;
        }
        
        private bool HasCircularReference(string targetScene)
        {
            if (string.IsNullOrEmpty(targetScene))
                return false;
            
            // Simple check - does target scene have override back to this scene?
            if (overrideRegistry.ContainsKey(targetScene))
            {
                foreach (var targetOverride in overrideRegistry[targetScene])
                {
                    if (targetOverride.GetDestinationScene() == gameObject.scene.name)
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
        
        public void RestoreOriginalDestination()
        {
            if (transitionTrigger != null && !string.IsNullOrEmpty(originalDestination))
            {
                transitionTrigger.SetDestination(originalDestination, originalSpawnPoint);
                
                if (debugLogging)
                {
                    Debug.Log($"[SceneConnectionOverride] Restored original destination: {originalDestination}");
                }
            }
        }
        
        #region Editor Helpers
        
        public static List<string> GetAllScenesInBuildSettings()
        {
            var scenes = new List<string>();
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                scenes.Add(sceneName);
            }
            
            return scenes;
        }
        
        public static List<SceneConnectionOverride> GetAllOverridesInScene(string sceneName)
        {
            if (overrideRegistry.ContainsKey(sceneName))
            {
                return new List<SceneConnectionOverride>(overrideRegistry[sceneName]);
            }
            
            return new List<SceneConnectionOverride>();
        }
        
        public void TestConnection()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[SceneConnectionOverride] Test connection only works in Play mode");
                return;
            }
            
            ValidateOverride();
            
            if (validationStatus == ValidationStatus.Valid)
            {
                ApplyOverride();
                Debug.Log($"[SceneConnectionOverride] Test successful - Would transition to: {GetDestinationScene()}");
            }
            else
            {
                Debug.LogError($"[SceneConnectionOverride] Test failed - {validationMessage}");
            }
        }
        
        #endregion
        
        #region Gizmos
        
        private void OnDrawGizmos()
        {
            if (!overrideEnabled) return;
            DrawOverrideGizmo(0.5f);
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawOverrideGizmo(1f);
            DrawConnectionLine();
        }
        
        private void DrawOverrideGizmo(float alpha)
        {
            Color color = gizmoColor;
            
            // Modify color based on validation status
            switch (validationStatus)
            {
                case ValidationStatus.Valid:
                    color = Color.green;
                    break;
                case ValidationStatus.Warning:
                    color = Color.yellow;
                    break;
                case ValidationStatus.Error:
                    color = Color.red;
                    break;
            }
            
            color.a = alpha;
            Gizmos.color = color;
            
            // Draw override indicator
            Vector3 position = transform.position + Vector3.up * 3f;
            Gizmos.DrawWireCube(position, Vector3.one * 0.5f);
            
            // Draw priority number
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(position + Vector3.up * 0.5f, $"P{priority}");
            #endif
        }
        
        private void DrawConnectionLine()
        {
            if (string.IsNullOrEmpty(GetDestinationScene()))
                return;
            
            // Draw a line indicating override connection
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.5f);
            
            Vector3 start = transform.position;
            Vector3 end = start + Vector3.right * 5f;
            
            // Solid line for manual override
            Gizmos.DrawLine(start, end);
            
            // Draw arrow
            Vector3 arrowDir = (end - start).normalized;
            Vector3 arrowRight = Vector3.Cross(arrowDir, Vector3.forward).normalized;
            Gizmos.DrawLine(end, end - arrowDir * 0.5f + arrowRight * 0.3f);
            Gizmos.DrawLine(end, end - arrowDir * 0.5f - arrowRight * 0.3f);
            
            // Label destination
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(end + Vector3.up, GetDestinationScene());
            #endif
        }
        
        #endregion
        
        #region Public API
        
        public bool IsEnabled() => overrideEnabled;
        public void SetEnabled(bool enabled) => overrideEnabled = enabled;
        
        public int GetPriority() => priority;
        public void SetPriority(int newPriority) => priority = newPriority;
        
        public string GetTargetScene() => GetDestinationScene();
        public string GetTargetSpawnPoint() => overrideSpawnPoint ? targetSpawnPointName : null;
        
        public ValidationStatus GetValidationStatus() => validationStatus;
        public string GetValidationMessage() => validationMessage;
        
        public void AddConditionalOverride(ConditionalOverride conditional)
        {
            if (conditional != null && !conditionalOverrides.Contains(conditional))
            {
                conditionalOverrides.Add(conditional);
            }
        }
        
        public void RemoveConditionalOverride(ConditionalOverride conditional)
        {
            conditionalOverrides.Remove(conditional);
        }
        
        public List<ConditionalOverride> GetConditionalOverrides()
        {
            return new List<ConditionalOverride>(conditionalOverrides);
        }
        
        #endregion
    }
}