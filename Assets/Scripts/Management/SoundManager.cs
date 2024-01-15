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
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

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
    [SerializeField] private AudioClip phasing;
    [SerializeField] private AudioClip orderPickup;
    [SerializeField] private AudioClip orderDropoff;
    [SerializeField] private AudioClip orderTheft;

    [Header("UI")]
    [SerializeField] private AudioClip enter;
    [SerializeField] private AudioClip back;
    [SerializeField] private AudioClip scroll;

    [Header("Emotes")]
    [Tooltip("[0]: Top, [1]: Right, [2]: Bottom, [3]: Left")]
    [SerializeField] private AudioClip[] emoteSFX;
    [Range(0f, 1f)]
    [SerializeField] private float emotePitchMin;
    [Range(1f, 2f)]
    [SerializeField] private float emotePitchMax;

    private void OnEnable()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.volume = 0.2f;
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

    /// <summary>
    /// Here's where you'll find all the AudioKeys
    /// </summary>
    private void Start()
    {
        sfxDictionary.Add("engine", engineActive);
        sfxDictionary.Add("idle", engineIdle);
        sfxDictionary.Add("drift", drift);
        sfxDictionary.Add("brake", brake);
        sfxDictionary.Add("boost_used", boostUsed);
        sfxDictionary.Add("boost_charged", boostCharged);
        sfxDictionary.Add("pickup", orderPickup);
        sfxDictionary.Add("dropoff", orderDropoff);
        sfxDictionary.Add("whoosh", orderTheft);
        sfxDictionary.Add("phasing", phasing);
        sfxDictionary.Add("confirm", enter);
        sfxDictionary.Add("back", back);
        sfxDictionary.Add("scroll", scroll);
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

    public void PlaySFX(string key, AudioSource source)
    {
        source.clip = sfxDictionary[key];
        source.gameObject.SetActive(true);
        source.Play();
    }

    public void PlayEngineSound(AudioSource source)
    {
        //source.volume = 0.1f;
        source.clip = engineActive;
        source.gameObject.SetActive(true);
        source.Play();
    }

    public void PlayIdleSound(AudioSource source)
    {
        source.clip = engineIdle;
        source.gameObject.SetActive(true);
        source.Play();
    }

    // for playing sticker sounds
    public void PlayEmoteSound(AudioSource source, int index)
    {
        try
        {
            source.clip = emoteSFX[index];
            source.pitch = Random.Range(emotePitchMin, emotePitchMax);
            source.gameObject.SetActive(true);
            source.Play();
        } 
        catch
        {
            Debug.LogError($"Couldn't find index {index} of EmoteSFX array length {emoteSFX.Length}.");
            return;
        }
    }

    /// <summary>
    /// This method returns the SFX from the dictionary with specified key. Returns null if clip can't be found and debugs the error.
    /// </summary>
    /// <param name="key">Key of the clip in the dictionary.</param>
    /// <returns></returns>
    public AudioClip GetSFX(string key)
    {
        AudioClip outClip;
        if(sfxDictionary.TryGetValue(key, out outClip))
        {
            return outClip;
        }
        Debug.LogError($"Couldn't find clip in dictionary with key: {key}.");
        return null;
    }
}
