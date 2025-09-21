using UnityEngine;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Events;
using NeonLadder.Debugging;

namespace NeonLadder.Trackers
{
    public class PlayerStateTracker : MonoBehaviour
    {
        private Player player;
        private PlayerAction playerAction;
        private KinematicObject kinematicObject;

        private void Awake()
        {
            player = GetComponent<Player>();
            playerAction = GetComponent<PlayerAction>();
            kinematicObject = GetComponent<KinematicObject>();
            Debugger.LogInformation(LogCategory.Player, "PlayerStateTracker initialized.");
        }

        private void OnEnable()
        {
            PlayerLanded.OnExecute += TrackPlayerState;
            //Debug.Log("Tracking Player State.");
        }

        private void OnDisable()
        {
            PlayerLanded.OnExecute -= TrackPlayerState;
            //Debug.Log("End Tracking Player State.");
        }

        private void TrackPlayerState(PlayerLanded playerLandedEvent)
        {
            if (playerLandedEvent.Player == player)
            {
                Debug.Log("Tracking Player State on PlayerLanded event.");
                Debug.Log($"Player Input: {playerAction.playerInput}");
                Debug.Log($"Player Velocity: {kinematicObject.velocity}");
                TrackRaycast();
            }
        }

        private void TrackRaycast()
        {
            Vector3 rayOrigin = player.transform.position;
            Vector3 rayDirection = Vector3.down;
            float rayDistance = 1.0f; // Adjust as needed
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
            {
                Debug.Log($"Raycast hit: {hit.collider.name}, Point: {hit.point}, Normal: {hit.normal}");
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }
        }
    }
}
