using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
// using Newtonsoft.Json; // Replaced with Unity JsonUtility

namespace NeonLadder.Editor.ProceduralGeneration
{
    /// <summary>
    /// Visual editor for procedural path generation testing and analysis
    /// Provides real-time preview, seed testing, and parameter tuning
    /// </summary>
    public class PathGeneratorEditor : EditorWindow
    {
        #region Constants
        
        private const float PREVIEW_WIDTH = 800f;
        private const float PREVIEW_HEIGHT = 600f;
        private const float NODE_SIZE = 40f;
        private const float CONNECTION_WIDTH = 3f;
        private const float SIDEBAR_WIDTH = 300f;
        private const float TOOLBAR_HEIGHT = 30f;
        
        #endregion
        
        #region Private Fields
        
        // Generation
        private PathGenerator pathGenerator;
        private MysticalMap currentMap;
        private string currentSeed = "";
        private GenerationRules currentRules;
        
        // UI State
        private Vector2 scrollPosition;
        private Vector2 sidebarScroll;
        private Vector2 previewScroll;
        private float zoomLevel = 1.0f;
        private Vector2 panOffset = Vector2.zero;
        private bool isDragging = false;
        private Vector2 lastMousePosition;
        
        // Seed Testing
        private List<string> seedHistory = new List<string>();
        private List<string> favoritedSeeds = new List<string>();
        private string seedInput = "";
        private bool autoGenerate = true;
        
        // Visualization Options
        private bool showConnections = true;
        private bool showRoomTypes = true;
        private bool showDepthInfo = true;
        private bool showMetrics = true;
        private bool showGrid = true;
        private bool highlightBosses = true;
        private bool highlightSecrets = true;
        
        // Analysis
        private GenerationMetrics currentMetrics;
        private List<GenerationMetrics> metricsHistory = new List<GenerationMetrics>();
        
        // Export
        private string exportPath = "Assets/ProceduralData";
        private bool includeMetrics = true;
        private bool includeVisualization = true;
        
        // Colors
        private readonly Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private readonly Color connectionColor = new Color(0.7f, 0.7f, 0.7f, 0.8f);
        private readonly Color normalRoomColor = new Color(0.5f, 0.7f, 1f, 1f);
        private readonly Color bossRoomColor = new Color(1f, 0.3f, 0.3f, 1f);
        private readonly Color secretRoomColor = new Color(0.8f, 0.6f, 1f, 1f);
        private readonly Color shopRoomColor = new Color(1f, 0.9f, 0.3f, 1f);
        private readonly Color treasureRoomColor = new Color(0.3f, 1f, 0.3f, 1f);
        
        #endregion
        
        #region Window Management
        
        [MenuItem("NeonLadder/Procedural/Path Visualizer")]
        public static void ShowWindow()
        {
            var window = GetWindow<PathGeneratorEditor>("Path Generator");
            window.minSize = new Vector2(1200, 700);
            window.Initialize();
        }
        
        private void Initialize()
        {
            pathGenerator = new PathGenerator();
            currentRules = GenerationRules.CreateBalancedRules();
            
            // Load saved preferences
            LoadPreferences();
            
            // Generate initial map
            if (string.IsNullOrEmpty(currentSeed))
            {
                currentSeed = GenerateRandomSeed();
            }
            GenerateMap();
        }
        
        private void OnEnable()
        {
            if (pathGenerator == null)
            {
                Initialize();
            }
        }
        
        private void OnDisable()
        {
            SavePreferences();
        }
        
        #endregion
        
        #region GUI Drawing
        
