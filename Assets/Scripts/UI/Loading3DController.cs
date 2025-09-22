using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonLadder.Debugging;
using Random = UnityEngine.Random;

namespace NeonLadder.UI
{
    /// <summary>
    /// Manages 3D loading screen models with associated loading tips
    /// Similar to Skyrim's loading screens with rotating 3D models and lore text
    /// </summary>
    public class Loading3DController : MonoBehaviour
    {
        #region Configuration

        [Header("3D Model Settings")]
        [SerializeField] private Camera loading3DCamera;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Transform modelContainer;
        [SerializeField] private List<Loading3DModel> loadingModels = new List<Loading3DModel>();

        [Header("Auto Content Database")]
        [SerializeField] private LoadingScreenContentDatabase contentDatabase;

        [Header("Rotation Settings")]
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float initialFacingAngle = 180f; // Degrees to rotate Y-axis to face camera

        [Header("Camera Settings")]
        [SerializeField] private float cameraDistance = 5f;
        [SerializeField] private float cameraHeight = 0f;
        [SerializeField] private float fieldOfView = 60f;

        [Header("UI References")]
        [SerializeField] private RawImage renderDisplay;
        [SerializeField] private TextMeshProUGUI loadingTipText;
        [SerializeField] private TextMeshProUGUI loadingTipTitle;

        [Header("Selection")]
        [SerializeField] private bool randomizeModel = true;
        [SerializeField] private int defaultModelIndex = 0;

        #endregion

        #region Private Fields

        private Loading3DModel currentModel;
        private GameObject currentModelInstance;
        private Coroutine rotationCoroutine;
        private int lastUsedModelIndex = -1; // Track last model to prevent consecutive repeats
        private int currentModelIndex = 0; // Track current model index for navigation

        #endregion

        #region Initialization

        private void Awake()
        {
            SetupCamera();
            ValidateRenderTexture();
        }

        private void SetupCamera()
        {
            if (loading3DCamera == null)
            {
                // Create camera if not assigned
                GameObject camGO = new GameObject("Loading3DCamera");
                camGO.transform.SetParent(transform);
                loading3DCamera = camGO.AddComponent<Camera>();
            }

            // Configure camera for UI rendering
            loading3DCamera.clearFlags = CameraClearFlags.SolidColor;
            loading3DCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparent
            loading3DCamera.cullingMask = 1 << LayerMask.NameToLayer("UI"); // Only render UI layer
            loading3DCamera.orthographic = false;
            loading3DCamera.fieldOfView = fieldOfView;
            loading3DCamera.nearClipPlane = 0.1f;
            loading3DCamera.farClipPlane = 100f;
            loading3DCamera.depth = -100; // Render before main camera

            // Disable camera by default
            loading3DCamera.enabled = false;
        }

