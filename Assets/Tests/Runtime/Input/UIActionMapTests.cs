using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using System.Collections;

namespace NeonLadder.Tests.Runtime.Input
{
    /// <summary>
    /// Tests for UI action map Move action integration and PlayerControls asset validation
    /// Ensures loading screen navigation works with controller and keyboard input
    /// </summary>
    public class UIActionMapTests
    {
        private InputActionAsset playerControls;
        private InputActionMap uiActionMap;
        private InputAction uiMoveAction;

        [SetUp]
        public void SetUp()
        {
            // Load the actual PlayerControls asset from Resources
            playerControls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
            Assert.IsNotNull(playerControls, "PlayerControls asset should exist in Resources/Controls/");

            // Get the UI action map
            uiActionMap = playerControls.FindActionMap("UI");
            Assert.IsNotNull(uiActionMap, "UI action map should exist in PlayerControls");

            // Get the Move action
            uiMoveAction = uiActionMap.FindAction("Move");
        }

        [TearDown]
        public void TearDown()
        {
            if (uiActionMap != null && uiActionMap.enabled)
            {
                uiActionMap.Disable();
            }
        }

        [Test]
        public void UIActionMap_ExistsInPlayerControls()
        {
            Assert.IsNotNull(uiActionMap, "UI action map should exist in PlayerControls asset");
            Assert.AreEqual("UI", uiActionMap.name, "Action map should be named 'UI'");
        }

        [Test]
        public void UIMoveAction_ExistsAndIsConfiguredCorrectly()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist in UI action map");
            Assert.AreEqual("Move", uiMoveAction.name, "Action should be named 'Move'");
            Assert.AreEqual(InputActionType.Value, uiMoveAction.type, "Move should be a Value action");
            Assert.AreEqual("Vector2", uiMoveAction.expectedControlType, "Move should expect Vector2 input");
        }

        [Test]
        public void UIMoveAction_HasKeyboardBindings()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;
            bool hasWASD = false;
            bool hasArrows = false;

            foreach (var binding in bindings)
            {
                var path = binding.path;
                if (path.Contains("Keyboard>/w") || path.Contains("Keyboard>/a") ||
                    path.Contains("Keyboard>/s") || path.Contains("Keyboard>/d"))
                {
                    hasWASD = true;
                }
                if (path.Contains("upArrow") || path.Contains("downArrow") ||
                    path.Contains("leftArrow") || path.Contains("rightArrow"))
                {
                    hasArrows = true;
                }
            }

