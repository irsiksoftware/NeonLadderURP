using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Gameplay
{
    public class SpawnPointManager : MonoBehaviour
    {
        private static SpawnPointManager instance;
        public static SpawnPointManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SpawnPointManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SpawnPointManager");
                        instance = go.AddComponent<SpawnPointManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Spawn Points")]
        [SerializeField] private Dictionary<string, Transform> namedSpawnPoints = new Dictionary<string, Transform>();
        [SerializeField] private Dictionary<TransitionDirection, Transform> directionalSpawnPoints = new Dictionary<TransitionDirection, Transform>();
        
        [Header("Configuration")]
        [SerializeField] private float spawnOffset = 0.5f;
        [SerializeField] private bool autoDiscoverSpawnPoints = true;
        [SerializeField] private string defaultSpawnPointTag = "SpawnPoint";
        [SerializeField] private LayerMask groundCheckLayer = 1;
        
        [Header("Fallback Settings")]
        [SerializeField] private Vector3 fallbackSpawnPosition = Vector3.zero;
        [SerializeField] private bool useSceneCenterAsFallback = true;
        [SerializeField] private bool enableDebugLogging = true;
        
        // Events
        public event Action<Vector3, string> OnPlayerSpawned;
        public event Action<string> OnSpawnPointNotFound;
        
        // Current spawn context
        private TransitionDirection lastTransitionDirection = TransitionDirection.Forward;
        private string requestedSpawnPointName;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            if (autoDiscoverSpawnPoints)
            {
                DiscoverSpawnPoints();
            }
        }
        
        public void DiscoverSpawnPoints()
        {
            namedSpawnPoints.Clear();
            directionalSpawnPoints.Clear();
            
            // Find all SpawnPoint components
            var spawnPoints = FindObjectsOfType<SpawnPoint>();
            
            foreach (var spawnPoint in spawnPoints)
            {
                RegisterSpawnPointInternal(spawnPoint.gameObject);
            }
            
            // Also find GameObjects with SpawnPoint tag
            var taggedSpawnPoints = GameObject.FindGameObjectsWithTag(defaultSpawnPointTag);
            
            foreach (var spawnPoint in taggedSpawnPoints)
            {
                if (!spawnPoints.Any(sp => sp.gameObject == spawnPoint))
                {
                    RegisterSpawnPointInternal(spawnPoint);
                }
            }
            
            LogInfo($"Discovered {namedSpawnPoints.Count} named spawn points and {directionalSpawnPoints.Count} directional spawn points");
        }
        
        private void RegisterSpawnPointInternal(GameObject spawnPointObject)
        {
            string spawnPointName = spawnPointObject.name.ToUpperInvariant();
            Transform spawnTransform = spawnPointObject.transform;
            
            // Register by exact name
            if (!namedSpawnPoints.ContainsKey(spawnPointName))
            {
                namedSpawnPoints[spawnPointName] = spawnTransform;
            }
            
            // Parse directional spawn points
            TransitionDirection direction = ParseDirectionFromName(spawnPointName);
            if (direction != TransitionDirection.Forward && !directionalSpawnPoints.ContainsKey(direction))
            {
                directionalSpawnPoints[direction] = spawnTransform;
            }
            
            LogInfo($"Registered spawn point: {spawnPointName}");
        }
        
        private TransitionDirection ParseDirectionFromName(string name)
        {
            if (name.Contains("LEFT") && name.Contains("INSCENE"))
                return TransitionDirection.Right; // Player enters from left, so they came from right
                
            if (name.Contains("RIGHT") && name.Contains("INSCENE"))
                return TransitionDirection.Left; // Player enters from right, so they came from left
                
            if (name.Contains("UP") && name.Contains("INSCENE"))
                return TransitionDirection.Down; // Player enters from up, so they came from down
                
            if (name.Contains("DOWN") && name.Contains("INSCENE"))
                return TransitionDirection.Up; // Player enters from down, so they came from up
                
            return TransitionDirection.Forward; // Default
        }
        
        public Vector3 GetSpawnPosition(string spawnPointName)
        {
            if (string.IsNullOrEmpty(spawnPointName))
            {
                LogWarning("Spawn point name is empty, using fallback");
                return GetFallbackSpawnPosition();
            }
            
            string upperName = spawnPointName.ToUpperInvariant();
            
            // Try exact match first
            if (namedSpawnPoints.TryGetValue(upperName, out Transform exactMatch))
            {
                Vector3 position = CalculateSpawnPosition(exactMatch.position);
                LogInfo($"Spawning at named spawn point: {spawnPointName} -> {position}");
                OnPlayerSpawned?.Invoke(position, spawnPointName);
                return position;
            }
            
            // Try partial match
            var partialMatch = namedSpawnPoints.FirstOrDefault(kvp => 
                kvp.Key.Contains(upperName) || upperName.Contains(kvp.Key));
                
            if (partialMatch.Value != null)
            {
                Vector3 position = CalculateSpawnPosition(partialMatch.Value.position);
                LogInfo($"Spawning at partial match spawn point: {partialMatch.Key} -> {position}");
                OnPlayerSpawned?.Invoke(position, partialMatch.Key);
                return position;
            }
            
            LogWarning($"Spawn point not found: {spawnPointName}");
            OnSpawnPointNotFound?.Invoke(spawnPointName);
            return GetFallbackSpawnPosition();
        }
        
        public Vector3 GetSpawnPositionByDirection(TransitionDirection fromDirection)
        {
            // Convert transition direction to spawn direction
            TransitionDirection spawnDirection = GetSpawnDirectionFromTransition(fromDirection);
            
            if (directionalSpawnPoints.TryGetValue(spawnDirection, out Transform spawnPoint))
            {
                Vector3 position = CalculateSpawnPosition(spawnPoint.position);
                LogInfo($"Spawning by direction: {fromDirection} -> {spawnDirection} -> {position}");
                OnPlayerSpawned?.Invoke(position, $"Direction_{spawnDirection}");
                return position;
            }
            
            // Try named directional spawn points as fallback
            string[] directionNames = GetDirectionNames(spawnDirection);
            
            foreach (string directionName in directionNames)
            {
                var matchingSpawnPoint = namedSpawnPoints.FirstOrDefault(kvp => 
                    kvp.Key.Contains(directionName));
                    
                if (matchingSpawnPoint.Value != null)
                {
                    Vector3 position = CalculateSpawnPosition(matchingSpawnPoint.Value.position);
                    LogInfo($"Spawning by named direction: {directionName} -> {position}");
                    OnPlayerSpawned?.Invoke(position, matchingSpawnPoint.Key);
                    return position;
                }
            }
            
            LogWarning($"No spawn point found for direction: {fromDirection}");
            OnSpawnPointNotFound?.Invoke($"Direction_{fromDirection}");
            return GetFallbackSpawnPosition();
        }
        
        private TransitionDirection GetSpawnDirectionFromTransition(TransitionDirection transitionDirection)
        {
            // Opposite direction logic: if player exited left, spawn on right side of next scene
            switch (transitionDirection)
            {
                case TransitionDirection.Left: return TransitionDirection.Right;
                case TransitionDirection.Right: return TransitionDirection.Left;
                case TransitionDirection.Up: return TransitionDirection.Down;
                case TransitionDirection.Down: return TransitionDirection.Up;
                default: return TransitionDirection.Forward;
            }
        }
        
        private string[] GetDirectionNames(TransitionDirection direction)
        {
            switch (direction)
            {
                case TransitionDirection.Left:
                    return new[] { "LEFT", "LEFTSIDE", "WEST" };
                case TransitionDirection.Right:
                    return new[] { "RIGHT", "RIGHTSIDE", "EAST" };
                case TransitionDirection.Up:
                    return new[] { "UP", "TOP", "NORTH", "UPPER" };
                case TransitionDirection.Down:
                    return new[] { "DOWN", "BOTTOM", "SOUTH", "LOWER" };
                default:
                    return new[] { "DEFAULT", "CENTER", "MIDDLE" };
            }
        }
        
        private Vector3 CalculateSpawnPosition(Vector3 basePosition)
        {
            Vector3 spawnPosition = basePosition;
            
            // Apply spawn offset
            if (spawnOffset > 0)
            {
                spawnPosition.y += spawnOffset;
            }
            
            // Validate spawn position is above ground
            if (IsPositionValidForSpawning(spawnPosition))
            {
                return spawnPosition;
            }
            
            // Try to find valid ground position nearby
            Vector3 validPosition = FindNearbyValidPosition(spawnPosition);
            if (validPosition != Vector3.zero)
            {
                return validPosition;
            }
            
            LogWarning($"Spawn position validation failed for {basePosition}, using as-is");
            return spawnPosition;
        }
        
        private bool IsPositionValidForSpawning(Vector3 position)
        {
            // Check if there's ground below the spawn point
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 5f, groundCheckLayer))
            {
                return hit.distance < 3f; // Within reasonable distance of ground
            }
            
            return true; // If no ground check layers defined, assume valid
        }
        
        private Vector3 FindNearbyValidPosition(Vector3 originalPosition)
        {
            float searchRadius = 2f;
            int attempts = 8;
            
            for (int i = 0; i < attempts; i++)
            {
                float angle = (360f / attempts) * i;
                Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                Vector3 testPosition = originalPosition + direction * searchRadius;
                
                if (IsPositionValidForSpawning(testPosition))
                {
                    return testPosition;
                }
            }
            
            return Vector3.zero; // No valid position found
        }
        
        private Vector3 GetFallbackSpawnPosition()
        {
            if (useSceneCenterAsFallback)
            {
                // Try to find scene center or a reasonable default
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 cameraPosition = mainCamera.transform.position;
                    fallbackSpawnPosition = new Vector3(cameraPosition.x, cameraPosition.y, cameraPosition.z);
                }
            }
            
            LogInfo($"Using fallback spawn position: {fallbackSpawnPosition}");
            OnPlayerSpawned?.Invoke(fallbackSpawnPosition, "Fallback");
            return fallbackSpawnPosition;
        }
        
        public void RegisterSpawnPoint(string name, Transform point)
        {
            if (string.IsNullOrEmpty(name) || point == null)
            {
                LogError("Cannot register spawn point with empty name or null transform");
                return;
            }
            
            string upperName = name.ToUpperInvariant();
            namedSpawnPoints[upperName] = point;
            
            // Also register as directional if applicable
            TransitionDirection direction = ParseDirectionFromName(upperName);
            if (direction != TransitionDirection.Forward)
            {
                directionalSpawnPoints[direction] = point;
            }
            
            LogInfo($"Manually registered spawn point: {name}");
        }
        
        public void SetTransitionContext(TransitionDirection direction, string requestedSpawnPoint = null)
        {
            lastTransitionDirection = direction;
            requestedSpawnPointName = requestedSpawnPoint;
        }
        
        public Vector3 SpawnPlayerWithContext()
        {
            if (!string.IsNullOrEmpty(requestedSpawnPointName))
            {
                return GetSpawnPosition(requestedSpawnPointName);
            }
            
            return GetSpawnPositionByDirection(lastTransitionDirection);
        }
        
        public bool ValidateSpawnPoints()
        {
            int issues = 0;
            
            // Check for essential spawn points
            string[] essentialDirections = { "LEFT-INSCENE", "RIGHT-INSCENE" };
            
            foreach (string direction in essentialDirections)
            {
                if (!namedSpawnPoints.ContainsKey(direction + "-SPAWNPOINT"))
                {
                    LogWarning($"Missing essential spawn point: {direction}-SPAWNPOINT");
                    issues++;
                }
            }
            
            // Validate spawn point positions
            foreach (var kvp in namedSpawnPoints)
            {
                if (!IsPositionValidForSpawning(kvp.Value.position))
                {
                    LogWarning($"Spawn point '{kvp.Key}' may be in invalid position: {kvp.Value.position}");
                    issues++;
                }
            }
            
            LogInfo($"Spawn point validation complete. Found {issues} issues.");
            return issues == 0;
        }
        
        public Dictionary<string, Transform> GetAllNamedSpawnPoints()
        {
            return new Dictionary<string, Transform>(namedSpawnPoints);
        }
        
        public Dictionary<TransitionDirection, Transform> GetAllDirectionalSpawnPoints()
        {
            return new Dictionary<TransitionDirection, Transform>(directionalSpawnPoints);
        }
        
        private void LogInfo(string message)
        {
            if (enableDebugLogging)
                Debug.Log($"[SpawnPointManager] {message}");
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[SpawnPointManager] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SpawnPointManager] {message}");
        }
        
        private void OnDrawGizmosSelected()
        {
            if (namedSpawnPoints == null) return;
            
            Gizmos.color = Color.green;
            foreach (var spawnPoint in namedSpawnPoints.Values)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + Vector3.up);
                }
            }
            
            Gizmos.color = Color.blue;
            foreach (var dirSpawnPoint in directionalSpawnPoints.Values)
            {
                if (dirSpawnPoint != null)
                {
                    Gizmos.DrawWireCube(dirSpawnPoint.position, Vector3.one * 0.3f);
                }
            }
        }
    }
}