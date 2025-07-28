using Unity.Cinemachine;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using UnityEngine;
using UnityEngine.SceneManagement;

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
                    return virtualCamera ?? (virtualCamera = GameObject.FindGameObjectWithTag(Tags.GameController.ToString()).GetComponentInChildren<CinemachineCamera>());
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
