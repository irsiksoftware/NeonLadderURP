using UnityEngine;
using UnityEngine.InputSystem;
using NeonLadder.Debugging;

namespace NeonLadder.UI
{
    /// <summary>
    /// Adds navigation controls to Loading3DController for testing purposes
    /// Allows moving left/right through loading screen models using game's input system
    /// </summary>
    public class Loading3DNavigation : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Loading3DController loading3DController;
        [SerializeField] private bool enableNavigation = true;

        [Header("UI Input Integration")]
        [SerializeField] private bool useUIInputSystem = true;
        [SerializeField] private InputActionAsset playerControls;

        [Header("Legacy Input Actions (Optional)")]
        [SerializeField] private InputActionReference moveLeftAction;
        [SerializeField] private InputActionReference moveRightAction;

        // UI input system for loading screen navigation
        private InputAction uiNavigateAction;
        private Vector2 lastNavigateInput = Vector2.zero;

        private void OnEnable()
        {
            SetupInputSystem();
        }

        private void OnDisable()
        {
            CleanupInputSystem();
        }

        private void SetupInputSystem()
        {
            if (useUIInputSystem)
            {
                // Try to load PlayerControls if not assigned
                if (playerControls == null)
                {
                    playerControls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
                }

                if (playerControls != null)
                {
                    // Get the Move action from UI action map (for menu navigation)
                    var uiActionMap = playerControls.FindActionMap("UI");
                    if (uiActionMap != null)
                    {
                        uiNavigateAction = uiActionMap.FindAction("Move");
                        if (uiNavigateAction != null)
                        {
                            uiNavigateAction.Enable();
                            Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] Connected to UI Move action for loading screen navigation");
                        }
                        else
                        {
                            Debugger.LogWarning(LogCategory.Loading, "[Loading3DNavigation] Could not find Move action in UI action map - please add Move action to UI action map for controller menu navigation");
                        }
                    }
                    else
                    {
                        Debugger.LogWarning(LogCategory.Loading, "[Loading3DNavigation] Could not find UI action map in PlayerControls");
                    }
                }
                else
                {
                    Debugger.LogWarning(LogCategory.Loading, "[Loading3DNavigation] Could not load PlayerControls from Resources");
                }
            }

            // Fallback to legacy input action references
            if (moveLeftAction != null)
            {
                moveLeftAction.action.performed += OnMoveLeft;
                moveLeftAction.action.Enable();
            }

            if (moveRightAction != null)
            {
                moveRightAction.action.performed += OnMoveRight;
                moveRightAction.action.Enable();
            }
        }

        private void CleanupInputSystem()
        {
            if (uiNavigateAction != null)
            {
                uiNavigateAction.Disable();
                uiNavigateAction = null;
            }

            if (moveLeftAction != null)
            {
                moveLeftAction.action.performed -= OnMoveLeft;
                moveLeftAction.action.Disable();
            }

            if (moveRightAction != null)
            {
                moveRightAction.action.performed -= OnMoveRight;
                moveRightAction.action.Disable();
            }
        }

        private void Update()
        {
            if (!enableNavigation || loading3DController == null) return;

            // Check UI input system first (for menu navigation during loading)
            if (useUIInputSystem && uiNavigateAction != null)
            {
                Vector2 navigateInput = uiNavigateAction.ReadValue<Vector2>();

                // Detect left/right input changes (edge detection)
                if (navigateInput.x > 0.5f && lastNavigateInput.x <= 0.5f)
                {
                    Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] UI Move Right detected - navigating to next model");
                    NavigateNext();
                }
                else if (navigateInput.x < -0.5f && lastNavigateInput.x >= -0.5f)
                {
                    Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] UI Move Left detected - navigating to previous model");
                    NavigatePrevious();
                }

                lastNavigateInput = navigateInput;
            }
            // Fallback keyboard input if InputSystem actions aren't set up
            else if (moveLeftAction == null || moveRightAction == null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                {
                    Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] Left/A key pressed - navigating to previous model");
                    NavigatePrevious();
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                {
                    Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] Right/D key pressed - navigating to next model");
                    NavigateNext();
                }
            }
        }

        private void OnMoveLeft(InputAction.CallbackContext context)
        {
            NavigatePrevious();
        }

        private void OnMoveRight(InputAction.CallbackContext context)
        {
            NavigateNext();
        }

        private void NavigatePrevious()
        {
            if (loading3DController != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] Calling NavigateToPreviousModel()");
                loading3DController.NavigateToPreviousModel();
            }
            else
            {
                Debugger.LogWarning(LogCategory.Loading, "[Loading3DNavigation] loading3DController is null!");
            }
        }

        private void NavigateNext()
        {
            if (loading3DController != null)
            {
                Debugger.LogInformation(LogCategory.Loading, "[Loading3DNavigation] Calling NavigateToNextModel()");
                loading3DController.NavigateToNextModel();
            }
            else
            {
                Debugger.LogWarning(LogCategory.Loading, "[Loading3DNavigation] loading3DController is null!");
            }
        }

        private void OnGUI()
        {
            // Removed on-screen navigation instructions
        }
    }
}