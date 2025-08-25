using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Real-time spawn point monitoring component for debugging
    /// </summary>
    public class SpawnPointMonitor : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnPointInfo
        {
            public string name;
            public Vector3 worldPosition;
            public Vector3 localPosition;
            public string spawnType;
            public string parentName;
            public bool isActive;
        }

        [Header("Monitor Settings")]
        [SerializeField] private bool autoRefresh = true;
        [SerializeField] private float refreshInterval = 0.5f;
        
        [Header("Live Spawn Point Data")]
        [SerializeField] private List<SpawnPointInfo> spawnPoints = new List<SpawnPointInfo>();
        
        [Header("Player Info")]
        [SerializeField] private Vector3 playerWorldPosition;
        [SerializeField] private Vector3 playerLocalPosition;
        [SerializeField] private string playerParentName = "None";
        [SerializeField] private string playerStatus = "Not Found";
        
        private float lastRefreshTime;

        private void Start()
        {
            RefreshSpawnPointData();
        }

        private void Update()
        {
            if (autoRefresh && Time.time - lastRefreshTime > refreshInterval)
            {
                RefreshSpawnPointData();
                lastRefreshTime = Time.time;
            }
        }

        public void RefreshSpawnPointData()
        {
            // Clear and update spawn points
            spawnPoints.Clear();
            
            var allSpawnConfigs = FindObjectsOfType<SpawnPointConfiguration>();
            
            foreach (var config in allSpawnConfigs)
            {
                var info = new SpawnPointInfo
                {
                    name = config.gameObject.name,
                    worldPosition = config.transform.position,
                    localPosition = config.transform.localPosition,
                    spawnType = config.SpawnMode.ToString(),
                    parentName = config.transform.parent != null ? config.transform.parent.name : "None",
                    isActive = config.gameObject.activeInHierarchy
                };
                
                if (!string.IsNullOrEmpty(config.CustomSpawnName))
                {
                    info.spawnType += $" ({config.CustomSpawnName})";
                }
                
                spawnPoints.Add(info);
            }
            
            // Sort by name for consistency
            spawnPoints = spawnPoints.OrderBy(sp => sp.name).ToList();
            
            // Update player info
            UpdatePlayerInfo();
        }

        private void UpdatePlayerInfo()
        {
            Player player = null;
            
            // Try to find player
            if (Game.Instance != null && Game.Instance.model != null && Game.Instance.model.Player != null)
            {
                player = Game.Instance.model.Player;
                playerStatus = "Found via Game.Instance";
            }
            else
            {
                var playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.GetComponent<Player>();
                    playerStatus = "Found via Tag";
                }
            }
            
            if (player != null)
            {
                playerWorldPosition = player.transform.position;
                playerLocalPosition = player.transform.localPosition;
                playerParentName = player.transform.parent != null ? player.transform.parent.name : "None (Root Level)";
            }
            else
            {
                playerStatus = "Not Found";
                playerWorldPosition = Vector3.zero;
                playerLocalPosition = Vector3.zero;
                playerParentName = "N/A";
            }
        }
    }
}