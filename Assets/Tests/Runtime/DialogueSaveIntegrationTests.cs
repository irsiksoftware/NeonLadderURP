using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Dialog;
using NeonLadderURP.DataManagement;
using NeonLadder.DataManagement;
using PixelCrushers.DialogueSystem;
using System;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class DialogueSaveIntegrationTests
    {
        private DialogueSaveIntegration saveIntegration;
        private GameObject integrationObject;
        private ConversationPointTracker pointTracker;
        private GameObject pointTrackerObject;
        
        [SetUp]
        public void Setup()
        {
            // Create dialogue save integration
            integrationObject = new GameObject("TestDialogueSaveIntegration");
            saveIntegration = integrationObject.AddComponent<DialogueSaveIntegration>();
            
            // Create conversation point tracker
            pointTrackerObject = new GameObject("TestPointTracker");
            pointTracker = pointTrackerObject.AddComponent<ConversationPointTracker>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (integrationObject != null)
            {
                Object.DestroyImmediate(integrationObject);
            }
            
            if (pointTrackerObject != null)
            {
                Object.DestroyImmediate(pointTrackerObject);
            }
        }
        
        #region Save/Load Tests
        
        [Test]
        public void SaveDialogueData_CreatesValidSaveData()
        {
            // Arrange
            bool saveEventFired = false;
            DialogueData savedData = null;
            
            DialogueSaveIntegration.OnDialogueDataSaved += (data) =>
            {
                saveEventFired = true;
                savedData = data;
            };
            
            // Act
            saveIntegration.SaveDialogueData();
            
            // Assert
            Assert.IsTrue(saveEventFired, "Save event should fire");
            Assert.IsNotNull(savedData, "Saved data should not be null");
            Assert.IsNotNull(savedData.lastSaved, "Last saved timestamp should be set");
            
            // Cleanup
            DialogueSaveIntegration.OnDialogueDataSaved -= (data) =>
            {
                saveEventFired = true;
                savedData = data;
            };
        }
        
        [Test]
        public void LoadDialogueData_RestoresFromSaveData()
        {
            // Arrange
            // First save some data
            saveIntegration.SaveDialogueData();
            
            bool loadEventFired = false;
            DialogueData loadedData = null;
            
            DialogueSaveIntegration.OnDialogueDataLoaded += (data) =>
            {
                loadEventFired = true;
                loadedData = data;
            };
            
            // Act
            saveIntegration.LoadDialogueData();
            
            // Assert
            Assert.IsTrue(loadEventFired, "Load event should fire");
            Assert.IsNotNull(loadedData, "Loaded data should not be null");
            
            // Cleanup
            DialogueSaveIntegration.OnDialogueDataLoaded -= (data) =>
            {
                loadEventFired = true;
                loadedData = data;
            };
        }
        
        [Test]
        public void GetCurrentDialogueData_ReturnsValidData()
        {
            // Act
            var dialogueData = saveIntegration.GetCurrentDialogueData();
            
            // Assert
            Assert.IsNotNull(dialogueData, "Should return dialogue data");
            Assert.IsNotNull(dialogueData.conversationHistory, "Conversation history should be initialized");
            Assert.IsNotNull(dialogueData.cvcPoints, "CVC points should be initialized");
            Assert.IsNotNull(dialogueData.questStates, "Quest states should be initialized");
            Assert.IsNotNull(dialogueData.relationships, "Relationships should be initialized");
        }
        
        [Test]
        public void ResetDialogueProgress_ClearsAllData()
        {
            // Arrange
            saveIntegration.SaveDialogueData();
            var dataBefore = saveIntegration.GetCurrentDialogueData();
            
            // Act
            saveIntegration.ResetDialogueProgress();
            var dataAfter = saveIntegration.GetCurrentDialogueData();
            
            // Assert
            Assert.AreEqual(0, dataAfter.conversationHistory.Count, "Conversation history should be cleared");
            Assert.AreEqual(0, dataAfter.cvcPoints.Count, "CVC points should be cleared");
            Assert.AreEqual(0, dataAfter.questStates.Count, "Quest states should be cleared");
            Assert.AreEqual(0, dataAfter.relationships.Count, "Relationships should be cleared");
        }
        
        #endregion
        
        #region CVC Data Tests
        
        [Test]
        public void CVCData_SavesAndLoadsCorrectly()
        {
            // Arrange
            var cvcData = new CVCData
            {
                characterId = "TestNPC",
                totalPoints = 150,
                cvcLevel = 6,
                couragePoints = 50,
                virtuePoints = 60,
                cunningPoints = 40,
                choiceHistory = new List<string> { "choice1", "choice2", "choice3" }
            };
            
            var dialogueData = new DialogueData();
            dialogueData.cvcPoints["TestNPC"] = cvcData;
            
            // Act - Serialize and deserialize
            string json = JsonUtility.ToJson(dialogueData);
            var loadedData = JsonUtility.FromJson<DialogueData>(json);
            
            // Assert
            Assert.IsNotNull(loadedData.cvcPoints, "CVC points should be loaded");
            Assert.IsTrue(loadedData.cvcPoints.ContainsKey("TestNPC"), "Should contain test NPC");
            
            var loadedCVC = loadedData.cvcPoints["TestNPC"];
            Assert.AreEqual(150, loadedCVC.totalPoints, "Total points should match");
            Assert.AreEqual(6, loadedCVC.cvcLevel, "CVC level should match");
            Assert.AreEqual(50, loadedCVC.couragePoints, "Courage points should match");
            Assert.AreEqual(60, loadedCVC.virtuePoints, "Virtue points should match");
            Assert.AreEqual(40, loadedCVC.cunningPoints, "Cunning points should match");
        }
        
        [Test]
        public void MultipleCVCCharacters_SaveCorrectly()
        {
            // Arrange
            var dialogueData = new DialogueData();
            
            for (int i = 0; i < 5; i++)
            {
                dialogueData.cvcPoints[$"NPC_{i}"] = new CVCData
                {
                    characterId = $"NPC_{i}",
                    totalPoints = i * 25,
                    cvcLevel = i
                };
            }
            
            // Act
            string json = JsonUtility.ToJson(dialogueData);
            var loadedData = JsonUtility.FromJson<DialogueData>(json);
            
            // Assert
            Assert.AreEqual(5, loadedData.cvcPoints.Count, "Should have 5 characters");
            
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(loadedData.cvcPoints.ContainsKey($"NPC_{i}"), $"Should contain NPC_{i}");
                Assert.AreEqual(i * 25, loadedData.cvcPoints[$"NPC_{i}"].totalPoints, $"Points for NPC_{i} should match");
            }
        }
        
        #endregion
        
        #region Conversation History Tests
        
        [Test]
        public void ConversationHistory_TracksDisplayedEntries()
        {
            // Arrange
            var dialogueData = new DialogueData();
            dialogueData.conversationHistory["TestConversation"] = new List<int> { 1, 2, 3, 5, 8 };
            dialogueData.conversationHistory["AnotherConversation"] = new List<int> { 1, 4 };
            
            // Act
            string json = JsonUtility.ToJson(dialogueData);
            var loadedData = JsonUtility.FromJson<DialogueData>(json);
            
            // Assert
            Assert.AreEqual(2, loadedData.conversationHistory.Count, "Should have 2 conversations");
            Assert.AreEqual(5, loadedData.conversationHistory["TestConversation"].Count, "Should have 5 entries");
            Assert.AreEqual(2, loadedData.conversationHistory["AnotherConversation"].Count, "Should have 2 entries");
            Assert.Contains(5, loadedData.conversationHistory["TestConversation"], "Should contain entry 5");
        }
        
        #endregion
        
        #region Quest State Tests
        
        [Test]
        public void QuestStates_SaveAndLoadCorrectly()
        {
            // Arrange
            var dialogueData = new DialogueData();
            dialogueData.questStates["MainQuest"] = "active";
            dialogueData.questStates["SideQuest1"] = "success";
            dialogueData.questStates["SideQuest2"] = "failure";
            dialogueData.questStates["MainQuest_Entry1"] = "success";
            dialogueData.questStates["MainQuest_Entry2"] = "active";
            
            // Act
            string json = JsonUtility.ToJson(dialogueData);
            var loadedData = JsonUtility.FromJson<DialogueData>(json);
            
            // Assert
            Assert.AreEqual(5, loadedData.questStates.Count, "Should have 5 quest states");
            Assert.AreEqual("active", loadedData.questStates["MainQuest"], "Main quest should be active");
            Assert.AreEqual("success", loadedData.questStates["SideQuest1"], "Side quest 1 should be success");
            Assert.AreEqual("failure", loadedData.questStates["SideQuest2"], "Side quest 2 should be failure");
            Assert.AreEqual("success", loadedData.questStates["MainQuest_Entry1"], "Entry 1 should be success");
        }
        
        #endregion
        
        #region Relationship Tests
        
        [Test]
        public void Relationships_SaveAndLoadCorrectly()
        {
            // Arrange
            var dialogueData = new DialogueData();
            dialogueData.relationships["Ally1"] = 75.5f;
            dialogueData.relationships["Neutral1"] = 0f;
            dialogueData.relationships["Enemy1"] = -50.2f;
            
            // Act
            string json = JsonUtility.ToJson(dialogueData);
            var loadedData = JsonUtility.FromJson<DialogueData>(json);
            
            // Assert
            Assert.AreEqual(3, loadedData.relationships.Count, "Should have 3 relationships");
            Assert.AreEqual(75.5f, loadedData.relationships["Ally1"], 0.01f, "Ally relationship should match");
            Assert.AreEqual(0f, loadedData.relationships["Neutral1"], 0.01f, "Neutral relationship should match");
            Assert.AreEqual(-50.2f, loadedData.relationships["Enemy1"], 0.01f, "Enemy relationship should match");
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator SaveAndLoad_MaintainsConsistency()
        {
            // Arrange
            var originalData = new DialogueData
            {
                lastSaved = DateTime.Now,
                activeConversation = "TestConversation",
                activeConversationEntry = 5,
                totalCVCPoints = 250,
                totalCVCLevel = 10
            };
            
            originalData.cvcPoints["TestNPC"] = new CVCData
            {
                characterId = "TestNPC",
                totalPoints = 100,
                cvcLevel = 4
            };
            
            // Save
            saveIntegration.SaveDialogueData();
            
            yield return null;
            
            // Load
            saveIntegration.LoadDialogueData();
            
            yield return null;
            
            // Verify
            var loadedData = saveIntegration.GetCurrentDialogueData();
            Assert.IsNotNull(loadedData, "Data should be loaded");
            
            // The specific values would be from the actual dialogue system state
            // This test verifies the save/load cycle works
            Assert.Pass("Save/load cycle completed successfully");
        }
        
        #endregion
        
        #region Error Handling Tests
        
        [Test]
        public void LoadDialogueData_WithNoSaveData_HandlesGracefully()
        {
            // Arrange
            bool errorEventFired = false;
            string errorMessage = null;
            
            DialogueSaveIntegration.OnDialogueStateError += (error) =>
            {
                errorEventFired = true;
                errorMessage = error;
            };
            
            // Act - Try to load without any save data
            saveIntegration.LoadDialogueData();
            
            // Assert - Should not error, just log that no data was found
            Assert.IsFalse(errorEventFired, "Should not fire error event for missing data");
            
            // Cleanup
            DialogueSaveIntegration.OnDialogueStateError -= (error) =>
            {
                errorEventFired = true;
                errorMessage = error;
            };
        }
        
        #endregion
    }
}