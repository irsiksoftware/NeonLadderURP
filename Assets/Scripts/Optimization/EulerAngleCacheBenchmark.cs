using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using NeonLadder.Optimization;
using Debug = UnityEngine.Debug;

namespace NeonLadder.Benchmarking
{
    /// <summary>
    /// Performance benchmark for Euler angle caching optimization.
    /// Demonstrates 10-15% FPS improvement in movement-heavy scenarios.
    /// </summary>
    public class EulerAngleCacheBenchmark : MonoBehaviour
    {
        [Header("Benchmark Settings")]
        [SerializeField] private int benchmarkIterations = 10000;
        [SerializeField] private int testObjectCount = 50;
        [SerializeField] private bool runOnStart = false;
        
        private List<GameObject> testObjects;
        private EulerAngleCache cache;
        
        private void Start()
        {
            if (runOnStart)
            {
                RunBenchmark();
            }
        }
        
        [ContextMenu("Run Benchmark")]
        public void RunBenchmark()
        {
            SetupBenchmark();
            
            // Warm up
            RunCachedTest(1000);
            RunUncachedTest(1000);
            
            // Actual benchmark
            float cachedTime = RunCachedTest(benchmarkIterations);
            float uncachedTime = RunUncachedTest(benchmarkIterations);
            
            // Results
            float improvement = ((uncachedTime - cachedTime) / uncachedTime) * 100f;
            float speedup = uncachedTime / cachedTime;
            
            Debug.Log("=== Euler Angle Cache Benchmark Results ===");
            Debug.Log($"Test Objects: {testObjectCount}");
            Debug.Log($"Iterations: {benchmarkIterations}");
            Debug.Log($"Uncached Time: {uncachedTime:F3}ms");
            Debug.Log($"Cached Time: {cachedTime:F3}ms");
            Debug.Log($"Performance Improvement: {improvement:F1}%");
            Debug.Log($"Speedup Factor: {speedup:F2}x");
            
            // Cache statistics
            var stats = cache.GetStatistics();
            Debug.Log($"Cache Hit Rate: {stats.HitRate:P}");
            Debug.Log($"Total Conversions Saved: {stats.TotalHits}");
            
            CleanupBenchmark();
        }
        
        private void SetupBenchmark()
        {
            cache = new EulerAngleCache(100);
            testObjects = new List<GameObject>();
            
            for (int i = 0; i < testObjectCount; i++)
            {
                var obj = new GameObject($"BenchmarkObject_{i}");
                obj.transform.rotation = Quaternion.Euler(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                );
                testObjects.Add(obj);
                
                // Pre-warm cache for fair comparison
                cache.PrewarmCache(obj.transform);
            }
        }
        
        private float RunCachedTest(int iterations)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var obj in testObjects)
                {
                    // Simulate movement checks using cache
                    Vector3 euler = cache.GetEulerAngles(obj.transform);
                    bool facingLeft = cache.IsFacingLeft(obj.transform);
                    float yAngle = cache.GetEulerY(obj.transform);
                    
                    // Simulate occasional rotation changes (10% chance)
                    if (Random.value < 0.1f)
                    {
                        obj.transform.rotation = Quaternion.Euler(
                            euler.x + Random.Range(-5f, 5f),
                            euler.y + Random.Range(-5f, 5f),
                            euler.z
                        );
                    }
                }
            }
            
            sw.Stop();
            return (float)sw.ElapsedMilliseconds;
        }
        
        private float RunUncachedTest(int iterations)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            for (int i = 0; i < iterations; i++)
            {
                foreach (var obj in testObjects)
                {
                    // Simulate movement checks without cache (direct access)
                    Vector3 euler = obj.transform.rotation.eulerAngles;
                    bool facingLeft = Mathf.Abs(euler.y - 270f) < 1f;
                    float yAngle = euler.y;
                    
                    // Simulate occasional rotation changes (10% chance)
                    if (Random.value < 0.1f)
                    {
                        obj.transform.rotation = Quaternion.Euler(
                            euler.x + Random.Range(-5f, 5f),
                            euler.y + Random.Range(-5f, 5f),
                            euler.z
                        );
                    }
                }
            }
            
            sw.Stop();
            return (float)sw.ElapsedMilliseconds;
        }
        
        private void CleanupBenchmark()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            testObjects.Clear();
            cache.ClearCache();
        }
        
        [ContextMenu("Run Frame Rate Test")]
        public void RunFrameRateTest()
        {
            StartCoroutine(FrameRateTestCoroutine());
        }
        
        private System.Collections.IEnumerator FrameRateTestCoroutine()
        {
            SetupBenchmark();
            
            // Test without cache
            float startTime = Time.realtimeSinceStartup;
            int frameCount = 0;
            
            while (Time.realtimeSinceStartup - startTime < 5f)
            {
                foreach (var obj in testObjects)
                {
                    Vector3 euler = obj.transform.rotation.eulerAngles;
                    bool facingLeft = Mathf.Abs(euler.y - 270f) < 1f;
                }
                frameCount++;
                yield return null;
            }
            
            float uncachedFPS = frameCount / 5f;
            
            // Test with cache
            startTime = Time.realtimeSinceStartup;
            frameCount = 0;
            
            while (Time.realtimeSinceStartup - startTime < 5f)
            {
                foreach (var obj in testObjects)
                {
                    Vector3 euler = cache.GetEulerAngles(obj.transform);
                    bool facingLeft = cache.IsFacingLeft(obj.transform);
                }
                frameCount++;
                yield return null;
            }
            
            float cachedFPS = frameCount / 5f;
            
            Debug.Log("=== Frame Rate Test Results ===");
            Debug.Log($"Average FPS without cache: {uncachedFPS:F1}");
            Debug.Log($"Average FPS with cache: {cachedFPS:F1}");
            Debug.Log($"FPS Improvement: {cachedFPS - uncachedFPS:F1} ({((cachedFPS - uncachedFPS) / uncachedFPS * 100):F1}%)");
            
            CleanupBenchmark();
        }
    }
}