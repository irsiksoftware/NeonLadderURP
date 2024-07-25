using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using System;
using System.Linq;

public static class AchievementResolver
{
    public static Achievements? Resolve(string bossTransformation)
    {
        var bossTransformations = BossTransformations.bossTransformations;
        var baseBoss = bossTransformations.FirstOrDefault(x => x.Value == bossTransformation).Key;

        if (baseBoss != null) //Note the change to NOT equal.
        {
            baseBoss = $"{baseBoss.ToUpper()}_SIN_DEFEATED";
        }

        // Attempt to get the achievement associated with the base boss name
        if (Enum.TryParse(baseBoss, out Achievements achievement))
        {
            return achievement;
        }

        //Debug.Log("No Achievement found for the boss.");
        return null;
    }
}
