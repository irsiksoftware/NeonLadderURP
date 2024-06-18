using NeonLadder.Mechanics.Enums;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    public class ShopKeeperWindow : MonoBehaviour
    {
        public InputActionAsset inputActions;
        private GameObject shopKeeperCanvas;
        private InputActionMap playerActionMap;

        public void Start()
        {
            // Load InputActionAsset from Resources if not assigned in the inspector
            if (inputActions == null)
            {
                inputActions = Resources.Load<InputActionAsset>("Controls/PlayerControls");
                if (inputActions == null)
                {
                    Debug.LogError("ShopKeeperWindow: Failed to load InputActionAsset from Resources.");
                    return;
                }
            }

            shopKeeperCanvas = GameObject.FindGameObjectWithTag(Tags.ShopkeeperWindow.ToString());
            playerActionMap = inputActions.FindActionMap("Player");

            if (shopKeeperCanvas == null)
            {
                Debug.LogError("ShopKeeperWindow: No shopkeeper canvas found in scene");
                return;
            }

            shopKeeperCanvas.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                shopKeeperCanvas.SetActive(true);
                var attackAction = playerActionMap.FindAction("Attack");
                if (attackAction != null)
                {
                    attackAction.Disable();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                shopKeeperCanvas.SetActive(false);
                var attackAction = playerActionMap.FindAction("Attack");
                if (attackAction != null)
                {
                    attackAction.Enable();
                }
            }
        }
    }
}
