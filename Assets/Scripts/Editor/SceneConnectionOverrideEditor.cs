using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.ProceduralGeneration;

namespace NeonLadder.Editor
{
    [CustomEditor(typeof(SceneConnectionOverride))]
    public class SceneConnectionOverrideEditor : UnityEditor.Editor
    {
        private SerializedProperty overrideEnabled;
        private SerializedProperty priority;
        private SerializedProperty selectionMode;
        private SerializedProperty targetSceneName;
        private SerializedProperty targetSceneBuildIndex;
        private SerializedProperty targetSceneAssetPath;
        private SerializedProperty overrideSpawnPoint;
        private SerializedProperty targetSpawnPointName;
        private SerializedProperty conditionalOverrides;
        private SerializedProperty validateOnAwake;
        private SerializedProperty validationStatus;
        private SerializedProperty validationMessage;
        private SerializedProperty debugLogging;
        private SerializedProperty gizmoColor;
        
        private List<string> availableScenes;
        private int selectedSceneIndex = 0;
        private bool showConditionalOverrides = true;
        private bool showValidation = true;
        private bool showDebugOptions = false;
        
        private GUIStyle headerStyle;
        private GUIStyle validStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        
        private void OnEnable()
        {
            // Cache serialized properties
            overrideEnabled = serializedObject.FindProperty("overrideEnabled");
            priority = serializedObject.FindProperty("priority");
            selectionMode = serializedObject.FindProperty("selectionMode");
            targetSceneName = serializedObject.FindProperty("targetSceneName");
            targetSceneBuildIndex = serializedObject.FindProperty("targetSceneBuildIndex");
            targetSceneAssetPath = serializedObject.FindProperty("targetSceneAssetPath");
            overrideSpawnPoint = serializedObject.FindProperty("overrideSpawnPoint");
            targetSpawnPointName = serializedObject.FindProperty("targetSpawnPointName");
            conditionalOverrides = serializedObject.FindProperty("conditionalOverrides");
            validateOnAwake = serializedObject.FindProperty("validateOnAwake");
            validationStatus = serializedObject.FindProperty("validationStatus");
            validationMessage = serializedObject.FindProperty("validationMessage");
            debugLogging = serializedObject.FindProperty("debugLogging");
            gizmoColor = serializedObject.FindProperty("gizmoColor");
            
            // Get available scenes
            RefreshSceneList();
        }
        
        private void RefreshSceneList()
        {
            availableScenes = SceneConnectionOverride.GetAllScenesInBuildSettings();
            
            // Find current selection
            if (!string.IsNullOrEmpty(targetSceneName.stringValue))
            {
                selectedSceneIndex = availableScenes.IndexOf(targetSceneName.stringValue);
                if (selectedSceneIndex < 0) selectedSceneIndex = 0;
            }
        }
        
        public override void OnInspectorGUI()
        {
            InitializeStyles();
            
            serializedObject.Update();
            
            SceneConnectionOverride override_ = (SceneConnectionOverride)target;
            
            // Header
            DrawHeader(override_);
            
            EditorGUILayout.Space();
            
            // Main settings
            DrawMainSettings();
            
            EditorGUILayout.Space();
            
            // Destination settings
            DrawDestinationSettings();
            
            EditorGUILayout.Space();
            
            // Conditional overrides
            DrawConditionalOverrides();
            
            EditorGUILayout.Space();
            
            // Validation section
            DrawValidationSection(override_);
            
            EditorGUILayout.Space();
            
            // Debug options
            DrawDebugOptions(override_);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void InitializeStyles()
        {
            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(EditorStyles.boldLabel);
                headerStyle.fontSize = 14;
                
                validStyle = new GUIStyle(EditorStyles.label);
                validStyle.normal.textColor = Color.green;
                
                warningStyle = new GUIStyle(EditorStyles.label);
                warningStyle.normal.textColor = new Color(1f, 0.6f, 0f);
                
                errorStyle = new GUIStyle(EditorStyles.label);
                errorStyle.normal.textColor = Color.red;
            }
        }
        
        private void DrawHeader(SceneConnectionOverride override_)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Enable checkbox
            overrideEnabled.boolValue = EditorGUILayout.Toggle(overrideEnabled.boolValue, GUILayout.Width(20));
            
            // Title
            EditorGUILayout.LabelField("Scene Connection Override", headerStyle);
            
            // Status indicator
            ValidationStatus status = (ValidationStatus)validationStatus.enumValueIndex;
            GUIStyle statusStyle = status switch
            {
                ValidationStatus.Valid => validStyle,
                ValidationStatus.Warning => warningStyle,
                ValidationStatus.Error => errorStyle,
                _ => EditorStyles.label
            };
            
            EditorGUILayout.LabelField(status.ToString(), statusStyle, GUILayout.Width(80));
            
            EditorGUILayout.EndHorizontal();
            
            if (!overrideEnabled.boolValue)
            {
                EditorGUILayout.HelpBox("Override is disabled. Enable to apply scene destination overrides.", MessageType.Info);
            }
        }
        
        private void DrawMainSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Override Settings", EditorStyles.boldLabel);
            
            // Priority slider
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Priority", GUILayout.Width(60));
            priority.intValue = EditorGUILayout.IntSlider(priority.intValue, 0, 1000);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.HelpBox("Higher priority overrides are evaluated first.", MessageType.None);
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDestinationSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Destination Override", EditorStyles.boldLabel);
            
            // Selection mode
            EditorGUILayout.PropertyField(selectionMode, new GUIContent("Selection Mode"));
            
            SceneSelectionMode mode = (SceneSelectionMode)selectionMode.enumValueIndex;
            
