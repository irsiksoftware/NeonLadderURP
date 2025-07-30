using UnityEngine;
namespace NeonLadder.Models
{
    /// <summary>
    /// ScriptableObject for configuring player unlockable abilities and upgrades.
    /// Defines progression items that can be purchased with currency and used in gameplay.
    /// </summary>
    [CreateAssetMenu(fileName = "New Unlock", menuName = "NeonLadder/Progression/Unlock")]
    public class UnlockScriptableObject : ScriptableObject
    {
        [Header("🔓 Unlock Configuration")]
        [Tooltip("Display name for this unlock in the UI")]
        public string unlockName;
        
        [Tooltip("Description shown to players explaining what this unlock does")]
        public string description;
        
        [Tooltip("Currency cost to purchase this unlock")]
        public int cost;
        
        [Tooltip("Icon displayed in the progression UI")]
        public Sprite icon;
        
        [Header("⚙️ Unlock Properties")]
        [Tooltip("Category type for this unlock (determines behavior)")]
        public UnlockTypes type;
        
        [Tooltip("Stamina cost when using this unlock ability")]
        public float staminaCost;
        
        [Tooltip("Cooldown time in seconds before this unlock can be used again")]
        public float cooldownTime;
    }
}