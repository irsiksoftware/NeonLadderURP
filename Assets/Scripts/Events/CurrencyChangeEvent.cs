using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for currency changes (Meta/Perma) with visual effects
    /// Replaces direct currency Increment/Decrement calls
    /// </summary>
    public class CurrencyChangeEvent : Simulation.Event
    {
        public Player player;
        public CurrencyType currencyType;
        public int amount;

        public override bool Precondition()
        {
            return player != null && amount != 0;
        }

        public override void Execute()
        {
            if (player != null)
            {
                switch (currencyType)
                {
                    case CurrencyType.Meta:
                        player.MetaCurrency.Increment(amount);
                        break;
                    case CurrencyType.Perma:
                        player.PermaCurrency.Increment(amount);
                        break;
                }
                
                // Could schedule visual effects for currency pickup here
                // var pickupEffectEvent = Simulation.Schedule<VFXEvent>(0f);
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }

    /// <summary>
    /// Types of currency in the game
    /// </summary>
    public enum CurrencyType
    {
        Meta,
        Perma
    }
}