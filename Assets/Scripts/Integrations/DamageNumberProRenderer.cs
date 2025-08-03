using UnityEngine;
using NeonLadder.Core;
using DamageNumbersPro;

namespace NeonLadder.Integrations
{
    /// <summary>
    /// DamageNumbersPro implementation of the damage number renderer
    /// This isolates all third-party dependencies to this single file
    /// </summary>
    public class DamageNumberProRenderer : MonoBehaviour, IDamageNumberRenderer
    {
        [SerializeField] public DamageNumber damageNumberPrefab;
        
        public void SpawnNumber(Vector3 position, float value, DamageNumberConfig config)
        {
            if (damageNumberPrefab != null)
            {
                DamageNumber newPopup = damageNumberPrefab.Spawn(position, value);
                
                // Apply configuration
                newPopup.SetColor(config.color);
                newPopup.SetScale(config.scale);
                
                // Optional: Set motion, duration, etc. based on config
                // newPopup.SetVelocity(config.motion);
            }
            else
            {
                // Fallback when prefab is not assigned
                Debug.Log($"DamageNumbersPro: {value} at {position} (color: {config.color}, scale: {config.scale})");
            }
        }
    }
}