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
            else if (sceneName.ToLower().Contains(Scenes.Test.ToString().ToLower()))
            {
                return default;
            }
            else
            { 
                Debug.LogWarning($"Scene name '{sceneName}' is unaccounted for, Managers and events will act as if '{default}'");
                return default; // make this return Scenes.Unknown
            }
        }
    }
}
