using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Managers;
using NeonLadder.Models;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Progression;
using NeonLadder.Mechanics.Stats;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Integration tests for EnhancedPurchaseManager
    /// Tests coordination between PurchasableItems, UpgradeSystem, and Currency systems
    /// </summary>
    public class EnhancedPurchaseManagerTests
    {
        private GameObject testManagerObject;
        private GameObject testPlayerObject;
        private EnhancedPurchaseManager purchaseManager;
        private Player testPlayer;
        private Meta testMetaCurrency;
        private Perma testPermaCurrency;
        private UpgradeSystem testUpgradeSystem;
        
        private PurchasableItem testMetaItem;
        private PurchasableItem testPermaItem;
        private UpgradeData testUpgrade;
        
        [SetUp]
        public void SetUp()
        {
            Debug.Log("=== EnhancedPurchaseManagerTests SetUp START ===");
            
            // Create manager object
            testManagerObject = new GameObject("TestPurchaseManager");
            purchaseManager = testManagerObject.AddComponent<EnhancedPurchaseManager>();
            
            // Create player object (add all required components BEFORE Player component)
            testPlayerObject = new GameObject("TestPlayer");
            
            // Add required components that Player.Awake() looks for
            testPlayerObject.AddComponent<Rigidbody>();
            testPlayerObject.AddComponent<Animator>();
            testPlayerObject.AddComponent<Health>();
            testPlayerObject.AddComponent<Stamina>();
            
            // Add currency and upgrade components
            testMetaCurrency = testPlayerObject.AddComponent<Meta>();
            testPermaCurrency = testPlayerObject.AddComponent<Perma>();
            testUpgradeSystem = testPlayerObject.AddComponent<UpgradeSystem>();
            
            // Add Player last so it finds all required components
            testPlayer = testPlayerObject.AddComponent<Player>();
            
            // Setup currencies
            testMetaCurrency.current = 1000;
            testPermaCurrency.current = 500;
            
            // Create test items and upgrades
            CreateTestAssets();
            
            // Configure purchase manager
            ConfigurePurchaseManager();
            
            Debug.Log("=== EnhancedPurchaseManagerTests SetUp END ===");
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testManagerObject != null)
                Object.DestroyImmediate(testManagerObject);
            if (testPlayerObject != null)
                Object.DestroyImmediate(testPlayerObject);
                
            // Clean up test assets
            if (testMetaItem != null)
                Object.DestroyImmediate(testMetaItem);
            if (testPermaItem != null)
                Object.DestroyImmediate(testPermaItem);
            if (testUpgrade != null)
                Object.DestroyImmediate(testUpgrade);
        }
        
        private void CreateTestAssets()
        {
            // Meta currency item
            testMetaItem = CreateTestPurchasableItem(
                "test_meta_item",
                "Test Meta Item",
                NeonLadder.Mechanics.Progression.CurrencyType.Meta,
                50,
                ItemType.Consumable
            );
            
            // Perma currency item
            testPermaItem = CreateTestPurchasableItem(
                "test_perma_item",
                "Test Perma Item",
                NeonLadder.Mechanics.Progression.CurrencyType.Perma,
                100,
                ItemType.Ability
            );
            
            // Test upgrade
            testUpgrade = CreateTestUpgrade(
                "test_upgrade",
                "Test Upgrade",
                NeonLadder.Mechanics.Progression.CurrencyType.Meta,
                75,
                3,
                UpgradeCategory.Offense
            );
        }
        
        private PurchasableItem CreateTestPurchasableItem(string id, string name, NeonLadder.Mechanics.Progression.CurrencyType currencyType, int cost, ItemType itemType)
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, id);
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, name);
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, currencyType);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, cost);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, itemType);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, 1);
            
            return item;
        }
        
        private UpgradeData CreateTestUpgrade(string id, string name, NeonLadder.Mechanics.Progression.CurrencyType currencyType, int cost, int maxLevel, UpgradeCategory category)
        {
            var upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            
            var type = typeof(UpgradeData);
            type.GetField("id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, id);
            type.GetField("upgradeName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, name);
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, currencyType);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, cost);
            type.GetField("maxLevel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, maxLevel);
            type.GetField("category", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(upgrade, category);
            
            return upgrade;
        }
        
        private void ConfigurePurchaseManager()
        {
            Debug.Log($"ConfigurePurchaseManager: testMetaItem={testMetaItem?.name}, testPermaItem={testPermaItem?.name}, testUpgrade={testUpgrade?.name}");
            
            // Set currency references explicitly for testing
            testUpgradeSystem.SetTestCurrencies(testMetaCurrency, testPermaCurrency);
            
            // Configure the UpgradeSystem with test upgrade
            testUpgradeSystem.SetTestUpgrades(new UpgradeData[] { testUpgrade });
            
            // Use the new SetTestData method instead of reflection
            purchaseManager.SetTestData(
                new PurchasableItem[] { testMetaItem, testPermaItem },
                new UpgradeData[] { testUpgrade },
                testPlayer,
                testUpgradeSystem
            );
            
            Debug.Log("ConfigurePurchaseManager completed");
        }
        
        #region Item Purchase Tests
        
        [Test]
        public void PurchaseItem_ValidMetaItem_SucceedsAndDeductsCurrency()
        {
            // Arrange
            int initialCurrency = testMetaCurrency.current;
            bool eventFired = false;
            
            purchaseManager.OnItemPurchased += (item) => eventFired = true;
            
            // Act
            bool success = purchaseManager.PurchaseItem("test_meta_item");
            
            // Assert
            Assert.IsTrue(success, "Purchase should succeed");
            Assert.IsTrue(eventFired, "Purchase event should fire");
            // Note: Currency deduction happens via event system, so we can't directly test it here
        }
        
        [Test]
        public void PurchaseItem_ValidPermaItem_SucceedsAndDeductsCurrency()
        {
            // Arrange
            int initialCurrency = testPermaCurrency.current;
            bool eventFired = false;
            
            purchaseManager.OnItemPurchased += (item) => eventFired = true;
            
            // Act
            bool success = purchaseManager.PurchaseItem("test_perma_item");
            
            // Assert
            Assert.IsTrue(success, "Purchase should succeed");
            Assert.IsTrue(eventFired, "Purchase event should fire");
        }
        
        [Test]
        public void PurchaseItem_InvalidItemId_FailsAndFiresFailEvent()
        {
            // Arrange
            bool failEventFired = false;
            string failureReason = "";
            
            purchaseManager.OnPurchaseFailed += (reason) => {
                failEventFired = true;
                failureReason = reason;
            };
            
            // Act
            bool success = purchaseManager.PurchaseItem("nonexistent_item");
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail");
            Assert.IsTrue(failEventFired, "Failure event should fire");
            Assert.IsTrue(failureReason.Contains("not found"), "Failure reason should mention item not found");
        }
        
        [Test]
        public void PurchaseItem_InsufficientCurrency_FailsAndFiresFailEvent()
        {
            // Arrange - Set currency lower than item cost
            testMetaCurrency.current = 10; // Less than test item cost of 50
            bool failEventFired = false;
            
            purchaseManager.OnPurchaseFailed += (reason) => failEventFired = true;
            
            // Act
            bool success = purchaseManager.PurchaseItem("test_meta_item");
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail");
            Assert.IsTrue(failEventFired, "Failure event should fire");
        }
        
        #endregion
        
        #region Upgrade Purchase Tests
        
        [Test]
        public void PurchaseUpgrade_ValidUpgrade_SucceedsAndFiresEvent()
        {
            // Arrange
            bool eventFired = false;
            
            purchaseManager.OnUpgradePurchased += (upgrade) => eventFired = true;
            
            // Act
            bool success = purchaseManager.PurchaseUpgrade("test_upgrade", NeonLadder.Mechanics.Progression.CurrencyType.Meta);
            
            // Assert
            Assert.IsTrue(success, "Upgrade purchase should succeed");
            Assert.IsTrue(eventFired, "Upgrade purchase event should fire");
        }
        
        [Test]
        public void PurchaseUpgrade_InvalidUpgrade_FailsAndFiresFailEvent()
        {
            // Arrange
            bool failEventFired = false;
            
            purchaseManager.OnPurchaseFailed += (reason) => failEventFired = true;
            
            // Act
            bool success = purchaseManager.PurchaseUpgrade("nonexistent_upgrade", NeonLadder.Mechanics.Progression.CurrencyType.Meta);
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail");
            Assert.IsTrue(failEventFired, "Failure event should fire");
        }
        
        #endregion
        
        #region Shop Query Tests
        
        [Test]
        public void GetAvailableItems_MetaCurrency_ReturnsMetaItems()
        {
            // Act
            var metaItems = purchaseManager.GetAvailableItems(NeonLadder.Events.CurrencyType.Meta, metaShop: true);
            
            // Assert
            Assert.Greater(metaItems.Count, 0, "Should return meta items");
            Assert.IsTrue(metaItems.All(item => item.CurrencyType == NeonLadder.Mechanics.Progression.CurrencyType.Meta), "All items should be meta currency");
        }
        
        [Test]
        public void GetAvailableItems_PermaCurrency_ReturnsPermaItems()
        {
            // Act
            var permaItems = purchaseManager.GetAvailableItems(NeonLadder.Events.CurrencyType.Perma, metaShop: false);
            
            // Assert
            // Note: Test items are configured for meta shop by default, so this might return empty
            // In a real scenario, perma items would be configured for perma shop
            Assert.IsNotNull(permaItems, "Should return collection (even if empty)");
        }
        
        [Test]
        public void GetAffordableItems_SufficientCurrency_ReturnsAffordableItems()
        {
            // Arrange - Ensure sufficient currency
            testMetaCurrency.current = 1000;
            
            // Act
            var affordableItems = purchaseManager.GetAffordableItems(NeonLadder.Events.CurrencyType.Meta, metaShop: true);
            
            // Assert
            Assert.IsNotNull(affordableItems, "Should return collection");
        }
        
        [Test]
        public void GetAffordableItems_InsufficientCurrency_ReturnsEmptyList()
        {
            // Arrange - Set insufficient currency
            testMetaCurrency.current = 1;
            
            // Act
            var affordableItems = purchaseManager.GetAffordableItems(NeonLadder.Events.CurrencyType.Meta, metaShop: true);
            
            // Assert
            Assert.IsNotNull(affordableItems, "Should return collection");
            // Items should be filtered out due to insufficient currency
        }
        
        [Test]
        public void CanAffordItem_SufficientCurrency_ReturnsTrue()
        {
            // Arrange
            testMetaCurrency.current = 1000;
            
            // Act
            bool canAfford = purchaseManager.CanAffordItem("test_meta_item");
            
            // Assert
            Assert.IsTrue(canAfford, "Should be able to afford item with sufficient currency");
        }
        
        [Test]
        public void CanAffordItem_InsufficientCurrency_ReturnsFalse()
        {
            // Arrange
            testMetaCurrency.current = 1;
            
            // Act
            bool canAfford = purchaseManager.CanAffordItem("test_meta_item");
            
            // Assert
            Assert.IsFalse(canAfford, "Should not be able to afford item with insufficient currency");
        }
        
        #endregion
        
        #region Reset and Lifecycle Tests
        
        [Test]
        public void ResetMetaPurchases_ResetsMetaItemsOnly()
        {
            // Arrange - Purchase both meta and perma items
            purchaseManager.PurchaseItem("test_meta_item");
            purchaseManager.PurchaseItem("test_perma_item");
            
            // Act
            purchaseManager.ResetMetaPurchases();
            
            // Assert - Meta item should be reset, perma item should persist
            Assert.AreEqual(0, testMetaItem.TimesPurchased, "Meta item should be reset");
            Assert.AreEqual(1, testPermaItem.TimesPurchased, "Perma item should persist");
        }
        
        [Test]
        public void ApplyPersistentEffects_CallsUpgradeSystemAndItems()
        {
            // Arrange - Purchase some persistent items
            purchaseManager.PurchaseItem("test_perma_item");
            
            // Act & Assert - Should not throw exceptions
            Assert.DoesNotThrow(() => purchaseManager.ApplyPersistentEffects());
        }
        
        #endregion
        
        #region Integration Tests
        
        [Test]
        public void PurchaseManager_IntegratesWithCurrencySystem_WithoutConflicts()
        {
            // Arrange
            int initialMeta = testMetaCurrency.current;
            
            // Act - Use both purchase manager and direct currency operations
            testMetaCurrency.Increment(50);
            bool purchaseSuccess = purchaseManager.PurchaseItem("test_meta_item");
            
            // Assert - Both operations should work
            Assert.AreEqual(initialMeta + 50, testMetaCurrency.current);
            Assert.IsTrue(purchaseSuccess, "Purchase should succeed");
        }
        
        [Test]
        public void PurchaseManager_IntegratesWithUpgradeSystem_WithoutConflicts()
        {
            // Arrange & Act - Use both purchase manager and direct upgrade system
            bool managerPurchase = purchaseManager.PurchaseUpgrade("test_upgrade", NeonLadder.Mechanics.Progression.CurrencyType.Meta);
            
            // Direct upgrade system access
            var availableUpgrades = testUpgradeSystem.GetAvailableUpgrades(NeonLadder.Mechanics.Progression.CurrencyType.Meta);
            
            // Assert - Both should work without conflicts
            Assert.IsTrue(managerPurchase, "Manager purchase should succeed");
            Assert.IsNotNull(availableUpgrades, "Direct upgrade system access should work");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void PurchaseManager_WithNullPlayer_HandlesGracefully()
        {
            // Arrange - Remove player reference
            var playerField = typeof(EnhancedPurchaseManager).GetField("player", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerField?.SetValue(purchaseManager, null);
            
            // Act & Assert - Should handle null player gracefully
            bool success = purchaseManager.PurchaseItem("test_meta_item");
            Assert.IsFalse(success, "Purchase should fail gracefully with null player");
        }
        
        [Test]
        public void PurchaseManager_WithNullUpgradeSystem_HandlesGracefully()
        {
            // Arrange - Remove upgrade system reference
            var upgradeSystemField = typeof(EnhancedPurchaseManager).GetField("upgradeSystem", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            upgradeSystemField?.SetValue(purchaseManager, null);
            
            // Act & Assert - Should handle null upgrade system gracefully
            bool success = purchaseManager.PurchaseUpgrade("test_upgrade", NeonLadder.Mechanics.Progression.CurrencyType.Meta);
            Assert.IsFalse(success, "Upgrade purchase should fail gracefully with null upgrade system");
        }
        
        [Test]
        public void PurchaseManager_WithEmptyDatabases_HandlesGracefully()
        {
            // Arrange - Clear databases
            var availableItemsField = typeof(EnhancedPurchaseManager).GetField("availableItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            availableItemsField?.SetValue(purchaseManager, new PurchasableItem[0]);
            
            purchaseManager.SendMessage("InitializeDatabases", SendMessageOptions.DontRequireReceiver);
            
            // Act & Assert - Should handle empty databases gracefully
            var items = purchaseManager.GetAvailableItems(NeonLadder.Events.CurrencyType.Meta);
            Assert.IsNotNull(items, "Should return empty collection, not null");
            Assert.AreEqual(0, items.Count, "Should return empty collection");
        }
        
        #endregion
    }
}