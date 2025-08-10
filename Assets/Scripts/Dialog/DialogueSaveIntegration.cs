using System;
using System.Collections.Generic;
using UnityEngine;
using NeonLadderURP.DataManagement;
using PixelCrushers.DialogueSystem;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Integrates PixelCrushers Dialogue System with NeonLadder's save system.
    /// Handles persistence of dialogue state, choices, and CVC values.
    /// </summary>
    public class DialogueSaveIntegration : MonoBehaviour
    {
        private static DialogueSaveIntegration instance;
        public static DialogueSaveIntegration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DialogueSaveIntegration>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DialogueSaveIntegration");
                        instance = go.AddComponent<DialogueSaveIntegration>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        [Header("Configuration")]
        [SerializeField] private bool autoSaveOnConversationEnd = true;
        [SerializeField] private bool trackChoiceHistory = true;
        [SerializeField] private bool debugLogging = false;
        
        [Header("Current State")]
        [SerializeField] private DialogueSaveData currentDialogueData;
        
        // Events
        public static event Action<DialogueSaveData> OnDialogueDataSaved;
        public static event Action<DialogueSaveData> OnDialogueDataLoaded;
        public static event Action<int, int, int> OnCVCValuesChanged; // courage, virtue, cunning
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize with empty data
            currentDialogueData = DialogueSaveData.CreateNew();
            
            // Subscribe to Dialogue System events
            SubscribeToDialogueEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromDialogueEvents();
        }
        
        private void SubscribeToDialogueEvents()
        {
            // Conversation events
            DialogueManager.instance.conversationStarted += OnConversationStarted;
            DialogueManager.instance.conversationEnded += OnConversationEnded;
            
            // Variable change events
            DialogueLua.SetVariable = SetVariableWithTracking;
        }
        
        private void UnsubscribeFromDialogueEvents()
        {
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.conversationStarted -= OnConversationStarted;
                DialogueManager.instance.conversationEnded -= OnConversationEnded;
            }
        }
        
        #region Dialogue System Event Handlers
        
        private void OnConversationStarted(Transform actor)
        {
            if (debugLogging)
                Debugger.Log($"[DialogueSaveIntegration] Conversation started with {actor?.name}");
            
            // Track conversation start
            currentDialogueData.totalConversationsPlayed++;
        }
        
        private void OnConversationEnded(Transform actor)
        {
            if (debugLogging)
                Debugger.Log($"[DialogueSaveIntegration] Conversation ended with {actor?.name}");
            
            // Get conversation info
            var lastConversation = DialogueManager.lastConversationStarted;
            if (!string.IsNullOrEmpty(lastConversation))
            {
                currentDialogueData.lastConversationId = lastConversation;
                
                // Update conversation state
                UpdateConversationState(lastConversation);
            }
            
            // Auto-save if enabled
            if (autoSaveOnConversationEnd)
            {
                SaveDialogueData();
            }
        }
        
        private void SetVariableWithTracking(string variableName, object value)
        {
            // Call original setter
            DialogueLua.SetVariable(variableName, value);
            
            // Track variable change
            TrackVariableChange(variableName, value);
            
            // Check for CVC value changes
            CheckCVCValueChange(variableName, value);
        }
        
        #endregion
        
        #region Save/Load Operations
        
        /// <summary>
        /// Saves dialogue data to the enhanced save system
        /// </summary>
        public void SaveDialogueData()
        {
            try
            {
                // Capture current Dialogue System state
                CaptureDialogueSystemState();
                
                // Get or create consolidated save data
                var consolidatedSave = EnhancedSaveSystem.Load() ?? ConsolidatedSaveData.CreateNew();
                
                // Convert dialogue data to JSON string for storage
                string dialogueJson = JsonUtility.ToJson(currentDialogueData, true);
                consolidatedSave.dialogueDataJson = dialogueJson;
                
                // Save the consolidated data
                EnhancedSaveSystem.Save(consolidatedSave);
                
                // Also save separately for backward compatibility
                SaveDialogueDataSeparately(currentDialogueData);
                
                if (debugLogging)
                    Debugger.Log("[DialogueSaveIntegration] Dialogue data saved successfully");
                
                OnDialogueDataSaved?.Invoke(currentDialogueData);
            }
            catch (Exception ex)
            {
                Debugger.LogError($"[DialogueSaveIntegration] Failed to save dialogue data: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Loads dialogue data from the save system
        /// </summary>
        public void LoadDialogueData()
        {
            try
            {
                // Try to load from consolidated save first
                var consolidatedSave = EnhancedSaveSystem.Load();
                
                if (consolidatedSave != null && !string.IsNullOrEmpty(consolidatedSave.dialogueDataJson))
                {
                    // Load from consolidated save
                    currentDialogueData = JsonUtility.FromJson<DialogueSaveData>(consolidatedSave.dialogueDataJson);
                    
                    if (debugLogging)
                        Debugger.Log("[DialogueSaveIntegration] Dialogue data loaded from consolidated save");
                }
                else
                {
                    // Fallback to separate file
                    currentDialogueData = LoadDialogueDataSeparately();
                }
                
                if (currentDialogueData == null)
                {
                    currentDialogueData = DialogueSaveData.CreateNew();
                    if (debugLogging)
                        Debugger.Log("[DialogueSaveIntegration] No saved dialogue data found, created new");
                }
                else
                {
                    // Validate the loaded data
                    currentDialogueData.Validate();
                    
                    // Apply loaded state to Dialogue System
                    ApplyDialogueSystemState();
                    
                    if (debugLogging)
                        Debugger.Log("[DialogueSaveIntegration] Dialogue data loaded successfully");
                }
                
                OnDialogueDataLoaded?.Invoke(currentDialogueData);
            }
            catch (Exception ex)
            {
                Debugger.LogError($"[DialogueSaveIntegration] Failed to load dialogue data: {ex.Message}");
                currentDialogueData = DialogueSaveData.CreateNew();
            }
        }
        
        #endregion
        
        #region Dialogue System State Management
        
        /// <summary>
        /// Captures the current state from the Dialogue System
        /// </summary>
        private void CaptureDialogueSystemState()
        {
            // Capture variables
            CaptureVariables();
            
            // Capture quest states
            CaptureQuestStates();
            
            // Capture actor states
            CaptureActorStates();
            
            // Update timestamp
            currentDialogueData.lastDialogueTimestamp = DateTime.Now;
        }
        
        /// <summary>
        /// Applies saved state to the Dialogue System
        /// </summary>
        private void ApplyDialogueSystemState()
        {
            // Apply variables
            ApplyVariables();
            
            // Apply quest states
            ApplyQuestStates();
            
            // Apply actor states
            ApplyActorStates();
            
            // Apply conversation states
            ApplyConversationStates();
        }
        
        private void CaptureVariables()
        {
            currentDialogueData.dialogueVariables.Clear();
            
            // Get all variables from Dialogue System
            var database = DialogueManager.masterDatabase;
            if (database != null)
            {
                foreach (var variable in database.variables)
                {
                    var saveVar = new DialogueSaveData.DialogueVariable
                    {
                        name = variable.Name,
                        value = DialogueLua.GetVariable(variable.Name).asString,
                        type = GetVariableType(variable.Type)
                    };
                    currentDialogueData.dialogueVariables.Add(saveVar);
                }
            }
        }
        
        private void ApplyVariables()
        {
            foreach (var variable in currentDialogueData.dialogueVariables)
            {
                switch (variable.type)
                {
                    case DialogueSaveData.DialogueVariable.VariableType.Boolean:
                        DialogueLua.SetVariable(variable.name, bool.Parse(variable.value));
                        break;
                    case DialogueSaveData.DialogueVariable.VariableType.Number:
                        DialogueLua.SetVariable(variable.name, float.Parse(variable.value));
                        break;
                    default:
                        DialogueLua.SetVariable(variable.name, variable.value);
                        break;
                }
            }
        }
        
        private void CaptureQuestStates()
        {
            currentDialogueData.questStates.Clear();
            
            // Get all quests from Dialogue System
            var database = DialogueManager.masterDatabase;
            if (database != null)
            {
                foreach (var quest in database.items)
                {
                    if (quest.IsItem) continue; // Skip non-quest items
                    
                    var questState = new DialogueSaveData.QuestState
                    {
                        questName = quest.Name,
                        currentState = QuestLog.GetQuestState(quest.Name).ToString()
                    };
                    
                    // Get quest entries
                    int entryCount = QuestLog.GetQuestEntryCount(quest.Name);
                    for (int i = 1; i <= entryCount; i++)
                    {
                        var entryState = QuestLog.GetQuestEntryState(quest.Name, i);
                        if (entryState == QuestState.Success || entryState == QuestState.Failure)
                        {
                            questState.completedEntries.Add(i);
                        }
                    }
                    
                    currentDialogueData.questStates.Add(questState);
                }
            }
        }
        
        private void ApplyQuestStates()
        {
            foreach (var questState in currentDialogueData.questStates)
            {
                // Parse and apply quest state
                if (Enum.TryParse<QuestState>(questState.currentState, out var state))
                {
                    QuestLog.SetQuestState(questState.questName, state);
                }
                
                // Apply completed entries
                foreach (var entryId in questState.completedEntries)
                {
                    QuestLog.SetQuestEntryState(questState.questName, entryId, QuestState.Success);
                }
            }
        }
        
        private void CaptureActorStates()
        {
            currentDialogueData.actorStates.Clear();
            
            // Capture relationship values and other actor-specific data
            var database = DialogueManager.masterDatabase;
            if (database != null)
            {
                foreach (var actor in database.actors)
                {
                    var actorState = new DialogueSaveData.ActorState
                    {
                        actorName = actor.Name,
                        actorId = actor.id,
                        relationshipLevel = GetRelationshipLevel(actor.Name)
                    };
                    
                    currentDialogueData.actorStates.Add(actorState);
                }
            }
        }
        
        private void ApplyActorStates()
        {
            foreach (var actorState in currentDialogueData.actorStates)
            {
                SetRelationshipLevel(actorState.actorName, actorState.relationshipLevel);
            }
        }
        
        private void ApplyConversationStates()
        {
            foreach (var convState in currentDialogueData.conversationStates)
            {
                // Mark conversations as played
                if (convState.hasBeenPlayed)
                {
                    DialogueLua.SetVariable($"Conversation_{convState.conversationId}_Played", true);
                }
                
                // Apply times played
                DialogueLua.SetVariable($"Conversation_{convState.conversationId}_TimesPlayed", convState.timesPlayed);
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private void UpdateConversationState(string conversationTitle)
        {
            var conversation = DialogueManager.masterDatabase.GetConversation(conversationTitle);
            if (conversation == null) return;
            
            var convState = currentDialogueData.conversationStates.Find(c => c.conversationId == conversation.id);
            if (convState == null)
            {
                convState = new DialogueSaveData.ConversationState
                {
                    conversationId = conversation.id,
                    conversationTitle = conversationTitle
                };
                currentDialogueData.conversationStates.Add(convState);
            }
            
            convState.hasBeenPlayed = true;
            convState.timesPlayed++;
            convState.lastPlayed = DateTime.Now;
        }
        
        private void TrackVariableChange(string variableName, object value)
        {
            var variable = currentDialogueData.dialogueVariables.Find(v => v.name == variableName);
            if (variable == null)
            {
                variable = new DialogueSaveData.DialogueVariable
                {
                    name = variableName,
                    type = DialogueSaveData.DialogueVariable.VariableType.Text
                };
                currentDialogueData.dialogueVariables.Add(variable);
            }
            
            variable.value = value.ToString();
        }
        
        private void CheckCVCValueChange(string variableName, object value)
        {
            // Check if this is a CVC value
            switch (variableName.ToLower())
            {
                case "courage":
                case "cvc_courage":
                    int oldCourage = currentDialogueData.cvcData.courage;
                    currentDialogueData.cvcData.courage = Convert.ToInt32(value);
                    TrackCVCChange("courage", oldCourage, currentDialogueData.cvcData.courage);
                    break;
                    
                case "virtue":
                case "cvc_virtue":
                    int oldVirtue = currentDialogueData.cvcData.virtue;
                    currentDialogueData.cvcData.virtue = Convert.ToInt32(value);
                    TrackCVCChange("virtue", oldVirtue, currentDialogueData.cvcData.virtue);
                    break;
                    
                case "cunning":
                case "cvc_cunning":
                    int oldCunning = currentDialogueData.cvcData.cunning;
                    currentDialogueData.cvcData.cunning = Convert.ToInt32(value);
                    TrackCVCChange("cunning", oldCunning, currentDialogueData.cvcData.cunning);
                    break;
            }
            
            // Notify listeners of CVC changes
            OnCVCValuesChanged?.Invoke(
                currentDialogueData.cvcData.courage,
                currentDialogueData.cvcData.virtue,
                currentDialogueData.cvcData.cunning
            );
        }
        
        private void TrackCVCChange(string valueName, int oldValue, int newValue)
        {
            if (oldValue != newValue)
            {
                var change = new DialogueSaveData.CVCData.CVCChange
                {
                    valueName = valueName,
                    oldValue = oldValue,
                    newValue = newValue,
                    reason = "Dialogue choice",
                    timestamp = DateTime.Now
                };
                currentDialogueData.cvcData.changeHistory.Add(change);
            }
        }
        
        private float GetRelationshipLevel(string actorName)
        {
            // Get relationship value from Dialogue System
            var relationshipVar = $"Relationship_{actorName}";
            if (DialogueLua.DoesVariableExist(relationshipVar))
            {
                return DialogueLua.GetVariable(relationshipVar).asFloat;
            }
            return 0f;
        }
        
        private void SetRelationshipLevel(string actorName, float level)
        {
            var relationshipVar = $"Relationship_{actorName}";
            DialogueLua.SetVariable(relationshipVar, level);
        }
        
        private DialogueSaveData.DialogueVariable.VariableType GetVariableType(FieldType fieldType)
        {
            switch (fieldType)
            {
                case FieldType.Boolean:
                    return DialogueSaveData.DialogueVariable.VariableType.Boolean;
                case FieldType.Number:
                    return DialogueSaveData.DialogueVariable.VariableType.Number;
                default:
                    return DialogueSaveData.DialogueVariable.VariableType.Text;
            }
        }
        
        #endregion
        
        #region Separate Save File Management (Temporary)
        
        private void SaveDialogueDataSeparately(DialogueSaveData data)
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "dialogue_save.json");
            string json = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(path, json);
        }
        
        private DialogueSaveData LoadDialogueDataSeparately()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "dialogue_save.json");
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                return JsonUtility.FromJson<DialogueSaveData>(json);
            }
            return null;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Gets the current dialogue save data
        /// </summary>
        public DialogueSaveData GetCurrentDialogueData()
        {
            return currentDialogueData;
        }
        
        /// <summary>
        /// Records a player choice
        /// </summary>
        public void RecordPlayerChoice(int conversationId, int nodeId, string choiceText, string consequence = null)
        {
            if (!trackChoiceHistory) return;
            
            var choice = new DialogueSaveData.DialogueChoice
            {
                conversationId = conversationId,
                nodeId = nodeId,
                choiceText = choiceText,
                consequence = consequence,
                timestamp = DateTime.Now
            };
            
            // Track CVC impact if any
            choice.cvcImpact["courage"] = currentDialogueData.cvcData.courage;
            choice.cvcImpact["virtue"] = currentDialogueData.cvcData.virtue;
            choice.cvcImpact["cunning"] = currentDialogueData.cvcData.cunning;
            
            currentDialogueData.choiceHistory.Add(choice);
        }
        
        /// <summary>
        /// Gets the CVC values
        /// </summary>
        public (int courage, int virtue, int cunning) GetCVCValues()
        {
            return (
                currentDialogueData.cvcData.courage,
                currentDialogueData.cvcData.virtue,
                currentDialogueData.cvcData.cunning
            );
        }
        
        /// <summary>
        /// Sets CVC values directly
        /// </summary>
        public void SetCVCValues(int courage, int virtue, int cunning)
        {
            currentDialogueData.cvcData.courage = courage;
            currentDialogueData.cvcData.virtue = virtue;
            currentDialogueData.cvcData.cunning = cunning;
            
            OnCVCValuesChanged?.Invoke(courage, virtue, cunning);
        }
        
        #endregion
    }
}