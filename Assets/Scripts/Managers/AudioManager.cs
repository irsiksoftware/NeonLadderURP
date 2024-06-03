using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class BossTrack
{
    public int bossId;
    public AudioClip intro;
    public AudioClip loop;
    public AudioClip outro;
}

public interface IAudioManager
{
    void PlayBackgroundTrack();
    void PlayBossTrack(int bossId);
}


public class AudioManager : MonoBehaviour, IAudioManager
{
    public List<AudioClip> backgroundTracks;
    public List<BossTrack> bossTracks;
    public AudioClip PlayerMotivation;
    public AudioClip BossIntroReveal;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayBackgroundTrack();
    }

    public void PlayBackgroundTrack()
    {
        AudioClip selectedTrack = backgroundTracks[Random.Range(0, backgroundTracks.Count)];
        audioSource.clip = selectedTrack;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    public void PlayBossTrack(int bossId)
    {
        StopAllCoroutines();
        StartCoroutine(BossIntro(bossId));
    }

    private IEnumerator BossIntro(int bossId)
    {
        var track = bossTracks.Single(bt => bt.bossId == bossId);
        audioSource.Stop();
        audioSource.clip = track.intro;
        audioSource.loop = false;
        audioSource.Play();
        yield return new WaitForSeconds(track.intro.length - 0.5f);
        
        BossMidFight(bossId);
    }

    private void BossMidFight(int bossId)
    {
        var track = bossTracks[bossId];
        audioSource.clip = track.loop;
        audioSource.loop = true;
        audioSource.Play();
    }
}
