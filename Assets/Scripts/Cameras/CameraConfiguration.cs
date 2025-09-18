using System.Collections.Generic;
using NeonLadder.Mechanics.Enums;
using UnityEngine;

namespace NeonLadder.Cameras
{
    /// <summary>
    /// Defines camera presets for different scene types
    /// </summary>
    public enum CameraPreset
    {
        Default,      // Standard gameplay camera (distance: 12)
        CloseUp,      // Shop/rest areas (distance: 2.2)
        Medium,       // Special areas (distance: 6)
        Combat,       // Boss/combat areas (distance: 8)
        Cinematic     // Cutscene areas (distance: varies)
    }

    /// <summary>
    /// Camera configuration for a specific scene or preset
    /// </summary>
    [System.Serializable]
    public struct CameraConfig
    {
        public float cameraDistance;
        public Vector3 targetOffset;
        public float damping;
        
        public CameraConfig(float distance, Vector3 offset, float damp = 0.5f)
        {
            cameraDistance = distance;
            targetOffset = offset;
            damping = damp;
        }
    }

    /// <summary>
    /// Centralized camera configuration manager
    /// </summary>
    public static class CameraSettings
    {
        // Default camera distance for unaccounted scenes
        public const float DEFAULT_CAMERA_DISTANCE = 12f;
        
        // Predefined camera configurations
        private static readonly Dictionary<CameraPreset, CameraConfig> presetConfigs = new Dictionary<CameraPreset, CameraConfig>
        {
            { CameraPreset.Default, new CameraConfig(12f, new Vector3(0, 2.75f, 0)) },
            { CameraPreset.CloseUp, new CameraConfig(2.2f, new Vector3(0, 2.75f, 0)) },
            { CameraPreset.Medium, new CameraConfig(6f, new Vector3(0, 2.75f, 0)) },
            { CameraPreset.Combat, new CameraConfig(8f, new Vector3(0, 2.75f, 0)) },
            { CameraPreset.Cinematic, new CameraConfig(10f, new Vector3(0, 2.75f, 0)) }
        };
        
        // Scene-specific camera configurations
        private static readonly Dictionary<string, CameraPreset> scenePresets = new Dictionary<string, CameraPreset>
        {
            // Shop/Rest areas with close-up camera
            { Scenes.Core.MetaShop, CameraPreset.CloseUp },
            { Scenes.Core.PermaShop, CameraPreset.CloseUp },
            
            // Medium distance scenes
            { Scenes.Core.Staging, CameraPreset.Default },
            { Scenes.Core.Start, CameraPreset.Default },
            { Scenes.Legacy.SidePath1, CameraPreset.Default },
            
            // Default gameplay distance
            { Scenes.Legacy.MainPath1, CameraPreset.Default },
            { Scenes.Legacy.MainPath2, CameraPreset.Default },
            { Scenes.Legacy.MainPath3, CameraPreset.Default },
            
            // Add more scene mappings as needed
        };
        
        /// <summary>
        /// Get camera configuration for a specific scene
        /// </summary>
        public static CameraConfig GetConfigForScene(string scene)
        {
            if (scenePresets.TryGetValue(scene, out CameraPreset preset))
            {
                return presetConfigs[preset];
            }
            
            // Return default configuration for unaccounted scenes
            return presetConfigs[CameraPreset.Default];
        }
        
        /// <summary>
        /// Get camera configuration for a specific preset
        /// </summary>
        public static CameraConfig GetConfigForPreset(CameraPreset preset)
        {
            return presetConfigs.GetValueOrDefault(preset, presetConfigs[CameraPreset.Default]);
        }
        
        /// <summary>
        /// Apply camera configuration to a CinemachinePositionComposer
        /// </summary>
        public static void ApplyConfig(Unity.Cinemachine.CinemachinePositionComposer composer, CameraConfig config)
        {
            if (composer != null)
            {
                composer.CameraDistance = config.cameraDistance;
                composer.TargetOffset = config.targetOffset;
                // Note: Damping might need to be applied differently based on your setup
            }
        }
    }
}