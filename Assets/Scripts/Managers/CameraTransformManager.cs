using UnityEngine;
using System.Collections.Generic;

namespace NeonLadder.Managers
{
    public class CameraTransformManager : MonoBehaviour
    {
        private Dictionary<string, Vector3> scenePositions = new Dictionary<string, Vector3>();

        // Method to save player position
        public void SavePlayerPosition(string sceneName, Vector3 position)
        {
            scenePositions[sceneName] = position;
        }

        // Method to try and get the last player position
        public bool TryGetLastPlayerPosition(string sceneName, out Vector3 position)
        {
            return scenePositions.TryGetValue(sceneName, out position);
        }
    }
}