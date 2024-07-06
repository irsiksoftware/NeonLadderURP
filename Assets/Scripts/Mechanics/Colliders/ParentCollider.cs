using UnityEngine;
using NeonLadder.Managers;

public class ParentCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ManagerController.Instance.eventManager.TriggerEvent("OnTriggerEnter", gameObject, other);
    }
}
