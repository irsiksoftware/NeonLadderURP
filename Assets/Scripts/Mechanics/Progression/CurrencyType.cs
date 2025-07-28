using System;

namespace NeonLadder.Mechanics.Progression
{
    [Serializable]
    public enum CurrencyType
    {
        Meta,   // Per-run currency like Slay the Spire gold
        Perma   // Persistent currency like Hades Darkness
    }
}