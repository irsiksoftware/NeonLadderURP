using UnityEngine;
using NeonLadder.Core;
using NeonLadder.Integrations;
using DamageNumbersPro;
using NeonLadder.Debugging;

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
        private LogCategory logCategory = LogCategory.Packages;

        private DamageNumberProRenderer proRenderer;
        
        void Awake()
        {
            // Check if another DamageNumberManager already exists
            if (FindObjectsByType<DamageNumberManager>(FindObjectsSortMode.None).Length > 1)
            {
                Debugger.LogError(logCategory, "Multiple DamageNumberManagers found in scene");
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
                
                Debugger.Log(logCategory, "DamageNumbersPro renderer initialized");
            }
            else
            {
                Debugger.LogWarning(logCategory, "DamageNumbersPro not configured, using debug renderer");
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