using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Tracks conversation points and CVC (Conversation Victory Condition) levels
    /// T'Challa's system for measuring diplomatic mastery
    /// </summary>
    public class ConversationPointTracker : MonoBehaviour
    {
        [Header("CVC Level Configuration")]
        [SerializeField] private int pointsPerCVCLevel = 25;
        [SerializeField] private int maxCVCLevel = 10;
        
        [Header("Point Tracking")]
        [SerializeField] private Dictionary<string, int> characterPoints = new Dictionary<string, int>();
        [SerializeField] private Dictionary<string, List<ConversationPointEntry>> pointHistory = new Dictionary<string, List<ConversationPointEntry>>();
        
        // Events for UI updates and achievements
        public System.Action<string, int, int> OnCVCLevelUp; // character, oldLevel, newLevel
        public System.Action<string, int> OnPointsAwarded; // character, points
        public System.Action<string, DialogChoice> OnChoiceRecorded; // character, choice

        void Awake()
        {
            InitializePointTracker();
        }

        private void InitializePointTracker()
        {
            if (characterPoints == null)
                characterPoints = new Dictionary<string, int>();
                
            if (pointHistory == null)
                pointHistory = new Dictionary<string, List<ConversationPointEntry>>();
        }

        #region Public Interface

        /// <summary>
        /// Award conversation points for a dialog choice
        /// </summary>
        public void AwardPoints(string characterId, DialogChoice choice, int points)
        {
            InitializeCharacterIfNeeded(characterId);
            
            int oldLevel = GetCVCLevel(characterId);
            
            characterPoints[characterId] += points;
            
            // Ensure points don't go below zero
            if (characterPoints[characterId] < 0)
                characterPoints[characterId] = 0;
            
            // Record in history
            RecordPointEntry(characterId, choice, points);
            
            int newLevel = GetCVCLevel(characterId);
            
            // Trigger events
            OnPointsAwarded?.Invoke(characterId, points);
            OnChoiceRecorded?.Invoke(characterId, choice);
            
            if (newLevel != oldLevel)
            {
                OnCVCLevelUp?.Invoke(characterId, oldLevel, newLevel);
                Debug.Log($"<color=gold>CVC Level Up!</color> {characterId}: {oldLevel} → {newLevel}");
            }
            
            Debug.Log($"ConversationPoints: {characterId} {(points >= 0 ? "+" : "")}{points} (Total: {characterPoints[characterId]}, Level: {newLevel})");
        }

        /// <summary>
        /// Get total conversation points for a character
        /// </summary>
        public int GetPointsForCharacter(string characterId)
        {
            InitializeCharacterIfNeeded(characterId);
            return characterPoints[characterId];
        }

        /// <summary>
        /// Get current CVC level for a character
        /// </summary>
        public int GetCVCLevel(string characterId)
        {
            int points = GetPointsForCharacter(characterId);
            int level = Mathf.Min(points / pointsPerCVCLevel, maxCVCLevel);
            return level;
        }

        /// <summary>
        /// Get total points across all characters
        /// </summary>
        public int GetTotalPoints()
        {
            return characterPoints.Values.Sum();
        }

        /// <summary>
        /// Get points needed for next CVC level
        /// </summary>
        public int GetPointsToNextLevel(string characterId)
        {
            int currentLevel = GetCVCLevel(characterId);
            if (currentLevel >= maxCVCLevel) return 0;
            
            int currentPoints = GetPointsForCharacter(characterId);
            int pointsForNextLevel = (currentLevel + 1) * pointsPerCVCLevel;
            
            return pointsForNextLevel - currentPoints;
        }

        /// <summary>
        /// Check if character has reached specific CVC level
        /// </summary>
        public bool HasReachedCVCLevel(string characterId, int targetLevel)
        {
            return GetCVCLevel(characterId) >= targetLevel;
        }

        /// <summary>
        /// Get conversation history for character
        /// </summary>
        public List<ConversationPointEntry> GetConversationHistory(string characterId)
        {
            InitializeCharacterIfNeeded(characterId);
            return new List<ConversationPointEntry>(pointHistory[characterId]);
        }

        /// <summary>
        /// Get most successful dialog choice type for character
        /// </summary>
        public DialogChoice GetMostSuccessfulChoice(string characterId)
        {
            var history = GetConversationHistory(characterId);
            if (!history.Any()) return DialogChoice.Diplomatic;
            
            var positiveChoices = history.Where(h => h.pointsAwarded > 0);
            if (!positiveChoices.Any()) return DialogChoice.Diplomatic;
            
            return positiveChoices
                .GroupBy(h => h.choice)
                .OrderByDescending(g => g.Sum(h => h.pointsAwarded))
                .First()
                .Key;
        }

        /// <summary>
        /// Reset all conversation points (for new game, etc.)
        /// </summary>
        public void ResetAllPoints()
        {
            characterPoints.Clear();
            pointHistory.Clear();
            Debug.Log("ConversationPointTracker: All points reset");
        }

        /// <summary>
        /// Reset points for specific character
        /// </summary>
        public void ResetCharacterPoints(string characterId)
        {
            if (characterPoints.ContainsKey(characterId))
            {
                characterPoints[characterId] = 0;
            }
            
            if (pointHistory.ContainsKey(characterId))
            {
                pointHistory[characterId].Clear();
            }
            
            Debug.Log($"ConversationPointTracker: Points reset for {characterId}");
        }

        #endregion

        #region CVC Level Benefits

        /// <summary>
        /// Get benefits available at current CVC level
        /// </summary>
        public List<CVCLevelBenefit> GetCurrentBenefits(string characterId)
        {
            int currentLevel = GetCVCLevel(characterId);
            var benefits = new List<CVCLevelBenefit>();
            
            for (int level = 1; level <= currentLevel; level++)
            {
                benefits.AddRange(GetBenefitsForLevel(characterId, level));
            }
            
            return benefits;
        }

        private List<CVCLevelBenefit> GetBenefitsForLevel(string characterId, int level)
        {
            var benefits = new List<CVCLevelBenefit>();
            
            switch (level)
            {
                case 1:
                    benefits.Add(new CVCLevelBenefit
                    {
                        type = BenefitType.UnlockDialogOption,
                        description = "Unlocks empathetic responses",
                        dialogOption = DialogChoice.Empathetic
                    });
                    break;
                    
                case 2:
                    benefits.Add(new CVCLevelBenefit
                    {
                        type = BenefitType.DiscountShop,
                        description = "5% shop discount",
                        discountPercentage = 5
                    });
                    break;
                    
                case 3:
                    benefits.Add(new CVCLevelBenefit
                    {
                        type = BenefitType.UnlockDialogOption,
                        description = "Unlocks philosophical insights",
                        dialogOption = DialogChoice.Philosophical
                    });
                    break;
                    
                case 5:
                    benefits.Add(new CVCLevelBenefit
                    {
                        type = BenefitType.UnlockDialogOption,
                        description = "Unlocks psychological analysis",
                        dialogOption = DialogChoice.PsychologicalInsight
                    });
                    break;
                    
                case 7:
                    if (characterId == "spaceship")
                    {
                        benefits.Add(new CVCLevelBenefit
                        {
                            type = BenefitType.UnlockDialogOption,
                            description = "Unlocks time manipulation dialog",
                            dialogOption = DialogChoice.TimeManipulation
                        });
                    }
                    break;
                    
                case 10:
                    benefits.Add(new CVCLevelBenefit
                    {
                        type = BenefitType.UnlockSpecialEnding,
                        description = $"Unlocks peaceful resolution with {characterId}"
                    });
                    break;
            }
            
            return benefits;
        }

        #endregion

        #region Helper Methods

        private void InitializeCharacterIfNeeded(string characterId)
        {
            if (!characterPoints.ContainsKey(characterId))
            {
                characterPoints[characterId] = 0;
            }
            
            if (!pointHistory.ContainsKey(characterId))
            {
                pointHistory[characterId] = new List<ConversationPointEntry>();
            }
        }

        private void RecordPointEntry(string characterId, DialogChoice choice, int points)
        {
            var entry = new ConversationPointEntry
            {
                choice = choice,
                pointsAwarded = points,
                timestamp = System.DateTime.Now,
                cvcLevelAfter = GetCVCLevel(characterId)
            };
            
            pointHistory[characterId].Add(entry);
        }

        #endregion

        #region Debug and Analytics

        /// <summary>
        /// Get analytics summary for all characters
        /// </summary>
        public ConversationAnalytics GetAnalytics()
        {
            return new ConversationAnalytics
            {
                totalPoints = GetTotalPoints(),
                charactersWithLevel5Plus = characterPoints.Keys.Count(c => GetCVCLevel(c) >= 5),
                mostSuccessfulCharacter = characterPoints.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key,
                totalConversations = pointHistory.Values.Sum(h => h.Count),
                averageCVCLevel = characterPoints.Values.Any() ? (float)characterPoints.Values.Average(p => p / pointsPerCVCLevel) : 0f
            };
        }

        /// <summary>
        /// Debug: Print all character points
        /// </summary>
        [ContextMenu("Debug: Print All Points")]
        public void DebugPrintAllPoints()
        {
            Debug.Log("=== Conversation Points Summary ===");
            foreach (var kvp in characterPoints.OrderByDescending(x => x.Value))
            {
                Debug.Log($"{kvp.Key}: {kvp.Value} points (Level {GetCVCLevel(kvp.Key)})");
            }
            Debug.Log($"Total: {GetTotalPoints()} points");
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class ConversationPointEntry
    {
        public DialogChoice choice;
        public int pointsAwarded;
        public System.DateTime timestamp;
        public int cvcLevelAfter;
    }

    [System.Serializable]
    public class CVCLevelBenefit
    {
        public BenefitType type;
        public string description;
        public DialogChoice dialogOption;
        public int discountPercentage;
    }

    public enum BenefitType
    {
        UnlockDialogOption,
        DiscountShop,
        UnlockSpecialEnding,
        BonusRewards,
        SkipCombat
    }

    [System.Serializable]
    public class ConversationAnalytics
    {
        public int totalPoints;
        public int charactersWithLevel5Plus;
        public string mostSuccessfulCharacter;
        public int totalConversations;
        public float averageCVCLevel;
    }

    #endregion
}