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
                        // Schedule enemy damage through event system
                        var enemyDamageEvent = Schedule<HealthDamageEvent>(0f);
                        enemyDamageEvent.health = enemyHealth;
                        enemyDamageEvent.damageAmount = 1f;
                        enemyDamageEvent.triggerEffects = true;
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
                    // Schedule player damage through event system
                    player.ScheduleDamage(1f, 0f);
                    //player.playerActions.knockback = true;
                }
            }
        }
    }
}