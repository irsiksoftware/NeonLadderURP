using UnityEngine;
using System.Linq;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Debug tool for testing spawn point teleportation directly in the Inspector
    /// </summary>
    public class SpawnTester : MonoBehaviour
    {
        public void TeleportToSpawnPoint(SpawnPointConfiguration spawnConfig)
        {
            if (spawnConfig == null)
                return;

            Player player = null;
            
            if (Game.Instance != null && Game.Instance.model != null && Game.Instance.model.Player != null)
            {
                player = Game.Instance.model.Player;
            }
            else
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.GetComponent<Player>();
                }
            }

            if (player == null)
                return;

            Vector3 spawnPosition = spawnConfig.transform.position;
            player.Teleport(spawnPosition);
            
            // Trigger any spawn initialization logic
            var sceneTransitionManager = GetComponentInParent<SceneTransitionManager>();
            if (sceneTransitionManager != null)
            {
                // Call the spawn initialization if there's any special logic needed
                // For now, just handle basic positioning
                
                // Handle facing direction based on spawn type
                var kinematicObject = player.GetComponent<KinematicObject>();
                if (kinematicObject != null)
                {
                    switch (spawnConfig.SpawnMode)
                    {
                        case SpawnPointType.Left:
                            kinematicObject.IsFacingLeft = false; // Face right when spawning from left
                            break;
                        case SpawnPointType.Right:
                            kinematicObject.IsFacingLeft = true; // Face left when spawning from right
                            break;
                        // Auto and others maintain current facing
                    }
                }
            }
        }

        public SpawnPointConfiguration[] GetAllSpawnPointsInScene()
        {
            return FindObjectsOfType<SpawnPointConfiguration>()
                .OrderBy(sp => sp.SpawnMode.ToString())
                .ThenBy(sp => sp.name)
                .ToArray();
        }
    }
}