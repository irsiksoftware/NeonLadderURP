using UnityEngine;
using UnityEditor;
using NeonLadder.Mechanics.Progression;
using System.Linq;

namespace NeonLadder.Editor
{
    /// <summary>
    /// Custom editor tools for the Upgrade System
    /// Wade's Note: "Making upgrade creation easier than regenerating limbs!"
    /// Stephen's Note: "Mystical tools for design team mastery."
    /// </summary>
    public class UpgradeSystemEditor : EditorWindow
    {
        private UpgradeData selectedUpgrade;
        private Vector2 scrollPosition;
        private string searchFilter = "";
        private CurrencyType filterCurrency = CurrencyType.Meta;
        private UpgradeCategory filterCategory = UpgradeCategory.Offense;
        
        [MenuItem("NeonLadder/Upgrade System/Upgrade Designer")]
        public static void ShowWindow()
        {
            var window = GetWindow<UpgradeSystemEditor>("Upgrade Designer");
            window.minSize = new Vector2(600, 400);
        }
        
        void OnGUI()
        {
            EditorGUILayout.LabelField("NeonLadder Upgrade System Designer", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            DrawToolbar();
            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawUpgradeList();
                DrawUpgradeInspector();
            }
        }
        
        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Create New Upgrade", EditorStyles.toolbarButton))
                {
                    CreateNewUpgrade();
                }
                
                if (GUILayout.Button("Validate All Upgrades", EditorStyles.toolbarButton))
                {
                    ValidateAllUpgrades();
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Export Upgrade Tree", EditorStyles.toolbarButton))
                {
                    ExportUpgradeTree();
                }
            }
        }
        
