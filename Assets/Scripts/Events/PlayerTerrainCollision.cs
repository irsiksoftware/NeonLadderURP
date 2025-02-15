using UnityEngine;

using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class PlayerTerrainCollision : Event<PlayerTerrainCollision>
    {

        public override void Execute()
        {
            Schedule<PlayerLanded>();
            Debug.Log("Player collided with terrain.");
        }
    }
}