            switch (mode)
            {
                case SceneSelectionMode.SceneName:
                    DrawSceneNameSelection();
                    break;
                    
                case SceneSelectionMode.BuildIndex:
                    targetSceneBuildIndex.intValue = EditorGUILayout.IntSlider(
                        "Build Index", 
                        targetSceneBuildIndex.intValue, 
                        0, 
                        SceneManager.sceneCountInBuildSettings - 1);
                    
                    if (targetSceneBuildIndex.intValue >= 0)
                    {
                        string scenePath = SceneUtility.GetScenePathByBuildIndex(targetSceneBuildIndex.intValue);
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                        EditorGUILayout.LabelField("Scene:", sceneName, EditorStyles.miniLabel);
                    }
                    break;
                    
                case SceneSelectionMode.AssetPath:
                    EditorGUILayout.PropertyField(targetSceneAssetPath, new GUIContent("Asset Path"));
                    break;
            }
            
            EditorGUILayout.Space();
            
            // Spawn point override
            overrideSpawnPoint.boolValue = EditorGUILayout.Toggle("Override Spawn Point", overrideSpawnPoint.boolValue);
            
            if (overrideSpawnPoint.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(targetSpawnPointName, new GUIContent("Spawn Point Name"));
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSceneNameSelection()
        {
            if (availableScenes == null || availableScenes.Count == 0)
            {
                EditorGUILayout.HelpBox("No scenes found in Build Settings!", MessageType.Warning);
                
                if (GUILayout.Button("Open Build Settings"))
                {
                    EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                }
                return;
            }
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Scene", GUILayout.Width(80));
            
            selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, availableScenes.ToArray());
            
            if (selectedSceneIndex >= 0 && selectedSceneIndex < availableScenes.Count)
            {
                targetSceneName.stringValue = availableScenes[selectedSceneIndex];
            }
            
            if (GUILayout.Button("↻", GUILayout.Width(25)))
            {
                RefreshSceneList();
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Show current value if not in list
            if (!availableScenes.Contains(targetSceneName.stringValue) && !string.IsNullOrEmpty(targetSceneName.stringValue))
            {
                EditorGUILayout.HelpBox($"Scene '{targetSceneName.stringValue}' not found in Build Settings!", MessageType.Error);
            }
        }
        
        private void DrawConditionalOverrides()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            showConditionalOverrides = EditorGUILayout.Foldout(showConditionalOverrides, "Conditional Overrides", true);
            EditorGUILayout.LabelField($"({conditionalOverrides.arraySize})", GUILayout.Width(30));
            EditorGUILayout.EndHorizontal();
            
            if (showConditionalOverrides)
            {
                EditorGUI.indentLevel++;
                
                for (int i = 0; i < conditionalOverrides.arraySize; i++)
                {
                    SerializedProperty element = conditionalOverrides.GetArrayElementAtIndex(i);
                    
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.BeginHorizontal();
                    
                    SerializedProperty desc = element.FindPropertyRelative("description");
                    string title = string.IsNullOrEmpty(desc.stringValue) ? $"Condition {i + 1}" : desc.stringValue;
                    
                    element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, title, true);
                    
                    if (GUILayout.Button("×", GUILayout.Width(20)))
                    {
                        conditionalOverrides.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if (element.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("type"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("conditionKey"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("invertCondition"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("overrideSceneName"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("overrideSpawnPoint"));
                        EditorGUILayout.PropertyField(element.FindPropertyRelative("description"));
                        
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.EndVertical();
                }
                
                if (GUILayout.Button("Add Conditional Override"))
                {
                    conditionalOverrides.InsertArrayElementAtIndex(conditionalOverrides.arraySize);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationSection(SceneConnectionOverride override_)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showValidation = EditorGUILayout.Foldout(showValidation, "Validation", true);
            
            if (showValidation)
            {
                EditorGUILayout.PropertyField(validateOnAwake);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Status:", GUILayout.Width(50));
                
                ValidationStatus status = (ValidationStatus)validationStatus.enumValueIndex;
                GUIStyle style = status switch
                {
                    ValidationStatus.Valid => validStyle,
                    ValidationStatus.Warning => warningStyle,
                    ValidationStatus.Error => errorStyle,
                    _ => EditorStyles.label
                };
                
                EditorGUILayout.LabelField(status.ToString(), style);
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(validationMessage.stringValue))
                {
                    MessageType messageType = status switch
                    {
                        ValidationStatus.Error => MessageType.Error,
                        ValidationStatus.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };
                    
                    EditorGUILayout.HelpBox(validationMessage.stringValue, messageType);
                }
                
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Validate Now"))
                {
                    override_.ValidateOverride();
                    serializedObject.Update();
                }
                
                if (Application.isPlaying && GUILayout.Button("Test Connection"))
                {
                    override_.TestConnection();
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawDebugOptions(SceneConnectionOverride override_)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showDebugOptions = EditorGUILayout.Foldout(showDebugOptions, "Debug Options", true);
            
            if (showDebugOptions)
            {
                EditorGUILayout.PropertyField(debugLogging);
                EditorGUILayout.PropertyField(gizmoColor);
                
                if (Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    
                    if (GUILayout.Button("Apply Override"))
                    {
                        override_.ApplyOverride();
                    }
                    
                    if (GUILayout.Button("Restore Original"))
                    {
                        override_.RestoreOriginalDestination();
                    }
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scene Overrides", EditorStyles.boldLabel);
                
                var overrides = SceneConnectionOverride.GetAllOverridesInScene(override_.gameObject.scene.name);
                foreach (var o in overrides)
                {
                    EditorGUILayout.LabelField($"  • {o.name} (P{o.GetPriority()}) → {o.GetTargetScene()}");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}