using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

/// <summary>
/// This class is a singleton that plays audio. It has an audio source for game music and methods that play respective audio clips on passed in sources.
/// </summary>
public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    private AudioSource musicSource;
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioMixerSnapshot> snapshotDictionary = new Dictionary<string, AudioMixerSnapshot>();

    [Header("Mixing Information")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixerSnapshot gameplaySnapshot;
    [SerializeField] private AudioMixerSnapshot pausedSnapshot;

    [Header("Pooling Information")]
    [Tooltip("How many audio sources are in each player's pool")]
    [SerializeField] private int playerPoolSize = 10;
    [Tooltip("Prefab of the audio source GO")]
    [SerializeField] private GameObject audioSourcePrefab;
    public GameObject AudioSourcePrefab { get { return audioSourcePrefab; } }
    public int PoolSize { get { return playerPoolSize; } }

    [Header("Audio Clips")]
    [Header("Music")]
    [SerializeField] private AudioClip mainGameIntro;
    [SerializeField] private AudioClip mainGameLoop;
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip playerSelectBGM;
    [SerializeField] private AudioClip finalOrderBGM;
    [SerializeField] private AudioClip resultsBGM;

    [Header("SFX")]
    [SerializeField] private AudioClip engineActive;
    [SerializeField] private AudioClip engineIdle;
    [SerializeField] private AudioClip drift;
    [SerializeField] private AudioClip brake;
    [SerializeField] private AudioClip boostUsed;
    [SerializeField] private AudioClip boostCharged;
    [SerializeField] private AudioClip miniBoost;
    [SerializeField] private AudioClip phasing;
    [SerializeField] private AudioClip orderPickup;
    [SerializeField] private AudioClip orderDropoff;
    [SerializeField] private AudioClip finalDropoff;
    [SerializeField] private AudioClip orderTheft;
    [SerializeField] private AudioClip death;

    [Header("UI")]
    [SerializeField] private AudioClip enter;
    [SerializeField] private AudioClip back;
    [SerializeField] private AudioClip scroll;
    [SerializeField] private AudioClip pause;


    [Header("Emotes")]
    [Tooltip("[0]: Top, [1]: Right, [2]: Bottom, [3]: Left")]
    [SerializeField] private AudioClip[] emoteSFX;
    [Range(0f, 1f)] [SerializeField] private float emotePitchMin;
    [Range(1f, 2f)] [SerializeField] private float emotePitchMax;

    private void OnEnable()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.volume = 0.2f;
        // events for music
        GameManager.Instance.OnSwapMenu += PlayMenuTheme;
        GameManager.Instance.OnSwapPlayerSelect += PlayPlayerSelectTheme;
        GameManager.Instance.OnSwapMainLoop += PlayMainTheme;
        GameManager.Instance.OnSwapFinalPackage += PlayFinalTheme;
        GameManager.Instance.OnSwapResults += PlayResultsTheme;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapMenu -= PlayMenuTheme;
        GameManager.Instance.OnSwapPlayerSelect -= PlayPlayerSelectTheme;
        GameManager.Instance.OnSwapMainLoop -= PlayMainTheme;
        GameManager.Instance.OnSwapFinalPackage -= PlayFinalTheme;
        GameManager.Instance.OnSwapResults -= PlayResultsTheme;
    }

    /// <summary>
    /// Here's where you'll find all the AudioKeys
    /// </summary>
    private void Start()
    {
        // snapshot
        snapshotDictionary.Add("gameplay", gameplaySnapshot);
        snapshotDictionary.Add("paused", pausedSnapshot);

        // gameplay
        sfxDictionary.Add("engine", engineActive);
        sfxDictionary.Add("idle", engineIdle);
        sfxDictionary.Add("drift", drift);
        sfxDictionary.Add("brake", brake);
        sfxDictionary.Add("boost_used", boostUsed);
        sfxDictionary.Add("boost_charged", boostCharged);
        sfxDictionary.Add("pickup", orderPickup);
        sfxDictionary.Add("dropoff", orderDropoff);
        sfxDictionary.Add("final_dropoff", finalDropoff);
        sfxDictionary.Add("whoosh", orderTheft);
        sfxDictionary.Add("phasing", phasing);
        sfxDictionary.Add("mini", miniBoost);
        sfxDictionary.Add("death", death);

        // UI
        sfxDictionary.Add("confirm", enter);
        sfxDictionary.Add("back", back);
        sfxDictionary.Add("scroll", scroll);
        sfxDictionary.Add("pause", pause);

        
    }
    // below are methods to play various BGMs
    private void PlayMenuTheme()
    {
        Debug.Log(Application.persistentDataPath);
        musicSource.clip = mainMenuBGM;
        musicSource.Play();
    }

    private void PlayPlayerSelectTheme()
    {
        musicSource.clip = playerSelectBGM;
        musicSource.Play();
    }

    private void PlayMainTheme()
    {
        if (mainGameIntro != null)
        {
            musicSource.clip = mainGameIntro;
            musicSource.loop = false;
            StartCoroutine(PlayIntro(musicSource, mainGameLoop));
        }
        else
        {
            musicSource.clip = mainGameLoop;
            musicSource.loop = true;
            musicSource.Play();
        }
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
            SwitchSource(ref source, "Emote");
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
    /// Switches the audio snapshot based on the provided key over the course of provided seconds.
    /// </summary>
    /// <param name="key">Key of the snapshot you're switching to</param>
    /// <param name="time">Time you want it to take, default is 0.1f</param>
    public void ChangeSnapshot(string key, float time=0.1f)
    {
        if(snapshotDictionary.ContainsKey(key)) // this won't get called as often as SFX so I'm less worried about performance impact
        {
            snapshotDictionary[key].TransitionTo(time);
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

    /// <summary>
    /// This method switches the output mixer of an audio source, or debugs an error if the mixer couldn't be found.
    /// </summary>
    /// <param name="source">Source being switched</param>
    /// <param name="channel">Name of the output channel</param>
    public void SwitchSource(ref AudioSource source, string channel)
    {
        AudioMixerGroup targetGroup = mainMixer.FindMatchingGroups(channel)[0];
        if(targetGroup != null)
        {
            source.outputAudioMixerGroup = targetGroup;
        }
        else
        {
            Debug.LogError($"Couldn't find group in mixer with name: {channel}.");
        }
    }

    /// <summary>
    /// This coroutine waits for the intro to a song to play and then sets the looping source.
    /// </summary>
    /// <param name="source">Audio source music is playing from</param>
    /// <param name="loop">Looping song that should play after the into</param>
    /// <returns></returns>
    private IEnumerator PlayIntro(AudioSource source, AudioClip loop)
    {
        source.Play();
        yield return new WaitUntil(() => !source.isPlaying);
        source.clip = loop;
        source.Play();
        source.loop = true;
    }
}
