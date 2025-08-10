using System.Collections.Generic;
using UnityEngine;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Extensions for ConversationPointTracker to support save/load functionality
    /// </summary>
    public static class ConversationPointTrackerExtensions
    {
        /// <summary>
        /// Extension methods for ConversationPointTracker save/load support
        /// These would normally be added to the ConversationPointTracker class directly
        /// </summary>
        public static void SetPointsForCharacter(this ConversationPointTracker tracker, string characterId, int points)
        {
            // Use reflection or make the field public in the actual implementation
            var characterPointsField = tracker.GetType().GetField("characterPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (characterPointsField != null)
            {
                var characterPoints = characterPointsField.GetValue(tracker) as Dictionary<string, int>;
                if (characterPoints != null)
                {
                    characterPoints[characterId] = points;
                }
            }
        }
        
        public static void SetCouragePoints(this ConversationPointTracker tracker, string characterId, int points)
        {
            // Store in a separate courage points dictionary
            // This would be implemented in the actual ConversationPointTracker
        }
        
        public static void SetVirtuePoints(this ConversationPointTracker tracker, string characterId, int points)
        {
            // Store in a separate virtue points dictionary
            // This would be implemented in the actual ConversationPointTracker
        }
        
        public static void SetCunningPoints(this ConversationPointTracker tracker, string characterId, int points)
        {
            // Store in a separate cunning points dictionary
            // This would be implemented in the actual ConversationPointTracker
        }
        
        public static int GetCouragePoints(this ConversationPointTracker tracker, string characterId)
        {
            // Return courage points for character
            // This would access the actual data in ConversationPointTracker
            return 0;
        }
        
        public static int GetVirtuePoints(this ConversationPointTracker tracker, string characterId)
        {
            // Return virtue points for character
            // This would access the actual data in ConversationPointTracker
            return 0;
        }
        
        public static int GetCunningPoints(this ConversationPointTracker tracker, string characterId)
        {
            // Return cunning points for character
            // This would access the actual data in ConversationPointTracker
            return 0;
        }
        
        public static List<string> GetAllTrackedCharacters(this ConversationPointTracker tracker)
        {
            var characterPointsField = tracker.GetType().GetField("characterPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (characterPointsField != null)
            {
                var characterPoints = characterPointsField.GetValue(tracker) as Dictionary<string, int>;
                if (characterPoints != null)
                {
                    return new List<string>(characterPoints.Keys);
                }
            }
            
            return new List<string>();
        }
        
        public static int GetOverallCVCLevel(this ConversationPointTracker tracker)
        {
            // Calculate overall CVC level across all characters
            int totalPoints = tracker.GetTotalPoints();
            return totalPoints / 100; // Assuming 100 points per overall level
        }
        
        public static void ResetAllPoints(this ConversationPointTracker tracker)
        {
            var characterPointsField = tracker.GetType().GetField("characterPoints", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (characterPointsField != null)
            {
                var characterPoints = characterPointsField.GetValue(tracker) as Dictionary<string, int>;
                if (characterPoints != null)
                {
                    characterPoints.Clear();
                }
            }
            
            // Also clear point history
            var pointHistoryField = tracker.GetType().GetField("pointHistory", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (pointHistoryField != null)
            {
                var pointHistory = pointHistoryField.GetValue(tracker) as Dictionary<string, List<ConversationPointEntry>>;
                if (pointHistory != null)
                {
                    pointHistory.Clear();
                }
            }
        }
        
        public static List<string> GetChoiceHistory(this ConversationPointTracker tracker, string characterId)
        {
            // Return choice history for character
            // This would access the actual point history in ConversationPointTracker
            return new List<string>();
        }
        
        public static void RestoreChoiceHistory(this ConversationPointTracker tracker, string characterId, List<string> history)
        {
            // Restore choice history for character
            // This would update the actual point history in ConversationPointTracker
        }
    }
}