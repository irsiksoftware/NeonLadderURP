using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

[CustomEditor(typeof(SpawnTester))]
public class SpawnTesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SpawnTester spawnTester = (SpawnTester)target;
        
        EditorGUILayout.LabelField("Spawn Point Tester", EditorStyles.boldLabel);
        
        // Show script reference for easy access
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(spawnTester), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to test spawn points", MessageType.Info);
            return;
        }
        
        // Get all spawn points in the scene
        var spawnPoints = spawnTester.GetAllSpawnPointsInScene();
        
        if (spawnPoints.Length == 0)
        {
            EditorGUILayout.HelpBox("No spawn points found in scene", MessageType.Warning);
            return;
        }
        
        EditorGUILayout.LabelField($"Found {spawnPoints.Length} spawn points:", EditorStyles.miniBoldLabel);
        EditorGUILayout.Space();
        
        // Create a button for each spawn point
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;
            
            string buttonLabel = $"Spawn at {spawnPoint.SpawnMode}";
            if (spawnPoint.SpawnMode == SpawnPointType.Custom && !string.IsNullOrEmpty(spawnPoint.CustomSpawnName))
            {
                buttonLabel = $"Spawn at Custom: {spawnPoint.CustomSpawnName}";
            }
            else if (spawnPoint.SpawnMode == SpawnPointType.None)
            {
                buttonLabel = $"Spawn at {spawnPoint.name} (None)";
            }
            
            // Add object name if different from type
            buttonLabel += $" [{spawnPoint.name}]";
            
            if (GUILayout.Button(buttonLabel))
            {
                spawnTester.TeleportToSpawnPoint(spawnPoint);
            }
        }
        
        EditorGUILayout.Space();
        
        // Add a refresh button
        if (GUILayout.Button("Refresh Spawn Points"))
        {
            // Force a repaint to update the list
            Repaint();
        }
    }
}