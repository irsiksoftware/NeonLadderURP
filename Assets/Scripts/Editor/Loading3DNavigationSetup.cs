using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using NeonLadder.UI;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Editor utility to add Loading3DNavigationStarter to scenes
    /// </summary>
    public class Loading3DNavigationSetup : MonoBehaviour
    {
        [MenuItem("NeonLadder/Setup Loading3D Navigation in Current Scene")]
        public static void SetupNavigationInCurrentScene()
        {
            // Check if we already have a Loading3DNavigationStarter in the scene
            var existingStarter = FindFirstObjectByType<Loading3DNavigationStarter>();
            if (existingStarter != null)
            {
                Debug.Log("[Loading3DNavigationSetup] Loading3DNavigationStarter already exists in scene.");
                Selection.activeGameObject = existingStarter.gameObject;
                return;
            }

            // Create a new GameObject for the navigation starter
            GameObject navigationStarter = new GameObject("Loading3D Navigation Starter");
            var starter = navigationStarter.AddComponent<Loading3DNavigationStarter>();

            // Configure for test mode by default
            var spawnTestField = typeof(Loading3DNavigationStarter).GetField("spawnTestScreen",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            spawnTestField?.SetValue(starter, true);

            // Mark the scene as dirty so Unity knows to save changes
            EditorUtility.SetDirty(navigationStarter);

            Debug.Log("[Loading3DNavigationSetup] Added Loading3DNavigationStarter to scene in test mode. Assign the Loading3DScreen prefab in the inspector.");

            // Select the new object in the hierarchy
            Selection.activeGameObject = navigationStarter;
        }

        [MenuItem("NeonLadder/Remove Loading3D Navigation from Current Scene")]
        public static void RemoveNavigationFromCurrentScene()
        {
            var existingStarter = FindFirstObjectByType<Loading3DNavigationStarter>();
            if (existingStarter != null)
            {
                DestroyImmediate(existingStarter.gameObject);
                Debug.Log("[Loading3DNavigationSetup] Removed Loading3DNavigationStarter from scene.");
            }
            else
            {
                Debug.Log("[Loading3DNavigationSetup] No Loading3DNavigationStarter found in scene.");
            }
        }
    }
}