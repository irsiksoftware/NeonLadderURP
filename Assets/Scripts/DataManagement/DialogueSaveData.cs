using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonLadderURP.DataManagement
{
    /// <summary>
    /// Comprehensive dialogue save data structure for persisting narrative state
    /// Integrates with EnhancedSaveSystem for dialogue continuity across sessions
    /// </summary>
    [Serializable]
    public class DialogueSaveData
    {
        [Header("Conversation History")]
        public List<ConversationRecord> conversationHistory = new List<ConversationRecord>();
        public Dictionary<string, List<int>> choicesMadePerConversation = new Dictionary<string, List<int>>();
        public List<string> completedConversations = new List<string>();
        public string lastActiveConversation = "";
        public float totalDialogueTime = 0f;
        
        [Header("Character Relationships")]
        public Dictionary<string, CharacterRelationshipData> characterRelationships = new Dictionary<string, CharacterRelationshipData>();
        public Dictionary<string, CVCLevelData> cvcLevels = new Dictionary<string, CVCLevelData>();
        
        [Header("Dialogue Unlocks")]
        public List<string> unlockedDialoguePaths = new List<string>();
        public List<string> unlockedCharacters = new List<string>();
        public List<string> unlockedTopics = new List<string>();
        
        [Header("Current State")]
        public ConversationStateData currentConversationState = null;
        public Dictionary<string, object> dialogueVariables = new Dictionary<string, object>();
        
        [Header("Statistics")]
        public int totalChoicesMade = 0;
        public int totalConversationsStarted = 0;
        public int totalConversationsCompleted = 0;
        public Dictionary<string, int> choiceTypeFrequency = new Dictionary<string, int>();
        
        /// <summary>
        /// Record a conversation being started
        /// </summary>
        public void RecordConversationStart(string conversationId, string characterName = null)
        {
            var record = new ConversationRecord
            {
                conversationId = conversationId,
                characterName = characterName,
                startTime = DateTime.Now,
                choices = new List<ChoiceRecord>()
            };
            
            conversationHistory.Add(record);
            lastActiveConversation = conversationId;
            totalConversationsStarted++;
        }
        
        /// <summary>
        /// Record a choice made in a conversation
        /// </summary>
        public void RecordChoice(string conversationId, int choiceIndex, string choiceText, string choiceType = null)
        {
            // Add to conversation history
            var currentConversation = GetCurrentConversation(conversationId);
            if (currentConversation != null)
            {
                currentConversation.choices.Add(new ChoiceRecord
                {
                    choiceIndex = choiceIndex,
                    choiceText = choiceText,
                    timestamp = DateTime.Now
                });
            }
            
            // Track choices per conversation
            if (!choicesMadePerConversation.ContainsKey(conversationId))
            {
                choicesMadePerConversation[conversationId] = new List<int>();
            }
            choicesMadePerConversation[conversationId].Add(choiceIndex);
            
            // Update statistics
            totalChoicesMade++;
            
            if (!string.IsNullOrEmpty(choiceType))
            {
                if (!choiceTypeFrequency.ContainsKey(choiceType))
                {
                    choiceTypeFrequency[choiceType] = 0;
                }
                choiceTypeFrequency[choiceType]++;
            }
        }
        
        /// <summary>
        /// Record conversation completion
        /// </summary>
        public void RecordConversationComplete(string conversationId)
        {
            var conversation = GetCurrentConversation(conversationId);
            if (conversation != null)
            {
                conversation.endTime = DateTime.Now;
                conversation.completed = true;
            }
            
            if (!completedConversations.Contains(conversationId))
            {
                completedConversations.Add(conversationId);
                totalConversationsCompleted++;
            }
        }
        
        /// <summary>
        /// Update character relationship data
        /// </summary>
        public void UpdateRelationship(string characterName, int relationshipDelta, string relationshipType = "default")
        {
            if (!characterRelationships.ContainsKey(characterName))
            {
                characterRelationships[characterName] = new CharacterRelationshipData
                {
                    characterName = characterName
                };
            }
            
            var relationship = characterRelationships[characterName];
            relationship.relationshipPoints += relationshipDelta;
            relationship.lastInteraction = DateTime.Now;
            relationship.totalInteractions++;
            
            // Track relationship type changes
            if (!relationship.relationshipHistory.ContainsKey(relationshipType))
            {
                relationship.relationshipHistory[relationshipType] = 0;
            }
            relationship.relationshipHistory[relationshipType] += relationshipDelta;
        }
        
        /// <summary>
        /// Update CVC (Charisma, Violence, Coercion) levels
        /// </summary>
        public void UpdateCVCLevel(string characterName, CVCType cvcType, int pointsDelta)
        {
            string key = $"{characterName}_{cvcType}";
            
            if (!cvcLevels.ContainsKey(key))
            {
                cvcLevels[key] = new CVCLevelData
                {
                    characterName = characterName,
                    cvcType = cvcType,
                    currentLevel = 0,
                    currentPoints = 0
                };
            }
            
            var cvcData = cvcLevels[key];
            cvcData.currentPoints += pointsDelta;
            cvcData.totalPointsEarned += Math.Max(0, pointsDelta);
            
            // Calculate level based on points (example: 100 points per level)
            int newLevel = cvcData.currentPoints / 100;
            if (newLevel > cvcData.currentLevel)
            {
                cvcData.currentLevel = newLevel;
                cvcData.levelUpCount++;
            }
        }
        
        /// <summary>
        /// Unlock a dialogue path
        /// </summary>
        public void UnlockDialoguePath(string pathId)
        {
            if (!unlockedDialoguePaths.Contains(pathId))
            {
                unlockedDialoguePaths.Add(pathId);
            }
        }
        
        /// <summary>
        /// Save current conversation state for resuming
        /// </summary>
        public void SaveConversationState(string conversationId, string currentNodeId, Dictionary<string, object> localVariables)
        {
            currentConversationState = new ConversationStateData
            {
                conversationId = conversationId,
                currentNodeId = currentNodeId,
                savedTime = DateTime.Now,
                localVariables = new Dictionary<string, object>(localVariables)
            };
        }
        
        /// <summary>
        /// Clear current conversation state
        /// </summary>
        public void ClearConversationState()
        {
            currentConversationState = null;
        }
        
        /// <summary>
        /// Set a dialogue variable
        /// </summary>
        public void SetDialogueVariable(string variableName, object value)
        {
            dialogueVariables[variableName] = value;
        }
        
        /// <summary>
        /// Get a dialogue variable
        /// </summary>
        public T GetDialogueVariable<T>(string variableName, T defaultValue = default)
        {
            if (dialogueVariables.ContainsKey(variableName))
            {
                try
                {
                    return (T)dialogueVariables[variableName];
                }
                catch
                {
                    Debug.LogWarning($"[DialogueSaveData] Variable type mismatch for: {variableName}");
                }
            }
            return defaultValue;
        }
        
        /// <summary>
        /// Check if a conversation has been completed
        /// </summary>
        public bool HasCompletedConversation(string conversationId)
        {
            return completedConversations.Contains(conversationId);
        }
        
        /// <summary>
        /// Get choices made in a specific conversation
        /// </summary>
        public List<int> GetChoicesForConversation(string conversationId)
        {
            return choicesMadePerConversation.ContainsKey(conversationId) 
                ? new List<int>(choicesMadePerConversation[conversationId]) 
                : new List<int>();
        }
        
        /// <summary>
        /// Get relationship data for a character
        /// </summary>
        public CharacterRelationshipData GetRelationship(string characterName)
        {
            return characterRelationships.ContainsKey(characterName) 
                ? characterRelationships[characterName] 
                : null;
        }
        
        /// <summary>
        /// Get CVC level for a character
        /// </summary>
        public CVCLevelData GetCVCLevel(string characterName, CVCType cvcType)
        {
            string key = $"{characterName}_{cvcType}";
            return cvcLevels.ContainsKey(key) ? cvcLevels[key] : null;
        }
        
        /// <summary>
        /// Reset all dialogue data
        /// </summary>
        public void Reset()
        {
            conversationHistory.Clear();
            choicesMadePerConversation.Clear();
            completedConversations.Clear();
            characterRelationships.Clear();
            cvcLevels.Clear();
            unlockedDialoguePaths.Clear();
            unlockedCharacters.Clear();
            unlockedTopics.Clear();
            dialogueVariables.Clear();
            currentConversationState = null;
            lastActiveConversation = "";
            totalDialogueTime = 0f;
            totalChoicesMade = 0;
            totalConversationsStarted = 0;
            totalConversationsCompleted = 0;
            choiceTypeFrequency.Clear();
        }
        
        /// <summary>
        /// Get analytics summary
        /// </summary>
        public DialogueAnalytics GetAnalytics()
        {
            return new DialogueAnalytics
            {
                totalConversations = totalConversationsStarted,
                completedConversations = totalConversationsCompleted,
                totalChoices = totalChoicesMade,
                averageChoicesPerConversation = totalConversationsCompleted > 0 
                    ? (float)totalChoicesMade / totalConversationsCompleted 
                    : 0,
                mostFrequentChoiceType = GetMostFrequentChoiceType(),
                totalPlayTime = totalDialogueTime,
                uniqueCharactersInteracted = characterRelationships.Count
            };
        }
        
        private ConversationRecord GetCurrentConversation(string conversationId)
        {
            for (int i = conversationHistory.Count - 1; i >= 0; i--)
            {
                if (conversationHistory[i].conversationId == conversationId && !conversationHistory[i].completed)
                {
                    return conversationHistory[i];
                }
            }
            return null;
        }
        
        private string GetMostFrequentChoiceType()
        {
            string mostFrequent = null;
            int maxCount = 0;
            
            foreach (var kvp in choiceTypeFrequency)
            {
                if (kvp.Value > maxCount)
                {
                    maxCount = kvp.Value;
                    mostFrequent = kvp.Key;
                }
            }
            
            return mostFrequent ?? "none";
        }
    }
    
    /// <summary>
    /// Record of a single conversation
    /// </summary>
    [Serializable]
    public class ConversationRecord
    {
        public string conversationId;
        public string characterName;
        public DateTime startTime;
        public DateTime endTime;
        public List<ChoiceRecord> choices;
        public bool completed;
    }
    
    /// <summary>
    /// Record of a single choice made
    /// </summary>
    [Serializable]
    public class ChoiceRecord
    {
        public int choiceIndex;
        public string choiceText;
        public DateTime timestamp;
    }
    
    /// <summary>
    /// Character relationship data
    /// </summary>
    [Serializable]
    public class CharacterRelationshipData
    {
        public string characterName;
        public int relationshipPoints;
        public DateTime lastInteraction;
        public int totalInteractions;
        public Dictionary<string, int> relationshipHistory = new Dictionary<string, int>();
    }
    
    /// <summary>
    /// CVC (Charisma, Violence, Coercion) level data
    /// </summary>
    [Serializable]
    public class CVCLevelData
    {
        public string characterName;
        public CVCType cvcType;
        public int currentLevel;
        public int currentPoints;
        public int totalPointsEarned;
        public int levelUpCount;
    }
    
    /// <summary>
    /// Current conversation state for resuming
    /// </summary>
    [Serializable]
    public class ConversationStateData
    {
        public string conversationId;
        public string currentNodeId;
        public DateTime savedTime;
        public Dictionary<string, object> localVariables;
    }
    
    /// <summary>
    /// Dialogue analytics summary
    /// </summary>
    [Serializable]
    public class DialogueAnalytics
    {
        public int totalConversations;
        public int completedConversations;
        public int totalChoices;
        public float averageChoicesPerConversation;
        public string mostFrequentChoiceType;
        public float totalPlayTime;
        public int uniqueCharactersInteracted;
    }
    
    /// <summary>
    /// CVC type enumeration
    /// </summary>
    public enum CVCType
    {
        Charisma,
        Violence,
        Coercion
    }
}