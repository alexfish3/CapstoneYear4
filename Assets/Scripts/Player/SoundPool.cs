using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Animations;

public class SoundPool : MonoBehaviour
{
    private AudioSource[] sourcePool;

    // refs to specific sources
    private AudioSource drivingSource;
    private AudioSource engineSource;
    private AudioSource brakeSource;
    private AudioSource driftSource;
    private AudioSource boostSource;
    private AudioSource miniBoostSource;
    private AudioSource driftSparkSource;

    private int currDriftIndex = -1;

    // bools for controlling when certain sounds should play
    private bool shouldPlay = false;
    private bool phasing = false;

    private IEnumerator emoteRoutine;

    private void Awake()
    {
        GameObject sourceGO;
        sourcePool = new AudioSource[SoundManager.Instance.PoolSize];
        GameObject audioSource = SoundManager.Instance.AudioSourcePrefab;
        for(int i=0;i<sourcePool.Length; i++)
        {
            sourceGO = Instantiate(audioSource, transform);
            sourcePool[i] = sourceGO.GetComponent<AudioSource>();
            sourcePool[i].gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.OnSwapBegin += InitEngineSource;
        GameManager.Instance.OnSwapFinalPackage += InitEngineSource;
        GameManager.Instance.OnSwapResults += TurnOffPlayerSounds;
        GameManager.Instance.OnSwapMenu += TurnOffPlayerSounds;
        GameManager.Instance.OnSwapAnything += StopAudio;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnSwapBegin -= InitEngineSource;
        GameManager.Instance.OnSwapFinalPackage -= InitEngineSource;
        GameManager.Instance.OnSwapResults -= TurnOffPlayerSounds;
        GameManager.Instance.OnSwapMenu -= TurnOffPlayerSounds;
        GameManager.Instance.OnSwapAnything -= StopAudio;
    }

    /// <summary>
    /// This method resets an audio source so it is able to be pulled from the sound pool again.
    /// </summary>
    /// <param name="source">AudioSource to be reset</param>
    private void ResetSource(AudioSource source)
    {
        source.Stop();
        source.gameObject.SetActive(false);
        SoundManager.Instance.SwitchSource(ref source, "SFX");
        source.volume = 1;
        source.pitch = 1;
        source.loop = false;
    }

    /// <summary>
    /// Gets an availabe audio source from the pool. Creates a new one if none are available.
    /// </summary>
    /// <returns></returns>
    public AudioSource GetAvailableSource()
    {
        foreach(AudioSource source in sourcePool)
        {
            if(!source.gameObject.activeInHierarchy)
            {
                return source;
            }
        }
        // hopefully won't get here, if it happens frequently we can increase the pool size in SM
        Debug.LogError("Created new source, pool too small.");
        return Instantiate(SoundManager.Instance.AudioSourcePrefab, transform).GetComponent<AudioSource>();
    }

    private void InitEngineSource()
    {
        if(engineSource == null) { engineSource = GetAvailableSource(); }
        SoundManager.Instance.SwitchSource(ref engineSource, "Player");
        engineSource.loop = true;
        shouldPlay = true;
        PlayIdleSound();
    }

    private void TurnOffPlayerSounds()
    {
        shouldPlay = false;
        if (engineSource == null) { return; }
        ResetSource(engineSource);
        engineSource = null;
    }

    // below are methods for starting and stopping specific sounds. Because of this there's no need to use our normal commenting standards.

    public void PlayDrivingSound()
    {
        if (!shouldPlay || drivingSource != null) { return; };
        drivingSource = GetAvailableSource();
        SoundManager.Instance.SwitchSource(ref drivingSource, "Player");
        drivingSource.loop = true;
        SoundManager.Instance.PlayEngineSound(drivingSource);
    }
    public void StopDrivingSound()
    {
        if(drivingSource != null)
        {
            ResetSource(drivingSource);
            drivingSource = null;
        }
    }
    public void PlayIdleSound()
    {
        if(!shouldPlay) { return; };
        if(drivingSource != null)
        {
            drivingSource.Stop();
            ResetSource(drivingSource);
            drivingSource = null;
        }
        engineSource.volume = 0.5f;
        engineSource.loop = true;
        SoundManager.Instance.PlayIdleSound(engineSource);
    }
    public void PlayDriftSound()
    {
        return; // TODO: fix this or don't play a drift
        if(driftSource != null) { return; }
        driftSource = GetAvailableSource();
        SoundManager.Instance.SwitchSource(ref driftSource, "Player");
        driftSource.loop = true;
        SoundManager.Instance.PlaySFX("drift", driftSource);

    }
    public void StopDriftSound()
    {
        if(driftSource == null) { return; }
        ResetSource(driftSource);
        driftSource = null;
    }
    public void PlayBrakeSound()
    {
        if(brakeSource == null) { brakeSource = GetAvailableSource(); }
        if (!shouldPlay || brakeSource.isPlaying) { return; }
        SoundManager.Instance.SwitchSource(ref brakeSource, "Player");
        SoundManager.Instance.PlaySFX("brake", brakeSource);
        StartCoroutine(KillSource(brakeSource));
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
        SoundManager.Instance.SwitchSource(ref boostSource, "Player");
        SoundManager.Instance.PlaySFX("boost_used", boostSource);
        StartCoroutine(KillSource(boostSource));
    }
    public void PlayMiniBoost()
    {
        if(miniBoostSource == null) { miniBoostSource = GetAvailableSource(); }
        if (miniBoostSource.isPlaying || !shouldPlay) { return; }
        SoundManager.Instance.SwitchSource(ref miniBoostSource, "Player");
        SoundManager.Instance.PlaySFX("mini", miniBoostSource);
        StartCoroutine(KillSource(miniBoostSource));
    }
    public void PlayPhaseSound()
    {
        if(phasing || boostSource == null) { return; }
        engineSource.Stop();
        SoundManager.Instance.PlaySFX("phasing", boostSource);
        phasing = true;
    }
    public void StopPhaseSound()
    {
        if(engineSource == null || boostSource == null) { return; }
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
    public void PlayOrderDropoff(string dropoffType = "dropoff")
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX(dropoffType, source);
        StartCoroutine(KillSource(source));
    }
    public void PlayOrderTheft()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("whoosh", source);
        StartCoroutine(KillSource(source));
    }
    public void PlayDeathSound()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.SwitchSource(ref source, "Player");
        SoundManager.Instance.PlaySFX("death", source);
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
    public void PlayPauseUI()
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlaySFX("pause", source);
        StartCoroutine(KillSource(source));
    }

    public void PlayDriftSpark(int index)
    {
        if (currDriftIndex == index) { return; }
        currDriftIndex = index;
        if(driftSparkSource == null)
            driftSparkSource = GetAvailableSource();

        SoundManager.Instance.SwitchSource(ref driftSparkSource, "SFX");
        SoundManager.Instance.PlayDriftSparkSound(driftSparkSource, index);
    }

    public void StopDriftSpark()
    {
        if (driftSparkSource != null)
            ResetSource(driftSparkSource);
        currDriftIndex = -1;
    }    

    // emote
    public void PlayEmote(int index)
    {
        AudioSource source = GetAvailableSource();
        SoundManager.Instance.PlayEmoteSound(source, index);
        StartCoroutine (KillSource(source));
    }

    public void StopAudio()
    {
        foreach(AudioSource source in sourcePool)
        {
            source.Stop();
        }
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
        engineSource.loop = true;
        SoundManager.Instance.PlayIdleSound(engineSource);
        
    }
}
