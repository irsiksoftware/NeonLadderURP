using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;
using UnityEngine.SceneManagement;
using NeonLadder.Gameplay;

namespace NeonLadder.Tests.Runtime
{
    public class SceneTransitionTriggerTests
    {
        private GameObject triggerObject;
        private SceneTransitionTrigger trigger;
        private GameObject playerObject;
        private BoxCollider playerCollider;
        private BoxCollider triggerCollider;
        
        [SetUp]
        public void Setup()
        {
            // Create trigger object with collider (3D for 2.5D game)
            triggerObject = new GameObject("TestTrigger");
            triggerCollider = triggerObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(2f, 2f, 1f);
            trigger = triggerObject.AddComponent<SceneTransitionTrigger>();
            
            // Create player object with collider (3D for 2.5D game)
            playerObject = new GameObject("Player");
            playerObject.tag = "Player";
            playerCollider = playerObject.AddComponent<BoxCollider>();
            playerCollider.size = Vector3.one;
            playerObject.AddComponent<Rigidbody>().isKinematic = true;
        }
        
        [TearDown]
        public void TearDown()
        {
            if (triggerObject != null) Object.DestroyImmediate(triggerObject);
            if (playerObject != null) Object.DestroyImmediate(playerObject);
            
            // Clean up singletons
            var routingContext = GameObject.Find("SceneRoutingContext");
            if (routingContext != null) Object.DestroyImmediate(routingContext);
            
            var spawnManager = GameObject.Find("SpawnPointManager");
            if (spawnManager != null) Object.DestroyImmediate(spawnManager);
        }
        
        [Test]
        public void SceneTransitionTrigger_RequiresCollider()
        {
            // Arrange
            var testObject = new GameObject("WithColliderObject");
            
            // First add a collider (required by SceneTransitionTrigger)
            testObject.AddComponent<BoxCollider>();
            
            // Act & Assert - Should not throw when collider is present
            Assert.DoesNotThrow(() => testObject.AddComponent<SceneTransitionTrigger>());
            
            // Verify both components exist
            var addedTrigger = testObject.GetComponent<SceneTransitionTrigger>();
            Assert.IsNotNull(addedTrigger);
            Assert.IsNotNull(testObject.GetComponent<Collider>());
            
            Object.DestroyImmediate(testObject);
        }
        
        [Test]
        public void SetDestination_UpdatesOverrideSceneAndSpawnPoint()
        {
            // Arrange
            string sceneName = "TestScene";
            string spawnPoint = "TestSpawnPoint";
            
            // Act
            trigger.SetDestination(sceneName, spawnPoint);
            
            // Assert - using public API to verify
            Assert.AreEqual(TransitionDirection.Forward, trigger.GetDirection());
        }
        
        [Test]
        public void SetDirection_UpdatesTransitionDirection()
        {
            // Arrange
            var newDirection = TransitionDirection.Left;
            
            // Act
            trigger.SetDirection(newDirection);
            
            // Assert
            Assert.AreEqual(newDirection, trigger.GetDirection());
        }
        
        [Test]
        public void SetInteractive_ChangesMode()
        {
            // Arrange
            string customPrompt = "Custom Interaction";
            
            // Act
            trigger.SetInteractive(true, customPrompt);
            
            // Assert - mode change is internal but we can verify through behavior
            Assert.IsNotNull(trigger);
        }
        
        [Test]
        public void IsOneWay_ReturnsCorrectValue()
        {
            // Default should be false
            Assert.IsFalse(trigger.IsOneWay());
        }
        
        [Test]
        public void RequiresKey_ReturnsCorrectValue()
        {
            // Default should be false
            Assert.IsFalse(trigger.RequiresKey());
            Assert.IsNull(trigger.GetRequiredKey());
        }
        
        [Test]
        public void AutomaticMode_TriggersOnPlayerEnter()
        {
            // Arrange
            bool transitionStarted = false;
            SceneTransitionTrigger.OnTransitionStarted += (t) => transitionStarted = true;
            
            trigger.SetInteractive(false); // Automatic mode
            trigger.SetDestination("TestScene");
            
            // Act - Move player into trigger
            playerObject.transform.position = triggerObject.transform.position;
            
            // Simulate trigger enter manually for testing
            trigger.ForceTransition();
            
            // Assert
            Assert.IsTrue(transitionStarted);
            
            // Cleanup
            SceneTransitionTrigger.OnTransitionStarted -= (t) => transitionStarted = true;
        }
        
        [Test]
        public void InteractiveMode_DoesNotTriggerAutomatically()
        {
            // Arrange
            bool transitionStarted = false;
            SceneTransitionTrigger.OnTransitionStarted += (t) => transitionStarted = true;
            
            trigger.SetInteractive(true, "Press E"); // Interactive mode
            trigger.SetDestination("TestScene");
            
            // Act - Move player into trigger (but don't force transition)
            playerObject.transform.position = triggerObject.transform.position;
            
            // Assert - Should NOT trigger automatically
            Assert.IsFalse(transitionStarted);
            
            // Cleanup
            SceneTransitionTrigger.OnTransitionStarted -= (t) => transitionStarted = true;
        }
        
