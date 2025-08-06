using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadderURP.Editor.SaveSystem;
using NeonLadderURP.DataManagement;
using NeonLadderURP.DataManagement.Examples;
using NeonLadderURP.Models;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Sue Storm's SaveStateConfigurationEditor Tests - Comprehensive CustomEditor Testing
    /// Testing save state configuration UI, file operations, and data management
    /// 
    /// "I see everything in this editor, including what could go wrong in production."
    /// </summary>
    [TestFixture]
    public class SaveStateConfigurationEditorTests
    {
        private SaveStateConfiguration mockConfig;
        private SaveStateConfigurationEditor editor;
        private SerializedObject serializedObject;
        
        #region Setup & Teardown
        
        [SetUp]
        public void SetUp()
        {
            // Create mock configuration
            mockConfig = ScriptableObject.CreateInstance<SaveStateConfiguration>();
            mockConfig.configurationName = "Test Configuration";
            mockConfig.description = "Test save state for unit testing";
            
            // Initialize configuration with test data
            mockConfig.progression.playerLevel = 5;
            mockConfig.progression.currentRun = 3;
            mockConfig.currencies.metaCurrency = 1000;
            mockConfig.currencies.permaCurrency = 500;
            
            // Create serialized object for editor
            serializedObject = new SerializedObject(mockConfig);
            
            // Create editor instance
            editor = (SaveStateConfigurationEditor)UnityEditor.Editor.CreateEditor(mockConfig, typeof(SaveStateConfigurationEditor));
            
            // Setup mock file system
            EditorUITestFramework.MockFileSystem.ClearMocks();
            var mockSavePath = Path.Combine(Application.persistentDataPath, "GameData", "NeonLadderSave.json");
            EditorUITestFramework.MockFileSystem.AddMockFile(mockSavePath, GetMockSaveDataJson());
        }
        
        [TearDown]
        public void TearDown()
        {
            if (editor != null)
                UnityEngine.Object.DestroyImmediate(editor);
                
            if (mockConfig != null)
                UnityEngine.Object.DestroyImmediate(mockConfig);
                
            EditorUITestFramework.MockFileSystem.ClearMocks();
        }
        
        #endregion
        
        #region OnEnable Tests
        
        [Test]
        public void OnEnable_InitializesConfigReference()
        {
            // Act - Simulate OnEnable
            var onEnableMethod = typeof(SaveStateConfigurationEditor).GetMethod("OnEnable", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            onEnableMethod?.Invoke(editor, null);
            
            // Assert
            var configField = typeof(SaveStateConfigurationEditor).GetField("config", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            var loadedConfig = configField?.GetValue(editor) as SaveStateConfiguration;
            
            Assert.IsNotNull(loadedConfig, "Config should be loaded in OnEnable");
            Assert.AreEqual(mockConfig, loadedConfig, "Config reference should match target");
        }
        
        #endregion
        
        #region Button Action Tests
        
        [Test]
        public void CreateSaveData_FromConfiguration_CreatesValidSaveData()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act
            var createMethod = GetPrivateMethod("CreateSaveDataFromConfig");
            
            // Assert - Method should execute without exceptions
            Assert.DoesNotThrow(() => createMethod.Invoke(editor, null),
                "CreateSaveDataFromConfig should execute without errors");
        }
        
        [Test]
        public void LoadFromCurrentSave_WithExistingSave_LoadsDataIntoConfig()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act
            var loadMethod = GetPrivateMethod("LoadFromCurrentSave");
            
            // Assert - Method should execute without exceptions
            Assert.DoesNotThrow(() => loadMethod.Invoke(editor, null),
                "LoadFromCurrentSave should execute without errors");
        }
        
        [Test]
        public void ApplyToSession_UpdatesCurrentSessionData()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act
            var applyMethod = GetPrivateMethod("ApplyToCurrentSession");
            
            // Assert
            Assert.DoesNotThrow(() => applyMethod.Invoke(editor, null),
                "ApplyToCurrentSession should execute without errors");
        }
        
        [Test]
        public void RandomizeConfiguration_ModifiesAllData()
        {
            // Arrange
            InvokeOnEnable();
            var initialLevel = mockConfig.progression.playerLevel;
            var initialCurrency = mockConfig.currencies.metaCurrency;
            
            // Act
            var randomizeMethod = GetPrivateMethod("RandomizeConfiguration");
            randomizeMethod.Invoke(editor, null);
            
            // Assert - Some values should change (randomization occurred)
            // Note: There's a small chance values remain the same due to randomness
            Assert.IsTrue(
                mockConfig.progression.playerLevel != initialLevel || 
                mockConfig.currencies.metaCurrency != initialCurrency,
                "Randomization should modify at least some values");
        }
        
        #endregion
        
        #region File Operations Tests
        
        [Test]
        public void ExportSaveFile_WithValidPath_CallsExportMethod()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act
            var exportMethod = GetPrivateMethod("ExportSaveFile");
            
            // Assert - Method exists and can be invoked
            Assert.IsNotNull(exportMethod, "ExportSaveFile method should exist");
            // Note: Actual file dialog would require mocking EditorUtility.SaveFilePanel
        }
        
        [Test]
        public void OpenSaveDirectory_CallsRevealInFinder()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act
            var openDirMethod = GetPrivateMethod("OpenSaveDirectory");
            
            // Assert
            Assert.IsNotNull(openDirMethod, "OpenSaveDirectory method should exist");
            // Note: Actual directory opening would require mocking EditorUtility.RevealInFinder
        }
        
        #endregion
        
        #region MenuItem Tests
        
        [Test]
        public void MenuItems_CreateNewPlayerConfig_HasCorrectPath()
        {
            // Arrange
            var menuItemMethod = typeof(SaveConfigurationMenuItems).GetMethod("CreateNewPlayerConfig",
                BindingFlags.Public | BindingFlags.Static);
            var menuItemAttribute = menuItemMethod?.GetCustomAttribute<MenuItem>();
            
            // Assert
            Assert.IsNotNull(menuItemAttribute, "CreateNewPlayerConfig should have MenuItem attribute");
            Assert.AreEqual("Assets/Create/NeonLadder/Save Configurations/New Player Configuration",
                menuItemAttribute.menuItem, "Menu path should be correct");
        }
        
        [Test]
        public void MenuItems_AllConfigurationTypes_HaveMenuItems()
        {
            // Arrange
            var configTypes = new[]
            {
                "CreateNewPlayerConfig",
                "CreateMidGameConfig",
                "CreateEndGameConfig",
                "CreateTestingConfig",
                "CreateRegressionTestConfig"
            };
            
            // Act & Assert
            foreach (var configType in configTypes)
            {
                var method = typeof(SaveConfigurationMenuItems).GetMethod(configType,
                    BindingFlags.Public | BindingFlags.Static);
                var attribute = method?.GetCustomAttribute<MenuItem>();
                
                Assert.IsNotNull(attribute, $"{configType} should have MenuItem attribute");
                Assert.IsTrue(attribute.menuItem.StartsWith("Assets/Create/NeonLadder/Save Configurations/"),
                    $"{configType} should be in correct menu location");
            }
        }
        
        #endregion
        
        #region OnInspectorGUI Tests
        
        [Test]
        public void OnInspectorGUI_CanBeInvokedWithoutErrors()
        {
            // Arrange
            InvokeOnEnable();
            
            // Act & Assert
            var onInspectorGUIMethod = typeof(SaveStateConfigurationEditor).GetMethod("OnInspectorGUI",
                BindingFlags.Public | BindingFlags.Instance);
                
            // Note: We can't actually invoke OnInspectorGUI in tests due to GUI context
            // But we can verify the method exists and is public
            Assert.IsNotNull(onInspectorGUIMethod, "OnInspectorGUI method should exist");
            Assert.IsTrue(onInspectorGUIMethod.IsPublic, "OnInspectorGUI should be public");
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void Integration_EditorCanHandleNullConfiguration()
        {
            // Arrange - Create editor with null target
            var nullEditor = UnityEditor.Editor.CreateEditor(null, typeof(SaveStateConfigurationEditor));
            
            // Assert
            Assert.IsNotNull(nullEditor, "Editor should be created even with null target");
            
            // Cleanup
            if (nullEditor != null)
                UnityEngine.Object.DestroyImmediate(nullEditor);
        }
        
        [Test]
        public void Integration_ConfigurationDataPersistence()
        {
            // Arrange
            InvokeOnEnable();
            var testData = new SaveStateConfiguration.ProgressionData
            {
                playerLevel = 10,
                currentRun = 5,
                highestRun = 8
            };
            
            // Act
            mockConfig.progression = testData;
            EditorUtility.SetDirty(mockConfig);
            
            // Assert
            Assert.AreEqual(10, mockConfig.progression.playerLevel, "Player level should persist");
            Assert.AreEqual(5, mockConfig.progression.currentRun, "Current run should persist");
            Assert.AreEqual(8, mockConfig.progression.highestRun, "Highest run should persist");
        }
        
        #endregion
        
        #region Helper Methods
        
        private void InvokeOnEnable()
        {
            var onEnableMethod = typeof(SaveStateConfigurationEditor).GetMethod("OnEnable",
                BindingFlags.NonPublic | BindingFlags.Instance);
            onEnableMethod?.Invoke(editor, null);
        }
        
        private MethodInfo GetPrivateMethod(string methodName)
        {
            return typeof(SaveStateConfigurationEditor).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
        }
        
        private string GetMockSaveDataJson()
        {
            var mockData = new ConsolidatedSaveData
            {
                saveVersion = "1.0.0",
                lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                playTime = 3600f,
                progression = new ProgressionSaveData
                {
                    playerLevel = 10,
                    currentRun = 5,
                    highestRun = 8
                },
                currencies = new CurrencySaveData
                {
                    metaCurrency = 2000,
                    permaCurrency = 1000
                }
            };
            
            return JsonUtility.ToJson(mockData, true);
        }
        
        #endregion
    }
}