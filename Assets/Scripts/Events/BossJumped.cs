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
                boss.audioSource.PlayOneShot(boss.jumpAudio);
        }
    }
}