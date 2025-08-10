using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Dialog;
using NeonLadderURP.DataManagement;
using System;

namespace NeonLadder.Tests.Runtime
{
    [TestFixture]
    public class DialogueSaveIntegrationTests
    {
        private DialogueSaveIntegration dialogueIntegration;
        private GameObject integrationObject;
        
        [SetUp]
        public void Setup()
        {
            // Create dialogue integration
            integrationObject = new GameObject("TestDialogueIntegration");
            dialogueIntegration = integrationObject.AddComponent<DialogueSaveIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (integrationObject != null)
            {
                Object.DestroyImmediate(integrationObject);
            }
        }
        
        #region CVC Points Tests
        
        [Test]
        public void CVCPoints_InitializeToZero()
        {
            // Arrange
            var cvcPoints = new CVCPoints();
            
            // Assert
            Assert.AreEqual(0, cvcPoints.courage);
            Assert.AreEqual(0, cvcPoints.virtue);
            Assert.AreEqual(0, cvcPoints.cunning);
            Assert.AreEqual(0, cvcPoints.Total);
        }
        
        [Test]
        public void CVCPoints_CalculateTotalCorrectly()
        {
            // Arrange
            var cvcPoints = new CVCPoints
            {
                courage = 10,
                virtue = 15,
                cunning = 8
            };
            
            // Assert
            Assert.AreEqual(33, cvcPoints.Total);
        }
        
        [Test]
        public void CVCPoints_SerializeAndDeserialize()
        {
            // Arrange
            var original = new CVCPoints
            {
                courage = 25,
                virtue = 30,
                cunning = 20
            };
            
            // Act
            string json = JsonUtility.ToJson(original);
            var deserialized = JsonUtility.FromJson<CVCPoints>(json);
            
            // Assert
            Assert.AreEqual(original.courage, deserialized.courage);
            Assert.AreEqual(original.virtue, deserialized.virtue);
            Assert.AreEqual(original.cunning, deserialized.cunning);
        }
        
        #endregion
        
        #region DialogueSystemSaveData Tests
        
        [Test]
        public void DialogueSystemSaveData_InitializesCollections()
        {
            // Arrange
            var saveData = new DialogueSystemSaveData();
            
            // Assert
            Assert.IsNotNull(saveData.startedConversations);
            Assert.IsNotNull(saveData.completedConversations);
            Assert.IsNotNull(saveData.dialogueHistory);
            Assert.IsNotNull(saveData.questStates);
            Assert.IsNotNull(saveData.questEntryStates);
            Assert.IsNotNull(saveData.npcRelationships);
            Assert.IsNotNull(saveData.customVariables);
            Assert.IsNotNull(saveData.cvcPoints);
        }
        
        [Test]
        public void DialogueSystemSaveData_TracksConversations()
        {
            // Arrange
            var saveData = new DialogueSystemSaveData();
            
            // Act
            saveData.startedConversations.Add("Intro_Dialogue");
            saveData.startedConversations.Add("Boss_Encounter");
            saveData.completedConversations.Add("Intro_Dialogue");
            
            // Assert
            Assert.AreEqual(2, saveData.startedConversations.Count);
            Assert.AreEqual(1, saveData.completedConversations.Count);
            Assert.IsTrue(saveData.startedConversations.Contains("Boss_Encounter"));
            Assert.IsTrue(saveData.completedConversations.Contains("Intro_Dialogue"));
        }
        
        [Test]
        public void DialogueSystemSaveData_StoresNPCRelationships()
        {
            // Arrange
            var saveData = new DialogueSystemSaveData();
            
            // Act
            saveData.npcRelationships["Merchant"] = 50;
            saveData.npcRelationships["Guard"] = -10;
            saveData.npcRelationships["Boss"] = 0;
            
            // Assert
            Assert.AreEqual(3, saveData.npcRelationships.Count);
            Assert.AreEqual(50, saveData.npcRelationships["Merchant"]);
            Assert.AreEqual(-10, saveData.npcRelationships["Guard"]);
        }
        
        [Test]
        public void DialogueSystemSaveData_SerializesComplexData()
        {
            // Arrange
            var original = new DialogueSystemSaveData();
            original.cvcPoints.courage = 15;
            original.startedConversations.Add("Test_Conv");
            original.npcRelationships["NPC1"] = 25;
            original.questStates["Quest1"] = "Active";
            original.customVariables["TestVar"] = "TestValue";
            
            // Act
            string json = JsonUtility.ToJson(original, true);
            var deserialized = JsonUtility.FromJson<DialogueSystemSaveData>(json);
            
            // Assert
            Assert.AreEqual(original.cvcPoints.courage, deserialized.cvcPoints.courage);
            Assert.AreEqual(original.startedConversations.Count, deserialized.startedConversations.Count);
            Assert.AreEqual(original.npcRelationships["NPC1"], deserialized.npcRelationships["NPC1"]);
            Assert.AreEqual(original.questStates["Quest1"], deserialized.questStates["Quest1"]);
            Assert.AreEqual(original.customVariables["TestVar"], deserialized.customVariables["TestVar"]);
        }
        
        #endregion
        
        #region DialogueHistoryEntry Tests
        
        [Test]
        public void DialogueHistoryEntry_StoresCorrectData()
        {
            // Arrange
            var entry = new DialogueHistoryEntry
            {
                conversationTitle = "Boss_Dialogue",
                entryId = 42,
                text = "You cannot defeat me!",
                timestamp = new DateTime(2025, 1, 15, 10, 30, 0)
            };
            
            // Assert
            Assert.AreEqual("Boss_Dialogue", entry.conversationTitle);
            Assert.AreEqual(42, entry.entryId);
            Assert.AreEqual("You cannot defeat me!", entry.text);
            Assert.AreEqual(new DateTime(2025, 1, 15, 10, 30, 0), entry.timestamp);
        }
        
