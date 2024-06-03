using Platformer.Mechanics;

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
            model.player.animator.SetTrigger("victory");
            model.player.controlEnabled = false;
        }
    }
}