using NeonLadder.Mechanics.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Utilities
{
    /// <summary>
    /// Optimized version of SceneEnumResolver that uses caching to avoid repeated string operations.
    /// This class eliminates per-frame allocations and string comparisons for better performance.
    /// </summary>
    public static class OptimizedSceneEnumResolver
    {
        // Cache for resolved scene names to avoid repeated parsing
        private static readonly Dictionary<string, Scenes> sceneCache = new Dictionary<string, Scenes>();
        
        // Cache for enum to string conversions to avoid ToString() allocations
        private static readonly Dictionary<Scenes, string> enumToStringCache = new Dictionary<Scenes, string>();
        
        // Pre-computed hash codes for faster lookups
        private static readonly Dictionary<int, Scenes> hashToSceneCache = new Dictionary<int, Scenes>();
        
        // Performance counters for monitoring
        private static int cacheHits = 0;
        private static int cacheMisses = 0;
        
        static OptimizedSceneEnumResolver()
        {
            // Pre-populate caches with all known scene enums
            InitializeCaches();
        }
        
        /// <summary>
        /// Initializes all caches with known scene values to avoid runtime allocations
        /// </summary>
        private static void InitializeCaches()
        {
            // Pre-cache all enum values
            foreach (Scenes scene in Enum.GetValues(typeof(Scenes)))
            {
                string sceneName = scene.ToString();
                
                // Cache enum to string
                enumToStringCache[scene] = sceneName;
                
                // Cache string to enum
                sceneCache[sceneName] = scene;
                
                // Cache hash codes
                hashToSceneCache[sceneName.GetHashCode()] = scene;
                
                // Also cache lowercase variants for case-insensitive lookups
                string lowerName = sceneName.ToLower();
                if (!sceneCache.ContainsKey(lowerName))
                {
                    sceneCache[lowerName] = scene;
                }
            }
        }
        
        /// <summary>
        /// Resolves a scene name to its enum value using optimized caching.
        /// This method avoids repeated string operations and allocations.
        /// </summary>
        /// <param name="sceneName">The scene name to resolve</param>
        /// <returns>The corresponding Scenes enum value</returns>
        public static Scenes Resolve(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return default(Scenes);
            }
            
            // First, try direct cache lookup (fastest path)
            if (sceneCache.TryGetValue(sceneName, out Scenes cachedScene))
            {
                cacheHits++;
                return cachedScene;
            }
            
            // Second, try case-insensitive lookup
            string lowerName = sceneName.ToLower();
            if (sceneCache.TryGetValue(lowerName, out Scenes lowerCachedScene))
            {
                // Cache the exact case for future lookups
                sceneCache[sceneName] = lowerCachedScene;
                cacheHits++;
                return lowerCachedScene;
            }
            
            // Third, check for test scenes (special case)
            if (lowerName.Contains("test"))
            {
                cacheMisses++;
                return Scenes.Test;
            }
            
            // Finally, try enum parsing as last resort
            if (Enum.TryParse(sceneName, out Scenes parsedScene))
            {
                // Cache the result for future lookups
                sceneCache[sceneName] = parsedScene;
                cacheMisses++;
                return parsedScene;
            }
            
            // Unknown scene - log warning and return default
            Debug.LogWarning($"Scene name '{sceneName}' is unaccounted for. Cache stats: {cacheHits} hits, {cacheMisses} misses");
            cacheMisses++;
            return default(Scenes);
        }
        
        /// <summary>
        /// Resolves a scene name using its hash code for even faster lookups.
        /// This method is useful when the hash code is already computed.
        /// </summary>
        /// <param name="sceneNameHash">The hash code of the scene name</param>
        /// <returns>The corresponding Scenes enum value</returns>
        public static Scenes ResolveByHash(int sceneNameHash)
        {
            if (hashToSceneCache.TryGetValue(sceneNameHash, out Scenes scene))
            {
                cacheHits++;
                return scene;
            }
            
            cacheMisses++;
            return default(Scenes);
        }
        
        /// <summary>
        /// Gets the cached string representation of a scene enum without allocation.
        /// </summary>
        /// <param name="scene">The scene enum value</param>
        /// <returns>The cached string representation</returns>
        public static string GetCachedSceneName(Scenes scene)
        {
            if (enumToStringCache.TryGetValue(scene, out string sceneName))
            {
                return sceneName;
            }
            
            // Cache miss - generate and cache
            sceneName = scene.ToString();
            enumToStringCache[scene] = sceneName;
            return sceneName;
        }
        
        /// <summary>
        /// Clears all caches. Useful for testing or memory management.
        /// </summary>
        public static void ClearCaches()
        {
            sceneCache.Clear();
            enumToStringCache.Clear();
            hashToSceneCache.Clear();
            cacheHits = 0;
            cacheMisses = 0;
            
            // Re-initialize with default values
            InitializeCaches();
        }
        
        /// <summary>
        /// Gets performance statistics for monitoring cache effectiveness.
        /// </summary>
        /// <returns>A tuple containing (hits, misses, hit rate)</returns>
        public static (int hits, int misses, float hitRate) GetCacheStatistics()
        {
            int total = cacheHits + cacheMisses;
            float hitRate = total > 0 ? (float)cacheHits / total : 0f;
            return (cacheHits, cacheMisses, hitRate);
        }
        
        /// <summary>
        /// Pre-warms the cache with a specific scene name.
        /// Useful for ensuring scenes are cached before gameplay.
        /// </summary>
        /// <param name="sceneName">The scene name to cache</param>
        public static void PrewarmCache(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName) && !sceneCache.ContainsKey(sceneName))
            {
                Resolve(sceneName);
            }
        }
        
        /// <summary>
        /// Pre-warms the cache with multiple scene names.
        /// </summary>
        /// <param name="sceneNames">The scene names to cache</param>
        public static void PrewarmCache(params string[] sceneNames)
        {
            foreach (var sceneName in sceneNames)
            {
                PrewarmCache(sceneName);
            }
        }
    }
}