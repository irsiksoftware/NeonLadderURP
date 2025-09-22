using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using NeonLadder.ProceduralGeneration;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Editor
{
    public class SceneConnectionValidatorWindow : EditorWindow
    {
        [System.Serializable]
        public class ValidationResult
        {
            public string sceneName;
            public string objectName;
            public ValidationSeverity severity;
            public string message;
            public string fixAction;
            
            public ValidationResult(string scene, string obj, ValidationSeverity sev, string msg, string fix = null)
            {
                sceneName = scene;
                objectName = obj;
                severity = sev;
                message = msg;
                fixAction = fix;
            }
        }
        
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error
        }
        
        public enum FilterLevel
        {
            All,
            WarningsAndErrors,
            ErrorsOnly
        }
        
        public enum ExportFormat
        {
            JSON,
            CSV,
            Markdown
        }
        
        [System.Serializable]
        public class ValidationSettings
        {
            public bool scanBuildScenesOnly = true;
            public bool includeDisabledScenes = false;
            public bool validateSpawnPoints = true;
            public bool validateOverrides = true;
            public bool validateTriggers = true;
            public bool checkCircularReferences = true;
            public bool showProceduralPaths = true;
            public bool showManualOverrides = true;
            public FilterLevel filterLevel = FilterLevel.All;
            public ExportFormat exportFormat = ExportFormat.JSON;
            public bool includeValidationReport = true;
        }
        
        private ValidationSettings settings = new ValidationSettings();
        private List<ValidationResult> validationResults = new List<ValidationResult>();
        private Dictionary<string, List<SceneConnectionOverride>> sceneOverrides = new Dictionary<string, List<SceneConnectionOverride>>();
        private Dictionary<string, List<SceneTransitionTrigger>> sceneTriggers = new Dictionary<string, List<SceneTransitionTrigger>>();
        
        private Vector2 scrollPosition;
        private bool showSettings = true;
        private bool showResults = true;
        private bool showGraph = false;
        private bool isValidating = false;
        
        private GUIStyle headerStyle;
        private GUIStyle infoStyle;
        private GUIStyle warningStyle;
        private GUIStyle errorStyle;
        
        [MenuItem("NeonLadder/Tools/Scene Management/Scene Connection Validator %&v", priority = 100)]
        public static void ShowWindowWithShortcut()
        {
            var window = GetWindow<SceneConnectionValidatorWindow>("Scene Validator");
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeStyles();
            RefreshSceneData();
        }
        
        private void InitializeStyles()
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 14;
            
            infoStyle = new GUIStyle(EditorStyles.label);
            infoStyle.normal.textColor = new Color(0.3f, 0.6f, 1f);
            
            warningStyle = new GUIStyle(EditorStyles.label);
            warningStyle.normal.textColor = new Color(1f, 0.6f, 0f);
            
            errorStyle = new GUIStyle(EditorStyles.label);
            errorStyle.normal.textColor = Color.red;
        }
        
        private void OnGUI()
        {
            if (headerStyle == null) InitializeStyles();
            
            EditorGUILayout.LabelField("Scene Connection Validator", headerStyle);
            EditorGUILayout.Space();
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawSettingsSection();
            EditorGUILayout.Space();
            
            DrawValidationSection();
            EditorGUILayout.Space();
            
            DrawResultsSection();
            EditorGUILayout.Space();
            
            if (showGraph)
            {
                DrawConnectionGraph();
                EditorGUILayout.Space();
            }
            
            DrawExportSection();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawSettingsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            showSettings = EditorGUILayout.Foldout(showSettings, "Validation Settings", true);
            
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Scan Options", EditorStyles.boldLabel);
                settings.scanBuildScenesOnly = EditorGUILayout.Toggle("Build Scenes Only", settings.scanBuildScenesOnly);
                settings.includeDisabledScenes = EditorGUILayout.Toggle("Include Disabled Scenes", settings.includeDisabledScenes);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Validation Checks", EditorStyles.boldLabel);
                settings.validateSpawnPoints = EditorGUILayout.Toggle("Validate Spawn Points", settings.validateSpawnPoints);
                settings.validateOverrides = EditorGUILayout.Toggle("Validate Overrides", settings.validateOverrides);
                settings.validateTriggers = EditorGUILayout.Toggle("Validate Triggers", settings.validateTriggers);
                settings.checkCircularReferences = EditorGUILayout.Toggle("Check Circular References", settings.checkCircularReferences);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Display Options", EditorStyles.boldLabel);
                settings.showProceduralPaths = EditorGUILayout.Toggle("Show Procedural Paths", settings.showProceduralPaths);
                settings.showManualOverrides = EditorGUILayout.Toggle("Show Manual Overrides", settings.showManualOverrides);
                settings.filterLevel = (FilterLevel)EditorGUILayout.EnumPopup("Filter Level", settings.filterLevel);
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !isValidating;
            if (GUILayout.Button("Validate All Scenes", GUILayout.Height(30)))
            {
                ValidateAllScenes();
            }
            
            if (GUILayout.Button("Validate Current Scene", GUILayout.Height(30)))
            {
                ValidateCurrentScene();
            }
            
            if (GUILayout.Button("Refresh Scene Data", GUILayout.Height(30)))
            {
                RefreshSceneData();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            if (isValidating)
            {
                EditorGUILayout.Space();
                var rect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(rect, 0.5f, "Validating scenes...");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawResultsSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            var filteredResults = FilterResults(validationResults);
            
            EditorGUILayout.BeginHorizontal();
            showResults = EditorGUILayout.Foldout(showResults, $"Validation Results ({filteredResults.Count})", true);
            
            if (validationResults.Count > 0)
            {
                var errorCount = validationResults.Count(r => r.severity == ValidationSeverity.Error);
                var warningCount = validationResults.Count(r => r.severity == ValidationSeverity.Warning);
                var infoCount = validationResults.Count(r => r.severity == ValidationSeverity.Info);
                
                EditorGUILayout.LabelField($"E:{errorCount} W:{warningCount} I:{infoCount}", GUILayout.Width(80));
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showResults && filteredResults.Count > 0)
            {
                EditorGUI.indentLevel++;
                
                foreach (var result in filteredResults)
                {
                    DrawValidationResult(result);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawValidationResult(ValidationResult result)
        {
            GUIStyle style = result.severity switch
            {
                ValidationSeverity.Error => errorStyle,
                ValidationSeverity.Warning => warningStyle,
                ValidationSeverity.Info => infoStyle,
                _ => EditorStyles.label
            };
            
            EditorGUILayout.BeginHorizontal();
            
            // Severity icon
            string icon = result.severity switch
            {
                ValidationSeverity.Error => "✗",
                ValidationSeverity.Warning => "⚠",
                ValidationSeverity.Info => "ℹ",
                _ => "•"
            };
            
            EditorGUILayout.LabelField(icon, style, GUILayout.Width(20));
            
            // Scene name (clickable)
            if (GUILayout.Button(result.sceneName, EditorStyles.linkLabel, GUILayout.Width(150)))
            {
                OpenScene(result.sceneName);
            }
            
            // Object name (if specified)
            if (!string.IsNullOrEmpty(result.objectName))
            {
                EditorGUILayout.LabelField("→", GUILayout.Width(15));
                EditorGUILayout.LabelField(result.objectName, EditorStyles.miniLabel, GUILayout.Width(100));
            }
            
            // Message
            EditorGUILayout.LabelField(result.message, style);
            
            // Fix button (if available)
            if (!string.IsNullOrEmpty(result.fixAction))
            {
                if (GUILayout.Button("Fix", GUILayout.Width(40)))
                {
                    ApplyFix(result);
                }
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawConnectionGraph()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            showGraph = EditorGUILayout.Foldout(showGraph, "Connection Graph", true);
            
            if (GUILayout.Button("Generate Graph", GUILayout.Width(100)))
            {
                GenerateConnectionGraph();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (showGraph)
            {
                // Simple text-based graph for now
                var scenes = GetValidationScenes();
                
                foreach (var scene in scenes.Take(10)) // Limit for display
                {
                    EditorGUILayout.LabelField($"📄 {scene}");
                    
                    if (sceneTriggers.ContainsKey(scene))
                    {
                        foreach (var trigger in sceneTriggers[scene])
                        {
                            var spawnType = trigger.SpawnType.ToString();
                            EditorGUILayout.LabelField($"  ├─ {spawnType} → ?", EditorStyles.miniLabel);
                        }
                    }
                    
                    if (sceneOverrides.ContainsKey(scene))
                    {
                        foreach (var override_ in sceneOverrides[scene])
                        {
                            var target = override_.GetTargetScene();
                            if (!string.IsNullOrEmpty(target))
                            {
                                EditorGUILayout.LabelField($"  ├─ Override → {target}", EditorStyles.miniLabel);
                            }
                        }
                    }
                    
                    EditorGUILayout.Space();
                }
                
                if (scenes.Count > 10)
                {
                    EditorGUILayout.LabelField($"... and {scenes.Count - 10} more scenes");
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExportSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Export", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            settings.exportFormat = (ExportFormat)EditorGUILayout.EnumPopup("Format", settings.exportFormat);
            settings.includeValidationReport = EditorGUILayout.Toggle("Include Report", settings.includeValidationReport);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Export Results"))
            {
                ExportResults();
            }
            
            if (GUILayout.Button("Export Graph"))
            {
                ExportConnectionGraph();
            }
            
            if (GUILayout.Button("Open Reports Folder"))
            {
                var reportsPath = System.IO.Path.Combine(Application.dataPath, "..", "ValidationReports");
                Directory.CreateDirectory(reportsPath);
                EditorUtility.RevealInFinder(reportsPath);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void ValidateAllScenes()
        {
            isValidating = true;
            validationResults.Clear();
            
            var scenes = GetValidationScenes();
            
            foreach (var sceneName in scenes)
            {
                ValidateScene(sceneName);
            }
            
            // Cross-scene validation
            ValidateSceneConnections();
            
            isValidating = false;
            Repaint();
        }
        
        private void ValidateCurrentScene()
        {
            isValidating = true;
            validationResults.Clear();
            
            var currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.IsValid())
            {
                ValidateScene(currentScene.name);
            }
            else
            {
                validationResults.Add(new ValidationResult("", "", ValidationSeverity.Warning, "No active scene to validate"));
            }
            
            isValidating = false;
            Repaint();
        }
        
        private void ValidateScene(string sceneName)
        {
            // Scene existence check
            if (!SceneExists(sceneName))
            {
                validationResults.Add(new ValidationResult(sceneName, "", ValidationSeverity.Error, 
                    "Scene not found", "Remove reference or create scene"));
                return;
            }
            
            // Build settings check
            if (settings.scanBuildScenesOnly && !IsSceneInBuildSettings(sceneName))
            {
                validationResults.Add(new ValidationResult(sceneName, "", ValidationSeverity.Error, 
                    "Scene not in Build Settings", "Add to Build Settings"));
            }
            
            // Validate triggers in scene
            if (settings.validateTriggers && sceneTriggers.ContainsKey(sceneName))
            {
                foreach (var trigger in sceneTriggers[sceneName])
                {
                    ValidateTrigger(sceneName, trigger);
                }
            }
            
            // Validate overrides in scene
            if (settings.validateOverrides && sceneOverrides.ContainsKey(sceneName))
            {
                foreach (var override_ in sceneOverrides[sceneName])
                {
                    ValidateOverride(sceneName, override_);
                }
            }
            
            // Validate spawn points
            if (settings.validateSpawnPoints)
            {
                ValidateSceneSpawnPoints(sceneName);
            }
        }
        
        private void ValidateTrigger(string sceneName, SceneTransitionTrigger trigger)
        {
            if (trigger == null) return;
            
            var objName = trigger.name;
            
            // Check if trigger has valid spawn type
            if (trigger.SpawnType == SpawnPointType.None)
            {
                validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Warning,
                    "Trigger has no spawn type configured"));
            }
            
            // Check for collider
            if (trigger.GetComponent<Collider2D>() == null)
            {
                validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Error,
                    "Trigger missing Collider2D component", "Add Collider2D"));
            }
            
        }
        
        private void ValidateOverride(string sceneName, SceneConnectionOverride override_)
        {
            if (override_ == null) return;
            
            var objName = override_.name;
            var validationStatus = override_.ValidateOverride();
            
            switch (validationStatus)
            {
                case ValidationStatus.Error:
                    validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Error,
                        override_.GetValidationMessage()));
                    break;
                    
                case ValidationStatus.Warning:
                    validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Warning,
                        override_.GetValidationMessage()));
                    break;
                    
                case ValidationStatus.Valid:
                    validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Info,
                        "Override validated successfully"));
                    break;
            }
            
            // Check target scene exists
            var targetScene = override_.GetTargetScene();
            if (!string.IsNullOrEmpty(targetScene) && !SceneExists(targetScene))
            {
                validationResults.Add(new ValidationResult(sceneName, objName, ValidationSeverity.Error,
                    $"Target scene '{targetScene}' does not exist", "Fix target scene name"));
            }
        }
        
        private void ValidateSceneSpawnPoints(string sceneName)
        {
            // This would require loading the scene to check spawn points
            // For now, add a placeholder validation
            validationResults.Add(new ValidationResult(sceneName, "", ValidationSeverity.Info,
                "Spawn point validation requires scene loading"));
        }
        
        private void ValidateSceneConnections()
        {
            if (!settings.checkCircularReferences) return;
            
            // Simple circular reference detection
            foreach (var sceneOverride in sceneOverrides)
            {
                var sceneName = sceneOverride.Key;
                
                foreach (var override_ in sceneOverride.Value)
                {
                    var targetScene = override_.GetTargetScene();
                    
                    if (!string.IsNullOrEmpty(targetScene) && 
                        sceneOverrides.ContainsKey(targetScene))
                    {
                        // Check if target scene has override back to this scene
                        var targetOverrides = sceneOverrides[targetScene];
                        foreach (var targetOverride in targetOverrides)
                        {
                            if (targetOverride.GetTargetScene() == sceneName)
                            {
                                validationResults.Add(new ValidationResult(sceneName, override_.name, 
                                    ValidationSeverity.Warning, 
                                    $"Potential circular reference with '{targetScene}'"));
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        private void RefreshSceneData()
        {
            sceneOverrides.Clear();
            sceneTriggers.Clear();
            
            // Find all overrides and triggers in scenes
            var overrides = FindObjectsOfType<SceneConnectionOverride>(true);
            var triggers = FindObjectsOfType<SceneTransitionTrigger>(true);
            
            foreach (var override_ in overrides)
            {
                var sceneName = override_.gameObject.scene.name;
                if (!sceneOverrides.ContainsKey(sceneName))
                {
                    sceneOverrides[sceneName] = new List<SceneConnectionOverride>();
                }
                sceneOverrides[sceneName].Add(override_);
            }
            
            foreach (var trigger in triggers)
            {
                var sceneName = trigger.gameObject.scene.name;
                if (!sceneTriggers.ContainsKey(sceneName))
                {
                    sceneTriggers[sceneName] = new List<SceneTransitionTrigger>();
                }
                sceneTriggers[sceneName].Add(trigger);
            }
        }
        
        private List<ValidationResult> FilterResults(List<ValidationResult> results)
        {
            return settings.filterLevel switch
            {
                FilterLevel.All => results,
                FilterLevel.WarningsAndErrors => results.Where(r => r.severity != ValidationSeverity.Info).ToList(),
                FilterLevel.ErrorsOnly => results.Where(r => r.severity == ValidationSeverity.Error).ToList(),
                _ => results
            };
        }
        
        private List<string> GetValidationScenes()
        {
            var scenes = new List<string>();
            
            if (settings.scanBuildScenesOnly)
            {
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                    scenes.Add(sceneName);
                }
            }
            else
            {
                // Scan all scene assets
                var guids = AssetDatabase.FindAssets("t:Scene");
                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
                    scenes.Add(sceneName);
                }
            }
            
            return scenes;
        }
        
        private bool SceneExists(string sceneName)
        {
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            return guids.Length > 0;
        }
        
        private bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (name == sceneName) return true;
            }
            return false;
        }
        
        private void OpenScene(string sceneName)
        {
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(path);
            }
        }
        
        private void ApplyFix(ValidationResult result)
        {
            switch (result.fixAction)
            {
                case "Add to Build Settings":
                    AddSceneToBuildSettings(result.sceneName);
                    break;
                    
                case "Add Collider2D":
                    // Would require finding the object and adding component
                    Debug.Log($"Manual fix required: {result.fixAction} for {result.objectName} in {result.sceneName}");
                    break;
                    
                default:
                    Debug.Log($"Fix not implemented: {result.fixAction}");
                    break;
            }
        }
        
        private void AddSceneToBuildSettings(string sceneName)
        {
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var buildScenes = EditorBuildSettings.scenes.ToList();
                
                if (!buildScenes.Any(s => s.path == path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                    EditorBuildSettings.scenes = buildScenes.ToArray();
                    Debug.Log($"Added {sceneName} to Build Settings");
                }
            }
        }
        
        private void GenerateConnectionGraph()
        {
            // Simple graph generation - could be expanded with actual graph visualization
            Debug.Log("Connection graph generated - check console for details");
            
            var scenes = GetValidationScenes();
            foreach (var scene in scenes.Take(5))
            {
                Debug.Log($"Scene: {scene}");
                
                if (sceneTriggers.ContainsKey(scene))
                {
                    foreach (var trigger in sceneTriggers[scene])
                    {
                        Debug.Log($"  - Trigger: {trigger.name} ({trigger.SpawnType})");
                    }
                }
                
                if (sceneOverrides.ContainsKey(scene))
                {
                    foreach (var override_ in sceneOverrides[scene])
                    {
                        Debug.Log($"  - Override: {override_.name} → {override_.GetTargetScene()}");
                    }
                }
            }
        }
        
        private void ExportResults()
        {
            var reportsPath = System.IO.Path.Combine(Application.dataPath, "..", "ValidationReports");
            Directory.CreateDirectory(reportsPath);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"SceneValidation_{timestamp}.{settings.exportFormat.ToString().ToLower()}";
            var filePath = System.IO.Path.Combine(reportsPath, fileName);
            
            switch (settings.exportFormat)
            {
                case ExportFormat.JSON:
                    ExportToJSON(filePath);
                    break;
                case ExportFormat.CSV:
                    ExportToCSV(filePath);
                    break;
                case ExportFormat.Markdown:
                    ExportToMarkdown(filePath);
                    break;
            }
            
            Debug.Log($"Validation results exported to: {filePath}");
            EditorUtility.RevealInFinder(filePath);
        }
        
        private void ExportToJSON(string filePath)
        {
            var export = new
            {
                timestamp = DateTime.Now,
                settings = settings,
                results = validationResults,
                summary = new
                {
                    totalScenes = GetValidationScenes().Count,
                    totalResults = validationResults.Count,
                    errors = validationResults.Count(r => r.severity == ValidationSeverity.Error),
                    warnings = validationResults.Count(r => r.severity == ValidationSeverity.Warning),
                    info = validationResults.Count(r => r.severity == ValidationSeverity.Info)
                }
            };
            
            File.WriteAllText(filePath, JsonUtility.ToJson(export, true));
        }
        
        private void ExportToCSV(string filePath)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Scene,Object,Severity,Message,Fix");
            
            foreach (var result in validationResults)
            {
                csv.AppendLine($"\"{result.sceneName}\",\"{result.objectName}\",\"{result.severity}\",\"{result.message}\",\"{result.fixAction}\"");
            }
            
            File.WriteAllText(filePath, csv.ToString());
        }
        
        private void ExportToMarkdown(string filePath)
        {
            var md = new StringBuilder();
            md.AppendLine("# Scene Connection Validation Report");
            md.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            md.AppendLine();
            
            var errorCount = validationResults.Count(r => r.severity == ValidationSeverity.Error);
            var warningCount = validationResults.Count(r => r.severity == ValidationSeverity.Warning);
            var infoCount = validationResults.Count(r => r.severity == ValidationSeverity.Info);
            
            md.AppendLine("## Summary");
            md.AppendLine($"- Total Results: {validationResults.Count}");
            md.AppendLine($"- Errors: {errorCount}");
            md.AppendLine($"- Warnings: {warningCount}");
            md.AppendLine($"- Info: {infoCount}");
            md.AppendLine();
            
            if (errorCount > 0)
            {
                md.AppendLine("## Errors");
                foreach (var result in validationResults.Where(r => r.severity == ValidationSeverity.Error))
                {
                    md.AppendLine($"- **{result.sceneName}**: {result.message}");
                }
                md.AppendLine();
            }
            
            if (warningCount > 0)
            {
                md.AppendLine("## Warnings");
                foreach (var result in validationResults.Where(r => r.severity == ValidationSeverity.Warning))
                {
                    md.AppendLine($"- **{result.sceneName}**: {result.message}");
                }
                md.AppendLine();
            }
            
            File.WriteAllText(filePath, md.ToString());
        }
        
        private void ExportConnectionGraph()
        {
            var reportsPath = System.IO.Path.Combine(Application.dataPath, "..", "ValidationReports");
            Directory.CreateDirectory(reportsPath);
            
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var filePath = System.IO.Path.Combine(reportsPath, $"SceneGraph_{timestamp}.txt");
            
            var graph = new StringBuilder();
            graph.AppendLine("Scene Connection Graph");
            graph.AppendLine($"Generated: {DateTime.Now}");
            graph.AppendLine();
            
            var scenes = GetValidationScenes();
            foreach (var scene in scenes)
            {
                graph.AppendLine($"📄 {scene}");
                
                if (sceneTriggers.ContainsKey(scene))
                {
                    foreach (var trigger in sceneTriggers[scene])
                    {
                        graph.AppendLine($"  ├─ Trigger: {trigger.name} ({trigger.SpawnType})");
                    }
                }
                
                if (sceneOverrides.ContainsKey(scene))
                {
                    foreach (var override_ in sceneOverrides[scene])
                    {
                        var target = override_.GetTargetScene();
                        graph.AppendLine($"  ├─ Override: {override_.name} → {target ?? "None"}");
                    }
                }
                
                graph.AppendLine();
            }
            
            File.WriteAllText(filePath, graph.ToString());
            Debug.Log($"Connection graph exported to: {filePath}");
        }
    }
}