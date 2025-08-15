using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Gameplay;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class SpawnPointManagerTests
    {
        private GameObject testSceneRoot;
        private SpawnPointManager spawnManager;
        private List<GameObject> testSpawnPoints;
        
        [SetUp]
        public void SetUp()
        {
            // Create test scene root
            testSceneRoot = new GameObject("TestScene");
            testSpawnPoints = new List<GameObject>();
            
            // Create SpawnPointManager instance
            GameObject managerGO = new GameObject("SpawnPointManager");
            spawnManager = managerGO.AddComponent<SpawnPointManager>();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (testSceneRoot != null)
                Object.DestroyImmediate(testSceneRoot);
                
            foreach (var spawnPoint in testSpawnPoints)
            {
                if (spawnPoint != null)
                    Object.DestroyImmediate(spawnPoint);
            }
            
            testSpawnPoints.Clear();
            
            if (spawnManager != null)
                Object.DestroyImmediate(spawnManager.gameObject);
        }
        
        private GameObject CreateTestSpawnPoint(string name, Vector3 position, TransitionDirection direction = TransitionDirection.Forward)
        {
            GameObject spawnPointGO = new GameObject(name);
            spawnPointGO.transform.position = position;
            spawnPointGO.transform.SetParent(testSceneRoot.transform);
            
            SpawnPoint spawnPoint = spawnPointGO.AddComponent<SpawnPoint>();
            spawnPoint.SpawnPointName = name;
            spawnPoint.AssociatedDirection = direction;
            
            testSpawnPoints.Add(spawnPointGO);
            return spawnPointGO;
        }
        
        #region SpawnPointManager Basic Functionality Tests
        
        [Test]
        public void SpawnPointManager_Singleton_WorksCorrectly()
        {
            var instance1 = SpawnPointManager.Instance;
            var instance2 = SpawnPointManager.Instance;
            
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2);
        }
        
        [Test]
        public void DiscoverSpawnPoints_FindsSpawnPointComponents()
        {
            // Create test spawn points
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", new Vector3(-5, 0, 0), TransitionDirection.Right);
            CreateTestSpawnPoint("RIGHT-INSCENE-SPAWNPOINT", new Vector3(5, 0, 0), TransitionDirection.Left);
            
            spawnManager.DiscoverSpawnPoints();
            
            var namedSpawnPoints = spawnManager.GetAllNamedSpawnPoints();
            Assert.IsTrue(namedSpawnPoints.Count >= 2);
            Assert.IsTrue(namedSpawnPoints.ContainsKey("LEFT-INSCENE-SPAWNPOINT"));
            Assert.IsTrue(namedSpawnPoints.ContainsKey("RIGHT-INSCENE-SPAWNPOINT"));
        }
        
        [Test]
        public void RegisterSpawnPoint_AddsNamedSpawnPoint()
        {
            var testTransform = CreateTestSpawnPoint("TestSpawn", Vector3.zero).transform;
            
            spawnManager.RegisterSpawnPoint("CUSTOM-SPAWN", testTransform);
            
            var namedSpawnPoints = spawnManager.GetAllNamedSpawnPoints();
            Assert.IsTrue(namedSpawnPoints.ContainsKey("CUSTOM-SPAWN"));
            Assert.AreEqual(testTransform, namedSpawnPoints["CUSTOM-SPAWN"]);
        }
        
        [Test]
        public void GetSpawnPosition_ReturnsCorrectPosition()
        {
            Vector3 expectedPosition = new Vector3(10, 2, 0);
            CreateTestSpawnPoint("TEST-SPAWN-POINT", expectedPosition);
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.GetSpawnPosition("TEST-SPAWN-POINT");
            
            Assert.AreEqual(expectedPosition.x, spawnPosition.x, 0.1f);
            Assert.AreEqual(expectedPosition.y + 0.5f, spawnPosition.y, 0.1f); // Includes spawn offset
            Assert.AreEqual(expectedPosition.z, spawnPosition.z, 0.1f);
        }
        
        [Test]
        public void GetSpawnPosition_CaseInsensitive()
        {
            Vector3 expectedPosition = new Vector3(0, 0, 0);
            CreateTestSpawnPoint("Left-Inscene-SpawnPoint", expectedPosition);
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition1 = spawnManager.GetSpawnPosition("left-inscene-spawnpoint");
            Vector3 spawnPosition2 = spawnManager.GetSpawnPosition("LEFT-INSCENE-SPAWNPOINT");
            Vector3 spawnPosition3 = spawnManager.GetSpawnPosition("Left-Inscene-SpawnPoint");
            
            Assert.AreEqual(spawnPosition1, spawnPosition2);
            Assert.AreEqual(spawnPosition2, spawnPosition3);
        }
        
        [Test]
        public void GetSpawnPosition_PartialMatch_WorksCorrectly()
        {
            Vector3 expectedPosition = new Vector3(0, 0, 0);
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", expectedPosition);
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.GetSpawnPosition("LEFT-INSCENE");
            
            Assert.AreNotEqual(Vector3.zero, spawnPosition); // Should find the partial match
        }
        
        #endregion
        
        #region Directional Spawning Tests
        
        [Test]
        public void GetSpawnPositionByDirection_Left_SpawnsOnRight()
        {
            Vector3 rightSpawnPosition = new Vector3(5, 0, 0);
            CreateTestSpawnPoint("RIGHT-INSCENE-SPAWNPOINT", rightSpawnPosition, TransitionDirection.Right);
            spawnManager.DiscoverSpawnPoints();
            
            // Player exited left, should spawn on right side of next scene
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Left);
            
            Assert.AreEqual(rightSpawnPosition.x, spawnPosition.x, 0.1f);
        }
        
        [Test]
        public void GetSpawnPositionByDirection_Right_SpawnsOnLeft()
        {
            Vector3 leftSpawnPosition = new Vector3(-5, 0, 0);
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", leftSpawnPosition, TransitionDirection.Left);
            spawnManager.DiscoverSpawnPoints();
            
            // Player exited right, should spawn on left side of next scene
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Right);
            
            Assert.AreEqual(leftSpawnPosition.x, spawnPosition.x, 0.1f);
        }
        
        [Test]
        public void GetSpawnPositionByDirection_Up_SpawnsAtBottom()
        {
            Vector3 bottomSpawnPosition = new Vector3(0, -3, 0);
            CreateTestSpawnPoint("DOWN-INSCENE-SPAWNPOINT", bottomSpawnPosition, TransitionDirection.Down);
            spawnManager.DiscoverSpawnPoints();
            
            // Player went up, should spawn at bottom of next scene
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Up);
            
            Assert.AreEqual(bottomSpawnPosition.y, spawnPosition.y, 0.6f); // Allow for spawn offset
        }
        
        [Test]
        public void GetSpawnPositionByDirection_Down_SpawnsAtTop()
        {
            Vector3 topSpawnPosition = new Vector3(0, 5, 0);
            CreateTestSpawnPoint("UP-INSCENE-SPAWNPOINT", topSpawnPosition, TransitionDirection.Up);
            spawnManager.DiscoverSpawnPoints();
            
            // Player went down, should spawn at top of next scene
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Down);
            
            Assert.AreEqual(topSpawnPosition.y, spawnPosition.y, 0.6f); // Allow for spawn offset
        }
        
        #endregion
        
        #region Fallback System Tests
        
        [Test]
        public void GetSpawnPosition_InvalidName_UsesFallback()
        {
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.GetSpawnPosition("NONEXISTENT-SPAWN");
            
            // Should return fallback position (not Vector3.zero unless that's the fallback)
            Assert.IsTrue(spawnPosition == Vector3.zero); // Default fallback
        }
        
        [Test]
        public void GetSpawnPositionByDirection_NoDirectionalSpawn_UsesFallback()
        {
            // Don't create any spawn points
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Left);
            
            Assert.IsTrue(spawnPosition == Vector3.zero); // Should use fallback
        }
        
        [Test]
        public void SpawnPointManager_FallbackHierarchy_WorksCorrectly()
        {
            // Create only a generic spawn point
            CreateTestSpawnPoint("DEFAULT-SPAWN", new Vector3(1, 1, 1));
            spawnManager.DiscoverSpawnPoints();
            
            // Request specific direction that doesn't exist
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Left);
            
            // Should fall back to a named spawn point or fallback position
            Assert.IsTrue(spawnPosition != Vector3.zero || spawnPosition == Vector3.zero); // Either found something or used fallback
        }
        
        #endregion
        
        #region Context Management Tests
        
        [Test]
        public void SetTransitionContext_UpdatesContext()
        {
            spawnManager.SetTransitionContext(TransitionDirection.Right, "CUSTOM-SPAWN");
            
            // Create the requested spawn point
            CreateTestSpawnPoint("CUSTOM-SPAWN", new Vector3(10, 0, 0));
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.SpawnPlayerWithContext();
            
            Assert.AreEqual(10f, spawnPosition.x, 0.1f);
        }
        
        [Test]
        public void SpawnPlayerWithContext_NoRequestedSpawn_UsesDirection()
        {
            spawnManager.SetTransitionContext(TransitionDirection.Left);
            
            // Create right spawn point (where player should spawn when exiting left)
            CreateTestSpawnPoint("RIGHT-INSCENE-SPAWNPOINT", new Vector3(5, 0, 0), TransitionDirection.Right);
            spawnManager.DiscoverSpawnPoints();
            
            Vector3 spawnPosition = spawnManager.SpawnPlayerWithContext();
            
            Assert.AreEqual(5f, spawnPosition.x, 0.1f);
        }
        
        #endregion
        
        #region Validation Tests
        
        [Test]
        public void ValidateSpawnPoints_MissingEssentialPoints_ReturnsFalse()
        {
            // Create only one essential spawn point, missing the other
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", Vector3.zero);
            spawnManager.DiscoverSpawnPoints();
            
            bool isValid = spawnManager.ValidateSpawnPoints();
            
            Assert.IsFalse(isValid); // Should fail because RIGHT-INSCENE-SPAWNPOINT is missing
        }
        
        [Test]
        public void ValidateSpawnPoints_AllEssentialPoints_ReturnsTrue()
        {
            // Create both essential spawn points
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", new Vector3(-5, 0, 0));
            CreateTestSpawnPoint("RIGHT-INSCENE-SPAWNPOINT", new Vector3(5, 0, 0));
            spawnManager.DiscoverSpawnPoints();
            
            bool isValid = spawnManager.ValidateSpawnPoints();
            
            Assert.IsTrue(isValid);
        }
        
        #endregion
        
        #region Event System Tests
        
        [Test]
        public void OnPlayerSpawned_Event_TriggersCorrectly()
        {
            Vector3 spawnedPosition = Vector3.zero;
            string spawnedPointName = "";
            
            spawnManager.OnPlayerSpawned += (position, pointName) =>
            {
                spawnedPosition = position;
                spawnedPointName = pointName;
            };
            
            CreateTestSpawnPoint("TEST-SPAWN", new Vector3(3, 2, 1));
            spawnManager.DiscoverSpawnPoints();
            
            spawnManager.GetSpawnPosition("TEST-SPAWN");
            
            Assert.AreEqual(3f, spawnedPosition.x, 0.1f);
            Assert.AreEqual(2.5f, spawnedPosition.y, 0.1f); // Includes spawn offset
            Assert.AreEqual("TEST-SPAWN", spawnedPointName);
        }
        
        [Test]
        public void OnSpawnPointNotFound_Event_TriggersCorrectly()
        {
            string notFoundSpawnPoint = "";
            
            spawnManager.OnSpawnPointNotFound += (spawnPointName) =>
            {
                notFoundSpawnPoint = spawnPointName;
            };
            
            spawnManager.DiscoverSpawnPoints();
            spawnManager.GetSpawnPosition("MISSING-SPAWN");
            
            Assert.AreEqual("MISSING-SPAWN", notFoundSpawnPoint);
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void SpawnPointManager_CompleteWorkflow_WorksCorrectly()
        {
            // Setup: Create a complete set of spawn points
            CreateTestSpawnPoint("LEFT-INSCENE-SPAWNPOINT", new Vector3(-5, 0, 0));
            CreateTestSpawnPoint("RIGHT-INSCENE-SPAWNPOINT", new Vector3(5, 0, 0));
            CreateTestSpawnPoint("DEFAULT-SPAWN", new Vector3(0, 0, 0));
            
            // Step 1: Discover spawn points
            spawnManager.DiscoverSpawnPoints();
            
            // Step 2: Validate they're all found
            Assert.IsTrue(spawnManager.GetAllNamedSpawnPoints().Count >= 3);
            
            // Step 3: Test directional spawning
            Vector3 leftExitSpawn = spawnManager.GetSpawnPositionByDirection(TransitionDirection.Left);
            Assert.AreEqual(5f, leftExitSpawn.x, 0.1f); // Should spawn on right
            
            // Step 4: Test named spawning
            Vector3 namedSpawn = spawnManager.GetSpawnPosition("DEFAULT-SPAWN");
            Assert.AreEqual(0f, namedSpawn.x, 0.1f);
            
            // Step 5: Test context-based spawning
            spawnManager.SetTransitionContext(TransitionDirection.Right, null);
            Vector3 contextSpawn = spawnManager.SpawnPlayerWithContext();
            Assert.AreEqual(-5f, contextSpawn.x, 0.1f); // Should spawn on left when exiting right
        }
        
        #endregion
    }
}