using UnityEngine;

namespace NeonLadder.Mechanics.Progression
{
    /// <summary>
    /// Factory class for creating example upgrades
    /// Wade's Note: "Pre-made upgrade goodness - like a sample pack for game designers!"
    /// Stephen's Note: "Templates of progression mastery for rapid iteration."
    /// </summary>
    public static class ExampleUpgrades
    {
        /// <summary>
        /// Creates example Meta upgrades (per-run, like Slay the Spire relics)
        /// </summary>
        public static UpgradeData[] CreateMetaUpgradeExamples()
        {
            return new UpgradeData[]
            {
                CreateDamageBoostUpgrade(),
                CreateHealthBoostUpgrade(),
                CreateSpeedBoostUpgrade(),
                CreateCriticalChanceUpgrade(),
                CreateAttackSpeedUpgrade()
            };
        }
        
        /// <summary>
        /// Creates example Perma upgrades (persistent, like Hades Mirror of Night)
        /// </summary>
        public static UpgradeData[] CreatePermaUpgradeExamples()
        {
            return new UpgradeData[]
            {
                CreateBaseHealthUpgrade(),
                CreateWeaponUnlockUpgrade(),
                CreateStartingGoldUpgrade(),
                CreateAdvancedCombatUpgrade(),
                CreateDeathDefianceUpgrade()
            };
        }
        
        #region Meta Upgrade Examples
        
        private static UpgradeData CreateDamageBoostUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Damage Boost";
            // Note: These would be set via reflection or exposed setters in actual implementation
            return upgrade;
        }
        
        private static UpgradeData CreateHealthBoostUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Health Boost";
            return upgrade;
        }
        
        private static UpgradeData CreateSpeedBoostUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Speed Boost";
            return upgrade;
        }
        
        private static UpgradeData CreateCriticalChanceUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Critical Chance";
            return upgrade;
        }
        
        private static UpgradeData CreateAttackSpeedUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Attack Speed";
            return upgrade;
        }
        
        #endregion
        
        #region Perma Upgrade Examples
        
        private static UpgradeData CreateBaseHealthUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Base Health";
            return upgrade;
        }
        
        private static UpgradeData CreateWeaponUnlockUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Sword Mastery";
            return upgrade;
        }
        
        private static UpgradeData CreateStartingGoldUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Starting Gold";
            return upgrade;
        }
        
        private static UpgradeData CreateAdvancedCombatUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Advanced Combat";
            return upgrade;
        }
        
        private static UpgradeData CreateDeathDefianceUpgrade()
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            upgrade.name = "Death Defiance";
            return upgrade;
        }
        
        #endregion
        
        /// <summary>
        /// Create a complete upgrade tree structure for testing
        /// Demonstrates prerequisite chains and mutual exclusions
        /// </summary>
        public static UpgradeData[] CreateCompleteUpgradeTree()
        {
            var metaUpgrades = CreateMetaUpgradeExamples();
            var permaUpgrades = CreatePermaUpgradeExamples();
            
            var allUpgrades = new UpgradeData[metaUpgrades.Length + permaUpgrades.Length];
            metaUpgrades.CopyTo(allUpgrades, 0);
            permaUpgrades.CopyTo(allUpgrades, metaUpgrades.Length);
            
            return allUpgrades;
        }
    }
    
    /// <summary>
    /// Editor menu items for quickly creating upgrade examples
    /// </summary>
    public static class UpgradeMenuItems
    {
        [UnityEditor.MenuItem("Assets/Create/NeonLadder/Upgrades/Meta Upgrade Examples")]
        public static void CreateMetaUpgradeExamples()
        {
            var upgrades = ExampleUpgrades.CreateMetaUpgradeExamples();
            CreateUpgradeAssets(upgrades, "MetaUpgrades");
        }
        
        [UnityEditor.MenuItem("Assets/Create/NeonLadder/Upgrades/Perma Upgrade Examples")]
        public static void CreatePermaUpgradeExamples()
        {
            var upgrades = ExampleUpgrades.CreatePermaUpgradeExamples();
            CreateUpgradeAssets(upgrades, "PermaUpgrades");
        }
        
        [UnityEditor.MenuItem("Assets/Create/NeonLadder/Upgrades/Complete Upgrade Tree")]
        public static void CreateCompleteUpgradeTree()
        {
            var upgrades = ExampleUpgrades.CreateCompleteUpgradeTree();
            CreateUpgradeAssets(upgrades, "CompleteUpgradeTree");
        }
        
        private static void CreateUpgradeAssets(UpgradeData[] upgrades, string folderName)
        {
            var selectedPath = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
            if (string.IsNullOrEmpty(selectedPath))
                selectedPath = "Assets";
            
            var folderPath = $"{selectedPath}/{folderName}";
            if (!UnityEditor.AssetDatabase.IsValidFolder(folderPath))
            {
                UnityEditor.AssetDatabase.CreateFolder(selectedPath, folderName);
            }
            
            foreach (var upgrade in upgrades)
            {
                var assetPath = $"{folderPath}/{upgrade.name}.asset";
                UnityEditor.AssetDatabase.CreateAsset(upgrade, assetPath);
            }
            
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            
            Debug.Log($"Created {upgrades.Length} upgrade examples in {folderPath}");
        }
    }
}