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
                case "BossTransformation":
                    // the Counter side of the boss -> boss transformation event, which still has the Enemy Script, so is able to die.
                    break;
                default:
                    Debug.Log($"Enemy with name: {health.gameObject.name} has not tag, and has died.");
                    break;
            }
        }
    }
}
