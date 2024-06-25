using NeonLadder.Common;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    public class BaseAction : MonoBehaviour
    {
        [SerializeField]
        public TextMeshProUGUI PlayerActionsDebugText;

        [SerializeField]
        public TextMeshProUGUI AnimationDebuggingText;

        [SerializeField]
        public TextMeshProUGUI InputDebuggingText;

        [SerializeField]
        public TextMeshProUGUI ControllerNameDebuggingText;

        [SerializeField]
        public TextMeshProUGUI TouchScreenSupportDebuggingText;

        private bool HasTouchScreen => Input.touchSupported;

        private bool showControllerDebugLastState = Constants.DisplayControllerDebugInfo;

        private InputDevice controller;

        protected virtual void ConfigureControls(Player player)
        {
            ControllerDebugging.PrintDebugControlConfiguration(player);
        }

        void OnDrawGizmos()
        {
            Constants.DisplayAnimationDebugInfo = true;
            Constants.DisplayPlayerActionDebugInfo = true;
            Constants.DisplayKeyPresses = true;
            Constants.DisplayControllerDebugInfo = true;
        }

        protected virtual void Awake()
        {
            // Initialize the controller field with the first connected controller device
            if (Gamepad.current != null)
            {
                controller = Gamepad.current;
            }
            else if (Keyboard.current != null)
            {
                controller = Keyboard.current;
            }
            else
            {
                Debug.LogWarning("No controller or keyboard found.");
            }
        }

        protected void OnDestroy() { }

        protected void OnDisable() { }

        protected virtual void Update()
        {
            if (showControllerDebugLastState != Constants.DisplayControllerDebugInfo)
            {
                showControllerDebugLastState = Constants.DisplayControllerDebugInfo;
            }

            if (TouchScreenSupportDebuggingText != null)
            {
                TouchScreenSupportDebuggingText.gameObject.SetActive(Constants.DisplayTouchScreenDebugInfo);
                if (Constants.DisplayTouchScreenDebugInfo)
                {
                    TouchScreenSupportDebuggingText.text = $"Touch Screen support: {HasTouchScreen}";
                }
            }

            if (ControllerNameDebuggingText != null)
            {
                ControllerNameDebuggingText.gameObject.SetActive(Constants.DisplayControllerDebugInfo);
                if (Constants.DisplayControllerDebugInfo)
                {
                    if (controller != null)
                    {
                        ControllerNameDebuggingText.text = $"Controller: {ControllerDebugging.GetDeviceDebugText(controller)}";
                    }
                    else
                    {
                        ControllerNameDebuggingText.text = "Controller: None";
                    }
                }
            }
        }
    }
}
