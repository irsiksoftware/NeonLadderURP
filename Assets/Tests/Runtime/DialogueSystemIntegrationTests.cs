using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.Collections.Generic;
using NeonLadder.Dialog;

namespace NeonLadder.Tests.Runtime
{
    /// <summary>
    /// Unit tests for the Dialogue System Integration
    /// Tests database management, conversation flow, and system integrations
    /// </summary>
    [TestFixture]
    public class DialogueSystemIntegrationTests
    {
        private DialogueSystemIntegration integration;
        private GameObject testGameObject;
        
        [SetUp]
        public void Setup()
        {
            // Create test GameObject with integration component
            testGameObject = new GameObject("TestDialogueIntegration");
            integration = testGameObject.AddComponent<DialogueSystemIntegration>();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testGameObject != null)
            {
                Object.DestroyImmediate(testGameObject);
            }
        }
        
        [Test]
        public void DialogueSystemIntegration_SingletonWorks()
        {
            // Act
            var instance1 = DialogueSystemIntegration.Instance;
            var instance2 = DialogueSystemIntegration.Instance;
            
            // Assert
            Assert.IsNotNull(instance1, "First instance should not be null");
            Assert.IsNotNull(instance2, "Second instance should not be null");
            Assert.AreEqual(instance1, instance2, "Both instances should be the same");
        }
        
        [Test]
        public void DialogueSystemIntegration_VariableSystemWorks()
        {
            // Arrange
            string varName = "TestVariable";
            int testValue = 42;
            
            // Act
            integration.SetVariable(varName, testValue);
            int retrievedValue = integration.GetVariable<int>(varName);
            
            // Assert
            Assert.AreEqual(testValue, retrievedValue, "Variable should store and retrieve correct value");
        }
        
        [Test]
        public void DialogueSystemIntegration_VariableSystemHandlesTypes()
        {
            // Test different variable types
            integration.SetVariable("IntVar", 100);
            integration.SetVariable("StringVar", "Hello");
            integration.SetVariable("FloatVar", 3.14f);
            integration.SetVariable("BoolVar", true);
            
            // Assert
            Assert.AreEqual(100, integration.GetVariable<int>("IntVar"));
            Assert.AreEqual("Hello", integration.GetVariable<string>("StringVar"));
            Assert.AreEqual(3.14f, integration.GetVariable<float>("FloatVar"), 0.01f);
            Assert.AreEqual(true, integration.GetVariable<bool>("BoolVar"));
        }
        
        [Test]
        public void DialogueSystemIntegration_HasVariableWorks()
        {
            // Arrange
            integration.SetVariable("ExistingVar", "test");
            
            // Act & Assert
            Assert.IsTrue(integration.HasVariable("ExistingVar"), "Should return true for existing variable");
            Assert.IsFalse(integration.HasVariable("NonExistentVar"), "Should return false for non-existent variable");
        }
        
        [Test]
        public void DialogueSystemIntegration_VariableDefaultValueWorks()
        {
            // Act
            int defaultInt = integration.GetVariable<int>("NonExistent", 999);
            string defaultString = integration.GetVariable<string>("NonExistent", "default");
            
            // Assert
            Assert.AreEqual(999, defaultInt, "Should return default value for non-existent int variable");
            Assert.AreEqual("default", defaultString, "Should return default value for non-existent string variable");
        }
        
        [Test]
        public void DialogueSystemIntegration_EventSystemWorks()
        {
            // Act & Assert - Should not throw
            Assert.DoesNotThrow(() => integration.TriggerEvent("TestEvent"), 
                "Triggering event should not throw exception");
        }
        
        [Test]
        public void DialogueSystemIntegration_ConversationManagementBasics()
        {
            // Test conversation state queries
            Assert.IsFalse(integration.IsConversationActive("NonExistent"), 
                "Non-existent conversation should not be active");
            
            var activeConversations = integration.GetActiveConversations();
            Assert.IsNotNull(activeConversations, "Active conversations list should not be null");
            Assert.AreEqual(0, activeConversations.Count, "Should start with no active conversations");
        }
        
        [Test]
        public void DialogueSystemIntegration_SaveDataGeneration()
        {
            // Arrange
            integration.SetVariable("SaveTest1", 100);
            integration.SetVariable("SaveTest2", "TestString");
            
            // Act
            var saveData = integration.GetSaveData();
            
            // Assert
            Assert.IsNotNull(saveData, "Save data should not be null");
            Assert.IsNotNull(saveData.Variables, "Variables dictionary should not be null");
            Assert.AreEqual(2, saveData.Variables.Count, "Should have 2 variables in save data");
            Assert.AreEqual(100, saveData.Variables["SaveTest1"]);
            Assert.AreEqual("TestString", saveData.Variables["SaveTest2"]);
        }
        
