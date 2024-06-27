using NeonLadder.Mechanics.Enums;
using System;
using UnityEngine;

namespace NeonLadder.Utilities
{
    public static class SceneEnumResolver
    {
        public static Scenes Resolve(string sceneName)
        {
            if (Enum.TryParse(sceneName, out Scenes parsedScene))
            {
                return parsedScene;
            }
            else
            {
                Debug.LogWarning($"Scene name '{sceneName}' not recognized. Returning default scene.");
                return default(Scenes); // make this return Scenes.Unknown
            }
        }
    }
}
