using Assets.Scripts;
using UnityEngine;


namespace NeonLadder.Events
{
    /// <summary>
    /// Fired when the player is spawned after dying.
    /// </summary>
    public class PlayerSpawn : BaseGameEvent<PlayerSpawn>
    {
        public override void Execute()
        {
            var player = model.Player;
            player.GetComponent<Collider>().enabled = true;
            player.controlEnabled = false;
            if (player.audioSource && player.respawnAudio)
                player.audioSource.PlayOneShot(player.respawnAudio);
            player.Health.Increment(Constants.MaxHealth);
            player.Stamina.Increment(Constants.MaxStamina);
            //player.Teleport(player.transform.position);
            //player.playerActions.jumpState = ActionStates.Ready;
            player.animator.SetInteger("locomotion_animation", 0);
            model.VirtualCamera.m_Follow = player.transform;
            model.VirtualCamera.m_LookAt = player.transform;
            player.controlEnabled = true;
        }
    }
}