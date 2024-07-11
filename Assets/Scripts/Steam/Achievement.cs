using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Steam
{
    public class Achievement
    {
        public Achievements AchievementID;
        public string Name;
        public string Description;
        public bool Achieved;

        public Achievement(Achievements achievementID, string name, string desc)
        {
            AchievementID = achievementID;
            Name = name;
            Description = desc;
            Achieved = false;
        }
    }
}