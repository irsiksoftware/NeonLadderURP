using NeonLadder.Mechanics.Controllers;
using NeonLadder.Debugging;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.UI
{
    /// <summary>
    /// The MetaGameController is responsible for switching control between the high level
    /// contexts of the application, eg the Main Menu and Gameplay systems.
    /// </summary>
    public class MetaGameController : MonoBehaviour
    {        
        /// <summary>
        /// The main UI object which used for the menu.
        /// </summary>
        public MainUIController mainMenu;

        /// <summary>
        /// A list of canvas objects which are used during gameplay (when the main ui is turned off)
        /// </summary>
        public Canvas[] gamePlayCanvasii;

        /// <summary>
        /// The game controller.
        /// </summary>
        public Game game;
        public InputActionReference toggleMenuAction;
        public InputActionAsset inputActions;

        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;
        private bool showMainCanvas = false;

        void Awake()
        {
            // Validate required references
            if (inputActions == null)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] inputActions is null on {gameObject.name}! Menu functionality will not work.");
                enabled = false;
                return;
            }

            // Find the Player and UI action maps
            playerActionMap = inputActions.FindActionMap("Player");
            uiActionMap = inputActions.FindActionMap("UI");
            
            if (playerActionMap == null)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] 'Player' action map not found in {inputActions.name}! Menu functionality will not work.");
                enabled = false;
                return;
            }
            
            if (uiActionMap == null)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] 'UI' action map not found in {inputActions.name}! Menu functionality will not work.");
                enabled = false;
                return;
            }

            // Safely find StatUI canvas
            var statUI = GetComponentInChildren<StatUI>();
            if (statUI != null)
            {
                var canvas = statUI.gameObject.GetComponent<Canvas>();
                if (canvas != null && gamePlayCanvasii != null && gamePlayCanvasii.Length > 0)
                {
                    gamePlayCanvasii[0] = canvas;
                }
                else
                {
                    Debugger.LogWarning(LogCategory.UI, $"[MetaGameController] Could not find Canvas on StatUI or gamePlayCanvasii is not properly configured on {gameObject.name}");
                }
            }
            else
            {
                Debugger.LogWarning(LogCategory.UI, $"[MetaGameController] StatUI component not found in children of {gameObject.name}");
            }
        }

        void OnEnable()
        {
            // Validate references before enabling
            if (uiActionMap == null)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] uiActionMap is null on {gameObject.name}! Cannot enable menu functionality.");
                return;
            }
            
            if (toggleMenuAction == null)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] toggleMenuAction is null on {gameObject.name}! Menu toggle will not work.");
                return;
            }

            // Enable the UI action map which includes the ToggleMenu action
            try
            {
                uiActionMap.Enable();
                toggleMenuAction.action.performed += OnToggleMenu;
                _ToggleMainMenu(showMainCanvas);
                Debugger.LogInformation(LogCategory.UI, $"[MetaGameController] Successfully enabled on {gameObject.name}");
            }
            catch (System.Exception e)
            {
                Debugger.LogError(LogCategory.UI, $"[MetaGameController] Failed to enable on {gameObject.name}: {e.Message}");
            }
        }

        void OnDisable()
        {
            // Safely unsubscribe and disable
            if (toggleMenuAction?.action != null)
            {
                toggleMenuAction.action.performed -= OnToggleMenu;
            }
            
            if (uiActionMap != null)
            {
                uiActionMap.Disable();
            }
            
            Debugger.LogInformation(LogCategory.UI, $"[MetaGameController] Disabled on {gameObject.name}");
        }

        void OnToggleMenu(InputAction.CallbackContext context)
        {
            ToggleMainMenu(!showMainCanvas);
        }

        public void ToggleMainMenu(bool show)
        {
            if (showMainCanvas != show)
            {
                _ToggleMainMenu(show);
            }
        }

        void _ToggleMainMenu(bool show)
        {
            if (show)
            {
                Time.timeScale = 0;
                mainMenu.gameObject.SetActive(true);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(false);
                playerActionMap.Disable(); // Disable the Player action map
            }
            else
            {
                Time.timeScale = 1;
                mainMenu.gameObject.SetActive(false);
                foreach (var i in gamePlayCanvasii) i.gameObject.SetActive(true);
                playerActionMap.Enable(); // Enable the Player action map
            }

            showMainCanvas = show;
        }
    }
}
