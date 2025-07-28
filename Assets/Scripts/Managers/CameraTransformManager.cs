using UnityEngine;
using System.Collections.Generic;

public class CameraTransformManager : MonoBehaviour
    {
        private Dictionary<string, Vector3> scenePositions = new Dictionary<string, Vector3>();

        public void SavePlayerPosition(string sceneName, Vector3 position)
        {
            scenePositions[sceneName] = position;
        }

        public bool TryGetLastPlayerPosition(string sceneName, out Vector3 position)
        {
            return scenePositions.TryGetValue(sceneName, out position);
        }
    }