using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class SoundPool : MonoBehaviour
{
    private GameObject[] sourceGOs;

    // refs to specific sources
    private AudioSource engineSource;
    private AudioSource idleSource;
    private AudioSource driftSource;
    private AudioSource brakeSource;
    
    private void Awake()
    {
        sourceGOs = new GameObject[SoundManager.Instance.PoolSize];
        GameObject audioSource = SoundManager.Instance.AudioSourcePrefab;
        for(int i=0;i<sourceGOs.Length; i++)
        {
            sourceGOs[i] = Instantiate(audioSource, transform);
            sourceGOs[i].SetActive(false);
        }
    }

    /// <summary>
    /// This method resets an audio source so it is able to be pulled from the sound pool again.
    /// </summary>
    /// <param name="source">AudioSource to be reset</param>
    private void ResetSource(AudioSource source)
    {
        source.gameObject.SetActive(false);
        source.volume = 1;
        source.loop = false;
        source = null;
    }

    /// <summary>
    /// Gets an availabe audio source from the pool. Creates a new one if none are available.
    /// </summary>
    /// <returns></returns>
    public AudioSource GetAvailableSource()
    {
        foreach(GameObject go in sourceGOs)
        {
            if(!go.activeInHierarchy)
            {
                return go.GetComponent<AudioSource>();
            }
        }
        // hopefully won't get here, if it happens frequently we can increase the pool size in SM
        Debug.Log("Created new source");
        return Instantiate(SoundManager.Instance.AudioSourcePrefab, transform).GetComponent<AudioSource>();
    }

    // below are methods for starting and stopping specific sounds. Because of this there's no need to use our normal commenting standards.

    public void PlayEngineSound()
    {
        if (engineSource != null) { return; }
        engineSource = GetAvailableSource();
        engineSource.loop = true;
        SoundManager.Instance.PlayEngineSound(engineSource);
    }
    public void StopEngineSound()
    {
        if(engineSource == null) { return; }
        StartCoroutine(FadeOutSFX(engineSource, 1));
    }

    public void PlayDriftSound()
    {
        if(driftSource != null) { return; }
        driftSource = GetAvailableSource();
        driftSource.loop = true;
        SoundManager.Instance.PlayDriftingSound(driftSource);

    }
    public void StopDriftSound()
    {
        if(driftSource == null) { return; }
        ResetSource(driftSource);
    }

    public void PlayBoostReady()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlayBoostCharged(source);
        StartCoroutine(KillSource(source));
    }

    public void PlayOrderPickup()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlayPickupSound(source);
        StartCoroutine(KillSource(source));
    }

    public void PlayOrderDropoff()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlayDropoffSound(source);
        StartCoroutine(KillSource(source));
    }

    public void PlayOrderTheft()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlayStealingSound(source);
        StartCoroutine(KillSource(source));
    }

    /// <summary>
    /// This coroutine "fades out" a passed in audio source over duration seconds. It's not called with the dedicated start/stop coroutine methods
    /// as it might need to run on multiple threads at once.
    /// </summary>
    /// <param name="source">The audio source to fade out</param>
    /// <param name="duration">Time in seconds the audio source takes to fade</param>
    /// <returns></returns>
    private IEnumerator FadeOutSFX(AudioSource source, float duration)
    {
        source.DOFade(0, duration);
        while (source.volume > 0)
        {
            yield return null;
        }
        ResetSource(source);
    }

    /// <summary>
    /// This coroutine is used for killing non-looping SFXs. It also won't be called with dedicated methods.
    /// </summary>
    /// <param name="source">Source to be killed after playback</param>
    /// <returns></returns>
    private IEnumerator KillSource(AudioSource source)
    {
        while(source.isPlaying)
        {
            yield return null;
        }
        ResetSource(source);
    }
}
