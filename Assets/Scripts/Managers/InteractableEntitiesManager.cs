using System.Collections.Generic;
using UnityEngine;

public class InteractableEntitiesManager : MonoBehaviour
{
    protected List<GameObject> interactableEntities;

    protected virtual void Awake()
    {
        interactableEntities = new List<GameObject>();
    }

    public virtual void RegisterEntity(GameObject entity)
    {
        if (!interactableEntities.Contains(entity))
        {
            interactableEntities.Add(entity);
        }
    }

    public virtual void UnregisterEntity(GameObject entity)
    {
        if (interactableEntities.Contains(entity))
        {
            interactableEntities.Remove(entity);
            Destroy(entity);
        }
    }

    public virtual void InteractWithEntity(GameObject entity)
    {
        // Define interaction logic or leave it for child classes to override.
    }
}
