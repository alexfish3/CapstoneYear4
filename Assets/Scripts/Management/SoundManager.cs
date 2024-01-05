using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    private AudioSource musicSource;

    [Header("Pooling Information")]
    [SerializeField] private int playerPoolSize = 10;
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

    private void OnEnable()
    {
        musicSource = GetComponent<AudioSource>();

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
        source.clip = engineActive;
        source.gameObject.SetActive(true);
    }
}
