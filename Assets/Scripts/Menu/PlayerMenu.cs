using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : SingletonMonobehaviour<PlayerMenu>
{
    [SerializeField] PlayerInstantiate playerInstantiate;

    private void Start()
    {
        playerInstantiate = PlayerInstantiate.Instance;
        playerInstantiate.OnReadiedUp += LoadGameScene;
        GameManager.Instance.OnSwapResults += LoadResultsScene;
        GameManager.Instance.OnSwapMenu += LoadMenuScene;
    }

    private void OnDisable()
    {
        playerInstantiate.OnReadiedUp -= LoadGameScene;
        GameManager.Instance.OnSwapResults -= LoadResultsScene;
        GameManager.Instance.OnSwapMenu -= LoadMenuScene;
    }

    ///<summary>
    /// Main method that loads the game
    ///</summary>
    private void LoadGameScene()
    {
        StartCoroutine(LoadSceneAsync(1));
    }

    ///<summary>
    /// Main method that loads the menu
    ///</summary>
    private void LoadMenuScene()
    {
        Debug.Log("Load menu");
        StartCoroutine(LoadSceneAsync(0));
    }
    private void LoadResultsScene()
    {
        Debug.Log("Load results");
        StartCoroutine(LoadSceneAsync(2));
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
