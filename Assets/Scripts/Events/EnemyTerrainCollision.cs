using NeonLadder.Mechanics.Controllers;
using UnityEngine;

using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class EnemyTerrainCollision : Event<EnemyTerrainCollision>
    {
        public Enemy enemy { get; set; }
        public override void Execute()
        {
            Debug.Log($"Enemy {enemy.GetComponentInParent<Rigidbody>().gameObject.name} collided with terrain.");
        }
    }
}