using System;
using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Managers;
using NeonLadder.Common;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Core integration layer for Dialogue System for Unity
    /// Provides foundation for complex branching conversations and NeonLadder system integration
    /// Part of the Disco Elysium-style dialog implementation
    /// </summary>
    public class DialogueSystemIntegration : MonoBehaviour
    {
        #region Singleton
        
        private static DialogueSystemIntegration instance;
        public static DialogueSystemIntegration Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DialogueSystemIntegration>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DialogueSystemIntegration");
                        instance = go.AddComponent<DialogueSystemIntegration>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
        
        #endregion
        
        #region Configuration
        
        [Header("Database Configuration")]
        [SerializeField] private DialogueDatabaseConfig databaseConfig;
        [SerializeField] private bool autoLoadDatabase = true;
        [SerializeField] private string defaultDatabasePath = "DialogueData/MainDatabase";
        
        [Header("Integration Settings")]
        [SerializeField] private bool integrateWithSaveSystem = true;
        [SerializeField] private bool integrateWithCurrencySystem = true;
        [SerializeField] private bool integrateWithProgressionSystem = true;
        [SerializeField] private bool enableAnalytics = true;
        
        [Header("Performance")]
        [SerializeField] private int maxActiveConversations = 3;
        [SerializeField] private bool preloadCommonDialogues = true;
        [SerializeField] private float dialogueMemoryCacheMB = 10f;
        
        #endregion
        
        #region Private Fields
        
        private DialogueDatabase currentDatabase;
        private Dictionary<string, ConversationState> activeConversations;
        private Dictionary<string, DialogueVariable> dialogueVariables;
        private Queue<DialogueEvent> pendingEvents;
        private DialogueAnalytics analytics;
        private bool isInitialized = false;
        
        #endregion
        
        #region Events
        
        public static event Action<string> OnConversationStarted;
        public static event Action<string> OnConversationEnded;
        public static event Action<DialogueChoice> OnChoiceMade;
        public static event Action<string, object> OnVariableChanged;
        public static event Action<CurrencyReward> OnCurrencyRewarded;
        public static event Action<ProgressionUnlock> OnProgressionUnlocked;
        
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
            
            InitializeSystem();
        }
        
        private void InitializeSystem()
        {
            activeConversations = new Dictionary<string, ConversationState>();
            dialogueVariables = new Dictionary<string, DialogueVariable>();
            pendingEvents = new Queue<DialogueEvent>();
            
            if (enableAnalytics)
            {
                analytics = new DialogueAnalytics();
            }
            
            if (autoLoadDatabase)
            {
                LoadDatabase(defaultDatabasePath);
            }
            
            SetupIntegrations();
            
            isInitialized = true;
            Debug.Log("[DialogueSystem] ✅ Integration initialized successfully");
        }
        
        private void SetupIntegrations()
        {
            // Save System Integration
            if (integrateWithSaveSystem)
            {
                SetupSaveSystemIntegration();
            }
            
            // Currency System Integration
            if (integrateWithCurrencySystem)
            {
                SetupCurrencyIntegration();
            }
            
            // Progression System Integration
            if (integrateWithProgressionSystem)
            {
                SetupProgressionIntegration();
            }
        }
        
        #endregion
        
        #region Database Management
        
        public void LoadDatabase(string databasePath)
        {
            try
            {
                var resource = Resources.Load<DialogueDatabaseAsset>(databasePath);
                if (resource != null)
                {
                    currentDatabase = new DialogueDatabase(resource);
                    
                    if (preloadCommonDialogues)
                    {
                        PreloadCommonConversations();
                    }
                    
                    Debug.Log($"[DialogueSystem] Loaded database: {databasePath}");
                }
                else
                {
                    Debug.LogError($"[DialogueSystem] Failed to load database at: {databasePath}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[DialogueSystem] Error loading database: {e.Message}");
            }
        }
        
        public void CreateDatabase(string databaseName)
        {
            currentDatabase = new DialogueDatabase(databaseName);
            Debug.Log($"[DialogueSystem] Created new database: {databaseName}");
        }
        
        private void PreloadCommonConversations()
        {
            if (currentDatabase == null) return;
            
            var commonConversations = currentDatabase.GetConversationsByTag("common");
            foreach (var conversation in commonConversations)
            {
                // Cache conversation data
                conversation.Preload();
            }
        }
        
        #endregion
        
        #region Conversation Management
        
        public bool StartConversation(string conversationName, Transform actor = null)
        {
            if (!isInitialized || currentDatabase == null)
            {
                Debug.LogError("[DialogueSystem] System not initialized or database not loaded");
                return false;
            }
            
            if (activeConversations.Count >= maxActiveConversations)
            {
                Debug.LogWarning("[DialogueSystem] Maximum active conversations reached");
                return false;
            }
            
            var conversation = currentDatabase.GetConversation(conversationName);
            if (conversation == null)
            {
                Debug.LogError($"[DialogueSystem] Conversation not found: {conversationName}");
                return false;
            }
            
            var state = new ConversationState
            {
                ConversationName = conversationName,
                CurrentNodeId = conversation.StartNodeId,
                StartTime = Time.time,
                Actor = actor,
                Variables = new Dictionary<string, object>()
            };
            
            activeConversations[conversationName] = state;
            
            OnConversationStarted?.Invoke(conversationName);
            
            if (analytics != null)
            {
                analytics.TrackConversationStart(conversationName);
            }
            
            Debug.Log($"[DialogueSystem] Started conversation: {conversationName}");
            return true;
        }
        
        public void EndConversation(string conversationName)
        {
            if (!activeConversations.ContainsKey(conversationName))
            {
                Debug.LogWarning($"[DialogueSystem] Conversation not active: {conversationName}");
                return;
            }
            
            var state = activeConversations[conversationName];
            
            // Process any pending rewards or consequences
            ProcessConversationConsequences(state);
            
            activeConversations.Remove(conversationName);
            
            OnConversationEnded?.Invoke(conversationName);
            
            if (analytics != null)
            {
                analytics.TrackConversationEnd(conversationName, Time.time - state.StartTime);
            }
            
            Debug.Log($"[DialogueSystem] Ended conversation: {conversationName}");
        }
        
        public DialogueNode GetCurrentNode(string conversationName)
        {
            if (!activeConversations.ContainsKey(conversationName))
                return null;
            
            var state = activeConversations[conversationName];
            var conversation = currentDatabase.GetConversation(conversationName);
            
            return conversation?.GetNode(state.CurrentNodeId);
        }
        
        public void SelectChoice(string conversationName, int choiceIndex)
        {
            if (!activeConversations.ContainsKey(conversationName))
            {
                Debug.LogError($"[DialogueSystem] Conversation not active: {conversationName}");
                return;
            }
            
            var state = activeConversations[conversationName];
            var conversation = currentDatabase.GetConversation(conversationName);
            var currentNode = conversation.GetNode(state.CurrentNodeId);
            
            if (currentNode == null || choiceIndex >= currentNode.Choices.Count)
            {
                Debug.LogError("[DialogueSystem] Invalid choice index");
                return;
            }
            
            var choice = currentNode.Choices[choiceIndex];
            
            // Check conditions
            if (!EvaluateConditions(choice.Conditions))
            {
                Debug.LogWarning("[DialogueSystem] Choice conditions not met");
                return;
            }
            
            // Apply consequences
            ApplyConsequences(choice.Consequences);
            
            // Update state
            state.CurrentNodeId = choice.NextNodeId;
            state.ChoiceHistory.Add(new ChoiceRecord
            {
                NodeId = currentNode.Id,
                ChoiceIndex = choiceIndex,
                Timestamp = Time.time
            });
            
            OnChoiceMade?.Invoke(choice);
            
            if (analytics != null)
            {
                analytics.TrackChoice(conversationName, currentNode.Id, choiceIndex);
            }
            
            // Check if conversation should end
            if (string.IsNullOrEmpty(choice.NextNodeId) || choice.EndsConversation)
            {
                EndConversation(conversationName);
            }
        }
        
        #endregion
        
        #region Variable Management
        
        public void SetVariable(string variableName, object value)
        {
            if (!dialogueVariables.ContainsKey(variableName))
            {
                dialogueVariables[variableName] = new DialogueVariable
                {
                    Name = variableName,
                    Value = value,
                    Type = value?.GetType()
                };
            }
            else
            {
                dialogueVariables[variableName].Value = value;
            }
            
            OnVariableChanged?.Invoke(variableName, value);
        }
        
        public T GetVariable<T>(string variableName, T defaultValue = default)
        {
            if (dialogueVariables.ContainsKey(variableName))
            {
                try
                {
                    return (T)dialogueVariables[variableName].Value;
                }
                catch
                {
                    Debug.LogWarning($"[DialogueSystem] Variable type mismatch: {variableName}");
                }
            }
            
            return defaultValue;
        }
        
        public bool HasVariable(string variableName)
        {
            return dialogueVariables.ContainsKey(variableName);
        }
        
        #endregion
        
        #region Condition Evaluation
        
        private bool EvaluateConditions(List<DialogueCondition> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true;
            
            foreach (var condition in conditions)
            {
                if (!EvaluateCondition(condition))
                    return false;
            }
            
            return true;
        }
        
        private bool EvaluateCondition(DialogueCondition condition)
        {
            switch (condition.Type)
            {
                case ConditionType.Variable:
                    return EvaluateVariableCondition(condition);
                    
                case ConditionType.Currency:
                    return EvaluateCurrencyCondition(condition);
                    
                case ConditionType.Progression:
                    return EvaluateProgressionCondition(condition);
                    
                case ConditionType.Skill:
                    return EvaluateSkillCondition(condition);
                    
                default:
                    return true;
            }
        }
        
        private bool EvaluateVariableCondition(DialogueCondition condition)
        {
            if (!HasVariable(condition.VariableName))
                return false;
            
            var value = GetVariable<IComparable>(condition.VariableName);
            var targetValue = condition.TargetValue as IComparable;
            
            switch (condition.Operator)
            {
                case ComparisonOperator.Equals:
                    return value?.CompareTo(targetValue) == 0;
                case ComparisonOperator.NotEquals:
                    return value?.CompareTo(targetValue) != 0;
                case ComparisonOperator.GreaterThan:
                    return value?.CompareTo(targetValue) > 0;
                case ComparisonOperator.LessThan:
                    return value?.CompareTo(targetValue) < 0;
                default:
                    return false;
            }
        }
        
        private bool EvaluateCurrencyCondition(DialogueCondition condition)
        {
            // Integration with currency system
            // This would check player's currency against condition
            return true; // Placeholder
        }
        
        private bool EvaluateProgressionCondition(DialogueCondition condition)
        {
            // Integration with progression system
            // This would check player's progression state
            return true; // Placeholder
        }
        
        private bool EvaluateSkillCondition(DialogueCondition condition)
        {
            // Check if player has required skill level
            // This integrates with the skill system
            return true; // Placeholder
        }
        
        #endregion
        
        #region Consequence Application
        
        private void ApplyConsequences(List<DialogueConsequence> consequences)
        {
            if (consequences == null) return;
            
            foreach (var consequence in consequences)
            {
                ApplyConsequence(consequence);
            }
        }
        
        private void ApplyConsequence(DialogueConsequence consequence)
        {
            switch (consequence.Type)
            {
                case ConsequenceType.Variable:
                    SetVariable(consequence.VariableName, consequence.Value);
                    break;
                    
                case ConsequenceType.Currency:
                    ApplyCurrencyConsequence(consequence);
                    break;
                    
                case ConsequenceType.Progression:
                    ApplyProgressionConsequence(consequence);
                    break;
                    
                case ConsequenceType.Trigger:
                    TriggerEvent(consequence.EventName);
                    break;
            }
        }
        
        private void ApplyCurrencyConsequence(DialogueConsequence consequence)
        {
            var reward = new CurrencyReward
            {
                CurrencyType = consequence.CurrencyType,
                Amount = consequence.Amount
            };
            
            OnCurrencyRewarded?.Invoke(reward);
        }
        
        private void ApplyProgressionConsequence(DialogueConsequence consequence)
        {
            var unlock = new ProgressionUnlock
            {
                UnlockType = consequence.UnlockType,
                UnlockId = consequence.UnlockId
            };
            
            OnProgressionUnlocked?.Invoke(unlock);
        }
        
        private void ProcessConversationConsequences(ConversationState state)
        {
            // Process any final consequences when conversation ends
            // This could include reputation changes, quest updates, etc.
        }
        
        #endregion
        
        #region Event System
        
        public void TriggerEvent(string eventName)
        {
            var dialogueEvent = new DialogueEvent
            {
                EventName = eventName,
                Timestamp = Time.time
            };
            
            pendingEvents.Enqueue(dialogueEvent);
            ProcessPendingEvents();
        }
        
        private void ProcessPendingEvents()
        {
            while (pendingEvents.Count > 0)
            {
                var evt = pendingEvents.Dequeue();
                // Process event through game systems
                Debug.Log($"[DialogueSystem] Processing event: {evt.EventName}");
            }
        }
        
        #endregion
        
        #region Save System Integration
        
        private void SetupSaveSystemIntegration()
        {
            // Register with save system to persist dialogue state
            Debug.Log("[DialogueSystem] Save system integration setup");
        }
        
        public DialogueSaveData GetSaveData()
        {
            return new DialogueSaveData
            {
                Variables = new Dictionary<string, object>(dialogueVariables.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value)),
                ConversationStates = activeConversations.Values.ToList(),
                CompletedConversations = GetCompletedConversations()
            };
        }
        
        public void LoadSaveData(DialogueSaveData saveData)
        {
            if (saveData == null) return;
            
            // Restore variables
            foreach (var kvp in saveData.Variables)
            {
                SetVariable(kvp.Key, kvp.Value);
            }
            
            // Restore conversation states if needed
            // This would typically only restore persistent conversation flags
        }
        
        private List<string> GetCompletedConversations()
        {
            // Return list of completed conversation IDs
            return new List<string>();
        }
        
        #endregion
        
        #region Currency Integration
        
        private void SetupCurrencyIntegration()
        {
            // Connect to NeonLadder's currency system
            Debug.Log("[DialogueSystem] Currency system integration setup");
        }
        
        #endregion
        
        #region Progression Integration
        
        private void SetupProgressionIntegration()
        {
            // Connect to NeonLadder's progression system
            Debug.Log("[DialogueSystem] Progression system integration setup");
        }
        
        #endregion
        
        #region Analytics
        
        public DialogueAnalyticsReport GetAnalyticsReport()
        {
            return analytics?.GenerateReport();
        }
        
        #endregion
        
        #region Utility Methods
        
        public bool IsConversationActive(string conversationName)
        {
            return activeConversations.ContainsKey(conversationName);
        }
        
        public List<string> GetActiveConversations()
        {
            return new List<string>(activeConversations.Keys);
        }
        
        public void ClearAllConversations()
        {
            foreach (var conversation in activeConversations.Keys.ToList())
            {
                EndConversation(conversation);
            }
        }
        
        #endregion
    }
    
    #region Data Classes
    
    [Serializable]
    public class DialogueDatabaseConfig
    {
        public string databaseName = "MainDialogueDatabase";
        public string version = "1.0.0";
        public List<string> supportedLanguages = new List<string> { "en", "zh-Hans", "ur" };
        public bool enableLocalization = true;
        public bool enableVoiceActing = false;
    }
    
    [Serializable]
    public class ConversationState
    {
        public string ConversationName;
        public string CurrentNodeId;
        public float StartTime;
        public Transform Actor;
        public Dictionary<string, object> Variables;
        public List<ChoiceRecord> ChoiceHistory = new List<ChoiceRecord>();
    }
    
    [Serializable]
    public class ChoiceRecord
    {
        public string NodeId;
        public int ChoiceIndex;
        public float Timestamp;
    }
    
    [Serializable]
    public class DialogueVariable
    {
        public string Name;
        public object Value;
        public Type Type;
    }
    
    [Serializable]
    public class DialogueEvent
    {
        public string EventName;
        public float Timestamp;
        public Dictionary<string, object> Parameters;
    }
    
    [Serializable]
    public class CurrencyReward
    {
        public string CurrencyType;
        public int Amount;
    }
    
    [Serializable]
    public class ProgressionUnlock
    {
        public string UnlockType;
        public string UnlockId;
    }
    
    [Serializable]
    public class DialogueSaveData
    {
        public Dictionary<string, object> Variables;
        public List<ConversationState> ConversationStates;
        public List<string> CompletedConversations;
    }
    
    public class DialogueDatabase
    {
        public string Name { get; private set; }
        private Dictionary<string, DialogueConversation> conversations;
        
        public DialogueDatabase(string name)
        {
            Name = name;
            conversations = new Dictionary<string, DialogueConversation>();
        }
        
        public DialogueDatabase(DialogueDatabaseAsset asset)
        {
            Name = asset.name;
            conversations = new Dictionary<string, DialogueConversation>();
            // Load conversations from asset
        }
        
        public DialogueConversation GetConversation(string name)
        {
            return conversations.ContainsKey(name) ? conversations[name] : null;
        }
        
        public List<DialogueConversation> GetConversationsByTag(string tag)
        {
            return conversations.Values.Where(c => c.Tags.Contains(tag)).ToList();
        }
    }
    
    public class DialogueConversation
    {
        public string Name { get; set; }
        public string StartNodeId { get; set; }
        public List<string> Tags { get; set; }
        private Dictionary<string, DialogueNode> nodes;
        
        public DialogueNode GetNode(string nodeId)
        {
            return nodes.ContainsKey(nodeId) ? nodes[nodeId] : null;
        }
        
        public void Preload()
        {
            // Preload conversation data
        }
    }
    
    public class DialogueNode
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Speaker { get; set; }
        public List<DialogueChoice> Choices { get; set; }
    }
    
    public class DialogueChoice
    {
        public string Text { get; set; }
        public string NextNodeId { get; set; }
        public bool EndsConversation { get; set; }
        public List<DialogueCondition> Conditions { get; set; }
        public List<DialogueConsequence> Consequences { get; set; }
    }
    
    public class DialogueCondition
    {
        public ConditionType Type { get; set; }
        public string VariableName { get; set; }
        public ComparisonOperator Operator { get; set; }
        public object TargetValue { get; set; }
        public string CurrencyType { get; set; }
        public int RequiredAmount { get; set; }
    }
    
    public class DialogueConsequence
    {
        public ConsequenceType Type { get; set; }
        public string VariableName { get; set; }
        public object Value { get; set; }
        public string CurrencyType { get; set; }
        public int Amount { get; set; }
        public string EventName { get; set; }
        public string UnlockType { get; set; }
        public string UnlockId { get; set; }
    }
    
    public enum ConditionType
    {
        Variable,
        Currency,
        Progression,
        Skill
    }
    
    public enum ConsequenceType
    {
        Variable,
        Currency,
        Progression,
        Trigger
    }
    
    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }
    
    // Placeholder for asset reference
    public class DialogueDatabaseAsset : ScriptableObject
    {
        // This would be the actual Dialogue System for Unity database asset
    }
    
    public class DialogueAnalytics
    {
        private List<AnalyticsEvent> events = new List<AnalyticsEvent>();
        
        public void TrackConversationStart(string conversationName)
        {
            events.Add(new AnalyticsEvent
            {
                Type = "ConversationStart",
                Data = conversationName,
                Timestamp = Time.time
            });
        }
        
        public void TrackConversationEnd(string conversationName, float duration)
        {
            events.Add(new AnalyticsEvent
            {
                Type = "ConversationEnd",
                Data = conversationName,
                Duration = duration,
                Timestamp = Time.time
            });
        }
        
        public void TrackChoice(string conversationName, string nodeId, int choiceIndex)
        {
            events.Add(new AnalyticsEvent
            {
                Type = "Choice",
                Data = $"{conversationName}:{nodeId}:{choiceIndex}",
                Timestamp = Time.time
            });
        }
        
        public DialogueAnalyticsReport GenerateReport()
        {
            return new DialogueAnalyticsReport
            {
                TotalEvents = events.Count,
                Events = new List<AnalyticsEvent>(events)
            };
        }
        
        private class AnalyticsEvent
        {
            public string Type;
            public string Data;
            public float Duration;
            public float Timestamp;
        }
    }
    
    public class DialogueAnalyticsReport
    {
        public int TotalEvents;
        public List<object> Events;
    }
    
    #endregion
}