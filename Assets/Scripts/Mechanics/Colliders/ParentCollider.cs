using UnityEngine;

public class ParentCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (ManagerController.Instance == null)
        {
            Debug.Log("Managers prefab is missing, or it's instance is missing an implementaiton.");
        }
        else
        {
            ManagerController.Instance.eventManager.TriggerEvent("OnTriggerEnter", gameObject, other);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider is TerrainCollider)
        {
            ManagerController.Instance.eventManager.TriggerEvent("OnTriggerEnter", gameObject, collision.collider);
            // We just hit the floor
            Debug.Log("Ground collision detected!");
            // Possibly schedule an event
            // e.g. Schedule<PlayerTerrainCollision>();
        }
    }
}
