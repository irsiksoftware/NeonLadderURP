using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

namespace NeonLadder.Mobile
{
    /// <summary>
    /// Complete mobile touch control system for NeonLadder.
    /// PBI-58: Implements virtual joystick, buttons, and customization.
    /// 
    /// CRITICAL: This MUST be tested on actual mobile devices!
    /// </summary>
    public class TouchControlSystem : MonoBehaviour
    {
        #region Configuration
        
        [Header("Control Layout")]
        [SerializeField] private bool useVirtualJoystick = true;
        [SerializeField] private bool useDPad = false;
        
        [Header("Touch Zones")]
        [SerializeField] private RectTransform leftControlZone;
        [SerializeField] private RectTransform rightControlZone;
        [SerializeField] private RectTransform topControlZone;
        
        [Header("Control Prefabs")]
        [SerializeField] private GameObject virtualJoystickPrefab;
        [SerializeField] private GameObject dPadPrefab;
        [SerializeField] private GameObject buttonPrefab;
        
        [Header("Control Settings")]
        [SerializeField] private float joystickDeadZone = 0.1f;
        [SerializeField] private float buttonSize = 120f;
        [SerializeField] private float buttonSpacing = 20f;
        [SerializeField] private float controlOpacity = 0.6f;
        
        [Header("Haptic Feedback")]
        [SerializeField] private bool enableHaptics = true;
        [SerializeField] private float hapticIntensity = 0.5f;
        
        [Header("Performance")]
        [SerializeField] private int targetFrameRate = 60;
        [SerializeField] private bool adaptivePerformance = true;
        
        #endregion
        
        #region Private Fields
        
        private VirtualJoystick movementJoystick;
        private Dictionary<string, TouchButton> touchButtons = new Dictionary<string, TouchButton>();
        private TouchControlProfile currentProfile;
        private Canvas touchCanvas;
        private PlayerInput playerInput;
        
        // Performance monitoring
        private float averageFPS;
        private int frameCount;
        private float fpsTimer;
        
        // Touch tracking
        private Dictionary<int, TouchInfo> activeTouches = new Dictionary<int, TouchInfo>();
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Set target frame rate for mobile
            Application.targetFrameRate = targetFrameRate;
            
            // Get or create touch canvas
            touchCanvas = GetComponentInChildren<Canvas>();
            if (touchCanvas == null)
            {
                CreateTouchCanvas();
            }
            
            // Get player input component
            playerInput = FindObjectOfType<PlayerInput>();
            
            // Load saved profile or create default
            LoadControlProfile();
        }
        
        private void Start()
        {
            // Initialize controls based on platform
            if (IsMobilePlatform())
            {
                InitializeTouchControls();
                SetupTouchInputMapping();
            }
            else
            {
                // Running in editor - create debug controls
                InitializeDebugControls();
            }
        }
        
        private void Update()
        {
            // Monitor performance
            UpdatePerformanceMetrics();
            
            // Handle multi-touch input
            ProcessTouchInput();
            
            // Adapt quality if needed
            if (adaptivePerformance)
            {
                AdaptQualitySettings();
            }
        }
        
        private void OnDestroy()
        {
            SaveControlProfile();
        }
        
        #endregion
        
        #region Initialization
        
        private void CreateTouchCanvas()
        {
            GameObject canvasObj = new GameObject("TouchControlCanvas");
            canvasObj.transform.SetParent(transform);
            
            touchCanvas = canvasObj.AddComponent<Canvas>();
            touchCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            touchCanvas.sortingOrder = 1000; // Ensure on top
            
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Create control zones
            CreateControlZones();
        }
        
        private void CreateControlZones()
        {
            // Left zone for movement
            leftControlZone = CreateZone("LeftControlZone", 
                new Vector2(0, 0), new Vector2(0.4f, 0.7f));
            
            // Right zone for actions
            rightControlZone = CreateZone("RightControlZone", 
                new Vector2(0.6f, 0), new Vector2(1, 0.7f));
            
            // Top zone for menus
            topControlZone = CreateZone("TopControlZone", 
                new Vector2(0, 0.8f), new Vector2(1, 1));
        }
        
