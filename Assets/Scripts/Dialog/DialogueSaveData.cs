using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Serializable data structure for dialogue system persistence.
    /// Integrates with PixelCrushers Dialogue System for Unity.
    /// </summary>
    [Serializable]
    public class DialogueSaveData
    {
        // Conversation State
        [SerializeField] public List<ConversationState> conversationStates = new List<ConversationState>();
        
        // Variable State (Dialogue System Variables)
        [SerializeField] public List<DialogueVariable> dialogueVariables = new List<DialogueVariable>();
        
        // Quest State
        [SerializeField] public List<QuestState> questStates = new List<QuestState>();
        
        // Actor State (NPC relationships, attitudes)
        [SerializeField] public List<ActorState> actorStates = new List<ActorState>();
        
        // Player Choices History
        [SerializeField] public List<DialogueChoice> choiceHistory = new List<DialogueChoice>();
        
        // CVC (Character Value Core) System
        [SerializeField] public CVCData cvcData = new CVCData();
        
        // Metadata
        [SerializeField] public string lastConversationId;
        [SerializeField] public int totalConversationsPlayed;
        [SerializeField] public float totalDialogueTime;
        [SerializeField] public DateTime lastDialogueTimestamp;
        
        /// <summary>
        /// Represents the state of a conversation
        /// </summary>
        [Serializable]
        public class ConversationState
        {
            public int conversationId;
            public string conversationTitle;
            public bool hasBeenPlayed;
            public int timesPlayed;
            public List<int> nodesVisited = new List<int>();
            public DateTime lastPlayed;
        }
        
        /// <summary>
        /// Represents a Dialogue System variable
        /// </summary>
        [Serializable]
        public class DialogueVariable
        {
            public string name;
            public string value;
            public VariableType type;
            
            public enum VariableType
            {
                Boolean,
                Number,
                Text
            }
        }
        
        /// <summary>
        /// Represents quest state
        /// </summary>
        [Serializable]
        public class QuestState
        {
            public string questName;
            public string currentState; // unassigned, active, success, failure, done
            public int currentEntryId;
            public List<int> completedEntries = new List<int>();
            public Dictionary<string, bool> questFlags = new Dictionary<string, bool>();
        }
        
        /// <summary>
        /// Represents an actor's state (NPC)
        /// </summary>
        [Serializable]
        public class ActorState
        {
            public string actorName;
            public int actorId;
            public float relationshipLevel; // -100 to 100
            public string currentMood;
            public Dictionary<string, float> attributes = new Dictionary<string, float>();
            public List<string> rememberedTopics = new List<string>();
        }
        
        /// <summary>
        /// Represents a dialogue choice made by the player
        /// </summary>
        [Serializable]
        public class DialogueChoice
        {
            public int conversationId;
            public int nodeId;
            public string choiceText;
            public DateTime timestamp;
            public string consequence;
            public Dictionary<string, int> cvcImpact = new Dictionary<string, int>();
        }
        
        /// <summary>
        /// Character Value Core data
        /// </summary>
        [Serializable]
        public class CVCData
        {
            // Core values
            public int courage = 0;
            public int virtue = 0;
            public int cunning = 0;
            
            // Derived values
            public int reputation = 0;
            public int karma = 0;
            
            // Value history for tracking changes
            public List<CVCChange> changeHistory = new List<CVCChange>();
            
            [Serializable]
            public class CVCChange
            {
                public string valueName;
                public int oldValue;
                public int newValue;
                public string reason;
                public DateTime timestamp;
            }
        }
        
        /// <summary>
        /// Creates a new empty dialogue save data
        /// </summary>
        public static DialogueSaveData CreateNew()
        {
            return new DialogueSaveData
            {
                lastDialogueTimestamp = DateTime.Now,
                cvcData = new CVCData()
            };
        }
        
        /// <summary>
        /// Validates the save data
        /// </summary>
        public bool Validate()
        {
            // Check for null references
            if (conversationStates == null) conversationStates = new List<ConversationState>();
            if (dialogueVariables == null) dialogueVariables = new List<DialogueVariable>();
            if (questStates == null) questStates = new List<QuestState>();
            if (actorStates == null) actorStates = new List<ActorState>();
            if (choiceHistory == null) choiceHistory = new List<DialogueChoice>();
            if (cvcData == null) cvcData = new CVCData();
            
            return true;
        }
        
        /// <summary>
        /// Merges another dialogue save data into this one
        /// </summary>
        public void MergeWith(DialogueSaveData other)
        {
            if (other == null) return;
            
            // Merge conversation states
            foreach (var otherConv in other.conversationStates)
            {
                var existing = conversationStates.Find(c => c.conversationId == otherConv.conversationId);
                if (existing != null)
                {
                    existing.timesPlayed = Math.Max(existing.timesPlayed, otherConv.timesPlayed);
                    existing.hasBeenPlayed = existing.hasBeenPlayed || otherConv.hasBeenPlayed;
                    foreach (var node in otherConv.nodesVisited)
                    {
                        if (!existing.nodesVisited.Contains(node))
                            existing.nodesVisited.Add(node);
                    }
                }
                else
                {
                    conversationStates.Add(otherConv);
                }
            }
            
            // Merge variables (newer values win)
            foreach (var otherVar in other.dialogueVariables)
            {
                var existing = dialogueVariables.Find(v => v.name == otherVar.name);
                if (existing != null)
                {
                    existing.value = otherVar.value;
                }
                else
                {
                    dialogueVariables.Add(otherVar);
                }
            }
            
            // Merge CVC data (take maximum values)
            cvcData.courage = Math.Max(cvcData.courage, other.cvcData.courage);
            cvcData.virtue = Math.Max(cvcData.virtue, other.cvcData.virtue);
            cvcData.cunning = Math.Max(cvcData.cunning, other.cvcData.cunning);
            cvcData.reputation = Math.Max(cvcData.reputation, other.cvcData.reputation);
            cvcData.karma = Math.Max(cvcData.karma, other.cvcData.karma);
        }
    }
}