            Assert.IsTrue(hasWASD, "UI Move action should have WASD key bindings");
            Assert.IsTrue(hasArrows, "UI Move action should have arrow key bindings");
        }

        [Test]
        public void UIMoveAction_HasGamepadBindings()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;
            bool hasLeftStick = false;
            bool hasDPad = false;

            foreach (var binding in bindings)
            {
                var path = binding.path;
                if (path.Contains("leftStick"))
                {
                    hasLeftStick = true;
                }
                if (path.Contains("dpad"))
                {
                    hasDPad = true;
                }
            }

            Assert.IsTrue(hasLeftStick, "UI Move action should have gamepad left stick binding");
            Assert.IsTrue(hasDPad, "UI Move action should have gamepad D-pad bindings");
        }

        [Test]
        public void UIMoveAction_SupportsAllDirections()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;
            bool hasUp = false;
            bool hasDown = false;
            bool hasLeft = false;
            bool hasRight = false;

            foreach (var binding in bindings)
            {
                if (binding.isPartOfComposite)
                {
                    switch (binding.name)
                    {
                        case "up": hasUp = true; break;
                        case "down": hasDown = true; break;
                        case "left": hasLeft = true; break;
                        case "right": hasRight = true; break;
                    }
                }
            }

            Assert.IsTrue(hasUp, "UI Move action should support up direction");
            Assert.IsTrue(hasDown, "UI Move action should support down direction");
            Assert.IsTrue(hasLeft, "UI Move action should support left direction");
            Assert.IsTrue(hasRight, "UI Move action should support right direction");
        }

        [Test]
        public void UIToggleMenuAction_StillExistsAfterMoveAddition()
        {
            var toggleMenuAction = uiActionMap.FindAction("ToggleMenu");
            Assert.IsNotNull(toggleMenuAction, "ToggleMenu action should still exist in UI action map");
            Assert.AreEqual("ToggleMenu", toggleMenuAction.name, "Action should be named 'ToggleMenu'");
            Assert.AreEqual(InputActionType.Button, toggleMenuAction.type, "ToggleMenu should be a Button action");
        }

        [UnityTest]
        public IEnumerator UIMoveAction_CanBeEnabledAndDisabled()
        {
            uiActionMap.Enable();
            yield return null;

            Assert.IsTrue(uiActionMap.enabled, "UI action map should be enabled");
            Assert.IsTrue(uiMoveAction.enabled, "UI Move action should be enabled");

            uiActionMap.Disable();
            yield return null;

            Assert.IsFalse(uiActionMap.enabled, "UI action map should be disabled");
            Assert.IsFalse(uiMoveAction.enabled, "UI Move action should be disabled");
        }

        [Test]
        public void UIMoveAction_HasUniqueBindingIDs()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;
            var bindingIds = new System.Collections.Generic.HashSet<string>();

            foreach (var binding in bindings)
            {
                var bindingIdString = binding.id.ToString();
                if (!string.IsNullOrEmpty(bindingIdString))
                {
                    Assert.IsTrue(bindingIds.Add(bindingIdString),
                        $"Binding ID '{bindingIdString}' should be unique");
                }
            }

            Assert.Greater(bindingIds.Count, 0, "UI Move action should have bindings with valid IDs");
        }

        [Test]
        public void UIMoveAction_BindingIDsAreValidGUIDs()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;

            foreach (var binding in bindings)
            {
                var bindingIdString = binding.id.ToString();
                if (!string.IsNullOrEmpty(bindingIdString))
                {
                    Assert.DoesNotThrow(() => System.Guid.Parse(bindingIdString),
                        $"Binding ID '{bindingIdString}' should be a valid GUID format");
                }
            }
        }

        [Test]
        public void PlayerControlsAsset_LoadsWithoutErrors()
        {
            Assert.IsNotNull(playerControls, "PlayerControls asset should load without errors");
            Assert.Greater(playerControls.actionMaps.Count, 0, "PlayerControls should have action maps");

            // Verify both Player and UI action maps exist
            var playerMap = playerControls.FindActionMap("Player");
            Assert.IsNotNull(playerMap, "Player action map should exist");
            Assert.IsNotNull(uiActionMap, "UI action map should exist");
        }

        [Test]
        public void UIMoveAction_DoesNotConflictWithPlayerMoveAction()
        {
            var playerActionMap = playerControls.FindActionMap("Player");
            Assert.IsNotNull(playerActionMap, "Player action map should exist");

            var playerMoveAction = playerActionMap.FindAction("Move");
            Assert.IsNotNull(playerMoveAction, "Player Move action should exist");

            // Actions should have different IDs
            Assert.AreNotEqual(playerMoveAction.id.ToString(), uiMoveAction.id.ToString(),
                "Player Move and UI Move actions should have different IDs");

            // Both actions should be able to exist simultaneously
            Assert.IsNotNull(playerMoveAction, "Player Move action should exist");
            Assert.IsNotNull(uiMoveAction, "UI Move action should exist");
        }

        [UnityTest]
        public IEnumerator UIAndPlayerActionMaps_CanBeEnabledSimultaneously()
        {
            var playerActionMap = playerControls.FindActionMap("Player");
            Assert.IsNotNull(playerActionMap, "Player action map should exist");

            // Enable both action maps
            playerActionMap.Enable();
            uiActionMap.Enable();
            yield return null;

            Assert.IsTrue(playerActionMap.enabled, "Player action map should be enabled");
            Assert.IsTrue(uiActionMap.enabled, "UI action map should be enabled");

            // Clean up
            playerActionMap.Disable();
            uiActionMap.Disable();
        }

        [Test]
        public void UIMoveAction_HasExpectedNumberOfBindings()
        {
            Assert.IsNotNull(uiMoveAction, "Move action should exist");

            var bindings = uiMoveAction.bindings;

            // Count different types of bindings
            int compositeBindings = 0;
            int directBindings = 0;
            int componentBindings = 0;

            foreach (var binding in bindings)
            {
                if (binding.isComposite)
                    compositeBindings++;
                else if (binding.isPartOfComposite)
                    componentBindings++;
                else
                    directBindings++;
            }

            // Should have at least:
            // - 2 composite bindings (keyboard, d-pad)
            // - 1 direct binding (gamepad stick)
            // - 8+ component bindings (up/down/left/right for keyboard and d-pad)
            Assert.GreaterOrEqual(compositeBindings, 2, "Should have at least 2 composite bindings");
            Assert.GreaterOrEqual(directBindings, 1, "Should have at least 1 direct binding");
            Assert.GreaterOrEqual(componentBindings, 8, "Should have at least 8 component bindings");
        }
    }
}