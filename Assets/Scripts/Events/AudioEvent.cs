using NeonLadder.Core;
using UnityEngine;

namespace NeonLadder.Events
{
    /// <summary>
    /// Event for centralized audio management
    /// Replaces direct audioSource.PlayOneShot() calls throughout the codebase
    /// </summary>
    public class AudioEvent : Simulation.Event
    {
        public AudioSource audioSource;
        public AudioClip audioClip;
        public AudioEventType audioType;
        public float volume = 1.0f;

        public override bool Precondition()
        {
            return audioSource != null && (audioClip != null || audioType != AudioEventType.Custom);
        }

        public override void Execute()
        {
            if (audioSource != null)
            {
                AudioClip clipToPlay = audioClip ?? GetClipForType(audioType);
                
                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay, volume);
                }
            }
        }

        private AudioClip GetClipForType(AudioEventType type)
        {
            // This would need to be connected to the specific character's audio clips
            // For now, return null and let the calling code specify the clip directly
            return null;
        }

        internal override void Cleanup()
        {
            audioSource = null;
            audioClip = null;
        }
    }

    /// <summary>
    /// Types of audio events for consistent sound management
    /// </summary>
    public enum AudioEventType
    {
        Jump,
        Land,
        Damage,
        Death,
        Attack,
        Respawn,
        Healing,
        Custom
    }
}