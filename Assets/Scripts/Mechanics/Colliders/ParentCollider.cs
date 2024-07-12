using UnityEngine;
using NeonLadder.Managers;

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
}
