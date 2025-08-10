using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PixelCrushers.DialogueSystem;
using NeonLadder.Dialog;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog.UI
{
    /// <summary>
    /// Enhanced dialogue UI with consequence preview system.
    /// Shows potential outcomes, requirements, and impacts before player makes choices.
    /// Inspired by Disco Elysium's thoughtful choice presentation.
    /// </summary>
    public class EnhancedDialogueUI : MonoBehaviour, IDialogueUI
    {
        #region Configuration
        
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Transform responseButtonContainer;
        [SerializeField] private GameObject responseButtonPrefab;
        
        [Header("Consequence Preview")]
        [SerializeField] private GameObject consequencePreviewPanel;
        [SerializeField] private TextMeshProUGUI consequenceText;
        [SerializeField] private Transform consequenceIconContainer;
        [SerializeField] private float previewDelay = 0.3f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color positiveColor = new Color(0.3f, 0.8f, 0.3f);
        [SerializeField] private Color negativeColor = new Color(0.8f, 0.3f, 0.3f);
        [SerializeField] private Color neutralColor = new Color(0.7f, 0.7f, 0.7f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f);
        
        [Header("Choice Type Colors")]
        [SerializeField] private Color courageColor = new Color(0.9f, 0.3f, 0.2f);
        [SerializeField] private Color virtueColor = new Color(0.2f, 0.6f, 0.9f);
        [SerializeField] private Color cunningColor = new Color(0.7f, 0.5f, 0.9f);
        
        [Header("Icons")]
        [SerializeField] private Sprite metaCurrencyIcon;
        [SerializeField] private Sprite permaCurrencyIcon;
        [SerializeField] private Sprite relationshipIcon;
        [SerializeField] private Sprite unlockIcon;
        [SerializeField] private Sprite warningIcon;
        
        #endregion
        
        #region Private Fields
        
        private List<DialogueResponseButton> activeButtons = new List<DialogueResponseButton>();
        private ConsequenceCalculator consequenceCalculator;
        private DialogueSaveIntegration saveIntegration;
        private Response[] currentResponses;
        private Transform currentActor;
        private bool isShowingPreview = false;
        
        #endregion
        
        #region Events
        
        public static event Action<Response, ConsequencePreview> OnResponseHovered;
        public static event Action<Response> OnResponseSelected;
        public static event Action<ConsequencePreview> OnConsequencePreviewShown;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            consequenceCalculator = new ConsequenceCalculator();
            saveIntegration = DialogueSaveIntegration.Instance;
            
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
                
            if (consequencePreviewPanel != null)
                consequencePreviewPanel.SetActive(false);
        }
        
        private void Start()
        {
            // Register as the dialogue UI
            var dialogueManager = DialogueManager.instance;
            if (dialogueManager != null)
            {
                dialogueManager.dialogueUI = this;
            }
        }
        
        #endregion
        
        #region IDialogueUI Implementation
        
        public void Open()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
        }
        
        public void Close()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
                
            HideConsequencePreview();
            ClearResponses();
        }
        
        public void ShowSubtitle(Subtitle subtitle)
        {
            if (subtitle == null) return;
            
            // Update speaker name
            if (speakerNameText != null)
            {
                speakerNameText.text = subtitle.speakerInfo?.Name ?? "Unknown";
            }
            
            // Update dialogue text
            if (dialogueText != null)
            {
                dialogueText.text = subtitle.formattedText.text;
            }
            
            currentActor = subtitle.speakerInfo?.transform;
        }
        
        public void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
        {
            currentResponses = responses;
            ClearResponses();
            
            foreach (var response in responses)
            {
                CreateResponseButton(response);
            }
        }
        
        public void HideResponses()
        {
            ClearResponses();
        }
        
        public void ShowQTEIndicator(int index)
        {
            // Quick Time Event indicator (if needed)
        }
        
        public void HideQTEIndicator(int index)
        {
            // Hide QTE indicator
        }
        
        public void ShowAlert(string message, float duration)
        {
            Debugger.Log($"[Dialog Alert] {message}");
        }
        
        public void HideAlert()
        {
            // Hide alert if shown
        }
        
        public void ShowContinueButton(Subtitle subtitle)
        {
            // Show continue button for non-choice dialogues
        }
        
        public void HideContinueButton()
        {
            // Hide continue button
        }
        
        public void SetPCPortrait(Texture2D portrait, string portraitName)
        {
            // Set player character portrait if used
        }
        
        public void SetPCName(string name)
        {
            // Set player character name if displayed
        }
        
        public void SetActorPortraitTexture(string actorName, Texture2D portrait)
        {
            // Set NPC portrait if used
        }
        
        #endregion
        
        #region Response Management
        
        private void CreateResponseButton(Response response)
        {
            if (responseButtonPrefab == null || responseButtonContainer == null)
                return;
            
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            var responseButton = buttonObj.GetComponent<DialogueResponseButton>();
            
            if (responseButton == null)
            {
                responseButton = buttonObj.AddComponent<DialogueResponseButton>();
            }
            
            // Configure button
            responseButton.Initialize(response, this);
            
            // Set button text
            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = response.formattedText.text;
            }
            
            // Calculate and apply visual styling
            var preview = consequenceCalculator.CalculateConsequences(response);
            ApplyButtonStyling(responseButton, preview);
            
            // Set up hover events
            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnResponseClicked(response));
                
                // Add hover detection
                var eventTrigger = buttonObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((data) => OnResponseHover(response, preview));
                eventTrigger.triggers.Add(pointerEnter);
                
                var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((data) => HideConsequencePreview());
                eventTrigger.triggers.Add(pointerExit);
            }
            
            activeButtons.Add(responseButton);
        }
        
        private void ApplyButtonStyling(DialogueResponseButton button, ConsequencePreview preview)
        {
            if (button == null) return;
            
            var image = button.GetComponent<Image>();
            if (image == null) return;
            
            // Color based on primary choice type
            if (preview.choiceType == ChoiceType.Courage)
            {
                image.color = courageColor;
            }
            else if (preview.choiceType == ChoiceType.Virtue)
            {
                image.color = virtueColor;
            }
            else if (preview.choiceType == ChoiceType.Cunning)
            {
                image.color = cunningColor;
            }
            else
            {
                image.color = neutralColor;
            }
            
            // Apply locked state if requirements not met
            if (!preview.requirementsMet)
            {
                image.color = lockedColor;
                button.GetComponent<Button>().interactable = false;
            }
            
            // Add visual indicators for special choices
            AddChoiceIndicators(button, preview);
        }
        
        private void AddChoiceIndicators(DialogueResponseButton button, ConsequencePreview preview)
        {
            // Add small icons to indicate consequence types
            if (preview.currencyChanges.Count > 0)
            {
                AddIndicatorIcon(button, preview.currencyChanges.Any(c => c.amount > 0) ? metaCurrencyIcon : warningIcon);
            }
            
            if (preview.relationshipChanges.Count > 0)
            {
                AddIndicatorIcon(button, relationshipIcon);
            }
            
            if (preview.unlocksContent)
            {
                AddIndicatorIcon(button, unlockIcon);
            }
        }
        
        private void AddIndicatorIcon(DialogueResponseButton button, Sprite icon)
        {
            if (icon == null) return;
            
            // Implementation would add small icon to button
            // This is simplified - actual implementation would position icons properly
        }
        
        private void ClearResponses()
        {
            foreach (var button in activeButtons)
            {
                if (button != null && button.gameObject != null)
                {
                    Destroy(button.gameObject);
                }
            }
            activeButtons.Clear();
        }
        
        #endregion
        
        #region Consequence Preview
        
        private void OnResponseHover(Response response, ConsequencePreview preview)
        {
            if (preview == null) return;
            
            isShowingPreview = true;
            ShowConsequencePreview(preview);
            
            OnResponseHovered?.Invoke(response, preview);
        }
        
        private void ShowConsequencePreview(ConsequencePreview preview)
        {
            if (consequencePreviewPanel == null) return;
            
            consequencePreviewPanel.SetActive(true);
            
            // Build consequence text
            var sb = new System.Text.StringBuilder();
            
            // Show requirements if not met
            if (!preview.requirementsMet)
            {
                sb.AppendLine($"<color=#{ColorUtility.ToHtmlStringRGB(lockedColor)}>Requirements not met:</color>");
                foreach (var req in preview.requirements)
                {
                    sb.AppendLine($"  • {req}");
                }
                sb.AppendLine();
            }
            
            // Show CVC points impact
            if (preview.cvcPointChanges.courage != 0 || preview.cvcPointChanges.virtue != 0 || preview.cvcPointChanges.cunning != 0)
            {
                sb.AppendLine("<b>Character Impact:</b>");
                if (preview.cvcPointChanges.courage != 0)
                    sb.AppendLine($"  Courage {FormatChange(preview.cvcPointChanges.courage)}");
                if (preview.cvcPointChanges.virtue != 0)
                    sb.AppendLine($"  Virtue {FormatChange(preview.cvcPointChanges.virtue)}");
                if (preview.cvcPointChanges.cunning != 0)
                    sb.AppendLine($"  Cunning {FormatChange(preview.cvcPointChanges.cunning)}");
                sb.AppendLine();
            }
            
            // Show currency changes
            if (preview.currencyChanges.Count > 0)
            {
                sb.AppendLine("<b>Currency Impact:</b>");
                foreach (var change in preview.currencyChanges)
                {
                    var color = change.amount > 0 ? positiveColor : negativeColor;
                    sb.AppendLine($"  <color=#{ColorUtility.ToHtmlStringRGB(color)}>{change.currencyType}: {FormatChange(change.amount)}</color>");
                }
                sb.AppendLine();
            }
            
            // Show relationship changes
            if (preview.relationshipChanges.Count > 0)
            {
                sb.AppendLine("<b>Relationship Impact:</b>");
                foreach (var change in preview.relationshipChanges)
                {
                    var color = change.change > 0 ? positiveColor : negativeColor;
                    sb.AppendLine($"  <color=#{ColorUtility.ToHtmlStringRGB(color)}>{change.npcName}: {FormatChange(change.change)}</color>");
                }
                sb.AppendLine();
            }
            
            // Show success probability if applicable
            if (preview.successProbability < 1.0f)
            {
                sb.AppendLine($"<b>Success Chance:</b> {preview.successProbability:P0}");
                sb.AppendLine();
            }
            
            // Show story hint
            if (!string.IsNullOrEmpty(preview.storyHint))
            {
                sb.AppendLine($"<i>{preview.storyHint}</i>");
            }
            
            if (consequenceText != null)
            {
                consequenceText.text = sb.ToString();
            }
            
            OnConsequencePreviewShown?.Invoke(preview);
        }
        
        private void HideConsequencePreview()
        {
            isShowingPreview = false;
            
            if (consequencePreviewPanel != null)
            {
                consequencePreviewPanel.SetActive(false);
            }
        }
        
        private string FormatChange(int value)
        {
            return value > 0 ? $"+{value}" : value.ToString();
        }
        
        #endregion
        
        #region Response Handling
        
        private void OnResponseClicked(Response response)
        {
            HideConsequencePreview();
            
            // Apply consequences
            var preview = consequenceCalculator.CalculateConsequences(response);
            ApplyConsequences(preview);
            
            // Notify listeners
            OnResponseSelected?.Invoke(response);
            
            // Send response to dialogue system
            DialogueManager.instance.OnConversationResponseMenu(response);
        }
        
        private void ApplyConsequences(ConsequencePreview preview)
        {
            if (preview == null) return;
            
            // Apply CVC changes
            if (saveIntegration != null)
            {
                var currentCVC = saveIntegration.GetCVCPoints();
                currentCVC.courage += preview.cvcPointChanges.courage;
                currentCVC.virtue += preview.cvcPointChanges.virtue;
                currentCVC.cunning += preview.cvcPointChanges.cunning;
                saveIntegration.SetCVCPoints(currentCVC);
            }
            
            // Apply currency changes
            foreach (var change in preview.currencyChanges)
            {
                ApplyCurrencyChange(change);
            }
            
            // Apply relationship changes
            foreach (var change in preview.relationshipChanges)
            {
                if (saveIntegration != null)
                {
                    int current = saveIntegration.GetRelationship(change.npcName);
                    saveIntegration.SetRelationship(change.npcName, current + change.change);
                }
            }
            
            // Show feedback
            ShowConsequenceFeedback(preview);
        }
        
        private void ApplyCurrencyChange(CurrencyChange change)
        {
            // This would integrate with your currency system
            // For now, we'll just log it
            Debugger.Log($"[Dialog] Currency change: {change.currencyType} {change.amount:+#;-#;0}");
        }
        
        private void ShowConsequenceFeedback(ConsequencePreview preview)
        {
            // Show visual feedback for applied consequences
            // This could be floating text, UI animations, etc.
        }
        
        #endregion
    }
    
    #region Support Classes
    
    /// <summary>
    /// Individual response button component
    /// </summary>
    public class DialogueResponseButton : MonoBehaviour
    {
        public Response Response { get; private set; }
        public EnhancedDialogueUI ParentUI { get; private set; }
        
        public void Initialize(Response response, EnhancedDialogueUI parentUI)
        {
            Response = response;
            ParentUI = parentUI;
        }
    }
    
    /// <summary>
    /// Calculates consequences for dialogue choices
    /// </summary>
    public class ConsequenceCalculator
    {
        public ConsequencePreview CalculateConsequences(Response response)
        {
            var preview = new ConsequencePreview();
            
            if (response == null || response.destinationEntry == null)
                return preview;
            
            // Parse response for consequence data
            var entry = response.destinationEntry;
            
            // Check for CVC changes in script
            preview.cvcPointChanges = ParseCVCChanges(entry.userScript);
            
            // Determine choice type from tags or content
            preview.choiceType = DetermineChoiceType(response);
            
            // Check requirements
            preview.requirementsMet = CheckRequirements(entry.conditionsString);
            if (!preview.requirementsMet)
            {
                preview.requirements = ParseRequirements(entry.conditionsString);
            }
            
            // Parse currency changes
            preview.currencyChanges = ParseCurrencyChanges(entry.userScript);
            
            // Parse relationship changes
            preview.relationshipChanges = ParseRelationshipChanges(entry.userScript);
            
            // Check for unlocks
            preview.unlocksContent = CheckForUnlocks(entry.userScript);
            
            // Calculate success probability if skill check
            preview.successProbability = CalculateSuccessProbability(entry);
            
            // Generate story hint
            preview.storyHint = GenerateStoryHint(entry);
            
            return preview;
        }
        
        private CVCPoints ParseCVCChanges(string script)
        {
            var points = new CVCPoints();
            
            if (string.IsNullOrEmpty(script))
                return points;
            
            // Parse AddCourage, AddVirtue, AddCunning calls
            if (script.Contains("AddCourage"))
            {
                points.courage = ExtractValue(script, "AddCourage");
            }
            if (script.Contains("AddVirtue"))
            {
                points.virtue = ExtractValue(script, "AddVirtue");
            }
            if (script.Contains("AddCunning"))
            {
                points.cunning = ExtractValue(script, "AddCunning");
            }
            
            return points;
        }
        
        private ChoiceType DetermineChoiceType(Response response)
        {
            var text = response.formattedText.text.ToLower();
            
            if (text.Contains("fight") || text.Contains("attack") || text.Contains("brave"))
                return ChoiceType.Courage;
            
            if (text.Contains("help") || text.Contains("kind") || text.Contains("honest"))
                return ChoiceType.Virtue;
            
            if (text.Contains("trick") || text.Contains("clever") || text.Contains("sneak"))
                return ChoiceType.Cunning;
            
            return ChoiceType.Neutral;
        }
        
        private bool CheckRequirements(string conditions)
        {
            if (string.IsNullOrEmpty(conditions))
                return true;
            
            // This would actually evaluate Lua conditions
            // For now, we'll assume all requirements are met
            return true;
        }
        
        private List<string> ParseRequirements(string conditions)
        {
            var requirements = new List<string>();
            
            // Parse condition string for human-readable requirements
            // This is simplified - actual implementation would parse Lua
            
            return requirements;
        }
        
        private List<CurrencyChange> ParseCurrencyChanges(string script)
        {
            var changes = new List<CurrencyChange>();
            
            if (string.IsNullOrEmpty(script))
                return changes;
            
            // Parse currency modification calls
            if (script.Contains("AddMetaCurrency"))
            {
                changes.Add(new CurrencyChange
                {
                    currencyType = "Meta",
                    amount = ExtractValue(script, "AddMetaCurrency")
                });
            }
            
            if (script.Contains("AddPermaCurrency"))
            {
                changes.Add(new CurrencyChange
                {
                    currencyType = "Perma",
                    amount = ExtractValue(script, "AddPermaCurrency")
                });
            }
            
            return changes;
        }
        
        private List<RelationshipChange> ParseRelationshipChanges(string script)
        {
            var changes = new List<RelationshipChange>();
            
            // Parse relationship modification calls
            // This is simplified - actual implementation would parse Lua
            
            return changes;
        }
        
        private bool CheckForUnlocks(string script)
        {
            if (string.IsNullOrEmpty(script))
                return false;
            
            return script.Contains("Unlock") || script.Contains("SetQuestState");
        }
        
        private float CalculateSuccessProbability(DialogueEntry entry)
        {
            // If it's a skill check, calculate probability
            // For now, return 1.0 (always succeeds)
            return 1.0f;
        }
        
        private string GenerateStoryHint(DialogueEntry entry)
        {
            // Generate contextual hint based on entry content
            // This could parse special tags or metadata
            
            return "";
        }
        
        private int ExtractValue(string script, string functionName)
        {
            // Simple extraction - actual implementation would use regex or proper parsing
            int startIndex = script.IndexOf(functionName + "(");
            if (startIndex == -1) return 0;
            
            startIndex += functionName.Length + 1;
            int endIndex = script.IndexOf(")", startIndex);
            if (endIndex == -1) return 0;
            
            string valueStr = script.Substring(startIndex, endIndex - startIndex);
            if (int.TryParse(valueStr, out int value))
            {
                return value;
            }
            
            return 0;
        }
    }
    
    /// <summary>
    /// Preview of consequences for a dialogue choice
    /// </summary>
    [Serializable]
    public class ConsequencePreview
    {
        public ChoiceType choiceType = ChoiceType.Neutral;
        public CVCPoints cvcPointChanges = new CVCPoints();
        public List<CurrencyChange> currencyChanges = new List<CurrencyChange>();
        public List<RelationshipChange> relationshipChanges = new List<RelationshipChange>();
        public bool requirementsMet = true;
        public List<string> requirements = new List<string>();
        public float successProbability = 1.0f;
        public bool unlocksContent = false;
        public string storyHint = "";
    }
    
    /// <summary>
    /// Type of dialogue choice
    /// </summary>
    public enum ChoiceType
    {
        Neutral,
        Courage,
        Virtue,
        Cunning,
        Aggressive,
        Diplomatic,
        Clever
    }
    
    /// <summary>
    /// Currency change information
    /// </summary>
    [Serializable]
    public class CurrencyChange
    {
        public string currencyType;
        public int amount;
    }
    
    /// <summary>
    /// Relationship change information
    /// </summary>
    [Serializable]
    public class RelationshipChange
    {
        public string npcName;
        public int change;
    }
    
    #endregion
}