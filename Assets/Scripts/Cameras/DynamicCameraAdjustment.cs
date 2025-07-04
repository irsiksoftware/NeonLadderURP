using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Cameras
{
    public class DynamicCameraAdjustment : MonoBehaviour
    {
        public float DynamicCameraOffsetChangeSpeed = 2f; // Speed factor for the transition
        public float MinimumCameraDistance = 2.4f; // Smallest float that the FramingTransposer's Camera Distance will reach before giving up trying
        public float MinimumTrackedObjectOffsetY = 1f; // Minimum Y offset for the tracked object
        public List<string> TagsToIgnore;
        public List<string> LayersToIgnore;
        private CinemachineCamera CinemachineCamera;
        private Transform playerTransform;
        private float initialCameraDistance;
        private Vector3 initialCameraPosition;
        private Quaternion initialCameraRotation;
        private CinemachinePositionComposer framingTransposer;
        private Vector3 initialTrackedObjectOffset;
        private List<Renderer> renderers;
        private Renderer lastBlockingObject;

        private void Start()
        {
            CacheInitialSettings();
        }

        void Awake()
        {
            //CacheInitialSettings();
            //enabled = false;
        }

        void OnEnable()
        {
            CinemachineCamera = GetComponent<CinemachineCamera>();
            framingTransposer = CinemachineCamera.GetComponent<CinemachinePositionComposer>();
            playerTransform = GameObject.FindWithTag(Tags.Player.ToString()).transform; // Assume the player has the tag "Player"
            RefreshRenderers();
            //CacheInitialSettings();
            CinemachineCamera.enabled = true;
        }

        private void OnDisable()
        {
            //ResetToInitialSettings();
            CinemachineCamera.enabled = false;
            CinemachineCamera.enabled = true;
        }

        void Update()
        {
            AdjustCameraIfNeeded();
        }

        private void CacheInitialSettings()
        {
            initialCameraDistance = framingTransposer.CameraDistance;
            initialTrackedObjectOffset = framingTransposer.TargetOffset;
            initialCameraPosition = transform.position;
            initialCameraRotation = transform.rotation;
        }

        public void ResetToInitialSettings()
        {
            framingTransposer.CameraDistance = initialCameraDistance;
            framingTransposer.TargetOffset = initialTrackedObjectOffset;
            transform.position = initialCameraPosition;
            transform.rotation = initialCameraRotation;
        }

        public void RefreshRenderers()
        {
            renderers = new List<Renderer>(FindObjectsOfType<Renderer>());
        }

        void AdjustCameraIfNeeded()
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            Ray ray = new Ray(transform.position, directionToPlayer);
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            bool objectCleared = true;
            Renderer currentBlockingObject = null;

            // Clean up null references from the list
            renderers.RemoveAll(renderer => renderer == null);

            foreach (Renderer renderer in renderers)
            {
                // Ensure the renderer is not null and its tag or its parent's tag is not in the ignore list
                if (renderer != null && !IsTagOrLayerIgnored(renderer))
                {
                    if (renderer.bounds.IntersectRay(ray, out float distance) && distance < distanceToPlayer)
                    {
                        objectCleared = false;
                        currentBlockingObject = renderer;
                        break;
                    }
                }
            }

            if (objectCleared)
            {
                framingTransposer.CameraDistance = initialCameraDistance;
                framingTransposer.TargetOffset = initialTrackedObjectOffset;
                lastBlockingObject = null;
            }
            else
            {
                if (currentBlockingObject != lastBlockingObject || framingTransposer.CameraDistance > MinimumCameraDistance * 1.05f)
                {
                    framingTransposer.CameraDistance = MinimumCameraDistance;
                    framingTransposer.TargetOffset.y = MinimumTrackedObjectOffsetY;
                    lastBlockingObject = currentBlockingObject;
                }
            }
        }

        bool IsTagOrLayerIgnored(Renderer renderer)
        {
            Transform currentTransform = renderer.transform;
            while (currentTransform != null)
            {
                if (TagsToIgnore.Contains(currentTransform.tag))
                    return true;

                if (LayersToIgnore.Contains(LayerMask.LayerToName(currentTransform.gameObject.layer)))
                    return true;

                currentTransform = currentTransform.parent;
            }
            return false;
        }
    }
}