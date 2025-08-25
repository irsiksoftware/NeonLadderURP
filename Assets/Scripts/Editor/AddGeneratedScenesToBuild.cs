using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AddGeneratedScenesToBuild : Editor
{
    [MenuItem("NeonLadder/Procedural/Add All Generated Scenes to Build Settings")]
    public static void AddGeneratedScenesToBuildSettings()
    {
        var generatedScenesPath = "Assets/Scenes/Generated";
        var searchPattern = "*.unity";
        
        // Get all .unity files in Generated folder and subfolders
        var generatedScenes = Directory.GetFiles(generatedScenesPath, searchPattern, SearchOption.AllDirectories)
            .Where(path => !path.EndsWith(".meta"))
            .Select(path => path.Replace('\\', '/'))
            .OrderBy(path => path)
            .ToList();
        
        // Get current build scenes
        var currentScenes = EditorBuildSettings.scenes.ToList();
        var currentScenePaths = currentScenes.Select(s => s.path).ToHashSet();
        
        int addedCount = 0;
        var newScenes = new List<EditorBuildSettingsScene>();
        
        Debug.Log($"[AddGeneratedScenesToBuild] Found {generatedScenes.Count} scenes in Generated folder");
        Debug.Log($"[AddGeneratedScenesToBuild] Current build has {currentScenes.Count} scenes");
        
        // Check each generated scene
        foreach (var scenePath in generatedScenes)
        {
            if (!currentScenePaths.Contains(scenePath))
            {
                var newScene = new EditorBuildSettingsScene(scenePath, true);
                newScenes.Add(newScene);
                addedCount++;
                Debug.Log($"  Adding: {scenePath}");
            }
            else
            {
                Debug.Log($"  Already in build: {scenePath}");
            }
        }
        
        if (addedCount > 0)
        {
            // Add new scenes to build settings
            currentScenes.AddRange(newScenes);
            EditorBuildSettings.scenes = currentScenes.ToArray();
            
            Debug.Log($"[AddGeneratedScenesToBuild] Successfully added {addedCount} scenes to build settings!");
            Debug.Log($"[AddGeneratedScenesToBuild] Total scenes in build: {EditorBuildSettings.scenes.Length}");
            
            // List all Generated scenes now in build
            Debug.Log("\n=== Generated Scenes Now in Build Settings ===");
            foreach (var scene in EditorBuildSettings.scenes.Where(s => s.path.Contains("Generated")))
            {
                Debug.Log($"  [{(scene.enabled ? "✓" : "✗")}] {scene.path}");
            }
        }
        else
        {
            Debug.Log("[AddGeneratedScenesToBuild] All Generated scenes are already in build settings!");
        }
        
        // Save the project to persist changes
        AssetDatabase.SaveAssets();
    }
    
    [MenuItem("NeonLadder/Procedural/List Generated Scenes in Build")]
    public static void ListGeneratedScenesInBuild()
    {
        var generatedScenes = EditorBuildSettings.scenes
            .Where(s => s.path.Contains("Generated"))
            .OrderBy(s => s.path)
            .ToList();
            
        Debug.Log($"\n=== Generated Scenes in Build Settings ({generatedScenes.Count} total) ===");
        
        // Group by subfolder
        var grouped = generatedScenes.GroupBy(s => 
        {
            var parts = s.path.Split('/');
            return parts.Length > 3 ? parts[3] : "Root";
        });
        
        foreach (var group in grouped.OrderBy(g => g.Key))
        {
            Debug.Log($"\n{group.Key} ({group.Count()} scenes):");
            foreach (var scene in group)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                Debug.Log($"  [{(scene.enabled ? "✓" : "✗")}] {sceneName}");
            }
        }
    }
    
    [MenuItem("NeonLadder/Procedural/Remove Duplicate Scenes from Build")]
    public static void RemoveDuplicateScenesFromBuild()
    {
        var scenes = EditorBuildSettings.scenes;
        var uniqueScenes = new Dictionary<string, EditorBuildSettingsScene>();
        var duplicatesRemoved = 0;
        
        foreach (var scene in scenes)
        {
            if (!uniqueScenes.ContainsKey(scene.path))
            {
                uniqueScenes[scene.path] = scene;
            }
            else
            {
                duplicatesRemoved++;
                Debug.LogWarning($"Removing duplicate: {scene.path}");
            }
        }
        
        if (duplicatesRemoved > 0)
        {
            EditorBuildSettings.scenes = uniqueScenes.Values.ToArray();
            Debug.Log($"[RemoveDuplicates] Removed {duplicatesRemoved} duplicate scenes from build settings");
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.Log("[RemoveDuplicates] No duplicate scenes found in build settings");
        }
    }
}