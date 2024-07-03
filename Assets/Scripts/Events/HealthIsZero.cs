using NeonLadder.Mechanics.Controllers;
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
    public class HealthIsZero : BaseGameEvent<HealthIsZero>
    {
        public Health health;
        public Transform spawnPoint;

        public override void Execute()
        {
            switch (health.gameObject.tag)
            {
                case "Player":
                    SaveGameManager.Save(model.Player);
                    Schedule<PlayerDeath>();
                    break;
                case "Boss":
                case "Major":
                case "Minor":
                    var ev = Schedule<EnemyDeath>();
                    ev.enemy = health.gameObject.GetComponentInChildren<Enemy>();
                    break;
                default:
                    Debug.Log(health.gameObject.tag);
                    break;
            }
        }
    }
}