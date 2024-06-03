using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    public AudioManager musicController;
    public int BossTrackNumber;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player has entered the trigger
        if (other.CompareTag("Player")) // Make sure your player GameObject has the tag "Player"
        {
            musicController.PlayBossTrack(1);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //stop the music when the
        // Optional: Change back to original music when player leaves
        //if (other.CompareTag("Player"))
        //{
        //    musicController.PlayRandomBackgroundTrack();
        //}
    }
}
