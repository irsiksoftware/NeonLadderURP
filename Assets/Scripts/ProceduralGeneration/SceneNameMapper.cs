using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    [Serializable]
    public class SceneNameMapper
    {
        private readonly Dictionary<string, string> eventTypeToScene = new Dictionary<string, string>
        {
            { "Portal", "Portal" },
            { "Shrine", "Shrine" },
            { "TreasureChest", "TreasureRoom" },
            { "MysteriousAltar", "MysteriousAltar" },
            { "Merchant", "Merchant" },
            { "Riddle", "RiddleRoom" },
            { "Shop", "Shop" },
            { "Rest", "RestArea" }
        };
        
        public string GetBossSceneFromIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return null;
            
            // Use BossLocationData to get the actual scene name
            var bossLocation = BossLocationData.GetByIdentifier(identifier);
            return bossLocation?.Identifier?.ToLowerInvariant();
        }
        
        public string GetServiceSceneFromEventType(string eventType)
        {
            if (string.IsNullOrEmpty(eventType))
                return null;
            
            if (eventTypeToScene.TryGetValue(eventType, out string sceneName))
                return sceneName;
            
            // Try case-insensitive match
            foreach (var kvp in eventTypeToScene)
            {
                if (kvp.Key.Equals(eventType, StringComparison.OrdinalIgnoreCase))
                    return kvp.Value;
            }
            
            Debug.LogWarning($"No service scene mapping found for event type: {eventType}");
            return null;
        }
        
        public string GetConnectionSceneName(string bossSceneName, int connectionNumber)
        {
            if (string.IsNullOrEmpty(bossSceneName))
                return null;
            
            connectionNumber = Mathf.Clamp(connectionNumber, 1, 2);
            return $"{bossSceneName}_Connection{connectionNumber}";
        }
        
        public bool IsServiceScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            
            var serviceScenes = new HashSet<string>
            {
                "Shop", "RestArea", "TreasureRoom", "Merchant",
                "Portal", "Shrine", "MysteriousAltar", "RiddleRoom"
            };
            
            return serviceScenes.Contains(sceneName);
        }
        
        public bool IsBossScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            
            return BossLocationData.Locations.Any(b => b.Identifier.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
        }
        
        public bool IsConnectionScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
            
            return sceneName.Contains("_Connection");
        }
        
        public List<string> GetAllBossSceneNames()
        {
            return BossLocationData.Locations.Select(b => b.Identifier).ToList();
        }
        
        public List<string> GetAllServiceSceneNames()
        {
            return new List<string>
            {
                "Shop", "RestArea", "TreasureRoom", "Merchant",
                "Portal", "Shrine", "MysteriousAltar", "RiddleRoom"
            };
        }
        
        public List<string> GetAllConnectionSceneNames()
        {
            var connections = new List<string>();
            var bosses = GetAllBossSceneNames();
            
            foreach (var boss in bosses)
            {
                connections.Add($"{boss}_Connection1");
                connections.Add($"{boss}_Connection2");
            }
            
            return connections;
        }
    }
}