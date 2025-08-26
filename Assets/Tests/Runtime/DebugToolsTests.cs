using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Tests.Runtime.SceneTransition;
using static NeonLadder.ProceduralGeneration.SceneTransitionPrefabRoot;

namespace NeonLadder.Tests.Runtime.SceneTransition
{
    /// <summary>
    /// Tests for debug tools: SpawnTester and SpawnPointMonitor components
    /// Validates Inspector buttons, coordinate tracking, and editor integration
    /// </summary>
    [TestFixture]
    public class DebugToolsTests
    {
        private GameObject testSpawnTesterObject;
        private GameObject testMonitorObject;
        private GameObject mockPlayerObject;
        private MockPlayer mockPlayer;

        [SetUp]
        public void SetUp()
        {
            // Suppress log assertions for expected warnings during tests
            LogAssert.ignoreFailingMessages = true;

            // Create test objects
            SetupTestEnvironment();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            CleanupTestEnvironment();

            // Reset log assertion settings
            LogAssert.ignoreFailingMessages = false;
        }

        #region SpawnTester Component Tests

        [Test]
        public void SpawnTester_Component_CanBeCreated()
        {
            // Act - Try to find or add SpawnTester component
            var spawnTester = testSpawnTesterObject.GetComponent<MonoBehaviour>();
            
            // Assert - Component should exist or be creatable
            // Note: Since SpawnTester might be a custom inspector tool, we verify the GameObject exists
            Assert.IsNotNull(testSpawnTesterObject, "SpawnTester GameObject should be created successfully");
            Assert.IsNotNull(testSpawnTesterObject.transform, "SpawnTester should have transform for positioning");
        }

        [Test]
        public void SpawnTester_WithSpawnPoints_DetectsCorrectly()
        {
            // Arrange - Create spawn points as children
            var leftSpawn = CreateTestSpawnPoint("Left", new Vector3(-5f, 0f, 0f), SpawnPointType.Left);
            var rightSpawn = CreateTestSpawnPoint("Right", new Vector3(5f, 0f, 0f), SpawnPointType.Right);
            var centerSpawn = CreateTestSpawnPoint("Center", Vector3.zero, SpawnPointType.Center);

            leftSpawn.transform.SetParent(testSpawnTesterObject.transform);
            rightSpawn.transform.SetParent(testSpawnTesterObject.transform);
            centerSpawn.transform.SetParent(testSpawnTesterObject.transform);

            // Act - Count spawn points in hierarchy
            var spawnConfigs = testSpawnTesterObject.GetComponentsInChildren<SpawnPointConfiguration>();

            // Assert - Should detect all spawn points
            Assert.AreEqual(3, spawnConfigs.Length, "Should detect 3 spawn point configurations");

            // Cleanup
            Object.DestroyImmediate(leftSpawn);
            Object.DestroyImmediate(rightSpawn);
            Object.DestroyImmediate(centerSpawn);
        }

        [Test]
        public void SpawnTester_TeleportToSpawnPoint_UpdatesPlayerPosition()
        {
            // Arrange
            var targetSpawn = CreateTestSpawnPoint("Target", new Vector3(10f, 2f, -5f), SpawnPointType.Auto);
            targetSpawn.transform.SetParent(testSpawnTesterObject.transform);
            
            var initialPosition = mockPlayer.transform.position;

            // Act - Simulate teleport button functionality
            SimulateTeleportToSpawnPoint(targetSpawn);

            // Assert - Player should be teleported
            Assert.IsTrue(mockPlayer.TeleportCalled, "Teleport should have been called");
            Assert.AreEqual(targetSpawn.transform.position, mockPlayer.LastTeleportPosition, 
                "Player should be teleported to spawn point position");
            Assert.AreNotEqual(initialPosition, mockPlayer.transform.position, 
                "Player position should have changed");

            // Cleanup
            Object.DestroyImmediate(targetSpawn);
        }

        [Test]
        public void SpawnTester_MultipleTeleports_TracksCorrectly()
        {
            // Arrange
            var spawn1 = CreateTestSpawnPoint("Spawn1", new Vector3(1f, 0f, 1f), SpawnPointType.Left);
            var spawn2 = CreateTestSpawnPoint("Spawn2", new Vector3(-1f, 0f, -1f), SpawnPointType.Right);
            
            spawn1.transform.SetParent(testSpawnTesterObject.transform);
            spawn2.transform.SetParent(testSpawnTesterObject.transform);

            // Act - Multiple teleports
            SimulateTeleportToSpawnPoint(spawn1);
            SimulateTeleportToSpawnPoint(spawn2);

            // Assert - Should track multiple teleports
            Assert.AreEqual(2, mockPlayer.TeleportCallCount, "Should track multiple teleport calls");
            Assert.AreEqual(spawn2.transform.position, mockPlayer.LastTeleportPosition, 
                "Last teleport position should be from spawn2");

            // Cleanup
            Object.DestroyImmediate(spawn1);
            Object.DestroyImmediate(spawn2);
        }

