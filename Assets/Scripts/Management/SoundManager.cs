using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

/// <summary>
/// This class is a singleton that plays audio. It has an audio source for game music and methods that play respective audio clips on passed in sources.
/// </summary>
public class SoundManager : SingletonMonobehaviour<SoundManager>
{
    [Tooltip("Reference to the source that will play all the music.")]
    [SerializeField] private static AudioSource musicSource;
    private Dictionary<string, AudioObject> sfxDictionary = new Dictionary<string, AudioObject>();
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
    [SerializeField] private AudioObject mainGameLoop;
    [SerializeField] private AudioObject mainMenuBGM;
    [SerializeField] private AudioObject playerSelectBGM;
    [SerializeField] private AudioObject finalOrderBGM;
    [SerializeField] private AudioObject resultsBGM;

    [Header("SFX")]
    [SerializeField] private AudioObject engineActive;
    [SerializeField] private AudioObject engineIdle;
    //[SerializeField] private AudioObject drift;
    [SerializeField] private AudioObject brake;
    [SerializeField] private AudioObject boostUsed;
    [SerializeField] private AudioObject boostCharged;
    [SerializeField] private AudioObject miniBoost;
    [SerializeField] private AudioObject phasing;
    [SerializeField] private AudioObject orderPickup;
    [SerializeField] private AudioObject orderDropoff;
    [SerializeField] private AudioObject finalDropoff;
    [SerializeField] private AudioObject orderTheft;
    [SerializeField] private AudioObject death;
    [SerializeField] private AudioObject clockTowerBells;

    [SerializeField] private AudioObject[] driftSparks;

    [Header("UI")]
    [SerializeField] private AudioObject enter;
    [SerializeField] private AudioObject back;
    [SerializeField] private AudioObject scroll;
    [SerializeField] private AudioObject pause;


    [Header("Emotes")]
    [Tooltip("[0]: Top, [1]: Right, [2]: Bottom, [3]: Left")]
    [SerializeField] private AudioObject[] emoteSFX;
    [Range(0f, 1f)] [SerializeField] private float emotePitchMin;
    [Range(1f, 2f)] [SerializeField] private float emotePitchMax;

    [Header("Looping Logic")]
    [Tooltip("Length of the intro for the main gameplay theme.")]
    [SerializeField] private float gameThemeIntroLength = 1f;
    [SerializeField] private float finalThemeIntroLength = 1f;

    [Header("Debug")]
    [Tooltip("Will randomize noises to simulate multiple players.")]
    [SerializeField] private bool simulatePlayers;
    [Tooltip("We might not have player select music.")]
    [SerializeField] private bool playPlayerSelect;

    // looping coroutine
    private IEnumerator bgmRoutine;

    private bool shouldPlayMain = true;

    private void OnEnable()
    {
        musicSource = GetComponent<AudioSource>();
        // events for music
        GameManager.Instance.OnSwapMenu += PlayMenuTheme;
        GameManager.Instance.OnSwapPlayerSelect += PlayPlayerSelectTheme;
        GameManager.Instance.OnSwapStartingCutscene += PlayMainTheme;
        GameManager.Instance.OnSwapGoldenCutscene += PlayFinalTheme;
        GameManager.Instance.OnSwapResults += PlayResultsTheme;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnSwapMenu -= PlayMenuTheme;
        GameManager.Instance.OnSwapPlayerSelect += PlayPlayerSelectTheme;
        GameManager.Instance.OnSwapStartingCutscene -= PlayMainTheme;
        GameManager.Instance.OnSwapGoldenCutscene -= PlayFinalTheme;
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
        //sfxDictionary.Add("drift", drift);
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
        sfxDictionary.Add("bells", clockTowerBells);

        // UI
        sfxDictionary.Add("confirm", enter);
        sfxDictionary.Add("back", back);
        sfxDictionary.Add("scroll", scroll);
        sfxDictionary.Add("pause", pause);

        if(simulatePlayers)
        {
            IEnumerator p2, p3, p4;
            p2 = RandomNoises();
            p3 = RandomNoises();
            p4 = RandomNoises();
            StartCoroutine(p2);
            StartCoroutine(p3);
            StartCoroutine(p4);

        }
    }

