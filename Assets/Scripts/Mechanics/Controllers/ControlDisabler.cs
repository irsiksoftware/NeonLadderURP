using NeonLadder.Mechanics.Controllers.Interfaces;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class ControlDisabler : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            // Check if the other object has a component implementing IControllable
            IControllable controllable = other.GetComponentInParent<Player>().GetComponentInChildren<PlayerAction>();
            if (controllable != null)
            {
                controllable.DisableControls();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Optionally re-enable controls when the player exits the collider
            IControllable controllable = other.GetComponentInParent<Player>().GetComponentInChildren<PlayerAction>();
            if (controllable != null)
            {
                controllable.EnableControls();
            }
        }
    }
}
