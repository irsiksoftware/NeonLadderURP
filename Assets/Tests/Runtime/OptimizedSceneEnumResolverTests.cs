using NUnit.Framework;
using UnityEngine;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Utilities;
using System.Diagnostics;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Performance and functionality tests for OptimizedSceneEnumResolver
    /// Verifies caching behavior and performance improvements
    /// </summary>
    [TestFixture]
    public class OptimizedSceneEnumResolverTests
    {
        [SetUp]
        public void Setup()
        {
            // Clear caches before each test for consistent results
            OptimizedSceneEnumResolver.ClearCaches();
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up after tests
            OptimizedSceneEnumResolver.ClearCaches();
        }
        
        #region Functionality Tests
        
        [Test]
        public void Resolve_ValidSceneName_ReturnsCorrectEnum()
        {
            // Act
            var result = OptimizedSceneEnumResolver.Resolve("Title");
            
            // Assert
            Assert.AreEqual(Scenes.Core.Title, result, "Should resolve Title scene correctly");
        }
        
        [Test]
        public void Resolve_CaseInsensitive_ReturnsCorrectEnum()
        {
            // Act
            var result1 = OptimizedSceneEnumResolver.Resolve("title");
            var result2 = OptimizedSceneEnumResolver.Resolve("TITLE");
            var result3 = OptimizedSceneEnumResolver.Resolve("TiTlE");
            
            // Assert
            Assert.AreEqual(Scenes.Core.Title, result1, "Should resolve lowercase");
            Assert.AreEqual(Scenes.Core.Title, result2, "Should resolve uppercase");
            Assert.AreEqual(Scenes.Core.Title, result3, "Should resolve mixed case");
        }
        
        [Test]
        public void Resolve_TestScene_ReturnsTestEnum()
        {
            // Act
            var result1 = OptimizedSceneEnumResolver.Resolve("TestScene");
            var result2 = OptimizedSceneEnumResolver.Resolve("test_something");
            var result3 = OptimizedSceneEnumResolver.Resolve("MyTestLevel");
            
            // Assert
            Assert.AreEqual(Scenes.Core.Test, result1, "Should identify test scenes");
            Assert.AreEqual(Scenes.Core.Test, result2, "Should identify test with underscore");
            Assert.AreEqual(Scenes.Core.Test, result3, "Should identify test in middle of name");
        }
        
        [Test]
        public void Resolve_UnknownScene_ReturnsDefault()
        {
            // Act
            var result = OptimizedSceneEnumResolver.Resolve("NonExistentScene");
            
            // Assert
            Assert.AreEqual(default(Scenes), result, "Should return default for unknown scenes");
        }
        
        [Test]
        public void Resolve_NullOrEmpty_ReturnsDefault()
        {
            // Act
            var result1 = OptimizedSceneEnumResolver.Resolve(null);
            var result2 = OptimizedSceneEnumResolver.Resolve("");
            var result3 = OptimizedSceneEnumResolver.Resolve("   ");
            
            // Assert
            Assert.AreEqual(default(Scenes), result1, "Should handle null");
            Assert.AreEqual(default(Scenes), result2, "Should handle empty string");
            Assert.AreEqual(default(Scenes), result3, "Should handle whitespace");
        }
        
        #endregion
        
        #region Caching Tests
        
        [Test]
        public void Resolve_SecondCall_UsesCachedValue()
        {
            // Arrange
            var sceneName = "Staging";
            
            // Act - First call
            var result1 = OptimizedSceneEnumResolver.Resolve(sceneName);
            var stats1 = OptimizedSceneEnumResolver.GetCacheStatistics();
            
            // Act - Second call
            var result2 = OptimizedSceneEnumResolver.Resolve(sceneName);
            var stats2 = OptimizedSceneEnumResolver.GetCacheStatistics();
            
            // Assert
            Assert.AreEqual(result1, result2, "Should return same result");
            Assert.Greater(stats2.hits, stats1.hits, "Cache hits should increase");
            Assert.AreEqual(stats1.misses, stats2.misses, "Cache misses should not increase");
        }
        
        [Test]
        public void IsValidScene_ValidatesSceneNames()
        {
            // Arrange & Act & Assert
            Assert.IsTrue(OptimizedSceneEnumResolver.IsValidScene(Scenes.Core.MetaShop), "MetaShop should be valid");
            Assert.IsTrue(OptimizedSceneEnumResolver.IsValidScene(Scenes.Core.Title), "Title should be valid");
            Assert.IsFalse(OptimizedSceneEnumResolver.IsValidScene("NonExistentScene"), "NonExistentScene should be invalid");
            Assert.IsFalse(OptimizedSceneEnumResolver.IsValidScene(null), "null should be invalid");
            Assert.IsFalse(OptimizedSceneEnumResolver.IsValidScene(""), "empty string should be invalid");
        }
        
        [Test]
        public void PrewarmCache_LoadsScenesIntoCache()
        {
            // Arrange
            var sceneNames = new[] { "Title", "Staging", "MetaShop" };
            
            // Act
            OptimizedSceneEnumResolver.PrewarmCache(sceneNames);
            
            // Now resolve them - should all be cache hits
            foreach (var name in sceneNames)
            {
                OptimizedSceneEnumResolver.Resolve(name);
            }
            
            var stats = OptimizedSceneEnumResolver.GetCacheStatistics();
            
            // Assert
            Assert.GreaterOrEqual(stats.hits, sceneNames.Length, 
                "All prewarmed scenes should result in cache hits");
        }
        
        [Test]
        public void CacheStatistics_TracksHitsAndMisses()
        {
            // Arrange
            OptimizedSceneEnumResolver.ClearCaches();
            
            // Act - Some hits and misses
            OptimizedSceneEnumResolver.Resolve("Title");     // Should be hit (pre-cached)
            OptimizedSceneEnumResolver.Resolve("Title");     // Hit
            OptimizedSceneEnumResolver.Resolve("Unknown1");  // Miss
            OptimizedSceneEnumResolver.Resolve("Unknown2");  // Miss
            OptimizedSceneEnumResolver.Resolve("Title");     // Hit
            
            var stats = OptimizedSceneEnumResolver.GetCacheStatistics();
            
            // Assert
            Assert.Greater(stats.hits, 0, "Should have cache hits");
            Assert.Greater(stats.misses, 0, "Should have cache misses");
            Assert.Greater(stats.hitRate, 0.5f, "Hit rate should be > 50% for this test");
        }
        
        #endregion
        
        #region Performance Tests
        
        [Test]
        public void Resolve_Performance_CachedLookupsAreFast()
        {
            // Arrange
            const int iterations = 10000;
            var sceneName = "MetaShop";

            // Warm up caches - first call will be slower
            OptimizedSceneEnumResolver.Resolve(sceneName);

            // Act - Measure cached lookups (should be very fast)
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = OptimizedSceneEnumResolver.Resolve(sceneName);
            }
            sw.Stop();
            var cachedTime = sw.ElapsedMilliseconds;

            // Assert - Cached lookups should be very fast
            UnityEngine.Debug.Log($"Cached lookups for {iterations} iterations: {cachedTime}ms");
            Assert.Less(cachedTime, 50,
                "Cached lookups should be very fast (< 50ms for 10k iterations)");

            // Verify cache efficiency
            var stats = OptimizedSceneEnumResolver.GetCacheStatistics();
            Assert.Greater(stats.hitRate, 0.9f, "Hit rate should be > 90% after warm-up");
        }
        
        [Test]
        public void ResolveByHash_Performance_FastestMethod()
        {
            // Arrange
            const int iterations = 10000;
            var sceneName = "MetaShop";
            var sceneHash = sceneName.GetHashCode();
            
            // Act - Measure hash lookup
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = OptimizedSceneEnumResolver.ResolveByHash(sceneHash);
            }
            sw.Stop();
            
            // Assert
            UnityEngine.Debug.Log($"Hash lookup time for {iterations} iterations: {sw.ElapsedMilliseconds}ms");
            Assert.Less(sw.ElapsedMilliseconds, 100, 
                "Hash lookup should be extremely fast (< 100ms for 10k iterations)");
        }
        
        [Test]
        public void ResolveByHash_WorksCorrectly()
        {
            // This test verifies that ResolveByHash works correctly
            
            // Arrange
            var sceneName = Scenes.Core.PermaShop;
            var hash = sceneName.GetHashCode();
            
            // Act - Resolve by hash
            var result = OptimizedSceneEnumResolver.ResolveByHash(hash);
            
            // Assert
            Assert.AreEqual(sceneName, result, "Should resolve to correct scene by hash");
        }
        
        #endregion
    }
}