using UnityEngine;
using Cinemachine;
using System.Collections.Generic;

public class DynamicCameraAdjustment : MonoBehaviour
{
    public float MaximumPercentOfFrameTakenUpByObject = 0.1f; // e.g., 10%
    public float DynamicCameraOffsetChangeSpeed = 2f; // e.g., speed factor for the transition
    public float MinDistanceToPlayer = 2.1f; // Minimum distance the camera should move to the player
    public float MinOffset = 2.1f; // Minimum allowable offset for the camera
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

        bool objectExceedsScreenPercentage = false;

        foreach (Renderer renderer in renderers)
        {
            // Check if the object's tag or its parent's tag is in the ignore list
            if (IsTagOrLayerIgnored(renderer))
                continue;

            if (renderer.bounds.IntersectRay(ray, out float distance) && distance < distanceToPlayer)
            {
                if (IsObjectExceedingScreenPercentage(renderer))
                {
                    //Debug.Log("Object " + renderer.gameObject.name + " is taking up too much of the screen!");
                    float newDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, MinDistanceToPlayer, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
                    newDistance = Mathf.Max(newDistance, MinDistanceToPlayer);

                    if (Mathf.Abs(framingTransposer.m_CameraDistance - newDistance) > 0.0001f)
                    {
                        //Debug.Log($"Setting camera distance to {newDistance}, current distance is {framingTransposer.m_CameraDistance}");
                        framingTransposer.m_CameraDistance = newDistance;
                    }

                    // Adjust Tracked Object Offset Y
                    float newOffsetY = Mathf.Lerp(initialTrackedObjectOffset.y, MinimumTrackedObjectOffsetY, 1 - (newDistance / initialCameraDistance));
                    if (Mathf.Abs(framingTransposer.m_TrackedObjectOffset.y - newOffsetY) > 0.0001f)
                    {
                        //Debug.Log($"Setting tracked object offset Y to {newOffsetY}, current offset Y is {framingTransposer.m_TrackedObjectOffset.y}");
                        framingTransposer.m_TrackedObjectOffset.y = newOffsetY;
                    }

                    objectExceedsScreenPercentage = true;
                    break; // Move camera once an object is found
                }
            }
        }

        // Reset the camera distance if no object exceeds the screen percentage
        if (!objectExceedsScreenPercentage)
        {
            float resetDistance = Mathf.Lerp(framingTransposer.m_CameraDistance, initialCameraDistance, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
            resetDistance = Mathf.Max(resetDistance, MinOffset);

            if (Mathf.Abs(framingTransposer.m_CameraDistance - resetDistance) > 0.0001f)
            {
                //Debug.Log($"Resetting camera distance to {resetDistance}, current distance is {framingTransposer.m_CameraDistance}");
                framingTransposer.m_CameraDistance = resetDistance;
            }

            // Reset Tracked Object Offset Y
            float resetOffsetY = Mathf.Lerp(framingTransposer.m_TrackedObjectOffset.y, initialTrackedObjectOffset.y, Time.deltaTime * DynamicCameraOffsetChangeSpeed);
            if (Mathf.Abs(framingTransposer.m_TrackedObjectOffset.y - resetOffsetY) > 0.0001f)
            {
                //Debug.Log($"Resetting tracked object offset Y to {resetOffsetY}, current offset Y is {framingTransposer.m_TrackedObjectOffset.y}");
                framingTransposer.m_TrackedObjectOffset.y = resetOffsetY;
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

    bool IsObjectExceedingScreenPercentage(Renderer renderer)
    {
        Bounds bounds = renderer.bounds;
        Vector3[] screenPoints = new Vector3[8];
        Camera mainCamera = Camera.main;

        screenPoints[0] = mainCamera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z));
        screenPoints[1] = mainCamera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z));
        screenPoints[2] = mainCamera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z));
        screenPoints[3] = mainCamera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z));
        screenPoints[4] = mainCamera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z));
        screenPoints[5] = mainCamera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z));
        screenPoints[6] = mainCamera.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z));
        screenPoints[7] = mainCamera.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.max.z));

        Rect boundingBox = new Rect(screenPoints[0], Vector2.zero);
        foreach (Vector3 sp in screenPoints)
        {
            boundingBox = Rect.MinMaxRect(Mathf.Min(boundingBox.xMin, sp.x), Mathf.Min(boundingBox.yMin, sp.y),
                                          Mathf.Max(boundingBox.xMax, sp.x), Mathf.Max(boundingBox.yMax, sp.y));
        }

        float screenArea = Screen.width * Screen.height;
        float boundingBoxArea = boundingBox.width * boundingBox.height;

        return (boundingBoxArea / screenArea) > MaximumPercentOfFrameTakenUpByObject;
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
