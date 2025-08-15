using UnityEngine;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Gameplay
{
    /// <summary>
    /// Marks a gameobject as a spawnpoint in a scene.
    /// Enhanced to support directional spawning and validation.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Spawn Point Configuration")]
        [SerializeField] private string spawnPointName;
        [SerializeField] private TransitionDirection associatedDirection = TransitionDirection.Forward;
        [SerializeField] private bool isDefaultSpawnPoint = false;
        
        [Header("Validation")]
        [SerializeField] private float groundCheckDistance = 2f;
        [SerializeField] private LayerMask groundLayer = 1;
        [SerializeField] private bool validateOnStart = true;
        
        [Header("Visual")]
        [SerializeField] private bool showGizmos = true;
        [SerializeField] private Color gizmoColor = Color.green;
        [SerializeField] private float gizmoSize = 0.5f;
        
        public string SpawnPointName 
        { 
            get => string.IsNullOrEmpty(spawnPointName) ? gameObject.name : spawnPointName; 
            set => spawnPointName = value;
        }
        
        public TransitionDirection AssociatedDirection 
        { 
            get => associatedDirection; 
            set => associatedDirection = value;
        }
        
        public bool IsDefaultSpawnPoint 
        { 
            get => isDefaultSpawnPoint; 
            set => isDefaultSpawnPoint = value;
        }
        
        public Vector3 SpawnPosition => transform.position;
        
        private void Start()
        {
            if (validateOnStart)
            {
                ValidateSpawnPoint();
            }
            
            // Auto-register with SpawnPointManager if available
            var spawnManager = SpawnPointManager.Instance;
            if (spawnManager != null)
            {
                spawnManager.RegisterSpawnPoint(SpawnPointName, transform);
            }
        }
        
        public bool ValidateSpawnPoint()
        {
            bool isValid = true;
            
            // Check if spawn point is above ground
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, groundCheckDistance + 1f, groundLayer))
            {
                if (hit.distance > groundCheckDistance)
                {
                    Debug.LogWarning($"Spawn point '{SpawnPointName}' is too far from ground: {hit.distance}m", this);
                    isValid = false;
                }
            }
            else if (groundLayer != 0) // Only warn if ground layers are specified
            {
                Debug.LogWarning($"Spawn point '{SpawnPointName}' has no ground below it", this);
                isValid = false;
            }
            
            return isValid;
        }
        
        public void SetSpawnPointName(string name)
        {
            spawnPointName = name;
            
            // Auto-detect direction from name
            string upperName = name.ToUpperInvariant();
            if (upperName.Contains("LEFT"))
                associatedDirection = TransitionDirection.Left;
            else if (upperName.Contains("RIGHT"))
                associatedDirection = TransitionDirection.Right;
            else if (upperName.Contains("UP"))
                associatedDirection = TransitionDirection.Up;
            else if (upperName.Contains("DOWN"))
                associatedDirection = TransitionDirection.Down;
        }
        
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;
            
            Gizmos.color = gizmoColor;
            
            // Draw spawn point indicator
            Gizmos.DrawWireSphere(transform.position, gizmoSize);
            
            // Draw direction indicator
            Vector3 directionVector = GetDirectionVector();
            if (directionVector != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + directionVector * gizmoSize * 2);
            }
            
            // Draw ground check
            if (groundLayer != 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            
            // Draw detailed info when selected
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize * 2);
            
#if UNITY_EDITOR
            // Draw spawn name
            UnityEditor.Handles.Label(transform.position + Vector3.up, SpawnPointName);
#endif
        }
        
        private Vector3 GetDirectionVector()
        {
            switch (associatedDirection)
            {
                case TransitionDirection.Left: return Vector3.left;
                case TransitionDirection.Right: return Vector3.right;
                case TransitionDirection.Up: return Vector3.up;
                case TransitionDirection.Down: return Vector3.down;
                case TransitionDirection.Forward: return Vector3.forward;
                case TransitionDirection.Backward: return Vector3.back;
                default: return Vector3.zero;
            }
        }
    }
}