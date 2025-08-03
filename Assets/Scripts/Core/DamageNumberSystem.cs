using UnityEngine;
using NeonLadder.Events;
using NeonLadder.Common;
using NeonLadder.Debugging;
using NeonLadder.Integrations;

namespace NeonLadder.Core
{
    /// <summary>
    /// Abstract damage number system that decouples game logic from third-party UI libraries
    /// This allows swapping DamageNumbersPro for any other damage number system
    /// </summary>
    [System.Serializable]
    public class DamageNumberSystem
    {
        private IDamageNumberRenderer renderer;

        public DamageNumberSystem()
        {
            // Initialize the renderer - this could be configured via inspector or code
            InitializeRenderer();
        }

        private void InitializeRenderer()
        {
            // Try to find DamageNumbersPro renderer in scene
            var damageNumbersPro = GameObject.FindObjectOfType<DamageNumberProRenderer>();
            if (damageNumbersPro != null)
            {
                renderer = damageNumbersPro;
            }
            else
            {
                // Fall back to simple debug renderer - create a GameObject for it
                var debugRendererGO = new GameObject("DebugDamageNumberRenderer");
                renderer = debugRendererGO.AddComponent<DebugDamageNumberRenderer>();
                Debugger.LogWarning("DamageNumbersPro not found, using debug renderer");
            }
        }

        public void SpawnNumber(Vector3 position, float value, DamageNumberType type, Color? customColor = null, float? customScale = null)
        {
            if (renderer != null)
            {
                // Apply type-based styling
                var config = GetConfigForType(type, value);
                
                // Override with custom values if provided
                if (customColor.HasValue)
                    config.color = customColor.Value;
                if (customScale.HasValue)
                    config.scale = customScale.Value;
                
                renderer.SpawnNumber(position, value, config);
            }
        }

        private DamageNumberConfig GetConfigForType(DamageNumberType type, float value)
        {
            var config = new DamageNumberConfig();
            
            switch (type)
            {
                case DamageNumberType.Damage:
                    if (value > 5)
                    {
                        config.color = new Color(1f, 0.2f, 0.2f); // Red for high damage
                        config.scale = Constants.UI.DamageNumbers.CriticalHitScale;
                    }
                    else
                    {
                        config.color = new Color(1f, 0.7f, 0.5f); // Orange for normal damage
                        config.scale = Constants.UI.DamageNumbers.NormalHitScale;
                    }
                    break;
                    
                case DamageNumberType.Healing:
                    config.color = new Color(0.2f, 1f, 0.2f); // Green for healing
                    config.scale = Constants.UI.DamageNumbers.NormalHitScale;
                    break;
                    
                case DamageNumberType.CriticalDamage:
                    config.color = new Color(1f, 0f, 0f); // Pure red for crits
                    config.scale = Constants.UI.DamageNumbers.CriticalHitScale * 1.2f;
                    break;
                    
                case DamageNumberType.StaminaLoss:
                    config.color = new Color(0.5f, 0.5f, 1f); // Blue for stamina
                    config.scale = Constants.UI.DamageNumbers.NormalHitScale * 0.8f;
                    break;
            }
            
            return config;
        }
    }

    /// <summary>
    /// Configuration for damage number appearance
    /// </summary>
    public struct DamageNumberConfig
    {
        public Color color;
        public float scale;
        public float duration;
        public Vector3 motion;
        
        public DamageNumberConfig(Color color, float scale = 1f)
        {
            this.color = color;
            this.scale = scale;
            this.duration = 1f;
            this.motion = Vector3.up;
        }
    }

    /// <summary>
    /// Interface for damage number rendering implementations
    /// </summary>
    public interface IDamageNumberRenderer
    {
        void SpawnNumber(Vector3 position, float value, DamageNumberConfig config);
    }

    /// <summary>
    /// Simple debug implementation for when DamageNumbersPro isn't available
    /// </summary>
    public class DebugDamageNumberRenderer : MonoBehaviour, IDamageNumberRenderer
    {
        public void SpawnNumber(Vector3 position, float value, DamageNumberConfig config)
        {
            Debugger.Log($"<color=#{ColorUtility.ToHtmlStringRGB(config.color)}>Damage: {value} at {position}</color>");
        }
    }
}