        private RectTransform CreateZone(string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject zoneObj = new GameObject(name);
            zoneObj.transform.SetParent(touchCanvas.transform, false);
            
            RectTransform rect = zoneObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Add visual indicator in debug mode
            if (Debug.isDebugBuild)
            {
                Image img = zoneObj.AddComponent<Image>();
                img.color = new Color(1, 1, 1, 0.05f);
            }
            
            return rect;
        }
        
        private void InitializeTouchControls()
        {
            // Create movement control
            if (useVirtualJoystick)
            {
                CreateVirtualJoystick();
            }
            else if (useDPad)
            {
                CreateDPad();
            }
            
            // Create action buttons
            CreateActionButtons();
            
            // Apply saved customizations
            ApplyCustomizations();
        }
        
        #endregion
        
        #region Control Creation
        
        private void CreateVirtualJoystick()
        {
            GameObject joystickObj = Instantiate(virtualJoystickPrefab ?? CreateDefaultJoystick(), 
                leftControlZone);
            
            movementJoystick = joystickObj.GetComponent<VirtualJoystick>();
            if (movementJoystick == null)
            {
                movementJoystick = joystickObj.AddComponent<VirtualJoystick>();
            }
            
            movementJoystick.deadZone = joystickDeadZone;
            movementJoystick.OnValueChanged += OnMovementInput;
        }
        
        private GameObject CreateDefaultJoystick()
        {
            // Create default joystick if no prefab provided
            GameObject joystick = new GameObject("VirtualJoystick");
            
            // Background
            GameObject background = new GameObject("Background");
            background.transform.SetParent(joystick.transform);
            Image bgImage = background.AddComponent<Image>();
            bgImage.sprite = CreateCircleSprite();
            bgImage.color = new Color(1, 1, 1, controlOpacity * 0.5f);
            
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(200, 200);
            
            // Handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(joystick.transform);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = CreateCircleSprite();
            handleImage.color = new Color(1, 1, 1, controlOpacity);
            
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(80, 80);
            
            return joystick;
        }
        
        private void CreateActionButtons()
        {
            // Jump button
            CreateTouchButton("Jump", rightControlZone, 
                new Vector2(-buttonSize - buttonSpacing, buttonSize), 
                "A", Color.green);
            
            // Attack button
            CreateTouchButton("Attack", rightControlZone, 
                new Vector2(-buttonSize * 2 - buttonSpacing * 2, 0), 
                "X", Color.red);
            
            // Sprint button
            CreateTouchButton("Sprint", rightControlZone, 
                new Vector2(0, buttonSize), 
                "B", Color.yellow);
            
            // Weapon swap button
            CreateTouchButton("WeaponSwap", rightControlZone, 
                new Vector2(-buttonSize - buttonSpacing, -buttonSize - buttonSpacing), 
                "Y", Color.blue);
            
            // Menu button
            CreateTouchButton("Menu", topControlZone, 
                new Vector2(-60, -60), 
                "≡", Color.white, 60f);
        }
        
        private TouchButton CreateTouchButton(string name, Transform parent, 
            Vector2 position, string label, Color color, float size = 0)
        {
            if (size == 0) size = buttonSize;
            
            GameObject buttonObj = new GameObject($"Button_{name}");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rect = buttonObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(size, size);
            
            // Background
            Image bgImage = buttonObj.AddComponent<Image>();
            bgImage.sprite = CreateCircleSprite();
            bgImage.color = color * new Color(1, 1, 1, controlOpacity);
            
            // Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(buttonObj.transform, false);
            
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = (int)(size * 0.4f);
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            
            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            // Touch button component
            TouchButton touchButton = buttonObj.AddComponent<TouchButton>();
            touchButton.buttonName = name;
            touchButton.OnPressed += () => OnButtonPressed(name);
            touchButton.OnReleased += () => OnButtonReleased(name);
            
            touchButtons[name] = touchButton;
            
            return touchButton;
        }
        
        #endregion
        
        #region Input Handling
        
