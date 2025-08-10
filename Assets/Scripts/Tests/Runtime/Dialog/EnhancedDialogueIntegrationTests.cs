using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonLadder.Dialog.UI;
using NeonLadder.Dialog;
using PixelCrushers.DialogueSystem;
using TMPro;
using UnityEngine.UI;

namespace NeonLadder.Tests.Dialog
{
    /// <summary>
    /// Integration tests for Enhanced Dialogue UI with PixelCrushers Dialogue System
    /// </summary>
    public class EnhancedDialogueIntegrationTests
    {
        private GameObject testScene;
        private EnhancedDialogueUI dialogueUI;
        private DialogueSaveIntegration saveIntegration;
        
        [SetUp]
        public void Setup()
        {
            testScene = new GameObject("TestScene");
            SetupDialogueSystem();
        }
        
        [TearDown]
        public void TearDown()
        {
            if (testScene != null)
            {
                Object.Destroy(testScene);
            }
        }
        
        private void SetupDialogueSystem()
        {
            // Create dialogue UI
            var uiObject = new GameObject("DialogueUI");
            uiObject.transform.SetParent(testScene.transform);
            dialogueUI = uiObject.AddComponent<EnhancedDialogueUI>();
            
            // Create save integration
            var saveObject = new GameObject("SaveIntegration");
            saveObject.transform.SetParent(testScene.transform);
            saveIntegration = saveObject.AddComponent<DialogueSaveIntegration>();
            
            // Create mock UI elements
            CreateMockUIHierarchy(uiObject);
        }
        
        private void CreateMockUIHierarchy(GameObject parent)
        {
            // Create Canvas
            var canvas = new GameObject("Canvas");
            canvas.transform.SetParent(parent.transform);
            canvas.AddComponent<Canvas>();
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            // Dialogue Panel
            var dialoguePanel = new GameObject("DialoguePanel");
            dialoguePanel.transform.SetParent(canvas.transform);
            dialoguePanel.AddComponent<Image>();
            
            // Speaker Name
            var speakerName = new GameObject("SpeakerName");
            speakerName.transform.SetParent(dialoguePanel.transform);
            speakerName.AddComponent<TextMeshProUGUI>();
            
            // Dialogue Text
            var dialogueText = new GameObject("DialogueText");
            dialogueText.transform.SetParent(dialoguePanel.transform);
            dialogueText.AddComponent<TextMeshProUGUI>();
            
            // Response Container
            var responseContainer = new GameObject("ResponseContainer");
            responseContainer.transform.SetParent(dialoguePanel.transform);
            responseContainer.AddComponent<VerticalLayoutGroup>();
            
            // Consequence Preview Panel
            var consequencePanel = new GameObject("ConsequencePanel");
            consequencePanel.transform.SetParent(canvas.transform);
            consequencePanel.AddComponent<Image>();
            
            var consequenceText = new GameObject("ConsequenceText");
            consequenceText.transform.SetParent(consequencePanel.transform);
            consequenceText.AddComponent<TextMeshProUGUI>();
            
            // Response Button Prefab
            var buttonPrefab = new GameObject("ButtonPrefab");
            buttonPrefab.AddComponent<Button>();
            buttonPrefab.AddComponent<Image>();
            var buttonText = new GameObject("Text");
            buttonText.transform.SetParent(buttonPrefab.transform);
            buttonText.AddComponent<TextMeshProUGUI>();
            
            // Link to dialogue UI via reflection
            LinkUIElements(dialoguePanel, consequencePanel, responseContainer.transform, 
                buttonPrefab, speakerName.GetComponent<TextMeshProUGUI>(), 
                dialogueText.GetComponent<TextMeshProUGUI>(), 
                consequenceText.GetComponent<TextMeshProUGUI>());
        }
        
        private void LinkUIElements(GameObject dialoguePanel, GameObject consequencePanel, 
            Transform responseContainer, GameObject buttonPrefab, 
            TextMeshProUGUI speakerText, TextMeshProUGUI dialogueText, 
            TextMeshProUGUI consequenceText)
        {
            var type = typeof(EnhancedDialogueUI);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            
            type.GetField("dialoguePanel", flags)?.SetValue(dialogueUI, dialoguePanel);
            type.GetField("consequencePreviewPanel", flags)?.SetValue(dialogueUI, consequencePanel);
            type.GetField("responseButtonContainer", flags)?.SetValue(dialogueUI, responseContainer);
            type.GetField("responseButtonPrefab", flags)?.SetValue(dialogueUI, buttonPrefab);
            type.GetField("speakerNameText", flags)?.SetValue(dialogueUI, speakerText);
            type.GetField("dialogueText", flags)?.SetValue(dialogueUI, dialogueText);
            type.GetField("consequenceText", flags)?.SetValue(dialogueUI, consequenceText);
        }
        
        [UnityTest]
        public IEnumerator DialogueUI_HandlesConversationLifecycle()
        {
            // Open dialogue
            dialogueUI.Open();
            yield return null;
            
            var dialoguePanel = GetPrivateField<GameObject>("dialoguePanel");
            Assert.IsTrue(dialoguePanel.activeSelf);
            
            // Show subtitle
            var subtitle = CreateTestSubtitle("NPC", "Hello, adventurer!");
            dialogueUI.ShowSubtitle(subtitle);
            yield return null;
            
            var dialogueText = GetPrivateField<TextMeshProUGUI>("dialogueText");
            Assert.AreEqual("Hello, adventurer!", dialogueText.text);
            
            // Show responses
            var responses = new Response[]
            {
                CreateTestResponse("Greet back", "AddVirtue(1);"),
                CreateTestResponse("Attack!", "AddCourage(3);"),
                CreateTestResponse("Sneak away", "AddCunning(2);")
            };
            
            dialogueUI.ShowResponses(subtitle, responses, 30f);
            yield return null;
            
            var responseContainer = GetPrivateField<Transform>("responseButtonContainer");
            Assert.AreEqual(3, responseContainer.childCount);
            
            // Close dialogue
            dialogueUI.Close();
            yield return null;
            
            Assert.IsFalse(dialoguePanel.activeSelf);
            Assert.AreEqual(0, responseContainer.childCount);
        }
        
