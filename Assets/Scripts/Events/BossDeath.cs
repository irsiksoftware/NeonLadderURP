using Assets.Scripts;
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
                boss.audioSource.PlayOneShot(boss.ouchAudio);
        }
    }
}