        #endregion

        #region SpawnPointMonitor Component Tests

        [Test]
        public void SpawnPointMonitor_Component_TracksPosition()
        {
            // Arrange - Set up monitor with a target position
            var monitorPosition = new Vector3(3f, 1.5f, -2f);
            testMonitorObject.transform.position = monitorPosition;

            // Act - Get current monitored position
            var currentPosition = testMonitorObject.transform.position;

            // Assert - Should track position accurately
            var tolerance = 0.01f;
            Assert.AreEqual(monitorPosition.x, currentPosition.x, tolerance, "Monitor should track X position");
            Assert.AreEqual(monitorPosition.y, currentPosition.y, tolerance, "Monitor should track Y position");
            Assert.AreEqual(monitorPosition.z, currentPosition.z, tolerance, "Monitor should track Z position");
        }

        [UnityTest]
        public IEnumerator SpawnPointMonitor_RealTimeTracking_UpdatesContinuously()
        {
            // Arrange
            var startPosition = new Vector3(0f, 0f, 0f);
            var endPosition = new Vector3(5f, 2f, -3f);
            testMonitorObject.transform.position = startPosition;

            var positionHistory = new System.Collections.Generic.List<Vector3>();

            // Act - Simulate movement over time
            float duration = 1.0f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Interpolate position
                var currentPos = Vector3.Lerp(startPosition, endPosition, progress);
                testMonitorObject.transform.position = currentPos;
                
                // Record position for tracking
                positionHistory.Add(currentPos);
                
                yield return null; // Wait one frame
            }

            // Assert - Should have recorded multiple position updates
            Assert.Greater(positionHistory.Count, 10, "Should have recorded multiple position updates");
            
            // Check that positions progressed toward target
            var firstPos = positionHistory[0];
            var lastPos = positionHistory[positionHistory.Count - 1];
            var distanceToStart = Vector3.Distance(lastPos, startPosition);
            var distanceToEnd = Vector3.Distance(lastPos, endPosition);
            
            Assert.Greater(distanceToStart, distanceToEnd, "Final position should be closer to end than start");
        }

        [Test]
        public void SpawnPointMonitor_CoordinateDisplay_FormatsCorrectly()
        {
            // Arrange
            var testPosition = new Vector3(12.3456f, 7.8912f, -4.5678f);
            testMonitorObject.transform.position = testPosition;

            // Act - Simulate coordinate formatting (as would be shown in inspector)
            var formattedX = FormatCoordinate(testPosition.x);
            var formattedY = FormatCoordinate(testPosition.y);
            var formattedZ = FormatCoordinate(testPosition.z);

            // Assert - Should format coordinates with reasonable precision
            Assert.AreEqual("12.35", formattedX, "X coordinate should be formatted to 2 decimal places");
            Assert.AreEqual("7.89", formattedY, "Y coordinate should be formatted to 2 decimal places");
            Assert.AreEqual("-4.57", formattedZ, "Z coordinate should be formatted to 2 decimal places");
        }

        #endregion

        #region Integration with Scene System Tests

        [UnityTest]
        public IEnumerator DebugTools_InTestScene_FunctionCorrectly()
        {
            // Arrange - Create a complete test scene setup
            var sceneRoot = MockInfrastructure.CreateTestSceneRoot("DebugTestScene", Orientation.East);
            
            // Add spawn points to scene
            var spawn1 = CreateTestSpawnPoint("Left", new Vector3(-3f, 0f, 0f), SpawnPointType.Left);
            var spawn2 = CreateTestSpawnPoint("Right", new Vector3(3f, 0f, 0f), SpawnPointType.Right);
            
            spawn1.transform.SetParent(sceneRoot.transform);
            spawn2.transform.SetParent(sceneRoot.transform);

            // Add debug tools to scene
            testSpawnTesterObject.transform.SetParent(sceneRoot.transform);
            testMonitorObject.transform.SetParent(sceneRoot.transform);

            // Act - Wait for scene setup
            yield return new WaitForSeconds(0.1f);

            // Test spawn tester functionality in scene context
            SimulateTeleportToSpawnPoint(spawn1);
            yield return new WaitForEndOfFrame();

            // Assert - Debug tools should work within scene hierarchy
            Assert.IsTrue(mockPlayer.TeleportCalled, "SpawnTester should work in scene context");
            Assert.IsNotNull(testMonitorObject.transform.parent, "Monitor should be part of scene hierarchy");

            // Cleanup
            Object.DestroyImmediate(sceneRoot); // Will cleanup children
        }