        [Test]
        public void DialogueSystemIntegration_LoadSaveData()
        {
            // Arrange
            var saveData = new DialogueSaveData
            {
                Variables = new Dictionary<string, object>
                {
                    { "LoadedVar1", 500 },
                    { "LoadedVar2", "LoadedString" }
                },
                ConversationStates = new List<ConversationState>(),
                CompletedConversations = new List<string>()
            };
            
            // Act
            integration.LoadSaveData(saveData);
            
            // Assert
            Assert.AreEqual(500, integration.GetVariable<int>("LoadedVar1"));
            Assert.AreEqual("LoadedString", integration.GetVariable<string>("LoadedVar2"));
        }
        
        [Test]
        public void DialogueSystemIntegration_AnalyticsReportGeneration()
        {
            // Act
            var report = integration.GetAnalyticsReport();
            
            // Assert
            Assert.IsNotNull(report, "Analytics report should not be null");
        }
        
        [Test]
        public void DialogueSystemIntegration_ClearAllConversations()
        {
            // Act & Assert - Should not throw even with no active conversations
            Assert.DoesNotThrow(() => integration.ClearAllConversations(), 
                "Clearing conversations should not throw");
        }
        
        [Test]
        public void DialogueSystemIntegration_DatabaseCreation()
        {
            // Act
            integration.CreateDatabase("TestDatabase");
            
            // Assert - Should not throw
            Assert.Pass("Database creation executed without errors");
        }
        
        [Test]
        public void DialogueSystemIntegration_EventSubscription()
        {
            // Arrange
            bool eventFired = false;
            string receivedData = null;
            
            DialogueSystemIntegration.OnVariableChanged += (name, value) =>
            {
                eventFired = true;
                receivedData = name;
            };
            
            // Act
            integration.SetVariable("EventTest", 123);
            
            // Assert
            Assert.IsTrue(eventFired, "Variable changed event should fire");
            Assert.AreEqual("EventTest", receivedData, "Event should pass correct variable name");
            
            // Cleanup
            DialogueSystemIntegration.OnVariableChanged = null;
        }
        
        [UnityTest]
        public IEnumerator DialogueSystemIntegration_PersistsThroughSceneChange()
        {
            // Arrange
            var instance = DialogueSystemIntegration.Instance;
            instance.SetVariable("PersistenceTest", "ShouldPersist");
            
            // Wait a frame
            yield return null;
            
            // Act - Get instance again (simulating scene change)
            var newInstance = DialogueSystemIntegration.Instance;
            
            // Assert
            Assert.AreEqual(instance, newInstance, "Instance should persist");
            Assert.AreEqual("ShouldPersist", newInstance.GetVariable<string>("PersistenceTest"), 
                "Variables should persist");
        }
        
        [Test]
        public void DialogueDatabase_CanBeCreated()
        {
            // Act
            var database = new DialogueDatabase("TestDB");
            
            // Assert
            Assert.IsNotNull(database, "Database should be created");
            Assert.AreEqual("TestDB", database.Name, "Database name should match");
        }
        
        [Test]
        public void DialogueChoice_HasCorrectStructure()
        {
            // Arrange
            var choice = new DialogueChoice
            {
                Text = "Test Choice",
                NextNodeId = "Node2",
                EndsConversation = false,
                Conditions = new List<DialogueCondition>(),
                Consequences = new List<DialogueConsequence>()
            };
            
            // Assert
            Assert.AreEqual("Test Choice", choice.Text);
            Assert.AreEqual("Node2", choice.NextNodeId);
            Assert.IsFalse(choice.EndsConversation);
            Assert.IsNotNull(choice.Conditions);
            Assert.IsNotNull(choice.Consequences);
        }
        
        [Test]
        public void DialogueCondition_TypesAreDefined()
        {
            // Assert - Check that all condition types are defined
            Assert.IsDefined(typeof(ConditionType), ConditionType.Variable);
            Assert.IsDefined(typeof(ConditionType), ConditionType.Currency);
            Assert.IsDefined(typeof(ConditionType), ConditionType.Progression);
            Assert.IsDefined(typeof(ConditionType), ConditionType.Skill);
        }
        
        [Test]
        public void DialogueConsequence_TypesAreDefined()
        {
            // Assert - Check that all consequence types are defined
            Assert.IsDefined(typeof(ConsequenceType), ConsequenceType.Variable);
            Assert.IsDefined(typeof(ConsequenceType), ConsequenceType.Currency);
            Assert.IsDefined(typeof(ConsequenceType), ConsequenceType.Progression);
            Assert.IsDefined(typeof(ConsequenceType), ConsequenceType.Trigger);
        }
    }
}