using Platformer.Mechanics;
using NeonLadder.Mechanics.Controllers;

namespace NeonLadder.Events
{

    /// <summary>
    /// This event is triggered when the player character enters a trigger with a VictoryZone component.
    /// </summary>
    /// <typeparam name="PlayerEnteredVictoryZone"></typeparam>
    public class PlayerEnteredVictoryZone : BaseGameEvent<PlayerEnteredVictoryZone>
    {
        public VictoryZone victoryZone;

        public override void Execute()
        {
            model.Player.Animator.SetTrigger("victory");
            var mediator = model.Player.GetComponent<PlayerStateMediator>();
            mediator?.DisablePlayerActionMap();
        }
    }
}