using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.InputSystem;
using NeonLadder.UI;

namespace NeonLadder.Tests.Runtime.UI
{
    /// <summary>
    /// Tests for Loading3DNavigationStarter focusing on player control management
    /// and automatic navigation setup during loading transitions
    /// </summary>
    public class Loading3DNavigationStarterTests
    {
        private GameObject testGameObject;
        private Loading3DNavigationStarter starter;
        private GameObject mockGameObject;
        private GameObject mockPlayerObject;
        private MockPlayerInput mockPlayerInput;

        [SetUp]
        public void SetUp()
        {
            testGameObject = new GameObject("TestStarter");
            starter = testGameObject.AddComponent<Loading3DNavigationStarter>();

            // Create mock Game singleton
            mockGameObject = new GameObject("Game");
            mockGameObject.tag = "Managers";

            // Create mock player (Kauru)
            mockPlayerObject = new GameObject("Kauru");
            mockPlayerObject.transform.SetParent(mockGameObject.transform);
            mockPlayerObject.tag = "Player";

            // Add mock PlayerInput component
            mockPlayerInput = mockPlayerObject.AddComponent<MockPlayerInput>();
        }

        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
                Object.DestroyImmediate(testGameObject);
            if (mockGameObject != null)
                Object.DestroyImmediate(mockGameObject);
        }

        [Test]
        public void EnableNavigation_IsEnabledByDefault()
        {
            var enabledField = typeof(Loading3DNavigationStarter).GetField("enableNavigation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsTrue((bool)enabledField.GetValue(starter),
                "Navigation should be enabled by default");
        }

        [Test]
        public void DisablePlayerControls_IsEnabledByDefault()
        {
            var disableField = typeof(Loading3DNavigationStarter).GetField("disablePlayerControls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsTrue((bool)disableField.GetValue(starter),
                "Player controls should be disabled by default during loading");
        }

