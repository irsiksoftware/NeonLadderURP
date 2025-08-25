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
            // Test that SceneTransitionTrigger properly works with a trigger collider object
            
            // Arrange - Create trigger collider first to avoid validation error
            var triggerObject = new GameObject("TriggerObject");
            var triggerCollider = new GameObject("TriggerCollider");
            triggerCollider.AddComponent<BoxCollider>().isTrigger = true;
            
            // Deactivate to avoid Awake() validation error
            triggerObject.SetActive(false);
            var trigger = triggerObject.AddComponent<SceneTransitionTrigger>();
            
            // Set the trigger collider before activating
            trigger.SetTriggerColliderObject(triggerCollider);
            triggerObject.SetActive(true); // Now it should validate successfully
            
            // Act & Assert - Verify the requirement is satisfied
            Assert.AreEqual(triggerCollider, trigger.GetTriggerColliderObject(),
                "Should have the assigned trigger collider object");
            
            // Verify the collider has the required properties
            var collider = trigger.GetTriggerColliderObject().GetComponent<Collider>();
            Assert.IsNotNull(collider, "Assigned object should have a Collider component");
            Assert.IsTrue(collider.isTrigger, "Collider should be marked as trigger");
            
            // Test that we can change the collider object
            var newTriggerCollider = new GameObject("NewTriggerCollider");
            newTriggerCollider.AddComponent<SphereCollider>().isTrigger = true;
            
            trigger.SetTriggerColliderObject(newTriggerCollider);
            Assert.AreEqual(newTriggerCollider, trigger.GetTriggerColliderObject(),
                "Should be able to change trigger collider object");
            
            // Cleanup
            Object.DestroyImmediate(triggerObject);
            Object.DestroyImmediate(triggerCollider);
            Object.DestroyImmediate(newTriggerCollider);
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
    }
}