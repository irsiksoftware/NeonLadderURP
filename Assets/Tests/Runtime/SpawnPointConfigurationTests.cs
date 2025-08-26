using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Tests.Runtime.SceneTransition;

namespace NeonLadder.Tests.Runtime.SceneTransition
{
    /// <summary>
    /// Unit tests for SpawnPointConfiguration component
    /// Tests Auto/Left/Right/Custom spawn type resolution and positioning
    /// </summary>
    [TestFixture]
    public class SpawnPointConfigurationTests
    {
        private GameObject testSpawnPointObject;
        private SpawnPointConfiguration spawnConfig;

        [SetUp]
        public void SetUp()
        {
            // Suppress log assertions for expected warnings during tests
            LogAssert.ignoreFailingMessages = true;

            // Create test spawn point
            testSpawnPointObject = new GameObject("TestSpawnPoint");
            spawnConfig = testSpawnPointObject.AddComponent<SpawnPointConfiguration>();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (testSpawnPointObject != null)
            {
                Object.DestroyImmediate(testSpawnPointObject);
            }

            // Reset log assertion settings
            LogAssert.ignoreFailingMessages = false;
        }

        #region Basic Configuration Tests

        [Test]
        public void SpawnPointConfiguration_DefaultValues_AreCorrect()
        {
            // Assert
            Assert.AreEqual(SpawnPointType.Auto, spawnConfig.SpawnMode, "Default spawn mode should be Auto");
            Assert.AreEqual("", spawnConfig.CustomSpawnName, "Default custom name should be empty");
        }

        [Test]
        public void SpawnPointConfiguration_GetSpawnWorldPosition_ReturnsTransformPosition()
        {
            // Arrange
            var expectedPosition = new Vector3(5f, 2f, -3f);
            testSpawnPointObject.transform.position = expectedPosition;

            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert
            Assert.AreEqual(expectedPosition, worldPosition, "Should return transform position");
        }

        #endregion

        #region Spawn Type Configuration Tests

        [Test]
        public void SpawnPointConfiguration_SetSpawnModeAuto_ConfiguresCorrectly()
        {
            // Arrange & Act - Set through reflection since field is private
            SetSpawnMode(SpawnPointType.Auto);

            // Assert
            Assert.AreEqual(SpawnPointType.Auto, spawnConfig.SpawnMode, "Spawn mode should be Auto");
        }

        [Test]
        public void SpawnPointConfiguration_SetSpawnModeLeft_ConfiguresCorrectly()
        {
            // Arrange & Act
            SetSpawnMode(SpawnPointType.Left);

            // Assert
            Assert.AreEqual(SpawnPointType.Left, spawnConfig.SpawnMode, "Spawn mode should be Left");
        }

        [Test]
        public void SpawnPointConfiguration_SetSpawnModeRight_ConfiguresCorrectly()
        {
            // Arrange & Act
            SetSpawnMode(SpawnPointType.Right);

            // Assert
            Assert.AreEqual(SpawnPointType.Right, spawnConfig.SpawnMode, "Spawn mode should be Right");
        }

        [Test]
        public void SpawnPointConfiguration_SetSpawnModeCustom_ConfiguresCorrectly()
        {
            // Arrange & Act
            SetSpawnMode(SpawnPointType.Custom);
            SetCustomSpawnName("MyCustomSpawn");

            // Assert
            Assert.AreEqual(SpawnPointType.Custom, spawnConfig.SpawnMode, "Spawn mode should be Custom");
            Assert.AreEqual("MyCustomSpawn", spawnConfig.CustomSpawnName, "Custom name should be set");
        }

        [Test]
        public void SpawnPointConfiguration_SetSpawnModeBossArena_ConfiguresCorrectly()
        {
            // Arrange & Act
            SetSpawnMode(SpawnPointType.BossArena);

            // Assert
            Assert.AreEqual(SpawnPointType.BossArena, spawnConfig.SpawnMode, "Spawn mode should be BossArena");
        }

        #endregion

        #region Position Accuracy Tests

        [TestCase(0f, 0f, 0f)]
        [TestCase(10f, 5f, -2f)]
        [TestCase(-5f, 0f, 8f)]
        [TestCase(100.5f, 25.75f, -50.25f)]
        public void SpawnPointConfiguration_GetSpawnWorldPosition_AccurateToTolerance(float x, float y, float z)
        {
            // Arrange
            var testPosition = new Vector3(x, y, z);
            testSpawnPointObject.transform.position = testPosition;

            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert - Use tolerance for floating point comparison
            var tolerance = 0.01f; // ±0.01 Unity units as specified in PBI
            Assert.AreEqual(testPosition.x, worldPosition.x, tolerance, $"X position should be accurate within {tolerance}");
            Assert.AreEqual(testPosition.y, worldPosition.y, tolerance, $"Y position should be accurate within {tolerance}");
            Assert.AreEqual(testPosition.z, worldPosition.z, tolerance, $"Z position should be accurate within {tolerance}");
        }