        private void ValidateRenderTexture()
        {
            if (renderTexture == null)
            {
                // Create default render texture
                renderTexture = new RenderTexture(512, 512, 16);
                renderTexture.name = "Loading3DRenderTexture";
            }

            if (loading3DCamera != null)
            {
                loading3DCamera.targetTexture = renderTexture;
            }

            if (renderDisplay != null)
            {
                renderDisplay.texture = renderTexture;

                // Set larger size for the render display to give model more room
                RectTransform rectTransform = renderDisplay.rectTransform;
                rectTransform.sizeDelta = new Vector2(600f, 600f);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Show the 3D loading screen with a random or specific model
        /// </summary>
        public void Show(int modelIndex = -1)
        {
            gameObject.SetActive(true);

            // Get the active model list (manual or auto-generated)
            var activeModels = GetActiveModelList();
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Show called with modelIndex {modelIndex}, randomizeModel={randomizeModel}, activeModels.Count={activeModels.Count}");

            // Get list of enabled models
            var enabledModels = new List<int>();
            for (int i = 0; i < activeModels.Count; i++)
            {
                if (activeModels[i].enabled)
                {
                    enabledModels.Add(i);
                }
            }

            if (enabledModels.Count == 0)
            {
                Debugger.LogWarning(LogCategory.Loading, "[Loading3DController] No enabled models found!");
                return;
            }

            // Always select a model to ensure rotation restarts
            if (modelIndex >= 0 && modelIndex < activeModels.Count && activeModels[modelIndex].enabled)
            {
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Using specified model index {modelIndex}");
                SelectModel(modelIndex, activeModels);
            }
            else if (randomizeModel)
            {
                int randomEnabledIndex = GetRandomModelIndexAvoidingRepeat(enabledModels);
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Using random enabled model index {randomEnabledIndex} (from {enabledModels.Count} enabled models, avoiding repeat of {lastUsedModelIndex})");
                SelectModel(randomEnabledIndex, activeModels);
            }
            else
            {
                // Use first enabled model or clamp default to enabled models
                int modelToUse = enabledModels.Contains(defaultModelIndex) ? defaultModelIndex : enabledModels[0];
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Using default/first enabled model index {modelToUse}");
                SelectModel(modelToUse, activeModels);
            }

            // Enable camera
            if (loading3DCamera != null)
            {
                loading3DCamera.enabled = true;
            }
        }

        /// <summary>
        /// Hide the 3D loading screen
        /// </summary>
        public void Hide()
        {
            // Stop rotation
            if (rotationCoroutine != null)
            {
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }

            // Disable camera
            if (loading3DCamera != null)
            {
                loading3DCamera.enabled = false;
            }

            // Clean up model
            if (currentModelInstance != null)
            {
                Destroy(currentModelInstance);
                currentModelInstance = null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Add a new loading model to the collection
        /// </summary>
        public void AddLoadingModel(Loading3DModel model)
        {
            if (model != null && !loadingModels.Contains(model))
            {
                loadingModels.Add(model);
            }
        }

        /// <summary>
        /// Set the rotation speed for the current model
        /// </summary>
        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = speed;
        }

        /// <summary>
        /// Navigate to the next model in the list (for testing/preview purposes)
        /// </summary>
        public void NavigateToNextModel()
        {
            var activeModels = GetActiveModelList();
            if (activeModels == null || activeModels.Count == 0) return;

            int nextIndex = (currentModelIndex + 1) % activeModels.Count;
            LoadSpecificModel(nextIndex);
        }

        /// <summary>
        /// Navigate to the previous model in the list (for testing/preview purposes)
        /// </summary>
        public void NavigateToPreviousModel()
        {
            var activeModels = GetActiveModelList();
            if (activeModels == null || activeModels.Count == 0) return;

            int prevIndex = (currentModelIndex - 1 + activeModels.Count) % activeModels.Count;
            LoadSpecificModel(prevIndex);
        }

        /// <summary>
        /// Load a specific model by index (for testing/preview purposes)
        /// </summary>
        public void LoadSpecificModel(int index)
        {
            var activeModels = GetActiveModelList();
            if (activeModels == null || activeModels.Count == 0 || index < 0 || index >= activeModels.Count) return;

            currentModelIndex = index;
            SelectModel(index, activeModels);
        }

        /// <summary>
        /// Get the current model index for external reference
        /// </summary>
        public int GetCurrentModelIndex()
        {
            return currentModelIndex;
        }

        /// <summary>
        /// Get the current model name for display
        /// </summary>
        public string GetCurrentModelName()
        {
            var activeModels = GetActiveModelList();
            if (activeModels != null && currentModelIndex >= 0 && currentModelIndex < activeModels.Count)
            {
                return activeModels[currentModelIndex].modelName;
            }
            return "Unknown";
        }

        #endregion

        #region Private Methods

        private List<Loading3DModel> GetActiveModelList()
        {
            // If manual models are specified, use them
            if (loadingModels != null && loadingModels.Count > 0)
            {
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Using {loadingModels.Count} manually specified models");
                return loadingModels;
            }

            // Otherwise, auto-load from database (assigned or from Resources)
            if (contentDatabase != null)
            {
                var autoModels = contentDatabase.GetAllModels();
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Using {autoModels.Count} auto-generated models from assigned database");
                return autoModels;
            }

            // Try to load database from Resources if not assigned
            contentDatabase = Resources.Load<LoadingScreenContentDatabase>("LoadingScreenContentDatabase");
            if (contentDatabase != null)
            {
                var autoModels = contentDatabase.GetAllModels();
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Loaded database from Resources with {autoModels.Count} models");
                return autoModels;
            }

            // Fallback to empty list
            Debugger.LogWarning(LogCategory.Loading, "[Loading3DController] No models available - neither manual models nor LoadingScreenContentDatabase found in Resources");
            return new List<Loading3DModel>();
        }

        private int GetRandomModelIndexAvoidingRepeat(List<int> enabledModels)
        {
            // If only one model enabled, return it
            if (enabledModels.Count <= 1)
            {
                return enabledModels.Count > 0 ? enabledModels[0] : 0;
            }

            // If we have multiple models, avoid repeating the last one
            List<int> availableModels = new List<int>(enabledModels);

            // Remove the last used model if it exists in the enabled list
            if (lastUsedModelIndex >= 0 && availableModels.Contains(lastUsedModelIndex))
            {
                availableModels.Remove(lastUsedModelIndex);
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Removed last used model {lastUsedModelIndex} from selection to avoid repeat");
            }

            // Select from remaining models
            int selectedModel = availableModels[Random.Range(0, availableModels.Count)];
            return selectedModel;
        }

        private void SelectModel(int index, List<Loading3DModel> activeModels)
        {
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] SelectModel called with index {index}");

            // Clean up previous model
            if (currentModelInstance != null)
            {
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Destroying existing model instance");
                Destroy(currentModelInstance);
            }

            // Stop any existing rotation
            if (rotationCoroutine != null)
            {
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Stopping existing rotation coroutine");
                StopCoroutine(rotationCoroutine);
                rotationCoroutine = null;
            }

            // Get selected model data
            currentModel = activeModels[index];

            // Track this model to avoid consecutive repeats
            lastUsedModelIndex = index;

            // Instantiate model
            if (currentModel.modelPrefab != null)
            {
                currentModelInstance = Instantiate(currentModel.modelPrefab, modelContainer);
                currentModelInstance.transform.localPosition = currentModel.positionOffset;

                // Face the camera using configurable initial facing angle
                currentModelInstance.transform.localRotation = Quaternion.Euler(0, initialFacingAngle, 0);

                currentModelInstance.transform.localScale = currentModel.displayScale;

                // Set layer for camera culling
                SetLayerRecursively(currentModelInstance, LayerMask.NameToLayer("UI"));

                // Freeze rigidbody to prevent falling during loading
                FreezeRigidbody(currentModelInstance);

                // Set animator parameter if specified
                SetAnimatorParameter();

                // Position camera
                PositionCamera();

                // Start rotation immediately after setup - before screen becomes visible
                if (autoRotate)
                {
                    rotationCoroutine = StartCoroutine(RotateModel());
                    Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Started rotation for model '{currentModel.modelName}' using prefab '{currentModel.modelPrefab?.name}' -> instance '{currentModelInstance?.name}'");
                }
                else
                {
                    Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] AutoRotate disabled, no rotation started");
                }
            }

            // Update text
            UpdateLoadingTip();
        }

        private void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void FreezeRigidbody(GameObject obj)
        {
            // For loading screen: disable rigidbody completely to prevent any physics interference
            Rigidbody[] allRigidbodies = obj.GetComponentsInChildren<Rigidbody>(true);
            foreach (Rigidbody rb in allRigidbodies)
            {
                rb.isKinematic = true; // Make kinematic to disable all physics
                rb.useGravity = false; // Extra safety
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Set rigidbody to kinematic on '{rb.gameObject.name}' - physics disabled, transform rotation allowed");
            }

            // Disable Enemy AI components that might interfere with loading screen display
            DisableGameplayComponents(obj);

            // Debug: Analyze components that might interfere with rotation
            AnalyzeModelComponents(obj);
        }

        private void DisableGameplayComponents(GameObject obj)
        {
            // Disable Enemy AI scripts that might try to control movement/animation
            var enemies = obj.GetComponentsInChildren<NeonLadder.Mechanics.Controllers.Enemy>(true);
            foreach (var enemy in enemies)
            {
                enemy.enabled = false;
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Disabled Enemy component on '{enemy.gameObject.name}' to prevent AI interference");
            }

            // Disable any other gameplay controllers that might interfere
            var kinematicObjects = obj.GetComponentsInChildren<NeonLadder.Mechanics.Controllers.KinematicObject>(true);
            foreach (var kinematic in kinematicObjects)
            {
                // Only disable if it's not the Enemy component we already handled
                if (kinematic.GetComponent<NeonLadder.Mechanics.Controllers.Enemy>() == null)
                {
                    kinematic.enabled = false;
                    Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Disabled KinematicObject component on '{kinematic.gameObject.name}' to prevent movement interference");
                }
            }

            // Disable colliders to prevent any collision-based behavior
            var colliders = obj.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                collider.enabled = false;
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Disabled Collider on '{collider.gameObject.name}' to prevent collision interference");
            }
        }

