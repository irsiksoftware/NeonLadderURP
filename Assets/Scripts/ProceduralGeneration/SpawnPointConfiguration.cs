using UnityEngine;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Configuration component for spawn points within SceneTransition prefabs.
    /// Allows customization of spawn behavior.
    /// </summary>
    public class SpawnPointConfiguration : MonoBehaviour
    {

        // Public properties
        public SpawnPointType SpawnMode => spawnMode;
        public string CustomSpawnName => customSpawnName;

        [Header("Spawn Point Settings")]
        [Tooltip("How to determine spawn position when players arrive at this point")]
        [SerializeField] private SpawnPointType spawnMode = SpawnPointType.Auto;

        [Tooltip("Custom spawn point name (only used when spawnMode is Custom)")]
        [SerializeField] private string customSpawnName = "";

        /// <summary>
        /// Get the actual world position where the player should spawn
        /// </summary>
        public Vector3 GetSpawnWorldPosition()
        {
            // Always use the transform position - FromCenter is just a spawn point name/tag
            // The SpawnPointManager will look for a spawn point named "FromCenter"
            Vector3 worldPos = transform.position;
            Vector3 localPos = transform.localPosition;
            
            UnityEngine.Debug.Log($"[SpawnPointConfiguration] {gameObject.name} - Local: {localPos}, World: {worldPos}");
            UnityEngine.Debug.Log($"[SpawnPointConfiguration] Parent: {transform.parent?.name ?? "null"} at {transform.parent?.position ?? Vector3.zero}");
            
            return worldPos;
        }
    }
}