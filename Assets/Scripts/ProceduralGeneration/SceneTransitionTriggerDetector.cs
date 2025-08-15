using UnityEngine;

namespace NeonLadder.ProceduralGeneration
{
    /// <summary>
    /// Helper component that detects triggers for SceneTransitionTrigger
    /// This is automatically added to the assigned trigger collider object
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class SceneTransitionTriggerDetector : MonoBehaviour
    {
        private SceneTransitionTrigger parentTrigger;
        
        public void Initialize(SceneTransitionTrigger trigger)
        {
            parentTrigger = trigger;
            
            // Ensure collider is set as trigger
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (parentTrigger != null)
            {
                parentTrigger.OnPlayerEnterTrigger(other);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (parentTrigger != null)
            {
                parentTrigger.OnPlayerExitTrigger(other);
            }
        }
        
        private void OnValidate()
        {
            // Ensure this object has a collider and it's set as trigger
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }
    }
}