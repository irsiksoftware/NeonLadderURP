using System;
using System.Collections.Generic;
using UnityEngine;
using NeonLadderURP.DataManagement;
using NeonLadder.Dialog;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Integration layer between Dialogue System and Enhanced Save System
    /// Handles persistence of dialogue state across sessions and scene transitions
    /// </summary>
    public class DialogueSystemSaveIntegration : MonoBehaviour
    {
        #region Singleton
        
        private static DialogueSystemSaveIntegration instance;
        public static DialogueSystemSaveIntegration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DialogueSystemSaveIntegration>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DialogueSystemSaveIntegration");
                        instance = go.AddComponent<DialogueSystemSaveIntegration>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Save Configuration")]
        [SerializeField] private bool autoSaveOnConversationEnd = true;
        [SerializeField] private bool autoSaveOnChoiceMade = false;
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private float autoSaveDebounceTime = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogs = true;
        
        #endregion
        
        #region Private Fields
        
        private DialogueSaveData currentDialogueData;
        private ConsolidatedSaveData lastLoadedSaveData;
        private float lastAutoSaveTime;
        private bool isDirty = false;
        
        #endregion
        
        #region Events
        
        public static event Action<DialogueSaveData> OnDialogueSaved;
        public static event Action<DialogueSaveData> OnDialogueLoaded;
        public static event Action<string> OnSaveError;
        
        #endregion
        
        #region Initialization
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }
        
        private void Initialize()
        {
            // Subscribe to dialogue system events
            if (DialogueSystemIntegration.Instance != null)
            {
                DialogueSystemIntegration.OnConversationStarted += HandleConversationStarted;
                DialogueSystemIntegration.OnConversationEnded += HandleConversationEnded;
                DialogueSystemIntegration.OnChoiceMade += HandleChoiceMade;
                DialogueSystemIntegration.OnVariableChanged += HandleVariableChanged;
            }
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveCompleted += HandleSaveCompleted;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
            
            // Load existing save data if configured
            if (autoLoadOnStart)
            {
                LoadDialogueState();
            }
            
            LogDebug("DialogueSystemSaveIntegration initialized");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (DialogueSystemIntegration.Instance != null)
            {
                DialogueSystemIntegration.OnConversationStarted -= HandleConversationStarted;
                DialogueSystemIntegration.OnConversationEnded -= HandleConversationEnded;
                DialogueSystemIntegration.OnChoiceMade -= HandleChoiceMade;
                DialogueSystemIntegration.OnVariableChanged -= HandleVariableChanged;
            }
            
            EnhancedSaveSystem.OnSaveCompleted -= HandleSaveCompleted;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
        }
        
        #endregion
        
        #region Save/Load Operations
        
        /// <summary>
        /// Save current dialogue state to the save system
        /// </summary>
        public void SaveDialogueState()
        {
            try
            {
                // Get or create consolidated save data
                var saveData = lastLoadedSaveData ?? EnhancedSaveSystem.Load() ?? CreateNewSaveData();
                
                // Update dialogue data
                saveData.dialogueData = GetCurrentDialogueData();
                
                // Save to file system
                bool success = EnhancedSaveSystem.Save(saveData);
                
                if (success)
                {
                    lastLoadedSaveData = saveData;
                    isDirty = false;
                    lastAutoSaveTime = Time.time;
                    
                    OnDialogueSaved?.Invoke(saveData.dialogueData);
                    LogDebug("Dialogue state saved successfully");
                }
                else
                {
                    LogError("Failed to save dialogue state");
                }
            }
            catch (Exception e)
            {
                LogError($"Error saving dialogue state: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }
        
        /// <summary>
        /// Load dialogue state from the save system
        /// </summary>
        public void LoadDialogueState()
        {
            try
            {
                var saveData = EnhancedSaveSystem.Load();
                
                if (saveData != null && saveData.dialogueData != null)
                {
                    currentDialogueData = saveData.dialogueData;
                    lastLoadedSaveData = saveData;
                    
                    // Apply loaded data to dialogue system
                    ApplyLoadedDialogueData(currentDialogueData);
                    
                    OnDialogueLoaded?.Invoke(currentDialogueData);
                    LogDebug($"Dialogue state loaded: {currentDialogueData.totalConversationsStarted} conversations, {currentDialogueData.totalChoicesMade} choices");
                }
                else
                {
                    // No save data exists, create new
                    currentDialogueData = new DialogueSaveData();
                    LogDebug("No dialogue save data found, starting fresh");
                }
            }
            catch (Exception e)
            {
                LogError($"Error loading dialogue state: {e.Message}");
                currentDialogueData = new DialogueSaveData();
            }
        }
        
        /// <summary>
        /// Get current dialogue data from the dialogue system
        /// </summary>
        private DialogueSaveData GetCurrentDialogueData()
        {
            if (currentDialogueData == null)
            {
                currentDialogueData = new DialogueSaveData();
            }
            
            // Sync with DialogueSystemIntegration if available
            if (DialogueSystemIntegration.Instance != null)
            {
                var dialogueSaveData = DialogueSystemIntegration.Instance.GetSaveData();
                if (dialogueSaveData != null && dialogueSaveData.Variables != null)
                {
                    // Merge dialogue variables
                    foreach (var kvp in dialogueSaveData.Variables)
                    {
                        currentDialogueData.SetDialogueVariable(kvp.Key, kvp.Value);
                    }
                }
            }
            
            return currentDialogueData;
        }
        
        /// <summary>
        /// Apply loaded dialogue data to the dialogue system
        /// </summary>
        private void ApplyLoadedDialogueData(DialogueSaveData data)
        {
            if (data == null || DialogueSystemIntegration.Instance == null)
                return;
            
            // Create DialogueSaveData for DialogueSystemIntegration
            var integrationSaveData = new NeonLadder.Dialog.DialogueSaveData
            {
                Variables = new Dictionary<string, object>(data.dialogueVariables),
                CompletedConversations = new List<string>(data.completedConversations)
            };
            
            // Apply to dialogue system
            DialogueSystemIntegration.Instance.LoadSaveData(integrationSaveData);
            
            // Restore conversation state if any
            if (data.currentConversationState != null)
            {
                RestoreConversationState(data.currentConversationState);
            }
            
            LogDebug($"Applied dialogue data: {data.dialogueVariables.Count} variables, {data.completedConversations.Count} completed conversations");
        }
        
        /// <summary>
        /// Restore a saved conversation state
        /// </summary>
        private void RestoreConversationState(ConversationStateData state)
        {
            if (state == null || string.IsNullOrEmpty(state.conversationId))
                return;
            
            // Check if conversation was saved recently (within 24 hours)
            var timeSinceSave = DateTime.Now - state.savedTime;
            if (timeSinceSave.TotalHours > 24)
            {
                LogDebug("Saved conversation state too old, not restoring");
                return;
            }
            
            LogDebug($"Restoring conversation state: {state.conversationId} at node {state.currentNodeId}");
            
            // TODO: Implement conversation restoration when dialogue UI is ready
            // This would involve:
            // 1. Starting the conversation
            // 2. Fast-forwarding to the saved node
            // 3. Restoring local variables
        }
        
        #endregion
        
        #region Event Handlers
        
        private void HandleConversationStarted(string conversationId)
        {
            if (currentDialogueData == null)
                currentDialogueData = new DialogueSaveData();
            
            currentDialogueData.RecordConversationStart(conversationId);
            isDirty = true;
            
            LogDebug($"Conversation started: {conversationId}");
        }
        
        private void HandleConversationEnded(string conversationId)
        {
            if (currentDialogueData == null)
                return;
            
            currentDialogueData.RecordConversationComplete(conversationId);
            isDirty = true;
            
            if (autoSaveOnConversationEnd)
            {
                ScheduleAutoSave();
            }
            
            LogDebug($"Conversation ended: {conversationId}");
        }
        
        private void HandleChoiceMade(DialogueChoice choice)
        {
            if (currentDialogueData == null || choice == null)
                return;
            
            // Get current conversation from DialogueSystemIntegration
            var activeConversations = DialogueSystemIntegration.Instance?.GetActiveConversations();
            if (activeConversations != null && activeConversations.Count > 0)
            {
                string conversationId = activeConversations[0]; // Get first active conversation
                
                // Record the choice (we don't have index here, so use -1)
                currentDialogueData.RecordChoice(conversationId, -1, choice.Text);
                isDirty = true;
                
                if (autoSaveOnChoiceMade)
                {
                    ScheduleAutoSave();
                }
                
                LogDebug($"Choice made: {choice.Text}");
            }
        }
        
        private void HandleVariableChanged(string variableName, object value)
        {
            if (currentDialogueData == null)
                return;
            
            currentDialogueData.SetDialogueVariable(variableName, value);
            isDirty = true;
            
            LogDebug($"Variable changed: {variableName} = {value}");
        }
        
        private void HandleSaveCompleted(ConsolidatedSaveData saveData)
        {
            // Update our reference when save completes
            lastLoadedSaveData = saveData;
            LogDebug("Save system completed, dialogue data included");
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            // Update our data when load completes
            if (saveData != null && saveData.dialogueData != null)
            {
                currentDialogueData = saveData.dialogueData;
                lastLoadedSaveData = saveData;
                ApplyLoadedDialogueData(currentDialogueData);
                
                LogDebug("Save system loaded, dialogue data restored");
            }
        }
        
        #endregion
        
        #region Auto-Save
        
        private void ScheduleAutoSave()
        {
            // Debounce auto-saves to avoid excessive disk writes
            if (Time.time - lastAutoSaveTime > autoSaveDebounceTime)
            {
                SaveDialogueState();
            }
        }
        
        private void Update()
        {
            // Check for pending auto-save
            if (isDirty && autoSaveDebounceTime > 0)
            {
                if (Time.time - lastAutoSaveTime > autoSaveDebounceTime)
                {
                    SaveDialogueState();
                }
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Update character relationship
        /// </summary>
        public void UpdateRelationship(string characterName, int delta, string relationshipType = "default")
        {
            if (currentDialogueData == null)
                currentDialogueData = new DialogueSaveData();
            
            currentDialogueData.UpdateRelationship(characterName, delta, relationshipType);
            isDirty = true;
            
            LogDebug($"Relationship updated: {characterName} {(delta >= 0 ? "+" : "")}{delta}");
        }
        
        /// <summary>
        /// Update CVC level
        /// </summary>
        public void UpdateCVCLevel(string characterName, CVCType cvcType, int points)
        {
            if (currentDialogueData == null)
                currentDialogueData = new DialogueSaveData();
            
            currentDialogueData.UpdateCVCLevel(characterName, cvcType, points);
            isDirty = true;
            
            LogDebug($"CVC updated: {characterName} {cvcType} +{points}");
        }
        
        /// <summary>
        /// Unlock dialogue path
        /// </summary>
        public void UnlockDialoguePath(string pathId)
        {
            if (currentDialogueData == null)
                currentDialogueData = new DialogueSaveData();
            
            currentDialogueData.UnlockDialoguePath(pathId);
            isDirty = true;
            
            LogDebug($"Dialogue path unlocked: {pathId}");
        }
        
        /// <summary>
        /// Check if conversation has been completed
        /// </summary>
        public bool HasCompletedConversation(string conversationId)
        {
            return currentDialogueData?.HasCompletedConversation(conversationId) ?? false;
        }
        
        /// <summary>
        /// Get character relationship data
        /// </summary>
        public CharacterRelationshipData GetRelationship(string characterName)
        {
            return currentDialogueData?.GetRelationship(characterName);
        }
        
        /// <summary>
        /// Get CVC level data
        /// </summary>
        public CVCLevelData GetCVCLevel(string characterName, CVCType cvcType)
        {
            return currentDialogueData?.GetCVCLevel(characterName, cvcType);
        }
        
        /// <summary>
        /// Get dialogue analytics
        /// </summary>
        public DialogueAnalytics GetAnalytics()
        {
            return currentDialogueData?.GetAnalytics() ?? new DialogueAnalytics();
        }
        
        /// <summary>
        /// Force save current state
        /// </summary>
        public void ForceSave()
        {
            SaveDialogueState();
        }
        
        /// <summary>
        /// Reset all dialogue data
        /// </summary>
        public void ResetDialogueData()
        {
            currentDialogueData = new DialogueSaveData();
            isDirty = true;
            SaveDialogueState();
            
            LogDebug("Dialogue data reset");
        }
        
        #endregion
        
        #region Utility Methods
        
        private ConsolidatedSaveData CreateNewSaveData()
        {
            return new ConsolidatedSaveData
            {
                dialogueData = new DialogueSaveData()
            };
        }
        
        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debugger.Log($"[DialogueSaveIntegration] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[DialogueSaveIntegration] {message}");
        }
        
        #endregion
    }
}