        private void AnalyzeModelComponents(GameObject obj)
        {
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] === Component Analysis for '{obj.name}' ===");

            // Check root object components
            Component[] rootComponents = obj.GetComponents<Component>();
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Root components: {string.Join(", ", System.Array.ConvertAll(rootComponents, c => c.GetType().Name))}");

            // Check for constraints that might prevent rotation
            FixedJoint[] fixedJoints = obj.GetComponentsInChildren<FixedJoint>();
            ConfigurableJoint[] configurableJoints = obj.GetComponentsInChildren<ConfigurableJoint>();
            SpringJoint[] springJoints = obj.GetComponentsInChildren<SpringJoint>();

            if (fixedJoints.Length > 0)
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Found {fixedJoints.Length} FixedJoint(s): {string.Join(", ", System.Array.ConvertAll(fixedJoints, j => j.gameObject.name))}");
            if (configurableJoints.Length > 0)
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Found {configurableJoints.Length} ConfigurableJoint(s): {string.Join(", ", System.Array.ConvertAll(configurableJoints, j => j.gameObject.name))}");
            if (springJoints.Length > 0)
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Found {springJoints.Length} SpringJoint(s): {string.Join(", ", System.Array.ConvertAll(springJoints, j => j.gameObject.name))}");

            // Check for colliders that might have weird constraints
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Found {colliders.Length} collider(s): {string.Join(", ", System.Array.ConvertAll(colliders, c => $"{c.gameObject.name}({c.GetType().Name})"))}");
        }

