using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Models
{
    [System.Serializable]
    public class PlatformerModel
    {
        [SerializeField]
        private Cinemachine.CinemachineVirtualCamera virtualCamera;
        public Cinemachine.CinemachineVirtualCamera VirtualCamera
        {
            get
            {
                if (virtualCamera == null)
                {
                    var cameraObject = GameObject.Find("PlayerCamera");
                    if (cameraObject != null)
                    {
                        virtualCamera = cameraObject.GetComponent<Cinemachine.CinemachineVirtualCamera>();
                    }
                }
                return virtualCamera;
            }
            set => virtualCamera = value;
        }

        [SerializeField]
        private Player player;
        public Player Player
        {
            get => player ?? (player = Object.FindObjectOfType<Player>());
            set => player = value;
        }

        [SerializeField]
        private Transform spawnPoint;
        public Transform SpawnPoint
        {
            get => spawnPoint ?? (spawnPoint = GameObject.FindWithTag("SpawnPoint")?.transform);
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
