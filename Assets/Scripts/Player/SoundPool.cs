using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class SoundPool : MonoBehaviour
{
    private class Ref<T>
    {
        private T backing;
        public T Value { get { return backing; } set { backing = value; } }
        public Ref(T reference)
        {
            backing = reference;
        }
    }

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
    private void ResetSource(Ref<AudioSource> source)
    {
        source.Value.gameObject.SetActive(false);
        source.Value.volume = 1;
        source.Value.loop = false;
        source.Value = null;
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
        Ref<AudioSource> refEngine = new Ref<AudioSource>(engineSource);
        StartCoroutine(FadeOutSFX(refEngine, 1));
        engineSource = null;
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
        //ResetSource(driftSource);
    }

    public void PlayBoostReady()
    {
        AudioSource source = GetAvailableSource();
        Ref<AudioSource> refSource = new Ref<AudioSource>(source);
        SoundManager.Instance.PlayBoostCharged(source);
        StartCoroutine(KillSource(refSource));
    }

    public void PlayOrderPickup()
    {
        AudioSource source = GetAvailableSource();
        Ref<AudioSource> refSource = new Ref<AudioSource> (source);
        SoundManager.Instance.PlayPickupSound(source);
        StartCoroutine(KillSource(refSource));
    }

    public void PlayOrderDropoff()
    {
        AudioSource source = GetAvailableSource();
        Ref<AudioSource> refSource = new Ref<AudioSource>(source);
        SoundManager.Instance.PlayDropoffSound(source);
        StartCoroutine(KillSource(refSource));
    }

    public void PlayOrderTheft()
    {
        AudioSource source = GetAvailableSource();
        Ref<AudioSource> refSource = new Ref<AudioSource>(source);
        SoundManager.Instance.PlayStealingSound(source);
        StartCoroutine(KillSource(refSource));
    }

    /// <summary>
    /// This coroutine "fades out" a passed in audio source over duration seconds. It's not called with the dedicated start/stop coroutine methods
    /// as it might need to run on multiple threads at once.
    /// </summary>
    /// <param name="source">The audio source to fade out</param>
    /// <param name="duration">Time in seconds the audio source takes to fade</param>
    /// <returns></returns>
    private IEnumerator FadeOutSFX(Ref<AudioSource> source, float duration)
    {
        source.Value.DOFade(0, duration);
        while (source.Value.volume > 0.1f)
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
    private IEnumerator KillSource(Ref<AudioSource> source)
    {
        while(source.Value.isPlaying)
        {
            yield return null;
        }
        ResetSource(source);
    }
}
