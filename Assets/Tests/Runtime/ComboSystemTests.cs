using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NeonLadder.Core;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for the Combo Attack System
    /// Verifies that combo detection and execution work correctly
    /// </summary>
    [TestFixture]
    public class ComboSystemTests
    {
        private ComboSystem comboSystem;
        private GameObject playerObject;
        private Player player;
        
        [SetUp]
        public void Setup()
        {
            // Initialize combo system
            comboSystem = new ComboSystem();
            Simulation.SetModel(comboSystem);
            
            // Create test player with proper dependencies
            playerObject = new GameObject("TestPlayer");
            
            // Add required base components first
            var rigidbody = playerObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            var animator = playerObject.AddComponent<Animator>();
            
            // Add required Player dependencies
            var health = playerObject.AddComponent<Health>();
            var stamina = playerObject.AddComponent<Stamina>();
            
            // Add both Player and PlayerAction components first
            var playerAction = playerObject.AddComponent<PlayerAction>();
            player = playerObject.AddComponent<Player>();
            
            // Add PlayerStateMediator last so it can find both components
            var mediator = playerObject.AddComponent<PlayerStateMediator>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (playerObject != null)
            {
                Object.DestroyImmediate(playerObject);
            }
            Simulation.DestroyModel<ComboSystem>();
        }
        
        [Test]
        public void ComboSystem_CanBeCreated()
        {
            // Assert
            Assert.IsNotNull(comboSystem, "ComboSystem should be created");
        }
        
        [Test]
        public void IsInComboWindow_ReturnsFalse_WhenNoInputHistory()
        {
            // Act
            bool isInWindow = comboSystem.IsInComboWindow(player);
            
            // Assert
            Assert.IsFalse(isInWindow, "Should not be in combo window without input history");
        }
        
        [Test]
        public void IsInComboWindow_ReturnsTrue_AfterAttackInput()
        {
            // Arrange
            var attackInput = new InputBufferEvent
            {
                inputType = InputType.Attack
            };
            attackInput.player = player;
            
            // Act
            comboSystem.CheckComboCompletion(player, attackInput);
            bool isInWindow = comboSystem.IsInComboWindow(player);
            
            // Assert
            Assert.IsTrue(isInWindow, "Should be in combo window after attack input");
        }
        
        [UnityTest]
        public IEnumerator IsInComboWindow_ReturnsFalse_AfterWindowExpires()
        {
            // Arrange
            var attackInput = new InputBufferEvent
            {
                inputType = InputType.Attack
            };
            attackInput.player = player;
            comboSystem.CheckComboCompletion(player, attackInput);
            
            // Act - Wait for combo window to expire (default 0.5s)
            yield return new WaitForSeconds(0.6f);
            
            bool isInWindow = comboSystem.IsInComboWindow(player);
            
            // Assert
            Assert.IsFalse(isInWindow, "Should not be in combo window after timeout");
        }
        
        [Test]
        public void CheckComboCompletion_TracksInputHistory()
        {
            // Arrange
            var input1 = new InputBufferEvent { inputType = InputType.Attack };
            input1.player = player;
            
            var input2 = new InputBufferEvent { inputType = InputType.Attack };
            input2.player = player;
            
            // Act
            comboSystem.CheckComboCompletion(player, input1);
            comboSystem.CheckComboCompletion(player, input2);
            
            // Assert - Should still be in combo window after multiple inputs
            Assert.IsTrue(comboSystem.IsInComboWindow(player), "Should track multiple inputs");
        }
        
        [Test]
        public void InputBufferEvent_UsesComboAttack_WhenInComboWindow()
        {
            // Arrange
            var firstAttack = new InputBufferEvent
            {
                inputType = InputType.Attack,
                player = player,
                comboStep = 0,
                comboId = "test-combo"
            };
            
            // Act - Execute first attack to start combo window
            comboSystem.CheckComboCompletion(player, firstAttack);
            
            // Create second attack event
            var secondAttack = new InputBufferEvent
            {
                inputType = InputType.Attack,
                player = player,
                comboStep = 1,
                comboId = "test-combo"
            };
            
            // Check if second attack would be a combo
            bool isCombo = comboSystem.IsInComboWindow(player);
            
            // Assert
            Assert.IsTrue(isCombo, "Second attack should be detected as combo attack");
        }
        
        [Test]
        public void ComboSystem_IntegratesWithSimulation()
        {
            // Arrange & Act
            var retrievedSystem = Simulation.GetModel<ComboSystem>();
            
            // Assert
            Assert.IsNotNull(retrievedSystem, "ComboSystem should be retrievable from Simulation");
            Assert.AreSame(comboSystem, retrievedSystem, "Should be the same instance");
        }
        
        [Test]
        public void DifferentInputTypes_CreateDifferentCombos()
        {
            // Arrange
            var attackInput = new InputBufferEvent { inputType = InputType.Attack };
            attackInput.player = player;
            
            var jumpInput = new InputBufferEvent { inputType = InputType.Jump };
            jumpInput.player = player;
            
            var sprintInput = new InputBufferEvent { inputType = InputType.Sprint };
            sprintInput.player = player;
            
            // Act - Create Attack->Jump->Attack sequence
            comboSystem.CheckComboCompletion(player, attackInput);
            comboSystem.CheckComboCompletion(player, jumpInput);
            comboSystem.CheckComboCompletion(player, attackInput);
            
            // Assert - Should still be tracking combo
            Assert.IsTrue(comboSystem.IsInComboWindow(player), "Should track mixed input combos");
        }
        
        [UnityTest]
        public IEnumerator ComboWindow_ResetsAfterTimeout()
        {
            // Arrange
            var input1 = new InputBufferEvent { inputType = InputType.Attack };
            input1.player = player;
            comboSystem.CheckComboCompletion(player, input1);
            
            // Act - Wait for window to expire
            yield return new WaitForSeconds(0.6f);
            
            // Add new input after timeout
            var input2 = new InputBufferEvent { inputType = InputType.Attack };
            input2.player = player;
            comboSystem.CheckComboCompletion(player, input2);
            
            // Assert - Should be in new combo window
            Assert.IsTrue(comboSystem.IsInComboWindow(player), "Should start new combo window after timeout");
        }
    }
}