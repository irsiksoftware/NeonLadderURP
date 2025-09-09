using Unity.Cinemachine;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Cameras;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonLadder.Debugging;

namespace NeonLadder.Models
{
    [System.Serializable]
    public class PlatformerModel
    {
        [SerializeField]
        private CinemachineCamera virtualCamera;
        public CinemachineCamera VirtualCamera
        {
            get
            {
                if (SceneManager.GetActiveScene().name != Scenes.Title.ToString())
                {
                    if (virtualCamera == null)
                    {
                        Debugger.Log($"[VirtualCamera] Looking for camera in scene: {SceneManager.GetActiveScene().name}");
                        
                        // First try to find by PlayerCamera tag (preferred)
                        var playerCameraObj = GameObject.FindGameObjectWithTag(Tags.PlayerCamera.ToString());
                        if (playerCameraObj != null)
                        {
                            Debugger.Log($"[VirtualCamera] Found object with PlayerCamera tag: {playerCameraObj.name}");
                            virtualCamera = playerCameraObj.GetComponent<CinemachineCamera>();
                            if (virtualCamera == null)
                            {
                                Debugger.LogError($"[VirtualCamera] Object '{playerCameraObj.name}' has PlayerCamera tag but no CinemachineCamera component!");
                            }
                        }
                        else
                        {
                            Debugger.Log("[VirtualCamera] No object found with PlayerCamera tag");
                        }
                        
                        // Fallback: try to find as child of GameController
                        if (virtualCamera == null)
                        {
                            var gameControllerObj = GameObject.FindGameObjectWithTag(Tags.GameController.ToString());
                            if (gameControllerObj != null)
                            {
                                Debugger.Log($"[VirtualCamera] Found GameController: {gameControllerObj.name}");
                                virtualCamera = gameControllerObj.GetComponentInChildren<CinemachineCamera>();
                                if (virtualCamera == null)
                                {
                                    Debugger.LogError($"[VirtualCamera] GameController '{gameControllerObj.name}' has no CinemachineCamera in children!");
                                }
                            }
                            else
                            {
                                Debugger.LogError("[VirtualCamera] No object found with GameController tag!");
                            }
                        }
                        
                        // Last resort: find any CinemachineCamera in scene
                        if (virtualCamera == null)
                        {
                            virtualCamera = Object.FindFirstObjectByType<CinemachineCamera>();
                            if (virtualCamera != null)
                            {
                                Debugger.LogWarning($"[VirtualCamera] Found CinemachineCamera by type search: {virtualCamera.name} (consider tagging it properly)");
                            }
                            else
                            {
                                Debugger.LogError("[VirtualCamera] No CinemachineCamera found anywhere in the scene!");
                            }
                        }
                    }
                    return virtualCamera;
                }
                return null;
            }
            set => virtualCamera = value;
        }

        [SerializeField]
        private Player player;
        public Player Player
        {
            get
            {
                if (player == null)
                {
                    var playerObject = GameObject.FindGameObjectWithTag(Tags.Player.ToString());
                    if (playerObject != null)
                    {
                        player = playerObject.GetComponentInChildren<Player>();
                    }
                }
                return player;
            }
            set => player = value;
        }

        [SerializeField]
        private Transform spawnPoint;
        public Transform SpawnPoint
        {
            get => spawnPoint ?? (spawnPoint = GameObject.FindGameObjectWithTag(Tags.SpawnPoint.ToString()).transform);
            set => spawnPoint = value;
        }

        [SerializeField]
        private float jumpModifier = 1.5f;
        public float JumpModifier
        {
            get => jumpModifier;
            set => jumpModifier = value;
        }

        [SerializeField]
        private float jumpDeceleration = 0.5f;
        public float JumpDeceleration
        {
            get => jumpDeceleration;
            set => jumpDeceleration = value;
        }

        [SerializeField]
        private float timeScaleMultiplier = 1f;
        public float TimeScaleMultiplier
        {
            get => timeScaleMultiplier;
            set => timeScaleMultiplier = value;
        }
    }
}
