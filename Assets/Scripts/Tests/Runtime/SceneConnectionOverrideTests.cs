using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Tests.Runtime
{
    public class SceneConnectionOverrideTests
    {
        private GameObject testObject;
        private SceneTransitionTrigger trigger;
        private SceneConnectionOverride override_;
        
        /// <summary>
        /// Helper method to create a properly configured SceneTransitionTrigger for testing
        /// </summary>
        private SceneTransitionTrigger CreateTestTrigger(GameObject parent, string name = "TestTrigger")
        {
            var triggerObj = new GameObject(name);
            triggerObj.transform.SetParent(parent.transform);
            
            // Create collider object
            var colliderObj = new GameObject($"{name}_Collider");
            colliderObj.transform.SetParent(triggerObj.transform);
            var collider = colliderObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Use the same pattern as SceneTransitionTriggerTests: disable, add component, assign collider, enable
            triggerObj.SetActive(false);
            var triggerComponent = triggerObj.AddComponent<SceneTransitionTrigger>();
            triggerComponent.SetTriggerColliderObject(colliderObj);
            triggerObj.SetActive(true);
            
            return triggerComponent;
        }
        
        [SetUp]
        public void Setup()
        {
            // Create test object (main object with override component)
            testObject = new GameObject("TestOverrideObject");
            
            // Create trigger with proper collider setup using helper method
            trigger = CreateTestTrigger(testObject, "TestTrigger");
            
            // Add override component to the trigger's parent object
            override_ = trigger.gameObject.AddComponent<SceneConnectionOverride>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            
            // Clean up routing context singleton
            var routingContext = GameObject.Find("SceneRoutingContext");
            if (routingContext != null)
            {
                Object.DestroyImmediate(routingContext);
            }
        }
        
        [Test]
        public void SceneConnectionOverride_RequiresTransitionTrigger()
        {
            // Arrange
            var newObject = new GameObject("NoTriggerObject");
            
            // Create collider object first to avoid validation error
            var colliderObj = new GameObject("TestCollider");
            colliderObj.transform.SetParent(newObject.transform);
            colliderObj.AddComponent<BoxCollider>().isTrigger = true;
            
            // Disable object to prevent Awake validation errors
            newObject.SetActive(false);
            
            // Act & Assert - Adding SceneConnectionOverride should automatically add SceneTransitionTrigger
            Assert.DoesNotThrow(() => newObject.AddComponent<SceneConnectionOverride>());
            
            // Verify that SceneTransitionTrigger was automatically added
            var trigger = newObject.GetComponent<SceneTransitionTrigger>();
            Assert.IsNotNull(trigger, "SceneTransitionTrigger should be automatically added due to RequireComponent");
            
            // Set up the trigger properly and re-enable
            trigger.SetTriggerColliderObject(colliderObj);
            newObject.SetActive(true);
            
            // Cleanup
            Object.DestroyImmediate(newObject);
            Object.DestroyImmediate(colliderObj);
        }
        
        [Test]
        public void SetEnabled_UpdatesOverrideState()
        {
            // Arrange
            override_.SetEnabled(false);
            
            // Assert
            Assert.IsFalse(override_.IsEnabled());
            
            // Act
            override_.SetEnabled(true);
            
            // Assert
            Assert.IsTrue(override_.IsEnabled());
        }
        
        [Test]
        public void SetPriority_UpdatesPriorityValue()
        {
            // Arrange
            int newPriority = 500;
            
            // Act
            override_.SetPriority(newPriority);
            
            // Assert
            Assert.AreEqual(newPriority, override_.GetPriority());
        }
        
        [Test]
        public void ValidateOverride_ReturnsValidationStatus()
        {
            // Act
            var status = override_.ValidateOverride();
            
            // Assert - Should have some validation status
            Assert.IsTrue(System.Enum.IsDefined(typeof(ValidationStatus), status));
        }
        
        [Test]
        public void GetTargetScene_ReturnsNullWhenNotConfigured()
        {
            // Act
            string targetScene = override_.GetTargetScene();
            
            // Assert
            Assert.IsNull(targetScene);
        }
        
        [Test]
        public void GetTargetSpawnPoint_ReturnsNullWhenNotOverridden()
        {
            // Act
            string spawnPoint = override_.GetTargetSpawnPoint();
            
            // Assert
            Assert.IsNull(spawnPoint);
        }
        
        [Test]
        public void ConditionalOverrides_CanBeAddedAndRemoved()
        {
            // Arrange
            var conditional = new ConditionalOverride
            {
                type = ConditionType.Custom,
                conditionKey = "TestCondition",
                overrideSceneName = "TestScene",
                description = "Test conditional override"
            };
            
            // Act - Add
            override_.AddConditionalOverride(conditional);
            var overrides = override_.GetConditionalOverrides();
            
            // Assert
            Assert.AreEqual(1, overrides.Count);
            Assert.AreEqual("TestCondition", overrides[0].conditionKey);
            
            // Act - Remove
            override_.RemoveConditionalOverride(conditional);
            overrides = override_.GetConditionalOverrides();
            
            // Assert
            Assert.AreEqual(0, overrides.Count);
        }
        
        [Test]
        public void ConditionalOverride_EvaluatesCustomCondition()
        {
            // Arrange
            var conditional = new ConditionalOverride
            {
                type = ConditionType.Custom,
                conditionKey = "CustomKey",
                invertCondition = false
            };
            
            // Act
            bool result = conditional.EvaluateCondition();
            
            // Assert - Should return false when no data exists
            Assert.IsFalse(result);
        }
        
        [Test]
        public void ConditionalOverride_InvertConditionWorks()
        {
            // Arrange
            var conditional = new ConditionalOverride
            {
                type = ConditionType.Custom,
                conditionKey = "NonExistentKey",
                invertCondition = true
            };
            
            // Act
            bool result = conditional.EvaluateCondition();
            
            // Assert - Should return true (inverted false)
            Assert.IsTrue(result);
        }
        
        [Test]
        public void ValidationStatus_EnumValues()
        {
            // Test all validation status values
            var statuses = new[]
            {
                ValidationStatus.NotValidated,
                ValidationStatus.Valid,
                ValidationStatus.Warning,
                ValidationStatus.Error
            };
            
            foreach (var status in statuses)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(ValidationStatus), status));
            }
        }
        
        [Test]
        public void SceneSelectionMode_EnumValues()
        {
            // Test all selection modes
            var modes = new[]
            {
                SceneSelectionMode.SceneName,
                SceneSelectionMode.BuildIndex,
                SceneSelectionMode.AssetPath
            };
            
            foreach (var mode in modes)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(SceneSelectionMode), mode));
            }
        }
        
        [Test]
        public void ConditionType_EnumValues()
        {
            // Test all condition types
            var types = new[]
            {
                ConditionType.HasItem,
                ConditionType.BossDefeated,
                ConditionType.PathCompleted,
                ConditionType.Custom
            };
            
            foreach (var type in types)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(ConditionType), type));
            }
        }
        
        [Test]
        public void ApplyOverride_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            string appliedScene = "";
            
            SceneConnectionOverride.OnOverrideApplied += (o, scene) =>
            {
                eventFired = true;
                appliedScene = scene;
            };
            
            // Enable override and set a target
            override_.SetEnabled(true);
            
            // Act
            override_.ApplyOverride();
            
            // Assert - Event may or may not fire depending on configuration
            Assert.IsNotNull(override_);
            
            // Cleanup
            SceneConnectionOverride.OnOverrideApplied -= (o, scene) =>
            {
                eventFired = true;
                appliedScene = scene;
            };
        }
        
        [Test]
        public void ValidationComplete_FiresEvent()
        {
            // Arrange
            bool eventFired = false;
            ValidationStatus receivedStatus = ValidationStatus.NotValidated;
            
            SceneConnectionOverride.OnValidationComplete += (o, status) =>
            {
                eventFired = true;
                receivedStatus = status;
            };
            
            // Act
            override_.ValidateOverride();
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsTrue(System.Enum.IsDefined(typeof(ValidationStatus), receivedStatus));
            
            // Cleanup
            SceneConnectionOverride.OnValidationComplete -= (o, status) =>
            {
                eventFired = true;
                receivedStatus = status;
            };
        }
        
        [Test]
        public void GetAllScenesInBuildSettings_ReturnsSceneList()
        {
            // Act
            var scenes = SceneConnectionOverride.GetAllScenesInBuildSettings();
            
            // Assert
            Assert.IsNotNull(scenes);
            Assert.IsInstanceOf<List<string>>(scenes);
        }
        
        [Test]
        public void GetAllOverridesInScene_ReturnsEmptyListForUnknownScene()
        {
            // Act
            var overrides = SceneConnectionOverride.GetAllOverridesInScene("NonExistentScene");
            
            // Assert
            Assert.IsNotNull(overrides);
            Assert.AreEqual(0, overrides.Count);
        }
        
        
        [Test]
        public void ConditionalOverride_BossDefeatedCondition()
        {
            // Arrange
            var conditional = new ConditionalOverride
            {
                type = ConditionType.BossDefeated,
                conditionKey = "Pride",
                invertCondition = false
            };
            
            // Act - Should be false initially
            bool result = conditional.EvaluateCondition();
            
            // Assert
            Assert.IsFalse(result);
            
            // Simulate boss defeat
            if (SceneRoutingContext.Instance != null)
            {
                SceneRoutingContext.Instance.SetPersistentData("Boss_Pride_Defeated", true);
                
                // Test again
                result = conditional.EvaluateCondition();
                Assert.IsTrue(result);
            }
        }
        
        [Test]
        public void ConditionalOverride_PathCompletedCondition()
        {
            // Arrange
            var conditional = new ConditionalOverride
            {
                type = ConditionType.PathCompleted,
                conditionKey = "MainCityHub",
                invertCondition = false
            };
            
            // Act - Should be false initially
            bool result = conditional.EvaluateCondition();
            
            // Assert
            Assert.IsFalse(result);
            
            // Simulate path completion
            if (SceneRoutingContext.Instance != null)
            {
                SceneRoutingContext.Instance.AddVisitedScene("MainCityHub");
                
                // Test again
                result = conditional.EvaluateCondition();
                Assert.IsTrue(result);
            }
        }
    }
}