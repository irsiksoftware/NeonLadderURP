using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Editor.ProceduralGeneration
{
    [CustomEditor(typeof(SceneTransitionTrigger))]
    public class SceneTransitionTriggerEditor : UnityEditor.Editor
    {
        private SerializedProperty transitionType;
        private SerializedProperty triggerColliderObject;
        private SerializedProperty destinationType;
        private SerializedProperty overrideSceneName;
        private SerializedProperty spawnPointType;
        private SerializedProperty customSpawnPointName;
        private SerializedProperty oneWayOnly;
        
        
        private GUIStyle headerStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        
        private void OnEnable()
        {
            transitionType = serializedObject.FindProperty("transitionType");
            triggerColliderObject = serializedObject.FindProperty("triggerColliderObject");
            destinationType = serializedObject.FindProperty("destinationType");
            overrideSceneName = serializedObject.FindProperty("overrideSceneName");
            spawnPointType = serializedObject.FindProperty("spawnPointType");
            customSpawnPointName = serializedObject.FindProperty("customSpawnPointName");
            oneWayOnly = serializedObject.FindProperty("oneWayOnly");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Show the clickable Script field first
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }
            
            InitializeStyles();
            
            SceneTransitionTrigger trigger = (SceneTransitionTrigger)target;
            
            // Title
            EditorGUILayout.LabelField("Scene Transition Trigger", headerStyle);
            EditorGUILayout.Space(5);
            
            // Validation Section
            DrawValidationSection(trigger);
            
            EditorGUILayout.Space(10);
            
            // Transition Configuration
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Transition Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(transitionType, new GUIContent("Transition Type", "Type of transition (Portal, Shrine, etc.)"));
            
            // Trigger Collider Object with validation
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(triggerColliderObject, new GUIContent("Trigger Collider Object", "GameObject that contains the trigger collider"));
            
            if (GUILayout.Button("Create", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                CreateTriggerColliderObject(trigger);
            }
            EditorGUILayout.EndHorizontal();
            
            // Show collider validation
            if (triggerColliderObject.objectReferenceValue != null)
            {
                ValidateColliderObject((GameObject)triggerColliderObject.objectReferenceValue);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Transition Settings section removed - using automatic mode only
            
            // Destination Settings
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Destination Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(destinationType);
            
            if (destinationType.enumValueIndex == 2) // Manual
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(overrideSceneName, new GUIContent("Scene Name"));
                EditorGUI.indentLevel--;
            }
            
            // Spawn Point Configuration
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Spawn Settings for Next Scene", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(spawnPointType, new GUIContent("Player Should Spawn At", "Which type of spawn point should the player appear at in the destination scene"));
            if ((SpawnPointType)spawnPointType.enumValueIndex == SpawnPointType.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customSpawnPointName, new GUIContent("Custom Spawn Point Name"));
                EditorGUI.indentLevel--;
            }
            
            // Add helpful context
            if (spawnPointType.enumValueIndex == (int)SpawnPointType.Auto)
            {
                EditorGUILayout.HelpBox("Auto mode: System will choose first available spawn point", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Restrictions
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Restrictions", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(oneWayOnly);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Visual Feedback section removed - using default gizmo only
            
            EditorGUILayout.Space(10);
            
            // Action Buttons
            if (Application.isPlaying)
            {
                GUI.enabled = trigger.GetTriggerColliderObject() != null;
                if (GUILayout.Button("Force Transition", GUILayout.Height(25)))
                {
                    trigger.ForceTransition();
                }
                GUI.enabled = true;
            }
            else
            {
                EditorGUILayout.HelpBox("Enter Play Mode to test transitions", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void DrawValidationSection(SceneTransitionTrigger trigger)
        {
            bool hasErrors = false;
            
            // Check for missing trigger collider object
            if (triggerColliderObject.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("No trigger collider object assigned! This component requires a GameObject with a Collider component.", MessageType.Error);
                hasErrors = true;
            }
            else
            {
                GameObject colliderObj = (GameObject)triggerColliderObject.objectReferenceValue;
                Collider collider = colliderObj.GetComponent<Collider>();
                
                if (collider == null)
                {
                    EditorGUILayout.HelpBox($"Assigned object '{colliderObj.name}' has no Collider component!", MessageType.Error);
                    hasErrors = true;
                }
                else if (!collider.isTrigger)
                {
                    EditorGUILayout.HelpBox($"Collider on '{colliderObj.name}' is not set as trigger. This will be auto-fixed at runtime.", MessageType.Warning);
                }
            }
            
            // Check for manual destination without scene name
            if (destinationType.enumValueIndex == 2 && string.IsNullOrEmpty(overrideSceneName.stringValue))
            {
                EditorGUILayout.HelpBox("Manual destination type selected but no scene name specified!", MessageType.Error);
                hasErrors = true;
            }
            
            // Key requirement validation removed
            
            if (!hasErrors && triggerColliderObject.objectReferenceValue != null)
            {
                EditorGUILayout.HelpBox("Configuration is valid!", MessageType.Info);
            }
        }
        
        private void ValidateColliderObject(GameObject colliderObj)
        {
            if (colliderObj == null) return;
            
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(true);
            
            Collider collider = colliderObj.GetComponent<Collider>();
            EditorGUILayout.LabelField("Has Collider:", collider != null ? "Yes" : "No");
            if (collider != null)
            {
                EditorGUILayout.LabelField("Is Trigger:", collider.isTrigger ? "Yes" : "No");
                EditorGUILayout.LabelField("Collider Type:", collider.GetType().Name);
            }
            
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
        }
        
        private void CreateTriggerColliderObject(SceneTransitionTrigger trigger)
        {
            // Create a new GameObject for the trigger
            GameObject triggerObj = new GameObject($"{transitionType.enumDisplayNames[transitionType.enumValueIndex]}_Trigger");
            triggerObj.transform.position = trigger.transform.position;
            triggerObj.transform.parent = trigger.transform;
            
            // Add BoxCollider and set as trigger
            BoxCollider collider = triggerObj.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(2f, 3f, 1f);
            
            // Assign to the trigger
            triggerColliderObject.objectReferenceValue = triggerObj;
            
            // Mark dirty for undo/redo
            Undo.RegisterCreatedObjectUndo(triggerObj, "Create Trigger Collider Object");
            EditorUtility.SetDirty(trigger);
            
            Debug.Log($"Created trigger collider object: {triggerObj.name}");
        }

        /// <summary>
        /// Generate smart name based on destination type
        /// </summary>
        private string GetSmartNameForDestinationType(SceneTransitionTrigger trigger, string currentName)
        {
            // Smart naming logic - enum: None=0, Procedural=1, Manual=2
            switch (destinationType.enumValueIndex)
            {
                case 0: // None
                    // No scene transition - this is just an entry point
                    Debug.Log($"[SceneTransitionTriggerEditor] Renaming to SceneEntry (None type)");
                    return GetUniqueSceneName("SceneEntry", currentName);
                    
                case 1: // Procedural
                case 2: // Manual
                    // Has scene transition - this is both entry and exit
                    Debug.Log($"[SceneTransitionTriggerEditor] Renaming to SceneEntryExit ({(destinationType.enumValueIndex == 1 ? "Procedural" : "Manual")} type)");
                    return GetUniqueSceneName("SceneEntryExit", currentName);
                    
                default:
                    Debug.LogWarning($"[SceneTransitionTriggerEditor] Unknown destination type index: {destinationType.enumValueIndex}");
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
                // Check both the trigger object name and its root parent name
                GameObject rootObj = obj.GetComponentInParent<SceneTransitionPrefabRoot>()?.gameObject ?? obj.gameObject;
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
        
        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.largeLabel)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
            
            if (warningStyle == null)
            {
                warningStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.yellow }
                };
            }
            
            if (errorStyle == null)
            {
                errorStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = Color.red }
                };
            }
        }
    }
}