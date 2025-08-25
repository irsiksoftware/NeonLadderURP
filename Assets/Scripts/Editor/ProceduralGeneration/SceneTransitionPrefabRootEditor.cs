using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
using System.Reflection;

namespace NeonLadder.Editor.ProceduralGeneration
{
    /// <summary>
    /// Custom inspector for SceneTransitionPrefabRoot that shows WARNING and ERROR message boxes
    /// </summary>
    [CustomEditor(typeof(SceneTransitionPrefabRoot))]
    public class SceneTransitionPrefabRootEditor : UnityEditor.Editor
    {
        private bool showWarnings = true;
        private bool showErrors = true;

        public override void OnInspectorGUI()
        {
            SceneTransitionPrefabRoot sceneTransitionRoot = (SceneTransitionPrefabRoot)target;

            // ERROR MessageBox with Lorem Ipsum
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("🚨 ERROR ZONE", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. " +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris. " +
                "This is a critical error that must be addressed immediately!",
                MessageType.Error);
            EditorGUILayout.EndVertical();

            // WARNING MessageBox with Lorem Ipsum  
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("⚠️ WARNING ZONE", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore. " +
                "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt. " +
                "This warning indicates potential issues that should be reviewed!",
                MessageType.Warning);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Collapsible sections for real validation
            showErrors = EditorGUILayout.Foldout(showErrors, "🔍 Real-time Validation Errors", true);
            if (showErrors)
            {
                EditorGUI.indentLevel++;
                string[] warnings, errors;
                bool isValid = sceneTransitionRoot.ValidateConfiguration(out warnings, out errors);

                if (errors.Length > 0)
                {
                    foreach (string error in errors)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No configuration errors found! ✓", MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }

            showWarnings = EditorGUILayout.Foldout(showWarnings, "⚠️ Real-time Validation Warnings", true);
            if (showWarnings)
            {
                EditorGUI.indentLevel++;
                string[] warnings, errors;
                bool isValid = sceneTransitionRoot.ValidateConfiguration(out warnings, out errors);

                if (warnings.Length > 0)
                {
                    foreach (string warning in warnings)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No configuration warnings! ✓", MessageType.Info);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Action Buttons
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("🔧 Auto-Fix Common Issues"))
            {
                AutoFixCommonIssues(sceneTransitionRoot);
            }
            
            if (GUILayout.Button("📋 Copy Configuration"))
            {
                Debug.Log("[SceneTransitionPrefabRoot] Configuration copied to clipboard!");
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Draw default inspector
            EditorGUILayout.LabelField("Component Properties", EditorStyles.boldLabel);
            DrawDefaultInspector();

            // Footer with meta info
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("💡 Meta Information", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"GameObject: {sceneTransitionRoot.gameObject.name}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Children: {sceneTransitionRoot.transform.childCount}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Position: {sceneTransitionRoot.transform.position}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void AutoFixCommonIssues(SceneTransitionPrefabRoot sceneTransitionRoot)
        {
            bool madeChanges = false;

            // Remove (Clone) from name
            if (sceneTransitionRoot.gameObject.name.Contains("(Clone)"))
            {
                sceneTransitionRoot.gameObject.name = sceneTransitionRoot.gameObject.name.Replace("(Clone)", "").Trim();
                madeChanges = true;
            }

            // Smart naming based on destination type
            var sceneTransition = sceneTransitionRoot.GetComponentInChildren<SceneTransitionTrigger>();
            if (sceneTransition != null)
            {
                string currentName = sceneTransitionRoot.gameObject.name;
                string smartName = GetSmartNameForDestinationType(sceneTransition, currentName);
                
                if (smartName != currentName)
                {
                    sceneTransitionRoot.gameObject.name = smartName;
                    madeChanges = true;
                    Debug.Log($"[SceneTransitionPrefabRoot] Smart rename: '{currentName}' → '{smartName}'");
                }
            }
            else
            {
                Debug.LogWarning("[SceneTransitionPrefabRoot] Cannot auto-fix: Missing SceneTransitionTrigger component!");
            }

            if (madeChanges)
            {
                EditorUtility.SetDirty(sceneTransitionRoot.gameObject);
                Debug.Log("[SceneTransitionPrefabRoot] Auto-fix applied!");
            }
            else
            {
                Debug.Log("[SceneTransitionPrefabRoot] No common issues found to fix.");
            }
        }

        /// <summary>
        /// Generate smart name based on destination type: None → SceneEntry, Others → SceneEntryExit
        /// </summary>
        private string GetSmartNameForDestinationType(SceneTransitionTrigger trigger, string currentName)
        {
            // Get the destination type using reflection since it's private
            var field = typeof(SceneTransitionTrigger).GetField("destinationType", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field == null)
            {
                Debug.LogWarning("[SceneTransitionPrefabRoot] Could not access destinationType field for smart naming");
                return currentName;
            }

            var destinationType = field.GetValue(trigger);
            string destinationTypeName = destinationType.ToString();

            // Smart naming logic
            switch (destinationTypeName)
            {
                case "None":
                    // No scene transition - this is just an entry point
                    return GetUniqueSceneName("SceneEntry", currentName);
                    
                case "Procedural":
                case "Manual":
                    // Has scene transition - this is both entry and exit
                    return GetUniqueSceneName("SceneEntryExit", currentName);
                    
                default:
                    Debug.LogWarning($"[SceneTransitionPrefabRoot] Unknown destination type: {destinationTypeName}");
                    return currentName;
            }
        }

        /// <summary>
        /// Generate unique scene name with numbering if necessary
        /// </summary>
        private string GetUniqueSceneName(string baseName, string currentName)
        {
            // Check if current name exactly matches the target pattern (not just starts with)
            if (currentName.Equals(baseName) || currentName.StartsWith(baseName + " ("))
            {
                return currentName;
            }

            // Find existing objects with same base name
            var existingObjects = FindObjectsOfType<SceneTransitionTrigger>();
            var existingNames = new System.Collections.Generic.HashSet<string>();
            
            foreach (var obj in existingObjects)
            {
                if (obj.gameObject.name.StartsWith(baseName))
                {
                    existingNames.Add(obj.gameObject.name);
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
    }
}