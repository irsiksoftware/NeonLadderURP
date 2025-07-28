using NeonLadder.Common;
using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    public class BossDeath : BaseGameEvent<BossDeath>
    {
        public Boss boss;

        public override void Execute()
        {
            Constants.DefeatedBosses.Add(boss.gameObject.name);
            boss.GetComponent<Collider>().enabled = false;
            boss.GetComponent<Animator>().enabled = false;
            if (boss.audioSource && boss.ouchAudio)
            {
                // Schedule death audio through event system
                var audioEvent = Simulation.Schedule<AudioEvent>(0f);
                audioEvent.audioSource = boss.audioSource;
                audioEvent.audioClip = boss.ouchAudio;
                audioEvent.audioType = AudioEventType.Death;
            }
        }
    }
}
