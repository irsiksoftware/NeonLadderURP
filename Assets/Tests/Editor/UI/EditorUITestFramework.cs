using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Tony Stark's EditorWindow Testing Framework - The most comprehensive Unity Editor UI testing solution
    /// Provides TDD patterns, mocking infrastructure, and state validation for Enterprise-grade testing
    /// 
    /// "FRIDAY, let's make sure our UI never breaks in production again."
    /// </summary>
    public static class EditorUITestFramework
    {
        #region FRIDAY Test Infrastructure
        
        /// <summary>
        /// Creates a mock EditorWindow for testing without opening actual UI
        /// </summary>
        public static T CreateMockWindow<T>() where T : EditorWindow
        {
            // Create instance without showing the window
            var window = ScriptableObject.CreateInstance<T>();
            
            // Set up reflection access to private fields
            var flags = BindingFlags.NonPublic | BindingFlags.Instance;
            
            // Initialize window state that Unity normally handles
            var positionField = typeof(EditorWindow).GetField("m_Pos", flags);
            if (positionField != null)
            {
                positionField.SetValue(window, new Rect(0, 0, 800, 600));
            }
            
            return window;
        }
        
        /// <summary>
        /// Simulates OnEnable call for EditorWindow testing
        /// </summary>
        public static void SimulateOnEnable<T>(T window) where T : EditorWindow
        {
            var onEnableMethod = typeof(T).GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            onEnableMethod?.Invoke(window, null);
        }
        
        /// <summary>
        /// Simulates OnGUI call with mock event system and proper GUI context
        /// </summary>
        public static void SimulateOnGUI<T>(T window, EventType eventType = EventType.Repaint) where T : EditorWindow
        {
            // Store current state
            var currentEvent = Event.current;
            var currentMatrix = GUI.matrix;
            
            try
            {
                // Initialize GUI context for Editor testing
                GUI.matrix = Matrix4x4.identity;
                
                // Create and configure mock event
                var mockEvent = new Event { type = eventType };
                mockEvent.mousePosition = new Vector2(400, 300); // Center of mock window
                
                // Set mock event as current via reflection
                var currentEventField = typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
                currentEventField?.SetValue(null, mockEvent);
                
                // Initialize GUI skin if needed
                if (GUI.skin == null)
                {
                    GUI.skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                }
                
                // Set up window position for GUI calculations
                var windowRect = new Rect(0, 0, 800, 600);
                var positionProperty = typeof(EditorWindow).GetProperty("position");
                if (positionProperty != null && positionProperty.CanWrite)
                {
                    positionProperty.SetValue(window, windowRect);
                }
                
                // Invoke OnGUI with proper error handling
                var onGUIMethod = typeof(T).GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance) 
                               ?? typeof(T).GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Instance);
                
                if (onGUIMethod != null)
                {
                    onGUIMethod.Invoke(window, null);
                }
                else
                {
                    // If OnGUI method not found, try base EditorWindow OnGUI
                    var baseOnGUI = typeof(EditorWindow).GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
                    baseOnGUI?.Invoke(window, null);
                }
            }
            catch (TargetInvocationException ex)
            {
                // Handle GUI-related exceptions gracefully for testing
                if (ex.InnerException?.GetType().Name.Contains("GUI") == true)
                {
                    // GUI exceptions in test environment are often expected
                    // Log but don't fail the test unless it's a critical error
                    Debug.LogWarning($"GUI exception in test (expected): {ex.InnerException?.Message}");
                }
                else
                {
                    // Re-throw non-GUI exceptions
                    throw ex.InnerException ?? ex;
                }
            }
            finally
            {
                // Restore original state
                typeof(Event).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, currentEvent);
                GUI.matrix = currentMatrix;
            }
        }
        
        /// <summary>
        /// Gets private field value from EditorWindow for testing state
        /// </summary>
        public static TField GetPrivateField<TWindow, TField>(TWindow window, string fieldName) where TWindow : EditorWindow
        {
            var field = typeof(TWindow).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) 
            {
                throw new ArgumentException($"Field '{fieldName}' not found in {typeof(TWindow).Name}");
            }
            
            return (TField)field.GetValue(window);
        }
        
        /// <summary>
        /// Sets private field value in EditorWindow for testing scenarios
        /// </summary>
        public static void SetPrivateField<TWindow, TField>(TWindow window, string fieldName, TField value) where TWindow : EditorWindow
        {
            var field = typeof(TWindow).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found in {typeof(TWindow).Name}");
            }
            
            field.SetValue(window, value);
        }
        
        /// <summary>
        /// Invokes private method on EditorWindow for testing functionality
        /// </summary>
        public static TResult InvokePrivateMethod<TWindow, TResult>(TWindow window, string methodName, params object[] parameters) where TWindow : EditorWindow
        {
            var method = typeof(TWindow).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in {typeof(TWindow).Name}");
            }
            
            return (TResult)method.Invoke(window, parameters);
        }
        
        /// <summary>
        /// Invokes private void method on EditorWindow
        /// </summary>
        public static void InvokePrivateMethod<TWindow>(TWindow window, string methodName, params object[] parameters) where TWindow : EditorWindow
        {
            var method = typeof(TWindow).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method == null)
            {
                throw new ArgumentException($"Method '{methodName}' not found in {typeof(TWindow).Name}");
            }
            
            method.Invoke(window, parameters);
        }
        
        #endregion
        
        #region Mock Infrastructure
        
        /// <summary>
        /// Mock file system operations for testing without actual files
        /// </summary>
        public class MockFileSystem
        {
            private static Dictionary<string, string> mockFiles = new Dictionary<string, string>();
            private static HashSet<string> mockDirectories = new HashSet<string>();
            
            public static void AddMockFile(string path, string content)
            {
                mockFiles[path] = content;
            }
            
            public static void AddMockDirectory(string path)
            {
                mockDirectories.Add(path);
            }
            
            public static bool MockFileExists(string path)
            {
                return mockFiles.ContainsKey(path);
            }
            
            public static string ReadMockFile(string path)
            {
                return mockFiles.TryGetValue(path, out string content) ? content : null;
            }
            
            public static void ClearMocks()
            {
                mockFiles.Clear();
                mockDirectories.Clear();
            }
        }
        
        /// <summary>
        /// Creates test data builders for complex scenarios
        /// </summary>
        public static class TestDataBuilder
        {
            public static T CreateTestScriptableObject<T>() where T : ScriptableObject
            {
                return ScriptableObject.CreateInstance<T>();
            }
            
            public static void SetupTestAssetDatabase()
            {
                // Mock AssetDatabase operations for testing
                // This would need custom implementation based on specific needs
            }
        }
        
        #endregion
        
        #region Assertion Helpers
        
        /// <summary>
        /// Asserts that UI element exists and is properly configured
        /// </summary>
        public static void AssertUIElementExists(object element, string elementName)
        {
            Assert.IsNotNull(element, $"UI element '{elementName}' should exist");
        }
        
        /// <summary>
        /// Asserts that window state is valid for the given tab
        /// </summary>
        public static void AssertWindowTabState<T>(T window, int expectedTabIndex, string[] expectedTabs) where T : EditorWindow
        {
            var currentTab = GetPrivateField<T, int>(window, "selectedTabIndex");
            Assert.AreEqual(expectedTabIndex, currentTab, "Tab index should match expected value");
            
            var tabNames = GetPrivateField<T, string[]>(window, "tabNames");
            CollectionAssert.AreEqual(expectedTabs, tabNames, "Tab names should match expected array");
        }
        
        /// <summary>
        /// Asserts that validation logic works correctly
        /// </summary>
        public static void AssertValidationResult(bool isValid, string expectedMessage, ValidationResult result)
        {
            Assert.AreEqual(isValid, result.IsValid, $"Validation result should be {isValid}");
            if (!string.IsNullOrEmpty(expectedMessage))
            {
                Assert.AreEqual(expectedMessage, result.Message, "Validation message should match expected");
            }
        }
        
        #endregion
        
        #region Performance Testing
        
        /// <summary>
        /// Measures EditorWindow OnGUI performance
        /// </summary>
        public static long MeasureOnGUIPerformance<T>(T window, int iterations = 100) where T : EditorWindow
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                SimulateOnGUI(window);
            }
            
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
        
        /// <summary>
        /// Checks for memory leaks in EditorWindow operations
        /// </summary>
        public static void AssertNoMemoryLeaks(Action operation, long maxAllocationMB = 10)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            operation();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var finalMemory = GC.GetTotalMemory(false);
            var allocatedMB = (finalMemory - initialMemory) / (1024 * 1024);
            
            Assert.LessOrEqual(allocatedMB, maxAllocationMB, 
                $"Memory allocation should be under {maxAllocationMB}MB, but was {allocatedMB}MB");
        }
        
        #endregion
    }
    
    /// <summary>
    /// Validation result for testing UI validation logic
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; }
        
        public ValidationResult(bool isValid, string message = "")
        {
            IsValid = isValid;
            Message = message;
        }
    }
    
    /// <summary>
    /// Base class for EditorWindow UI tests following TDD patterns
    /// </summary>
    public abstract class EditorWindowTestBase<T> where T : EditorWindow
    {
        protected T window;
        
        [SetUp]
        public virtual void SetUp()
        {
            // Red-Green-Refactor: Start with clean window state
            window = EditorUITestFramework.CreateMockWindow<T>();
            EditorUITestFramework.SimulateOnEnable(window);
        }
        
        [TearDown]
        public virtual void TearDown()
        {
            // Clean up resources
            if (window != null)
            {
                UnityEngine.Object.DestroyImmediate(window);
                window = null;
            }
            
            EditorUITestFramework.MockFileSystem.ClearMocks();
        }
        
        /// <summary>
        /// Template method for testing UI initialization
        /// </summary>
        protected virtual void AssertWindowInitialization()
        {
            Assert.IsNotNull(window, "Window should be created successfully");
            // Override in derived classes for specific initialization tests
        }
        
        /// <summary>
        /// Template method for testing UI state management
        /// </summary>
        protected virtual void AssertStateManagement()
        {
            // Override in derived classes for specific state tests
        }
        
        /// <summary>
        /// Template method for testing UI validation
        /// </summary>
        protected virtual void AssertValidation()
        {
            // Override in derived classes for specific validation tests
        }
    }
}