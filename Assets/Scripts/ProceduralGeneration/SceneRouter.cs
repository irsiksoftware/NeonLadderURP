using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonLadder.ProceduralGeneration
{
    public class SceneRouter : MonoBehaviour
    {
        private static SceneRouter instance;
        public static SceneRouter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<SceneRouter>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("SceneRouter");
                        instance = go.AddComponent<SceneRouter>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [SerializeField] private SceneNameMapper sceneMapper;
        [SerializeField] private SceneRoutingContext routingContext;
        [SerializeField] private bool enableDebugLogging = true;
        
        private PathGenerator pathGenerator;
        private bool isTransitioning = false;
        
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action<string> OnSceneLoadFailed;
        
        public SceneRoutingContext CurrentContext => routingContext;
        public bool IsTransitioning => isTransitioning;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (sceneMapper == null)
                sceneMapper = new SceneNameMapper();
            
            if (routingContext == null)
                routingContext = SceneRoutingContext.Instance;
                
            // PathGenerator is not a MonoBehaviour, create instance if needed
            if (pathGenerator == null)
            {
                pathGenerator = new PathGenerator();
            }
        }
        
        public async Task<bool> LoadSceneFromMapNode(MapNode node, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (node == null)
            {
                LogError("Cannot load scene from null MapNode");
                return false;
            }
            
            if (isTransitioning)
            {
                LogWarning("Scene transition already in progress");
                return false;
            }
            
            string sceneName = GetSceneNameFromMapNode(node);
            if (string.IsNullOrEmpty(sceneName))
            {
                LogError($"Could not determine scene name for node: {node.Type}");
                OnSceneLoadFailed?.Invoke(sceneName);
                return false;
            }
            
            return await LoadSceneByName(sceneName, mode, node);
        }
        
        public string GetSceneNameFromMapNode(MapNode node)
        {
            if (node == null) return null;
            
            switch (node.Type)
            {
                case NodeType.Start:
                    return "MainCityHub";
                    
                case NodeType.Boss:
                    return GetBossSceneName(node);
                    
                case NodeType.RestShop:
                    return GetServiceSceneName(node, true);
                    
                case NodeType.Event:
                    return GetServiceSceneName(node, false);
                    
                case NodeType.Connection:
                    return GetConnectionSceneName(node);
                    
                default:
                    LogWarning($"Unknown node type: {node.Type}");
                    return null;
            }
        }
        
        private string GetBossSceneName(MapNode node)
        {
            if (node.Properties.TryGetValue("BossName", out object bossNameObj))
            {
                string bossName = bossNameObj.ToString();
                return sceneMapper.GetBossSceneFromIdentifier(bossName);
            }
            
            LogError("Boss node missing BossName property");
            return null;
        }
        
        private string GetServiceSceneName(MapNode node, bool isRestShop)
        {
            if (isRestShop)
            {
                bool useShop = UnityEngine.Random.value > 0.5f;
                return useShop ? "Shop" : "RestArea";
            }
            
            if (node.Properties.TryGetValue("EventType", out object eventTypeObj))
            {
                string eventType = eventTypeObj.ToString();
                return sceneMapper.GetServiceSceneFromEventType(eventType);
            }
            
            var serviceScenes = new[] { "Shop", "RestArea", "TreasureRoom", "Merchant", "Portal", "Shrine", "MysteriousAltar", "RiddleRoom" };
            return serviceScenes[UnityEngine.Random.Range(0, serviceScenes.Length)];
        }
        
        private string GetConnectionSceneName(MapNode node)
        {
            string baseName = DetermineConnectionBaseName(node);
            if (string.IsNullOrEmpty(baseName))
                return null;
                
            int connectionNumber = DetermineConnectionNumber(node);
            return $"{baseName}_Connection{connectionNumber}";
        }
        
        private string DetermineConnectionBaseName(MapNode node)
        {
            if (routingContext.CurrentPath != null && routingContext.CurrentPath.Count > 0)
            {
                var bossNode = routingContext.CurrentPath.FirstOrDefault(n => n.Type == NodeType.Boss);
                if (bossNode != null)
                {
                    return GetBossSceneName(bossNode);
                }
            }
            
            if (node.Properties.TryGetValue("TargetBoss", out object targetBossObj))
            {
                string targetBoss = targetBossObj.ToString();
                return sceneMapper.GetBossSceneFromIdentifier(targetBoss);
            }
            
            var defaultBosses = sceneMapper.GetAllBossSceneNames();
            return defaultBosses[UnityEngine.Random.Range(0, defaultBosses.Count)];
        }
        
        private int DetermineConnectionNumber(MapNode node)
        {
            if (node.Properties.TryGetValue("ConnectionIndex", out object indexObj))
            {
                if (int.TryParse(indexObj.ToString(), out int index))
                    return Mathf.Clamp(index, 1, 2);
            }
            
            if (routingContext.VisitedScenes.Any(s => s.Contains("_Connection1")))
                return 2;
                
            return 1;
        }
        
        public async Task<bool> LoadSceneByName(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, MapNode associatedNode = null)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                LogError("Cannot load scene with empty name");
                return false;
            }
            
            isTransitioning = true;
            
            try
            {
                LogInfo($"Loading scene: {sceneName}");
                OnSceneLoadStarted?.Invoke(sceneName);
                
                routingContext.PreviousScene = routingContext.CurrentScene;
                routingContext.CurrentScene = sceneName;
                routingContext.CurrentNode = associatedNode;
                
                if (!routingContext.VisitedScenes.Contains(sceneName))
                    routingContext.VisitedScenes.Add(sceneName);
                
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, mode);
                
                if (loadOperation == null)
                {
                    throw new Exception($"Failed to start loading scene: {sceneName}");
                }
                
                while (!loadOperation.isDone)
                {
                    await Task.Yield();
                }
                
                LogInfo($"Successfully loaded scene: {sceneName}");
                OnSceneLoadCompleted?.Invoke(sceneName);
                
                return true;
            }
            catch (Exception e)
            {
                LogError($"Failed to load scene '{sceneName}': {e.Message}");
                OnSceneLoadFailed?.Invoke(sceneName);
                return false;
            }
            finally
            {
                isTransitioning = false;
            }
        }
        
        public void SetCurrentPath(List<MapNode> path)
        {
            routingContext.CurrentPath = path;
            routingContext.CurrentPathIndex = 0;
        }
        
        public MapNode GetNextNodeInPath()
        {
            if (routingContext.CurrentPath == null || routingContext.CurrentPathIndex >= routingContext.CurrentPath.Count - 1)
                return null;
                
            routingContext.CurrentPathIndex++;
            return routingContext.CurrentPath[routingContext.CurrentPathIndex];
        }
        
        public MapNode GetPreviousNodeInPath()
        {
            if (routingContext.CurrentPath == null || routingContext.CurrentPathIndex <= 0)
                return null;
                
            routingContext.CurrentPathIndex--;
            return routingContext.CurrentPath[routingContext.CurrentPathIndex];
        }
        
        public void ResetRoutingContext()
        {
            if (routingContext != null)
            {
                routingContext.Reset();
            }
            else
            {
                routingContext = SceneRoutingContext.Instance;
            }
        }
        
        public bool ValidateSceneExists(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
                return false;
                
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                if (name.Equals(sceneName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }
        
        private void LogInfo(string message)
        {
            if (enableDebugLogging)
                Debug.Log($"[SceneRouter] {message}");
        }
        
        private void LogWarning(string message)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"[SceneRouter] {message}");
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[SceneRouter] {message}");
        }
    }
}