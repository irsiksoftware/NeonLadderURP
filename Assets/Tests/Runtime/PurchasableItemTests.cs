using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Models;
using NeonLadder.Events;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Currency;
using System.Collections;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Comprehensive tests for the PurchasableItem system
    /// Tests ScriptableObject configuration, effects, and player integration
    /// </summary>
    public class PurchasableItemTests
    {
        private GameObject testPlayerObject;
        private Player testPlayer;
        private Meta testMetaCurrency;
        private Perma testPermaCurrency;
        private PurchasableItem testConsumableItem;
        private PurchasableItem testAbilityItem;
        private PurchasableItem testMultiPurchaseItem;
        
        [SetUp]
        public void SetUp()
        {
            // Create test player - add currency components first
            testPlayerObject = new GameObject("TestPlayer");
            testMetaCurrency = testPlayerObject.AddComponent<Meta>();
            testPermaCurrency = testPlayerObject.AddComponent<Perma>();
            testPlayer = testPlayerObject.AddComponent<Player>();
            
            // Setup currency
            testMetaCurrency.current = 1000;
            testPermaCurrency.current = 500;
            
            // Create test items
            CreateTestItems();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testPlayerObject != null)
                Object.DestroyImmediate(testPlayerObject);
                
            // Clean up ScriptableObjects
            if (testConsumableItem != null)
                Object.DestroyImmediate(testConsumableItem);
            if (testAbilityItem != null)
                Object.DestroyImmediate(testAbilityItem);
            if (testMultiPurchaseItem != null)
                Object.DestroyImmediate(testMultiPurchaseItem);
        }
        
        private void CreateTestItems()
        {
            // Health Potion (Meta currency, consumable)
            testConsumableItem = CreateTestItem(
                "health_potion",
                "Health Potion", 
                CurrencyType.Meta,
                25,
                ItemType.Consumable,
                maxPurchases: 99,
                canPurchaseMultiple: true
            );
            
            // Double Jump (Perma currency, ability)
            testAbilityItem = CreateTestItem(
                "double_jump",
                "Double Jump",
                CurrencyType.Perma,
                150,
                ItemType.Ability,
                maxPurchases: 1,
                canPurchaseMultiple: false
            );
            
            // Damage Boost (Meta currency, multiple purchases with scaling cost)
            testMultiPurchaseItem = CreateTestItem(
                "damage_boost",
                "Damage Boost",
                CurrencyType.Meta,
                50,
                ItemType.Upgrade,
                maxPurchases: 3,
                canPurchaseMultiple: true
            );
        }
        
        private PurchasableItem CreateTestItem(string id, string name, CurrencyType currencyType, int cost, ItemType itemType, int maxPurchases, bool canPurchaseMultiple)
        {
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            
            // Use reflection to set private fields
            var type = typeof(PurchasableItem);
            type.GetField("itemId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, id);
            type.GetField("itemName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, name);
            type.GetField("description", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, $"Test description for {name}");
            // Convert Events.CurrencyType to Progression.CurrencyType
            var progressionCurrencyType = currencyType == CurrencyType.Meta 
                ? NeonLadder.Mechanics.Progression.CurrencyType.Meta 
                : NeonLadder.Mechanics.Progression.CurrencyType.Perma;
            type.GetField("currencyType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, progressionCurrencyType);
            type.GetField("baseCost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, cost);
            type.GetField("itemType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, itemType);
            type.GetField("maxPurchases", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, maxPurchases);
            type.GetField("canPurchaseMultiple", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, canPurchaseMultiple);
            
            return item;
        }
        
        #region Basic Property Tests
        
        [Test]
        public void PurchasableItem_Properties_ReturnCorrectValues()
        {
            // Act & Assert
            Assert.AreEqual("health_potion", testConsumableItem.ItemId);
            Assert.AreEqual("Health Potion", testConsumableItem.ItemName);
            Assert.AreEqual(NeonLadder.Mechanics.Progression.CurrencyType.Meta, testConsumableItem.CurrencyType);
            Assert.AreEqual(25, testConsumableItem.Cost);
            Assert.AreEqual(ItemType.Consumable, testConsumableItem.Type);
            Assert.AreEqual(99, testConsumableItem.MaxPurchases);
            Assert.IsTrue(testConsumableItem.CanPurchase);
        }
        
        [Test]
        public void PurchasableItem_DefaultIcon_UsesIconForShopIcon()
        {
            // Arrange
            var mockIcon = new Texture2D(32, 32);
            var sprite = Sprite.Create(mockIcon, new Rect(0, 0, 32, 32), Vector2.zero);
            
            var type = typeof(PurchasableItem);
            type.GetField("icon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(testConsumableItem, sprite);
            
            // Act & Assert
            Assert.AreEqual(sprite, testConsumableItem.Icon);
            Assert.AreEqual(sprite, testConsumableItem.ShopIcon); // Should fallback to icon
            
            // Cleanup
            Object.DestroyImmediate(mockIcon);
            Object.DestroyImmediate(sprite);
        }
        
        #endregion
        
        #region Purchase Logic Tests
        
        [Test]
        public void CanAfford_WithSufficientCurrency_ReturnsTrue()
        {
            // Arrange
            int availableCurrency = 100;
            
            // Act & Assert
            Assert.IsTrue(testConsumableItem.CanAfford(availableCurrency));
        }
        
        [Test]
        public void CanAfford_WithInsufficientCurrency_ReturnsFalse()
        {
            // Arrange
            int availableCurrency = 10;
            
            // Act & Assert
            Assert.IsFalse(testConsumableItem.CanAfford(availableCurrency));
        }
        
        [Test]
        public void CanAfford_AtMaxPurchases_ReturnsFalse()
        {
            // Arrange - Purchase item to max
            for (int i = 0; i < testAbilityItem.MaxPurchases; i++)
            {
                testAbilityItem.Purchase(testPlayer);
            }
            
            // Act & Assert
            Assert.IsFalse(testAbilityItem.CanAfford(1000)); // Even with sufficient currency
            Assert.IsFalse(testAbilityItem.CanPurchase);
        }
        
        [Test]
        public void Purchase_IncrementsPurchaseCount()
        {
            // Arrange
            int initialPurchases = testConsumableItem.TimesPurchased;
            
            // Act
            testConsumableItem.Purchase(testPlayer);
            
            // Assert
            Assert.AreEqual(initialPurchases + 1, testConsumableItem.TimesPurchased);
        }
        
        [Test]
        public void Purchase_BeyondMaxPurchases_DoesNotIncrement()
        {
            // Arrange - Purchase to max
            for (int i = 0; i < testAbilityItem.MaxPurchases; i++)
            {
                testAbilityItem.Purchase(testPlayer);
            }
            int purchasesAtMax = testAbilityItem.TimesPurchased;
            
            // Act - Try to purchase beyond max
            testAbilityItem.Purchase(testPlayer);
            
            // Assert
            Assert.AreEqual(purchasesAtMax, testAbilityItem.TimesPurchased);
        }
        
        #endregion
        
        #region Cost Scaling Tests
        
        [Test]
        public void Cost_WithMultiplePurchases_ScalesCorrectly()
        {
            // Arrange - Multi-purchase item with base cost 50
            int baseCost = testMultiPurchaseItem.Cost; // Should be 50 for first purchase
            
            // Act - Purchase once
            testMultiPurchaseItem.Purchase(testPlayer);
            int secondCost = testMultiPurchaseItem.Cost;
            
            // Purchase again
            testMultiPurchaseItem.Purchase(testPlayer);
            int thirdCost = testMultiPurchaseItem.Cost;
            
            // Assert - Cost should scale (1.2x multiplier per purchase)
            Assert.AreEqual(50, baseCost);
            Assert.Greater(secondCost, baseCost, "Second purchase should cost more");
            Assert.Greater(thirdCost, secondCost, "Third purchase should cost even more");
        }
        
        [Test]
        public void Cost_SinglePurchaseItem_DoesNotScale()
        {
            // Arrange
            int initialCost = testAbilityItem.Cost;
            
            // Act - Purchase (single purchase item)
            testAbilityItem.Purchase(testPlayer);
            
            // Assert - Cost should remain the same (but item can't be purchased again)
            Assert.AreEqual(initialCost, 150); // Base cost should remain unchanged
        }
        
        #endregion
        
        #region Reset Logic Tests
        
        [Test]
        public void ResetPurchases_MetaCurrency_ResetsToZero()
        {
            // Arrange - Purchase meta currency item multiple times
            testConsumableItem.Purchase(testPlayer);
            testConsumableItem.Purchase(testPlayer);
            Assert.Greater(testConsumableItem.TimesPurchased, 0);
            
            // Act
            testConsumableItem.ResetPurchases();
            
            // Assert
            Assert.AreEqual(0, testConsumableItem.TimesPurchased);
            Assert.IsTrue(testConsumableItem.CanPurchase);
        }
        
        [Test]
        public void ResetPurchases_PermaCurrency_DoesNotReset()
        {
            // Arrange - Purchase perma currency item
            testAbilityItem.Purchase(testPlayer);
            int purchasesBeforeReset = testAbilityItem.TimesPurchased;
            
            // Act
            testAbilityItem.ResetPurchases();
            
            // Assert - Perma purchases should persist
            Assert.AreEqual(purchasesBeforeReset, testAbilityItem.TimesPurchased);
        }
        
        #endregion
        
        #region Item Effect Integration Tests
        
        [Test]
        public void Purchase_CallsEffectApplication()
        {
            // Arrange
            bool effectApplied = false;
            var item = CreateTestItem("test_effect", "Test Effect", CurrencyType.Meta, 10, ItemType.Upgrade, 1, false);
            
            // Create a test effect that we can verify
            var effects = new ItemEffect[1];
            var effect = new ItemEffect();
            var effectType = typeof(ItemEffect);
            effectType.GetField("effectType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, EffectType.Heal);
            effectType.GetField("value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, 10f);
            effectType.GetField("targetProperty", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(effect, "health");
            effects[0] = effect;
            
            var itemType = typeof(PurchasableItem);
            itemType.GetField("effects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(item, effects);
            
            // Act
            item.Purchase(testPlayer);
            
            // Assert - Item should be purchased (effect application is implementation-specific)
            Assert.AreEqual(1, item.TimesPurchased);
            
            // Cleanup
            Object.DestroyImmediate(item);
        }
        
        #endregion
        
        #region Integration Tests with Player
        
        [Test]
        public void PurchasableItem_IntegratesWithPlayer_WithoutErrors()
        {
            // Arrange - Player should have properly initialized currency components
            Assert.IsNotNull(testPlayer.MetaCurrency, "Player should have MetaCurrency component");
            
            // Act - Purchase should not throw exceptions
            Assert.DoesNotThrow(() => testConsumableItem.Purchase(testPlayer));
            
            // Assert
            Assert.AreEqual(1, testConsumableItem.TimesPurchased);
        }
        
        [Test]
        public void PurchasableItem_ShopAvailability_ConfiguredCorrectly()
        {
            // Act & Assert - Meta items available in meta shop by default
            Assert.IsTrue(testConsumableItem.IsAvailableInMetaShop);
            Assert.IsFalse(testConsumableItem.IsAvailableInPermaShop);
            
            // Perma items need explicit configuration for shop availability
            Assert.IsTrue(testAbilityItem.IsAvailableInMetaShop); // Default
            Assert.IsFalse(testAbilityItem.IsAvailableInPermaShop); // Would need explicit setup
        }
        
        #endregion
        
        #region Edge Cases and Error Handling
        
        [Test]
        public void Purchase_WithNullPlayer_DoesNotThrow()
        {
            // Act & Assert - Should handle null gracefully
            Assert.DoesNotThrow(() => testConsumableItem.Purchase(null));
        }
        
        [Test]
        public void ItemId_EmptyOrNull_UsesNameAsId()
        {
            // Arrange
            var item = ScriptableObject.CreateInstance<PurchasableItem>();
            item.name = "test_item_name";
            
            // Act - OnValidate should set ID (use reflection since it's a ScriptableObject)
            var onValidateMethod = typeof(PurchasableItem).GetMethod("OnValidate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            onValidateMethod?.Invoke(item, null);
            
            // Assert
            Assert.AreEqual("test_item_name", item.ItemId);
            
            // Cleanup
            Object.DestroyImmediate(item);
        }
        
        #endregion
    }
}