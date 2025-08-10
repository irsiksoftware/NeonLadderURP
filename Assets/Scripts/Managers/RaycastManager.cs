using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Models;
using NeonLadder.Optimization;
using UnityEngine;

public class RaycastManager : MonoBehaviour
{
    public LineRenderer LineRenderer;
    public float RayLength = 0.5f; // Global ray length
    private Player player;
    private EulerAngleCache eulerCache;

    private void Awake()
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        player = model.Player;
        LineRenderer.startWidth = LineRenderer.endWidth = 0.05f; // Adjust this value as needed
        
        // Initialize Euler angle cache for performance optimization
        eulerCache = EulerAngleCacheManager.Cache;
        // Pre-warm the cache with player transform
        eulerCache?.PrewarmCache(player.transform);
    }

    private void OnDestroy()
    {
        Destroy(LineRenderer);
    }

    private void Update()
    {
        var rayInfo = GetRaycastInfo();
        DrawRay(rayInfo.rayStart, rayInfo.rayDirection);
    }

    public (Vector2 rayStart, Vector2 rayDirection) GetRaycastInfo()
    {
        // OPTIMIZED: Use cached Euler angle instead of direct property access
        // This avoids expensive Quaternion to Euler conversion every frame
        float yAngle = eulerCache != null ? eulerCache.GetEulerY(player.transform) : player.transform.eulerAngles.y;
        Vector2 rayDirection = yAngle < 90 || yAngle > 270 ? Vector2.right : Vector2.left;
        Vector2 rayStart = player.GetComponent<Collider>().bounds.center;
        return (rayStart, rayDirection);
    }

    private void DrawRay(Vector2 start, Vector2 direction)
    {
        LineRenderer.SetPosition(0, start);
        LineRenderer.SetPosition(1, start + direction * RayLength);
    }

    private void OnEnable()
    {
        // Enable the LineRenderer when RaycastManager is enabled
        if (LineRenderer != null)
            LineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        // Disable the LineRenderer when RaycastManager is disabled
        if (LineRenderer != null)
            LineRenderer.enabled = false;
    }
}
