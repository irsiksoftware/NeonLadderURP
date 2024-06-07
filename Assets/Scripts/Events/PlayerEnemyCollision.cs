using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;

using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class PlayerEnemyCollision : Event<PlayerEnemyCollision>
    {
        public Enemy enemy;
        public Player player;


        public override void Execute()
        {
            //get enemys capsulecollider2d
            var enemyCollider = enemy.GetComponent<CapsuleCollider2D>();
            if (!enemyCollider.isTrigger)
            {
                var willHurtEnemy = player.GetComponent<Collider>().bounds.center.y >= enemy.GetComponent<Collider>().bounds.max.y;
                var willHurtPlayer = !willHurtEnemy;
                if (willHurtEnemy)
                {
                    var enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.Decrement();
                        if (!enemyHealth.IsAlive)
                        {
                            Schedule<EnemyDeath>().enemy = enemy;
                            player.Bounce(2);
                        }
                        else
                        {
                            player.Bounce(7);
                        }
                    }
                    else
                    {
                        Schedule<EnemyDeath>().enemy = enemy;
                        player.Bounce(2);
                    }
                }
                
                if (willHurtPlayer)
                {
                    player.health.Decrement();
                    //player.playerActions.knockback = true;
                }
            }
        }
    }
}