        private void OnGUI()
        {
            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            {
                DrawSidebar();
                DrawPreviewArea();
            }
            EditorGUILayout.EndHorizontal();
            
            ProcessInput();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(TOOLBAR_HEIGHT));
            {
                if (GUILayout.Button("Generate", EditorStyles.toolbarButton, GUILayout.Width(80)))
                {
                    GenerateMap();
                }
                
                if (GUILayout.Button("Random Seed", EditorStyles.toolbarButton, GUILayout.Width(100)))
                {
                    currentSeed = GenerateRandomSeed();
                    if (autoGenerate) GenerateMap();
                }
                
                GUILayout.Space(10);
                
                EditorGUI.BeginChangeCheck();
                seedInput = EditorGUILayout.TextField(seedInput, EditorStyles.toolbarTextField, GUILayout.Width(200));
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(seedInput))
                {
                    currentSeed = seedInput;
                    if (autoGenerate) GenerateMap();
                }
                
                if (GUILayout.Button("Use Seed", EditorStyles.toolbarButton, GUILayout.Width(70)))
                {
                    if (!string.IsNullOrEmpty(seedInput))
                    {
                        currentSeed = seedInput;
                        GenerateMap();
                    }
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Export", EditorStyles.toolbarDropDown, GUILayout.Width(60)))
                {
                    ShowExportMenu();
                }
                
                if (GUILayout.Button("Help", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    ShowHelpWindow();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawSidebar()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(SIDEBAR_WIDTH));
            {
                sidebarScroll = EditorGUILayout.BeginScrollView(sidebarScroll);
                {
                    DrawGenerationSettings();
                    DrawVisualizationOptions();
                    DrawMetricsPanel();
                    DrawSeedHistory();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGenerationSettings()
        {
            EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            
            // Current seed display
            EditorGUILayout.LabelField("Current Seed:", currentSeed);
            
            // Auto-generate toggle
            autoGenerate = EditorGUILayout.Toggle("Auto Generate", autoGenerate);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rules Configuration", EditorStyles.boldLabel);
            
            // Rule presets
            if (GUILayout.Button("Balanced Rules"))
            {
                currentRules = GenerationRules.CreateBalancedRules();
                if (autoGenerate) GenerateMap();
            }
            
            if (GUILayout.Button("Easy Rules"))
            {
                currentRules = GenerationRules.CreateSafeRules();
                if (autoGenerate) GenerateMap();
            }
            
            if (GUILayout.Button("Hard Rules"))
            {
                currentRules = GenerationRules.CreateChaoticRules();
                if (autoGenerate) GenerateMap();
            }
            
            EditorGUILayout.Space();
            
            // Custom rule editing
            if (currentRules != null)
            {
                EditorGUILayout.LabelField("Custom Parameters", EditorStyles.boldLabel);
                
                currentRules.minPathsPerLayer = EditorGUILayout.IntSlider(
                    "Min Paths/Layer", currentRules.minPathsPerLayer, 3, 10);
                
                currentRules.maxPathsPerLayer = EditorGUILayout.IntSlider(
                    "Max Paths/Layer", currentRules.maxPathsPerLayer, 5, 15);
                
                currentRules.baseEventChance = EditorGUILayout.Slider(
                    "Event Chance", currentRules.baseEventChance, 0f, 1f);
                
                currentRules.ruleFlexibility = EditorGUILayout.Slider(
                    "Rule Flexibility", currentRules.ruleFlexibility, 0f, 0.5f);
                
                currentRules.guaranteedCombatPerLayer = EditorGUILayout.IntSlider(
                    "Guaranteed Combat/Layer", currentRules.guaranteedCombatPerLayer, 1, 5);
                
                currentRules.maxMajorEnemiesPerLayer = EditorGUILayout.IntSlider(
                    "Max Major Enemies/Layer", currentRules.maxMajorEnemiesPerLayer, 0, 3);
            }
            
            if (EditorGUI.EndChangeCheck() && autoGenerate)
            {
                GenerateMap();
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawVisualizationOptions()
        {
            EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            showConnections = EditorGUILayout.Toggle("Show Connections", showConnections);
            showRoomTypes = EditorGUILayout.Toggle("Show Room Types", showRoomTypes);
            showDepthInfo = EditorGUILayout.Toggle("Show Depth Info", showDepthInfo);
            showMetrics = EditorGUILayout.Toggle("Show Metrics", showMetrics);
            showGrid = EditorGUILayout.Toggle("Show Grid", showGrid);
            highlightBosses = EditorGUILayout.Toggle("Highlight Bosses", highlightBosses);
            highlightSecrets = EditorGUILayout.Toggle("Highlight Secrets", highlightSecrets);
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Zoom Level", zoomLevel.ToString("F2"));
            zoomLevel = EditorGUILayout.Slider(zoomLevel, 0.5f, 3f);
            
            if (GUILayout.Button("Reset View"))
            {
                zoomLevel = 1.0f;
                panOffset = Vector2.zero;
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawMetricsPanel()
        {
            if (!showMetrics || currentMetrics == null) return;
            
            EditorGUILayout.LabelField("Generation Metrics", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField($"Total Rooms: {currentMetrics.TotalRooms}");
            EditorGUILayout.LabelField($"Total Connections: {currentMetrics.TotalConnections}");
            EditorGUILayout.LabelField($"Average Branching: {currentMetrics.AverageBranching:F2}");
            EditorGUILayout.LabelField($"Max Depth: {currentMetrics.MaxDepth}");
            EditorGUILayout.LabelField($"Boss Rooms: {currentMetrics.BossRoomCount}");
            EditorGUILayout.LabelField($"Secret Rooms: {currentMetrics.SecretRoomCount}");
            EditorGUILayout.LabelField($"Shop Rooms: {currentMetrics.ShopRoomCount}");
            EditorGUILayout.LabelField($"Complexity Score: {currentMetrics.ComplexityScore:F2}");
            
            EditorGUILayout.Space();
        }
        
        private void DrawSeedHistory()
        {
            EditorGUILayout.LabelField("Seed History", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Favorites
            if (favoritedSeeds.Count > 0)
            {
                EditorGUILayout.LabelField("Favorites:", EditorStyles.miniBoldLabel);
                foreach (var seed in favoritedSeeds)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(seed, EditorStyles.miniButton))
                        {
                            currentSeed = seed;
                            seedInput = seed;
                            GenerateMap();
                        }
                        
                        if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            favoritedSeeds.Remove(seed);
                            break;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();
            }
            
            // Recent seeds
            EditorGUILayout.LabelField("Recent:", EditorStyles.miniBoldLabel);
            for (int i = 0; i < Mathf.Min(10, seedHistory.Count); i++)
            {
                var seed = seedHistory[i];
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(seed, EditorStyles.miniButton))
                    {
                        currentSeed = seed;
                        seedInput = seed;
                        GenerateMap();
                    }
                    
                    if (GUILayout.Button("★", EditorStyles.miniButton, GUILayout.Width(20)))
                    {
                        if (!favoritedSeeds.Contains(seed))
                        {
                            favoritedSeeds.Add(seed);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (seedHistory.Count > 10)
            {
                if (GUILayout.Button("Clear History", EditorStyles.miniButton))
                {
                    seedHistory.Clear();
                }
            }
            
            EditorGUILayout.Space();
        }
        
        private void DrawPreviewArea()
        {
            EditorGUILayout.BeginVertical();
            {
                // Preview area with dark background
                Rect previewRect = GUILayoutUtility.GetRect(
                    GUIContent.none,
                    GUIStyle.none,
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                );
                
                GUI.Box(previewRect, GUIContent.none, EditorStyles.helpBox);
                
                if (Event.current.type == UnityEngine.EventType.Repaint)
                {
                    DrawPreview(previewRect);
                }
                
                // Handle input in preview area
                HandlePreviewInput(previewRect);
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPreview(Rect rect)
        {
            if (currentMap == null) return;
            
            // Set up clipping
            GUI.BeginClip(rect);
            
            // Apply zoom and pan
            Matrix4x4 oldMatrix = GUI.matrix;
            Vector2 pivot = new Vector2(rect.width / 2, rect.height / 2);
            GUIUtility.ScaleAroundPivot(Vector2.one * zoomLevel, pivot);
            
            // Draw grid
            if (showGrid)
            {
                DrawGrid(rect);
            }
            
            // Calculate layout positions for nodes
            Dictionary<string, Vector2> nodePositions = CalculateNodePositions(rect);
            
            // Draw connections
            if (showConnections)
            {
                DrawConnections(nodePositions);
            }
            
            // Draw nodes
            DrawNodes(nodePositions);
            
            // Draw overlay information
            if (showDepthInfo)
            {
                DrawDepthInfo(rect);
            }
            
            // Restore matrix
            GUI.matrix = oldMatrix;
            GUI.EndClip();
        }
        
        private void DrawGrid(Rect rect)
        {
            float gridSize = 50f * zoomLevel;
            int cols = Mathf.CeilToInt(rect.width / gridSize);
            int rows = Mathf.CeilToInt(rect.height / gridSize);
            
            Handles.color = gridColor;
            
            for (int i = 0; i <= cols; i++)
            {
                float x = i * gridSize + panOffset.x % gridSize;
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, rect.height));
            }
            
            for (int i = 0; i <= rows; i++)
            {
                float y = i * gridSize + panOffset.y % gridSize;
                Handles.DrawLine(new Vector3(0, y), new Vector3(rect.width, y));
            }
        }
        
        private Dictionary<string, Vector2> CalculateNodePositions(Rect rect)
        {
            var positions = new Dictionary<string, Vector2>();
            
            if (currentMap == null || currentMap.Layers == null) return positions;
            
            float layerHeight = rect.height / (currentMap.Layers.Count + 1);
            
            for (int layerIndex = 0; layerIndex < currentMap.Layers.Count; layerIndex++)
            {
                var layer = currentMap.Layers[layerIndex];
                if (layer.Nodes == null) continue;
                
                float y = (layerIndex + 1) * layerHeight;
                float nodeWidth = rect.width / (layer.Nodes.Count + 1);
                
                for (int nodeIndex = 0; nodeIndex < layer.Nodes.Count; nodeIndex++)
                {
                    var node = layer.Nodes[nodeIndex];
                    float x = (nodeIndex + 1) * nodeWidth;
                    
                    Vector2 position = new Vector2(x + panOffset.x, y + panOffset.y);
                    positions[GetNodeId(layerIndex, nodeIndex)] = position;
                }
            }
            
            return positions;
        }
        
        private void DrawConnections(Dictionary<string, Vector2> nodePositions)
        {
            if (currentMap == null) return;
            
            Handles.color = connectionColor;
            
            for (int layerIndex = 0; layerIndex < currentMap.Layers.Count - 1; layerIndex++)
            {
                var layer = currentMap.Layers[layerIndex];
                var nextLayer = currentMap.Layers[layerIndex + 1];
                
                for (int nodeIndex = 0; nodeIndex < layer.Nodes.Count; nodeIndex++)
                {
                    var node = layer.Nodes[nodeIndex];
                    string nodeId = GetNodeId(layerIndex, nodeIndex);
                    
                    if (!nodePositions.ContainsKey(nodeId)) continue;
                    Vector2 startPos = nodePositions[nodeId];
                    
                    // Draw connections to next layer
                    foreach (var connection in node.ConnectedNodeIds)
                    {
                        string targetId = connection; // ConnectedNodeIds are already formatted node IDs
                        if (nodePositions.ContainsKey(targetId))
                        {
                            Vector2 endPos = nodePositions[targetId];
                            Handles.DrawAAPolyLine(CONNECTION_WIDTH, startPos, endPos);
                        }
                    }
                }
            }
        }
        
        private void DrawNodes(Dictionary<string, Vector2> nodePositions)
        {
            if (currentMap == null) return;
            
            for (int layerIndex = 0; layerIndex < currentMap.Layers.Count; layerIndex++)
            {
                var layer = currentMap.Layers[layerIndex];
                
                for (int nodeIndex = 0; nodeIndex < layer.Nodes.Count; nodeIndex++)
                {
                    var node = layer.Nodes[nodeIndex];
                    string nodeId = GetNodeId(layerIndex, nodeIndex);
                    
                    if (!nodePositions.ContainsKey(nodeId)) continue;
                    Vector2 position = nodePositions[nodeId];
                    
                    // Determine node color
                    Color nodeColor = GetNodeColor(node);
                    
                    // Draw node
                    Rect nodeRect = new Rect(
                        position.x - NODE_SIZE / 2,
                        position.y - NODE_SIZE / 2,
                        NODE_SIZE,
                        NODE_SIZE
                    );
                    
                    EditorGUI.DrawRect(nodeRect, nodeColor);
                    
                    // Draw room type icon or text
                    if (showRoomTypes)
                    {
                        GUI.Label(nodeRect, GetRoomTypeLabel(node), GetNodeLabelStyle());
                    }
                    
                    // Highlight special rooms
                    if ((highlightBosses && node.Type == NodeType.Boss) ||
                        (highlightSecrets && node.Type == NodeType.Mystery))
                    {
                        DrawNodeHighlight(nodeRect, nodeColor);
                    }
                }
            }
        }
        
        private void DrawNodeHighlight(Rect rect, Color color)
        {
            float pulseAmount = Mathf.Sin((float)EditorApplication.timeSinceStartup * 3f) * 0.5f + 0.5f;
            Color highlightColor = Color.Lerp(color, Color.white, pulseAmount * 0.3f);
            
            Rect highlightRect = new Rect(
                rect.x - 5,
                rect.y - 5,
                rect.width + 10,
                rect.height + 10
            );
            
            EditorGUI.DrawRect(highlightRect, highlightColor * 0.3f);
        }
        
        private void DrawDepthInfo(Rect rect)
        {
            if (currentMap == null) return;
            
            GUIStyle depthStyle = new GUIStyle(EditorStyles.boldLabel);
            depthStyle.fontSize = 14;
            depthStyle.normal.textColor = Color.white;
            
            float layerHeight = rect.height / (currentMap.Layers.Count + 1);
            
            for (int i = 0; i < currentMap.Layers.Count; i++)
            {
                float y = (i + 1) * layerHeight + panOffset.y;
                string depthText = $"Depth {i + 1}";
                
                if (i < currentMap.Layers.Count && currentMap.Layers[i].Boss != null)
                {
                    depthText += $" - {currentMap.Layers[i].Boss}";
                }
                
                GUI.Label(new Rect(10, y - 10, 200, 20), depthText, depthStyle);
            }
        }
        
        #endregion
        
        #region Input Handling
        
        private void ProcessInput()
        {
            Event e = Event.current;
            
            // Keyboard shortcuts
            if (e.type == UnityEngine.EventType.KeyDown)
            {
                switch (e.keyCode)
                {
                    case KeyCode.G:
                        if (e.control)
                        {
                            GenerateMap();
                            e.Use();
                        }
                        break;
                    
                    case KeyCode.R:
                        if (e.control)
                        {
                            currentSeed = GenerateRandomSeed();
                            GenerateMap();
                            e.Use();
                        }
                        break;
                    
                    case KeyCode.E:
                        if (e.control)
                        {
                            ShowExportMenu();
                            e.Use();
                        }
                        break;
                    
                    case KeyCode.Equals:
                    case KeyCode.KeypadPlus:
                        zoomLevel = Mathf.Min(zoomLevel + 0.1f, 3f);
                        Repaint();
                        e.Use();
                        break;
                    
                    case KeyCode.Minus:
                    case KeyCode.KeypadMinus:
                        zoomLevel = Mathf.Max(zoomLevel - 0.1f, 0.5f);
                        Repaint();
                        e.Use();
                        break;
                }
            }
        }
        
        private void HandlePreviewInput(Rect rect)
        {
            Event e = Event.current;
            
            if (!rect.Contains(e.mousePosition)) return;
            
            // Mouse wheel zoom
            if (e.type == UnityEngine.EventType.ScrollWheel)
            {
                float zoomDelta = -e.delta.y * 0.05f;
                zoomLevel = Mathf.Clamp(zoomLevel + zoomDelta, 0.5f, 3f);
                Repaint();
                e.Use();
            }
            
            // Pan with middle mouse or alt+left mouse
            if (e.type == UnityEngine.EventType.MouseDown && (e.button == 2 || (e.button == 0 && e.alt)))
            {
                isDragging = true;
                lastMousePosition = e.mousePosition;
                e.Use();
            }
            
            if (e.type == UnityEngine.EventType.MouseDrag && isDragging)
            {
                Vector2 delta = e.mousePosition - lastMousePosition;
                panOffset += delta;
                lastMousePosition = e.mousePosition;
                Repaint();
                e.Use();
            }
            
            if (e.type == UnityEngine.EventType.MouseUp && isDragging)
            {
                isDragging = false;
                e.Use();
            }
        }
        
        #endregion
        
        #region Generation
        
        private void GenerateMap()
        {
            if (string.IsNullOrEmpty(currentSeed))
            {
                currentSeed = GenerateRandomSeed();
            }
            
            currentMap = pathGenerator.GenerateMapWithRules(currentSeed, currentRules);
            currentMetrics = CalculateMetrics(currentMap);
            
            // Add to history
            if (!seedHistory.Contains(currentSeed))
            {
                seedHistory.Insert(0, currentSeed);
                if (seedHistory.Count > 50)
                {
                    seedHistory.RemoveAt(seedHistory.Count - 1);
                }
            }
            
            Repaint();
        }
        
        private string GenerateRandomSeed()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }
        
        private GenerationMetrics CalculateMetrics(MysticalMap map)
        {
            if (map == null) return null;
            
            var metrics = new GenerationMetrics();
            
            metrics.TotalRooms = map.Layers.Sum(l => l.Nodes?.Count ?? 0);
            metrics.TotalConnections = map.Layers.Sum(l => 
                l.Nodes?.Sum(n => n.ConnectedNodeIds?.Count ?? 0) ?? 0);
            
            metrics.MaxDepth = map.Layers.Count;
            
            foreach (var layer in map.Layers)
            {
                if (layer.Nodes == null) continue;
                
                foreach (var node in layer.Nodes)
                {
                    switch (node.Type)
                    {
                        case NodeType.Boss:
                            metrics.BossRoomCount++;
                            break;
                        case NodeType.Mystery:
                            metrics.SecretRoomCount++;
                            break;
                        case NodeType.RestShop:
                            metrics.ShopRoomCount++;
                            break;
                        case NodeType.Treasure:
                            metrics.TreasureRoomCount++;
                            break;
                        case NodeType.Elite:
                            metrics.EliteRoomCount++;
                            break;
                    }
                }
            }
            
            if (metrics.TotalRooms > 0)
            {
                metrics.AverageBranching = (float)metrics.TotalConnections / metrics.TotalRooms;
            }
            
            // Calculate complexity score
            metrics.ComplexityScore = 
                metrics.TotalRooms * 1.0f +
                metrics.TotalConnections * 0.5f +
                metrics.SecretRoomCount * 2.0f +
                metrics.EliteRoomCount * 1.5f;
            
            return metrics;
        }
        
        #endregion
        
        #region Export
        
        private void ShowExportMenu()
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Export Map JSON"), false, () => ExportMapJSON());
            menu.AddItem(new GUIContent("Export Metrics CSV"), false, () => ExportMetricsCSV());
            menu.AddItem(new GUIContent("Export Screenshot"), false, () => ExportScreenshot());
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Export Seed List"), false, () => ExportSeedList());
            menu.AddItem(new GUIContent("Export Favorites"), false, () => ExportFavorites());
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Generate Test Report"), false, () => GenerateTestReport());
            
            menu.ShowAsContext();
        }
        
        private void ExportMapJSON()
        {
            if (currentMap == null)
            {
                EditorUtility.DisplayDialog("Export Error", "No map to export", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel(
                "Export Map JSON",
                exportPath,
                $"map_{currentSeed}.json",
                "json"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                string json = JsonUtility.ToJson(currentMap, true);
                File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("Export Complete", $"Map exported to:\n{path}", "OK");
            }
        }
        
        private void ExportMetricsCSV()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Metrics CSV",
                exportPath,
                "generation_metrics.csv",
                "csv"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    // Write header
                    writer.WriteLine("Seed,TotalRooms,TotalConnections,MaxDepth,BossRooms,SecretRooms,ShopRooms,ComplexityScore");
                    
                    // Write current metrics
                    if (currentMetrics != null)
                    {
                        writer.WriteLine($"{currentSeed},{currentMetrics.TotalRooms},{currentMetrics.TotalConnections}," +
                            $"{currentMetrics.MaxDepth},{currentMetrics.BossRoomCount},{currentMetrics.SecretRoomCount}," +
                            $"{currentMetrics.ShopRoomCount},{currentMetrics.ComplexityScore:F2}");
                    }
                    
                    // Write history metrics
                    for (int i = 0; i < metricsHistory.Count; i++)
                    {
                        var m = metricsHistory[i];
                        string seed = i < seedHistory.Count ? seedHistory[i] : $"seed_{i}";
                        writer.WriteLine($"{seed},{m.TotalRooms},{m.TotalConnections}," +
                            $"{m.MaxDepth},{m.BossRoomCount},{m.SecretRoomCount}," +
                            $"{m.ShopRoomCount},{m.ComplexityScore:F2}");
                    }
                }
                
                EditorUtility.DisplayDialog("Export Complete", $"Metrics exported to:\n{path}", "OK");
            }
        }
        
        private void ExportScreenshot()
        {
            EditorUtility.DisplayDialog("Screenshot", "Screenshot export would capture the preview area", "OK");
        }
        
        private void ExportSeedList()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Seed List",
                exportPath,
                "seed_list.txt",
                "txt"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllLines(path, seedHistory.ToArray());
                EditorUtility.DisplayDialog("Export Complete", $"Seed list exported to:\n{path}", "OK");
            }
        }
        
        private void ExportFavorites()
        {
            string path = EditorUtility.SaveFilePanel(
                "Export Favorites",
                exportPath,
                "favorite_seeds.txt",
                "txt"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllLines(path, favoritedSeeds.ToArray());
                EditorUtility.DisplayDialog("Export Complete", $"Favorites exported to:\n{path}", "OK");
            }
        }
        
        private void GenerateTestReport()
        {
            string path = EditorUtility.SaveFilePanel(
                "Generate Test Report",
                exportPath,
                $"test_report_{DateTime.Now:yyyyMMdd_HHmmss}.md",
                "md"
            );
            
            if (!string.IsNullOrEmpty(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"# Procedural Generation Test Report");
                    writer.WriteLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    writer.WriteLine();
                    
                    writer.WriteLine($"## Current Configuration");
                    writer.WriteLine($"- Seed: `{currentSeed}`");
                    writer.WriteLine($"- Rules: {currentRules?.GetType().Name ?? "Default"}");
                    writer.WriteLine();
                    
                    if (currentMetrics != null)
                    {
                        writer.WriteLine($"## Generation Metrics");
                        writer.WriteLine($"- Total Rooms: {currentMetrics.TotalRooms}");
                        writer.WriteLine($"- Total Connections: {currentMetrics.TotalConnections}");
                        writer.WriteLine($"- Average Branching: {currentMetrics.AverageBranching:F2}");
                        writer.WriteLine($"- Max Depth: {currentMetrics.MaxDepth}");
                        writer.WriteLine($"- Boss Rooms: {currentMetrics.BossRoomCount}");
                        writer.WriteLine($"- Secret Rooms: {currentMetrics.SecretRoomCount}");
                        writer.WriteLine($"- Shop Rooms: {currentMetrics.ShopRoomCount}");
                        writer.WriteLine($"- Complexity Score: {currentMetrics.ComplexityScore:F2}");
                        writer.WriteLine();
                    }
                    
                    writer.WriteLine($"## Tested Seeds");
                    foreach (var seed in seedHistory.Take(20))
                    {
                        writer.WriteLine($"- `{seed}`");
                    }
                    writer.WriteLine();
                    
                    writer.WriteLine($"## Favorited Seeds");
                    foreach (var seed in favoritedSeeds)
                    {
                        writer.WriteLine($"- `{seed}` ⭐");
                    }
                }
                
                EditorUtility.DisplayDialog("Export Complete", $"Test report generated at:\n{path}", "OK");
            }
        }
        
        #endregion
        
        #region Utility
        
        private string GetNodeId(int layer, int node)
        {
            return $"L{layer}_N{node}";
        }
        
        private Color GetNodeColor(MapNode node)
        {
            switch (node.Type)
            {
                case NodeType.Boss: return bossRoomColor;
                case NodeType.Mystery: return secretRoomColor;
                case NodeType.RestShop: return shopRoomColor;
                case NodeType.Treasure: return treasureRoomColor;
                case NodeType.Elite: return Color.Lerp(normalRoomColor, bossRoomColor, 0.5f);
                default: return normalRoomColor;
            }
        }
        
        private string GetRoomTypeLabel(MapNode node)
        {
            switch (node.Type)
            {
                case NodeType.Boss: return "B";
                case NodeType.Mystery: return "?";
                case NodeType.RestShop: return "$";
                case NodeType.Treasure: return "T";
                case NodeType.Elite: return "E";
                case NodeType.Encounter: return "⚔";
                default: return "•";
            }
        }
        
        private GUIStyle GetNodeLabelStyle()
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            style.fontSize = 16;
            return style;
        }
        
        private void ShowHelpWindow()
        {
            EditorUtility.DisplayDialog(
                "Path Generator Help",
                "Keyboard Shortcuts:\n" +
                "• Ctrl+G: Generate new map\n" +
                "• Ctrl+R: Random seed\n" +
                "• Ctrl+E: Export menu\n" +
                "• +/-: Zoom in/out\n" +
                "• Middle Mouse: Pan view\n" +
                "• Alt+Left Mouse: Pan view\n\n" +
                "Room Types:\n" +
                "• B: Boss room\n" +
                "• ?: Secret room\n" +
                "• $: Shop\n" +
                "• T: Treasure\n" +
                "• E: Elite enemy\n" +
                "• ⚔: Combat",
                "OK"
            );
        }
        
        private void SavePreferences()
        {
            EditorPrefs.SetString("PathGen_FavoriteSeeds", string.Join(",", favoritedSeeds));
            EditorPrefs.SetString("PathGen_LastSeed", currentSeed);
            EditorPrefs.SetBool("PathGen_AutoGenerate", autoGenerate);
            EditorPrefs.SetFloat("PathGen_ZoomLevel", zoomLevel);
        }
        
        private void LoadPreferences()
        {
            string favs = EditorPrefs.GetString("PathGen_FavoriteSeeds", "");
            if (!string.IsNullOrEmpty(favs))
            {
                favoritedSeeds = favs.Split(',').ToList();
            }
            
            currentSeed = EditorPrefs.GetString("PathGen_LastSeed", "");
            autoGenerate = EditorPrefs.GetBool("PathGen_AutoGenerate", true);
            zoomLevel = EditorPrefs.GetFloat("PathGen_ZoomLevel", 1.0f);
        }
        
        #endregion
        
        #region Helper Classes
        
        [Serializable]
        private class GenerationMetrics
        {
            public int TotalRooms;
            public int TotalConnections;
            public float AverageBranching;
            public int MaxDepth;
            public int BossRoomCount;
            public int SecretRoomCount;
            public int ShopRoomCount;
            public int TreasureRoomCount;
            public int EliteRoomCount;
            public float ComplexityScore;
        }
        
        #endregion
    }
}