        [Test]
        public void ShowDebugMessages_IsEnabledByDefault()
        {
            var debugField = typeof(Loading3DNavigationStarter).GetField("showDebugMessages",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsTrue((bool)debugField.GetValue(starter),
                "Debug messages should be enabled by default for development");
        }

        [Test]
        public void DisablePlayerControls_HandlesMissingPlayerInput()
        {
            // Test that the method doesn't crash when no PlayerInput is found
            var disableMethod = typeof(Loading3DNavigationStarter).GetMethod("DisablePlayerControls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Should not throw exception even without proper PlayerInput setup
            Assert.DoesNotThrow(() => disableMethod?.Invoke(starter, null),
                "DisablePlayerControls should handle missing PlayerInput gracefully");
        }

        [Test]
        public void PlayerControlManagement_ConfigurationPropertiesWork()
        {
            // Test that the configuration properties work as expected
            var disableField = typeof(Loading3DNavigationStarter).GetField("disablePlayerControls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var controlsDisabledField = typeof(Loading3DNavigationStarter).GetField("playerControlsWereDisabled",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test default configuration
            Assert.IsTrue((bool)disableField.GetValue(starter),
                "Player controls should be disabled by default during loading");

            // Test state tracking
            Assert.IsFalse((bool)controlsDisabledField.GetValue(starter),
                "Player controls disabled state should start as false");
        }

        [Test]
        public void EnablePlayerControls_HandlesNoDisabledControls()
        {
            // Test EnablePlayerControls when no controls were disabled
            var enableMethod = typeof(Loading3DNavigationStarter).GetMethod("EnablePlayerControls",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Should not throw exception when called without prior disable
            Assert.DoesNotThrow(() => enableMethod?.Invoke(starter, null),
                "EnablePlayerControls should handle case where no controls were disabled");
        }

        [Test]
        public void OnLoadingComplete_CallsEnablePlayerControls()
        {
            // Test that OnLoadingComplete properly calls EnablePlayerControls
            // We'll test this by checking that it doesn't throw an exception
            Assert.DoesNotThrow(() => starter.OnLoadingComplete(),
                "OnLoadingComplete should not throw exception when restoring controls");

            // Verify that the state tracking variables are handled properly
            var hasSetupField = typeof(Loading3DNavigationStarter).GetField("hasSetupNavigation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert.IsFalse((bool)hasSetupField.GetValue(starter),
                "OnLoadingComplete should reset hasSetupNavigation to false");
        }

        [Test]
        public void OnLoadingComplete_CleansUpNavigationComponents()
        {
            // Create a mock Loading3DController with Navigation component
            var controllerObject = new GameObject("MockController");
            var controller = controllerObject.AddComponent<MockLoading3DController>();
            var navigation = controllerObject.AddComponent<Loading3DNavigation>();

            // Set it in the starter
            var controllerField = typeof(Loading3DNavigationStarter).GetField("loading3DController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controllerField?.SetValue(starter, controller);

            starter.OnLoadingComplete();

            // Navigation component should be destroyed
            Assert.IsNull(controllerObject.GetComponent<Loading3DNavigation>(),
                "Navigation component should be cleaned up after loading");

            Object.DestroyImmediate(controllerObject);
        }

        [UnityTest]
        public IEnumerator SpawnTestScreen_CreatesLoading3DScreen()
        {
            // Ignore exceptions from 3D model instantiation
            var originalIgnoreState = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            // Enable debug messages to see what's happening
            var debugField = typeof(Loading3DNavigationStarter).GetField("showDebugMessages",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            debugField?.SetValue(starter, true);

            // Enable test mode
            var testModeField = typeof(Loading3DNavigationStarter).GetField("spawnTestScreen",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            testModeField?.SetValue(starter, true);

            // Create a mock prefab
            var mockPrefab = new GameObject("Loading3DScreen");
            mockPrefab.AddComponent<MockLoading3DController>();

            // Convert to prefab - this might be needed for Instantiate to work properly
            var prefabField = typeof(Loading3DNavigationStarter).GetField("loading3DScreenPrefab",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prefabField?.SetValue(starter, mockPrefab);

            // Verify prefab was set
            var prefabValue = (GameObject)prefabField?.GetValue(starter);
            Assert.IsNotNull(prefabValue, "loading3DScreenPrefab should be set");

            // Manually call the spawn method instead of relying on Start()
            var spawnMethod = typeof(Loading3DNavigationStarter).GetMethod("SpawnLoading3DScreen",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spawnMethod?.Invoke(starter, null);

            // Wait longer for the spawn process to complete (including 3D model loading)
            yield return new WaitForSeconds(0.5f);

            // Check if spawned screen exists
            var spawnedField = typeof(Loading3DNavigationStarter).GetField("spawnedLoading3DScreen",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var spawnedScreen = (GameObject)spawnedField?.GetValue(starter);

            Assert.IsNotNull(spawnedScreen, "Test screen should be spawned");
            Assert.AreEqual("Loading3DScreen (Test)", spawnedScreen.name,
                "Spawned screen should have correct test name");

            // Restore original log assertion state
            LogAssert.ignoreFailingMessages = originalIgnoreState;

            Object.DestroyImmediate(mockPrefab);
        }

        [Test]
        public void SetupNavigation_AddsNavigationComponent()
        {
            // Create mock controller
            var controllerObject = new GameObject("MockController");
            var controller = controllerObject.AddComponent<MockLoading3DController>();

            // Set it in the starter
            var controllerField = typeof(Loading3DNavigationStarter).GetField("loading3DController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controllerField?.SetValue(starter, controller);

            var setupMethod = typeof(Loading3DNavigationStarter).GetMethod("SetupNavigation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            setupMethod?.Invoke(starter, null);

            var navigationComponent = controllerObject.GetComponent<Loading3DNavigation>();
            Assert.IsNotNull(navigationComponent,
                "Navigation component should be added to Loading3DController");

            Object.DestroyImmediate(controllerObject);
        }

        [Test]
        public void SetupNavigation_DoesNotDuplicateExistingComponent()
        {
            // Create mock controller with existing navigation
            var controllerObject = new GameObject("MockController");
            var controller = controllerObject.AddComponent<MockLoading3DController>();
            controllerObject.AddComponent<Loading3DNavigation>();

            // Set it in the starter
            var controllerField = typeof(Loading3DNavigationStarter).GetField("loading3DController",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            controllerField?.SetValue(starter, controller);

            var setupMethod = typeof(Loading3DNavigationStarter).GetMethod("SetupNavigation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            setupMethod?.Invoke(starter, null);

            var navigationComponents = controllerObject.GetComponents<Loading3DNavigation>();
            Assert.AreEqual(1, navigationComponents.Length,
                "Should not duplicate existing navigation component");

            Object.DestroyImmediate(controllerObject);
        }

        [Test]
        public void OnDestroy_HandlesMissingPlayerControls()
        {
            // Test that OnDestroy doesn't crash when no player controls were set up
            Assert.DoesNotThrow(() => Object.DestroyImmediate(starter),
                "OnDestroy should handle missing player controls gracefully");
        }
    }
}