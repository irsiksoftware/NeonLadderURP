using UnityEngine;
using System.Collections.Generic;

namespace NeonLadder.Dialog
{
    /// <summary>
    /// Core types for the dialog system
    /// </summary>
    
    [System.Serializable]
    public class PlayerSkills
    {
        public int intuition;
        public int rhetoric;
        public int spaceshipSynergy;
    }

    [System.Serializable]
    public class DialogConsequence
    {
        public int pointChange;
        public string description;
        public bool unlocksNewPath;
    }

    [System.Serializable]
    public class SkillBasedChoice
    {
        public DialogChoice choice;
        public SkillType requiredSkill;
        public int requiredLevel;
        public string description;
    }

    [System.Serializable]
    public class DialogReward
    {
        public string type;
        public int value;
        
        public static DialogReward MetaCurrency(int amount) => new DialogReward { type = "meta", value = amount };
    }

    public enum SkillType
    {
        Intuition,
        Rhetoric, 
        SpaceshipSynergy,
        TimeControl,
        Charisma
    }
}