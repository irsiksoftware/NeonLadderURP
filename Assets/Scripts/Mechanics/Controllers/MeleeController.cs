using NeonLadder.Mechanics.Enums;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class MeleeController : MonoBehaviour
    {
        [SerializeField]
        private int damage; // Backing field

        public int Damage
        {
            get
            {
                if (damage <= 0)
                {
                    // Fallback to switch statement value if no value or 0 is specified
                    switch (gameObject.tag)
                    {
                        case nameof(Tags.Boss):
                            return 100;
                        case nameof(Tags.Major):
                            return 10;
                        case nameof(Tags.Minor):
                            return 5;
                        case nameof(Tags.Player):
                            return 100;
                        default:
                            return 1;
                    }
                }
                return damage;
            }
            set
            {
                damage = value;
            }
        }

        private void Awake()
        {
            // No need for switch statement here, as the Damage property handles the fallback logic
        }
    }
}