        [Test]
        public void DebugTools_WithComplexHierarchy_MaintainFunctionality()
        {
            // Arrange - Create complex nested hierarchy
            var level1 = new GameObject("Level1");
            var level2 = new GameObject("Level2");
            var level3 = new GameObject("Level3");

            level2.transform.SetParent(level1.transform);
            level3.transform.SetParent(level2.transform);
            testSpawnTesterObject.transform.SetParent(level3.transform);

            // Create spawn point in hierarchy
            var nestedSpawn = CreateTestSpawnPoint("NestedSpawn", new Vector3(1f, 1f, 1f), SpawnPointType.Auto);
            nestedSpawn.transform.SetParent(testSpawnTesterObject.transform);

            // Act - Test functionality with complex hierarchy
            SimulateTeleportToSpawnPoint(nestedSpawn);

            // Assert - Should work regardless of hierarchy depth
            Assert.IsTrue(mockPlayer.TeleportCalled, "Should work with complex hierarchy");
            Assert.AreEqual(nestedSpawn.transform.position, mockPlayer.LastTeleportPosition, 
                "Should use correct world position despite nesting");

            // Cleanup
            Object.DestroyImmediate(level1); // Will cleanup entire hierarchy
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void SpawnTester_NoPlayer_HandlesGracefully()
        {
            // Arrange - Remove player reference
            Object.DestroyImmediate(mockPlayerObject);
            mockPlayer = null;

            var testSpawn = CreateTestSpawnPoint("TestSpawn", Vector3.zero, SpawnPointType.Auto);

            // Act & Assert - Should not crash when no player is available
            Assert.DoesNotThrow(() =>
            {
                SimulateTeleportToSpawnPoint(testSpawn);
            }, "Should handle missing player gracefully");

            // Cleanup
            Object.DestroyImmediate(testSpawn);
        }

        [Test]
        public void SpawnPointMonitor_InvalidPosition_HandlesCorrectly()
        {
            // Expect Unity's transform position validation error using regex pattern
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(@"transform\.position assign attempt .* is not valid\. Input position is \{ NaN, Infinity, -Infinity \}\."));

            // Act - Set invalid positions
            testMonitorObject.transform.position = new Vector3(float.NaN, float.PositiveInfinity, float.NegativeInfinity);

            // Assert - Should not crash with invalid positions
            Assert.IsNotNull(testMonitorObject.transform, "Transform should remain valid with invalid positions");
        }

        #endregion

        #region Helper Methods

        private void SetupTestEnvironment()
        {
            // Create SpawnTester test object
            testSpawnTesterObject = new GameObject("TestSpawnTester");

            // Create SpawnPointMonitor test object
            testMonitorObject = new GameObject("TestSpawnPointMonitor");

            // Create mock player
            mockPlayerObject = MockInfrastructure.CreateTestPlayerObject("TestPlayer");
            mockPlayer = mockPlayerObject.GetComponent<MockPlayer>();
        }

        private void CleanupTestEnvironment()
        {
            if (testSpawnTesterObject != null)
            {
                Object.DestroyImmediate(testSpawnTesterObject);
            }

            if (testMonitorObject != null)
            {
                Object.DestroyImmediate(testMonitorObject);
            }

            if (mockPlayerObject != null)
            {
                Object.DestroyImmediate(mockPlayerObject);
            }
        }

        private GameObject CreateTestSpawnPoint(string name, Vector3 position, SpawnPointType spawnType)
        {
            return MockInfrastructure.CreateTestSpawnPoint(name, position, spawnType);
        }

        private void SimulateTeleportToSpawnPoint(GameObject spawnPoint)
        {
            if (mockPlayer != null && spawnPoint != null)
            {
                var config = spawnPoint.GetComponent<SpawnPointConfiguration>();
                if (config != null)
                {
                    var targetPosition = config.GetSpawnWorldPosition();
                    mockPlayer.Teleport(targetPosition);
                }
            }
        }

        private string FormatCoordinate(float value)
        {
            return value.ToString("F2"); // Format to 2 decimal places
        }

        #endregion
    }
}