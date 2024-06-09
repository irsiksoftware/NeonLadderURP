using NeonLadder.Effects.Text;
using UnityEngine;

namespace NeonLadder.Effects.Text
{
    public class FollowCharacterController : MonoBehaviour
    {
        public FollowCharacter followCharacter;
        public FollowCharacterVirtualCamera followCharacterVirtualCamera;

        void Start()
        {
        }

        // Call this from the timeline to switch to FollowCharacter
        public void EnableFollowCharacter()
        {
            followCharacter.enabled = true;
            followCharacterVirtualCamera.enabled = false;
        }

        // Call this from the timeline to switch to FollowCharacterVirtualCamera
        public void EnableFollowCharacterVirtualCamera()
        {
            followCharacter.enabled = false;
            followCharacterVirtualCamera.enabled = true;
        }
    }
}