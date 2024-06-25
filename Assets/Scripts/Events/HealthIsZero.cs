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
            //Debug.Log($"{nameof(HealthIsZero)} event executed for ${health.gameObject.tag}");
            switch (health.gameObject.tag)
            {
                case "Player":
                    Schedule<SaveGame>();
                    Schedule<PlayerDeath>();
                    //Schedule<FadeOutCamera>();
                    //Schedule<RestartScene>(7); // Delay to allow fade out and death animation
                    //Schedule<PlayerSpawn>();
                    break;
                case "Boss":
                case "Major":
                case "Minor":
                    health.gameObject.layer = LayerMask.NameToLayer("TransparentFX");
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