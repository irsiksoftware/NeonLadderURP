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
        private SerializedProperty mode;
        private SerializedProperty interactionPrompt;
        private SerializedProperty interactionKey;
        private SerializedProperty activationDelay;
        private SerializedProperty destinationType;
        private SerializedProperty overrideSceneName;
        private SerializedProperty spawnPointType;
        private SerializedProperty customSpawnPointName;
        private SerializedProperty direction;
        private SerializedProperty oneWayOnly;
        private SerializedProperty requiresKey;
        private SerializedProperty requiredKeyId;
        private SerializedProperty showPromptUI;
        private SerializedProperty promptOffset;
        private SerializedProperty gizmoColor;
        
        private GUIStyle headerStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        
        private void OnEnable()
        {
            transitionType = serializedObject.FindProperty("transitionType");
            triggerColliderObject = serializedObject.FindProperty("triggerColliderObject");
            mode = serializedObject.FindProperty("mode");
            interactionPrompt = serializedObject.FindProperty("interactionPrompt");
            interactionKey = serializedObject.FindProperty("interactionKey");
            activationDelay = serializedObject.FindProperty("activationDelay");
            destinationType = serializedObject.FindProperty("destinationType");
            overrideSceneName = serializedObject.FindProperty("overrideSceneName");
            spawnPointType = serializedObject.FindProperty("spawnPointType");
            customSpawnPointName = serializedObject.FindProperty("customSpawnPointName");
            direction = serializedObject.FindProperty("direction");
            oneWayOnly = serializedObject.FindProperty("oneWayOnly");
            requiresKey = serializedObject.FindProperty("requiresKey");
            requiredKeyId = serializedObject.FindProperty("requiredKeyId");
            showPromptUI = serializedObject.FindProperty("showPromptUI");
            promptOffset = serializedObject.FindProperty("promptOffset");
            gizmoColor = serializedObject.FindProperty("gizmoColor");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
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
            
            // Transition Settings
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Transition Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(mode);
            if (mode.enumValueIndex == 1) // Interactive mode
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(interactionPrompt);
                EditorGUILayout.PropertyField(interactionKey);
                EditorGUI.indentLevel--;
            }
            else // Automatic mode
            {
                if (activationDelay.floatValue > 0)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(activationDelay);
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.PropertyField(activationDelay);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Destination Settings
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Destination Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(destinationType);
            
            if (destinationType.enumValueIndex == 1) // Manual
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(overrideSceneName, new GUIContent("Scene Name"));
                EditorGUI.indentLevel--;
            }
            
            // Spawn Point Configuration
            EditorGUILayout.PropertyField(spawnPointType, new GUIContent("Spawn Point", "Which spawn point to use in destination scene"));
            if (spawnPointType.enumValueIndex == 5) // Custom
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customSpawnPointName, new GUIContent("Custom Spawn Point Name"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.PropertyField(direction);
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Restrictions
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Restrictions", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(oneWayOnly);
            EditorGUILayout.PropertyField(requiresKey);
            if (requiresKey.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(requiredKeyId, new GUIContent("Required Key ID"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
            
            // Visual Feedback
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Visual Feedback", EditorStyles.boldLabel);
            EditorGUILayout.Space(3);
            
            EditorGUILayout.PropertyField(showPromptUI);
            if (showPromptUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(promptOffset);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(gizmoColor);
            
            EditorGUILayout.EndVertical();
            
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
            if (destinationType.enumValueIndex == 1 && string.IsNullOrEmpty(overrideSceneName.stringValue))
            {
                EditorGUILayout.HelpBox("Manual destination type selected but no scene name specified!", MessageType.Error);
                hasErrors = true;
            }
            
            // Check for key requirement without key ID
            if (requiresKey.boolValue && string.IsNullOrEmpty(requiredKeyId.stringValue))
            {
                EditorGUILayout.HelpBox("Requires key is checked but no key ID specified!", MessageType.Warning);
            }
            
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