using UnityEngine;
using System;

namespace NeonLadder.Documentation
{
    /// <summary>
    /// ScriptableObject-based Marvel Squad persona configuration
    /// Follows the NeonLadder UpgradeData pattern for consistent asset management
    /// </summary>
    [CreateAssetMenu(fileName = "New Marvel Persona", menuName = "NeonLadder/Documentation/Marvel Squad Persona")]
    public class MarvelSquadPersona : ScriptableObject
    {
        [Header("Persona Identity")]
        [SerializeField] private string personaName;
        [SerializeField] private string codeName; // e.g., "@tony-stark"
        [SerializeField] private string role;
        [SerializeField] private string catchPhrase;
        
        [Header("Expertise & Specialization")]
        [SerializeField] private PersonaCategory category;
        [SerializeField] private string[] primarySkills = new string[0];
        [SerializeField] private string[] secondarySkills = new string[0];
        [SerializeField] private int experienceLevel = 1; // 1-10 scale
        
        [Header("Documentation Assets")]
        [SerializeField] private Sprite avatarIcon;
        [SerializeField] private Color themeColor = Color.white;
        [TextArea(3, 6)]
        [SerializeField] private string personalityDescription;
        
        [Header("Collaboration Patterns")]
        [SerializeField] private string[] frequentPartners = new string[0];
        [SerializeField] private string[] avoidedPersonas = new string[0]; // For conflict resolution
        [SerializeField] private ConflictResolutionStyle conflictStyle;
        
        [Header("Memory & Context")]
        [SerializeField] private bool hasPersistentMemory = true;
        [SerializeField] private string memoryFilePath; // Relative to .claude/team-memory/personas/
        [SerializeField] private int maxMemoryEntries = 100;
        
        // Public accessors following UpgradeData pattern
        public string PersonaName => personaName;
        public string CodeName => codeName;
        public string Role => role;
        public string CatchPhrase => catchPhrase;
        public PersonaCategory Category => category;
        public string[] PrimarySkills => primarySkills;
        public string[] SecondarySkills => secondarySkills;
        public int ExperienceLevel => experienceLevel;
        public Sprite AvatarIcon => avatarIcon;
        public Color ThemeColor => themeColor;
        public string PersonalityDescription => personalityDescription;
        public string[] FrequentPartners => frequentPartners;
        public string[] AvoidedPersonas => avoidedPersonas;
        public ConflictResolutionStyle ConflictStyle => conflictStyle;
        public bool HasPersistentMemory => hasPersistentMemory;
        public string MemoryFilePath => memoryFilePath;
        public int MaxMemoryEntries => maxMemoryEntries;
        
        /// <summary>
        /// Generate a formatted introduction for this persona
        /// </summary>
        public string GetIntroduction()
        {
            return $"*{catchPhrase}*\n\n" +
                   $"**{personaName} ({codeName})** - {role}\n" +
                   $"**Specialization**: {string.Join(", ", primarySkills)}\n" +
                   $"**Experience Level**: {experienceLevel}/10\n\n" +
                   $"{personalityDescription}";
        }
        
        /// <summary>
        /// Get tooltip text for Unity Editor display
        /// </summary>
        public string GetTooltipText()
        {
            return $"{personaName} ({codeName})\n" +
                   $"Role: {role}\n" +
                   $"Skills: {string.Join(", ", primarySkills)}\n" +
                   $"Experience: {experienceLevel}/10";
        }
        
        /// <summary>
        /// Generate PBI assignment recommendation based on skills
        /// </summary>
        public float GetSkillMatchScore(string[] requiredSkills)
        {
            if (requiredSkills == null || requiredSkills.Length == 0) return 0f;
            
            float totalScore = 0f;
            foreach (string skill in requiredSkills)
            {
                if (System.Array.Exists(primarySkills, s => s.ToLower().Contains(skill.ToLower())))
                {
                    totalScore += 2f; // Primary skill match
                }
                else if (System.Array.Exists(secondarySkills, s => s.ToLower().Contains(skill.ToLower())))
                {
                    totalScore += 1f; // Secondary skill match
                }
            }
            
            return (totalScore / requiredSkills.Length) * (experienceLevel / 10f);
        }
        
        private void OnValidate()
        {
            // Auto-generate codeName if empty
            if (string.IsNullOrEmpty(codeName) && !string.IsNullOrEmpty(personaName))
            {
                codeName = "@" + personaName.ToLower().Replace(" ", "-");
            }
            
            // Auto-generate memory file path
            if (string.IsNullOrEmpty(memoryFilePath) && !string.IsNullOrEmpty(personaName))
            {
                memoryFilePath = personaName.ToLower().Replace(" ", "-") + "-memory.json";
            }
        }
    }
    
    [Serializable]
    public enum PersonaCategory
    {
        CoreDevelopment,    // Avengers
        ProductInfrastructure, // X-Men
        QualityInnovation,  // Fantastic Four  
        SupportDocumentation, // Guardians
        BusinessAnalysis    // Special roles like Nick Fury
    }
    
    [Serializable]
    public enum ConflictResolutionStyle
    {
        Collaborative,      // Works together to find solutions
        DirectChallenge,    // Questions decisions openly but respectfully
        AlternativeProposal, // Suggests different approaches
        DataDriven,         // Uses metrics and evidence to resolve conflicts
        CompromiseSeeking   // Finds middle ground solutions
    }
}