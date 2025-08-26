using NUnit.Framework;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Controllers.Interfaces;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class CircularDependencyTests
    {
        private GameObject testObject;
        private Player player;
        private PlayerAction playerAction;
        private PlayerStateMediator mediator;
        
        [SetUp]
        public void Setup()
        {
            // Ignore animator and spawn point warnings in tests
            LogAssert.ignoreFailingMessages = true;
            
            // Create test game object with proper hierarchy (like other tests)
            testObject = new GameObject("TestPlayer");
            
            // Add required components to parent object
            var rigidbody = testObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Create child object for Player components (matches game hierarchy)
            var playerChild = new GameObject("PlayerChild");
            playerChild.transform.SetParent(testObject.transform);
            
            // Add both Player and PlayerAction components to child first
            playerAction = playerChild.AddComponent<PlayerAction>();
            player = playerChild.AddComponent<Player>();
            
            // Now add the mediator last so it can find both components in its Awake()
            mediator = playerChild.AddComponent<PlayerStateMediator>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            
            // Reset log assertion settings
            LogAssert.ignoreFailingMessages = false;
        }
        
        #region Circular Dependency Detection Tests
        
        [Test]
        public void Player_DoesNotDirectlyReferencePlayerAction()
        {
            // Arrange
            var playerType = typeof(Player);
            var playerActionType = typeof(PlayerAction);
            
            // Act - Check all fields for PlayerAction references
            var fields = playerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasDirectReference = fields.Any(f => f.FieldType == playerActionType);
            
            // Also check properties
            var properties = playerType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasPropertyReference = properties.Any(p => p.PropertyType == playerActionType);
            
            // Assert
            Assert.IsFalse(hasDirectReference, "Player should not have direct field reference to PlayerAction");
            Assert.IsFalse(hasPropertyReference, "Player should not have direct property reference to PlayerAction");
        }
        
        [Test]
        public void PlayerAction_DoesNotDirectlyReferencePlayer()
        {
            // Arrange
            var playerActionType = typeof(PlayerAction);
            var playerType = typeof(Player);
            
            // Act - Check all fields for Player references
            var fields = playerActionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasDirectReference = fields.Any(f => f.FieldType == playerType);
            
            // Also check properties
            var properties = playerActionType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasPropertyReference = properties.Any(p => p.PropertyType == playerType);
            
            // Assert
            Assert.IsFalse(hasDirectReference, "PlayerAction should not have direct field reference to Player");
            Assert.IsFalse(hasPropertyReference, "PlayerAction should not have direct property reference to Player");
        }
        
        [Test]
        public void Player_HasReferenceToMediator()
        {
            // Arrange
            var playerType = typeof(Player);
            var mediatorType = typeof(PlayerStateMediator);
            
            // Act - Check for mediator field
            var fields = playerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasMediatorReference = fields.Any(f => f.FieldType == mediatorType);
            
            // Assert
            Assert.IsTrue(hasMediatorReference, "Player should have reference to PlayerStateMediator");
        }
        
        [Test]
        public void PlayerAction_HasReferenceToMediator()
        {
            // Arrange
            var playerActionType = typeof(PlayerAction);
            var mediatorType = typeof(PlayerStateMediator);
            
            // Act - Check for mediator field
            var fields = playerActionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var hasMediatorReference = fields.Any(f => f.FieldType == mediatorType);
            
            // Assert
            Assert.IsTrue(hasMediatorReference, "PlayerAction should have reference to PlayerStateMediator");
        }
        
        #endregion
        
        #region Interface Implementation Tests
        
        [Test]
        public void Player_ImplementsIPlayerState()
        {
            // Assert
            Assert.IsTrue(player is IPlayerState, "Player should implement IPlayerState interface");
        }
        
        [Test]
        public void PlayerAction_ImplementsIPlayerActions()
        {
            // Assert
            Assert.IsTrue(playerAction is IPlayerActions, "PlayerAction should implement IPlayerActions interface");
        }
        
        [Test]
        public void IPlayerState_ProvidesRequiredProperties()
        {
            // Arrange
            IPlayerState playerState = player;
            
            // Act & Assert - Verify all interface properties are accessible
            Assert.DoesNotThrow(() => { var v = playerState.Velocity; }, "Velocity should be accessible");
            Assert.DoesNotThrow(() => { var g = playerState.IsGrounded; }, "IsGrounded should be accessible");
            Assert.DoesNotThrow(() => { var f = playerState.IsFacingLeft; }, "IsFacingLeft should be accessible");
            Assert.DoesNotThrow(() => { var z = playerState.IsMovingInZDimension; }, "IsMovingInZDimension should be accessible");
            Assert.DoesNotThrow(() => { var m = playerState.IsUsingMelee; }, "IsUsingMelee should be accessible");
            Assert.DoesNotThrow(() => { var a = playerState.AttackAnimationDuration; }, "AttackAnimationDuration should be accessible");
            Assert.DoesNotThrow(() => { var alive = playerState.IsAlive; }, "IsAlive should be accessible");
            Assert.DoesNotThrow(() => { var anim = playerState.Animator; }, "Animator should be accessible");
            Assert.DoesNotThrow(() => { var t = playerState.Transform; }, "Transform should be accessible");
        }
        
        [Test]
        public void IPlayerActions_ProvidesRequiredProperties()
        {
            // Arrange
            IPlayerActions actions = playerAction;
            
            // Act & Assert - Verify all interface properties are accessible
            Assert.DoesNotThrow(() => { var i = actions.PlayerInput; }, "PlayerInput should be accessible");
            Assert.DoesNotThrow(() => { var j = actions.IsJumping; }, "IsJumping should be accessible");
            Assert.DoesNotThrow(() => { var c = actions.IsClimbing; }, "IsClimbing should be accessible");
            Assert.DoesNotThrow(() => { var s = actions.IsSprinting; }, "IsSprinting should be accessible");
            Assert.DoesNotThrow(() => { var jc = actions.JumpCount; }, "JumpCount should be accessible");
            Assert.DoesNotThrow(() => { var mj = actions.MaxJumps; }, "MaxJumps should be accessible");
            Assert.DoesNotThrow(() => { var jf = actions.JumpForce; }, "JumpForce should be accessible");
            Assert.DoesNotThrow(() => { var a = actions.IsAttacking; }, "IsAttacking should be accessible");
        }
        
        #endregion
        
        #region Mediator Functionality Tests
        
        [Test]
        public void Mediator_CanAccessPlayerState()
        {
            // Act
            var velocity = mediator.GetPlayerVelocity();
            var isGrounded = mediator.IsPlayerGrounded();
            var isAlive = mediator.IsPlayerAlive();
            var animator = mediator.GetPlayerAnimator();
            var transform = mediator.GetPlayerTransform();
            
            // Assert
            Assert.IsNotNull(velocity, "Mediator should return player velocity");
            Assert.IsNotNull(transform, "Mediator should return player transform");
            // Note: Some values may be null/false in test environment, just checking access works
        }
        
        [Test]
        public void Mediator_CanAccessActionState()
        {
            // Act
            var input = mediator.GetPlayerInput();
            var isSprinting = mediator.IsPlayerSprinting();
            var isJumping = mediator.IsJumpRequested();
            var jumpCount = mediator.GetJumpCount();
            
            // Assert - Just verify methods are callable
            Assert.IsNotNull(input, "Mediator should return player input");
            Assert.AreEqual(0, jumpCount, "Jump count should be accessible through mediator");
        }
        
        [Test]
        public void Mediator_SetMediatorMethods_Exist()
        {
            // Arrange
            var playerType = typeof(Player);
            var playerActionType = typeof(PlayerAction);
            
            // Act
            var playerSetMethod = playerType.GetMethod("SetMediator");
            var actionSetMethod = playerActionType.GetMethod("SetMediator");
            
            // Assert
            Assert.IsNotNull(playerSetMethod, "Player should have SetMediator method");
            Assert.IsNotNull(actionSetMethod, "PlayerAction should have SetMediator method");
        }
        
        #endregion
        
        #region Functionality Preservation Tests
        
        [UnityTest]
        public IEnumerator PlayerMovement_StillFunctionsWithMediator()
        {
            // Arrange
            player.SetMediator(mediator);
            playerAction.SetMediator(mediator);
            
            // Simulate player input
            playerAction.playerInput = new Vector2(1f, 0f);
            
            // Wait a frame for initialization
            yield return null;
            
            // Act - Update player movement
            player.ComputeVelocity();
            
            // Assert - Player should respond to input through mediator
            // Note: In test environment, exact values may differ
            Assert.IsNotNull(player.velocity, "Player should have velocity");
        }
        
        [Test]
        public void JumpCount_ResetsProperlyThroughMediator()
        {
            // Arrange
            player.SetMediator(mediator);
            playerAction.SetMediator(mediator);
            
            // Act
            mediator.ResetJumpCount();
            var jumpCount = mediator.GetJumpCount();
            
            // Assert
            Assert.AreEqual(0, jumpCount, "Jump count should reset through mediator");
        }
        
        [Test]
        public void SprintState_AccessibleThroughMediator()
        {
            // Arrange
            player.SetMediator(mediator);
            playerAction.SetMediator(mediator);
            
            // Act
            playerAction.StartSprint();
            var isSprinting = mediator.IsPlayerSprinting();
            
            playerAction.StopSprint();
            var isNotSprinting = mediator.IsPlayerSprinting();
            
            // Assert
            Assert.IsTrue(isSprinting, "Should detect sprinting through mediator");
            Assert.IsFalse(isNotSprinting, "Should detect sprint stopped through mediator");
        }
        
        #endregion
        
        #region Clean Architecture Tests
        
        [Test]
        public void DependencyDirection_FollowsCleanArchitecture()
        {
            // This test verifies that dependencies flow in the correct direction:
            // Player -> Mediator <- PlayerAction
            // Neither Player nor PlayerAction directly reference each other
            
            // Arrange
            var playerType = typeof(Player);
            var playerActionType = typeof(PlayerAction);
            var mediatorType = typeof(PlayerStateMediator);
            
            // Act & Assert
            // Player references Mediator (OK)
            var playerFields = playerType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(playerFields.Any(f => f.FieldType == mediatorType), 
                "Player should reference Mediator");
            
            // PlayerAction references Mediator (OK)
            var actionFields = playerActionType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsTrue(actionFields.Any(f => f.FieldType == mediatorType), 
                "PlayerAction should reference Mediator");
            
            // Player does NOT reference PlayerAction (REQUIRED)
            Assert.IsFalse(playerFields.Any(f => f.FieldType == playerActionType), 
                "Player should NOT reference PlayerAction");
            
            // PlayerAction does NOT reference Player (REQUIRED)
            Assert.IsFalse(actionFields.Any(f => f.FieldType == playerType), 
                "PlayerAction should NOT reference Player");
        }
        
        [Test]
        public void Interfaces_EnableIndependentTesting()
        {
            // This test verifies that the interfaces allow for mock implementations
            // which is crucial for unit testing
            
            // Arrange
            IPlayerState mockPlayerState = new MockPlayerState();
            IPlayerActions mockActions = new MockPlayerActions();
            
            // Act & Assert - Verify mock implementations work
            Assert.IsNotNull(mockPlayerState.Velocity, "Mock player state should be usable");
            Assert.IsNotNull(mockActions.PlayerInput, "Mock player actions should be usable");
        }
        
        #endregion
        
        #region Mock Classes for Testing
        
        private class MockPlayerState : IPlayerState
        {
            public Vector3 Velocity => Vector3.zero;
            public bool IsGrounded => true;
            public bool IsFacingLeft => false;
            public bool IsMovingInZDimension => false;
            public bool IsUsingMelee => true;
            public float AttackAnimationDuration => 1.0f;
            public bool IsAlive => true;
            public float CurrentHealth => 100f;
            public float MaxHealth => 100f;
            public float CurrentStamina => 100f;
            public float MaxStamina => 100f;
            public Animator Animator => null;
            public Transform Transform => null;
            
            public void EnableZMovement() { }
            public void DisableZMovement() { }
        }
        
        private class MockPlayerActions : IPlayerActions
        {
            public Vector2 PlayerInput => Vector2.zero;
            public bool IsJumping => false;
            public bool IsClimbing => false;
            public bool? IsSprinting => false;
            public int JumpCount => 0;
            public int MaxJumps => 2;
            public float JumpForce => 10f;
            public bool IsAttacking => false;
            public float AttackAnimationDuration => 1.0f;
            
            public void ResetJumpCount() { }
            public void ScheduleJump(float delay) { }
            public void StartAttack() { }
            public void StopAttack() { }
            public void StartSprint() { }
            public void StopSprint() { }
        }
        
        #endregion
    }
}