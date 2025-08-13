using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    [Serializable]
    public class SceneRoutingContext
    {
        [Header("Current State")]
        public string currentScene;
        public string previousScene;
        public MapNode currentNode;
        
        [Header("Path Progress")]
        public List<MapNode> currentPath;
        public int currentPathIndex;
        
        [Header("History")]
        public List<string> visitedScenes;
        public Dictionary<string, object> persistentData;
        
        [Header("Transition Context")]
        public TransitionDirection lastTransitionDirection;
        public string intendedSpawnPoint;
        public float transitionStartTime;
        
        public string CurrentScene
        {
            get => currentScene;
            set => currentScene = value;
        }
        
        public string PreviousScene
        {
            get => previousScene;
            set => previousScene = value;
        }
        
        public MapNode CurrentNode
        {
            get => currentNode;
            set => currentNode = value;
        }
        
        public List<MapNode> CurrentPath
        {
            get => currentPath ?? (currentPath = new List<MapNode>());
            set => currentPath = value;
        }
        
        public int CurrentPathIndex
        {
            get => currentPathIndex;
            set => currentPathIndex = Mathf.Max(0, value);
        }
        
        public List<string> VisitedScenes
        {
            get => visitedScenes ?? (visitedScenes = new List<string>());
            set => visitedScenes = value;
        }
        
        public Dictionary<string, object> PersistentData
        {
            get => persistentData ?? (persistentData = new Dictionary<string, object>());
            set => persistentData = value;
        }
        
        public TransitionDirection LastTransitionDirection
        {
            get => lastTransitionDirection;
            set => lastTransitionDirection = value;
        }
        
        public string IntendedSpawnPoint
        {
            get => intendedSpawnPoint;
            set => intendedSpawnPoint = value;
        }
        
        public SceneRoutingContext()
        {
            currentScene = "";
            previousScene = "";
            currentNode = null;
            currentPath = new List<MapNode>();
            currentPathIndex = 0;
            visitedScenes = new List<string>();
            persistentData = new Dictionary<string, object>();
            lastTransitionDirection = TransitionDirection.Forward;
            intendedSpawnPoint = "";
            transitionStartTime = 0f;
        }
        
        public bool HasVisitedScene(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && VisitedScenes.Contains(sceneName);
        }
        
        public void AddVisitedScene(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName) && !HasVisitedScene(sceneName))
            {
                VisitedScenes.Add(sceneName);
            }
        }
        
        public MapNode GetNodeAtIndex(int index)
        {
            if (CurrentPath == null || index < 0 || index >= CurrentPath.Count)
                return null;
            
            return CurrentPath[index];
        }
        
        public MapNode GetNextNode()
        {
            return GetNodeAtIndex(CurrentPathIndex + 1);
        }
        
        public MapNode GetPreviousNode()
        {
            return GetNodeAtIndex(CurrentPathIndex - 1);
        }
        
        public bool IsAtPathEnd()
        {
            return CurrentPath == null || CurrentPathIndex >= CurrentPath.Count - 1;
        }
        
        public bool IsAtPathStart()
        {
            return CurrentPathIndex <= 0;
        }
        
        public int GetPathProgress()
        {
            if (CurrentPath == null || CurrentPath.Count == 0)
                return 0;
            
            return Mathf.RoundToInt((float)CurrentPathIndex / (CurrentPath.Count - 1) * 100);
        }
        
        public void SetPersistentData(string key, object value)
        {
            PersistentData[key] = value;
        }
        
        public T GetPersistentData<T>(string key, T defaultValue = default(T))
        {
            if (PersistentData.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)value;
                }
                catch (InvalidCastException)
                {
                    Debug.LogWarning($"Cannot cast persistent data '{key}' to type {typeof(T)}");
                    return defaultValue;
                }
            }
            
            return defaultValue;
        }
        
        public bool HasPersistentData(string key)
        {
            return PersistentData.ContainsKey(key);
        }
        
        public void RemovePersistentData(string key)
        {
            PersistentData.Remove(key);
        }
        
        public void ClearPersistentData()
        {
            PersistentData.Clear();
        }
        
        public void Reset()
        {
            currentScene = "";
            previousScene = "";
            currentNode = null;
            currentPath?.Clear();
            currentPathIndex = 0;
            visitedScenes?.Clear();
            persistentData?.Clear();
            lastTransitionDirection = TransitionDirection.Forward;
            intendedSpawnPoint = "";
            transitionStartTime = 0f;
        }
        
        public SceneRoutingContextSnapshot CreateSnapshot()
        {
            return new SceneRoutingContextSnapshot
            {
                CurrentScene = currentScene,
                PreviousScene = previousScene,
                CurrentPathIndex = currentPathIndex,
                VisitedScenes = new List<string>(VisitedScenes),
                LastTransitionDirection = lastTransitionDirection,
                IntendedSpawnPoint = intendedSpawnPoint,
                SnapshotTime = Time.time
            };
        }
        
        public void RestoreFromSnapshot(SceneRoutingContextSnapshot snapshot)
        {
            if (snapshot == null)
                return;
            
            currentScene = snapshot.CurrentScene;
            previousScene = snapshot.PreviousScene;
            currentPathIndex = snapshot.CurrentPathIndex;
            visitedScenes = new List<string>(snapshot.VisitedScenes);
            lastTransitionDirection = snapshot.LastTransitionDirection;
            intendedSpawnPoint = snapshot.IntendedSpawnPoint;
        }
    }
    
    [Serializable]
    public class SceneRoutingContextSnapshot
    {
        public string CurrentScene;
        public string PreviousScene;
        public int CurrentPathIndex;
        public List<string> VisitedScenes;
        public TransitionDirection LastTransitionDirection;
        public string IntendedSpawnPoint;
        public float SnapshotTime;
    }
    
    public enum TransitionDirection
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
}