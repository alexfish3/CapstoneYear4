using System.Collections;
using Udar.SceneManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : SingletonMonobehaviour<PlayerMenu>
{
    [SerializeField] PlayerInstantiate playerInstantiate;

    [SerializeField] SceneField PlayerSelectScene;
    [SerializeField] SceneField GameScene;
    [SerializeField] SceneField ResultsScene;

    private void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
        playerInstantiate.OnReadiedUp += LoadGameScene;

        GameManager.Instance.OnSwapMenu += LoadPlayerSelectScene;
    }

    private void OnDisable()
    {
        playerInstantiate.OnReadiedUp -= LoadGameScene;

        GameManager.Instance.OnSwapMenu -= LoadPlayerSelectScene;
    }

    ///<summary>
    /// Main method that loads the player select scene
    ///</summary>
    private void LoadPlayerSelectScene()
    {
        Debug.Log("Load menu");

        if (SceneManager.GetActiveScene().buildIndex != PlayerSelectScene.BuildIndex)
            StartCoroutine(LoadSceneAsync(PlayerSelectScene.BuildIndex));

        // Once menu scene is loaded, set players to spawn, if there are any
        playerInstantiate.SetAllPlayerSpawn();
    }

    ///<summary>
    /// Main method that loads the game
    ///</summary>
    private void LoadGameScene()
    {
        if(SceneManager.GetActiveScene().buildIndex != GameScene.BuildIndex)
            StartCoroutine(LoadSceneAsync(GameScene.BuildIndex));
    }

    ///<summary>
    /// Main method that loads the results screen
    ///</summary>
    private void LoadResultsScene()
    {
        Debug.Log("Load results");

        if (SceneManager.GetActiveScene().buildIndex != ResultsScene.BuildIndex)
            StartCoroutine(LoadSceneAsync(ResultsScene.BuildIndex));
    }

    ///<summary>
    /// Loads the scene async
    ///</summary>
    private IEnumerator LoadSceneAsync(int sceneToLoad)
    {
        // Loads the first scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene is allowed to be activated
        while (!asyncLoad.allowSceneActivation)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
