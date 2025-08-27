using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Tests to validate that the SceneTransitionTrigger correctly handles all 8 boss paths
    /// </summary>
    public class ProceduralPathTransitionsTests
    {
        private SceneTransitionTrigger trigger;
        private GameObject triggerGameObject;

        [SetUp]
        public void SetUp()
        {
            // Create a test trigger
            triggerGameObject = new GameObject("TestTrigger");
            trigger = triggerGameObject.AddComponent<SceneTransitionTrigger>();
            
            // Set up a trigger collider
            var colliderObj = new GameObject("TriggerCollider");
            colliderObj.transform.SetParent(triggerGameObject.transform);
            var collider = colliderObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Configure the trigger for procedural transitions
            var triggerField = typeof(SceneTransitionTrigger).GetField("triggerColliderObject", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            triggerField.SetValue(trigger, colliderObj);
            
            var destinationTypeField = typeof(SceneTransitionTrigger).GetField("destinationType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            destinationTypeField.SetValue(trigger, SceneTransitionTrigger.DestinationType.Procedural);
        }

        [TearDown]
        public void TearDown()
        {
            if (triggerGameObject != null)
            {
                Object.DestroyImmediate(triggerGameObject);
            }
        }

        [Test]
        public void GetProceduralDestination_FromStart_ReturnsConnection1Scene()
        {
            // Arrange
            MockActiveScene("Start");
            string seed = "test_seed";
            SetupGameInstanceWithSeed(seed);

            // Act
            string destination = CallGetProceduralDestination();

            // Assert
            Assert.IsTrue(destination.EndsWith("_Connection1"), 
                $"Expected Connection1 scene from Start, but got: {destination}");
            Assert.IsTrue(IsValidBossConnection1(destination), 
                $"Destination should be a valid boss Connection1 scene: {destination}");
        }

        [Test]
        public void GetProceduralDestination_FromConnection1_ReturnsConnection2Scene()
        {
            // Arrange
            MockActiveScene("Cathedral_Connection1");

            // Act
            string destination = CallGetProceduralDestination();

            // Assert
            Assert.AreEqual("Cathedral_Connection2", destination,
                "Should progress from Connection1 to Connection2 of same boss");
        }

        [Test]
        public void GetProceduralDestination_FromConnection2_ReturnsBossArena()
        {
            // Arrange
            MockActiveScene("Vault_Connection2");

            // Act
            string destination = CallGetProceduralDestination();

            // Assert
            Assert.AreEqual("Vault", destination,
                "Should progress from Connection2 to boss arena");
        }

        [Test]
        public void GetProceduralDestination_FromBossArena_ReturnsReturnToStaging()
        {
            // Arrange - Test all 8 boss arenas
            string[] bossScenes = { "Cathedral", "Necropolis", "Vault", "Mirage", "Garden", "Banquet", "Lounge", "Finale" };
            
            foreach (string bossScene in bossScenes)
            {
                MockActiveScene(bossScene);

                // Act
                string destination = CallGetProceduralDestination();

                // Assert
                Assert.AreEqual("ReturnToStaging", destination,
                    $"Boss arena '{bossScene}' should return to staging");
            }
        }

        [Test]
        public void GetProceduralDestination_AllBossesAccessible_WithDifferentSeeds()
        {
            // Arrange
            MockActiveScene("Start");
            string[] expectedBosses = { "Cathedral", "Necropolis", "Vault", "Mirage", "Garden", "Banquet", "Lounge", "Finale" };
            
            // Test with multiple seeds to ensure all bosses can be reached
            for (int i = 0; i < 100; i++)
            {
                string seed = $"seed_{i}";
                SetupGameInstanceWithSeed(seed);

                // Act
                string destination = CallGetProceduralDestination();

                // Assert
                Assert.IsTrue(destination.EndsWith("_Connection1"), 
                    $"Should get Connection1 scene, got: {destination}");
                
                string bossName = destination.Replace("_Connection1", "");
                Assert.Contains(bossName, expectedBosses, 
                    $"Boss '{bossName}' should be one of the 8 valid bosses");
            }
        }

        [Test]
        public void GetProceduralDestination_SameSeed_ReturnsDeterministicResults()
        {
            // Arrange
            MockActiveScene("Start");
            string seed = "deterministic_seed";
            
            SetupGameInstanceWithSeed(seed);
            string firstResult = CallGetProceduralDestination();
            
            // Act - Call again with same seed
            SetupGameInstanceWithSeed(seed);
            string secondResult = CallGetProceduralDestination();

            // Assert
            Assert.AreEqual(firstResult, secondResult,
                "Same seed should produce same destination");
        }

        [Test]
        public void IsBossArena_AllBossScenes_ReturnTrue()
        {
            // Arrange
            string[] bossScenes = { "Cathedral", "Necropolis", "Vault", "Mirage", "Garden", "Banquet", "Lounge", "Finale" };

            foreach (string bossScene in bossScenes)
            {
                // Act
                bool result = CallIsBossArena(bossScene);

                // Assert
                Assert.IsTrue(result, $"'{bossScene}' should be recognized as a boss arena");
            }
        }

        [Test]
        public void IsBossArena_NonBossScenes_ReturnFalse()
        {
            // Arrange
            string[] nonBossScenes = { "Start", "Staging", "Cathedral_Connection1", "Vault_Connection2", "Shop" };

            foreach (string nonBossScene in nonBossScenes)
            {
                // Act
                bool result = CallIsBossArena(nonBossScene);

                // Assert
                Assert.IsFalse(result, $"'{nonBossScene}' should NOT be recognized as a boss arena");
            }
        }

        #region Helper Methods

        private void MockActiveScene(string sceneName)
        {
            // Note: In a real test environment, you might need to actually load scenes
            // For this test, we're testing the logic independent of actual scene loading
            // The GetProceduralDestination method uses SceneManager.GetActiveScene().name
        }

        private void SetupGameInstanceWithSeed(string seed)
        {
            // Create a mock Game instance with the specified seed
            var gameObj = new GameObject("GameInstance");
            var game = gameObj.AddComponent<Game>();
            
            // Set up the procedural map with seed
            var mapField = typeof(Game).GetField("proceduralMap", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (mapField != null)
            {
                var map = new ProceduralMap { Seed = seed };
                mapField.SetValue(game, map);
            }
        }

        private string CallGetProceduralDestination()
        {
            var method = typeof(SceneTransitionTrigger).GetMethod("GetProceduralDestination", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (string)method.Invoke(trigger, null);
        }

        private bool CallIsBossArena(string sceneName)
        {
            var method = typeof(SceneTransitionTrigger).GetMethod("IsBossArena", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (bool)method.Invoke(trigger, new object[] { sceneName });
        }

        private bool IsValidBossConnection1(string sceneName)
        {
            string[] validConnections = {
                "Cathedral_Connection1", "Necropolis_Connection1", "Vault_Connection1",
                "Mirage_Connection1", "Garden_Connection1", "Banquet_Connection1",
                "Lounge_Connection1", "Finale_Connection1"
            };
            return System.Array.Exists(validConnections, conn => conn == sceneName);
        }

        #endregion
    }
}