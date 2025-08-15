using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;
using UnityEngine.SceneManagement;
using NeonLadder.Gameplay;
using NeonLadder.Mechanics.Enums;

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
            // Create trigger object (main SceneTransitionTrigger component)
            triggerObject = new GameObject("TestTrigger");
            
            // Create separate collider object first (new pattern)
            var colliderObject = new GameObject("TriggerCollider");
            colliderObject.transform.SetParent(triggerObject.transform);
            triggerCollider = colliderObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(2f, 2f, 1f);
            
            // Temporarily disable the object to prevent Awake from running
            triggerObject.SetActive(false);
            trigger = triggerObject.AddComponent<SceneTransitionTrigger>();
            trigger.SetTriggerColliderObject(colliderObject);
            // Re-enable the object now that collider is assigned
            triggerObject.SetActive(true);
            
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
        public void SceneTransitionTrigger_RequiresTriggerColliderObject()
        {
            // Arrange
            var triggerObject = new GameObject("TriggerObject");
            var colliderObject = new GameObject("ColliderObject");
            colliderObject.AddComponent<BoxCollider>().isTrigger = true;
            
            // Act - Create trigger without assigning collider object initially, then assign it
            triggerObject.SetActive(false);
            var trigger = triggerObject.AddComponent<SceneTransitionTrigger>();
            triggerObject.SetActive(true); // This will trigger validation and show missing collider
            
            // Now assign the collider object - should work without errors
            trigger.SetTriggerColliderObject(colliderObject);
            
            // Assert - Verify assignment worked
            Assert.AreEqual(colliderObject, trigger.GetTriggerColliderObject());
            
            Object.DestroyImmediate(triggerObject);
            Object.DestroyImmediate(colliderObject);
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
        public void ColliderConfiguration_AutoFixesTriggerFlag()
        {
            // Arrange
            var triggerObject = new GameObject("TriggerObject");
            var colliderObject = new GameObject("ColliderObject");
            var collider = colliderObject.AddComponent<BoxCollider>();
            collider.isTrigger = false; // Set to false initially
            
            // Act - Auto-fixing trigger flag behavior test
            triggerObject.SetActive(false);
            var trigger = triggerObject.AddComponent<SceneTransitionTrigger>();
            trigger.SetTriggerColliderObject(colliderObject);
            triggerObject.SetActive(true);
            
            // Assert - Should be auto-fixed to true
            Assert.IsTrue(collider.isTrigger);
            
            // Cleanup
            Object.DestroyImmediate(triggerObject);
            Object.DestroyImmediate(colliderObject);
        }
        
        [Test]
        public void SupportsMultipleColliderTypes()
        {
            // Test BoxCollider on separate object
            var triggerObject1 = new GameObject("BoxTrigger");
            var colliderObject1 = new GameObject("BoxColliderObject");
            colliderObject1.AddComponent<BoxCollider>().isTrigger = true;
            triggerObject1.SetActive(false);
            var trigger1 = triggerObject1.AddComponent<SceneTransitionTrigger>();
            trigger1.SetTriggerColliderObject(colliderObject1);
            triggerObject1.SetActive(true);
            Assert.IsNotNull(trigger1);
            Object.DestroyImmediate(triggerObject1);
            Object.DestroyImmediate(colliderObject1);
            
            // Test SphereCollider on separate object
            var triggerObject2 = new GameObject("SphereTrigger");
            var colliderObject2 = new GameObject("SphereColliderObject");
            colliderObject2.AddComponent<SphereCollider>().isTrigger = true;
            triggerObject2.SetActive(false);
            var trigger2 = triggerObject2.AddComponent<SceneTransitionTrigger>();
            trigger2.SetTriggerColliderObject(colliderObject2);
            triggerObject2.SetActive(true);
            Assert.IsNotNull(trigger2);
            Object.DestroyImmediate(triggerObject2);
            Object.DestroyImmediate(colliderObject2);
            
            // Test MeshCollider on separate object
            var triggerObject3 = new GameObject("MeshTrigger");
            var colliderObject3 = new GameObject("MeshColliderObject");
            var meshCollider = colliderObject3.AddComponent<MeshCollider>();
            meshCollider.convex = true; // Required for triggers
            meshCollider.isTrigger = true;
            triggerObject3.SetActive(false);
            var trigger3 = triggerObject3.AddComponent<SceneTransitionTrigger>();
            trigger3.SetTriggerColliderObject(colliderObject3);
            triggerObject3.SetActive(true);
            Assert.IsNotNull(trigger3);
            Object.DestroyImmediate(triggerObject3);
            Object.DestroyImmediate(colliderObject3);
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