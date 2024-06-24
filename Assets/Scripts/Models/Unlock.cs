using System;

namespace NeonLadderURP.Models
{
    [Serializable]
    public class Unlock
    {
        public string Name; // Name of the unlock (e.g., "Climbing", "Double Jumping")
        public string Description; // Description of what the unlock does
        public bool IsPurchased; // Indicates if the unlock has been purchased
        public int Cost; // Cost in permaCurrency to unlock
        public UnlockTypes Type; // Type of the unlock, e.g., ability, upgrade

        // Additional properties for abilities
        public float StaminaCost; // Cost in stamina to use the ability, if applicable
        public float CooldownTime; // Cooldown time for the ability, if applicable

        public Unlock(string name, string description, int cost, UnlockTypes type, float staminaCost = 0, float cooldownTime = 0)
        {
            Name = name;
            Description = description;
            Cost = cost;
            Type = type;
            StaminaCost = staminaCost;
            CooldownTime = cooldownTime;
        }
    }
}
