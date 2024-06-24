using UnityEngine;
namespace NeonLadder.Models
{
    [CreateAssetMenu(fileName = "New Unlock", menuName = "Unlock")]
    public class UnlockScriptableObject : ScriptableObject
    {
        public string unlockName;
        public string description;
        public int cost;
        public Sprite icon;
        public UnlockTypes type;
        public float staminaCost;
        public float cooldownTime;
    }
}