    // below are methods to play various BGMs
    private void PlayMenuTheme()
    {
        if (!shouldPlayMain)
            return;

        shouldPlayMain = false;

        if(bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        musicSource.timeSamples = 0;
        musicSource.clip = mainMenuBGM.clip;
        musicSource.volume = mainMenuBGM.volume;
        musicSource.Play();
    }

    private void PlayPlayerSelectTheme()
    {
        if (!playPlayerSelect)
            return;

        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        musicSource.Pause();
        musicSource.clip = playerSelectBGM.clip;
        musicSource.volume = playerSelectBGM.volume;
        musicSource.Play();
    }

    private void PlayMainTheme()
    {
        shouldPlayMain = true;
        if (bgmRoutine != null)
        {
            StopCoroutine(bgmRoutine);
            bgmRoutine = null;
        }

        musicSource.Pause();
        musicSource.clip = mainGameLoop.clip;
        musicSource.volume = mainGameLoop.volume;
        bgmRoutine = PlayLoopedSongWithIntro(musicSource, gameThemeIntroLength);
        StartCoroutine(bgmRoutine);
    }

    private void PlayFinalTheme()
    {
        if (bgmRoutine != null)
        {
            StopCoroutine(bgmRoutine);
            bgmRoutine = null;
        }

        musicSource.clip = finalOrderBGM.clip;
        musicSource.volume = finalOrderBGM.volume;
        bgmRoutine = PlayLoopedSongWithIntro(musicSource, finalThemeIntroLength);
        StartCoroutine(bgmRoutine);
    }

    private void PlayResultsTheme()
    {
        if (bgmRoutine != null)
            StopCoroutine(bgmRoutine);

        musicSource.clip = resultsBGM.clip;
        musicSource.volume = resultsBGM.volume;
        musicSource.Play();
    }

    // the below methods are similar but play SFXs. The audio source to be used is the only param

    public void PlaySFX(string key, AudioSource source)
    {
        source.clip = sfxDictionary[key].clip;
        source.volume = sfxDictionary[key].volume;
        source.gameObject.SetActive(true);
        source.Play();
    }

    public void PlayEngineSound(AudioSource source)
    {
        //source.volume = 0.1f;
        source.clip = engineActive.clip;
        source.volume = engineActive.volume;
        source.gameObject.SetActive(true);
        source.Play();
    }

    public void PlayIdleSound(AudioSource source)
    {
        source.clip = engineIdle.clip;
        source.volume = engineIdle.volume;
        source.gameObject.SetActive(true);
        source.Play();
    }

    public void PlayDriftSparkSound(AudioSource source, int index)
    {
        try
        {
            source.clip = driftSparks[index].clip;
            source.volume = driftSparks[index].volume;
            source.gameObject.SetActive(true);
            source.Play();
        }
        catch
        {
            Debug.LogError($"Couldn't find index {index} of DriftSparks array length {driftSparks.Length}.");
            return;
        }
    }

    // for playing sticker sounds
    public void PlayEmoteSound(AudioSource source, int index)
    {
        try
        {
            source.clip = emoteSFX[index].clip;
            source.volume = emoteSFX[index].volume;
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
    public AudioObject GetSFX(string key)
    {
        AudioObject outClip;
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

    // for volume control

    public void SetSFX(float value)
    {
        mainMixer.SetFloat("SFX", value);        
    }

    public void SetMusic(float value)
    {
        mainMixer.SetFloat("Music", value);
    }

    /// <summary>
    /// This coroutine plays a looped song with an intro, so on subsequent loops it skips the intro.
    /// </summary>
    /// <param name="source">Audio source with preloaded clip</param>
    /// <param name="introLength">Length of the intro section</param>
    /// <returns></returns>
    IEnumerator PlayLoopedSongWithIntro(AudioSource source, float introLength)
    {
        if (source.clip != null)
        {
            int totalLength = Mathf.RoundToInt(source.clip.frequency * source.clip.length);
            int introTime = Mathf.RoundToInt(source.clip.frequency * introLength);
            while (true)
            {
                source.Play();
                while (source.timeSamples < totalLength)
                {
                    yield return null;
                }
                source.timeSamples = introTime;
            }
        }
        else
        {
            Debug.LogError("No audio clip present in looping coroutine.");
        }
    }

    /// <summary>
    /// This coroutine plays random noises from the SFX dictionary every. Used in debug to simulate multiple players because I have no bitches.
    /// </summary>
    /// <returns></returns>
    private IEnumerator RandomNoises()
    {
        AudioSource engine = Instantiate(audioSourcePrefab, this.transform).GetComponent<AudioSource>();
        SwitchSource(ref engine, "Player");
        engine.volume = 0.5f;
        engine.clip = engineIdle.clip;
        engine.loop = true;
        engine.Play();

        while (true)
        {
            AudioSource source = Instantiate(audioSourcePrefab, this.transform).GetComponent<AudioSource>();
            string channel = Random.Range(0, 10) < 5f ? "SFX" : "Player"; // to randomize the source
            SwitchSource(ref source, channel);


            source.clip = sfxDictionary.ElementAt(Random.Range(0, sfxDictionary.Count)).Value.clip;
            source.volume = sfxDictionary.ElementAt(Random.Range(0, sfxDictionary.Count)).Value.volume;
            source.Play();
            while (source.isPlaying)
            {
                yield return null;
            }
            yield return new WaitForSeconds(Random.Range(2, 5));
            Destroy(source.gameObject);
        }
    }
}
