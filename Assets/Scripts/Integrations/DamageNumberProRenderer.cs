using UnityEngine;
using NeonLadder.Core;
// using DamageNumbersPro; // TODO: Uncomment when package is installed

namespace NeonLadder.Integrations
{
    /// <summary>
    /// DamageNumbersPro implementation of the damage number renderer
    /// This isolates all third-party dependencies to this single file
    /// </summary>
    public class DamageNumberProRenderer : MonoBehaviour, IDamageNumberRenderer
    {
        [SerializeField] private GameObject damageNumberPrefab; // TODO: Change to DamageNumber type when available
        
        public void SpawnNumber(Vector3 position, float value, DamageNumberConfig config)
        {
            // TODO: Implement when DamageNumbersPro is available
            /*
            if (damageNumberPrefab != null)
            {
                DamageNumber newPopup = damageNumberPrefab.Spawn(position, value);
                
                // Apply configuration
                newPopup.SetColor(config.color);
                newPopup.SetScale(config.scale);
                
                // Optional: Set motion, duration, etc. based on config
                // newPopup.SetVelocity(config.motion);
            }
            */
            
            // Temporary fallback
            Debug.Log($"DamageNumbersPro: {value} at {position} (color: {config.color}, scale: {config.scale})");
        }
    }
}