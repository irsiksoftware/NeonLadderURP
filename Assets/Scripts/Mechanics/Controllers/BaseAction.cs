using Assets.Scripts;
using System.Collections.Generic;
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
                    ControllerNameDebuggingText.text = $"Controller: {ControllerDebugging.GetDeviceDebugText(controller)}";
                }
            }
        }
    }
}
