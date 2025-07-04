using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class WeaponBossCollision : BaseGameEvent<WeaponBossCollision>
    {
        public Boss boss { get; set; }
        public Player player { get; set; }
        public Collision2D collider { get; set; }

        private bool willHurt { get; set; }

        public override void Execute()
        {
            willHurt = IsPlayerAttacking();

            //AppLogger.Logger.Information($"Will Hurt Enemy: {willHurt}");
            var enemyCollider = boss.GetComponent<BoxCollider2D>();
            if (!enemyCollider.isTrigger)
            {
                if (willHurt)
                {
                    var enemyHealth = boss.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Decrement();
                        if (!enemyHealth.IsAlive)
                        {
                            Schedule<BossDeath>().boss = boss;
                            //player.Bounce(2);
                        }
                        else
                        {
                            //player.Bounce(7);
                        }
                    }
                    else
                    {
                        Schedule<BossDeath>().boss = boss;
                        //player.Bounce(2);
                    }
                }
            }
        }

        private bool IsPlayerAttacking()
        {
            var isAttacking = player.Animator.GetBool("isAttacking");
            return isAttacking;
        }
    }
}
