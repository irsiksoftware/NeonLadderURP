using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;

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
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private Color fadeColor = Color.black;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Loading Screen")]
        [SerializeField] private bool useLoadingScreen = true;
        [SerializeField] private float minimumLoadingTime = 1f;
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
        private TransitionData currentTransition;
        
        // Player state preservation
        private PlayerStateSnapshot playerSnapshot;
        
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
        }
        
        #endregion
        
        #region Public API
        
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
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Show loading screen
            if (useLoadingScreen)
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
            
            // Wait for minimum loading time
            float elapsedTime = Time.time - loadStartTime;
            if (elapsedTime < minimumLoadingTime)
            {
                yield return new WaitForSeconds(minimumLoadingTime - elapsedTime);
            }
            
            // Wait for scene to actually load
            yield return new WaitUntil(() => !ProceduralSceneLoader.Instance.IsLoading());
            
            // Hide loading screen
            if (useLoadingScreen)
            {
                HideLoadingScreen();
            }
            
            // Restore player state
            if (preservePlayerState)
            {
                yield return new WaitForSeconds(0.1f); // Small delay to ensure scene is ready
                RestorePlayerState();
            }
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
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
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Show loading screen
            if (useLoadingScreen)
            {
                ShowLoadingScreen();
            }
            
            // Load the scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(transition.TargetSceneName);
            
            // Wait for scene to load
            while (!asyncLoad.isDone)
            {
                transition.LoadProgress = asyncLoad.progress;
                yield return null;
            }
            
            // Hide loading screen
            if (useLoadingScreen)
            {
                HideLoadingScreen();
            }
            
            // Restore player state
            if (preservePlayerState)
            {
                yield return new WaitForSeconds(0.1f);
                RestorePlayerState();
            }
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
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