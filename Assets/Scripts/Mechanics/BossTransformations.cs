using NeonLadder.Mechanics.Enums;
using System.Collections.Generic;

namespace NeonLadder.Mechanics.Controllers
{
    public static class BossTransformations
    {
        public static Dictionary<string, string> bossTransformations = new()
        {
            { nameof(Bosses.Wrath), nameof(Bosses.Anglerox) },
            { nameof(Bosses.Gluttony), nameof(Bosses.Gobbler) },
            { nameof(Bosses.Greed), nameof(Bosses.Rapax) },
            { nameof(Bosses.Sloth), nameof(Bosses.Ursacetus) },
            { nameof(Bosses.Envy), nameof(Bosses.Ceratoferox) },
            { nameof(Bosses.Lust), nameof(Bosses.Arack) },
            { nameof(Bosses.Pride), nameof(Bosses.Onyscidus) },
            { nameof(Bosses.Devil), nameof(Bosses.Devil) }
        };
    }
}