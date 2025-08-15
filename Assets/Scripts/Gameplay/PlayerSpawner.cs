using UnityEngine;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Gameplay
{
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Player References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private GameObject playerGameObject;
        
        [Header("Camera Integration")]
        [SerializeField] private bool updateCameraPosition = true;
        [SerializeField] private Camera playerCamera;
        
        [Header("Spawning Settings")]
        [SerializeField] private bool spawnOnSceneLoad = true;
        [SerializeField] private float spawnDelay = 0.1f;
        [SerializeField] private bool preservePlayerVelocity = false;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showSpawnGizmo = true;
        
        private Vector3 lastSpawnPosition;
        private SpawnPointManager spawnManager;
        
        private void Awake()
        {
            // Auto-find player if not assigned
            if (playerGameObject == null)
            {
                playerGameObject = GameObject.FindGameObjectWithTag("Player");
            }
            
            if (playerGameObject != null && playerTransform == null)
            {
                playerTransform = playerGameObject.transform;
            }
            
            // Auto-find camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null)
                {
                    playerCamera = FindObjectOfType<Camera>();
                }
            }
        }
        
        private void Start()
        {
            spawnManager = SpawnPointManager.Instance;
            
            if (spawnManager != null)
            {
                spawnManager.OnPlayerSpawned += OnPlayerSpawned;
            }
            
            if (spawnOnSceneLoad)
            {
                Invoke(nameof(SpawnPlayerWithContext), spawnDelay);
            }
        }
        
        private void OnDestroy()
        {
            if (spawnManager != null)
            {
                spawnManager.OnPlayerSpawned -= OnPlayerSpawned;
            }
        }
        
        public void SpawnPlayerWithContext()
        {
            if (spawnManager == null)
            {
                LogError("SpawnPointManager not found!");
                return;
            }
            
            if (playerTransform == null)
            {
                LogError("Player transform not assigned!");
                return;
            }
            
            Vector3 spawnPosition = spawnManager.SpawnPlayerWithContext();
            SpawnPlayerAtPosition(spawnPosition);
        }
        
        public void SpawnPlayerAtPosition(Vector3 position)
        {
            if (playerTransform == null)
            {
                LogError("Cannot spawn player - transform is null");
                return;
            }
            
            // Clear player velocity if requested
            if (!preservePlayerVelocity)
            {
                Rigidbody rb = playerGameObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                
                Rigidbody2D rb2D = playerGameObject.GetComponent<Rigidbody2D>();
                if (rb2D != null)
                {
                    rb2D.linearVelocity = Vector2.zero;
                    rb2D.angularVelocity = 0f;
                }
            }
            
            // Move player to spawn position
            playerTransform.position = position;
            lastSpawnPosition = position;
            
            // Update camera if enabled
            if (updateCameraPosition && playerCamera != null)
            {
                UpdateCameraPosition(position);
            }
            
            // Notify other systems
            OnPlayerSpawnedInternal(position);
            
            LogInfo($"Player spawned at position: {position}");
        }
        
        public void SpawnPlayerByDirection(TransitionDirection direction)
        {
            if (spawnManager == null)
            {
                LogError("SpawnPointManager not found!");
                return;
            }
            
            Vector3 spawnPosition = spawnManager.GetSpawnPositionByDirection(direction);
            SpawnPlayerAtPosition(spawnPosition);
        }
        
        public void SpawnPlayerAtNamedPoint(string spawnPointName)
        {
            if (spawnManager == null)
            {
                LogError("SpawnPointManager not found!");
                return;
            }
            
            Vector3 spawnPosition = spawnManager.GetSpawnPosition(spawnPointName);
            SpawnPlayerAtPosition(spawnPosition);
        }
        
        private void UpdateCameraPosition(Vector3 playerPosition)
        {
            // Check for PlayerCameraPositionManager first
            var cameraPositionManager = FindObjectOfType<MonoBehaviour>();
            if (cameraPositionManager != null && 
                cameraPositionManager.GetType().Name == "PlayerCameraPositionManager")
            {
                // Let the existing camera manager handle positioning
                LogInfo("PlayerCameraPositionManager found - delegating camera positioning");
                return;
            }
            
            // Simple camera follow if no specialized manager exists
            if (playerCamera != null)
            {
                Vector3 cameraPosition = playerCamera.transform.position;
                cameraPosition.x = playerPosition.x;
                cameraPosition.y = playerPosition.y;
                // Preserve camera's Z position for 2.5D setup
                playerCamera.transform.position = cameraPosition;
            }
        }
        
        private void OnPlayerSpawned(Vector3 position, string spawnPointName)
        {
            LogInfo($"Player spawned via SpawnPointManager at {spawnPointName}: {position}");
        }
        
        private void OnPlayerSpawnedInternal(Vector3 position)
        {
            // Enable player controls if they were disabled
            var playerInput = playerGameObject.GetComponent<MonoBehaviour>();
            if (playerInput != null && playerInput.GetType().Name.Contains("Input"))
            {
                playerInput.enabled = true;
            }
            
            // Re-enable player controller
            var playerController = playerGameObject.GetComponent<MonoBehaviour>();
            if (playerController != null && playerController.GetType().Name.Contains("Controller"))
            {
                playerController.enabled = true;
            }
            
            // Send spawn event to other systems
            SendMessage("OnPlayerSpawned", position, SendMessageOptions.DontRequireReceiver);
        }
        
        public Vector3 GetLastSpawnPosition()
        {
            return lastSpawnPosition;
        }
        
        public bool IsPlayerAtSpawnPosition()
        {
            if (playerTransform == null)
                return false;
                
            return Vector3.Distance(playerTransform.position, lastSpawnPosition) < 0.5f;
        }
        
        private void LogInfo(string message)
        {
            if (enableDebugLogging)
                Debug.Log($"[PlayerSpawner] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[PlayerSpawner] {message}");
        }
        
        private void OnDrawGizmosSelected()
        {
            if (showSpawnGizmo && lastSpawnPosition != Vector3.zero)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(lastSpawnPosition, 0.3f);
                Gizmos.DrawLine(lastSpawnPosition, lastSpawnPosition + Vector3.up * 2f);
            }
        }
        
        // Integration methods for other systems to call
        public static void SpawnPlayer(Vector3 position)
        {
            var spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPlayerAtPosition(position);
            }
        }
        
        public static void SpawnPlayerStaticByDirection(TransitionDirection direction)
        {
            var spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPlayerByDirection(direction);
            }
        }
        
        public static void SpawnPlayerStaticAtNamedPoint(string spawnPointName)
        {
            var spawner = FindObjectOfType<PlayerSpawner>();
            if (spawner != null)
            {
                spawner.SpawnPlayerAtNamedPoint(spawnPointName);
            }
        }
    }
}