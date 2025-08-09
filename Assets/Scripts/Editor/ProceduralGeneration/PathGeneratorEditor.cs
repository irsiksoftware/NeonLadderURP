using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using NeonLadder.ProceduralGeneration;
using Newtonsoft.Json;

namespace NeonLadder.Editor.ProceduralGeneration
{
    /// <summary>
    /// Visual editor window for procedural generation testing and analysis
    /// Provides real-time preview, seed testing, and parameter tuning
    /// </summary>
    public class PathGeneratorEditor : EditorWindow
    {
        #region Constants
        
        private const string WINDOW_TITLE = "Path Generator Visualizer";
        private const float NODE_SIZE = 30f;
        private const float LAYER_SPACING = 120f;
        private const float NODE_SPACING = 60f;
        private const float CONNECTION_WIDTH = 2f;
        private const float PREVIEW_PADDING = 50f;
        
        #endregion
        
        #region Private Fields
        
        private PathGenerator generator;
        private PathGeneratorConfig currentConfig;
        private MysticalMap currentMap;
        private List<MysticalMap> seedComparisons = new List<MysticalMap>();
        
        // UI State
        private Vector2 scrollPosition;
        private Vector2 seedListScrollPosition;
        private bool showPreview = true;
        private bool showStatistics = true;
        private bool showSeedTesting = true;
        private bool showParameters = true;
        private bool autoRefresh = true;
        
        // Seed Testing
        private string testSeed = "";
        private List<string> seedHistory = new List<string>();
        private int selectedSeedIndex = -1;
        
        // Visualization Options
        private bool showNodeTypes = true;
        private bool showConnections = true;
        private bool showBossRooms = true;
        private bool showEliteRooms = true;
        private bool showRestSites = true;
        private bool showShops = true;
        private bool animatePreview = false;
        
        // Statistics
        private GenerationStatistics currentStats;
        private Dictionary<string, GenerationStatistics> comparisonStats = new Dictionary<string, GenerationStatistics>();
        
        // Preview
        private Rect previewRect;
        private float zoomLevel = 1f;
        private Vector2 panOffset = Vector2.zero;
        
        // Colors
        private readonly Color normalRoomColor = new Color(0.3f, 0.6f, 1f);
        private readonly Color eliteRoomColor = new Color(1f, 0.8f, 0.2f);
        private readonly Color bossRoomColor = new Color(1f, 0.2f, 0.2f);
        private readonly Color restRoomColor = new Color(0.2f, 1f, 0.2f);
        private readonly Color shopRoomColor = new Color(0.8f, 0.2f, 1f);
        private readonly Color treasureRoomColor = new Color(1f, 0.6f, 0f);
        private readonly Color eventRoomColor = new Color(0.5f, 0.5f, 0.5f);
        
        #endregion
        
        #region Menu Items
        
        [MenuItem("NeonLadder/Procedural/Path Visualizer", false, 400)]
        public static void ShowWindow()
        {
            var window = GetWindow<PathGeneratorEditor>(WINDOW_TITLE);
            window.minSize = new Vector2(800, 600);
            window.Initialize();
        }
        
        [MenuItem("NeonLadder/Procedural/Seed Testing", false, 401)]
        public static void ShowSeedTesting()
        {
            var window = GetWindow<PathGeneratorEditor>(WINDOW_TITLE);
            window.minSize = new Vector2(800, 600);
            window.Initialize();
            window.showSeedTesting = true;
        }
        
        [MenuItem("NeonLadder/Procedural/Generation Statistics", false, 402)]
        public static void ShowStatistics()
        {
            var window = GetWindow<PathGeneratorEditor>(WINDOW_TITLE);
            window.minSize = new Vector2(800, 600);
            window.Initialize();
            window.showStatistics = true;
        }
        
        [MenuItem("NeonLadder/Procedural/Export Generation Data", false, 403)]
        public static void ExportGenerationData()
        {
            var window = GetWindow<PathGeneratorEditor>(WINDOW_TITLE);
            window.ExportCurrentMapData();
        }
        
        #endregion
        
        #region Initialization
        
        private void Initialize()
        {
            generator = new PathGenerator();
            LoadOrCreateConfig();
            GenerateNewMap();
        }
        
        private void OnEnable()
        {
            if (generator == null)
                Initialize();
        }
        
