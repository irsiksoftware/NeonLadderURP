using UnityEngine;
using UnityEngine.InputSystem;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.UI
{
    /// <summary>
    /// Automatically adds navigation to Loading3DController when it appears
    /// Designed to be attached to SceneTransitionManager to work with real game loading
    /// </summary>
    public class Loading3DNavigationStarter : MonoBehaviour
    {
        [Header("Navigation Setup")]
        [SerializeField] private bool enableNavigation = true;
        [SerializeField] private bool disablePlayerControls = true;
        [SerializeField] private bool showDebugMessages = true;

        [Header("Test Mode (Optional)")]
        [SerializeField] private bool spawnTestScreen = false;
        [SerializeField] private GameObject loading3DScreenPrefab;

        private GameObject spawnedLoading3DScreen;
        private Loading3DController loading3DController;
        private bool hasSetupNavigation = false;

        // Player control management
        private InputActionAsset playerControls;
        private InputActionMap playerActionMap;
        private bool playerControlsWereDisabled = false;

        private void Start()
        {
            // Disable player controls immediately when loading starts
            if (disablePlayerControls && !spawnTestScreen)
            {
                DisablePlayerControls();
            }

            if (spawnTestScreen)
            {
                // Test mode: spawn a Loading3DScreen for testing
                SpawnLoading3DScreen();
            }
            else
            {
                // Real game mode: wait for SceneTransitionManager to spawn Loading3DController
                StartCoroutine(WaitForLoading3DController());
            }
        }

        private System.Collections.IEnumerator WaitForLoading3DController()
        {
            if (showDebugMessages)
            {
                Debug.Log("[Loading3DNavigationStarter] Waiting for SceneTransitionManager to spawn Loading3DController...");
            }

            while (true)
            {
                // Look for Loading3DController as child of this GameObject (SceneTransitionManager)
                loading3DController = GetComponentInChildren<Loading3DController>();

                if (loading3DController == null)
                {
                    // Also check globally in case it's spawned elsewhere
                    loading3DController = FindFirstObjectByType<Loading3DController>();
                }

                if (loading3DController != null)
                {
                    SetupContentDatabase();

                    if (enableNavigation)
                    {
                        SetupNavigation();
                    }

                    if (showDebugMessages)
                    {
                        Debug.Log("[Loading3DNavigationStarter] Found Loading3DController and set up navigation! Ready for A/D or movement controls.");
                    }
                    yield break;
                }

                yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            }
        }

        private void SpawnLoading3DScreen()
        {
            // Try to find the prefab if not assigned
            if (loading3DScreenPrefab == null)
            {
                // Load directly from Assets path instead of Resources
                #if UNITY_EDITOR
                loading3DScreenPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/Loading3DScreen.prefab");
                #endif
            }

            if (loading3DScreenPrefab == null)
            {
                if (showDebugMessages)
                {
                    Debug.LogError("[Loading3DNavigationStarter] Loading3DScreen prefab not assigned. Please drag the prefab from Assets/Prefabs/UI/Loading3DScreen.prefab into the inspector.");
                }
                return;
            }

            // Spawn the Loading3DScreen
            spawnedLoading3DScreen = Instantiate(loading3DScreenPrefab);
            spawnedLoading3DScreen.name = "Loading3DScreen (Test)";

            // Find the Loading3DController component
            loading3DController = spawnedLoading3DScreen.GetComponent<Loading3DController>();
            if (loading3DController == null)
            {
                loading3DController = spawnedLoading3DScreen.GetComponentInChildren<Loading3DController>();
            }

            if (loading3DController != null)
            {
                // Ensure the content database is loaded
                SetupContentDatabase();

                // Add navigation if enabled
                if (enableNavigation)
                {
                    SetupNavigation();
                }

                if (showDebugMessages)
                {
                    Debug.Log("[Loading3DNavigationStarter] Spawned Loading3DScreen for testing. Use Left/Right arrows or A/D to navigate models.");
                }
            }
            else
            {
                if (showDebugMessages)
                {
                    Debug.LogError("[Loading3DNavigationStarter] Could not find Loading3DController component in spawned prefab.");
                }
            }
        }

        private void SetupNavigation()
        {
            if (loading3DController == null || hasSetupNavigation) return;

            // Check if navigation component already exists
            var existingNavigation = loading3DController.GetComponent<Loading3DNavigation>();
            if (existingNavigation == null)
            {
                // Add navigation component
                var navigation = loading3DController.gameObject.AddComponent<Loading3DNavigation>();

                // Set up the loading3DController reference using reflection since it's private
                var field = typeof(Loading3DNavigation).GetField("loading3DController",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(navigation, loading3DController);
                }

                hasSetupNavigation = true;

                if (showDebugMessages)
                {
                    Debug.Log("[Loading3DNavigationStarter] Added Loading3DNavigation component and set controller reference. Navigation ready! Use A/D or Arrow keys.");
                }
            }
            else
            {
                hasSetupNavigation = true;
                if (showDebugMessages)
                {
                    Debug.Log("[Loading3DNavigationStarter] Loading3DNavigation component already exists.");
                }
            }
        }

        private void SetupContentDatabase()
        {
            if (loading3DController == null) return;

            // Try to load the content database from Resources
            var contentDatabase = Resources.Load<LoadingScreenContentDatabase>("LoadingScreenContentDatabase");

            if (contentDatabase != null)
            {
                // Use reflection to set the private contentDatabase field
                var field = typeof(Loading3DController).GetField("contentDatabase",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (field != null)
                {
                    field.SetValue(loading3DController, contentDatabase);

                    if (showDebugMessages)
                    {
                        var enabledModels = contentDatabase.GetEnabledModels();
                        Debug.Log($"[Loading3DNavigationStarter] Loaded content database with {enabledModels.Count} enabled models. Triggering loading screen refresh...");
                    }

                    // Force the loading controller to refresh and use the database
                    loading3DController.Show(0); // Show first model to initialize
                }
                else if (showDebugMessages)
                {
                    Debug.LogWarning("[Loading3DNavigationStarter] Could not find contentDatabase field in Loading3DController.");
                }
            }
            else if (showDebugMessages)
            {
                Debug.LogWarning("[Loading3DNavigationStarter] Could not load LoadingScreenContentDatabase from Resources.");
            }
        }

        /// <summary>
        /// Manually look for Loading3DController and set up navigation
        /// </summary>
        [ContextMenu("Setup Navigation Now")]
        public void SetupNavigationNow()
        {
            StartCoroutine(WaitForLoading3DController());
        }

        /// <summary>
        /// Remove navigation from current Loading3DController
        /// </summary>
        [ContextMenu("Remove Navigation")]
        public void RemoveNavigation()
        {
            if (loading3DController != null)
            {
                var navigation = loading3DController.GetComponent<Loading3DNavigation>();
                if (navigation != null)
                {
                    DestroyImmediate(navigation);
                    if (showDebugMessages)
                    {
                        Debug.Log("[Loading3DNavigationStarter] Removed Loading3DNavigation component.");
                    }
                }
            }
            hasSetupNavigation = false;
        }

        private void DisablePlayerControls()
        {
            // Try to find the player through Game singleton first
            GameObject playerObject = null;

            // Look for Game singleton
            var gameInstance = GameObject.FindFirstObjectByType<Game>();
            if (gameInstance != null)
            {
                // Look for Kauru as child of Game
                var kauruTransform = gameInstance.transform.Find("Kauru");
                if (kauruTransform != null)
                {
                    playerObject = kauruTransform.gameObject;
                }
            }

            // Fallback: find by Player tag
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerObject != null)
            {
                // Try to get PlayerInput component (Unity's Input System)
                var playerInput = playerObject.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerActionMap = playerInput.actions.FindActionMap("Player");
                    if (playerActionMap != null && playerActionMap.enabled)
                    {
                        playerActionMap.Disable();
                        playerControlsWereDisabled = true;

                        if (showDebugMessages)
                        {
                            Debug.Log($"[Loading3DNavigationStarter] Disabled Player action map on {playerObject.name}. Actions in map: {playerActionMap.actions.Count}");

                            // Debug individual actions
                            var moveAction = playerActionMap.FindAction("Move");
                            var attackAction = playerActionMap.FindAction("Attack");
                            Debug.Log($"[Loading3DNavigationStarter] Found Move action: {moveAction != null}, Found Attack action: {attackAction != null}");
                        }
                        return;
                    }
                }

                // Fallback: try to access action asset directly
                if (playerControls == null)
                {
                    playerControls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
                }

                if (playerControls != null)
                {
                    playerActionMap = playerControls.FindActionMap("Player");
                    if (playerActionMap != null && playerActionMap.enabled)
                    {
                        playerActionMap.Disable();
                        playerControlsWereDisabled = true;

                        if (showDebugMessages)
                        {
                            Debug.Log("[Loading3DNavigationStarter] Disabled Player action map via PlayerControls asset");
                        }
                        return;
                    }
                }
            }

            if (showDebugMessages)
            {
                Debug.LogWarning("[Loading3DNavigationStarter] Could not find player object or PlayerInput component to disable controls");
            }
        }

        private void EnablePlayerControls()
        {
            if (playerActionMap != null && playerControlsWereDisabled)
            {
                playerActionMap.Enable();
                playerControlsWereDisabled = false;

                if (showDebugMessages)
                {
                    Debug.Log($"[Loading3DNavigationStarter] Re-enabled Player action map after loading screen. Enabled: {playerActionMap.enabled}");

                    // Debug individual actions
                    var moveAction = playerActionMap.FindAction("Move");
                    var attackAction = playerActionMap.FindAction("Attack");
                    Debug.Log($"[Loading3DNavigationStarter] Move action enabled: {moveAction?.enabled}, Attack action enabled: {attackAction?.enabled}");
                }
            }
            else if (showDebugMessages)
            {
                Debug.LogWarning($"[Loading3DNavigationStarter] Could not re-enable controls. ActionMap null: {playerActionMap == null}, Were disabled: {playerControlsWereDisabled}");
            }
        }

        /// <summary>
        /// Call this when loading is complete to re-enable player controls
        /// </summary>
        public void OnLoadingComplete()
        {
            if (disablePlayerControls)
            {
                EnablePlayerControls();
            }

            // Clean up navigation
            if (loading3DController != null)
            {
                var navigation = loading3DController.GetComponent<Loading3DNavigation>();
                if (navigation != null)
                {
                    // Use DestroyImmediate in tests for immediate cleanup
                    #if UNITY_EDITOR
                    // In Unity Editor (tests), always use DestroyImmediate for immediate cleanup
                    DestroyImmediate(navigation);
                    #else
                    Destroy(navigation);
                    #endif
                }
            }

            hasSetupNavigation = false;
            loading3DController = null;

            if (showDebugMessages)
            {
                Debug.Log("[Loading3DNavigationStarter] Loading complete - cleaned up navigation and restored player controls");
            }
        }

        private void OnDestroy()
        {
            // Ensure player controls are restored
            if (disablePlayerControls && playerControlsWereDisabled)
            {
                EnablePlayerControls();
            }

            // Clean up spawned objects when this component is destroyed
            if (spawnedLoading3DScreen != null)
            {
                DestroyImmediate(spawnedLoading3DScreen);
            }
        }
    }
}