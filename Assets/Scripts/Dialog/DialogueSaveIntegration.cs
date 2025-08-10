using System;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;
using NeonLadderURP.DataManagement;
using NeonLadder.Debugging;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Integrates PixelCrushers Dialogue System with NeonLadder's EnhancedSaveSystem.
    /// Handles persistence of dialogue choices, CVC points, relationships, and conversation history.
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
        
        [Header("Save Settings")]
        [SerializeField] private bool autoSaveOnConversationEnd = true;
        [SerializeField] private bool saveDialogueHistory = true;
        [SerializeField] private int maxHistoryEntries = 100;
        
        [Header("CVC System")]
        [SerializeField] private bool trackCVCPoints = true;
        [SerializeField] private string courageVariableName = "CVC.Courage";
        [SerializeField] private string virtueVariableName = "CVC.Virtue";
        [SerializeField] private string cunningVariableName = "CVC.Cunning";
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;
        
        #endregion
        
        #region Private Fields
        
        private DialogueSystemSaveData currentDialogueData;
        private bool isInitialized = false;
        
        #endregion
        
        #region Events
        
        public static event Action<DialogueSystemSaveData> OnDialogueDataSaved;
        public static event Action<DialogueSystemSaveData> OnDialogueDataLoaded;
        public static event Action<string, int> OnCVCPointsChanged;
        
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
            currentDialogueData = new DialogueSystemSaveData();
            
            // Subscribe to Dialogue System events
            DialogueManager.instance.conversationStarted += OnConversationStarted;
            DialogueManager.instance.conversationEnded += OnConversationEnded;
            DialogueManager.instance.linkedConversationStart += OnLinkedConversationStart;
            
            // Subscribe to save system events
            EnhancedSaveSystem.OnSaveRequested += HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted += HandleLoadCompleted;
            
            // Register custom Lua functions for CVC
            RegisterCVCFunctions();
            
            isInitialized = true;
            LogDebug("DialogueSaveIntegration initialized");
        }
        
        private void OnDestroy()
        {
            if (DialogueManager.instance != null)
            {
                DialogueManager.instance.conversationStarted -= OnConversationStarted;
                DialogueManager.instance.conversationEnded -= OnConversationEnded;
                DialogueManager.instance.linkedConversationStart -= OnLinkedConversationStart;
            }
            
            EnhancedSaveSystem.OnSaveRequested -= HandleSaveRequested;
            EnhancedSaveSystem.OnLoadCompleted -= HandleLoadCompleted;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Save current dialogue state to the save system
        /// </summary>
        public void SaveDialogueData()
        {
            CaptureDialogueSystemState();
            
            // Get or create consolidated save data
            var consolidatedSave = EnhancedSaveSystem.Load() ?? ConsolidatedSaveData.CreateNew();
            
            // Serialize dialogue data to JSON
            string dialogueJson = JsonUtility.ToJson(currentDialogueData, true);
            consolidatedSave.dialogueDataJson = dialogueJson;
            
            // Save to file
            EnhancedSaveSystem.Save(consolidatedSave);
            
            OnDialogueDataSaved?.Invoke(currentDialogueData);
            LogDebug("Dialogue data saved");
        }
        
        /// <summary>
        /// Load dialogue state from the save system
        /// </summary>
        public void LoadDialogueData()
        {
            var consolidatedSave = EnhancedSaveSystem.Load();
            if (consolidatedSave == null || string.IsNullOrEmpty(consolidatedSave.dialogueDataJson))
            {
                LogDebug("No dialogue save data found");
                return;
            }
            
            try
            {
                currentDialogueData = JsonUtility.FromJson<DialogueSystemSaveData>(consolidatedSave.dialogueDataJson);
                RestoreDialogueSystemState();
                
                OnDialogueDataLoaded?.Invoke(currentDialogueData);
                LogDebug("Dialogue data loaded");
            }
            catch (Exception e)
            {
                LogError($"Failed to load dialogue data: {e.Message}");
            }
        }
        
        /// <summary>
        /// Get current CVC points
        /// </summary>
        public CVCPoints GetCVCPoints()
        {
            return new CVCPoints
            {
                courage = DialogueLua.GetVariable(courageVariableName).asInt,
                virtue = DialogueLua.GetVariable(virtueVariableName).asInt,
                cunning = DialogueLua.GetVariable(cunningVariableName).asInt
            };
        }
        
        /// <summary>
        /// Set CVC points
        /// </summary>
        public void SetCVCPoints(CVCPoints points)
        {
            DialogueLua.SetVariable(courageVariableName, points.courage);
            DialogueLua.SetVariable(virtueVariableName, points.virtue);
            DialogueLua.SetVariable(cunningVariableName, points.cunning);
            
            OnCVCPointsChanged?.Invoke("Courage", points.courage);
            OnCVCPointsChanged?.Invoke("Virtue", points.virtue);
            OnCVCPointsChanged?.Invoke("Cunning", points.cunning);
        }
        
        /// <summary>
        /// Add to dialogue history
        /// </summary>
        public void AddToHistory(string conversationTitle, int entryId, string text)
        {
            if (!saveDialogueHistory) return;
            
            var entry = new DialogueHistoryEntry
            {
                conversationTitle = conversationTitle,
                entryId = entryId,
                text = text,
                timestamp = DateTime.Now
            };
            
            currentDialogueData.dialogueHistory.Add(entry);
            
            // Trim history if too long
            while (currentDialogueData.dialogueHistory.Count > maxHistoryEntries)
            {
                currentDialogueData.dialogueHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Check if a conversation has been completed
        /// </summary>
        public bool HasCompletedConversation(string conversationTitle)
        {
            return currentDialogueData.completedConversations.Contains(conversationTitle);
        }
        
        /// <summary>
        /// Get relationship level with an NPC
        /// </summary>
        public int GetRelationship(string npcName)
        {
            return currentDialogueData.npcRelationships.TryGetValue(npcName, out int value) ? value : 0;
        }
        
        /// <summary>
        /// Set relationship level with an NPC
        /// </summary>
        public void SetRelationship(string npcName, int value)
        {
            currentDialogueData.npcRelationships[npcName] = value;
        }
        
        #endregion
        
        #region Private Methods
        
        private void CaptureDialogueSystemState()
        {
            // Capture current Dialogue System state
            currentDialogueData.dialogueSystemSaveData = PersistentDataManager.GetSaveData();
            
            // Capture CVC points
            if (trackCVCPoints)
            {
                currentDialogueData.cvcPoints = GetCVCPoints();
            }
            
            // Capture quest states
            CaptureQuestStates();
            
            // Capture variables
            CaptureVariables();
            
            // Update timestamp
            currentDialogueData.lastSaved = DateTime.Now;
        }
        
        private void RestoreDialogueSystemState()
        {
            // Restore Dialogue System state
            if (!string.IsNullOrEmpty(currentDialogueData.dialogueSystemSaveData))
            {
                PersistentDataManager.ApplySaveData(currentDialogueData.dialogueSystemSaveData);
            }
            
            // Restore CVC points
            if (trackCVCPoints && currentDialogueData.cvcPoints != null)
            {
                SetCVCPoints(currentDialogueData.cvcPoints);
            }
            
            // Restore quest states
            RestoreQuestStates();
            
            // Restore variables
            RestoreVariables();
        }
        
        private void CaptureQuestStates()
        {
            currentDialogueData.questStates.Clear();
            
            // Get all quest states from the Dialogue System
            foreach (var questTitle in QuestLog.GetAllQuests())
            {
                var state = QuestLog.GetQuestState(questTitle);
                currentDialogueData.questStates[questTitle] = state.ToString();
                
                // Capture quest entry states
                int entryCount = QuestLog.GetQuestEntryCount(questTitle);
                for (int i = 1; i <= entryCount; i++)
                {
                    var entryState = QuestLog.GetQuestEntryState(questTitle, i);
                    currentDialogueData.questEntryStates[$"{questTitle}_Entry{i}"] = entryState.ToString();
                }
            }
        }
        
        private void RestoreQuestStates()
        {
            foreach (var kvp in currentDialogueData.questStates)
            {
                if (Enum.TryParse<QuestState>(kvp.Value, out QuestState state))
                {
                    QuestLog.SetQuestState(kvp.Key, state);
                }
            }
            
            // Restore quest entry states
            foreach (var kvp in currentDialogueData.questEntryStates)
            {
                string[] parts = kvp.Key.Split(new[] { "_Entry" }, StringSplitOptions.None);
                if (parts.Length == 2 && int.TryParse(parts[1], out int entryNum))
                {
                    if (Enum.TryParse<QuestState>(kvp.Value, out QuestState state))
                    {
                        QuestLog.SetQuestEntryState(parts[0], entryNum, state);
                    }
                }
            }
        }
        
        private void CaptureVariables()
        {
            currentDialogueData.customVariables.Clear();
            
            // Capture all dialogue variables
            var database = DialogueManager.masterDatabase;
            if (database != null)
            {
                foreach (var variable in database.variables)
                {
                    var luaValue = DialogueLua.GetVariable(variable.Name);
                    if (luaValue != null)
                    {
                        currentDialogueData.customVariables[variable.Name] = luaValue.ToString();
                    }
                }
            }
        }
        
        private void RestoreVariables()
        {
            foreach (var kvp in currentDialogueData.customVariables)
            {
                // Skip CVC variables as they're handled separately
                if (kvp.Key.StartsWith("CVC.")) continue;
                
                // Try to parse as different types
                if (bool.TryParse(kvp.Value, out bool boolValue))
                {
                    DialogueLua.SetVariable(kvp.Key, boolValue);
                }
                else if (int.TryParse(kvp.Value, out int intValue))
                {
                    DialogueLua.SetVariable(kvp.Key, intValue);
                }
                else if (float.TryParse(kvp.Value, out float floatValue))
                {
                    DialogueLua.SetVariable(kvp.Key, floatValue);
                }
                else
                {
                    DialogueLua.SetVariable(kvp.Key, kvp.Value);
                }
            }
        }
        
        private void RegisterCVCFunctions()
        {
            // Register custom Lua functions for CVC system
            Lua.RegisterFunction("AddCourage", this, typeof(DialogueSaveIntegration).GetMethod("AddCourage"));
            Lua.RegisterFunction("AddVirtue", this, typeof(DialogueSaveIntegration).GetMethod("AddVirtue"));
            Lua.RegisterFunction("AddCunning", this, typeof(DialogueSaveIntegration).GetMethod("AddCunning"));
            Lua.RegisterFunction("GetCVCTotal", this, typeof(DialogueSaveIntegration).GetMethod("GetCVCTotal"));
        }
        
        #endregion
        
        #region Lua Functions
        
        public void AddCourage(double amount)
        {
            int current = DialogueLua.GetVariable(courageVariableName).asInt;
            int newValue = current + (int)amount;
            DialogueLua.SetVariable(courageVariableName, newValue);
            OnCVCPointsChanged?.Invoke("Courage", newValue);
            LogDebug($"Courage increased by {amount}, now {newValue}");
        }
        
        public void AddVirtue(double amount)
        {
            int current = DialogueLua.GetVariable(virtueVariableName).asInt;
            int newValue = current + (int)amount;
            DialogueLua.SetVariable(virtueVariableName, newValue);
            OnCVCPointsChanged?.Invoke("Virtue", newValue);
            LogDebug($"Virtue increased by {amount}, now {newValue}");
        }
        
        public void AddCunning(double amount)
        {
            int current = DialogueLua.GetVariable(cunningVariableName).asInt;
            int newValue = current + (int)amount;
            DialogueLua.SetVariable(cunningVariableName, newValue);
            OnCVCPointsChanged?.Invoke("Cunning", newValue);
            LogDebug($"Cunning increased by {amount}, now {newValue}");
        }
        
        public double GetCVCTotal()
        {
            var points = GetCVCPoints();
            return points.courage + points.virtue + points.cunning;
        }
        
        #endregion
        
        #region Event Handlers
        
        private void OnConversationStarted(Transform actor)
        {
            string conversationTitle = DialogueManager.lastConversationStarted;
            LogDebug($"Conversation started: {conversationTitle}");
            
            // Track conversation start
            if (!currentDialogueData.startedConversations.Contains(conversationTitle))
            {
                currentDialogueData.startedConversations.Add(conversationTitle);
            }
        }
        
        private void OnConversationEnded(Transform actor)
        {
            string conversationTitle = DialogueManager.lastConversationStarted;
            LogDebug($"Conversation ended: {conversationTitle}");
            
            // Mark conversation as completed
            if (!currentDialogueData.completedConversations.Contains(conversationTitle))
            {
                currentDialogueData.completedConversations.Add(conversationTitle);
            }
            
            // Auto-save if enabled
            if (autoSaveOnConversationEnd)
            {
                SaveDialogueData();
            }
        }
        
        private void OnLinkedConversationStart(Transform actor)
        {
            LogDebug("Linked conversation started");
        }
        
        private void HandleSaveRequested()
        {
            SaveDialogueData();
        }
        
        private void HandleLoadCompleted(ConsolidatedSaveData saveData)
        {
            if (saveData != null && !string.IsNullOrEmpty(saveData.dialogueDataJson))
            {
                try
                {
                    currentDialogueData = JsonUtility.FromJson<DialogueSystemSaveData>(saveData.dialogueDataJson);
                    RestoreDialogueSystemState();
                    LogDebug("Dialogue state restored from save");
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
                Debugger.Log($"[DialogueSaveIntegration] {message}");
            }
        }
        
        private void LogError(string message)
        {
            Debugger.LogError($"[DialogueSaveIntegration] {message}");
        }
        
        #endregion
    }
    
    #region Data Classes
    
    /// <summary>
    /// Container for all dialogue system save data
    /// </summary>
    [Serializable]
    public class DialogueSystemSaveData
    {
        // PixelCrushers internal save data
        public string dialogueSystemSaveData = "";
        
        // CVC Points
        public CVCPoints cvcPoints = new CVCPoints();
        
        // Conversation tracking
        public List<string> startedConversations = new List<string>();
        public List<string> completedConversations = new List<string>();
        
        // Dialogue history
        public List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();
        
        // Quest states
        public Dictionary<string, string> questStates = new Dictionary<string, string>();
        public Dictionary<string, string> questEntryStates = new Dictionary<string, string>();
        
        // NPC relationships
        public Dictionary<string, int> npcRelationships = new Dictionary<string, int>();
        
        // Custom variables
        public Dictionary<string, string> customVariables = new Dictionary<string, string>();
        
        // Metadata
        public DateTime lastSaved = DateTime.Now;
        public int saveVersion = 1;
    }
    
    /// <summary>
    /// CVC (Courage, Virtue, Cunning) points
    /// </summary>
    [Serializable]
    public class CVCPoints
    {
        public int courage = 0;
        public int virtue = 0;
        public int cunning = 0;
        
        public int Total => courage + virtue + cunning;
    }
    
    /// <summary>
    /// Entry in dialogue history
    /// </summary>
    [Serializable]
    public class DialogueHistoryEntry
    {
        public string conversationTitle;
        public int entryId;
        public string text;
        public DateTime timestamp;
    }
    
    #endregion
}