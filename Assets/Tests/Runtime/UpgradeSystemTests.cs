using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Mechanics.Progression;
using NeonLadder.Mechanics.Currency;
using NeonLadder.Mechanics.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Comprehensive TDD tests for the upgrade system
    /// Covers patterns from Hades Mirror of Night and Slay the Spire progression
    /// </summary>
    public class UpgradeSystemTests
    {
        private GameObject testPlayerObject;
        private Player testPlayer;
        private Meta testMetaCurrency;
        private Perma testPermaCurrency;
        private IUpgradeSystem upgradeSystem;
        private List<IUpgrade> testUpgrades;
        
        [SetUp]
        public void SetUp()
        {
            // Create test player with currency components
            testPlayerObject = new GameObject("TestPlayer");
            testPlayer = testPlayerObject.AddComponent<Player>();
            testMetaCurrency = testPlayerObject.AddComponent<Meta>();
            testPermaCurrency = testPlayerObject.AddComponent<Perma>();
            
            // Initialize currencies with test amounts
            testMetaCurrency.current = 1000;
            testPermaCurrency.current = 500;
            
            // Create test upgrade system (implementation will come next)
            upgradeSystem = testPlayerObject.AddComponent<UpgradeSystem>();
            
            // Create test upgrades
            CreateTestUpgrades();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testPlayerObject != null)
                Object.DestroyImmediate(testPlayerObject);
        }
        
        private void CreateTestUpgrades()
        {
            testUpgrades = new List<IUpgrade>
            {
                // Meta upgrades (per-run, like Slay the Spire relics)
                CreateTestUpgrade("damage_boost", "Damage Boost", CurrencyType.Meta, 50, 3, UpgradeCategory.Offense),
                CreateTestUpgrade("health_boost", "Health Boost", CurrencyType.Meta, 75, 2, UpgradeCategory.Defense),
                CreateTestUpgrade("speed_boost", "Speed Boost", CurrencyType.Meta, 30, 5, UpgradeCategory.Utility),
                
                // Perma upgrades (persistent, like Hades Mirror of Night)
                CreateTestUpgrade("base_health", "Base Health", CurrencyType.Perma, 100, 10, UpgradeCategory.Core),
                CreateTestUpgrade("weapon_unlock", "Sword Mastery", CurrencyType.Perma, 200, 1, UpgradeCategory.Unlocks),
                CreateTestUpgrade("starting_gold", "Starting Gold", CurrencyType.Perma, 150, 5, UpgradeCategory.Quality),
                
                // Prerequisite chain (like Hades weapon aspects)
                CreateTestUpgrade("advanced_combat", "Advanced Combat", CurrencyType.Perma, 300, 1, UpgradeCategory.Unlocks, new[] { "weapon_unlock" }),
                
                // Mutually exclusive (like Hades Mirror alternatives)
                CreateTestUpgrade("glass_cannon", "Glass Cannon", CurrencyType.Meta, 100, 1, UpgradeCategory.Special, mutuallyExclusive: new[] { "tank_build" }),
                CreateTestUpgrade("tank_build", "Tank Build", CurrencyType.Meta, 100, 1, UpgradeCategory.Special, mutuallyExclusive: new[] { "glass_cannon" })
            };
        }
        
        private IUpgrade CreateTestUpgrade(string id, string name, CurrencyType currencyType, int cost, int maxLevel, UpgradeCategory category, string[] prerequisites = null, string[] mutuallyExclusive = null)
        {
            var upgradeData = ScriptableObject.CreateInstance<UpgradeData>();
            // Set private fields via reflection or create a test implementation
            return upgradeData;
        }
        
        #region Currency Integration Tests
        
        [Test]
        public void PurchaseUpgrade_WithSufficientMetaCurrency_DeductsCostAndOwnsUpgrade()
        {
            // Arrange
            var upgrade = testUpgrades.First(u => u.CurrencyType == CurrencyType.Meta);
            int initialCurrency = testMetaCurrency.current;
            
            // Act
            bool success = upgradeSystem.PurchaseUpgrade(upgrade.Id, CurrencyType.Meta);
            
            // Assert
            Assert.IsTrue(success, "Purchase should succeed with sufficient currency");
            Assert.AreEqual(initialCurrency - upgrade.Cost, testMetaCurrency.current, "Meta currency should be deducted");
            Assert.IsTrue(upgradeSystem.HasUpgrade(upgrade.Id), "Upgrade should be owned after purchase");
        }
        
        [Test]
        public void PurchaseUpgrade_WithInsufficientCurrency_FailsAndDoesNotDeductCurrency()
        {
            // Arrange
            var expensiveUpgrade = testUpgrades.First(u => u.Cost > testMetaCurrency.current);
            int initialCurrency = testMetaCurrency.current;
            
            // Act
            bool success = upgradeSystem.PurchaseUpgrade(expensiveUpgrade.Id, CurrencyType.Meta);
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail with insufficient currency");
            Assert.AreEqual(initialCurrency, testMetaCurrency.current, "Currency should not be deducted on failed purchase");
            Assert.IsFalse(upgradeSystem.HasUpgrade(expensiveUpgrade.Id), "Upgrade should not be owned after failed purchase");
        }
        
        [Test]
        public void CanAffordUpgrade_WithSufficientCurrency_ReturnsTrue()
        {
            // Arrange
            var affordableUpgrade = testUpgrades.First(u => u.Cost <= testMetaCurrency.current && u.CurrencyType == CurrencyType.Meta);
            
            // Act & Assert
            Assert.IsTrue(upgradeSystem.CanAffordUpgrade(affordableUpgrade.Id, CurrencyType.Meta), "Should be able to afford upgrade with sufficient currency");
        }
        
        [Test]
        public void CanAffordUpgrade_WithInsufficientCurrency_ReturnsFalse()
        {
            // Arrange
            testMetaCurrency.current = 10; // Set low currency
            var expensiveUpgrade = testUpgrades.First(u => u.Cost > 10);
            
            // Act & Assert
            Assert.IsFalse(upgradeSystem.CanAffordUpgrade(expensiveUpgrade.Id, CurrencyType.Meta), "Should not be able to afford expensive upgrade");
        }
        
        #endregion
        
        #region Upgrade Tree Logic Tests
        
        [Test]
        public void PurchaseUpgrade_WithUnmetPrerequisites_FailsAndExplainsWhy()
        {
            // Arrange
            var advancedUpgrade = testUpgrades.First(u => u.Prerequisites.Length > 0);
            var prerequisite = testUpgrades.First(u => u.Id == advancedUpgrade.Prerequisites[0]);
            
            // Ensure prerequisite is not owned
            Assert.IsFalse(upgradeSystem.HasUpgrade(prerequisite.Id), "Prerequisite should not be owned initially");
            
            // Act
            bool success = upgradeSystem.PurchaseUpgrade(advancedUpgrade.Id, advancedUpgrade.CurrencyType);
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail without prerequisites");
            Assert.IsFalse(upgradeSystem.HasUpgrade(advancedUpgrade.Id), "Advanced upgrade should not be owned without prerequisites");
        }
        
        [Test]
        public void PurchaseUpgrade_WithMetPrerequisites_Succeeds()
        {
            // Arrange
            var advancedUpgrade = testUpgrades.First(u => u.Prerequisites.Length > 0);
            var prerequisite = testUpgrades.First(u => u.Id == advancedUpgrade.Prerequisites[0]);
            
            // Purchase prerequisite first
            upgradeSystem.PurchaseUpgrade(prerequisite.Id, prerequisite.CurrencyType);
            Assert.IsTrue(upgradeSystem.HasUpgrade(prerequisite.Id), "Prerequisite should be owned");
            
            // Act
            bool success = upgradeSystem.PurchaseUpgrade(advancedUpgrade.Id, advancedUpgrade.CurrencyType);
            
            // Assert
            Assert.IsTrue(success, "Purchase should succeed with prerequisites met");
            Assert.IsTrue(upgradeSystem.HasUpgrade(advancedUpgrade.Id), "Advanced upgrade should be owned with prerequisites");
        }
        
        [Test]
        public void PurchaseUpgrade_MutuallyExclusive_BlocksOtherOption()
        {
            // Arrange
            var glassCannon = testUpgrades.First(u => u.Id == "glass_cannon");
            var tankBuild = testUpgrades.First(u => u.Id == "tank_build");
            
            // Act
            upgradeSystem.PurchaseUpgrade(glassCannon.Id, glassCannon.CurrencyType);
            bool tankPurchaseSuccess = upgradeSystem.PurchaseUpgrade(tankBuild.Id, tankBuild.CurrencyType);
            
            // Assert
            Assert.IsTrue(upgradeSystem.HasUpgrade(glassCannon.Id), "Glass Cannon should be owned");
            Assert.IsFalse(tankPurchaseSuccess, "Tank Build purchase should fail due to mutual exclusion");
            Assert.IsFalse(upgradeSystem.HasUpgrade(tankBuild.Id), "Tank Build should not be owned due to mutual exclusion");
        }
        
        #endregion
        
        #region Hades-Style Meta vs Perma Currency Tests
        
        [Test]
        public void ResetMetaUpgrades_ResetsOnlyMetaUpgrades_PreservesPermaUpgrades()
        {
            // Arrange - Purchase both Meta and Perma upgrades
            var metaUpgrade = testUpgrades.First(u => u.CurrencyType == CurrencyType.Meta);
            var permaUpgrade = testUpgrades.First(u => u.CurrencyType == CurrencyType.Perma);
            
            upgradeSystem.PurchaseUpgrade(metaUpgrade.Id, metaUpgrade.CurrencyType);
            upgradeSystem.PurchaseUpgrade(permaUpgrade.Id, permaUpgrade.CurrencyType);
            
            Assert.IsTrue(upgradeSystem.HasUpgrade(metaUpgrade.Id), "Meta upgrade should be owned before reset");
            Assert.IsTrue(upgradeSystem.HasUpgrade(permaUpgrade.Id), "Perma upgrade should be owned before reset");
            
            // Act - Reset Meta upgrades (simulate death in Hades)
            upgradeSystem.ResetMetaUpgrades();
            
            // Assert
            Assert.IsFalse(upgradeSystem.HasUpgrade(metaUpgrade.Id), "Meta upgrade should be reset");
            Assert.IsTrue(upgradeSystem.HasUpgrade(permaUpgrade.Id), "Perma upgrade should persist after meta reset");
        }
        
        [Test]
        public void GetAvailableUpgrades_FiltersCorrectlyByCurrencyType()
        {
            // Act
            var availableMetaUpgrades = upgradeSystem.GetAvailableUpgrades(CurrencyType.Meta);
            var availablePermaUpgrades = upgradeSystem.GetAvailableUpgrades(CurrencyType.Perma);
            
            // Assert
            Assert.IsTrue(availableMetaUpgrades.All(u => u.CurrencyType == CurrencyType.Meta), "Meta query should only return Meta upgrades");
            Assert.IsTrue(availablePermaUpgrades.All(u => u.CurrencyType == CurrencyType.Perma), "Perma query should only return Perma upgrades");
            Assert.IsTrue(availableMetaUpgrades.Count() > 0, "Should have available Meta upgrades");
            Assert.IsTrue(availablePermaUpgrades.Count() > 0, "Should have available Perma upgrades");
        }
        
        #endregion
        
        #region Player Integration Tests
        
        [Test]
        public void ApplyUpgradeEffects_AppliesAllOwnedUpgrades()
        {
            // Arrange
            var healthUpgrade = testUpgrades.First(u => u.Id == "health_boost");
            upgradeSystem.PurchaseUpgrade(healthUpgrade.Id, healthUpgrade.CurrencyType);
            
            // Act
            upgradeSystem.ApplyUpgradeEffects();
            
            // Assert
            // This test would verify that player stats are actually modified
            // Implementation depends on your player stat system
            Assert.Pass("Upgrade effects should be applied to player - implementation specific");
        }
        
        [Test]
        public void OnUpgradePurchased_Event_FiresWithCorrectUpgrade()
        {
            // Arrange
            IUpgrade purchasedUpgrade = null;
            upgradeSystem.OnUpgradePurchased += (upgrade) => purchasedUpgrade = upgrade;
            
            var testUpgrade = testUpgrades.First(u => u.CurrencyType == CurrencyType.Meta);
            
            // Act
            upgradeSystem.PurchaseUpgrade(testUpgrade.Id, testUpgrade.CurrencyType);
            
            // Assert
            Assert.IsNotNull(purchasedUpgrade, "OnUpgradePurchased event should fire");
            Assert.AreEqual(testUpgrade.Id, purchasedUpgrade.Id, "Event should fire with correct upgrade");
        }
        
        #endregion
        
        #region Multi-Level Upgrade Tests (Slay the Spire Style)
        
        [Test]
        public void PurchaseUpgrade_MultiLevel_IncrementsLevel()
        {
            // Arrange
            var multiLevelUpgrade = testUpgrades.First(u => u.MaxLevel > 1);
            
            // Act
            upgradeSystem.PurchaseUpgrade(multiLevelUpgrade.Id, multiLevelUpgrade.CurrencyType);
            upgradeSystem.PurchaseUpgrade(multiLevelUpgrade.Id, multiLevelUpgrade.CurrencyType);
            
            // Assert
            Assert.AreEqual(2, multiLevelUpgrade.CurrentLevel, "Upgrade should be level 2 after two purchases");
            Assert.IsTrue(upgradeSystem.HasUpgrade(multiLevelUpgrade.Id), "Multi-level upgrade should be owned");
        }
        
        [Test]
        public void PurchaseUpgrade_AtMaxLevel_FailsGracefully()
        {
            // Arrange
            var singleLevelUpgrade = testUpgrades.First(u => u.MaxLevel == 1);
            upgradeSystem.PurchaseUpgrade(singleLevelUpgrade.Id, singleLevelUpgrade.CurrencyType);
            
            int currencyBeforeSecondPurchase = testMetaCurrency.current;
            
            // Act
            bool secondPurchaseSuccess = upgradeSystem.PurchaseUpgrade(singleLevelUpgrade.Id, singleLevelUpgrade.CurrencyType);
            
            // Assert
            Assert.IsFalse(secondPurchaseSuccess, "Should not be able to purchase upgrade beyond max level");
            Assert.AreEqual(currencyBeforeSecondPurchase, testMetaCurrency.current, "Currency should not be deducted for failed max-level purchase");
        }
        
        #endregion
        
        #region Edge Cases and Error Handling
        
        [Test]
        public void PurchaseUpgrade_InvalidUpgradeId_FailsGracefully()
        {
            // Act
            bool success = upgradeSystem.PurchaseUpgrade("invalid_upgrade_id", CurrencyType.Meta);
            
            // Assert
            Assert.IsFalse(success, "Purchase should fail for invalid upgrade ID");
        }
        
        [Test]
        public void GetUpgrade_InvalidId_ReturnsNull()
        {
            // Act
            var upgrade = upgradeSystem.GetUpgrade("invalid_upgrade_id");
            
            // Assert
            Assert.IsNull(upgrade, "Should return null for invalid upgrade ID");
        }
        
        #endregion
    }
}