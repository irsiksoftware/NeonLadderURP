using NeonLadder.Mechanics.Enums;
using System.Collections.Generic;

namespace NeonLadder.Mechanics.Controllers
{
    public static class BossTransformations
    {
        public static Dictionary<string, string> bossTransformations = new Dictionary<string, string>
        {
            { nameof(Bosses.Wrath), nameof(Bosses.Anglerox) },
            { nameof(Bosses.Gluttony), nameof(Bosses.Gobbler) }
        };
    }
}
