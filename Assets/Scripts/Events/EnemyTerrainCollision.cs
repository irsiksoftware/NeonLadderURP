using UnityEngine;

using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class EnemyTerrainCollision : Event<EnemyTerrainCollision>
    {
        public override void Execute()
        {
            Debug.Log("Enemy collided with terrain.");
        }
    }
}