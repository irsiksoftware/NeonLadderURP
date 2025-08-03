using UnityEngine;
using Unity.Cinemachine;

namespace NeonLadder.Cameras
{
    /// <summary>
    /// Provides camera rotation information for save states.
    /// Attach this to the GameObject with CinemachineCamera component.
    /// </summary>
    public class CameraRotationProvider : MonoBehaviour
    {
        private CinemachineCamera cinemachineCamera;
        
        /// <summary>
        /// Gets the current camera rotation for save state purposes.
        /// Returns the GameObject's rotation regardless of Cinemachine settings.
        /// </summary>
        public Quaternion CurrentRotation => transform.rotation;
        
        /// <summary>
        /// Static helper to find and get camera rotation from any CameraRotationProvider in scene.
        /// </summary>
        public static Quaternion GetCameraRotation()
        {
            var provider = FindObjectOfType<CameraRotationProvider>();
            if (provider != null)
            {
                return provider.CurrentRotation;
            }
            
            // Fallback: try to find any CinemachineCamera
            var cinemachineCamera = FindObjectOfType<CinemachineCamera>();
            if (cinemachineCamera != null)
            {
                Debug.LogWarning("No CameraRotationProvider found, using CinemachineCamera transform directly");
                return cinemachineCamera.transform.rotation;
            }
            
            Debug.LogWarning("No camera found for rotation, using identity");
            return Quaternion.identity;
        }
        
        void Awake()
        {
            cinemachineCamera = GetComponent<CinemachineCamera>();
            if (cinemachineCamera == null)
            {
                Debug.LogError($"CameraRotationProvider on {gameObject.name} requires a CinemachineCamera component!");
            }
        }
    }
}