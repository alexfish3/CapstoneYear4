using System;
using System.Collections;
using Udar.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneManager : SingletonMonobehaviour<SceneManager>
{
    [SerializeField] PlayerInstantiate playerInstantiate;
    public event Action OnReturnToMenu;

    [Header("Loading Screen")]
    [SerializeField] bool LoadingScreenEnabled;
    [SerializeField] float loadingScreenDelay;
    [SerializeField] Image UIRender;
    Material UIRenderMaterial;
    [SerializeField] Animator loadingScreenAnimation;

    [Header("Scene References")]
    [SerializeField] SceneField PlayerSelectScene;
    [SerializeField] SceneField GameScene;

    private void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
        playerInstantiate.OnReadiedUp += LoadGameScene;
        OnReturnToMenu += LoadMenuScene;

        GameManager.Instance.OnSwapMenu += HideLoadingScreen;
        GameManager.Instance.OnSwapStartingCutscene += HideLoadingScreen;
    }

    private void OnDisable()
    {
        playerInstantiate.OnReadiedUp -= LoadGameScene;
        OnReturnToMenu -= LoadMenuScene;

        GameManager.Instance.OnSwapMenu -= HideLoadingScreen;
        GameManager.Instance.OnSwapStartingCutscene -= HideLoadingScreen;
    }

    public void InvokeMenuSceneEvent() {
        Debug.Log("Return to main menu");
        OnReturnToMenu?.Invoke(); }

    ///<summary>
    /// Main method that loads the player select scene
    ///</summary>
    private void LoadMenuScene()
    {
        Debug.Log("Load menu");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != PlayerSelectScene.BuildIndex)
        {
            ShowLoadingScreen();
            StartCoroutine(LoadSceneAsync(PlayerSelectScene.BuildIndex, loadingScreenDelay, true));
        }
    }

    ///<summary>
    /// Main method that loads the game
    ///</summary>
    private void LoadGameScene()
    {
        if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != GameScene.BuildIndex)
        {
            ShowLoadingScreen();
            StartCoroutine(LoadSceneAsync(GameScene.BuildIndex, loadingScreenDelay, false));
        }
    }

    ///<summary>
    /// Loads the scene async
    ///</summary>
    private IEnumerator LoadSceneAsync(int sceneToLoad, float delayTime, bool spawnMenu)
    {
        // Sets all players to no input during loading screen
        playerInstantiate.SwapPlayerControlSchemeToNoInput();

        // Loads the first scene asynchronously
        AsyncOperation asyncLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        yield return new WaitForSeconds(delayTime);

        // Wait until the asynchronous scene is allowed to be activated
        while (!asyncLoad.allowSceneActivation)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        if(spawnMenu)
        {
            // Once menu scene is loaded, set players to spawn, if there are any
            playerInstantiate.SetAllPlayerSpawn();
        }
    }

    private void ShowLoadingScreen()
    {
        // Checks if loading screen is set, if is, return
        if (LoadingScreenEnabled == true)
            return;

        // Sets loading screen to true
        LoadingScreenEnabled = true;

        // Caches screen material
        UIRenderMaterial = new Material(UIRender.material);
        UIRender.material = UIRenderMaterial;

        Debug.Log("Show Loading Screen");
        LoadIn();
        loadingScreenAnimation.SetTrigger("LoadIn");
    }

    private void HideLoadingScreen()
    {
        // Checks if loading screen is not set, if is, return
        if(LoadingScreenEnabled == false) 
            return;

        LoadingScreenEnabled = false;
        Debug.Log("Hide Loading Screen");
        LoadOut();
        loadingScreenAnimation.SetTrigger("LoadOut");
    }

    void LoadIn() { StartCoroutine(AnimateMaterial(false)); }
    void LoadOut() { StartCoroutine(AnimateMaterial(true)); }

    private IEnumerator AnimateMaterial(bool type)
    {
        yield return null;

        // Increase value until 1
        if (type)
        {
            float value = 0;

            while(value < 1)
            {
                yield return null;
                value += Time.deltaTime;
                UIRenderMaterial.SetFloat("_Value", value);
            }
        }
        // Decrease value unitl 0
        else
        {
            float value = 1;

            while (value > 0)
            {
                yield return null;
                value -= Time.deltaTime;
                UIRenderMaterial.SetFloat("_Value", value);
            }
        }
    }
}
