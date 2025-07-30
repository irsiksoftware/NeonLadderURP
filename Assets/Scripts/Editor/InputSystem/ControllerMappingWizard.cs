using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Editor.InputSystem
{
    /// <summary>
    /// Tony Stark & Storm's Controller Button Mapping Wizard
    /// Real-time controller testing and button remapping interface for rapid development
    /// Perfect for testing until production UI is ready!
    /// </summary>
    public class ControllerMappingWizard : EditorWindow
    {
        private InputActionAsset playerControls;
        private InputActionMap playerActionMap;
        private Vector2 scrollPosition;
        private bool isListening = false;
        private string currentListeningAction = "";
        private InputAction listeningAction;
        
        // UI State
        private bool showControllerStatus = true;
        private bool showActionMappings = true;
        private bool showRealTimeInput = true;
        private Dictionary<string, bool> actionFoldouts = new Dictionary<string, bool>();
        
        // Controller detection
        private Gamepad currentGamepad;
        private string controllerInfo = "No controller detected";
        
        // Real-time input display
        private Dictionary<string, float> realtimeValues = new Dictionary<string, float>();
        private Dictionary<string, bool> realtimeButtons = new Dictionary<string, bool>();

        [MenuItem("NeonLadder/Input System/Controller Mapping Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<ControllerMappingWizard>("Controller Mapping Wizard");
            window.minSize = new Vector2(600, 800);
            window.Show();
        }

        private void OnEnable()
        {
            LoadPlayerControls();
            DetectController();
            EditorApplication.update += UpdateRealtimeInput;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateRealtimeInput;
            if (listeningAction != null)
            {
                StopListening();
            }
        }

        private void LoadPlayerControls()
        {
            playerControls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
            if (playerControls != null)
            {
                playerActionMap = playerControls.FindActionMap("Player");
                
                // Initialize foldout states
                if (playerActionMap != null)
                {
                    foreach (var action in playerActionMap.actions)
                    {
                        if (!actionFoldouts.ContainsKey(action.name))
                        {
                            actionFoldouts[action.name] = false;
                        }
                    }
                }
            }
        }

        private void DetectController()
        {
            currentGamepad = Gamepad.current;
            if (currentGamepad != null)
            {
                controllerInfo = $"{currentGamepad.displayName} ({currentGamepad.device.deviceId})";
            }
            else
            {
                controllerInfo = "No controller detected - Connect a gamepad";
            }
        }

        private void UpdateRealtimeInput()
        {
            if (currentGamepad == null)
            {
                DetectController();
                return;
            }

            // Update real-time input values for display
            realtimeValues["Left Stick X"] = currentGamepad.leftStick.x.ReadValue();
            realtimeValues["Left Stick Y"] = currentGamepad.leftStick.y.ReadValue();
            realtimeValues["Right Stick X"] = currentGamepad.rightStick.x.ReadValue();
            realtimeValues["Right Stick Y"] = currentGamepad.rightStick.y.ReadValue();
            realtimeValues["Left Trigger"] = currentGamepad.leftTrigger.ReadValue();
            realtimeValues["Right Trigger"] = currentGamepad.rightTrigger.ReadValue();

            // Update button states
            realtimeButtons["A/Cross"] = currentGamepad.buttonSouth.isPressed;
            realtimeButtons["B/Circle"] = currentGamepad.buttonEast.isPressed;
            realtimeButtons["X/Square"] = currentGamepad.buttonWest.isPressed;
            realtimeButtons["Y/Triangle"] = currentGamepad.buttonNorth.isPressed;
            realtimeButtons["Left Bumper"] = currentGamepad.leftShoulder.isPressed;
            realtimeButtons["Right Bumper"] = currentGamepad.rightShoulder.isPressed;
            realtimeButtons["Start"] = currentGamepad.startButton.isPressed;
            realtimeButtons["Select"] = currentGamepad.selectButton.isPressed;
            realtimeButtons["D-Pad Up"] = currentGamepad.dpad.up.isPressed;
            realtimeButtons["D-Pad Down"] = currentGamepad.dpad.down.isPressed;
            realtimeButtons["D-Pad Left"] = currentGamepad.dpad.left.isPressed;
            realtimeButtons["D-Pad Right"] = currentGamepad.dpad.right.isPressed;

            Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Header
            GUILayout.Label("🎮 Controller Mapping Wizard", EditorStyles.largeLabel);
            GUILayout.Label("Real-time controller testing and button remapping", EditorStyles.miniLabel);
            
            EditorGUILayout.Space(10);
            
            if (playerControls == null)
            {
                EditorGUILayout.HelpBox("PlayerControls.inputactions not found in Resources/Controls/", MessageType.Error);
                if (GUILayout.Button("Reload Player Controls"))
                {
                    LoadPlayerControls();
                }
                EditorGUILayout.EndVertical();
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Controller Status Section
            DrawControllerStatus();
            
            EditorGUILayout.Space(10);
            
            // Real-time Input Section
            DrawRealtimeInput();
            
            EditorGUILayout.Space(10);
            
            // Action Mappings Section
            DrawActionMappings();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawControllerStatus()
        {
            showControllerStatus = EditorGUILayout.BeginFoldoutHeaderGroup(showControllerStatus, "🔌 Controller Status");
            if (showControllerStatus)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                GUILayout.Label($"Controller: {controllerInfo}");
                
                if (currentGamepad == null)
                {
                    EditorGUILayout.HelpBox("Connect a gamepad to see real-time input and test mappings", MessageType.Info);
                }
                else
                {
                    GUILayout.Label($"Type: {currentGamepad.GetType().Name}");
                    GUILayout.Label($"Layout: {currentGamepad.layout}");
                }
                
                if (GUILayout.Button("Refresh Controller Detection"))
                {
                    DetectController();
                }
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRealtimeInput()
        {
            showRealTimeInput = EditorGUILayout.BeginFoldoutHeaderGroup(showRealTimeInput, "⚡ Real-time Input");
            if (showRealTimeInput && currentGamepad != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Analog inputs
                GUILayout.Label("📊 Analog Inputs:", EditorStyles.boldLabel);
                foreach (var kvp in realtimeValues)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(kvp.Key, GUILayout.Width(120));
                    
                    // Visual bar for analog values
                    Rect rect = GUILayoutUtility.GetRect(100, 16);
                    EditorGUI.ProgressBar(rect, Mathf.Abs(kvp.Value), kvp.Value.ToString("F2"));
                    
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.Space(5);
                
                // Button inputs
                GUILayout.Label("🔘 Button States:", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                int buttonCount = 0;
                foreach (var kvp in realtimeButtons)
                {
                    if (buttonCount % 3 == 0 && buttonCount > 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    
                    GUI.color = kvp.Value ? Color.green : Color.white;
                    GUILayout.Label(kvp.Key, kvp.Value ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.Width(90));
                    GUI.color = Color.white;
                    
                    buttonCount++;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawActionMappings()
        {
            showActionMappings = EditorGUILayout.BeginFoldoutHeaderGroup(showActionMappings, "🎯 Action Mappings");
            if (showActionMappings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                if (isListening)
                {
                    EditorGUILayout.HelpBox($"🎧 Listening for input for action: {currentListeningAction}\nPress any button or move any stick/trigger to map it.", MessageType.Warning);
                    
                    if (GUILayout.Button("Cancel Listening"))
                    {
                        StopListening();
                    }
                }
                
                if (playerActionMap != null)
                {
                    foreach (var action in playerActionMap.actions)
                    {
                        DrawActionMapping(action);
                    }
                }
                
                EditorGUILayout.Space(10);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("💾 Save Changes"))
                {
                    SavePlayerControls();
                }
                if (GUILayout.Button("🔄 Reload from File"))
                {
                    LoadPlayerControls();
                }
                if (GUILayout.Button("📋 Test All Mappings"))
                {
                    TestAllMappings();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawActionMapping(InputAction action)
        {
            if (!actionFoldouts.ContainsKey(action.name))
                actionFoldouts[action.name] = false;

            actionFoldouts[action.name] = EditorGUILayout.BeginFoldoutHeaderGroup(
                actionFoldouts[action.name], 
                $"🎮 {action.name} ({action.bindings.Count(b => !b.isComposite && !b.isPartOfComposite)} bindings)"
            );

            if (actionFoldouts[action.name])
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Action info
                GUILayout.Label($"Type: {action.type} | Expected Control: {action.expectedControlType}");
                
                // Current bindings
                var gamepadBindings = action.bindings.Where(b => 
                    IsGamepadBinding(b.path) && !b.isComposite && !b.isPartOfComposite).ToList();
                
                if (gamepadBindings.Any())
                {
                    GUILayout.Label("Current Gamepad Bindings:", EditorStyles.boldLabel);
                    foreach (var binding in gamepadBindings)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label($"• {CleanBindingPath(binding.path)}", GUILayout.Width(200));
                        
                        if (GUILayout.Button("🗑️", GUILayout.Width(30)))
                        {
                            RemoveBinding(action, binding);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No gamepad bindings found for this action", MessageType.Warning);
                }
                
                // Add new binding button
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"🎧 Listen for New Binding"))
                {
                    StartListening(action);
                }
                
                if (GUILayout.Button("➕ Add Quick Gamepad Binding"))
                {
                    ShowQuickBindingMenu(action);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private bool IsGamepadBinding(string path)
        {
            return path.Contains("Gamepad") || path.Contains("XInputController") || 
                   path.Contains("DualShockGamepad") || path.Contains("SwitchProControllerHID");
        }

        private string CleanBindingPath(string path)
        {
            // Clean up binding paths for display
            return path.Replace("<Gamepad>/", "")
                      .Replace("<XInputController>/", "Xbox: ")
                      .Replace("<DualShockGamepad>/", "PlayStation: ")
                      .Replace("<SwitchProControllerHID>/", "Switch: ")
                      .Replace("button", "")
                      .Replace("South", "A/Cross")
                      .Replace("East", "B/Circle")
                      .Replace("West", "X/Square")
                      .Replace("North", "Y/Triangle");
        }

        private void StartListening(InputAction action)
        {
            isListening = true;
            currentListeningAction = action.name;
            listeningAction = action;
            
            // Enable input system for listening
            UnityEngine.InputSystem.InputSystem.onEvent += OnInputEvent;
        }

        private void StopListening()
        {
            isListening = false;
            currentListeningAction = "";
            listeningAction = null;
            UnityEngine.InputSystem.InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!isListening || listeningAction == null || !(device is Gamepad))
                return;

            // Check for button presses or significant analog movement
            var gamepad = device as Gamepad;
            var path = "";

            // Check buttons
            if (gamepad.buttonSouth.wasPressedThisFrame) path = "<Gamepad>/buttonSouth";
            else if (gamepad.buttonEast.wasPressedThisFrame) path = "<Gamepad>/buttonEast";
            else if (gamepad.buttonWest.wasPressedThisFrame) path = "<Gamepad>/buttonWest";
            else if (gamepad.buttonNorth.wasPressedThisFrame) path = "<Gamepad>/buttonNorth";
            else if (gamepad.leftShoulder.wasPressedThisFrame) path = "<Gamepad>/leftShoulder";
            else if (gamepad.rightShoulder.wasPressedThisFrame) path = "<Gamepad>/rightShoulder";
            else if (gamepad.leftTrigger.ReadValue() > 0.5f) path = "<Gamepad>/leftTrigger";
            else if (gamepad.rightTrigger.ReadValue() > 0.5f) path = "<Gamepad>/rightTrigger";
            // Add more as needed...

            if (!string.IsNullOrEmpty(path))
            {
                AddBindingToAction(listeningAction, path);
                StopListening();
                Repaint();
            }
        }

        private void AddBindingToAction(InputAction action, string path)
        {
            // Add binding using InputActionSetupExtensions
            #if UNITY_EDITOR
            UnityEngine.InputSystem.InputActionSetupExtensions.AddBinding(action, path);
            EditorUtility.SetDirty(playerControls);
            #endif
        }

        private void RemoveBinding(InputAction action, InputBinding binding)
        {
            #if UNITY_EDITOR
            // Simple approach: disable the binding by setting empty path
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (action.bindings[i].path == binding.path)
                {
                    action.ChangeBinding(i).WithPath("");
                    break;
                }
            }
            EditorUtility.SetDirty(playerControls);
            #endif
        }

        private void ShowQuickBindingMenu(InputAction action)
        {
            var menu = new GenericMenu();
            
            // Common gamepad bindings
            menu.AddItem(new GUIContent("Buttons/A Button (South)"), false, () => AddBindingToAction(action, "<Gamepad>/buttonSouth"));
            menu.AddItem(new GUIContent("Buttons/B Button (East)"), false, () => AddBindingToAction(action, "<Gamepad>/buttonEast"));
            menu.AddItem(new GUIContent("Buttons/X Button (West)"), false, () => AddBindingToAction(action, "<Gamepad>/buttonWest"));
            menu.AddItem(new GUIContent("Buttons/Y Button (North)"), false, () => AddBindingToAction(action, "<Gamepad>/buttonNorth"));
            
            menu.AddItem(new GUIContent("Triggers & Bumpers/Left Bumper"), false, () => AddBindingToAction(action, "<Gamepad>/leftShoulder"));
            menu.AddItem(new GUIContent("Triggers & Bumpers/Right Bumper"), false, () => AddBindingToAction(action, "<Gamepad>/rightShoulder"));
            menu.AddItem(new GUIContent("Triggers & Bumpers/Left Trigger"), false, () => AddBindingToAction(action, "<Gamepad>/leftTrigger"));
            menu.AddItem(new GUIContent("Triggers & Bumpers/Right Trigger"), false, () => AddBindingToAction(action, "<Gamepad>/rightTrigger"));
            
            if (action.expectedControlType == "Vector2")
            {
                menu.AddItem(new GUIContent("Sticks/Left Stick"), false, () => AddBindingToAction(action, "<Gamepad>/leftStick"));
                menu.AddItem(new GUIContent("Sticks/Right Stick"), false, () => AddBindingToAction(action, "<Gamepad>/rightStick"));
                menu.AddItem(new GUIContent("D-Pad/D-Pad"), false, () => AddBindingToAction(action, "<Gamepad>/dpad"));
            }
            
            menu.ShowAsContext();
        }

        private void SavePlayerControls()
        {
            if (playerControls != null)
            {
                EditorUtility.SetDirty(playerControls);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log("🎮 Controller mappings saved successfully!");
            }
        }

        private void TestAllMappings()
        {
            if (playerActionMap != null)
            {
                Debug.Log("🧪 Testing all action mappings...");
                
                foreach (var action in playerActionMap.actions)
                {
                    var gamepadBindings = action.bindings.Count(b => IsGamepadBinding(b.path));
                    Debug.Log($"Action '{action.name}': {gamepadBindings} gamepad bindings");
                }
                
                Debug.Log("🎮 Mapping test complete - check console for details");
            }
        }
    }
}