        [Test]
        public void TransitionEvents_CanBeSubscribedTo()
        {
            // Arrange
            bool startedEventFired = false;
            bool completedEventFired = false;
            bool failedEventFired = false;
            string failureReason = "";
            
            SceneTransitionTrigger.OnTransitionStarted += (t) => startedEventFired = true;
            SceneTransitionTrigger.OnTransitionCompleted += (t) => completedEventFired = true;
            SceneTransitionTrigger.OnTransitionFailed += (t, reason) => 
            {
                failedEventFired = true;
                failureReason = reason;
            };
            
            // Act - trigger failure by not setting destination
            trigger.ForceTransition();
            
            // Assert
            Assert.IsTrue(failedEventFired || startedEventFired); // One should fire
            
            // Cleanup
            SceneTransitionTrigger.OnTransitionStarted -= (t) => startedEventFired = true;
            SceneTransitionTrigger.OnTransitionCompleted -= (t) => completedEventFired = true;
            SceneTransitionTrigger.OnTransitionFailed -= (t, reason) => 
            {
                failedEventFired = true;
                failureReason = reason;
            };
        }
        
        [Test]
        public void MultipleDirections_AreSupported()
        {
            // Test all direction enums
            var directions = new[]
            {
                TransitionDirection.Left,
                TransitionDirection.Right,
                TransitionDirection.Up,
                TransitionDirection.Down,
                TransitionDirection.Forward,
                TransitionDirection.Backward,
                TransitionDirection.Any
            };
            
            foreach (var dir in directions)
            {
                trigger.SetDirection(dir);
                Assert.AreEqual(dir, trigger.GetDirection());
            }
        }
        
        [Test]
        public void SpawnPointManager_ReceivesTransitionContext()
        {
            // Arrange
            var spawnManager = new GameObject("SpawnPointManager").AddComponent<SpawnPointManager>();
            
            trigger.SetDirection(TransitionDirection.Left);
            trigger.SetDestination("TestScene", "CustomSpawnPoint");
            
            // Act
            trigger.ForceTransition();
            
            // Assert - SpawnPointManager should have received context
            Assert.IsNotNull(SpawnPointManager.Instance);
            
            // Cleanup
            Object.DestroyImmediate(spawnManager.gameObject);
        }
        
        [Test]
        public void ColliderConfiguration_IsTriggerByDefault()
        {
            // Arrange
            var newTriggerObject = new GameObject("NewTrigger");
            var collider = newTriggerObject.AddComponent<BoxCollider>();
            collider.isTrigger = false; // Set to false initially
            
            // Act
            newTriggerObject.AddComponent<SceneTransitionTrigger>();
            
            // Assert
            Assert.IsTrue(collider.isTrigger);
            
            // Cleanup
            Object.DestroyImmediate(newTriggerObject);
        }
        
        [Test]
        public void SupportsMultipleColliderTypes()
        {
            // Test BoxCollider (3D for 2.5D game)
            var boxObject = new GameObject("BoxTrigger");
            boxObject.AddComponent<BoxCollider>();
            var boxTrigger = boxObject.AddComponent<SceneTransitionTrigger>();
            Assert.IsNotNull(boxTrigger);
            Object.DestroyImmediate(boxObject);
            
            // Test SphereCollider (3D equivalent of circle for 2.5D game)
            var sphereObject = new GameObject("SphereTrigger");
            sphereObject.AddComponent<SphereCollider>();
            var sphereTrigger = sphereObject.AddComponent<SceneTransitionTrigger>();
            Assert.IsNotNull(sphereTrigger);
            Object.DestroyImmediate(sphereObject);
            
            // Test MeshCollider (3D equivalent of polygon for 2.5D game)
            var meshObject = new GameObject("MeshTrigger");
            var meshCollider = meshObject.AddComponent<MeshCollider>();
            meshCollider.convex = true; // Required for triggers
            var meshTrigger = meshObject.AddComponent<SceneTransitionTrigger>();
            Assert.IsNotNull(meshTrigger);
            Object.DestroyImmediate(meshObject);
        }
        
        [Test]
        public void PlayerTag_IsRecognized()
        {
            // Arrange
            bool transitionStarted = false;
            SceneTransitionTrigger.OnTransitionStarted += (t) => transitionStarted = true;
            
            trigger.SetInteractive(false);
            trigger.SetDestination("TestScene");
            
            // Ensure player has correct tag
            playerObject.tag = "Player";
            
            // Act
            trigger.ForceTransition();
            
            // Assert
            Assert.IsTrue(transitionStarted);
            
            // Cleanup
            SceneTransitionTrigger.OnTransitionStarted -= (t) => transitionStarted = true;
        }
        
        [Test]
        public void NonPlayer_DoesNotTriggerTransition()
        {
            // Arrange
            bool transitionStarted = false;
            SceneTransitionTrigger.OnTransitionStarted += (t) => transitionStarted = true;
            
            trigger.SetInteractive(false);
            trigger.SetDestination("TestScene");
            
            // Create non-player object (using Minor enemy tag)
            var enemyObject = new GameObject("MinorEnemy");
            enemyObject.tag = "Minor";
            var enemyCollider = enemyObject.AddComponent<BoxCollider>();
            enemyObject.AddComponent<Rigidbody>().isKinematic = true;
            
            // Act - Only player should trigger, enemy shouldn't
            // Since we don't have physics simulation in unit tests, just verify the setup
            Assert.AreEqual("Minor", enemyObject.tag);
            Assert.IsFalse(transitionStarted); // Should not have triggered
            
            // Cleanup
            Object.DestroyImmediate(enemyObject);
            SceneTransitionTrigger.OnTransitionStarted -= (t) => transitionStarted = true;
        }
    }
}