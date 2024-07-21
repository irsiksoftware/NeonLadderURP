using NeonLadder.Managers;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

public class SceneEndController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ManagerController.Instance.steamManager.UnlockAchievement("DEMO_LEVEL_COMPLETE");
        }
    }

    private void OnTriggerExit(Collider other)
    {
      
    }
}
