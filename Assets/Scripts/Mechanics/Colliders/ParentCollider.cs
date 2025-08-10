using NeonLadder.Core.ServiceContainer;
using NeonLadder.Managers;
using UnityEngine;

public class ParentCollider : MonoBehaviour
{
    private EventManager eventManager;
    
    private void Start()
    {
        // Cache the event manager reference
        if (ServiceLocator.Instance.TryGet<EventManager>(out var manager))
        {
            eventManager = manager;
        }
        else
        {
            // Fallback to migration helper
            eventManager = ManagerControllerMigration.GetEventManager();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (eventManager == null)
        {
            // Try to get it again in case it was registered late
            if (ServiceLocator.Instance.TryGet<EventManager>(out var manager))
            {
                eventManager = manager;
            }
            else
            {
                eventManager = ManagerControllerMigration.GetEventManager();
            }
        }
        
        if (eventManager == null)
        {
            Debug.Log("EventManager service is not registered. Managers prefab may be missing.");
        }
        else
        {
            eventManager.TriggerEvent("OnTriggerEnter", gameObject, other);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider is TerrainCollider)
        {
            if (eventManager == null)
            {
                // Try to get it again in case it was registered late
                if (ServiceLocator.Instance.TryGet<EventManager>(out var manager))
                {
                    eventManager = manager;
                }
                else
                {
                    eventManager = ManagerControllerMigration.GetEventManager();
                }
            }
            
            if (eventManager != null)
            {
                eventManager.TriggerEvent("OnTriggerEnter", gameObject, collision.collider);
            }
            
            // We just hit the floor
            Debug.Log("Ground collision detected!");
            // Possibly schedule an event
            // e.g. Schedule<PlayerTerrainCollision>();
        }
    }
}
