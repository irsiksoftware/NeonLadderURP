using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the Jump Input is deactivated by the user, cancelling the upward velocity of the jump.
    /// </summary>
    /// <typeparam name="PlayerStopJump"></typeparam>
    public class BossStopJump : BaseGameEvent<BossStopJump>
    {
        public Boss boss;

        public override void Execute()
        {
            boss.GetComponent<Rigidbody>().velocity = new Vector2(boss.GetComponent<Rigidbody>().velocity.x, 0);
        }
    }
}