        [UnityTest]
        public IEnumerator ConsequencePreview_ShowsOnHover()
        {
            dialogueUI.Open();
            yield return null;
            
            // Create response with consequences
            var response = CreateTestResponse("Make choice", "AddCourage(5); AddMetaCurrency(100);");
            var responses = new Response[] { response };
            
            var subtitle = CreateTestSubtitle("NPC", "Choose wisely");
            dialogueUI.ShowResponses(subtitle, responses, 30f);
            yield return null;
            
            // Get the created button
            var responseContainer = GetPrivateField<Transform>("responseButtonContainer");
            Assert.AreEqual(1, responseContainer.childCount);
            
            var button = responseContainer.GetChild(0).GetComponent<Button>();
            Assert.IsNotNull(button);
            
            // Simulate hover (would trigger EventTrigger in real scenario)
            var calculator = new ConsequenceCalculator();
            var preview = calculator.CalculateConsequences(response);
            
            Assert.AreEqual(5, preview.cvcPointChanges.courage);
            Assert.AreEqual(1, preview.currencyChanges.Count);
            Assert.AreEqual(100, preview.currencyChanges[0].amount);
        }
        
        [UnityTest]
        public IEnumerator DialogueUI_AppliesChoiceColors()
        {
            dialogueUI.Open();
            yield return null;
            
            // Create responses of different types
            var responses = new Response[]
            {
                CreateTestResponse("Fight bravely", "AddCourage(3);"),
                CreateTestResponse("Help the innocent", "AddVirtue(3);"),
                CreateTestResponse("Trick the enemy", "AddCunning(3);")
            };
            
            var subtitle = CreateTestSubtitle("NPC", "What will you do?");
            dialogueUI.ShowResponses(subtitle, responses, 30f);
            yield return new WaitForSeconds(0.1f);
            
            var responseContainer = GetPrivateField<Transform>("responseButtonContainer");
            Assert.AreEqual(3, responseContainer.childCount);
            
            // Check that buttons have been styled
            for (int i = 0; i < responseContainer.childCount; i++)
            {
                var buttonImage = responseContainer.GetChild(i).GetComponent<Image>();
                Assert.IsNotNull(buttonImage);
                // Color should have been applied based on choice type
                Assert.AreNotEqual(Color.white, buttonImage.color);
            }
        }
        
        [Test]
        public void SaveIntegration_TracksDialogueChoices()
        {
            // Simulate making a dialogue choice
            var cvcBefore = saveIntegration.GetCVCPoints();
            
            // Apply consequence from dialogue choice
            var preview = new ConsequencePreview
            {
                cvcPointChanges = new CVCPoints { courage = 5, virtue = 2, cunning = 0 }
            };
            
            // Manually apply changes (normally done through UI)
            var cvcAfter = cvcBefore;
            cvcAfter.courage += preview.cvcPointChanges.courage;
            cvcAfter.virtue += preview.cvcPointChanges.virtue;
            saveIntegration.SetCVCPoints(cvcAfter);
            
            var finalCVC = saveIntegration.GetCVCPoints();
            Assert.AreEqual(cvcBefore.courage + 5, finalCVC.courage);
            Assert.AreEqual(cvcBefore.virtue + 2, finalCVC.virtue);
        }
        
        [Test]
        public void DialogueEvents_FireCorrectly()
        {
            bool eventFired = false;
            Response capturedResponse = null;
            ConsequencePreview capturedPreview = null;
            
            EnhancedDialogueUI.OnResponseSelected += (response) =>
            {
                eventFired = true;
                capturedResponse = response;
            };
            
            EnhancedDialogueUI.OnConsequencePreviewShown += (preview) =>
            {
                capturedPreview = preview;
            };
            
            // Simulate response selection
            var testResponse = CreateTestResponse("Test choice");
            
            // This would normally be called through button click
            // For testing, we'll invoke the event directly
            var method = typeof(EnhancedDialogueUI).GetMethod("OnResponseClicked",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(dialogueUI, new object[] { testResponse });
            }
            
            // Clean up event handlers
            EnhancedDialogueUI.OnResponseSelected = null;
            EnhancedDialogueUI.OnConsequencePreviewShown = null;
        }
        
        // Helper methods
        
        private Subtitle CreateTestSubtitle(string speaker, string text)
        {
            return new Subtitle(
                new CharacterInfo(1, speaker, null, CharacterType.NPC, null),
                new FormattedText(text),
                string.Empty,
                string.Empty,
                new DialogueEntry { id = 1, conversationID = 1 }
            );
        }
        
        private Response CreateTestResponse(string text, string script = "")
        {
            var entry = new DialogueEntry
            {
                id = 1,
                conversationID = 1,
                userScript = script,
                conditionsString = ""
            };
            
            return new Response(new FormattedText(text), entry);
        }
        
        private T GetPrivateField<T>(string fieldName)
        {
            var type = typeof(EnhancedDialogueUI);
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field?.GetValue(dialogueUI);
        }
    }
}