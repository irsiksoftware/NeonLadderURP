using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadder.Core;
using NeonLadder.Events;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Manages checkpoint saving and respawn positions across scenes
    /// Works alongside SceneTransitionManager to handle player respawning
    /// </summary>
    public class CheckpointManager : MonoBehaviour
    {
        #region Singleton
        
        private static CheckpointManager instance;
        public static CheckpointManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CheckpointManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("CheckpointManager");
                        instance = go.AddComponent<CheckpointManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Checkpoint Settings")]
        [SerializeField] private bool debugLogging = true;
        
        [Header("Visual Feedback")]
        [SerializeField] private bool showCheckpointFeedback = true;
        [SerializeField] private float feedbackDuration = 2f;
        
        #endregion
        
        #region Private Fields
        
        private Vector3 lastCheckpointPosition;
        private string lastCheckpointScene;
        private bool hasCheckpoint = false;
        private float lastCheckpointTime;
        
        #endregion
        
        #region Events
        
        public static event Action<Vector3, string> OnCheckpointSaved;
        public static event Action<Vector3> OnCheckpointRestored;
        
        #endregion
        
        #region Initialization
        
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
        
        /// <summary>
        /// Called by PlayerSpawn event when player successfully spawns
        /// This automatically saves the spawn location as a checkpoint
        /// </summary>
        public void OnPlayerSpawnCompleted(Vector3 spawnPosition)
        {
            SaveCheckpoint(spawnPosition);
            
            if (debugLogging)
            {
                Debug.Log($"CheckpointManager: Auto-saved checkpoint from PlayerSpawn at {spawnPosition}");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Save a checkpoint at the specified position
        /// </summary>
        public void SaveCheckpoint(Vector3 position)
        {
            lastCheckpointPosition = position;
            lastCheckpointScene = SceneManager.GetActiveScene().name;
            lastCheckpointTime = Time.time;
            hasCheckpoint = true;
            
            if (debugLogging)
            {
                Debug.Log($"CheckpointManager: Saved checkpoint at {position} in scene {lastCheckpointScene}");
            }
            
            OnCheckpointSaved?.Invoke(position, lastCheckpointScene);
        }
        
        /// <summary>
        /// Save checkpoint at player's current position
        /// </summary>
        public void SaveCheckpointAtPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SaveCheckpoint(player.transform.position);
            }
            else if (debugLogging)
            {
                Debug.LogWarning("CheckpointManager: Cannot save checkpoint - Player not found");
            }
        }
        
        /// <summary>
        /// Get the last saved checkpoint position
        /// </summary>
        public Vector3 GetLastCheckpoint()
        {
            if (!hasCheckpoint)
            {
                if (debugLogging)
                {
                    Debug.LogWarning("CheckpointManager: No checkpoint saved, returning Vector3.zero");
                }
                return Vector3.zero;
            }
            
            return lastCheckpointPosition;
        }
        
        /// <summary>
        /// Get the scene name of the last checkpoint
        /// </summary>
        public string GetLastCheckpointScene()
        {
            return lastCheckpointScene;
        }
        
        /// <summary>
        /// Check if we have a valid checkpoint
        /// </summary>
        public bool HasCheckpoint()
        {
            return hasCheckpoint;
        }
        
        /// <summary>
        /// Check if the checkpoint is in the current scene
        /// </summary>
        public bool IsCheckpointInCurrentScene()
        {
            return hasCheckpoint && lastCheckpointScene == SceneManager.GetActiveScene().name;
        }
        
        /// <summary>
        /// Clear the saved checkpoint
        /// </summary>
        public void ClearCheckpoint()
        {
            hasCheckpoint = false;
            lastCheckpointPosition = Vector3.zero;
            lastCheckpointScene = "";
            
            if (debugLogging)
            {
                Debug.Log("CheckpointManager: Checkpoint cleared");
            }
        }
        
        /// <summary>
        /// Restore player to last checkpoint and invoke event
        /// </summary>
        public void RestoreToCheckpoint()
        {
            if (hasCheckpoint)
            {
                OnCheckpointRestored?.Invoke(lastCheckpointPosition);
                
                if (debugLogging)
                {
                    Debug.Log($"CheckpointManager: Restored to checkpoint at {lastCheckpointPosition}");
                }
            }
        }
        
        /// <summary>
        /// Get time since last checkpoint was saved
        /// </summary>
        public float GetTimeSinceCheckpoint()
        {
            return hasCheckpoint ? Time.time - lastCheckpointTime : -1f;
        }
        
        #endregion
        
        #region Debug Methods
        
        [ContextMenu("Debug: Save Current Position")]
        private void DebugSaveCurrentPosition()
        {
            SaveCheckpointAtPlayer();
        }
        
        [ContextMenu("Debug: Log Checkpoint Info")]
        private void DebugLogCheckpointInfo()
        {
            Debug.Log($"Has Checkpoint: {hasCheckpoint}");
            Debug.Log($"Position: {lastCheckpointPosition}");
            Debug.Log($"Scene: {lastCheckpointScene}");
            Debug.Log($"Time Since Save: {GetTimeSinceCheckpoint()}s");
        }
        
        #endregion
    }
}