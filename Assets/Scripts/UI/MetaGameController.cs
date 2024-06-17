using NeonLadder.Mechanics.Controllers;
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
            // Find the Player and UI action maps
            playerActionMap = inputActions.FindActionMap("Player");
            uiActionMap = inputActions.FindActionMap("UI");
        }

        void OnEnable()
        {
            // Enable the UI action map which includes the ToggleMenu action
            uiActionMap.Enable();
            toggleMenuAction.action.performed += OnToggleMenu;
            _ToggleMainMenu(showMainCanvas);
        }

        void OnDisable()
        {
            toggleMenuAction.action.performed -= OnToggleMenu;
            uiActionMap.Disable();
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