        [Test]
        public void SpawnPointConfiguration_PositionChanges_UpdatedCorrectly()
        {
            // Arrange
            var initialPosition = new Vector3(1f, 2f, 3f);
            var updatedPosition = new Vector3(4f, 5f, 6f);
            
            testSpawnPointObject.transform.position = initialPosition;
            var firstPosition = spawnConfig.GetSpawnWorldPosition();

            // Act
            testSpawnPointObject.transform.position = updatedPosition;
            var secondPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert
            Assert.AreEqual(initialPosition, firstPosition, "First position should match initial");
            Assert.AreEqual(updatedPosition, secondPosition, "Second position should match updated");
            Assert.AreNotEqual(firstPosition, secondPosition, "Positions should be different");
        }

        #endregion

        #region Custom Spawn Name Tests

        [TestCase("")]
        [TestCase("DefaultSpawn")]
        [TestCase("CustomSpawnPoint")]
        [TestCase("Boss_Arena_Center")]
        [TestCase("Special-Spawn_01")]
        public void SpawnPointConfiguration_CustomSpawnName_HandlesVariousStrings(string customName)
        {
            // Arrange & Act
            SetCustomSpawnName(customName);

            // Assert
            Assert.AreEqual(customName, spawnConfig.CustomSpawnName, $"Custom name should handle: '{customName}'");
        }

        [Test]
        public void SpawnPointConfiguration_CustomSpawnName_HandlesNullGracefully()
        {
            // Arrange & Act
            SetCustomSpawnName(null);

            // Assert
            // Should either be null or empty string, both are acceptable
            var customName = spawnConfig.CustomSpawnName;
            Assert.IsTrue(string.IsNullOrEmpty(customName), "Null custom name should be handled gracefully");
        }

        #endregion

        #region Integration with Parent Transform Tests

        [Test]
        public void SpawnPointConfiguration_WithParentTransform_UsesCorrectWorldPosition()
        {
            // Arrange - Create parent-child hierarchy
            var parentObject = new GameObject("Parent");
            parentObject.transform.position = new Vector3(10f, 5f, 2f);
            
            testSpawnPointObject.transform.SetParent(parentObject.transform);
            testSpawnPointObject.transform.localPosition = new Vector3(1f, 1f, 1f);
            
            var expectedWorldPosition = parentObject.transform.position + testSpawnPointObject.transform.localPosition;

            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert
            Assert.AreEqual(expectedWorldPosition, worldPosition, "Should use correct world position with parent transform");
            
            // Cleanup
            Object.DestroyImmediate(parentObject);
        }

        [Test]
        public void SpawnPointConfiguration_WithRotatedParent_UsesCorrectWorldPosition()
        {
            // Arrange - Test Kaoru 90° rotation scenario mentioned in PBI
            var parentObject = new GameObject("RotatedParent");
            parentObject.transform.position = Vector3.zero;
            parentObject.transform.rotation = Quaternion.Euler(0, 90, 0); // 90° Y rotation
            
            testSpawnPointObject.transform.SetParent(parentObject.transform);
            testSpawnPointObject.transform.localPosition = new Vector3(1f, 0f, 0f); // 1 unit to the right locally
            
            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();
            
            // Assert - After 90° Y rotation, local (1,0,0) becomes world (0,0,-1)
            // Unity uses left-handed coordinate system where +Y rotation is clockwise when viewed from +Y
            var expectedWorldPosition = new Vector3(0f, 0f, -1f);
            var tolerance = 0.01f;
            
            Assert.AreEqual(expectedWorldPosition.x, worldPosition.x, tolerance, "X should account for parent rotation");
            Assert.AreEqual(expectedWorldPosition.y, worldPosition.y, tolerance, "Y should account for parent rotation");
            Assert.AreEqual(expectedWorldPosition.z, worldPosition.z, tolerance, "Z should account for parent rotation");
            
            // Cleanup
            Object.DestroyImmediate(parentObject);
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void SpawnPointConfiguration_VeryLargePosition_HandledCorrectly()
        {
            // Arrange - Test with very large coordinates
            var largePosition = new Vector3(999999f, 999999f, 999999f);
            testSpawnPointObject.transform.position = largePosition;

            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert
            Assert.AreEqual(largePosition, worldPosition, "Should handle very large positions");
        }

        [Test]
        public void SpawnPointConfiguration_VerySmallPosition_HandledCorrectly()
        {
            // Arrange - Test with very small coordinates
            var smallPosition = new Vector3(0.000001f, 0.000001f, 0.000001f);
            testSpawnPointObject.transform.position = smallPosition;

            // Act
            var worldPosition = spawnConfig.GetSpawnWorldPosition();

            // Assert
            var tolerance = 0.0000001f;
            Assert.AreEqual(smallPosition.x, worldPosition.x, tolerance, "Should handle very small X positions");
            Assert.AreEqual(smallPosition.y, worldPosition.y, tolerance, "Should handle very small Y positions");
            Assert.AreEqual(smallPosition.z, worldPosition.z, tolerance, "Should handle very small Z positions");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to set spawn mode through reflection
        /// </summary>
        private void SetSpawnMode(SpawnPointType spawnMode)
        {
            var field = typeof(SpawnPointConfiguration).GetField("spawnMode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(spawnConfig, spawnMode);
        }

        /// <summary>
        /// Helper method to set custom spawn name through reflection
        /// </summary>
        private void SetCustomSpawnName(string customName)
        {
            var field = typeof(SpawnPointConfiguration).GetField("customSpawnName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(spawnConfig, customName);
        }

        #endregion
    }
}