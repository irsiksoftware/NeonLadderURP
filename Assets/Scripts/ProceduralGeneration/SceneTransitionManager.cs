using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Core;
using NeonLadder.Events;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Manages scene transitions for procedural content
    /// Handles fade effects, loading screens, and state preservation during transitions
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        #region Singleton
        
        private static SceneTransitionManager instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneTransitionManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SceneTransitionManager");
                        instance = go.AddComponent<SceneTransitionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Transition Settings")]
        [SerializeField] private bool enableFadeOut = true;
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [Tooltip("Minimum time the screen stays the fade color between fade out and fade in")]
        [SerializeField] private float minimumFadeDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Loading Screen")]
        [SerializeField] private bool useLoadingScreen = true;
        [Tooltip("Minimum time for actual loading operations")]
        [SerializeField] private float minimumLoadDuration = 1f;
        [SerializeField] private GameObject loadingScreenPrefab;
        
        [Header("State Preservation")]
        [SerializeField] private bool preservePlayerState = true;
        [SerializeField] private bool autoSaveOnTransition = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion
        
        #region Private Fields
        
        private Canvas transitionCanvas;
        private Image fadeImage;
        private GameObject loadingScreen;
        private bool isTransitioning = false;
        private bool isHandlingSpawn = false;
        private TransitionData currentTransition;
        
        // Player state preservation
        private PlayerStateSnapshot playerSnapshot;
        
        // Spawn point management
        private SpawnPointType pendingSpawnType = SpawnPointType.Auto;
        private string pendingCustomSpawnName = "";
        
        #endregion
        
        #region Events
        
        public static event Action<TransitionData> OnTransitionStarted;
        public static event Action<TransitionData> OnTransitionCompleted;
        public static event Action<string> OnTransitionError;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Initialize()
        {
            CreateTransitionCanvas();
            
            // Subscribe to procedural loader events
            ProceduralSceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
            ProceduralSceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;
            ProceduralSceneLoader.OnSceneLoadError += HandleSceneLoadError;
            
            // Subscribe to scene loading for MetaGameController reset hack
            SceneManager.sceneLoaded += OnSceneLoadedForMetaGameReset;
            
            LogDebug("SceneTransitionManager initialized");
        }
        
        private void CreateTransitionCanvas()
        {
            // Create persistent canvas for transitions
            GameObject canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(transform);
            
            transitionCanvas = canvasGO.AddComponent<Canvas>();
            transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            transitionCanvas.sortingOrder = 9999; // Ensure it's on top
            
            // Add canvas scaler for responsive UI
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Add raycaster for UI interaction
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Create fade image
            GameObject fadeGO = new GameObject("FadeImage");
            fadeGO.transform.SetParent(canvasGO.transform, false);
            
            fadeImage = fadeGO.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            
            // Make it fill the screen
            RectTransform rect = fadeGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
            // Start with fade invisible
            fadeImage.gameObject.SetActive(false);
        }
        
        private void OnDestroy()
        {
            ProceduralSceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
            ProceduralSceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            ProceduralSceneLoader.OnSceneLoadError -= HandleSceneLoadError;
            SceneManager.sceneLoaded -= OnSceneLoadedForMetaGameReset;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Set spawn context for the next scene transition
        /// </summary>
        public void SetSpawnContext(SpawnPointType spawnType = SpawnPointType.Auto, string customSpawnName = "")
        {
            pendingSpawnType = spawnType;
            pendingCustomSpawnName = customSpawnName;
            
            LogDebug($"Spawn context set: type={spawnType}, custom='{customSpawnName}'");
        }
        
        /// <summary>
        /// Transition to a scene with spawn context
        /// </summary>
        public void TransitionToScene(string sceneName, SpawnPointType spawnType = SpawnPointType.Auto, string customSpawnName = "")
        {
            if (isTransitioning)
            {
                LogDebug("Transition already in progress");
                return;
            }
            
            SetSpawnContext(spawnType, customSpawnName);
            TransitionToScene(sceneName);
        }
        
        /// <summary>
        /// Transition to a procedurally generated scene
        /// </summary>
        public void TransitionToProceduralScene(string seed, int depth, string pathType = "main")
        {
            if (isTransitioning)
            {
                LogDebug("Transition already in progress");
                return;
            }
            
            var transition = new TransitionData
            {
                TargetSeed = seed,
                TargetDepth = depth,
                PathType = pathType,
                StartTime = Time.time
            };
            
            StartCoroutine(PerformTransition(transition));
        }
        
        /// <summary>
        /// Transition to next room in procedural path
        /// </summary>
        public void TransitionToNextRoom()
        {
            var currentMap = ProceduralSceneLoader.Instance.GetCurrentMap();
            var currentScene = ProceduralSceneLoader.Instance.GetCurrentSceneData();
            
            if (currentMap == null || currentScene == null)
            {
                LogError("Cannot transition: No current procedural map or scene data");
                return;
            }
            
            int nextDepth = currentScene.depth + 1;
            TransitionToProceduralScene(currentMap.Seed, nextDepth);
        }
        
        /// <summary>
        /// Transition to a specific scene by name
        /// </summary>
        public void TransitionToScene(string sceneName)
        {
            if (isTransitioning)
            {
                LogDebug("Transition already in progress");
                return;
            }
            
            // Check if this component is still valid before starting coroutine
            if (this == null)
            {
                Debug.LogWarning("[SceneTransitionManager] Component destroyed, cannot start transition");
                return;
            }
            
            var transition = new TransitionData
            {
                TargetSceneName = sceneName,
                StartTime = Time.time
            };
            
            StartCoroutine(PerformSceneTransition(transition));
        }
        
        /// <summary>
        /// Quick transition without effects
        /// </summary>
        public void QuickTransition(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        #endregion
        
        #region Transition Implementation
        
        private IEnumerator PerformTransition(TransitionData transition)
        {
            isTransitioning = true;
            currentTransition = transition;
            
            OnTransitionStarted?.Invoke(transition);
            
            // Save player state
            if (preservePlayerState)
            {
                SavePlayerState();
            }
            
            // Auto-save if configured
            if (autoSaveOnTransition)
            {
                PerformAutoSave();
            }
            
            // Fade out (if enabled)
            if (enableFadeOut)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // Show loading screen (if fade enabled)
            if (useLoadingScreen && enableFadeOut)
            {
                ShowLoadingScreen();
            }
            
            float loadStartTime = Time.time;
            
            // Trigger procedural scene load
            ProceduralSceneLoader.Instance.LoadProceduralScene(
                transition.TargetSeed,
                transition.TargetDepth,
                transition.PathType
            );
            
            // Wait for scene to actually load
            yield return new WaitUntil(() => !ProceduralSceneLoader.Instance.IsLoading());
            
            // Handle player spawning while screen is still black
            yield return new WaitForSeconds(0.1f); // Small delay to ensure scene is ready
            HandlePlayerSpawning();
            
            // Wait a moment for spawning to complete
            yield return new WaitForSeconds(0.1f);
            
            // Wait for minimum fade duration (time screen stays black) - only if fade enabled
            if (enableFadeOut)
            {
                float elapsedTime = Time.time - loadStartTime;
                if (elapsedTime < minimumFadeDuration)
                {
                    yield return new WaitForSeconds(minimumFadeDuration - elapsedTime);
                }
            }
            
            // Hide loading screen (if fade enabled)
            if (useLoadingScreen && enableFadeOut)
            {
                HideLoadingScreen();
            }
            
            // Restore player state
            if (preservePlayerState)
            {
                RestorePlayerState();
            }
            
            // Fade in
            // Fade in (if enabled)
            if (enableFadeOut)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            transition.EndTime = Time.time;
            OnTransitionCompleted?.Invoke(transition);
            
            isTransitioning = false;
            currentTransition = null;
        }
        
        private IEnumerator PerformSceneTransition(TransitionData transition)
        {
            isTransitioning = true;
            currentTransition = transition;
            
            OnTransitionStarted?.Invoke(transition);
            
            // Save player state
            if (preservePlayerState)
            {
                SavePlayerState();
            }
            
            // Auto-save if configured
            if (autoSaveOnTransition)
            {
                PerformAutoSave();
            }
            
            // Fade out (if enabled)
            if (enableFadeOut)
            {
                yield return StartCoroutine(FadeOut());
            }
            
            // Show loading screen (if fade enabled)
            if (useLoadingScreen && enableFadeOut)
            {
                ShowLoadingScreen();
            }
            
            float loadingStartTime = Time.time;
            
            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(transition.TargetSceneName);
            
            // Check if scene load failed
            if (asyncLoad == null)
            {
                Debug.LogError($"[SceneTransitionManager] Failed to load scene '{transition.TargetSceneName}' - scene not found in build settings or build profile");
                isTransitioning = false;
                yield break;
            }
            
            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                transition.LoadProgress = asyncLoad.progress;
                yield return null;
            }
            
            // Handle player spawning while screen is still black
            yield return new WaitForSeconds(0.1f);
            HandlePlayerSpawning();
            
            // Wait a moment for spawning to complete
            yield return new WaitForSeconds(0.1f);
            
            // Ensure minimum fade duration has passed (time screen stays black) - only if fade enabled
            if (enableFadeOut)
            {
                float elapsedTime = Time.time - loadingStartTime;
                if (elapsedTime < minimumFadeDuration)
                {
                    LogDebug($"Scene loaded quickly ({elapsedTime:F1}s), waiting additional {minimumFadeDuration - elapsedTime:F1}s for minimum fade duration");
                    yield return new WaitForSeconds(minimumFadeDuration - elapsedTime);
                }
            }
            
            // Hide loading screen (if fade enabled)
            if (useLoadingScreen && enableFadeOut)
            {
                HideLoadingScreen();
            }
            
            // Wait a frame and check if position stuck
            yield return new WaitForSeconds(0.1f);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                LogDebug($"Player position after spawn delay: {player.transform.position}");
            }
            
            // Restore player state
            if (preservePlayerState)
            {
                RestorePlayerState();
            }
            
            // Fade in
            // Fade in (if enabled)
            if (enableFadeOut)
            {
                yield return StartCoroutine(FadeIn());
            }
            
            transition.EndTime = Time.time;
            OnTransitionCompleted?.Invoke(transition);
            
            isTransitioning = false;
            currentTransition = null;
        }
        
        #endregion
        
        #region Fade Effects
        
        private IEnumerator FadeOut()
        {
            fadeImage.gameObject.SetActive(true);
            float elapsedTime = 0;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeOutDuration;
                float alpha = fadeCurve.Evaluate(progress);
                
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1);
        }
        
        private IEnumerator FadeIn()
        {
            float elapsedTime = 0;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeInDuration;
                float alpha = 1 - fadeCurve.Evaluate(progress);
                
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeImage.gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Loading Screen
        
        private void ShowLoadingScreen()
        {
            if (loadingScreenPrefab != null && loadingScreen == null)
            {
                loadingScreen = Instantiate(loadingScreenPrefab, transitionCanvas.transform);
                
                // Make it fill the screen
                RectTransform rect = loadingScreen.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.sizeDelta = Vector2.zero;
                    rect.anchoredPosition = Vector2.zero;
                }
            }
            else if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
        }
        
        private void HideLoadingScreen()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
        }
        
        #endregion
        
        #region State Preservation
        
        private void SavePlayerState()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            playerSnapshot = new PlayerStateSnapshot();
            
            // Save position and rotation
            playerSnapshot.position = player.transform.position;
            playerSnapshot.rotation = player.transform.rotation;
            
            // Save player component data
            var playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerSnapshot.health = Mathf.RoundToInt(playerComponent.health.current);
                playerSnapshot.stamina = playerComponent.stamina.current;
                playerSnapshot.facingDirection = playerComponent.facingDirection;
                playerSnapshot.velocity = playerComponent.velocity;
            }
            
            LogDebug("Player state saved for transition");
        }
        
        private void RestorePlayerState()
        {
            if (playerSnapshot == null) return;
            
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;
            
            // Restore player component data
            var playerComponent = player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.health.current = playerSnapshot.health;
                playerComponent.stamina.current = playerSnapshot.stamina;
                playerComponent.facingDirection = playerSnapshot.facingDirection;
                
                // Note: Position is set by ProceduralSceneLoader spawn position
            }
            
            LogDebug("Player state restored after transition");
        }
        
        private void PerformAutoSave()
        {
            var saveData = EnhancedSaveSystem.Load() ?? new ConsolidatedSaveData();
            
            // Update scene information
            saveData.worldState.currentSceneName = SceneManager.GetActiveScene().name;
            
            // Save
            EnhancedSaveSystem.Save(saveData);
            
            LogDebug("Auto-save performed during transition");
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleSceneLoadStarted(string sceneName)
        {
            LogDebug($"Scene load started: {sceneName}");
        }
        
        private void HandleSceneLoadCompleted(string sceneName)
        {
            LogDebug($"Scene load completed: {sceneName}");
        }
        
        private void HandleSceneLoadError(string error)
        {
            LogError($"Scene load error: {error}");
            OnTransitionError?.Invoke(error);
            
            // Clean up transition state
            isTransitioning = false;
            currentTransition = null;
            
            // Hide loading screen and fade
            HideLoadingScreen();
            fadeImage.gameObject.SetActive(false);
        }
        
        #endregion
        
        #region Utility
        
        public bool IsTransitioning()
        {
            return isTransitioning;
        }
        
        public TransitionData GetCurrentTransition()
        {
            return currentTransition;
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debugger.Log($"[SceneTransitionManager] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[SceneTransitionManager] {message}");
        }

        #endregion

        #region Spawn Point Management

        public List<string> CutScenes = new List<string>
        {
            Scenes.Death.ToString(),
            Scenes.BossDefeated.ToString()
        };

        public List<string> DefaultSpawnScenes = new List<string>
        {
            Scenes.Staging.ToString()
        };

        private bool isExcludedCutscene(string scene)
        {
            return CutScenes.Contains(scene);
        }

        private bool isDefaultSpawnScene(string scene)
        {
            return DefaultSpawnScenes.Contains(scene);
        }
        private void HandlePlayerSpawning()
        {
            if (isExcludedCutscene(SceneManager.GetActiveScene().name))
            {
                LogDebug($"Skipping spawn point handling for excluded cutscene: {SceneManager.GetActiveScene().name}");
                return;
            }

            // Special handling for scenes that always use Auto spawn (like Staging)
            if (isDefaultSpawnScene(SceneManager.GetActiveScene().name))
            {
                LogDebug($"Using default Auto spawn for scene: {SceneManager.GetActiveScene().name}");
                pendingSpawnType = SpawnPointType.Auto;
                pendingCustomSpawnName = "";
            }

            // Find all spawn point configurations in the scene
            var spawnConfigs = FindObjectsOfType<SpawnPointConfiguration>();

            if (spawnConfigs.Length == 0)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                
                // Create highly visible error messages
                Debug.LogError("════════════════════════════════════════════════════════════════════════");
                Debug.LogError("║                          BLACK SCREEN WARNING                         ║");
                Debug.LogError("════════════════════════════════════════════════════════════════════════");
                Debug.LogError($"║ CRITICAL: No SpawnPointConfiguration found in scene '{sceneName}'");
                Debug.LogError("║");
                Debug.LogError("║ REASON: Cannot spawn player without spawn points!");
                Debug.LogError("║");
                Debug.LogError("║ TO FIX THIS:");
                Debug.LogError("║   1. Add a GameObject with SpawnPointConfiguration component");
                Debug.LogError("║   2. Set the spawn direction (Left, Right, Up, Down, or Default)");
                Debug.LogError("║   3. Position it where the player should spawn");
                Debug.LogError("║");
                Debug.LogError("║ TEST MODE: Scene will continue but player won't spawn");
                Debug.LogError("════════════════════════════════════════════════════════════════════════");
                
                LogError($"CRITICAL: No SpawnPointConfiguration components found in scene '{sceneName}'! Cannot spawn player.");
                
                // Don't pause in tests, just log the error
                #if UNITY_EDITOR && !UNITY_INCLUDE_TESTS
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            LogDebug($"Found {spawnConfigs.Length} spawn points in scene");
            
            // Log all spawn points for debugging
            for (int i = 0; i < spawnConfigs.Length; i++)
            {
                var sp = spawnConfigs[i];
                LogDebug($"  Spawn Point {i}: {sp.name} (mode: {sp.SpawnMode}) at position {sp.transform.position}");
            }
            
            // Find the appropriate spawn point based on context
            SpawnPointConfiguration targetSpawn = null;
            
            // If we have a specific spawn type requested
            if (pendingSpawnType != SpawnPointType.Auto)
            {
                if (pendingSpawnType == SpawnPointType.Custom && !string.IsNullOrEmpty(pendingCustomSpawnName))
                {
                    // Look for custom named spawn point
                    targetSpawn = System.Array.Find(spawnConfigs, sp => 
                        sp.SpawnMode == SpawnPointType.Custom && 
                        sp.CustomSpawnName.Equals(pendingCustomSpawnName, System.StringComparison.OrdinalIgnoreCase));
                        
                    if (targetSpawn == null)
                    {
                        LogError($"CRITICAL: Custom spawn point '{pendingCustomSpawnName}' not found!");
                    }
                }
                else
                {
                    // Look for specific spawn type
                    targetSpawn = System.Array.Find(spawnConfigs, sp => sp.SpawnMode == pendingSpawnType);
                    
                    if (targetSpawn == null)
                    {
                        LogError($"CRITICAL: Spawn point of type '{pendingSpawnType}' not found!");
                    }
                }
            }
            
            // Auto mode: find any matching Auto spawn point
            if (targetSpawn == null && pendingSpawnType == SpawnPointType.Auto)
            {
                // For Auto mode, just find any Auto spawn point
                targetSpawn = System.Array.Find(spawnConfigs, sp => sp.SpawnMode == SpawnPointType.Auto);
                
                if (targetSpawn == null)
                {
                    LogDebug($"No Auto spawn point found, trying Default");
                    
                    // Last resort: try Default spawn point
                    targetSpawn = System.Array.Find(spawnConfigs, sp => sp.SpawnMode == SpawnPointType.Default);
                }
            }
            
            // Critical error if no spawn point found
            if (targetSpawn == null)
            {
                LogError($"CRITICAL: No valid spawn point found! Type={pendingSpawnType}");
                Debug.LogError("[SceneTransitionManager] CRITICAL: Failed to find any valid spawn point. Halting.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            // Get spawn position
            Vector3 spawnPosition = targetSpawn.GetSpawnWorldPosition();
            LogDebug($"Spawning player at {targetSpawn.name} (type: {targetSpawn.SpawnMode}) position: {spawnPosition}");
            
            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                LogError("CRITICAL: Player GameObject with tag 'Player' not found!");
                Debug.LogError("[SceneTransitionManager] CRITICAL: No player found. Halting.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            // Use the PlayerSpawn event to properly spawn the player
            // Add delay to ensure scene is fully loaded and initialized
            
            // Debug: Check player position before scheduling spawn event
            LogDebug($"About to schedule PlayerSpawn at {spawnPosition}");
            LogDebug($"Player current position before spawn event: {player.transform.position}");
            
            var spawnEvent = Simulation.Schedule<PlayerSpawn>(1.5f);
            spawnEvent.spawnPosition = spawnPosition;
            
            LogDebug($"PlayerSpawn event scheduled at position {spawnPosition} with 1.5s delay");
            LogDebug($"Player position immediately after scheduling: {player.transform.position}");
            
            // Reset spawn context for next transition
            pendingSpawnType = SpawnPointType.Auto;
            pendingCustomSpawnName = "";
        }
        
        #endregion
        
        #region MetaGameController Reset Hack
        
        /// <summary>
        /// TODO: Hack to reset MetaGameController on scene load
        /// MetaGameController gets silently disabled during scene transitions,
        /// this forces a reset to restore menu functionality
        /// </summary>
        private void OnSceneLoadedForMetaGameReset(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single)
                return;
                
            // Find the MetaGameController (should be on persistent GameController object)
            var metaGameController = FindObjectOfType<NeonLadder.UI.MetaGameController>();
            if (metaGameController != null)
            {
                LogDebug($"Resetting MetaGameController in scene {scene.name}");
                
                // Disable and re-enable to reset the component
                metaGameController.enabled = false;
                metaGameController.enabled = true;
            }
            else
            {
                LogDebug($"No MetaGameController found in scene {scene.name}");
            }
        }
        
        #endregion
        
        #region Helper Classes
        
        [Serializable]
        public class TransitionData
        {
            public string TargetSceneName;
            public string TargetSeed;
            public int TargetDepth;
            public string PathType;
            public float StartTime;
            public float EndTime;
            public float LoadProgress;
            
            public float Duration => EndTime - StartTime;
        }
        
        [Serializable]
        private class PlayerStateSnapshot
        {
            public Vector3 position;
            public Quaternion rotation;
            public int health;
            public float stamina;
            public int facingDirection;
            public Vector2 velocity;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Extension methods for ProceduralSceneLoader
    /// </summary>
    public static class ProceduralSceneLoaderExtensions
    {
        public static bool IsLoading(this ProceduralSceneLoader loader)
        {
            // This would check internal loading state
            // For now, return false as placeholder
            return false;
        } 
    }
}