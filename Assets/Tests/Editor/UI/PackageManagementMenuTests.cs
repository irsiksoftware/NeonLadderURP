using NUnit.Framework;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using NeonLadder.Editor;

namespace NeonLadder.Tests.Editor.UI
{
    [TestFixture]
    public class PackageManagementMenuTests
    {
        #region Test Setup and Teardown
        
        [SetUp]
        public void Setup()
        {
            // Test setup if needed
        }
        
        [TearDown]
        public void TearDown()
        {
            // Test teardown if needed
        }
        
        #endregion
        
        #region MenuItem Tests
        
        [Test]
        public void MenuItem_PackageManagement_ExistsUnderNeonLadder()
        {
            // Arrange
            var packageManagementType = System.Type.GetType("NeonLadder.Editor.PackageManagement, NeonLadder.Editor");
            
            // Act & Assert - First check if the type exists
            Assert.IsNotNull(packageManagementType, 
                "PackageManagement class should exist in NeonLadder.Editor namespace");
            
            // Check for the MenuItem attribute on any method
            var methods = packageManagementType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            bool hasMenuItemAttribute = false;
            MenuItem foundMenuItem = null;
            
            foreach (var method in methods)
            {
                var menuItemAttr = method.GetCustomAttribute<MenuItem>();
                if (menuItemAttr != null && menuItemAttr.menuItem.StartsWith("NeonLadder/Package Management"))
                {
                    hasMenuItemAttribute = true;
                    foundMenuItem = menuItemAttr;
                    break;
                }
            }
            
            // Assert
            Assert.IsTrue(hasMenuItemAttribute, 
                "Should have at least one MenuItem under 'NeonLadder/Package Management'");
            Assert.IsNotNull(foundMenuItem, 
                "MenuItem attribute should be found");
            Assert.That(foundMenuItem.menuItem, Does.StartWith("NeonLadder/Package Management"),
                "Menu path should start with 'NeonLadder/Package Management'");
        }
        
        [Test] 
        public void MenuItem_PackageManagement_HasCorrectPriority()
        {
            // Arrange
            var packageManagementType = System.Type.GetType("NeonLadder.Editor.PackageManagement, NeonLadder.Editor");
            
            // Skip test if type doesn't exist yet (TDD approach)
            if (packageManagementType == null)
            {
                Assert.Inconclusive("PackageManagement type not yet implemented");
                return;
            }
            
            // Act
            var methods = packageManagementType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            MenuItem menuItem = null;
            
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<MenuItem>();
                if (attr != null && attr.menuItem.Contains("Package Management"))
                {
                    menuItem = attr;
                    break;
                }
            }
            
            // Assert - Priority should be reasonable (e.g., between 0-100)
            Assert.IsNotNull(menuItem, "Should find MenuItem attribute");
            Assert.That(menuItem.priority, Is.InRange(0, 100), 
                "Menu priority should be between 0 and 100");
        }
        
        #endregion
        
        #region Functional Tests
        
        [Test]
        public void PackageManagement_MenuStructure_IsCorrect()
        {
            // This test verifies the actual implemented menu structure
            // Current design: Single menu item "NeonLadder/Package Management/Open Package Manager"
            
            var packageManagementType = System.Type.GetType("NeonLadder.Editor.PackageManagement, NeonLadder.Editor");
            
            if (packageManagementType == null)
            {
                Assert.Fail("PackageManagement class should exist for menu structure test");
                return;
            }
            
            // Get all static methods with MenuItem attributes
            var methods = packageManagementType.GetMethods(BindingFlags.Public | BindingFlags.Static);
            var menuItems = new System.Collections.Generic.List<string>();
            
            foreach (var method in methods)
            {
                var menuItemAttr = method.GetCustomAttribute<MenuItem>();
                if (menuItemAttr != null)
                {
                    menuItems.Add(menuItemAttr.menuItem);
                }
            }
            
            // Assert we have exactly one menu item (the current implementation)
            Assert.AreEqual(1, menuItems.Count, "Should have exactly one menu item: Open Package Manager");
            Assert.That(menuItems[0], Does.StartWith("NeonLadder/Package Management"),
                "Menu item should be under NeonLadder/Package Management");
            Assert.That(menuItems[0], Does.Contain("Open Package Manager"),
                "Menu item should be 'Open Package Manager'");
        }
        
        [Test]
        public void PackageManagement_WindowCanBeOpened()
        {
            // This test verifies that the Package Manager window can be opened
            // Current design uses a single window instead of nested menu items
            
            // Act - Open the Package Manager window
            PackageManagerWindow.ShowWindow();
            
            // Assert - Verify window was created
            var window = EditorWindow.GetWindow<PackageManagerWindow>(false);
            Assert.IsNotNull(window, "Package Manager window should be created");
            // Note: Unity EditorWindow titles default to class name, our ShowWindow() sets the title
            Assert.IsTrue(window.titleContent.text.Contains("Package") || 
                         window.titleContent.text.Contains("PackageManagerWindow"), 
                         $"Window title should relate to package management, got: {window.titleContent.text}");
            
            // Clean up
            if (window != null)
            {
                window.Close();
            }
        }
        
        [Test]
        public void PackageManagement_DiscoverPackages_IncludesDialogSystem()
        {
            // Test that the PackageManagementDynamic discovers Dialog System For Unity
            // Current design: All operations are handled through the PackageManagerWindow
            
            var discoveredPackages = PackageManagementDynamic.GetDiscoveredPackages();
            
            Assert.IsNotNull(discoveredPackages, "Should return package list");
            Assert.IsTrue(discoveredPackages.Count > 0, "Should discover some packages");
            Assert.That(discoveredPackages, Has.Member("Dialog System For Unity"),
                "Should discover 'Dialog System For Unity' package in Assets/Packages/");
        }
        
        #endregion
    }
}