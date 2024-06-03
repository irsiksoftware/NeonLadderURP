using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{
    /// <summary>
    /// This event is fired when user input should be enabled.
    /// </summary>
    public class EnablePlayerInput : BaseGameEvent<EnablePlayerInput>
    {
        Player player;

        public override void Execute()
        {
            player.controlEnabled = true;
        }
    }
}