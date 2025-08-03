using UnityEngine;
using NeonLadder.Core;
using NeonLadder.Integrations;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Setup component for the damage number system
    /// Add this to a GameObject in your scene to enable damage numbers
    /// </summary>
    public class DamageNumberSystemSetup : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool useDamageNumbersPro = false;
        [SerializeField] private GameObject damageNumberProPrefab; // Only used if DamageNumbersPro is available
        
        private void Awake()
        {
            // Check if a DamageNumberSystem already exists
            if (Simulation.GetModel<DamageNumberSystem>() != null)
            {
                Debug.LogWarning("DamageNumberSystem already exists in the scene");
                Destroy(gameObject);
                return;
            }
            
            // Add the core system
            //var system = gameObject.AddComponent<DamageNumberSystem>();
            
            // Add the appropriate renderer
            if (useDamageNumbersPro && damageNumberProPrefab != null)
            {
                var proRenderer = gameObject.AddComponent<DamageNumberProRenderer>();
                // TODO: Set the prefab when DamageNumbersPro is available
                // proRenderer.damageNumberPrefab = damageNumberProPrefab;
            }
            // Otherwise the system will use the debug renderer automatically
        }
    }
}