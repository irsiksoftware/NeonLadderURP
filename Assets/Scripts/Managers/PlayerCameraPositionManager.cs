namespace NeonLadder.Managers
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PlayerCameraPositionManager : MonoBehaviour
    {

        void Start()
        {
            enabled = false;
        }

        void Awake()
        {
            
        }
        private Dictionary<string, PlayerAndCameraState> sceneStates = new Dictionary<string, PlayerAndCameraState>();

        public void SaveState(string sceneName, Vector3 playerPosition, Quaternion cameraRotation)
        {
            sceneStates[sceneName] = new PlayerAndCameraState(playerPosition, cameraRotation);
        }

        public bool TryGetState(string sceneName, out Vector3 playerPosition, out Quaternion cameraRotation)
        {
            if (sceneStates.TryGetValue(sceneName, out PlayerAndCameraState state))
            {
                playerPosition = state.PlayerPosition;
                cameraRotation = state.CameraRotation;
                return true;
            }

            playerPosition = Vector3.zero;
            cameraRotation = Quaternion.identity;
            return false;
        }
    }

    public class PlayerAndCameraState
    {
        public Vector3 PlayerPosition { get; }
        public Vector3 CameraPosition { get; }
        public Quaternion CameraRotation { get; }

        public PlayerAndCameraState(Vector3 playerPosition, Quaternion cameraRotation)
        {
            PlayerPosition = playerPosition;
            CameraRotation = cameraRotation;
        }
    }

}