        private void LoadOrCreateConfig()
        {
            // Try to find existing config
            var configs = AssetDatabase.FindAssets("t:PathGeneratorConfig");
            if (configs.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(configs[0]);
                currentConfig = AssetDatabase.LoadAssetAtPath<PathGeneratorConfig>(path);
            }
            
            // Create default config if none exists
            if (currentConfig == null)
            {
                currentConfig = ScriptableObject.CreateInstance<PathGeneratorConfig>();
                currentConfig.rules = GenerationRules.CreateBalancedRules();
            }
        }
        
        #endregion
        
        #region GUI
        
        private void OnGUI()
        {
            DrawToolbar();
            
            EditorGUILayout.BeginHorizontal();
            
            // Left Panel - Controls
            DrawControlPanel();
            
            // Right Panel - Preview
            DrawPreviewPanel();
            
            EditorGUILayout.EndHorizontal();
            
            // Handle input
            HandleInput();
            
            if (autoRefresh && GUI.changed)
            {
                GenerateNewMap();
            }
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            if (GUILayout.Button("Generate New", EditorStyles.toolbarButton, GUILayout.Width(100)))
            {
                GenerateNewMap();
            }
            
            if (GUILayout.Button("Export JSON", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ExportCurrentMapData();
            }
            
            if (GUILayout.Button("Export Image", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ExportPreviewImage();
            }
            
            GUILayout.FlexibleSpace();
            
            autoRefresh = GUILayout.Toggle(autoRefresh, "Auto Refresh", EditorStyles.toolbarButton);
            animatePreview = GUILayout.Toggle(animatePreview, "Animate", EditorStyles.toolbarButton);
            
            if (GUILayout.Button("Reset View", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ResetPreviewView();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawControlPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(350));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Configuration Section
            DrawConfigurationSection();
            
            // Seed Testing Section
            if (showSeedTesting)
                DrawSeedTestingSection();
            
            // Parameters Section
            if (showParameters)
                DrawParametersSection();
            
            // Statistics Section
            if (showStatistics)
                DrawStatisticsSection();
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawConfigurationSection()
        {
            EditorGUILayout.BeginVertical("box");
            showPreview = EditorGUILayout.Foldout(showPreview, "Configuration", true);
            
            if (showPreview)
            {
                EditorGUI.BeginChangeCheck();
                currentConfig = EditorGUILayout.ObjectField("Config Asset", currentConfig, typeof(PathGeneratorConfig), false) as PathGeneratorConfig;
                
                if (EditorGUI.EndChangeCheck() && currentConfig != null)
                {
                    GenerateNewMap();
                }
                
                EditorGUILayout.Space(5);
                
                // Visualization Options
                EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);
                showNodeTypes = EditorGUILayout.Toggle("Show Node Types", showNodeTypes);
                showConnections = EditorGUILayout.Toggle("Show Connections", showConnections);
                showBossRooms = EditorGUILayout.Toggle("Show Boss Rooms", showBossRooms);
                showEliteRooms = EditorGUILayout.Toggle("Show Elite Rooms", showEliteRooms);
                showRestSites = EditorGUILayout.Toggle("Show Rest Sites", showRestSites);
                showShops = EditorGUILayout.Toggle("Show Shops", showShops);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawSeedTestingSection()
        {
            EditorGUILayout.BeginVertical("box");
            showSeedTesting = EditorGUILayout.Foldout(showSeedTesting, "Seed Testing", true);
            
            if (showSeedTesting)
            {
                EditorGUILayout.BeginHorizontal();
                testSeed = EditorGUILayout.TextField("Test Seed", testSeed);
                
                if (GUILayout.Button("Generate", GUILayout.Width(70)))
                {
                    if (!string.IsNullOrEmpty(testSeed))
                    {
                        GenerateWithSeed(testSeed);
                        if (!seedHistory.Contains(testSeed))
                            seedHistory.Add(testSeed);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                if (GUILayout.Button("Generate Random Seed"))
                {
                    testSeed = Guid.NewGuid().ToString().Substring(0, 8);
                    GenerateWithSeed(testSeed);
                    seedHistory.Add(testSeed);
                }
                
                // Seed History
                if (seedHistory.Count > 0)
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("Seed History", EditorStyles.boldLabel);
                    
                    seedListScrollPosition = EditorGUILayout.BeginScrollView(seedListScrollPosition, GUILayout.MaxHeight(100));
                    
                    for (int i = seedHistory.Count - 1; i >= 0; i--)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        if (GUILayout.Button(seedHistory[i], EditorStyles.label))
                        {
                            testSeed = seedHistory[i];
                            GenerateWithSeed(testSeed);
                            selectedSeedIndex = i;
                        }
                        
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                        {
                            seedHistory.RemoveAt(i);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndScrollView();
                }
                
                // Seed Comparison
                if (GUILayout.Button("Compare Seeds"))
                {
                    CompareSeedsWindow.ShowWindow(this);
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawParametersSection()
        {
            EditorGUILayout.BeginVertical("box");
            showParameters = EditorGUILayout.Foldout(showParameters, "Generation Parameters", true);
            
            if (showParameters && currentConfig != null && currentConfig.rules != null)
            {
                var rules = currentConfig.rules;
                
                EditorGUI.BeginChangeCheck();
                
                // Node Count Ranges
                EditorGUILayout.LabelField("Node Count Ranges", EditorStyles.boldLabel);
                rules.MinNodesPerLayer = EditorGUILayout.IntSlider("Min Nodes", rules.MinNodesPerLayer, 3, 10);
                rules.MaxNodesPerLayer = EditorGUILayout.IntSlider("Max Nodes", rules.MaxNodesPerLayer, rules.MinNodesPerLayer, 15);
                
                EditorGUILayout.Space(5);
                
                // Room Type Weights
                EditorGUILayout.LabelField("Room Type Weights", EditorStyles.boldLabel);
                rules.CombatWeight = EditorGUILayout.Slider("Combat", rules.CombatWeight, 0f, 1f);
                rules.EliteWeight = EditorGUILayout.Slider("Elite", rules.EliteWeight, 0f, 0.5f);
                rules.RestWeight = EditorGUILayout.Slider("Rest", rules.RestWeight, 0f, 0.5f);
                rules.ShopWeight = EditorGUILayout.Slider("Shop", rules.ShopWeight, 0f, 0.5f);
                rules.EventWeight = EditorGUILayout.Slider("Event", rules.EventWeight, 0f, 1f);
                rules.TreasureWeight = EditorGUILayout.Slider("Treasure", rules.TreasureWeight, 0f, 0.3f);
                
                EditorGUILayout.Space(5);
                
                // Connection Rules
                EditorGUILayout.LabelField("Connection Rules", EditorStyles.boldLabel);
                rules.MinConnectionsPerNode = EditorGUILayout.IntSlider("Min Connections", rules.MinConnectionsPerNode, 1, 3);
                rules.MaxConnectionsPerNode = EditorGUILayout.IntSlider("Max Connections", rules.MaxConnectionsPerNode, rules.MinConnectionsPerNode, 5);
                rules.CrossConnectionChance = EditorGUILayout.Slider("Cross Connection %", rules.CrossConnectionChance, 0f, 0.5f);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(currentConfig);
                    if (autoRefresh)
                        GenerateNewMap();
                }
                
                EditorGUILayout.Space(5);
                
                // Preset Management
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Save Preset"))
                {
                    SavePreset();
                }
                if (GUILayout.Button("Load Preset"))
                {
                    LoadPreset();
                }
                if (GUILayout.Button("Reset to Default"))
                {
                    currentConfig.rules = GenerationRules.CreateBalancedRules();
                    GenerateNewMap();
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawStatisticsSection()
        {
            EditorGUILayout.BeginVertical("box");
            showStatistics = EditorGUILayout.Foldout(showStatistics, "Generation Statistics", true);
            
            if (showStatistics && currentStats != null)
            {
                EditorGUILayout.LabelField("Current Map", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Seed: {currentStats.Seed}");
                EditorGUILayout.LabelField($"Total Nodes: {currentStats.TotalNodes}");
                EditorGUILayout.LabelField($"Total Connections: {currentStats.TotalConnections}");
                EditorGUILayout.LabelField($"Layers: {currentStats.LayerCount}");
                
                EditorGUILayout.Space(5);
                
                // Room Type Distribution
                EditorGUILayout.LabelField("Room Distribution", EditorStyles.boldLabel);
                foreach (var kvp in currentStats.RoomTypeDistribution)
                {
                    float percentage = (float)kvp.Value / currentStats.TotalNodes * 100f;
                    EditorGUILayout.LabelField($"{kvp.Key}: {kvp.Value} ({percentage:F1}%)");
                }
                
                EditorGUILayout.Space(5);
                
                // Path Complexity
                EditorGUILayout.LabelField("Path Complexity", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Average Connections: {currentStats.AverageConnectionsPerNode:F2}");
                EditorGUILayout.LabelField($"Max Path Length: {currentStats.MaxPathLength}");
                EditorGUILayout.LabelField($"Path Branching Factor: {currentStats.BranchingFactor:F2}");
                
                EditorGUILayout.Space(5);
                
                if (GUILayout.Button("Export Statistics"))
                {
                    ExportStatistics();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawPreviewPanel()
        {
            EditorGUILayout.BeginVertical();
            
            // Get preview rect
            previewRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            
            // Draw background
            EditorGUI.DrawRect(previewRect, new Color(0.15f, 0.15f, 0.15f));
            
            if (currentMap != null)
            {
                DrawMap(currentMap, previewRect);
            }
            else
            {
                var centeredStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(previewRect, "No map generated\nClick 'Generate New' to create a map", centeredStyle);
            }
            
            // Draw zoom controls
            DrawZoomControls();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMap(MysticalMap map, Rect rect)
        {
            if (map == null || map.Layers == null || map.Layers.Count == 0)
                return;
            
            // Calculate layout
            float totalWidth = rect.width - (PREVIEW_PADDING * 2);
            float totalHeight = rect.height - (PREVIEW_PADDING * 2);
            
            // Apply zoom and pan
            Matrix4x4 oldMatrix = GUI.matrix;
            GUIUtility.ScaleAroundPivot(Vector2.one * zoomLevel, rect.center);
            
            Vector2 offset = new Vector2(rect.x + PREVIEW_PADDING + panOffset.x, rect.y + PREVIEW_PADDING + panOffset.y);
            
            // Draw connections first (behind nodes)
            if (showConnections)
            {
                DrawConnections(map, offset, totalWidth, totalHeight);
            }
            
            // Draw nodes
            DrawNodes(map, offset, totalWidth, totalHeight);
            
            // Draw layer labels
            DrawLayerLabels(map, offset, totalWidth, totalHeight);
            
            GUI.matrix = oldMatrix;
        }
        
        private void DrawConnections(MysticalMap map, Vector2 offset, float width, float height)
        {
            Handles.BeginGUI();
            
            for (int layerIndex = 0; layerIndex < map.Layers.Count - 1; layerIndex++)
            {
                var currentLayer = map.Layers[layerIndex];
                var nextLayer = map.Layers[layerIndex + 1];
                
                float currentY = offset.y + (layerIndex * LAYER_SPACING);
                float nextY = offset.y + ((layerIndex + 1) * LAYER_SPACING);
                
                foreach (var node in currentLayer.Nodes)
                {
                    float currentX = offset.x + GetNodeXPosition(node.Position, currentLayer.Nodes.Count, width);
                    
                    foreach (var connectionId in node.ConnectionIds)
                    {
                        var targetNode = nextLayer.Nodes.FirstOrDefault(n => n.Id == connectionId);
                        if (targetNode != null)
                        {
                            float targetX = offset.x + GetNodeXPosition(targetNode.Position, nextLayer.Nodes.Count, width);
                            
                            Color connectionColor = Color.white * 0.3f;
                            if (animatePreview)
                            {
                                float pulse = Mathf.Sin((float)EditorApplication.timeSinceStartup * 2f) * 0.5f + 0.5f;
                                connectionColor = Color.Lerp(Color.white * 0.2f, Color.white * 0.5f, pulse);
                            }
                            
                            Handles.color = connectionColor;
                            Handles.DrawAAPolyLine(CONNECTION_WIDTH, 
                                new Vector3(currentX, currentY + NODE_SIZE / 2), 
                                new Vector3(targetX, nextY - NODE_SIZE / 2));
                        }
                    }
                }
            }
            
            Handles.EndGUI();
        }
        
        private void DrawNodes(MysticalMap map, Vector2 offset, float width, float height)
        {
            for (int layerIndex = 0; layerIndex < map.Layers.Count; layerIndex++)
            {
                var layer = map.Layers[layerIndex];
                float y = offset.y + (layerIndex * LAYER_SPACING);
                
                foreach (var node in layer.Nodes)
                {
                    float x = offset.x + GetNodeXPosition(node.Position, layer.Nodes.Count, width);
                    
                    // Determine node color based on type
                    Color nodeColor = GetNodeColor(node.Properties?.RoomType ?? "Combat");
                    
                    if (!ShouldShowNodeType(node.Properties?.RoomType))
                        continue;
                    
                    // Draw node
                    Rect nodeRect = new Rect(x - NODE_SIZE / 2, y - NODE_SIZE / 2, NODE_SIZE, NODE_SIZE);
                    
                    if (animatePreview && node.Properties?.RoomType == "Boss")
                    {
                        float pulse = Mathf.Sin((float)EditorApplication.timeSinceStartup * 4f) * 0.2f + 0.8f;
                        nodeColor = Color.Lerp(nodeColor, Color.red, pulse);
                    }
                    
                    EditorGUI.DrawRect(nodeRect, nodeColor);
                    
                    // Draw node label
                    if (showNodeTypes)
                    {
                        var labelStyle = new GUIStyle(EditorStyles.miniLabel) 
                        { 
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = Color.white }
                        };
                        
                        string label = GetNodeTypeAbbreviation(node.Properties?.RoomType ?? "?");
                        GUI.Label(nodeRect, label, labelStyle);
                    }
                }
            }
        }
        
        private void DrawLayerLabels(MysticalMap map, Vector2 offset, float width, float height)
        {
            var labelStyle = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleLeft };
            
            for (int i = 0; i < map.Layers.Count; i++)
            {
                float y = offset.y + (i * LAYER_SPACING);
                string label = $"Layer {i + 1}: {map.Layers[i].BossName}";
                
                GUI.Label(new Rect(offset.x - 100, y - 10, 90, 20), label, labelStyle);
            }
        }
        
        private void DrawZoomControls()
        {
            Rect zoomRect = new Rect(previewRect.xMax - 150, previewRect.yMax - 30, 140, 20);
            
            EditorGUI.BeginChangeCheck();
            zoomLevel = GUI.HorizontalSlider(new Rect(zoomRect.x, zoomRect.y, 100, 20), zoomLevel, 0.5f, 2f);
            GUI.Label(new Rect(zoomRect.x + 105, zoomRect.y, 40, 20), $"{(zoomLevel * 100):F0}%");
            
            if (EditorGUI.EndChangeCheck())
            {
                Repaint();
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void GenerateNewMap()
        {
            if (generator == null)
                generator = new PathGenerator();
            
            string seed = string.IsNullOrEmpty(testSeed) ? null : testSeed;
            currentMap = generator.GenerateMapWithRules(seed, currentConfig?.rules);
            
            if (currentMap != null)
            {
                currentStats = CalculateStatistics(currentMap);
                testSeed = currentMap.Seed;
            }
            
            Repaint();
        }
        
        private void GenerateWithSeed(string seed)
        {
            if (generator == null)
                generator = new PathGenerator();
            
            currentMap = generator.GenerateMapWithRules(seed, currentConfig?.rules);
            
            if (currentMap != null)
            {
                currentStats = CalculateStatistics(currentMap);
            }
            
            Repaint();
        }
        
        private float GetNodeXPosition(int position, int nodeCount, float totalWidth)
        {
            if (nodeCount <= 1)
                return totalWidth / 2;
            
            float spacing = totalWidth / (nodeCount + 1);
            return spacing * (position + 1);
        }
        
        private Color GetNodeColor(string roomType)
        {
            switch (roomType?.ToLower())
            {
                case "boss": return bossRoomColor;
                case "elite": return eliteRoomColor;
                case "rest": return restRoomColor;
                case "shop": return shopRoomColor;
                case "treasure": return treasureRoomColor;
                case "event": return eventRoomColor;
                default: return normalRoomColor;
            }
        }
        
        private bool ShouldShowNodeType(string roomType)
        {
            switch (roomType?.ToLower())
            {
                case "boss": return showBossRooms;
                case "elite": return showEliteRooms;
                case "rest": return showRestSites;
                case "shop": return showShops;
                default: return true;
            }
        }
        
        private string GetNodeTypeAbbreviation(string roomType)
        {
            switch (roomType?.ToLower())
            {
                case "boss": return "B";
                case "elite": return "E";
                case "rest": return "R";
                case "shop": return "$";
                case "treasure": return "T";
                case "event": return "?";
                case "combat": return "⚔";
                default: return "•";
            }
        }
        
        private GenerationStatistics CalculateStatistics(MysticalMap map)
        {
            var stats = new GenerationStatistics
            {
                Seed = map.Seed,
                LayerCount = map.Layers.Count,
                TotalNodes = 0,
                TotalConnections = 0,
                RoomTypeDistribution = new Dictionary<string, int>()
            };
            
            foreach (var layer in map.Layers)
            {
                stats.TotalNodes += layer.Nodes.Count;
                
                foreach (var node in layer.Nodes)
                {
                    stats.TotalConnections += node.ConnectionIds.Count;
                    
                    string roomType = node.Properties?.RoomType ?? "Unknown";
                    if (!stats.RoomTypeDistribution.ContainsKey(roomType))
                        stats.RoomTypeDistribution[roomType] = 0;
                    stats.RoomTypeDistribution[roomType]++;
                }
            }
            
            if (stats.TotalNodes > 0)
            {
                stats.AverageConnectionsPerNode = (float)stats.TotalConnections / stats.TotalNodes;
            }
            
            // Calculate path complexity
            stats.MaxPathLength = map.Layers.Count;
            stats.BranchingFactor = CalculateBranchingFactor(map);
            
            return stats;
        }
        
        private float CalculateBranchingFactor(MysticalMap map)
        {
            if (map.Layers.Count < 2)
                return 0;
            
            float totalBranching = 0;
            int layerCount = 0;
            
            for (int i = 0; i < map.Layers.Count - 1; i++)
            {
                int currentNodes = map.Layers[i].Nodes.Count;
                int nextNodes = map.Layers[i + 1].Nodes.Count;
                
                if (currentNodes > 0)
                {
                    totalBranching += (float)nextNodes / currentNodes;
                    layerCount++;
                }
            }
            
            return layerCount > 0 ? totalBranching / layerCount : 0;
        }
        
        private void HandleInput()
        {
            Event e = Event.current;
            
            if (previewRect.Contains(e.mousePosition))
            {
                // Mouse wheel zoom
                if (e.type == EventType.ScrollWheel)
                {
                    float delta = e.delta.y * 0.05f;
                    zoomLevel = Mathf.Clamp(zoomLevel - delta, 0.5f, 2f);
                    e.Use();
                    Repaint();
                }
                
                // Middle mouse pan
                if (e.type == EventType.MouseDrag && e.button == 2)
                {
                    panOffset += e.delta;
                    e.Use();
                    Repaint();
                }
            }
        }
        
        private void ResetPreviewView()
        {
            zoomLevel = 1f;
            panOffset = Vector2.zero;
            Repaint();
        }
        
        #endregion
        
        #region Export Methods
        
        private void ExportCurrentMapData()
        {
            if (currentMap == null)
            {
                EditorUtility.DisplayDialog("No Map", "Generate a map first before exporting.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Export Map Data", "", $"map_{currentMap.Seed}.json", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = JsonConvert.SerializeObject(currentMap, Formatting.Indented);
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Export Complete", $"Map data exported to:\n{path}", "OK");
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Export Failed", $"Failed to export map data:\n{e.Message}", "OK");
                }
            }
        }
        
        private void ExportPreviewImage()
        {
            EditorUtility.DisplayDialog("Export Image", "Image export functionality coming soon!", "OK");
        }
        
        private void ExportStatistics()
        {
            if (currentStats == null)
            {
                EditorUtility.DisplayDialog("No Statistics", "Generate a map first to create statistics.", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanel("Export Statistics", "", $"stats_{currentStats.Seed}.json", "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string json = JsonConvert.SerializeObject(currentStats, Formatting.Indented);
                    File.WriteAllText(path, json);
                    EditorUtility.DisplayDialog("Export Complete", $"Statistics exported to:\n{path}", "OK");
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Export Failed", $"Failed to export statistics:\n{e.Message}", "OK");
                }
            }
        }
        
        private void SavePreset()
        {
            if (currentConfig == null)
                return;
            
            string path = EditorUtility.SaveFilePanel("Save Preset", "Assets", "GenerationPreset", "asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                
                if (string.IsNullOrEmpty(path))
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please save within the project Assets folder.", "OK");
                    return;
                }
                
                var preset = ScriptableObject.CreateInstance<PathGeneratorConfig>();
                preset.rules = currentConfig.rules;
                preset.configurationName = Path.GetFileNameWithoutExtension(path);
                
                AssetDatabase.CreateAsset(preset, path);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("Preset Saved", $"Preset saved to:\n{path}", "OK");
            }
        }
        
        private void LoadPreset()
        {
            string path = EditorUtility.OpenFilePanel("Load Preset", "Assets", "asset");
            
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                
                if (string.IsNullOrEmpty(path))
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Please select a file within the project.", "OK");
                    return;
                }
                
                var preset = AssetDatabase.LoadAssetAtPath<PathGeneratorConfig>(path);
                
                if (preset != null)
                {
                    currentConfig = preset;
                    GenerateNewMap();
                    EditorUtility.DisplayDialog("Preset Loaded", $"Loaded preset: {preset.configurationName}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Load Failed", "Failed to load preset. Make sure it's a valid PathGeneratorConfig asset.", "OK");
                }
            }
        }
        
        #endregion
        
        #region Data Classes
        
        [Serializable]
        private class GenerationStatistics
        {
            public string Seed;
            public int LayerCount;
            public int TotalNodes;
            public int TotalConnections;
            public float AverageConnectionsPerNode;
            public int MaxPathLength;
            public float BranchingFactor;
            public Dictionary<string, int> RoomTypeDistribution;
        }
        
        #endregion
    }
    
    /// <summary>
    /// Seed comparison window for side-by-side analysis
    /// </summary>
    public class CompareSeedsWindow : EditorWindow
    {
        private PathGeneratorEditor parentEditor;
        private List<string> compareSeeds = new List<string> { "", "", "", "" };
        private List<MysticalMap> compareMaps = new List<MysticalMap>();
        private PathGenerator generator = new PathGenerator();
        
        public static void ShowWindow(PathGeneratorEditor parent)
        {
            var window = GetWindow<CompareSeedsWindow>("Seed Comparison");
            window.parentEditor = parent;
            window.minSize = new Vector2(800, 400);
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("Compare Multiple Seeds", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Seed inputs
            EditorGUILayout.BeginHorizontal();
            
            for (int i = 0; i < 4; i++)
            {
                EditorGUILayout.BeginVertical();
                compareSeeds[i] = EditorGUILayout.TextField($"Seed {i + 1}", compareSeeds[i]);
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Generate All", GUILayout.Height(30)))
            {
                GenerateComparisons();
            }
            
            // Display preview grids
            if (compareMaps.Count > 0)
            {
                EditorGUILayout.Space();
                Rect previewArea = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                
                float gridWidth = previewArea.width / 2;
                float gridHeight = previewArea.height / 2;
                
                for (int i = 0; i < Mathf.Min(4, compareMaps.Count); i++)
                {
                    int row = i / 2;
                    int col = i % 2;
                    
                    Rect gridRect = new Rect(
                        previewArea.x + (col * gridWidth),
                        previewArea.y + (row * gridHeight),
                        gridWidth - 5,
                        gridHeight - 5
                    );
                    
                    EditorGUI.DrawRect(gridRect, new Color(0.2f, 0.2f, 0.2f));
                    
                    // Draw mini map preview
                    if (compareMaps[i] != null)
                    {
                        GUI.Label(new Rect(gridRect.x + 5, gridRect.y + 5, 200, 20), 
                            $"Seed: {compareMaps[i].Seed}", EditorStyles.whiteBoldLabel);
                    }
                }
            }
        }
        
        private void GenerateComparisons()
        {
            compareMaps.Clear();
            
            foreach (var seed in compareSeeds)
            {
                if (!string.IsNullOrEmpty(seed))
                {
                    var map = generator.GenerateMap(seed);
                    compareMaps.Add(map);
                }
            }
            
            Repaint();
        }
    }
}