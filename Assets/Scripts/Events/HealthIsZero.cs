using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Stats;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when actor health reaches 0. This usually would result in a 
    /// </summary>
    /// <typeparam name="HealthIsZero"></typeparam>
    public class HealthIsZero : BaseGameEvent<HealthIsZero>
    {
        public Health health;
        private Simulation.Event scheduledEvent;

        public override void Execute()
        {
            var enemy = health.gameObject.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                scheduledEvent = Schedule<EnemyDeath>();
                ((EnemyDeath)scheduledEvent).enemy = enemy;
                Debug.Log($"Enemy {health.name} has died.");
            }

            switch (health.gameObject.tag)
            {
                case "Player":
                    SaveGameManager.Save(model.Player);
                    scheduledEvent = Schedule<PlayerDeath>();
                    break;
                case "Boss":
                    var boss = health.gameObject.GetComponentInChildren<Boss>();
                    if (boss != null)
                    {
                        scheduledEvent = Schedule<BossTransformationEvent>(2); // Schedule with a delay
                        ((BossTransformationEvent)scheduledEvent).boss = boss;
                    }
                    break;
                case "Major":
                case "Minor":
                    // Major and Minor enemies already handled by the common enemy logic
                    break;
                default:
                    Debug.Log(health.gameObject.tag);
                    break;
            }
        }
    }
}
