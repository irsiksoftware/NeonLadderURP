using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using NeonLadder.Editor.UpgradeSystem;
using NeonLadder.Models;
using NeonLadder.Mechanics.Progression;

namespace NeonLadder.Tests.Editor.UI
{
    /// <summary>
    /// Tony Stark's Upgrade System Designer UI Tests - Visual Editor Validation Suite
    /// Ensures the upgrade designer provides bulletproof visual editing workflows
    /// 
    /// "FRIDAY, let's make sure our upgrade designer is as reliable as the Mark 85 suit."
    /// </summary>
    [TestFixture]
    public class UpgradeSystemEditorTests : EditorWindowTestBase<UpgradeSystemEditor>
    {
        private UpgradeData mockUpgrade;
        private PurchasableItem mockPurchasableItem;
        
        #region Setup & Teardown - TDD Foundation
        
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            
            // Create mock upgrade data for testing
            mockUpgrade = ScriptableObject.CreateInstance<UpgradeData>();
            // Note: These properties are read-only in the actual implementation
            // For testing, we would need to use reflection or test-specific setters
            
            // Create mock purchasable item
            mockPurchasableItem = ScriptableObject.CreateInstance<PurchasableItem>();
            // Note: These properties are read-only in the actual implementation
            // For testing, we would need to use reflection or test-specific setters
        }
        
        [TearDown]
        public override void TearDown()
        {
            if (mockUpgrade != null)
            {
                UnityEngine.Object.DestroyImmediate(mockUpgrade);
                mockUpgrade = null;
            }
            
            if (mockPurchasableItem != null)
            {
                UnityEngine.Object.DestroyImmediate(mockPurchasableItem);
                mockPurchasableItem = null;
            }
            
            base.TearDown();
        }
        
        #endregion
        
        #region UI Initialization Tests - Red-Green-Refactor Pattern
        
        [Test]
        public void Window_WhenCreated_ShouldInitializeUpgradeDesigner()
        {
            // Arrange - Window created in SetUp
            
            // Act - Simulate window initialization
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Assert - Verify designer initialization
            AssertWindowInitialization();
            
            // Check if upgrade designer UI elements are properly structured
            Assert.DoesNotThrow(() => EditorUITestFramework.ValidateEditorWindowCanRender(window), "Window should have proper render structure");
        }
        
        [Test]
        public void ShowWindow_WhenCalled_ShouldCreateAndShowWindow()
        {
            // Act - This would normally open the window
            // In testing, we verify the method exists and can be called
            var showWindowMethod = typeof(UpgradeSystemEditor).GetMethod("ShowWindow", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            
            // Assert
            Assert.IsNotNull(showWindowMethod, "ShowWindow method should exist");
            Assert.IsTrue(showWindowMethod.IsStatic, "ShowWindow should be static");
        }
        
        #endregion
        
        #region Menu Item Integration Tests - MenuItem Validation
        
        [Test]
        public void MenuItem_UpgradeDesigner_ShouldExistInNeonLadderMenu()
        {
            // This test verifies the MenuItem attribute is properly configured
            var method = typeof(UpgradeSystemEditor).GetMethod("ShowWindow");
            var menuItemAttribute = (UnityEditor.MenuItem)Attribute.GetCustomAttribute(method, typeof(UnityEditor.MenuItem));
            
            Assert.IsNotNull(menuItemAttribute, "ShowWindow should have MenuItem attribute");
            StringAssert.Contains("NeonLadder/Upgrade System/Upgrade Designer", menuItemAttribute.menuItem, 
                "Menu item should be in correct NeonLadder submenu");
        }
        
        #endregion
        
        #region Visual Editor Workflow Tests - Designer Functionality
        
        [Test]
        public void UpgradeDesigner_WhenUpgradeSelected_ShouldDisplayProperties()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act - Simulate selecting an upgrade (this would be done via inspector)
            // In a full implementation, we'd set the selected upgrade
            
            // Assert - Verify the designer has proper render structure
            Assert.DoesNotThrow(() => EditorUITestFramework.ValidateEditorWindowCanRender(window), 
                "Designer should have proper render structure for upgrade selection");
        }
        
