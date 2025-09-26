using NeonLadder.Mechanics.Enums;
using System;
using System.Linq;
using UnityEngine;

namespace NeonLadder.Utilities
{
    public static class SceneEnumResolver
    {
        /// <summary>
        /// Resolves a scene name string to a validated scene constant from the nested Scenes structure.
        /// Returns the scene name if valid, or Scenes.Core.Unknown if not found.
        /// </summary>
        public static string Resolve(string sceneName)
        {
            // Check if the scene exists in any of our scene collections
            if (IsValidScene(sceneName))
            {
                return sceneName;
            }
            else if (sceneName.ToLower().Contains(Scenes.Core.Test.ToLower()))
            {
                return Scenes.Core.Unknown;
            }
            else
            {
                Debug.LogWarning($"Scene name '{sceneName}' is unaccounted for, Managers and events will act as if '{Scenes.Core.Unknown}'");
                return Scenes.Core.Unknown;
            }
        }
        
        /// <summary>
        /// Checks if a scene name exists in any of the Scenes nested classes
        /// </summary>
        private static bool IsValidScene(string sceneName)
        {
            // Check each category
            return Scenes.Core.All.Contains(sceneName) ||
                   Scenes.Boss.All.Contains(sceneName) ||
                   Scenes.Connection.All.Contains(sceneName) ||
                   Scenes.Service.All.Contains(sceneName) ||
                   Scenes.Legacy.All.Contains(sceneName) ||
                   Scenes.Packaged.All.Contains(sceneName) ||
                   Scenes.Test.All.Contains(sceneName) ||
                   Scenes.Cutscene.All.Contains(sceneName);
        }
    }
}