using NeonLadder.Mechanics.Enums;
using UnityEngine;

namespace NeonLadder.Mechanics.Controllers
{
    public class MeleeController : MonoBehaviour
    {
        public int Damage;

        private void Awake()
        {
           
            switch (gameObject.tag)
            {
                case nameof(Tags.Major):
                    Damage = 10;
                    break;
                case nameof(Tags.Minor):
                    Damage = 5;
                    break;
                default:
                    Damage = 1;
                    break;
            }
        }
    }
}