using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadderURP.DataManagement;
using NeonLadder.Dialog;
using System;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for Dialogue Save State Integration
    /// Tests persistence of dialogue data across sessions
    /// </summary>
    [TestFixture]
    public class DialogueSaveIntegrationTests
    {
        private DialogueSaveData dialogueSaveData;
        private ConsolidatedSaveData consolidatedSaveData;
        
        [SetUp]
        public void Setup()
        {
            dialogueSaveData = new DialogueSaveData();
            consolidatedSaveData = new ConsolidatedSaveData();
        }
        
        [Test]
        public void DialogueSaveData_CanBeCreated()
        {
            // Assert
            Assert.IsNotNull(dialogueSaveData, "DialogueSaveData should be created");
            Assert.IsNotNull(dialogueSaveData.conversationHistory, "Conversation history should be initialized");
            Assert.IsNotNull(dialogueSaveData.choicesMadePerConversation, "Choices dictionary should be initialized");
            Assert.IsNotNull(dialogueSaveData.completedConversations, "Completed conversations list should be initialized");
        }
        
        [Test]
        public void ConsolidatedSaveData_IncludesDialogueData()
        {
            // Assert
            Assert.IsNotNull(consolidatedSaveData.dialogueData, "ConsolidatedSaveData should include dialogueData");
        }
        
        [Test]
        public void RecordConversationStart_CreatesRecord()
        {
            // Arrange
            string conversationId = "TestConversation";
            string characterName = "TestNPC";
            
            // Act
            dialogueSaveData.RecordConversationStart(conversationId, characterName);
            
            // Assert
            Assert.AreEqual(1, dialogueSaveData.conversationHistory.Count, "Should have one conversation record");
            Assert.AreEqual(conversationId, dialogueSaveData.lastActiveConversation, "Last active conversation should be set");
            Assert.AreEqual(1, dialogueSaveData.totalConversationsStarted, "Total conversations started should be 1");
            
            var record = dialogueSaveData.conversationHistory[0];
            Assert.AreEqual(conversationId, record.conversationId);
            Assert.AreEqual(characterName, record.characterName);
            Assert.IsFalse(record.completed);
        }
        
        [Test]
        public void RecordChoice_AddsToHistory()
        {
            // Arrange
            string conversationId = "TestConversation";
            dialogueSaveData.RecordConversationStart(conversationId);
            
            // Act
            dialogueSaveData.RecordChoice(conversationId, 0, "Choice 1", "aggressive");
            dialogueSaveData.RecordChoice(conversationId, 1, "Choice 2", "diplomatic");
            
            // Assert
            Assert.AreEqual(2, dialogueSaveData.totalChoicesMade, "Should have made 2 choices");
            Assert.IsTrue(dialogueSaveData.choicesMadePerConversation.ContainsKey(conversationId));
            Assert.AreEqual(2, dialogueSaveData.choicesMadePerConversation[conversationId].Count);
            
            // Check choice type tracking
            Assert.AreEqual(1, dialogueSaveData.choiceTypeFrequency["aggressive"]);
            Assert.AreEqual(1, dialogueSaveData.choiceTypeFrequency["diplomatic"]);
        }
        
        [Test]
        public void RecordConversationComplete_MarksAsCompleted()
        {
            // Arrange
            string conversationId = "TestConversation";
            dialogueSaveData.RecordConversationStart(conversationId);
            
            // Act
            dialogueSaveData.RecordConversationComplete(conversationId);
            
            // Assert
            Assert.IsTrue(dialogueSaveData.HasCompletedConversation(conversationId));
            Assert.AreEqual(1, dialogueSaveData.totalConversationsCompleted);
            Assert.Contains(conversationId, dialogueSaveData.completedConversations);
        }
        
        [Test]
        public void UpdateRelationship_TracksChanges()
        {
            // Arrange
            string characterName = "TestNPC";
            
            // Act
            dialogueSaveData.UpdateRelationship(characterName, 10, "friendly");
            dialogueSaveData.UpdateRelationship(characterName, -5, "hostile");
            dialogueSaveData.UpdateRelationship(characterName, 15, "friendly");
            
            // Assert
            var relationship = dialogueSaveData.GetRelationship(characterName);
            Assert.IsNotNull(relationship);
            Assert.AreEqual(20, relationship.relationshipPoints); // 10 - 5 + 15
            Assert.AreEqual(3, relationship.totalInteractions);
            Assert.AreEqual(25, relationship.relationshipHistory["friendly"]); // 10 + 15
            Assert.AreEqual(-5, relationship.relationshipHistory["hostile"]);
        }
        
        [Test]
        public void UpdateCVCLevel_CalculatesLevels()
        {
            // Arrange
            string characterName = "TestNPC";
            
            // Act
            dialogueSaveData.UpdateCVCLevel(characterName, CVCType.Charisma, 50);
            dialogueSaveData.UpdateCVCLevel(characterName, CVCType.Charisma, 60); // Total: 110, Level 1
            dialogueSaveData.UpdateCVCLevel(characterName, CVCType.Violence, 250); // Level 2
            
            // Assert
            var charismaLevel = dialogueSaveData.GetCVCLevel(characterName, CVCType.Charisma);
            Assert.IsNotNull(charismaLevel);
            Assert.AreEqual(1, charismaLevel.currentLevel); // 110 / 100 = 1
            Assert.AreEqual(110, charismaLevel.currentPoints);
            Assert.AreEqual(110, charismaLevel.totalPointsEarned);
            
            var violenceLevel = dialogueSaveData.GetCVCLevel(characterName, CVCType.Violence);
            Assert.IsNotNull(violenceLevel);
            Assert.AreEqual(2, violenceLevel.currentLevel); // 250 / 100 = 2
            Assert.AreEqual(250, violenceLevel.currentPoints);
        }
        
        [Test]
        public void DialogueVariables_StoreAndRetrieve()
        {
            // Act
            dialogueSaveData.SetDialogueVariable("TestInt", 42);
            dialogueSaveData.SetDialogueVariable("TestString", "Hello");
            dialogueSaveData.SetDialogueVariable("TestBool", true);
            dialogueSaveData.SetDialogueVariable("TestFloat", 3.14f);
            
            // Assert
            Assert.AreEqual(42, dialogueSaveData.GetDialogueVariable<int>("TestInt"));
            Assert.AreEqual("Hello", dialogueSaveData.GetDialogueVariable<string>("TestString"));
            Assert.AreEqual(true, dialogueSaveData.GetDialogueVariable<bool>("TestBool"));
            Assert.AreEqual(3.14f, dialogueSaveData.GetDialogueVariable<float>("TestFloat"), 0.01f);
            
            // Test default values
            Assert.AreEqual(999, dialogueSaveData.GetDialogueVariable<int>("NonExistent", 999));
        }
        
        [Test]
        public void UnlockDialoguePath_TracksUnlocks()
        {
            // Act
            dialogueSaveData.UnlockDialoguePath("SecretPath1");
            dialogueSaveData.UnlockDialoguePath("SecretPath2");
            dialogueSaveData.UnlockDialoguePath("SecretPath1"); // Duplicate, should not add again
            
            // Assert
            Assert.AreEqual(2, dialogueSaveData.unlockedDialoguePaths.Count);
            Assert.Contains("SecretPath1", dialogueSaveData.unlockedDialoguePaths);
            Assert.Contains("SecretPath2", dialogueSaveData.unlockedDialoguePaths);
        }
        
        [Test]
        public void SaveConversationState_PreservesState()
        {
            // Arrange
            string conversationId = "TestConversation";
            string nodeId = "Node_5";
            var localVars = new Dictionary<string, object>
            {
                { "LocalVar1", 100 },
                { "LocalVar2", "Test" }
            };
            
            // Act
            dialogueSaveData.SaveConversationState(conversationId, nodeId, localVars);
            
            // Assert
            Assert.IsNotNull(dialogueSaveData.currentConversationState);
            Assert.AreEqual(conversationId, dialogueSaveData.currentConversationState.conversationId);
            Assert.AreEqual(nodeId, dialogueSaveData.currentConversationState.currentNodeId);
            Assert.AreEqual(2, dialogueSaveData.currentConversationState.localVariables.Count);
        }
        
        [Test]
        public void GetChoicesForConversation_ReturnsCorrectChoices()
        {
            // Arrange
            string conversationId = "TestConversation";
            dialogueSaveData.RecordConversationStart(conversationId);
            dialogueSaveData.RecordChoice(conversationId, 0, "Choice 1");
            dialogueSaveData.RecordChoice(conversationId, 2, "Choice 2");
            dialogueSaveData.RecordChoice(conversationId, 1, "Choice 3");
            
            // Act
            var choices = dialogueSaveData.GetChoicesForConversation(conversationId);
            
            // Assert
            Assert.AreEqual(3, choices.Count);
            Assert.AreEqual(0, choices[0]);
            Assert.AreEqual(2, choices[1]);
            Assert.AreEqual(1, choices[2]);
        }
        
        [Test]
        public void GetAnalytics_CalculatesCorrectly()
        {
            // Arrange
            dialogueSaveData.RecordConversationStart("Conv1");
            dialogueSaveData.RecordChoice("Conv1", 0, "Choice1", "aggressive");
            dialogueSaveData.RecordChoice("Conv1", 1, "Choice2", "aggressive");
            dialogueSaveData.RecordConversationComplete("Conv1");
            
            dialogueSaveData.RecordConversationStart("Conv2");
            dialogueSaveData.RecordChoice("Conv2", 0, "Choice3", "diplomatic");
            dialogueSaveData.RecordConversationComplete("Conv2");
            
            dialogueSaveData.UpdateRelationship("NPC1", 10);
            dialogueSaveData.UpdateRelationship("NPC2", 5);
            
            // Act
            var analytics = dialogueSaveData.GetAnalytics();
            
            // Assert
            Assert.AreEqual(2, analytics.totalConversations);
            Assert.AreEqual(2, analytics.completedConversations);
            Assert.AreEqual(3, analytics.totalChoices);
            Assert.AreEqual(1.5f, analytics.averageChoicesPerConversation); // 3 choices / 2 completed
            Assert.AreEqual("aggressive", analytics.mostFrequentChoiceType); // 2 aggressive vs 1 diplomatic
            Assert.AreEqual(2, analytics.uniqueCharactersInteracted);
        }
        
        [Test]
        public void Reset_ClearsAllData()
        {
            // Arrange
            dialogueSaveData.RecordConversationStart("TestConv");
            dialogueSaveData.RecordChoice("TestConv", 0, "Choice");
            dialogueSaveData.UpdateRelationship("NPC", 10);
            dialogueSaveData.SetDialogueVariable("TestVar", 42);
            dialogueSaveData.UnlockDialoguePath("Path1");
            
            // Act
            dialogueSaveData.Reset();
            
            // Assert
            Assert.AreEqual(0, dialogueSaveData.conversationHistory.Count);
            Assert.AreEqual(0, dialogueSaveData.choicesMadePerConversation.Count);
            Assert.AreEqual(0, dialogueSaveData.completedConversations.Count);
            Assert.AreEqual(0, dialogueSaveData.characterRelationships.Count);
            Assert.AreEqual(0, dialogueSaveData.dialogueVariables.Count);
            Assert.AreEqual(0, dialogueSaveData.unlockedDialoguePaths.Count);
            Assert.AreEqual(0, dialogueSaveData.totalChoicesMade);
            Assert.AreEqual(0, dialogueSaveData.totalConversationsStarted);
            Assert.IsNull(dialogueSaveData.currentConversationState);
            Assert.AreEqual("", dialogueSaveData.lastActiveConversation);
        }
        
        [Test]
        public void DialogueSaveData_SerializesCorrectly()
        {
            // Arrange
            dialogueSaveData.RecordConversationStart("TestConv", "TestNPC");
            dialogueSaveData.RecordChoice("TestConv", 0, "TestChoice");
            dialogueSaveData.SetDialogueVariable("TestVar", 123);
            dialogueSaveData.UpdateRelationship("TestNPC", 50);
            
            // Act - Test JSON serialization
            string json = JsonUtility.ToJson(dialogueSaveData, true);
            DialogueSaveData deserialized = JsonUtility.FromJson<DialogueSaveData>(json);
            
            // Assert - Basic fields should serialize
            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1, deserialized.totalConversationsStarted);
            Assert.AreEqual(1, deserialized.totalChoicesMade);
            Assert.AreEqual("TestConv", deserialized.lastActiveConversation);
            
            // Note: Dictionary fields won't serialize with JsonUtility, 
            // but that's expected and handled by the save system
        }
        
        [UnityTest]
        public IEnumerator DialogueSystemSaveIntegration_PersistsData()
        {
            // Create integration instance
            GameObject go = new GameObject("TestDialogueSaveIntegration");
            var integration = go.AddComponent<DialogueSystemSaveIntegration>();
            
            // Wait for initialization
            yield return null;
            
            // Test basic operations
            integration.UpdateRelationship("TestNPC", 25);
            integration.UpdateCVCLevel("TestNPC", CVCType.Charisma, 150);
            integration.UnlockDialoguePath("SecretPath");
            
            // Verify data is tracked
            var relationship = integration.GetRelationship("TestNPC");
            Assert.IsNotNull(relationship);
            Assert.AreEqual(25, relationship.relationshipPoints);
            
            var cvcLevel = integration.GetCVCLevel("TestNPC", CVCType.Charisma);
            Assert.IsNotNull(cvcLevel);
            Assert.AreEqual(1, cvcLevel.currentLevel);
            
            // Cleanup
            Object.DestroyImmediate(go);
        }
    }
}