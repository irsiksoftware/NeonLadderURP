using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Editor.ProceduralGeneration
{
    /// <summary>
    /// Custom editor for SpawnPointConfiguration to conditionally show custom spawn name field
    /// </summary>
    [CustomEditor(typeof(SpawnPointConfiguration))]
    public class SpawnPointConfigurationEditor : UnityEditor.Editor
    {
        private SerializedProperty spawnModeProperty;
        private SerializedProperty customSpawnNameProperty;
        
        private void OnEnable()
        {
            spawnModeProperty = serializedObject.FindProperty("spawnMode");
            customSpawnNameProperty = serializedObject.FindProperty("customSpawnName");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            // Show the clickable Script field first
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }
            
            // Draw header
            EditorGUILayout.LabelField("Spawn Point Settings", EditorStyles.boldLabel);
            
            // Draw spawn mode dropdown
            EditorGUILayout.PropertyField(spawnModeProperty, new GUIContent("Arriving Players Came From", "Where did the player exit from in the previous scene to arrive at this spawn point?"));
            
            // Only show custom spawn name field if Custom is selected
            SpawnPointType selectedMode = (SpawnPointType)spawnModeProperty.enumValueIndex;
            if (selectedMode == SpawnPointType.Custom)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(customSpawnNameProperty, new GUIContent("Custom Spawn Name", "Name of the custom spawn point to use"));
                EditorGUI.indentLevel--;
            }
            
            // Show helpful info based on selection
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(GetHelpText(selectedMode), MessageType.Info);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private string GetHelpText(SpawnPointType mode)
        {
            return mode switch
            {
                SpawnPointType.None => "Spawning is disabled at this point.",
                SpawnPointType.Auto => "Spawn point will be automatically determined by the system.",
                SpawnPointType.Default => "Player will spawn at the 'Default' spawn point.",
                SpawnPointType.Center => "Players who exited through a center door/portal arrive here.",
                SpawnPointType.Left => "Players who exited LEFT in the previous scene arrive here (right side of this scene).",
                SpawnPointType.Right => "Players who exited RIGHT in the previous scene arrive here (left side of this scene).",
                SpawnPointType.BossArena => "Player will spawn at the 'BossArena' spawn point.",
                SpawnPointType.Custom => "Player will spawn at the custom-named spawn point.",
                _ => "Unknown spawn mode."
            };
        }
    }
}