using Assets.Scripts;
using NeonLadder.Core;
using NeonLadder.Mechanics.Controllers;
using NeonLadder.Mechanics.Enums;
using UnityEngine;


namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : BaseGameEvent<PlayerSpawn>
    {
        Transform spawnPoint;

        public override void Execute()
        {
            var player = model.Player;
            player.GetComponent<Collider>().enabled = true;
            player.controlEnabled = false;
            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);
            player.health.Increment(Constants.MaxHealth);
            player.stamina.Increment(Constants.MaxStamina);
            player.Teleport(spawnPoint.transform.position);
            player.playerActions.jumpState = ActionStates.Ready;
            player.animator.SetBool("dead", false);
            model.VirtualCamera.m_Follow = player.transform;
            model.VirtualCamera.m_LookAt = player.transform;
            Simulation.Schedule<EnablePlayerInput>(2f);
        }
    }
}