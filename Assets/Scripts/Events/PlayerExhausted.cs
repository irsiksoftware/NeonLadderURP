namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player has died.
    /// </summary>
    /// <typeparam name="PlayerDeath"></typeparam>
    public class PlayerExhausted : BaseGameEvent<PlayerDeath>
    {

        public override void Execute()
        {
            var player = model.Player;
            if (player.Health.IsAlive)
            {
                player.Stamina.Deplete();
                
            }
        }
    }
}