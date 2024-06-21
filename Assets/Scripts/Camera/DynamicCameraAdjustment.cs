using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using NeonLadder.Mechanics.Enums;

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
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private CinemachineFramingTransposer framingTransposer;
    private CinemachineCollider cinemachineCollider;
    private Vector3 initialTrackedObjectOffset;

    private List<Renderer> renderers; // Cached renderers
    private Renderer lastBlockingObject; // Last object that was blocking the camera

    void Awake()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        framingTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        cinemachineCollider = cinemachineVirtualCamera.GetComponent<CinemachineCollider>();
        CacheInitialSettings(); // Cache initial settings on Awake
        playerTransform = GameObject.FindWithTag(Tags.Player.ToString()).transform; // Assume the player has the tag "Player"
    }

    void OnEnable()
    {
        //ResetToInitialSettings();
        RefreshRenderers(); // Refresh renderers for the new scene
        cinemachineVirtualCamera.enabled = true;
    }

    private void OnDisable()
    {
        cinemachineVirtualCamera.enabled = false;
        //ResetToInitialSettings();
    }

    void Update()
    {
        if (enabled) //CYA
        {
            AdjustCameraIfNeeded();
        }
    }

    private void CacheInitialSettings()
    {
        initialCameraDistance = framingTransposer.m_CameraDistance;
        initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        initialCameraPosition = transform.position;
        initialCameraRotation = transform.rotation;
        LogFramingTransposerSettings();
    }

    public void ResetToInitialSettings()
    {
        framingTransposer.m_CameraDistance = initialCameraDistance;
        framingTransposer.m_TrackedObjectOffset = initialTrackedObjectOffset;
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
            // Reset the camera distance and tracked object offset Y
            framingTransposer.m_CameraDistance = initialCameraDistance;
            framingTransposer.m_TrackedObjectOffset = initialTrackedObjectOffset;
            lastBlockingObject = null;
        }
        else
        {
            // Adjust camera distance and tracked object offset Y
            if (currentBlockingObject != lastBlockingObject || framingTransposer.m_CameraDistance > MinimumCameraDistance * 1.05f)
            {
                framingTransposer.m_CameraDistance = MinimumCameraDistance;
                framingTransposer.m_TrackedObjectOffset.y = MinimumTrackedObjectOffsetY;

                // Update last blocking object
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

    void LogFramingTransposerSettings()
    {
        string logMessage = $"Initial Camera Distance: {initialCameraDistance}\n" +
                            $"Dead Zone Height: {framingTransposer.m_DeadZoneHeight}, Dead Zone Width: {framingTransposer.m_DeadZoneWidth}\n" +
                            $"Soft Zone Height: {framingTransposer.m_SoftZoneHeight}, Soft Zone Width: {framingTransposer.m_SoftZoneWidth}\n" +
                            $"Bias X: {framingTransposer.m_BiasX}, Bias Y: {framingTransposer.m_BiasY}\n" +
                            $"Minimum FOV: {framingTransposer.m_MinimumFOV}, Maximum FOV: {framingTransposer.m_MaximumFOV}\n" +
                            $"XDamping: {framingTransposer.m_XDamping}, YDamping: {framingTransposer.m_YDamping}, ZDamping: {framingTransposer.m_ZDamping}\n" +
                            $"Transform Position: {transform.position}\n" +
                            $"Transform Rotation: {transform.rotation}\n";

        if (cinemachineCollider != null)
        {
            logMessage += $"\nCinemachine Collider Minimum Distance: {cinemachineCollider.m_MinimumDistanceFromTarget}";
        }

        Debug.Log(logMessage);
    }
}
