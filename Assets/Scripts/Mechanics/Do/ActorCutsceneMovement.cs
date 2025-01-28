using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace Assets.Scripts.Mechanics.Do
{
    public class ActorCutsceneMovement : MonoBehaviour
    {
        [SerializeField]
        private float waitTimeInMs = 500f;

        [SerializeField]
        private float velocity = 2f;

        [SerializeField]
        private float duration = 3f;

        [SerializeField]
        private float degree = 90f;

        private KinematicObject actor;

        void Start()
        {
            actor = GetComponentInChildren<KinematicObject>();

            if (actor != null)
            {
                actor.Walk(waitTimeInMs, velocity, duration, degree);
            }
            else
            {
                Debug.LogWarning("KinematicObject component is missing on this GameObject.");
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