        private void PositionCamera()
        {
            if (loading3DCamera == null || currentModelInstance == null) return;

            // Calculate bounds of the model
            Bounds bounds = CalculateBounds(currentModelInstance);

            // Position camera to fit model
            float distance = currentModel.cameraDistanceOverride > 0 ?
                currentModel.cameraDistanceOverride : cameraDistance;

            Vector3 cameraPos = bounds.center + Vector3.back * distance + Vector3.up * cameraHeight;
            loading3DCamera.transform.position = cameraPos;
            loading3DCamera.transform.LookAt(bounds.center);
        }

        private Bounds CalculateBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        private void UpdateLoadingTip()
        {
            if (currentModel == null) return;

            // Get localized tip (for now just use the first/default)
            var tip = currentModel.GetLocalizedTip("en");

            if (loadingTipTitle != null && !string.IsNullOrEmpty(tip.title))
            {
                loadingTipTitle.text = tip.title;
            }

            if (loadingTipText != null && !string.IsNullOrEmpty(tip.description))
            {
                loadingTipText.text = tip.description;
            }
        }

        private IEnumerator RotateModel()
        {
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] RotateModel coroutine started");
            int frameCount = 0;

            // Check initial state
            if (currentModelInstance != null)
            {
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Initial rotation before any changes: {currentModelInstance.transform.rotation.eulerAngles}");
            }

