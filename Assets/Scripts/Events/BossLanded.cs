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
            {
                // Schedule landing audio through event system
                var audioEvent = Schedule<AudioEvent>(0f);
                audioEvent.audioSource = boss.audioSource;
                audioEvent.audioClip = boss.landAudio;
                audioEvent.audioType = AudioEventType.Land;
            }
        }
    }
}