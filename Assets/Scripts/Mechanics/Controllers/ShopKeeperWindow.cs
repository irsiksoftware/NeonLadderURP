using NeonLadder.Mechanics.Enums;
using NeonLadder.Items.Shop;
using NeonLadder.UI.Shop;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonLadder.Mechanics.Controllers
{
    /// <summary>
    /// Updated ShopKeeperWindow that integrates with the new item system
    /// </summary>
    public class ShopKeeperWindow : MonoBehaviour
    {
        [Header("Legacy Support")]
        public InputActionAsset inputActions;
        
        [Header("New Shop System")]
        [SerializeField] private bool useNewShopSystem = true; // Re-enabled with proper assembly refs
        [SerializeField] private ShopWindowUI shopWindowUI; // Proper typing restored
        
        // Legacy system
        private GameObject shopKeeperCanvas;
        private InputActionMap playerActionMap;
        
        // New system - proper typing restored
        private NPCShopSource npcShopSource;
        private Player currentPlayer;

        public void Start()
        {
            // Initialize input system
            InitializeInputSystem();
            
            if (useNewShopSystem)
            {
                InitializeNewSystem();
            }
            else
            {
                InitializeLegacySystem();
            }
        }
        
        private void InitializeInputSystem()
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

            playerActionMap = inputActions.FindActionMap("Player");
        }
        
        private void InitializeNewSystem()
        {
            // Find NPCShopSource component
            npcShopSource = GetComponent<NPCShopSource>();
            if (npcShopSource == null)
            {
                npcShopSource = GetComponentInParent<NPCShopSource>();
            }
            
            if (npcShopSource == null)
            {
                Debug.LogError("ShopKeeperWindow: No NPCShopSource found! Add NPCShopSource component to use new shop system.");
                useNewShopSystem = false;
                InitializeLegacySystem();
                return;
            }
            
            // Find ShopWindowUI if not assigned
            if (shopWindowUI == null)
            {
                shopWindowUI = FindObjectOfType<ShopWindowUI>();
                
                if (shopWindowUI == null)
                {
                    // Try finding by tag
                    GameObject shopCanvas = GameObject.FindGameObjectWithTag(Tags.ShopkeeperWindow.ToString());
                    if (shopCanvas != null)
                    {
                        shopWindowUI = shopCanvas.GetComponent<ShopWindowUI>();
                    }
                }
            }
            
            if (shopWindowUI == null)
            {
                Debug.LogError("ShopKeeperWindow: No ShopWindowUI found! Create a UI canvas with ShopWindowUI component.");
                useNewShopSystem = false;
                InitializeLegacySystem();
                return;
            }
            
            // Make sure shop UI starts closed
            shopWindowUI.gameObject.SetActive(false);
        }
        
        private void InitializeLegacySystem()
        {
            shopKeeperCanvas = GameObject.FindGameObjectWithTag(Tags.ShopkeeperWindow.ToString());

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
                currentPlayer = other.GetComponent<Player>();
                
                if (useNewShopSystem)
                {
                    OpenNewShop();
                }
                else
                {
                    OpenLegacyShop();
                }
                
                DisablePlayerAttack();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (useNewShopSystem)
                {
                    CloseNewShop();
                }
                else
                {
                    CloseLegacyShop();
                }
                
                EnablePlayerAttack();
                currentPlayer = null;
            }
        }
        
        private void OpenNewShop()
        {
            if (shopWindowUI != null && npcShopSource != null && currentPlayer != null)
            {
                shopWindowUI.OpenShop(npcShopSource, currentPlayer);
            }
        }
        
        private void CloseNewShop()
        {
            if (shopWindowUI != null)
            {
                shopWindowUI.CloseShop();
            }
        }
        
        private void OpenLegacyShop()
        {
            if (shopKeeperCanvas != null)
            {
                shopKeeperCanvas.SetActive(true);
            }
        }
        
        private void CloseLegacyShop()
        {
            if (shopKeeperCanvas != null)
            {
                shopKeeperCanvas.SetActive(false);
            }
        }
        
        private void DisablePlayerAttack()
        {
            if (playerActionMap != null)
            {
                var attackAction = playerActionMap.FindAction("Attack");
                attackAction?.Disable();
            }
        }
        
        private void EnablePlayerAttack()
        {
            if (playerActionMap != null)
            {
                var attackAction = playerActionMap.FindAction("Attack");
                attackAction?.Enable();
            }
        }
        
        /// <summary>
        /// Switch between new and legacy shop systems at runtime
        /// </summary>
        public void SetUseNewSystem(bool useNew)
        {
            useNewShopSystem = useNew;
            
            // Close current shop
            if (currentPlayer != null)
            {
                CloseNewShop();
                CloseLegacyShop();
            }
        }
    }
}
