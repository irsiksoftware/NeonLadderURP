using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for centralized player audio management
    /// Handles all player-related sound effects through the event system
    /// </summary>
    public class PlayerAudioEvent : Simulation.Event
    {
        public Player player;
        public PlayerAudioType audioType;

        public override bool Precondition()
        {
            return player != null && player.audioSource != null;
        }

        public override void Execute()
        {
            if (player?.audioSource != null)
            {
                AudioClip clipToPlay = audioType switch
                {
                    PlayerAudioType.Jump => player.jumpAudio,
                    PlayerAudioType.Land => player.landOnGroundAudio,
                    PlayerAudioType.Ouch => player.ouchAudio,
                    PlayerAudioType.Respawn => player.respawnAudio,
                    _ => null
                };

                if (clipToPlay != null)
                {
                    player.audioSource.PlayOneShot(clipToPlay);
                }
            }
        }

        internal override void Cleanup()
        {
            player = null;
        }
    }

    /// <summary>
    /// Types of audio events for the player
    /// </summary>
    public enum PlayerAudioType
    {
        Jump,
        Land,
        Ouch,
        Respawn
    }
}