using UnityEngine;

namespace NeonLadder.Mechanics.Progression
{
    /// <summary>
    /// ScriptableObject-based upgrade definition for easy designer configuration
    /// Wade's Note: "Like a trading card, but for making players OP!"
    /// Stephen's Note: "The mystical formulae for progression excellence."
    /// </summary>
    [CreateAssetMenu(fileName = "New Upgrade", menuName = "NeonLadder/Progression/Upgrade")]
    public class UpgradeData : ScriptableObject, IUpgrade
    {
        [Header("Basic Info")]
        [SerializeField] private string upgradeId;
        [SerializeField] private string upgradeName;
        [TextArea(2, 4)]
        [SerializeField] private string description;
        [TextArea(1, 2)]
        [SerializeField] private string flavorText;
        
        [Header("Cost & Category")]
        [SerializeField] private CurrencyType currencyType;
        [SerializeField] private UpgradeCategory category;
        [SerializeField] private int cost;
        [SerializeField] private int maxLevel = 1;
        
        [Header("Dependencies")]
        [SerializeField] private string[] prerequisites = new string[0];
        [SerializeField] private string[] mutuallyExclusive = new string[0];
        
        [Header("Effects")]
        [SerializeField] private UpgradeEffect[] effects;
        [TextArea(1, 2)]
        [SerializeField] private string effectTemplate = "Increases {0} by {1}%";
        
        // Runtime state
        private int currentLevel = 0;
        
        // Interface implementation
        public string Id => upgradeId;
        public string Name => upgradeName;
        public string Description => description;
        public string FlavorText => flavorText;
        public CurrencyType CurrencyType => currencyType;
        public UpgradeCategory Category => category;
        public int Cost => cost;
        public int MaxLevel => maxLevel;
        public int CurrentLevel 
        { 
            get => currentLevel; 
            set => currentLevel = Mathf.Clamp(value, 0, maxLevel); 
        }
        public bool IsMaxLevel => currentLevel >= maxLevel;
        public bool IsOwned => currentLevel > 0;
        public string[] Prerequisites => prerequisites;
        public string[] MutuallyExclusive => mutuallyExclusive;
        
        public void ApplyEffect(GameObject player)
        {
            if (!IsOwned) return;
            
            foreach (var effect in effects)
            {
                effect.Apply(player, currentLevel);
            }
        }
        
        public void RemoveEffect(GameObject player)
        {
            foreach (var effect in effects)
            {
                effect.Remove(player, currentLevel);
            }
        }
        
        public string GetEffectDescription()
        {
            if (effects.Length == 0) return "No effect defined";
            return effects[0].GetDescription(currentLevel);
        }
        
        public string GetNextLevelDescription()
        {
            if (IsMaxLevel) return "Max Level";
            if (effects.Length == 0) return "No effect defined";
            return $"Next: {effects[0].GetDescription(currentLevel + 1)}";
        }
    }
    
    /// <summary>
    /// Serializable effect definition for upgrades
    /// Supports multiple effect types like Hades boons
    /// </summary>
    [System.Serializable]
    public class UpgradeEffect
    {
        public UpgradeEffectType effectType;
        public string targetProperty;  // "Health.max", "Player.moveSpeed", etc.
        public float baseValue;
        public float perLevelIncrease;
        public bool isPercentage;
        
        public void Apply(GameObject player, int level)
        {
            float totalValue = baseValue + (perLevelIncrease * (level - 1));
            // Apply effect to player using reflection or component lookup
            // Implementation will depend on your component architecture
        }
        
        public void Remove(GameObject player, int level)
        {
            float totalValue = baseValue + (perLevelIncrease * (level - 1));
            // Remove effect from player
        }
        
        public string GetDescription(int level)
        {
            float totalValue = baseValue + (perLevelIncrease * (level - 1));
            string valueText = isPercentage ? $"{totalValue:F0}%" : $"{totalValue:F1}";
            return $"Increases {targetProperty} by {valueText}";
        }
    }
    
    public enum UpgradeEffectType
    {
        StatModifier,       // Direct stat changes (+10 Health, +15% damage)
        AbilityUnlock,      // Unlock new abilities
        BehaviorModifier,   // Change how systems work
        ResourceGeneration, // Generate resources over time
        Conditional         // Effects that trigger under conditions
    }
}