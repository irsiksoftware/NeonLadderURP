using UnityEngine;
using UnityEditor;
using NeonLadder.Mechanics.Progression;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Editor.UpgradeSystem
{
    public class UpgradeSystemEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private CurrencyType filterCurrency = CurrencyType.Meta;
        private UpgradeCategory filterCategory = UpgradeCategory.Offense;
        private bool showFilters = true;
        
        private UpgradeData selectedUpgrade;
        private List<UpgradeData> allUpgrades = new List<UpgradeData>();
        
        [MenuItem("NeonLadder/Upgrade System/Upgrade Designer")]
        public static void ShowWindow()
        {
            GetWindow<UpgradeSystemEditor>("Upgrade Designer");
        }
        
        private void OnEnable()
        {
            RefreshUpgradeList();
        }
        
        private void OnGUI()
        {
            EditorGUILayout.LabelField("NeonLadder Upgrade System Designer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawToolbar();
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            {
                // Left panel - Upgrade list
                DrawUpgradeList();
                
                EditorGUILayout.Space();
                
                // Right panel - Selected upgrade details
                DrawUpgradeDetails();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            DrawActionButtons();
        }
        
        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                {
                    RefreshUpgradeList();
                }
                
                GUILayout.FlexibleSpace();
                
                showFilters = GUILayout.Toggle(showFilters, "Filters", EditorStyles.toolbarButton);
                
                if (GUILayout.Button("Create New", EditorStyles.toolbarButton))
                {
                    CreateNewUpgrade();
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (showFilters)
            {
                DrawFilters();
            }
        }
        
        private void DrawFilters()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Filters", EditorStyles.boldLabel);
                
                searchFilter = EditorGUILayout.TextField("Search", searchFilter);
                filterCurrency = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type", filterCurrency);
                filterCategory = (UpgradeCategory)EditorGUILayout.EnumPopup("Category", filterCategory);
                
                if (GUILayout.Button("Clear Filters"))
                {
                    searchFilter = "";
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawUpgradeList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(300));
            {
                EditorGUILayout.LabelField($"Upgrades ({GetFilteredUpgrades().Count})", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    var filteredUpgrades = GetFilteredUpgrades();
                    
                    foreach (var upgrade in filteredUpgrades)
                    {
                        DrawUpgradeListItem(upgrade);
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawUpgradeListItem(UpgradeData upgrade)
        {
            bool isSelected = selectedUpgrade == upgrade;
            var style = isSelected ? EditorStyles.selectionRect : EditorStyles.helpBox;
            
            EditorGUILayout.BeginVertical(style);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(upgrade.Name, EditorStyles.label))
                    {
                        selectedUpgrade = upgrade;
                        Selection.activeObject = upgrade;
                    }
                    
                    GUILayout.FlexibleSpace();
                    
                    // Currency type indicator
                    var currencyColor = upgrade.CurrencyType == CurrencyType.Meta ? Color.cyan : Color.yellow;
                    var prevColor = GUI.color;
                    GUI.color = currencyColor;
                    EditorGUILayout.LabelField(upgrade.CurrencyType.ToString(), GUILayout.Width(50));
                    GUI.color = prevColor;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.LabelField($"Cost: {upgrade.Cost} | Max: {upgrade.MaxLevel} | {upgrade.Category}");
                
                if (!string.IsNullOrEmpty(upgrade.Description))
                {
                    EditorGUILayout.LabelField(upgrade.Description, EditorStyles.wordWrappedMiniLabel);
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawUpgradeDetails()
        {
            EditorGUILayout.BeginVertical();
            {
                if (selectedUpgrade != null)
                {
                    EditorGUILayout.LabelField("Upgrade Details", EditorStyles.boldLabel);
                    
                    // Create a custom editor for the selected upgrade
                    var editor = UnityEditor.Editor.CreateEditor(selectedUpgrade);
                    editor.OnInspectorGUI();
                    
                    EditorGUILayout.Space();
                    
                    DrawUpgradeValidation();
                    
                    if (Application.isPlaying)
                    {
                        DrawPlayModeActions();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Select an upgrade to view details", EditorStyles.centeredGreyMiniLabel);
                }
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawUpgradeValidation()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            
            var issues = ValidateUpgrade(selectedUpgrade);
            
            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("✓ No issues found", MessageType.Info);
            }
            else
            {
                foreach (var issue in issues)
                {
                    EditorGUILayout.HelpBox(issue, MessageType.Warning);
                }
            }
        }
        
        private void DrawPlayModeActions()
        {
            EditorGUILayout.LabelField("Play Mode Testing", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Test in Play Mode"))
            {
                TestUpgradeInPlayMode();
            }
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Create Example Upgrades"))
                {
                    CreateExampleUpgrades();
                }
                
                if (GUILayout.Button("Export Upgrade Tree"))
                {
                    ExportUpgradeTree();
                }
                
                if (selectedUpgrade != null && GUILayout.Button("Duplicate Selected"))
                {
                    DuplicateUpgrade(selectedUpgrade);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        
        private List<UpgradeData> GetFilteredUpgrades()
        {
            var filtered = allUpgrades.AsEnumerable();
            
            if (!string.IsNullOrEmpty(searchFilter))
            {
                filtered = filtered.Where(u => u.Name.ToLower().Contains(searchFilter.ToLower()) || 
                                             u.Id.ToLower().Contains(searchFilter.ToLower()));
            }
            
            return filtered.ToList();
        }
        
        private void RefreshUpgradeList()
        {
            allUpgrades.Clear();
            
            string[] guids = AssetDatabase.FindAssets("t:UpgradeData");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (upgrade != null)
                {
                    allUpgrades.Add(upgrade);
                }
            }
            
            allUpgrades = allUpgrades.OrderBy(u => u.Name).ToList();
        }
        
        private void CreateNewUpgrade()
        {
            var newUpgrade = CreateInstance<UpgradeData>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Upgrade",
                "NewUpgrade",
                "asset",
                "Choose where to save the upgrade");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(newUpgrade, path);
                AssetDatabase.SaveAssets();
                RefreshUpgradeList();
                selectedUpgrade = newUpgrade;
            }
        }
        
        private void CreateExampleUpgrades()
        {
            var examples = new[]
            {
                ("damage_boost", "Damage Boost", CurrencyType.Meta, 50, 3, UpgradeCategory.Offense, "Increases weapon damage"),
                ("health_boost", "Vitality", CurrencyType.Meta, 75, 2, UpgradeCategory.Defense, "Increases maximum health"),
                ("speed_boost", "Swift Strike", CurrencyType.Meta, 30, 5, UpgradeCategory.Utility, "Increases movement speed"),
                ("base_health", "Constitution", CurrencyType.Perma, 100, 10, UpgradeCategory.Core, "Permanent health increase"),
                ("weapon_mastery", "Weapon Mastery", CurrencyType.Perma, 200, 1, UpgradeCategory.Unlocks, "Unlocks advanced weapons"),
                ("starting_gold", "Inheritance", CurrencyType.Perma, 150, 5, UpgradeCategory.Quality, "Start runs with bonus gold")
            };
            
            foreach (var (id, name, currency, cost, maxLevel, category, description) in examples)
            {
                CreateExampleUpgrade(id, name, currency, cost, maxLevel, category, description);
            }
            
            RefreshUpgradeList();
            Debug.Log("Created example upgrades");
        }
        
        private void CreateExampleUpgrade(string id, string name, CurrencyType currency, int cost, int maxLevel, UpgradeCategory category, string description)
        {
            var upgrade = CreateInstance<UpgradeData>();
            
            // Set fields via reflection (since they're private)
            var type = typeof(UpgradeData);
            type.GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, id);
            type.GetField("upgradeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, name);
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, currency);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, cost);
            type.GetField("maxLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, maxLevel);
            type.GetField("category", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, category);
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, description);
            
            string path = $"Assets/Data/Upgrades/{id}.asset";
            string directory = System.IO.Path.GetDirectoryName(path);
            
            if (!AssetDatabase.IsValidFolder(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.CreateAsset(upgrade, path);
        }
        
        private void DuplicateUpgrade(UpgradeData original)
        {
            var duplicate = Instantiate(original);
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Duplicate Upgrade",
                original.name + "_Copy",
                "asset",
                "Choose where to save the duplicated upgrade");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(duplicate, path);
                AssetDatabase.SaveAssets();
                RefreshUpgradeList();
            }
        }
        
        private void TestUpgradeInPlayMode()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Test in Play Mode requires the game to be running");
                return;
            }
            
            var upgradeSystem = FindFirstObjectByType<NeonLadder.Mechanics.Progression.UpgradeSystem>();
            if (upgradeSystem != null)
            {
                // Apply the upgrade effect for testing
                selectedUpgrade.ApplyEffect(upgradeSystem.gameObject);
                Debug.Log($"Applied {selectedUpgrade.Name} for testing");
            }
            else
            {
                Debug.LogWarning("No UpgradeSystem found in scene");
            }
        }
        
        private void ExportUpgradeTree()
        {
            string path = EditorUtility.SaveFilePanel("Export Upgrade Tree", "", "upgrade_tree", "md");
            if (!string.IsNullOrEmpty(path))
            {
                var content = GenerateUpgradeTreeMarkdown();
                System.IO.File.WriteAllText(path, content);
                Debug.Log($"Exported upgrade tree to {path}");
            }
        }
        
        private string GenerateUpgradeTreeMarkdown()
        {
            var content = new System.Text.StringBuilder();
            content.AppendLine("# Upgrade Tree Documentation");
            content.AppendLine();
            
            var metaUpgrades = allUpgrades.Where(u => u.CurrencyType == CurrencyType.Meta).ToList();
            var permaUpgrades = allUpgrades.Where(u => u.CurrencyType == CurrencyType.Perma).ToList();
            
            content.AppendLine("## Meta Upgrades (Per-Run)");
            content.AppendLine();
            foreach (var upgrade in metaUpgrades)
            {
                content.AppendLine($"### {upgrade.Name}");
                content.AppendLine($"- **ID**: {upgrade.Id}");
                content.AppendLine($"- **Cost**: {upgrade.Cost}");
                content.AppendLine($"- **Max Level**: {upgrade.MaxLevel}");
                content.AppendLine($"- **Category**: {upgrade.Category}");
                content.AppendLine($"- **Description**: {upgrade.Description}");
                content.AppendLine();
            }
            
            content.AppendLine("## Perma Upgrades (Persistent)");
            content.AppendLine();
            foreach (var upgrade in permaUpgrades)
            {
                content.AppendLine($"### {upgrade.Name}");
                content.AppendLine($"- **ID**: {upgrade.Id}");
                content.AppendLine($"- **Cost**: {upgrade.Cost}");
                content.AppendLine($"- **Max Level**: {upgrade.MaxLevel}");
                content.AppendLine($"- **Category**: {upgrade.Category}");
                content.AppendLine($"- **Description**: {upgrade.Description}");
                content.AppendLine();
            }
            
            return content.ToString();
        }
        
        private List<string> ValidateUpgrade(UpgradeData upgrade)
        {
            var issues = new List<string>();
            
            if (string.IsNullOrEmpty(upgrade.Id))
                issues.Add("ID is required");
                
            if (string.IsNullOrEmpty(upgrade.Name))
                issues.Add("Name is required");
                
            if (upgrade.Cost <= 0)
                issues.Add("Cost must be greater than 0");
                
            if (upgrade.MaxLevel <= 0)
                issues.Add("Max Level must be greater than 0");
                
            // Check for circular dependencies
            foreach (var prereq in upgrade.Prerequisites)
            {
                if (prereq == upgrade.Id)
                {
                    issues.Add("Upgrade cannot be a prerequisite of itself");
                }
            }
            
            return issues;
        }
    }
}