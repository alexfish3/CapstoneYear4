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
    private AudioSource driftSource;
    private AudioSource boostSource;

    // bools for controlling when certain sounds should play
    private bool shouldPlay = false;
    private bool idling = false;
    private bool phasing = false;

    private IEnumerator brakingRoutine;
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

    private void OnEnable()
    {
        GameManager.Instance.OnSwapMainLoop += InitEngineSource;
        GameManager.Instance.OnSwapResults += TurnOffPlayerSounds;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapMainLoop -= InitEngineSource;
        GameManager.Instance.OnSwapResults -= TurnOffPlayerSounds;
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
        Debug.LogError("Created new source, pool too small.");
        return Instantiate(SoundManager.Instance.AudioSourcePrefab, transform).GetComponent<AudioSource>();
    }

    private void InitEngineSource()
    {
        if(engineSource != null) { return; }
        engineSource = GetAvailableSource();
        engineSource.loop = true;
        shouldPlay = true;
    }

    private void TurnOffPlayerSounds()
    {
        shouldPlay = false;
    }

    // below are methods for starting and stopping specific sounds. Because of this there's no need to use our normal commenting standards.

    // TODO: fix these using GetClip() method
    public void PlayEngineSound()
    {
        if (!idling || !shouldPlay ) { return; };
        //if(brakingRoutine != null) { StopCoroutine(brakingRoutine); brakingRoutine = null; }
        SoundManager.Instance.PlayEngineSound(engineSource);
        idling = false;
        engineSource.loop = true;
    }
    public void StopEngineSound()
    {
        if(idling || !shouldPlay) { return; };
        /*        engineSource.loop = false;
                idling = true;
                SoundManager.Instance.PlaySFX("brake", engineSource);
                brakingRoutine = WaitForBrake();
                StartCoroutine(brakingRoutine);*/
        idling = true;
        engineSource.loop = true;
        //brakingRoutine = null;
        SoundManager.Instance.PlayIdleSound(engineSource);

    }
    public void PlayDriftSound()
    {
        if(driftSource != null) { return; }
        driftSource = GetAvailableSource();
        driftSource.loop = true;
        SoundManager.Instance.PlaySFX("drift", driftSource);

    }
    public void StopDriftSound()
    {
        if(driftSource == null) { return; }
        ResetSource(driftSource);
        driftSource = null;
    }
    public void PlayBoostReady()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("boost_charged", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayBoostActivate()
    {
        boostSource = GetAvailableSource();
        SoundManager.Instance.PlaySFX("boost_used", boostSource);
        StartCoroutine(KillSource(boostSource));
    }
    public void PlayPhaseSound()
    {
        if(phasing) { return; }
        engineSource.Stop();
        SoundManager.Instance.PlaySFX("phasing", boostSource);
        phasing = true;
    }
    public void StopPhaseSound()
    {
        if(boostSource == null) { return; }
        ResetSource(boostSource);
        boostSource = null;
        phasing = false;
        engineSource.Play();
    }
    public void PlayOrderPickup()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("pickup", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayOrderDropoff()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("dropoff", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayOrderTheft()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("whoosh", source);
        StartCoroutine(KillSource(source));
    }

    // UI sounds
    public void PlayEnterUI()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("confirm", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayBackUI()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("back", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayScrollUI()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("scroll", source);
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
        while (source.volume > 0.1f)
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

    private IEnumerator WaitForBrake()
    {
        while (engineSource.isPlaying)
        {
            yield return null;
        }
        idling = true;
        engineSource.loop = true;
        brakingRoutine = null;
        SoundManager.Instance.PlayIdleSound(engineSource);
        
    }
}
