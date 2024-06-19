using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class DynamicCameraAdjustment : MonoBehaviour
{
    public float DynamicCameraOffsetChangeSpeed = 2f; // Speed factor for the transition
    public float MinimumCameraDistance = 2.4f; // Smallest float that the FramingTransposer's Camera Distance will reach before giving up trying
    public float MinimumTrackedObjectOffsetY = 1f; // Minimum Y offset for the tracked object

    public List<string> TagsToIgnore; // List of tags to ignore
    public List<string> LayersToIgnore; // List of layers to ignore

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Transform playerTransform;
    private float initialCameraDistance;
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineCollider cinemachineCollider;
    private Vector3 initialTrackedObjectOffset;

    private Renderer[] renderers; // Cached renderers
    private Renderer lastBlockingObject; // Last object that was blocking the camera

    void Start()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        playerTransform = GameObject.FindWithTag("Player").transform; // Assume the player has the tag "Player"
        framingTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        cinemachineCollider = cinemachineVirtualCamera.GetComponent<CinemachineCollider>();
        initialCameraDistance = framingTransposer.m_CameraDistance;
        initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;

        CacheRenderers(); // Cache renderers at the start
        LogFramingTransposerSettings();
    }

    void Update()
    {
        AdjustCameraIfNeeded();
    }

    void CacheRenderers()
    {
        renderers = FindObjectsOfType<Renderer>();
    }

    public void RefreshRenderers()
    {
        CacheRenderers();
    }

    void AdjustCameraIfNeeded()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        Ray ray = new Ray(transform.position, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        bool objectCleared = true;
        Renderer currentBlockingObject = null;

        foreach (Renderer renderer in renderers)
        {
            // Ensure the object's tag or its parent's tag is not in the ignore list
            if (!IsTagOrLayerIgnored(renderer))
            {
                if (renderer.bounds.IntersectRay(ray, out float distance) && distance < distanceToPlayer)
                {
                    //Debug.Log($"Object in the way: {renderer.gameObject.name}");
                    objectCleared = false;
                    currentBlockingObject = renderer;
                    break;
                }
            }
        }

        if (objectCleared)
        {
            //Debug.Log("No objects in the way");
            // Reset the camera distance and tracked object offset Y
            float resetDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, initialCameraDistance, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
            resetDistance = Mathf.Max(resetDistance, MinimumCameraDistance);

            if (Mathf.Abs(framingTransposer.m_CameraDistance - resetDistance) > 0.0001f)
            {
                framingTransposer.m_CameraDistance = resetDistance;
            }

            float resetOffsetY = Mathf.Lerp(framingTransposer.m_TrackedObjectOffset.y, initialTrackedObjectOffset.y, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
            if (Mathf.Abs(framingTransposer.m_TrackedObjectOffset.y - resetOffsetY) > 0.0001f)
            {
                framingTransposer.m_TrackedObjectOffset.y = resetOffsetY;
            }

            // Reset last blocking object
            lastBlockingObject = null;
        }
        else
        {
            // Adjust camera distance and tracked object offset Y
            if (currentBlockingObject != lastBlockingObject || framingTransposer.m_CameraDistance > MinimumCameraDistance * 1.05f)
            {
                float newDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, MinimumCameraDistance, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
                newDistance = Mathf.Max(newDistance, MinimumCameraDistance);

                if (Mathf.Abs(framingTransposer.m_CameraDistance - newDistance) > 0.0001f)
                {
                    framingTransposer.m_CameraDistance = newDistance;
                }

                float newOffsetY = Mathf.Lerp(initialTrackedObjectOffset.y, MinimumTrackedObjectOffsetY, 1 - (newDistance / initialCameraDistance));
                if (Mathf.Abs(framingTransposer.m_TrackedObjectOffset.y - newOffsetY) > 0.0001f)
                {
                    framingTransposer.m_TrackedObjectOffset.y = newOffsetY;
                }

                // Update last blocking object
                lastBlockingObject = currentBlockingObject;
            }
            else
            {
                //Debug.Log($"Reached minimum camera distance with the same blocking object: {currentBlockingObject.gameObject.name}");
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

    void LogFramingTransposerSettings()
    {
        string logMessage = $"Initial Camera Distance: {initialCameraDistance}\n" +
                            $"Dead Zone Height: {framingTransposer.m_DeadZoneHeight}, Dead Zone Width: {framingTransposer.m_DeadZoneWidth}\n" +
                            $"Soft Zone Height: {framingTransposer.m_SoftZoneHeight}, Soft Zone Width: {framingTransposer.m_SoftZoneWidth}\n" +
                            $"Bias X: {framingTransposer.m_BiasX}, Bias Y: {framingTransposer.m_BiasY}\n" +
                            $"Minimum FOV: {framingTransposer.m_MinimumFOV}, Maximum FOV: {framingTransposer.m_MaximumFOV}\n" +
                            $"XDamping: {framingTransposer.m_XDamping}, YDamping: {framingTransposer.m_YDamping}, ZDamping: {framingTransposer.m_ZDamping}";

        if (cinemachineCollider != null)
        {
            logMessage += $"\nCinemachine Collider Minimum Distance: {cinemachineCollider.m_MinimumDistanceFromTarget}";
        }

        Debug.Log(logMessage);
    }
}
