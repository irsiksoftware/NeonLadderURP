using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using NeonLadder.Mechanics.Enums;
using NeonLadder.Debug;

namespace NeonLadder.Cameras
{
    public class CameraActivator : MonoBehaviour
    {
        [SerializeField]
        private string cameraName; // Name of the Cinemachine Virtual Camera in the hierarchy

        private List<CinemachineCamera> altCameras = new List<CinemachineCamera>();

        private void OnEnable()
        {
            DisableAndStoreAltCameras();
        }

        private void DisableAndStoreAltCameras()
        {
            // Find all GameObjects with the "AltCamera" tag
            GameObject[] taggedCameras = GameObject.FindGameObjectsWithTag(Tags.AltCamera.ToString());

            foreach (GameObject cameraObject in taggedCameras)
            {
                CinemachineCamera virtualCamera = cameraObject.GetComponent<CinemachineCamera>();

                if (virtualCamera != null)
                {
                    // Disable the camera and store the reference
                    NLDebug.LogWarning($"{virtualCamera.gameObject.name} with Cinemachine Virtual Cameras have been disabled and it's reference stored.");
                    virtualCamera.enabled = false;
                    altCameras.Add(virtualCamera);
                }
                else
                {
                    NLDebug.LogWarning($"GameObject '{cameraObject.name}' tagged as 'AltCamera' does not have a CinemachineCamera component.");
                }
            }

            NLDebug.Log("All AltCamera-tagged Cinemachine Virtual Cameras have been disabled and stored.");
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if the player entered the trigger
            if (other.CompareTag("Player"))
            {
                ActivateCamera();
            }
        }

        private void ActivateCamera()
        {
            if (string.IsNullOrEmpty(cameraName))
            {
                NLDebug.LogError("Camera name is not set in the CameraActivator script.");
                return;
            }

            // Find the camera by name in the hierarchy
            GameObject cameraObject = GameObject.Find(cameraName);

            if (cameraObject == null)
            {
                NLDebug.LogError($"Camera with name '{cameraName}' not found in the hierarchy.");
                return;
            }

            // Ensure the object has a CinemachineCamera component
            CinemachineCamera virtualCamera = cameraObject.GetComponent<CinemachineCamera>();

            if (virtualCamera == null)
            {
                NLDebug.LogError($"GameObject '{cameraName}' does not have a CinemachineCamera component.");
                return;
            }

            // Activate and enable the camera
            virtualCamera.gameObject.SetActive(true);
            var rotationController = virtualCamera.gameObject.GetComponent<RotationController>();
            rotationController.enabled = true;
            rotationController.StartAnimationManually();
            virtualCamera.enabled = true;

            NLDebug.Log($"Camera '{cameraName}' activated and enabled.");
        }
    }
}
