using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Dialog;
using NeonLadderURP.DataManagement;
using System;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class DialogueSaveIntegrationTests
    {
        private DialogueSaveIntegration dialogueIntegration;
        private GameObject testGameObject;
        
        [SetUp]
        public void Setup()
        {
            // Create test game object with integration component
            testGameObject = new GameObject("TestDialogueIntegration");
            dialogueIntegration = testGameObject.AddComponent<DialogueSaveIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.Destroy(testGameObject);
            }
            
            // Clean up any test save files
            EnhancedSaveSystem.DeleteSave();
        }
        
        [Test]
        public void DialogueSaveData_CreateNew_InitializesCorrectly()
        {
            // Act
            var saveData = DialogueSaveData.CreateNew();
            
            // Assert
            Assert.NotNull(saveData);
            Assert.NotNull(saveData.conversationStates);
            Assert.NotNull(saveData.dialogueVariables);
            Assert.NotNull(saveData.questStates);
            Assert.NotNull(saveData.actorStates);
            Assert.NotNull(saveData.choiceHistory);
            Assert.NotNull(saveData.cvcData);
            Assert.AreEqual(0, saveData.cvcData.courage);
            Assert.AreEqual(0, saveData.cvcData.virtue);
            Assert.AreEqual(0, saveData.cvcData.cunning);
        }
        
        [Test]
        public void DialogueSaveData_Validate_FixesNullReferences()
        {
            // Arrange
            var saveData = new DialogueSaveData();
            saveData.conversationStates = null;
            saveData.dialogueVariables = null;
            
            // Act
            bool isValid = saveData.Validate();
            
            // Assert
            Assert.IsTrue(isValid);
            Assert.NotNull(saveData.conversationStates);
            Assert.NotNull(saveData.dialogueVariables);
            Assert.NotNull(saveData.questStates);
            Assert.NotNull(saveData.actorStates);
            Assert.NotNull(saveData.choiceHistory);
            Assert.NotNull(saveData.cvcData);
        }
        
        [Test]
        public void DialogueSaveData_MergeWith_CombinesDataCorrectly()
        {
            // Arrange
            var data1 = DialogueSaveData.CreateNew();
            data1.cvcData.courage = 5;
            data1.cvcData.virtue = 3;
            
            var conv1 = new DialogueSaveData.ConversationState
            {
                conversationId = 1,
                conversationTitle = "Test Conv",
                hasBeenPlayed = true,
                timesPlayed = 1
            };
            data1.conversationStates.Add(conv1);
            
            var data2 = DialogueSaveData.CreateNew();
            data2.cvcData.courage = 10;
            data2.cvcData.cunning = 7;
            
            var conv2 = new DialogueSaveData.ConversationState
            {
                conversationId = 1,
                conversationTitle = "Test Conv",
                hasBeenPlayed = true,
                timesPlayed = 2
            };
            data2.conversationStates.Add(conv2);
            
            // Act
            data1.MergeWith(data2);
            
            // Assert
            Assert.AreEqual(10, data1.cvcData.courage); // Takes max value
            Assert.AreEqual(3, data1.cvcData.virtue);   // Keeps original
            Assert.AreEqual(7, data1.cvcData.cunning);  // Takes max value
            Assert.AreEqual(1, data1.conversationStates.Count);
            Assert.AreEqual(2, data1.conversationStates[0].timesPlayed); // Takes max
        }
        
        [UnityTest]
        public IEnumerator DialogueSaveIntegration_SaveAndLoad_PersistsData()
        {
            // Arrange
            var testData = dialogueIntegration.GetCurrentDialogueData();
            testData.cvcData.courage = 15;
            testData.cvcData.virtue = 8;
            testData.cvcData.cunning = 12;
            testData.lastConversationId = "TestConversation";
            testData.totalConversationsPlayed = 5;
            
            // Act - Save
            dialogueIntegration.SaveDialogueData();
            yield return null;
            
            // Reset the integration to simulate fresh load
            Object.Destroy(testGameObject);
            testGameObject = new GameObject("TestDialogueIntegration2");
            dialogueIntegration = testGameObject.AddComponent<DialogueSaveIntegration>();
            yield return null;
            
            // Act - Load
            dialogueIntegration.LoadDialogueData();
            var loadedData = dialogueIntegration.GetCurrentDialogueData();
            
            // Assert
            Assert.NotNull(loadedData);
            Assert.AreEqual(15, loadedData.cvcData.courage);
            Assert.AreEqual(8, loadedData.cvcData.virtue);
            Assert.AreEqual(12, loadedData.cvcData.cunning);
            Assert.AreEqual("TestConversation", loadedData.lastConversationId);
            Assert.AreEqual(5, loadedData.totalConversationsPlayed);
        }
        
        [Test]
        public void DialogueSaveIntegration_RecordPlayerChoice_AddsToHistory()
        {
            // Arrange
            int conversationId = 1;
            int nodeId = 10;
            string choiceText = "I choose courage!";
            string consequence = "Gained courage points";
            
            // Act
            dialogueIntegration.RecordPlayerChoice(conversationId, nodeId, choiceText, consequence);
            var currentData = dialogueIntegration.GetCurrentDialogueData();
            
            // Assert
            Assert.AreEqual(1, currentData.choiceHistory.Count);
            var recordedChoice = currentData.choiceHistory[0];
            Assert.AreEqual(conversationId, recordedChoice.conversationId);
            Assert.AreEqual(nodeId, recordedChoice.nodeId);
            Assert.AreEqual(choiceText, recordedChoice.choiceText);
            Assert.AreEqual(consequence, recordedChoice.consequence);
            Assert.NotNull(recordedChoice.timestamp);
        }
        
        [Test]
        public void DialogueSaveIntegration_SetCVCValues_UpdatesAndTriggersEvent()
        {
            // Arrange
            bool eventTriggered = false;
            int eventCourage = 0;
            int eventVirtue = 0;
            int eventCunning = 0;
            
            DialogueSaveIntegration.OnCVCValuesChanged += (c, v, cu) =>
            {
                eventTriggered = true;
                eventCourage = c;
                eventVirtue = v;
                eventCunning = cu;
            };
            
            // Act
            dialogueIntegration.SetCVCValues(20, 15, 10);
            
            // Assert
            var (courage, virtue, cunning) = dialogueIntegration.GetCVCValues();
            Assert.AreEqual(20, courage);
            Assert.AreEqual(15, virtue);
            Assert.AreEqual(10, cunning);
            
            Assert.IsTrue(eventTriggered);
            Assert.AreEqual(20, eventCourage);
            Assert.AreEqual(15, eventVirtue);
            Assert.AreEqual(10, eventCunning);
            
            // Clean up event subscription
            DialogueSaveIntegration.OnCVCValuesChanged = null;
        }
        
        [Test]
        public void ConversationState_TrackingWorks()
        {
            // Arrange
            var saveData = DialogueSaveData.CreateNew();
            var convState = new DialogueSaveData.ConversationState
            {
                conversationId = 42,
                conversationTitle = "Boss Battle Dialog",
                hasBeenPlayed = false,
                timesPlayed = 0
            };
            
            // Act
            saveData.conversationStates.Add(convState);
            convState.hasBeenPlayed = true;
            convState.timesPlayed++;
            convState.nodesVisited.Add(1);
            convState.nodesVisited.Add(5);
            convState.nodesVisited.Add(10);
            convState.lastPlayed = DateTime.Now;
            
            // Assert
            Assert.AreEqual(1, saveData.conversationStates.Count);
            Assert.IsTrue(saveData.conversationStates[0].hasBeenPlayed);
            Assert.AreEqual(1, saveData.conversationStates[0].timesPlayed);
            Assert.AreEqual(3, saveData.conversationStates[0].nodesVisited.Count);
            Assert.Contains(5, saveData.conversationStates[0].nodesVisited);
        }
        
        [Test]
        public void QuestState_TrackingWorks()
        {
            // Arrange
            var saveData = DialogueSaveData.CreateNew();
            var questState = new DialogueSaveData.QuestState
            {
                questName = "Find the Sacred Sword",
                currentState = "active",
                currentEntryId = 2
            };
            
            // Act
            questState.completedEntries.Add(1);
            questState.questFlags["talked_to_elder"] = true;
            questState.questFlags["found_map"] = false;
            saveData.questStates.Add(questState);
            
            // Assert
            Assert.AreEqual(1, saveData.questStates.Count);
            Assert.AreEqual("active", saveData.questStates[0].currentState);
            Assert.AreEqual(2, saveData.questStates[0].currentEntryId);
            Assert.Contains(1, saveData.questStates[0].completedEntries);
            Assert.IsTrue(saveData.questStates[0].questFlags["talked_to_elder"]);
            Assert.IsFalse(saveData.questStates[0].questFlags["found_map"]);
        }
        
        [Test]
        public void ActorState_RelationshipTracking()
        {
            // Arrange
            var saveData = DialogueSaveData.CreateNew();
            var actorState = new DialogueSaveData.ActorState
            {
                actorName = "Merchant NPC",
                actorId = 101,
                relationshipLevel = 0f,
                currentMood = "neutral"
            };
            
            // Act
            actorState.relationshipLevel += 25f; // Improve relationship
            actorState.currentMood = "friendly";
            actorState.attributes["trust"] = 50f;
            actorState.attributes["loyalty"] = 30f;
            actorState.rememberedTopics.Add("player_helped_with_bandits");
            actorState.rememberedTopics.Add("player_bought_expensive_item");
            saveData.actorStates.Add(actorState);
            
            // Assert
            Assert.AreEqual(1, saveData.actorStates.Count);
            Assert.AreEqual(25f, saveData.actorStates[0].relationshipLevel);
            Assert.AreEqual("friendly", saveData.actorStates[0].currentMood);
            Assert.AreEqual(50f, saveData.actorStates[0].attributes["trust"]);
            Assert.AreEqual(2, saveData.actorStates[0].rememberedTopics.Count);
        }
        
        [Test]
        public void CVCData_ChangeHistory_TracksCorrectly()
        {
            // Arrange
            var cvcData = new DialogueSaveData.CVCData();
            
            // Act
            var change1 = new DialogueSaveData.CVCData.CVCChange
            {
                valueName = "courage",
                oldValue = 0,
                newValue = 5,
                reason = "Stood up to bully",
                timestamp = DateTime.Now
            };
            
            var change2 = new DialogueSaveData.CVCData.CVCChange
            {
                valueName = "virtue",
                oldValue = 0,
                newValue = -3,
                reason = "Stole from merchant",
                timestamp = DateTime.Now
            };
            
            cvcData.changeHistory.Add(change1);
            cvcData.changeHistory.Add(change2);
            cvcData.courage = 5;
            cvcData.virtue = -3;
            
            // Assert
            Assert.AreEqual(2, cvcData.changeHistory.Count);
            Assert.AreEqual("courage", cvcData.changeHistory[0].valueName);
            Assert.AreEqual(5, cvcData.changeHistory[0].newValue);
            Assert.AreEqual("virtue", cvcData.changeHistory[1].valueName);
            Assert.AreEqual(-3, cvcData.changeHistory[1].newValue);
        }
        
        [UnityTest]
        public IEnumerator Integration_WithEnhancedSaveSystem_WorksCorrectly()
        {
            // Arrange
            var testData = dialogueIntegration.GetCurrentDialogueData();
            testData.cvcData.courage = 99;
            testData.totalDialogueTime = 1234.5f;
            
            // Act - Save through integration
            dialogueIntegration.SaveDialogueData();
            yield return null;
            
            // Load through enhanced save system
            var consolidatedSave = EnhancedSaveSystem.Load();
            
            // Assert
            Assert.NotNull(consolidatedSave);
            Assert.IsFalse(string.IsNullOrEmpty(consolidatedSave.dialogueDataJson));
            
            // Deserialize and verify
            var loadedDialogueData = JsonUtility.FromJson<DialogueSaveData>(consolidatedSave.dialogueDataJson);
            Assert.NotNull(loadedDialogueData);
            Assert.AreEqual(99, loadedDialogueData.cvcData.courage);
            Assert.AreEqual(1234.5f, loadedDialogueData.totalDialogueTime, 0.01f);
        }
    }
}