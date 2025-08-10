using System;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using NeonLadder.DataManagement;
using NeonLadder.Debugging;
using NeonLadderURP.DataManagement;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Integrates PixelCrushers Dialogue System with NeonLadder's save system.
    /// Ensures all dialogue choices, CVC levels, and narrative state persist across sessions.
    /// </summary>
    public class DialogueSaveIntegration : MonoBehaviour
    {
        #region Singleton
        
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
        
        #endregion
        
        #region Configuration
        
        [Header("Save Configuration")]
        [SerializeField] private bool autoSaveOnConversationEnd = true;
        [SerializeField] private bool saveRelationshipData = true;
        [SerializeField] private bool saveCVCProgress = true;
        [SerializeField] private bool saveQuestStates = true;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        private ConversationPointTracker pointTracker;
        private DialogueSystemController dialogueController;
        private DialogueData currentDialogueData;
        private bool isInitialized = false;
        
        #endregion
        
        #region Events
        
        public static event Action<DialogueData> OnDialogueDataSaved;
        public static event Action<DialogueData> OnDialogueDataLoaded;
        public static event Action<string> OnDialogueStateError;
        
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
            // Find dialogue system components
            pointTracker = FindObjectOfType<ConversationPointTracker>();
            dialogueController = FindObjectOfType<DialogueSystemController>();
            
            if (dialogueController == null)
            {
                LogWarning("DialogueSystemController not found. Creating one...");
                GameObject dialogueGO = new GameObject("DialogueSystemController");
                dialogueController = dialogueGO.AddComponent<DialogueSystemController>();
            }
            
            // Subscribe to dialogue events
            DialogueManager.instance.conversationStarted += OnConversationStarted;
            DialogueManager.instance.conversationEnded += OnConversationEnded;
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveRequested += HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
            
            isInitialized = true;
            LogDebug("DialogueSaveIntegration initialized");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (DialogueManager.hasInstance)
            {
                DialogueManager.instance.conversationStarted -= OnConversationStarted;
                DialogueManager.instance.conversationEnded -= OnConversationEnded;
            }
            
            EnhancedSaveSystem.OnSaveRequested -= HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Manually save current dialogue state
        /// </summary>
        public void SaveDialogueData()
        {
            if (!isInitialized)
            {
                LogError("DialogueSaveIntegration not initialized");
                return;
            }
            
            // Capture current dialogue system state
            CaptureDialogueSystemState();
            
            // Save to consolidated save data
            var consolidatedSave = EnhancedSaveSystem.Load() ?? ConsolidatedSaveData.CreateNew();
            
            // Convert to JSON and store
            string dialogueJson = JsonUtility.ToJson(currentDialogueData, true);
            consolidatedSave.dialogueDataJson = dialogueJson;
            
            // Save
            EnhancedSaveSystem.Save(consolidatedSave);
            
            OnDialogueDataSaved?.Invoke(currentDialogueData);
            LogDebug("Dialogue data saved successfully");
        }
        
        /// <summary>
        /// Manually load dialogue state
        /// </summary>
        public void LoadDialogueData()
        {
            if (!isInitialized)
            {
                LogError("DialogueSaveIntegration not initialized");
                return;
            }
            
            var consolidatedSave = EnhancedSaveSystem.Load();
            if (consolidatedSave == null || string.IsNullOrEmpty(consolidatedSave.dialogueDataJson))
            {
                LogDebug("No dialogue save data found");
                return;
            }
            
            try
            {
                currentDialogueData = JsonUtility.FromJson<DialogueData>(consolidatedSave.dialogueDataJson);
                RestoreDialogueSystemState();
                
                OnDialogueDataLoaded?.Invoke(currentDialogueData);
                LogDebug("Dialogue data loaded successfully");
            }
            catch (Exception e)
            {
                LogError($"Failed to load dialogue data: {e.Message}");
                OnDialogueStateError?.Invoke(e.Message);
            }
        }
        
        /// <summary>
        /// Get current dialogue save data
        /// </summary>
        public DialogueData GetCurrentDialogueData()
        {
            if (currentDialogueData == null)
            {
                CaptureDialogueSystemState();
            }
            return currentDialogueData;
        }
        
        /// <summary>
        /// Reset all dialogue progress
        /// </summary>
        public void ResetDialogueProgress()
        {
            currentDialogueData = new DialogueData();
            
            // Reset PixelCrushers Dialogue System
            if (dialogueController != null)
            {
                DialogueManager.ResetDatabase(DatabaseResetOptions.RevertToDefault);
            }
            
            // Reset CVC points
            if (pointTracker != null)
            {
                pointTracker.ResetAllPoints();
            }
            
            LogDebug("Dialogue progress reset");
        }
        
        #endregion
        
        #region Private Methods - State Capture
        
        private void CaptureDialogueSystemState()
        {
            if (currentDialogueData == null)
            {
                currentDialogueData = new DialogueData();
            }
            
            // Capture timestamp
            currentDialogueData.lastSaved = DateTime.Now;
            
            // Capture PixelCrushers Dialogue System state
            if (dialogueController != null)
            {
                CaptureDialogueSystemData();
            }
            
            // Capture CVC points
            if (saveCVCProgress && pointTracker != null)
            {
                CaptureCVCData();
            }
            
            // Capture quest states
            if (saveQuestStates)
            {
                CaptureQuestData();
            }
            
            // Capture relationship data
            if (saveRelationshipData)
            {
                CaptureRelationshipData();
            }
        }
        
        private void CaptureDialogueSystemData()
        {
            // Get Dialogue System save data string
            string pixelCrushersSaveData = PersistentDataManager.GetSaveData();
            currentDialogueData.pixelCrushersData = pixelCrushersSaveData;
            
            // Capture active conversation if any
            if (DialogueManager.isConversationActive)
            {
                currentDialogueData.activeConversation = DialogueManager.lastConversationStarted;
                currentDialogueData.activeConversationEntry = DialogueManager.currentConversationState.subtitle.dialogueEntry.id;
            }
            
            // Capture SimStatus (tracks which dialogue entries have been shown)
            var database = DialogueManager.masterDatabase;
            currentDialogueData.conversationHistory = new Dictionary<string, List<int>>();
            
            foreach (var conversation in database.conversations)
            {
                var shownEntries = new List<int>();
                foreach (var entry in conversation.dialogueEntries)
                {
                    if (DialogueLua.GetSimStatus(entry) == DialogueEntryStatus.WasDisplayed)
                    {
                        shownEntries.Add(entry.id);
                    }
                }
                
                if (shownEntries.Count > 0)
                {
                    currentDialogueData.conversationHistory[conversation.Title] = shownEntries;
                }
            }
        }
        
        private void CaptureCVCData()
        {
            currentDialogueData.cvcPoints = new Dictionary<string, CVCData>();
            
            // Get all tracked characters
            var trackedCharacters = pointTracker.GetAllTrackedCharacters();
            
            foreach (var characterId in trackedCharacters)
            {
                var cvcData = new CVCData
                {
                    characterId = characterId,
                    totalPoints = pointTracker.GetPointsForCharacter(characterId),
                    cvcLevel = pointTracker.GetCVCLevel(characterId),
                    couragePoints = pointTracker.GetCouragePoints(characterId),
                    virtuePoints = pointTracker.GetVirtuePoints(characterId),
                    cunningPoints = pointTracker.GetCunningPoints(characterId)
                };
                
                // Get choice history if available
                cvcData.choiceHistory = pointTracker.GetChoiceHistory(characterId);
                
                currentDialogueData.cvcPoints[characterId] = cvcData;
            }
            
            // Capture global CVC stats
            currentDialogueData.totalCVCPoints = pointTracker.GetTotalPoints();
            currentDialogueData.totalCVCLevel = pointTracker.GetOverallCVCLevel();
        }
        
        private void CaptureQuestData()
        {
            currentDialogueData.questStates = new Dictionary<string, string>();
            
            // Get all quest states from Dialogue System
            var questLog = QuestLog.GetAllQuests();
            foreach (var questTitle in questLog)
            {
                var questState = QuestLog.GetQuestState(questTitle);
                currentDialogueData.questStates[questTitle] = questState.ToString();
                
                // Also capture quest entry states
                int entryCount = QuestLog.GetQuestEntryCount(questTitle);
                for (int i = 1; i <= entryCount; i++)
                {
                    var entryState = QuestLog.GetQuestEntryState(questTitle, i);
                    currentDialogueData.questStates[$"{questTitle}_Entry{i}"] = entryState.ToString();
                }
            }
        }
        
        private void CaptureRelationshipData()
        {
            currentDialogueData.relationships = new Dictionary<string, float>();
            
            // Get all actors from database
            var database = DialogueManager.masterDatabase;
            foreach (var actor in database.actors)
            {
                // Get relationship value (using Dialogue System's relationship functions)
                float relationshipValue = DialogueLua.GetVariable($"Relationship_{actor.Name}").asFloat;
                if (relationshipValue != 0)
                {
                    currentDialogueData.relationships[actor.Name] = relationshipValue;
                }
            }
        }
        
        #endregion
        
        #region Private Methods - State Restoration
        
        private void RestoreDialogueSystemState()
        {
            if (currentDialogueData == null)
            {
                LogWarning("No dialogue data to restore");
                return;
            }
            
            // Restore PixelCrushers Dialogue System state
            if (!string.IsNullOrEmpty(currentDialogueData.pixelCrushersData))
            {
                PersistentDataManager.ApplySaveData(currentDialogueData.pixelCrushersData);
            }
            
            // Restore conversation history
            if (currentDialogueData.conversationHistory != null)
            {
                RestoreConversationHistory();
            }
            
            // Restore CVC points
            if (saveCVCProgress && currentDialogueData.cvcPoints != null)
            {
                RestoreCVCData();
            }
            
            // Restore quest states
            if (saveQuestStates && currentDialogueData.questStates != null)
            {
                RestoreQuestData();
            }
            
            // Restore relationships
            if (saveRelationshipData && currentDialogueData.relationships != null)
            {
                RestoreRelationshipData();
            }
            
            LogDebug($"Dialogue state restored from {currentDialogueData.lastSaved}");
        }
        
        private void RestoreConversationHistory()
        {
            var database = DialogueManager.masterDatabase;
            
            foreach (var kvp in currentDialogueData.conversationHistory)
            {
                var conversation = database.GetConversation(kvp.Key);
                if (conversation != null)
                {
                    foreach (int entryId in kvp.Value)
                    {
                        var entry = conversation.GetDialogueEntry(entryId);
                        if (entry != null)
                        {
                            DialogueLua.MarkDialogueEntryDisplayed(entry);
                        }
                    }
                }
            }
        }
        
        private void RestoreCVCData()
        {
            if (pointTracker == null)
            {
                pointTracker = FindObjectOfType<ConversationPointTracker>();
                if (pointTracker == null)
                {
                    LogWarning("ConversationPointTracker not found, cannot restore CVC data");
                    return;
                }
            }
            
            foreach (var kvp in currentDialogueData.cvcPoints)
            {
                var cvcData = kvp.Value;
                
                // Restore points for character
                pointTracker.SetPointsForCharacter(cvcData.characterId, cvcData.totalPoints);
                
                // Restore individual CVC components
                pointTracker.SetCouragePoints(cvcData.characterId, cvcData.couragePoints);
                pointTracker.SetVirtuePoints(cvcData.characterId, cvcData.virtuePoints);
                pointTracker.SetCunningPoints(cvcData.characterId, cvcData.cunningPoints);
                
                // Restore choice history if available
                if (cvcData.choiceHistory != null)
                {
                    pointTracker.RestoreChoiceHistory(cvcData.characterId, cvcData.choiceHistory);
                }
            }
        }
        
        private void RestoreQuestData()
        {
            foreach (var kvp in currentDialogueData.questStates)
            {
                if (kvp.Key.Contains("_Entry"))
                {
                    // This is a quest entry state
                    var parts = kvp.Key.Split(new[] { "_Entry" }, StringSplitOptions.None);
                    if (parts.Length == 2 && int.TryParse(parts[1], out int entryNumber))
                    {
                        var questTitle = parts[0];
                        if (Enum.TryParse<QuestState>(kvp.Value, out QuestState entryState))
                        {
                            QuestLog.SetQuestEntryState(questTitle, entryNumber, entryState);
                        }
                    }
                }
                else
                {
                    // This is a main quest state
                    if (Enum.TryParse<QuestState>(kvp.Value, out QuestState questState))
                    {
                        QuestLog.SetQuestState(kvp.Key, questState);
                    }
                }
            }
        }
        
        private void RestoreRelationshipData()
        {
            foreach (var kvp in currentDialogueData.relationships)
            {
                DialogueLua.SetVariable($"Relationship_{kvp.Key}", kvp.Value);
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnConversationStarted(Transform actor)
        {
            LogDebug($"Conversation started with {actor?.name}");
        }
        
        private void OnConversationEnded(Transform actor)
        {
            LogDebug($"Conversation ended with {actor?.name}");
            
            if (autoSaveOnConversationEnd)
            {
                SaveDialogueData();
            }
        }
        
        private void HandleSaveRequested(ConsolidatedSaveData saveData)
        {
            // Capture current dialogue state
            CaptureDialogueSystemState();
            
            // Add to save data
            if (currentDialogueData != null)
            {
                string dialogueJson = JsonUtility.ToJson(currentDialogueData, true);
                saveData.dialogueDataJson = dialogueJson;
            }
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            // Load dialogue state from save data
            if (!string.IsNullOrEmpty(saveData.dialogueDataJson))
            {
                try
                {
                    currentDialogueData = JsonUtility.FromJson<DialogueData>(saveData.dialogueDataJson);
                    RestoreDialogueSystemState();
                }
                catch (Exception e)
                {
                    LogError($"Failed to restore dialogue state: {e.Message}");
                }
            }
        }
        
        #endregion
        
        #region Logging
        
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debugger.Log($"[DialogueSave] {message}");
            }
        }
        
        private void LogWarning(string message)
        {
            Debugger.LogWarning($"[DialogueSave] {message}");
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[DialogueSave] {message}");
            OnDialogueStateError?.Invoke(message);
        }
        
        #endregion
    }
    
    #region Data Classes
    
    /// <summary>
    /// Complete dialogue system save data
    /// </summary>
    [Serializable]
    public class DialogueData
    {
        public DateTime lastSaved;
        
        // PixelCrushers Dialogue System data
        public string pixelCrushersData;
        public string activeConversation;
        public int activeConversationEntry;
        public Dictionary<string, List<int>> conversationHistory;
        
        // CVC System data
        public Dictionary<string, CVCData> cvcPoints;
        public int totalCVCPoints;
        public int totalCVCLevel;
        
        // Quest data
        public Dictionary<string, string> questStates;
        
        // Relationship data
        public Dictionary<string, float> relationships;
        
        // Custom dialogue flags
        public Dictionary<string, bool> dialogueFlags;
        
        public DialogueData()
        {
            conversationHistory = new Dictionary<string, List<int>>();
            cvcPoints = new Dictionary<string, CVCData>();
            questStates = new Dictionary<string, string>();
            relationships = new Dictionary<string, float>();
            dialogueFlags = new Dictionary<string, bool>();
        }
    }
    
    /// <summary>
    /// CVC (Courage, Virtue, Cunning) character data
    /// </summary>
    [Serializable]
    public class CVCData
    {
        public string characterId;
        public int totalPoints;
        public int cvcLevel;
        public int couragePoints;
        public int virtuePoints;
        public int cunningPoints;
        public List<string> choiceHistory;
        
        public CVCData()
        {
            choiceHistory = new List<string>();
        }
    }
    
    #endregion
}