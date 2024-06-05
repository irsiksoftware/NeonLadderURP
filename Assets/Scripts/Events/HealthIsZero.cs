using NeonLadder.Mechanics.Stats;
using UnityEngine;
using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player health reaches 0. This usually would result in a 
    /// PlayerDeath event.
    /// </summary>
    /// <typeparam name="HealthIsZero"></typeparam>
    public class HealthIsZero : Event<HealthIsZero>
    {
        public Health health;

        public override void Execute()
        {
            Debug.Log($"{nameof(HealthIsZero)} event executed for ${health.gameObject.tag}");
            switch (health.gameObject.tag)
            {
                case "Player":
                    Schedule<PlayerDeath>();
                    break;
                case "Boss":
                case "Major":
                case "Minor":
                    Schedule<EnemyDeath>();
                    break;
                default:
                    Debug.Log(health.gameObject.tag);
                    break;
            }
        }
    }
}