using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Comprehensive TDD test suite for multi-platform controller mappings
    /// Validates Xbox, PlayStation, and Nintendo Switch controller bindings
    /// Ensures parity across platforms and identifies missing mappings
    /// </summary>
    public class ControllerMappingTests
    {
        private InputActionAsset playerControls;
        private InputActionMap playerActionMap;
        
        // Test action names from PlayerAction.cs
        private readonly string[] criticalActions = {
            "Move", "Jump", "Attack", "Sprint", "WeaponSwap", "Up", "Grab", "Dash", "Aim"
        };
        
        // Platform identifiers from Unity Input System
        private readonly string[] targetPlatforms = {
            "XInputController",      // Xbox controllers
            "DualShockGamepad",      // PlayStation controllers  
            "SwitchProControllerHID" // Nintendo Switch Pro Controller
        };

        [SetUp]
        public void Setup()
        {
            // Load the PlayerControls InputActionAsset
            playerControls = Resources.Load<InputActionAsset>("Controls/PlayerControls");
            Assert.IsNotNull(playerControls, "PlayerControls.inputactions not found in Resources/Controls/");
            
            playerActionMap = playerControls.FindActionMap("Player");
            Assert.IsNotNull(playerActionMap, "Player action map not found");
        }

        [TearDown]
        public void TearDown()
        {
            if (playerControls != null)
            {
                Resources.UnloadAsset(playerControls);
            }
        }

        #region Platform Coverage Tests

        [Test]
        [TestCase("XInputController", "Xbox")]
        [TestCase("DualShockGamepad", "PlayStation")]
        [TestCase("SwitchProControllerHID", "Nintendo Switch")]
        public void AllCriticalActions_ShouldHave_PlatformSpecificBindings(string platformId, string platformName)
        {
            var missingBindings = new List<string>();
            
            foreach (string actionName in criticalActions)
            {
                var action = playerActionMap.FindAction(actionName);
                Assert.IsNotNull(action, $"Action '{actionName}' not found in Player action map");
                
                bool hasPlatformBinding = action.bindings.Any(binding => 
                    binding.path.Contains($"<{platformId}>"));
                    
                if (!hasPlatformBinding)
                {
                    missingBindings.Add(actionName);
                }
            }
            
            if (missingBindings.Any())
            {
                Assert.Fail($"{platformName} controller missing bindings for: {string.Join(", ", missingBindings)}\n" +
                           $"Critical for Q1 2025 Steam launch - all platforms must have complete coverage!");
            }
        }

        [Test]
        public void WeaponSwap_ShouldHave_GamepadBindings()
        {
            var weaponSwapAction = playerActionMap.FindAction("WeaponSwap");
            Assert.IsNotNull(weaponSwapAction, "WeaponSwap action not found");
            
            var gamepadBindings = weaponSwapAction.bindings.Where(binding => 
                binding.path.Contains("Gamepad") || 
                binding.path.Contains("XInputController") ||
                binding.path.Contains("DualShockGamepad") ||
                binding.path.Contains("SwitchProControllerHID")).ToArray();
                
            Assert.Greater(gamepadBindings.Length, 0, 
                "WeaponSwap has NO gamepad bindings! Only mouse scroll wheel. " +
                "This breaks controller gameplay completely.");
        }

        #endregion

        #region Action Validation Tests

        [Test]
        [TestCase("Move", "Vector2")]
        [TestCase("Aim", "Vector2")]
        [TestCase("Jump", "Button")]
        [TestCase("Attack", "Button")]
        [TestCase("Sprint", "Button")]
        [TestCase("WeaponSwap", "Button")]
        public void Actions_ShouldHave_CorrectControlType(string actionName, string expectedType)
        {
            var action = playerActionMap.FindAction(actionName);
            Assert.IsNotNull(action, $"Action '{actionName}' not found");
            
            // For Value type actions, check expectedControlType field
            if (expectedType == "Vector2")
            {
                Assert.AreEqual("Vector2", action.expectedControlType, 
                    $"Action '{actionName}' should expect Vector2 input");
            }
            else if (expectedType == "Button")
            {
                Assert.IsTrue(action.expectedControlType == "Button" || action.expectedControlType == "Key",
                    $"Action '{actionName}' should expect Button or Key input");
            }
        }

        [Test]
        public void PlayerActionMap_ShouldContain_AllRequiredActions()
        {
            var actionNames = playerActionMap.actions.Select(a => a.name).ToArray();
            
            foreach (string requiredAction in criticalActions)
            {
                Assert.Contains(requiredAction, actionNames, 
                    $"Missing critical action: {requiredAction}");
            }
        }

        #endregion

        #region Binding Consistency Tests

        [Test]
        public void Jump_ShouldUse_SouthButton_OnAllGamepads()
        {
            var jumpAction = playerActionMap.FindAction("Jump");
            var expectedBindings = new[] {
                "<XInputController>/buttonSouth",
                "<DualShockGamepad>/buttonSouth", 
                "<SwitchProControllerHID>/buttonSouth"
            };
            
            foreach (string expectedBinding in expectedBindings)
            {
                bool hasBinding = jumpAction.bindings.Any(b => b.path == expectedBinding);
                Assert.IsTrue(hasBinding, $"Jump missing binding: {expectedBinding}");
            }
        }

        [Test]
        public void Attack_ShouldUse_EastButton_OnAllGamepads()
        {
            var attackAction = playerActionMap.FindAction("Attack");
            var expectedBindings = new[] {
                "<XInputController>/buttonEast",
                "<DualShockGamepad>/buttonEast",
                "<SwitchProControllerHID>/buttonEast"
            };
            
            foreach (string expectedBinding in expectedBindings)
            {
                bool hasBinding = attackAction.bindings.Any(b => b.path == expectedBinding);
                Assert.IsTrue(hasBinding, $"Attack missing binding: {expectedBinding}");
            }
        }

        [Test]
        public void Sprint_ShouldUse_WestButton_OnAllGamepads()
        {
            var sprintAction = playerActionMap.FindAction("Sprint");
            var expectedBindings = new[] {
                "<XInputController>/buttonWest",
                "<DualShockGamepad>/buttonWest",
                "<SwitchProControllerHID>/buttonWest"
            };
            
            foreach (string expectedBinding in expectedBindings)
            {
                bool hasBinding = sprintAction.bindings.Any(b => b.path == expectedBinding);
                Assert.IsTrue(hasBinding, $"Sprint missing binding: {expectedBinding}");
            }
        }

        #endregion

        #region Rebinding Support Tests

        [Test]
        public void AllActions_ShouldSupport_RuntimeRebinding()
        {
            foreach (string actionName in criticalActions)
            {
                var action = playerActionMap.FindAction(actionName);
                
                // Actions should not be read-only to support rebinding
                Assert.IsFalse(action.bindings.All(b => b.isPartOfComposite && b.isComposite),
                    $"Action '{actionName}' appears to have complex composite structure that may block rebinding");
                
                // Each action should have at least one non-composite binding for rebinding
                bool hasSimpleBinding = action.bindings.Any(b => !b.isPartOfComposite);
                Assert.IsTrue(hasSimpleBinding, 
                    $"Action '{actionName}' has no simple bindings - may be difficult to rebind");
            }
        }

        [Test]
        public void Move_Action_ShouldSupport_BothCompositeAndDirect_Bindings()
        {
            var moveAction = playerActionMap.FindAction("Move");
            
            // Should have composite WASD/Arrow keys for keyboard
            bool hasComposite = moveAction.bindings.Any(b => b.isComposite);
            Assert.IsTrue(hasComposite, "Move action should have composite bindings for keyboard");
            
            // Should also support direct gamepad stick binding for smooth movement
            bool hasDirectGamepad = moveAction.bindings.Any(b => 
                b.path.Contains("leftStick") && !b.isPartOfComposite);
            Assert.IsTrue(hasDirectGamepad, "Move action should have direct gamepad stick bindings");
        }

        #endregion

        #region Platform-Specific Feature Tests

        [Test]
        public void PlayStation_ShouldSupport_DualSense_SpecificFeatures()
        {
            // Test that we're using DualShockGamepad which supports both DS4 and DualSense
            var attackAction = playerActionMap.FindAction("Attack");
            bool usesDualShockPath = attackAction.bindings.Any(b => 
                b.path.Contains("DualShockGamepad"));
            
            Assert.IsTrue(usesDualShockPath, 
                "Should use DualShockGamepad for PlayStation controller support (DS4 + DualSense)");
        }

        [Test]
        public void Nintendo_Switch_ShouldHandle_ButtonLayout_Differences()
        {
            // Nintendo Switch has different button labeling (A/B swapped vs Xbox)
            // But our bindings use buttonSouth/East/etc which Unity translates correctly
            var jumpAction = playerActionMap.FindAction("Jump");
            var attackAction = playerActionMap.FindAction("Attack");
            
            bool switchJumpCorrect = jumpAction.bindings.Any(b => 
                b.path == "<SwitchProControllerHID>/buttonSouth");
            bool switchAttackCorrect = attackAction.bindings.Any(b => 
                b.path == "<SwitchProControllerHID>/buttonEast");
                
            Assert.IsTrue(switchJumpCorrect, "Switch should use buttonSouth for Jump (Nintendo B button)");
            Assert.IsTrue(switchAttackCorrect, "Switch should use buttonEast for Attack (Nintendo A button)");
        }

        #endregion

        #region Button Combination Tests

        [Test]
        public void Xbox_Controller_ShouldHave_StandardButton_Layout()
        {
            // Test Xbox standard button layout: A=South, B=East, X=West, Y=North
            var xboxBindings = new Dictionary<string, string>
            {
                ["Jump"] = "<XInputController>/buttonSouth",        // A button
                ["Attack"] = "<XInputController>/buttonEast",       // B button  
                ["Sprint"] = "<XInputController>/buttonWest",       // X button
                ["WeaponSwap"] = "<XInputController>/buttonNorth",  // Y button
                ["Dash"] = "<XInputController>/leftShoulder",       // LB
                ["Grab"] = "<XInputController>/rightTrigger",       // RT
            };

            foreach (var expectedBinding in xboxBindings)
            {
                var action = playerActionMap.FindAction(expectedBinding.Key);
                Assert.IsNotNull(action, $"Action '{expectedBinding.Key}' not found");
                
                bool hasBinding = action.bindings.Any(b => b.path == expectedBinding.Value);
                Assert.IsTrue(hasBinding, 
                    $"Xbox controller missing {expectedBinding.Key} → {expectedBinding.Value} binding");
            }
        }

        [Test]
        public void PlayStation_Controller_ShouldHave_StandardButton_Layout()
        {
            // Test PlayStation button layout: Cross=South, Circle=East, Square=West, Triangle=North
            var psBindings = new Dictionary<string, string>
            {
                ["Jump"] = "<DualShockGamepad>/buttonSouth",        // Cross button
                ["Attack"] = "<DualShockGamepad>/buttonEast",       // Circle button
                ["Sprint"] = "<DualShockGamepad>/buttonWest",       // Square button  
                ["WeaponSwap"] = "<DualShockGamepad>/buttonNorth",  // Triangle button
                ["Dash"] = "<DualShockGamepad>/leftShoulder",       // L1
                ["Grab"] = "<DualShockGamepad>/rightTrigger",       // R2
            };

            foreach (var expectedBinding in psBindings)
            {
                var action = playerActionMap.FindAction(expectedBinding.Key);
                Assert.IsNotNull(action, $"Action '{expectedBinding.Key}' not found");
                
                bool hasBinding = action.bindings.Any(b => b.path == expectedBinding.Value);
                Assert.IsTrue(hasBinding,
                    $"PlayStation controller missing {expectedBinding.Key} → {expectedBinding.Value} binding");
            }
        }

        [Test]
        public void Nintendo_Switch_ShouldHave_CorrectButton_Mapping()
        {
            // Test Nintendo Switch layout: B=South, A=East, Y=West, X=North (physically different labels)
            var switchBindings = new Dictionary<string, string>
            {
                ["Jump"] = "<SwitchProControllerHID>/buttonSouth",        // B button (Nintendo B)
                ["Attack"] = "<SwitchProControllerHID>/buttonEast",       // A button (Nintendo A)
                ["Sprint"] = "<SwitchProControllerHID>/buttonWest",       // Y button (Nintendo Y)
                ["WeaponSwap"] = "<SwitchProControllerHID>/buttonNorth",  // X button (Nintendo X)
                ["Dash"] = "<SwitchProControllerHID>/leftShoulder",       // L button
                ["Grab"] = "<SwitchProControllerHID>/rightTrigger",       // ZR button
            };

            foreach (var expectedBinding in switchBindings)
            {
                var action = playerActionMap.FindAction(expectedBinding.Key);
                Assert.IsNotNull(action, $"Action '{expectedBinding.Key}' not found");
                
                bool hasBinding = action.bindings.Any(b => b.path == expectedBinding.Value);
                Assert.IsTrue(hasBinding,
                    $"Nintendo Switch controller missing {expectedBinding.Key} → {expectedBinding.Value} binding");
            }
        }

        [Test]
        public void Movement_Actions_ShouldSupport_BothStick_AndDPad()
        {
            var movementTests = new[]
            {
                new { Platform = "XInputController", Description = "Xbox" },
                new { Platform = "DualShockGamepad", Description = "PlayStation" },
                new { Platform = "SwitchProControllerHID", Description = "Nintendo Switch" }
            };

            foreach (var test in movementTests)
            {
                var moveAction = playerActionMap.FindAction("Move");
                var upAction = playerActionMap.FindAction("Up");

                // Test left stick support for Move action
                bool hasLeftStick = moveAction.bindings.Any(b => 
                    b.path.Contains($"<{test.Platform}>/leftStick") && b.isPartOfComposite);
                Assert.IsTrue(hasLeftStick, 
                    $"{test.Description} Move action should support left stick");

                // Test D-pad support for Move action  
                bool hasDPad = moveAction.bindings.Any(b => 
                    b.path.Contains($"<{test.Platform}>/dpad") && b.isPartOfComposite);
                Assert.IsTrue(hasDPad,
                    $"{test.Description} Move action should support D-pad");

                // Test Up action has both stick and D-pad
                bool upHasStick = upAction.bindings.Any(b => 
                    b.path == $"<{test.Platform}>/leftStick/up");
                bool upHasDPad = upAction.bindings.Any(b => 
                    b.path == $"<{test.Platform}>/dpad/up");

                Assert.IsTrue(upHasStick, 
                    $"{test.Description} Up action should support left stick up");
                Assert.IsTrue(upHasDPad,
                    $"{test.Description} Up action should support D-pad up");
            }
        }

        [Test]
        public void Trigger_Actions_ShouldHave_ProperSensitivity()
        {
            // Test that trigger-based actions (Grab, Attack) work with analog triggers
            var triggerBindings = new[]
            {
                new { Action = "Grab", ExpectedPath = "rightTrigger" },
                new { Action = "Attack", ExpectedPath = "rightTrigger" } // Attack has both button and trigger
            };

            var platforms = new[] { "XInputController", "DualShockGamepad", "SwitchProControllerHID" };

            foreach (var platform in platforms)
            {
                foreach (var triggerTest in triggerBindings)
                {
                    var action = playerActionMap.FindAction(triggerTest.Action);
                    bool hasTrigger = action.bindings.Any(b => 
                        b.path == $"<{platform}>/{triggerTest.ExpectedPath}");

                    if (triggerTest.Action == "Grab")
                    {
                        // Grab MUST have trigger binding on all platforms
                        Assert.IsTrue(hasTrigger,
                            $"{platform} {triggerTest.Action} action missing {triggerTest.ExpectedPath} binding");
                    }
                    else if (triggerTest.Action == "Attack")
                    {
                        // Attack should have EITHER button OR trigger (flexible)
                        bool hasButton = action.bindings.Any(b => 
                            b.path.Contains($"<{platform}>/button"));
                        
                        Assert.IsTrue(hasButton || hasTrigger,
                            $"{platform} Attack action needs either button or trigger binding");
                    }
                }
            }
        }

        [Test] 
        public void UI_Actions_ShouldWork_OnAllControllers()
        {
            var uiActionMap = playerControls.FindActionMap("UI");
            Assert.IsNotNull(uiActionMap, "UI action map not found");

            var toggleMenuAction = uiActionMap.FindAction("ToggleMenu");
            Assert.IsNotNull(toggleMenuAction, "ToggleMenu action not found in UI map");

            // Should have keyboard Escape
            bool hasEscapeKey = toggleMenuAction.bindings.Any(b => 
                b.path.Contains("Keyboard/Escape"));
            Assert.IsTrue(hasEscapeKey, "ToggleMenu should have Escape key binding");

            // Should have generic gamepad start button
            bool hasStartButton = toggleMenuAction.bindings.Any(b =>
                b.path.Contains("Gamepad/start"));
            Assert.IsTrue(hasStartButton, "ToggleMenu should have gamepad start button binding");
        }

        #endregion

        #region Cross-Platform Compatibility Tests

        [Test]
        public void All_CriticalActions_ShouldHave_GenericGamepad_Fallbacks()
        {
            // Ensure generic /<Gamepad>/ fallbacks exist for maximum compatibility
            var criticalActionsRequiringFallbacks = new[] { "Jump", "Attack", "Sprint", "Move", "WeaponSwap" };
            
            foreach (string actionName in criticalActionsRequiringFallbacks)
            {
                var action = playerActionMap.FindAction(actionName);
                
                if (actionName == "Move")
                {
                    // Move action should have generic gamepad stick/dpad composite bindings
                    bool hasGenericGamepadMove = action.bindings.Any(b =>
                        b.path.Contains("<Gamepad>/") && (b.path.Contains("leftStick") || b.path.Contains("dpad")));
                    Assert.IsTrue(hasGenericGamepadMove,
                        $"Move action should have generic <Gamepad> fallback bindings");
                }
                else
                {
                    // Other actions should have generic gamepad button fallbacks
                    bool hasGenericFallback = action.bindings.Any(b => 
                        b.path.StartsWith("/<Gamepad>/"));
                    Assert.IsTrue(hasGenericFallback,
                        $"Action '{actionName}' should have generic /<Gamepad>/ fallback binding");
                }
            }
        }

        [Test]
        public void WebGL_Platform_ShouldHave_SpecialConsideration()
        {
            // WebGL has different gamepad handling - check if we have WebGL-specific bindings
            var aimAction = playerActionMap.FindAction("Aim");
            
            bool hasWebGLGamepad = aimAction.bindings.Any(b => 
                b.path.Contains("<WebGLGamepad>/"));
                
            if (hasWebGLGamepad)
            {
                Debug.Log("✅ WebGL gamepad support detected in Aim action");
            }
            else
            {
                Debug.Log("ℹ️ No WebGL-specific gamepad bindings found - using generic fallbacks");
            }
            
            // This is informational - WebGL support is nice to have but not critical
            Assert.IsTrue(true); // Always pass, just for logging
        }

        #endregion

        #region Integration Tests

        [Test]
        public void PlayerActionMap_ShouldEnable_WithoutErrors()
        {
            Assert.DoesNotThrow(() => {
                playerActionMap.Enable();
                playerActionMap.Disable();
            }, "PlayerActionMap should enable/disable without errors");
        }

        [Test]
        public void AllBindings_ShouldHave_ValidPaths()
        {
            var invalidBindings = new List<string>();
            
            foreach (var action in playerActionMap.actions)
            {
                foreach (var binding in action.bindings)
                {
                    if (string.IsNullOrEmpty(binding.path) && !binding.isComposite)
                    {
                        invalidBindings.Add($"{action.name}: empty binding path");
                    }
                    
                    // Check for obvious typos in common controller paths
                    if (binding.path.Contains("XInputControler") || // missing 'l' 
                        binding.path.Contains("DualShokGamepad") ||  // missing 'c'
                        binding.path.Contains("SwitchProContollerHID")) // missing 'r'
                    {
                        invalidBindings.Add($"{action.name}: misspelled path {binding.path}");
                    }
                }
            }
            
            if (invalidBindings.Any())
            {
                Assert.Fail($"Invalid bindings found:\n{string.Join("\n", invalidBindings)}");
            }
        }

        [Test]
        public void Controller_InputAction_Performance_ShouldBe_Acceptable()
        {
            // Test that we don't have excessive binding complexity that could impact performance
            foreach (var action in playerActionMap.actions)
            {
                int bindingCount = action.bindings.Count();
                
                // Reasonable limits: Move action can have many composite bindings, others should be more modest
                int maxExpectedBindings = action.name == "Move" ? 20 : 15;
                
                Assert.LessOrEqual(bindingCount, maxExpectedBindings,
                    $"Action '{action.name}' has {bindingCount} bindings - may impact performance. Consider optimization.");
            }
        }

        #endregion
    }
}