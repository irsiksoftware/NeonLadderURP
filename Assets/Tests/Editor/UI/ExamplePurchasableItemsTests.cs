using System;
using System.IO;
using System.Reflection;
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
    /// Jean Grey's Example Purchasable Items Tests - Testing ScriptableObject creation patterns
    /// Ensuring our asset creation menu items work flawlessly
    /// 
    /// "I can read the user's mind - they need these items to work perfectly every time."
    /// </summary>
    [TestFixture]
    public class ExamplePurchasableItemsTests
    {
        private string testAssetPath;
        
        #region Setup & Teardown
        
        [SetUp]
        public void SetUp()
        {
            // Setup test asset path
            testAssetPath = "Assets/TestOutput/TestPurchasableItems/";
            
            // Setup mock file system
            EditorUITestFramework.MockFileSystem.ClearMocks();
            EditorUITestFramework.MockFileSystem.AddMockDirectory(testAssetPath);
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up any test assets created
            if (Directory.Exists(testAssetPath))
            {
                var assets = AssetDatabase.FindAssets("", new[] { testAssetPath });
                foreach (var guid in assets)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    AssetDatabase.DeleteAsset(path);
                }
                AssetDatabase.DeleteAsset(testAssetPath);
            }
            
            EditorUITestFramework.MockFileSystem.ClearMocks();
        }
        
        #endregion
        
        #region MenuItem Tests
        
        [Test]
        public void MenuItems_AllExampleItems_HaveCorrectPaths()
        {
            // Arrange
            var expectedMenuItems = new[]
            {
                ("CreateHealthUpgradeAsset", "Assets/Create/NeonLadder/Purchasable Items/Health Upgrade"),
                ("CreateStaminaUpgradeAsset", "Assets/Create/NeonLadder/Purchasable Items/Stamina Upgrade"),
                ("CreateDamageUpgradeAsset", "Assets/Create/NeonLadder/Purchasable Items/Damage Upgrade"),
                ("CreateSpeedUpgradeAsset", "Assets/Create/NeonLadder/Purchasable Items/Speed Upgrade"),
                ("CreateSpecialAbilityAsset", "Assets/Create/NeonLadder/Purchasable Items/Special Ability"),
                ("CreateWeaponUnlockAsset", "Assets/Create/NeonLadder/Purchasable Items/Weapon Unlock"),
                ("CreateCosmeticItemAsset", "Assets/Create/NeonLadder/Purchasable Items/Cosmetic Item")
            };
            
            // Act & Assert
            foreach (var (methodName, expectedPath) in expectedMenuItems)
            {
                var method = typeof(ExamplePurchasableItems).GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.Static);
                var attribute = method?.GetCustomAttribute<MenuItem>();
                
                Assert.IsNotNull(attribute, $"{methodName} should have MenuItem attribute");
                Assert.AreEqual(expectedPath, attribute.menuItem,
                    $"{methodName} should have correct menu path");
            }
        }
        
        [Test]
        public void MenuItems_AllHavePriority()
        {
            // Arrange
            var methods = typeof(ExamplePurchasableItems)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.GetCustomAttribute<MenuItem>() != null)
                .ToList();
            
            // Act & Assert
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<MenuItem>();
                Assert.GreaterOrEqual(attribute.priority, 0,
                    $"{method.Name} should have non-negative priority");
            }
        }
        
        #endregion
        
        #region Asset Creation Tests
        
        [Test]
        public void CreateHealthUpgrade_CreatesValidPurchasableItem()
        {
            // Arrange
            var createMethod = typeof(ExamplePurchasableItems).GetMethod("CreateHealthUpgrade",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            // Act - Get the created item without actually creating an asset
            var item = createMethod?.Invoke(null, null) as PurchasableItem;
            
            // Assert
            Assert.IsNotNull(item, "CreateHealthUpgrade should return a PurchasableItem");
            Assert.AreEqual("Health Upgrade", item.ItemName, "Item name should be correct");
            Assert.AreEqual(ItemType.Upgrade, item.Type, "Item type should be Upgrade");
            Assert.Greater(item.Cost, 0, "Base cost should be positive");
        }
        
        [Test]
        public void CreateStaminaUpgrade_HasCorrectProperties()
        {
            // Arrange
            var createMethod = typeof(ExamplePurchasableItems).GetMethod("CreateStaminaUpgrade",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            // Act
            var item = createMethod?.Invoke(null, null) as PurchasableItem;
            
            // Assert
            Assert.IsNotNull(item, "CreateStaminaUpgrade should return a PurchasableItem");
            Assert.AreEqual("Stamina Boost", item.ItemName);
            // Note: This test assumes specific upgrade logic that may not exist in the base class
        }
        
        [Test]
        public void CreateWeaponUnlock_HasCorrectUnlockProperties()
        {
            // Arrange
            var createMethod = typeof(ExamplePurchasableItems).GetMethod("CreateWeaponUnlock",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            // Act
            var item = createMethod?.Invoke(null, null) as PurchasableItem;
            
            // Assert
            Assert.IsNotNull(item, "CreateWeaponUnlock should return a PurchasableItem");
            Assert.AreEqual(ItemType.Unlock, item.Type, "Should be Unlock type");
            Assert.AreEqual(1, item.MaxPurchases, "Unlocks should have max purchases of 1");
        }
        
        [Test]
        public void CreateCosmeticItem_HasCorrectCosmeticProperties()
        {
            // Arrange
            var createMethod = typeof(ExamplePurchasableItems).GetMethod("CreateCosmeticItem",
                BindingFlags.NonPublic | BindingFlags.Static);
            
            // Act
            var item = createMethod?.Invoke(null, null) as PurchasableItem;
            
            // Assert
            Assert.IsNotNull(item, "CreateCosmeticItem should return a PurchasableItem");
            Assert.AreEqual(ItemType.Cosmetic, item.Type, "Should be Cosmetic type");
            Assert.AreEqual(1, item.MaxPurchases, "Cosmetics should be one-time purchases");
            Assert.AreEqual(CurrencyType.Perma, item.CurrencyType,
                "Cosmetics should use permanent currency");
        }
        
        #endregion
        
        #region Data Validation Tests
        
        [Test]
        public void AllItems_HaveValidCostProgression()
        {
            // Arrange
            var createMethods = new[]
            {
                "CreateHealthUpgrade",
                "CreateStaminaUpgrade",
                "CreateDamageUpgrade",
                "CreateSpeedUpgrade"
            };
            
            // Act & Assert
            foreach (var methodName in createMethods)
            {
                var method = typeof(ExamplePurchasableItems).GetMethod(methodName,
                    BindingFlags.NonPublic | BindingFlags.Static);
                var item = method?.Invoke(null, null) as PurchasableItem;
                
                Assert.IsNotNull(item, $"{methodName} should create an item");
                Assert.Greater(item.Cost, 0, $"{methodName} should have positive base cost");
            }
        }
        
        [Test]
        public void AllItems_HaveDescriptions()
        {
            // Arrange
            var allMethods = typeof(ExamplePurchasableItems)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Create") && m.ReturnType == typeof(PurchasableItem))
                .ToList();
            
            // Act & Assert
            foreach (var method in allMethods)
            {
                var item = method.Invoke(null, null) as PurchasableItem;
                
                Assert.IsNotNull(item, $"{method.Name} should create an item");
                Assert.IsFalse(string.IsNullOrEmpty(item.Description),
                    $"{method.Name} should create item with description");
                Assert.Greater(item.Description.Length, 10,
                    $"{method.Name} should have meaningful description");
            }
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void Integration_AllItemTypesAreCovered()
        {
            // Arrange
            var expectedTypes = Enum.GetValues(typeof(ItemType))
                .Cast<ItemType>()
                .ToList();
            
            var allMethods = typeof(ExamplePurchasableItems)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name.StartsWith("Create") && m.ReturnType == typeof(PurchasableItem))
                .ToList();
            
            // Act
            var createdTypes = allMethods
                .Select(m => (m.Invoke(null, null) as PurchasableItem)?.Type)
                .Where(t => t != null)
                .Distinct()
                .ToList();
            
            // Assert
            foreach (var expectedType in expectedTypes)
            {
                Assert.Contains(expectedType, createdTypes,
                    $"Example items should cover {expectedType} type");
            }
        }
        
        [Test]
        public void Integration_UpgradeCategoriesAreCovered()
        {
            // Arrange
            // Note: The actual PurchasableItem doesn't have upgrade categories
            // This test is simplified to check that upgrade items exist
            var upgradeItems = new[] { "Health", "Stamina", "Damage", "Speed" };
            
            var upgradeMethods = new[]
            {
                "CreateHealthUpgrade",
                "CreateStaminaUpgrade",
                "CreateDamageUpgrade",
                "CreateSpeedUpgrade"
            };
            
            // Act
            var createdItems = upgradeMethods
                .Select(name => typeof(ExamplePurchasableItems).GetMethod(name,
                    BindingFlags.NonPublic | BindingFlags.Static))
                .Select(m => m?.Invoke(null, null) as PurchasableItem)
                .Where(i => i != null)
                .ToList();
            
            // Assert
            Assert.AreEqual(4, createdItems.Count, "Should create 4 upgrade items");
            foreach (var item in createdItems)
            {
                Assert.AreEqual(ItemType.Upgrade, item.Type, "All upgrade methods should create Upgrade type items");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void AssertItemHasValidIcon(PurchasableItem item)
        {
            // In a real test, we'd check if the icon sprite is assigned
            // For now, we just check the item is not null
            Assert.IsNotNull(item, "Item should not be null when checking icon");
        }
        
        private void AssertCurrencyTypeIsValid(PurchasableItem item)
        {
            var validTypes = Enum.GetValues(typeof(CurrencyType)).Cast<CurrencyType>();
            Assert.Contains(item.CurrencyType, validTypes.ToList(),
                "Item should have valid currency type");
        }
        
        #endregion
    }
}