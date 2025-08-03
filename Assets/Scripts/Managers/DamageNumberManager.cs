using UnityEngine;
using NeonLadder.Core;
using NeonLadder.Integrations;
using DamageNumbersPro;

namespace NeonLadder.Managers
{
    /// <summary>
    /// Manager for the damage number system
    /// Handles initialization and configuration of DamageNumbersPro integration
    /// </summary>
    public class DamageNumberManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool useDamageNumbersPro = true;
        [SerializeField] private DamageNumber damageNumberProPrefab;
        
        private DamageNumberProRenderer proRenderer;
        
        private void Awake()
        {
            // Check if another DamageNumberManager already exists
            if (FindObjectsOfType<DamageNumberManager>().Length > 1)
            {
                Debug.LogWarning("Multiple DamageNumberManagers found in scene");
                Destroy(gameObject);
                return;
            }
            
            InitializeDamageNumberSystem();
        }
        
        private void InitializeDamageNumberSystem()
        {
            // Add the appropriate renderer
            if (useDamageNumbersPro && damageNumberProPrefab != null)
            {
                proRenderer = gameObject.AddComponent<DamageNumberProRenderer>();
                proRenderer.damageNumberPrefab = damageNumberProPrefab;
                
                Debug.Log("DamageNumberManager: DamageNumbersPro renderer initialized");
            }
            else
            {
                Debug.LogWarning("DamageNumberManager: DamageNumbersPro not configured, using debug renderer");
            }
        }
        
        /// <summary>
        /// Get the damage number renderer for manual spawning if needed
        /// </summary>
        public IDamageNumberRenderer GetRenderer()
        {
            return proRenderer;
        }
    }
}