            while (currentModelInstance != null)
            {
                Vector3 rotationBefore = currentModelInstance.transform.rotation.eulerAngles;
                float deltaRotation = rotationSpeed * Time.deltaTime;

                currentModelInstance.transform.Rotate(rotationAxis, deltaRotation);

                Vector3 rotationAfter = currentModelInstance.transform.rotation.eulerAngles;

                // Log every 60 frames (roughly once per second)
                // Removed noisy frame-by-frame rotation logging

                // Detect if rotation got stuck
                if (frameCount > 60 && frameCount % 60 == 0)
                {
                    float yRotationChange = Mathf.Abs(rotationAfter.y - rotationBefore.y);
                    if (yRotationChange < 0.1f)
                    {
                        Debugger.LogWarning(LogCategory.Loading, $"[Loading3DController] Rotation appears stuck! Y rotation only changed by {yRotationChange:F4}°");

                        // Try to force rotation
                        currentModelInstance.transform.rotation = Quaternion.Euler(0, rotationAfter.y + 1f, 0);
                    }
                }

                frameCount++;
                yield return null;
            }
            Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] RotateModel coroutine ended");
        }

        private void SetAnimatorParameter()
        {
            if (currentModelInstance == null || currentModel == null)
                return;

            // Find animator in the instantiated model
            Animator animator = currentModelInstance.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                // No animator found - that's okay, just log for debugging
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] No Animator found on model '{currentModel.modelName}'");
                return;
            }

            // Check if we should disable the animator completely
            if (currentModel.disableAnimator)
            {
                animator.enabled = false;
                Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Animator disabled for model '{currentModel.modelName}' (frozen appearance)");
                return;
            }

            // Animator is enabled, set parameter if specified
            if (currentModel.animationValue >= 0 && !string.IsNullOrEmpty(currentModel.animatorParameterName))
            {
                try
                {
                    animator.SetInteger(currentModel.animatorParameterName, currentModel.animationValue);
                    Debugger.LogInformation(LogCategory.Loading, $"[Loading3DController] Set animator parameter '{currentModel.animatorParameterName}' = {currentModel.animationValue} for model '{currentModel.modelName}'");
                }
                catch (System.Exception e)
                {
                    Debugger.LogWarning(LogCategory.Loading, $"[Loading3DController] Failed to set animator parameter '{currentModel.animatorParameterName}' on model '{currentModel.modelName}': {e.Message}");
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Data structure for a 3D loading screen model with associated tips
    /// </summary>
    [Serializable]
    public class Loading3DModel
    {
        [Header("Model")]
        public bool enabled = true;
        public string modelName = "Loading Model";
        public GameObject modelPrefab;
        public Vector3 displayScale = Vector3.one;
        public Vector3 positionOffset = Vector3.zero;
        public float cameraDistanceOverride = 0f; // 0 = use default

        [Header("Animation Control")]
        [Tooltip("Disable the animator completely (model appears frozen)")]
        public bool disableAnimator = false;
        [Tooltip("Name of the integer parameter in the Animator (e.g. 'animation')")]
        public string animatorParameterName = "animation";
        [Tooltip("Value to set for the animator parameter (-1 = don't set)")]
        public int animationValue = -1;

        [Header("Loading Tips")]
        public List<LoadingTip> loadingTips = new List<LoadingTip>();

        /// <summary>
        /// Get a loading tip for the specified language
        /// </summary>
        public LoadingTip GetLocalizedTip(string languageCode)
        {
            // Try to find exact match
            var tip = loadingTips.Find(t => t.languageCode == languageCode);

            // Fall back to first available
            if (tip == null && loadingTips.Count > 0)
            {
                tip = loadingTips[0];
            }

            // Return empty if nothing found
            return tip ?? new LoadingTip { languageCode = languageCode };
        }
    }

    /// <summary>
    /// Multilingual loading screen tip
    /// </summary>
    [Serializable]
    public class LoadingTip
    {
        public string languageCode = "en";
        public string title = "";
        [TextArea(2, 4)]
        public string description = "";
    }
}