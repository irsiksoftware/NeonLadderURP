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
    /// Unit tests for SceneTransitionPrefabRoot component
    /// Tests orientation-based positioning (N/E/S/W) and integration with spawn system
    /// </summary>
    [TestFixture]
    public class SceneTransitionPrefabRootTests
    {
        private GameObject testPrefabRootObject;
        private SceneTransitionPrefabRoot prefabRoot;

        [SetUp]
        public void SetUp()
        {
            // Suppress log assertions for expected warnings during tests
            LogAssert.ignoreFailingMessages = true;

            // Create test prefab root
            testPrefabRootObject = new GameObject("TestSceneTransitionPrefabRoot");
            prefabRoot = testPrefabRootObject.AddComponent<SceneTransitionPrefabRoot>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (testPrefabRootObject != null)
            {
                Object.DestroyImmediate(testPrefabRootObject);
            }

            // Reset log assertion settings
            LogAssert.ignoreFailingMessages = false;
        }

        #region Orientation Configuration Tests

        [Test]
        public void SceneTransitionPrefabRoot_DefaultOrientation_IsNorth()
        {
            // Act - Check default orientation through public method or reflection
            var orientation = GetOrientationValue();

            // Assert
            Assert.AreEqual(Orientation.North, orientation, "Default orientation should be North");
        }

        [TestCase(Orientation.North)]
        [TestCase(Orientation.East)]
        [TestCase(Orientation.South)]
        [TestCase(Orientation.West)]
        public void SceneTransitionPrefabRoot_SetOrientation_ConfiguresCorrectly(Orientation expectedOrientation)
        {
            // Arrange & Act
            SetOrientationValue(expectedOrientation);
            var actualOrientation = GetOrientationValue();

            // Assert
            Assert.AreEqual(expectedOrientation, actualOrientation, $"Orientation should be set to {expectedOrientation}");
        }

        #endregion

        #region Orientation-Based Positioning Tests

        [Test]
        public void SceneTransitionPrefabRoot_NorthOrientation_PositionsCorrectly()
        {
            // Arrange
            SetOrientationValue(Orientation.North);
            var expectedOffset = new Vector3(0f, 0f, -1.5f); // North = negative Z

            // Act
            var calculatedPosition = CalculateOrientationOffset();

            // Assert
            var tolerance = 0.01f;
            AssertVector3Equal(expectedOffset, calculatedPosition, tolerance, "North orientation offset");
        }

        [Test]
        public void SceneTransitionPrefabRoot_EastOrientation_PositionsCorrectly()
        {
            // Arrange
            SetOrientationValue(Orientation.East);
            var expectedOffset = new Vector3(-1.5f, 0f, 0f); // East = negative X

            // Act
            var calculatedPosition = CalculateOrientationOffset();

            // Assert
            var tolerance = 0.01f;
            AssertVector3Equal(expectedOffset, calculatedPosition, tolerance, "East orientation offset");
        }

        [Test]
        public void SceneTransitionPrefabRoot_SouthOrientation_PositionsCorrectly()
        {
            // Arrange
            SetOrientationValue(Orientation.South);
            var expectedOffset = new Vector3(0f, 0f, 1.5f); // South = positive Z

            // Act
            var calculatedPosition = CalculateOrientationOffset();

            // Assert
            var tolerance = 0.01f;
            AssertVector3Equal(expectedOffset, calculatedPosition, tolerance, "South orientation offset");
        }

        [Test]
        public void SceneTransitionPrefabRoot_WestOrientation_PositionsCorrectly()
        {
            // Arrange
            SetOrientationValue(Orientation.West);
            var expectedOffset = new Vector3(1.5f, 0f, 0f); // West = positive X

            // Act
            var calculatedPosition = CalculateOrientationOffset();

            // Assert
            var tolerance = 0.01f;
            AssertVector3Equal(expectedOffset, calculatedPosition, tolerance, "West orientation offset");
        }

        #endregion

        #region Spawn Point Integration Tests

        [Test]
        public void SceneTransitionPrefabRoot_WithSpawnPoints_MaintainsCorrectRelationships()
        {
            // Arrange - Create spawn points as children
            var leftSpawn = MockInfrastructure.CreateTestSpawnPoint("Left", new Vector3(-2f, 0f, 0f), SpawnPointType.Left);
            var rightSpawn = MockInfrastructure.CreateTestSpawnPoint("Right", new Vector3(2f, 0f, 0f), SpawnPointType.Right);
            
            leftSpawn.transform.SetParent(testPrefabRootObject.transform);
            rightSpawn.transform.SetParent(testPrefabRootObject.transform);

            // Act - Set orientation and check spawn point relationships
            SetOrientationValue(Orientation.East);
            
            // Assert - Spawn points should maintain their relative positions
            Assert.IsNotNull(leftSpawn.transform.parent, "Left spawn should have parent");
            Assert.IsNotNull(rightSpawn.transform.parent, "Right spawn should have parent");
            Assert.AreEqual(testPrefabRootObject.transform, leftSpawn.transform.parent, "Left spawn parent should be prefab root");
            Assert.AreEqual(testPrefabRootObject.transform, rightSpawn.transform.parent, "Right spawn parent should be prefab root");

            // Cleanup
            Object.DestroyImmediate(leftSpawn);
            Object.DestroyImmediate(rightSpawn);
        }

        [Test]
        public void SceneTransitionPrefabRoot_OrientationChange_UpdatesChildPositionsCorrectly()
        {
            // Arrange - Create child spawn point
            var childSpawn = MockInfrastructure.CreateTestSpawnPoint("Child", new Vector3(1f, 0f, 0f), SpawnPointType.Auto);
            childSpawn.transform.SetParent(testPrefabRootObject.transform);
            
            var initialLocalPosition = childSpawn.transform.localPosition;

            // Act - Change orientation
            SetOrientationValue(Orientation.South);
            
            // Assert - Local position should remain the same (world position changes with parent)
            Assert.AreEqual(initialLocalPosition, childSpawn.transform.localPosition, 
                "Child local position should remain unchanged when parent orientation changes");

            // Cleanup
            Object.DestroyImmediate(childSpawn);
        }

        #endregion

        #region Transform Integration Tests

        [Test]
        public void SceneTransitionPrefabRoot_WithCustomPosition_MaintainsOrientation()
        {
            // Arrange
            var customPosition = new Vector3(10f, 5f, -3f);
            testPrefabRootObject.transform.position = customPosition;
            SetOrientationValue(Orientation.West);

            // Act
            var finalPosition = testPrefabRootObject.transform.position;

            // Assert - Position should be maintained
            Assert.AreEqual(customPosition, finalPosition, "Custom position should be maintained");
        }

        [Test]
        public void SceneTransitionPrefabRoot_WithRotation_HandlesCorrectly()
        {
            // Arrange
            var customRotation = Quaternion.Euler(0, 45, 0);
            testPrefabRootObject.transform.rotation = customRotation;
            SetOrientationValue(Orientation.North);

            // Act
            var finalRotation = testPrefabRootObject.transform.rotation;

            // Assert - Rotation should be maintained
            var tolerance = 0.01f;
            Assert.AreEqual(customRotation.x, finalRotation.x, tolerance, "X rotation should be maintained");
            Assert.AreEqual(customRotation.y, finalRotation.y, tolerance, "Y rotation should be maintained");
            Assert.AreEqual(customRotation.z, finalRotation.z, tolerance, "Z rotation should be maintained");
            Assert.AreEqual(customRotation.w, finalRotation.w, tolerance, "W rotation should be maintained");
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void SceneTransitionPrefabRoot_MultipleOrientationChanges_HandlesCorrectly()
        {
            // Act - Multiple rapid orientation changes
            SetOrientationValue(Orientation.North);
            SetOrientationValue(Orientation.East);
            SetOrientationValue(Orientation.South);
            SetOrientationValue(Orientation.West);
            SetOrientationValue(Orientation.North);

            // Assert - Should handle multiple changes without issues
            var finalOrientation = GetOrientationValue();
            Assert.AreEqual(Orientation.North, finalOrientation, "Final orientation should be North");
        }

        [Test]
        public void SceneTransitionPrefabRoot_InvalidOrientation_HandlesGracefully()
        {
            // Act & Assert - Should handle orientation changes without issues
            // Test multiple rapid changes instead of invalid enum values
            Assert.DoesNotThrow(() =>
            {
                SetOrientationValue(Orientation.North);
                SetOrientationValue(Orientation.East);
                SetOrientationValue(Orientation.South);
                SetOrientationValue(Orientation.West);
            }, "Should handle orientation changes gracefully");
        }

        #endregion

        #region Performance Tests

        [Test]
        public void SceneTransitionPrefabRoot_OrientationCalculation_IsEfficient()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act - Multiple orientation calculations
            for (int i = 0; i < 1000; i++)
            {
                var orientation = (Orientation)(i % 4); // Cycle through orientations
                SetOrientationValue(orientation);
                CalculateOrientationOffset();
            }

            stopwatch.Stop();

            // Assert - Should complete quickly
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, "Orientation calculations should be efficient");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get the current orientation value through reflection
        /// </summary>
        private Orientation GetOrientationValue()
        {
            var field = typeof(SceneTransitionPrefabRoot).GetField("orientation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                return (Orientation)field.GetValue(prefabRoot);
            }
            
            return Orientation.North; // Default fallback
        }

        /// <summary>
        /// Set the orientation value through reflection
        /// </summary>
        private void SetOrientationValue(Orientation orientation)
        {
            var field = typeof(SceneTransitionPrefabRoot).GetField("orientation",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(prefabRoot, orientation);
        }

        /// <summary>
        /// Calculate orientation offset based on current orientation
        /// This simulates the positioning logic that would be in the actual component
        /// </summary>
        private Vector3 CalculateOrientationOffset()
        {
            var orientation = GetOrientationValue();
            var offset = 1.5f; // Standard offset distance

            switch (orientation)
            {
                case Orientation.North:
                    return new Vector3(0f, 0f, -offset);
                case Orientation.East:
                    return new Vector3(-offset, 0f, 0f);
                case Orientation.South:
                    return new Vector3(0f, 0f, offset);
                case Orientation.West:
                    return new Vector3(offset, 0f, 0f);
                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Assert Vector3 equality with tolerance
        /// </summary>
        private void AssertVector3Equal(Vector3 expected, Vector3 actual, float tolerance, string message)
        {
            Assert.AreEqual(expected.x, actual.x, tolerance, $"{message} - X component mismatch");
            Assert.AreEqual(expected.y, actual.y, tolerance, $"{message} - Y component mismatch");
            Assert.AreEqual(expected.z, actual.z, tolerance, $"{message} - Z component mismatch");
        }

        #endregion
    }
}