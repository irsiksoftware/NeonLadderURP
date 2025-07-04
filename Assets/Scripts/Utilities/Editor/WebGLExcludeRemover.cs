using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NeonLadder.Utilities.Editor
{
    public class WebGLExcludeEditor : MonoBehaviour
    {
        [MenuItem("Tools/Remove WebGL Excluded Objects")]
        public static void RemoveWebGLExcludedObjects()
        {
            // Find all objects in the scene with the WebGLExclude component
            WebGLExclude[] objectsToExclude = FindObjectsByType<WebGLExclude>(FindObjectsSortMode.InstanceID);

            // Iterate over the array and destroy each game object
            foreach (WebGLExclude obj in objectsToExclude)
            {
                DestroyImmediate(obj.gameObject);
            }

            // Optionally, log how many objects were removed
            Debug.Log($"Removed {objectsToExclude.Length} objects marked for WebGL exclusion.");
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}