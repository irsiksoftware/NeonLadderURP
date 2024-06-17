using NeonLadder.Mechanics.Controllers;
using UnityEngine;

public class AllowZMovementController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerAction = other.GetComponentInChildren<PlayerAction>();
            if (playerAction != null)
            {
                playerAction.SetZMovementZone(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerAction = other.GetComponentInChildren<PlayerAction>();
            if (playerAction != null)
            {
                playerAction.SetZMovementZone(false);
            }
        }
    }
}
