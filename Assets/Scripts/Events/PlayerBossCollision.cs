using Assets.Scripts;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;
using static NeonLadder.Core.Simulation;
//using Serilog;

namespace NeonLadder.Events
{
    public class PlayerBossCollision : Event<PlayerBossCollision>
    {
        public Boss boss;
        public Player player;
        public Collision2D collider;


        private bool willHurtEnemy { get; set; }

        private bool willHurtPlayer { get; set; }

        public override void Execute()
        {
            willHurtEnemy = (player.GetComponent<Collider>().bounds.center.y >= boss.GetComponent<Collider>().bounds.max.y) ||
                            (collider.collider.tag == Constants.PlayerWeapon && IsPlayerAttacking());

            willHurtPlayer = IsBossAttacking();

            //AppLogger.Logger.Information($"Will Hurt Enemy: {willHurtEnemy}, Will Hurt Player: {willHurtPlayer}");
            //AppLogger.Logger.Information($"Boss Perspective Collider Name: {collider?.collider?.name ?? "NULL NAME"} \r\n Boss Perspective Collider Tag {collider?.collider?.tag ?? "NULL TAG"}");
            

            //get enemy's capsulecollider2d
            var enemyCollider = boss.GetComponent<BoxCollider2D>();
            if (!enemyCollider.isTrigger)
            {
                if (willHurtEnemy)
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

                if (willHurtPlayer)
                {
                    player.Health.Decrement();
                    //player.playerActions.knockback = true;
                }
            }
        }

        private bool IsBossAttacking()
        {
            var isAttacking = boss.GetComponent<Animator>().GetBool("isAttacking");
            //AppLogger.Logger.Information($"Is Boss Attacking: {isAttacking}"); // Log the state
            return isAttacking;
        }

        private bool IsPlayerAttacking()
        {
            var isAttacking = player.animator.GetBool("isAttacking");
            //AppLogger.Logger.Information($"Is Player Attacking: {isAttacking}"); // Log the state
            return isAttacking;
        }
    }
}
