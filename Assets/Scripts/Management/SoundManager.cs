using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This class is a singleton that plays audio. It has an audio source for game music and methods that play respective audio clips on passed in sources.
/// </summary>
public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    private AudioSource musicSource;

    [Header("Pooling Information")]
    [Tooltip("How many audio sources are in each player's pool")]
    [SerializeField] private int playerPoolSize = 10;
    [Tooltip("Prefab of the audio source GO")]
    [SerializeField] private GameObject audioSourcePrefab;
    public GameObject AudioSourcePrefab { get { return audioSourcePrefab; } }
    public int PoolSize { get { return playerPoolSize; } }

    [Header("Audio Clips")]
    [Header("Music")]
    [SerializeField] private AudioClip mainGameBGM;
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip finalOrderBGM;
    [SerializeField] private AudioClip resultsBGM;

    [Header("SFX")]
    [SerializeField] private AudioClip engineActive;
    [SerializeField] private AudioClip engineIdle;
    [SerializeField] private AudioClip drift;
    [SerializeField] private AudioClip brake;
    [SerializeField] private AudioClip boostUsed;
    [SerializeField] private AudioClip boostCharged;
    [SerializeField] private AudioClip orderPickup;
    [SerializeField] private AudioClip orderDropoff;
    [SerializeField] private AudioClip orderTheft;

    private void OnEnable()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.volume = 0.4f;
        // events for music
        GameManager.Instance.OnSwapMenu += PlayMenuTheme;
        GameManager.Instance.OnSwapMainLoop += PlayMainTheme;
        GameManager.Instance.OnSwapFinalPackage += PlayFinalTheme;
        GameManager.Instance.OnSwapResults += PlayResultsTheme;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapMenu -= PlayMenuTheme;
        GameManager.Instance.OnSwapMainLoop -= PlayMainTheme;
        GameManager.Instance.OnSwapFinalPackage -= PlayFinalTheme;
        GameManager.Instance.OnSwapResults -= PlayResultsTheme;
    }

    // below are methods to play various BGMs
    private void PlayMenuTheme()
    {
        musicSource.clip = mainMenuBGM;
        musicSource.Play();
    }

    private void PlayMainTheme()
    {
        musicSource.clip = mainGameBGM;
        musicSource.Play();
    }

    private void PlayFinalTheme()
    {
        musicSource.clip = finalOrderBGM;
        musicSource.Play();
    }

    private void PlayResultsTheme()
    {
        musicSource.clip = resultsBGM;
        musicSource.Play();
    }

    // the below methods are similar but play SFXs. The audio source to be used is the only param

    public void PlayEngineSound(AudioSource source)
    {
        source.volume = 0.3f;
        source.clip = engineActive;
        source.gameObject.SetActive(true);
    }

    public void PlayDriftingSound(AudioSource source)
    {
        source.clip = drift;
        source.gameObject.SetActive(true);
    }

    public void PlayPickupSound(AudioSource source)
    {
        source.clip = orderPickup;
        source.gameObject.SetActive(true);
    }

    public void PlayDropoffSound(AudioSource source)
    {
        source.clip = orderDropoff;
        source.gameObject.SetActive(true);
    }

    public void PlayStealingSound(AudioSource source)
    {
        source.clip = orderTheft; 
        source.gameObject.SetActive(true);
    }

    public void PlayBoostCharged(AudioSource source)
    {
        source.clip = boostCharged;
        source.gameObject.SetActive(true);
    }
}
