using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Dialog.UI;
using NeonLadder.Dialog;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace NeonLadder.Tests.Dialog
{
    /// <summary>
    /// Unit tests for EnhancedDialogueUI with consequence preview system
    /// </summary>
    [TestFixture]
    public class EnhancedDialogueUITests
    {
        private GameObject testObject;
        private EnhancedDialogueUI dialogueUI;
        private GameObject dialoguePanel;
        private GameObject consequencePanel;
        
        [SetUp]
        public void Setup()
        {
            // Create test GameObject with EnhancedDialogueUI
            testObject = new GameObject("TestDialogueUI");
            dialogueUI = testObject.AddComponent<EnhancedDialogueUI>();
            
            // Create mock UI structure
            CreateMockUIStructure();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testObject != null)
            {
                Object.DestroyImmediate(testObject);
            }
            if (dialoguePanel != null)
            {
                Object.DestroyImmediate(dialoguePanel);
            }
            if (consequencePanel != null)
            {
                Object.DestroyImmediate(consequencePanel);
            }
        }
        
        private void CreateMockUIStructure()
        {
            // Create dialogue panel
            dialoguePanel = new GameObject("DialoguePanel");
            var dialogueText = new GameObject("DialogueText");
            dialogueText.AddComponent<TextMeshProUGUI>();
            dialogueText.transform.SetParent(dialoguePanel.transform);
            
            var speakerText = new GameObject("SpeakerText");
            speakerText.AddComponent<TextMeshProUGUI>();
            speakerText.transform.SetParent(dialoguePanel.transform);
            
            var responseContainer = new GameObject("ResponseContainer");
            responseContainer.transform.SetParent(dialoguePanel.transform);
            
            // Create consequence preview panel
            consequencePanel = new GameObject("ConsequencePanel");
            var consequenceText = new GameObject("ConsequenceText");
            consequenceText.AddComponent<TextMeshProUGUI>();
            consequenceText.transform.SetParent(consequencePanel.transform);
            
            // Create response button prefab
            var buttonPrefab = new GameObject("ResponseButtonPrefab");
            buttonPrefab.AddComponent<Button>();
            buttonPrefab.AddComponent<Image>();
            var buttonText = new GameObject("Text");
            buttonText.AddComponent<TextMeshProUGUI>();
            buttonText.transform.SetParent(buttonPrefab.transform);
            
            // Use reflection to set private serialized fields
            var type = typeof(EnhancedDialogueUI);
            
            var dialoguePanelField = type.GetField("dialoguePanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dialoguePanelField?.SetValue(dialogueUI, dialoguePanel);
            
            var consequencePanelField = type.GetField("consequencePreviewPanel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            consequencePanelField?.SetValue(dialogueUI, consequencePanel);
            
            var responseContainerField = type.GetField("responseButtonContainer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            responseContainerField?.SetValue(dialogueUI, responseContainer.transform);
            
            var buttonPrefabField = type.GetField("responseButtonPrefab", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            buttonPrefabField?.SetValue(dialogueUI, buttonPrefab);
            
            var dialogueTextField = type.GetField("dialogueText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            dialogueTextField?.SetValue(dialogueUI, dialogueText.GetComponent<TextMeshProUGUI>());
            
            var speakerTextField = type.GetField("speakerNameText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            speakerTextField?.SetValue(dialogueUI, speakerText.GetComponent<TextMeshProUGUI>());
            
            var consequenceTextField = type.GetField("consequenceText", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            consequenceTextField?.SetValue(dialogueUI, consequenceText.GetComponent<TextMeshProUGUI>());
        }
        
        [Test]
        public void DialogueUI_InitializesCorrectly()
        {
            Assert.IsNotNull(dialogueUI);
            Assert.IsFalse(dialoguePanel.activeSelf, "Dialogue panel should be inactive on start");
            Assert.IsFalse(consequencePanel.activeSelf, "Consequence panel should be inactive on start");
        }
        
        [Test]
        public void Open_ActivatesDialoguePanel()
        {
            dialogueUI.Open();
            Assert.IsTrue(dialoguePanel.activeSelf, "Dialogue panel should be active after Open()");
        }
        
        [Test]
        public void Close_DeactivatesPanels()
        {
            dialogueUI.Open();
            dialogueUI.Close();
            
            Assert.IsFalse(dialoguePanel.activeSelf, "Dialogue panel should be inactive after Close()");
            Assert.IsFalse(consequencePanel.activeSelf, "Consequence panel should be inactive after Close()");
        }
        
        [Test]
        public void ShowSubtitle_UpdatesDialogueText()
        {
            var subtitle = CreateMockSubtitle("Test Speaker", "Test dialogue text");
            
            dialogueUI.ShowSubtitle(subtitle);
            
            var speakerText = GetPrivateField<TextMeshProUGUI>(dialogueUI, "speakerNameText");
            var dialogueText = GetPrivateField<TextMeshProUGUI>(dialogueUI, "dialogueText");
            
            Assert.AreEqual("Test Speaker", speakerText.text);
            Assert.AreEqual("Test dialogue text", dialogueText.text);
        }
        
        [Test]
        public void ShowResponses_CreatesResponseButtons()
        {
            var responses = new Response[]
            {
                CreateMockResponse("Choice 1"),
                CreateMockResponse("Choice 2"),
                CreateMockResponse("Choice 3")
            };
            
            var subtitle = CreateMockSubtitle("Boss", "Choose your response");
            dialogueUI.ShowResponses(subtitle, responses, 30f);
            
            var responseContainer = GetPrivateField<Transform>(dialogueUI, "responseButtonContainer");
            Assert.AreEqual(3, responseContainer.childCount, "Should create 3 response buttons");
        }
        
        [Test]
        public void ConsequenceCalculator_ParsesCVCChanges()
        {
            var calculator = new ConsequenceCalculator();
            var response = CreateMockResponse("Brave choice", "AddCourage(5); AddVirtue(2);");
            
            var preview = calculator.CalculateConsequences(response);
            
            Assert.AreEqual(5, preview.cvcPointChanges.courage);
            Assert.AreEqual(2, preview.cvcPointChanges.virtue);
            Assert.AreEqual(0, preview.cvcPointChanges.cunning);
        }
        
        [Test]
        public void ConsequenceCalculator_DeterminesChoiceType()
        {
            var calculator = new ConsequenceCalculator();
            
            var courageResponse = CreateMockResponse("I'll fight you!");
            var virtueResponse = CreateMockResponse("Let me help you");
            var cunningResponse = CreateMockResponse("I'll trick them");
            
            var couragePreview = calculator.CalculateConsequences(courageResponse);
            var virtuePreview = calculator.CalculateConsequences(virtueResponse);
            var cunningPreview = calculator.CalculateConsequences(cunningResponse);
            
            Assert.AreEqual(ChoiceType.Courage, couragePreview.choiceType);
            Assert.AreEqual(ChoiceType.Virtue, virtuePreview.choiceType);
            Assert.AreEqual(ChoiceType.Cunning, cunningPreview.choiceType);
        }
        
        [Test]
        public void ConsequenceCalculator_ParsesCurrencyChanges()
        {
            var calculator = new ConsequenceCalculator();
            var response = CreateMockResponse("Buy item", "AddMetaCurrency(100); AddPermaCurrency(50);");
            
            var preview = calculator.CalculateConsequences(response);
            
            Assert.AreEqual(2, preview.currencyChanges.Count);
            Assert.AreEqual("Meta", preview.currencyChanges[0].currencyType);
            Assert.AreEqual(100, preview.currencyChanges[0].amount);
            Assert.AreEqual("Perma", preview.currencyChanges[1].currencyType);
            Assert.AreEqual(50, preview.currencyChanges[1].amount);
        }
        
        [Test]
        public void ConsequenceCalculator_DetectsUnlocks()
        {
            var calculator = new ConsequenceCalculator();
            var unlockResponse = CreateMockResponse("Secret path", "UnlockArea('SecretZone');");
            var normalResponse = CreateMockResponse("Normal choice", "AddCourage(1);");
            
            var unlockPreview = calculator.CalculateConsequences(unlockResponse);
            var normalPreview = calculator.CalculateConsequences(normalResponse);
            
            Assert.IsTrue(unlockPreview.unlocksContent);
            Assert.IsFalse(normalPreview.unlocksContent);
        }
        
        [Test]
        public void DialogueResponseButton_InitializesCorrectly()
        {
            var buttonObj = new GameObject("TestButton");
            var responseButton = buttonObj.AddComponent<DialogueResponseButton>();
            var response = CreateMockResponse("Test Choice");
            
            responseButton.Initialize(response, dialogueUI);
            
            Assert.AreEqual(response, responseButton.Response);
            Assert.AreEqual(dialogueUI, responseButton.ParentUI);
            
            Object.DestroyImmediate(buttonObj);
        }
        
        [Test]
        public void ConsequencePreview_InitializesWithDefaults()
        {
            var preview = new ConsequencePreview();
            
            Assert.AreEqual(ChoiceType.Neutral, preview.choiceType);
            Assert.IsTrue(preview.requirementsMet);
            Assert.AreEqual(1.0f, preview.successProbability);
            Assert.IsFalse(preview.unlocksContent);
            Assert.AreEqual(0, preview.cvcPointChanges.courage);
            Assert.AreEqual(0, preview.cvcPointChanges.virtue);
            Assert.AreEqual(0, preview.cvcPointChanges.cunning);
        }
        
        [Test]
        public void ShowAlert_LogsMessage()
        {
            LogAssert.Expect(LogType.Log, "[Dialog Alert] Test alert message");
            dialogueUI.ShowAlert("Test alert message", 2f);
        }
        
        [Test]
        public void HideResponses_ClearsAllButtons()
        {
            // First create some responses
            var responses = new Response[]
            {
                CreateMockResponse("Choice 1"),
                CreateMockResponse("Choice 2")
            };
            
            var subtitle = CreateMockSubtitle("Boss", "Choose");
            dialogueUI.ShowResponses(subtitle, responses, 30f);
            
            var responseContainer = GetPrivateField<Transform>(dialogueUI, "responseButtonContainer");
            Assert.AreEqual(2, responseContainer.childCount);
            
            // Now hide them
            dialogueUI.HideResponses();
            
            Assert.AreEqual(0, responseContainer.childCount, "All response buttons should be cleared");
        }
        
        // Helper methods
        
        private Subtitle CreateMockSubtitle(string speaker, string text)
        {
            var subtitle = new Subtitle(
                new CharacterInfo(1, speaker, null, CharacterType.NPC, null),
                new FormattedText(text),
                string.Empty,
                string.Empty,
                new DialogueEntry { id = 1, conversationID = 1 }
            );
            return subtitle;
        }
        
        private Response CreateMockResponse(string text, string script = "")
        {
            var entry = new DialogueEntry
            {
                id = 1,
                conversationID = 1,
                userScript = script
            };
            
            var response = new Response(
                new FormattedText(text),
                entry
            );
            
            return response;
        }
        
        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var type = obj.GetType();
            var field = type.GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field?.GetValue(obj);
        }
    }
}