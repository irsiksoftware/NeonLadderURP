using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NeonLadder.Debug;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Validates and filters dialog choices based on player stats, CVC levels, and conversation history
    /// T'Challa's system for meaningful choice consequences
    /// </summary>
    public class DialogChoiceValidator : MonoBehaviour
    {
        [Header("Choice Validation Configuration")]
        public bool enableSkillChecks = true;
        public bool enableCVCLevelGating = true;
        public bool enableHistoryInfluence = true;
        
        [Header("Skill Requirements")]
        public List<SkillRequirement> skillRequirements;
        
        private ConversationPointTracker pointTracker;
        private Dictionary<string, List<DialogChoice>> characterAvailableChoices;

        void Awake()
        {
            InitializeValidator();
        }

        void Start()
        {
            pointTracker = FindObjectOfType<ConversationPointTracker>();
            if (pointTracker == null)
            {
                Debug.LogWarning("DialogChoiceValidator: ConversationPointTracker not found");
            }
        }

        private void InitializeValidator()
        {
            characterAvailableChoices = new Dictionary<string, List<DialogChoice>>();
            InitializeSkillRequirements();
        }

        #region Skill Requirements Initialization

        private void InitializeSkillRequirements()
        {
            skillRequirements = new List<SkillRequirement>
            {
                // High-level philosophical choices
                new SkillRequirement 
                { 
                    choice = DialogChoice.PsychologicalInsight, 
                    requiredSkill = SkillType.Intuition, 
                    requiredLevel = 6,
                    description = "Deep psychological understanding required"
                },
                new SkillRequirement 
                { 
                    choice = DialogChoice.Philosophical, 
                    requiredSkill = SkillType.Rhetoric, 
                    requiredLevel = 4,
                    description = "Advanced rhetoric skills needed"
                },
                
                // Spaceship-specific abilities
                new SkillRequirement 
                { 
                    choice = DialogChoice.TimeManipulation, 
                    requiredSkill = SkillType.SpaceshipSynergy, 
                    requiredLevel = 7,
                    description = "Deep bond with immortal spaceship required"
                },
                
                // Charisma-based choices
                new SkillRequirement 
                { 
                    choice = DialogChoice.CharmingPersuasion, 
                    requiredSkill = SkillType.Charisma, 
                    requiredLevel = 5,
                    description = "Exceptional charisma required"
                },
                
                // Special end-game choices
                new SkillRequirement 
                { 
                    choice = DialogChoice.UnityAgainstEvil, 
                    requiredSkill = SkillType.Intuition, 
                    requiredLevel = 8,
                    description = "Ultimate wisdom and understanding required"
                }
            };
        }

        #endregion

        #region Public Interface

        /// <summary>
        /// Get all available dialog choices for character and context
        /// </summary>
        public List<DialogChoice> GetAvailableChoices(string characterId, string context, PlayerStats playerStats = null)
        {
            var baseChoices = GetBaseChoicesForContext(characterId, context);
            var filteredChoices = new List<DialogChoice>(baseChoices);

            if (enableCVCLevelGating)
            {
                filteredChoices = FilterChoicesByCVCLevel(characterId, filteredChoices);
            }

            if (enableSkillChecks && playerStats != null)
            {
                filteredChoices = FilterChoicesBySkills(filteredChoices, GetPlayerSkillsFromStats(playerStats));
            }

            if (enableHistoryInfluence)
            {
                filteredChoices = FilterChoicesByHistory(characterId, filteredChoices);
            }

            // Always ensure at least basic choices are available
            EnsureMinimumChoices(filteredChoices);

            return filteredChoices;
        }

        /// <summary>
        /// Get skill-based special choices
        /// </summary>
        public List<SkillBasedChoice> GetSkillBasedChoices(string characterId, PlayerSkills playerSkills)
        {
            var skillChoices = new List<SkillBasedChoice>();

            foreach (var requirement in skillRequirements)
            {
                if (MeetsSkillRequirement(requirement, playerSkills))
                {
                    skillChoices.Add(new SkillBasedChoice
                    {
                        choice = requirement.choice,
                        requiredSkill = requirement.requiredSkill,
                        requiredLevel = requirement.requiredLevel,
                        description = requirement.description
                    });
                }
            }

            return skillChoices;
        }

        /// <summary>
        /// Preview consequences of a dialog choice
        /// </summary>
        public DialogConsequence PreviewChoiceConsequence(string characterId, DialogChoice choice)
        {
            var pointChange = CalculatePointChange(characterId, choice);
            var description = GenerateConsequenceDescription(characterId, choice, pointChange);
            var unlocksNewPath = CheckIfUnlocksNewPath(characterId, choice);

            return new DialogConsequence
            {
                pointChange = pointChange,
                description = description,
                unlocksNewPath = unlocksNewPath
            };
        }

        /// <summary>
        /// Check if choice is available for character
        /// </summary>
        public bool IsChoiceAvailable(string characterId, DialogChoice choice, PlayerStats playerStats = null)
        {
            var availableChoices = GetAvailableChoices(characterId, "general", playerStats);
            return availableChoices.Contains(choice);
        }

        #endregion

        #region Choice Filtering

        private List<DialogChoice> GetBaseChoicesForContext(string characterId, string context)
        {
            var baseChoices = new List<DialogChoice>();

            // Universal choices always available
            baseChoices.AddRange(new[] 
            { 
                DialogChoice.Diplomatic, 
                DialogChoice.Empathetic,
                DialogChoice.Bargaining
            });

            // Character-specific base choices
            switch (characterId.ToLower())
            {
                case "wrath":
                    baseChoices.AddRange(new[] { DialogChoice.Challenge, DialogChoice.Aggressive });
                    break;
                    
                case "pride":
                    baseChoices.AddRange(new[] { DialogChoice.Humble, DialogChoice.Arrogant });
                    break;
                    
                case "greed":
                    baseChoices.AddRange(new[] { DialogChoice.Bargaining });
                    break;
                    
                case "sloth":
                    baseChoices.AddRange(new[] { DialogChoice.Motivational });
                    break;
                    
                case "spaceship":
                    baseChoices.AddRange(new[] { DialogChoice.Philosophical, DialogChoice.Insightful });
                    break;
                    
                case "elli":
                case "aria":
                case "merchant":
                    baseChoices.AddRange(new[] { DialogChoice.Bargaining, DialogChoice.CharmingPersuasion });
                    break;
            }

            // Context-specific choices
            if (context == "climax_confrontation")
            {
                baseChoices.Add(DialogChoice.UnityAgainstEvil);
            }

            return baseChoices.Distinct().ToList();
        }

        private List<DialogChoice> FilterChoicesByCVCLevel(string characterId, List<DialogChoice> choices)
        {
            if (pointTracker == null) return choices;

            var cvcLevel = pointTracker.GetCVCLevel(characterId);
            var filteredChoices = new List<DialogChoice>(choices);

            // Add choices unlocked by CVC level
            if (cvcLevel >= 1 && !filteredChoices.Contains(DialogChoice.Empathetic))
                filteredChoices.Add(DialogChoice.Empathetic);
                
            if (cvcLevel >= 3 && !filteredChoices.Contains(DialogChoice.Philosophical))
                filteredChoices.Add(DialogChoice.Philosophical);
                
            if (cvcLevel >= 5 && !filteredChoices.Contains(DialogChoice.PsychologicalInsight))
                filteredChoices.Add(DialogChoice.PsychologicalInsight);

            return filteredChoices;
        }

        private List<DialogChoice> FilterChoicesBySkills(List<DialogChoice> choices, PlayerSkills playerSkills)
        {
            var filteredChoices = new List<DialogChoice>(choices);

            foreach (var requirement in skillRequirements)
            {
                if (choices.Contains(requirement.choice))
                {
                    if (!MeetsSkillRequirement(requirement, playerSkills))
                    {
                        filteredChoices.Remove(requirement.choice);
                    }
                }
                else if (MeetsSkillRequirement(requirement, playerSkills))
                {
                    filteredChoices.Add(requirement.choice);
                }
            }

            return filteredChoices;
        }

        private List<DialogChoice> FilterChoicesByHistory(string characterId, List<DialogChoice> choices)
        {
            // This would integrate with conversation history system
            // For now, returning choices as-is
            return choices;
        }

        private void EnsureMinimumChoices(List<DialogChoice> choices)
        {
            if (choices.Count < 2)
            {
                if (!choices.Contains(DialogChoice.Diplomatic))
                    choices.Add(DialogChoice.Diplomatic);
                    
                if (!choices.Contains(DialogChoice.Empathetic))
                    choices.Add(DialogChoice.Empathetic);
            }
        }

        #endregion

        #region Skill Validation

        private bool MeetsSkillRequirement(SkillRequirement requirement, PlayerSkills playerSkills)
        {
            var skillValue = GetSkillValue(requirement.requiredSkill, playerSkills);
            return skillValue >= requirement.requiredLevel;
        }

        private int GetSkillValue(SkillType skillType, PlayerSkills playerSkills)
        {
            switch (skillType)
            {
                case SkillType.Intuition:
                    return playerSkills.intuition;
                case SkillType.Rhetoric:
                    return playerSkills.rhetoric;
                case SkillType.SpaceshipSynergy:
                    return playerSkills.spaceshipSynergy;
                case SkillType.Charisma:
                    return playerSkills.intuition; // Using intuition as proxy for charisma
                case SkillType.TimeControl:
                    return playerSkills.spaceshipSynergy; // Time control tied to spaceship bond
                default:
                    return 0;
            }
        }

        private PlayerSkills GetPlayerSkillsFromStats(PlayerStats playerStats)
        {
            return new PlayerSkills
            {
                intuition = playerStats.charismaLevel * 2, // Convert stats to skills
                rhetoric = playerStats.charismaLevel,
                spaceshipSynergy = playerStats.spaceshipBondLevel + playerStats.timeControlMastery
            };
        }

        #endregion

        #region Consequence Calculation

        private int CalculatePointChange(string characterId, DialogChoice choice)
        {
            // This would integrate with ConversationManager's point calculation
            // Simplified version for preview
            switch (choice)
            {
                case DialogChoice.Diplomatic: return 15;
                case DialogChoice.Empathetic: return 20;
                case DialogChoice.Philosophical: return 30;
                case DialogChoice.PsychologicalInsight: return 35;
                case DialogChoice.TimeManipulation: return 40;
                case DialogChoice.Aggressive: return -10;
                case DialogChoice.Hostile: return -15;
                default: return 10;
            }
        }

        private string GenerateConsequenceDescription(string characterId, DialogChoice choice, int pointChange)
        {
            var direction = pointChange > 0 ? "improve" : "worsen";
            var magnitude = Mathf.Abs(pointChange) > 20 ? "significantly" : "slightly";
            
            return $"This choice will {magnitude} {direction} your relationship with {characterId}.";
        }

        private bool CheckIfUnlocksNewPath(string characterId, DialogChoice choice)
        {
            // Check if this choice would unlock new conversation branches
            if (pointTracker == null) return false;
            
            var currentLevel = pointTracker.GetCVCLevel(characterId);
            var pointChange = CalculatePointChange(characterId, choice);
            var newPoints = pointTracker.GetPointsForCharacter(characterId) + pointChange;
            var newLevel = newPoints / 25; // Assuming 25 points per level
            
            return newLevel > currentLevel;
        }

        #endregion

        #region Debug and Utilities

        /// <summary>
        /// Debug: Print all available choices for character
        /// </summary>
        [ContextMenu("Debug: Print Available Choices")]
        public void DebugPrintAvailableChoices()
        {
            var testStats = new PlayerStats { timeControlMastery = 5, spaceshipBondLevel = 3, charismaLevel = 4 };
            
            var characters = new[] { "wrath", "pride", "elli", "aria", "spaceship" };
            
            Debug.Log("=== Available Dialog Choices ===");
            foreach (var character in characters)
            {
                var choices = GetAvailableChoices(character, "general", testStats);
                Debug.Log($"{character}: {string.Join(", ", choices)}");
            }
        }

        /// <summary>
        /// Get choice statistics for analytics
        /// </summary>
        public ChoiceAnalytics GetChoiceAnalytics()
        {
            return new ChoiceAnalytics
            {
                totalUniqueChoices = System.Enum.GetValues(typeof(DialogChoice)).Length,
                skillGatedChoices = skillRequirements.Count,
                charactersWithSpecialChoices = characterAvailableChoices.Count,
                averageChoicesPerCharacter = characterAvailableChoices.Values.Any() ? 
                    (float)characterAvailableChoices.Values.Average(c => c.Count) : 0f
            };
        }

        #endregion
    }

    #region Data Structures

    [System.Serializable]
    public class SkillRequirement
    {
        public DialogChoice choice;
        public SkillType requiredSkill;
        public int requiredLevel;
        public string description;
    }

    [System.Serializable]
    public class ChoiceAnalytics
    {
        public int totalUniqueChoices;
        public int skillGatedChoices;
        public int charactersWithSpecialChoices;
        public float averageChoicesPerCharacter;
    }


    #endregion
}