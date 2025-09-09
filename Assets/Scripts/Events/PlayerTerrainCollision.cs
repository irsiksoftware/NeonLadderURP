using NeonLadder.Debugging;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

using static NeonLadder.Core.Simulation;

namespace NeonLadder.Events
{
    public class PlayerTerrainCollision : Event<PlayerTerrainCollision>
    {

        public Player player;
        public override void Execute()
        {
            var playerLandedEvent = Schedule<PlayerLanded>();
            playerLandedEvent.Player = player;
            Debugger.Log(LogCategory.Player, "Player collided with terrain.");
        }
    }
}