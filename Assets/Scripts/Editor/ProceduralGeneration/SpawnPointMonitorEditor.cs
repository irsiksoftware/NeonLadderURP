using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;

[CustomEditor(typeof(SpawnPointMonitor))]
public class SpawnPointMonitorEditor : Editor
{
    private GUIStyle headerStyle;
    private GUIStyle dataStyle;
    private bool showRawData = false;

    private void InitStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 12;
        }
        
        if (dataStyle == null)
        {
            dataStyle = new GUIStyle(EditorStyles.label);
            dataStyle.fontStyle = FontStyle.Normal;
            dataStyle.richText = true;
        }
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        
        SpawnPointMonitor monitor = (SpawnPointMonitor)target;
        
        EditorGUILayout.LabelField("🔍 Spawn Point Monitor", headerStyle);
        
        // Show script reference for easy access
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(monitor), typeof(MonoScript), false);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        // Manual refresh button
        if (GUILayout.Button("🔄 Refresh Now"))
        {
            monitor.RefreshSpawnPointData();
        }
        
        EditorGUILayout.Space();
        
        // Get the spawn points via reflection (since it's private serialized)
        var spawnPointsField = typeof(SpawnPointMonitor).GetField("spawnPoints", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var spawnPoints = spawnPointsField?.GetValue(monitor) as System.Collections.Generic.List<SpawnPointMonitor.SpawnPointInfo>;
        
        // Player info
        var playerWorldPosField = typeof(SpawnPointMonitor).GetField("playerWorldPosition", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerLocalPosField = typeof(SpawnPointMonitor).GetField("playerLocalPosition", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerParentField = typeof(SpawnPointMonitor).GetField("playerParentName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerStatusField = typeof(SpawnPointMonitor).GetField("playerStatus", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Vector3 playerWorldPos = playerWorldPosField != null ? (Vector3)playerWorldPosField.GetValue(monitor) : Vector3.zero;
        Vector3 playerLocalPos = playerLocalPosField != null ? (Vector3)playerLocalPosField.GetValue(monitor) : Vector3.zero;
        string playerParent = playerParentField != null ? (string)playerParentField.GetValue(monitor) : "Unknown";
        string playerStatus = playerStatusField != null ? (string)playerStatusField.GetValue(monitor) : "Unknown";
        
        // Player Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("👤 Player Status", headerStyle);
        EditorGUILayout.LabelField($"Status: {playerStatus}");
        EditorGUILayout.LabelField($"World: {FormatVector(playerWorldPos)}", dataStyle);
        EditorGUILayout.LabelField($"Local: {FormatVector(playerLocalPos)}", dataStyle);
        EditorGUILayout.LabelField($"Parent: {playerParent}", dataStyle);
        
        // Show if local differs from world (indicates parenting)
        if (playerWorldPos != playerLocalPos && playerStatus != "Not Found")
        {
            EditorGUILayout.LabelField("⚠️ Player has parent transform", EditorStyles.miniLabel);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space();
        
        // Spawn Points Section
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            EditorGUILayout.LabelField($"📍 Found {spawnPoints.Count} Spawn Points:", headerStyle);
            EditorGUILayout.Space();
            
            foreach (var sp in spawnPoints)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Name and type
                string activeIndicator = sp.isActive ? "✅" : "❌";
                EditorGUILayout.LabelField($"{activeIndicator} {sp.name} [{sp.spawnType}]", headerStyle);
                
                // Position info
                EditorGUILayout.LabelField($"World: {FormatVector(sp.worldPosition)}", dataStyle);
                EditorGUILayout.LabelField($"Local: {FormatVector(sp.localPosition)}", dataStyle);
                
                // Parent info
                EditorGUILayout.LabelField($"Parent: {sp.parentName}", dataStyle);
                
                // Distance from player (if player exists)
                if (playerStatus != "Not Found")
                {
                    float distance = Vector3.Distance(playerWorldPos, sp.worldPosition);
                    EditorGUILayout.LabelField($"Distance from Player: {distance:F2} units", dataStyle);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No spawn points found in scene", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Copy to clipboard button
        if (GUILayout.Button("📋 Copy All Data to Clipboard"))
        {
            CopyDataToClipboard(monitor, spawnPoints, playerWorldPos, playerLocalPos, playerParent, playerStatus);
        }
        
        EditorGUILayout.Space();
        
        // Show/hide raw inspector
        showRawData = EditorGUILayout.Foldout(showRawData, "Raw Component Data");
        if (showRawData)
        {
            DrawDefaultInspector();
        }
        
        // Force repaint for live updates
        if (Application.isPlaying && monitor.enabled)
        {
            Repaint();
        }
    }
    
    private string FormatVector(Vector3 v)
    {
        return $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
    
    private void CopyDataToClipboard(SpawnPointMonitor monitor, System.Collections.Generic.List<SpawnPointMonitor.SpawnPointInfo> spawnPoints, 
        Vector3 playerWorldPos, Vector3 playerLocalPos, string playerParent, string playerStatus)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=== SPAWN POINT MONITOR DATA ===");
        sb.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine($"Timestamp: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        
        // Player section
        sb.AppendLine("👤 PLAYER STATUS");
        sb.AppendLine($"Status: {playerStatus}");
        sb.AppendLine($"World Position: {FormatVector(playerWorldPos)}");
        sb.AppendLine($"Local Position: {FormatVector(playerLocalPos)}");
        sb.AppendLine($"Parent: {playerParent}");
        
        if (playerWorldPos != playerLocalPos && playerStatus != "Not Found")
        {
            sb.AppendLine("⚠️  Player has parent transform (Local ≠ World)");
            Vector3 parentOffset = playerWorldPos - playerLocalPos;
            sb.AppendLine($"Parent World Offset: {FormatVector(parentOffset)}");
        }
        sb.AppendLine();
        
        // Spawn points section
        if (spawnPoints != null && spawnPoints.Count > 0)
        {
            sb.AppendLine($"📍 SPAWN POINTS ({spawnPoints.Count} found)");
            sb.AppendLine();
            
            foreach (var sp in spawnPoints)
            {
                string activeStatus = sp.isActive ? "✅ ACTIVE" : "❌ INACTIVE";
                sb.AppendLine($"{activeStatus} - {sp.name} [{sp.spawnType}]");
                sb.AppendLine($"  World Position: {FormatVector(sp.worldPosition)}");
                sb.AppendLine($"  Local Position: {FormatVector(sp.localPosition)}");
                sb.AppendLine($"  Parent: {sp.parentName}");
                
                // Show parent offset if different
                if (sp.worldPosition != sp.localPosition)
                {
                    Vector3 parentOffset = sp.worldPosition - sp.localPosition;
                    sb.AppendLine($"  Parent World Offset: {FormatVector(parentOffset)}");
                }
                
                // Distance from player
                if (playerStatus != "Not Found")
                {
                    float distance = Vector3.Distance(playerWorldPos, sp.worldPosition);
                    sb.AppendLine($"  Distance from Player: {distance:F2} units");
                }
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("📍 SPAWN POINTS");
            sb.AppendLine("No spawn points found in scene");
        }
        
        // Transform hierarchy analysis
        sb.AppendLine("🔍 TRANSFORM HIERARCHY ANALYSIS");
        if (playerStatus != "Not Found" && spawnPoints != null)
        {
            sb.AppendLine("Potential Issues:");
            
            foreach (var sp in spawnPoints)
            {
                if (sp.worldPosition != sp.localPosition)
                {
                    sb.AppendLine($"• {sp.name}: Has parent transform - teleportation might behave unexpectedly");
                }
            }
            
            if (playerWorldPos != playerLocalPos)
            {
                sb.AppendLine($"• Player: Parented to '{playerParent}' - teleport uses world coords but player stores local coords");
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("=== END DATA ===");
        
        // Copy to clipboard
        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("[SpawnPointMonitor] All data copied to clipboard!");
    }
}