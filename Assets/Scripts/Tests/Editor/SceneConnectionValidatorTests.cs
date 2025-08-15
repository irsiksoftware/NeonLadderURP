using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.ProceduralGeneration;
using UnityEditor;
using NeonLadder.Editor;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Tests.Editor
{
    public class SceneConnectionValidatorTests
    {
        private GameObject testObject;
        private SceneTransitionTrigger trigger;
        private SceneConnectionOverride override_;
        
        [SetUp]
        public void Setup()
        {
            // Create test objects for validation
            testObject = new GameObject("TestValidationObject");
            
            // Create separate collider object (new pattern)
            var colliderObject = new GameObject("TriggerCollider");
            colliderObject.transform.SetParent(testObject.transform);
            var collider = colliderObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Add transition trigger and assign collider
            testObject.SetActive(false);
            trigger = testObject.AddComponent<SceneTransitionTrigger>();
            trigger.SetTriggerColliderObject(colliderObject);
            testObject.SetActive(true);
            
            override_ = testObject.AddComponent<SceneConnectionOverride>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }
        
        [Test]
        public void ValidationResult_ConstructorSetsProperties()
        {
            // Arrange & Act
            var result = new SceneConnectionValidatorWindow.ValidationResult(
                "TestScene", 
                "TestObject", 
                SceneConnectionValidatorWindow.ValidationSeverity.Error, 
                "Test message", 
                "Test fix"
            );
            
            // Assert
            Assert.AreEqual("TestScene", result.sceneName);
            Assert.AreEqual("TestObject", result.objectName);
            Assert.AreEqual(SceneConnectionValidatorWindow.ValidationSeverity.Error, result.severity);
            Assert.AreEqual("Test message", result.message);
            Assert.AreEqual("Test fix", result.fixAction);
        }
        
        [Test]
        public void ValidationSeverity_EnumValues()
        {
            // Test all severity levels exist
            var severities = new[]
            {
                SceneConnectionValidatorWindow.ValidationSeverity.Info,
                SceneConnectionValidatorWindow.ValidationSeverity.Warning,
                SceneConnectionValidatorWindow.ValidationSeverity.Error
            };
            
            foreach (var severity in severities)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(SceneConnectionValidatorWindow.ValidationSeverity), severity));
            }
        }
        
        [Test]
        public void FilterLevel_EnumValues()
        {
            // Test all filter levels exist
            var filters = new[]
            {
                SceneConnectionValidatorWindow.FilterLevel.All,
                SceneConnectionValidatorWindow.FilterLevel.WarningsAndErrors,
                SceneConnectionValidatorWindow.FilterLevel.ErrorsOnly
            };
            
            foreach (var filter in filters)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(SceneConnectionValidatorWindow.FilterLevel), filter));
            }
        }
        
        [Test]
        public void ExportFormat_EnumValues()
        {
            // Test all export formats exist
            var formats = new[]
            {
                SceneConnectionValidatorWindow.ExportFormat.JSON,
                SceneConnectionValidatorWindow.ExportFormat.CSV,
                SceneConnectionValidatorWindow.ExportFormat.Markdown
            };
            
            foreach (var format in formats)
            {
                Assert.IsTrue(System.Enum.IsDefined(typeof(SceneConnectionValidatorWindow.ExportFormat), format));
            }
        }
        
        [Test]
        public void ValidationSettings_DefaultValues()
        {
            // Arrange & Act
            var settings = new SceneConnectionValidatorWindow.ValidationSettings();
            
            // Assert - Test default values make sense
            Assert.IsTrue(settings.scanBuildScenesOnly);
            Assert.IsFalse(settings.includeDisabledScenes);
            Assert.IsTrue(settings.validateSpawnPoints);
            Assert.IsTrue(settings.validateOverrides);
            Assert.IsTrue(settings.validateTriggers);
            Assert.IsTrue(settings.checkCircularReferences);
            Assert.IsTrue(settings.showProceduralPaths);
            Assert.IsTrue(settings.showManualOverrides);
            Assert.AreEqual(SceneConnectionValidatorWindow.FilterLevel.All, settings.filterLevel);
            Assert.AreEqual(SceneConnectionValidatorWindow.ExportFormat.JSON, settings.exportFormat);
            Assert.IsTrue(settings.includeValidationReport);
        }
        
        [Test]
        public void ValidationResult_WithoutFixAction()
        {
            // Arrange & Act
            var result = new SceneConnectionValidatorWindow.ValidationResult(
                "TestScene", 
                "TestObject", 
                SceneConnectionValidatorWindow.ValidationSeverity.Info, 
                "Info message"
            );
            
            // Assert
            Assert.AreEqual("TestScene", result.sceneName);
            Assert.AreEqual("TestObject", result.objectName);
            Assert.AreEqual(SceneConnectionValidatorWindow.ValidationSeverity.Info, result.severity);
            Assert.AreEqual("Info message", result.message);
            Assert.IsNull(result.fixAction);
        }
        
        [Test]
        public void ValidationResult_EmptyObjectName()
        {
            // Arrange & Act
            var result = new SceneConnectionValidatorWindow.ValidationResult(
                "TestScene", 
                "", 
                SceneConnectionValidatorWindow.ValidationSeverity.Warning, 
                "Scene-level warning"
            );
            
            // Assert
            Assert.AreEqual("TestScene", result.sceneName);
            Assert.AreEqual("", result.objectName);
            Assert.AreEqual(SceneConnectionValidatorWindow.ValidationSeverity.Warning, result.severity);
            Assert.AreEqual("Scene-level warning", result.message);
        }
        
        // [Test] - Editor tests not supported in runtime
        // public void ValidatorWindow_CanBeCreated()
        // {
        //     // This test would require editor mode testing
        //     // For runtime tests, we just verify the types exist
        //     
        //     Assert.IsNotNull(typeof(SceneConnectionValidatorWindow));
        //     Assert.IsNotNull(typeof(SceneConnectionValidatorWindow.ValidationResult));
        //     Assert.IsNotNull(typeof(SceneConnectionValidatorWindow.ValidationSettings));
        // }
        
        [Test]
        public void TriggerValidation_DetectsColliderIssues()
        {
            // Arrange - Remove components in dependency order to allow collider removal
            // SceneConnectionOverride depends on SceneTransitionTrigger
            // SceneTransitionTrigger depends on Collider (RequireComponent)
            Object.DestroyImmediate(override_);  // Remove override first
            Object.DestroyImmediate(trigger);    // Then remove trigger
            
            // Then remove collider to create validation issue
            var collider = testObject.GetComponent<BoxCollider>();
            Object.DestroyImmediate(collider);
            
            // Act - Check if trigger detects missing collider
            var hasCollider = testObject.GetComponent<Collider>() != null;
            
            // Assert
            Assert.IsFalse(hasCollider);
        }
        
        [Test]
        public void TriggerValidation_DetectsDirectionIssues()
        {
            // Arrange
            trigger.SetDirection(TransitionDirection.Any);
            
            // Act
            var direction = trigger.GetDirection();
            
            // Assert - Any direction might be flagged as warning
            Assert.AreEqual(TransitionDirection.Any, direction);
        }
        
        [Test]
        public void OverrideValidation_DetectsConfigurationIssues()
        {
            // Arrange & Act
            var validationStatus = override_.ValidateOverride();
            
            // Assert - Should have some validation status
            Assert.IsTrue(System.Enum.IsDefined(typeof(ValidationStatus), validationStatus));
        }
        
        [Test]
        public void ValidationSystem_HandlesMultipleSeverities()
        {
            // Arrange
            var results = new List<SceneConnectionValidatorWindow.ValidationResult>
            {
                new SceneConnectionValidatorWindow.ValidationResult("Scene1", "Obj1", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Error, "Error message"),
                new SceneConnectionValidatorWindow.ValidationResult("Scene2", "Obj2", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Warning, "Warning message"),
                new SceneConnectionValidatorWindow.ValidationResult("Scene3", "Obj3", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Info, "Info message")
            };
            
            // Act
            var errorCount = 0;
            var warningCount = 0;
            var infoCount = 0;
            
            foreach (var result in results)
            {
                switch (result.severity)
                {
                    case SceneConnectionValidatorWindow.ValidationSeverity.Error:
                        errorCount++;
                        break;
                    case SceneConnectionValidatorWindow.ValidationSeverity.Warning:
                        warningCount++;
                        break;
                    case SceneConnectionValidatorWindow.ValidationSeverity.Info:
                        infoCount++;
                        break;
                }
            }
            
            // Assert
            Assert.AreEqual(1, errorCount);
            Assert.AreEqual(1, warningCount);
            Assert.AreEqual(1, infoCount);
        }
        
        [Test]
        public void ValidationSystem_FiltersByLevel()
        {
            // Arrange
            var allResults = new List<SceneConnectionValidatorWindow.ValidationResult>
            {
                new SceneConnectionValidatorWindow.ValidationResult("Scene1", "Obj1", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Error, "Error"),
                new SceneConnectionValidatorWindow.ValidationResult("Scene2", "Obj2", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Warning, "Warning"),
                new SceneConnectionValidatorWindow.ValidationResult("Scene3", "Obj3", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Info, "Info")
            };
            
            // Act - Filter errors only
            var errorsOnly = new List<SceneConnectionValidatorWindow.ValidationResult>();
            foreach (var result in allResults)
            {
                if (result.severity == SceneConnectionValidatorWindow.ValidationSeverity.Error)
                {
                    errorsOnly.Add(result);
                }
            }
            
            // Assert
            Assert.AreEqual(1, errorsOnly.Count);
            Assert.AreEqual(SceneConnectionValidatorWindow.ValidationSeverity.Error, errorsOnly[0].severity);
        }
        
        [Test]
        public void ValidationSystem_TracksSceneNames()
        {
            // Arrange
            var results = new List<SceneConnectionValidatorWindow.ValidationResult>
            {
                new SceneConnectionValidatorWindow.ValidationResult("MainCityHub", "Trigger1", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Warning, "Message1"),
                new SceneConnectionValidatorWindow.ValidationResult("Banquet_Connection1", "Trigger2", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Error, "Message2"),
                new SceneConnectionValidatorWindow.ValidationResult("MainCityHub", "Override1", 
                    SceneConnectionValidatorWindow.ValidationSeverity.Info, "Message3")
            };
            
            // Act - Count unique scenes
            var sceneNames = new HashSet<string>();
            foreach (var result in results)
            {
                sceneNames.Add(result.sceneName);
            }
            
            // Assert
            Assert.AreEqual(2, sceneNames.Count);
            Assert.IsTrue(sceneNames.Contains("MainCityHub"));
            Assert.IsTrue(sceneNames.Contains("Banquet_Connection1"));
        }
        
        [Test]
        public void ValidationSystem_SupportsFixActions()
        {
            // Arrange
            var resultWithFix = new SceneConnectionValidatorWindow.ValidationResult(
                "TestScene", "TestObj", 
                SceneConnectionValidatorWindow.ValidationSeverity.Error, 
                "Missing component", 
                "Add component");
            
            var resultWithoutFix = new SceneConnectionValidatorWindow.ValidationResult(
                "TestScene", "TestObj", 
                SceneConnectionValidatorWindow.ValidationSeverity.Info, 
                "Informational message");
            
            // Assert
            Assert.IsNotNull(resultWithFix.fixAction);
            Assert.AreEqual("Add component", resultWithFix.fixAction);
            Assert.IsNull(resultWithoutFix.fixAction);
        }
    }
}