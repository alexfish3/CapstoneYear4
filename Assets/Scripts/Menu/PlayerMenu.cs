using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMenu : SingletonMonobehaviour<PlayerMenu>
{
    [SerializeField] PlayerInstantiate playerInstantiate;

    private void OnEnable()
    {
        playerInstantiate.OnReadiedUp += LoadGameScene;
    }

    private void OnDisable()
    {
        playerInstantiate.OnReadiedUp -= LoadGameScene;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerInstantiate.CheckReadyUpCount();
        }
    }

    ///<summary>
    /// Main method that loads the game
    ///</summary>
    private void LoadGameScene()
    {
        StartCoroutine(LoadSceneAsync());
    }

    ///<summary>
    /// Loads the scene async
    ///</summary>
    private IEnumerator LoadSceneAsync()
    {
        // Loads the first scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
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
