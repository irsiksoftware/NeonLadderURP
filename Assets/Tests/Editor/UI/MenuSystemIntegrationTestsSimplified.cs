using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadder.Editor.SaveSystem;
using NeonLadder.Editor.UpgradeSystem;
using NeonLadder.Editor.InputSystem;
using NeonLadder.Debugging.Editor;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Simplified Menu System Integration Tests - Core functionality validation
    /// </summary>
    [TestFixture]
    public class MenuSystemIntegrationTestsSimplified
    {
        #region Menu Hierarchy Validation Tests
        
        [Test]
        public void MenuHierarchy_NeonLadderMenuItems_ShouldExist()
        {
            // This test validates that key menu items exist and follow naming conventions
            var menuItems = FindAllMenuItems();
            var neonLadderMenus = menuItems.Where(m => m.menuItem.StartsWith("NeonLadder/")).ToList();
            
            Assert.Greater(neonLadderMenus.Count, 0, "Should have NeonLadder menu items");
            
            // Check for expected categories
            var hasDebugMenus = neonLadderMenus.Any(m => m.menuItem.Contains("/Debug/"));
            var hasUpgradeMenus = neonLadderMenus.Any(m => m.menuItem.Contains("/Upgrade System/"));
            var hasSaveMenus = neonLadderMenus.Any(m => m.menuItem.Contains("/Saves/"));
            
            Assert.IsTrue(hasDebugMenus, "Should have Debug menu items");
            Assert.IsTrue(hasUpgradeMenus, "Should have Upgrade System menu items");
            Assert.IsTrue(hasSaveMenus, "Should have Save System menu items");
        }
        
        #endregion
        
        #region Cross-System Integration Tests
        
        [Test]
        public void SaveSystemAndUpgradeSystem_Integration_ShouldWorkTogether()
        {
            // Test that Save System can handle upgrade-related data
            var saveSystemWindow = EditorUITestFramework.CreateMockWindow<SaveSystemCommandCenter>();
            var upgradeSystemWindow = EditorUITestFramework.CreateMockWindow<UpgradeSystemEditor>();
            
            try
            {
                // Both systems should initialize without conflicts
                Assert.DoesNotThrow(() => EditorUITestFramework.SimulateOnEnable(saveSystemWindow), 
                    "Save System should initialize alongside Upgrade System");
                
                Assert.DoesNotThrow(() => EditorUITestFramework.SimulateOnEnable(upgradeSystemWindow), 
                    "Upgrade System should initialize alongside Save System");
                
                // Both should be able to render simultaneously (validate structure without GUI context issues)
                Assert.DoesNotThrow(() => {
                    EditorUITestFramework.ValidateEditorWindowCanRender(saveSystemWindow);
                    EditorUITestFramework.ValidateEditorWindowCanRender(upgradeSystemWindow);
                }, "Both systems should have proper render capability without conflicts");
            }
            finally
            {
                if (saveSystemWindow != null) UnityEngine.Object.DestroyImmediate(saveSystemWindow);
                if (upgradeSystemWindow != null) UnityEngine.Object.DestroyImmediate(upgradeSystemWindow);
            }
        }
        
        #endregion
        
        #region ScriptableObject Validation Tests
        
        [Test]
        public void ScriptableObjects_CoreTypes_ShouldBeAccessible()
        {
            // Test that key ScriptableObject types are accessible and can be instantiated
            
            // Test PurchasableItem
            Assert.DoesNotThrow(() => {
                var item = ScriptableObject.CreateInstance<NeonLadder.Models.PurchasableItem>();
                Assert.IsNotNull(item, "Should be able to create PurchasableItem");
                UnityEngine.Object.DestroyImmediate(item);
            }, "PurchasableItem should be instantiable");
            
            // Test UpgradeData
            Assert.DoesNotThrow(() => {
                var upgrade = ScriptableObject.CreateInstance<NeonLadder.Mechanics.Progression.UpgradeData>();
                Assert.IsNotNull(upgrade, "Should be able to create UpgradeData");
                UnityEngine.Object.DestroyImmediate(upgrade);
            }, "UpgradeData should be instantiable");
            
            // Test SaveSystemConfig
            Assert.DoesNotThrow(() => {
                var config = ScriptableObject.CreateInstance<NeonLadder.DataManagement.SaveSystemConfig>();
                Assert.IsNotNull(config, "Should be able to create SaveSystemConfig");
                UnityEngine.Object.DestroyImmediate(config);
            }, "SaveSystemConfig should be instantiable");
        }
        
        #endregion
        
        #region Performance Integration Tests
        
        [Test]
        public void MenuSystem_ConcurrentUsage_ShouldRemainResponsive()
        {
            var saveSystemWindow = EditorUITestFramework.CreateMockWindow<SaveSystemCommandCenter>();
            var upgradeSystemWindow = EditorUITestFramework.CreateMockWindow<UpgradeSystemEditor>();
            
            try
            {
                EditorUITestFramework.SimulateOnEnable(saveSystemWindow);
                EditorUITestFramework.SimulateOnEnable(upgradeSystemWindow);
                
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // Validate concurrent structural integrity (avoid GUI context issues in performance tests)
                for (int i = 0; i < 10; i++)
                {
                    EditorUITestFramework.ValidateEditorWindowCanRender(saveSystemWindow);
                    EditorUITestFramework.ValidateEditorWindowCanRender(upgradeSystemWindow);
                }
                
                stopwatch.Stop();
                var averageMs = stopwatch.ElapsedMilliseconds / 10.0;
                
                Assert.Less(averageMs, 50, "Concurrent menu system usage should remain responsive (under 50ms average)");
            }
            finally
            {
                if (saveSystemWindow != null) UnityEngine.Object.DestroyImmediate(saveSystemWindow);
                if (upgradeSystemWindow != null) UnityEngine.Object.DestroyImmediate(upgradeSystemWindow);
            }
        }
        
        #endregion
        
        #region Tester-Focused Integration Validation
        
        [Test]
        public void TesterValidation_MenuSystemReadiness_AllCriticalSystemsWork()
        {
            // Master integration test - if this passes, the menu system is ready for QA
            
            // 1. Menu hierarchy is properly organized
            var menuItems = FindAllMenuItems();
            var neonLadderMenus = menuItems.Where(m => m.menuItem.StartsWith("NeonLadder/")).ToList();
            Assert.Greater(neonLadderMenus.Count, 5, "Should have multiple NeonLadder menu items");
            
            // 2. Key categories exist
            var categories = new[] { "Debug", "Upgrade System", "Saves" };
            foreach (var category in categories)
            {
                var hasCategory = neonLadderMenus.Any(m => m.menuItem.Contains($"/{category}/"));
                Assert.IsTrue(hasCategory, $"Should have '{category}' menu category");
            }
            
            // 3. Core ScriptableObjects are accessible
            Assert.DoesNotThrow(() => {
                var item = ScriptableObject.CreateInstance<NeonLadder.Models.PurchasableItem>();
                UnityEngine.Object.DestroyImmediate(item);
            }, "Core ScriptableObjects should be accessible");
            
            // 4. Multiple systems can coexist without conflicts
            var saveSystem = EditorUITestFramework.CreateMockWindow<SaveSystemCommandCenter>();
            var upgradeSystem = EditorUITestFramework.CreateMockWindow<UpgradeSystemEditor>();
            
            try
            {
                Assert.DoesNotThrow(() => {
                    EditorUITestFramework.SimulateOnEnable(saveSystem);
                    EditorUITestFramework.SimulateOnEnable(upgradeSystem);
                    EditorUITestFramework.ValidateEditorWindowCanRender(saveSystem);
                    EditorUITestFramework.ValidateEditorWindowCanRender(upgradeSystem);
                }, "All menu systems should work together without conflicts");
            }
            finally
            {
                if (saveSystem != null) UnityEngine.Object.DestroyImmediate(saveSystem);
                if (upgradeSystem != null) UnityEngine.Object.DestroyImmediate(upgradeSystem);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private static MenuItem[] FindAllMenuItems()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                .Select(method => method.GetCustomAttribute<MenuItem>())
                .Where(attr => attr != null)
                .ToArray();
        }
        
        #endregion
    }
}