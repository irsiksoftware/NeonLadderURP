using NeonLadder.Mechanics.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Utilities
{
    /// <summary>
    /// Optimized version of SceneEnumResolver that uses caching to validate scene names.
    /// This class eliminates repeated string operations for better performance.
    /// Works with the new nested static class Scenes structure.
    /// </summary>
    public static class OptimizedSceneEnumResolver
    {
        // Cache for valid scene names (stores the correctly cased scene name if valid, null if invalid)
        private static readonly Dictionary<string, string> validationCache = new Dictionary<string, string>();
        
        // Set of all valid scene names for fast lookup
        private static readonly HashSet<string> validScenes = new HashSet<string>();
        
        // Pre-computed hash codes for faster lookups
        private static readonly Dictionary<int, string> hashToSceneCache = new Dictionary<int, string>();
        
        // Performance counters for monitoring
        private static int cacheHits = 0;
        private static int cacheMisses = 0;
        
        static OptimizedSceneEnumResolver()
        {
            // Pre-populate caches with all known scenes
            InitializeCaches();
        }
        
        /// <summary>
        /// Initializes all caches with known scene values to avoid runtime allocations
        /// </summary>
        private static void InitializeCaches()
        {
            // Add all scenes to the valid set
            AddScenesToSet(Scenes.Core.All);
            AddScenesToSet(Scenes.Boss.All);
            AddScenesToSet(Scenes.Connection.All);
            AddScenesToSet(Scenes.Service.All);
            AddScenesToSet(Scenes.Legacy.All);
            AddScenesToSet(Scenes.Packaged.All);
            AddScenesToSet(Scenes.Cutscene.All);
            
            // Pre-cache hash codes
            foreach (var scene in validScenes)
            {
                hashToSceneCache[scene.GetHashCode()] = scene;
                validationCache[scene] = scene; // Cache exact case

                // Also cache lowercase variants for case-insensitive lookups
                string lowerName = scene.ToLower();
                if (!validationCache.ContainsKey(lowerName))
                {
                    validationCache[lowerName] = scene; // Store the correctly cased version
                }
            }
        }
        
        private static void AddScenesToSet(string[] scenes)
        {
            foreach (var scene in scenes)
            {
                validScenes.Add(scene);
            }
        }
        
        /// <summary>
        /// Resolves a scene name to a validated scene string using optimized caching.
        /// This method avoids repeated string operations and allocations.
        /// </summary>
        /// <param name="sceneName">The scene name to resolve</param>
        /// <returns>The validated scene name or null if invalid</returns>
        public static string Resolve(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return null;
            }

            // First, try direct cache lookup (fastest path)
            if (validationCache.TryGetValue(sceneName, out string cachedResult))
            {
                cacheHits++;
                return cachedResult; // Returns correctly cased scene name or null
            }

            // Second, check if it's a valid scene (exact match)
            bool isValidScene = validScenes.Contains(sceneName);
            if (isValidScene)
            {
                validationCache[sceneName] = sceneName;
                cacheHits++;
                return sceneName;
            }

            // Third, check for test scenes (special case)
            string lowerName = sceneName.ToLower();
            if (lowerName.Contains("test"))
            {
                cacheMisses++;
                validationCache[sceneName] = Scenes.Core.Test;
                return Scenes.Core.Test;
            }

            // Fourth, try case-insensitive lookup
            var matchingScene = validScenes.FirstOrDefault(s => s.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
            if (matchingScene != null)
            {
                validationCache[sceneName] = matchingScene; // Cache the correctly cased version
                cacheMisses++;
                return matchingScene; // Return the correctly cased version
            }

            // Unknown scene - log warning and return null
            Debug.LogWarning($"Scene name '{sceneName}' is unaccounted for. Cache stats: {cacheHits} hits, {cacheMisses} misses");
            validationCache[sceneName] = null;
            cacheMisses++;
            return null;
        }
        
        /// <summary>
        /// Resolves a scene name using its hash code for even faster lookups.
        /// This method is useful when the hash code is already computed.
        /// </summary>
        /// <param name="sceneNameHash">The hash code of the scene name</param>
        /// <returns>The corresponding scene name or null</returns>
        public static string ResolveByHash(int sceneNameHash)
        {
            if (hashToSceneCache.TryGetValue(sceneNameHash, out string scene))
            {
                cacheHits++;
                return scene;
            }

            cacheMisses++;
            return null;
        }
        
        /// <summary>
        /// Checks if a scene name is valid without allocating memory for Unknown returns.
        /// </summary>
        /// <param name="sceneName">The scene name to check</param>
        /// <returns>True if the scene name is valid</returns>
        public static bool IsValidScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return false;
            }

            // Check cache first
            if (validationCache.TryGetValue(sceneName, out string cachedResult))
            {
                return cachedResult != null;
            }

            // Check valid scenes
            return validScenes.Contains(sceneName) ||
                   validScenes.Any(s => s.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Clears all caches. Useful for testing or memory management.
        /// </summary>
        public static void ClearCaches()
        {
            validationCache.Clear();
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
            if (!string.IsNullOrEmpty(sceneName) && !validationCache.ContainsKey(sceneName))
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