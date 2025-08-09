using NUnit.Framework;
using NeonLadder.Optimization;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace NeonLadder.Tests.Runtime.Optimization
{
    [TestFixture]
    public class EulerAngleCacheTests
    {
        private EulerAngleCache cache;
        private GameObject testObject;
        private Transform testTransform;

        [SetUp]
        public void Setup()
        {
            cache = new EulerAngleCache(maxCacheSize: 10);
            testObject = new GameObject("TestObject");
            testTransform = testObject.transform;
        }

        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
        }

        #region Core Functionality Tests

        [Test]
        public void GetEulerAngles_FirstCall_PerformsConversionAndCaches()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Act
            Vector3 result = cache.GetEulerAngles(testTransform);
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(45f, result.x, 0.1f, "X angle should be cached correctly");
            Assert.AreEqual(90f, result.y, 0.1f, "Y angle should be cached correctly");
            Assert.AreEqual(30f, result.z, 0.1f, "Z angle should be cached correctly");
            Assert.AreEqual(0, stats.TotalHits, "First call should be a miss");
            Assert.AreEqual(1, stats.TotalMisses, "First call should register as miss");
            Assert.AreEqual(1, stats.TotalConversions, "Should perform one conversion");
        }

        [Test]
        public void GetEulerAngles_SecondCallSameRotation_ReturnsCachedValue()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            cache.GetEulerAngles(testTransform); // First call to populate cache
            
            // Act
            Vector3 result = cache.GetEulerAngles(testTransform); // Second call
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(45f, result.x, 0.1f);
            Assert.AreEqual(90f, result.y, 0.1f);
            Assert.AreEqual(30f, result.z, 0.1f);
            Assert.AreEqual(1, stats.TotalHits, "Second call should be a hit");
            Assert.AreEqual(1, stats.TotalMisses, "Only first call should be a miss");
            Assert.AreEqual(1, stats.TotalConversions, "Should not perform additional conversion");
        }

        [Test]
        public void GetEulerAngles_RotationChanged_RecalculatesAndUpdatesCache()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            cache.GetEulerAngles(testTransform);
            
            // Act
            testTransform.rotation = Quaternion.Euler(60f, 120f, 45f);
            Vector3 result = cache.GetEulerAngles(testTransform);
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(60f, result.x, 0.1f);
            Assert.AreEqual(120f, result.y, 0.1f);
            Assert.AreEqual(45f, result.z, 0.1f);
            Assert.AreEqual(0, stats.TotalHits, "Rotation change should cause miss");
            Assert.AreEqual(2, stats.TotalMisses, "Both calls should be misses");
            Assert.AreEqual(2, stats.TotalConversions, "Should perform two conversions");
        }

        #endregion

        #region Individual Axis Tests

        [Test]
        public void GetEulerY_ReturnsCachedYAxis()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Act
            float yAngle = cache.GetEulerY(testTransform);
            float yAngle2 = cache.GetEulerY(testTransform); // Second call
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(90f, yAngle, 0.1f);
            Assert.AreEqual(90f, yAngle2, 0.1f);
            Assert.AreEqual(1, stats.TotalHits, "Second call should hit cache");
        }

        [Test]
        public void GetEulerX_ReturnsCachedXAxis()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Act
            float xAngle = cache.GetEulerX(testTransform);
            float xAngle2 = cache.GetEulerX(testTransform);
            
            // Assert
            Assert.AreEqual(45f, xAngle, 0.1f);
            Assert.AreEqual(45f, xAngle2, 0.1f);
            Assert.AreEqual(1, cache.GetStatistics().TotalHits);
        }

        [Test]
        public void GetEulerZ_ReturnsCachedZAxis()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Act
            float zAngle = cache.GetEulerZ(testTransform);
            float zAngle2 = cache.GetEulerZ(testTransform);
            
            // Assert
            Assert.AreEqual(30f, zAngle, 0.1f);
            Assert.AreEqual(30f, zAngle2, 0.1f);
            Assert.AreEqual(1, cache.GetStatistics().TotalHits);
        }

        #endregion

        #region Direction Helper Tests

        [Test]
        public void IsFacingLeft_DetectsLeftFacingCorrectly()
        {
            // Arrange & Act & Assert
            testTransform.rotation = Quaternion.Euler(0f, 270f, 0f);
            Assert.IsTrue(cache.IsFacingLeft(testTransform), "Should detect left facing at 270 degrees");
            
            testTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Assert.IsFalse(cache.IsFacingLeft(testTransform), "Should not detect left facing at 90 degrees");
        }

        [Test]
        public void IsFacingRight_DetectsRightFacingCorrectly()
        {
            // Arrange & Act & Assert
            testTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Assert.IsTrue(cache.IsFacingRight(testTransform), "Should detect right facing at 90 degrees");
            
            testTransform.rotation = Quaternion.Euler(0f, 270f, 0f);
            Assert.IsFalse(cache.IsFacingRight(testTransform), "Should not detect right facing at 270 degrees");
        }

        [Test]
        public void GetFacingDirection2D_ReturnsCorrectVectors()
        {
            // Test cardinal directions
            testTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Assert.AreEqual(Vector2.right, cache.GetFacingDirection2D(testTransform));
            
            testTransform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Assert.AreEqual(Vector2.up, cache.GetFacingDirection2D(testTransform));
            
            testTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
            Assert.AreEqual(Vector2.left, cache.GetFacingDirection2D(testTransform));
            
            testTransform.rotation = Quaternion.Euler(0f, 270f, 0f);
            Assert.AreEqual(Vector2.down, cache.GetFacingDirection2D(testTransform));
        }

        #endregion

        #region Cache Management Tests

        [Test]
        public void PrewarmCache_PopulatesCacheBeforeUse()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Act
            cache.PrewarmCache(testTransform);
            Vector3 result = cache.GetEulerAngles(testTransform);
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(1, stats.TotalHits, "Should hit cache after prewarm");
            Assert.AreEqual(1, stats.TotalMisses, "Prewarm should count as miss");
            Assert.AreEqual(1, stats.TotalConversions, "Should only convert once during prewarm");
        }

        [Test]
        public void InvalidateCache_RemovesCachedEntry()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            cache.GetEulerAngles(testTransform);
            
            // Act
            cache.InvalidateCache(testTransform);
            cache.GetEulerAngles(testTransform);
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(0, stats.TotalHits, "Should not hit after invalidation");
            Assert.AreEqual(2, stats.TotalMisses, "Both calls should miss");
        }

        [Test]
        public void ClearCache_RemovesAllEntries()
        {
            // Arrange
            var obj1 = new GameObject("Test1");
            var obj2 = new GameObject("Test2");
            cache.GetEulerAngles(obj1.transform);
            cache.GetEulerAngles(obj2.transform);
            
            // Act
            cache.ClearCache();
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(0, stats.TotalHits);
            Assert.AreEqual(0, stats.TotalMisses);
            Assert.AreEqual(0, stats.TotalConversions);
            Assert.AreEqual(0, stats.CacheSize);
            
            // Cleanup
            Object.DestroyImmediate(obj1);
            Object.DestroyImmediate(obj2);
        }

        [Test]
        public void CacheSize_EnforcesMaximumLimit()
        {
            // Arrange
            var smallCache = new EulerAngleCache(maxCacheSize: 3);
            var objects = new GameObject[5];
            
            // Act
            for (int i = 0; i < 5; i++)
            {
                objects[i] = new GameObject($"Test{i}");
                smallCache.GetEulerAngles(objects[i].transform);
            }
            
            var stats = smallCache.GetStatistics();
            
            // Assert
            Assert.LessOrEqual(stats.CacheSize, 3, "Cache should not exceed max size");
            
            // Cleanup
            foreach (var obj in objects)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
        }

        #endregion

        #region Performance Statistics Tests

        [Test]
        public void GetStatistics_ReturnsAccurateMetrics()
        {
            // Arrange & Act
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            cache.GetEulerAngles(testTransform); // Miss
            cache.GetEulerAngles(testTransform); // Hit
            cache.GetEulerAngles(testTransform); // Hit
            
            var stats = cache.GetStatistics();
            
            // Assert
            Assert.AreEqual(2, stats.TotalHits);
            Assert.AreEqual(1, stats.TotalMisses);
            Assert.AreEqual(1, stats.TotalConversions);
            Assert.AreEqual(0.667f, stats.HitRate, 0.01f, "Hit rate should be 2/3");
            Assert.AreEqual(1, stats.CacheSize);
        }

        [Test]
        public void GetTransformStatistics_ReturnsDetailedTransformMetrics()
        {
            // Arrange
            testTransform.rotation = Quaternion.Euler(45f, 90f, 30f);
            cache.GetEulerAngles(testTransform); // Miss
            cache.GetEulerAngles(testTransform); // Hit
            cache.GetEulerAngles(testTransform); // Hit
            
            // Act
            var transformStats = cache.GetTransformStatistics(testTransform);
            
            // Assert
            Assert.AreEqual(2, transformStats.HitCount);
            Assert.AreEqual(1, transformStats.MissCount);
            Assert.AreEqual(testTransform.rotation, transformStats.CachedRotation);
            Assert.AreEqual(45f, transformStats.CachedEuler.x, 0.1f);
            Assert.AreEqual(90f, transformStats.CachedEuler.y, 0.1f);
            Assert.AreEqual(30f, transformStats.CachedEuler.z, 0.1f);
        }

        #endregion

        #region Edge Cases

        [Test]
        public void GetEulerAngles_NullTransform_ReturnsZero()
        {
            // Act
            Vector3 result = cache.GetEulerAngles(null);
            
            // Assert
            Assert.AreEqual(Vector3.zero, result);
        }

        [Test]
        public void GetEulerY_NullTransform_ReturnsZero()
        {
            // Act
            float result = cache.GetEulerY(null);
            
            // Assert
            Assert.AreEqual(0f, result);
        }

        [Test]
        public void MultipleTransforms_MaintainsSeparateCaches()
        {
            // Arrange
            var obj1 = new GameObject("Test1");
            var obj2 = new GameObject("Test2");
            obj1.transform.rotation = Quaternion.Euler(45f, 90f, 30f);
            obj2.transform.rotation = Quaternion.Euler(60f, 120f, 45f);
            
            // Act
            Vector3 result1 = cache.GetEulerAngles(obj1.transform);
            Vector3 result2 = cache.GetEulerAngles(obj2.transform);
            Vector3 result1Again = cache.GetEulerAngles(obj1.transform);
            
            // Assert
            Assert.AreEqual(45f, result1.x, 0.1f);
            Assert.AreEqual(60f, result2.x, 0.1f);
            Assert.AreEqual(45f, result1Again.x, 0.1f);
            
            var stats = cache.GetStatistics();
            Assert.AreEqual(1, stats.TotalHits, "Third call should hit cache");
            Assert.AreEqual(2, stats.TotalMisses, "First two calls should miss");
            
            // Cleanup
            Object.DestroyImmediate(obj1);
            Object.DestroyImmediate(obj2);
        }

        #endregion
    }

    [TestFixture]
    public class EulerAngleCacheManagerTests
    {
        private GameObject managerObject;
        private EulerAngleCacheManager manager;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // Clean up any existing manager
            var existing = GameObject.Find("EulerAngleCacheManager");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
            
            // Access Instance to create the manager
            manager = EulerAngleCacheManager.Instance;
            managerObject = manager.gameObject;
            
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (managerObject != null)
            {
                Object.DestroyImmediate(managerObject);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator Instance_CreatesSingleton()
        {
            // Act
            var instance1 = EulerAngleCacheManager.Instance;
            var instance2 = EulerAngleCacheManager.Instance;
            
            // Assert
            Assert.IsNotNull(instance1);
            Assert.AreSame(instance1, instance2, "Should return same instance");
            Assert.IsTrue(instance1.gameObject.scene.name == "DontDestroyOnLoad", 
                "Manager should be in DontDestroyOnLoad scene");
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Cache_ProvidesGlobalAccess()
        {
            // Act
            var cache = EulerAngleCacheManager.Cache;
            
            // Assert
            Assert.IsNotNull(cache, "Cache should be accessible globally");
            
            // Test cache functionality
            var testObject = new GameObject("TestObject");
            testObject.transform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            var result = cache.GetEulerAngles(testObject.transform);
            Assert.AreEqual(90f, result.y, 0.1f);
            
            Object.DestroyImmediate(testObject);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Update_CallsCacheUpdate()
        {
            // Arrange
            var cache = EulerAngleCacheManager.Cache;
            var testObject = new GameObject("TestObject");
            cache.GetEulerAngles(testObject.transform);
            
            // Act - wait for several frames to trigger update
            yield return new WaitForSeconds(0.1f);
            
            // Assert - cache should still be functional
            var stats = cache.GetStatistics();
            Assert.GreaterOrEqual(stats.CacheSize, 0);
            
            // Cleanup
            Object.DestroyImmediate(testObject);
        }

        [UnityTest]
        public IEnumerator LogStatistics_OutputsCacheMetrics()
        {
            // Arrange
            var cache = EulerAngleCacheManager.Cache;
            var testObject = new GameObject("TestObject");
            testObject.transform.rotation = Quaternion.Euler(45f, 90f, 30f);
            
            // Populate cache with some data
            cache.GetEulerAngles(testObject.transform);
            cache.GetEulerAngles(testObject.transform);
            
            // Act
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex("Cache Stats"));
            manager.LogStatistics();
            
            // Cleanup
            Object.DestroyImmediate(testObject);
            yield return null;
        }
    }
}