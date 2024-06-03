using NeonLadder.Mechanics.Controllers;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player character lands after being airborne.
    /// </summary>
    /// <typeparam name="PlayerLanded"></typeparam>
    public class BossLanded : Event<BossLanded>
    {
        public Boss boss;

        public override void Execute()
        {
            if (boss.audioSource && boss.landAudio)
                boss.audioSource.PlayOneShot(boss.landAudio);
        }
    }
}