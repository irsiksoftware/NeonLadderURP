using UnityEngine;
using System;

namespace NeonLadder.Mechanics.Progression
{
    [CreateAssetMenu(fileName = "New Upgrade", menuName = "NeonLadder/Progression/Upgrade")]
    public class UpgradeData : ScriptableObject, IUpgrade
    {
        [Header("Basic Properties")]
        [SerializeField] private string id;
        [SerializeField] private string upgradeName;
        [SerializeField] private string description;
        [SerializeField] private string flavorText;
        
        [Header("Currency & Cost")]
        [SerializeField] private CurrencyType currencyType;
        [SerializeField] private UpgradeCategory category;
        [SerializeField] private int baseCost;
        [SerializeField] private int maxLevel = 1;
        
        [Header("Dependencies")]
        [SerializeField] private string[] prerequisites = new string[0];
        [SerializeField] private string[] mutuallyExclusive = new string[0];
        
        [Header("Effects")]
        [SerializeField] private UpgradeEffect[] effects = new UpgradeEffect[0];
        
        [SerializeField] private int currentLevel = 0;
        
        // IUpgrade implementation
        public string Id => id;
        public string Name => upgradeName;
        public string Description => description;
        public string FlavorText => flavorText;
        public CurrencyType CurrencyType => currencyType;
        public UpgradeCategory Category => category;
        public int Cost => GetCostForLevel(currentLevel + 1);
        public int MaxLevel => maxLevel;
        public int CurrentLevel 
        { 
            get => currentLevel; 
            set => currentLevel = Mathf.Clamp(value, 0, maxLevel); 
        }
        public string[] Prerequisites => prerequisites;
        public string[] MutuallyExclusive => mutuallyExclusive;
        public bool IsMaxLevel => currentLevel >= maxLevel;
        public bool CanUpgrade => !IsMaxLevel;
        
        public int GetCostForLevel(int level)
        {
            if (level <= 0 || level > maxLevel) return 0;
            
            // Cost scaling: each level costs more
            return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, level - 1));
        }
        
        public void ApplyEffect(GameObject target)
        {
            foreach (var effect in effects)
            {
                effect.Apply(target, currentLevel);
            }
        }
        
        public void RemoveEffect(GameObject target)
        {
            foreach (var effect in effects)
            {
                effect.Remove(target, currentLevel);
            }
        }
        
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = name.ToLower().Replace(" ", "_");
            }
        }
    }
    
    [Serializable]
    public class UpgradeEffect
    {
        [SerializeField] private string targetProperty;
        [SerializeField] private float baseValue;
        [SerializeField] private float perLevelIncrease;
        [SerializeField] private bool isPercentage;
        
        public void Apply(GameObject target, int level)
        {
            if (level <= 0) return;
            
            float totalValue = baseValue + (perLevelIncrease * (level - 1));
            
            // This is a simplified implementation
            // In practice, you'd use reflection or a property system to apply effects
            Debug.Log($"Applying {targetProperty}: {totalValue}{(isPercentage ? "%" : "")} to {target.name}");
        }
        
        public void Remove(GameObject target, int level)
        {
            if (level <= 0) return;
            
            float totalValue = baseValue + (perLevelIncrease * (level - 1));
            
            // Remove the effect (implementation depends on your stat system)
            Debug.Log($"Removing {targetProperty}: {totalValue}{(isPercentage ? "%" : "")} from {target.name}");
        }
    }
}