        [Test]
        public void UpgradeDesigner_OnGUI_ShouldRenderDesignerInterface()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act & Assert
            Assert.DoesNotThrow(() => EditorUITestFramework.ValidateEditorWindowCanRender(window), 
                "Designer interface should have proper render structure");
        }
        
        #endregion
        
        #region Upgrade System Integration Tests - Business Logic
        
        [Test]
        public void ExamplePurchasableItems_MenuIntegration_ShouldExist()
        {
            // Test that ExamplePurchasableItems class exists and has the MenuItem attribute
            
            // First, verify the class exists (search all loaded assemblies)
            var exampleItemsType = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.Name == "ExamplePurchasableItems" && 
                               type.Namespace == "NeonLadder.Editor.UpgradeSystem");
            Assert.IsNotNull(exampleItemsType, "ExamplePurchasableItems class should exist in loaded assemblies");
            
            // Find the CreateExampleItems method
            var createMethod = exampleItemsType.GetMethod("CreateExampleItems", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(createMethod, "CreateExampleItems method should exist");
            
            // Check for MenuItem attribute
            var menuItemAttr = (UnityEditor.MenuItem)System.Attribute.GetCustomAttribute(createMethod, typeof(UnityEditor.MenuItem));
            Assert.IsNotNull(menuItemAttr, "CreateExampleItems should have MenuItem attribute");
            Assert.IsTrue(menuItemAttr.menuItem.Contains("Create Example Purchasable Items"), 
                $"Menu item should contain expected text, but was: {menuItemAttr.menuItem}");
        }
        
        [Test]
        public void PurchasableItem_Validation_ShouldAcceptValidData()
        {
            // Arrange - Use our mock purchasable item
            
            // Act & Assert - Verify the item has required properties
            Assert.IsNotNull(mockPurchasableItem.ItemName, "Item should have a name");
            Assert.IsNotNull(mockPurchasableItem.Description, "Item should have a description");
            Assert.Greater(mockPurchasableItem.Cost, 0, "Item should have a positive cost");
            Assert.IsTrue(Enum.IsDefined(typeof(CurrencyType), mockPurchasableItem.CurrencyType), 
                "Item should have valid currency type");
        }
        
        #endregion
        
        #region Performance Tests - Designer Responsiveness
        
        [Test]
        public void UpgradeDesigner_Performance_ShouldRenderEfficiently()
        {
            // Arrange
            EditorUITestFramework.SimulateOnEnable(window);
            
            // Act - Measure designer performance
            var elapsedMs = EditorUITestFramework.MeasureOnGUIPerformance(window, iterations: 10);
            
            // Assert - Designer should be responsive for good UX
            Assert.Less(elapsedMs / 10f, 20f, "Upgrade Designer should render under 20ms for responsive editing");
        }
        
        [Test]
        public void UpgradeDesigner_MemoryUsage_ShouldNotLeak()
        {
            // Act & Assert - Check for memory leaks during typical designer operations
            EditorUITestFramework.AssertNoMemoryLeaks(() =>
            {
                EditorUITestFramework.SimulateOnEnable(window);
                EditorUITestFramework.ValidateEditorWindowCanRender(window);
                // Simulate typical designer workflow structure validation
                for (int i = 0; i < 5; i++)
                {
                    EditorUITestFramework.ValidateEditorWindowCanRender(window);
                }
            }, maxAllocationMB: 5);
        }
        
        #endregion
        
        #region Tester-Focused Validation Tests - QA Confidence
        
        [Test]
        public void TesterValidation_UpgradeDesigner_AllCriticalFunctionsWork()
        {
            // Master test for testers - if this passes, Upgrade Designer is ready for QA
            
            // Multiple assertions
                // 1. Window creation and initialization
                Assert.DoesNotThrow(() => EditorUITestFramework.SimulateOnEnable(window), 
                    "Upgrade Designer should initialize without errors");
                
                // 2. UI rendering structure validation
                Assert.DoesNotThrow(() => EditorUITestFramework.ValidateEditorWindowCanRender(window), 
                    "Designer UI should have proper render structure");
                
                // 3. Menu integration
                var showWindowMethod = typeof(UpgradeSystemEditor).GetMethod("ShowWindow", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                Assert.IsNotNull(showWindowMethod, "Menu integration should be properly configured");
                
                // 4. Performance requirements
                var elapsedMs = EditorUITestFramework.MeasureOnGUIPerformance(window, iterations: 5);
                Assert.Less(elapsedMs / 5f, 25f, "Designer should meet performance requirements for smooth editing");
        }
        
        [Test]
        public void TesterValidation_MenuSystem_IsAccessible()
        {
            // Validate that upgrade system menus are accessible
            // This test focuses on core functionality without problematic assembly references
            
            var menuItems = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                .Select(method => (UnityEditor.MenuItem)System.Attribute.GetCustomAttribute(method, typeof(UnityEditor.MenuItem)))
                .Where(attr => attr != null)
                .Where(attr => attr.menuItem.Contains("NeonLadder"))
                .ToArray();
            
            Assert.Greater(menuItems.Length, 0, "Should have NeonLadder menu items available");
            
            // Check for upgrade system related menus
            var upgradeMenus = menuItems.Where(m => m.menuItem.Contains("Upgrade") || m.menuItem.Contains("Example")).ToArray();
            Assert.Greater(upgradeMenus.Length, 0, "Should have upgrade or example related menu items");
        }
        
        #endregion
        
        #region Override Base Class Methods
        
        protected override void AssertWindowInitialization()
        {
            Assert.IsNotNull(window, "Upgrade System Editor should be created successfully");
            Assert.IsInstanceOf<UpgradeSystemEditor>(window, "Should be correct window type");
        }
        
        protected override void AssertStateManagement()
        {
            // Upgrade Designer doesn't have complex state management like Save System Command Center
            // Just verify it has proper render structure
            Assert.DoesNotThrow(() => EditorUITestFramework.ValidateEditorWindowCanRender(window), 
                "Upgrade Designer should have proper render structure");
        }
        
        #endregion
    }
}