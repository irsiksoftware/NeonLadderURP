using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

        protected List<string> Keypresses = new List<string>();

        private bool showControllerDebugLastState = Constants.DisplayControllerDebugInfo;

        private InputDevice controller;


        protected virtual void ConfigureControls(Player player)
        {
            ControllerDebugging.PrintDebugControlConfiguration(player);
        }

        protected void UpdateRotation(int moveDirection)
        {
            // Adjust the character's local rotation to align with the forward-facing orientation
            // Assuming the forward direction is along the camera view or along a consistent world axis
            if (moveDirection != 0)
            {
                // Setting rotation to face right if moving right (1), and left if moving left (-1)
                float yRotation = moveDirection == 1 ? -270 : -90;
                transform.parent.localRotation = Quaternion.Euler(0, yRotation, 0);
            }
        }


        void OnDrawGizmos()
        {
            Constants.DisplayAnimationDebugInfo = true;
            Constants.DisplayPlayerActionDebugInfo = true;
            Constants.DisplayKeyPresses = true;
            Constants.DisplayControllerDebugInfo = true;
        }

        protected void InitAction(Animator animator, string name, float duration)
        {
            SetFlagAndAnimation(animator, name, true);
            StartCoroutine(ResetFlagAndAnimation(animator, name, duration));
        }

        private void SetFlag(string propertyName, bool value)
        {
            var prop = GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                prop.SetValue(this, value);
            }

        }
        private void SetFlag(string propertyName, float value)
        {
            GetType().GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(this, value);
        }

        private void SetFlagAndAnimation(Animator animator, string name, bool value)
        {
            SetFlag(name, value);
            animator.SetBool(name, value);
        }

        private IEnumerator ResetFlagAndAnimation(Animator animator, string name, float duration)
        {
            yield return new WaitForSeconds(duration);
            GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)?.SetValue(this, false);
            animator.SetBool(name, false);
        }

        protected virtual void Update()
        {
            if (showControllerDebugLastState != Constants.DisplayControllerDebugInfo)
            {
                showControllerDebugLastState = Constants.DisplayControllerDebugInfo;
            }


            if (InputDebuggingText != null)
            {
                InputDebuggingText.gameObject.SetActive(Constants.DisplayKeyPresses);
                if (Constants.DisplayKeyPresses)
                {
                    InputDebuggingText.text = $"Keys: {string.Join(", ", Keypresses)}";
                }
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

        protected void UpdateJumpAnimationParameters(Player player)
        {
            bool isGrounded = player.IsGrounded;
            float verticalVelocity = player.velocity.y;
            //player.animator.SetFloat("velocityX", Mathf.Abs(player.velocity.x));
            //player.animator.SetFloat("velocityY", verticalVelocity);
            //player.animator.SetBool("grounded", isGrounded);
            float fallingThreshold = -0.01f;
            if (!isGrounded && verticalVelocity < fallingThreshold)
            {
                //player?.animator.SetBool("isFalling", true);
            }
            else if (isGrounded)
            {
                //player?.animator.SetBool("isFalling", false);
            }
        }
    }
}