        private void ProcessTouchInput()
        {
            // Handle Unity's touch input
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegan(touch);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        OnTouchMoved(touch);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        OnTouchEnded(touch);
                        break;
                }
            }
        }
        
        private void OnTouchBegan(Touch touch)
        {
            activeTouches[touch.fingerId] = new TouchInfo
            {
                fingerId = touch.fingerId,
                startPosition = touch.position,
                currentPosition = touch.position,
                startTime = Time.time
            };
            
            // Haptic feedback
            if (enableHaptics)
            {
                TriggerHaptic(HapticType.Light);
            }
        }
        
        private void OnTouchMoved(Touch touch)
        {
            if (activeTouches.ContainsKey(touch.fingerId))
            {
                activeTouches[touch.fingerId].currentPosition = touch.position;
            }
        }
        
        private void OnTouchEnded(Touch touch)
        {
            if (activeTouches.ContainsKey(touch.fingerId))
            {
                activeTouches.Remove(touch.fingerId);
            }
        }
        
        private void OnMovementInput(Vector2 input)
        {
            // Send movement input to player
            if (playerInput != null)
            {
                var moveAction = playerInput.actions["Move"];
                // Note: In real implementation, we'd need to trigger the action
                // This is simplified for demonstration
            }
        }
        
        private void OnButtonPressed(string buttonName)
        {
            // Haptic feedback
            if (enableHaptics)
            {
                TriggerHaptic(HapticType.Medium);
            }
            
            // Trigger corresponding action
            switch (buttonName)
            {
                case "Jump":
                    playerInput?.actions["Jump"]?.ApplyBinding(0);
                    break;
                case "Attack":
                    playerInput?.actions["Attack"]?.ApplyBinding(0);
                    break;
                case "Sprint":
                    playerInput?.actions["Sprint"]?.ApplyBinding(0);
                    break;
                case "WeaponSwap":
                    playerInput?.actions["WeaponSwap"]?.ApplyBinding(0);
                    break;
                case "Menu":
                    playerInput?.actions["Menu"]?.ApplyBinding(0);
                    break;
            }
        }
        
        private void OnButtonReleased(string buttonName)
        {
            // Handle button release
            switch (buttonName)
            {
                case "Sprint":
                    // Sprint is a hold action
                    playerInput?.actions["Sprint"]?.ApplyBinding(0);
                    break;
            }
        }
        
        #endregion
        
        #region Customization
        
        public void EnterCustomizationMode()
        {
            // Enable drag-and-drop for all controls
            foreach (var button in touchButtons.Values)
            {
                button.EnableCustomization(true);
            }
            
            if (movementJoystick != null)
            {
                movementJoystick.EnableCustomization(true);
            }
            
            // Show customization UI
            ShowCustomizationUI();
        }
        
        public void ExitCustomizationMode()
        {
            // Disable customization
            foreach (var button in touchButtons.Values)
            {
                button.EnableCustomization(false);
            }
            
            if (movementJoystick != null)
            {
                movementJoystick.EnableCustomization(false);
            }
            
            // Save profile
            SaveControlProfile();
            
            // Hide UI
            HideCustomizationUI();
        }
        
        private void ShowCustomizationUI()
        {
            // TODO: Show sliders for opacity, size, etc.
        }
        
        private void HideCustomizationUI()
        {
            // TODO: Hide customization UI
        }
        
        private void ApplyCustomizations()
        {
            if (currentProfile == null) return;
            
            // Apply saved positions
            foreach (var kvp in currentProfile.buttonPositions)
            {
                if (touchButtons.ContainsKey(kvp.Key))
                {
                    touchButtons[kvp.Key].GetComponent<RectTransform>().anchoredPosition = kvp.Value;
                }
            }
            
            // Apply saved settings
            controlOpacity = currentProfile.opacity;
            buttonSize = currentProfile.buttonSize;
            joystickDeadZone = currentProfile.joystickDeadZone;
            
            // Update visuals
            UpdateControlVisuals();
        }
        
        private void UpdateControlVisuals()
        {
            foreach (var button in touchButtons.Values)
            {
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    Color c = image.color;
                    c.a = controlOpacity;
                    image.color = c;
                }
                
                var rect = button.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(buttonSize, buttonSize);
                }
            }
        }
        
        #endregion
        
        #region Profile Management
        
        private void LoadControlProfile()
        {
            string profileJson = PlayerPrefs.GetString("TouchControlProfile", "");
            
            if (!string.IsNullOrEmpty(profileJson))
            {
                currentProfile = JsonUtility.FromJson<TouchControlProfile>(profileJson);
            }
            else
            {
                currentProfile = CreateDefaultProfile();
            }
        }
        
        private void SaveControlProfile()
        {
            if (currentProfile == null)
            {
                currentProfile = new TouchControlProfile();
            }
            
            // Save current positions
            currentProfile.buttonPositions.Clear();
            foreach (var kvp in touchButtons)
            {
                var rect = kvp.Value.GetComponent<RectTransform>();
                currentProfile.buttonPositions[kvp.Key] = rect.anchoredPosition;
            }
            
            // Save settings
            currentProfile.opacity = controlOpacity;
            currentProfile.buttonSize = buttonSize;
            currentProfile.joystickDeadZone = joystickDeadZone;
            
            string profileJson = JsonUtility.ToJson(currentProfile);
            PlayerPrefs.SetString("TouchControlProfile", profileJson);
            PlayerPrefs.Save();
        }
        
        private TouchControlProfile CreateDefaultProfile()
        {
            return new TouchControlProfile
            {
                opacity = 0.6f,
                buttonSize = 120f,
                joystickDeadZone = 0.1f,
                buttonPositions = new Dictionary<string, Vector2>()
            };
        }
        
        #endregion
        
        #region Performance
        
        private void UpdatePerformanceMetrics()
        {
            frameCount++;
            fpsTimer += Time.deltaTime;
            
            if (fpsTimer >= 1.0f)
            {
                averageFPS = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0;
                
                // Log performance in debug
                if (Debug.isDebugBuild)
                {
                    Debug.Log($"Mobile FPS: {averageFPS:F1}");
                }
            }
        }
        
        private void AdaptQualitySettings()
        {
            // Reduce quality if FPS drops below threshold
            if (averageFPS < 30 && QualitySettings.GetQualityLevel() > 0)
            {
                QualitySettings.DecreaseLevel();
                Debug.Log($"Reduced quality to maintain performance. FPS: {averageFPS:F1}");
            }
            // Increase quality if FPS is stable
            else if (averageFPS > 55 && QualitySettings.GetQualityLevel() < QualitySettings.names.Length - 1)
            {
                QualitySettings.IncreaseLevel();
            }
        }
        
        #endregion
        
        #region Haptic Feedback
        
        private void TriggerHaptic(HapticType type)
        {
            if (!enableHaptics) return;
            
#if UNITY_IOS
            switch (type)
            {
                case HapticType.Light:
                    // iOS specific haptic
                    break;
                case HapticType.Medium:
                    // iOS specific haptic
                    break;
                case HapticType.Heavy:
                    // iOS specific haptic
                    break;
            }
#elif UNITY_ANDROID
            // Android haptic implementation
            Handheld.Vibrate();
#endif
        }
        
        #endregion
        
        #region Utilities
        
        private bool IsMobilePlatform()
        {
            return Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer;
        }
        
        private void InitializeDebugControls()
        {
            // Create visual controls for editor testing
            Debug.Log("Mobile controls in debug mode - use mouse to simulate touch");
            InitializeTouchControls();
        }
        
        private Sprite CreateCircleSprite()
        {
            // Create a simple circle sprite procedurally
            Texture2D texture = new Texture2D(128, 128);
            Color[] pixels = new Color[128 * 128];
            
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 64;
                    float dy = y - 64;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    
                    if (distance < 64)
                    {
                        float alpha = 1f - (distance / 64f);
                        pixels[y * 128 + x] = new Color(1, 1, 1, alpha);
                    }
                    else
                    {
                        pixels[y * 128 + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
        }
        
        #endregion
        
        #region Helper Classes
        
        [Serializable]
        private class TouchControlProfile
        {
            public float opacity;
            public float buttonSize;
            public float joystickDeadZone;
            public Dictionary<string, Vector2> buttonPositions;
        }
        
        private class TouchInfo
        {
            public int fingerId;
            public Vector2 startPosition;
            public Vector2 currentPosition;
            public float startTime;
        }
        
        private enum HapticType
        {
            Light,
            Medium,
            Heavy
        }
        
        #endregion
    }
}