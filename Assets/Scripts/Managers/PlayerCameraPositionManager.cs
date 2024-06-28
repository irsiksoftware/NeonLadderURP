using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Managers
{
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


        public void EmptySceneStates()
        {
            sceneStates.Clear();
        }

        public void SaveState(string sceneName, Vector3 playerPosition, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            sceneStates[sceneName] = new PlayerAndCameraState(playerPosition, cameraPosition, cameraRotation);
        }

        public bool TryGetState(string sceneName, out Vector3 playerPosition, out Vector3 cameraPosition, out Quaternion cameraRotation)
        {
            if (sceneStates.TryGetValue(sceneName, out PlayerAndCameraState state))
            {
                playerPosition = state.PlayerPosition;
                cameraPosition = state.CameraPosition;
                cameraRotation = state.CameraRotation;
                return true;
            }

            playerPosition = Vector3.zero;
            cameraPosition = Vector3.zero;
            cameraRotation = Quaternion.identity;
            return false;
        }
    }

    public class PlayerAndCameraState
    {
        public Vector3 PlayerPosition { get; }
        public Vector3 CameraPosition { get; }
        public Quaternion CameraRotation { get; }

        public PlayerAndCameraState(Vector3 playerPosition, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            PlayerPosition = playerPosition;
            CameraPosition = cameraPosition;
            CameraRotation = cameraRotation;
        }
    }

}