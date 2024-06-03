using Platformer.Mechanics;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when a player collides with a token.
    /// </summary>
    /// <typeparam name="PlayerCollision"></typeparam>
    public class PlayerCurrencyCollision : BaseGameEvent<PlayerCurrencyCollision>
    {
        public CurrencyInstance token;

        public override void Execute()
        {
            AudioSource.PlayClipAtPoint(token.tokenCollectAudio, token.transform.position);
        }
    }
}