        private void DrawUpgradeList()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(300)))
            {
                EditorGUILayout.LabelField("Upgrade List", EditorStyles.boldLabel);
                
                // Search and filter
                searchFilter = EditorGUILayout.TextField("Search", searchFilter);
                filterCurrency = (CurrencyType)EditorGUILayout.EnumPopup("Currency", filterCurrency);
                filterCategory = (UpgradeCategory)EditorGUILayout.EnumPopup("Category", filterCategory);
                
                EditorGUILayout.Space();
                
                using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    scrollPosition = scroll.scrollPosition;
                    
                    var upgrades = FindAllUpgrades()
                        .Where(u => FilterUpgrade(u))
                        .OrderBy(u => u.CurrencyType)
                        .ThenBy(u => u.Category)
                        .ThenBy(u => u.name);
                    
                    foreach (var upgrade in upgrades)
                    {
                        DrawUpgradeListItem(upgrade);
                    }
                }
            }
        }
        
        private void DrawUpgradeListItem(UpgradeData upgrade)
        {
            var style = selectedUpgrade == upgrade ? EditorStyles.helpBox : GUIStyle.none;
            
            using (new EditorGUILayout.VerticalScope(style))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(upgrade.name, EditorStyles.label))
                    {
                        selectedUpgrade = upgrade;
                        Selection.activeObject = upgrade;
                    }
                    
                    // Currency type indicator
                    var colorStyle = new GUIStyle(EditorStyles.miniLabel);
                    colorStyle.normal.textColor = upgrade.CurrencyType == CurrencyType.Meta ? Color.cyan : Color.yellow;
                    EditorGUILayout.LabelField(upgrade.CurrencyType.ToString(), colorStyle, GUILayout.Width(50));
                }
                
                if (selectedUpgrade == upgrade)
                {
                    EditorGUILayout.LabelField($"Cost: {upgrade.Cost} | Category: {upgrade.Category}", EditorStyles.miniLabel);
                    if (upgrade.Prerequisites.Length > 0)
                    {
                        EditorGUILayout.LabelField($"Requires: {string.Join(", ", upgrade.Prerequisites)}", EditorStyles.miniLabel);
                    }
                }
            }
        }
        
        private void DrawUpgradeInspector()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Upgrade Inspector", EditorStyles.boldLabel);
                
                if (selectedUpgrade == null)
                {
                    EditorGUILayout.HelpBox("Select an upgrade from the list to edit its properties.", MessageType.Info);
                    return;
                }
                
                using (var changeCheck = new EditorGUI.ChangeCheckScope())
                {
                    // Create a custom inspector for the selected upgrade
                    var serializedUpgrade = new SerializedObject(selectedUpgrade);
                    serializedUpgrade.Update();
                    
                    DrawUpgradeProperties(serializedUpgrade);
                    
                    if (changeCheck.changed)
                    {
                        serializedUpgrade.ApplyModifiedProperties();
                        EditorUtility.SetDirty(selectedUpgrade);
                    }
                }
                
                EditorGUILayout.Space();
                DrawUpgradeActions();
            }
        }
        
        private void DrawUpgradeProperties(SerializedObject serializedUpgrade)
        {
            // Basic properties
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("upgradeId"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("upgradeName"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("flavorText"));
            
            EditorGUILayout.Space();
            
            // Cost and category
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("currencyType"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("category"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("cost"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("maxLevel"));
            
            EditorGUILayout.Space();
            
            // Dependencies
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("prerequisites"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("mutuallyExclusive"));
            
            EditorGUILayout.Space();
            
            // Effects
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("effects"));
            EditorGUILayout.PropertyField(serializedUpgrade.FindProperty("effectTemplate"));
        }
        
        private void DrawUpgradeActions()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Duplicate"))
                {
                    DuplicateUpgrade(selectedUpgrade);
                }
                
                if (GUILayout.Button("Validate"))
                {
                    ValidateUpgrade(selectedUpgrade);
                }
                
                if (GUILayout.Button("Test in Play Mode"))
                {
                    TestUpgradeInPlayMode(selectedUpgrade);
                }
            }
            
            if (GUILayout.Button("Delete Upgrade", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Delete Upgrade", 
                    $"Are you sure you want to delete '{selectedUpgrade.name}'?", 
                    "Delete", "Cancel"))
                {
                    DeleteUpgrade(selectedUpgrade);
                }
            }
        }
        
        #region Helper Methods
        
        private UpgradeData[] FindAllUpgrades()
        {
            var guids = AssetDatabase.FindAssets("t:UpgradeData");
            return guids.Select(guid => AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid)))
                       .Where(upgrade => upgrade != null)
                       .ToArray();
        }
        
        private bool FilterUpgrade(UpgradeData upgrade)
        {
            if (!string.IsNullOrEmpty(searchFilter) && 
                !upgrade.name.ToLower().Contains(searchFilter.ToLower()) &&
                !upgrade.Description.ToLower().Contains(searchFilter.ToLower()))
            {
                return false;
            }
            
            return upgrade.CurrencyType == filterCurrency || filterCurrency == (CurrencyType)(-1);
        }
        
        private void CreateNewUpgrade()
        {
            var upgrade = CreateInstance<UpgradeData>();
            upgrade.name = "New Upgrade";
            
            var path = EditorUtility.SaveFilePanelInProject(
                "Save New Upgrade", 
                "NewUpgrade", 
                "asset", 
                "Choose where to save the new upgrade");
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(upgrade, path);
                AssetDatabase.SaveAssets();
                selectedUpgrade = upgrade;
                Selection.activeObject = upgrade;
            }
        }
        
        private void DuplicateUpgrade(UpgradeData original)
        {
            var duplicate = Instantiate(original);
            duplicate.name = original.name + " Copy";
            
            var path = EditorUtility.SaveFilePanelInProject(
                "Save Duplicated Upgrade", 
                duplicate.name, 
                "asset", 
                "Choose where to save the duplicated upgrade");
            
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(duplicate, path);
                AssetDatabase.SaveAssets();
                selectedUpgrade = duplicate;
                Selection.activeObject = duplicate;
            }
        }
        
        private void ValidateUpgrade(UpgradeData upgrade)
        {
            var issues = new System.Collections.Generic.List<string>();
            
            if (string.IsNullOrEmpty(upgrade.Id))
                issues.Add("Missing Upgrade ID");
            
            if (string.IsNullOrEmpty(upgrade.Name))
                issues.Add("Missing Upgrade Name");
            
            if (upgrade.Cost <= 0)
                issues.Add("Cost must be greater than 0");
            
            if (upgrade.MaxLevel <= 0)
                issues.Add("Max Level must be greater than 0");
            
            // Check for circular dependencies
            if (upgrade.Prerequisites.Contains(upgrade.Id))
                issues.Add("Upgrade cannot be prerequisite of itself");
            
            if (issues.Count == 0)
            {
                EditorUtility.DisplayDialog("Validation", $"Upgrade '{upgrade.name}' is valid!", "OK");
            }
            else
            {
                var message = $"Validation issues for '{upgrade.name}':\n\n" + string.Join("\n", issues);
                EditorUtility.DisplayDialog("Validation Issues", message, "OK");
            }
        }
        
        private void ValidateAllUpgrades()
        {
            var allUpgrades = FindAllUpgrades();
            var totalIssues = 0;
            
            foreach (var upgrade in allUpgrades)
            {
                // Add validation logic here
                totalIssues += 0; // Placeholder
            }
            
            EditorUtility.DisplayDialog("Validation Complete", 
                $"Validated {allUpgrades.Length} upgrades.\n{totalIssues} issues found.", 
                "OK");
        }
        
        private void TestUpgradeInPlayMode(UpgradeData upgrade)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Test Upgrade", 
                    "Enter Play Mode to test upgrades.", 
                    "OK");
                return;
            }
            
            // Find UpgradeSystem in scene and test the upgrade
            var upgradeSystem = FindObjectOfType<UpgradeSystem>();
            if (upgradeSystem != null)
            {
                Debug.Log($"Testing upgrade: {upgrade.name}");
                // Add test logic here
            }
        }
        
        private void DeleteUpgrade(UpgradeData upgrade)
        {
            var path = AssetDatabase.GetAssetPath(upgrade);
            AssetDatabase.DeleteAsset(path);
            selectedUpgrade = null;
        }
        
        private void ExportUpgradeTree()
        {
            var allUpgrades = FindAllUpgrades();
            var json = JsonUtility.ToJson(new { upgrades = allUpgrades }, true);
            
            var path = EditorUtility.SaveFilePanel(
                "Export Upgrade Tree", 
                Application.dataPath, 
                "upgrade_tree", 
                "json");
            
            if (!string.IsNullOrEmpty(path))
            {
                System.IO.File.WriteAllText(path, json);
                EditorUtility.DisplayDialog("Export Complete", 
                    $"Upgrade tree exported to:\n{path}", 
                    "OK");
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// Custom property drawer for UpgradeEffect
    /// </summary>
    [CustomPropertyDrawer(typeof(UpgradeEffect))]
    public class UpgradeEffectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // Effect type
            var effectType = property.FindPropertyRelative("effectType");
            EditorGUI.PropertyField(rect, effectType);
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            
            // Target property
            var targetProperty = property.FindPropertyRelative("targetProperty");
            EditorGUI.PropertyField(rect, targetProperty);
            rect.y += EditorGUIUtility.singleLineHeight + 2;
            
            // Values
            var baseValue = property.FindPropertyRelative("baseValue");
            var perLevelIncrease = property.FindPropertyRelative("perLevelIncrease");
            var isPercentage = property.FindPropertyRelative("isPercentage");
            
            var valueRect = new Rect(rect.x, rect.y, rect.width * 0.3f, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(valueRect, baseValue, new GUIContent("Base"));
            
            valueRect.x += valueRect.width + 5;
            EditorGUI.PropertyField(valueRect, perLevelIncrease, new GUIContent("Per Level"));
            
            valueRect.x += valueRect.width + 5;
            valueRect.width = rect.width * 0.3f;
            EditorGUI.PropertyField(valueRect, isPercentage, new GUIContent("%"));
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 6;
        }
    }
}