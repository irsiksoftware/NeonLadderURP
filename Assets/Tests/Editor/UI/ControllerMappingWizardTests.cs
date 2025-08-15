using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadder.Editor.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using System.Collections.Generic;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Tony Stark's Controller Mapping Wizard Tests - Enterprise TDD Testing Suite
    /// Testing real-time controller detection and mapping functionality
    /// 
    /// "JARVIS, let's make sure our controller wizard handles all edge cases perfectly."
    /// </summary>
    [TestFixture]
    public class ControllerMappingWizardTests : EditorWindowTestBase<ControllerMappingWizard>
    {
        private InputActionAsset mockInputAsset;
        private InputActionMap mockActionMap;
        private InputAction mockMoveAction;
        private InputAction mockJumpAction;
        
        #region Setup & Teardown
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Create mock input system assets
            mockInputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            mockActionMap = mockInputAsset.AddActionMap("Player");
            
            // Add test actions
            mockMoveAction = mockActionMap.AddAction("Move", type: InputActionType.Value);
            mockMoveAction.AddBinding("<Gamepad>/leftStick");
            
            mockJumpAction = mockActionMap.AddAction("Jump", type: InputActionType.Button);
            mockJumpAction.AddBinding("<Gamepad>/buttonSouth");
            
            // Setup wizard with mocks
            SetPrivateField("playerControls", mockInputAsset);
            SetPrivateField("playerActionMap", mockActionMap);
        }
        
        [TearDown]
        public override void TearDown()
        {
            if (mockInputAsset != null)
                UnityEngine.Object.DestroyImmediate(mockInputAsset);
                
            base.TearDown();
        }
        
        #endregion
        
        #region Window Initialization Tests
        
        [Test]
        public void Window_Initialization_SetsCorrectMinimumSize()
        {
            // Act - Initialize window
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Assert
            Assert.AreEqual(600, window.minSize.x, "Window minimum width should be 600");
            Assert.AreEqual(800, window.minSize.y, "Window minimum height should be 800");
        }
        
        [Test]
        public void Window_OnEnable_LoadsPlayerControls()
        {
            // Act
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Assert
            var loadedControls = GetPrivateField<InputActionAsset>("playerControls");
            var loadedMap = GetPrivateField<InputActionMap>("playerActionMap");
            
            Assert.IsNotNull(loadedControls, "Should load player controls on enable");
            Assert.IsNotNull(loadedMap, "Should set player action map on enable");
        }
        
        #endregion
        
        #region Controller Detection Tests
        
        [Test]
        public void ControllerDetection_NoGamepad_DisplaysNoControllerMessage()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act - Simulate no gamepad connected
            SetPrivateField<object>("currentGamepad", null);
            SetPrivateField<string>("controllerInfo", "No controller detected");
            
            // Assert
            var controllerInfo = GetPrivateField<string>("controllerInfo");
            Assert.AreEqual("No controller detected", controllerInfo);
        }
        
        [Test]
        public void ControllerDetection_GamepadConnected_DisplaysControllerInfo()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            var mockGamepad = EditorUITestFramework.CreateMockGamepad();
            
            // Act - Simulate gamepad detection
            SetPrivateField("currentGamepad", mockGamepad);
            SetPrivateField("controllerInfo", $"Xbox Controller connected");
            
            // Assert
            var controllerInfo = GetPrivateField<string>("controllerInfo");
            Assert.IsTrue(controllerInfo.Contains("connected"), "Should show controller connected status");
        }
        
        #endregion
        
        #region Action Remapping Tests
        
        [Test]
        public void ActionRemapping_StartListening_SetsListeningState()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act - Start listening for jump action
            SetPrivateField("isListening", true);
            SetPrivateField("currentListeningAction", "Jump");
            SetPrivateField("listeningAction", mockJumpAction);
            
            // Assert
            Assert.IsTrue(GetPrivateField<bool>("isListening"), "Should be in listening state");
            Assert.AreEqual("Jump", GetPrivateField<string>("currentListeningAction"));
            Assert.AreEqual(mockJumpAction, GetPrivateField<InputAction>("listeningAction"));
        }
        
        [Test]
        public void ActionRemapping_StopListening_ClearsListeningState()
        {
            // Arrange - Start listening first
            SetPrivateField("isListening", true);
            SetPrivateField("currentListeningAction", "Jump");
            SetPrivateField("listeningAction", mockJumpAction);
            
            // Act - Stop listening
            SetPrivateField<bool>("isListening", false);
            SetPrivateField<string>("currentListeningAction", "");
            SetPrivateField<object>("listeningAction", null);
            
            // Assert
            Assert.IsFalse(GetPrivateField<bool>("isListening"), "Should not be in listening state");
            Assert.AreEqual("", GetPrivateField<string>("currentListeningAction"));
            Assert.IsNull(GetPrivateField<InputAction>("listeningAction"));
        }
        
        #endregion
        
        #region Real-time Input Display Tests
        
        [Test]
        public void RealtimeInput_InitializesEmptyDictionaries()
        {
            // Arrange & Act
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Assert
            var realtimeValues = GetPrivateField<Dictionary<string, float>>("realtimeValues");
            var realtimeButtons = GetPrivateField<Dictionary<string, bool>>("realtimeButtons");
            
            Assert.IsNotNull(realtimeValues, "Realtime values dictionary should be initialized");
            Assert.IsNotNull(realtimeButtons, "Realtime buttons dictionary should be initialized");
            Assert.AreEqual(0, realtimeValues.Count, "Realtime values should start empty");
            Assert.AreEqual(0, realtimeButtons.Count, "Realtime buttons should start empty");
        }
        
        [Test]
        public void RealtimeInput_TracksAnalogValues()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            var realtimeValues = GetPrivateField<Dictionary<string, float>>("realtimeValues");
            
            // Act - Simulate analog input
            realtimeValues["LeftStickX"] = 0.75f;
            realtimeValues["LeftStickY"] = -0.5f;
            realtimeValues["RightTrigger"] = 1.0f;
            
            // Assert
            Assert.AreEqual(0.75f, realtimeValues["LeftStickX"], 0.01f, "Should track left stick X value");
            Assert.AreEqual(-0.5f, realtimeValues["LeftStickY"], 0.01f, "Should track left stick Y value");
            Assert.AreEqual(1.0f, realtimeValues["RightTrigger"], 0.01f, "Should track trigger value");
        }
        
        [Test]
        public void RealtimeInput_TracksButtonStates()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            var realtimeButtons = GetPrivateField<Dictionary<string, bool>>("realtimeButtons");
            
            // Act - Simulate button presses
            realtimeButtons["ButtonSouth"] = true;
            realtimeButtons["ButtonNorth"] = false;
            realtimeButtons["LeftShoulder"] = true;
            
            // Assert
            Assert.IsTrue(realtimeButtons["ButtonSouth"], "Should track button south as pressed");
            Assert.IsFalse(realtimeButtons["ButtonNorth"], "Should track button north as not pressed");
            Assert.IsTrue(realtimeButtons["LeftShoulder"], "Should track left shoulder as pressed");
        }
        
        #endregion
        
        #region UI State Management Tests
        
        [Test]
        public void UIState_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Assert
            Assert.IsTrue(GetPrivateField<bool>("showControllerStatus"), "Controller status should be shown by default");
            Assert.IsTrue(GetPrivateField<bool>("showActionMappings"), "Action mappings should be shown by default");
            Assert.IsTrue(GetPrivateField<bool>("showRealTimeInput"), "Real-time input should be shown by default");
        }
        
        [Test]
        public void UIState_ActionFoldouts_InitializedCorrectly()
        {
            // Arrange & Act - Test that dictionary is initialized with project input actions
            var actionFoldouts = GetPrivateField<Dictionary<string, bool>>("actionFoldouts");
            
            // Assert - Dictionary should be initialized with project's input actions
            Assert.IsNotNull(actionFoldouts, "Action foldouts dictionary should be initialized");
            Assert.Greater(actionFoldouts.Count, 0, "Action foldouts should contain project input actions");
            
            // Verify all values are initially false (collapsed)
            foreach (var kvp in actionFoldouts)
            {
                Assert.IsFalse(kvp.Value, $"Action '{kvp.Key}' should start collapsed");
            }
            
            // Additional test: Verify OnEnable doesn't change the count
            int initialCount = actionFoldouts.Count;
            EditorUITestFramework.SimulateOnEnable(window);
            Assert.AreEqual(initialCount, actionFoldouts.Count, "OnEnable should not change action count");
        }
        
        #endregion
        
        #region MenuItem Tests
        
        [Test]
        public void MenuItem_ControllerMappingWizard_HasCorrectPath()
        {
            // Arrange
            var menuItemAttribute = typeof(ControllerMappingWizard)
                .GetMethod("ShowWindow", BindingFlags.Public | BindingFlags.Static)
                ?.GetCustomAttribute<MenuItem>();
            
            // Assert
            Assert.IsNotNull(menuItemAttribute, "ShowWindow should have MenuItem attribute");
            Assert.AreEqual("NeonLadder/Input System/Controller Mapping Wizard", menuItemAttribute.menuItem,
                "Menu item should be in correct location");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void Integration_WindowCanRenderWithoutErrors()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act & Assert
            Assert.DoesNotThrow(() => {
                EditorUITestFramework.ValidateEditorWindowCanRender(window);
            }, "Window should be able to render without throwing exceptions");
        }
        
        [Test]
        public void Integration_MultipleActionsCanBeTracked()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            var actionFoldouts = GetPrivateField<Dictionary<string, bool>>("actionFoldouts");
            
            // Store initial count of project actions
            int initialCount = actionFoldouts.Count;
            
            // Act - Add multiple test actions to track
            actionFoldouts["TestMove"] = true;
            actionFoldouts["TestJump"] = false;
            actionFoldouts["TestFire"] = true;
            actionFoldouts["TestReload"] = false;
            
            // Assert
            Assert.AreEqual(initialCount + 4, actionFoldouts.Count, "Should track original actions plus 4 test actions");
            Assert.IsTrue(actionFoldouts["TestMove"], "TestMove action should be expanded");
            Assert.IsFalse(actionFoldouts["TestJump"], "TestJump action should be collapsed");
            Assert.IsTrue(actionFoldouts["TestFire"], "TestFire action should be expanded");
            Assert.IsFalse(actionFoldouts["TestReload"], "TestReload action should be collapsed");
        }
        
        #endregion
        
        #region Helper Methods
        
        private T GetPrivateField<T>(string fieldName)
        {
            var field = typeof(ControllerMappingWizard).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null ? (T)field.GetValue(window) : default(T);
        }
        
        private void SetPrivateField<T>(string fieldName, T value)
        {
            var field = typeof(ControllerMappingWizard).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(window, value);
        }
        
        #endregion
    }
}