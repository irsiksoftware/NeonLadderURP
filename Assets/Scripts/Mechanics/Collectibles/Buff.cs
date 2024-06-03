using NeonLadder.Mechanics.Enums;

namespace NeonLadder.Mechanics.Collectibles
{
    public class Buff
    {
        public BuffTypes Type;
        public float Duration;
        public float Magnitude;

        // Constructor
        public Buff(BuffTypes type, float duration, float magnitude)
        {
            Type = type;
            Duration = duration;
            Magnitude = magnitude;
        }
    }
}