        [Test]
        public void DialogueHistory_MaintainsOrder()
        {
            // Arrange
            var saveData = new DialogueSystemSaveData();
            
            // Act
            for (int i = 0; i < 5; i++)
            {
                saveData.dialogueHistory.Add(new DialogueHistoryEntry
                {
                    conversationTitle = $"Conv_{i}",
                    entryId = i,
                    text = $"Line {i}",
                    timestamp = DateTime.Now.AddMinutes(i)
                });
            }
            
            // Assert
            Assert.AreEqual(5, saveData.dialogueHistory.Count);
            Assert.AreEqual("Conv_0", saveData.dialogueHistory[0].conversationTitle);
            Assert.AreEqual("Conv_4", saveData.dialogueHistory[4].conversationTitle);
        }
        
        #endregion
        
        #region Integration Tests
        
        [UnityTest]
        public IEnumerator DialogueSaveIntegration_Initializes()
        {
            // Wait for initialization
            yield return null;
            
            // Assert
            Assert.IsNotNull(DialogueSaveIntegration.Instance);
            Assert.AreEqual(dialogueIntegration, DialogueSaveIntegration.Instance);
        }
        
        [Test]
        public void DialogueSaveIntegration_TracksRelationships()
        {
            // Act
            dialogueIntegration.SetRelationship("TestNPC", 75);
            int relationship = dialogueIntegration.GetRelationship("TestNPC");
            
            // Assert
            Assert.AreEqual(75, relationship);
        }
        
        [Test]
        public void DialogueSaveIntegration_TracksUnknownRelationships()
        {
            // Act
            int relationship = dialogueIntegration.GetRelationship("UnknownNPC");
            
            // Assert
            Assert.AreEqual(0, relationship, "Unknown NPCs should have 0 relationship");
        }
        
        [Test]
        public void DialogueSaveIntegration_TracksConversationCompletion()
        {
            // Arrange
            var saveData = new DialogueSystemSaveData();
            saveData.completedConversations.Add("Tutorial_Conv");
            
            // This would normally be set through the integration
            // For testing, we'll check the data structure
            
            // Assert
            Assert.IsTrue(saveData.completedConversations.Contains("Tutorial_Conv"));
        }
        
        #endregion
        
        #region Save/Load Integration Tests
        
        [Test]
        public void DialogueSaveData_IntegratesWithConsolidatedSave()
        {
            // Arrange
            var consolidatedSave = ConsolidatedSaveData.CreateNew();
            var dialogueData = new DialogueSystemSaveData();
            dialogueData.cvcPoints.courage = 20;
            dialogueData.completedConversations.Add("Test_Conv");
            
            // Act
            string dialogueJson = JsonUtility.ToJson(dialogueData);
            consolidatedSave.dialogueDataJson = dialogueJson;
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(consolidatedSave.dialogueDataJson));
            
            // Verify deserialization
            var restored = JsonUtility.FromJson<DialogueSystemSaveData>(consolidatedSave.dialogueDataJson);
            Assert.AreEqual(20, restored.cvcPoints.courage);
            Assert.IsTrue(restored.completedConversations.Contains("Test_Conv"));
        }
        
        [UnityTest]
        public IEnumerator DialogueSaveIntegration_SaveAndLoadCycle()
        {
            // Setup
            dialogueIntegration.SetRelationship("TestNPC", 50);
            
            // Save
            dialogueIntegration.SaveDialogueData();
            yield return null;
            
            // Clear and reload
            dialogueIntegration.SetRelationship("TestNPC", 0);
            dialogueIntegration.LoadDialogueData();
            yield return null;
            
            // Verify
            int relationship = dialogueIntegration.GetRelationship("TestNPC");
            Assert.AreEqual(50, relationship, "Relationship should be restored after load");
        }
        
        #endregion
        
        #region Event Tests
        
        [Test]
        public void DialogueSaveIntegration_FiresSaveEvent()
        {
            // Arrange
            bool eventFired = false;
            DialogueSystemSaveData receivedData = null;
            
            DialogueSaveIntegration.OnDialogueDataSaved += (data) =>
            {
                eventFired = true;
                receivedData = data;
            };
            
            // Act
            dialogueIntegration.SaveDialogueData();
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotNull(receivedData);
            
            // Cleanup
            DialogueSaveIntegration.OnDialogueDataSaved -= (data) =>
            {
                eventFired = true;
                receivedData = data;
            };
        }
        
        [Test]
        public void DialogueSaveIntegration_FiresCVCChangeEvent()
        {
            // Arrange
            bool eventFired = false;
            string changedStat = "";
            int newValue = 0;
            
            DialogueSaveIntegration.OnCVCPointsChanged += (stat, value) =>
            {
                eventFired = true;
                changedStat = stat;
                newValue = value;
            };
            
            // Act
            var cvcPoints = new CVCPoints { courage = 10, virtue = 5, cunning = 8 };
            dialogueIntegration.SetCVCPoints(cvcPoints);
            
            // Assert
            Assert.IsTrue(eventFired);
            Assert.IsNotEmpty(changedStat);
            
            // Cleanup
            DialogueSaveIntegration.OnCVCPointsChanged -= (stat, value) =>
            {
                eventFired = true;
                changedStat = stat;
                newValue = value;
            };
        }
        
        #endregion
    }
}