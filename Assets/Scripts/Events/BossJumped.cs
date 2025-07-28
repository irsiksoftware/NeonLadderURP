using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player performs a Jump.
    /// </summary>
    /// <typeparam name="PlayerJumped"></typeparam>
    public class BossJumped : BaseGameEvent<BossJumped>
    {
        public Boss boss;

        public override void Execute()
        {
            if (boss.audioSource && boss.jumpAudio)
            {
                // Schedule jump audio through event system
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioSource = boss.audioSource;
                audioEvent.audioClip = boss.jumpAudio;
                audioEvent.audioType = AudioEventType.Jump;
            }
        }
    }
}