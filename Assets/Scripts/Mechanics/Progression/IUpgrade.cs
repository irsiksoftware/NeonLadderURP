using System;

namespace NeonLadder.Mechanics.Progression
{
    public interface IUpgrade
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string FlavorText { get; }
        CurrencyType CurrencyType { get; }
        UpgradeCategory Category { get; }
        int Cost { get; }
        int MaxLevel { get; }
        int CurrentLevel { get; set; }
        string[] Prerequisites { get; }
        string[] MutuallyExclusive { get; }
        bool IsMaxLevel { get; }
        bool CanUpgrade { get; }
        
        int GetCostForLevel(int level);
        void ApplyEffect(UnityEngine.GameObject target);
        void RemoveEffect(UnityEngine.GameObject target);
    }
}