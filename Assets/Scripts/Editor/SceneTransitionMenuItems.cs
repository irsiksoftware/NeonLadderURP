using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;
using System.Reflection;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Unity Editor menu items for quickly adding scene transitions to the current scene
    /// </summary>
    public static class SceneTransitionMenuItems
    {
        private const string SCENE_TRANSITION_PREFAB_PATH = "Assets/Prefabs/ProceduralGeneration/SceneTransition.prefab";
        private const string MENU_ITEM_PATH = "NeonLadder/Procedural/Transitions/Add";
        private const int MENU_PRIORITY = 1;

        [MenuItem(MENU_ITEM_PATH, false, MENU_PRIORITY)]
        public static void AddSceneTransition()
        {
            // Load the SceneTransition prefab
            GameObject sceneTransitionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SCENE_TRANSITION_PREFAB_PATH);
            
            if (sceneTransitionPrefab == null)
            {
                Debug.LogError($"[SceneTransitionMenuItems] Could not find SceneTransition prefab at: {SCENE_TRANSITION_PREFAB_PATH}");
                EditorUtility.DisplayDialog("Error", 
                    $"SceneTransition prefab not found!\n\nExpected location:\n{SCENE_TRANSITION_PREFAB_PATH}", 
                    "OK");
                return;
            }

            // Get the active scene
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                Debug.LogError("[SceneTransitionMenuItems] No active scene found!");
                EditorUtility.DisplayDialog("Error", "No active scene found!", "OK");
                return;
            }

            // Instantiate the prefab in the scene
            GameObject sceneTransitionInstance = PrefabUtility.InstantiatePrefab(sceneTransitionPrefab) as GameObject;
            
            if (sceneTransitionInstance == null)
            {
                Debug.LogError("[SceneTransitionMenuItems] Failed to instantiate SceneTransition prefab!");
                return;
            }

            // Position it at a reasonable default location with unique offset
            sceneTransitionInstance.transform.position = GetDefaultExitPosition();
            
            // Apply smart naming based on default destination type (Procedural)
            ApplySmartNamingToNewExit(sceneTransitionInstance);

            // Move to end of scene hierarchy (makes it easy to find)
            sceneTransitionInstance.transform.SetAsLastSibling();

            // Register undo operation
            Undo.RegisterCreatedObjectUndo(sceneTransitionInstance, "Add Scene Exit");

            // Select the new object in hierarchy
            Selection.activeGameObject = sceneTransitionInstance;

            // Focus the scene view on the new object
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log($"[SceneTransitionMenuItems] Added SceneTransition: {sceneTransitionInstance.name} at {sceneTransitionInstance.transform.position}");
        }

        /// <summary>
        /// Validate menu item - only show when we have an active scene
        /// </summary>
        [MenuItem(MENU_ITEM_PATH, true)]
        public static bool ValidateAddSceneTransition()
        {
            return SceneManager.GetActiveScene().IsValid();
        }

        /// <summary>
        /// Get a consistent default position for new scene exits at the Player position
        /// </summary>
        private static Vector3 GetDefaultExitPosition()
        {
            // Try to find the Player GameObject by tag
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Debug.Log($"[SceneTransitionMenuItems] Positioning SceneTransition at Player position: {player.transform.position}");
                return player.transform.position;
            }
            
            // Fallback if no player found - use scene center
            Debug.LogWarning("[SceneTransitionMenuItems] No GameObject with 'Player' tag found. Using default position (0, 0, 0)");
            return Vector3.zero;
        }

        /// <summary>
        /// Apply smart naming to a newly created SceneTransition based on its default destination type
        /// </summary>
        private static void ApplySmartNamingToNewExit(GameObject sceneTransitionInstance)
        {
            // Get the SceneTransitionTrigger component to check destination type
            var trigger = sceneTransitionInstance.GetComponentInChildren<SceneTransitionTrigger>();
            if (trigger == null)
            {
                Debug.LogWarning("[SceneTransitionMenuItems] Cannot apply smart naming: No SceneTransitionTrigger found!");
                return;
            }

            // Get destination type using reflection (since it's private)
            var field = typeof(SceneTransitionTrigger).GetField("destinationType", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (field == null)
            {
                Debug.LogWarning("[SceneTransitionMenuItems] Cannot access destinationType for smart naming");
                return;
            }

            var destinationType = field.GetValue(trigger);
            string destinationTypeName = destinationType.ToString();

            // Default is "Procedural", so should be named "SceneEntryExit"
            string smartName = GetSmartNameForDestinationType(destinationTypeName, sceneTransitionInstance.name);
            
            if (smartName != sceneTransitionInstance.name)
            {
                sceneTransitionInstance.name = smartName;
                Debug.Log($"[SceneTransitionMenuItems] Smart naming applied: '{smartName}' (destination type: {destinationTypeName})");
            }
        }

        /// <summary>
        /// Generate smart name based on destination type: None → SceneEntry, Others → SceneEntryExit
        /// </summary>
        private static string GetSmartNameForDestinationType(string destinationTypeName, string currentName)
        {
            switch (destinationTypeName)
            {
                case "None":
                    return GetUniqueSceneName("SceneEntry", currentName);
                    
                case "Procedural":
                case "Manual":
                    return GetUniqueSceneName("SceneEntryExit", currentName);
                    
                default:
                    Debug.LogWarning($"[SceneTransitionMenuItems] Unknown destination type: {destinationTypeName}");
                    return currentName;
            }
        }

        /// <summary>
        /// Generate unique scene name with numbering if necessary
        /// </summary>
        private static string GetUniqueSceneName(string baseName, string currentName)
        {
            // Check if current name exactly matches the target pattern (not just starts with)
            if (currentName.Equals(baseName) || currentName.StartsWith(baseName + " ("))
            {
                return currentName;
            }

            // Find existing objects with same base name
            var existingObjects = GameObject.FindObjectsOfType<SceneTransitionTrigger>();
            var existingNames = new System.Collections.Generic.HashSet<string>();
            
            foreach (var obj in existingObjects)
            {
                // Check the root GameObject name (should have SceneTransitionPrefabRoot or be the trigger itself)
                var rootComponent = obj.GetComponentInParent<SceneTransitionPrefabRoot>();
                GameObject rootObj = rootComponent != null ? rootComponent.gameObject : obj.gameObject;
                
                if (rootObj.name.StartsWith(baseName))
                {
                    existingNames.Add(rootObj.name);
                }
            }

            // Try base name first
            if (!existingNames.Contains(baseName))
            {
                return baseName;
            }

            // Add numbering if base name is taken
            for (int i = 1; i <= 99; i++)
            {
                string numberedName = $"{baseName} ({i})";
                if (!existingNames.Contains(numberedName))
                {
                    return numberedName;
                }
            }

            // Fallback - should never happen
            return $"{baseName} ({System.DateTime.Now.Ticks})";
        }

        /// <summary>
        /// Verify scene exit configuration in current scene
        /// </summary>
        [MenuItem("NeonLadder/Procedural/Transitions/Verify", false, MENU_PRIORITY + 10)]
        public static void VerifySceneTransitions()
        {
            // Find all SceneTransitionTrigger components in the scene
            SceneTransitionTrigger[] allTriggers = GameObject.FindObjectsOfType<SceneTransitionTrigger>();
            
            if (allTriggers.Length == 0)
            {
                EditorUtility.DisplayDialog("Scene Exit Verification", 
                    "No scene exits found in current scene!\n\nAdd some exits first.", 
                    "OK");
                return;
            }

            // Analyze the configuration
            int exitTriggers = 0;
            int spawnTriggers = 0;
            int leftExits = 0;
            int rightExits = 0;
            int misconfigured = 0;
            System.Text.StringBuilder report = new System.Text.StringBuilder();

            report.AppendLine($"Scene Exit Verification Report");
            report.AppendLine($"Found {allTriggers.Length} scene transition triggers:");
            report.AppendLine();

            foreach (var trigger in allTriggers)
            {
                string status = "✓";
                
                if (trigger.CanExitHere) exitTriggers++;
                if (trigger.CanSpawnHere) spawnTriggers++;
                
                // Direction no longer tracked - just count spawn types
                switch (trigger.SpawnType)
                {
                    case SpawnPointType.Left:
                        leftExits++;
                        break;
                    case SpawnPointType.Right:
                        rightExits++;
                        break;
                }

                // Check for common issues
                if (!trigger.CanExitHere && !trigger.CanSpawnHere)
                {
                    status = "⚠️ Neither exit nor spawn enabled";
                    misconfigured++;
                }
                else if (trigger.GetTriggerColliderObject() == null)
                {
                    status = "❌ No trigger collider assigned";
                    misconfigured++;
                }

                report.AppendLine($"• {trigger.name} (SpawnType: {trigger.SpawnType}) - {status}");
            }

            report.AppendLine();
            report.AppendLine("Summary:");
            report.AppendLine($"• Exit-enabled: {exitTriggers}");
            report.AppendLine($"• Spawn-enabled: {spawnTriggers}");
            report.AppendLine($"• Left exits: {leftExits}");
            report.AppendLine($"• Right exits: {rightExits}");
            
            if (misconfigured > 0)
            {
                report.AppendLine($"• Issues found: {misconfigured}");
            }

            // Recommendations
            report.AppendLine();
            report.AppendLine("Recommendations:");
            
            if (leftExits == 0 && rightExits == 0)
            {
                report.AppendLine("• Add left and right exits for sidescroller flow");
            }
            else if (leftExits == 0)
            {
                report.AppendLine("• Consider adding a left exit");
            }
            else if (rightExits == 0)
            {
                report.AppendLine("• Consider adding a right exit");
            }
            
            if (spawnTriggers == 0)
            {
                report.AppendLine("• No spawn points available - enable 'Can Spawn Here' on some exits");
            }

            // Show the report
            string title = misconfigured > 0 ? "Scene Exit Issues Found" : "Scene Exit Verification Complete";
            EditorUtility.DisplayDialog(title, report.ToString(), "OK");
            
            Debug.Log($"[SceneTransitionMenuItems] Verification complete: {allTriggers.Length} triggers, {misconfigured} issues");
        }

        [MenuItem("NeonLadder/Procedural/Transitions/Verify", true)]
        public static bool ValidateVerifySceneTransitions()
        {
            return ValidateAddSceneTransition();
        }
    }
}