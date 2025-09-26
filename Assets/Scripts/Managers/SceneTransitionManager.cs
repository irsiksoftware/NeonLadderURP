using NeonLadder.Core;
using NeonLadder.Debugging;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.ProceduralGeneration;
using NeonLadder.UI;
using NeonLadderURP.DataManagement;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


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
                    instance = FindFirstObjectByType<SceneTransitionManager>();
                    if (instance == null)
                    {
                        Debugger.LogError(LogCategory.General, "No SceneTransitionManager found! Managers prefab may be missing from scene.");
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
        
        [Header("Loading Screen - Basic")]
        [SerializeField] private bool useLoadingScreen = true;
        [Tooltip("Minimum time for actual loading operations")]
        [SerializeField] private float minimumLoadDuration = 1f;
        [SerializeField] private GameObject loadingScreenPrefab;

        [Header("Loading Screen - 3D")]
        [SerializeField] private bool use3DLoadingScreen = false;
        [Tooltip("Prefab containing Loading3DController and UI setup")]
        [SerializeField] private GameObject loading3DScreenPrefab;
        [Tooltip("If true, randomly selects from available 3D models")]
        [SerializeField] private bool randomize3DModel = true;
        [Tooltip("Specific model index to show (-1 for random)")]
        [SerializeField] private int specific3DModelIndex = -1;

        [Header("Debug")]
        [Tooltip("Override minimum fade duration for testing (0 = use normal duration)")]
        [SerializeField] private float debugLoadingDurationOverride = 0f;
        
        [Header("State Preservation")]
        [SerializeField] private bool preservePlayerState = true;
        [SerializeField] private bool autoSaveOnTransition = true;
        
        [Header("Debug")]
        
        #endregion
        
        #region Private Fields
        
        private Canvas transitionCanvas;
        private Image fadeImage;
        private GameObject loadingScreen;
        private GameObject loading3DScreen;
        private Loading3DController loading3DController;
        private bool isTransitioning = false;
        private bool isHandlingSpawn = false;
        private bool isSpawnEventCompleted = false;
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
            // Note: DontDestroyOnLoad handled by parent Managers GameObject singleton

            Initialize();
        }
        
        private void Initialize()
        {
            Debugger.LogInformation(LogCategory.Loading, "SceneTransitionManager.Initialize() called");
            Debugger.LogInformation(LogCategory.Loading, $"useLoadingScreen: {useLoadingScreen}, use3DLoadingScreen: {use3DLoadingScreen}");

            // Create appropriate children based on checkbox settings
            if (useLoadingScreen && !use3DLoadingScreen)
            {
                // Basic 2D loading screen - create transition canvas
                Debugger.LogInformation(LogCategory.Loading, "Creating transition canvas for basic 2D loading screen");
                CreateTransitionCanvas();
            }
            else if (use3DLoadingScreen)
            {
                // 3D loading screen - create fade canvas AND 3D children
                Debugger.LogInformation(LogCategory.Loading, "Creating fade canvas and 3D loading screen children");
                CreateTransitionCanvas(); // Still need fade canvas for fade out/in
                Create3DLoadingScreenChildren();
            }
            else
            {
                // Both unchecked - no loading screen children created
                Debugger.LogInformation(LogCategory.Loading, "No loading screen enabled - no children created");
            }

            // Subscribe to procedural loader events
            ProceduralSceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
            ProceduralSceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;
            ProceduralSceneLoader.OnSceneLoadError += HandleSceneLoadError;

            // Subscribe to scene loading for MetaGameController reset hack
            SceneManager.sceneLoaded += OnSceneLoadedForMetaGameReset;

            // Subscribe to PlayerSpawn event completion
            PlayerSpawn.OnExecute += OnPlayerSpawnCompleted;

            Debugger.LogInformation(LogCategory.Loading, "SceneTransitionManager initialization complete");
        }
        
        private void CreateTransitionCanvas()
        {
            Debugger.LogInformation(LogCategory.Loading, "Creating TransitionCanvas for basic 2D loading screen");

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

            Debugger.LogInformation(LogCategory.Loading, "TransitionCanvas created successfully");
        }

        private void Create3DLoadingScreenChildren()
        {
            Debugger.LogInformation(LogCategory.Loading, "Creating 3D loading screen children (always spawned when enabled)");

            if (loading3DScreenPrefab != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "Instantiating 3D loading screen prefab");
                loading3DScreen = Instantiate(loading3DScreenPrefab, transform);

                // Get the controller component
                loading3DController = loading3DScreen.GetComponentInChildren<Loading3DController>();
                Debugger.LogInformation(LogCategory.Loading, $"Loading3DController found: {loading3DController != null}");

                // Start deactivated - will be activated during transitions
                loading3DScreen.SetActive(false);
                Debugger.LogInformation(LogCategory.Loading, "3D loading screen instantiated and deactivated (ready for transitions)");
            }
            else
            {
                Debugger.LogError(LogCategory.Loading, "3D Loading Screen enabled but loading3DScreenPrefab is not assigned!");
            }
        }
        
        private void OnDestroy()
        {
            ProceduralSceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
            ProceduralSceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            ProceduralSceneLoader.OnSceneLoadError -= HandleSceneLoadError;
            SceneManager.sceneLoaded -= OnSceneLoadedForMetaGameReset;
            PlayerSpawn.OnExecute -= OnPlayerSpawnCompleted;
        }
        
        #endregion

        #region ProceduralPathTransitions Integration

        /// <summary>
        /// Mark a boss as defeated in the procedural path system
        /// Call this when a boss is actually defeated in gameplay
        /// </summary>
        public void MarkBossAsDefeated(string bossIdentifier)
        {
            var pathTransitions = GetComponent<ProceduralPathTransitions>();
            if (pathTransitions != null)
            {
                pathTransitions.MarkBossAsDefeated(bossIdentifier);
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Boss marked as defeated: {bossIdentifier}");
            }
            else
            {
                Debugger.LogError(LogCategory.ProceduralGeneration, "ProceduralPathTransitions component not found! Cannot mark boss as defeated.");
            }
        }

        /// <summary>
        /// Get the current procedural path state for debugging
        /// </summary>
        public string GetPathVisualization()
        {
            var pathTransitions = GetComponent<ProceduralPathTransitions>();
            if (pathTransitions != null)
            {
                return pathTransitions.GetPathTreeVisualization();
            }
            return "ProceduralPathTransitions not available";
        }

        /// <summary>
        /// Reset the procedural path system with a new seed
        /// </summary>
        public void ResetProceduralPaths(string newSeed = null)
        {
            var pathTransitions = GetComponent<ProceduralPathTransitions>();
            if (pathTransitions != null)
            {
                pathTransitions.ResetWithNewSeed(newSeed);
                Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Procedural paths reset with seed: {pathTransitions.CurrentSeed}");
            }
            else
            {
                Debugger.LogError(LogCategory.ProceduralGeneration, "ProceduralPathTransitions component not found! Cannot reset paths.");
            }
        }

        #endregion

        #region Public API
        
        /// <summary>
        /// Set spawn context for the next scene transition
        /// Prevents overwriting during active transitions to maintain spawn point integrity
        /// </summary>
        public void SetSpawnContext(SpawnPointType spawnType = SpawnPointType.Auto, string customSpawnName = "")
        {
            // Prevent overwriting spawn context during active transitions
            // This preserves the original spawn intent from the triggering scene
            if (isTransitioning && pendingSpawnType != SpawnPointType.Auto)
            {
                Debugger.LogInformation(LogCategory.Spawn, $"Spawn context already set to {pendingSpawnType} during transition, ignoring override to {spawnType}");
                return;
            }

            pendingSpawnType = spawnType;
            pendingCustomSpawnName = customSpawnName;

            Debugger.LogInformation(LogCategory.Spawn, $"Spawn context set: type={spawnType}, custom='{customSpawnName}'");
        }
        
        /// <summary>
        /// Transition to a scene with spawn context
        /// </summary>
        public void TransitionToScene(string sceneName, SpawnPointType spawnType = SpawnPointType.Auto, string customSpawnName = "")
        {
            if (isTransitioning)
            {
                Debugger.LogInformation(LogCategory.ProceduralGeneration, "Transition already in progress");
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
                Debugger.LogInformation(LogCategory.ProceduralGeneration, "Transition already in progress");
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
                Debugger.LogError(LogCategory.ProceduralGeneration, "Cannot transition: No current procedural map or scene data");
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
                Debugger.LogInformation(LogCategory.Loading, "Transition already in progress - ignoring duplicate request");
                return;
            }
            
            // Check if this component is still valid before starting coroutine
            if (this == null)
            {
                Debugger.LogWarning(LogCategory.ProceduralGeneration, "[SceneTransitionManager] Component destroyed, cannot start transition");
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
            
            // Show loading screen (if any loading screen enabled and fade enabled)
            if ((useLoadingScreen || use3DLoadingScreen) && enableFadeOut)
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

            // Reset spawn completion flag and trigger spawning
            isSpawnEventCompleted = false;
            HandlePlayerSpawning();

            // Wait for PlayerSpawn event to actually complete
            yield return new WaitUntil(() => isSpawnEventCompleted);

            // Wait for minimum fade duration (time screen stays black) - only if fade enabled
            if (enableFadeOut)
            {
                float targetDuration = debugLoadingDurationOverride > 0 ? debugLoadingDurationOverride : minimumFadeDuration;
                float elapsedTime = Time.time - loadStartTime;
                if (elapsedTime < targetDuration)
                {
                    Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Waiting additional {targetDuration - elapsedTime:F1}s for fade duration (debug override: {debugLoadingDurationOverride > 0})");
                    yield return new WaitForSeconds(targetDuration - elapsedTime);
                }
            }
            
            // Hide loading screen (if any loading screen enabled and fade enabled)
            if ((useLoadingScreen || use3DLoadingScreen) && enableFadeOut)
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
            
            // Show loading screen (if any loading screen enabled and fade enabled)
            if ((useLoadingScreen || use3DLoadingScreen) && enableFadeOut)
            {
                ShowLoadingScreen();
            }
            
            float loadingStartTime = Time.time;
            
            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(transition.TargetSceneName);
            
            // Check if scene load failed
            if (asyncLoad == null)
            {
                Debugger.LogError(LogCategory.ProceduralGeneration, $"[SceneTransitionManager] Failed to load scene '{transition.TargetSceneName}' - scene not found in build settings or build profile");
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

            // Reset spawn completion flag and trigger spawning
            isSpawnEventCompleted = false;

            // Check if this is a cutscene that doesn't need player spawning
            if (isExcludedCutscene(transition.TargetSceneName))
            {
                Debugger.LogInformation(LogCategory.Spawn, $"Skipping PlayerSpawn wait for cutscene: {transition.TargetSceneName}");
                isSpawnEventCompleted = true; // Mark as completed immediately for cutscenes
            }
            else
            {
                HandlePlayerSpawning();
                // Wait for PlayerSpawn event to actually complete
                yield return new WaitUntil(() => isSpawnEventCompleted);
            }

            // Ensure minimum fade duration has passed (time screen stays black) - only if fade enabled
            if (enableFadeOut)
            {
                float targetDuration = debugLoadingDurationOverride > 0 ? debugLoadingDurationOverride : minimumFadeDuration;
                float elapsedTime = Time.time - loadingStartTime;
                if (elapsedTime < targetDuration)
                {
                    Debugger.LogInformation(LogCategory.Loading, $"Scene loaded quickly ({elapsedTime:F1}s), waiting additional {targetDuration - elapsedTime:F1}s for fade duration (debug override: {debugLoadingDurationOverride > 0})");
                    yield return new WaitForSeconds(targetDuration - elapsedTime);
                }
            }
            
            // Hide loading screen (if any loading screen enabled and fade enabled)
            if ((useLoadingScreen || use3DLoadingScreen) && enableFadeOut)
            {
                HideLoadingScreen();
            }
            
            // Wait a frame and check if position stuck
            yield return new WaitForSeconds(0.1f);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debugger.LogInformation(LogCategory.Spawn, $"Player position after spawn delay: {player.transform.position}");
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
            if (fadeImage == null)
            {
                Debugger.LogInformation(LogCategory.Loading, "No fadeImage available (3D loading screen mode), skipping fade out");
                yield break;
            }

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

            // Deactivate fadeImage after fade out completes - let the loading screen content show
            fadeImage.gameObject.SetActive(false);
            Debugger.LogInformation(LogCategory.Loading, "Fade out complete - fadeImage deactivated, loading screen content now visible");
        }
        
        private IEnumerator FadeIn()
        {
            if (fadeImage == null)
            {
                Debugger.LogInformation(LogCategory.Loading, "No fadeImage available (3D loading screen mode), skipping fade in");
                yield break;
            }

            // Reactivate fadeImage for fade in transition
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1); // Start fully black
            Debugger.LogInformation(LogCategory.Loading, "Fade in starting - fadeImage reactivated");

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
            Debugger.LogInformation(LogCategory.Loading, $"ShowLoadingScreen called - useLoadingScreen: {useLoadingScreen}, use3DLoadingScreen: {use3DLoadingScreen}");

            // Check if ANY loading screen is enabled
            if (!useLoadingScreen && !use3DLoadingScreen)
            {
                Debugger.LogInformation(LogCategory.Loading, "No loading screens enabled - skipping loading screen display");
                return;
            }

            // Boolean is the source of truth!
            if (use3DLoadingScreen)
            {
                if (loading3DScreenPrefab != null)
                {
                    Debugger.LogInformation(LogCategory.Loading, "Showing 3D loading screen");
                    Show3DLoadingScreen();
                }
                else
                {
                    Debugger.LogError(LogCategory.Loading, "3D Loading Screen enabled but loading3DScreenPrefab is not assigned!");
                }
            }
            else if (useLoadingScreen)
            {
                if (loadingScreenPrefab != null)
                {
                    Debugger.LogInformation(LogCategory.Loading, "Showing basic loading screen");
                    ShowBasicLoadingScreen();
                }
                else
                {
                    Debugger.LogInformation(LogCategory.Loading, "Basic loading screen enabled but no prefab assigned");
                }
            }
        }

        private void ShowBasicLoadingScreen()
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

        private void Show3DLoadingScreen()
        {
            Debugger.LogInformation(LogCategory.Loading, $"Show3DLoadingScreen called - existing screen: {loading3DScreen != null}, controller: {loading3DController != null}");

            // Screen should already be created in Initialize() - just activate and show
            if (loading3DScreen != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "Activating pre-created 3D loading screen");
                loading3DScreen.SetActive(true);

                if (loading3DController != null)
                {
                    // Show with specific model or random based on settings
                    int modelIndex = randomize3DModel ? -1 : specific3DModelIndex;
                    Debugger.LogInformation(LogCategory.Loading, $"Calling Loading3DController.Show with modelIndex: {modelIndex}");
                    loading3DController.Show(modelIndex);
                }
                else
                {
                    Debugger.LogError(LogCategory.Loading, "3D loading screen activated but no Loading3DController found!");
                }
            }
            else
            {
                Debugger.LogError(LogCategory.Loading, "CRITICAL: 3D loading screen not pre-created! Initialize() logic may have failed.");
            }
        }
        
        private void HideLoadingScreen()
        {
            Debugger.LogInformation(LogCategory.Loading, "HideLoadingScreen called");

            // Hide basic loading screen
            if (loadingScreen != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "Hiding basic loading screen");
                loadingScreen.SetActive(false);
            }

            // Hide 3D loading screen
            if (loading3DController != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "Hiding 3D loading screen via controller");
                loading3DController.Hide();
            }
            else if (loading3DScreen != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "Hiding 3D loading screen directly");
                loading3DScreen.SetActive(false);
            }

            Debugger.LogInformation(LogCategory.Loading, "HideLoadingScreen complete");
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
            
            Debugger.LogInformation(LogCategory.ProceduralGeneration, "Player state saved for transition");
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
            
            Debugger.LogInformation(LogCategory.ProceduralGeneration, "Player state restored after transition");
        }
        
        private void PerformAutoSave()
        {
            var saveData = EnhancedSaveSystem.Load() ?? new ConsolidatedSaveData();
            
            // Update scene information
            saveData.worldState.currentSceneName = SceneManager.GetActiveScene().name;
            
            // Save
            EnhancedSaveSystem.Save(saveData);
            
            Debugger.LogInformation(LogCategory.ProceduralGeneration, "Auto-save performed during transition");
        }
        
        #endregion
        
        #region Event Handlers

        private void OnPlayerSpawnCompleted(PlayerSpawn spawn)
        {
            isSpawnEventCompleted = true;
            Debugger.LogInformation(LogCategory.Spawn, $"PlayerSpawn event completed at position: {spawn.spawnPosition}");
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Scene load started: {sceneName}");
        }
        
        private void HandleSceneLoadCompleted(string sceneName)
        {
            Debugger.LogInformation(LogCategory.ProceduralGeneration, $"Scene load completed: {sceneName}");
        }
        
        private void HandleSceneLoadError(string error)
        {
            Debugger.LogError(LogCategory.ProceduralGeneration, $"Scene load error: {error}");
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
        


        #endregion

        #region Spawn Point Management

        private bool isExcludedCutscene(string scene)
        {
            return Scenes.SceneGroups.IsCutScene(scene);
        }

        private bool isDefaultSpawnScene(string scene)
        {
            return Scenes.SceneGroups.IsDefaultSpawnScene(scene);
        }
        private void HandlePlayerSpawning()
        {
            if (isExcludedCutscene(SceneManager.GetActiveScene().name))
            {
                Debugger.LogInformation(LogCategory.Spawn, $"Skipping spawn point handling for excluded cutscene: {SceneManager.GetActiveScene().name}");
                return;
            }

            // Special handling for scenes that always use Auto spawn (like Staging)
            if (isDefaultSpawnScene(SceneManager.GetActiveScene().name))
            {
                Debugger.LogInformation(LogCategory.Spawn, $"Using default Auto spawn for scene: {SceneManager.GetActiveScene().name}");
                pendingSpawnType = SpawnPointType.Auto;
                pendingCustomSpawnName = "";
            }

            // Find all spawn point configurations in the scene
            var spawnConfigs = FindObjectsOfType<SpawnPointConfiguration>();

            if (spawnConfigs.Length == 0)
            {
                string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                
                // Create highly visible error messages
                Debugger.LogError(LogCategory.ProceduralGeneration, "════════════════════════════════════════════════════════════════════════");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║                          BLACK SCREEN WARNING                         ║");
                Debugger.LogError(LogCategory.ProceduralGeneration, "════════════════════════════════════════════════════════════════════════");
                Debugger.LogError(LogCategory.ProceduralGeneration, $"║ CRITICAL: No SpawnPointConfiguration found in scene '{sceneName}'");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║ REASON: Cannot spawn player without spawn points!");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║ TO FIX THIS:");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║   1. Add a GameObject with SpawnPointConfiguration component");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║   2. Set the spawn direction (Left, Right, Up, Down, or Default)");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║   3. Position it where the player should spawn");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║");
                Debugger.LogError(LogCategory.ProceduralGeneration, "║ TEST MODE: Scene will continue but player won't spawn");
                Debugger.LogError(LogCategory.ProceduralGeneration, "════════════════════════════════════════════════════════════════════════");
                
                Debugger.LogError(LogCategory.Spawn, $"CRITICAL: No SpawnPointConfiguration components found in scene '{sceneName}'! Cannot spawn player.");
                
                // Don't pause in tests, just log the error
                #if UNITY_EDITOR && !UNITY_INCLUDE_TESTS
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            Debugger.LogInformation(LogCategory.Spawn, $"Found {spawnConfigs.Length} spawn points in scene");
            
            // Log all spawn points for debugging
            for (int i = 0; i < spawnConfigs.Length; i++)
            {
                var sp = spawnConfigs[i];
                Debugger.LogInformation(LogCategory.Spawn, $"  Spawn Point {i}: {sp.name} (mode: {sp.SpawnMode}) at position {sp.transform.position}");
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
                        Debugger.LogError(LogCategory.Spawn, $"CRITICAL: Custom spawn point '{pendingCustomSpawnName}' not found!");
                    }
                }
                else
                {
                    // Look for specific spawn type
                    Debugger.LogInformation(LogCategory.Spawn, $"Looking for spawn point of type: {pendingSpawnType}");

                    // Find all spawn points of the requested type
                    var matchingSpawns = System.Array.FindAll(spawnConfigs, sp => sp.SpawnMode == pendingSpawnType);
                    if (matchingSpawns.Length > 0)
                    {
                        targetSpawn = matchingSpawns[0];
                        if (matchingSpawns.Length > 1)
                        {
                            Debugger.LogInformation(LogCategory.Spawn, $"Multiple spawn points of type '{pendingSpawnType}' found ({matchingSpawns.Length}). Using first: {targetSpawn.name}");
                        }
                    }

                    if (targetSpawn == null)
                    {
                        Debugger.LogError(LogCategory.Spawn, $"CRITICAL: Spawn point of type '{pendingSpawnType}' not found!");
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
                    Debugger.LogInformation(LogCategory.Spawn, $"No Auto spawn point found, trying Default");
                    
                    // Last resort: try Default spawn point
                    targetSpawn = System.Array.Find(spawnConfigs, sp => sp.SpawnMode == SpawnPointType.Default);
                }
            }
            
            // Critical error if no spawn point found
            if (targetSpawn == null)
            {
                Debugger.LogError(LogCategory.Spawn, $"CRITICAL: No valid spawn point found! Type={pendingSpawnType}");
                Debugger.LogError(LogCategory.ProceduralGeneration, "[SceneTransitionManager] CRITICAL: Failed to find any valid spawn point. Halting.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            // Get spawn position
            Vector3 spawnPosition = targetSpawn.GetSpawnWorldPosition();
            Debugger.LogInformation(LogCategory.Spawn, $"Spawning player at {targetSpawn.name} (type: {targetSpawn.SpawnMode}) position: {spawnPosition}");
            
            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debugger.LogError(LogCategory.Spawn, "CRITICAL: Player GameObject with tag 'Player' not found!");
                Debugger.LogError(LogCategory.ProceduralGeneration, "[SceneTransitionManager] CRITICAL: No player found. Halting.");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPaused = true;
                #endif
                return;
            }
            
            // Use the PlayerSpawn event to properly spawn the player
            // Add delay to ensure scene is fully loaded and initialized
            
            // Debug: Check player position before scheduling spawn event
            Debugger.LogInformation(LogCategory.Spawn, $"About to schedule PlayerSpawn at {spawnPosition}");
            Debugger.LogInformation(LogCategory.Spawn, $"Player current position before spawn event: {player.transform.position}");
            
            var spawnEvent = Simulation.Schedule<PlayerSpawn>(0.1f);
            spawnEvent.spawnPosition = spawnPosition;

            Debugger.LogInformation(LogCategory.Spawn, $"PlayerSpawn event scheduled at position {spawnPosition}");
            Debugger.LogInformation(LogCategory.Spawn, $"Player position immediately after scheduling: {player.transform.position}");
            
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
                Debugger.LogInformation(LogCategory.UI, $"Resetting MetaGameController in scene {scene.name}");
                
                // Disable and re-enable to reset the component
                metaGameController.enabled = false;
                metaGameController.enabled = true;
            }
            else
            {
                Debugger.LogInformation(LogCategory.UI, $"No MetaGameController found in scene {scene.name}");
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
